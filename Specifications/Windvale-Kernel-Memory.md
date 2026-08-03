# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 11 is the qualified Probe-32 contract. It expands the client for ABI 21 while retaining the compact six-page stack, checked tail release, and process-root reuse. [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md) owns version 11. Probe 33 and [Decision 0135](../Documents/Decisions/0135-Bounded-Guest-Resource-Request-Reply.md) retain the same arena because the larger guest IPC shims still fit its existing 109 client RX pages.

This remains one bounded deterministic boot arena, not a general physical-memory manager. Release is LIFO-only and can restore only a caller-proven suffix ending at the current cursor.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. The planner validates a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB alignment, nonzero page counts, checked last-page arithmetic, and absence of overlaps. The lowest eligible 2 MiB-aligned range at or above 1 MiB and wholly below 4 GiB wins.

An eligible descriptor must contain the complete 141-page range. All comparisons use widths capable of representing 141; the x86-64 implementation must not encode the arena bound as a signed imm8.

## Deterministic arena

The fixed 564 KiB layout is:

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM11`, copied handoff, paging/process/channel/resource/descriptor state |
| `1..4` | 16,384 | down-growing owned kernel stack |
| `5..140` | 557,056 | 136 initially free allocator pages |

The complete arena is zeroed before publication. Stack top is `arena + 0x5000`, aligned to 16 bytes.

Probe 33 consumes the free extent exactly:

| Pages | Owner |
| --- | --- |
| `5` | kernel IDT page |
| `6..11` | six-page kernel paging hierarchy |
| `12..20` | nine-page init/resource-owner extent |
| `21..140` | 120-page interpreter extent |

The client extent contains four table pages, 109 code pages, six stack pages, and one data/context page. A 1,024-byte execution-scoped record arena occupies offset `0x200`; the resource reply window occupies offset `0x800` through the end of that same page, so neither adds a physical page. The two resource aliases use later virtual pages backed by init's WVB and budget pages, not client placeholders. Generation 1 reaches cursor `141` with zero free pages. Exact tail release restores cursor `21` and 120 free pages; generation-2 allocation returns page `21` and restores cursor `141` with zero free pages.

The inherited four-page kernel stack remains the pinned-QEMU contract. The six-page user stack is separate and is the minimal whole-page envelope above the verified 24,240-byte native call-graph maximum.

## Memory-state header

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-11 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM11` |
| `0x08` | 4 | Version | `11` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `141` |
| `0x20` | 8 | Next free page | initially `5` |
| `0x28` | 8 | Free pages | initially `136` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | zero until IDT allocation succeeds |

The retained state-page record offsets remain unchanged through the channel: `WVKHAND1` at `0x40`, `WVKPAG04` at `0x80`, `WVPROC12` at `0x100` and `0x300`, GDT/TSS at `0x210`, and `WVCHAN02` at `0x410`. The expanded 96-byte channel moves the two `WVRES004` records to `0x470` and `0x4F0`.

## Bounded allocation and tail release ABI

`Windvale_kernel_allocate_pages` accepts an exact version-11 state pointer and a nonzero page count. Success advances the cursor, decreases free count, zeroes every returned byte, and returns the first page address. Failure returns zero without mutation.

`Windvale_kernel_release_tail_pages` accepts that state, candidate address, and nonzero page count. The candidate must describe a suffix ending exactly at the current cursor; all arithmetic must remain inside the 141-page arena. Success restores cursor/free count, zeroes the complete suffix, and returns its address. Invalid count, non-tail address, overflow, malformed state, or out-of-arena range returns zero without mutation.

Version 11 retains no free list or allocation-boundary records. The protected-process caller proves that the released 120-page suffix is one complete retired client.

## Diagnostics and limits

The retained diagnostics are `WVOS4001` invalid map envelope, `WVOS4002` unaligned descriptor, `WVOS4003` zero-page descriptor, `WVOS4004` address overflow, `WVOS4005` no eligible arena, and `WVOS4006` overlap. Malformed and random inputs remain bounded.

Version 11 does not claim all physical memory, loader-range reclamation, arbitrary release order, free lists, coalescing, runtime allocation policy, general process creation, concurrent root reuse, SMP, general interrupts, or hardware qualification. Probe 33 preserves its exact release/reallocate cycle while using the existing client extent for the bounded request/reply windows.
