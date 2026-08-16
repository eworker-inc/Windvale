# Decision 0618: First linkable OS provider process images

- Status: Implemented deterministic image candidate; boot embedding pending
- Date: 2026-08-15
- Contract: [provider process images](../../Specifications/Windvale-Os-Provider-Process-Images.md)

## Decision

Build filesystem and network providers as separate native user images instead
of importing their implementations into the nearly full supervisor window.
Each image owns a distinct exported entry and endpoint-wait shim and validates
its portable readiness state before receiving work. Keep both payloads outside
the current seven-section process object until the Windvale-owned constructor
and reviewed architecture fixture explicitly allocate, map, launch, and tear
them down.

## Evidence and consequences

The filesystem image is 195,657 bytes at
`453cef870da3f375400d1c58cc8ebd385f761c2eafbdf3b3fb70603db8520dab`;
the network image is 242,571 bytes at
`57067da10da68fc1d35b41784e147d8f60ed1e05441cb68bc803ad5a9682f6d1`.
Their host readiness executions return 46 and 47. The focused eight-case owner
rebuilds the WVB, WVO, WVA, and linked identities.

This provides the missing executable payloads but does not claim boot launch,
IPC replies, FAT32, a link driver, or packet processing.

## Reconsideration triggers

Change the images only with matching service contracts and deterministic
construction evidence. Do not combine them into one process merely to avoid
evolving the process-object boundary.
