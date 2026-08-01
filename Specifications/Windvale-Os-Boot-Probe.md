# Windvale OS firmware boot probe

## Status and purpose

Firmware boot probe version 20 is an implemented composition candidate. It retains probe 19's WVA-owned clean Q35 shutdown and normalized entries for invalid opcode and general protection, advances the portable AOT path to ABI 15/context 7 with a zero file-output-table pointer, and installs the first kernel-owned x86-64 page-table root with a null guard and W^X policy. Version 17 remains cross-host qualified at exact commit `ba2cf69cd4a97876f5e953b3938d032fc75a8ff7`; version 20 supersedes the local version-19 candidate, and cross-host qualification is pending.

Decisions 0045 through 0076 own the qualified probe progression through version 16 and ABI 14. [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) owns qualified probe 17; candidate [Decision 0085](../Documents/Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) owns probe 18; candidate [Decision 0086](../Documents/Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) owns probe 19; [Decision 0087](../Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) owns the composed probe-20 ABI rebuild; and candidate [Decision 0088](../Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md) owns its page-table root. PE32+ construction remains owned by [Windvale-Uefi-Application.md](Windvale-Uefi-Application.md), the special system subset by [Windvale-X64-Kernel-Target.md](Windvale-X64-Kernel-Target.md), the internal call boundary by [Windvale-Kernel-Handoff.md](Windvale-Kernel-Handoff.md), memory ownership by [Windvale-Kernel-Memory.md](Windvale-Kernel-Memory.md), page-table ownership by [Windvale-Kernel-Paging.md](Windvale-Kernel-Paging.md), CPU exception mechanics by [Windvale-Kernel-Exceptions.md](Windvale-Kernel-Exceptions.md), normalized entry layout by [Windvale-Kernel-Trap-Frame.md](Windvale-Kernel-Trap-Frame.md), Q35 poweroff by [Windvale-Kernel-Shutdown.md](Windvale-Kernel-Shutdown.md), native implementation roles by [Windvale-Kernel-Native-Seam.md](Windvale-Kernel-Native-Seam.md), the execution context by [Windvale-Native-Execution-Context.md](Windvale-Native-Execution-Context.md), and emulator inputs by [Windvale-Os-Boot-Environment.md](Windvale-Os-Boot-Environment.md).

