# Windvale compiler build-driver application

## Status and scope

`WVHB 1` is the implemented manifest for the first Windvale-native source-to-verified-WVB build driver. The same canonical Windvale application packages as deterministic Windows and Linux x86-64 processes and runs without loading .NET. Stage 0 still constructs, native-lowers, packages, and independently verifies the driver itself; cross-host qualification remains pending.

The driver is intentionally narrow. It accepts one root source, zero or more dependencies already ordered for the current source composer, and one output path:

```text
wvbuild <root.wv> [sorted-dependency.wv ...] <output.wvb>
```

It supports at most 64 source modules and rejects an output resource name exactly equal to any input resource name. On success it constructs `WVSS 1`, invokes the Windvale compiler in memory, invokes the portable compiler-aligned WVB verifier over the candidate bytes, performs exactly one `file.write_bytes` only after verifier acceptance, reports the published function/code/module sizes, and returns `0`. Invocation failure returns `64`; compilation or verifier rejection reports a stable diagnostic and returns `1`.

The canonical project is [`Windvale-Compiler-Build-Driver.wvproj`](../Windvale-Compiler-Build-Driver.wvproj). It composes the compiler sources from [`Windvale-Compiler.wvproj`](../Windvale-Compiler.wvproj), the portable verifier core, and `Tools/Windvale.Build/Compiler-Build-Driver.wv` as its only hosted adapter.

## Verification and publication boundary

`Compilerˉwvbˉverify(bytes) -> u32` is now a portable imported function shared by the standalone verifier tool and the build driver. It returns:

| Status | Meaning |
| ---: | --- |
| `0` | accepted by semantic, typed-execution, and control-reachability phases |
| `1` | semantic rejection |
| `2` | typed-execution rejection |
| `3` | control-reachability rejection |

The driver never calls `file.write_bytes` after an invocation, source, compilation, or verifier failure. Those deterministic failures therefore leave an existing output unchanged. The successful output is the exact verifier-accepted byte value; no separate candidate file or host process can replace it between verification and the write call.

This is not yet an atomic replacement contract. `file.write_bytes` retains its existing durable, whole-value but non-atomic host semantics. A host I/O failure may leave its separately specified result and is not reported as deterministic output preservation. The driver can reject exact input/output name equality but cannot prove that different host path spellings identify different files; canonical resource identity requires a separate provider contract. Atomic replacement, directory durability, and indeterminate-mutation evidence require a distinct future filesystem capability rather than silently strengthening this operation.

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
- instruction budget `8,000,000,000`;
- exact native-image and per-service SHA-256 identities.

The PE/ELF constructors and independent parsers take the expected compiler or build-driver profile explicitly. A format-3 compiler container is rejected as format 5 and a format-5 driver is rejected as format 3. Every changed header format, metadata magic, native bundle, digest, extent, reserved byte, truncation, or extension is rejected before execution or package publication.

Because the driver and compiler bind the same ten services and runtime tables, format 5 deliberately reuses the canonical hosted-compiler WVA startup and platform import/syscall adapters. It does not create a parallel runtime or another ABI. Only the outer format, metadata identity/profile, canonical WVB identity check, and native application bytes differ.

## Canonical identities

The canonical driver WVB is 718,058 bytes with SHA-256 `eb8d22a344a04e705c6e978ce3ddb941ca47686a8a79fa089db254cc3ede73fd`.

`windows-x64-build-driver-v1` emits an 18,099,712-byte PE32+ application with SHA-256 `204aa0ac555d47d72fc40424c0cb5c9cf30afc4f89d4b8ff4addaadf0a086677`.

`linux-x64-build-driver-v1` emits an 18,100,224-byte sectionless static-PIE ELF application with SHA-256 `e6a618364150d9631cf49ddecd090d8f8750d4bd6232680984584899113ba6cb`.

The Stage 0 construction route is:

```text
windvale build Windvale-Compiler-Build-Driver.wvproj
windvale aot Windvale-Compiler-Build-Driver.wvb --target windows-x64-build-driver-v1
windvale aot Windvale-Compiler-Build-Driver.wvb --target linux-x64-build-driver-v1
```

The existing exact-compiler AOT test constructs this project once, verifies deterministic paired packages, drives malformed outer inputs through both independent parsers, exercises the public current-host AOT route, compiles a real module through the raw application, verifies exact output bytes, rejects malformed source without changing an existing output, rejects exact source/output resource-name equality without changing the source, and inspects current-host modules or mappings for .NET.

## Retirement boundary

This milestone removes .NET from one useful execution path: a previously packaged Windvale-native driver can compile and verifier-admit ordinary source into canonical WVB on Windows or Linux. It does not yet read `.wvproj`, discover or sort dependencies, package WVB as native PE/ELF, run tests, assemble, link, inspect, or atomically replace the produced WVB. Stage 0 still builds and packages the driver and remains the recovery oracle. Decision 0057's complete dual-host native-retirement gate remains mandatory.
