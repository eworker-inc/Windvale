# Decision 0639: Windvale-owned client directory resource

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0638](0638-Windvale-Owned-Client-Store-Resource.md)
- Contract: [client directory-resource emission](../../Specifications/Windvale-Os-X64-Process-Client-Directory-Resource-Emission.md)

## Decision

Emit fixture offsets 10,399 through 10,637 as the exact private generation-four
directory descriptor. Require its admitted snapshot digest and preserve its
3,184-byte immutable bound, rights, private provider pointers, and page-table
derivation. Publication and remaining resources stay mandatory later steps.

## Evidence and consequences

The slice SHA-256 is
`47b9812c671bc35bc5b5e4067dde749e203354f92ad38a75c2a575fb53ef78b9`.
The owner passes 120 cases across twenty projects with results 50 through 69.
The retirement inventory is 70 suites and 3,684 cases. Windvale source owns the
first 10,638 process-machine bytes and 34 relocation fields.

## Reconsideration triggers

General metadata may replace fixed records but must preserve exact identity,
least rights, immutable bounds, private construction, readiness-only publication,
and rollback.
