# Windvale compiler build-driver application

## Status and scope

`WVHB 1` is the implemented manifest for the first Windvale-native source-to-verified-WVB build driver. The same canonical Windvale application packages as deterministic Windows and Linux x86-64 processes and runs without loading .NET. Stage 0 still constructs, native-lowers, packages, and independently verifies the driver itself; cross-host qualification remains pending.

The driver is intentionally narrow. Its explicit-source form accepts one root source, zero or more dependencies, and one output path:

```text
wvbuild <root.wv> [dependency.wv ...] <output.wvb>
```

Its project form consumes the existing Windvale Project 1 parser and requires an explicit output:

```text
wvbuild --project <project.wvproj> <output.wvb>
```

The explicit form supports at most 64 source modules. The project form supports at most 63 modules because its manifest occupies one of the fixed profile's 64 retained file snapshots. Both forms read each source resource exactly once, construct `WVSS 1` in memory, invoke the Windvale compiler, invoke the portable compiler-aligned WVB verifier over the candidate bytes, and perform exactly one `file.write_bytes` only after verifier acceptance. Success reports the published function/code/module sizes and returns `0`; invocation failure returns `64`; project, compilation, or verifier rejection reports a stable diagnostic and returns `1`.

The canonical project is [`Windvale-Compiler-Build-Driver.wvproj`](../Windvale-Compiler-Build-Driver.wvproj). It composes the compiler sources from [`Windvale-Compiler.wvproj`](../Windvale-Compiler.wvproj), the portable Project 1 parser, the portable verifier core, and `Tools/Windvale.Build/Compiler-Build-Driver.wv` as its only hosted adapter.

## Project resource boundary

Project mode reads the manifest once, validates it through `Windvaleˉprojectˉscanˉmanifest`, and obtains the root followed by the manifest's source entries through `Windvaleˉprojectˉpathˉat`. Source `import` declarations remain the semantic dependency graph; the driver neither discovers files nor guesses dependency order.

The hosted adapter derives each source resource name by retaining the manifest resource-name prefix through its final `/` and appending the Project 1 canonical relative path. The manifest argument therefore uses `/` as the profile's resource separator on both hosts; `\` is rejected instead of being given platform-dependent meaning. This is resource-name derivation inside the hosted adapter, not native path interpretation in the portable parser.

Before any source read, the driver rejects a resource name equal to an earlier source or the output. Comparison folds ASCII `A` through `Z`, making the accepted project subset conservative on case-sensitive Linux providers while preventing the ordinary Windows case alias. The adapter cannot resolve links, mount aliases, short names, or other provider identities; those remain excluded until a canonical resource-identity capability exists. `.wvproj` and `.wvb` suffixes are exact and mandatory in project mode.

Project failures use the existing `WVP1001` through `WVP1007` identities with one-based line and column evidence. The native 63-module bound reports `WVP1005`; conservative duplicate rejection reports `WVP1007` before source access. Malformed projects and compilation failures leave an existing output unchanged.

## Verification and publication boundary

`Compilerˉwvbˉverify(bytes) -> u32` is now a portable imported function shared by the standalone verifier tool and the build driver. It returns:

| Status | Meaning |
| ---: | --- |
| `0` | accepted by semantic, typed-execution, and control-reachability phases |
| `1` | semantic rejection |
| `2` | typed-execution rejection |
| `3` | control-reachability rejection |

The driver never calls `file.write_bytes` after an invocation, manifest, resource-set, source, compilation, or verifier failure. Those deterministic failures therefore leave an existing output unchanged. The successful output is the exact verifier-accepted byte value; no separate candidate file or host process can replace it between verification and the write call.

This is not yet an atomic replacement contract. `file.write_bytes` retains its existing durable, whole-value but non-atomic host semantics. A host I/O failure may leave its separately specified result and is not reported as deterministic output preservation. Conservative resource-name comparison is not canonical resource identity. Atomic replacement, directory durability, alias identity, and indeterminate-mutation evidence require distinct future provider contracts rather than silently strengthening this operation.

## Authority and native services

The driver declares the same six capabilities as the exact hosted compiler, in canonical order:

