# Windvale x86-64 kernel memory foundation

## Status and purpose

Kernel memory version 6 is the Probe 29 candidate. It expands the qualified version-5 arena from 60 to 63 pages: one page for init's second owned resource and two pages for the measured growth from a two-page to a four-page kernel stack.

[Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) owns candidate version 6. Version 5 remains the latest cross-host-qualified memory baseline through Probe 28.

This is one bounded boot arena, not a general physical-memory manager. Allocation remains monotonic, deterministic, visibly finite, and release-free.

## Ownership and validation

Only UEFI type 7, `EfiConventionalMemory`, is claimable. Loader, boot-service, runtime-service, ACPI, persistent, unaccepted, MMIO, reserved, and unknown ranges remain unowned. The retained map buffer and linked image remain outside the arena under their firmware classifications.

The planner requires a nonempty map no larger than 1 MiB, descriptor stride 40 through 256 bytes, exact divisibility, 4 KiB-aligned physical and virtual starts, nonzero page counts, and checked last-page arithmetic. Unknown types are structurally valid but not claimable. No arena is returned until the complete map passes.

## Deterministic arena

An eligible descriptor must contain a complete 63-page range beginning at its first 2 MiB-aligned address at or above 1 MiB and ending below 4 GiB. The lowest eligible address wins independent of descriptor order. Alignment loss is included in the fit check. Any overlap with another descriptor rejects the map.

The fixed 252 KiB layout is:

| Arena pages | Bytes | Purpose |
| --- | ---: | --- |
| `0` | 4,096 | `WVKMEM06`, copied handoff, paging/process/channel/resource/descriptor state |
| `1..4` | 16,384 | Down-growing owned kernel stack |
| `5..62` | 237,568 | Fifty-eight initially free allocator pages |

The complete arena is zeroed before state publication. Stack top is `arena + 0x5000`, aligned to 16 bytes.

Probe 29 consumes the free extent exactly:

| Pages | Owner |
| --- | --- |
| `5` | Kernel IDT page |
| `6..11` | Six-page kernel paging hierarchy |
| `12..20` | Nine-page init/resource-owner extent |
| `21..62` | Forty-two-page interpreter extent |

The init extent contains four table pages, code, stack, data, WVB, and budget pages. The client extent contains four table pages, 33 code pages, four stack pages, and one data page. Its two resource aliases are later virtual pages backed by the two init pages, not client placeholders. The final cursor is page `63` with zero free pages.

Pinned QEMU measurement is part of the contract choice: two pages allow the enlarged Windvale policy's generated native frame to overwrite state; three pages still do not complete process construction; four pages pass normal, both terminal kernel-fault scenarios, and contained user fault. After the policy call, the coordinator derives the 2 MiB-aligned arena base from the owned stack and revalidates `WVKMEM06` before process publication.

## Memory-state header

The first page begins with this 64-byte little-endian header:

| Offset | Bytes | Field | Version-6 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKMEM06` |
| `0x08` | 4 | Version | `6` |
| `0x0C` | 4 | Header bytes | `64` |
| `0x10` | 8 | Arena address | Selected 2 MiB-aligned base |
| `0x18` | 8 | Arena pages | `63` |
| `0x20` | 8 | Next free page | Initially `5` |
| `0x28` | 8 | Free pages | Initially `58` |
| `0x30` | 8 | Handoff-copy address | `arena + 64` |
| `0x38` | 8 | First allocation address | Zero until IDT allocation succeeds |

The 48-byte `WVKHAND1` record is copied to `arena + 64`; its map pointer remains borrowed. Paging record `WVKPAG03` remains at `0x80`; `WVPROC08` records are at `0x100` and `0x300`; GDT/TSS state begins at `0x200`; `WVCHAN01` begins at `0x400`; and the two `WVRES003` records begin at `0x440` and `0x4C0`. All are kernel-only.

## Allocate-only ABI

`Windvale_kernel_allocate_pages` accepts an exact version-6 state pointer in `RCX` and a nonzero page count in `EDX`. Success advances the cursor, decreases free count, zeroes every returned byte, and returns the first page address in `RAX`. Failure returns zero without mutation. Allocation is contiguous and monotonically increasing; version 6 has no release operation.

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

## Candidate evidence and limits

Probe 29 adds `typed-resources=pass` between `resource-grant=pass` and `resource-revoked=pass`. The four clean Windows QEMU identities are:

| Scenario | EFI bytes | SHA-256 | Guest result |
| --- | ---: | --- | ---: |
| Normal | 258,048 | `a8a14581eab4c1a6d67aba7af0cec1baa956574a410a4cd0de1121e1f843ee67` | poweroff `0` |
| Invalid opcode | 258,048 | `35ee08e97aff4f6a2c0018c962960d5c7ee8af58fe6d5b36565613a99292ad0f` | panic/host `3` |
| General protection | 258,048 | `92ae33986ab53245f57dc9263179e6dcd2c66cf79b634dcedaee51e93f915ca7` | panic/host `3` |
| Contained user fault | 258,560 | `35a3dece4e64463bc9df7ef73c83ec5f5fff3b0daedd7176f77f1c2ef5525484` | poweroff `0` |

Version 6 does not claim all physical memory, loader-range reclamation, page release, runtime allocation policy, process creation, address-space reuse, SMP, general interrupts, or hardware qualification. The arena is deliberately exhausted, so reclamation remains an explicit later decision.
