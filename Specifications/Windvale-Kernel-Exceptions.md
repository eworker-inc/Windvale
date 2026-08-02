# Windvale x86-64 kernel CPU exceptions

## Status and purpose

Kernel CPU exceptions version 1 remains cross-host qualified for firmware probe 17 at exact commit `ba2cf69cd4a97876f5e953b3938d032fc75a8ff7`. [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) records its one kernel-owned terminal invalid-opcode destination.

Version 2 is cross-host qualified at exact commit `12e9e2e` through the pre-paging firmware probe-20 baseline and retained unchanged through qualified probe 26 and candidate probe 27. It retains vector 6, adds general protection vector 13, and moves both entry-normalization stubs into WVA while one bounded Stage 0 object still owns descriptor publication and terminal policy. [Decision 0086](../Documents/Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) and [kernel trap frame version 1](Windvale-Kernel-Trap-Frame.md) own the boundary. Probes 22 through 27 add a process-private IDT extension for vectors 6, 13, and 14 plus CPL3 containment without changing the qualified ring-0 terminal contract.

CPU exceptions remain distinct from Windvale runtime traps such as `WVR3007`. Runtime traps are checked semantic results and do not raise processor faults.

## Ownership and installation

Kernel memory version 1 dedicates its first zeroed 4 KiB allocation to the exception table for the remainder of the probe. After switching to the two-page kernel stack and before entering the existing Main chain, the x86-64 exception installer:

1. requires a non-null 4 KiB-aligned page address;
2. clears the complete page so no stale gate can become present;
3. reads and validates the live ring-0 `CS` selector;
4. derives the complete linked WVA entry addresses through typed `relative-i32` relocations;
5. writes interrupt gates for vector 6 at byte offset 96 and vector 13 at byte offset 208;
6. writes the ten-byte IDTR operand at page offset 224 with base equal to the page and limit 223;
7. disables maskable interrupts with `CLI`; and
8. publishes the table with `LIDT`.

The IDT contains exactly 14 addressable 16-byte entries. Vectors other than 6 and 13 are zero and not present. The IDTR operand is outside the admitted table limit, and bytes 234 through 4095 remain zero. Maskable interrupts stay disabled; this candidate does not install an interrupt controller or claim absent vectors are safely handled.

## Gate shape

Both 16-byte little-endian descriptors have this logical shape:

| Bytes | Field | Version 2 rule |
| ---: | --- | --- |
| `0..1` | Handler offset `15..0` | Low WVA-entry address bits |
| `2..3` | Segment selector | Live ring-0 `CS`, nonzero |
| `4` | IST/reserved | Zero; no IST stack |
| `5` | Type and attributes | `0x8E`: present, DPL 0, interrupt gate |
| `6..7` | Handler offset `31..16` | Middle WVA-entry address bits |
| `8..11` | Handler offset `63..32` | High WVA-entry address bits |
| `12..15` | Reserved | Zero |

Reconstructing the three offset fields yields the exact linked WVA entry target. No gate targets the common handler directly: its input is valid only after the vector-specific stub has normalized the stack.

## WVA-owned entry normalization

`Operating-System/Kernel/X64-Kernel-Shims.wva` exports:

- `Windvale_kernel_x64_exception_6_entry`, which pushes synthetic error code 0, pushes vector 6, and jumps to the common terminal handler; and
- `Windvale_kernel_x64_exception_13_entry`, which preserves the CPU-pushed error code, pushes vector 13, and jumps to the same handler.

Both use WVA `push_i32`, which creates one sign-extended 64-bit stack cell in x86-64 mode. The resulting 40-byte same-privilege record is defined by [Windvale-Kernel-Trap-Frame.md](Windvale-Kernel-Trap-Frame.md). WVA owns the architecture-dependent normalization bytes; Stage 0 assembles, verifies, links, and packages them.

## Process-private extension

[Protected process version 6](Windvale-Protected-Process.md) reuses the zeroed IDT page while either fixed CPL3 thread is active. Its machine seam writes DPL-0 interrupt gates for vectors 6, 13, and 14, stores the ten-byte IDTR operand at page offset 240 with limit 239, and loads a private GDT/TSS before entering user mode. The TSS supplies the saved kernel stack on privilege-changing exception delivery.

WVA exports process-specific normalization stubs for invalid opcode, general protection, and page fault. Vector 6 pushes synthetic error 0 and vector 6; vectors 13 and 14 preserve their CPU error cell and push the vector. A privilege-changing CPU frame includes the interrupted user `RSP` and `SS`, so the normalized record is 56 bytes as described by the trap-frame contract.

