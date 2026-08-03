# Windvale open questions

This list records unresolved choices only. Accepted product-wide direction is recorded by [Decision 0178](../Decisions/0178-Project-Stewardship-Archives-And-Recovery.md), [Decision 0179](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md), [Decision 0180](../Decisions/0180-Compiler-Runtime-And-Native-Toolchain-Boundaries.md), [Decision 0181](../Decisions/0181-Next-Windvale-Os-Mechanism-Contracts.md), [Decision 0182](../Decisions/0182-Browser-And-WebAssembly-Product-Direction.md), [Decision 0183](../Decisions/0183-Product-Packaging-Trust-And-Evolution.md), and [Decision 0184](../Decisions/0184-Language-Syntax-And-Operator-Evolution.md). Implementation details remain open when those decisions deliberately require a measured consumer, hardware inventory, or qualification gate.

## Language and application model

- Which exact WVB payload-variant shape, ownership contract, and verifier flow should implement Decision 0184's accepted `variant`/exhaustive-`match` direction, and when is a later visible `try` propagation expression safe?
- Which exact source, package, and WVB encodings should separately carry platform scope, authority, required capabilities, and optional capabilities, and how should current profile bytes migrate without changing existing modules?
- Which bounded typed sequence and unique-builder operations are sufficient for the first database or application consumer, and which allocation, freeze, move, and exhaustion evidence must qualify them?
- Which first bounded consumer should add text/bytes content equality, and which explicit derived-equality syntax should later admit immutable records or variants without giving capabilities, builders, functions, or resources general equality?
- Which first real scientific, graphics, media, or ML workload justifies floating-point semantics, and which exact IEEE, NaN, conversion, comparison, and formatting rules does it require?
- Which ownership, scheduler, cancellation, and failure-propagation evidence is required before structured concurrency enters the source language?
- After initial self-hosting and the first stable language, is there enough normalization, confusable-character, editor, formatter, and security evidence to propose an optional broader-Unicode identifier revision?

## Compiler and runtime

- Which dual-host, browser, and native evidence should promote the implemented-candidate checked `i64`/`u64` path into the accepted default format and execution surface?
- Which first application graph requires cyclic ownership, and does measured evidence favor tracing, reference counting with cycle handling, regions, or another physical reclamation mechanism above the shared semantic ownership model?
- Which measured branch, call, data-reference, or wider-patch consumer should define the first stencil after `WVSP 1` and `WVSP 2`?
- Which measured compiler, runtime, OS, cryptographic, or performance consumer first requires division, variable-count shifts, conditional moves, or a shared production encoder?
- Which measured source consumer first requires `/`, `%`, or unsigned bitwise operators, and which WIR/WVB version should carry Decision 0184's accepted zero, overflow, remainder, count, and fixed-width shift behavior?
- Which deterministic hotness thresholds and native-cache limits are appropriate after representative interpreter, JIT, cached, and AOT workloads exist?
- What is the exact versioned normalized execution-transcript format for cross-engine differential evidence?
- Which binary-size, startup-memory, cold-start, and trusted-surface budgets should constrain the first permanent Windvale-native runtime?
- After the Windvale assembler retires the C# assembler from the normal path, which ergonomic source mode and source-map contract should expand expressions, constants, declaration ordering, or macros into canonical WVA?
- Which canonical debug-sidecar records are needed by the first debugger consumer, and which records should adapters translate to CodeView or DWARF?

## Operating system

- Which invariants from qualified `WVPROC17` and candidate Probe 39 must survive the first general timer/scheduler slice without freezing the private timer/context offsets, fixed three-slot order, or one-thread shape?
- Which physical-machine measurements qualify HPET/local APIC beyond the pinned Q35 candidate, and what calibrated evidence would justify selecting invariant TSC instead of HPET on a later machine profile?
- What bitmap size, page-state encoding, memory-object record, zeroing proof, and fragmentation threshold qualify the first physical-page allocator before dynamic process creation?
- Which exact COM1 configuration, batching limit, provider protocol, revocation sequence, and diagnostic separation qualify the first isolated serial-output service?
- Which interface identity and record shape publish the immutable directory provider to two clients, and which queue limit and backpressure result qualify the first multi-client endpoint?
- Which exact reduced-right copy, cancellation/deadline, provider-replacement, and shared-memory queue consumers should implement the accepted capability sequence one slice at a time?
- After checked `u64` is qualified on every intended target, which first versioned Windvale filesystem interfaces and provider protocols should implement `Open`, `Readˉat`, `Writeˉat`, `Setˉlength`, and `Close`?
- Which physical or root Windows and Linux machines own direct Hyper-V Generation 2, optional WHPX, and KVM qualification, and which nested topologies merit separate qualification?
- Does the first suitable physical Windvale machine select VMX or SVM, and what exact private-memory, reset-state, exit, budget, and teardown records qualify the minimal profile?
- Which secondary non-display GPU or accelerator can prove isolated IOMMU ownership, interrupt remapping, reset, DMA revocation, teardown, and rebind before exclusive passthrough is accepted?
- Which pinned workloads and per-machine noise measurements establish the first VM, memory, storage, network, graphics, and compute regression thresholds?

