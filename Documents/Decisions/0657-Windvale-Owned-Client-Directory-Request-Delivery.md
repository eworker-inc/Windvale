# Decision 0657: Windvale-owned client directory-request delivery

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0656](0656-Windvale-Owned-Client-Reply-Delivery.md)
- Contract: [client directory-request delivery emission](../../Specifications/Windvale-Os-X64-Process-Client-Directory-Request-Delivery-Emission.md)

## Decision

Emit fixture offsets 15,575 through 15,905 as one fail-closed directory-request
delivery transaction. Require the exact client syscall/thread state and queued
37-byte request, dispatch only to the admitted directory-provider generation,
reactivate its checked page table, restore its saved context, and return the
exact request length through `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`ee7ff64dd4ea0ecf7f91b6ca184e5ab9007e0bd105e14326db44bd0165daadaa`.
The focused owner advances to thirty-five projects and 210 cases with results
50 through 84. Windvale source owns the first 15,906 process-machine bytes and
105 external relocation fields.

The first directory request now reaches the isolated provider through a checked
machine transition. Provider receive completion, response publication and
delivery, syscall and exception handler bodies, context switching, and live
QEMU application execution remain separate evidence.

## Reconsideration triggers

Another delivery design must retain exact syscall/thread and request-shape
checks, admitted directory-provider selection, page-table revalidation,
saved-context restoration, exact request-length propagation, and fail-closed
transfer.
