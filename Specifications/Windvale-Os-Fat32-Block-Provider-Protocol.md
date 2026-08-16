# Windvale OS FAT32 block-provider protocol 1

## Status and scope

Block-provider protocol 1 is the implemented bounded wire contract between the
isolated FAT32 service and one separately bound block provider. The filesystem
service creates requests only from an admitted block-read plan; the kernel
transports bytes and capability references without interpreting FAT geometry.

## Request and response

`WVBR 1` is exactly 48 little-endian bytes. It carries exact total length,
capability generation, operation sequence, sector count, absolute device
sector, byte count, one zero reserved field, and a nonzero block-capability
reference. No path, native handle, partition name, or ambient device identity
appears on the wire.

`WVBP 1` contains a 48-byte header plus at most 4,096 payload bytes. It must
echo generation, sequence, sector geometry, byte count, and block reference.
Provider status is complete, unavailable, or provider lost. Only complete may
carry payload, and it must carry exactly the planned byte count; failure replies
must carry none.

[`Fat32-Block-Provider-Protocol.wv`](../Operating-System/Services/Fat32-Block-Provider-Protocol.wv)
constructs the request, validates the response before field use, copies the
exact successful payload, and maps the provider result through block-read
completion. A malformed reply becomes invalid payload and is never retried.

## Evidence and limits

The protocol WVB is 8,726 bytes at SHA-256
`5d37a54cc6e6763aca7f1e2c76d128cedae49d5febeef6ffa85d1d4de7e1348e`.
The composed transaction/protocol owner has 22 cases, returns 47 on Windows,
and pins deterministic Windows/Linux images.

This contract does not yet prove an endpoint send/receive loop, a hardware
driver, DMA, media-change notification, partition discovery, or directory data
interpretation.
