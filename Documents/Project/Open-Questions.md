# Windvale open questions

This list records unresolved decisions without presenting them as implementation commitments.

## Identity and community

- Which individual or service accounts should receive each least-privilege GitHub organization role as the maintainer group grows?
- Should GitHub Discussions be enabled for public design and usage questions?
- Where will the public project site and long-term release artifacts live?

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

- Which minimal read-only data and address-materialization rules will the first compiler-produced kernel WVO require beyond the accepted code-only relative-call adapter?
- What counts as “from scratch” at each bootstrap stage?
- What is the first process, protection, filesystem, driver, and application model?
- Does the first Windvale OS host bytecode inside the kernel, in a privileged runtime process, or in ordinary isolated processes?
- Which QEMU and Hyper-V behaviors must be qualified before the first OS milestone is complete?

## First decision sequence

The recommended next decisions are:

1. Define typed expression and control-flow semantics above the qualified WVSD/WVLB evidence, then introduce the smallest independently validated WIR boundary.
2. Use that semantic and WIR pressure to define only the memory, collection, and aggregate-type facilities actually needed by the compiler.
3. Define the restricted C bridge without changing Windvale semantics.
4. Extend WVA labels and address materialization only when native-backend evidence supplies exact requirements.
5. Replace the accepted Stage 0 kernel-entry object with compiler-produced Windvale WVO while preserving the bounded post-firmware handoff evidence.
6. Import the complete history into a private `eworker-inc/Windvale` repository, inspect the initial publication baseline, and configure DCO, security, branch, and automation settings before public visibility.
