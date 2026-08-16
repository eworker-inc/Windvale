# Decision 0653: Windvale-owned client return and init resume

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0652](0652-Windvale-Owned-Client-User-Context-Transfer.md)
- Contract: [client-return and init-transfer emission](../../Specifications/Windvale-Os-X64-Process-Client-Return-Init-Transfer-Emission.md)

## Decision

Emit fixture offsets 14,577 through 14,907 as one fail-closed client-return and
init-resume transaction. Require exact client syscall, thread, and process state,
move the client to waiting, dispatch only to the admitted init generation,
reactivate its checked page table, restore its saved context, and return through
`sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`382e1acec4dd1b287bf3a183c30c02443e4e170334406c13683955cbf58ac4f7`.
The focused owner advances to thirty-two projects and 192 cases with results 50
through 81. Windvale source owns the first 14,908 process-machine bytes and 102
external relocation fields.

The exact machine path now has guarded client entry and return. The following
init/provider exchange, syscall and exception handler bodies, context switching,
and live QEMU application execution remain separate evidence.

## Reconsideration triggers

Another return design must retain exact syscall/process/thread state checks,
explicit waiting-state publication, admitted init selection, page-table
revalidation, saved-context restoration, and fail-closed transfer.
