# Decision 0712: Group replacements at any tree ancestor

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0711](0711-Allocate-Durable-Transaction-Branch-Pages.md)
- Defines: [`WVAG 1`](../../Specifications/Windvale-Database-Transaction-Ancestor-Groups.md)

## Context

The transaction planner could create durable replacement leaves and one level
of durable branches. Its parent grouping was specialized to leaf replacements,
so the `WVCR 1` emitted by those branch pages could not yet be consumed at the
next ancestor.

## Decision

- Introduce one explicit child-level operation that consumes any complete
  `WVCR 1` branch replacement plan.
- Revalidate mutations and full committed paths before locating old children
  and their parents.
- Combine consecutive children owned by one parent, apply one `WVBP 1` final
  state to that parent, and reject non-adjacent duplicate parent output.
- Keep logical grouping separate from page allocation so each boundary has
  its own format, limits, malformed-input coverage, and performance evidence.
- Retain the current depth-eight and 16 MiB path bound rather than making
  recursive work depend on unbounded path memory.

## Evidence

The depth-three and depth-four scale-qualified projects build deterministically
to 180,714 and 176,158-byte WVBs with SHA-256
`5c87a7d2576a11ab3e51953c341b2906c2c734e4ccce03e76a3483e098e48842`
and `e078ec3b41ee13015e1da1cd8ade5b4811839627f3c322f1e53587cd0104ef68`.
Both verify through the native front door.

They lower deterministically to 2,699,723 and 2,610,719-byte WVOs with
SHA-256
`6a39ac56a4a8d0100d08221fd4404e055af65433659b5dd3f600019e9520350a`
and `756f2130355c8c9aba82938a62263d5c2590e5738e2791750e35a10896d04e98`.
This leaves substantial room below the fixed 4 MiB native code limit.

The packaged Windows applications contain 2,717,184 and 2,628,096 bytes with
SHA-256
`4e92477a4a5359b8af8dc5445186740bba46ee00d11396c97e80fbb74ed5b62e`
and `c72c9845c1056d5e2c5baf36823eb071caa39d421bcfb8f047e22063200b0552`.
Both return zero.

Twenty fresh sampled whole-process runs measured 110.746 and 62.433 ms
medians and 112.632 and 63.638 ms means. Sampled peak working sets were
18,829,312 and 9,170,944 bytes. The larger depth-three peak includes its
explicit 3 MiB oversize rejection case. These are correctness-test costs,
including process startup, not persistent-server throughput.

The focused Windows development target passed both cases in 60.510 seconds
with one cold project, link, and application cache. Its post-rebase warm-cache
run passes in 14.060 seconds, including 1.550 seconds of cached tool setup.
Changed-file planning passes 24 general and 145 native routing cases.
Independent Linux execution and broad qualification remain pending.

## Consequences

Windvale can now rebuild a logical parent level from durable child
replacements at either the root or an intermediate branch level. The same
operation applies while `Childˉlevel` decreases toward one.

Durable identities for these logical outputs, loop-wide page and memory
budgets, final root creation, compact-log construction, and superblock
publication remain explicit later boundaries.

## Reconsideration triggers

Increase the supported path depth only with a revised bounded path contract
and measured memory evidence. Replace immutable output construction only if
persistent-server measurements show material copy cost while preserving
deterministic bytes, exact limits, full validation, and atomic failure.
