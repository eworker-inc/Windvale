# Decision 0708: Compose bounded FAT32 file reads

- Status: Implemented architecture-neutral native candidate; live driver pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 file-read transaction 1](../../Specifications/Windvale-Os-Fat32-File-Read-Transaction.md)

## Context

One FAT32 read plan covers at most eight sectors and 4,096 bytes, while a shared
filesystem request may require 65,536 bytes and cross cluster boundaries. A
successful response must contain the exact semantic result, not one internal
block fragment. Directory, chain, block, and response contracts therefore need
one bounded composition that preserves authority and exact progress.

## Decision

- Resolve each file-relative cluster ordinal only after admitting the complete
  bounded chain trace through the existing chain validator.
- Bind the authorized file reference and one media generation before preparing
  any block work.
- Admit one ready capacity-one block exchange, flatten its exact grant and
  identity into transaction-owned state, and internally begin, dispatch, and
  complete each step. Do not accept a separately completed exchange that could
  have been produced from a fork of older state.
- Accumulate exact partial-sector slices until
  `min(requested, remaining-file)` is complete, never beyond 65,536 bytes.
- Emit a successful `WVFP 1` reply only when the existing filesystem-service
  validator accepts the exact correlation, reference, position, length,
  progress, and payload.

The 18 focused cases return 47 and pin paired Windows/Linux images. They include
one unaligned 4,500-byte read completed by 3,996-byte and 504-byte chunks across
two clusters and two dispatched exchanges.

## Consequences

Windvale now has portable proof that an authorized FAT32 read can traverse the
format and block-service boundaries and produce a shared filesystem reply.
Applications still cannot perform this read in the guest until the privileged
endpoint syscall adapter and a real block driver execute the pinned exchange.
Changed media, provider loss, unavailable media, malformed payload, and exchange
identity mismatch remain explicit failures rather than implicit retries.

## Reconsideration triggers

Add caching, scatter/gather, concurrent operations, or a shorter chain witness
only after measurement and a versioned proof that preserves the current media,
sequence, byte, and response bounds.
