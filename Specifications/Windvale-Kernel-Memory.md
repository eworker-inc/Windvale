# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 13 is the current Probe-34 implemented candidate. It composes version 12's second init RX page and immutable `WVRS 1` mapping with ABI 22's additional client code page while retaining the compact client stack, checked tail release, and same-root reuse. [Decision 0150](../Documents/Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) owns version 13; [Decision 0142](../Documents/Decisions/0142-Immutable-Guest-Resource-Store.md) owns the version-12 predecessor. Version 11 remains the latest cross-host-qualified contract under [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md).

This remains one bounded deterministic boot arena, not a general physical-memory manager. Release is LIFO-only and can restore only a caller-proven suffix ending at the current cursor.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. The planner validates a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB alignment, nonzero page counts, checked last-page arithmetic, and absence of overlaps. The lowest eligible 2 MiB-aligned range at or above 1 MiB and wholly below 4 GiB wins.

An eligible descriptor must contain the complete 144-page range. All comparisons use widths capable of representing 144; the x86-64 implementation must not encode the arena bound as a signed imm8.

## Deterministic arena

The fixed 576 KiB layout is:

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM13`, copied handoff, paging/process/channel/resource/descriptor state |
| `1..4` | 16,384 | down-growing owned kernel stack |
| `5..143` | 569,344 | 139 initially free allocator pages |

The complete 589,824-byte arena is zeroed before publication. Stack top is `arena + 0x5000`, aligned to 16 bytes.

Probe 34 consumes the free extent exactly:

| Pages | Owner |
| --- | --- |
| `5` | kernel IDT page |
| `6..11` | six-page kernel paging hierarchy |
| `12..22` | 11-page init/resource-owner extent |
| `23..143` | 121-page interpreter extent |

The init extent contains four table pages, two RX code pages, one stack page, one data page, two existing RO/NX runtime-resource pages, and one independent RO/NX `WVRS 1` page. The client extent contains four table pages, 110 code pages, six stack pages, and one data/context page. A 1,024-byte execution-scoped record arena occupies client data offset `0x200`; its reply window occupies offset `0x800` through the end of the page, so neither adds a physical page. The two client resource aliases use later virtual pages backed by init's WVB and budget pages, not client placeholders. The init store is never aliased into a client.

Generation 1 reaches cursor `144` with zero free pages. Exact tail release restores cursor `23` and 121 free pages; generation-2 allocation returns page `23` and restores cursor `144` with zero free pages.

The inherited four-page kernel stack remains the pinned-QEMU contract. The six-page user stack is separate and is the minimal whole-page envelope above the verified 24,240-byte native call-graph maximum.

## Memory-state header

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-13 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM13` |
| `0x08` | 4 | Version | `13` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `144` |
| `0x20` | 8 | Next free page | initially `5` |
| `0x28` | 8 | Free pages | initially `139` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | zero until IDT allocation succeeds |

The state page stores `WVKHAND1` at `0x40`, `WVKPAG04` at `0x80`, `WVPROC13` at `0x100` and `0x300`, GDT/TSS at `0x210`, and `WVCHAN03` at `0x410`. Three 128-byte `WVRES005` records begin at `0x480`, `0x500`, and `0x580`.

## Bounded allocation and tail release ABI

`Windvale_kernel_allocate_pages` accepts an exact version-13 state pointer and a nonzero page count. Success advances the cursor, decreases free count, zeroes every returned byte, and returns the first page address. Failure returns zero without mutation.

`Windvale_kernel_release_tail_pages` accepts that state, candidate address, and nonzero page count. The candidate must describe a suffix ending exactly at the current cursor; all arithmetic must remain inside the 144-page arena. Success restores cursor/free count, zeroes the complete suffix, and returns its address. Invalid count, non-tail address, overflow, malformed state, or out-of-arena range returns zero without mutation.

Version 13 retains no free list or allocation-boundary records. The protected-process caller proves that the released 121-page suffix is one complete retired client.

## Diagnostics and limits

The retained diagnostics are `WVOS4001` invalid map envelope, `WVOS4002` unaligned descriptor, `WVOS4003` zero-page descriptor, `WVOS4004` address overflow, `WVOS4005` no eligible arena, and `WVOS4006` overlap. Malformed and random inputs remain bounded.

Version 13 does not claim all physical memory, loader-range reclamation, arbitrary release order, free lists, coalescing, runtime allocation policy, general process creation, concurrent root reuse, SMP, general interrupts, or hardware qualification. Probe 34 preserves one exact release/reallocate cycle while retaining the immutable init store outside the reclaimed client suffix.
