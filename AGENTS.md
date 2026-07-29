# Agent Handbook

## Purpose

This document gives people and AI agents the durable rules needed to develop Windvale safely and coherently.

Windvale is an open-source experiment in constructing a small computing stack: language, compiler, bytecode, runtime, assembler, object model, linker, foundation library, tools, and operating system. The goal is not merely to generate code with AI. The result must be understandable, testable, reproducible, and useful independently at each layer.

## Quick start

- Read this file before making non-trivial changes.
- Use the active development lane. The initial Windows lane is `D:\windvale\dev01`.
- Use `Z:\Windvale.git` as the shared repository remote on the current Windows development environment.
- Use `main` as the default branch until the repository establishes a different branching policy.
- Before changes, run `git status --short --branch` and `git pull --ff-only`.
- Preserve unrelated work and stage only files belonging to the current task.
- Do not add implementation merely to make a design document appear complete.
- Do not commit secrets, private keys, credentials, local SDK installations, build caches, virtual disks, firmware images, or machine-specific configuration.

## Durable project direction

- Windows and Linux are permanent Windvale hosts. They are not the semantic definition of the language.
- Windvale defines its own source semantics, bytecode, module format, runtime contracts, capability profiles, and standard-library behavior.
- The same source language should eventually support portable bytecode and native compilation.
- Portable applications target Windvale contracts. Host adapters map those contracts to Windows, Linux, and Windvale OS.
- Host-specific capabilities remain explicit and must not leak into portable modules.
- The OS is a vertical integration target, not a reason to postpone useful host tools and libraries.
- Keep bootstrap dependencies explicit. A bootstrap tool may be temporary, but its role and replacement path must be documented.
- Prefer a small coherent path over parallel compilers, runtimes, object models, or compatibility layers.
- Do not preserve obsolete experimental formats during early development unless a named compatibility case is explicitly required. Update fixtures and tests to the current contract.

## Architecture boundaries

Keep these responsibilities distinct even if early prototypes temporarily share a project or process:

- `Specifications/` defines source semantics, bytecode, modules, object records, ABI rules, and platform contracts.
- `Compiler/` owns parsing, semantic analysis, Windvale IR, and code-generation orchestration.
- `Assembler/` owns textual assembly parsing and instruction encoding.
- `Object-Model/` owns structured sections, symbols, relocations, and serialization contracts.
- `Linker/` owns symbol resolution, layout, relocation, and final image production.
- `Runtime/` owns bytecode loading, validation, execution, memory/runtime services, and host adaptation.
- `Libraries/` owns reusable Windvale APIs and their portable or capability-specific implementations.
- `Operating-System/` owns boot, kernel, drivers, processes, and Windvale-native platform services.
- `Tools/` owns repository development, inspection, generation, and verification utilities.
- `Tests/` owns conformance, differential, integration, malformed-input, and reproducibility coverage.
- `Documents/` owns architecture, decisions, project direction, and runbooks.

Create these source areas only when implementation begins; do not add empty directory scaffolding without an owner or contract.

## Compiler and format rules

- Treat source syntax, semantic IR, distributable bytecode, native machine IR, object files, and executable images as different contracts.
- Do not use a backend format such as C, LLVM IR, WebAssembly, PE, or ELF as the implicit definition of Windvale semantics.
- Give every serialized format an explicit version, validation boundary, size limits, and malformed-input tests.
- Bytecode verification happens before execution. Revalidate indices, offsets, lengths, types, capabilities, imports, and control-flow targets.
- Compiler phases consume and produce explicit models. Avoid hidden cross-phase mutation and global compiler state.
- Prefer immutable evidence between phases. Diagnostics must identify the phase, source location when available, and violated rule.
- The compiler and assembler should converge on one structured native object model and object writer.
- Keep architecture-specific instruction selection, encoding, relocation, and ABI policy behind explicit contracts.
- Reproducible output is a product feature: identical inputs, tool versions, and options should produce identical bytes.

## Capability and safety rules

- Classify modules as portable, hosted, or system profile before implementation.
- Portable code must not depend on native paths, host handles, ambient process state, privileged instructions, or undocumented host behavior.
- Hosted and system operations require declared capabilities. Unsupported and unauthorized operations must fail explicitly.
- Unsafe operations must be syntactically and contractually visible; do not allow safety-sensitive behavior through ordinary convenience APIs.
- Treat every loaded module, object file, package, symbol table, relocation, and debug record as untrusted input.
- Use checked arithmetic for file offsets, memory sizes, indices, and address calculations.
- Define integer widths, overflow behavior, byte order, alignment, text encoding, and concurrency semantics rather than inheriting host defaults.

## Code style and naming

Carry the established E-Worker host-code convention into Windvale bootstrap code where the implementation language supports it:

- TypeScript and C# identifiers controlled by Windvale use the macron separator `ˉ` (U+02C9) between semantic words and start with capital case, such as `Moduleˉreader` and `Readˉsection`.
- Do not leave camelCase islands inside macron-separated identifiers.
- External APIs, wire formats, standard-library members, and persisted schemas keep required external casing only at the boundary. Map them to internal names.
- Constants use `ALL_CAPS_WITH_UNDERSCORES`.
- File and folder names capitalize the first word and use `-` between words, such as `Object-Model` and `Module-Reader.cs`.
- C, assembly, exported ABI symbols, and generated identifiers use a deliberately specified ASCII-safe convention for toolchain portability.
- The future Windvale source-language style is not decided by these host implementation rules. Specify it separately with the language grammar and conventions.
- Prefer explicit types and contracts over loosely shaped objects.
- Prefer focused modules named after their owned capability over broad `Helpers`, `Utils`, `Common`, or numbered-part files.
- Add succinct comments only for invariants, format rules, unsafe reasoning, or other non-obvious logic.
- Keep dependencies explicit; importing a declaration must not perform global registration or other runtime work.

## Testing and verification

- Run the narrowest reliable verifier for the changed behavior, then broaden only in proportion to risk.
- Every parser and binary reader needs valid, boundary, truncated, oversized, inconsistent, and malicious-input coverage.
- Use golden byte fixtures only where exact bytes are part of the contract. Pair them with structural assertions so failures remain diagnosable.
- Use differential tests when a temporary C backend, reference VM, native backend, or host adapter should implement the same semantics.
- Test deterministic builds by comparing output bytes, not only behavior.
- Keep a reference implementation simple enough to act as an oracle even when faster implementations appear.
- Documentation-only changes normally require `git diff --check`, link/path inspection, and review of the changed Markdown.
- Code changes require the relevant package checks and focused conformance tests once those commands exist.
- State exactly which broader checks were not run and why.

Windvale Seed code changes normally require the host verifier:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

On Linux, use `./Tools/Verify/Verify-Seed.sh`. Changes to portable semantics, bytecode, serialization, runtime behavior, or golden hashes require reports from both hosts before cross-host conformance is claimed.

## Documentation discipline

- Put enduring architecture under `Documents/Architecture/`.
- Put dated decisions under `Documents/Decisions/`, including status, context, decision, consequences, and reconsideration triggers.
- Put project framing and unresolved product questions under `Documents/Project/`.
- Distinguish accepted direction from proposals and experiments.
- Update documentation when semantics, formats, architecture, bootstrap stages, security boundaries, or durable workflows change.
- Do not record aspirational features as implemented behavior.

## Git workflow

Before changes:

```powershell
git status --short --branch
git pull --ff-only
```

After changes:

```powershell
git status --short --branch
git add <task-files>
git commit -m "<task-scoped message>"
git push
```

If the remote moved, use `git pull --rebase` and then push. Do not overwrite shared history.
