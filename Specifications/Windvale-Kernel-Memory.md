# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 14 is the cross-host-qualified Probe-35 contract owned by [Decision 0159](../Documents/Decisions/0159-First-Guest-Directory-Service.md). It adds an immutable `WVDS 1` mapping, dedicated init/client service-response pages, and the larger rebuilt client while retaining version 13's compact stacks, checked LIFO tail release, and same-root reuse. Qualified Probe 36 retains the same contract for service-failure containment. Locally verified Probe 37 and [Decision 0172](../Documents/Decisions/0172-First-Kernel-Owned-Service-Endpoint.md) also retain the arena and mappings while using previously zero state-page bytes for `WVENDP01`; cross-host qualification of that composition remains pending.

This is one bounded deterministic boot arena, not a general physical-memory manager. Release is LIFO-only and can restore only a caller-proven suffix ending at the current cursor.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. The planner validates a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB alignment, nonzero page counts, checked last-page arithmetic, and absence of overlaps. The lowest eligible 2 MiB-aligned range at or above 1 MiB and wholly below 4 GiB wins.

An eligible descriptor must contain the complete 147-page range. All comparisons use widths capable of representing 147; the x86-64 implementation must not encode the arena bound as a signed immediate byte.

## Deterministic arena

The fixed 588 KiB layout is:

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM14`, copied handoff, paging/process/channel/resource/descriptor state |
| `1..4` | 16,384 | down-growing owned kernel stack |
| `5..146` | 581,632 | 142 initially free allocator pages |

The complete 602,112-byte arena is zeroed before publication. Stack top is `arena + 0x5000`, aligned to 16 bytes.

Probes 35 through 37 consume the free extent exactly:

| Pages | Owner |
| --- | --- |
| `5` | kernel IDT page |
| `6..11` | six-page kernel paging hierarchy |
| `12..24` | 13-page init/resource-and-directory owner extent |
| `25..146` | 122-page interpreter extent |

The init extent contains four table pages, two RX code pages, one stack page, one data page, two RO/NX runtime-resource pages, one RO/NX `WVRS 1` page, one RO/NX `WVDS 1` page, and one RW/NX service-response page. The client extent contains four table pages, 110 code pages, six stack pages, one data/context page, and one RW/NX service-response page. The two client resource aliases use later virtual pages backed by init's WVB and budget pages, not client placeholders. Neither the store nor snapshot is aliased into a client.

Generation 1 reaches cursor `147` with zero free pages. Exact tail release restores cursor `25` and 122 free pages; generation-2 allocation returns page `25` and restores cursor `147` with zero free pages.

The inherited four-page kernel stack remains the pinned-QEMU contract. The six-page user stack is separate and is the minimal whole-page envelope above the verified 24,240-byte native call-graph maximum.

## Memory-state header

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-14 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM14` |
| `0x08` | 4 | Version | `14` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `147` |
| `0x20` | 8 | Next free page | initially `5` |
| `0x28` | 8 | Free pages | initially `142` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | zero until IDT allocation succeeds |

The Probe-37 state page stores `WVKHAND1` at `0x40`, `WVKPAG04` at `0x80`, `WVPROC16` at `0x100` and `0x300`, GDT/TSS at `0x210`, `WVCHAN04` at `0x410`, and the 64-byte `WVENDP01` at `0x480`. Four 128-byte `WVRES006` records begin at `0x4C0`, `0x540`, `0x5C0`, and `0x640`. Probe 36 used `WVPROC15`, no separate endpoint, and resource offsets `0x480` through `0x600`; no allocation or user mapping changes in Probe 37.

## Bounded allocation and tail release ABI

`Windvale_kernel_allocate_pages` accepts an exact version-14 state pointer and a nonzero page count. Success advances the cursor, decreases free count, zeroes every returned byte, and returns the first page address. Failure returns zero without mutation.

`Windvale_kernel_release_tail_pages` accepts that state, candidate address, and nonzero page count. The candidate must describe a suffix ending exactly at the current cursor; all arithmetic must remain inside the 147-page arena. Success restores cursor/free count, zeroes the complete suffix, and returns its address. Invalid count, non-tail address, overflow, malformed state, or out-of-arena range returns zero without mutation.

Version 14 retains no free list or allocation-boundary records. The protected-process caller proves that the released 122-page suffix is one complete retired client.

## Diagnostics and limits

The retained diagnostics are `WVOS4001` invalid map envelope, `WVOS4002` unaligned descriptor, `WVOS4003` zero-page descriptor, `WVOS4004` address overflow, `WVOS4005` no eligible arena, and `WVOS4006` overlap. Malformed and random inputs remain bounded.

Version 14 does not claim all physical memory, loader-range reclamation, arbitrary release order, free lists, coalescing, runtime allocation policy, general process creation, concurrent root reuse, SMP, general interrupts, or hardware qualification. Probes 36 and 37 preserve Probe 35's exact release/reallocate cycle on normal and client-fault paths while retaining the immutable init store and directory snapshot outside the reclaimed client suffix. Their service-fault path stops after contained cleanup of generation 1 and therefore makes no additional reclamation claim.
