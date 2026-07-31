# Decision 0044: First x86-64 UEFI boot environment

- Date: 2026-07-31
- Status: Accepted and implemented; first Windows QEMU boot qualified

## Context

Windvale has a qualified x86-64-first object model, assembler, and deterministic flat-image linker, but it does not yet define or produce a firmware-loadable image. The project vision and roadmap proposed x86-64 UEFI with QEMU as the first automated environment and Hyper-V as a compatibility target. Leaving that proposal unresolved would let early boot experiments select firmware, machine, acceleration, and mutable state implicitly.

The initial development checkout runs inside a Hyper-V virtual machine. QEMU 11.0.0 and its bundled EDK II firmware are available there. QEMU's TCG accelerator runs without nested virtualization, which keeps the first automated environment usable in that checkout while avoiding an additional host dependency. The first decision must identify a reproducible experiment without turning a particular emulator, firmware build, or host into Windvale language semantics.

## Decision

Accept this first boot environment:

- The architecture is x86-64 and the normative firmware-interface reference is [UEFI 2.11](https://uefi.org/specs/UEFI/2.11/). Qualification applies only to the explicitly exercised subset; the exact EDK II image is not presumed to implement every UEFI 2.11 facility.
- The firmware-loadable artifact will be a PE32+ x86-64 UEFI application at the removable-media path `\EFI\BOOT\BOOTX64.EFI`. Its deterministic disk/container representation remains a separate target-adapter contract.
- The UEFI entry boundary uses the specified x64 firmware convention: the image handle arrives in `RCX`, the system-table pointer arrives in `RDX`, and the result is an `EFI_STATUS` in `RAX`. This does not define Windvale's later internal native ABI.
- QEMU is the primary automated VM. The accepted first machine is `pc-q35-11.0` with CPU model `qemu64`, one virtual CPU, 128 MiB of memory, no network device, and TCG acceleration.
- The non-Secure-Boot EDK II code image is immutable. Each run receives a private copy of the accepted variable-store template; no run may modify the installed template.
- The accepted firmware code is 3,653,632 bytes with SHA-256 `33090cc07675baa5190d9f1e84bf5176b33bcbfa9bacac522961150cdb6dbb2a`.
- The accepted variable-store template is 540,672 bytes with SHA-256 `5d2ac383371b408398accee7ec27c8c09ea5b74a0de0ceea6513388b15be5d1e`.
- QEMU version, build identity, and executable SHA-256 are recorded in environment evidence. The first Windows environment uses QEMU 11.0.0 build `v11.0.0-12122-ga4bb4b10c9`, whose executable SHA-256 is `a930e028f93d0fa47e4d58bdad2432f7466dc2b6af0ae376f77ef7a298ffdd02`. Executable bytes may differ on another host, so that hash is evidence for this environment rather than a portable artifact identity.
- Hyper-V Generation 2 is the secondary Windows qualification target after QEMU boot automation is stable. It must receive the same `BOOTX64.EFI` bytes with Secure Boot disabled initially. Firmware and device differences remain explicit evidence, not conditional Windvale behavior.
- Physical x86-64 UEFI hardware is a later portability target. Legacy BIOS and 32-bit UEFI are outside the first boot contract.

QEMU, EDK II firmware, variable stores, VM disks, and generated firmware images remain external or generated dependencies and are not committed to the repository.

## Consequences

The first boot experiment now has a fixed architecture, firmware boundary, emulator machine, resource envelope, and immutable firmware identity. It can run under TCG inside the existing Hyper-V development VM without enabling nested virtualization. WHPX, KVM, and other accelerators may be measured later, but they are not required to reproduce the first environment check.

This decision does not claim an operating-system implementation. [Decision 0045](0045-First-Uefi-Application-And-Boot-Probe.md) defines the first PE32+ writer, serial marker, and QEMU completion transport; [Decision 0046](0046-Bounded-Uefi-Memory-Map-Probe.md) adds bounded memory-map acquisition and release; [Decision 0047](0047-Bounded-Exit-Boot-Services-Transition.md) retains the current map, bounds stale-key recovery, and proves post-firmware execution; [Decision 0048](0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) adds the separately linked bootstrap kernel handoff. Disk-image publication, compiler-generated kernel code, page allocation, traps, kernel/process boundaries, system capabilities, bytecode runtime port, and clean platform shutdown remain later bounded slices.

The environment may be revised if the accepted EDK II bytes cannot exercise a required UEFI rule, `pc-q35-11.0` cannot supply stable automation, TCG makes the bounded boot probe impractical, or Hyper-V exposes a material incompatibility. A newer dependency alone is not sufficient reason to change a qualified environment.

## Verification

`Tools/Verify/Verify-Os-Environment.ps1` validates the exact QEMU version, TCG accelerator, Q35 machine identity, firmware sizes, and firmware hashes and emits a path-independent environment report. `Tools/Verify/Verify-Os-Boot.ps1` additionally builds and boots the first probe. The initial Windows environment passes both. Cross-host and secondary-target qualification remain pending.
