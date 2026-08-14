# Windvale hosted read-only binary tool applications

## Status and scope

`WVHV 1` is the implemented manifest for packaging six fixed Windvale-written read-only binary tools as deterministic Windows and Linux x86-64 applications: the compiler-aligned WVB verifier, the complete structural `wvdump` inspector, the bounded portable-WVB runner, the WVO verifier/inspector, the two-input console-application verifier, and the exact hosted-verifier publisher admitter. All run without loading .NET. The WVB verifier and inspector applications have qualified histories; the runner has current-Windows focused reconstruction and execution evidence; and the WVO, console-verifier, and publisher-admission profiles remain implemented candidates at their documented evidence levels. Grouped dual-host retirement qualification remains pending. Stage 0 remains an independent recovery oracle until the broader native-retirement gate is qualified.

[Decision 0461](../Documents/Decisions/0461-Native-WVHV-Metadata-Ownership.md)
adds the first native-construction replacement for this family: an exact
`WVVR 1` request now drives portable Windvale construction and independent
admission of compiler-verifier profile 2 metadata. Runtime, startup, outer
container construction, and promotion remain pending and do not reuse the
separate `WVHB` build-driver meaning of numeric profile 2.

[Decision 0502](../Documents/Decisions/0502-Native-Console-Application-Verifier-Reconstruction.md)
extends that native construction ownership through the exact profile-7
console-application-verifier WVB, WVO oracle, linked fragment, and paired
Windows/Linux applications. The route runs on the current Windows host and
consumes retained same-release native toolsets; independent Linux execution,
clean bootstrap, qualification, promotion, and recovery release remain.

These are deliberately fixed tool profiles, not a general hosted-application format. The verifier enforces the same canonical compiler-aligned rules as the four-artifact verifier bundle from [the WebAssembly contract](Windvale-WebAssembly.md): complete envelope and canonical semantic validation, typed executable-flow validation, control-target reachability, and exact empty-stack join contracts. The native application retains one typed walk under a `u64` host meter. It constructs fixed-width per-function local-shape and control-boundary directories before the typed and reachability checks, avoiding repeated variable-width rescans without changing acceptance. The WebAssembly bundle partitions that walk only because execution ABI 3 exposes a `u32` meter. General WVB programs that require non-empty control-flow joins remain outside the verifier profile. The inspector decodes the separately specified structural/report subset and is never a substitute for semantic verification.

The canonical source project is [`Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj`](../Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj). It composes:

- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Tool.wv` as the hosted adapter;
- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv`;
- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv`;
- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv`.

The semantic and executable modules are portable. Their shared `Compilerˉwvbˉverify(bytes) -> u32` entry is also consumed in-process by the [compiler build driver](Windvale-Compiler-Build-Driver.md); the standalone tool owns only arguments, file input, diagnostics, and process result mapping.

The executable requires one candidate path:

```text
wvverify <module.wvb>
```

Success writes `wvb status=Valid profile=compiler-aligned` plus LF to standard output and returns `0`. Rejection writes one stable phase line to standard error and returns `1`. Invalid invocation writes the usage line to standard error and returns `64`.

The canonical WVB inspector project is [`Projects/Examples/Windvale-Wvb-Inspector.wvproj`](../Projects/Examples/Windvale-Wvb-Inspector.wvproj), which reuses the checked-in Windvale source `Examples/Foundation/Wv-Dump-Core.wv` without a second implementation. Its invocation is:

```text
wvdump <module.wvb>
```

Success writes the deterministic [`wvdump 1`](Wv-Dump-Report.md) line report and returns `0`. Structural rejection writes one stable diagnostic and returns `2`; invalid invocation returns `64`. Ordinary inspection first runs `wvverify` and invokes `wvdump` only after semantic acceptance.

The WVO profile packages [`Projects/Object-Model/Windvale-Wvo-Object.wvproj`](../Projects/Object-Model/Windvale-Wvo-Object.wvproj) under the separately specified [native WVO inspector contract](Windvale-Native-Wvo-Inspector.md). It uses the same eleven-service read-only startup as `wvdump`, but profile 6 binds the canonical `Wvoˉobjectˉcore` identity and its `verify`/`inspect` command contract. Profile 7 packages [`Projects/Tools/Windvale-Console-Application-Verifier.wvproj`](../Projects/Tools/Windvale-Console-Application-Verifier.wvproj) with the same eleven services and reads exactly two immutable application chunks before performing the portable console-application verification plan.

## Source and authority contract

Every application module has hosted authority and declares exactly these five capabilities in canonical order:

1. `console.write_line(text) -> void`;
2. `diagnostic.write_line(text) -> void`;
3. `file.read_bytes(text) -> bytes`;
4. `process.argument(u32) -> text`;
5. `process.argument_count() -> u32`.

