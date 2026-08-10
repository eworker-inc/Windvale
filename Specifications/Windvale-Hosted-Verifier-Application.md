# Windvale hosted read-only binary tool applications

## Status and scope

`WVHV 1` is the implemented manifest for packaging five fixed Windvale-written read-only binary tools as deterministic Windows and Linux x86-64 applications: the compiler-aligned WVB verifier, the complete structural `wvdump` inspector, the bounded portable-WVB runner, the WVO verifier/inspector, and the exact hosted-verifier publisher admitter. All run without loading .NET. The WVB verifier and inspector applications have qualified histories; the runner, WVO, and publisher-admission profiles are implemented candidates pending the grouped dual-host retirement gate. Stage 0 remains an independent recovery oracle until the broader native-retirement gate is qualified.

[Decision 0461](../Documents/Decisions/0461-Native-WVHV-Metadata-Ownership.md)
adds the first native-construction replacement for this family: an exact
`WVVR 1` request now drives portable Windvale construction and independent
admission of compiler-verifier profile 2 metadata. Runtime, startup, outer
container construction, and promotion remain pending and do not reuse the
separate `WVHB` build-driver meaning of numeric profile 2.

These are deliberately fixed tool profiles, not a general hosted-application format. The verifier enforces the same canonical compiler-aligned rules as the four-artifact verifier bundle from [the WebAssembly contract](Windvale-WebAssembly.md): complete envelope and canonical semantic validation, typed executable-flow validation, control-target reachability, and exact empty-stack join contracts. The native application retains one monolithic typed walk under a `u64` host meter; the WebAssembly bundle partitions that walk only because execution ABI 3 exposes a `u32` meter. General WVB programs that require non-empty control-flow joins remain outside the verifier profile. The inspector decodes the separately specified structural/report subset and is never a substitute for semantic verification.

The canonical source project is [`Windvale-Compiler-Wvb-Verifier.wvproj`](../Windvale-Compiler-Wvb-Verifier.wvproj). It composes:

- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Tool.wv` as the hosted adapter;
- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv`;
- `Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv`.

The semantic and executable modules are portable. Their shared `Compilerˉwvbˉverify(bytes) -> u32` entry is also consumed in-process by the [compiler build driver](Windvale-Compiler-Build-Driver.md); the standalone tool owns only arguments, file input, diagnostics, and process result mapping.

The executable requires one candidate path:

```text
wvverify <module.wvb>
```

Success writes `wvb status=Valid profile=compiler-aligned` plus LF to standard output and returns `0`. Rejection writes one stable phase line to standard error and returns `1`. Invalid invocation writes the usage line to standard error and returns `64`.

The canonical WVB inspector project is [`Windvale-Wvb-Inspector.wvproj`](../Windvale-Wvb-Inspector.wvproj), which reuses the checked-in Windvale source `Examples/Foundation/Wv-Dump-Core.wv` without a second implementation. Its invocation is:

```text
wvdump <module.wvb>
```

Success writes the deterministic [`wvdump 1`](Wv-Dump-Report.md) line report and returns `0`. Structural rejection writes one stable diagnostic and returns `2`; invalid invocation returns `64`. Ordinary inspection first runs `wvverify` and invokes `wvdump` only after semantic acceptance.

The WVO profile packages [`Windvale-Wvo-Object.wvproj`](../Windvale-Wvo-Object.wvproj) under the separately specified [native WVO inspector contract](Windvale-Native-Wvo-Inspector.md). It uses the same eleven-service read-only startup as `wvdump`, but profile 6 binds the canonical `Wvoˉobjectˉcore` identity and its `verify`/`inspect` command contract.

## Source and authority contract

Every application module has hosted authority and declares exactly these five capabilities in canonical order:

1. `console.write_line(text) -> void`;
2. `diagnostic.write_line(text) -> void`;
3. `file.read_bytes(text) -> bytes`;
4. `process.argument(u32) -> text`;
5. `process.argument_count() -> u32`.

The verifier fragment requires exactly five corresponding services. Its startup additionally binds `text.utf8_is_valid` to validate the host argument snapshot. That sixth service is marked startup-internal in the manifest and is not an application capability grant.

The inspector fragment requires the same read-only host services plus the capability-free `text.utf8_is_valid`, `enum.name`, `text.concat`, `text.quote`, `i32.format`, and `u32.format` report services. Its exact eleven-service sequence contains every native service through `u32.format` and excludes `file.write_bytes`. The runner omits the unused enum-name and quoting leaves, retaining exactly nine services. No profile has a file-output table, file-output scratch space, or file-write authority.

