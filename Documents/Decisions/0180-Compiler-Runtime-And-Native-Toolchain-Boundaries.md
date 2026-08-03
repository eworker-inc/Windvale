# Decision 0180: Compiler, runtime, and native-toolchain boundaries

- Date: 2026-08-03
- Status: Accepted architecture direction; measured expansions remain incremental
- Refines: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0137](0137-Bounded-Owned-Values-Before-Dynamic-Collections.md), and [Decision 0150](0150-Bounded-Native-Dynamic-Value-Lifetimes.md)
- Retains: one compiler, canonical stack WVB, typed stack-independent WIR, the shared native backend, and independent verification evidence

## Context

Several open questions describe alternatives that the current compiler has already resolved in practice. Typed stack-independent WIR deterministically lowers to verified stack WVB, and the same semantic path feeds the shared native backend. Replacing that foundation with register or hybrid distributable bytecode would create a new format and verifier without a measured product need.

Other questions concern implementation policies that must remain deterministic without turning performance heuristics, native templates, or one host object representation into language semantics.

## Decision

### Close the WIR and WVB representation question

- Canonical distributable WVB remains typed stack bytecode.
- Compiler WIR remains typed and stack-independent, with explicit basic blocks, terminators, virtual temporaries, locals, ownership evidence, and source identities.
- WIR may evolve with the compiler. WVB owns versioned portable runtime operations, types, control behavior, capability references, validation limits, and observable traps.
- Optimizer hints, debug information, source maps, and native-lowering metadata may be carried in optional evidence that does not change WVB semantics.
- JIT and AOT are modes over shared verified semantics and lowering rules, not parallel compilers.

### Share semantic ownership but permit profile-specific reclamation

The compiler and ABI preserve the semantic classes accepted by Decision 0137: scalar, borrowed immutable view, uniquely owned mutable builder, published immutable owned value, and capability-backed resource. Escapes, transfers, borrows, publication, close, and lifetime ends remain explicit in WIR and native evidence.

The first system runtime uses deterministic explicit ownership, frame/block storage, and bounded arenas without a tracing collector. Application runtimes may later add tracing for cyclic graphs while preserving the same visible value and capability semantics. A moving collector is not required for the kernel, driver, or first native runtime.

The first native representation uses versioned fixed-width value cells plus separate checked descriptor and backing records, explicit ownership actions, bounded allocation, and deterministic capability close. It avoids one universal boxed dynamic value. Root tables and safepoints are added only when a measured tracing or relocation mechanism needs them.

### Expand native templates and instructions only from consumers

- Add no new stencil merely to broaden an inventory. The next versioned stencil must be selected by a measured branch, call, data-reference, or patch consumer and retain exact reconstruction and differential evidence.
- Add division, variable-count shifts, conditional moves, or another WVA instruction family only when a compiler, runtime, OS, cryptographic, or performance consumer requires it.
- Create a shared production machine encoder only after two production paths need the same family and measured duplication creates drift risk. Keep an independent decoder, reconstruction oracle, or golden-byte verifier.

### Keep tier policy deterministic and non-semantic

- Tests and qualification can force the interpreter, baseline JIT, optimizing tier, cached/install-time path, or AOT path.
- Initial promotion uses deterministic semantic counters rather than wall-clock time. Adaptive policy may later use host measurements, but it cannot change language results, traps, or abstract charges.
- A native cache key includes canonical WVB identity, verifier and profile version, target and ABI, backend/code-generation version, required CPU feature set, and any code-generation-relevant service-table shape.
- Every tier reports the selected engine and preserves the same abstract instruction and capability accounting contract.

### Normalize differential evidence

Define a bounded execution transcript containing canonical artifact identity, selected engine/tier, result or trap, output bytes, capability-call and mutation-progress evidence, abstract instruction count, call count, and selected allocation counters. Compare that transcript across the reference interpreter, Windvale-native interpreter, forced JIT tiers, cached execution, and AOT where supported.

Use exact fixtures for format contracts, seeded generated programs for semantic breadth, and hostile or malformed inputs for validators. Native-byte reproducibility is tested separately from semantic transcript equality.

### Bound runtime responsibility rather than source line count

The trusted runtime owns module loading, verification, execution, value and memory primitives, budgets and traps, capability dispatch, and bounded diagnostics. Filesystem policy, networking, UI, packages, service management, and host convenience behavior remain libraries or providers.

Track trusted binary size, startup memory, cold-start time, and reachable privileged surface. Add a runtime primitive only when it cannot safely be a library or capability provider and has at least one measured semantic consumer.

### Defer assembly ergonomics until native assembler retirement

Do not spend bootstrap effort on macros or expression syntax while the C# assembler remains in the normal path. After the Windvale assembler has replaced it and is independently recoverable, add ergonomic declaration ordering, expressions, constants, or macros through a separate source mode or front-end that expands to inspectable canonical WVA with source maps. Canonical WVA remains small and reproducible.

### Keep the linker platform-neutral

- The shared object/linker model owns typed sections, symbols, layout, relocation, permission and alignment rules, archive resolution, dead stripping, reproducible maps, and final linked-image recipes.
- Target adapters own PE/COFF, ELF, UEFI container headers, platform imports, signatures, startup records, and provider-specific packaging.
- A Windvale kernel image uses a versioned manifest and linked payload inside the selected boot container rather than a kernel-only linker.
- Windvale debug evidence begins as a canonical sidecar that adapters may translate to CodeView or DWARF. Host interoperability uses explicit versioned ABI shims rather than making PE or ELF part of language semantics.

## Consequences

The repository avoids a second bytecode, compiler, runtime model, assembler language core, or platform linker. Stable distribution remains compact stack WVB while compiler and target work can use richer internal representations.

System and application runtimes may use different reclamation mechanisms without giving values different meanings. Tier policy can improve without invalidating deterministic tests or caches.

Instruction and stencil growth may occasionally wait for a concrete consumer, and a separate ergonomic assembly front-end adds a later tool boundary. Those costs preserve independent verification and prevent speculative native surface.

No new stencil, encoder, collector, tier, cache, object section, debug format, executable adapter, or assembly syntax is implemented by this decision.

## Reconsider when

- stack WVB creates a measured distribution, verification, or execution cost that cannot be removed by WIR/native lowering;
- an application memory workload cannot meet its contract under the shared ownership semantics;
- deterministic promotion counters produce unacceptable sustained performance;
- independent encoders become a larger correctness risk than the lost diversity of a shared implementation; or
- a target requires a linker responsibility that cannot be represented by the shared linked-image recipe.
