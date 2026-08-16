# Decision 0662: Windvale-owned first-client completion cleanup

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0660](0660-Windvale-Owned-Client-Directory-Reply-Delivery.md)
- Contract: [client-completion cleanup emission](../../Specifications/Windvale-Os-X64-Process-Client-Completion-Cleanup-Emission.md)

## Decision

Emit fixture offsets 16,573 through 17,923 as one fail-closed terminal cleanup
transaction. Validate the exiting generation-1 client, dormant compatibility
arena, both endpoint/channel records, generation identities, page mappings, and
retained message geometry before clearing both endpoint PTEs, scrubbing all IPC
destination/message fields, and closing both endpoint records.

## Evidence and consequences

The normalized slice SHA-256 is
`6b66ef89c367d568bf54b3bf07c8d123d06ef72054d61f6d02b19aa1734bfb9c`.
The focused owner advances to thirty-eight projects and 228 cases with results
50 through 87. Windvale source owns the first 17,924 process-machine bytes and
107 external relocation fields.

The first client can now reach a checked, fully scrubbed terminal IPC state.
Memory-object reclamation, generation-2 reconstruction, later lifecycle,
handler bodies, context switching, and live QEMU execution remain separate.

## Reconsideration triggers

Another cleanup design must retain exact client, endpoint, channel, generation,
mapping, and message checks; clear all externally reachable aliases before
record reuse; scrub all transient fields; and fail closed before publication.
