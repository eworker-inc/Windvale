# Windvale x86-64 kernel paging

## Status and purpose

Kernel paging version 5 is cross-host qualified with Probe 39 under [Decision 0188](../Documents/Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md). Current paging version 7 retains its seven-page low-1-GiB identity hierarchy, null guard, NX enforcement, supervisor write protection, two-code-table topology, and exact HPET/local-APIC MMIO directory. It expands the supervisor executable window from version 6's 772 KiB to 776 KiB so the boot-linked filesystem constructors remain executable, and it makes the ownership record report that exact mapped span.

The kernel root remains a bounded construction foundation, not a general virtual-memory manager. Protected-process version 17 derives three private roots and manages two exact client resource aliases without turning the kernel record into a public mapping API.

## Ownership split

- System-profile `Hello-World.wv` publishes `paging=owned` only after installation succeeds.
- `X64-Kernel-Shims.wva` owns the named operations that enable `EFER.NXE` and `CR0.WP`, load `CR3`, and read it back. `X64-Timer-Shims.wva` owns the admitted HPET/local-APIC accesses after mapping.
- The Windvale-owned Probe object producer emits the exact checked installer bytes for `Windvale_kernel_x64_paging_install`.
- The C# planner remains an independent oracle for tables, permissions, ownership bytes, and rejection cases.

## Admission boundary

The installer fails before changing control state unless NX is available, the selected memory contract is exact (`WVKMEM17` in Probe 40), the retained handoff map and live stack are bounded below 1 GiB, the GDT is valid, the linked boot entry is aligned and leaves the required two complete 2 MiB code regions, and the fixed allocator returns a non-overlapping seven-page range. The image builder separately requires the base-zero linked executable code to fit 776 KiB.

## Fixed hierarchy

Version 7 allocates exactly seven consecutive 4 KiB pages:

| Relative page | Structure | Rule |
| ---: | --- | --- |
| `0` | PML4 | entry 0 points to PDPT |
| `1` | PDPT | entry 0 points to the low-1-GiB directory; entry 3 points to page 6 |
| `2` | page directory | ordinary low-1-GiB entries are 2 MiB writable/NX leaves |
| `3` | null-region page table | page zero absent; leaves 1 through 511 writable/NX |
| `4` | first code-region page table | covers the boot entry's 2 MiB region |
| `5` | second code-region page table | covers the immediately following 2 MiB region |
| `6` | timer-MMIO page directory | entries 502 and 503 map the `0xFEC00000` and `0xFEE00000` 2 MiB windows |

All present entries are supervisor-only. Ordinary low-1-GiB identity leaves are writable and non-executable. Exactly 194 consecutive 4 KiB leaves beginning at `Windvale_boot_probe` are read-only and executable. The two timer windows are supervisor RW/NX 2 MiB leaves with page-write-through and cache-disable set. HPET at `0xFED00000` lies inside the first window; local APIC at `0xFEE00000` begins the second. Every private process root copies the kernel PDPT, so the same immutable supervisor-only MMIO directory remains present without exposing either window to CPL3. `CR0.WP` and `EFER.NXE` enforce W^X. Growth beyond 776 KiB is a build failure, not an implicit permission expansion.

## Paging ownership record

After exact CR3 readback, the installer writes this 64-byte little-endian record at state offset `0x80`:

| Offset | Bytes | Field | Version-7 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVKPAG07` |
| `0x08` | 4 | Version | `7` |
| `0x0C` | 4 | Record bytes | `64` |
| `0x10` | 8 | Root address | first page of the seven-page allocation |
| `0x18` | 8 | Table pages | `7` |
| `0x20` | 8 | Identity bytes | `1,073,741,824` |
| `0x28` | 8 | Executable address | runtime boot-entry address |
| `0x30` | 8 | Executable bytes | `794,624` |
| `0x38` | 8 | Flags | NX, supervisor write-protect, null guard, timer MMIO |

The record is evidence of the active root, not a mutable page-map interface.

## Process-root relationship

Protected process 17 copies the supervisor hierarchy, replaces exactly the process allocation's low-1-GiB directory entry with a private user page table, and marks only required leaves user-accessible. Init retains two RX, two RW/NX, and three owned RO/NX leaves. Directory retains two RX, two RW/NX, and one owned RO/NX leaf. Each interpreter generation begins with 110 RX, seven RW/NX, and two absent resource targets. Atomic grant installs two RO/NX aliases; cleanup clears them before the kernel reloads init's CR3. Generation 1's 122-page extent is then zeroed, released, and rebuilt at the same physical root for generation 2. The shared timer-MMIO directory remains supervisor-only in every root.

The kernel executable window remains supervisor-only in every process root. One CPU and non-global mappings make the retained CR3 reload the current translation-flush boundary.

## WVA privileged operations

WVA retains `enable_page_protection` (24 bytes, exact NXE/WP operations) and `activate_page_table` (6 bytes, load/readback CR3). Probe 39 also uses exact `cpuid` and `read_msr` operations to validate architectural APIC support and `IA32_APIC_BASE`; the owning timer contract fixes the leaf/MSR selection and rejects unsupported state. These operations do not expose a general source-level mapping, MSR, or control-register API.

## Deterministic qualified artifact and limits

Paging WVO version 7 is 1,292 bytes with SHA-256 `a76c4a199d46f6d91c0d3cd76aec7439a5e3fa72403cfc753fa6c36cd5b9b871`. Eleven focused object-producer cases and all four current Probe 40 build/QEMU cases pass on Windows; independent Windows/Linux qualification remains pending. Version 7 also corrects the stale version-6 ownership-record literal, which reported 768 KiB even though version 6 mapped 772 KiB.

Version 7 still retains one fixed identity-mapped kernel root plus two exact timer-MMIO windows. It does not map arbitrary addresses above 1 GiB, handle page faults generally, expose a map API, support arbitrary devices, optimize huge/global pages, support PCID, KASLR, SMP shootdown, copy-on-write, shared memory, demand paging, or executable publication.
