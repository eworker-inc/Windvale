# Windvale OS firmware boot probe

## Status and purpose

Firmware boot probe version 2 is the executable Windvale OS evidence slice for UEFI entry, structural table validation, bounded memory-map acquisition, serial observation, and guest-controlled QEMU completion. It is not a kernel or an operating-system qualification.

[Decision 0045](../Documents/Decisions/0045-First-Uefi-Application-And-Boot-Probe.md) records version 1 firmware entry. [Decision 0046](../Documents/Decisions/0046-Bounded-Uefi-Memory-Map-Probe.md) owns the version 2 memory-map boundary. PE32+ construction remains owned by [Windvale-Uefi-Application.md](Windvale-Uefi-Application.md), while emulator and firmware inputs remain governed by [Windvale-Os-Boot-Environment.md](Windvale-Os-Boot-Environment.md).

The ABI and table rules follow [UEFI 2.11 x64 calling conventions](https://uefi.org/specs/UEFI/2.11/02_Overview.html#detailed-calling-conventions), the [EFI System Table](https://uefi.org/specs/UEFI/2.11/04_EFI_System_Table.html), and the [`GetMemoryMap` memory-allocation contract](https://uefi.org/specs/UEFI/2.11/07_Services_Boot_Services.html#efi-boot-services-getmemorymap).

## Artifact construction

The bootstrap builder creates one canonical WVO object in memory, links it at base zero, and passes the verified result to UEFI application writer version 1. No generated WVO, EFI application, FAT view, variable store, firmware image, or captured memory map is committed.

The code is relocation-free and position-independent. A private label builder resolves only local x86-64 branch displacements while constructing the one code section. It does not parse source, select general instructions, produce WVO relocations, or define another assembler contract.

The canonical probe application is 4,096 bytes with SHA-256 `2fd7372854e549040108eea2327c0e1b384625f40914bf50dc711e127953f6cf`.

## Entry and firmware-call frame

On x64 UEFI entry, `RCX` carries the image handle, `RDX` carries the `EFI_SYSTEM_TABLE` pointer, and `RSP` points to the return address. The probe subtracts 136 bytes, which aligns the caller stack to 16 bytes. The frame contains:

- 32 bytes of firmware-call shadow space;
- the fifth `GetMemoryMap` argument at caller offset `0x20`;
- map size, map key, descriptor size, and descriptor version values;
- the allocated map-buffer pointer and capacity;
- the system-table and boot-services pointers; and
- the preserved image handle.

The probe uses only volatile x64 registers and restores the complete frame before returning. Success returns `EFI_SUCCESS` if the QEMU completion device is absent. Failure returns `EFI_DEVICE_ERROR`.

## Structural table boundary

Before calling firmware, the probe requires:

- non-null system table;
- system-table signature `0x5453595320494249`;
- revision at least EFI 1.02, header size at least 120 bytes, and reserved field zero;
- non-null boot-services pointer;
- boot-services signature `0x56524553544f4f42`;
- revision at least EFI 1.02, header size at least 80 bytes, and reserved field zero; and
- non-null `GetMemoryMap`, `AllocatePool`, and `FreePool` function pointers.

Version 2 does not recompute either table CRC and therefore calls this structural validation, not complete table authentication.

## Bounded memory-map sequence

The probe performs exactly this sequence:

1. Zero all output values and call `GetMemoryMap` with size zero and map pointer null.
2. Require `EFI_BUFFER_TOO_SMALL`, a nonzero required size, descriptor version 1, and descriptor size from 40 through 256 bytes.
3. Reject a required map or padded capacity beyond 1 MiB.
4. Add two descriptor widths of slack because allocating the buffer can grow the map.
5. Call `AllocatePool` with `EfiLoaderData` and store the returned non-null buffer.
6. Call `GetMemoryMap` with the full allocated capacity and require `EFI_SUCCESS`.
7. Revalidate descriptor version and size. Require returned bytes to be nonzero, no larger than capacity, and an exact multiple of descriptor size.
8. Walk every descriptor using the returned stride and require:
   - physical and virtual start addresses aligned to 4 KiB;
   - nonzero `NumberOfPages`; and
   - last physical page calculation within the unsigned 64-bit page-address range.
9. Call `FreePool` and require success.

Failures after successful allocation attempt one `FreePool` cleanup before returning failure. The freed allocation changes the memory map, so the returned map key is neither retained nor evidence for a later `ExitBootServices` call.

## Serial and completion evidence

COM1 is initialized at I/O base `0x3F8` for 8-N-1 operation. The transmitter is polled before every byte. Successful execution emits exact ASCII/LF bytes:

```text
windvale-os-boot 2
entry=pass
system-table=pass
memory-map=pass
status=pass
```

A failure after serial initialization emits `status=fail` and writes value 1 to QEMU test port `0xF4`. Success writes zero. QEMU's `isa-debug-exit` therefore returns host code 3 for probe failure and 1 for success. Port `0xF4` remains test transport rather than a Windvale OS device contract. The complete serial marker is required because a QEMU startup error can also return 1.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1
```

The harness first runs the environment preflight. It creates one unique directory beneath the native temporary directory, constructs `EFI/BOOT/BOOTX64.EFI`, copies the accepted variable-template bytes, and launches the fixed QEMU environment with:

- `pc-q35-11.0,accel=tcg`, `qemu64`, one CPU, and 128 MiB;
- immutable firmware code plus a run-private writable variable store;
- no network device, display, or monitor;
- a run-private writable VVFAT directory containing the removable-media path;
- serial output captured to a run-private file; and
- `isa-debug-exit` at port `0xF4`.

The harness verifies the EFI digest before and after launch and also rechecks installed firmware code and variable-template digests. The private variable copy may change and is discarded. Its default timeout is 60 seconds, and only timeout normally forces QEMU termination. Run artifacts are deleted unless `-KeepRunDirectory` is supplied; cleanup first validates the exact absolute temporary path.

## Success report

Successful execution emits this path-free field order:

```text
windvale-os-boot-report 2
status=pass
architecture=x86-64
application-format=pe32-plus-uefi-application-v1
probe-version=2
efi-bytes=4096
efi-sha256=2fd7372854e549040108eea2327c0e1b384625f40914bf50dc711e127953f6cf
serial-marker=windvale-os-boot-2-entry-system-table-memory-map-status-pass
qemu-exit-code=1
```

`-KeepRunDirectory` adds a native diagnostic path after the canonical fields, so that invocation is not portable report evidence.

## Failures

| Code | Meaning |
| --- | --- |
| `WVOS3001` | The .NET host or probe build failed. |
| `WVOS3002` | QEMU could not start. |
| `WVOS3003` | The bounded boot timeout expired. |
| `WVOS3004` | QEMU returned an unexpected exit code, including probe-declared failure. |
| `WVOS3005` | Serial output is missing or lacks the complete marker. |
| `WVOS3006` | The EFI application or an installed firmware input changed during the run. |
| `WVOS3007` | Temporary-directory cleanup failed its absolute-path boundary check. |

## What this does not prove

The probe does not verify table CRCs, retain the map, define memory-type ownership, call `ExitBootServices`, execute after firmware shutdown, configure paging, handle interrupts, discover hardware beyond firmware tables, load a kernel, run Windvale bytecode, or define a stable native ABI. Those enter as later bounded evidence slices.
