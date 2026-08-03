# Decision 0197: Bounded reclaiming Wasm guest records

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0189](0189-Bounded-Reclaiming-Wasm-Value-Storage.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0189 made the generated Wasm value arena reusable and advanced the exact capability-free compiler from guest instruction 1,511 to 37,085. Execution then failed with `WVR3017` because the interpreter still appended every constructed record to a separate 4 KiB guest-record arena. Increasing that arena would again make cumulative construction history, rather than the simultaneous live set, determine success.

Guest record values can survive in active locals, the operand stack, saved caller frames, records under construction, and fields of other records. Reuse therefore requires a bounded root and transitive-record policy that preserves stable handles and nominal identity without JavaScript objects, host garbage collection, memory growth, or a new public ABI.

## Decision

- Retain the 4 KiB field-cell arena and represent a record handle as its stable start slot from zero through 511. Each field remains one typed eight-byte cell. The all-ones handle remains the allocation-free default for zero-field records.
- Add a fixed 4 KiB metadata table. A live record's start-slot entry contains its nominal type token and field-cell count; non-start slots remain zero.
- Use address-ordered first fit over metadata gaps. When no contiguous span fits, clear a fixed 512-byte mark vector, trace candidate record cells from active locals, the operand stack, saved call frames, fields already evaluated for the current construction, and marked records' nested fields, then rebuild metadata with only marked records and retry allocation once.
- Recognize a root conservatively only when its slot is in range and its nominal token and extent match a live metadata entry. A scalar bit pattern can therefore retain storage until the next execution reset, but it cannot create an invalid record dereference or reclaim a live record. Exact-precision garbage collection is not claimed.
- Preserve immutable record values, stable handles, deterministic first-fit reuse, complete public-run reset, existing field access, guest charging, and exact `WVR3017` when the marked live set still leaves no adequate span.
- Raise the canonical WVB combined parameter/local admission ceiling from 4,096 to 8,192 so the Windvale-authored collector remains a valid canonical module. Keep the Windvale-native source emitter's deliberately narrower 4,096 ceiling unchanged. Raise profile 16's backend input envelope to 8,191 nonparameter locals and 131,072 code bytes for this verified interpreter.
- Pin both cumulative churn and actual live-set exhaustion. The live-set fixture remains inside the complete executable verifier's maximum-stack-16 profile by retaining thirty-two 16-field records across a helper call rather than relying on one over-wide constructor.

## Consequences

The exact compiler no longer reaches guest-record exhaustion at instruction 37,085. Guest budgets one, 1,511, 1,512, and 100,000 all return ordinary `WVXO 2` guest-budget status `WVR3011` with exact guest counts and no output. The 100,000 case completes after 96,797,247 outer Wasm instructions. The next observed bounded store is the separately retained 64 KiB guest text/bytes heap; complete `WVCO 1` compiler output is not yet claimed.

Record handles remain stable because compaction is not used. The collector reclaims whole metadata-described spans and does not rewrite roots. Conservative false retention can cause an earlier bounded `WVR3017` than a precise type-aware collector would, so future larger workloads must retain explicit live-set and false-retention evidence.

Execution ABI 3, fixed 129-page memory, the ten public exports, `WVXI 1`/`WVXI 2`, `WVXO 1`/`WVXO 2`, guest status meanings, and the static profile-8 page are unchanged.

## Local evidence

The retained interpreter is 70,846 WVB bytes with SHA-256 `3ae7718480a19b2a1de5858429e59cd833dd1beec7bc70f5de7c42c91aff0c40`. Its outer function has 4,323 nonparameter locals, 66,350 code bytes, 14,510 instructions, and maximum stack three. It lowers in 332,023,684 Windvale instructions to 468,320 deterministic import-free Wasm bytes with SHA-256 `dbcb971cb1dedac2169035d0cf436aaed9cc5abcce0a9347932c8e0b7d1bff1e`.

The 4,404-byte record-pressure WVB has SHA-256 `55675447848f249c911f848e45014d63916b51ea8c2c98ce5a26e150fe176f3c`. The reference runtime completes it with result 570 after 4,698 guest instructions. The fixed-arena Wasm interpreter first proves reclamation through 33 successive 16-field replacements, then reports exact guest `WVR3017` at instruction 4,332 after 4,071,115 outer instructions when the caller's live records occupy all 512 field cells. Repeating the same request in one Wasm instance returns the identical response and counters.

The canonical semantic verifier grows to 70,092 WVB bytes / SHA-256 `e272f54312e1acfb8ae94095d0ca7b08b8c5076bc142a6e32436476286bd0863` and 440,583 Wasm bytes / `78c8c7bd43b2036d336df956f693a1421a93a6bd55d6e2fda5afcc9c8df412e0`. The complete executable verifier is 115,559 WVB bytes / `956815c749ac9603e49fcdef6451b6d3d1d2bf56be700ab3f6a9c908ef8b4101` and 723,327 Wasm bytes / `c9249bd45a6ea7dcb14a11d1fbcf6dd004f6ce2bcf9eb4794ad65e2ba79a00fd`.

The focused Seed compiler/WebAssembly case passes a zero-warning Release build in 141.070 test seconds. A collector-free, fail-fast Node.js 24.18.0 run over the exact generated artifacts, verifier phases, malformed cases, reset cases, record pressure, compiler admission, and compiler execution budgets passes in 359.6 seconds. This is local development evidence, not dual-host or cross-browser qualification.

## Rejected alternatives

Increasing the record arena was rejected because it would postpone cumulative exhaustion without establishing ownership.

Moving records or compacting the arena was rejected because every live handle would require verified rewriting across locals, operand stacks, saved frames, and nested fields.

JavaScript objects, host garbage collection, Wasm memory growth, and an imported allocator were rejected because they would weaken fixed-resource determinism and move Windvale failure behavior into ambient host policy.

Expanding the executable-verifier maximum-stack-16 profile solely for the stress fixture was rejected. The same 512-cell live-set proof can be expressed with more 16-field records while retaining the existing verified profile.

## Reconsider when

- Conservative false retention becomes the next measured record boundary.
- Mutable records, observable identity, cyclic graphs, or multiple record arenas are proposed.
- Guest heap ownership requires one shared tracing or descriptor policy across records and text/bytes.
- Complete compiler execution requires a larger canonical combined parameter/local ceiling or wider source-emitter contract.
- Cross-host or browser engines disagree on tracing, reuse order, reset, or exact counters.
