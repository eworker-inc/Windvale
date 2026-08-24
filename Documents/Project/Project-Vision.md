# Windvale project vision

## Purpose

Windvale is an AI-led research and development project building a small,
coherent computing and assurance stack from low-level development tools through
useful applications, AI workloads, firmware-facing systems, and an operating
system. It investigates how much of that stack can remain independently
inspectable, reproducible, and verifiable under explicit resource and authority
contracts.

The intended result is not an AI-generated novelty kernel. Windvale should be
understandable enough to study, structured enough to extend, useful in stages
before its operating system is complete, and capable of carrying evidence from
source and immutable inputs through build, execution, and externally visible
effects.

Windvale began with a feasibility question: whether AI systems under human
direction could construct a coherent, reviewable computing stack from an empty
project. The signed `v0.1.0` preview completed that initial experimental phase by
publishing an installable, qualified native-only product preview. Windvale is now
in full research and development. Individual proposals, prototypes, profiles,
and measurements may remain experimental, but the project itself is not
described as an experiment.

Windvale is an [E-Worker Inc](https://eworker.ca) project whose code and project
documentation are authored entirely by AI systems. AI systems perform most
research, design, implementation, documentation, test construction, diagnosis,
and technical review. Humans establish purpose and constraints, direct the
research, review selected and risk-sensitive results, accept consequential
decisions, authorize publication, and retain institutional and legal
responsibility. Acceptance does not imply that a human inspected every generated
line. [Decision 0849](../Decisions/0849-Define-AI-Led-Research-And-Review-Evidence.md)
defines the exact division of work and review-evidence classes.

Windvale is model- and vendor-neutral: any AI system may contribute, and a
system or provider is recorded only when technically, legally, or operationally
material. Such a record does not imply sponsorship, affiliation, endorsement,
ownership, or partnership by its provider. “Author” is descriptive project
attribution, not a claim that an AI system is a legal person or copyright
holder. **Frontier model** is a dated comparative description rather than a
permanent project role.

As of July 2026, Windvale is among the earliest known publicly developed efforts to build its own language, compiler, verified bytecode, runtime, assembler, object model, linker, native path, Foundation library, and operating system as one coherent AI-authored stack from an empty project. Earlier AI-authored operating systems and language/toolchain projects exist; the claim is deliberately limited to this combined scope and supported by the dated [earliest-known claim evidence](Earliest-Known-Claim-Evidence.md). Windvale must also distinguish qualified evidence from future scope: its distinctive value is not priority alone, but reproducible evidence carried from one layer to the next.

## Intended stack

```text
Windvale source language
        |
        +-- canonical verified bytecode --> Windvale-native execution
        |                                  |-- interpreter
        |                                  |-- baseline/optimizing JIT
        |                                  `-- cached or install-time compilation
        |
        `-- shared native backend --> object model/linker --> AOT programs and kernel

Foundation library -------> deterministic capability-free contracts
Platform libraries -------> versioned capability requirements
                              |-- Windows providers
                              |-- Linux providers
                              `-- Windvale OS providers and services
Platform extensions ------> explicit target-scoped providers

Agent/workload identity ---> bounded cognitive operations
                               |-- local CPU or accelerator provider
                               |-- admitted inference infrastructure
                               `-- separately authorized remote model
```

The umbrella project is named **Windvale**. Its major tools should initially use clear descriptive names such as Windvale Compiler, Windvale Assembler, Windvale Linker, Windvale Runtime, and Windvale OS.

[Decision 0179](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md) defines the product character behind this stack: Windvale is a deterministic, capability-oriented language for applications and systems. “Code and data together” means that canonical code may be packaged with typed immutable data, resources, manifests, identities, and declared authority; it does not make self-modifying code, ambient files, or mutable databases part of the language model. [Decision 0184](../Decisions/0184-Language-Syntax-And-Operator-Evolution.md) and the [language-design guide](../Architecture/Language-Design.md) keep future syntax approachable while retaining explicit mutation, checked same-type operators, exhaustive results, bounded collections, and visible resource ownership.

## Assurance and standardization ambition

Windvale's long-term product proposition is broader than one language or
operating system:

> A producer should be able to publish source, firmware and software artifacts,
> immutable dependencies, build and tool identities, declared authority,
> conformance results, and signed provenance so that a recipient can inspect and
> independently verify the exact claims that cross an organizational or national
> boundary.

One Windvale stack means one coherent assurance path across the layers Windvale
controls. It does not mean that bytecode verification proves application intent,
that reproducibility proves source correctness, or that Windvale automatically
verifies silicon, microcode, third-party device firmware, host kernels, external
models, or opaque providers. Every assurance statement names its subject,
property, verifier, trust root, environment, evidence, and remaining non-claims.

Windvale aims to mature into independently implementable specifications backed
by a reference stack, conformance suites, hostile-input corpora, reproducible
artifacts, and machine-readable assurance evidence. It is not presently a
formal, national, or international standard. Formal standardization would
require stable scopes, independent implementations, external stakeholders,
interoperability, consensus governance, and an explicit specification, patent,
trademark, conformance, and licensing policy.

The [cross-border product assurance and standardization strategy](Cross-Border-Product-Assurance-And-Standardization.md)
owns this product thesis, its claim vocabulary, ecosystem relationships, first
use cases, and decision gates. It treats established provenance, bill-of-
materials, update, firmware-resilience, and evaluation standards as
interoperability inputs rather than renaming them as Windvale inventions.

## Success principles

- Produce useful host tools before requiring a mature OS.
- Preserve the Windows and Linux runtime ports after Windvale OS exists.
- Keep one source-language semantic model across bytecode and native execution.
- Make the bytecode/module contract portable, versioned, inspectable, and verifiable.
- Share native ABI, machine lowering, typed relocation, and runtime contracts across JIT and AOT rather than building parallel native compilers.
- Retire C#/.NET from the normal Windows and Linux workflow only after a reproducible Windvale-native compiler, verifier, runtime, toolchain, and recovery seed are qualified; accumulate the final Stage 0 recovery evidence throughout development and preserve the completed bundle as bootstrap history.
- Treat portability as a per-part promise and derive final compatibility from the complete dependency graph; allow honest Windows-, Linux-, or Windvale OS-specific libraries.
- Keep platform differences behind explicit versioned contracts and capabilities, with separate application approval, rights-limited grants, and provider binding.
- Reuse compiler, assembler, object, and linker infrastructure instead of building parallel pipelines.
- Reach self-hosting through documented stages rather than obscuring existing-tool dependencies.
- Measure AI contribution through completed specifications, tests, reproducibility, understandable changes, and defects found—not line-count claims.
- Keep reviewability, AI review, machine verification, human inspection,
  independent reproduction, and external audit or certification as distinct
  claims.
- Keep each layer independently testable and replaceable through explicit contracts.
- Keep packages immutable and content-addressed, dependency and authority selection locked, and time, entropy, networking, diagnostics, and updates behind explicit contracts rather than ambient host behavior.
- Keep physical pages, memory objects, virtual mappings, and aggregate resource-domain charges separate; reserve complete capacity and validate privately before publishing a process or service resource.
- Keep authentication, identity, authorization, and capability grants separate. Neither a TLS peer identity nor a package/release signature grants runtime authority.
- Implement standard Internet protocols behind a capability-oriented user-space network service and isolated device drivers; keep packet parsing, DNS, routing, TCP, and secure-transport policy outside the kernel.
- Treat existing inference engines and orchestration systems as eligible
  rights-limited providers. Use explicit adapters without making one vendor's
  model, API, cache, scheduler, or device policy Windvale semantics.
- Prefer interoperable provenance, bill-of-materials, attestation, update, and
  evaluation records over an isolated Windvale-only assurance vocabulary.

## First convincing system

The first Windvale OS milestone should be a small vertical system rather than a broad desktop:

- Boot in a virtual machine through a documented firmware boundary.
- Initialize diagnostics and memory management.
- Load and verify a Windvale bytecode module.
- Run a small interactive or scripted application.
- Expose a minimal filesystem or packaged-resource model.
- Demonstrate the same bytecode module on Windows, Linux, and Windvale OS.
- Shut down cleanly and produce machine-readable test evidence.

[Decision 0191](../Decisions/0191-Windvale-Console-Shell-And-Cli-Architecture.md) defines the later interactive command path without enlarging this first milestone: device or transport adapters, a terminal service, a capability-restricted shell, and ordinary CLI applications remain separate. Commands launch from immutable identity-bound plans with explicit streams, grants, and resource ceilings; the kernel retains only its independent emergency diagnostic path.

[Decision 0192](../Decisions/0192-Capability-Oriented-User-Space-Network-Stack.md) defines later networking without making it a first-system requirement: the kernel supplies interrupt, timer, IPC, DMA/IOMMU, accounting, and teardown mechanisms; an isolated driver owns the NIC; one user-space service initially owns the standards-based IP, UDP, and TCP path; and applications receive semantic rights-limited network capabilities. Local console work proceeds independently, while authenticated remote sessions wait for qualified secure networking.

[Decision 0193](../Decisions/0193-Simple-Windvale-Remote-Terminal-Protocol.md) keeps that eventual remote path small: one authenticated secure connection creates one rights-limited terminal session and shell resource domain through a supervised adapter. Windvale owns the bounded terminal messages and lifecycle while TCP, TLS, identity, authorization, and the existing terminal service retain their separate responsibilities.

Proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md) supplies a coherent next set of successor defaults for review. The [memory-object](../Architecture/Memory-Objects-And-Resource-Domains.md) and [launch/supervision](../Architecture/Process-Launch-And-Supervision.md) guides build from qualified Probe 40 and put a flat resource domain and atomic clean spawn before the shell or driver paths. The [identity/trust](../Architecture/Identity-Time-Entropy-And-Trust.md) and [package/release](../Architecture/Packages-Releases-And-Recovery.md) guides keep keys, authorization, package identity, release provenance, installed generations, and recovery separate. Probe 40 remains implemented and qualified; the successor contracts proposed by Decision 0198 are recommendations, not implementation or qualification claims.

