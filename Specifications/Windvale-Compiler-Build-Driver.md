# Windvale compiler build-driver application

## Status and scope

`WVHB 1` is the implemented manifest for the Windvale-native
source-to-verified-WVB build driver. The same canonical Windvale application
packages as deterministic Windows and Linux x86-64 processes. The pinned packages
are consumed by the ordinary [native source-to-WVB front door](Windvale-Native-Source-To-Wvb-Front-Door.md).

The driver is intentionally narrow. Its explicit-source form accepts one root source, zero or more dependencies, and one output path:

```text
wvbuild <root.wv> [dependency.wv ...] <output.wvb>
```

Its normal project form consumes an explicitly supplied Workspace 1 and Project 2
pair and requires an explicit output:

```text
wvbuild --workspace <workspace.wvws> --project <project.wvproj> <output.wvb>
```

The explicit form supports at most 64 source modules. The current project-driver
profile supports at most 63 modules. Both forms read each source resource exactly
once, construct `WVSS 1` in memory, invoke the Windvale compiler, invoke the
portable compiler-aligned WVB verifier over the candidate bytes, and perform exactly
one `file.write_bytes` only after verifier acceptance. Success reports the published
function/code/module sizes and returns `0`; invocation failure returns `64`;
workspace, project, compilation, or verifier rejection reports a stable diagnostic
and returns `1`.

The canonical project is [`Projects/Tools/Windvale-Compiler-Build-Driver.wvproj`](../Projects/Tools/Windvale-Compiler-Build-Driver.wvproj). It composes the compiler sources, the portable Project 2 parser, the portable verifier core, and `Tools/Windvale.Build/Compiler-Build-Driver.wv` as its only hosted adapter.

## Project resource boundary

Project mode reads the workspace marker and manifest once, validates the manifest
through `Windvaleˉprojectˉscanˉmanifest`, and obtains the root followed by the
manifest's source entries through `Windvaleˉprojectˉpathˉat`. Source `import`
declarations remain the semantic dependency graph; the driver neither discovers
files nor guesses dependency order.

