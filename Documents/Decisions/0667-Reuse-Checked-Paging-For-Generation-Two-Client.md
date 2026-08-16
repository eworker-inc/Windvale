# Decision 0667: Reuse checked paging for generation-2 client

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0666](0666-Windvale-Owned-Generation-Two-Client-Record.md)
- Contract: [generation-2 client-paging emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Paging-Emission.md)

## Decision

Source-own fixture offsets 20,241 through 24,988 by reusing the existing checked
4,748-byte recyclable-client paging constructor under an explicit generation-2
ownership window. Do not duplicate an identical byte sequence or create a
parallel paging policy.

## Evidence and consequences

Both fixture regions are byte-for-byte equal at SHA-256
`824ec2c944b5bebe479bf785eb2e30eeb05d06e04e95245e90c83cea27585a62`.
The focused owner advances to forty-two projects and 252 cases with results 50
through 91. Windvale source owns the first 24,989 process-machine bytes and 120
external relocation fields.

Generation 2 now reconstructs the same private W^X, null-hole, identity-window,
and guard-page topology as generation 1 without semantic drift. Image copies,
resources, context, endpoint rebinding, publication, and re-entry remain.

## Reconsideration triggers

Any generation-specific paging requirement must become an explicit policy and
test difference. Silent byte divergence or a copied parallel constructor is not
accepted as a generation distinction.
