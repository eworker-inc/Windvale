# Windvale OS FAT32 block-read transaction 1

## Status and scope

Block-read transaction 1 is the first implemented capability boundary between
the isolated FAT32 service and a future block provider. It plans exact 512-byte
sector reads without exposing an ambient device namespace or placing FAT logic
in the kernel.

[`Fat32-Block-Read-Transaction.wv`](../Operating-System/Services/Fat32-Block-Read-Transaction.wv)
admits a nonempty sector extent inside an independently supplied device size.
The grant carries one generation and an explicit read right. A request must
present that generation, the exact expected nonzero sequence, a relative range
inside the grant, and one through eight sectors. Successful plans contain the
absolute device sector, exact byte count, and next sequence.

Completion rechecks the generation and requires exactly the planned bytes.
Stale generation, denied authority, unavailable provider, provider loss, and
invalid payload are separate results. Failed results carry no payload, and the
contract never silently retries a provider operation.

## Evidence and limits

The policy WVB is 5,036 bytes at SHA-256
`8e6d447b4ee2bcbb6b549d37d42d1093ac7c1aa18ffacaa3f2e09bb4fcc913b5`.
The transaction is composed with the provider protocol and exchange lifecycle
in a 34,028-byte test WVB at SHA-256
`00f91945f789b8b8349ea54089b746f1de3de596c8ff7588a1b57277820a2dc9`.
The 37-case owner lowers deterministic Windows/Linux images and covers exact
planning/completion, malformed device extents, denied rights, stale generation,
sequence mismatch, transfer ceilings, grant escape, provider failures, short
payloads, request/response wire binding, capacity-one dispatch, exact-once
completion, cancellation, peer loss, and teardown.

This transaction and its exchange lifecycle are not a hardware driver, DMA
contract, cache, partition parser, or privileged IPC adapter. Those require
separate kernel/driver and service integration evidence.
