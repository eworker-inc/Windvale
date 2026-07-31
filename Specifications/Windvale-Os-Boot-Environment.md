# Windvale OS boot environment

## Status and purpose

Windvale OS boot environment version 1 defines the accepted dependency preflight for the first x86-64 UEFI experiment. It fixes the emulator machine and immutable firmware inputs before a Windvale boot image exists. Passing this contract proves only that the required environment is present; it does not prove firmware entry, a bootable image, a kernel, or an operating system.

[Decision 0044](../Documents/Decisions/0044-First-X64-Uefi-Boot-Environment.md) owns the architecture choice and its reconsideration conditions.

## Fixed environment

The version 1 environment requires:

| Item | Required value |
| --- | --- |
| Architecture | x86-64 |
| Firmware-interface reference | UEFI 2.11; exercised subset only |
| QEMU version | `11.0.0` |
| QEMU machine | `pc-q35-11.0` |
| QEMU CPU model | `qemu64` |
| Accelerator | `tcg` |
| Virtual CPUs | 1 |
| Memory | 128 MiB |
| Network | none |
| Secure Boot | disabled |
| Firmware code bytes | 3,653,632 |
| Firmware code SHA-256 | `33090cc07675baa5190d9f1e84bf5176b33bcbfa9bacac522961150cdb6dbb2a` |
| Variable-template bytes | 540,672 |
| Variable-template SHA-256 | `5d2ac383371b408398accee7ec27c8c09ea5b74a0de0ceea6513388b15be5d1e` |

The code image is read-only. A boot run must copy the complete variable-store template to a run-private location before giving it to QEMU as writable pflash. The source template must remain byte-identical before and after the run.

`pc-q35-11.0` is named explicitly. The moving `q35` alias is not part of this contract even when an installed QEMU version currently maps it to the same machine.

## Discovery boundary

The verifier accepts explicit QEMU, firmware-code, and variable-template paths. When a path is omitted, it may inspect the command path, QEMU's adjacent `share` directory, and conventional system QEMU/OVMF data directories. Discovery affects only where inputs are found. Native paths are never serialized into the environment report.

QEMU executable bytes are host tools rather than portable Windvale artifacts. The report therefore records their SHA-256 without requiring Windows and Linux executables to match. Firmware and variable-template bytes are VM inputs and must match the exact hashes above on every host that claims this environment.

The repository preflight is invoked from its root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Environment.ps1
```

Explicit `-QemuPath`, `-FirmwareCodePath`, and `-FirmwareVariablesTemplatePath` values override discovery without changing report contents.

With `-PassThru`, the verifier also supplies resolved native paths to the boot harness in process. Those host-only launch values are not fields in the canonical report and are not portable evidence.

## Canonical preflight report

Successful preflight writes UTF-8 text in this exact field order:

```text
windvale-os-environment 1
status=ready
architecture=x86-64
firmware=uefi-2.11
qemu-version=<version>
qemu-build=<build|unreported>
qemu-bytes=<decimal>
qemu-sha256=<lowercase-hex>
machine=pc-q35-11.0
cpu=qemu64
accelerator=tcg
virtual-cpus=1
memory-mib=128
network=none
secure-boot=disabled
firmware-code-bytes=3653632
firmware-code-sha256=33090cc07675baa5190d9f1e84bf5176b33bcbfa9bacac522961150cdb6dbb2a
firmware-variables-template-bytes=540672
firmware-variables-template-sha256=5d2ac383371b408398accee7ec27c8c09ea5b74a0de0ceea6513388b15be5d1e
```

Decimal values have no grouping. Hexadecimal uses lowercase ASCII. The report contains no timestamp, native path, host name, user name, locale-dependent text, or mutable variable-store identity.

## Failures

Preflight fails without a success report when:

| Code | Meaning |
| --- | --- |
| `WVOS1001` | The x86-64 QEMU executable is missing or cannot report a version. |
| `WVOS1002` | The QEMU version is not exactly 11.0.0. |
| `WVOS1003` | QEMU does not expose the TCG accelerator. |
| `WVOS1004` | QEMU does not expose machine `pc-q35-11.0`. |
| `WVOS1005` | The firmware code or variable-template file is missing or unreadable. |
| `WVOS1006` | The firmware code size or SHA-256 does not match version 1. |
| `WVOS1007` | The variable-template size or SHA-256 does not match version 1. |

## Deliberate limits

This environment contract does not itself launch QEMU, create or mutate a variable store, encode PE32+, capture serial output, define completion status, or execute Windvale code. The separate [firmware boot probe](Windvale-Os-Boot-Probe.md) now owns those bounded actions. The environment report alone remains dependency evidence and cannot be cited as a successful boot.
