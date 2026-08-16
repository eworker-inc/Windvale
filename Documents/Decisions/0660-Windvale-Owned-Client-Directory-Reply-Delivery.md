# Decision 0660: Windvale-owned client directory-reply delivery

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0659](0659-Windvale-Owned-Directory-Reply-Publication-Resume.md)
- Contract: [client directory-reply delivery emission](../../Specifications/Windvale-Os-X64-Process-Client-Directory-Reply-Delivery-Emission.md)

## Decision

Emit fixture offsets 16,242 through 16,572 as one fail-closed reply-delivery
transaction. Require exact syscall/thread state and the 3,096-byte reply,
dispatch only the admitted client generation, reactivate its checked page
table, restore its saved context, and return the exact length through `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`4b0fb59cdbab8e6e88aed5e26c89a26402148a548fd93fc1884fda7cfc120c55`.
The focused owner advances to thirty-seven projects and 222 cases with results
50 through 86. Windvale source owns the first 16,573 process-machine bytes and
107 external relocation fields. The first directory request/reply round trip is
now source-owned; handlers, context switching, later lifecycle, and live QEMU
execution remain separate evidence.

## Reconsideration triggers

Another delivery design must retain exact state and reply-shape checks, admitted
client selection, page-table revalidation, saved-context restoration, exact
result propagation, and fail-closed transfer.
