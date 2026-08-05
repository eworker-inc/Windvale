# Windvale hosted read-only WVB tool applications

## Status and scope

`WVHV 1` is the implemented manifest for packaging two Windvale-written read-only WVB tools as deterministic Windows and Linux x86-64 applications: the compiler-aligned verifier and the complete structural `wvdump` inspector. Both run without loading .NET. The verifier applications are cross-host qualified at exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` in GitHub [Verify run 30964566192](https://github.com/eworker-inc/Windvale/actions/runs/30964566192); the inspector profile is an implemented candidate pending its own dual-host qualification. Stage 0 still lowers, packages, and independently verifies the applications until the broader native-retirement gate is qualified.

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

The canonical inspector project is [`Windvale-Wvb-Inspector.wvproj`](../Windvale-Wvb-Inspector.wvproj), which reuses the checked-in Windvale source `Examples/Foundation/Wv-Dump-Core.wv` without a second implementation. Its invocation is:

```text
wvdump <module.wvb>
```

Success writes the deterministic [`wvdump 1`](Wv-Dump-Report.md) line report and returns `0`. Structural rejection writes one stable diagnostic and returns `2`; invalid invocation returns `64`. Ordinary inspection first runs `wvverify` and invokes `wvdump` only after semantic acceptance.

## Source and authority contract

Both modules have hosted authority and declare exactly these five capabilities in canonical order:

1. `console.write_line(text) -> void`;
2. `diagnostic.write_line(text) -> void`;
3. `file.read_bytes(text) -> bytes`;
4. `process.argument(u32) -> text`;
5. `process.argument_count() -> u32`.

The verifier fragment requires exactly five corresponding services. Its startup additionally binds `text.utf8_is_valid` to validate the host argument snapshot. That sixth service is marked startup-internal in the manifest and is not an application capability grant.

The inspector fragment requires the same read-only host services plus the capability-free `text.utf8_is_valid`, `enum.name`, `text.concat`, `text.quote`, `i32.format`, and `u32.format` report services. Its exact eleven-service sequence contains every native service through `u32.format` and excludes `file.write_bytes`. Neither profile has a file-output table, file-output scratch space, or file-write authority.

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
u32   service count = 6 verifier, 11 inspector
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
u32   profile flags: 2 compiler-WVB verifier, 4 WVB inspector
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

The current canonical application is 1,007,104 bytes with SHA-256 `f15422397ad890909f481f131f945e25651c858695ba5ce58b2a7305b34647f0`.

`windows-x64-wvb-inspector-v1` uses the same outer PE and read-only host imports with metadata profile `4`. Its implemented candidate is 795,136 bytes with SHA-256 `61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381`.

## Linux container

`linux-x64-verifier-v1` emits a sectionless x86-64 static-PIE ELF with read-only headers, RX text, RW/NX runtime data, a format-4 Windvale note, and a 64 MiB RW/NX GNU stack declaration. It has no interpreter, dynamic table, imports, or loader relocations and uses only checked startup syscalls.

The current canonical application is 1,007,616 bytes with SHA-256 `dd98cd8f42ee8237b030d96dd1305e23843f92ae7dfd92469a67579e2cbe718a`.

`linux-x64-wvb-inspector-v1` uses the same static outer ELF and metadata profile `4`. Its implemented candidate is 794,624 bytes with SHA-256 `d3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627`.

## Construction and verification

The Stage 0 routes are:

```text
windvale build Windvale-Compiler-Wvb-Verifier.wvproj
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target windows-x64-verifier-v1
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target linux-x64-verifier-v1
windvale build Windvale-Wvb-Inspector.wvproj
windvale aot Windvale-Wvb-Inspector.wvb --target windows-x64-wvb-inspector-v1
windvale aot Windvale-Wvb-Inspector.wvb --target linux-x64-wvb-inspector-v1
```

The canonical WVB is 125,721 bytes with SHA-256 `259db7fc70679153982ca70843cf002e87b786d04ebeb0eafb628207f44c723f`.

The canonical inspector WVB is 76,527 bytes with SHA-256 `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753`.

The verifier writers require exactly one exported `Main() -> i32`, the five canonical capability declarations, and the five canonical verifier-fragment services; they add only the startup-internal UTF-8 service. The inspector writers require the same entry and capabilities plus the exact eleven-service read-only fragment. Both construct the outer application, parse it independently, and atomically publish it only after every profile, manifest, startup, import, section, permission, extent, padding, digest, and native-entry check succeeds.

Qualification shares the existing exact-compiler AOT test so the suite compiles the large compiler once. That test compiles the verifier, verifies both deterministic packages, reconstructs both WVA startups, corrupts each outer boundary, runs the current-host application against the exact compiler WVB, rejects a corrupted candidate, and inspects the child for .NET modules or mappings. The same case then consumes the portable verifier core in the build-driver proof rather than compiling a second verifier implementation.

## Retirement boundary

The verifier satisfies one qualified part of the native-retirement inventory: a Windvale-authored semantic verifier runs as a direct Windows or Linux process, and its portable core is shared by the native compiler build driver. The inspector candidate removes a second ordinary CLI role once dual-host qualification is recorded. Neither retires Stage 0 packaging or test orchestration. Native x64 lowering, package constructors, assembler, linker, reference recovery compiler, test runner, and repository automation still invoke .NET. All remaining [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) conditions remain mandatory before .NET leaves the normal path.
