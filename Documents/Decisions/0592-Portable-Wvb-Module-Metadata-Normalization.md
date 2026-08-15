# Decision 0592: Portable WVB Module Metadata Normalization Contract

- Status: Implemented in the source verifier and build driver; pinned front-door promotion pending
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

The first verifier attempt was reported by the then-pinned build driver only as
`Sourceˉbindings function=0 operation=0` and was initially misclassified as a
binding-evidence capacity failure. Rebuilding the current diagnostic driver
exposed the actual result: the newly listed module was unreachable because the
semantic verifier had not imported it. The corresponding inspector source did
compile, but its packaged Windows file-report path crashed after the report
header for both absent and present inputs. The inspector integration remains
rejected, not implemented behavior.

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

Add a focused portable metadata-aware verifier adapter. It normalizes once and
passes that immutable absent-form view to the existing semantic,
typed-execution, and control-reachability verifier. The standalone verifier and
compiler build driver use this adapter and list the normalizer explicitly.
Other consumers retain the legacy absent-form verifier until their pinned
artifact families migrate in owner-sized batches.

Keep the normalizer's private parse results in bounded byte tuples rather than
new nominal records. The compiler build driver already reaches the native x64
runtime encoding's 64-record ceiling; adding the three initial parser records
made the otherwise valid 500-function driver an unsupported native module.
Compact private tuples preserve the target contract without widening or
wrapping the one-byte record-type encoding.

The focused native owner must build and package the complete profile-2 verifier
and execute it against the exact metadata-bearing WVB fixture on each host. Do
not promote the separately pinned ordinary verifier or connect the inspector
until their own exact-artifact gates are satisfied.

## Consequences

- The source-built verifier application and compiler build driver now accept
  validated present metadata without teaching later phases a second envelope
  shape.
- The complete 500-function build driver remains natively stageable because the
  normalizer adds no nominal records; the model-provider owner exercises that
  source-build, segmented-package, compile, and execution path.
- Later consumers have one bounded parser contract instead of copying the
  lowerer adapter again.
- Optional requirements remain admission metadata and grant no executable
  capability ordinal.
- Empty bytes are an unambiguous failure result because canonical WVB cannot be
  empty, but the API does not yet provide detailed error categories.
- The implementation duplicates the parser already retained at the lowerer
  boundary. Consolidating that consumer is deferred because its large project
  graph rejected the additional module call.
- The digest-pinned ordinary verifier, WVB inspector, publisher, and package
  consumers continue to reject present metadata until their owner-sized
  migrations replace them. The source verifier and build driver no longer block
  the first package proof, but inspection, publication, and front-door promotion
  remain required before a repository-wide source rewrite.
- The failed Windows inspector packaging smoke test is evidence against
  integration and creates no candidate or front-door identity.

## Reconsideration triggers

Reconsider when inspector reporting executes safely with a normalized byte
view, a consumer needs exact metadata diagnostics, catalog majors change, the
native runtime type encoding is widened coherently, or the lowerer can import
this contract without widening its source graph beyond current limits.
