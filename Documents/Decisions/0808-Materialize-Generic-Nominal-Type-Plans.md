# Decision 0808: Materialize generic nominal type plans

- Status: Accepted
- Date: 2026-08-21

## Context

Decision 0807 could reconstruct the concrete fields and cases of one admitted
WVGT instance, but its public random-access path revalidated and rescanned the
source declaration for each selected item. Letting WIR or WVB emission repeat
that work for every field would make the connected compiler slower as generic
layouts grow. Passing WVGT private shapes into later phases would also require
each consumer to reproduce the same mapping to ordinary WVB type identities.

The next boundary needs one compiler-owned representation that is compact,
bounded, deterministic, and already uses the output type space. It must not
claim that general generic source reaches WVB before the main analysis and
emission paths actually consume it.

## Decision

1. Make generic layout creation retain one immutable 16-byte record for every
   field and, for a variant, one immutable 16-byte record for every case.
   Sequential consumers use this evidence instead of invoking the defensive
   source-rescanning accessors for every item.
2. Add a generic nominal type-materialization phase over exact Source Set,
   Source Symbols, layout, and WVGT evidence. Traverse every admitted WVGT
   instance once in its existing dependency-first catalog order.
3. Let the caller provide the first available ordinary WVB Types index. Assign
   instance `i` index `first + i`, and reject a plan whose complete Types range
   would exceed the existing 1,024-type compiler bound.
4. Publish one 36-byte type entry per instance, one 16-byte case entry per
   variant case, and one 16-byte field entry per record field or variant
   payload. Type entries retain the source declaration and module identities
   needed for diagnostics while indexing contiguous case and field ranges.
5. Replace every private WVGT field shape during this forward traversal. An
   earlier generic record reference becomes ordinary record shape
   `65536 + output-index`; an earlier generic variant reference becomes
   ordinary variant shape `196608 + output-index`. No value at or above
   `0x80000000` may survive valid materialized field evidence.
6. Preflight every append. Each immutable evidence stream remains within the
   Foundation `bytes` 4 MiB bound, and the three streams plus retained WVGT
   evidence remain within the frozen 16 MiB generic emitted-evidence budget.
   Return `Evidence_limit` rather than trapping or publishing a partial plan.
7. Reconstruct the complete plan when validating caller-supplied evidence.
   Invalid source, symbol, catalog, layout, type-range, and byte-range states
   fail explicitly and identify the first failing instance when applicable.
8. Defer main WIR carriage, WVB Types emission, reachability pruning, operation
   lowering, and Foundation `Option`/`Result` migration to the next connected
   Slice 4 checkpoints.
9. Give this boundary an independent 20-case development owner. Keep the broad
   Language 1, storage, OS, paired-host, and Qualification gates for the final
   integrated migration gate.

## Evidence

The fixture materializes four dependency-ordered instances: `Box<i32>`,
`Packet<Box<i32>, text>`, `Choice<Box<i32>, text>`, and
`Envelope<Choice<Box<i32>, text>>`. It proves record-to-record and
record-to-variant private-shape replacement, assigned Types indices, contiguous
case and field ranges, missing-index behavior, an empty plan, the 1,024-type
limit, invalid and missing catalog declarations, post-substitution builder and
nested-variant rejection, and reconstructed rejection of tampered type and
field evidence.

The fixture builds to a 707,484-byte WVB with SHA-256
`0f91eb3d873f9dd9f5a68d53956b7be6f0ac7f62c70056241e99ea49ab47fe64`.
Its five-fragment 17,346,560-byte hosted Windows executable has SHA-256
`1989251b54de71bb6b7e69141e61529dd882a218bfd3892107ce4c6ff6f1e275`,
returns `42`, and writes no output. The maintained owner reports visible build,
package, and execute phases and passes all 20 declared cases.

The representation refinement changes the adjacent 18-case layout artifact to
a 688,672-byte WVB with SHA-256
`55fe9cf4744cfe26f42900c85ad8eed9f6e0940cd7d6b533b7a6a94295c042b1`
and a 16,976,896-byte hosted Windows executable with SHA-256
`f28acda8fb1dc64da27e7e08d191ab637600e23c2e69505ee89aed40cc374f5c`.
That owner still returns `42` without output.

## Consequences

The next compiler phase can consume one fixed-width plan in a bounded forward
scan. It no longer needs to rediscover generic declarations for each field or
reinterpret WVGT private shapes. The plan remains a compiler artifact rather
than a serialized runtime format.

This checkpoint does not make a general source use such as `Box<i32>` compile
through the application WIR or appear in WVB Types. The connection is now
smaller and explicit, but it remains required before Slice 4 is complete.

## Reconsideration triggers

Add reachability filtering if later analysis can admit unused instances into a
shared catalog. Replace incremental immutable concatenation with a measured
fixed-evidence builder if representative large generic workloads show that the
bounded current construction is a material hot path.
