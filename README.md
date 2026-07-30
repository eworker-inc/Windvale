# Windvale

Windvale is an open-source-intended experiment in building a small, understandable computing stack with AI as the primary implementation partner.

The intended stack includes a programming language, portable bytecode, a runtime, an assembler, an object format, a linker, a compact foundation library, and eventually a small operating system. The operating system is the final integration demonstration; the language, tools, and runtime remain independently useful on Windows and Linux.

## Current milestone: Windvale Seed

Windvale Seed is implemented as a dependency-free C# Stage 0 toolchain. It provides:

- A small typed source language with modules, functions, locals, control flow, immutable nominal records and enums, immutable text, integer and byte data, and explicit capabilities
- Foundation `u8`, `u32`, immutable byte slices and concatenation, bounded signed/unsigned little-endian reads and writes, and explicit byte widening
- Strict UTF-8 validation/encoding/decoding, safe ASCII quoting, deterministic enum names, invariant integer formatting, and bounded text construction
- A Windvale-written `.wvb` decoder that validates every section payload, reports declarations, and walks complete instruction streams through a hosted file shell
- A canonical x86-64-first WVO 1.0 object model with sections, symbols, relocations, a bounded C# oracle, and a Windvale-written producer/structural inspector
- A versioned WVA 1 textual assembly contract and Stage 0 assembler that infers definition offsets/sizes and emits verified WVO objects
- A Windvale-written bounded WVA scanner that validates strict UTF-8, source and line limits, line endings, comments, token boundaries, and the exact format header through explicit hosted input
- A stack-independent typed Windvale IR
- Deterministic `.wvb` bytecode generation
- A bounded binary reader and mandatory control-flow/type verifier
- A human-readable module inspector and disassembler
- A portable .NET reference runtime
- Explicit hosted arguments, bounded file-byte input and output, standard output, separate diagnostics, support preflight, and exact capability authorization
- Conformance, malformed-input, determinism, diagnostics, and runtime-limit coverage
- One CLI with module `compile`, `inspect`, `verify`, and `run`, textual `assemble`, plus object `object-inspect` and `object-verify` commands

The open-source intent is established. The exact source license has not been selected yet and must be chosen before the first public source release.

## Requirements

- .NET SDK 10.0.302 or a compatible later patch in the same feature band
- Windows or Linux

The repository pins the SDK in `global.json` and uses no external NuGet packages.

## Quick start

Build and run the complete Seed verifier on Windows:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

On Linux:

```sh
./Tools/Verify/Verify-Seed.sh
```

Compile, verify, inspect, and run the portable example:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv -o artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- verify artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Sum-Data.wvb
```

The result is:

```text
Result: 29
```

Compile and run the hosted example with its capability granted explicitly:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Hello-Windvale.wv -o artifacts/Hello-Windvale.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Hello-Windvale.wvb --allow console.write_line
```

The runtime refuses that module without `--allow console.write_line`.

Compile and run the portable Foundation example that validates a static `.wvb` header:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Foundation/Read-Wvb-Header.wv -o artifacts/Read-Wvb-Header.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Read-Wvb-Header.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Read-Wvb-Header.wvb
```

It exercises `u8`, `u32`, immutable byte slices, and bounded little-endian reads and returns `Result: 1`.

Compile and run the first Windvale-written `wvdump` core:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Foundation/Wv-Dump-Core.wv -o artifacts/Wv-Dump-Core.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Wv-Dump-Core.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Wv-Dump-Core.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 10000000 `
  -- artifacts/Sum-Data.wvb
```

This hosted module reads an explicit file argument through a bounded capability while pure Windvale functions validate WVB 1.5, decode declarations and nominal shapes, walk every instruction, and emit a versioned ASCII-safe line report. It validates the complete module before normal output. With no program arguments it runs embedded valid and adversarial self-checks.

Compile the first Windvale-written WVO object producer, write its representative object through an explicit capability, and inspect it with the independent Stage 0 object reader:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Foundation/Wvo-Object-Core.wv -o artifacts/Wvo-Object-Core.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Wvo-Object-Core.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.write_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 10000000 `
  -- artifacts/Sample.wvo
dotnet run --project Tools/Windvale.Tool -- object-verify artifacts/Sample.wvo
dotnet run --project Tools/Windvale.Tool -- object-inspect artifacts/Sample.wvo
```

The object is exactly 189 bytes. It contains `.text` and `.rodata`, local/export/import symbols, and one x86-64 relative `i32` relocation. The verifier rejects noncanonical or malformed objects before inspection.

Assemble the first WVA 1 source and inspect its canonical object:

```powershell
dotnet run --project Tools/Windvale.Tool -- assemble Examples/Assembler/Hello-Object.wva -o artifacts/Hello-Object.wvo
dotnet run --project Tools/Windvale.Tool -- object-verify artifacts/Hello-Object.wvo
dotnet run --project Tools/Windvale.Tool -- object-inspect artifacts/Hello-Object.wvo
```

The Stage 0 assembler emits exact x86-64 instruction bytes, derives symbol offsets and sizes from named definitions, and records unresolved relative and absolute fixups. It never performs link layout or import resolution. The same WVA contract is the target for the next Windvale-written assembler slice.

Compile and run the first Windvale-written WVA scanner against that source:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Assembler/Wva-Scanner-Core.wv -o artifacts/Wva-Scanner-Core.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Wva-Scanner-Core.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 10000000 `
  -- Examples/Assembler/Hello-Object.wva