The verifier fragment requires exactly five corresponding services. Its startup additionally binds `text.utf8_is_valid` to validate the host argument snapshot. That sixth service is marked startup-internal in the manifest and is not an application capability grant.

The inspector fragment requires the same read-only host services plus the capability-free `text.utf8_is_valid`, `enum.name`, `text.concat`, `text.quote`, `i32.format`, and `u32.format` report services. Its exact eleven-service sequence contains every native service through `u32.format` and excludes `file.write_bytes`. The runner omits the unused enum-name and quoting leaves, retaining exactly nine services. No profile has a file-output table, file-output scratch space, or file-write authority.

Each tool except profile 7 reads at most one candidate snapshot. Profile 7
requires exactly two immutable application-chunk paths and retains two
snapshot records. Every snapshot remains bounded by the ordinary 4 MiB `bytes`
limit. No profile has a file-output table or output scratch space.

## `WVHV 1` metadata

The manifest is a fixed 1,024-byte little-endian record in the first runtime-data page. Its magic is ASCII `WVHV`, metadata version is `1`, and outer container format is `4`.

```text
u32   magic = WVHV
u32   metadata version = 1
u32   metadata bytes = 1024
u32   target: 1 Windows x64, 2 Linux x64
u32   outer container format = 4
u32   native ABI version = 22
u32   execution-context version
u32   service-table version
u32   capability count = 5
u32   service count = 6 compiler verifier, 11 WVB/WVO inspectors or console verifier, 9 runner
u32   capability offset = 128
u32   capability record bytes = 16
u32   service offset = 208
u32   service record bytes = 64
u32   bundle offset = 4096
u32   bundle bytes
u32   native-image offset within bundle = 0
u32   native-image bytes
u32   native Main offset
u32   record-arena bytes
u32   text-arena bytes
u32   profile: 2 compiler-WVB verifier, 4 WVB inspector, 5 WVB runner, 6 WVO inspector, 7 console verifier, 8 publisher admission
u64   instruction budget = 16,000,000,000
bytes native-image SHA-256[32]
```

Five 16-byte capability records follow the header. Each contains the fixed capability identity, its service identity, exact signature identity, and contract major version `1`.

For profile `2`, six 64-byte service records follow the capability records in this exact order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `text.utf8_is_valid`;
6. `diagnostic.write_line`.

Each service record binds its service identity, capability identity or zero for startup-internal support, service-table slot, platform-adapter identity, image offset, code extent, flags, and SHA-256 digest. All reserved fields and the unused metadata tail are zero. Verification reconstructs the complete expected bundle and rejects any changed identity, ordering, address, extent, flag, digest, reserved byte, native entry, or target.

Profile `4` extends that exact sequence with `enum.name`, `text.concat`, `text.quote`, `i32.format`, and `u32.format`. Those services have zero capability identity and pure-service flags. The profile identity, service count, exact order, records, and zero tail are independently verified.

Profile `5` uses the verifier's first six services and then `text.concat`, `i32.format`, and `u32.format`. It reuses the inspector startup template; the two service-table slots not present in the runner profile are bound to zero and are unreachable from the independently verified runner fragment.

Profile `6` uses the same exact eleven services as profile `4` and reuses the same inspector startup template. Its distinct profile identity prevents a WVB inspector package from being accepted as the WVO front door or vice versa.

Profile `7` uses profile 6's exact eleven-service sequence and inspector startup
template. Its distinct metadata identity and two-snapshot runtime capacity bind
the console-application verifier without granting file-write authority.

Profile `8` uses the verifier's exact five capabilities and six-service bundle.
It reads one immutable publisher application snapshot, performs exact target,
length, and SHA-256 admission in Windvale source, and has no file-output table,
output scratch, or mutation authority. The numeric profile `7` used by the
`WVHG` hosted-container segmenter belongs to another metadata family.

## Runtime and startup

The runtime header retains the ABI-22 execution context, service table, output table, and file-input table. It reserves:

- at most 67 arguments, each at most 4,096 UTF-8 bytes and at most 65,536 aggregate bytes;
- one 32-byte file snapshot record, or two for profile 7;
- one 1 MiB name stride per snapshot;
- one 4 MiB data stride per snapshot;
- the existing fixed record arena and Decision 0201's 128 MiB hosted text arena;
- one platform path-conversion scratch region;
- no file-output scratch region.

Canonical WVA startup sources own process entry and runtime binding:

- `Linker/Startup/Windows-X64-Hosted-Verifier.wva`;
- `Linker/Startup/Linux-X64-Hosted-Verifier.wva`.

The inspector uses separate least-authority startup sources because its five additional pure report-service pointers must be bound before Windvale `Main`:

