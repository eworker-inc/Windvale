# Windvale project vision

## Purpose

Windvale explores whether people and AI can collaboratively construct a small, coherent, verifiable computing stack from low-level development tools through useful applications.

The intended result is not an AI-generated novelty kernel. Windvale should be understandable enough to study, structured enough to extend, and useful in stages before its operating system is complete.

## Intended stack

```text
Windvale source language
        |
        +-- portable bytecode and modules --> Windvale runtime
        |                                      |-- Windows host
        |                                      |-- Linux host
        |                                      `-- Windvale OS
        |
        `-- native code --> object model --> linker --> native programs and kernel

Foundation library --> portable contracts --> host and OS adapters
```

The umbrella project is named **Windvale**. Its major tools should initially use clear descriptive names such as Windvale Compiler, Windvale Assembler, Windvale Linker, Windvale Runtime, and Windvale OS.

## Success principles

- Produce useful host tools before requiring a mature OS.
- Preserve the Windows and Linux runtime ports after Windvale OS exists.
- Keep one source-language semantic model across bytecode and native execution.
- Make the bytecode/module contract portable, versioned, inspectable, and verifiable.
- Keep platform differences behind explicit contracts and capabilities.
- Reuse compiler, assembler, object, and linker infrastructure instead of building parallel pipelines.
- Reach self-hosting through documented stages rather than obscuring existing-tool dependencies.
- Measure AI contribution through completed specifications, tests, reproducibility, understandable changes, and defects found—not line-count claims.
- Keep each layer independently testable and replaceable through explicit contracts.

## First convincing system

The first Windvale OS milestone should be a small vertical system rather than a broad desktop:

- Boot in a virtual machine through a documented firmware boundary.
- Initialize diagnostics and memory management.
- Load and verify a Windvale bytecode module.
- Run a small interactive or scripted application.
- Expose a minimal filesystem or packaged-resource model.
- Demonstrate the same bytecode module on Windows, Linux, and Windvale OS.
- Shut down cleanly and produce machine-readable test evidence.

The likely first hardware target is x86-64 with UEFI because it provides a practical path to QEMU and Hyper-V. This remains a proposal until the architecture and boot decision is recorded separately.

## Non-goals for the first stages

- A complete desktop environment
- Broad hardware compatibility
- Native Windows and Linux executable backends for every Windvale application
- Compatibility with obsolete experimental Windvale formats
- A new CPU architecture
- A replacement for every existing compiler backend or debugger
- Claims that the stack has no external bootstrap dependencies

## Open-source intent

Windvale is intended to be open source. The license, contribution model, security-reporting process, and public hosting location remain open decisions. No source release should be described as open source until an approved license is present.
