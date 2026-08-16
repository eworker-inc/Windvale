# Decision 0654: Windvale-owned init reply-publication resume

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0653](0653-Windvale-Owned-Client-Return-And-Init-Resume.md)
- Contract: [init reply-publication resume emission](../../Specifications/Windvale-Os-X64-Process-Init-Reply-Publish-Resume-Emission.md)

## Decision

Emit fixture offsets 14,908 through 15,243 as one fail-closed init
reply-publication completion. Require exact init syscall/thread state and the
retained 116-byte reply record, clear its channel publication state, dispatch
only to the admitted init generation, reactivate its checked page table, restore
its saved user context, and return zero through `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`ea3769665f95a2054d4cc2594d743555a2893cc7067720add66ccf5ee995dc94`.
The focused owner advances to thirty-three projects and 198 cases with results
50 through 82. Windvale source owns the first 15,244 process-machine bytes and
103 external relocation fields.

Client delivery of the reply, later directory-provider exchanges, syscall and
exception handler bodies, context switching, and live QEMU application
execution remain separate evidence.

## Reconsideration triggers

Another reply-publication design must retain exact syscall/thread and reply
record checks, explicit channel-state clearing, admitted init selection,
page-table revalidation, saved-context restoration, and fail-closed transfer.