The accepted first boot environment is x86-64 with UEFI 2.11, QEMU as the primary automated VM, and Hyper-V Generation 2 as the later Windows compatibility target. [Decisions 0044](../Decisions/0044-First-X64-Uefi-Boot-Environment.md) through qualified [0100](../Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) establish deterministic PE32+, firmware exit, kernel handoff, owned memory/stack/page tables, WVA shutdown and normalized faults, fixed in-guest WVB admission, protected processes, a Windvale init service, Windvale-written bytecode interpretation at CPL3, a typed WVB/execution-budget pair, per-opcode budget enforcement, automatic terminal cleanup, and one exact generation-safe process-root reclaim/reuse cycle. Probe 30 is the cross-host baseline at exact implementation commit `4a077ab`; all four pinned-QEMU scenarios also pass on Windows. This is substantial vertical source-to-machine evidence, not yet an arbitrary or general WVB loader/verifier, dynamic resource namespace, general ownership-transfer/allocator system, JIT, scheduler, general trap system, or complete kernel runtime.

## Non-goals for the first stages

The active-development policy is intentionally direct: through at least September 3, 2026, and afterward until a named decision changes it, Windvale does not preserve compatibility with superseded experimental syntax or formats. Accepted changes update the implementation, repository sources, fixtures, and tools together. This date is a minimum no-compatibility window, not an automatic stability date.

