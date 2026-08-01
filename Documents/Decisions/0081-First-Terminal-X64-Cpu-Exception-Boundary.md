# Decision 0081: First terminal x86-64 CPU exception boundary

- Date: 2026-08-01
- Status: Accepted, implemented, and cross-host qualified
- Extends: [Decision 0052](0052-First-Kernel-Owned-Memory-Foundation.md)'s first allocation and [Decision 0056](0056-Windvale-Owned-Post-Memory-Evidence.md)'s kernel machine seam
- Preserves: Kernel handoff version 1, kernel memory version 1, native ABI 14, execution-context version 6, WVB 1.6, WVO 1.0, and UEFI application format version 3

## Context

Firmware probe 16 exits UEFI boot services, claims and clears one kernel-owned arena, switches to an owned stack, and runs compiler-generated Windvale code. It deliberately installs no interrupt descriptor table owned by the kernel. A CPU exception after firmware exit can therefore depend on stale firmware state or terminate without deterministic Windvale diagnostics.

The first exception boundary should prove one exact architectural transition without presenting a partial interrupt subsystem as a general kernel facility. Invalid opcode (`#UD`, vector 6) is synchronous, has no CPU error code, and can be triggered deterministically with `UD2` after the existing kernel computation. Page faults, double faults, interrupts, return from an exception, and language-runtime traps require distinct state and policy.

## Decision

- Define [kernel CPU exceptions version 1](../../Specifications/Windvale-Kernel-Exceptions.md) for x86-64 invalid opcode only.
- Advance the firmware probe to version 17. The accepted candidate has an ordinary success image and an explicit `InvalidOpcode` evidence image selected as a build input. This selection does not change portable Windvale source, WVB, native ABI, handoff, or PE32+ format.
- Use the first successful one-page allocation from kernel memory version 1 as the 4 KiB exception-table page. Preserve its address in the existing first-allocation field; do not add another memory-state field or allocation.
- Disable maskable interrupts before publication. Clear the page and construct a seven-entry, 112-byte IDT whose entries 0 through 5 remain absent and whose only present entry is vector 6.
- Construct the vector-6 interrupt gate from the live `CS` selector and the complete 64-bit address of the linked terminal handler. Require IST zero, gate type `0xE`, DPL zero, present one, and every reserved bit zero. Load an IDTR whose base is the allocated page and whose limit is exactly 111.
- Install the IDT on the kernel-owned stack before invoking the existing WVA, portable-WVB, and special system-profile Main chain. In the ordinary image, return from installation and preserve the existing computation. In the `InvalidOpcode` image, execute exactly one `UD2` after Main returns and before control returns to the loader.
- Make the vector-6 handler terminal. It enters through an interrupt gate after the probe has disabled maskable interrupts, emits these exact ASCII/LF lines through COM1, writes value 1 to QEMU test port `0xF4`, and otherwise remains in a CLI/HLT loop:

```text
panic=invalid-opcode
vector=6
error-code=none
status=panic
```

- Do not execute `IRETQ`, inspect or modify the processor-pushed frame, allocate memory, call firmware, invoke Windvale code, or attempt recovery from the handler.
- In the ordinary image, the loader emits `cpu-exceptions=armed` after Hello World and before its existing `native-context=pass` line. Reaching that marker proves the installation and both Main paths returned normally. The invalid-opcode image must reach the exact panic suffix instead and must not emit `cpu-exceptions=armed`, `native-context=pass`, `native-wvb=pass`, `windvale-source=pass`, or `status=pass` afterward.
- Treat QEMU `isa-debug-exit` host code 3 plus the complete panic suffix as expected negative evidence only for the explicit invalid-opcode scenario. Host code 3 alone is not evidence. The ordinary success image retains value 0 and host code 1.
- Keep the privileged mechanics in one bounded Stage 0 x86-64 object with an independently checked shape and a named replacement seam. Current WVA 1 cannot express `CLI`, `LIDT`, live `CS` capture, the IDT memory writes, or the complete terminal handler; later WVA owns those mechanics, while `.wv` owns exception policy after the native system subset can express it safely.

