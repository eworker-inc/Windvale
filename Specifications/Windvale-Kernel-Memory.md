# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 16 is the implemented Probe-39 candidate owned by [Decision 0188](../Documents/Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md). It adds one page for paging version 5's shared timer-MMIO directory and disjoint state-page storage for three private thread contexts plus one timer record while retaining version 15's provider/client extents, compact stacks, checked LIFO tail release, and same-root client reuse.

Version 15 is cross-host qualified at exact implementation commit `aae6818e3226e9e7e88d205b4666fb9904e4735b` and GitHub [Verify run 30834243770](https://github.com/eworker-inc/Windvale/actions/runs/30834243770). Version 16 has focused Windows and all five pinned-QEMU results; cross-host qualification remains pending. This is one bounded deterministic boot arena, not a general physical-memory manager. Release is LIFO-only and can restore only a caller-proven suffix ending at the current cursor.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. The planner validates a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB alignment, nonzero page counts, checked last-page arithmetic, and absence of overlaps. The lowest eligible 2 MiB-aligned range at or above 1 MiB and wholly below 4 GiB wins.

An eligible descriptor must contain the complete 157-page range. All comparisons use widths capable of representing 157; the x86-64 implementation must not encode the arena bound as a signed immediate byte.

## Deterministic arena

The fixed layout is:

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM16`, copied handoff, paging/process/channel/resource/descriptor/timer state |
| `1..4` | 16,384 | down-growing owned kernel stack |
| `5..156` | 622,592 | 152 initially free allocator pages |

The complete 643,072-byte arena is zeroed before publication. Stack top is `arena + 0x5000`, aligned to 16 bytes.

Probe 39 consumes the free extent exactly:

| Pages | Owner |
| --- | --- |
| `5` | kernel IDT page |
| `6..12` | seven-page kernel paging hierarchy including timer MMIO |
| `13..24` | 12-page init/resource-provider extent |
| `25..34` | ten-page directory-provider extent |
| `35..156` | 122-page interpreter extent |

The init extent contains four table pages, two RX code pages, one stack page, one data page, two RO/NX runtime-resource pages, one RO/NX `WVRS 1` page, and one RW/NX service-response page. It does not map `WVDS 1`.

The directory extent contains four table pages, two RX code pages, one stack page, one data page, one RO/NX `WVDS 1` page, and one RW/NX service-response page. It does not map the boot store or runtime-resource pages.

The client extent contains four table pages, 110 code pages, six stack pages, one data/context page, and one RW/NX service-response page. Its two runtime-resource aliases use later virtual pages backed by init's WVB and budget pages. Neither the store nor snapshot is aliased into a client.

Generation 1 reaches cursor `157` with zero free pages. Exact tail release restores cursor `35` and 122 free pages; generation-2 allocation returns page `35` and restores cursor `157` with zero free pages. Init and directory extents remain allocated and unchanged across client reuse.

The inherited four-page kernel stack remains the pinned-QEMU contract. The six-page user stack is separate and is the minimal whole-page envelope above the verified 24,240-byte native call-graph maximum.

## Memory-state header

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-16 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM16` |
| `0x08` | 4 | Version | `16` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `157` |
| `0x20` | 8 | Next free page | initially `5` |
| `0x28` | 8 | Free pages | initially `152` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | zero until IDT allocation succeeds |

The Probe-39 state page uses these disjoint intervals:

| Interval | Record |
| --- | --- |
| `0x040..0x06F` | `WVKHAND1` |
| `0x080...` | `WVKPAG05` |
| `0x100..0x21F` | init `WVPROC17` |
| `0x220..0x257` | private GDT |
| `0x260..0x269` | GDTR |
| `0x270..0x2D7` | TSS |
| `0x300..0x41F` | current client `WVPROC17` |
| `0x420..0x48F` | resource `WVCHAN04` |
| `0x490..0x4CF` | resource `WVENDP01` |
| `0x4D0..0x6CF` | four `WVRES006` records |
| `0x6D0..0x7EF` | directory-provider `WVPROC17` |
| `0x7F0..0x85F` | directory `WVCHAN04` |
| `0x860..0x89F` | directory `WVENDP01` |
| `0x8A0..0x97F` | init private `WVTHR001` context |
| `0x980..0xA5F` | client private `WVTHR001` context |
| `0xA60..0xB3F` | directory private `WVTHR001` context |
| `0xB40..0xB9F` | private `WVTIME01` timer evidence |

Process-machine construction checks the process/descriptor/client/final-page boundaries before emitting code. Each endpoint resolution independently checks the exact channel address, magic, version, record size, and capacity before channel mutation. This prevents a future record expansion from silently overlapping the next live object.

## Bounded allocation and tail release ABI

`Windvale_kernel_allocate_pages` accepts an exact version-16 state pointer and a nonzero page count. Success advances the cursor, decreases free count, zeroes every returned byte, and returns the first page address. Failure returns zero without mutation.

`Windvale_kernel_release_tail_pages` accepts that state, candidate address, and nonzero page count. The candidate must describe a suffix ending exactly at the current cursor; all arithmetic must remain inside the 157-page arena. Success restores cursor/free count, zeroes the complete suffix, and returns its address. Invalid count, non-tail address, overflow, malformed state, or out-of-arena range returns zero without mutation.

Version 16 retains no free list or allocation-boundary records. The protected-process caller proves that the released 122-page suffix is one complete retired client.

## Diagnostics and limits

The retained diagnostics are `WVOS4001` invalid map envelope, `WVOS4002` unaligned descriptor, `WVOS4003` zero-page descriptor, `WVOS4004` address overflow, `WVOS4005` no eligible arena, and `WVOS4006` overlap. Malformed and random inputs remain bounded.

Version 16 does not claim all physical memory, loader-range reclamation, arbitrary release order, free lists, coalescing, independently lived allocations, runtime allocation policy, general process creation, concurrent root reuse, SMP, or physical-hardware qualification. Probe 39 preserves exact release/reallocate behavior on normal and client-fault paths. Its service-fault path stops after contained cleanup of generation 1 and therefore makes no additional reclamation claim.
