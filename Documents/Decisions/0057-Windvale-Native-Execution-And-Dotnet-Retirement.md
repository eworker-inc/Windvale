# Decision 0057: Windvale-native execution and .NET retirement

- Date: 2026-07-31
- Status: Accepted direction; condition 1 has qualified evidence, Decisions 0059 through 0375 partially advance conditions 2 through 8, and the complete retirement gate remains open
- Refines: [Decision 0002](0002-Windvale-Seed-Bootstrap.md)'s long-term bootstrap and recovery role

## Context

Decision 0002 selected dependency-free C# on a pinned .NET SDK as the shortest safe Stage 0 path. That choice produced a working compiler, mandatory WVB verifier, reference interpreter, assembler, object model, linker, CLI, cross-host tests, and an independently recoverable semantic oracle. It remains the correct current bootstrap.

Windvale's destination is nevertheless an owned computing stack. Requiring the CLR to build or run ordinary Windvale programs indefinitely would retain a large external runtime, inherit its memory representation and garbage collector at the execution boundary, and prevent Windvale from controlling its complete native ABI and recovery chain. Windows and Linux are permanent Windvale hosts, so native independence must apply to them as well as to Windvale OS.

Portable WVB and native execution solve different problems. WVB remains the verified distributable contract. Native just-in-time and ahead-of-time compilation are execution strategies for that contract, not replacement module semantics. Current compilation practice also favors hybrid systems: interpretation for cold code, low-latency baseline compilation, selective optimization for hot code, cached native results, and deterministic AOT for release or system components.

## Decision

- Retire C# and .NET from the normal Windvale build, test, packaging, and execution path after an explicit native-retirement gate is qualified on Windows and Linux.
- Keep the current C# implementation as the active Stage 0 reference and recovery oracle until that gate. After retirement it may remain as archived bootstrap evidence, but it is not a permanent product dependency or required maintained implementation.
- Keep canonical WVB as the portable application and tool format. The mandatory verifier remains the trust boundary before interpretation, JIT compilation, AOT compilation, or execution of cached native code.
- Build the native compiler, runtime, verifier, execution engine, and developer tools in Windvale. A previous qualified native Windvale compiler may serve as the ordinary bootstrap seed once reproducible Stage 1 and Stage 2 convergence is established.
- Define one owned native ABI, value representation, runtime-service table, machine IR boundary, and architecture backend. Use them for both WVO-producing AOT and in-memory JIT output rather than creating unrelated native compilers.
- Use a tiered execution continuum: a simple verified interpreter as the semantic oracle, a low-latency baseline JIT, an optional optimizing JIT for measured hot code, and deterministic AOT for kernels, drivers, core tools, services, and deployments that prohibit executable-memory generation.
- Prefer WVA-authored machine stencils plus typed WVO-style patch records as the first baseline-JIT experiment. Copy-and-patch is a proposed mechanism, not yet an implemented contract; it must earn adoption through exact encoding, malformed-input, W^X, and differential evidence.
- Keep platform adapters narrow. Windows, Linux, and Windvale OS supply executable-memory policy, process startup, capability bindings, and native container integration without changing portable semantics.
- Keep JIT compilation outside the Windvale kernel. The OS may initially interpret or AOT-load WVB and later offer a user-space or isolated system JIT service.
- Own native memory management explicitly. CLR object layout, UTF-16 storage, allocation behavior, garbage-collection timing, exceptions, and JIT behavior do not become Windvale semantics. Native values, text, bytes, records, heaps, roots, reclamation, limits, and failure behavior require versioned contracts.
- Require writable-or-executable memory discipline, complete relocation validation, bounded code caches, exact capability authorization, and optional out-of-process compilation. No accepted design may require permanently writable and executable pages.
- Key reusable native-code caches by at least the complete WVB identity, target architecture and feature profile, native ABI version, runtime version, and compiler version. Cached code is derived evidence and must be rejected rather than guessed compatible.
- Permit deterministic profile-guided AOT and post-link optimization when the profile is a versioned explicit build input. Machine-learned heuristics, speculative specialization, deoptimization, tracing, and on-stack replacement remain optional later experiments and may never affect correctness or validation.

## Native-retirement gate

.NET leaves the normal workflow only after all of these conditions are qualified:

1. The Windvale compiler compiles its complete accepted source graph, and Stage 1 and Stage 2 satisfy the defined reproducibility comparison.
2. Native Windows and Linux tools can build, verify, test, link, package, and run the accepted repository subset without invoking .NET.
3. A Windvale-native WVB decoder and semantic verifier protect every native execution path.
4. A native runtime defines values, memory ownership, traps, capabilities, process entry, and host adapters independently of CLR behavior.
5. The shared native backend produces deterministic AOT evidence and at least one qualified baseline-JIT path for representative WVB programs on both hosts.
6. Interpreter, JIT, and AOT differential tests agree on results, output, diagnostics, traps, and defined resource counters for the accepted subset.
7. A clean-environment bootstrap starts from documented native seed artifacts and verifies their identity and provenance.
8. The final .NET-based recovery release is archived with source, dependency inventory, build instructions, and exact qualification evidence before removal from normal automation.

This gate retires a dependency; it does not erase bootstrap history or claim that every optional optimizer is complete.

Decision 0058 qualifies condition 1 on Windows and Debian with an exact committed 12-module inventory, byte-identical Stage 1 and Stage 2 compilers, and a clean recovery procedure. Later decisions add substantial native execution, compiler packaging, verifier, build-driver, runtime, and backend evidence without completing conditions 2 through 8. [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md) records their current partial status, freezes forward C# source-language growth at the next qualified WVB 1.11 baseline, and sequences the native front-door cutover; it does not retire .NET.

## Consequences

Windvale applications may remain portable WVB while executing as interpreted, JIT-compiled, cached, install-time-compiled, or AOT-compiled native code. Execution mode must not change specified behavior. The same native backend can support PE/COFF, ELF, WVO, in-memory images, and later Windvale OS process images through explicit output and platform adapters.

Native startup, installation size, and compact program-state representation are expected to improve substantially once the CLR is absent, but no numerical improvement is promised before measurement. Rewriting the interpreter in Windvale alone may initially regress throughput because the CLR JIT and garbage collector are mature. Performance claims require workload-specific evidence; package size, cold start, peak committed memory, live Windvale heap, compilation latency, code size, execution throughput, and pause behavior must be reported separately.

The trusted unsafe surface becomes more visible: executable-page transitions, stack frames, host ABI thunks, allocation, collection, and capability entry. Keeping that surface small and WVA-backed where appropriate is more important than prematurely matching an industrial optimizing JIT.

## Reconsider when

- A required host prohibits dynamic code generation or code signing makes a baseline JIT impractical; that host may remain interpreter/AOT-only without changing WVB.
- Measured copy-and-patch results do not justify its stencil size, maintenance cost, or code quality.
- A shared AOT/JIT backend creates materially worse safety or reproducibility than separate adapters over one machine IR.
- The accepted language memory model requires a different native value or collection strategy.
- Native recovery evidence shows that retaining an actively maintained non-Windvale implementation is necessary rather than merely useful as an archive.