- `Linker/Startup/Windows-X64-Hosted-Inspector.wva`;
- `Linker/Startup/Linux-X64-Hosted-Inspector.wva`.

Their assembled WVO identities are respectively:

- Windows: `4d97a1f30d9c871f2a72911cea2644b32d3ea29a2dbbc76105ec4ab1d001b95f`;
- Linux: `08a7afefb69904af8d8c899a86bec76e957dfe255d397dbd9015d9acaa018ae8`.

The inspector WVO identities are:

- Windows: `95ff213a8e59f28d148eb8223a100a5b24dcbc3eb1b444264783a860f159fe49`;
- Linux: `5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb`.

The package constructors retain only the zero-relocation templates plus typed patch plans. Tests reassemble the WVA sources, resolve local and imported symbols independently, and require byte equality with each packaged startup.

## Windows container

`windows-x64-verifier-v1` emits a PE32+ console application with RX `.text`, RW/NX `.data`, and read-only discardable `.reloc` sections. The startup imports twelve functions from `KERNEL32.dll`, including `ExitProcess`, and `CommandLineToArgvW` from `SHELL32.dll`. It imports no CLR or C runtime and exposes no file-output service. `CreateFileW` is used by the trusted startup with read-only access for the bounded input snapshots: one for profiles 2, 6, and 8, and two for profile 7.

The current WVB 1.11 reconstruction candidate is 1,226,240 bytes with SHA-256 `332488305b0b178dcb713edd81f2df0b8f04455b95e03ee46aa226c69e2ee018`.

`windows-x64-wvb-inspector-v1` uses the same outer PE and read-only host imports with metadata profile `4`. Its current corrected-backend candidate is 793,600 bytes with SHA-256 `31b958fa446e7b4776ba1db0469a6c9ab32c53d960f55a476a6a202cd322194c`.

`windows-x64-wvb-runner-v1` uses metadata profile `5`. The current source reconstruction candidate is 1,094,656 bytes with SHA-256 `28158b3fcd050b38d1054d2aa44da15e6e481a20f6918fab85279ba3c10ca05c`.

`windows-x64-wvo-inspector-v1` uses metadata profile `6`. Its current bounded-reporting source candidate is 1,037,312 bytes with SHA-256 `5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03`.

`windows-x64-console-application-verifier-v1` uses metadata profile `7` and
two immutable input snapshots. Its current native reconstruction candidate is
1,063,936 bytes with SHA-256
`a82027ab78ee5f4d7d9f34180392ee8b8364ea78616c11aeac1e684250fc3679`.

`windows-x64-native-hosted-verifier-publisher-admission-v1` uses profile `8`.
Its current native candidate is 570,368 bytes with SHA-256
`1407ed428387986e170b4d8394e9a0a6295408ef668d5d6e16d719102428dd4f`.

## Linux container

`linux-x64-verifier-v1` emits a sectionless x86-64 static-PIE ELF with read-only headers, RX text, RW/NX runtime data, a format-4 Windvale note, and a 64 MiB RW/NX GNU stack declaration. It has no interpreter, dynamic table, imports, or loader relocations and uses only checked startup syscalls.

The current WVB 1.11 reconstruction candidate is 1,224,704 bytes with SHA-256 `e59a0fd2b7c959306b446e8bf387d54118b4719d9099b71f224c1ea4d34802f3`.

`linux-x64-wvb-inspector-v1` uses the same static outer ELF and metadata profile `4`. Its current corrected-backend candidate is 794,624 bytes with SHA-256 `cc87e9b7dc9bd74d5e14ab079c94cec9e77669953e301d9d32c06c3cefff9f9e`.

`linux-x64-wvb-runner-v1` uses metadata profile `5`. The Decision 0510 current source reconstruction candidate is 1,093,632 bytes with SHA-256 `a674b455aecaec48889318fd190a2123bc8bc784b1ee9b9eaa76b491ebebcb2d`.

`linux-x64-wvo-inspector-v1` uses metadata profile `6`. Its current bounded-reporting source candidate is 1,036,288 bytes with SHA-256 `fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840`.

`linux-x64-console-application-verifier-v1` uses metadata profile `7` and two
immutable input snapshots. Its current native cross-target reconstruction
candidate is 1,064,960 bytes with SHA-256
`c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a`.

`linux-x64-native-hosted-verifier-publisher-admission-v1` uses profile `8`.
Its current native candidate is 569,344 bytes with SHA-256
`27fff54e139228586a6948aa234de60e5d4f5439e6b0616a55c057d4ad8661c2`.

The digest-bound [native read-only front door](Windvale-Native-Wvb-Read-Only-Front-Door.md) intentionally continues to use the previously qualified verifier and inspector applications until the corrected-backend candidates pass the same exact-commit dual-host gate. Candidate reconstruction does not silently replace a qualified ordinary artifact.