## Evidence contract

- Candidate acceptance requires the WVO reader and focused host test to lock the exception object's architecture, section, symbols, absence of relocations, complete code identity, installer/handler bounds, page-clear operation, live-selector capture, RIP-relative handler derivation, vector-6 field stores, exact IDTR limit, `CLI`/`LIDT`, panic writer, debug exit, and absence of `UD2` or `IRETQ` inside the object.
- The installer accepts no serialized or caller-shaped gate. Its only runtime input is the already-owned first allocation; null or non-page-aligned addresses fail before the page is cleared or the IDTR is published. Kernel memory's existing below-4-GiB arena contract proves the complete page range. The linked RIP-relative handler address and live `CS` are exercised by the real firmware run rather than guessed by a host-side descriptor fixture.
- Two builds of each scenario must be byte-identical. The ordinary image must retain the complete success transcript with the added armed marker. The invalid-opcode image must execute after firmware exit and Main completion, terminate through the exact panic transcript and host code 3, and complete without harness timeout.
- Candidate evidence may be recorded locally. Cross-host qualification, pinned-QEMU artifact identities, exact timings, and final test counts require a later committed qualification record.

## Qualification

Exact commit `ba2cf69cd4a97876f5e953b3938d032fc75a8ff7`, tree `7ca8f6fdbf5a7caff8300ad4b1dbd490295930d6`, was archived as 3,014,724 bytes with SHA-256 `ef6892d6c37cdc5e1461e66c8f2d5b72d7514f847078950220124b92690daa3c`. The same archive passed zero-warning Qualification on Windows and Debian GNU/Linux 12 x64 with all 63 Seed tests, exact compiler/retained-artifact reproduction, the complete native CLI gate, and all 17 OS tests. Complete Qualification took 485.476 seconds on Windows and 495.249 seconds on Debian; suite time was 241.821 and 247.813 seconds.

Pinned QEMU 11.0/Q35/TCG boots both exact 17,920-byte probe-17 images. The ordinary image has SHA-256 `d2c0a7e4e5e1605fc8639c05ab27ad07ee2b015ad2dc151d8637830b8acb3f18`, emits the complete success transcript including `cpu-exceptions=armed`, and returns guest-controlled host exit code 1. The explicit invalid-opcode image has SHA-256 `26ccfaf862024e022339ca9fa8114c71b4fe601fe59a806d366e1d330b6d106d`, emits the exact terminal panic transcript, emits no later success marker, and returns expected host code 3 without timeout. GitHub [Verify run 30715672194](https://github.com/eworker-inc/Windvale/actions/runs/30715672194) independently passes Windows and Linux verification for the implementation commit.

This qualifies one terminal vector-6 boundary only. It does not qualify recovery, another exception vector, a normalized trap dispatcher, page-fault state, IST/TSS containment, hardware interrupts, clean shutdown, or Hyper-V.

## Consequences and limits

Windvale OS now has a bounded kernel-owned CPU exception destination and deterministic terminal evidence for one real processor fault. It does not yet have a general trap dispatcher or interrupt subsystem.

Version 1 does not claim any other exception vector or CPU error-code frame, page-fault handling or `CR2`, double-fault containment, a TSS or IST stack, NMI, IRQ, PIC/APIC configuration, interrupt enablement, exception recovery, `IRETQ`, unwinding, processes, user mode, SMP, or scheduler integration. It does not map Windvale `WVR` runtime traps to CPU exceptions; those remain verified packed status results. Hyper-V, physical hardware, and clean platform shutdown remain separate milestones.

QEMU debug exit is test completion, not platform shutdown. Clean shutdown remains a later bounded Phase 11 decision so a successful lifecycle transition is not confused with expected panic termination.

## Reconsider when

- A second CPU exception requires normalized error-code frames or resumable dispatch.
- Page-table ownership makes page-fault reporting and `CR2` meaningful.
- Double-fault containment requires a TSS and independently owned IST stack.
- Hardware interrupts require controller initialization and a policy for enabling `IF`.
- WVA and system-profile Windvale can replace the temporary Stage 0 machine object without weakening exact verification.
