# Decision 0086: First WVA-owned normalized x86-64 trap entries

- Date: 2026-08-01
- Status: Accepted and implemented candidate; cross-host qualification pending
- Implements: The normalized-exception-frame part of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Extends: [Decision 0081](0081-First-Terminal-X64-Cpu-Exception-Boundary.md) and candidate [Decision 0085](0085-First-Wva-Owned-Q35-Clean-Shutdown.md)
- Contracts: [Kernel CPU exceptions version 2](../../Specifications/Windvale-Kernel-Exceptions.md) and [kernel trap frame version 1](../../Specifications/Windvale-Kernel-Trap-Frame.md)

## Context

Qualified probe 17 proves one terminal invalid-opcode destination, but its handler is a vector-specific C# Stage 0 byte sequence and `#UD` has no CPU error code. Decision 0084 calls for normalized essential traps and assigns irreducible architecture mechanics to WVA. A second vector must therefore prove that entries with and without CPU error codes can reach one explicit frame contract without prematurely designing recovery, saved-register state, page-fault policy, or a general dispatcher.

WVA already owns the bidirectional kernel shims and Q35 shutdown adapter, but cannot create a normalized stack cell. The smallest missing operation is fixed-width immediate push, not a raw-byte directive or general memory language.

## Decision

- Extend WVA 1 with `push_i32 <i32>`, encoded as `68 imm32`. In x86-64 mode it decrements `RSP` by eight and stores the sign-extended immediate in one 64-bit cell. Implement identical parsing, bounds, encoding, and rejection behavior in the C# reference/recovery assembler and Windvale-written assembler.
- Advance the executable kernel WVA seam to version 5. Export vector-6 and vector-13 entry stubs, and import one common terminal handler. Vector 6 pushes synthetic error 0 then vector 6; vector 13 retains the CPU error cell and pushes only vector 13. Both tail-jump to the common handler.
- Define kernel trap frame version 1 as the same-privilege 40-byte prefix at `RSP`: vector at 0, error code at 8, interrupted `RIP` at 16, `CS` at 24, and `RFLAGS` at 32.
- Advance kernel CPU exceptions to version 2. Retain the first allocated 4 KiB page, install present interrupt gates at vector offsets 96 and 208, place the IDTR operand at offset 224 with limit 223, keep interrupts disabled, and leave all other gates absent.
- Keep IDT publication and terminal serial policy in one independently verified Stage 0 object behind named replacement seams. It imports the WVA entries rather than embedding their machine bytes. Moving those remaining responsibilities requires explicit WVA memory/control-flow mechanics or a bounded system-profile `.wv` kernel ABI.
- Advance the firmware probe to version 19 with explicit `normal`, `invalid-opcode`, and `general-protection` construction scenarios. The general-protection image dereferences address `0x0100000000000000`, which is noncanonical under both accepted x86-64 paging widths and therefore produces `#GP(0)` before translation.
- Require exact panic markers containing numeric `error-code=0` for both faults, exact host exit code 3, absence of the other fault marker, and absence of later success. Preserve the normal WVA-owned Q35 poweroff path and host exit code 0.
- Do not define recovery, `IRETQ`, a complete saved-register record, nested-fault policy, user-mode delivery, interrupt routing, or a mapping from Windvale runtime traps to CPU faults.

## Candidate evidence

Local Windows evidence records:

- all 18 focused OS tests passing;
- all 6 focused assembler tests passing with byte-identical C#/Windvale `push_i32` output and bounded rejection cases;
- a 99,102-byte Windvale assembler WVB with SHA-256 `e1869d1ca62196328d0311fb0c42dc8789e00f2a90e041db2872e155128f4173`;
- a 620-byte WVA shim WVO with SHA-256 `4bb07c28877905de6e57d79454e33402de4ac54048d5ce09a26b49ad0d8347a5`;
- a 4,667-byte exception WVO with SHA-256 `49f15606d2cd41236f87e8a7a7e24a9532683ffe9d5a59795dc8084288b2f84a` and 4,348 code bytes with SHA-256 `9307e2e9e4471d15448326ab2a86464f652fadfe60901226ef32b02e4dc9f8b9`;
- an exact 20,992-byte normal image with SHA-256 `4f3566379fd55aa1707ac6182c5ec0dee176b35b5e17f8b74687374eb995de0f`, complete shutdown marker, and pinned-QEMU exit code 0;
- an exact 20,992-byte invalid-opcode image with SHA-256 `23ff09ee1d7b0fb20d770edbb76a7acb0bfc3b7a9b3c88571644092dc88ca9f2`, normalized `(6, 0)` marker, and expected exit code 3; and
- an exact 20,992-byte general-protection image with SHA-256 `75b48804f0803a5c747158ed77a71942237d376af48d0276933c69c44ef60562`, normalized `(13, 0)` marker, and expected exit code 3.

This is candidate evidence only. Windows/Debian archive identity, complete Seed qualification, normalized reports, portable-artifact comparison, and independent GitHub verification remain required before cross-host status changes.

## Consequences and limits

The machine layer now owns the first reusable distinction between exceptions that do and do not push CPU error codes. Descriptor construction and terminal policy no longer need one bespoke entry per vector, and another vector can be evaluated against an explicit frame contract rather than accidental stack layout.

This remains a terminal ring-0 probe. It is not yet a general trap dispatcher, interrupt subsystem, safe exception recovery path, or `.wv` kernel policy implementation. The IDT still has no containment for uninstalled vectors or faults raised inside the handler. The first allocation remains a fixed exception-table page with no reclamation.

The WVA extension is narrow but changes the composed Windvale assembler identity, so cross-host reproduction is mandatory. It does not change portable Windvale source, WVB semantics, ABI 14/context 6, the portable kernel module, handoff version 1, kernel memory version 1, shutdown version 1, WVO 1.0, or UEFI application format version 3.

## Reconsider when

- a page-fault slice needs `CR2` and a richer vector-specific prefix;
- double-fault containment requires a TSS and IST-owned stack;
- resumable dispatch requires saved registers, alignment, restoration, and `IRETQ`;
- process isolation requires privilege-transition `RSP`/`SS` fields and user-fault policy;
- interrupt-controller work requires IRQ acknowledgement, routing, nesting, or interrupt enablement; or
- WVA and system-profile `.wv` can replace the remaining Stage 0 descriptor and terminal-policy object behind an equally explicit contract.