```

The scanner reads immutable bytes through one explicit capability, validates WVA source boundaries without host text parsing, and emits a deterministic `wvascan 1` report. With no program arguments it runs embedded valid and adversarial checks. Semantic statement validation and WVO encoding remain the following Windvale-written gates.

## Seed language example

```text
module Sumˉdata profile portable;

data Values: [i32] = [3, 5, 8, 13];

fn Add(Left: i32, Right: i32) -> i32 {
    return Left + Right;
}

export fn Main() -> i32 {
    var Index: i32 = 0;
    var Total: i32 = 0;

    while Index < length(Values) {
        Total = Add(Total, Values[Index]);
        Index = Index + 1;
    }

    return Total;
}
```

## Repository layout

- `Compiler/` — source lexer, parser, semantic analysis, typed WIR, and bytecode lowering
- `Assembler/Windvale.Assembler/` — WVA parser, semantic validation, x86-64 encoding, and WVO production
- `Runtime/Windvale.Bytecode/` — module contracts, codec, verifier, digest, and inspector
- `Runtime/Windvale.Runtime/` — verified-bytecode reference interpreter and capability host
- `Object-Model/Windvale.ObjectModel/` — WVO contracts, codec, verifier, digest, and inspector
- `Tools/Windvale.Tool/` — command-line composition
- `Tools/Verify/` — Windows and Linux verification entry points
- `Tests/` — dependency-free Seed conformance runner
- `Examples/Seed/` — portable and hosted example programs
- `Examples/Foundation/` — incremental programs that exercise self-hosting prerequisites
- `Examples/Assembler/` — canonical WVA sources for the assembler and future linker
- `Specifications/` — implemented source, bytecode, CLI, and conformance contracts
- `Documents/` — architecture, decisions, project direction, and open questions

## Architecture direction

- Windows and Linux are permanent Windvale runtime and development hosts.
- Portable Windvale modules run through Windvale-defined contracts instead of inheriting host semantics.
- Source syntax, typed WIR, distributable bytecode, and future native IR remain distinct contracts.
- Portable, hosted, and system programming have explicit capability profiles.
- The OS should begin as a small vertical system running in virtual machines, with QEMU as the likely automation environment and Hyper-V as an important Windows compatibility target.
- Bootstrap dependencies and AI contributions must be documented honestly and reproducibly.

## Documents

- [Project vision](Documents/Project/Project-Vision.md)
- [Seed implementation](Documents/Architecture/Seed-Implementation.md)
- [Platform and portability model](Documents/Architecture/Platform-And-Portability.md)
- [Compiler bootstrap options](Documents/Architecture/Compiler-Bootstrap-Options.md)
- [Seed language specification](Specifications/Seed-Language.md)
- [Seed immutable records](Specifications/Seed-Records.md)
- [Seed enums and bounded formatting](Specifications/Seed-Enums-And-Formatting.md)
- [Hosted resource boundary](Specifications/Hosted-Resources.md)
- [Foundation byte primitives](Specifications/Foundation-Bytes.md)
- [Windvale wvdump core](Specifications/Wv-Dump-Core.md)
- [Windvale wvdump report](Specifications/Wv-Dump-Report.md)
- [Windvale WVO object core](Specifications/Wvo-Object-Core.md)
- [Source naming conventions](Specifications/Source-Naming.md)
- [Seed bytecode specification](Specifications/Seed-Bytecode.md)
- [Windvale object format](Specifications/Windvale-Object-Format.md)
- [Windvale textual assembly](Specifications/Windvale-Assembly.md)
- [Windvale WVA scanner core](Specifications/Wva-Scanner-Core.md)
- [Seed CLI specification](Specifications/Seed-CLI.md)
- [Seed conformance specification](Specifications/Seed-Conformance.md)
- [Seed verification evidence](Documents/Project/Seed-Verification-Evidence.md)
- [Repository foundation decision](Documents/Decisions/0001-Repository-And-Foundation.md)
- [Seed bootstrap decision](Documents/Decisions/0002-Windvale-Seed-Bootstrap.md)
- [Source naming and mutation decision](Documents/Decisions/0003-Source-Naming-And-Mutation.md)
- [Foundation byte primitives decision](Documents/Decisions/0004-Foundation-Byte-Primitives.md)
- [Immutable nominal records decision](Documents/Decisions/0005-Immutable-Nominal-Records.md)
- [Nominal enums and bounded formatting decision](Documents/Decisions/0006-Nominal-Enums-And-Bounded-Formatting.md)
- [Explicit hosted resources decision](Documents/Decisions/0007-Explicit-Hosted-Resources.md)
- [WvDump payload and report decision](Documents/Decisions/0008-WvDump-Payload-Decoding-And-Safe-Reports.md)
- [Minimal object foundation decision](Documents/Decisions/0009-Minimal-Windvale-Object-Foundation.md)
- [Minimal assembly contract decision](Documents/Decisions/0010-Minimal-Windvale-Assembly-Contract.md)
- [Open questions](Documents/Project/Open-Questions.md)
- [Development roadmap](Documents/Project/Roadmap.md)

## Development environment

The initial Windows development lane is `D:\windvale\dev01`; its shared source repository is `Z:\Windvale.git`. Development uses `main` until parallel work requires task branches or additional lanes. Read `AGENTS.md` before making non-trivial changes.
