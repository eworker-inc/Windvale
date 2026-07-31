# Decision 0046: Bounded UEFI memory-map probe

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

Firmware probe version 1 proved PE32+ loading and raw x86-64 entry but deliberately ignored the UEFI arguments. The next useful boundary is not a kernel-shaped placeholder: it is evidence that Windvale can obey the x64 firmware ABI, validate the system and boot-services tables needed by the loader, allocate firmware-owned memory, retrieve and inspect the current memory map, and release the allocation.

`GetMemoryMap` is also the prerequisite for `ExitBootServices`, but combining them would hide two distinct failure classes. Pool allocation and release change the memory map and invalidate its key. A later exit transition needs a current map and an explicit retry rule, with no intervening operations that can change it.

## Decision

- Advance the generated bootstrap to firmware probe version 2 while retaining UEFI application format version 1.
- Enter through the specified x64 UEFI convention, preserve the image handle and system-table pointer in a 136-byte local frame, keep the caller stack 16-byte aligned at every firmware call, reserve the required 32-byte shadow area, and place the fifth `GetMemoryMap` argument in its ABI stack slot.
- Structurally validate the system-table and boot-services signatures, minimum EFI 1.02 revision, minimum header sizes, zero reserved fields, non-null boot-services pointer, and non-null `GetMemoryMap`, `AllocatePool`, and `FreePool` entries. Header CRC calculation remains a separate validation enhancement and is not claimed by this slice.
- First call `GetMemoryMap` with a zero size and null map and require `EFI_BUFFER_TOO_SMALL`. Require descriptor version 1 and descriptor size from 40 through 256 bytes.
- Bound the map to 1 MiB. Allocate `EfiLoaderData` for the reported size plus two descriptor widths, then call `GetMemoryMap` again with that complete capacity.
- Require a non-empty returned size no larger than the allocation and exactly divisible by the returned descriptor size. Walk every descriptor using the firmware-reported stride. Require 4 KiB-aligned physical and virtual starts, a nonzero page count, and a final physical page calculation that cannot overflow the 64-bit page-address range.
- Free the pool and require `FreePool` success before publishing the memory-map success marker. If a failure occurs after allocation, attempt one cleanup call before reporting failure.
- Do not retain or publish the returned map or key. `FreePool` changes the map, so this key is explicitly unsuitable for `ExitBootServices`.
- Keep the raw machine-code builder private to the bootstrap. Its label fixups and firmware-call encodings are temporary Stage 0 system evidence, not a second assembler or a Windvale native ABI.

## Consequences

The canonical probe is now 4,096 bytes with SHA-256 `2fd7372854e549040108eea2327c0e1b384625f40914bf50dc711e127953f6cf`. On the accepted QEMU/EDK II environment it emits entry, system-table, memory-map, and final success phases and exits through the existing QEMU test transport.

This proves that Windvale bootstrap code receives and uses the firmware system table, makes ABI-correct boot-service calls, performs the specified memory-map sizing pattern, uses only allocated map storage, validates every returned descriptor at the exercised boundary, and releases its resource.

It does not prove table CRC validation, persistent map ownership, map serialization, memory-type policy, page allocation, `ExitBootServices`, post-firmware execution, a kernel, or `.wv` execution. The exact QEMU map contents are deliberately not a golden artifact because firmware state may change descriptor addresses and keys between runs even when the contract remains valid.

## Reconsider when

- A firmware target returns a descriptor larger than the bounded 256-byte exercised limit.
- A valid environment needs more than 1 MiB for its pre-exit memory map.
- The `ExitBootServices` slice defines allocation ownership and the bounded stale-key retry path.
- A reusable native backend can replace private bootstrap encodings without weakening exact-byte evidence.
