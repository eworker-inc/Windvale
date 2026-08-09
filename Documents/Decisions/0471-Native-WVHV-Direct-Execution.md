# Decision 0471: Native WVHV direct execution

- Status: Implemented Windows evidence; independent Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0470](0470-Native-WVHV-Container-Composition.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier container](../../Specifications/Windvale-Native-Hosted-Verifier-Container.md)

## Context

Decision 0470 proved that the Windvale constructor's PE and ELF bytes matched
the frozen Stage 0 application contracts. Exact bytes are necessary, but they
do not alone prove that the packaged constructor and the application it emits
execute as ordinary native processes without loading .NET.

## Decision

- Execute the digest-pinned current-host `wvhostverifiercompose` package against
  the same four admitted products used by the focused byte comparison.
- Require its file output to remain byte-identical to the independent contract.
- Execute that produced compiler-WVB verifier against the canonical verifier
  WVB and require the established successful report.
- Observe both processes' loaded modules or mappings and reject CLR, hostfxr,
  hostpolicy, or coreclr presence.
- Run only the current host in the focused local test. Preserve the opposite
  target's exact-byte assertion and defer its process execution to the final
  independent Linux gate.

## Evidence and consequences

The reviewed focused test passes 1/1 in 20.519 seconds after the incremental
build. On Windows, the packaged constructor emits the exact 1,004,032-byte PE;
that PE reports `wvb status=Valid profile=compiler-aligned` for the canonical
compiler verifier. Neither process loads CLR, hostfxr, or hostpolicy. The same
test still reconstructs the exact 1,003,520-byte ELF and retains bundle-digest,
destination-preservation, and alias rejection coverage.

This closes current-host execution of the final constructed verifier but does
not yet make publication durable. Format-4 admission must be added to the
existing native console-application publisher before its atomic replacement
transaction can be reused safely. Independent Linux execution, grouped
qualification, promotion, and recovery deletion remain. No broad Seed, OS,
Standard, Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Repeat the direct process evidence when the verifier application, format-4
container, native host adapters, or toolset package identity changes.
