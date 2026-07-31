# Windvale OS firmware boot probe

## Status and purpose

Firmware boot probe version 1 is the first executable Windvale OS evidence slice. It proves that the exact boot environment can load the deterministic Windvale UEFI application, enter x86-64 code, expose serial evidence, and let the guest complete the emulator run. It is not a kernel or an operating-system qualification.

The probe source is owned by `Operating-System/Windvale.Bootstrap`. PE32+ construction remains owned by the linker target adapter in [Windvale-Uefi-Application.md](Windvale-Uefi-Application.md). The emulator and firmware inputs remain governed by [Windvale-Os-Boot-Environment.md](Windvale-Os-Boot-Environment.md).

## Probe behavior

The bootstrap builder creates one canonical WVO object in memory, links it at base zero, and passes the verified result to the UEFI application writer. No generated WVO, EFI application, FAT view, variable store, or firmware image is committed.

The relocation-free x86-64 entry code:

1. initializes COM1 at I/O base `0x3F8` for 8-N-1 operation;
2. waits for transmitter readiness before each byte;
3. emits exact ASCII/LF bytes:

   ```text
   windvale-os-boot 1
   status=pass
   ```

4. writes value zero to QEMU test port `0xF4`; and
5. returns EFI success only if the QEMU-only completion device is absent.

The `isa-debug-exit` write makes QEMU return host exit code `(value << 1) | 1`, which is 1 for the probe. Port `0xF4` is test transport and is not a Windvale OS device contract. The serial marker is required because QEMU startup failures can also return 1.

The canonical probe application is 2,048 bytes with SHA-256 `7ee7acb6ca1bdce9e2179f302bd6a98dce1f1a638ca760991f362fc71d35f026`.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1
```

The harness first runs the environment preflight. It then creates one unique directory beneath the native temporary directory, constructs `EFI/BOOT/BOOTX64.EFI`, copies the complete accepted variable-template bytes, and launches the fixed QEMU environment with:

- `pc-q35-11.0,accel=tcg`, `qemu64`, one CPU, and 128 MiB;
- immutable firmware code plus the run-private writable variable store;
- no network device, display, or monitor;
- a run-private VVFAT hard-disk view containing the removable-media path;
- serial output captured to a run-private file; and
- `isa-debug-exit` at port `0xF4`.

The VVFAT directory is writable only inside the disposable run directory. The harness records the EFI digest before launch and verifies it again afterward. It also verifies the installed firmware code and installed variable-template digests after the run. The private variable copy may change and is discarded.

The default timeout is 60 seconds and may be set only from 5 through 300 seconds. Timeout is the only normal path that forcibly terminates QEMU. Run artifacts are deleted after success or failure unless `-KeepRunDirectory` is supplied for diagnosis; cleanup first checks that the absolute path is an exact `windvale-os-boot-*` child of the native temporary directory.

## Success report

Successful execution emits this path-free field order:

```text
windvale-os-boot-report 1
status=pass
architecture=x86-64
application-format=pe32-plus-uefi-application-v1
efi-bytes=2048
efi-sha256=7ee7acb6ca1bdce9e2179f302bd6a98dce1f1a638ca760991f362fc71d35f026
serial-marker=windvale-os-boot-1-status-pass
qemu-exit-code=1
```

`-KeepRunDirectory` adds a native diagnostic path after the canonical fields, so that invocation is not portable report evidence.

## Failures

| Code | Meaning |
| --- | --- |
| `WVOS3001` | The .NET host or probe build failed. |
| `WVOS3002` | QEMU could not start. |
| `WVOS3003` | The bounded boot timeout expired. |
| `WVOS3004` | QEMU returned an unexpected exit code. |
| `WVOS3005` | Serial output is missing or lacks the complete marker. |
| `WVOS3006` | The EFI application or an installed firmware input changed during the run. |
| `WVOS3007` | Temporary-directory cleanup failed its absolute-path boundary check. |

## What this does not prove

The probe does not read its UEFI image handle or system-table pointer, call firmware services, obtain the memory map, exit boot services, configure paging, handle interrupts, discover hardware, load a kernel, run Windvale bytecode, or define a stable native ABI. Those enter as later bounded evidence slices.
