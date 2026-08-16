# Decision 0684: Atomic transaction leaf rewrite

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0682](0682-Canonical-Bounded-Transaction-Mutations.md)
- Defines: [transaction leaf rewrite](../../Specifications/Windvale-Database-Transaction-Leaf-Rewrite.md)

## Context

`WVTM 1` gives Windvale one bounded transaction description, but the tree
planner still needs to consume it without exposing partial work. Re-decoding
and copying the whole mutation set for every operation would also waste memory.

## Decision

- Decode and own `WVTM 1` once, then expose a read of that decoded record for
  bounded consumers.
- Add one portable leaf-rewrite component that applies the complete mutation
  set in memory and returns bytes only after success.
- Treat missing deletes as successful no-ops and detect a byte-identical final
  leaf explicitly.
- On any later failure, return no partial leaf and zero counts.
- Reuse the individually verified `WVTN 1` put/delete operations for the first
  reference implementation. Keep its 32-operation work bound visible and
  optimize to a merge pass only with measurements.

## Evidence

The focused build produces a deterministic 60,297-byte WVB with SHA-256
`619e50dbde9630288dcac60bc49e173f9bf633880be5250c55eeb85167e6db97`.
It lowers to a 708,394-byte WVO with SHA-256
`5712da34a60fcd64241f1b852c8fe784834e95a34c106438f0764b556e157416`
and packages as a 725,504-byte Windows application with SHA-256
`49892fb606b8695b51cf732839ee311545d1e216d38d9d07c66fe2c3659f83a1`.
The application returns zero.

Twenty fresh whole-process runs measured 21.871 ms minimum, 23.147 ms median,
23.561 ms mean, and 29.525 ms maximum. Peak sampled working set was 7,491,584
bytes. This includes startup and all valid/failure tests; it is not server
transaction throughput.

Changed-file planning passes 24 general and 131 native routing cases. Native
development dependency closure passes for 3 owners and 34 declarations; WVB
verification, deterministic WVO comparison, Bash syntax, and diff checks pass.
The normal Linux-focused wrapper retains the WSL `node` environment limitation
recorded by Decision 0682, so this evidence uses the same repository Windows
native front door, lowerer, linker, and hosted packager path.

## Consequences

The shared tree planner now has an all-or-nothing leaf primitive and can avoid
repeated full transaction decoding. Windvale still does not claim durable
multi-record transactions: durable atomicity requires path grouping, split
handling, one shared ancestor rewrite, and one commit batch.

## Reconsideration triggers

Replace repeated leaf operations with one merge pass when a persistent-server
benchmark shows material CPU or allocation cost. Preserve identical framing,
failure, count, and deterministic-byte behavior.