## Browser and WebAssembly

The [WebAssembly playground exploration](WebAssembly-Playground-Exploration.md) remains the implementation and evidence inventory. Decision 0182 accepts a product direction without yet accepting WebAssembly as a permanent host or compiler target.

- Which bounded Windvale-native verifier, interpreter, compiler, or Module Inspector slice is the first useful experimental browser route, and which limitations must its UI expose?
- Which bounded reusable or reclaiming interpreter-owned storage model should follow Decision 0177's exact 1,511/1,512 compiler boundary, and what reset, stale-reference, allocation, and cross-engine evidence qualifies it?
- Which exact Chromium, Firefox, WebKit, and real-Safari versions, desktop/mobile environments, memory ceilings, and timeout evidence define the first supported browser profile?
- What are the exact source signatures, ordering, batching, cancellation, deadline, and closed-source behavior of the first bounded wait-set or event-stream interface?
- Which canonical output schema and bounded exported function make the Windvale Module Inspector a reproducible Windows/Linux/browser sample?
- When the Windvale-native route is complete, which retained .NET browser projects remain bootstrap/recovery evidence and which leave normal build and publication automation?
- Which real application should justify accepting direct WIR-to-WebAssembly compilation as a permanent target after WebAssembly is accepted separately as a host?

## Product and release lifecycle

- Which exact qualified source state and application define the Windvale 0.1 checklist, and which native-retirement, package, recovery, and host evidence must be complete before tagging it?
- What are the first canonical package-bundle and lockfile encodings, and how do they record target selection, transitive capability approval, dependency origin, license, and integrity without requiring a registry?
- Which independent version fields and compatibility rules belong in the first source edition, package manifest, and capability-binding implementation?
- Which key custody, rotation, revocation, offline-verification, and attestation rules are sufficient for the first official signed release while retaining unsigned local development?
- Which threat-model assets, attackers, boundaries, and residual risks should be documented first, and which normative validators or runbooks own each mitigation?
- Which structured diagnostic envelope and redaction rules should become the first shared compiler/runtime/service/OS observability contract?
- Which measured application first needs monotonic time, civil time, secure entropy, deterministic test entropy, name resolution, connection, or listening capabilities?
- After the x86-64 shared backend and initial OS process, scheduler, memory-object, and isolated-driver paths stabilize, which hardware and product value should trigger an ARM64 proposal?

## First decision sequence

Decisions 0058 through qualified 0103, 0105, 0108, 0109, 0111, 0112, 0133, and 0150 establish reproducible bytecode compiler convergence, the bounded shared native path through frame-owned direct records and generation-owned dynamic values, all current service leaves and calls through 64 parameters, typed block-scoped physical storage under the 2,048-cell bound, bounded exact-compiler publication and complete native reproduction, live Windvale-produced service leaves, Windvale-owned executable-image layout and lifetime, WVA-owned Q35 poweroff, normalized trap entries, the first kernel-owned W^X root, fixed in-guest WVB admission, protected processes, the first init/resource service, a user-space Windvale bytecode interpreter, section-derived validation, typed WVB/execution-budget publication, automatic terminal cleanup, generation-safe reclaim/reuse, and two exact compiler-produced WVB programs across hosts and Windvale OS. Decisions 0104 through 0177 also retain a separate WebAssembly interoperability track with a capability-free in-memory compiler contract and the exact 1,511/1,512 compiler execution boundary. The recommended next implementation decisions remain:

1. Add caller-visible descriptor liveness before relocating descriptor-bearing aggregate returns; do not infer aggregate safety from the direct-descriptor proof.
2. Complete independent Windows/Linux qualification of Probe 39's bounded HPET/local-APIC candidate, whose five pinned Windows scenarios already pass; then add independently lived memory and one flat resource domain before dynamic launch, supervision, or driver isolation. Keep each mechanism a separate evidence claim.
3. Satisfy the remaining Decision 0057 native-retirement conditions while accumulating Decision 0178 recovery evidence gradually; remove .NET from normal automation only from one fully qualified source state.

The early experimental Windvale-native browser route from Decision 0182 may advance independently when its bounded profile is honest. It does not replace these native and OS priorities or make WebAssembly permanent.