- A complete desktop environment
- Broad hardware compatibility
- Native Windows and Linux executable backends for every Windvale application
- Compatibility with obsolete experimental Windvale formats
- A new CPU architecture
- A replacement for every existing compiler backend or debugger
- Claims that the stack has no external bootstrap dependencies

The accepted native execution and retirement direction is defined by [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Native execution and .NET retirement](../Architecture/Native-Execution-And-Dotnet-Retirement.md). It is a destination and qualification plan, not current implementation status.

[Decision 0183](../Decisions/0183-Product-Packaging-Trust-And-Evolution.md) defines a future Windvale 0.1 as a reproducible vertical product slice rather than a broad feature checklist. Proposed Decision 0198 recommends that 0.1 require the complete .NET-free normal Windows/Linux gate, exact archived Stage 0 recovery, one useful packaged application and library, explicit capability evidence, signed reproducible artifacts, a public threat model, and offline third-party verification. Windvale OS completion remains outside that proposed gate. Product review may revise the checklist before it becomes accepted direction.

## Community-source licensing

Windvale-owned work is source-available under the Windvale Community Source License 1.0 with a copyright notice naming E-Worker Inc and Windvale contributors. The license preserves free personal, noncommercial, evaluation, and qualifying small-organization use while requiring separate commercial terms for large-organization production use and Windvale-as-a-product offerings. Independent applications belong to their creators and may use terms of their choice; third-party components remain under their separately identified licenses. E-Worker Inc initiated and stewards the project. [Decision 0114](../Decisions/0114-Community-Source-Licensing-And-Commercial-Stewardship.md) defines the licensing and contributor-rights foundation, [Decision 0031](../Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) defines vendor-neutral AI authorship, [Decision 0849](../Decisions/0849-Define-AI-Led-Research-And-Review-Evidence.md) defines AI-led research and review evidence, and [Decision 0032](../Decisions/0032-Public-Contribution-And-Governance-Foundation.md) defines the remaining contribution, security, governance, support, conduct, and project-identity policy. The official repository is public at [`eworker-inc/Windvale`](https://github.com/eworker-inc/Windvale), with [info@eworker.ca](mailto:info@eworker.ca) as the public business contact.
