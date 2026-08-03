# Decision 0189: Bounded reclaiming Wasm value storage

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0177](0177-Exact-Per-Function-Wasm-Interpreter-Frames.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0177 reduced each interpreted function to its exact frame width, but every immutable descriptor-producing operation still advanced one monotonic pointer through the generated module's 4 MiB value arena. The exact portable compiler therefore reached only 1,511 guest instructions; budget 1,512 failed in the enclosing Wasm runtime before the guest could return `WVXO 2`.

Increasing the arena would postpone the failure in proportion to execution history. The required boundary is instead bounded ownership of generated-Wasm byte backings while preserving immutable Windvale values, slices, exact metering, and the public execution ABI.

The interpreter also repeatedly recovered a local's declared shape by rescanning function declarations. That work was correct but made each additional compiler instruction unnecessarily expensive.

## Decision

- Retain the fixed 4 MiB value region at byte offsets 4,259,840 through 8,454,144. Replace its monotonic pointer with an address-ordered first-fit free list that splits allocations and coalesces adjacent spans on release.
- Reserve byte offsets 512 through 49,699 for 4,099 twelve-byte ownership entries containing backing address, logical length, and reference count. Reserve offset 50,000 for the free-list head. Initialization clears the complete metadata extent and restores one 4 MiB free span on every public run.
- Emit two private Wasm helpers after the public wrapper: one allocates an eight-byte-aligned span and one retains or releases a descriptor backing. Slices retain the backing that contains their pointer; local, operand-stack, result, and return transitions release an owned descriptor exactly when its final reference leaves the retained single-function runtime profile.
- Keep descriptors outside the managed value region non-owning. Allocation, metadata, range, reference-count, and arithmetic exhaustion fail through the existing explicit Windvale status path rather than trapping or growing memory.
- Preserve immutable Windvale value semantics. Reclamation is generated runtime policy; it does not add a mutable WVB operation, observable pointer identity, host import, or ambient garbage collector.
- During interpreter preflight, record a per-function offset into a compact eight-byte `(kind, nominal token)` table for every parameter and local. Default `local.load` shape lookup becomes constant time and retains malformed nominal-index rejection.
- Pin an increasing-size reclamation workload and the exact portable-compiler progress boundary in both the Stage 0 oracle and independent Node.js engine gate.

## Consequences

The exact compiler now crosses the former 1,511/1,512 boundary. Budgets 1,511 and 1,512 return normal `WVXO 2` guest-budget failures after 46,097,866 and 46,098,389 outer instructions. With a 100,000 guest budget, execution reaches guest instruction 37,085 and returns exact guest `WVR3017` after 63,171,965 outer instructions because the interpreter's retained 4 KiB record arena is exhausted.

This separates two resource owners. Generated-Wasm byte storage now reclaims and reuses its fixed arena; interpreted guest records and the retained 64 KiB guest heap remain bounded monotonic stores. Complete WVSS-to-`WVCO 1` compilation is therefore not yet claimed.

The compact shape table raises the one-time compiler preflight cost slightly but reduces the incremental cost from about 3,679 outer instructions per additional guest instruction near the old boundary to about 610, an approximately 83% reduction.

Execution ABI 3, fixed memory, the ten public exports, `WVXI`, `WVXO`, guest charging, and failure meanings are unchanged. Generated profile-16 runtime modules now contain three private functions/types where reclamation applies, so their deterministic sizes and hashes change.

## Local evidence

`Runtime-Reclaim-Main.wv` compiles to a 388-byte WVB with SHA-256 `b08792268aa477086cd347a7ea01306ab530b187d81c71900c16e6a7af872e15`. Its 8,192 concatenations cumulatively request 16 MiB of constructed payload through the fixed 4 MiB arena. The backend emits 2,399 deterministic import-free Wasm bytes with SHA-256 `5a89412a9f48e883a027da497406747f1c31c8eb0e6533f7103e52a078a8827a`. Node.js returns the exact doubled 2 KiB input after 262,167 instructions, returns `WVR3011` at 262,166, and then repeats the exact success in the same instance.

The retained interpreter is 66,312 WVB bytes with SHA-256 `d6cf7293f21e5fbdc80a92b356a6608b0a5f174ff873563eb2fc52c1c3fa5a90`. Its outer function has 4,036 nonparameter locals, 62,103 code bytes, 13,588 instructions, and maximum stack three. The updated backend lowers it in 309,791,380 Windvale instructions to 438,842 deterministic import-free Wasm bytes with SHA-256 `d80a665cc2e059450ac11b62dbc5dfa03fcab22bd5ed4fd9393520c9911dc0ba`.

The focused Seed WebAssembly case passes a zero-warning Release build and the exact 37,085 guest-record boundary in 118.656 test seconds. The complete 35-artifact WebAssembly gate rebuilds every artifact and passes under Node.js 24.18.0 on Windows in 447 seconds. A subsequent collector-free Node.js run over those exact artifacts passes all identities, malformed cases, repeated execution, reclamation stress, compiler boundaries, and verifier bundles in 327.6 seconds. This is local development evidence, not cross-host or cross-browser qualification.

## Rejected alternatives

Increasing the fixed arena was rejected because cumulative allocation, rather than simultaneous live payload, caused the former failure.

Resetting storage only between public calls was rejected because compiler execution exhausts storage within one call.

Using JavaScript allocation, WebAssembly memory growth, or a host garbage collector was rejected because each would weaken the import-free fixed-resource contract and move Windvale failure behavior into ambient host policy.

Retaining declaration rescans was rejected because the candidate is already verified and compact immutable shape evidence can be derived once during bounded preflight.

## Reconsider when

- Guest record ownership and reclamation advance the exact compiler beyond guest instruction 37,085.
- Guest heap ownership becomes the next measured failure and has explicit alias, slice, reset, exhaustion, and stale-reference evidence.
- Multiple generated runtime functions require ownership transitions beyond the retained single-function interpreter profile.
- Cross-host or browser engines disagree on allocator identity, coalescing, reset, or exact compiler progress.
