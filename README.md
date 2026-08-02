# Windvale

Windvale is an MIT-licensed [E-Worker Inc](https://eworker.ca) experiment to build an entire, understandable computing stack from the ground up. Its code and documentation are authored entirely by AI systems under human direction and review.

AI systems produce the source and prose. Humans define the objectives, direct the work, review and test the results, decide what the project accepts and publishes, and remain responsible for publication. [E-Worker Inc](https://eworker.ca) provides project stewardship.

At its center is a **new programming language**, together with its compiler, portable bytecode, verified runtime, assembler, object model, linker, and Foundation library. The long-term integration goal is a **new small operating system** capable of loading and running the same verified Windvale programs that run on Windows and Linux. The language and tools remain independently useful before the operating system is complete.

**[Visit windvale.ca](https://windvale.ca/)** · **[Try the browser playground](https://windvale.ca/playground/)** · **[Support Windvale](https://windvale.ca/support/)**

## Project overview

![Windvale project overview for July 2026](Documents/Project/Images/Windvale-Progress-July-2026.png)

*This July 2026 overview was refreshed on 1 August 2026 and is a periodic visual snapshot. The progress dashboard, accepted decisions, and qualification evidence govern where the project has advanced beyond what the image shows.*

## What works today

Windvale Seed is experimental, but several paths already work end to end:

```text
Windvale source -> deterministic WVB -> verification -> execution on Windows or Linux
Windvale assembly -> verified WVO object -> deterministic linked x86-64 image
Portable Sum-Data.wv -> the same canonical WVB -> Windows, Linux, and Windvale OS
Portable Function-Only.wv -> the same canonical WVB -> Windows, Linux, and Windvale OS
```

The current Stage 0 toolchain includes the typed language, compiler, bytecode verifier, portable reference runtime, assembler, object model, linker, CLI, editor support, and focused Foundation modules. A Windvale-written compiler reproduces its canonical bytecode compiler exactly. Qualified Probe 32 runs the exact 815-byte cross-compiler `Function-Only.wv` fixture in protected user-space client generations: four functions and four scalar families execute 199 guest instructions to result `6`. Qualified [Decision 0108](Documents/Decisions/0108-Native-One-Byte-Construction.md) advances the shared backend to ABI 19 with exact one-byte construction while retaining ABI 18's compact typed physical values, canonical semantic IDs, and 2,048-cell limit. Exact implementation commit `a35c348` passes all 68 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12 in GitHub [Verify run 30764320109](https://github.com/eworker-inc/Windvale/actions/runs/30764320109); all four Windows pinned-QEMU scenarios retain their exact identities. Native compiler preflight now reaches `Bytesˉfromˉu16ˉlittle`. C# remains the bootstrap, host adapter, packaging path, and recovery implementation.

Windvale is not production-stable. Native compiler execution, the general native toolchain, broader runtime services, and the operating system remain active milestones. For current detail, use the authoritative documents instead of treating this overview as a cumulative status log:

- [Progress dashboard](Documents/Project/Progress.md) — concise indicators and working paths
- [Development roadmap](Documents/Project/Roadmap.md) — phase gates, sequencing, and current focus
- [Seed implementation](Documents/Architecture/Seed-Implementation.md) — component ownership and implemented boundaries
- [Specification index](Specifications/README.md) — current language, format, runtime, native, and OS contracts
- [Qualification evidence](Documents/Project/Seed-Verification-Evidence.md) — exact cross-host history and artifact identities

## Browser playground

The experimental Stage 0 playground compiles, verifies, and runs Windvale entirely in the browser through .NET WebAssembly. It reuses the C# reference compiler and interpreter as the current semantic oracle, produces canonical WVB, and exposes only explicitly checked console and diagnostic capabilities. Its bounded portable subset is also lowered by the Windvale-authored backend and executed in a disposable Web Worker for differential evidence.

**[Open the Windvale Playground](https://windvale.ca/playground/)**

Run it locally:

```powershell
dotnet run --project Tools/Windvale.Playground
```

The [playground host specification](Specifications/Browser-Playground.md) defines its limits and non-claims. This is a browser host for the language, not a browser boot of Windvale OS and not yet an accepted permanent WebAssembly compiler target.

The separate [experimental WebAssembly target](Specifications/Windvale-WebAssembly.md) proves the first lower layer in Windvale source: cross-host-qualified portable `.wv` code revalidates canonical WVB and lowers a bounded straight-line `i32` instruction stream to deterministic import-free Wasm. Execution ABI 1 preserves successful results, checked add/subtract/multiply/negate overflow as `WVR3007`, and exact instruction accounting. The playground now runs selected output in a disposable worker and compares it with the reference interpreter; this does not replace its .NET compiler, verifier, general runtime, or fallback path.

## Quick start

Requirements:

- .NET SDK 10.0.302 or a compatible later patch in the same feature band
- Windows or Linux

The repository pins the SDK in `global.json` and uses no external NuGet packages.

Compile, verify, inspect, and run the portable example:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv -o artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- verify artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Sum-Data.wvb
```

The result is `Result: 29`.

For verification tiers, hosted capabilities, project builds, bootstrap convergence, assembly, linking, and component examples, continue with the [Seed development runbook](Documents/Runbooks/Seed-Development.md).

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

- `Compiler/` — parsing, semantic analysis, Windvale IR, and code generation
- `Assembler/` — Windvale and reference WVA parsing, instruction encoding, and WVO production
- `Object-Model/` — structured sections, symbols, relocations, and serialization
- `Linker/` — symbol resolution, layout, relocation, and image production
- `Runtime/` — bytecode loading, verification, execution, and host adaptation
- `Foundation/` — reusable Windvale APIs and implementations needed by current tools
- `Operating-System/` — boot, kernel, processes, services, and platform work
- `Tools/` — CLI, editors, website support, inspection, and verification tools
- `Tests/` — conformance, integration, malformed-input, and reproducibility coverage
- `Specifications/` — implemented language, bytecode, module, native, and platform contracts
- `Documents/` — architecture, decisions, project direction, evidence, and runbooks
- `Website/` — the public [windvale.ca](https://windvale.ca/) project site

## Architecture direction

- Windows and Linux are permanent Windvale runtime and development hosts, not the semantic definition of the language.
- Portable Windvale modules run through Windvale-defined contracts instead of inheriting host semantics.
- Source syntax, typed WIR, distributable bytecode, native IR, object files, and executable images remain distinct contracts.
- Portable, hosted, and system programming have explicit capability profiles.
- The durable [Windvale OS architecture](Documents/Architecture/Windvale-Os-Architecture.md) uses a small capability-oriented kernel written primarily in `.wv`, a bounded `.wva` machine layer, and isolated Windvale services.
- C# and .NET remain Stage 0 until the documented native-retirement gate is qualified on Windows and Linux.
- Bootstrap dependencies and AI contributions must be documented honestly and reproducibly.

## Documentation

Start with the [documentation guide](Documents/README.md). It separates current status, enduring architecture, specifications, accepted decisions, historical evidence, and operational records.

## License, stewardship, and participation

Windvale is open source under the [MIT License](LICENSE). Copyright © 2026 [E-Worker Inc](https://eworker.ca) and Windvale contributors. “Author” and “authored” describe how the project was produced; they do not assert that an AI system is a legal person or copyright holder. See [Decision 0031](Documents/Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) for the project-wide attribution policy.

- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)
- [Governance](GOVERNANCE.md)
- [Code of conduct](CODE_OF_CONDUCT.md)
- [Support](SUPPORT.md)
- [Project identity](TRADEMARKS.md)
- [Changelog](CHANGELOG.md)

Read [AGENTS.md](AGENTS.md) and [CONTRIBUTING.md](CONTRIBUTING.md) before making non-trivial changes.
