# Decision 0572: Portable WVB Module Metadata Normalization Contract

- Status: Implemented portable contract; consumer integration pending
- Date: 2026-08-15
- Advances: Decision 0571
- Contract: [Seed bytecode](../../Specifications/Seed-Bytecode.md)

## Context

Decision 0571 admitted independent WVB 1.11 Module metadata at the paired native
lowerer application boundary. The compiler-aligned verifier and WVB inspector
still require the retained absent form. Copying a parser directly into each
large consumer would make malformed-input behavior drift, while treating
metadata as ignorable trailing bytes would discard platform, authority, and
capability claims.

A first attempt to integrate a shared parser into the verifier exceeded that
source graph's current binding-evidence capacity. The corresponding inspector
source compiled, but its packaged Windows file-report path crashed after the
report header for both absent and present inputs. That integration is rejected,
not implemented behavior.

## Decision

Add one portable metadata normalizer with a deliberately narrow byte boundary:

```text
Windvaleˉwvbˉmetadataˉnormalize(Input: bytes) -> bytes
```

It accepts the retained absent form unchanged. For present metadata it validates
encoding version 1, authority, one through 32 ordered platform identities,
ordered catalog capability identities, major version 1, required/optional
disjointness, exact Module bounds, derived-profile agreement, and exact equality
between required identities and executable Capabilities entries. Only after
successful validation does it reconstruct an immutable absent-form view.
Invalid input returns an empty byte value.

Pin a portable self-test around the exact 369-byte source metadata vector. It
proves deterministic 282-byte normalization, idempotent absent input, invalid
metadata-version rejection, and required-capability mismatch rejection.

Do not connect this module to the pinned verifier or inspector until their
complete production graphs pass their own native execution gates.

## Consequences

- Later consumers have one bounded parser contract instead of copying the
  lowerer adapter again.
- Optional requirements remain admission metadata and grant no executable
  capability ordinal.
- Empty bytes are an unambiguous failure result because canonical WVB cannot be
  empty, but the API does not yet provide detailed error categories.
- The implementation duplicates the parser already retained at the lowerer
  boundary. Consolidating that consumer is deferred because its large project
  graph rejected the additional module call.
- The normal compiler-aligned verifier and pinned/source WVB inspector continue
  to reject present metadata. Package migration remains blocked at those gates.
- The failed Windows inspector packaging smoke test is evidence against
  integration and creates no candidate or front-door identity.

## Reconsideration triggers

Reconsider when verifier binding capacity grows, inspector reporting executes
safely with a normalized byte view, a consumer needs exact metadata diagnostics,
catalog majors change, or the lowerer can import this contract without widening
its source graph beyond current limits.
