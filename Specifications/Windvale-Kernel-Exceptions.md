# Windvale x86-64 kernel CPU exceptions

## Status and purpose

Kernel CPU exceptions version 1 is the implemented candidate for Windvale OS firmware probe 17. It establishes one kernel-owned, terminal invalid-opcode boundary after firmware shutdown and on the kernel-owned stack. [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) owns the decision. Cross-host and final pinned-QEMU qualification remain pending.

This contract covers x86-64 exception vector 6 (`#UD`) only. It is intentionally separate from Windvale runtime traps such as `WVR3007`, which are represented by checked native status returns and do not raise processor faults.

## Ownership and installation

Kernel memory version 1 allocates one zeroed page before switching stacks and stores its address in `WVKMEM01` field `First allocation address`. Probe 17 dedicates that complete 4 KiB page to the exception table for the remainder of the boot probe. The page is not returned to the allocator and has no concurrent owner.

After switching to the two-page kernel stack and before entering the existing Main chain, the x86-64 exception object:

1. requires a non-null 4 KiB-aligned page address;
2. disables maskable interrupts with `CLI`;
3. clears the complete 4 KiB page;
4. reads the live code-segment selector from `CS`;
5. derives the complete linked handler address without truncation;
6. writes one canonical vector-6 gate at byte offset 96;
7. writes a ten-byte IDTR operand at page offset 112 with that page as its base and limit 111; and
8. publishes the table with `LIDT`.

The IDT contains exactly seven addressable 16-byte entries. Vectors 0 through 5 are zero and not present. Vector 6 is the sole admitted present gate. The ten-byte IDTR operand at bytes 112 through 121 is outside the table limit, and bytes 122 through 4095 remain zero.

## Vector-6 gate

The 16-byte little-endian descriptor has this exact logical shape:

| Bytes | Field | Version 1 rule |
| ---: | --- | --- |
| `0..1` | Handler offset `15..0` | Low handler-address bits |
| `2..3` | Segment selector | Live `CS`, nonzero |
| `4` | IST/reserved | Zero; no IST stack |
| `5` | Type and attributes | `0x8E`: present, DPL 0, interrupt gate |
| `6..7` | Handler offset `31..16` | Middle handler-address bits |
| `8..11` | Handler offset `63..32` | High handler-address bits |
| `12..15` | Reserved | Zero |

Reconstructing the three offset fields yields the exact complete handler address derived by a RIP-relative `LEA`. The accepted UEFI mapping makes that linked address canonical. Kernel memory supplies a page from its existing checked below-4-GiB arena, and the installer independently requires that address to be nonzero and 4 KiB aligned before clearing it or publishing the IDTR.

Maskable interrupts remain disabled after installation. This slice does not install an interrupt controller or claim that absent vectors are safely handled.

## Normal and invalid-opcode scenarios

Probe 17 supports two explicit deterministic construction scenarios:

- `Normal` installs the table, runs the existing portable and system-profile Main chain, and returns to the loader. The loader emits `cpu-exceptions=armed` after Hello World and before `native-context=pass`.
- `InvalidOpcode` installs the same table, runs the same Main chain, and executes exactly one `UD2` after Main returns. The vector-6 handler is terminal, so control does not return to the loader.

The scenario is explicit image-construction input. It is not inferred from host state, firmware behavior, timing, or mutable guest data.

## Terminal handler

The vector-6 handler does not depend on an error code because x86-64 `#UD` pushes none. It does not decode the processor-pushed instruction pointer, code segment, flags, stack, or stack segment. It enters through the interrupt gate while maskable interrupts are already disabled and emits these exact ASCII/LF bytes through the existing polled COM1 boundary:

```text
panic=invalid-opcode
vector=6
error-code=none
status=panic
```

It then writes value 1 to QEMU test port `0xF4`. Under the accepted `isa-debug-exit` device this produces host exit code 3. If the test device does not complete the guest, the handler executes a CLI/HLT loop forever. It never executes `IRETQ`, returns, allocates, calls firmware, invokes a runtime service, or resumes faulting code.

The complete panic suffix and host code 3 are both required for the invalid-opcode evidence scenario. Host code 3 by itself is not accepted. No normal post-kernel marker may follow `status=panic`.

## Construction and evidence

Version 1 accepts no serialized IDT, descriptor, selector, handler address, or vector from a caller. One fixed generated object owns all descriptor stores. Before PE publication, the WVO reader and focused host test lock:

- the exact architecture, code section, local handler and exported installer symbols, and absence of relocations;
- the complete object and code identities plus the aligned installer/handler bounds;
- the full-page clear, live `CS` capture, RIP-relative handler derivation, vector-6 gate stores, exact IDTR limit, and `CLI`/`LIDT` shape;
- the exact panic writer, debug-exit and halt fallback; and
- absence of `UD2` and `IRETQ` from the exception object itself.

At runtime the installer rejects null or non-page-aligned addresses before mutation or publication. The fixed memory-owner contract supplies the page from the checked below-4-GiB arena. Real QEMU execution is required because a host fixture cannot substitute for live `CS`, the relocated handler address, `LIDT`, or processor delivery of `#UD`.

The QEMU boundary requires deterministic images for both scenarios, the complete ordinary success marker for `Normal`, and the exact terminal panic marker plus host exit code 3 for `InvalidOpcode`. A timeout is failure evidence, not an acceptable substitute for the terminal handler. If a later contract accepts external gate records or general table input, that reader must add truncated, trailing, changed-field, wrong-vector, noncanonical-target, and range-overflow rejection coverage before publication.

## Implementation seam and limits

The current implementation is one bounded Stage 0 x86-64 object because WVA 1 lacks the required descriptor-memory, live-segment, `CLI`, `LIDT`, and terminal-entry operations. Its exact code, symbols, relocations, and scenario-specific `UD2` placement are verified before linking. WVA should eventually own irreducible entry mechanics, while system-profile `.wv` owns dispatch and policy only after explicit unsafe memory and kernel ABI contracts exist.

Version 1 provides no other exception or error-code shape, page fault or `CR2` reporting, double-fault stack, TSS, IST, NMI, IRQ, PIC/APIC, interrupt enablement, `IRETQ`, recovery, unwinding, process isolation, user mode, SMP, scheduler integration, WVR-to-CPU mapping, clean platform shutdown, Hyper-V evidence, or physical-hardware claim.
