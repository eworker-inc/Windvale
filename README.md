# Windvale

Windvale is an open-source-intended experiment in building a small, understandable computing stack with AI as the primary implementation partner.

The intended stack includes a programming language, portable bytecode, a runtime, an assembler, an object format, a linker, a compact foundation library, and eventually a small operating system. The operating system is the final integration demonstration; the language, tools, and runtime remain independently useful on Windows and Linux.

## Current milestone: Windvale Seed

Windvale Seed is implemented as a dependency-free C# Stage 0 toolchain. It provides:

- A small typed source language with modules, functions, locals, control flow, immutable nominal records and enums, immutable text, integer and byte data, and explicit capabilities
- Foundation `u8`, `u32`, immutable byte slices, and bounded little-endian binary reads
- Deterministic enum names, invariant integer formatting, and bounded text concatenation
- A Windvale-written bounded walker for the complete `.wvb` header and seven section envelopes with structured results and a hosted file shell
- A stack-independent typed Windvale IR
- Deterministic `.wvb` bytecode generation
- A bounded binary reader and mandatory control-flow/type verifier
- A human-readable module inspector and disassembler
- A portable .NET reference runtime
- Explicit hosted arguments, bounded file-byte input, standard output, separate diagnostics, support preflight, and exact capability authorization
- Conformance, malformed-input, determinism, diagnostics, and runtime-limit coverage
- One CLI with `compile`, `inspect`, `verify`, and `run` commands

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
  -- artifacts/Sum-Data.wvb
```

This hosted module reads an explicit file argument through a bounded capability while its pure Windvale functions walk all seven section envelopes, return a nominal status plus section count and failure offset, and format a deterministic summary. With no program arguments it runs embedded valid and adversarial self-checks. It is an envelope inspector, not yet a complete declaration or instruction dumper.

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
- `Runtime/Windvale.Bytecode/` — module contracts, codec, verifier, digest, and inspector
- `Runtime/Windvale.Runtime/` — verified-bytecode reference interpreter and capability host
- `Tools/Windvale.Tool/` — command-line composition
- `Tools/Verify/` — Windows and Linux verification entry points
- `Tests/` — dependency-free Seed conformance runner
- `Examples/Seed/` — portable and hosted example programs
- `Examples/Foundation/` — incremental programs that exercise self-hosting prerequisites
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
- [Source naming conventions](Specifications/Source-Naming.md)
- [Seed bytecode specification](Specifications/Seed-Bytecode.md)
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
- [Open questions](Documents/Project/Open-Questions.md)
- [Development roadmap](Documents/Project/Roadmap.md)

## Development environment

The initial Windows development lane is `D:\windvale\dev01`; its shared source repository is `Z:\Windvale.git`. Development uses `main` until parallel work requires task branches or additional lanes. Read `AGENTS.md` before making non-trivial changes.
