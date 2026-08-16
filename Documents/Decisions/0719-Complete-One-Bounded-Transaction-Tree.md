# Decision 0719: Complete one bounded transaction tree

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0715](0715-Grow-One-Bounded-Durable-Transaction-Root.md)
- Defines: [`WVTC 1`](../../Specifications/Windvale-Database-Transaction-Tree-Completion.md)

## Context

Every individual bottom-up operation existed, but a caller still had to
manually alternate ancestor grouping and page allocation and then choose
between a completed old root and split-root growth. That gap prevented one
general bounded transaction from producing a complete consecutive tree-page
batch.

## Decision

- Introduce one portable coordinator for input root depths two through eight.
- Reuse the existing leaf, parent, ancestor, and root-growth planners without
  duplicating their node, partition, allocation, or validation rules.
- Run exactly `root depth - 2` ancestor rounds after the immediate-parent
  round, then invoke root growth only when the old root remains split.
- Aggregate every new tree page in allocation order and require the completed
  root to be the final page.
- Preserve logical no-op without advancing generation or returning pages.
- Cap output at 792 pages: 64 leaves, 95 immediate parents, 570 ancestor pages,
  and 63 root-growth pages. This is at most 51,904,512 page bytes.
- Bind mutations, exact paths, and all output pages in `WVTC 1`; validation
  replays the complete composition and requires byte-identical output.
- Use the repository's staged native-object and segmented hosted-image path for
  the composed tests because their native code exceeds the ordinary 4 MiB
  complete-object limit. Do not widen or bypass that ordinary limit.

## Evidence

The depth-four and split-root scale-qualified projects build to 355,563 and
351,557-byte WVBs with SHA-256
`ce97b087453ce20e7e17c9a04e8d830d22c8ccfc9b43816cde278b4317365480`
and `d59cf836615243a01eb5245a08d130dd1e83738b270fb635a2d10ad650359ea5`.

Their staged WVOs contain 5,902,324 and 5,795,199 bytes. Canonical linked
images contain 5,893,411 and 5,786,490 bytes and are transported in two bounded
segments. The packaged Windows applications contain 5,921,280 and 5,814,784
bytes with SHA-256
`94e34c933793b78973cc7e1a28fcc62cd47f305ae6082877ad5ba97a68c371de`
and `3e3f9ce7362a593b3e28a63dfe84ad5ed0ad6fe335bb74bc07350a61d29cbf1c`.
Both return zero.

The fully warm focused Windows owner passes both cases in 64.260 seconds,
including 1.610 seconds of cached tool preparation. The depth-four and
split-root cases take 31.470 and 30.890 seconds respectively, including
current-source WVB rebuild and segmented staging.

Twenty fresh whole-process runs measured 1,573.780 and 858.226 ms medians and
1,582.518 and 871.316 ms means. Sampled peak working sets were 12,881,920 and
12,361,728 bytes. The depth-four executable plans twice and performs one full
replay validation; the split-root executable plans once and performs one full
replay validation. These measurements include process startup and test-side
replay, not persistent-server throughput.

Changed-file planning passes 24 general and 149 native routing cases. Native
development dependency closure passes all 34 declarations; shell syntax and
whitespace checks pass. Independent Linux execution and broad qualification
remain pending.

## Consequences

A bounded changed transaction over an input tree of depth two through eight
can now produce one complete, deterministic, consecutive durable tree-page
batch and exactly one root. The split-root path is exercised through the same
coordinator rather than only as an isolated operation.

The batch is not yet a published commit. The next boundary must add complete
obsolete-page ownership, one compact commit-log page, target superblock bytes,
and the existing durable publication action plan. Provider-driven path
discovery, persistent process ownership, secondary indexes, and external
benchmarks remain later milestones.

## Reconsideration triggers

Replace immutable page aggregation when persistent-process evidence shows
material copy or peak-memory cost. Revisit native segmentation only when the
native backend gains a smaller-code mode or ordinary complete objects can hold
the same composition without weakening current limits. Increase transaction
page or depth bounds only with revised format limits, malicious-input tests,
and measured memory evidence.
