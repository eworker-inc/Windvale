# Windvale OS firmware boot probe

## Status and purpose

Firmware boot probe version 8 is the executable Windvale OS development slice for UEFI entry, structural table validation, bounded memory-map acquisition, bounded termination of boot services, a compiler-generated Windvale kernel-entry handoff, one kernel-owned physical arena, a zeroing page allocator, a copied handoff, an owned kernel stack, one ordinary portable-WVB module AOT-compiled through shared ABI 6 and its versioned execution context, post-firmware serial observation, and guest-controlled QEMU completion. It is not a functioning kernel or an operating-system qualification. A pinned development-QEMU run passes; exact committed-candidate qualification is pending.

[Decision 0045](../Documents/Decisions/0045-First-Uefi-Application-And-Boot-Probe.md) records version 1 firmware entry. [Decision 0046](../Documents/Decisions/0046-Bounded-Uefi-Memory-Map-Probe.md) owns version 2 memory-map acquisition. [Decision 0047](../Documents/Decisions/0047-Bounded-Exit-Boot-Services-Transition.md) owns version 3 firmware exit. [Decision 0048](../Documents/Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) owns version 4 kernel handoff. [Decision 0049](../Documents/Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) owns version 5 compiler-generated source. [Decision 0052](../Documents/Decisions/0052-First-Kernel-Owned-Memory-Foundation.md) owns version 6 kernel memory, [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) owns its bidirectional WVA/WV seam, [Decision 0064](../Documents/Decisions/0064-First-Shared-Native-Wvb-In-Windvale-Os.md) owns qualified version 7 shared native WVB adoption, and [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) owns development version 8's ABI-6 context. PE32+ construction remains owned by [Windvale-Uefi-Application.md](Windvale-Uefi-Application.md), the special system subset by [Windvale-X64-Kernel-Target.md](Windvale-X64-Kernel-Target.md), the internal call boundary by [Windvale-Kernel-Handoff.md](Windvale-Kernel-Handoff.md), memory ownership by [Windvale-Kernel-Memory.md](Windvale-Kernel-Memory.md), native implementation roles by [Windvale-Kernel-Native-Seam.md](Windvale-Kernel-Native-Seam.md), the execution context by [Windvale-Native-Execution-Context.md](Windvale-Native-Execution-Context.md), and emulator inputs by [Windvale-Os-Boot-Environment.md](Windvale-Os-Boot-Environment.md).

