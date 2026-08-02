# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 1 defines the first bounded ownership transition after successful `ExitBootServices`. The host planner, simulated allocator, matching x86-64 memory object, and QEMU qualification are implemented. [Decision 0052](../Documents/Decisions/0052-First-Kernel-Owned-Memory-Foundation.md) owns this boundary.

This contract deliberately establishes one small arena rather than claiming all reclaimable firmware memory. It supplies enough owned memory for copied handoff state, a kernel stack, and an allocate-only page allocator while leaving general physical-memory management and reclamation for later evidence. Candidate firmware probe 21 retains the first allocation as the CPU exception table, assigns the next six pages to [kernel paging version 1](Windvale-Kernel-Paging.md), and does not change this version-1 arena layout or allocation ABI.

## Ownership policy

Version 1 considers only UEFI type 7, `EfiConventionalMemory`, claimable. Loader code/data, boot-services code/data, runtime-services ranges, ACPI ranges, persistent or unaccepted memory, MMIO, reserved ranges, and unknown future types remain unowned regardless of whether a later kernel might reclaim some of them safely.

The retained memory-map buffer remains loader-owned `EfiLoaderData`. The selected kernel arena must not overlap it because version 1 never claims loader data. The linked image and loader stack likewise remain outside the arena under their firmware memory classifications.

## Validated map boundary

The planner receives the map bytes and descriptor stride from [kernel handoff version 1](Windvale-Kernel-Handoff.md). Before selecting memory it requires:

- nonempty map bytes, at most 1 MiB;
- descriptor stride from 40 through 256 bytes;
- map bytes exactly divisible by the stride;
- 4 KiB-aligned physical and virtual starts for every descriptor;
- nonzero page count for every descriptor; and
- checked last-page address arithmetic for every descriptor.

Unknown memory-type values are structurally valid but never claimable. The planner returns no address until the complete map passes this boundary.

## Deterministic arena selection

An eligible descriptor must be `EfiConventionalMemory`, begin at or above 1 MiB, contain at least 16 pages, and hold the complete 16-page arena below the exclusive 4 GiB boundary. The planner selects the eligible descriptor with the numerically lowest physical start, independent of descriptor order.

The selected 64 KiB range is checked against every other descriptor. Any overlap rejects the complete map rather than silently choosing another range. This makes contradictory ownership evidence a terminal error.

## Arena layout

The 16-page arena has this fixed version 1 layout:

| Arena pages | Bytes | Owner and purpose |
| --- | ---: | --- |
| `0` | 4,096 | Kernel memory-state header and copied handoff record |
| `1..2` | 8,192 | Down-growing kernel stack |
| `3..15` | 53,248 | Thirteen initially free allocator pages |

The complete arena is zeroed before state publication. The stack top is `arena + 0x3000`, aligned to 16 bytes. The memory adapter preserves the loader stack, calls compiler-generated Windvale code on the kernel stack, and restores the loader stack only to return bounded probe evidence.

## Memory-state record

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version 1 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM01` |
| `0x08` | 4 | Version | `1` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | Selected 4 KiB-aligned base |
| `0x18` | 8 | Arena pages | `16` |
| `0x20` | 8 | Next free page | Initially `3` |
| `0x28` | 8 | Free pages | Initially `13` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | Zero until the probe allocation succeeds |

The exact 48-byte `WVKHAND1` record is copied to `arena + 64` before the stack switch. Its map pointer remains valid but borrowed; the allocator must not publish or overwrite the retained map buffer. In probe 21, the first allocation is page 3 at `arena + 0x3000`; [kernel CPU exceptions version 2](Windvale-Kernel-Exceptions.md) owns that complete page as IDT storage. Paging then receives pages 4 through 9 as one contiguous allocation and publishes its separate 64-byte `WVKPAG01` record at state-page offset `0x80`. Pages 10 through 15 remain free under the unchanged allocator state.

## Allocate-only page ABI

The memory object exports ASCII symbol `Windvale_kernel_allocate_pages`:

- `RCX` points to a valid version 1 memory-state record.
- `EDX` is a nonzero page count no greater than the recorded free pages.
- Success advances the cursor, decreases the free count, zeroes every returned byte, and returns the first page address in `RAX`.
- Failure returns zero and leaves allocator state unchanged.
- Allocation is contiguous, monotonically increasing, and deterministic.
- Version 1 provides no release operation and no allocation outside its one arena.

The same object exports `Windvale_kernel_memory_enter`. It accepts the loader handoff pointer in `RCX`, initializes the arena, performs and records the IDT allocation, and switches stacks. Probe 21 passes that page to the exception installer, then passes the memory-state pointer to `Windvale_kernel_x64_paging_install`. The paging installer requests its own six-page allocation before calling WVA export `Windvale_kernel_wva_main` with the copied handoff pointer. Under the current [kernel native seam](Windvale-Kernel-Native-Seam.md), that exact shim tail-transfers to the ABI-16 WVB-admission bridge. Only verifier token 73, admitted-program result 29, and the retained portable-probe result 29 restore the handoff and reach compiler export `Windvale_kernel_main`. Main owns the source-selected success markers and can be reached only after every preceding memory, exception, paging, admission, and native-probe operation succeeds. The two explicit fault scenarios execute after Main and therefore after CR3 activation, but before control reaches the loader's final success and shutdown path.

## Diagnostics and limits

The independent host planner reports:

| Code | Meaning |
| --- | --- |
| `WVOS4001` | Invalid map envelope or descriptor stride. |
| `WVOS4002` | Unaligned descriptor address. |
| `WVOS4003` | Zero-page descriptor. |
| `WVOS4004` | Descriptor physical-address overflow. |
| `WVOS4005` | No eligible conventional-memory arena. |
| `WVOS4006` | Another descriptor overlaps the selected arena. |

Malformed and random bytes must produce a bounded result or one of these failures; they must not escape an index, arithmetic, or allocation exception.

## Current evidence and limit

Candidate firmware probe version 21 retains the version-1 arena, allocator, copied-handoff, and stack rules while running the Windvale-owned admission profile, admitted AOT module, retained ABI-16 portable native probe, and compiler export `Windvale_kernel_main` under a kernel-owned page-table root. The normal pinned-QEMU gate requires this memory/native suffix:

```text
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The separately selected invalid-opcode and general-protection images reach their exact normalized terminal panic contracts and QEMU host code 3 without emitting later success/shutdown evidence. [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) records both exact historical probe-17 artifact identities. [Decision 0086](../Documents/Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) records the normalized contract; [Decision 0087](../Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) records its qualified pre-paging probe-20 composition; candidate [Decision 0088](../Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md) records local paging evidence; and candidate [Decision 0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md) records the composed probe-21 evidence pending exact cross-host qualification.

Version 1 does not claim all physical memory, reclamation of the retained map or loader ranges, page release, general interrupts, multiple CPUs, processes, runtime allocation policy, or graphical output. Paging version 1 now owns one fixed low-1-GiB identity root, null guard, and W^X policy, but it does not add a general virtual-memory manager. The exception allocation still does not add page-fault, double-fault, interrupt-controller, recovery, or general lifecycle policy; the separate target adapter adds only narrow Q35 poweroff.
