# Decision 0045: First UEFI application and boot probe

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

The accepted x86-64 boot environment supplied exact QEMU and EDK II inputs but no firmware-loadable Windvale artifact. The existing linker intentionally ended at an independently verified flat image. The next step needed to prove the smallest honest vertical path from WVO through the linker, a deterministic PE32+ target adapter, firmware entry, serial evidence, and guest-controlled completion.

The current object model has 32-bit absolute and relative relocations but no PE base-relocation model or native-backend position-independence contract. The current WVA surface also does not encode the I/O instructions needed by the first probe. General PE section mapping or an invented relocation translation would therefore claim more than the stack can support.

## Decision

- Add target adapter `pe32-plus-x86-64-uefi-application-v1` under `Linker/`. It consumes only a successful, independently verified `flat-x86-64-v1` result.
- Version 1 accepts exactly one non-empty code section linked at base zero, with its entry inside the code and with no imports or WVO relocations. This deliberately supports address-independent bootstrap code only.
- Emit a deterministic PE32+ x86-64 EFI application with `.text` and `.reloc` sections, subsystem 10, zero timestamp and checksum, 512-byte file alignment, 4 KiB section alignment, and no host-derived metadata.
- Include one 12-byte base-relocation block containing only `IMAGE_REL_BASED_ABSOLUTE` padding. The first code has no load-address-dependent fields; the block gives the firmware a valid relocation directory without pretending that WVO already represents PE `DIR64` fixups.
- Parse and verify the complete target format independently before publishing it. The verifier accepts only the canonical version 1 layout, checks all lengths and offsets before use, rejects trailing bytes and nonzero padding, and returns the exact code and entry offset recovered from the file.
- Begin `Operating-System/` implementation with one generated bootstrap probe. It initializes COM1, emits `windvale-os-boot 1` and `status=pass`, then writes zero to QEMU's `isa-debug-exit` port. Its raw x86-64 encoding is temporary system-bootstrap evidence, not a source-language feature, public Windvale ABI, driver design, or compiler backend.
- Run the probe from a unique temporary FAT directory with a private copy of the accepted variable store. QEMU's VVFAT backend receives that run-private directory as writable because its hard-disk attachment rejects a read-only block node; the harness verifies that `BOOTX64.EFI`, installed firmware code, and installed variable-template bytes remain unchanged.
- Treat QEMU exit code 1 as success only when the complete serial marker is also present. Exit code 1 alone can represent a QEMU startup error and is never sufficient evidence.

## Consequences

The repository now produces a 2,048-byte `BOOTX64.EFI` with SHA-256 `7ee7acb6ca1bdce9e2179f302bd6a98dce1f1a638ca760991f362fc71d35f026`. The accepted Windows QEMU/EDK II environment loads and enters it, the marker reaches the emulated serial port, and the guest terminates QEMU through the explicit test device.

This proves firmware loading, PE32+ entry, raw x86-64 execution, serial observation, and deterministic completion. It does not yet prove use of the UEFI system table, boot-service calls, memory-map acquisition, `ExitBootServices`, a kernel, interrupts, paging, drivers, processes, or Windvale bytecode execution.

Generated EFI files, variable stores, FAT views, and firmware remain build/run artifacts and are not committed. The writer's narrow acceptance boundary prevents the first probe from silently becoming an underspecified general PE linker.

## Reconsider when

- The native backend supplies an explicit position-independence and relocation contract.
- A concrete boot stage needs read-only data, writable data, zero-fill, imports, or PE `DIR64` base fixups.
- The probe starts using UEFI boot services and needs a specified firmware-call wrapper.
- Hyper-V or physical hardware exposes a meaningful incompatibility in the exact same EFI bytes.
