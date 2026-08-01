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
- What measured branch, call, data-reference, or wider-patch case should justify the first stencil contract beyond the two exact qualified `WVSP 1`/`WVSP 2` service leaves?
- Which tier thresholds, native-cache identities, and resource counters remain deterministic across interpretation, JIT, and AOT?
- How will deterministic and differential execution be tested across the reference interpreter, Windvale-native interpreter, baseline JIT, and AOT backend?
- How small can the runtime remain while supporting useful libraries and diagnostics?

## Native toolchain

- Which labels, conditional branches, RIP-relative data operations, and 64-bit address forms should follow the accepted WVA 1 core?
- Should a later ergonomic assembly layer sort declarations and expand expressions/macros into canonical WVA, or should those facilities evolve directly in WVA?
- Which final formats are required for UEFI, the kernel, debug information, and host interoperability?
- Which later section-permission, archive-search, dead-stripping, and executable-container responsibilities should extend the accepted minimal linker rather than live in target adapters?

## Operating system

- Which kernel-owned data, address-materialization, and target-container rules should extend the accepted special kernel WVO without duplicating the shared ABI-14 backend?
- What counts as “from scratch” at each bootstrap stage?
- Which exact process, thread, capability, syscall, IPC, and resource-budget encodings should implement Decision 0084's accepted conceptual boundary?
- Which first boot-critical drivers should remain with the kernel temporarily, and which should begin as isolated AOT system services?
- What are the first package/resource and filesystem contracts, after the in-guest verifier and protected-process boundary exist?
- Which QEMU and Hyper-V behaviors must be qualified before the first OS milestone is complete?

## First decision sequence

Decisions 0058 through 0084 establish reproducible bytecode compiler convergence, the bounded shared ABI-14 native path, live Windvale-produced service leaves, Windvale-owned executable-image layout, the first terminal CPU-exception destination, and the durable capability-oriented OS boundary. The recommended next decisions are:

1. Preflight the qualified compiler WVB through the native backend, then decide whether its first explicit `file.write_bytes` blocker can be closed as one bounded file-publication slice without prematurely widening the platform FFI.
2. Execute the qualified Windvale-written compiler through the shared native path and identify the next backend or runtime contract demanded by that real workload.
3. Add native PE/COFF and ELF containers plus standalone Windows/Linux capability and process hosts without leaking host rules into portable modules.
4. Implement the next bounded Decision 0084 slice: extend the kernel from one terminal invalid-opcode destination toward normalized essential traps, clean shutdown, page-table ownership, and an AOT Windvale verifier that admits one embedded WVB, retaining pinned-QEMU evidence before Hyper-V qualification.
5. Satisfy the remaining Decision 0057 native-retirement conditions, archive the final .NET Stage 0 recovery release, and remove .NET from normal automation only from one fully qualified source state.
