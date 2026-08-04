# Windvale hosted compiler-WVB verifier application

## Status and scope

`WVHV 1` is the implemented manifest for packaging the Windvale-written, compiler-aligned WVB verifier as deterministic Windows and Linux x86-64 applications. The verifier applications run without loading .NET. Stage 0 still builds, lowers, packages, and independently verifies these artifacts until the broader native-retirement gate is qualified.

This is a deliberately fixed tool profile, not a general hosted-application format. It enforces the same canonical compiler-aligned rules as the four-artifact verifier bundle from [the WebAssembly contract](Windvale-WebAssembly.md): complete envelope and canonical semantic validation, typed executable-flow validation, control-target reachability, and exact empty-stack join contracts. The native application retains one monolithic typed walk under a `u64` host meter; the WebAssembly bundle partitions that walk only because execution ABI 3 exposes a `u32` meter. General WVB programs that require non-empty control-flow joins remain outside this profile.

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

## Source and authority contract

The verifier module has hosted authority and declares exactly these five capabilities in canonical order:

1. `console.write_line(text) -> void`;
2. `diagnostic.write_line(text) -> void`;
3. `file.read_bytes(text) -> bytes`;
4. `process.argument(u32) -> text`;
5. `process.argument_count() -> u32`.

The module's native fragment requires exactly five corresponding services. Startup additionally binds `text.utf8_is_valid` to validate the host argument snapshot. That sixth service is marked startup-internal in the manifest and is not an application capability grant. File output, enumeration metadata, text concatenation, integer formatting, and every other service are absent.

The verifier reads at most one candidate snapshot. The candidate remains bounded by the ordinary 4 MiB `bytes` limit. The runtime retains one file-input snapshot slot and no file-output table or output scratch space.

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
u32   service count = 6
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
u32   profile flags = 2
u64   instruction budget = 16,000,000,000
bytes native-image SHA-256[32]
```

Five 16-byte capability records follow the header. Each contains the fixed capability identity, its service identity, exact signature identity, and contract major version `1`.

Six 64-byte service records follow the capability records in this exact order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `text.utf8_is_valid`;
6. `diagnostic.write_line`.

Each service record binds its service identity, capability identity or zero for startup-internal support, service-table slot, platform-adapter identity, image offset, code extent, flags, and SHA-256 digest. All reserved fields and the unused metadata tail are zero. Verification reconstructs the complete expected bundle and rejects any changed identity, ordering, address, extent, flag, digest, reserved byte, native entry, or target.

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

Their assembled WVO identities are respectively:

- Windows: `755ffb99cba6a838dd9eec353ce72d4adfb3af130ec4bce5a2278828dd136616`;
- Linux: `08a7afefb69904af8d8c899a86bec76e957dfe255d397dbd9015d9acaa018ae8`.

The package constructors retain only the zero-relocation templates plus typed patch plans. Tests reassemble the WVA sources, resolve local and imported symbols independently, and require byte equality with each packaged startup.

## Windows container

`windows-x64-verifier-v1` emits a PE32+ console application with RX `.text`, RW/NX `.data`, and read-only discardable `.reloc` sections. The startup imports eleven functions from `KERNEL32.dll` and `CommandLineToArgvW` from `SHELL32.dll`. It imports no CLR or C runtime and exposes no file-output service. `CreateFileW` is used by the trusted startup with read-only access for the one bounded input snapshot.

The current canonical application is 961,536 bytes with SHA-256 `cac82b26c7af4edea01a808db718e66e65fd859f421d5e73f144b017f390bc59`.

## Linux container

`linux-x64-verifier-v1` emits a sectionless x86-64 static-PIE ELF with read-only headers, RX text, RW/NX runtime data, a format-4 Windvale note, and a 64 MiB RW/NX GNU stack declaration. It has no interpreter, dynamic table, imports, or loader relocations and uses only checked startup syscalls.

The current canonical application is 962,560 bytes with SHA-256 `d99f5d9c95f1ab7e731eaf4ea7f15e48a19cc72e689f99d1b00d5a58f2984ede`.

## Construction and verification

The Stage 0 routes are:

```text
windvale build Windvale-Compiler-Wvb-Verifier.wvproj
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target windows-x64-verifier-v1
windvale aot Windvale-Compiler-Wvb-Verifier.wvb --target linux-x64-verifier-v1
```

The canonical WVB is 118,496 bytes with SHA-256 `19760a4438a48c945de3e39fd612ed72f3ea3a33373b5d9da09cd1e2411938d7`.

Both public writers require exactly one exported `Main() -> i32`, the five canonical capability declarations, and the five canonical fragment services. They add only the startup-internal UTF-8 service, construct the outer application, parse it independently, and atomically publish it only after every manifest, startup, import, section, permission, extent, padding, digest, and native-entry check succeeds.

Qualification shares the existing exact-compiler AOT test so the suite compiles the large compiler once. That test compiles the verifier, verifies both deterministic packages, reconstructs both WVA startups, corrupts each outer boundary, runs the current-host application against the exact compiler WVB, rejects a corrupted candidate, and inspects the child for .NET modules or mappings. The same case then consumes the portable verifier core in the build-driver proof rather than compiling a second verifier implementation.

## Retirement boundary

These executables satisfy one part of the native-retirement inventory: a Windvale-authored semantic verifier can run as a direct Windows or Linux process, and its portable core is now shared by the native compiler build driver. They do not retire Stage 0. The normal CLI, native x64 lowering, package constructors, assembler, linker, inspector, reference recovery compiler, test runner, and repository automation still invoke .NET even though the packaged driver now owns a bounded Project 1 source-to-WVB path. Cross-host qualification and all remaining [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) conditions remain mandatory before .NET leaves the normal path.
