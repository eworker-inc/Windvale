# Windvale x86-64 kernel paging

## Status and purpose

Kernel paging version 1 defines the first page-table root owned and activated by the Windvale kernel after `ExitBootServices`. It is implemented by firmware probe 20 and recorded by [Decision 0088](../Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md). Candidate probe 21 composes the unchanged root with [Decision 0090's](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md) WVB admission path. Local Windows tests and pinned-QEMU scenarios pass; cross-host qualification is pending.

This is a bounded single-address-space foundation. It replaces inherited firmware translation state with a deterministic identity map, a null-page guard, NX enforcement, supervisor write protection, and one fixed executable window. It does not define a virtual-memory manager, address-space isolation, demand paging, reclamation, user mode, or a public mapping API.

## Ownership split

- System-profile [`Hello-World.wv`](../Operating-System/Kernel/Hello-World.wv) publishes `paging=owned` only after installation succeeds. This is the current Windvale-authored policy/evidence surface.
- [`X64-Kernel-Shims.wva`](../Operating-System/Kernel/X64-Kernel-Shims.wva) owns the irreducible privileged operations that enable `EFER.NXE` and `CR0.WP`, load `CR3`, and read `CR3` back.
- The Stage 0 object exported as `Windvale_kernel_x64_paging_install` temporarily validates live machine ranges and constructs the hierarchy. It is a named replacement seam for future system-profile Windvale memory operations and WVA control flow; it is not a permanent C# kernel component.
- The C# planner is an independent deterministic oracle for table bytes, permissions, ownership-record bytes, and rejection cases.

The installer imports four explicit symbols: `Windvale_boot_probe`, `Windvale_kernel_allocate_pages`, `Windvale_kernel_x64_page_protection_enable`, and `Windvale_kernel_x64_page_table_activate`.

## Admission boundary

The live installer fails before changing control state unless all of these are true:

- CPUID exposes extended leaf `0x80000001` and the NX feature bit;
- the memory-state header is exact version 1;
- the retained handoff map is nonempty, at most 1 MiB, arithmetically valid, and wholly below 1 GiB;
- the active stack pointer is nonzero and below 1 GiB;
- `SGDT` reports a nonempty GDT whose inclusive range is wholly below 1 GiB;
- the linked `Windvale_boot_probe` address is 4 KiB aligned, at least 2 MiB, and leaves two complete consecutive 2 MiB regions below 1 GiB;
- the allocator returns one aligned contiguous six-page range wholly below 1 GiB; and
- that range does not overlap the executable window.

The image builder separately requires the linked payload to begin at link offset zero and fit within the 64 KiB executable window. The existing kernel allocator supplies already-zeroed pages. A pre-activation failure returns status 1 without loading the candidate root. Successful activation is accepted only when the WVA readback equals the requested root.

## Fixed hierarchy

Version 1 allocates exactly six consecutive 4 KiB pages:

| Relative page | Structure | Rule |
| ---: | --- | --- |
| `0` | PML4 | Entry 0 points to the PDPT; all others are absent. |
| `1` | PDPT | Entry 0 points to the page directory; all others are absent. |
| `2` | Page directory | Covers the low 1 GiB. Ordinary entries are 2 MiB writable/NX leaves. |
| `3` | Null-region page table | Replaces directory entry 0; 4 KiB leaf 0 is absent and leaves 1 through 511 are writable/NX. |
| `4` | First code-region page table | Covers the 2 MiB region containing the boot entry. |
| `5` | Second code-region page table | Covers the immediately following 2 MiB region. |

All present entries are supervisor-only. Ordinary identity leaves are writable and non-executable. Exactly sixteen consecutive 4 KiB leaves beginning at `Windvale_boot_probe` are present, read-only, and executable. `CR0.WP` makes their read-only status apply in supervisor mode, and `EFER.NXE` makes the NX distinction effective. No admitted leaf is both writable and executable.

The two code tables are always consecutive, so the 64 KiB window may cross one 2 MiB boundary without a special case. The complete linked payload currently fits inside that window. Code growth beyond 64 KiB is a build failure rather than an implicit permission expansion.

## Paging ownership record

After successful CR3 readback, the installer writes this 64-byte little-endian record at memory-state offset `0x80`:

| Offset | Bytes | Field | Version 1 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKPAG01` |
| `0x08` | 4 | Version | `1` |
| `0x0C` | 4 | Record bytes | `64` |
| `0x10` | 8 | Root address | First page of the six-page allocation |
| `0x18` | 8 | Table pages | `6` |
| `0x20` | 8 | Identity bytes | `1,073,741,824` |
| `0x28` | 8 | Executable address | Runtime address of `Windvale_boot_probe` |
| `0x30` | 8 | Executable bytes | `65,536` |
| `0x38` | 8 | Flags | bit 0 NX, bit 1 supervisor write-protect, bit 2 null-page guard |

The record is evidence of the active version-1 root, not a mutable page-map interface.

## WVA privileged operations

WVA 1 adds two no-operand, x86-64-only semantic statements:

- `enable_page_protection` selects EFER, sets NXE, writes EFER, reads CR0, sets WP, and writes CR0. It is exactly 24 bytes and clobbers `RAX`, `RCX`, and `RDX`.
- `activate_page_table` loads `CR3` from `RAX` and reads active `CR3` into `RAX`. It is exactly 6 bytes.

These operations deliberately do not expose arbitrary MSR or control-register access. Their WVA function wrappers add `return`; callers own feature admission, table construction, calling convention, and failure policy.

## Diagnostics and evidence

The host planner reports:

| Code | Meaning |
| --- | --- |
| `WVOS5001` | The six-page table range is null, unaligned, incomplete, or outside the low 1 GiB. |
| `WVOS5002` | The executable address is unaligned or cannot be represented by the two admitted code tables. |
| `WVOS5003` | The table allocation overlaps the executable window. |

The candidate paging WVO is 1,244 bytes with SHA-256 `deeebe592b38890c9964cc4d9736b1d617c0d6b20bed494ba533dcb9b1d4f318`; its 851 code bytes have SHA-256 `12cbb64dad4558f94fd7075995cb5ac8a788ed5476999d14d2a585b310021678`. Its four relocations are exact, relative, and target only the four named imports.

Firmware probe 20 produces three exact 22,016-byte PE32+ images. The normal image has SHA-256 `392a2801bd8d8895bd9c34213336a69057c1ae81675269056c60b8c3e974ab01` and powers off with host code 0. The invalid-opcode image has SHA-256 `aa610e6ac00ed43466a87521bb4cebb2934d0885acb960db8913f025ced9cce9`; the general-protection image has SHA-256 `74632fcde4873f2d46e18b1b77c5cc8b495e83f0f750930e039da27dd67cd0ee`. Both faults occur after activation, retain their normalized error evidence, and terminate with host code 3.

Composed probe 21 produces three 47,104-byte images and retains the same paging object and activation order. All three pinned-QEMU scenarios pass after the added in-guest admission path; Decision 0090 records their exact identities. Neither candidate has completed exact cross-host qualification.

Local evidence includes all 20 OS tests, all 6 focused assembler tests, and all three pinned-QEMU scenarios. Complete Windows/Debian qualification and independent GitHub evidence remain pending.

## Deliberate limits

Version 1 retains one global identity-mapped ring-0 address space and keeps interrupts disabled. It does not protect kernel objects from one another, unmap firmware/loader ranges selectively, map memory above 1 GiB, handle page faults, use global or huge-page optimization policy, support PCID, KASLR, SMP shootdown, copy-on-write, shared memory, user/supervisor separation, or release table pages. Those require later contracts rather than silent expansion of this one.
