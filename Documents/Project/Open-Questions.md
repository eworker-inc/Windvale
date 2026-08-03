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
- Which exact source and module metadata should encode the accepted independent platform scope, authority level, required capabilities, and optional capabilities while preserving deterministic composition?
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
- Decision 0173 selects a statically constructed third directory-service process, separate resource and directory endpoints, and a smallest ready/wait dispatcher as the next process pressure. Which exact fixed records and validation evidence should survive that split before dynamic process creation begins?
- Which monotonic timer source, interrupt routing, fixed quantum, accounting unit, and starvation tests should define the first single-CPU preemptive scheduler without freezing a public timing guarantee?
- Which bounded physical-page allocation and memory-object design should replace exact tail-only reclaim before immutable launch plans can create independently lived processes?
- Decision 0173 recommends ordinary console/serial output as the first isolated AOT driver while retaining the kernel emergency sink. Which exact port, buffering, failure, restart, and diagnostic-separation contract should qualify that move?
- Which concrete multi-client, replacement, or optional-provider case should first justify endpoint names, a user-space registry, dynamic publication, and broader discovery beyond Decision 0172's fixed endpoint?
- Which generation-safe capability copy/move, non-amplification, optional rights reduction, queue backpressure, cancellation, deadline, and shared-memory ownership cases should be introduced first after the second fixed endpoint?
- Which first filesystem-core operations have exact shared semantics across Windows, Linux, and Windvale OS providers; which name-comparison, enumeration, offset-width, revocation, and mutation-completion rules do they require; and which guarantees must begin as optional or platform-scoped interfaces under [Decision 0140](../Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md)?
- Which QEMU and Hyper-V behaviors must be qualified before the first OS milestone is complete?
- Which physical/root Windows and Linux machines should own the first non-nested WHPX, direct Hyper-V Generation 2, and KVM evidence, and which nested topologies remain baseline developer-only or later require dedicated nesting qualification?
- After the memory, interrupt, scheduler, lifecycle, and physical-hardware prerequisites exist, should the first measured Windvale VM-host backend be Intel VMX or AMD SVM, and which exact hardware and nested-virtualization limits define its evidence?
- Which boot resource, terminal exit, virtual timer, console, and shared-queue rules define the minimal and performance-oriented machine profiles accepted by [Decision 0171](../Decisions/0171-Future-Virtualization-And-Accelerator-Architecture.md)?
- Which measured GPU or AI-accelerator hardware can first prove reliable IOMMU isolation, reset, DMA revocation, exclusive passthrough, or hardware partitioning without overstating hostile-tenant isolation?
- Which workloads and recorded host/provider/topology inputs should establish vCPU-exit, memory, storage, network, graphics, and compute regression budgets without turning performance into portable semantics?

## Browser and WebAssembly

The [WebAssembly playground exploration](WebAssembly-Playground-Exploration.md) records the current options and proposed demonstrations without accepting a target or implementation route.

- Which evidence and replacement gate should move the implemented C#/.NET WebAssembly experiment toward a Windvale-native WVB interpreter or direct backend?
- Should direct WebAssembly compilation consume typed WIR, canonical verified WVB, or a later shared machine-independent lowering model?
- Which browser engines, resource ceilings, capability adapters, and reproducibility evidence define the first browser profile?
- What is the smallest portable asynchronous UI/event contract that can map coherently to browsers, Windows, Linux, and Windvale OS?
- Which exact sample should prove equivalent Windvale behavior across Windows, Linux, and WebAssembly?
- What evidence is required before WebAssembly becomes an accepted permanent host or compiler target?

## First decision sequence

Decisions 0058 through qualified 0103, 0105, 0108, 0109, 0111, 0112, 0133, and 0150 establish reproducible bytecode compiler convergence, the bounded shared native path through frame-owned direct records and generation-owned dynamic values, all 12 current service leaves and calls through 64 parameters, typed block-scoped physical storage under the unchanged 2,048-cell bound, bounded exact-compiler publication and complete native reproduction, live Windvale-produced service leaves, Windvale-owned executable-image layout and lifetime, WVA-owned Q35 poweroff, normalized no-error/error-code trap entries, the first kernel-owned W^X root, fixed in-guest WVB admission, protected processes, the first Windvale init/resource service, the first user-space Windvale bytecode interpreter, section-derived validation, a typed WVB/execution-budget pair, automatic terminal cleanup, one generation-safe exact tail reclaim/reuse cycle, and two exact compiler-produced WVB programs across hosts and Windvale OS. Decisions 0104, 0106, and 0107 establish a separate WebAssembly interoperability track whose next gate is broader worker containment and cross-browser evidence. The recommended next decisions are:

1. Resolve the exact compiler's separate 4 MiB WVO/object and flat-linker AOT boundaries through measured ceiling revision, multiple objects, or function/data-granular publication.
2. Serialize hosted capability/runtime requirements, then package the compiler through paired Windows PE and Linux ELF targets without making either host define Windvale semantics.
3. Add caller-visible descriptor liveness before relocating descriptor-bearing aggregate returns; do not infer aggregate safety from the direct-descriptor proof.
4. Follow Decision 0173's bounded process sequence from the qualified Probe-37 endpoint baseline: a statically constructed directory-service process, second endpoint, and state-driven ready/wait dispatcher before timer preemption, general memory objects, dynamic launch, supervision, or driver isolation. Keep each later mechanism a separate evidence claim.
5. Satisfy the remaining Decision 0057 native-retirement conditions, archive the final .NET Stage 0 recovery release, and remove .NET from normal automation only from one fully qualified source state.
