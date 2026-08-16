# Decision 0680: Adapt checked directory-request delivery for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0678](0678-Adapt-Checked-Reply-Delivery-For-Generation-Two.md)
- Contract: [generation-2 client directory-request delivery emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Directory-Request-Delivery-Emission.md)

## Decision

Derive fixture offsets 28,137 through 28,467 from the existing checked client
directory-request delivery constructor. Change only the selected directory
provider generation from 1 to 2, then supply the later fixture position's exact
internal displacements.

## Evidence and consequences

The normalized payload differs from generation 1 only at byte 112 and has
SHA-256
`4aa89eaad181e85386ced312c77e59981684e6e5470a3e56f95f298fe32cd5aa`.
The focused owner advances to fifty-one projects and 306 cases with results 50
through 100. Windvale source owns the first 28,468 process-machine bytes and
290 internal or external relocation fields.

The generation-two directory request delivers the exact 37-byte request to the
selected isolated provider through the same dispatcher, page-table, context,
and `sysretq` contract. Provider receive completion, the later lifecycle, and
live QEMU evidence remain.

## Reconsideration triggers

Another directory-request delivery path must preserve the explicit selected
provider generation, exact request result, dispatcher and continuation targets,
external page-table activation, and provider `sysretq` boundary.