## Construction and verification

The Stage 0 routes are:

```text
windvale build Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target windows-x64-verifier-v1
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target linux-x64-verifier-v1
windvale build Projects/Examples/Windvale-Wvb-Inspector.wvproj
windvale aot Windvale-Wvb-Inspector.wvb --target windows-x64-wvb-inspector-v1
windvale aot Windvale-Wvb-Inspector.wvb --target linux-x64-wvb-inspector-v1
windvale build Projects/Tools/Windvale-Wvb-Runner.wvproj
windvale aot Windvale-Wvb-Runner.wvb --target windows-x64-wvb-runner-v1
windvale aot Windvale-Wvb-Runner.wvb --target linux-x64-wvb-runner-v1
windvale build Projects/Object-Model/Windvale-Wvo-Object.wvproj
windvale aot Windvale-Wvo-Object.wvb --target windows-x64-wvo-inspector-v1
windvale aot Windvale-Wvo-Object.wvb --target linux-x64-wvo-inspector-v1
windvale build Projects/Tools/Windvale-Console-Application-Verifier.wvproj
windvale aot Console-Application-Verifier.wvb --target windows-x64-console-application-verifier-v1
windvale aot Console-Application-Verifier.wvb --target linux-x64-console-application-verifier-v1
```

The current canonical-source reconstruction candidate is 148,793 bytes with SHA-256 `70bd61e78c2ddd6052adb15f24a155f006ded903ce7825d8f54adafa252b76f8`. The frozen Stage 0 compiler and qualified native build front door produce byte-identical WVBs. The digest-bound ordinary front door retains its previously qualified artifact until this candidate passes the exact-commit dual-host promotion gate.

The publisher admitter has no Stage 0 CLI target. Its paired candidates are
constructed by `Construct-Hosted-Verifier-Publisher-Admitter.cmd` and `.sh`
through the native WVB build/lower/link and hosted-container tools.

The canonical inspector WVB is 76,527 bytes with SHA-256 `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753`.

The current source-built runner WVB is 121,593 bytes with SHA-256 `e58f653445cd717d19c32fe1a0fbc57f03f475187cdec571825b9fd6685b3097`. Its exact current native object is 1,078,577 bytes with SHA-256 `7d0ec719ade7e55d46c5a6dc6f7cb63102db4633172bcab1812e16651002106d`.

The WVO inspector candidate is 73,322 bytes with SHA-256 `40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070`.

The verifier writers require exactly one exported `Main() -> i32`, the five canonical capability declarations, and the five canonical verifier-fragment services; they add only the startup-internal UTF-8 service. The WVB and WVO inspector writers require the same entry and capabilities plus the exact eleven-service read-only fragment, and the WVO writer additionally binds the canonical module identity. The runner writers require the same entry and capabilities plus its exact eight-service source fragment, and retain the same qualified startup-internal UTF-8 leaf in the fixed nine-service application bundle. All four profiles construct the outer application, parse it independently, and atomically publish it only after every profile, manifest, startup, import, section, permission, extent, padding, digest, and native-entry check succeeds.

Qualification shares the existing exact-compiler AOT test so the suite compiles the large compiler once. That test compiles the verifier, verifies both deterministic packages, reconstructs both WVA startups, corrupts each outer boundary, runs the current-host application against the exact compiler WVB, rejects a corrupted candidate, and inspects the child for .NET modules or mappings. The same case then consumes the portable verifier core in the build-driver proof rather than compiling a second verifier implementation.

## Retirement boundary

The verifier satisfies one qualified part of the native-retirement inventory: a Windvale-authored semantic verifier runs as a direct Windows or Linux process, and its portable core is shared by the native compiler build driver. The WVB inspector is already qualified. Decision 0509 gives the runner an exact current-Windows-host source-to-WVB-to-WVO-to-paired-application reconstruction plus current-host result/instruction-reporting evidence; Decision 0510 aligns its fixed budget with the ordinary CLI default and transfers three supported Foundation demo executions. Independent Linux execution, grouped qualification, and promotion remain open. The separate [`WVHP 1` console-packager](Windvale-Native-Console-Packager.md) and [`WVHN 1` WVB-to-WVO](Windvale-Native-Wvb-To-Wvo.md) candidates own bounded version-1 recipe materialization and accepted-subset native lowering. Decisions 0497 and 0498 reconstruct their exact current paired applications through retained Windvale-native paths on the current Windows host; Stage 0 remains recovery provenance rather than their only application constructor. Complete native x64 lowering, remaining hosted/tool container construction, capability-bearing execution, the reference recovery compiler, broad test orchestration, release production, and repository automation still invoke .NET. All remaining [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) conditions remain mandatory before .NET leaves the normal path.
