# Windvale open questions

This list records unresolved decisions without presenting them as implementation commitments.

## Identity and community

- Which open-source license best matches the desired contribution and commercial-use model?
- Where will the public repository and project site live?
- Will AI-generated changes require provenance metadata beyond ordinary commit history?
- What contribution, security-reporting, and governance model should accompany the first public release?

## Language

- What programming model makes Windvale distinct while remaining approachable?
- What exactly does “code and data together” mean in the language and package model?
- What are the integer, floating-point, text, error, concurrency, and memory semantics?
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

- Which x86-64 instruction subset is sufficient for the first native milestone?
- Should the first object writer produce an existing format, a Windvale format, or both?
- Which final formats are required for UEFI, the kernel, debug information, and host interoperability?
- Which responsibilities belong to the assembler versus the object model and linker?

## Operating system

- Is x86-64 with UEFI the accepted first boot target?
- What counts as “from scratch” at each bootstrap stage?
- What is the first process, protection, filesystem, driver, and application model?
- Does the first Windvale OS host bytecode inside the kernel, in a privileged runtime process, or in ordinary isolated processes?
- Which QEMU and Hyper-V behaviors must be qualified before the first OS milestone is complete?

## First decision sequence

The recommended next decisions are:

1. Define the smallest end-to-end demonstration program and its observable behavior.
2. Evaluate the implemented Seed semantic nucleus with several small programs before expanding it.
3. Define the memory and aggregate-type model needed for a self-hosted compiler.
4. Define the restricted C bridge without changing Windvale semantics.
5. Select the first architecture, object path, firmware boundary, and VM qualification targets.
6. Select the source license before publishing implementation code.
