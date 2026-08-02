# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 7 is the implemented Probe 30 contract. It retains version 6's 63-page arena and adds one exact checked tail-release operation so a terminal 42-page client extent can be scrubbed and reused.

[Decision 0100](../Documents/Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) owns version 7. [Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) and exact commit `3fd9ef7535d7536ed084144e4f697cda548bf35c` retain the qualified version-6 baseline.

This is one bounded boot arena, not a general physical-memory manager. Allocation is deterministic and visibly finite. Release is LIFO-only: it can restore only a caller-proven suffix ending at the current cursor. The memory state does not retain allocation-boundary provenance; Probe 30's process record and fixed layout prove that its 42-page suffix is the retired client extent.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. Loader, boot-service, runtime-service, ACPI, persistent, unaccepted, MMIO, reserved, and unknown ranges remain unowned. The retained map buffer and linked image remain outside the arena under their firmware classifications.

The planner requires a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB-aligned physical and virtual starts, nonzero page counts, and checked last-page arithmetic. Unknown types are structurally valid but not claimable. No arena is returned until the complete map passes.

## Deterministic arena

An eligible descriptor must contain a complete 63-page range beginning at its first 2 MiB-aligned address at or above 1 MiB and ending below 4 GiB. The lowest eligible address wins independent of descriptor order. Alignment loss is included in the fit check. Any overlap with another descriptor rejects the map.

The fixed 252 KiB layout is:

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM07`, copied handoff, paging/process/channel/resource/descriptor state |
| `1..4` | 16,384 | Down-growing owned kernel stack |
| `5..62` | 237,568 | Fifty-eight initially free allocator pages |

The complete arena is zeroed before state publication. Stack top is `arena + 0x5000`, aligned to 16 bytes.

Probe 30 first consumes the free extent exactly:

| Pages | Owner |
| --- | --- |
| `5` | Kernel IDT page |
| `6..11` | Six-page kernel paging hierarchy |
| `12..20` | Nine-page init/resource-owner extent |
| `21..62` | Forty-two-page interpreter extent |

The init extent contains four table pages, code, stack, data, WVB, and budget pages. The client extent contains four table pages, 33 code pages, four stack pages, and one data page. Its two resource aliases are later virtual pages backed by the two init pages, not client placeholders. Generation 1 reaches cursor `63` with zero free pages. Exact tail release restores cursor `21` and 42 free pages; immediate generation-2 allocation returns page `21` and restores cursor `63` with zero free pages.

Pinned QEMU measurement remains part of the inherited stack contract: two pages allowed the Probe-29 Windvale policy's generated native frame to overwrite state; three pages still did not complete process construction; four pages pass normal, both terminal kernel-fault scenarios, and contained user fault. After the policy call, the coordinator derives the 2 MiB-aligned arena base from the owned stack and revalidates `WVKMEM07` before process publication.

## Memory-state header

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-7 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM07` |
| `0x08` | 4 | Version | `7` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | Selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `63` |
| `0x20` | 8 | Next free page | Initially `5` |
| `0x28` | 8 | Free pages | Initially `58` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | Zero until IDT allocation succeeds |

The 48-byte `WVKHAND1` record is copied to `arena + 64`; its map pointer remains borrowed. Paging record `WVKPAG03` remains at `0x80`; `WVPROC09` records are at `0x100` and `0x300`; GDT/TSS state begins at `0x210`; `WVCHAN01` begins at `0x410`; and the two `WVRES004` records begin at `0x450` and `0x4D0`. All are kernel-only.

## Bounded allocation and tail release ABI

`Windvale_kernel_allocate_pages` accepts an exact version-7 state pointer in `RCX` and a nonzero page count in `EDX`. Success advances the cursor, decreases free count, zeroes every returned byte, and returns the first page address in `RAX`. Failure returns zero without mutation.

`Windvale_kernel_release_tail_pages` accepts that state pointer in `RCX`, the candidate address in `RDX`, and a nonzero page count in `R8D`. The candidate must describe a suffix ending exactly at the current cursor, all checked arithmetic must remain inside the 63-page arena, and restored free pages must remain bounded. Success restores cursor/free count, zeroes the complete suffix, and returns its address. A zero count, non-tail address, underflow, overflow, malformed state, or out-of-arena range returns zero without mutation. Version 7 does not retain allocation records or a free list, so its caller owns proof that the suffix corresponds to a complete retired allocation; it cannot release non-tail extents.

`Windvale_kernel_memory_enter` validates and copies the handoff, initializes the arena, records the IDT allocation, switches stacks, installs exceptions and paging, and enters the Windvale admission/process chain. Only the complete typed-resource sequence can reach final Main.

## Diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS4001` | Invalid map envelope or descriptor stride. |
| `WVOS4002` | Unaligned descriptor address. |
| `WVOS4003` | Zero-page descriptor. |
| `WVOS4004` | Descriptor physical-address overflow. |
| `WVOS4005` | No eligible 2 MiB-aligned 252 KiB conventional-memory arena. |
| `WVOS4006` | Another descriptor overlaps the selected arena. |

Malformed and random inputs must remain bounded and must not escape index, arithmetic, or allocation exceptions.

## Probe evidence and limits

Probe 30 adds `process-reuse=pass` after `resource-revoked=pass`. The four clean Windows QEMU identities are:

| Scenario | EFI bytes | SHA-256 | Guest result |
| --- | ---: | --- | ---: |
| Normal | 261,120 | `5034c01a98f20344d96fa091fd9a55a303e72669d746a4b83df2900eed93992f` | poweroff `0` |
| Invalid opcode | 261,120 | `bb57ebf7e50eb56bf3d42d91b2213ed5b262554416fdf76609142eccba44cc55` | panic/host `3` |
| General protection | 261,120 | `d56fe572fb7a7ff724f7b7c26aa5299a6c5cee4c203f009b63d651c1d3cd8fcc` | panic/host `3` |
| Contained user fault | 261,632 | `78dfa73a80a05021273cb44587f6b957d16d4cd4ebaec487f7b8a8f5427846ca` | poweroff `0` |

Version 7 does not claim all physical memory, loader-range reclamation, arbitrary release order, free lists, coalescing, runtime allocation policy, general process creation, concurrent address-space reuse, SMP, general interrupts, or hardware qualification. It proves one exact release/reallocate cycle only.
