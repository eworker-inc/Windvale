# Windvale

Windvale is an open-source experiment in building a small, understandable computing stack with AI as the primary implementation partner.

The intended stack includes a programming language, portable bytecode, a runtime, an assembler, an object format, a linker, a compact foundation library, and eventually a small operating system. The operating system is the final integration demonstration; the language, tools, and runtime should remain independently useful on Windows and Linux.

## Status

Windvale is in its architecture and bootstrap-planning phase. This repository currently contains project decisions and design notes, not an implementation.

The open-source intent is established. The exact source license has not been selected yet and must be chosen before the first public source release.

## Initial direction

- Windows and Linux are permanent Windvale runtime and development hosts, not temporary throwaway targets.
- Portable Windvale modules should run through a Windvale-defined runtime contract instead of inheriting Windows or Linux semantics directly.
- The same source language should eventually target both portable bytecode and native code.
- Portable, hosted, and system programming must have explicit capability profiles.
- The compiler and assembler should share an object-writing model; the compiler should not require text assembly as its internal interface.
- The OS should begin as a small vertical system running in virtual machines, with QEMU as the likely automation environment and Hyper-V as an important Windows compatibility target.
- Bootstrap dependencies and AI contributions must be documented honestly and reproducibly.

## Documents

- [Project vision](Documents/Project/Project-Vision.md)
- [Platform and portability model](Documents/Architecture/Platform-And-Portability.md)
- [Compiler bootstrap options](Documents/Architecture/Compiler-Bootstrap-Options.md)
- [Repository foundation decision](Documents/Decisions/0001-Repository-And-Foundation.md)
- [Open questions](Documents/Project/Open-Questions.md)

## Development layout

The initial Windows development lane is:

```text
D:\windvale\dev01
```

Its shared source repository is:

```text
Z:\Windvale.git
```

Development uses `main` until parallel work requires additional task branches or lanes. See `AGENTS.md` before making non-trivial changes.