1. `console.write_line(text) -> void`;
2. `diagnostic.write_line(text) -> void`;
3. `file.read_bytes(text) -> bytes`;
4. `file.write_bytes(text, bytes) -> void`;
5. `process.argument(u32) -> text`;
6. `process.argument_count() -> u32`.

Its native fragment requires the same exact ten services as the compiler profile: console output, argument count, argument lookup, file input, strict UTF-8, diagnostic output, enum names, text concatenation, `u32` formatting, and file output. No process-launch, directory, ambient-environment, network, clock, entropy, or privileged service is present.

The format-5 public writers additionally require the verified WVB module name `Windvaleˉcompilerˉbuildˉdriver`. This prevents accidentally labelling the canonical compiler module as a driver even though both deliberately have the same authority profile. The recorded artifact hash, rather than the module-name check, identifies the exact canonical driver bytes.

## `WVHB 1` metadata and container

The fixed 1,024-byte metadata record reuses the qualified compiler-authority capability/service layout while giving the driver a distinct interpretation:

- magic ASCII `WVHB` (`0x42485657` little-endian);
- metadata version `1`;
- outer container format `5`;
- native ABI `22`, execution-context format `7`, service-table format `5`;
- six capability records and ten service records;
- profile flags `3`, meaning compiler plus in-process verifier build driver;
- instruction budget `48,000,000,000` under the shared Decision 0201 hosted compiler/build-driver ceiling;
- exact native-image and per-service SHA-256 identities.

The PE/ELF constructors and independent parsers take the expected compiler or build-driver profile explicitly. A format-3 compiler container is rejected as format 5 and a format-5 driver is rejected as format 3. Every changed header format, metadata magic, native bundle, digest, extent, reserved byte, truncation, or extension is rejected before execution or package publication.

Because the driver and compiler bind the same ten services and runtime tables, format 5 deliberately reuses the canonical hosted-compiler WVA startup and platform import/syscall adapters. It does not create a parallel runtime or another ABI. Only the outer format, metadata identity/profile, canonical WVB identity check, and native application bytes differ.

## Canonical identities

The canonical driver WVB is 1,068,108 bytes with SHA-256 `04fdb0c8de6ada23bf3c28b840782764551a27daa1244ff8315d41b0fb879210`.

`windows-x64-build-driver-v1` emits a 28,820,992-byte PE32+ application with SHA-256 `dae678ebe263ae6aeb62eace0943f8666878cc904cb5614f29e0de52f6548621`.

`linux-x64-build-driver-v1` emits a 28,823,552-byte sectionless static-PIE ELF application with SHA-256 `ffbd7f2849cc9507c1d6231e5e77c60d2e70d232ffddf18f83af2deeb09b918b`.

The Stage 0 construction route is:

```text
windvale build Windvale-Compiler-Build-Driver.wvproj
windvale aot Windvale-Compiler-Build-Driver.wvb --target windows-x64-build-driver-v1
windvale aot Windvale-Compiler-Build-Driver.wvb --target linux-x64-build-driver-v1
```

The existing exact-compiler AOT test constructs this project once, verifies deterministic paired packages, drives malformed outer inputs through both independent parsers, exercises the public current-host AOT route, and runs the native driver in both explicit and project modes. Project evidence parses a real manifest, resolves a three-module composition beneath the manifest resource prefix, compares exact output bytes with the reference compiler, rejects malformed and conservatively duplicate manifests without changing an existing output, and inspects current-host modules or mappings for .NET. No new top-level compiler construction is added.

## Retirement boundary

This milestone removes .NET from another useful execution path: a previously packaged Windvale-native driver can parse a bounded `.wvproj`, read its explicit source set, compile it, verifier-admit the result, and publish canonical WVB on Windows or Linux. It does not discover files, consume packages or project references, native-lower WVB, package applications as PE/ELF, run tests, assemble, link, inspect, or atomically replace output. The shared x64 lowering backend remains C#-owned; moving only outer PE/ELF headers would not create a .NET-free source-to-executable path. Stage 0 still builds and packages the driver and remains the recovery oracle. Decision 0057's complete dual-host native-retirement gate remains mandatory.
