# Decision 0640: Windvale-owned client store validation

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-16
- Advances: [Decision 0639](0639-Windvale-Owned-Client-Directory-Resource.md)
- Contract: [client store-validation emission](../../Specifications/Windvale-Os-X64-Process-Client-Store-Validation-Emission.md)

## Decision

Emit fixture offsets 10,638 through 11,031 as the exact generation-three
immutable-store validation path. Validate identity, geometry, rights,
generation, digest, private pointers, page-table linkage, and W^X permissions
before the recyclable client can use the store. Keep all twenty-two failure
branches explicit and retain readiness publication as a later step.

## Evidence and consequences

The normalized slice SHA-256 is
`104dbc9735859a1ac61f3d03e47c613d60ea9eea665c418db211dab650f62ec7`.
The owner passes 126 cases across twenty-one projects with results 50 through
70. The retirement inventory is 70 suites and 3,690 cases. Windvale source owns
the first 11,032 process-machine bytes and 56 relocation fields.

## Reconsideration triggers

A generalized resource validator may replace the fixed path, but must preserve
exact identity, least rights, immutable bounds, private construction, W^X,
generation checks, explicit failure routing, readiness-only publication, and
rollback.
