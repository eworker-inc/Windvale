# Decision 0699: Freeze the FAT32 block-provider wire protocol

- Status: Implemented architecture-neutral native candidate; endpoint loop pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 block-provider protocol 1](../../Specifications/Windvale-Os-Fat32-Block-Provider-Protocol.md)

## Context

Decision 0697 bounds the read transaction, but a provider cannot receive an
in-process record. The isolated filesystem process needs a fixed byte envelope
that binds each reply to the exact grant, generation, sequence, sector range,
and byte count that produced it.

## Decision

- Freeze exact `WVBR 1` 48-byte requests and `WVBP 1` responses with at most
  4,096 payload bytes.
- Carry a nonzero block-capability reference rather than a native device handle.
- Echo and validate all transaction identity and geometry fields.
- Permit payload only for complete replies and require its exact planned size.
- Map malformed replies to invalid completion without retry.

The 8,726-byte protocol WVB has SHA-256
`5d37a54cc6e6763aca7f1e2c76d128cedae49d5febeef6ffa85d1d4de7e1348e`.
Its composed 22-case owner returns 47 and pins paired Windows/Linux images.

## Consequences

The next filesystem-process increment can send and receive one capacity-one
block operation without inventing a driver-specific message. Endpoint binding,
peer-loss wakeup, live driver execution, and media-change handling remain.

## Reconsideration triggers

Add a new major protocol only when measured batching, asynchronous queueing, or
device topology cannot retain the exact identity and authority properties here.
