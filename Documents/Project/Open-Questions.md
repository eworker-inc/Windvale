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

- What exact source, binaries, manifests, and instructions belong in the final archived .NET Stage 0 recovery release, and should a later smaller from-zero path supplement it?
- Should distributable bytecode be stack-based, register-based, or a hybrid?
- What is the boundary between compiler IR and stable bytecode?
- Which memory-management model works for both native system code and managed application code?
- What compact native value, text, bytes, root, heap, and reclamation contracts are sufficient for the first Windvale-native runtime?
- Which WVA stencil and typed patch shape should implement the first copy-and-patch baseline JIT experiment?
- Which tier thresholds, native-cache identities, and resource counters remain deterministic across interpretation, JIT, and AOT?
- How will deterministic and differential execution be tested across the reference interpreter, Windvale-native interpreter, baseline JIT, and AOT backend?
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

Decision 0058 completes reproducible Stage 0 → Stage 1 → Stage 2 bytecode compiler convergence. The recommended next decisions are:

1. Define the smallest native value representation, calling convention, runtime-service table, allocation/reclamation boundary, and platform thunk ABI required by representative WVB programs.
2. Define one structured machine-fragment and typed-patch boundary shared by verified WVB lowering, typed-WIR lowering, WVO/AOT, and in-memory linking.
3. Qualify a WVA-generated copy-and-patch baseline-JIT slice with strict writable-or-executable publication and interpreter/JIT/AOT differential evidence.
4. Add native PE/COFF and ELF adapters plus Windows/Linux capability and process hosts without leaking their rules into portable modules.
5. Satisfy the remaining Decision 0057 native-retirement conditions, archive the final .NET Stage 0 recovery release, and remove .NET from normal automation only from one fully qualified source state.