The hosted adapter derives each source resource name from the directory containing
the explicit workspace marker and appends the Project 2 canonical
workspace-relative path. Manifest location has no source-resolution semantics.
Resource names use `/` at this boundary on both hosts; `\` is rejected instead of
being given platform-dependent meaning.

Before any source read, the driver rejects a resource name equal to an earlier
source or the output. Comparison folds ASCII `A` through `Z`, making the accepted
project subset conservative on case-sensitive Linux providers while preventing the
ordinary Windows case alias. Repository wrappers reject reparse/link-bearing
workspaces until a canonical resource-identity capability exists. `.wvws`, `.wvproj`,
and `.wvb` suffixes are exact and mandatory in project mode.

Project failures use the existing `WVP1001` through `WVP1007` identities with one-based line and column evidence. The native 63-module bound reports `WVP1005`; conservative duplicate rejection reports `WVP1007` before source access. Malformed projects and compilation failures leave an existing output unchanged.

## Verification and publication boundary

`Compilerˉwvbˉverifyˉmetadata(bytes) -> u32` is the portable imported function
shared by the standalone verifier tool and the build driver. It validates and
normalizes independently present module metadata before invoking the retained
compiler-aligned semantic, typed-execution, and reachability verifier. It
returns:

| Status | Meaning |
| ---: | --- |
| `0` | accepted by semantic, typed-execution, and control-reachability phases |
| `1` | semantic rejection |
| `2` | typed-execution rejection |
| `3` | control-reachability rejection |

The driver never calls `file.write_bytes` after an invocation, manifest, resource-set, source, compilation, or verifier failure. Those deterministic failures therefore leave an existing output unchanged. The successful output is the exact verifier-accepted byte value; no separate candidate file or host process can replace it between verification and the write call.

The raw driver operation is not an atomic replacement contract. `file.write_bytes`
retains its durable, whole-value but non-atomic host semantics. A host I/O failure
may leave its separately specified result and is not reported as deterministic
output preservation. Conservative resource-name comparison is not canonical
resource identity.

The ordinary front door therefore directs the raw driver to a private caller-owned
candidate and passes that candidate to the separately qualified publisher. That
publisher owns native identity, same-directory atomic replacement, durability, and
indeterminate-completion evidence without silently strengthening `file.write_bytes`.
Direct use of `wvbuild` retains the raw contract defined here.

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
- instruction budget `64,000,000,000` under the successor to Decision 0201's hosted compiler/build-driver ceiling;
- a 234,881,024-byte dynamic text/byte arena and an 8,192-byte file-input name stride, while the compiler and hosted-tool profiles 3 through 7 retain a 134,217,728-byte arena and 1,048,576-byte name stride;
- exact native-image and per-service SHA-256 identities.

The larger arena and narrower name slots are profile-local. A hosted process
argument contains at most 4,096 UTF-8 bytes. The workspace resource prefix and one
Project 2 path are each bounded so their maximum concatenation fits the 8,192-byte
slot. The retained native
file-input leaves use `WVFI 1`'s declared name-stride field for both the
name-length check and slot addressing. The generic host executor and standalone
file-input-table constructor remain exact at 1,048,576 bytes and do not admit
this profile-local value.

The name arena begins at 237,051,904, the file-data arena at 237,576,192, and
file-input scratch at 506,011,648. The narrower slots do not narrow the retained
scratch capacities: file input and file output each retain 2,097,154 bytes on
Windows and 1,048,577 bytes on Linux. File-output scratch therefore begins at
508,112,896 on Windows and 507,064,320 on Linux. The final RW/NX runtime extent
is 510,214,144 bytes on Windows and 508,116,992 bytes on Linux, both below the
fixed 512 MiB runtime-data ceiling. That ceiling governs the runtime mapping
rather than the complete executable image; text and Windows import pages remain
separate mapped regions.

The PE/ELF constructors and independent parsers take the expected compiler or build-driver profile explicitly. A format-3 compiler container is rejected as format 5 and a format-5 driver is rejected as format 3. Every changed header format, metadata magic, native bundle, digest, extent, reserved byte, truncation, or extension is rejected before execution or package publication.

Because the driver and compiler bind the same ten services and runtime tables, format 5 deliberately reuses the canonical hosted-compiler WVA startup and platform import/syscall adapters. It does not create a parallel runtime or another ABI. Only the outer format, metadata identity/profile, canonical WVB identity check, and native application bytes differ.

## Canonical identities

The current canonical-source reconstruction candidate is 1,162,338 bytes with
SHA-256 `a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20`.
It stages as eight canonical native chunks with `Main` at entry offset 220,460.

The current WVB 1.11 reconstruction for `windows-x64-build-driver-v1` emits a
30,381,568-byte PE32+ application with SHA-256
`b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3`.

The current WVB 1.11 reconstruction for `linux-x64-build-driver-v1` emits a
30,380,032-byte sectionless static-PIE ELF application with SHA-256
`b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0`.

These are unqualified reconstruction candidates. `Build-Current-Wvb` uses the
current candidate driver's raw, self-verified, non-atomic output contract because
the semantic-freeze publisher does not admit forward-language WVB. The ordinary `Build-Wvb` launcher and
`Artifacts/Native-Front-Door` remain the qualified semantic-freeze baseline;
their historical qualification identities are not repinned by forward language
evolution.

The retained recovery construction route is recorded separately. The normal source
construction route is:

```text
Tools/Native/Build-Wvb Projects/Tools/Windvale-Compiler-Build-Driver.wvproj <output.wvb>
```

Forward-language consumers use the explicitly unqualified current route:

```text
Tools/Native/Build-Current-Wvb Projects/Tools/Windvale-Compiler-Build-Driver.wvproj <output.wvb>
```

`Test-Workspace-Project2` executes the native driver over the checked-in workspace,
writes and runs a valid module, and owns seven malformed or containment
rejections. `Test-Libraries` exercises Project 2 over reusable portable and hosted
library compositions. The qualified ordinary route preserves failed destinations;
the explicitly unqualified current route retains the raw driver's non-atomic output
and indeterminate-failure boundary.

## Retirement boundary

The ordinary launcher composes the packaged Windvale-native driver with the exact
native publisher. It does not discover files, consume packages or project
references, native-lower WVB, package applications as PE/ELF, run tests, assemble,
link, inspect, or execute output. Those are separate native contracts. The retained
recovery archive does not participate in ordinary builds or focused verification.
