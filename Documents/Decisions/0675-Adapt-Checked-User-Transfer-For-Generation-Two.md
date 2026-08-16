# Decision 0675: Adapt checked user transfer for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0672](0672-Windvale-Owned-Generation-Two-Return-Validation.md)
- Contract: [generation-2 client user-transfer emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-User-Transfer-Emission.md)

## Decision

Derive fixture offsets 26,965 through 27,138 from the existing checked client
user-transfer constructor. Change only its selected-client generation byte from
1 to 2, and supply the later fixture position's exact internal displacements.

## Evidence and consequences

The normalized payload differs from generation 1 only at byte 41 and has
SHA-256
`4dd0b6f855e8bcbce9f719d520d3c1902d4a71a65528e887950fc578e86ce9b7`.
The focused owner advances to forty-seven projects and 282 cases with results
50 through 96. Windvale source owns the first 27,139 process-machine bytes and
230 internal or external relocation fields.

The second generation-2 user entry reuses one transfer contract rather than
creating a parallel page-table/GS/context policy. Its return handler, later
lifecycle, and live QEMU evidence remain.

## Reconsideration triggers

Another transfer path must preserve the one-byte generation distinction,
dispatcher and continuation targets, external page-table activation symbol,
GS publication, private context restoration, and exact `sysretq` boundary.
