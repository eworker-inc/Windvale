# Windvale project vision

## Purpose

Windvale explores whether AI systems, working under human direction and review, can construct a small, coherent, verifiable computing stack from low-level development tools through useful applications.

The intended result is not an AI-generated novelty kernel. Windvale should be understandable enough to study, structured enough to extend, and useful in stages before its operating system is complete.

Windvale is an E-Worker Inc project whose code and documentation are authored entirely by AI systems under human direction and review. AI systems produce the source and prose; humans define objectives, direct the work, review and test results, decide what the project accepts and publishes, and remain responsible for publication. Windvale is model- and vendor-neutral: any AI system may contribute, and a system or provider is recorded only when technically, legally, or operationally material. Such a record does not imply sponsorship, affiliation, endorsement, or ownership by its provider. “Author” is descriptive project attribution, not a claim that an AI system is a legal person or copyright holder.

As of July 2026, Windvale is among the earliest known open-source efforts to build its own language, compiler, verified bytecode, runtime, assembler, object model, linker, native path, Foundation library, and operating system as one coherent AI-authored stack from an empty project. Earlier AI-authored operating systems and language/toolchain projects exist; the claim is deliberately limited to this combined scope and supported by the dated [earliest-known claim evidence](Earliest-Known-Claim-Evidence.md). Windvale must also distinguish qualified evidence from future scope: its distinctive value is not priority alone, but reproducible evidence carried from one layer to the next.

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

Windvale is open source under the MIT License with a copyright notice naming E-Worker Inc and Windvale contributors. E-Worker Inc initiated and stewards the project. [Decision 0031](../Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) defines vendor-neutral AI authorship, and [Decision 0032](../Decisions/0032-Public-Contribution-And-Governance-Foundation.md) defines contribution, security, governance, support, conduct, and project-identity policy. The official GitHub repository is the private `eworker-inc/Windvale` repository while pre-public inspection continues, with [info@eworker.ca](mailto:info@eworker.ca) as the public business contact. Public visibility and remaining hosting settings are publication-time operations.
