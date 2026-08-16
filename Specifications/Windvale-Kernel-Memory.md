# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 17 is the implemented Probe-40 candidate owned by [Decision 0196](../Documents/Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md). It replaces the qualified tail-only process release mechanism with a fixed 160-bit allocation bitmap, one owner byte per arena page, and three generation-safe `WVMEMO01` records. The normal and contained client-fault paths release the 122-page client object while the later directory object remains live, zero every released page, and first-fit the same physical root for generation 2.

Version 16 and Probe 39 are cross-host qualified at exact implementation commit `6a250c86c30e8921d6bf9244a27d0fd763716cb0` and GitHub [Verify run 30847279400](https://github.com/eworker-inc/Windvale/actions/runs/30847279400). Version 17 has focused Windows host and pinned-QEMU evidence; independent Windows/Linux qualification remains pending. This remains one bounded deterministic boot arena, not a general physical-memory manager.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. The planner validates a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB alignment, nonzero page counts, checked last-page arithmetic, and absence of overlaps. The lowest eligible 2 MiB-aligned range at or above 1 MiB and wholly below 4 GiB wins.

An eligible descriptor must contain the complete 157-page range. All comparisons use widths capable of representing 157; the x86-64 implementation must not encode the arena bound as a signed immediate byte.

## Deterministic arena

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM17`, copied handoff, paging/process/channel/resource/timer state, bitmap, owner map, and object records |
| `1..4` | 16,384 | down-growing owned kernel stack |
| `5..156` | 622,592 | 152 initially free allocator pages |

The complete 643,072-byte arena is zeroed before publication. Stack top is `arena + 0x5000`, aligned to 16 bytes. Probe 40 consumes the free range exactly:

| Pages | Owner |
| --- | --- |
| `5` | fixed kernel IDT page |
| `6..12` | fixed seven-page kernel paging hierarchy |
| `13..24` | init memory object `65537`, 12 pages |
| `25..146` | client memory object `65538`, 122 pages |
| `147..156` | directory memory object `65539`, 10 pages |

The fixed allocator advances its retained bootstrap cursor to 13. Object allocation does not reinterpret that cursor: first-fit scans the bitmap from page 5, while the free-page field tracks aggregate availability. After all three retained objects are active, free pages are zero. Releasing client generation 1 restores 122 free pages without changing the directory pages or raw cursor; generation 2 reference `131074` reuses pages `25..146` and returns free pages to zero. The current filesystem-publication successor releases terminal generation 2, then first-fits generation 3 reference `196610` as 85 pages at the same root, leaving 37 free pages without changing the directory object or raw cursor. It publishes a durable domain charge for the 81 user pages before process/thread readiness.

The provider/client page contents and mappings remain those in [Windvale-Protected-Process.md](Windvale-Protected-Process.md). The inherited four-page kernel stack and six-page client stack remain fixed evidence.

## Memory-state header and page evidence

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-17 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM17` |
| `0x08` | 4 | Version | `17` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `157` |
| `0x20` | 8 | Fixed bootstrap cursor | initially `5`, then exactly `13` |
| `0x28` | 8 | Free pages | initially `152`; object allocation/release changes it |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | zero until IDT allocation succeeds |

The complete state-page layout is disjoint:

| Interval | Record |
| --- | --- |
| `0x040..0x06F` | `WVKHAND1` |
| `0x080...` | `WVKPAG07` |
| `0x100..0x21F` | init `WVPROC17` |
| `0x220..0x2D7` | private GDT, GDTR, and TSS |
| `0x300..0x41F` | current client `WVPROC17` |
| `0x420..0x85F` | two channels, live resource endpoint, four resources, and directory `WVPROC17` |
| `0x860..0x89F` | filesystem `WVDOM001`, after exact terminal directory-endpoint reuse |
| `0x8A0..0xB9F` | three private `WVTHR001` contexts and `WVTIME01` |
| `0xBA0..0xBB3` | 160-bit allocation bitmap |
| `0xBC0..0xC5C` | 157-byte page-owner table |
| `0xC60..0xD8F` | init `WVMEMO01` |
| `0xD90..0xEBF` | client `WVMEMO01` |
| `0xEC0..0xFEF` | directory `WVMEMO01` |

Bitmap bits 157 through 159 are permanently set. Owner byte `0` means free, `255` means fixed kernel ownership, and `1..254` is the low 16-bit memory-object identifier after the allocator rejects larger identifiers. The fixed page allocator marks both bitmap and owner evidence before object allocation begins.

## `WVMEMO01`

Each 304-byte little-endian record is exact:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | magic `WVMEMO01` |
| `0x08` | 4 | version `1` |
| `0x0C` | 4 | record bytes `304` |
| `0x10` | 4 | state: active `1` or released `2` |
| `0x14` | 4 | generation-stamped reference |
| `0x18` | 4 | owner reference, equal to reference |
| `0x1C` | 4 | active page count, or zero when released |
| `0x20` | 4 | allocation count |
| `0x24` | 4 | release count |
| `0x28` | 4 | page-vector count, equal to active page count |
| `0x2C` | 4 | object identifier |
| `0x30` | 8 | active base address, or zero when released |
| `0x38` | 248 | little-endian `u16` page vector plus zero tail |

The reference encodes generation in the high 16 bits and object identifier in the low 16 bits. Active records require `allocation count = release count + 1`; released records require balanced counts, zero base, zero page/vector count, and a zero vector. This first slice admits at most 122 contiguous pages. Its explicit vector and owner evidence prepare later noncontiguous physical backing, but Probe 40 does not claim scatter allocation or a general virtual mapper.

## Allocation and release boundaries

`Windvale_kernel_allocate_pages` remains a private Stage 0 x86-64 leaf for the fixed IDT and paging pages. It validates `WVKMEM17`, advances the raw cursor, updates free count, marks bitmap/owner state as kernel-owned, zeroes the returned pages, and returns the first address. It is not the process allocator.

`Windvale_kernel_allocate_memory_object` and `Windvale_kernel_release_memory_object` are assembled from `X64-Memory-Object-Shims.wva`. The exact 2,538-byte WVO has SHA-256 `fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee`; its 2,374-byte relocation-free text contains a 1,389-byte allocator and 985-byte releaser.

Allocation validates the complete state and prior record, finds a complete first-fit run, preflights every bitmap bit and owner byte before mutation, marks ownership, zeroes every page, writes the page vector and active record, updates free pages, and returns the base. Reuse requires the exact released prior generation and balanced history.

Release validates the exact live reference and complete page vector, contiguity, base, bitmap, owner map, and resulting free bound before mutation. It then clears bitmap/owner evidence, zeroes every page, clears the vector/base, advances release history, and returns the retired base. Rejection returns zero without mutation.

Portable `Process-Foundation.wv` owns the matching object identities, layout bounds, non-tail ordering, generation transition, allocation/release counts, free-page transitions, and requirement that the directory object remain active. WVA owns bounded first-fit mechanics and page zeroing; C# remains the Stage 0 builder, reference oracle, and recovery implementation.

## Diagnostics and limits

The retained planner diagnostics are `WVOS4001` invalid map envelope, `WVOS4002` unaligned descriptor, `WVOS4003` zero-page descriptor, `WVOS4004` address overflow, `WVOS4005` no eligible arena, and `WVOS4006` overlap. Host tests cover truncation, corrupt ownership, trailing bytes, wrong generation, unchanged state on rejection, exact zeroing, live-directory preservation, and same-root reuse. On Windows x64 the focused test also executes the exact WVA allocator/releaser through the controlled native publication lifetime.

Version 17 does not claim all physical memory, loader-range reclamation, coalescing, fragmentation policy, arbitrary object count or size, noncontiguous allocation, general virtual mappings, executable publication, pinning, DMA, concurrency, SMP, dynamic process creation, or physical-hardware qualification. The service-fault path retains its bounded generation-1 cleanup and makes no additional memory-object reuse claim.
