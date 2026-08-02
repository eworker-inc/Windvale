# Windvale x86-64 kernel handoff

## Status and purpose

Kernel handoff version 1 defines the first internal transition from the UEFI loader to a separately linked Windvale kernel-entry object after successful `ExitBootServices`. It is a system-profile bootstrap ABI, not the general Windvale native function ABI. [Decision 0048](../Documents/Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) owns the record and call boundary; [Decision 0049](../Documents/Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) applies it to the first compiler-generated entry.

## Linked symbol boundary

The loader imports the ASCII-safe function symbol `Windvale_kernel_entry`. The special kernel object exports that symbol. Qualified firmware probe 21 and probes through 24 additionally import `Windvale_kernel_x64_q35_shutdown` from WVA for the post-return lifecycle call. Both contain code-only `.text` sections and are linked at base zero through one WVO `relative-i32` relocation with addend `-4` at the x86-64 `call rel32` displacement field. ABI-16 portable native objects, admission/process bridges, kernel exceptions, normalized frames, paging/process installation, interpretation, and shutdown do not change the handoff record or loader-to-entry call ABI. Exact commit `860c69c` qualifies the probe-21 composition; Decisions 0091 through 0093 retain it unchanged.

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

Memory-map bytes must be an exact multiple of descriptor bytes. Every descriptor is validated by current firmware probe version 24 before the exit call; the compiler-generated kernel wrapper independently revalidates the record envelope and divisibility before accepting it. The memory object then independently revalidates the envelope and every descriptor before making an ownership decision.

## Lifetime and ownership

The original record occupies the loader's live stack frame and is immutable for the duration of the kernel-entry call. Kernel memory version 2 copies all 48 bytes into its owned state page before changing stacks and passes that identity through the admission, process, retained-native, and compiler-generated paths. The retained memory-map buffer remains borrowed loader data and live after boot services terminate; later reclamation must preserve it until another ownership decision replaces this contract.

The handoff includes no valid boot-services pointer. Code reached through this ABI must not call a boot service, firmware device-handle protocol, or invalidated system-table field.

## Current evidence and limit

Candidate firmware probe version 24 retains handoff version 1 while linking the compiler-generated special kernel path, Windvale-owned admission, process policy, init service, user-space interpreter, shared ABI-16 portable native probes, WVA seams, kernel memory/paging version 2, two protected roots, kernel exception destinations, and the WVA Q35 shutdown adapter. The normal image enters the kernel after firmware shutdown and requires this serial suffix:

```text
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
processes=isolated
wvb-runtime=interpreted
init-service=pass
ipc=cross-process
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The memory layer calls WVA export `Windvale_kernel_wva_main` only after owned-state initialization, handoff copy, stack switch, exception installation, and [kernel paging version 2](Windvale-Kernel-Paging.md) activation. WVA tail-transfers to admission bridge version 2. Its exact 8,944/2 context accepts only Windvale admission token 73, then calls [protected process version 3](Windvale-Protected-Process.md). Only exact user-space interpretation, interpreter-process completion, and Windvale init-service completion reach the retained 271/2 portable-probe bridge; only that bridge's packed result 29 reaches compiler export `Windvale_kernel_main`. Normal return powers off through WVA. Separate images prove terminal CPL0 `(6, 0)` and `(13, 0)` plus contained interpreter CPL3 `(13, 0)`. Probe 24 does not prove general WVB loading, general physical/virtual-memory management, JIT publication, scheduling, interrupts, recovery, or a general service runtime.
