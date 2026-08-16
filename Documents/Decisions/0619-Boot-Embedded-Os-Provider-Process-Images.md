# Decision 0619: Boot-embedded OS provider process images

- Status: Implemented current-host boot candidate; Linux execution pending
- Date: 2026-08-15
- Advances: [Decision 0618](0618-First-Linkable-Os-Provider-Process-Images.md)
- Contracts: [OS process-object build](../../Specifications/Windvale-Os-Process-Object.md), [provider process images](../../Specifications/Windvale-Os-Provider-Process-Images.md), and [OS Probe 40](../../Specifications/Windvale-Os-Boot-Probe.md)

## Context

Decision 0581 produced deterministic filesystem and network user images but
left them outside the boot object. The Windvale-owned process-object constructor
already rebuilt every other embedded process payload and was the smallest
honest boundary at which to carry the new images without consuming the fixed
supervisor RX window.

Adding local data symbols changes every later WVO symbol ordinal. The process
architecture fixture's 55 reviewed relocations therefore have to retain their
data targets while shifting each imported-function target by two. Structural
WVO validity alone is insufficient evidence for that invariant; the integrated
boot must execute the relocated process path.

## Decision

- Extend the process object from eight to ten sections and from 25 to 27
  symbols while retaining 55 relocations.
- Rebuild both provider images from their canonical WVB, lowered WVO, and WVA
  shim inputs inside each ordinary process-object construction.
- Store the images in separate read-only `.rodata.hfilesystem` and
  `.rodata.inetwork` sections with named local symbols.
- Preserve canonical local/export/import symbol ordering and shift only the
  architecture fixture's imported-function relocation ordinals.
- Reconstruct and bind the paired hosted process-object tools from the updated
  Windvale source. Independently verify the final WVO before publication.
- Treat embedding as availability of immutable launch input, not as process
  creation, mapping, endpoint binding, request service, or supervision.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-object tool WVB | 15,148 | `6cd4e39796cc477b895b356863914c614e1dd1b504a918ea3c2e3325883fe02a` |
| Windows process-object tool | 190,464 | `9ddd81e3d2bf885c045a01921b3e48875050ab912d599f57462a9c3a9b239af2` |
| Linux process-object tool | 192,512 | `518e08f39231835655e40cc411e6ff2e70d973ba669db61e9032ad8206b17d05` |
| Filesystem user image | 195,657 | `453cef870da3f375400d1c58cc8ebd385f761c2eafbdf3b3fb70603db8520dab` |
| Network user image | 242,571 | `57067da10da68fc1d35b41784e147d8f60ed1e05441cb68bc803ad5a9682f6d1` |
| Final process WVO | 951,394 | `884152027e10221591f1fc79bbffd8875c14d507e5652719ede4d67dea22624e` |

The current native EFI identities are 1,691,136 bytes each: normal
`5c2625210ce9bae91def596c01881e8bad35ce9d6a0e5532bfa860ebc8533bcb`,
invalid-opcode
`a0c361386e8ce0aa1d8d73b2ca85f26768f2335992e993a869136db00d0daca0`,
and general-protection
`7a446760851890f26becb2c00e7e76f016e95f02d30b5a4ecef78d3b692e1afd`.
The normal image completes the pinned QEMU/OVMF serial contract and exits zero.
Linux-host execution and the grouped cross-host gate remain pending.

Probe 40 still launches only its established init, client, and directory
processes. [Decision 0620](0620-First-Checked-Os-Provider-Launch-Transaction.md)
now admits the exact provider domain, image/page, endpoint, publication,
rollback, and teardown transaction. The next privileged slice must make the
machine allocate those memory objects, map the embedded images RX, create and
bind their endpoints, and enter the providers. FAT32,
Windows/Linux filesystem compatibility, link devices, IP, UDP, and TCP remain
separate later providers and protocols.

## Reconsideration triggers

Move provider payloads out of the EFI image only when a versioned package,
boot-volume, or verified loader contract owns acquisition with equal identity
and failure evidence. Do not execute an embedded image before its complete
resource-domain, mapping, capability, endpoint, and teardown transaction is
admitted.
