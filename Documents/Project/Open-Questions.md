# Windvale open questions

This list records unresolved decisions without presenting them as implementation commitments.

## Identity and community

- Where will the public repository and project site live?
- Will AI-generated changes require provenance metadata beyond ordinary commit history?
- What contribution, security-reporting, and governance model should accompany the first public release?

## Language

- What programming model makes Windvale distinct while remaining approachable?
- What exactly does “code and data together” mean in the language and package model?
- Beyond the accepted checked `i32`, `u8`, `u32`, immutable text, immutable bytes, nominal records, and nominal enums, what integer, floating-point, text, error, concurrency, and memory semantics are needed?
- Which facilities belong to safe, unsafe, portable, hosted, and system profiles?
- Should Windvale eventually admit broader Unicode identifiers beyond ASCII segments joined by U+02C9, and if so, under which normalization and confusable-character rules?

## Compiler and runtime

- Which portions of the accepted C# Stage 0 implementation should remain in the permanent recovery bootstrap after self-hosting?
- Should distributable bytecode be stack-based, register-based, or a hybrid?
- What is the boundary between compiler IR and stable bytecode?
- Which memory-management model works for both native system code and managed application code?
- How will deterministic and differential execution be tested across the VM, C bridge, and native backend?
- How small can the runtime remain while supporting useful libraries and diagnostics?

## Native toolchain

- Which labels, conditional branches, RIP-relative data operations, and 64-bit address forms should follow the accepted WVA 1 core?
- Should a later ergonomic assembly layer sort declarations and expand expressions/macros into canonical WVA, or should those facilities evolve directly in WVA?
- Which final formats are required for UEFI, the kernel, debug information, and host interoperability?
- Which later section-permission, archive-search, dead-stripping, and executable-container responsibilities should extend the accepted minimal linker rather than live in target adapters?

## Operating system

- Is x86-64 with UEFI the accepted first boot target?
- What counts as “from scratch” at each bootstrap stage?
- What is the first process, protection, filesystem, driver, and application model?
- Does the first Windvale OS host bytecode inside the kernel, in a privileged runtime process, or in ordinary isolated processes?
- Which QEMU and Hyper-V behaviors must be qualified before the first OS milestone is complete?

## First decision sequence

The recommended next decisions are:

1. Reproduce the qualified Stage 0 flat-image linker contract in verified Windvale bytecode.
2. Define the internal-label and address-materialization extensions proven necessary by the linker/native experiment.
3. Define the memory and aggregate-type model needed for a self-hosted compiler.
4. Define the restricted C bridge without changing Windvale semantics.
5. Select the firmware boundary and VM qualification targets for the accepted first x86-64 path.
6. Define contribution, AI-provenance, security-reporting, and trademark policies around the accepted MIT license.
