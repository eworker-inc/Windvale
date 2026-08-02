# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 4 is cross-host qualified through probe 26 at exact commit `6bb34bb4c6dc23e89fbdcd8592b31f0585f91ec5`. It retains deterministic ownership, copied handoff, the two-page kernel stack, the allocate-only page ABI, and 2 MiB alignment while enlarging the arena to 59 pages for the interpreter's separate RO/NX boot-resource page. It uses the `WVKMEM04` identity; earlier experimental bytes are not accepted under the new bounds.

[Decision 0052](../Documents/Decisions/0052-First-Kernel-Owned-Memory-Foundation.md) owns the qualified version-1 foundation. [Decision 0091](../Documents/Decisions/0091-First-Protected-Windvale-Process.md) owns version 2 and its process-driven expansion. [Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) owns qualified version 3; [Decision 0095](../Documents/Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) owns version 4.

This is still one bounded boot arena, not a general physical-memory manager. It is large enough for copied state, the kernel stack, exception table, kernel page tables, and two protected processes while keeping all allocation deterministic and visibly finite.

## Ownership policy

Only UEFI type 7, `EfiConventionalMemory`, is claimable. Loader code/data, boot-services code/data, runtime-services ranges, ACPI ranges, persistent or unaccepted memory, MMIO, reserved ranges, and unknown future types remain unowned.

The retained memory-map buffer remains loader-owned `EfiLoaderData`. The selected arena must not overlap it or any other descriptor. The linked image and loader stack remain outside the arena under their firmware classifications.

## Validated map boundary

The planner consumes map bytes and descriptor stride from [kernel handoff version 1](Windvale-Kernel-Handoff.md). Before selecting memory it requires:

- nonempty map bytes, at most 1 MiB;
- descriptor stride from 40 through 256 bytes;
- map bytes exactly divisible by the stride;
- 4 KiB-aligned physical and virtual starts;
- nonzero page count for every descriptor; and
- checked last-page address arithmetic.

Unknown memory-type values are structurally valid but never claimable. No arena address is returned until the complete map passes.

## Deterministic arena selection

An eligible descriptor must be `EfiConventionalMemory`, begin at or above 1 MiB, and contain a complete 59-page arena beginning at the first 2 MiB-aligned address inside that descriptor and ending below the exclusive 4 GiB boundary. The planner selects the lowest eligible aligned candidate, independent of descriptor order. A descriptor with 59 pages may still be ineligible when alignment consumes a prefix; checked last-page arithmetic proves the complete candidate range.

The selected 236 KiB range is checked against every other descriptor. Any overlap rejects the complete map rather than silently choosing another range.

## Arena layout

The fixed version-4 layout is:

| Arena pages | Bytes | Owner and purpose |
| --- | ---: | --- |
| `0` | 4,096 | Memory-state header, copied handoff, paging/process records, and descriptor state |
| `1..2` | 8,192 | Down-growing kernel stack |
| `3..58` | 229,376 | Fifty-six initially free allocator pages |

The complete arena is zeroed before state publication. Stack top is `arena + 0x3000`, aligned to 16 bytes. The memory adapter preserves the loader stack, switches to the kernel stack, and restores the loader stack only to return bounded probe evidence.

Probe 26 consumes the free extent in this exact order:

| Pages | Owner |
| --- | --- |
| `3` | Kernel IDT page |
| `4..9` | Six-page kernel paging hierarchy |
| `10..16` | Seven-page init-service extent |
| `17..58` | Forty-two-page interpreter-process extent |

The init extent contains four table pages, one RX page, one stack page, and one data page. The interpreter extent contains four table pages, 32 RX pages, four stack pages, one data page, and one RO/NX runtime-input page as specified by [protected process version 5](Windvale-Protected-Process.md). No allocator pages remain after the fixed boot proof; another process or runtime extent requires a new arena or reclamation decision.

## Memory-state record

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version 4 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM04` |
| `0x08` | 4 | Version | `4` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | Selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `59` |
| `0x20` | 8 | Next free page | Initially `3` |
| `0x28` | 8 | Free pages | Initially `56` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | Zero until the IDT allocation succeeds |

The exact 48-byte `WVKHAND1` record is copied to `arena + 64`. Its map pointer remains valid but borrowed. The kernel paging version-3 record is published at state-page offset `0x80`; process-version-5 records live at offsets `0x100` and `0x300`; private GDT/TSS state begins at `0x200`; and the channel record begins at `0x400`. All remain kernel-only.

After the four required allocations, the live allocator cursor is page `59` with zero pages free. The first-allocation field continues to identify only page 3, the IDT page.

## Allocate-only page ABI

The memory object exports ASCII symbol `Windvale_kernel_allocate_pages`:

- `RCX` points to an exact version-4 memory-state record.
- `EDX` is a nonzero page count no greater than the recorded free pages.
- Success advances the cursor, decreases the free count, zeroes every returned byte, and returns the first page address in `RAX`.
- Failure returns zero and leaves allocator state unchanged.
- Allocation is contiguous, monotonically increasing, and deterministic.
- Version 4 provides no release operation and no allocation outside its arena.

The object also exports `Windvale_kernel_memory_enter`. It validates and copies the handoff, initializes the arena, records the IDT allocation, switches stacks, installs exceptions, installs kernel paging, and reaches the WVB-admission/process chain. Only successful in-guest admission, process-policy token 94, interpreted program result 29, interpreter send/terminal state, init wake/exit 29, and the retained portable native result can reach compiler export `Windvale_kernel_main`. The explicit kernel-fault scenarios still execute after Main and remain terminal.

## Diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS4001` | Invalid map envelope or descriptor stride. |
| `WVOS4002` | Unaligned descriptor address. |
| `WVOS4003` | Zero-page descriptor. |
| `WVOS4004` | Descriptor physical-address overflow. |
| `WVOS4005` | No eligible 2 MiB-aligned 236 KiB conventional-memory arena. |
| `WVOS4006` | Another descriptor overlaps the selected arena. |

Malformed and random bytes must produce a bounded result or one of these failures; they must not escape index, arithmetic, or allocation exceptions.

## Current evidence and limits

Probe 26 requires this normal-path suffix after firmware exit:

```text
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

The user-fault scenario adds `user-fault=contained` after source success. The invalid-opcode and general-protection kernel scenarios retain their exact terminal panic contracts and QEMU host code 3. [Windvale-Os-Boot-Probe.md](Windvale-Os-Boot-Probe.md) records current qualified artifact identities and live evidence; Decision 0052 and qualified Decisions 0088/0090 retain the historical version-1 evidence.

Version 4 does not claim all physical memory, reclamation of loader ranges, page release, runtime allocation policy, general process creation, process teardown, a general virtual-memory manager, general interrupts, multiple CPUs, or graphical output. Probe 26 deliberately exhausts the fixed arena, making allocator growth or reclamation a required explicit decision rather than permitting another silent static extension.
