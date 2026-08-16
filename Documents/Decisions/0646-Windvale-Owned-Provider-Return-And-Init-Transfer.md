# Decision 0646: Windvale-owned provider return and init transfer

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0645](0645-Windvale-Owned-Provider-User-Context-Transfer.md)
- Contract: [provider-return and init-transfer emission](../../Specifications/Windvale-Os-X64-Process-Provider-Return-Init-Transfer-Emission.md)

## Decision

Emit fixture offsets 13,169 through 13,447 as one fail-closed provider-return
and init-transfer transaction. Validate the returning provider thread and
process, select only the admitted init thread, reactivate its checked page table,
bind GS and continuation state, and load the admitted user context immediately
before `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`4ce7000384b72c28244c707357189c29fb6679519df928c4f24d829e8daff607`.
The focused owner advances to twenty-seven projects and 162 cases with results
50 through 76. Windvale source owns the first 13,448 process-machine bytes and
100 relocation fields.

Client return/transfer, syscall and exception handler bodies, context switching,
and live QEMU application execution remain separate evidence.

## Reconsideration triggers

Another return or scheduling design must retain explicit provider-state checks,
admitted-thread selection, page-table revalidation, per-thread GS ownership,
exact continuation state, and fail-closed user entry.
