# Decision 0678: Adapt checked reply delivery for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0677](0677-Adapt-Checked-Init-Reply-For-Generation-Two.md)
- Contract: [generation-2 client reply-delivery emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Reply-Delivery-Emission.md)

## Decision

Derive fixture offsets 27,806 through 28,136 from the existing checked client
reply-delivery constructor. Change only operation 4 to 8 and the three retained,
thread, and selected client-generation values from 1 to 2, then supply the later
fixture position's exact internal displacements.

## Evidence and consequences

The normalized payload differs from generation 1 only at bytes 44, 112, 126,
and 207 and has SHA-256
`4b9a80c7c1bd457cc37133c3ccec39e90003c7ec41842c358dc1d195c801afad`.
The focused owner advances to fifty projects and 300 cases with results 50
through 99. Windvale source owns the first 28,137 process-machine bytes and 275
internal or external relocation fields.

The generation-two reply delivery returns 116 to the selected client through
the same ownership, dispatcher, page-table, context, and `sysretq` contract.
The following dispatcher/handler path, later lifecycle, and live QEMU evidence
remain.

## Reconsideration triggers

Another reply-delivery path must preserve all four explicit state differences,
reply ownership, dispatcher and continuation targets, external page-table
activation, result 116, and the exact client `sysretq` boundary.
