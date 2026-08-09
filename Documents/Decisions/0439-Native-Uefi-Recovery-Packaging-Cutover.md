# Decision 0439: Native UEFI recovery-packaging cutover

- Status: Implemented current-host normal-scenario recovery cutover; native Probe 40 linking and composition pending
- Date: 2026-08-09
- Advances: [Decision 0438](0438-Retained-Native-Uefi-Packager-Containers.md), [Decision 0435](0435-Digest-Bound-Os-Boot-Execution.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale UEFI application format](../../Specifications/Windvale-Uefi-Application.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The recovery command still asked `Operating-System/Windvale.Bootstrap` to
produce the final EFI even after Decisions 0436 through 0438 established a
retained native packager. That left the frozen managed UEFI writer on an
executed workflow and made it unclear whether the native candidate could carry
the real 682-KiB Probe 40 payload rather than only compact fixtures.

Replacing every scenario-specific object producer and the 15-object link in
one change would mix several independent bootstrap boundaries. The smallest
honest cutover is to expose the already verified managed link result as a
temporary recovery payload and make the retained native packager own the final
EFI bytes.

## Decision

- Add a bounded `Buildˉlinkedˉimage` recovery boundary that returns the exact
  successful Probe 40 `Linkˉresult`. Keep `Buildˉapplication` as a thin frozen
  Stage 0 differential wrapper over that same result.
- Add `--linked-output <IMAGE.BIN>` to the Stage 0 recovery CLI. It writes the
  exact linked bytes and reports one decimal entry offset and SHA-256; it does
  not construct an EFI in this mode.
- Change `Rebuild-Os-Probe.ps1` to request that linked payload, strictly parse
  its single entry report, invoke the digest-bound `Package-Uefi.cmd`, and
  publish only the successfully constructed candidate.
- Keep all linked-payload and failed-candidate names private and remove them on
  success or failure. Preserve the existing refusal to overwrite the requested
  final destination.
- Add no source-language semantics, alternate UEFI writer, managed fallback,
  generated firmware, or vendor-specific metadata.

## Evidence and consequences

The reviewed normal-scenario recovery command completes in 16.5 seconds. It
publishes the exact established 683,008-byte Probe 40 EFI at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
The output directory contains only that requested EFI after completion; the
linked payload and private candidate are absent. This is construction evidence,
not a QEMU boot or broad OS verification rerun.

The actual recovery workflow no longer executes the managed UEFI writer. The
writer remains available only through the explicit Stage 0 CLI and tests as a
frozen differential oracle. O2 is still a candidate because Stage 0 continues
to compile scenario sources, build all bootstrap WVOs, and perform their link
before handing off the flat image. Native object production/linking, the other
four recovery scenarios through this exact command, independent Linux
execution, durable UEFI publication, five-scenario reconstruction, and the
grouped retirement gate remain open.

## Reconsideration triggers

Replace the linked-payload handoff when the recovery producer can emit a
versioned object inventory for the native linker. Do not make console parsing a
semantic owner: it may carry only the already verified decimal entry. Do not
remove the managed writer until the final Stage 0 archive and complete Decision
0057 gate are satisfied.
