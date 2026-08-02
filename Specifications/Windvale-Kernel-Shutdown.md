# Windvale kernel shutdown

## Status and purpose

Kernel shutdown version 1 is cross-host qualified at exact commit `12e9e2e`. It was introduced by Windvale OS firmware probe 18, retained by qualified probes 21 and 24, and remains byte-for-byte unchanged through candidate probe 25. It defines one deterministic clean-poweroff path for pinned QEMU `pc-q35-11.0` after successful kernel execution. [Decision 0085](../Documents/Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) owns the contract; Decisions 0087 through 0094 record later compositions.

This is a target-specific machine adapter, not a portable Windvale capability, general ACPI discovery, a Hyper-V shutdown contract, or a process/service shutdown policy.

## Ownership and symbol boundary

`Operating-System/Kernel/X64-Kernel-Shims.wva` owns the implementation and exports the ASCII-safe function symbol:

```text
Windvale_kernel_x64_q35_shutdown
```

The post-firmware loader imports that symbol through one WVO `relative-i32` relocation and calls it only after the complete kernel Main chain returns success and serial output has emitted:

```text
status=pass
shutdown=poweroff
```

The function is terminal and takes no arguments. It must not return to the loader or firmware.

The C# Stage 0 bootstrap assembles, independently decodes, shape-checks, links, and packages this WVA object. It does not emit the shutdown instruction bytes. The Windvale-written and reference assemblers must produce the same verified WVO for the accepted instruction surface.

## Q35 poweroff mechanics

The accepted pinned Q35 adapter performs exactly:

1. `move_u32 edx 1540`, selecting I/O port `0x0604`;
2. `move_u32 eax 8192`, selecting value `0x2000`;
3. `out_u16`, encoding operand-size-prefixed `OUT DX, AX` as `66 EF`;
4. `disable_interrupts`, encoding `CLI` as `FA`;
5. `halt`, encoding `HLT` as `F4`; and
6. `jump Windvale_kernel_x64_q35_shutdown`, providing a closed fallback that retries the poweroff request if an admitted non-maskable wake event resumes the halted CPU.

The resulting function is exactly 19 bytes:

```text
BA 04 06 00 00 B8 00 20 00 00 66 EF FA F4 E9 00 00 00 00
```

Its final four zero bytes are one `relative-i32` relocation field with addend `-4` targeting the function itself. The linker resolves that field before PE32+ publication.

The operation runs at the existing privileged x86-64 boot level after `ExitBootServices`. It uses no firmware service, host callback, runtime service table, heap, or mutable global state.

## Normal and fault evidence

Probe 25 retains the normal path, two terminal kernel-fault scenarios, and one contained interpreter-process-fault success scenario after WVB admission and two-process execution:

- `normal` completes Windvale admission, blocks the receive-only init service, interprets the admitted WVB in the send-only process, wakes and completes the service, runs the retained ABI-16 portable-WVB AOT path and system-profile Main, emits the exact success and shutdown markers once, executes the WVA Q35 poweroff request, and requires QEMU process exit code `0`.
- `invalid-opcode` executes `UD2` after Main, emits the exact normalized vector-6 terminal panic suffix, and uses the test-only `isa-debug-exit` path with host code `3`.
- `general-protection` dereferences a noncanonical address after Main, emits the exact normalized vector-13 terminal panic suffix, and uses the same test-only host code `3`.
- `user-fault` sends the client result, executes privileged `CLI` at CPL3, contains vector 13 against the client, wakes and completes the independent init service, emits `user-fault=contained`, and reaches the same clean shutdown with host code `0`.

Neither terminal kernel-fault image may emit the later armed, native, success, or shutdown markers. The contained user-fault image must emit all of them.

The normal path no longer writes success value zero to QEMU debug port `0xF4`. The debug-exit device remains attached to the test machine because failure and explicit fault scenarios use it. Exit code zero alone is not accepted as clean-shutdown evidence; the complete unique serial marker, deterministic image identity, lack of an opposite terminal marker, and bounded no-timeout QEMU completion are all required.

## Validation boundary

Before linking, the independently verified WVA object must lock:

- one code section with 16-byte alignment;
- the complete instruction bytes and 19-byte shutdown symbol range;
- the exported shutdown symbol and its absence from portable source contracts;
- the exact self-loop relocation at the final `jump` field;
- the `0x0604` port and `0x2000` value;
- exactly one `out_u16`, `disable_interrupts`, and `halt` sequence in the function; and
- no `return`, firmware call, or debug-exit write in the function.

The WVA conformance boundary additionally requires the C# reference and Windvale-written assemblers to agree on the new `disable_interrupts`, `halt`, and `out_u16` encodings, statement sizes, definition sizes, and relocation offsets. Operand-bearing forms are rejected.

Two builds of each firmware scenario must be byte-identical. The focused OS suite verifies the complete linked image shape; real pinned-QEMU runs verify the port behavior that a host byte fixture cannot prove.

## Limits

Version 1 hard-codes the accepted Q35 PM control port and poweroff value. It does not parse ACPI tables, discover PM blocks, enable ACPI mode, wait for device quiescence, flush storage, stop processes, notify services, coordinate CPUs, power off Hyper-V or physical hardware, call UEFI runtime services, reboot, sleep, or expose a Windvale shutdown capability.

The later platform lifecycle service must perform process/service coordination before selecting a target adapter. Q35 constants must not leak into portable modules or become a universal Windvale OS ABI.
