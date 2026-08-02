# Windvale x86-64 kernel paging

## Status and purpose

Kernel paging version 4 is the implemented Probe-31 candidate. It retains the six-page low-1-GiB identity hierarchy, null guard, NX enforcement, supervisor write protection, and two-code-table topology while expanding the fixed supervisor executable window from 256 KiB to 768 KiB. [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) owns version 4; [Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) retains the qualified version-3 history.

The kernel root remains a bounded construction foundation, not a general virtual-memory manager. Protected-process version 10 derives two private roots and manages two exact resource aliases without turning the kernel record into a public mapping API.

## Ownership split

- System-profile `Hello-World.wv` publishes `paging=owned` only after installation succeeds.
- `X64-Kernel-Shims.wva` owns the named operations that enable `EFER.NXE` and `CR0.WP`, load `CR3`, and read it back.
- Stage 0 temporarily validates live ranges and constructs the hierarchy through `Windvale_kernel_x64_paging_install`.
- The C# planner remains an independent oracle for tables, permissions, ownership bytes, and rejection cases.

## Admission boundary

The installer fails before changing control state unless NX is available, `WVKMEM08` is exact, the retained handoff map and live stack are bounded below 1 GiB, the GDT is valid, the linked boot entry is aligned and leaves the required two complete 2 MiB code regions, and the allocator returns a non-overlapping six-page range. The image builder separately requires the base-zero linked payload to fit 768 KiB.

## Fixed hierarchy

Version 4 allocates exactly six consecutive 4 KiB pages:

| Relative page | Structure | Rule |
| ---: | --- | --- |
| `0` | PML4 | entry 0 points to PDPT |
| `1` | PDPT | entry 0 points to page directory |
| `2` | page directory | ordinary low-1-GiB entries are 2 MiB writable/NX leaves |
| `3` | null-region page table | page zero absent; leaves 1 through 511 writable/NX |
| `4` | first code-region page table | covers the boot entry's 2 MiB region |
| `5` | second code-region page table | covers the immediately following 2 MiB region |

All present entries are supervisor-only. Ordinary identity leaves are writable and non-executable. Exactly 192 consecutive 4 KiB leaves beginning at `Windvale_boot_probe` are read-only and executable. `CR0.WP` and `EFER.NXE` enforce W^X. Growth beyond 768 KiB is a build failure, not an implicit permission expansion.

## Paging ownership record

After exact CR3 readback, the installer writes this 64-byte little-endian record at state offset `0x80`:

| Offset | Bytes | Field | Version-4 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKPAG04` |
| `0x08` | 4 | Version | `4` |
| `0x0C` | 4 | Record bytes | `64` |
| `0x10` | 8 | Root address | first page of the six-page allocation |
| `0x18` | 8 | Table pages | `6` |
| `0x20` | 8 | Identity bytes | `1,073,741,824` |
| `0x28` | 8 | Executable address | runtime boot-entry address |
| `0x30` | 8 | Executable bytes | `786,432` |
| `0x38` | 8 | Flags | NX, supervisor write-protect, null guard |

The record is evidence of the active root, not a mutable page-map interface.

## Process-root relationship

Protected process 10 copies the supervisor hierarchy, replaces exactly the process allocation's 2 MiB directory entry with a private user page table, and marks only required leaves user-accessible. Init retains one RX, two RW/NX, and two owned RO/NX leaves. Each interpreter generation begins with 98 RX, 13 RW/NX stack, one RW/NX context leaf, and two absent resource targets. Atomic grant installs two RO/NX aliases; cleanup clears them before the kernel reloads init's CR3. Generation 1's 116-page extent is then zeroed, released, and rebuilt at the same physical root for generation 2.

The kernel executable window remains supervisor-only in every process root. One CPU and non-global mappings make the retained CR3 reload the current translation-flush boundary.

## WVA privileged operations

WVA retains `enable_page_protection` (24 bytes, exact NXE/WP operations) and `activate_page_table` (6 bytes, load/readback CR3). These do not expose general MSR or control-register access.

## Deterministic candidate and limits

Paging WVO version 4 is 1,244 bytes with SHA-256 `c19ba2445452b314478c0979e5cd295c2fed1c482c08c95a1452c8f6ed2c06d1`; its 851 code bytes have SHA-256 `1401a0b5a0681d62d5505c4e64e4df713ff838c4a259fcb16dbcd0f573714540`.

Version 4 still retains one fixed identity-mapped kernel root with interrupts disabled. It does not map above 1 GiB, handle page faults generally, expose a map API, optimize huge/global pages, support PCID, KASLR, SMP shootdown, copy-on-write, shared memory, demand paging, executable publication, or scheduling.