Each tool reads at most one candidate snapshot. The candidate remains bounded by the ordinary 4 MiB `bytes` limit. The runtime retains one file-input snapshot slot and no file-output table or output scratch space.

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
u32   service count = 6 verifier, 11 WVB/WVO inspector, 9 runner
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
u32   profile: 2 compiler-WVB verifier, 4 WVB inspector, 5 WVB runner, 6 WVO inspector, 8 publisher admission
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

Profile `8` uses the verifier's exact five capabilities and six-service bundle.
It reads one immutable publisher application snapshot, performs exact target,
length, and SHA-256 admission in Windvale source, and has no file-output table,
output scratch, or mutation authority. Profile `7` remains assigned to the
separately owned console-application verifier; the numeric profile `7` used by
the `WVHG` hosted-container segmenter belongs to another metadata family.

## Runtime and startup

The runtime header retains the ABI-22 execution context, service table, output table, and file-input table. It reserves:

- at most 67 arguments, each at most 4,096 UTF-8 bytes and at most 65,536 aggregate bytes;
- one 32-byte file snapshot record;
- one 1 MiB name stride;
- one 4 MiB data stride;
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

- Windows: `755ffb99cba6a838dd9eec353ce72d4adfb3af130ec4bce5a2278828dd136616`;
- Linux: `08a7afefb69904af8d8c899a86bec76e957dfe255d397dbd9015d9acaa018ae8`.

The inspector WVO identities are:

- Windows: `1bb785d5a06c40b91e45ebdc26b33ae33cb8ee7b244daffaa30ee59b9509edf3`;
- Linux: `5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb`.

The package constructors retain only the zero-relocation templates plus typed patch plans. Tests reassemble the WVA sources, resolve local and imported symbols independently, and require byte equality with each packaged startup.

## Windows container

`windows-x64-verifier-v1` emits a PE32+ console application with RX `.text`, RW/NX `.data`, and read-only discardable `.reloc` sections. The startup imports eleven functions from `KERNEL32.dll` and `CommandLineToArgvW` from `SHELL32.dll`. It imports no CLR or C runtime and exposes no file-output service. `CreateFileW` is used by the trusted startup with read-only access for the one bounded input snapshot.

The current WVB 1.11 reconstruction candidate is 1,199,104 bytes with SHA-256 `ef0e57d9c1e9d3c4da134611ac0154926c3489b5315bf0bd643fb2f468769460`.

`windows-x64-wvb-inspector-v1` uses the same outer PE and read-only host imports with metadata profile `4`. Its current corrected-backend candidate is 793,600 bytes with SHA-256 `31b958fa446e7b4776ba1db0469a6c9ab32c53d960f55a476a6a202cd322194c`.

`windows-x64-wvb-runner-v1` uses metadata profile `5`. The committed pre-correction candidate remains 778,752 bytes with SHA-256 `91b046015660f5f9e2710ed9cb41d5da9a79a1c87f4cf9ed87790c013a6dcce4`; the current corrected-backend reconstruction is 778,240 bytes with SHA-256 `6231a60404fc49f85695eddcc2e0690e372c64c0cf2d2ca847fd0ffc3f76b028`.

`windows-x64-wvo-inspector-v1` uses metadata profile `6`. Its current source candidate is 606,720 bytes with SHA-256 `2a8f6f8ca8fc6054fff23441f7971c0b90900383d5bed0fecc54f9cac102a300`.

`windows-x64-native-hosted-verifier-publisher-admission-v1` uses profile `8`.
Its current native candidate is 570,368 bytes with SHA-256
`7f58a5e321d1b4baa16ba673b3e0e1c21c9acd040cba92dae0f180d629c63e6b`.

## Linux container

`linux-x64-verifier-v1` emits a sectionless x86-64 static-PIE ELF with read-only headers, RX text, RW/NX runtime data, a format-4 Windvale note, and a 64 MiB RW/NX GNU stack declaration. It has no interpreter, dynamic table, imports, or loader relocations and uses only checked startup syscalls.

The current WVB 1.11 reconstruction candidate is 1,200,128 bytes with SHA-256 `8a3edd4ea8d56746e37dcd49bddfb55adcf0dd8f52967d3d435867b4d7b4f938`.

`linux-x64-wvb-inspector-v1` uses the same static outer ELF and metadata profile `4`. Its current corrected-backend candidate is 794,624 bytes with SHA-256 `cc87e9b7dc9bd74d5e14ab079c94cec9e77669953e301d9d32c06c3cefff9f9e`.

`linux-x64-wvb-runner-v1` uses metadata profile `5`. The committed pre-correction candidate remains 778,240 bytes with SHA-256 `8fcfa1fe8dbdb3228c484f284655690d0bf14f4c595eaf820d55cc4ab4f6a294`; the current corrected-backend reconstruction has the same size and SHA-256 `74180ac7cd80192647f46df166a8ea97af17c9676afbe0b2ecb2c8c824db6944`.

