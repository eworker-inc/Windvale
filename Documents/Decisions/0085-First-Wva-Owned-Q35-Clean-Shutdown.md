# Decision 0085: First WVA-owned Q35 clean shutdown

- Date: 2026-08-01
- Status: Accepted and cross-host qualified through firmware probe 20 at exact commit `12e9e2e`
- Implements: The first clean-shutdown slice of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Extends: [Decision 0081](0081-First-Terminal-X64-Cpu-Exception-Boundary.md)'s firmware probe 17 lifecycle evidence
- Contract: [Kernel shutdown version 1](../../Specifications/Windvale-Kernel-Shutdown.md)

## Context

The qualified probe-17 normal path emits `status=pass`, writes value zero to QEMU test port `0xF4`, and enters a CLI/HLT fallback. That provides deterministic test completion with host code 1, but Decision 0081 explicitly excludes it from platform shutdown. The same debug device also transports expected terminal exception evidence, so a debug exit is not a clean lifecycle boundary.

Decision 0084 assigns architecture mechanics to WVA and shutdown enforcement to the kernel/platform boundary. The smallest useful next slice is a real poweroff of the pinned Q35 machine after the successful Windvale Main chain, without importing a firmware runtime pointer, inventing a general unsafe FFI, or moving target constants into portable `.wv` modules.

## Decision

- Extend WVA 1 with three exact no-operand x86-64 code statements demanded by the boot path: `disable_interrupts` (`FA`), `halt` (`F4`), and `out_u16` (`66 EF`, using `DX` and `AX`). Keep their privileged nature explicit and add them to both the independent C# reference assembler and Windvale-written assembler.
- Advance the firmware probe to version 18 while preserving handoff version 1, kernel memory version 1, kernel CPU exceptions version 1, native ABI 14/context 6, WVB 1.6, WVO 1.0, and UEFI application format version 3.
- Advance the bounded kernel WVA seam to version 4. Add WVA export `Windvale_kernel_x64_q35_shutdown` with exact port `0x0604`, value `0x2000`, `OUT DX, AX`, CLI/HLT, and a self-jump fallback. C# assembles and validates this object but does not emit its machine bytes.
- After the ordinary kernel call succeeds, emit exact `status=pass` and `shutdown=poweroff` lines, call the WVA shutdown export, and treat any impossible return as terminal failure.
- Remove the normal-path value-zero write to QEMU debug port `0xF4`. Require QEMU process exit code 0 plus the complete unique success/shutdown serial marker. Retain `isa-debug-exit` only for failure transport and the explicit invalid-opcode scenario.
- Preserve the invalid-opcode path: it reaches the existing vector-6 terminal handler before later success/shutdown markers and still requires the exact panic suffix plus host code 3.
- Treat fixed Q35 PM-control values as an explicit target adapter. Do not present this as ACPI discovery, Hyper-V support, physical-machine poweroff, process teardown, or a portable shutdown capability.

## Implementation evidence

Local Windows candidate evidence on the pinned QEMU 11.0/Q35/TCG environment records:

- all 17 focused OS tests passing;
- all 6 focused assembler tests passing, including complete reference/Windvale output parity for the new WVA statements;
- a deterministic 382-byte WVA shim object with SHA-256 `4cbc235de885ab9307974128e55d8c7472cc889349f94ca4b87587ee9399a08c`;
- a deterministic 98,228-byte Windvale assembler WVB with SHA-256 `d9c055cf9a38ab1af5426d723f7acecddea24dbaa50c959daad57ca7417c0540`;
- an exact 18,432-byte normal image with SHA-256 `035f7a25c263efdd0cec30c081ee36799b04ca85eba57d9f54a98e1ce06a6de5`, complete shutdown marker, and QEMU exit code 0; and
- an exact 18,432-byte invalid-opcode image with SHA-256 `3d0cd8f66a7cd50826f2b66b3961cb06888956c72e3925d5f2837405f0c9dacf`, unchanged panic semantics, and expected QEMU exit code 3.

The standalone probe-18 identities above remain useful construction evidence. Exact commit `12e9e2ebcd4960f856b90064f6343ea5856b5b43` cross-host qualifies the same unchanged WVA shutdown contract through composed firmware probe 20: Windows and Debian pass all 66 Seed tests and all 18 OS tests, normalized contracts and all 69 portable artifacts match, GitHub verification passes, and pinned QEMU reproduces the exact 20,992-byte normal image with SHA-256 `d4a9e3625779dd3ef2a03fd71ecfe1502c1ad39378da7adbcf7e4b55636eed8c`, complete shutdown marker, and exit code 0. Decision 0087 records the complete integrated archive evidence.

## Consequences and limits

Windvale OS now has a real successful poweroff candidate owned by WVA machine source rather than a C# byte emitter or testing-only exit device. The path also supplies the first concrete boot-driven reason to extend WVA with privileged machine operations.

The fixed Q35 adapter is intentionally narrower than a general shutdown service. It performs no process or service coordination because none exists yet. It discovers no ACPI structure and supplies no Hyper-V or hardware claim. The loader remains a C# Stage 0 construction owner, while the assembled image executes only linked native code after firmware exit.

Changing WVA source changes the composed Windvale assembler identity and therefore requires cross-host reproduction before qualification. It does not change portable Windvale source, WVB semantics, the shared native ABI, the portable kernel module, or the exception contract.

## Reconsider when

- ACPI table parsing can replace the pinned Q35 port with discovered target state.
- Hyper-V or physical-hardware evidence requires another shutdown adapter.
- Processes and services require ordered notification, quiescence, storage flush, timeouts, or forced termination.
- SMP requires stopping or coordinating additional processors.
- WVA gains a more general but equally explicit port-I/O instruction contract.
