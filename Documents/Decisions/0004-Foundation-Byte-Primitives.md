# Decision 0004: Foundation byte primitives

- Date: 2026-07-29
- Status: Accepted

## Context

The next useful self-hosting step is a Windvale-written `.wvb` inspector. Implementing filesystem access and a complete standard library first would create several host and capability loops before the language could validate its own binary format. The language instead needs a small portable nucleus for bounded binary inspection that can operate on embedded bytes now and capability-supplied bytes later.

## Decision

- Add `u8`, `u32`, and immutable `bytes` values to Seed.
- Use suffixed `u8` and `u32` literals with no implicit conversions.
- Make `u32` addition, subtraction, and multiplication checked; overflow and underflow trap instead of wrapping.
- Add pure Foundation operations for byte length, immutable slicing, and bounded little-endian `u8`, `u16`, and `u32` reads.
- Reserve the official Foundation operation names from source redefinition.
- Implement reference-runtime slices as zero-copy views over shared immutable storage.
- Keep file input outside the portable primitive set. A future explicit hosted capability may supply bytes.
- Advance the early-development bytecode format from 1.0 to 1.1 without a backward-compatibility reader.

## Consequences

- Windvale source can validate binary envelopes and walk bounded sections before filesystem and aggregate-type designs are complete.
- The same inspection function can run on embedded data, Windows, Linux, and eventually Windvale OS because host I/O is separated from byte interpretation.
- Unsigned values have deterministic cross-host behavior and cannot silently wrap.
- Bytecode, verifier, inspector, runtime, examples, golden hashes, and Windows/Linux reports change together.
- This is a compiler intrinsic surface for the bootstrap stage, not yet a general package or library linking model.

## Reconsider when

- The self-hosted inspector requires cursors, streaming, signed fields, mutable construction, or structured error results.
- Measurements show that immutable view lifetime or retention costs require a different storage contract.
- Foundation modules can express these operations efficiently without compiler-recognized intrinsics.
