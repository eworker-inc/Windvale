# Decision 0480: Native WVHV publisher Windows materialization

- Status: Implemented current-host candidate; pipeline integration pending
- Date: 2026-08-09
- Advances: [Decision 0479](0479-Native-WVHV-Publisher-Linux-Materialization.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher Windows materialization](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.md)

## Context

Decision 0479 removed the final Linux publisher writer. The frozen C# Windows
writer still expanded the generic base PE, shifted its data and relocation
regions, rewrote PE layout fields, and joined the instantiated objects,
publisher import page, and metadata.

## Decision

- Add a service-free controlled downstream constructor for the exact Windows
  publisher PE.
- Consume the transient generic base application, admitted `WVCR`, successful
  `WVIO`, exact `WVVP`, and successful native Windows-import response.
- Patch only the 512-byte PE header and construct the remaining output once in
  increasing file-offset order from immutable slices.
- Preserve the generic base runtime and relocation bytes except for the exact
  128-byte publisher-metadata slot; replace the generic import page completely.
- Keep full input/final digest admission in the adjacent qualified stages so
  this module remains a bounded placement owner below compiler limits.

## Evidence and consequences

The native front door builds a service-free 15,431-byte WVB with SHA-256
`73786b8bb60f8dc472c8ff111104480e16d1ac46e485125713a3fa4159aee633`.
The reviewed focused test passes 1/1 in 3.101 seconds. It pins the transient
248,832-byte base PE with SHA-256
`cf204201e5c26d71e78da1112de2bc724d389a5222cc835d48dbe8cd8bbc5988`,
proves interpreter/native equality, and reproduces the complete 256,000-byte
canonical publisher with SHA-256
`735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6`.
Envelope, base-header, failed-object, wrong-metadata, and wrong-import cases
reject.

Together with Decision 0479, this removes target-specific final publisher
image semantics from the remaining C# gap. Ordinary native pipeline wiring,
independent Linux execution, grouped qualification, promotion, and recovery
deletion remain. No broad Seed, OS, Standard, Qualification, WebAssembly,
QEMU, or Linux process gate ran.

## Reconsideration triggers

Version the request or response if base PE identity, section geometry, import
layout, component placement, metadata offset, or final identity changes.
