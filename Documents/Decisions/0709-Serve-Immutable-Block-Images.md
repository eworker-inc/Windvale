# Decision 0709: Serve immutable block images

- Status: Implemented architecture-neutral native candidate; live endpoint pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [immutable block-image provider 1](../../Specifications/Windvale-Os-Fat32-Block-Image-Provider.md)

## Context

Windvale has a bounded block grant, canonical provider messages, a capacity-one
exchange, and a composed FAT32 file-read transaction. It still lacks any
provider that turns an admitted block request into real bytes. Starting with a
hardware driver would combine transport, device discovery, DMA, interrupt, and
filesystem integration risks in one step.

## Decision

- Implement one read-only provider over a prebound immutable block image of at
  most 64 MiB and an explicit absolute first sector.
- Bind one nonzero block reference and media generation; reject ambient or
  mismatched authority.
- Independently admit every `WVBR 1` field before computing an image offset.
- Use subtraction-first range checks and a checked `u64` sector-to-byte mapping.
- Emit only canonical complete `WVBP 1` replies with the exact admitted payload;
  return no wire bytes for invalid, stale, or outside-image requests.

The focused owner now has 59 cases. It drives the capacity-one exchange from a
four-sector image, checks both sector boundaries, returns 47, and pins exact
Windows and Linux native images.

## Consequences

Windvale now has a usable block backend for a boot-mapped RAM disk or immutable
volume, and the filesystem transaction can receive provider-produced bytes
without trusting a synthetic response builder. The provider remains isolated
from FAT32 format policy.

Applications still cannot issue this read in the guest. The next slice must
transport the exact request and response through the privileged endpoint and
launch the filesystem/provider processes. Hardware storage remains a later
driver with separate PCI/VirtIO, DMA/IOMMU, interrupt, reset, and teardown
evidence.

## Reconsideration triggers

Add larger images, scatter/gather, caching, writable storage, or hardware queues
only with explicit bounds and without weakening generation, reference, range,
payload, or teardown validation.
