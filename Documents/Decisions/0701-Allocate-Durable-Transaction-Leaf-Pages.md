# Decision 0701: Allocate durable transaction leaf pages

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0694](0694-Carry-Bulk-Leaf-Partitions-In-Transaction-Groups.md)
- Defines: [`WVLD 1`](../../Specifications/Windvale-Database-Transaction-Leaf-Pages.md)

## Context

`WVLG 2` contains complete final leaf partitions, but branch rewriting cannot
refer to them until every replacement has a stable durable page identity. Page
allocation must also preserve copy-on-write ancestry, one target generation,
one target sequence, deterministic order, and bounded memory without repeating
the mutations.

## Decision

- Assign replacement pages consecutively from the committed page count in
  changed-group and partition order.
- Encode every replacement immediately as a complete checksummed `WVPG 1`
  page at the one next generation and commit sequence.
- Record the old leaf only as the first replacement's predecessor. Additional
  pages from the same split have no separate predecessor.
- Retain a depth-one, one-page replacement as `Root`; use `Leaf` for all other
  data pages so the next planner can grow or replace ancestors explicitly.
- Emit a compact versioned replacement map and validate the map, all durable
  pages, all leaf nodes, exact counts, and separators before returning bytes.
- Allocate nothing for unchanged groups and emit a small valid no-change plan.

## Evidence

The scale-qualified project build produces a deterministic 186,087-byte WVB
with SHA-256
`a07399791aa51acebd68c7460e36e8b1e9053f6bf7280defb23fd962706298b7`.
It lowers to a deterministic 2,911,527-byte WVO with SHA-256
`a404a9e86d2f10e5d5a868d1159fd951b1536a0b88b386fd4973fd9b0b8e2794`
and packages as a 2,929,152-byte Windows application with SHA-256
`daa1e474acac6a2b847e1d487f3b88350218d21b875af95e1a2cdcb7c664026e`.
The application returns zero.

Twenty fresh whole-process runs measured 176.557 ms minimum, 185.624 ms
median, 188.851 ms mean, and 229.848 ms maximum. Peak sampled working set was
10,240,000 bytes. This includes startup, SHA-256 page encoding and decoding,
deterministic comparison, two-group allocation, root replacement, leaf
partitioning, and malformed cases. It is correctness-test cost, not persistent
server transaction throughput.

Changed-file planning passes 24 general and 137 native routing cases. Native
development dependency closure passes for 3 owners and 34 declarations. WVB
verification, deterministic WVB/WVO comparison, Bash syntax, and diff checks
pass. The focused project uses the scale-qualified current project build
driver; lowering, packaging, and execution use repository-native tools.

The normal Windows development target passes its one focused leaf-page case in
56.650 seconds including a cached 1.620-second tool setup. While recording that
evidence, the Windows command wrapper was brought to parity with the Linux
script for all six transaction targets and the 32-case development count.

One broader changed-file run passed tool setup, 18 existing portable database
stages, and seven hosted stages through engine recovery. Its final hosted
tree-writer stage reached the existing native `Outputˉlimit`; therefore this
decision makes no complete broad-gate or cross-host qualification claim.

## Consequences

The transaction now has concrete durable data pages that a shared-parent
planner can reference without reapplying mutations. Changed leaf work remains
fully deterministic and bounded at 64 pages plus a compact map.

The portable byte builder currently concatenates fixed pages and therefore
copies intermediate page buffers. This is acceptable bounded evidence for the
portable planner, but the persistent server must measure it and may replace it
with a pre-sized arena without changing `WVLD 1` semantics.

No branch page or commit is published yet. Failure after this planning step is
still in-memory failure with no storage mutation.

## Reconsideration triggers

Change the map layout or construction strategy if persistent-server profiles
show material copy or decode cost. Any replacement must keep explicit limits,
complete pre-use validation, deterministic page order, and exact predecessor
semantics.
