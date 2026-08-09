# Decision 0436: Windvale-native UEFI application construction

- Status: Implemented current-host format core and native front doors; Probe 40 integration pending
- Date: 2026-08-09
- Advances: [Decision 0045](0045-First-Uefi-Application-And-Boot-Probe.md), [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), and [Decision 0435](0435-Digest-Bound-Os-Boot-Execution.md)
- Contract: [Windvale UEFI application format](../../Specifications/Windvale-Uefi-Application.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0435 removed Stage 0 from ordinary QEMU execution, but the final
flat-image-to-EFI packaging step still existed only in `Linker/Reference` C#.
Replacing the whole Probe 40 constructor at once would combine compiler,
assembler, linker, kernel-resource, scenario, and PE construction boundaries.
The smallest honest O2 transfer is the deterministic UEFI adapter that consumes
an already linked flat image.

## Decision

- Implement canonical UEFI v3 construction in portable Windvale with the same
  4 MiB linked-image and 4,195,328-byte application limits.
- Keep independent untrusted-byte verification in a separate reusable module.
  The constructor must invoke it and compare recovered code and entry before
  publishing bytes.
- Give construction and verification explicit version-1 byte envelopes with
  bounded lengths, status, failure offset, entry, payload length, and reserved
  fields. These are native-tool boundaries, not new EFI format versions.
- Use a small verifier bridge so each native application exposes exactly one
  `Main(bytes) -> bytes` entry point while the verification core remains
  reusable by the constructor.
- Add Project 1 front doors for both applications. Keep the managed writer and
  verifier unchanged as frozen differential and recovery evidence.
- Do not commit generated EFI, WVB, WVO, native application, firmware, or
  vendor-specific metadata artifacts as part of this slice.

## Evidence and consequences

The focused Seed case compiles both portable module graphs, confirms they
declare no capabilities, and executes them through both the reference runtime
and native x64 backend. Tiny entry-zero and nonzero-entry fixtures match the
frozen Stage 0 writer byte for byte, repeat deterministically, and round-trip
their exact code and entry through the independent Windvale verifier.

The same case rejects representative truncated, extended, DOS, COFF, optional
header, section, relocation, text-padding, relocation-padding, request-magic,
request-version, request-size, and entry failures. Both native Project 1 front
doors reproduce the exact WVBs emitted by the frozen compiler oracle. The
single focused Fast selection passes in 4.344 seconds after test review.

O2 therefore becomes a native candidate, but it is not promoted: the managed
Probe 40 coordinator still produces and links the upstream scenario-specific
objects before calling its writer. The next O2 slice should connect the native
link result to this constructor without adding a second PE implementation.

## Reconsideration triggers

Change this boundary only when a qualified linked-image stream exceeds the
current one-value limit, UEFI v4 needs a different section or relocation model,
or native Probe 40 composition proves that a smaller typed handoff is required.
Do not weaken independent verification or restore an implicit managed fallback.
