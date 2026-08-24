# Decision 0811: Serialize materialized generic nominals as ordinary WVB Types

- Status: Accepted; Foundation suffix superseded by Decision 0819; serialized
  output-range ordering superseded by Decision 0843
- Date: 2026-08-21

Decision 0843 retains dependency-ordered materialization but remaps every entry
to canonical WVB semantic-category/name order. Items 2 and 4 below describe the
historical focused serializer checkpoint, not the current serialized order.

## Context

The compiler now retains generic nominal instances in WVGT, reconstructs exact
record and variant layouts, and creates one bounded materialization plan with
ordinary type-table indices. The remaining WVB boundary needs a deterministic
representation for those concrete types. A second runtime generic object model,
template metadata in WVB, or source-name-derived mangling would create another
semantic path and make reproducibility harder to establish.

The active source analyzer is also within 8,798 bytes of the unchanged 32 MiB
native-object bound. Pulling all emission code into that product before its
contract is independently proved would confuse a capacity problem with a source
semantics problem.

## Decision

1. Serialize every retained generic nominal instance as one ordinary private WVB
   record or variant Types entry. WVB receives only concrete fields and cases;
   source type parameters and WVGT private shapes do not cross the boundary.
2. Preserve WVGT catalog order and assign its contiguous output range immediately
   after declared records, enums, and variants.
3. Use fixed-width private names `__WvY0000` through `__WvY1023`. They are
   deterministic emitter identities, cannot be written as official source
   identifiers, and sort before the existing concrete Foundation generic range
   `__WvZ000` through `__WvZ255`.
4. Place concrete Foundation Option and Result variants after all general generic
   nominal instances. A materialized field that uses one must resolve through the
   exact bounded first-use Foundation plan; missing, malformed, repeated, or
   non-Foundation entries are rejected.
5. Preserve existing WVB record, variant, multi-field-case, shape, and feature-bit
   encodings. Nested generic record and variant fields refer to their final
   ordinary Types indices. No WVB version or runtime generic mechanism is added.
6. Require the materialization base to equal the declared nominal type count,
   retain the 1,024 total-type and 256 Foundation-specialization limits, and bound
   emitted generic type payload to 4 MiB. Failure returns no partial payload.
7. Prove this serialization first in the focused materialization owner. Main
   Source WIR carriage and insertion into `Compilerˉsourceˉwvb` remain the next
   connected work; this decision does not claim either is complete.

## Evidence

The 30-case generic nominal materialization fixture serializes four concrete
instances: three records and one four-case variant. It proves stable private
names, record and multi-field-variant metadata, nested record and variant target
translation, exact repeated output, strict type-base admission, Foundation-plan
admission and rejection, and rejection of tampered materialization evidence.

On the current Windows host the project builds to a 731,861-byte WVB with
SHA-256
`c4283d87564abff8fe81d0d2fe6935745cbdc609dde20d2b97ad30d04f53c4c0`.
Its five-fragment 17,704,448-byte hosted executable has SHA-256
`fdc0a0325e4d3e68ec133e7ad726c37f52f56c4a770e106c8593f9b85de8c14a`,
returns `42`, and writes no output.

## Consequences

General generic nominal types can reuse the existing WVB verifier, interpreter,
native backend, and value layouts once the serializer is connected to the main
emitter. Canonical output does not expose source generic syntax or require a
runtime template registry. Foundation Option and Result remain compatible while
their special planning is migrated behind the same exact type-index ordering.

The focused module deliberately consumes a fully validated materialization plan.
It is not a public bytecode compiler entry point and cannot authorize unvalidated
WVGT evidence. Until main WIR carries the retained catalog and Source WVB invokes
the serializer, ordinary compilation behavior is unchanged.

## Reconsideration triggers

Revisit this decision if WVB gains an independently justified reified-generic
contract, if the 1,024-type limit changes, if reachable-type pruning changes
canonical instance order, or if main integration proves that the Foundation
first-use plan cannot remain one bounded deterministic suffix.
