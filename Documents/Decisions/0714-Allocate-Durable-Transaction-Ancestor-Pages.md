# Decision 0714: Allocate durable transaction ancestor pages

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0712](0712-Group-Replacements-At-Any-Tree-Ancestor.md)
- Defines: [`WVAP 1`](../../Specifications/Windvale-Database-Transaction-Ancestor-Pages.md)

## Context

The transaction planner could logically rebuild every remaining ancestor level,
but `WVAG 1` did not assign durable page identities. A caller therefore could
not append those results or feed their replacements into the next level.

## Decision

- Introduce one portable allocation boundary that consumes a complete `WVAG 1`
  result and emits checksummed `WVPG 1` pages plus one complete `WVCR 1` plan.
- Keep the first transaction allocation page separate from the first page used
  by this round so leaf, branch, and later ancestor allocations share one
  append-only identity range.
- Mark exactly one level-one output as the completed root. Keep split old-root
  output as ordinary branches for a separate new-root operation.
- Bind the logical groups and page bytes to the compact manifest with SHA-256,
  then fully revalidate both companions before exposing a result.
- Retain the fixed 32-parent, 95-page, 3 MiB logical-group, 512 KiB replacement,
  and depth-eight complete-path limits.

## Evidence

The depth-three and depth-four scale-qualified projects build deterministically
to 201,297 and 198,815-byte WVBs with SHA-256
`ee2ed644233b8cc3034c63467aed5bc27904b3c30ed52312ec65c8b35ef0cd26`
and `f6c140935f7323291f336eb3d35463ba0a51160d3267df32d617060de79c8c90`.
Both verify through the native front door and return zero.

They lower deterministically to 3,124,718 and 3,052,209-byte WVOs with
SHA-256
`86895f86668fef50e94eed272ede0544330ae2f411d23c76f74777987e1130a6`
and `51cee4122802bc5914ecf5b9300db524468cabe528a26ee2edf92f044fd55e29`.
This leaves more than one MiB below the fixed 4 MiB native code limit.

The packaged Windows applications contain 3,142,656 and 3,069,952 bytes with
SHA-256
`b8407d52433f77910a36d61ca77aa1780b98bb647dbe69c74509d6da9af2e449`
and `6fe075e0b27fbbc92ddd5cb3667739f1cd5b6d8154605452c9225bd188046e34`.
Independent repeat builds reproduced every WVB, WVO, and executable byte.

Twenty fresh sampled whole-process runs measured 156.643 and 128.844 ms
medians and 158.766 and 131.226 ms means. Sampled peak working sets were
10,997,760 and 9,621,504 bytes. The larger depth-three case includes malformed,
oversized, digest, and invalid-page validation. These are correctness-test
costs including process startup, not persistent-server throughput.

Focused tests cover completed-root and intermediate-branch output, deterministic
bytes, exact page and replacement metadata, invalid allocation, truncation,
oversize input, bad magic, digest mismatch, and a digest-bound invalid page.
Changed-file planning passes 24 general and 146 native routing cases, native
development dependency closure passes all 34 declarations, and shell syntax
plus whitespace checks pass. The standard focused Windows owner passes both
cases in 373.420 seconds with a changed upstream compiler and cold native tool,
project, link, and application caches; 261.020 seconds of that total creates the
shared compiler cache. Its immediate warm-cache run passes in 15.260 seconds,
including 1.490 seconds of cached tool preparation. Independent Linux execution
and broad qualification remain pending.

## Consequences

Every logical ancestor output can now become a durable page set with a generic
replacement plan. Depth-three transactions can finish one root; deeper
transactions can repeat the same grouping and allocation pair toward the root.

The bounded alternation loop, whole-transaction allocation proof, split-root
construction, compact-log assembly, superblock publication, and persistent
server remain later boundaries.

## Reconsideration triggers

Replace immutable page accumulation with a pre-sized arena or streaming writer
only when persistent-server measurements show material copy or peak-memory cost.
Increase page, group, path-depth, or manifest limits only with a revised format
contract, malicious-input coverage, and measured memory evidence.
