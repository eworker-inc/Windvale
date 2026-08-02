# Windvale x86-64 kernel paging

## Status and purpose

Kernel paging version 3 is used by candidate probe 25. It keeps the six-page low-1-GiB identity hierarchy, null guard, NX enforcement, and supervisor write protection while expanding the fixed kernel executable window to 256 KiB for the measured section-derived interpreter composition. It publishes `WVKPAG03`; earlier experimental ownership records are not accepted under the larger bound.

[Decision 0088](../Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md) owns the qualified version-1 root and probe-20/21 evidence. [Decision 0091](../Documents/Decisions/0091-First-Protected-Windvale-Process.md) owns version 2 and its first executable-window expansion. [Decision 0093](../Documents/Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) cross-host qualifies that form; [Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) owns version 3.

The kernel root remains a bounded construction foundation, not a general virtual-memory manager. Protected-process version 4 separately derives two process roots with three init leaves and 37 interpreter leaves; it does not mutate the kernel-root contract into a public mapping API.

## Ownership split

- System-profile [`Hello-World.wv`](../Operating-System/Kernel/Hello-World.wv) publishes `paging=owned` only after installation succeeds.
- [`X64-Kernel-Shims.wva`](../Operating-System/Kernel/X64-Kernel-Shims.wva) owns the named privileged operations that enable `EFER.NXE` and `CR0.WP`, load `CR3`, and read `CR3` back.
- The Stage 0 `Windvale_kernel_x64_paging_install` object temporarily validates live ranges and constructs the hierarchy. It is a named replacement seam for future system-profile Windvale memory operations and WVA control flow.
- The C# planner is an independent deterministic oracle for tables, permissions, ownership-record bytes, and rejection cases.

The installer imports `Windvale_boot_probe`, `Windvale_kernel_allocate_pages`, `Windvale_kernel_x64_page_protection_enable`, and `Windvale_kernel_x64_page_table_activate` through four exact relative relocations.

## Admission boundary

The live installer fails before changing control state unless:

- CPUID exposes extended leaf `0x80000001` and NX;
- the memory-state header is exact version 3;
- the retained handoff map is nonempty, at most 1 MiB, arithmetically valid, and wholly below 1 GiB;
- the active stack is nonzero and below 1 GiB;
- `SGDT` reports a nonempty GDT wholly below 1 GiB;
- linked `Windvale_boot_probe` is 4 KiB aligned, at least 2 MiB, and leaves two complete consecutive 2 MiB regions below 1 GiB;
- the allocator returns one aligned contiguous six-page range wholly below 1 GiB; and
- that range does not overlap the executable window.

The image builder separately requires the linked payload to begin at link offset zero and fit the 256 KiB executable window. Allocator pages are already zeroed. A pre-activation failure returns status 1 without loading the candidate root. Activation is accepted only when WVA readback equals the requested root.

## Fixed hierarchy

Version 3 allocates exactly six consecutive 4 KiB pages:

| Relative page | Structure | Rule |
| ---: | --- | --- |
| `0` | PML4 | Entry 0 points to the PDPT; all others absent. |
| `1` | PDPT | Entry 0 points to the page directory; all others absent. |
| `2` | Page directory | Covers low 1 GiB; ordinary entries are 2 MiB writable/NX leaves. |
| `3` | Null-region page table | Directory entry 0; leaf 0 absent, leaves 1 through 511 writable/NX. |
| `4` | First code-region page table | Covers the 2 MiB region containing the boot entry. |
| `5` | Second code-region page table | Covers the immediately following 2 MiB region. |

All present entries are supervisor-only. Ordinary identity leaves are writable and non-executable. Exactly 64 consecutive 4 KiB leaves beginning at `Windvale_boot_probe` are read-only and executable. `CR0.WP` enforces read-only status in supervisor mode, `EFER.NXE` makes NX effective, and no admitted leaf is writable and executable.

