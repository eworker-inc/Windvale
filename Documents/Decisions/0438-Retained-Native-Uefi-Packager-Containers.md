# Decision 0438: Retained native UEFI packager containers

- Status: Implemented paired construction candidate; independent Linux execution and Probe 40 composition pending
- Date: 2026-08-09
- Advances: [Decision 0437](0437-Native-Linker-To-Uefi-Packaging.md), [Decision 0418](0418-Segmented-Compiler-Hosted-Package-Composition.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Windvale UEFI application format](../../Specifications/Windvale-Uefi-Application.md), [native hosted-container packaging](../../Specifications/Windvale-Native-Hosted-Container-Packaging.md), and [native retirement suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0437 proved the native linker-to-UEFI handoff, but its focused test
still constructed temporary Windows and Linux packager containers with frozen
Stage 0 builders. The existing 19-tool native hosted-container chain already
owned the same profile-5 container format. Its host scripts selected only their
own target even though the Windvale processes consume explicit platform
evidence. Retaining another managed UEFI-specific constructor would duplicate
that general path.

## Decision

- Let both `Package-Hosted-Wvb` host scripts accept an optional final
  `windows` or `linux` target. Omitting it preserves the previous current-host
  behavior and command syntax.
- Select the target's exact startup object and four platform service leaves;
  continue to use the invoking host's digest-bound Windvale tool processes for
  all format construction, admission, and publication.
- Require the output extension to agree with the explicit target. Keep both
  ordinary and image-input modes on the same path.
- Retain the exact UEFI packager WVB and paired PE/ELF containers under one
  candidate manifest. Add digest-bound `Package-Uefi.cmd` and `.sh` launchers;
  add no C# production implementation or fallback.
- Transfer the real native link-to-UEFI success, repetition, and invalid-entry
  preservation cases into a paired `.NET-free` command and add that command to
  the digest-bound retirement plan.

## Evidence and consequences

The focused Windows hosted-packaging command passes 3/3 in 18.3 seconds. It
reproduces the established Windows application, constructs the established
Linux ELF byte for byte on Windows, and preserves input, destination, and
private-scratch invariants for an invalid WVB. The UEFI candidate build then
completes through the native Project 1 and hosted-container paths in 15.7
seconds with these retained identities:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Uefi-Packager.wvb` | 25,999 | `063f95f53e39390c76bcf31fbf7bdc87eed6194388101fadc4d60ee41b2802e4` |
| `Uefi-Packager.exe` | 278,528 | `326401d0e3d9e6b1c1e329a1fa0c7f5e550f4d48e073d0ab83f6f8657edff320` |
| `Uefi-Packager.elf` | 278,528 | `9b1c3a364e21b3fb66b246fb89df907b523272fee4e3ac5eaa39f5414e39e5b6` |

The new native UEFI command passes 3/3 in 1.1 seconds. It links the pinned WVO
pair at base zero, requires the exact 24-byte image and entry-zero evidence,
constructs the exact 1,536-byte EFI at SHA-256
`7d30fd4d220a2d578b0ce3da4cbb6006175f012268b7d3a08e80543e7e388b09`,
repeats it exactly, and preserves an existing destination for entry offset 24.
The digest-bound `uefi-packager` retirement filter passes its three cases in
1.2 seconds. The reduced C# differential test no longer constructs a host
container; it verifies retained identities, exact Stage 0 EFI agreement, and
no CLR in the executed candidate, passing in 2.544 test seconds.

This removes the managed host-container constructor from the UEFI packager's
candidate path and grows the native plan to 25 suites and 3,121 fixed cases.
It does not promote O2: the retained Linux ELF has not yet executed on an
independent Linux host, successful `.efi` output still uses the current exact
file-write service rather than a UEFI-specific atomic publisher, and the
managed Probe 40 coordinator still owns upstream objects and five-scenario
composition. No generated EFI, firmware, or vendor-specific metadata is
retained.

## Reconsideration triggers

Replace explicit cross-target selection only if the native tool contract gains
a typed target manifest that is at least as strict. Add an atomic UEFI
publisher before normal-path promotion if successful output replacement must
survive process or power interruption. Do not infer a target from host identity
when an explicit opposite-target request is present, and do not restore a
managed fallback.
