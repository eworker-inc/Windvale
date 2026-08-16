# Decision 0637: Windvale-owned client budget resource

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0636](0636-Windvale-Owned-Client-Program-Resource.md)
- Contract: [client budget-resource emission](../../Specifications/Windvale-Os-X64-Process-Client-Budget-Resource-Emission.md)

## Decision

Emit fixture offsets 9,931 through 10,159 as an exact private generation-two
budget descriptor. Require its 32-byte admitted digest and preserve the
four-byte payload bound, rights, generation, private pointers, response bounds,
and page-table derivation. Remaining resources and readiness publication stay
mandatory later steps.

## Evidence and consequences

The 229-byte slice has SHA-256
`c302afea1399673cc047272d17f712a301d9bff35c1c5df062eec2232776605f`.
The focused owner passes 108 cases across eighteen projects with results 50
through 67. The retirement inventory is 70 suites and 3,672 cases. Windvale
source owns the first 10,160 process-machine bytes and 34 relocation fields.

## Reconsideration triggers

General metadata may replace fixed records, but must preserve exact identity,
rights, generations, private construction, readiness-only publication, and
failure-atomic rollback.
