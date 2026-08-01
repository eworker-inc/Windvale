# Decision 0088: First kernel-owned x86-64 page tables

- Date: 2026-08-01
- Status: Accepted and implemented candidate; cross-host qualification pending
- Implements: The page-table-ownership part of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Extends: [Decision 0052](0052-First-Kernel-Owned-Memory-Foundation.md) and qualified [Decision 0086](0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md)
- Contract: [Kernel paging version 1](../../Specifications/Windvale-Kernel-Paging.md)

## Context

Firmware probe 19 owns a bounded arena, stack, IDT, normalized vector-6/vector-13 entries, deterministic panic, and Q35 shutdown, but it continues under page tables inherited from firmware. That leaves a fundamental machine resource outside the kernel's explicit ownership and prevents meaningful W^X or null-page policy.

Decision 0084 assigns durable policy to system-profile Windvale, irreducible architecture mechanics to WVA, and temporary bounded construction to named Stage 0 seams. Current `.wv` cannot yet populate arbitrary 64-bit memory records or perform the bounded loops needed for page tables. WVA also lacks the conditional control flow and memory addressing for the complete installer. A small, independently checked construction seam is therefore preferable to a general raw-privilege instruction surface or a premature language expansion.

## Decision

- Define kernel paging version 1 as one six-page, four-level x86-64 hierarchy that identity maps the low 1 GiB.
- Leave virtual page zero absent. Map ordinary memory supervisor-writable/NX. Narrow exactly one fixed 64 KiB boot-image window to supervisor read-only/executable and enforce it with `EFER.NXE` plus `CR0.WP`. Admit no writable-executable leaf.
- Require the live stack, retained handoff map, GDT, table allocation, and executable window to remain inside the new identity map. Require CPUID NX support and exact checked ranges before changing control state.
- Extend WVA 1 with only `enable_page_protection` and `activate_page_table`. The first owns the exact NXE/WP sequence; the second loads and reads back CR3. Do not expose generic MSR/control-register reads or writes.
- Advance the kernel WVA seam to version 6 with named wrappers for both operations. Keep table construction and live-range validation in Stage 0 export `Windvale_kernel_x64_paging_install` behind an explicit future `.wv`/WVA replacement seam.
- Allocate the six tables through `Windvale_kernel_allocate_pages` after the existing one-page IDT allocation. Write a versioned `WVKPAG01` ownership record at memory-state offset `0x80` only after CR3 readback matches.
- Advance the firmware probe to version 20. Compiler-generated system-profile Windvale prints `paging=owned` only after the installer returns success. Retain the normal shutdown and both post-activation terminal-fault scenarios.
- Preserve kernel memory version 1, handoff version 1, exception version 2, trap-frame version 1, shutdown version 1, native ABI 15/context 7, WVB 1.6, WVO 1.0, and UEFI application format version 3.
- Do not claim a virtual-memory manager, process isolation, page-fault handling, reclamation, user mode, mappings above 1 GiB, or a public page-map API.

## Candidate evidence

Local Windows evidence records:

- all 20 OS tests passing, including exact hierarchy permissions, null guard, W^X checks, malformed planning inputs, the paging WVO, and all three reproducible firmware images;
- all 6 focused assembler tests passing with byte-identical C#/Windvale encoding and bounded operand rejection;
- Windvale assembler WVB SHA-256 `e32d237127b07de73a639f47292c7cfeb3f7cb88f233c107ad3f852d9781d03b`;
- a 773-byte WVA shim WVO with SHA-256 `5c4b0bcfa1c6463ebbe631562deb7714aa510dfbc2418b1544b0df6c8df6bedb`;
- a 1,244-byte paging WVO with SHA-256 `deeebe592b38890c9964cc4d9736b1d617c0d6b20bed494ba533dcb9b1d4f318` and exact 851-byte code SHA-256 `12cbb64dad4558f94fd7075995cb5ac8a788ed5476999d14d2a585b310021678`;
- a 22,016-byte normal image with SHA-256 `392a2801bd8d8895bd9c34213336a69057c1ae81675269056c60b8c3e974ab01`, complete `paging=owned` transcript, and pinned-QEMU exit code 0;
- a 22,016-byte invalid-opcode image with SHA-256 `aa610e6ac00ed43466a87521bb4cebb2934d0885acb960db8913f025ced9cce9`, normalized `(6, 0)`, and exit code 3; and
- a 22,016-byte general-protection image with SHA-256 `74632fcde4873f2d46e18b1b77c5cc8b495e83f0f750930e039da27dd67cd0ee`, normalized `(13, 0)`, and exit code 3.

The normal path executes the portable ABI-15 WVB-derived object, compiler-generated `.wv` Main, and WVA shutdown after activation. The two fault scenarios prove the WVA entries and Stage 0 terminal policy remain reachable through the new root. This is candidate evidence only; complete cross-host and independent CI qualification remain pending.

## Consequences

Windvale no longer relies on firmware's page-table root after the bounded transition. The first real kernel memory-protection policy is now observable: null is absent, ordinary pages are NX, and the admitted boot window is read-only/executable under supervisor write protection.

The construction emitter remains C# Stage 0 and is therefore intentionally temporary. Its named export, four imports, independent table oracle, exact object identity, and WVA-owned privileged calls make the replacement boundary explicit. Moving the constructor into `.wv` requires checked 64-bit integers, bounded unsafe memory operations, and enough control flow; moving more of it into WVA requires equivalent semantic validation rather than raw bytes.

The fixed low-1-GiB identity map is not the future process layout. It is a coherent bridge to the next Decision 0084 slice: boot an AOT Windvale decoder/verifier and validate one embedded canonical WVB inside the guest. Process-specific address spaces and page-fault policy come later.

## Reconsider when

- the first in-guest verifier or runtime requires an admitted range outside the fixed window;
- page-fault evidence requires `CR2`, richer trap state, or recovery policy;
- process isolation requires per-process roots, user permissions, kernel/global mappings, or TLB invalidation;
- physical-memory ownership expands beyond the bounded arena and low-1-GiB admission; or
- system-profile Windvale and WVA can replace the Stage 0 constructor while preserving the exact validation and evidence boundary.
