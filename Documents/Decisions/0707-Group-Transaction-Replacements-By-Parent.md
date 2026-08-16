# Decision 0707: Group transaction replacements by parent

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0703](0703-Bulk-Partition-Transaction-Branches.md)
- Defines: [`WVPP 1`](../../Specifications/Windvale-Database-Transaction-Parent-Groups.md)

## Context

The transaction layer can allocate durable replacement leaf pages and can
bulk-rebuild one branch from several changed children. Those two operations
were still separate: no planner associated changed leaves with the parents in
the validated transaction paths. Calling the branch partitioner independently
for each leaf would repeat work and could lose the parent's complete final
state.

## Decision

- Run leaf-page planning once, then use the same proven paths to locate the
  branch immediately above every changed leaf.
- Combine consecutive changed leaves owned by the same parent and apply one
  complete `WVCR 1` plan to that parent exactly once.
- Return logical `WVBP 1` parent outputs separately from the durable `WVLD 1`
  leaf pages so branch allocation remains an explicit next boundary.
- Encode the handoff as bounded, versioned `WVPP 1`, with complete embedded
  partition validation and globally unique parent identities.
- Preserve a valid no-change result that allocates no pages and performs no
  publication work.

## Evidence

The scale-qualified project build produces a deterministic 246,222-byte WVB
with SHA-256
`bf43d654d1f062b984e61fa416e22ae31c752f62719c19ed1232fe4e82e34992`.
It verifies through the native front door and lowers deterministically to a
3,768,690-byte WVO with SHA-256
`293f695f8ad4569941ea38cc611568331875f4dc1f9bd46b6f69e77faad3d0bd`.
The packaged 3,786,240-byte Windows application has SHA-256
`81b002c0290cfc2b6b0d129d9347e5c30359e50859a99b40aeb383cb68dba889`
and returns zero.

Twenty fresh sampled whole-process runs measured 178.891 ms minimum, 186.375
ms median, 189.670 ms mean, and 216.137 ms maximum. Peak sampled working set
was 20,279,296 bytes. This includes native process startup, both shared- and
separate-parent plans, deterministic comparison, routing checks, malformed
cases, the explicit 3 MiB oversize boundary, and duplicate-parent rejection.
It is correctness-test cost, not persistent-server throughput.

The cold-cache focused Windows development target passes its one case in
76.250 seconds, including 1.690 seconds of cached tool setup and creation of
the project, link, and application caches. Changed-file planning passes 24
general and 141 native routing cases. WVB verification, deterministic WVB/WVO
comparison, Windows target execution, and focused test execution pass.
Independent Linux execution and broad qualification remain pending.

## Consequences

Transactions now cross the first shared-ancestor boundary correctly. Several
changed leaves under one parent cause one final parent rebuild, while leaves
under different parents produce separate bounded groups. The same grouping
shape can be reused at every higher tree level after durable branch pages are
allocated.

`WVPP 1` is not a commit and does not assign branch page identities. Durable
branch allocation, recursive upward propagation, root completion, compact-log
construction, and superblock publication remain explicit later milestones.

## Reconsideration triggers

Replace immutable result construction only if persistent-server measurements
show material copy cost or memory pressure. Any replacement must preserve
deterministic bytes, exact final-state grouping, one rebuild per parent,
bounded work, full validation before use, and atomic failure.