The two code tables remain consecutive, so the 256 KiB window may cross one 2 MiB boundary. Code growth beyond 256 KiB is a build failure, not an implicit permission expansion.

## Paging ownership record

After successful CR3 readback, the installer writes this 64-byte little-endian record at memory-state offset `0x80`:

| Offset | Bytes | Field | Version 3 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKPAG03` |
| `0x08` | 4 | Version | `3` |
| `0x0C` | 4 | Record bytes | `64` |
| `0x10` | 8 | Root address | First page of the six-page allocation |
| `0x18` | 8 | Table pages | `6` |
| `0x20` | 8 | Identity bytes | `1,073,741,824` |
| `0x28` | 8 | Executable address | Runtime address of `Windvale_boot_probe` |
| `0x30` | 8 | Executable bytes | `262,144` |
| `0x38` | 8 | Flags | bit 0 NX, bit 1 supervisor write-protect, bit 2 null guard |

The record is evidence of the active kernel root, not a mutable page-map interface.

## Process-root relationship

[Protected process version 4](Windvale-Protected-Process.md) allocates separate init and interpreter PML4/PDPT/page-directory roots after the kernel root is active. Each copies the kernel hierarchy, replaces exactly its process allocation's 2 MiB directory entry with a private page table, and adds user permission only to the required hierarchy path. Init has one RX and two RW/NX user leaves. The interpreter has 32 RX, four RW/NX stack, and one RW/NX context leaf. Kernel memory version 3 aligns the complete arena to 2 MiB so both process extents satisfy this one-private-table rule. The machine switches roots only after every page, descriptor, process record, channel record, and syscall MSR is complete.

The kernel executable window remains supervisor-only in both process roots. When either process exits, blocks, or faults, the current bounded continuation remains mapped and returns to kernel code; version 3 does not yet reclaim or recycle either root.

## WVA privileged operations

WVA 1 supplies two no-operand x86-64-only semantic statements:

- `enable_page_protection` selects EFER, sets NXE, writes EFER, reads CR0, sets WP, and writes CR0. It is exactly 24 bytes and clobbers `RAX`, `RCX`, and `RDX`.
- `activate_page_table` loads `CR3` from `RAX` and reads active `CR3` into `RAX`. It is exactly 6 bytes.

These operations do not expose arbitrary MSR or control-register access. Callers own feature admission, table construction, calling convention, and failure policy.

## Diagnostics and evidence

The host planner reports:

| Code | Meaning |
| --- | --- |
| `WVOS5001` | The six-page table range is null, unaligned, incomplete, or outside low 1 GiB. |
| `WVOS5002` | The executable address is unaligned or cannot use the two admitted code tables. |
| `WVOS5003` | The table allocation overlaps the executable window. |

The current version-3 paging WVO is 1,244 bytes with SHA-256 `1112e7f21b63ee339a2e17211ae9ce96712b446b5039384f9d38e18ff0430acd`; its 851 code bytes have SHA-256 `147de96e836a0e3ea9f3cb0b937742f56c8f45cb9f5c9e57cb06288e0035c398`. Focused tests lock the 64 RX leaves, every other permission, record identity, four imports/relocations, and deterministic repetition.

Decision 0088 retains version-1 WVO and probe-20 identities. Decision 0090 retains the qualified probe-21 composition. Decision 0093 records the cross-host-qualified version-2 probe-24 composition. [Windvale-Os-Boot-Probe.md](Windvale-Os-Boot-Probe.md) records the larger probe-25 images and live candidate evidence.

## Deliberate limits

Version 3 still retains one fixed identity-mapped kernel root and keeps interrupts disabled. It does not selectively unmap firmware/loader ranges, map memory above 1 GiB, handle page faults, release table pages, optimize global or huge pages, support PCID, KASLR, SMP shootdown, copy-on-write, shared memory, or expose a public map API. The two process roots are specified separately and add no general address-space manager, demand paging, teardown, executable-publication API, or scheduler.
