# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 2 is retained by the current probe-23 candidate. It retains version 1's deterministic ownership, copied handoff, two-page kernel stack, and allocate-only page ABI while enlarging the single arena from 16 to 32 pages so two protected processes can own distinct page-table roots and user pages. It uses the `WVKMEM02` identity; version-1 bytes are not accepted under the larger bounds.

[Decision 0052](../Documents/Decisions/0052-First-Kernel-Owned-Memory-Foundation.md) owns the qualified version-1 foundation. [Decision 0091](../Documents/Decisions/0091-First-Protected-Windvale-Process.md) owns version 2 and its process-driven expansion. Probe 21 remains the latest cross-host-qualified version-1 composition; version 2 has focused Windows and pinned-QEMU evidence with cross-host qualification pending.

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

An eligible descriptor must be `EfiConventionalMemory`, begin at or above 1 MiB, contain at least 32 pages, and hold the complete 32-page arena below the exclusive 4 GiB boundary. The planner selects the eligible descriptor with the lowest physical start, independent of descriptor order.

The selected 128 KiB range is checked against every other descriptor. Any overlap rejects the complete map rather than silently choosing another range.

## Arena layout

The fixed version-2 layout is:

| Arena pages | Bytes | Owner and purpose |
| --- | ---: | --- |
| `0` | 4,096 | Memory-state header, copied handoff, paging/process records, and descriptor state |
| `1..2` | 8,192 | Down-growing kernel stack |
| `3..31` | 118,784 | Twenty-nine initially free allocator pages |

The complete arena is zeroed before state publication. Stack top is `arena + 0x3000`, aligned to 16 bytes. The memory adapter preserves the loader stack, switches to the kernel stack, and restores the loader stack only to return bounded probe evidence.

Probe 23 consumes the free extent in this exact order:

| Pages | Owner |
| --- | --- |
| `3` | Kernel IDT page |
| `4..9` | Six-page kernel paging hierarchy |
| `10..16` | Seven-page init-service extent |
| `17..23` | Seven-page admitted-client extent |
| `24..31` | Eight pages retained free |

Each process extent contains four table pages followed by user code, stack, and data as specified by [protected process version 2](Windvale-Protected-Process.md).

## Memory-state record

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version 2 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM02` |
| `0x08` | 4 | Version | `2` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | Selected 4 KiB-aligned base |
| `0x18` | 8 | Arena pages | `32` |
| `0x20` | 8 | Next free page | Initially `3` |
| `0x28` | 8 | Free pages | Initially `29` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | Zero until the IDT allocation succeeds |

The exact 48-byte `WVKHAND1` record is copied to `arena + 64`. Its map pointer remains valid but borrowed. The kernel paging version-2 record is published at state-page offset `0x80`, and the protected-process version-1 record at offset `0x100`. Private process descriptor state begins at offset `0x200`; all of it remains kernel-only.

After the three required allocations, the live allocator cursor is page `17` with `15` pages free. The first-allocation field continues to identify only page 3, the IDT page.

## Allocate-only page ABI

The memory object exports ASCII symbol `Windvale_kernel_allocate_pages`:

- `RCX` points to an exact version-2 memory-state record.
- `EDX` is a nonzero page count no greater than the recorded free pages.
- Success advances the cursor, decreases the free count, zeroes every returned byte, and returns the first page address in `RAX`.
- Failure returns zero and leaves allocator state unchanged.
- Allocation is contiguous, monotonically increasing, and deterministic.
- Version 2 provides no release operation and no allocation outside its arena.

The object also exports `Windvale_kernel_memory_enter`. It validates and copies the handoff, initializes the arena, records the IDT allocation, switches stacks, installs exceptions, installs kernel paging, and reaches the WVB-admission/process chain. Only successful in-guest admission, process-policy token 92, client send/terminal state, init wake/exit 29, and the retained portable native result can reach compiler export `Windvale_kernel_main`. The explicit kernel-fault scenarios still execute after Main and remain terminal.

## Diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS4001` | Invalid map envelope or descriptor stride. |
| `WVOS4002` | Unaligned descriptor address. |
| `WVOS4003` | Zero-page descriptor. |
| `WVOS4004` | Descriptor physical-address overflow. |
| `WVOS4005` | No eligible 128 KiB conventional-memory arena. |
| `WVOS4006` | Another descriptor overlaps the selected arena. |

Malformed and random bytes must produce a bounded result or one of these failures; they must not escape index, arithmetic, or allocation exceptions.

## Current evidence and limits

Probe 23 requires this normal-path suffix after firmware exit:

```text
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
processes=isolated
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

The user-fault scenario adds `user-fault=contained` after source success. The invalid-opcode and general-protection kernel scenarios retain their exact terminal panic contracts and QEMU host code 3. [Windvale-Os-Boot-Probe.md](Windvale-Os-Boot-Probe.md) records current candidate artifact identities and live evidence; Decision 0052 and qualified Decisions 0088/0090 retain the historical version-1 evidence.

Version 2 does not claim all physical memory, reclamation of loader ranges, page release, runtime allocation policy, general process creation, process teardown, a general virtual-memory manager, general interrupts, multiple CPUs, or graphical output. The added pages are a measured bound for two fixed protected processes, not a promise to keep extending one static arena.
