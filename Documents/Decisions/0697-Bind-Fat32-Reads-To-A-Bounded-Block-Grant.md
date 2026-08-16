# Decision 0697: Bind FAT32 reads to a bounded block grant

- Status: Implemented architecture-neutral native candidate; live provider pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 block-read transaction 1](../../Specifications/Windvale-Os-Fat32-Block-Read-Transaction.md)

## Context

Volume and chain admission cannot authorize device access. The FAT32 service
needs a separate read-only grant whose extent, generation, request sequence,
and transfer size are checked before a driver or provider is invoked.

## Decision

- Admit a nonempty block extent only inside an independent device-sector count.
- Carry a generation and explicit read right; reject stale or denied requests.
- Use relative sectors inside the grant and subtraction-first range checks.
- Limit one request to eight 512-byte sectors and require exact completion bytes.
- Keep unavailable, provider-loss, stale, and malformed completion distinct and
  never imply an automatic retry.

The 5,036-byte policy WVB has SHA-256
`8e6d447b4ee2bcbb6b549d37d42d1093ac7c1aa18ffacaa3f2e09bb4fcc913b5`.
Its composed 22-case native owner returns 47 and pins paired Windows/Linux
images, including the successor provider wire protocol.

## Consequences

The FAT32 service can now construct bounded sector transactions for boot, FAT,
directory, and data reads. A live block provider, partition binding, media
change handling, and guest IPC composition remain pending.

## Reconsideration triggers

Change the eight-sector ceiling only from measured directory/data-read pressure
and retain an exact upper bound on bytes, work, accounting, and teardown.
