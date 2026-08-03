# Windvale x86-64 kernel handoff

## Status and purpose

Kernel handoff version 1 defines the first internal transition from the UEFI loader to a separately linked Windvale kernel-entry object after successful `ExitBootServices`. It is a system-profile bootstrap ABI, not the general Windvale native function ABI. [Decision 0048](../Documents/Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) owns the record and call boundary; [Decision 0049](../Documents/Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) applies it to the first compiler-generated entry.

## Linked symbol boundary

The loader imports the ASCII-safe function symbol `Windvale_kernel_entry`. The special kernel object exports that symbol. Probes 21 onward additionally import `Windvale_kernel_x64_q35_shutdown` from WVA for the post-return lifecycle call. Both contain code-only `.text` sections and are linked at base zero through one WVO `relative-i32` relocation with addend `-4` at the x86-64 `call rel32` displacement field. Later portable native objects, admission/process bridges, exceptions, paging/process installation, resource and directory services, interpretation, endpoints, dispatch, and shutdown do not change the handoff record or loader-to-entry call ABI. Cross-host-qualified Probe 38 retains handoff version 1.

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

Memory-map bytes must be an exact multiple of descriptor bytes. Every descriptor is validated by the current firmware probe before the exit call; the compiler-generated kernel wrapper independently revalidates the record envelope and divisibility before accepting it. The memory object then independently revalidates the envelope and every descriptor before making an ownership decision.

## Lifetime and ownership

The original record occupies the loader's live stack frame and is immutable for the duration of the kernel-entry call. Kernel memory version 16 copies all 48 bytes into its owned state page before changing to the measured four-page stack and passes that identity through the admission, process, retained-native, and compiler-generated paths. The retained memory-map buffer remains borrowed loader data and live after boot services terminate; later reclamation must preserve it until another ownership decision replaces this contract.

The handoff includes no valid boot-services pointer. Code reached through this ABI must not call a boot service, firmware device-handle protocol, or invalidated system-table field.

## Current evidence and limit

Implemented-candidate firmware Probe 39 under Decision 0188 retains handoff version 1 while linking the compiler-generated special kernel path, Windvale-owned admission/resource/preemption policy, three protected process roots, two service endpoints and channels, the bounded ready/wait dispatcher, budgeted user-space interpreter, ABI-22 portable native probes, expanded WVA seams, memory 16/paging 5, HPET/local-APIC timer evidence, kernel exception destinations, and the WVA Q35 shutdown adapter. The normal image enters the kernel after firmware shutdown and requires this serial suffix:

```text
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
processes=isolated
directory-process=isolated
dispatcher=ready-wait
service-endpoints=2
resource-grant=pass
typed-resources=pass
resource-revoked=pass
process-reuse=pass
wvb-runtime=interpreted
init-service=pass
directory-service=pass
ipc=resource-and-directory
Hello from Windvale
timer-preemption=pass
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The memory layer calls WVA export `Windvale_kernel_wva_main` only after owned-state initialization, handoff copy, stack switch, exception installation, and [paging version 5](Windvale-Kernel-Paging.md) activation. Admission bridge 2 retains its exact `39,712/2` context and token 73, then calls [protected process version 17](Windvale-Protected-Process.md). The private four-interrupt preemption experiment completes before the retained service workload; only the exact two-resource grant, both endpoint-backed service exchanges, budgeted interpretation, two-alias terminal cleanup, generation reuse, and independent service completion reach the retained 271/2 portable bridge; only its packed result 29 reaches `Windvale_kernel_main`. Separate images retain terminal CPL0 `(6, 0)` and `(13, 0)`, contained client CPL3 `(13, 0)`, and contained directory-provider failure. Probe 38 remains cross-host qualified at implementation commit `aae6818e3226e9e7e88d205b4666fb9904e4735b`; Probe 39 qualification is pending. This does not prove general loading, general memory management, capability transfer, JIT publication, general timer/scheduler behavior, service restart, or a general service runtime.
