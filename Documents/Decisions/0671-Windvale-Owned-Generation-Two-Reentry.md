# Decision 0671: Windvale-owned generation-2 re-entry

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0670](0670-Windvale-Owned-Generation-Two-Endpoint-Rebind.md)
- Contract: [generation-2 client re-entry emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Reentry-Emission.md)

## Decision

Emit fixture offsets 25,513 through 25,953 as one checked re-entry transaction.
Keep all twelve terminal branches, the dispatcher call, and the resume address
as explicit internal PC-relative fields, and reject any mismatch before the
generation-2 `sysretq` can be claimed.

## Evidence and consequences

The normalized payload SHA-256 is
`13dd9ec88a9f406705bda82054ce4935a66f134e3bd582a93d7a4f6e8c6ce2c8`.
The focused owner advances to forty-five projects and 270 cases with results 50
through 94. Windvale source owns the first 25,954 process-machine bytes and 163
internal or external relocation fields.

The first generation-2 user return is now reproducible from Windvale source.
The return target's handler body, subsequent user behavior, teardown, and live
QEMU evidence remain separate boundaries.

## Reconsideration triggers

Another re-entry design must preserve recycled-memory validation, dispatcher
generation checks, GS binding, resume publication, resource-state accounting,
exact user-register restoration, and fail-closed relocation identity.
