# Decision 0037: Canonical backend remapping and static data

- Date: 2026-07-31
- Status: Accepted; cross-host qualification pending

## Context

Decision 0036 proved direct Windvale-written WVIR-to-WVB emission for functions already declared in ordinal order. That temporary restriction made WVSD declaration identities equal to WVB function indices, but it could not survive interleaved declarations or the addition of static data. Text literals also require deterministic data interning, including exact escape decoding and collision-free synthetic names.

The next backend slice should expand useful language coverage without introducing another IR or making WVB ordering depend on source declaration order.

## Decision

Extend `Compilerˉsourceˉwvb` with explicit canonical identity translation and complete primitive static-data lowering for one portable module.

WVSD entries remain the stable identities carried by WVIR. The backend independently derives ordinal function ranks and ordinal data ranks from the validated symbol directory. It emits functions, code, exports, and data in WVB canonical name order and translates every function-call and data-reference target at emission time. Source declarations may therefore be interleaved and need not already be ordinal.

The data encoder supports `[i32]`, `text`, and `bytes`. It serializes signed integer values as their exact little-endian two's-complement bit patterns and preserves byte values exactly. Text is strict UTF-8. Source escapes are decoded inside the portable backend, including paired UTF-16 surrogate escapes.

Explicit text data is indexed by decoded value in source declaration order, matching Stage 0. Remaining string literals are interned while functions are traversed in canonical function order. Synthetic names use `__Text_000000` through `__Text_999999`, skip explicit data-name collisions, reuse prior values, and participate in the same ordinal data ordering as explicit declarations.

The accepted primitive operation surface now includes `text`, `bytes`, all existing Foundation byte/text/formatting intrinsics, static data length/indexing, and the earlier arithmetic, control-flow, local, and function-call operations. Records, enums, capabilities, hosted profiles, imports, and multi-module index translation remain explicit later boundaries.

## Consequences

Ordinary declaration order is no longer part of the backend contract. WVSD identities and WVB indices are deliberately different domains connected by deterministic translation, which is the required foundation for later nominal, capability, and multi-module metadata.

The backend remains a direct two-pass code emitter. No new general intermediate representation is introduced. Canonical rank lookup is currently linear over the bounded symbol directory; a packed remapping table may replace it when compiler-closure performance evidence shows that repeated lookup is material.

The differential fixture combines interleaved functions and data, unsorted function names, signed integer data, bytes, explicit and synthetic text, a synthetic-name collision, escaped Unicode including a surrogate pair, Foundation intrinsics, and cross-function calls. Windvale and Stage 0 produce byte-identical WVB and the generated program executes with result `13`.

## Verification gate

The candidate must pass:

- the focused source-to-WVB conformance test with both the original function-only fixture and the new data/text fixture;
- exact byte equality with Stage 0 for both fixtures;
- mandatory WVB verification and runtime execution of both generated modules;
- the complete Standard suite and native verifier on Windows; and
- exact-commit Debian qualification with matching normalized reports and byte-identical retrieved portable artifacts.

The implementation remains pending cross-host qualification until all gates above are recorded in `Documents/Project/Seed-Verification-Evidence.md`.