The ABI and table rules follow [UEFI 2.11 x64 calling conventions](https://uefi.org/specs/UEFI/2.11/02_Overview.html#detailed-calling-conventions), the [EFI System Table](https://uefi.org/specs/UEFI/2.11/04_EFI_System_Table.html), the [`GetMemoryMap` memory-allocation contract](https://uefi.org/specs/UEFI/2.11/07_Services_Boot_Services.html#efi-boot-services-getmemorymap), and the [`ExitBootServices` transition contract](https://uefi.org/specs/UEFI/2.11/07_Services_Boot_Services.html#efi-boot-services-exitbootservices).

## Artifact construction

The bootstrap builder embeds `Operating-System/Kernel/Hello-World.wv`, `Native-Wvb-Probe.wv`, and `X64-Kernel-Shims.wva` as deterministic source inputs. The special kernel source passes through the ordinary frontend/typed WIR and its versioned system target. The portable probe passes through ordinary WVB production, mandatory verification, the shared ABI-15 selector/fragment verifier, and the same WVO sink used for host AOT. It must have an empty native service list. The reference/recovery assembler independently verifies the WVA shims, including both normalized trap entries, the two paging operations, and exact Q35 shutdown body. The builder also creates loader, kernel-memory, CPU-exception, page-table-construction, exact native-bridge, and x64 byte-adapter WVO objects. The paging object has four named relative imports and is independently checked against the host table oracle. All three images share those objects and differ only at the explicit scenario injection boundary. The existing linker resolves all calls, tail jumps, and RIP-relative data relocations, independently reconstructs the base-zero image, and passes verified code/read-only-data bytes to UEFI application writer version 3. The linked payload must begin at entry offset zero and fit the fixed 64 KiB executable window. No generated WVB, WVO, EFI application, FAT view, variable store, firmware image, or captured memory map is committed.

The linked image is position-independent. A private OS label builder resolves local loader, adapter, and exact bridge branches and exposes only typed external relocation holes. Shared compiler-native instruction selection remains isolated behind ABI 15 and publishes calls and data through verified WVO relocations; the special system target remains separate pending kernel services and broader value coverage.

The canonical special compiler object is 2,954 bytes with SHA-256 `61df8691c2b1c6eff31a6782cca144669aad32c26294e60fb97b8d5b15ff4de4`; its only source-visible addition is the `paging=owned` line. The portable probe WVB is 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`; its service-free 8,010-byte ABI-15 WVO remains SHA-256 `f3d0d2aec5b7fb81d02e4188fb6ba48b6a21dc91c89bdf7f00daaf7b0a981038`. The 355-byte native bridge has SHA-256 `bfa2b522b2bf22b3681b523c66c2986b1af366ba042adeddd8a106b5a96a5225`. The candidate version-6 WVA seam is 773 bytes with SHA-256 `5c4b0bcfa1c6463ebbe631562deb7714aa510dfbc2418b1544b0df6c8df6bedb`. The paging WVO is 1,244 bytes with SHA-256 `deeebe592b38890c9964cc4d9736b1d617c0d6b20bed494ba533dcb9b1d4f318`. The exception WVO remains 4,667 bytes with SHA-256 `49f15606d2cd41236f87e8a7a7e24a9532683ffe9d5a59795dc8084288b2f84a`.

All probe-20 images are exactly 22,016 bytes. The normal image has SHA-256 `392a2801bd8d8895bd9c34213336a69057c1ae81675269056c60b8c3e974ab01`; invalid opcode has SHA-256 `aa610e6ac00ed43466a87521bb4cebb2934d0885acb960db8913f025ced9cce9`; and general protection has SHA-256 `74632fcde4873f2d46e18b1b77c5cc8b495e83f0f750930e039da27dd67cd0ee`. The exact probe-17 identities remain the cross-host-qualified baseline.

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

The probe uses only volatile x64 registers and restores the complete frame when a failure safely returns before any `ExitBootServices` attempt. A successful exit cannot return to firmware. Probe 20's normal path calls the terminal WVA Q35 poweroff adapter; if Q35 does not power off, that adapter disables interrupts, halts, and retries after an admitted wake. A failure after the first exit attempt also reports through debug-exit and halts because firmware may have partially shut down boot services. Earlier failures return `EFI_DEVICE_ERROR`.

## Structural table boundary

Before calling firmware, the probe requires:

- non-null system table;
- system-table signature `0x5453595320494249`;
- revision at least EFI 1.02, header size at least 120 bytes, and reserved field zero;
- non-null boot-services pointer;
- boot-services signature `0x56524553544f4f42`;
- revision at least EFI 1.02, header size at least 240 bytes, and reserved field zero; and
- non-null `GetMemoryMap`, `AllocatePool`, `FreePool`, and `ExitBootServices` function pointers.

Probe 20 still does not recompute either table CRC and therefore calls this structural validation, not complete table authentication.

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

The compiler-generated wrapper validates the `WVKHAND1` envelope, then calls the independent memory object. That object revalidates every descriptor, selects the lowest eligible 16-page `EfiConventionalMemory` arena from 1 MiB through 4 GiB, rejects contradictory overlap, clears all 64 KiB, initializes `WVKMEM01`, copies the handoff, and completes one zeroing page allocation. It dedicates that page to CPU exceptions and switches to the two-page owned stack. The exception installer disables maskable interrupts, constructs vector-6 and vector-13 gates from live `CS` and the complete linked WVA entry addresses, and publishes them with `LIDT`.

The memory object then calls `Windvale_kernel_x64_paging_install` with the memory-state pointer. The installer validates NX support plus the live stack, retained map, GDT, executable window, and new allocation; asks the existing allocator for six contiguous zeroed pages; builds the low-1-GiB identity hierarchy; calls WVA to enable NX/WP and activate/read back CR3; and publishes `WVKPAG01` only after readback succeeds. Page zero is absent, ordinary leaves are writable/NX, and the fixed 64 KiB payload window is read-only/executable. Only then does the memory object call WVA export `Windvale_kernel_wva_main`.

The WVA tail reaches the exact native bridge, which preserves the handoff, constructs the ABI-15 version-7 execution context with budgets 271/2 and zero service-table, record-arena, text-arena, argument-table/count, output-table, file-input-table, file-output-table, failure-detail, and reserved fields, calls portable `Main`, and accepts only packed result 29 after its immutable-byte checks. Only then does it restore the handoff and tail-transfer to compiler-generated special `Windvale_kernel_main`. Main emits `paging=owned` under the new root. After Main returns, the normal image returns to the loader, which emits final success/shutdown evidence and calls WVA export `Windvale_kernel_x64_q35_shutdown`. The invalid-opcode image executes `UD2`; the general-protection image dereferences a universally noncanonical x86-64 address. Their CPU frames enter separate WVA stubs and one common terminal handler under the same root. Any runtime trap, wrong result, special-Main failure, unexpected exception return, or impossible shutdown return becomes terminal post-firmware failure. [Windvale-Kernel-Paging.md](Windvale-Kernel-Paging.md) owns the new root; the handoff, memory, trap-frame, exception, shutdown, and native-seam documents own their existing boundaries.

## Serial and completion evidence

COM1 is initialized at I/O base `0x3F8` for 8-N-1 operation. The transmitter is polled before every byte. Successful execution emits exact ASCII/LF bytes:

```text
windvale-os-boot 20
entry=pass
system-table=pass
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The `memory-map`, `boot-services`, memory, allocator, stack, paging, Hello World, CPU-exception, native-context, native-WVB, source-pass, success, and shutdown lines are all emitted only after `ExitBootServices` returns success. `memory-owned=pass`, `allocator=pass`, `kernel-stack=pass`, `paging=owned`, and Hello World are selected from typed WIR and emitted through relocatable calls in the special compiler-generated Main after the memory, stack, exception, and paging transitions. Each byte crosses the WVA output shim. The loader emits `cpu-exceptions=armed` only after the IDT installation and both Main paths return normally. It then emits `native-context=pass` and `native-wvb=pass` because the ABI-15 module consumed the exact context, validated its borrowed bytes, and returned packed 29; `windvale-source=pass` records aggregate source success. `shutdown=poweroff` is emitted immediately before the terminal WVA target adapter is called.

The explicit invalid-opcode image shares the prefix through Hello World and then terminates with:

```text
panic=invalid-opcode
vector=6
error-code=0
status=panic
```

The explicit general-protection image shares the same prefix through Hello World and then terminates with:

```text
panic=general-protection
vector=13
error-code=0
status=panic
```

Each handler path writes value 1 to QEMU test port `0xF4`, producing host code 3, and otherwise enters a CLI/HLT loop. Neither fault emits an armed, success, or shutdown marker or returns with `IRETQ`. Ordinary success no longer writes to port `0xF4`; the WVA Q35 adapter writes `0x2000` to port `0x0604` and QEMU exits with code 0. Other failures retain `status=fail` and debug-exit value 1. Port `0xF4` remains test transport rather than a Windvale OS device or shutdown contract. The complete scenario-specific serial marker is required because a host exit code alone is ambiguous.

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

The harness verifies the EFI digest before and after launch and also rechecks installed firmware code and variable-template digests. The private variable copy may change and is discarded. Scenario `normal` is the default; `-Scenario invalid-opcode` and `-Scenario general-protection` build and run the two deliberate fault images. The expected host code and complete serial marker are selected together; the opposite terminal state and other fault marker are rejected. The default timeout is 60 seconds, and only timeout normally forces QEMU termination. Run artifacts are deleted unless `-KeepRunDirectory` is supplied; cleanup first validates the exact absolute temporary path.

## Success report

Successful execution emits this path-free field order:

```text
windvale-os-boot-report 20
status=pass
scenario=normal
architecture=x86-64
application-format=pe32-plus-uefi-application-v3
probe-version=20
efi-bytes=20992
efi-sha256=d4a9e3625779dd3ef2a03fd71ecfe1502c1ad39378da7adbcf7e4b55636eed8c
serial-marker=windvale-os-boot-20-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-hello-cpu-exceptions-armed-native-context-native-wvb-windvale-source-status-pass-shutdown-poweroff
qemu-exit-code=0
```

The invalid-opcode report uses `scenario=invalid-opcode`, SHA-256 `705670b1054589b80e3c918c03e9f751304e3f4b5bda77485f606433db68a757`, and the `panic-invalid-opcode-vector-6-error-code-0-status-panic` suffix. The general-protection report uses `scenario=general-protection`, SHA-256 `df45d8e0f69581e5ed3b46608598e6170413f80c5c1bbba9233e9842cdd7a04d`, and the `panic-general-protection-vector-13-error-code-0-status-panic` suffix. Both use `efi-bytes=20992` and `qemu-exit-code=3`. `-KeepRunDirectory` adds a native diagnostic path after the canonical fields, so that invocation is not portable report evidence.

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

The probe does not verify firmware-table CRCs, define physical ownership beyond one conventional-memory arena, reclaim loader memory, provide a general virtual-memory manager, or discover hardware beyond firmware tables. It does not load, retain, decode, or verify WVB inside the guest: the portable module is AOT-compiled during host image construction. The special system-profile target, page-table constructor, and temporary COM1 adapter remain Stage 0 seams.

The CPU exception boundary admits only terminal invalid opcode and general protection on the current valid ring-0 stack. It does not cover page faults or `CR2`, double faults, TSS/IST stacks, privilege transitions, saved registers, nested faults, NMI, IRQ, PIC/APIC, interrupt enablement, `IRETQ`, recovery, processes, SMP, or mapping Windvale `WVR` traps to CPU faults. The shutdown candidate is fixed to the pinned Q35 PM control port and performs no process/service coordination or ACPI discovery. A guest WVB verifier/loader, general virtual-memory manager, functioning kernel runtime, general interrupt system, Hyper-V shutdown/evidence, physical-hardware shutdown, and a second-host firmware boot remain later bounded slices.