The ABI and table rules follow [UEFI 2.11 x64 calling conventions](https://uefi.org/specs/UEFI/2.11/02_Overview.html#detailed-calling-conventions), the [EFI System Table](https://uefi.org/specs/UEFI/2.11/04_EFI_System_Table.html), the [`GetMemoryMap` memory-allocation contract](https://uefi.org/specs/UEFI/2.11/07_Services_Boot_Services.html#efi-boot-services-getmemorymap), and the [`ExitBootServices` transition contract](https://uefi.org/specs/UEFI/2.11/07_Services_Boot_Services.html#efi-boot-services-exitbootservices).

## Artifact construction

The bootstrap builder embeds `Operating-System/Kernel/Hello-World.wv`, `Native-Wvb-Probe.wv`, and `X64-Kernel-Shims.wva` as deterministic source inputs. The special kernel source passes through the ordinary frontend/typed WIR and its versioned system target. The portable probe passes through ordinary WVB production, mandatory verification, the shared ABI-6 selector/fragment verifier, and the same WVO sink used for host AOT. It must have an empty native service list. The reference/recovery assembler independently verifies the WVA shims. The builder also creates loader, kernel-memory, exact native-bridge, and x64 byte-adapter WVO objects. The existing linker resolves all calls, tail jumps, and the RIP-relative data relocation, independently reconstructs the base-zero image, and passes verified code/read-only-data bytes to UEFI application writer version 3. No generated WVB, WVO, EFI application, FAT view, variable store, firmware image, or captured memory map is committed.

The linked image is position-independent. A private OS label builder resolves local loader, adapter, and exact bridge branches and exposes only typed external relocation holes. Shared compiler-native instruction selection remains isolated behind ABI 6 and publishes calls/data through verified WVO relocations; the special system target remains separate pending kernel services and broader value coverage.

The canonical special compiler object remains 2,564 bytes with SHA-256 `f2c28eb5f020f59b8acb480fc8dc62e393ebb14405b3c12ecb05076176d44420`. The portable probe WVB is 502 bytes with SHA-256 `1f384f77c4e1c718a331aaa1a3c1f1e4173bbae9d870ec9023d70c7b15c1f7ef`; its 2,360-byte ABI-6 WVO has SHA-256 `712350a395120c42f604966dffe04c397012af3696666f51c1a069cd9db0be61`. The 305-byte native bridge has SHA-256 `2e22ee17e52ee8cc2c8fa6547424f0234bd770555b0a75c23d63003b6257331e`; the 291-byte WVA seam has SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`. The development probe application is 10,240 bytes with SHA-256 `61cd90eac963ed96d1fdd86d447cb7f2cdeffa50a3de2f5306239de27c1be6b0`.

## Entry and firmware-call frame

On x64 UEFI entry, `RCX` carries the image handle, `RDX` carries the `EFI_SYSTEM_TABLE` pointer, and `RSP` points to the return address. The probe subtracts 136 bytes, which aligns the caller stack to 16 bytes. The frame contains:

- 32 bytes of firmware-call shadow space;
- the fifth `GetMemoryMap` argument at caller offset `0x20`;
- map size, map key, descriptor size, and descriptor version values;
- the allocated map-buffer pointer and capacity;
- the system-table and boot-services pointers;
- the preserved image handle;
- a three-attempt exit counter plus a flag that distinguishes pre-exit cleanup from terminal recovery after an attempted exit; and
- after successful exit, a 48-byte handoff record overlaid on locals no longer needed by firmware calls.

The probe uses only volatile x64 registers and restores the complete frame when a failure safely returns before any `ExitBootServices` attempt. A successful exit cannot return to firmware: if the QEMU completion device is absent, the probe disables interrupts and remains in a halt loop. A failure after the first exit attempt also reports and halts because firmware may have partially shut down boot services. Earlier failures return `EFI_DEVICE_ERROR`.

## Structural table boundary

Before calling firmware, the probe requires:

- non-null system table;
- system-table signature `0x5453595320494249`;
- revision at least EFI 1.02, header size at least 120 bytes, and reserved field zero;
- non-null boot-services pointer;
- boot-services signature `0x56524553544f4f42`;
- revision at least EFI 1.02, header size at least 240 bytes, and reserved field zero; and
- non-null `GetMemoryMap`, `AllocatePool`, `FreePool`, and `ExitBootServices` function pointers.

Version 8 does not recompute either table CRC and therefore calls this structural validation, not complete table authentication.

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
9. Retain the allocation and current map for the exit transition.

Failures after successful allocation and before any exit attempt call `FreePool` once before returning failure. The successful path does not free the allocation because its map and key are the handoff evidence needed immediately by `ExitBootServices`.

## Bounded boot-services exit

The probe performs at most three `ExitBootServices` attempts:

1. After validating the current map, set the attempted flag and call `ExitBootServices(ImageHandle, MapKey)` without an intervening firmware call.
2. Treat `EFI_SUCCESS` as an irreversible transition. Do not call any boot service, firmware protocol, or invalidated system-table field afterward.
3. On `EFI_INVALID_PARAMETER`, decrement the remaining-attempt count. If attempts remain, reset the supplied size to the retained allocation capacity, call only `GetMemoryMap` into that same buffer, revalidate the complete returned map, and retry with its new key.
4. Treat any other status, a retry-map failure, invalid retry data, or exhaustion of three attempts as terminal. `FreePool` is attempted because it remains a memory-allocation service permitted after a failed first exit call; the probe then emits failure evidence and halts instead of returning to potentially partially shut-down firmware.

No allocation, release, firmware console operation, or other boot service occurs between a successful final `GetMemoryMap` and its corresponding `ExitBootServices` call. On success the retained map buffer is `EfiLoaderData`, which the later Windvale kernel handoff may consume and reclaim under an explicit memory-ownership contract.

## Kernel entry and owned memory

After successful exit, the loader preserves the retained map values in volatile registers, overlays a 48-byte handoff record on completed firmware-call locals, and calls the separately linked `Windvale_kernel_entry` symbol with the record address in `RCX`. The caller stack remains 16-byte aligned and its original 32-byte shadow space remains available.

The compiler-generated wrapper validates the `WVKHAND1` envelope, then calls the independent memory object. That object revalidates every descriptor, selects the lowest eligible 16-page `EfiConventionalMemory` arena from 1 MiB through 4 GiB, rejects contradictory overlap, clears all 64 KiB, initializes `WVKMEM01`, copies the handoff, and completes one zeroing page allocation. It then switches to the two-page owned stack and calls WVA export `Windvale_kernel_wva_main`. The WVA tail reaches the exact native bridge, which preserves the handoff, constructs the ABI-6 version-1 execution context with budgets 203/2 and a zero service-table pointer, calls portable `Main`, and accepts only packed result 29. Only then does it restore the handoff and tail-transfer to compiler-generated special `Windvale_kernel_main`. Any trap, wrong result, or special-Main failure becomes terminal post-firmware failure. [Windvale-Kernel-Handoff.md](Windvale-Kernel-Handoff.md) defines the incoming record, [Windvale-Kernel-Memory.md](Windvale-Kernel-Memory.md) defines ownership and layout, and [Windvale-Kernel-Native-Seam.md](Windvale-Kernel-Native-Seam.md) defines implementation roles.

## Serial and completion evidence

COM1 is initialized at I/O base `0x3F8` for 8-N-1 operation. The transmitter is polled before every byte. Successful execution emits exact ASCII/LF bytes:

```text
windvale-os-boot 8
entry=pass
system-table=pass
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
Hello from Windvale
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
```

The `memory-map`, `boot-services`, memory, allocator, stack, Hello World, native-context, native-WVB, source-pass, and success lines are all emitted only after `ExitBootServices` returns success. `memory-owned=pass`, `allocator=pass`, `kernel-stack=pass`, and Hello World are selected from typed WIR and emitted through relocatable calls in the special compiler-generated Main after the memory transition and stack switch. Each byte crosses the WVA output shim. `native-context=pass` and `native-wvb=pass` come from the loader only after the ABI-6 module consumes the exact context, returns packed 29, and the special Main returns zero; `windvale-source=pass` then records aggregate source success. A failure after serial initialization emits `status=fail` and writes value 1 to QEMU test port `0xF4`. Success writes zero. QEMU's `isa-debug-exit` therefore returns host code 3 for probe failure and 1 for success. Port `0xF4` remains test transport rather than a Windvale OS device contract. The complete serial marker is required because a QEMU startup error can also return 1.

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
windvale-os-boot-report 8
status=pass
architecture=x86-64
application-format=pe32-plus-uefi-application-v3
probe-version=8
efi-bytes=10240
efi-sha256=61cd90eac963ed96d1fdd86d447cb7f2cdeffa50a3de2f5306239de27c1be6b0
serial-marker=windvale-os-boot-8-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-hello-native-context-native-wvb-windvale-source-status-pass
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

The probe does not verify table CRCs, define ownership beyond one conventional-memory arena, reclaim loader memory, configure paging, install interrupt handling, or discover hardware beyond firmware tables. It does not load, retain, decode, or verify WVB inside the guest: the portable module is AOT-compiled during host image construction. The special system-profile target and temporary COM1 adapter remain. A guest WVB verifier/loader, general memory manager, functioning kernel runtime, interrupt system, clean platform shutdown, Hyper-V evidence, and cross-host boot qualification remain later bounded slices.