`linux-x64-wvo-inspector-v1` uses metadata profile `6`. Its current source candidate is 606,208 bytes with SHA-256 `bdc4817c252ecf2592299a6646161b396bfb251acabc68d3f5d75ff40891541e`.

`linux-x64-native-hosted-verifier-publisher-admission-v1` uses profile `8`.
Its current native candidate is 569,344 bytes with SHA-256
`9bfe16fa751e21a32847f5534eff7de18ba74cfe5b714c63fb6a6589d30d7cad`.

The digest-bound [native read-only front door](Windvale-Native-Wvb-Read-Only-Front-Door.md) intentionally continues to use the previously qualified verifier and inspector applications until the corrected-backend candidates pass the same exact-commit dual-host gate. Candidate reconstruction does not silently replace a qualified ordinary artifact.

## Construction and verification

The Stage 0 routes are:

```text
windvale build Windvale-Compiler-Wvb-Verifier.wvproj
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target windows-x64-verifier-v1
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target linux-x64-verifier-v1
windvale build Windvale-Wvb-Inspector.wvproj
windvale aot Windvale-Wvb-Inspector.wvb --target windows-x64-wvb-inspector-v1
windvale aot Windvale-Wvb-Inspector.wvb --target linux-x64-wvb-inspector-v1
windvale build Windvale-Wvb-Runner.wvproj
windvale aot Windvale-Wvb-Runner.wvb --target windows-x64-wvb-runner-v1
windvale aot Windvale-Wvb-Runner.wvb --target linux-x64-wvb-runner-v1
windvale build Windvale-Wvo-Object.wvproj
windvale aot Windvale-Wvo-Object.wvb --target windows-x64-wvo-inspector-v1
windvale aot Windvale-Wvo-Object.wvb --target linux-x64-wvo-inspector-v1
```

The current canonical-source reconstruction candidate is 148,351 bytes with SHA-256 `519c32fda8d95167d54c723a35860eb80663be5f90ff12e4555ffa6031d505e6`. The digest-bound ordinary front door retains its previously qualified artifact until this candidate passes the exact-commit dual-host promotion gate.

The publisher admitter has no Stage 0 CLI target. Its paired candidates are
constructed by `Construct-Hosted-Verifier-Publisher-Admitter.cmd` and `.sh`
through the native WVB build/lower/link and hosted-container tools.

The canonical inspector WVB is 76,527 bytes with SHA-256 `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753`.

The current native-compiler runner candidate is 90,009 bytes with SHA-256 `3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626`.

The WVO inspector candidate is 60,974 bytes with SHA-256 `b0d0568cb6861c84ea9cad0b77f9722a9141b30c94952e5662aaa3afc47eae0f`.

The verifier writers require exactly one exported `Main() -> i32`, the five canonical capability declarations, and the five canonical verifier-fragment services; they add only the startup-internal UTF-8 service. The WVB and WVO inspector writers require the same entry and capabilities plus the exact eleven-service read-only fragment, and the WVO writer additionally binds the canonical module identity. The runner writers require the same entry and capabilities plus its exact eight-service source fragment, and retain the same qualified startup-internal UTF-8 leaf in the fixed nine-service application bundle. All four profiles construct the outer application, parse it independently, and atomically publish it only after every profile, manifest, startup, import, section, permission, extent, padding, digest, and native-entry check succeeds.

Qualification shares the existing exact-compiler AOT test so the suite compiles the large compiler once. That test compiles the verifier, verifies both deterministic packages, reconstructs both WVA startups, corrupts each outer boundary, runs the current-host application against the exact compiler WVB, rejects a corrupted candidate, and inspects the child for .NET modules or mappings. The same case then consumes the portable verifier core in the build-driver proof rather than compiling a second verifier implementation.

## Retirement boundary

The verifier satisfies one qualified part of the native-retirement inventory: a Windvale-authored semantic verifier runs as a direct Windows or Linux process, and its portable core is shared by the native compiler build driver. The WVB inspector is already qualified; the runner and WVO candidates remove additional ordinary CLI roles once the grouped dual-host and artifact-promotion gates are recorded. The separate [`WVHP 1` console-packager](Windvale-Native-Console-Packager.md) and [`WVHN 1` WVB-to-WVO](Windvale-Native-Wvb-To-Wvo.md) candidates now own bounded version-1 recipe materialization and accepted-subset native lowering, but Stage 0 still constructs both. Complete native x64 lowering, remaining hosted/tool container construction, the reference recovery compiler, test orchestration, release production, and repository automation still invoke .NET. All remaining [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) conditions remain mandatory before .NET leaves the normal path.
