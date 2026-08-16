# Decision 0656: Windvale-owned client reply delivery

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0654](0654-Windvale-Owned-Init-Reply-Publication-Resume.md)
- Contract: [client reply-delivery emission](../../Specifications/Windvale-Os-X64-Process-Client-Reply-Delivery-Emission.md)

## Decision

Emit fixture offsets 15,244 through 15,574 as one fail-closed reply-delivery
transaction. Require exact syscall/thread state and the init-owned reply record,
dispatch only to the admitted client generation, reactivate its checked page
table, restore its saved context, and return the exact 116-byte result through
`sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`4e527e65c9007a43dd523b1fd8e2518a0be8699738d574177eb66b25b0ff8773`.
The focused owner advances to thirty-four projects and 204 cases with results 50
through 83. Windvale source owns the first 15,575 process-machine bytes and 104
external relocation fields.

The first client/init request/reply exchange is now source-owned through client
delivery. Later directory-provider exchanges, syscall and exception handler
bodies, context switching, and live QEMU application execution remain separate
evidence.

## Reconsideration triggers

Another delivery design must retain exact syscall/thread and reply-record
checks, admitted client selection, page-table revalidation, saved-context
restoration, exact result propagation, and fail-closed transfer.
