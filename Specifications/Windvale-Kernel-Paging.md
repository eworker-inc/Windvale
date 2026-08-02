# Windvale x86-64 kernel paging

## Status and purpose

Kernel paging version 2 is the current probe-22 candidate. It retains the qualified version-1 six-page low-1-GiB identity hierarchy, null guard, NX enforcement, and supervisor write protection while expanding the fixed kernel executable window from 64 KiB to 128 KiB. It publishes `WVKPAG02`; version-1 ownership records are not accepted under the larger bound.

[Decision 0088](../Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md) owns the qualified version-1 root and probe-20/21 evidence. [Decision 0091](../Documents/Decisions/0091-First-Protected-Windvale-Process.md) owns version 2 and its measured executable-window expansion. Cross-host qualification remains pending, so probe 21 remains the latest cross-host-qualified paging composition.

The kernel root remains a bounded construction foundation, not a general virtual-memory manager. Protected-process version 1 separately derives one process root with three user leaves; it does not mutate the kernel-root contract into a public mapping API.

## Ownership split

- System-profile [`Hello-World.wv`](../Operating-System/Kernel/Hello-World.wv) publishes `paging=owned` only after installation succeeds.
- [`X64-Kernel-Shims.wva`](../Operating-System/Kernel/X64-Kernel-Shims.wva) owns the named privileged operations that enable `EFER.NXE` and `CR0.WP`, load `CR3`, and read `CR3` back.
- The Stage 0 `Windvale_kernel_x64_paging_install` object temporarily validates live ranges and constructs the hierarchy. It is a named replacement seam for future system-profile Windvale memory operations and WVA control flow.
- The C# planner is an independent deterministic oracle for tables, permissions, ownership-record bytes, and rejection cases.

The installer imports `Windvale_boot_probe`, `Windvale_kernel_allocate_pages`, `Windvale_kernel_x64_page_protection_enable`, and `Windvale_kernel_x64_page_table_activate` through four exact relative relocations.

## Admission boundary

The live installer fails before changing control state unless:

- CPUID exposes extended leaf `0x80000001` and NX;
- the memory-state header is exact version 2;
- the retained handoff map is nonempty, at most 1 MiB, arithmetically valid, and wholly below 1 GiB;
- the active stack is nonzero and below 1 GiB;
- `SGDT` reports a nonempty GDT wholly below 1 GiB;
- linked `Windvale_boot_probe` is 4 KiB aligned, at least 2 MiB, and leaves two complete consecutive 2 MiB regions below 1 GiB;
- the allocator returns one aligned contiguous six-page range wholly below 1 GiB; and
- that range does not overlap the executable window.

The image builder separately requires the linked payload to begin at link offset zero and fit the 128 KiB executable window. Allocator pages are already zeroed. A pre-activation failure returns status 1 without loading the candidate root. Activation is accepted only when WVA readback equals the requested root.

## Fixed hierarchy

Version 2 allocates exactly six consecutive 4 KiB pages:

| Relative page | Structure | Rule |
| ---: | --- | --- |
| `0` | PML4 | Entry 0 points to the PDPT; all others absent. |
| `1` | PDPT | Entry 0 points to the page directory; all others absent. |
| `2` | Page directory | Covers low 1 GiB; ordinary entries are 2 MiB writable/NX leaves. |
| `3` | Null-region page table | Directory entry 0; leaf 0 absent, leaves 1 through 511 writable/NX. |
| `4` | First code-region page table | Covers the 2 MiB region containing the boot entry. |
| `5` | Second code-region page table | Covers the immediately following 2 MiB region. |

All present entries are supervisor-only. Ordinary identity leaves are writable and non-executable. Exactly 32 consecutive 4 KiB leaves beginning at `Windvale_boot_probe` are read-only and executable. `CR0.WP` enforces read-only status in supervisor mode, `EFER.NXE` makes NX effective, and no admitted leaf is writable and executable.

The two code tables remain consecutive, so the 128 KiB window may cross one 2 MiB boundary. Code growth beyond 128 KiB is a build failure, not an implicit permission expansion.

## Paging ownership record

After successful CR3 readback, the installer writes this 64-byte little-endian record at memory-state offset `0x80`:

| Offset | Bytes | Field | Version 2 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKPAG02` |
| `0x08` | 4 | Version | `2` |
| `0x0C` | 4 | Record bytes | `64` |
| `0x10` | 8 | Root address | First page of the six-page allocation |
| `0x18` | 8 | Table pages | `6` |
| `0x20` | 8 | Identity bytes | `1,073,741,824` |
| `0x28` | 8 | Executable address | Runtime address of `Windvale_boot_probe` |
| `0x30` | 8 | Executable bytes | `131,072` |
| `0x38` | 8 | Flags | bit 0 NX, bit 1 supervisor write-protect, bit 2 null guard |

The record is evidence of the active kernel root, not a mutable page-map interface.

## Process-root relationship

[Protected process version 1](Windvale-Protected-Process.md) allocates a separate PML4/PDPT/page-directory root after the kernel root is active. It copies the kernel hierarchy, replaces exactly the process allocation's 2 MiB directory entry with a private page table, adds user permission only to the required hierarchy path, and marks exactly three leaves user-accessible. The machine switches to the process root only after every user page, descriptor, process record, and syscall MSR is complete.

The kernel executable window remains supervisor-only in that process root. When the process exits or faults, the current bounded continuation remains mapped and returns to kernel code; version 1 does not yet reclaim or recycle either root.

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

The current version-2 paging WVO is 1,244 bytes with SHA-256 `43bc3a191ebaec3944bb1fa47927e9623341dbb11085ea3c76fbe70b6ca16cb0`; its 851 code bytes have SHA-256 `c77b367b120299f39ca65e4b6955d48ab57408440ad762e3deab17988e01606d`. Focused tests lock the 32 RX leaves, every other permission, record identity, four imports/relocations, and deterministic repetition.

Decision 0088 retains version-1 WVO and probe-20 identities. Decision 0090 retains the qualified probe-21 composition. [Windvale-Os-Boot-Probe.md](Windvale-Os-Boot-Probe.md) records the larger probe-22 images and live candidate evidence.

## Deliberate limits

Version 2 still retains one fixed identity-mapped kernel root and keeps interrupts disabled. It does not selectively unmap firmware/loader ranges, map memory above 1 GiB, handle page faults, release table pages, optimize global or huge pages, support PCID, KASLR, SMP shootdown, copy-on-write, shared memory, or expose a public map API. The one process root is specified separately and adds no general address-space manager, demand paging, teardown, or scheduler.
