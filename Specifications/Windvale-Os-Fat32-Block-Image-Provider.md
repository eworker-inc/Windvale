# Windvale OS FAT32 immutable block-image provider 1

## Status and scope

Immutable block-image provider 1 is the implemented architecture-neutral
provider for a prebound read-only block image. It is suitable for a boot-mapped
RAM disk or immutable test volume and is the first executable provider behind
the existing `WVBR 1`/`WVBP 1` boundary.

[`Fat32-Block-Image-Provider.wv`](../Operating-System/Services/Fat32-Block-Image-Provider.wv)
owns request admission, checked sector mapping, the exact image slice, and the
canonical successful response. It does not parse FAT32 or receive ambient file
or device authority.

## Binding and request admission

One provider instance binds:

- a nonzero block-capability reference;
- a nonzero current media generation;
- an absolute first sector; and
- a nonempty, 512-byte-aligned immutable image of at most 64 MiB.

Every request must be exactly 48 bytes with `WVBR 1` magic, version, total
length, zero reserved field, a nonzero nonterminal sequence, one through eight
sectors, and an exact `sector_count * 512` byte count. The generation and block
reference must match the binding. The requested absolute sector range must fit
entirely inside the image after subtracting the bound first sector with checked
arithmetic.

Invalid binding, malformed request, stale generation, and outside-image range
are distinct results and carry no response bytes. A successful request copies
only its admitted image interval and emits one `WVBP 1` response echoing the
exact generation, sequence, sector geometry, byte count, and block reference.

## Evidence and limits

The provider module is a 4,639-byte WVB at SHA-256
`60b56a15ad26ff54993e004768439f6a567353debd4a95e05efe60550b89a5bf`.
The expanded 59-case block owner returns 47, pins paired Windows/Linux images,
and completes a capacity-one exchange from a four-sector image while proving
the first and last byte of both requested sectors.

This is a real immutable-memory provider, not a hardware driver. It does not
perform endpoint syscalls, discover partitions, access PCI or VirtIO devices,
use DMA, observe media-change interrupts, cache blocks, or write storage. Those
remain separate privileged and driver contracts.
