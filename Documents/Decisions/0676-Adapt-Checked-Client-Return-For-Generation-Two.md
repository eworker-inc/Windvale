# Decision 0676: Adapt checked client return for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0675](0675-Adapt-Checked-User-Transfer-For-Generation-Two.md)
- Contract: [generation-2 client-return/init-transfer emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Return-Init-Transfer-Emission.md)

## Decision

Derive fixture offsets 27,139 through 27,469 from the existing checked
client-return/init-transfer constructor. Change only its returning-client
generation byte from 1 to 2 and supply the later fixture position's exact
internal displacements.

## Evidence and consequences

The normalized payload differs from generation 1 only at byte 112 and has
SHA-256
`c8ca2e13217d55c420ff809110d0ea8596e09a99b69a01b8fbb0fb3be8f4d9c0`.
The focused owner advances to forty-eight projects and 288 cases with results
50 through 97. Windvale source owns the first 27,470 process-machine bytes and
245 internal or external relocation fields.

Generation-two client completion now returns result 55 to init through the
same page-table, GS, continuation, and `sysretq` contract as generation 1. The
following init handler, later lifecycle, and live QEMU evidence remain.

## Reconsideration triggers

Another return path must preserve the one-byte client-generation distinction,
processor/thread result checks, dispatcher and continuation targets, external
page-table activation symbol, result 55, and exact init `sysretq` boundary.
