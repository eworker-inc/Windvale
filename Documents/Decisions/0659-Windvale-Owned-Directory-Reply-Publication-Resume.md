# Decision 0659: Windvale-owned directory reply-publication resume

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0657](0657-Windvale-Owned-Client-Directory-Request-Delivery.md)
- Contract: [directory reply-publication resume emission](../../Specifications/Windvale-Os-X64-Process-Directory-Reply-Publish-Resume-Emission.md)

## Decision

Emit fixture offsets 15,906 through 16,241 as one fail-closed reply-publication
transaction. Require the exact provider syscall/thread state and 3,096-byte
reply, clear its channel publication state, dispatch only the admitted provider
generation, reactivate its checked page table, restore its saved context, and
return zero through `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`7611c8e76784e456894a6c2e60315c6ee9af5cd9daa97c7e945be96f8afc8512`.
The focused owner advances to thirty-six projects and 216 cases with results 50
through 85. Windvale source owns the first 16,242 process-machine bytes and 106
external relocation fields.

The isolated provider now publishes the first complete directory reply and
resumes cleanly. Client delivery, syscall and exception handler bodies, context
switching, and live QEMU application execution remain separate evidence.

## Reconsideration triggers

Another publication design must retain exact syscall/thread and reply-shape
checks, channel-state clearing, admitted provider selection, page-table
revalidation, saved-context restoration, zero-result propagation, and
fail-closed transfer.
