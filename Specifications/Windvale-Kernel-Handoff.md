# Windvale x86-64 kernel handoff

## Status and purpose

Kernel handoff version 1 defines the first internal transition from the UEFI loader to a separately linked Windvale kernel-entry object after successful `ExitBootServices`. It is a system-profile bootstrap ABI, not the general Windvale native function ABI. [Decision 0048](../Documents/Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) owns the record and call boundary; [Decision 0049](../Documents/Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) applies it to the first compiler-generated entry.

## Linked symbol boundary

The loader imports the ASCII-safe function symbol `Windvale_kernel_entry`. The special kernel object exports that symbol. Both contain code-only `.text` sections and are linked at base zero through one WVO `relative-i32` relocation with addend `-4` at the x86-64 `call rel32` displacement field. Candidate firmware probe version 20 additionally imports `Windvale_kernel_x64_q35_shutdown` from the WVA object for a separate post-return lifecycle call. The ABI-15 portable native object, two kernel-owned exception destinations, normalized trap frame, paging installation, and shutdown call do not change the handoff record or loader-to-entry call ABI. Probe 17 remains the latest qualified predecessor until Decisions 0085 through 0088 complete qualification.

The successful flat link must report only code and read-only-data sections, at least one code section, zero absolute relocations, and no relocation kind other than `relative-i32` before UEFI application format version 3 accepts it. Firmware may relocate the complete PE image because each resolved relative displacement remains invariant when caller, target, and immutable data move together.

## Call ABI

The loader calls `Windvale_kernel_entry` only after `ExitBootServices` returns `EFI_SUCCESS` and direct serial evidence records that transition.

- `RCX` points to one immutable handoff record.
- `RSP` is 16-byte aligned immediately before `call`, and the caller provides 32 bytes of shadow space.
- The callee may clobber the standard x64 volatile registers and must preserve any nonvolatile register it uses.
- `RAX = 0` reports accepted handoff and kernel-entry success. Any other value reports failure.
- Returning transfers control only to the post-firmware loader continuation; neither side may return to UEFI firmware.

Version 1 passes no second argument, stack argument, system-table pointer, runtime-service pointer, allocator, console protocol, or ambient capability.

## Handoff record

The record is 48 little-endian bytes:

| Offset | Bytes | Field | Version 1 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKHAND1` |
| `0x08` | 4 | Version | `1` |
| `0x0C` | 4 | Record bytes | `48` |
| `0x10` | 8 | Memory-map address | Nonzero address of retained `EfiLoaderData` |
| `0x18` | 8 | Memory-map bytes | Nonzero, at most 1 MiB |
| `0x20` | 8 | Descriptor bytes | 40 through 256 |
| `0x28` | 4 | Descriptor version | `1` |
| `0x2C` | 4 | Reserved | Zero |

Memory-map bytes must be an exact multiple of descriptor bytes. Every descriptor is validated by firmware probe version 20 before the exit call; the compiler-generated kernel wrapper independently revalidates the record envelope and divisibility before accepting it. The memory object then independently revalidates the envelope and every descriptor before making an ownership decision.

## Lifetime and ownership

The original record occupies the loader's live stack frame and is immutable for the duration of the kernel-entry call. Kernel memory version 1 copies all 48 bytes into its owned state page before changing stacks and passes that copy to compiler-generated `Windvale_kernel_main`. The retained memory-map buffer remains borrowed loader data and live after boot services terminate; later reclamation must preserve it until another ownership decision replaces this contract.

The handoff includes no valid boot-services pointer. Code reached through this ABI must not call a boot service, firmware device-handle protocol, or invalidated system-table field.

## Current evidence and limit

Candidate firmware probe version 20 retains the handoff and version-1 memory contracts while linking the compiler-generated special kernel path, shared ABI-15 portable native probe, WVA seams, kernel memory/paging layers, kernel-owned vector-6/vector-13 exception destinations, and WVA Q35 shutdown adapter. The normal image enters the kernel after firmware shutdown and requires this serial suffix:

```text
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

The memory layer calls WVA export `Windvale_kernel_wva_main` only after initializing owned state, completing a zeroing allocation, copying the handoff, switching stacks, installing the bounded exception table, and—in the current Decision 0088 candidate—activating [kernel paging version 1](Windvale-Kernel-Paging.md). WVA tail-transfers first to the native bridge. The bridge constructs the ABI-15/context-7 service-free execution context with exact instruction/depth budgets 271/2 and a zero file-output-table pointer, accepts only packed result 29 after the portable source has decoded its immutable bytes, restores the handoff, and tail-transfers to compiler export `Windvale_kernel_main`. On normal return the loader emits final lifecycle evidence and calls the separate WVA Q35 shutdown adapter; separately selected images prove normalized invalid-opcode `(6, 0)` and general-protection `(13, 0)` terminal frames. The pre-paging probe-20 baseline is cross-host qualified at exact commit `12e9e2e`; the page-table extension has local QEMU evidence but still requires exact cross-host qualification. This does not prove in-guest WVB loading, general physical/virtual-memory management, interrupts, recovery, process isolation, or a functioning kernel runtime.
