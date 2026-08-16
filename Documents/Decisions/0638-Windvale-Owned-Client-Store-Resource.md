# Decision 0638: Windvale-owned client store resource

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0637](0637-Windvale-Owned-Client-Budget-Resource.md)
- Contract: [client store-resource emission](../../Specifications/Windvale-Os-X64-Process-Client-Store-Resource-Emission.md)

## Decision

Emit fixture offsets 10,160 through 10,398 as the exact private generation-three
store descriptor. Require its admitted digest and preserve the 1,196-byte bound,
rights, mutation fields, private pointers, and page-table derivation. Publication
and remaining resources stay mandatory later steps.

## Evidence and consequences

The 239-byte slice has SHA-256
`279526ded2e778bf10716f07a304ee82201feac248e33b1fc9dd463657704e7f`.
The focused owner passes 114 cases across nineteen projects with results 50
through 68. The retirement inventory is 70 suites and 3,678 cases. Windvale
source owns the first 10,399 machine bytes and 34 relocation fields.

## Reconsideration triggers

General metadata may replace fixed records but must preserve exact identity,
rights/generations, explicit mutation semantics, private construction,
readiness-only publication, and rollback.
