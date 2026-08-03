# Decision 0185: Standalone compiler-WVB verifier applications

- Date: 2026-08-03
- Status: Implemented; cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0170](0170-Compiler-Capacity-Wasm-Wvb-Verifier-Bundle.md) and [Decision 0169](0169-Public-Format3-Compiler-Targets.md)
- Contract: [Hosted compiler-WVB verifier application](../../Specifications/Windvale-Hosted-Verifier-Application.md)

## Context

The exact Windvale compiler already self-reproduces as WVB and runs from deterministic Windows and Linux native packages without loading .NET. The compiler-aligned verifier also already exists in Windvale source and admits that exact compiler through three import-free WebAssembly phases. Normal verification nevertheless still enters the C# CLI and C# bytecode verifier.

The next useful retirement slice is therefore not another compiler package. It is a standalone verifier that can reject an untrusted compiler WVB before later native tools consume it. Reusing the hosted-compiler application profile would falsely declare file-output and formatting authority that the verifier does not need.

Native preflight found one concrete implementation blocker: the original executable-phase function required 2,089 physical frame slots while ABI 22 permits 2,048. Extracting the call and capability opcode paths into typed helper functions reduced the largest frame without changing the ABI or verifier semantics.

## Decision

Implement one fixed `WVHV 1` verifier application profile and public `windows-x64-verifier-v1` / `linux-x64-verifier-v1` AOT targets.

Retain the compiler-capacity semantic, typed-execution, and control-reachability algorithms in Windvale source. The application accepts one path, reads one ordinary bounded byte value, reports the first failed phase, and returns a stable process result.

Grant exactly five application capabilities: console line output, diagnostic line output, file input, process argument, and process argument count. Bind those five native services plus one startup-internal UTF-8 validator. Do not grant or bind file output, enum metadata, text concatenation, integer formatting, or any unused convenience service.

Retain one file snapshot rather than the compiler package's 64 snapshots, and omit the file-output runtime table and scratch allocation. Use distinct canonical WVA startups and a distinct manifest magic/profile/container version. Keep the PE and ELF constructors paired with independent parsers and corruption coverage.

Integrate qualification into the existing exact-compiler AOT test. It already constructs the expensive canonical compiler WVB, so the verifier proof consumes those bytes rather than compiling the compiler again. On the current host, run the raw verifier child against the exact compiler and one corrupted candidate while inspecting loaded modules or mappings for .NET.

## Consequences

- Windvale now has a directly executable, Windvale-authored compiler-WVB admission tool on both permanent hosts.
- The verifier's declared authority matches its actual transitive requirements.
- ABI 22 remains unchanged; the measured frame pressure was solved by source decomposition.
- The verifier uses the compiler-aligned subset and must not be advertised as accepting every semantically valid future WVB.
- Stage 0 still owns build, native lowering, package construction, independent outer verification, and test orchestration.
- The next retirement slice can place this verifier in front of a Windvale-native build driver or native assembler/linker/inspector path without first designing a general hosted application format.

## Reconsideration triggers

Reconsider the fixed profile when one of these becomes true:

- general WVB requires non-empty control-flow joins in the normal compiler output;
- a shared hosted-tool container can preserve equally exact authority and malformed-input boundaries;
- the verifier must stream or segment candidates larger than the ordinary 4 MiB byte value;
- a Windvale-native launcher binds versioned capabilities dynamically rather than through fixed startup tables;
- cross-host qualification exposes different artifact bytes, runtime behavior, or host dependencies.
