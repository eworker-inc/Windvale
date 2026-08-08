# Decision 0355: Windvale-owned native UTF-8 service construction

- Status: Accepted current-host ownership transfer; direct normal consumption advanced by Decision 0364; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0070](0070-First-Runtime-Native-Utf8-Service.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Advanced by: [Decision 0364](0364-Direct-Fixed-Native-Service-Leaf-Consumption.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#pure-utf-8-validation-service)

## Context

Decision 0070 removed the managed UTF-8 callback from native execution, but
`Runtime/Windvale.Native/X64-Native-Utf8-Service.cs` still constructed the
complete 800-byte x86-64 leaf with a C# label and relative-branch builder.
That generator was deterministic and qualified on both hosts, yet it remained
a normal service-bundle construction dependency wherever native applications
needed the capability-free UTF-8 slot.

Reassembling the leaf through ordinary WVA would change its bytes because the
current assembler deliberately uses canonical longer memory and immediate
encodings. Preserving the already qualified service and every descendant
bundle therefore requires transferring the exact bounded generator rather
than silently selecting a different equivalent instruction sequence.

## Decision

- Add `Runtime/Windvale/Native-X64-Utf8-Service.wv` as the focused portable
  owner of the exact x86-64 construction algorithm. It emits typed bytes,
  records bounded branch patches, derives forward and two's-complement
  backward relative displacements, and applies each four-byte patch through
  immutable slices and concatenation.
- Keep the dispatch and sequence blocks named by their UTF-8 role rather than
  storing one opaque 800-byte literal. The module remains architecture-specific
  runtime construction; it does not define source-language UTF-8 semantics.
- Add the small capability-free byte-result bridge and its ordinary Project 1
  closure. The native source front door must reproduce the Stage 0 WVB byte
  for byte.
- Retain that exact WVB in `Runtime/Windvale.Native/Consumers`. The temporary
  C# recovery wrapper may load, hash, verify, lower, independently verify,
  execute once through W^X, and cache the result. It must not contain a second
  leaf-emission or relative-branch algorithm.
- Preserve Decision 0070's exact 800-byte leaf and SHA-256. No service-table,
  native ABI, adapter, bundle placement, application, or package identity may
  change in this transfer.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale construction core WVB | 11,577 | `adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7` |
| Retained byte-result bridge WVB | 11,511 | `4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f` |
| Shared native UTF-8 leaf | 800 | `4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf` |

## Evidence and consequences

The source closure first compiled through Stage 0 for precise diagnostics.
The ordinary native Project 1 front door then independently published the
same 11,511-byte bridge WVB with the same digest. After reviewing the focused
test, the affected Release project built with zero warnings and errors in
22.57 seconds. The one named test passed 1/1 in 2.193 seconds.

That test recompiles and pins both Windvale modules, compares the retained WVB
with the source result, rebuilds it through the native front door, and requires
the reference interpreter and verified x64 backend to return the exact same
800 bytes. It also rejects a corrupted final leaf through the unchanged
identity gate. The older semantic boundary corpus remains the qualified
evidence for accepted and rejected UTF-8 sequences; it was not redundantly
rerun for this byte-identical construction transfer. The paired final
qualification scripts now reproduce and inspect both modules and compare the
retained bridge exactly; only their PowerShell and shell syntax was checked in
this focused slice.

The live service leaf is now constructed by Windvale source, not duplicated
in C#. The managed runtime still owns temporary WVB loading, native lowering,
independent fragment verification, W^X allocation, invocation, caching, and
service-bundle/container orchestration. Those are separate retirement items.
Linux execution, Development, Standard, Qualification, and the grouped native
retirement suite remain deferred to the final goal gate.

## Reconsideration triggers

Revisit the retained bridge when normal native service-bundle construction no
longer needs the managed loader/executor. Change the leaf bytes only through a
new explicit service identity with complete semantic and descendant-package
qualification; do not refresh the pinned WVB or leaf hash as an incidental
refactor.
