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

- Which kernel-owned data, address-materialization, and target-container rules should extend the accepted special kernel WVO without duplicating the shared native backend?
- What counts as “from scratch” at each bootstrap stage?
- Which parts of Decision 0092's two-process records, reduced endpoints, saved context, and fixed wait/wake coordinator should survive a third runnable or general loader case?
- Which first boot-critical drivers should remain with the kernel temporarily, and which should begin as isolated AOT system services?
- What are the first package/resource and filesystem contracts, after the in-guest verifier and protected-process boundary exist?
- Which QEMU and Hyper-V behaviors must be qualified before the first OS milestone is complete?

## Browser and WebAssembly

The [WebAssembly playground exploration](WebAssembly-Playground-Exploration.md) records the current options and proposed demonstrations without accepting a target or implementation route.

- Which evidence and replacement gate should move the implemented C#/.NET WebAssembly experiment toward a Windvale-native WVB interpreter or direct backend?
- Should direct WebAssembly compilation consume typed WIR, canonical verified WVB, or a later shared machine-independent lowering model?
- Which browser engines, resource ceilings, capability adapters, and reproducibility evidence define the first browser profile?
- What is the smallest portable asynchronous UI/event contract that can map coherently to browsers, Windows, Linux, and Windvale OS?
- Which exact sample should prove equivalent Windvale behavior across Windows, Linux, and WebAssembly?
- What evidence is required before WebAssembly becomes an accepted permanent host or compiler target?

## First decision sequence

Decisions 0058 through 0093 establish reproducible bytecode compiler convergence, the bounded shared ABI-16 native path with all 12 current service leaves and calls through 64 parameters, live Windvale-produced service leaves, Windvale-owned executable-image layout and lifetime, WVA-owned Q35 poweroff, normalized no-error/error-code trap entries, the first kernel-owned W^X root, fixed in-guest WVB admission, protected processes, the first Windvale init/resource service, and the first user-space Windvale bytecode interpreter. The recommended next decisions are:

1. Admit the smallest safe frame contract demanded by `Compilerˉsourceˉwirˉcompileˉblock`: 1,049 locals and declared stack depth 12 against the current 1,024-slot bound. Measure lowered numbered-value pressure before choosing a larger fixed frame versus deterministic slot reuse.
2. Continue compiling and then execute the qualified Windvale-written compiler through the shared native path, recording each next backend or runtime contract demanded by that real workload.
3. Add native PE/COFF and ELF containers plus standalone Windows/Linux capability and process hosts without leaking host rules into portable modules.
4. Cross-host-qualify candidate probe 24, then generalize exactly one runtime boundary: either a bounded boot-resource/module input that removes fixed interpreter offsets or a third runnable that creates real scheduler pressure. Keep JIT publication, broader traps, virtual memory, and lifecycle adapters separate.
5. Satisfy the remaining Decision 0057 native-retirement conditions, archive the final .NET Stage 0 recovery release, and remove .NET from normal automation only from one fully qualified source state.
