# Decision 0641: Windvale-owned client directory validation

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-16
- Advances: [Decision 0640](0640-Windvale-Owned-Client-Store-Validation.md)
- Contract: [client directory-validation emission](../../Specifications/Windvale-Os-X64-Process-Client-Directory-Validation-Emission.md)

## Decision

Emit fixture offsets 11,032 through 11,441 as the exact generation-four
read-only-directory validation path. Validate identity, geometry, rights,
generation, digest, private pointers, snapshot count, page-table linkage, and
W^X permissions before use. Keep all twenty-three failure branches explicit and
retain readiness publication as a later step.

## Evidence and consequences

The normalized slice SHA-256 is
`204d82fd3eebd1e2d99ad5c0e5fd35a4466406d7696c78cb3449e22c5360dd08`.
The owner passes 132 cases across twenty-two projects with results 50 through
71. The retirement inventory is 70 suites and 3,696 cases. Windvale source owns
the first 11,442 process-machine bytes and 79 relocation fields.

## Reconsideration triggers

A generalized descriptor validator may replace this fixed path only if it
preserves least rights, immutable bounds, snapshot identity, private
construction, W^X, generation checks, explicit failure routing,
readiness-only publication, and rollback.
