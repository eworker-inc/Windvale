# Decision 0687: Bounded transaction path set

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0684](0684-Atomic-Transaction-Leaf-Rewrite.md)
- Defines: [transaction paths](../../Specifications/Windvale-Database-Transaction-Paths.md)

## Context

Atomic leaf rewrite is not enough for mutations routed to different leaves.
The shared planner must receive all affected paths from one snapshot and must
reject stale, incorrectly routed, or internally inconsistent page views before
allocating replacement page identities.

## Decision

- Supply one exact root-to-leaf path per canonical mutation. Keep the simple
  repeated representation for the first planner, bounded to 16 MiB.
- Validate depths one through eight and all currently admitted durable page
  sizes without unchecked path-length arithmetic.
- Prove page identity, snapshot visibility, node kind/count, key routing, child
  age, committed-page bounds, and inherited leaf ranges for every path.
- Require byte-identical pages whenever adjacent sorted-key paths share an
  identity at one level.
- Report consecutive unique leaf groups so the successor can apply each leaf
  once and can later replace repeated path storage with a compact collector
  without changing validation semantics.

## Evidence

The scale-qualified project build produces a deterministic 122,317-byte WVB
with SHA-256
`9c5bf582272d7bac70087a6deb6db2f9c295b326a9d979186da9db0201dd2944`.
It lowers to a 1,910,714-byte WVO with SHA-256
`d5df8dbbbf3f298395fb4be40b99aa47b9ddca94f24fc2df4732e902e0e0809d`
and packages as a 1,928,192-byte Windows application with SHA-256
`2bf5144617b82aabf64d165ef02c0873c6c42803f37c5339b3e38e485e5bcf35`.
The application returns zero.

Twenty fresh whole-process runs measured 63.949 ms minimum, 67.294 ms median,
67.978 ms mean, and 80.131 ms maximum. Peak sampled working set was 8,749,056
bytes. This includes startup, SHA-256 page construction and validation, and all
positive and malformed cases; it is not server transaction throughput.

Changed-file planning passes 24 general and 132 native routing cases. Native
development dependency closure passes for 3 owners and 34 declarations; WVB
verification, deterministic WVB/WVO comparison, Bash syntax, and diff checks
pass. The current small front door reaches its known function-directory limit
on this 117-function project, so source compilation uses the scale-qualified
current project build driver; lowering, linking, packaging, and execution use
the repository's current Windows native tools.

## Consequences

The planner now has a bounded and adversarially validated complete-path input
instead of trusting provider-discovered byte arrays independently. It still
does not claim atomic multi-record durability; shared bottom-up rewrite and one
commit publication remain required.

The repeated-path format spends memory to keep the first contract simple. A
provider collector can deduplicate after measurements, but any replacement
must preserve the same snapshot and shared-page consistency proof.

## Reconsideration triggers

Replace repeated paths when persistent-server measurements show material copy
or memory cost. Lower the 16 MiB portable ceiling if server admission evidence
shows a smaller practical limit; do not raise it without planner and memory
measurements.
