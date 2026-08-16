# Decision 0669: Reuse checked image for generation-2 client

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0667](0667-Reuse-Checked-Paging-For-Generation-Two-Client.md)
- Contract: [generation-2 client-image emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Image-Emission.md)

## Decision

Source-own fixture offsets 24,989 through 25,064 by reusing the existing checked
76-byte interpreter-copy and execution-context constructor under an explicit
generation-2 ownership window.

## Evidence and consequences

Both fixture regions are byte-for-byte equal at SHA-256
`54432a2880a44c20e9c9246eeab45a488a9f9aa7746d2eff9aaef0671faac633`.
The focused owner advances to forty-three projects and 258 cases with results 50
through 92. Windvale source owns the first 25,065 process-machine bytes and 121
external relocation fields.

Generation 2 now reconstructs the same admitted interpreter and bounded native
context seed without a parallel implementation. Endpoint rebinding, resources,
remaining context state, publication, and re-entry remain.

## Reconsideration triggers

Any generation-specific image or context requirement must be explicit and
tested. Silent byte divergence or an unreviewed symbol change is rejected.
