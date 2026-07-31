# Decision 0047: Bounded ExitBootServices transition

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

Firmware probe version 2 validated and released a bounded UEFI memory map. Releasing that buffer deliberately invalidated its map key, so it could not support the irreversible transition from a firmware application into an OS-owned execution environment. The next evidence slice must retain a current map, use its key without an intervening firmware operation, recover from a stale key without unbounded retry, and prove that Windvale code continues after boot services terminate.

UEFI 2.11 requires the current `GetMemoryMap` key at [`ExitBootServices`](https://uefi.org/specs/UEFI/2.11/07_Services_Boot_Services.html#efi-boot-services-exitbootservices). An incorrect key returns `EFI_INVALID_PARAMETER`; firmware may partially shut down during the first attempt, after which an OS loader should call no boot service outside the memory-allocation services. A successful call transfers responsibility for continued platform operation to the OS loader and invalidates boot-service function pointers.

## Decision

- Advance the generated bootstrap to firmware probe version 3 while retaining UEFI application format version 1.
- Require a boot-services header of at least 240 bytes and a non-null `ExitBootServices` entry in addition to the version 2 table checks.
- Allocate and validate the bounded map as before, but retain its `EfiLoaderData` buffer and current key instead of calling `FreePool` on the successful path.
- Permit at most three `ExitBootServices(ImageHandle, MapKey)` attempts. Call it immediately after complete map validation, with no intervening firmware service.
- On `EFI_INVALID_PARAMETER`, reacquire the map into the same retained bounded buffer using only `GetMemoryMap`, revalidate every descriptor, and retry with the new key. Do not allocate a larger buffer during this phase; a size increase beyond the retained capacity is a bounded terminal failure.
- On any other exit status, invalid retry map, or attempt exhaustion, attempt `FreePool` as the only additional memory-allocation operation, emit direct serial failure evidence, and halt. Do not return to firmware that may be partially shut down.
- After `EFI_SUCCESS`, make no firmware or protocol call and do not return from the UEFI entry point. Emit the exact `memory-map=pass`, `boot-services=exited`, and `status=pass` lines through direct COM1 I/O, complete the QEMU test transport, and otherwise disable interrupts and halt.
- Retain the map fields and buffer in the existing bootstrap frame for the next kernel-handoff slice. This frame layout is not yet the stable Windvale kernel ABI.

## Consequences

The QEMU evidence now proves that the accepted PE32+ image acquires a current bounded memory map, terminates boot services successfully, and continues executing direct x86-64 serial code after firmware shutdown. The successful path intentionally retains its map allocation because no boot service remains available to free it and the next handoff needs the map.

This does not yet prove a stable handoff record, memory-type ownership policy, page allocator, paging, interrupts, kernel entry, `.wv` native execution, runtime execution, hardware portability, or shutdown. The retry path is structurally bounded and reviewed but the accepted QEMU run succeeds on its first attempt; stale-key fault injection remains future test infrastructure rather than claimed runtime evidence.

## Reconsider when

- A supported firmware requires more than the retained 1 MiB map capacity during the retry window.
- A deterministic firmware fault harness can exercise stale-key and partial-shutdown behavior directly.
- The kernel handoff needs map storage with stronger alignment, page allocation, or a different lifetime.
- A secondary boot environment cannot observe direct legacy COM1 or the QEMU completion transport.
