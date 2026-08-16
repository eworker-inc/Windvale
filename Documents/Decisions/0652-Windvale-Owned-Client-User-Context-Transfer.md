# Decision 0652: Windvale-owned client user-context transfer

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0650](0650-Windvale-Owned-Remaining-Client-Resource-Validation.md)
- Contract: [client user-transfer emission](../../Specifications/Windvale-Os-X64-Process-Client-User-Transfer-Emission.md)

## Decision

Emit fixture offsets 14,403 through 14,576 as one guarded client-transfer
transaction. Leave the returning init context, dispatch only to the admitted
client role and generation, reactivate its checked private page table, bind GS
and continuation ownership, load its private instruction and stack context, and
enter user mode only through `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`74a9b3c03618324e6acdbc56f088c9385be19b7d71fa2ecd9ce4eca8e13f8a84`.
The focused owner advances to thirty-one projects and 186 cases with results 50
through 80. Windvale source owns the first 14,577 process-machine bytes and 101
external relocation fields.

The current exact machine path can now reach the admitted client's first user
instruction. Client return, syscall and exception handler bodies, context
switching, and live QEMU application execution remain separate evidence.

## Reconsideration triggers

Another client-entry design must retain admitted role/generation selection,
page-table revalidation, per-thread GS ownership, exact continuation and private
user context, and fail-closed transfer.