The process common entry inspects the saved `CS`. A CPL0 origin tail-transfers unchanged to the qualified terminal handler. A CPL3 origin records vector/error and returns to the saved kernel continuation with the process and thread faulted. Probe 27 retains deterministic interpreter-process `CLI` delivery as `(13, 0)` after the init grant, interpretation, and one valid send, then continues by waking init; vector 6 and vector 14 are structurally installed but are not yet claimed as live user-fault evidence.

## Deterministic scenarios

Probe 27 supports four explicit construction scenarios after page-table activation, WVB admission, protected-process construction, and the init-owned resource grant:

- `normal` completes init resource selection/grant, init block, bytecode interpretation, client send/exit, init wake/receive/exit, both later Main paths, success markers, and the WVA Q35 shutdown adapter;
- `invalid-opcode` executes `UD2`, proving delivery of vector 6 without a CPU error code; and
- `general-protection` dereferences `0x0100000000000000`, an address noncanonical under both four- and five-level x86-64 paging, proving terminal kernel delivery of vector 13 with CPU error code 0 before translation; and
- `user-fault` lets the client send, executes privileged `CLI`, records `(13, 0)` against that client, wakes the independent init service, and emits `user-fault=contained` after service completion.

The scenario is fixed when the image is built. It is not inferred from timing, firmware behavior, mutable guest input, or host state. Instructions after either deliberate fault only select failure if the processor incorrectly resumes.

## Terminal handler

The common handler validates the normalized vector/error pair and emits one exact ASCII/LF suffix through the existing polled COM1 boundary:

```text
panic=invalid-opcode
vector=6
error-code=0
status=panic
```

or:

```text
panic=general-protection
vector=13
error-code=0
status=panic
```

Any other pair emits `panic=malformed-exception-frame` followed by `status=panic`. Every branch then writes value 1 to QEMU test port `0xF4`, producing host exit code 3 under the accepted test device. If that device does not complete the guest, the handler enters a CLI/HLT loop forever. It does not allocate, call firmware, invoke runtime services, return, resume, unwind, or execute `IRETQ`.

The complete scenario-specific panic suffix and host code 3 are both required evidence. No normal success or shutdown marker may follow a panic.

## Construction, validation, and evidence

The Stage 0 object exports the installer and common terminal handler, and imports the two WVA entry symbols. Its typed relocations are the only address transfer from descriptor construction to WVA. Before PE publication, focused tests lock:

- the exact WVA object architecture, code, symbols, definition offsets/sizes, `push_i32` bytes, and five relocations;
- the exception object's architecture, code section, exports, imports, two relocation fields/addends, and exact object/code identities;
- full-page clearing, live `CS` capture, both complete gate stores, IDTR offset/limit, `CLI`, and `LIDT`;
- normalized frame offsets and both terminal marker writers; and
- absence of `UD2` and `IRETQ` from the exception object itself.

The qualified pre-process seam records a 620-byte WVA object with SHA-256 `4bb07c28877905de6e57d79454e33402de4ac54048d5ce09a26b49ad0d8347a5`; a 4,667-byte exception WVO with SHA-256 `49f15606d2cd41236f87e8a7a7e24a9532683ffe9d5a59795dc8084288b2f84a`; and 4,348 code bytes with SHA-256 `9307e2e9e4471d15448326ab2a86464f652fadfe60901226ef32b02e4dc9f8b9`. The installer occupies 222 bytes and the common terminal handler begins at aligned offset 224. Probe 22's expanded WVA seam is 1,123 bytes with SHA-256 `8a6f54950f15c7331107a5bfa7bd2d863f64b25d395b7cfd9983c31130599363`; the separate terminal exception object remains byte-for-byte unchanged.

Real pinned QEMU 11.0/Q35/TCG execution passes all three scenarios: normal exits cleanly with host code 0, and both fault paths emit their exact normalized records and exit with expected host code 3. Static fixtures cannot substitute for live `CS`, `LIDT`, processor exception delivery, or the CPU-pushed vector-13 error cell.

## Limits

The qualified version-2 terminal contract still provides no page-fault `CR2`, double-fault containment, IST, NMI, IRQ, PIC/APIC, interrupt enablement, register-save frame, nested-fault policy, `IRETQ`, recovery, unwinding, SMP, scheduler integration, or WVR-to-CPU mapping. Probe 27's process-private extension retains one TSS and bounded interpreter-process fault containment but no general dispatcher, page-fault policy, resumption, signal model, or process-facing exception ABI. The shared terminal handler remains Stage 0 C#-emitted machine code because current WVA lacks comparisons, conditional branches, memory reads, and its polled serial loop; system-profile `.wv` policy still requires bounded unsafe memory plus a specified kernel call convention.
