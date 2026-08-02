# Windvale project vision

## Purpose

Windvale explores whether AI systems, working under human direction and review, can construct a small, coherent, verifiable computing stack from low-level development tools through useful applications.

The intended result is not an AI-generated novelty kernel. Windvale should be understandable enough to study, structured enough to extend, and useful in stages before its operating system is complete.

Windvale is an [E-Worker Inc](https://eworker.ca) project whose code and documentation are authored entirely by AI systems under human direction and review. AI systems produce the source and prose; humans define objectives, direct the work, review and test results, decide what the project accepts and publishes, and remain responsible for publication. Windvale is model- and vendor-neutral: any AI system may contribute, and a system or provider is recorded only when technically, legally, or operationally material. Such a record does not imply sponsorship, affiliation, endorsement, or ownership by its provider. “Author” is descriptive project attribution, not a claim that an AI system is a legal person or copyright holder.

As of July 2026, Windvale is among the earliest known open-source efforts to build its own language, compiler, verified bytecode, runtime, assembler, object model, linker, native path, Foundation library, and operating system as one coherent AI-authored stack from an empty project. Earlier AI-authored operating systems and language/toolchain projects exist; the claim is deliberately limited to this combined scope and supported by the dated [earliest-known claim evidence](Earliest-Known-Claim-Evidence.md). Windvale must also distinguish qualified evidence from future scope: its distinctive value is not priority alone, but reproducible evidence carried from one layer to the next.

## Intended stack

```text
Windvale source language
        |
        +-- portable verified bytecode --> Windvale-native execution
        |                                  |-- interpreter
        |                                  |-- baseline/optimizing JIT
        |                                  `-- cached or install-time compilation
        |
        `-- shared native backend --> object model/linker --> AOT programs and kernel

Windows adapter ---------+
Linux adapter -----------+--> runtime capabilities and process services
Windvale OS adapter -----+

Foundation library --> portable contracts --> host and OS adapters
```

The umbrella project is named **Windvale**. Its major tools should initially use clear descriptive names such as Windvale Compiler, Windvale Assembler, Windvale Linker, Windvale Runtime, and Windvale OS.

## Success principles

- Produce useful host tools before requiring a mature OS.
- Preserve the Windows and Linux runtime ports after Windvale OS exists.
- Keep one source-language semantic model across bytecode and native execution.
- Make the bytecode/module contract portable, versioned, inspectable, and verifiable.
- Share native ABI, machine lowering, typed relocation, and runtime contracts across JIT and AOT rather than building parallel native compilers.
- Retire C#/.NET from the normal Windows and Linux workflow only after a reproducible Windvale-native compiler, verifier, runtime, toolchain, and recovery seed are qualified; preserve the final Stage 0 evidence as bootstrap history.
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

The accepted first boot environment is x86-64 with UEFI 2.11, QEMU as the primary automated VM, and Hyper-V Generation 2 as the later Windows compatibility target. [Decisions 0044](../Decisions/0044-First-X64-Uefi-Boot-Environment.md) through [0094](../Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) establish deterministic PE32+, firmware exit, kernel handoff, owned memory/stack/page tables, WVA shutdown and normalized faults, fixed in-guest WVB admission, protected processes, a Windvale init service, and Windvale-written bytecode interpretation at CPL3. Probe 24 is the cross-host-qualified OS baseline; candidate probe 25 derives and validates the embedded module's WVB sections before executing its bounded profile. This is substantial vertical source-to-machine evidence, not yet a runtime-supplied or general WVB loader/verifier, JIT, scheduler, resource namespace, general trap system, or complete kernel runtime.

## Non-goals for the first stages

- A complete desktop environment
- Broad hardware compatibility
- Native Windows and Linux executable backends for every Windvale application
- Compatibility with obsolete experimental Windvale formats
- A new CPU architecture
- A replacement for every existing compiler backend or debugger
- Claims that the stack has no external bootstrap dependencies

The accepted native execution and retirement direction is defined by [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Native execution and .NET retirement](../Architecture/Native-Execution-And-Dotnet-Retirement.md). It is a destination and qualification plan, not current implementation status.

## Open-source intent

Windvale is open source under the MIT License with a copyright notice naming E-Worker Inc and Windvale contributors. E-Worker Inc initiated and stewards the project. [Decision 0031](../Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) defines vendor-neutral AI authorship, and [Decision 0032](../Decisions/0032-Public-Contribution-And-Governance-Foundation.md) defines contribution, security, governance, support, conduct, and project-identity policy. The official repository is public at [`eworker-inc/Windvale`](https://github.com/eworker-inc/Windvale), with [info@eworker.ca](mailto:info@eworker.ca) as the public business contact. The initial publication-baseline record and ongoing public operations remain active project-foundation work.
