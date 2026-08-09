# Decision 0479: Native WVHV publisher Linux materialization

- Status: Implemented current-host candidate; Windows PE materialization pending
- Date: 2026-08-09
- Advances: [Decision 0478](0478-Native-WVHV-Publisher-Windows-Imports.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher Linux materialization](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.md)

## Context

Decisions 0475 through 0478 moved metadata, layout, object instantiation, and
the Windows import page into Windvale. The frozen C# Linux publisher writer
still joined those values with the generic hosted-verifier base ELF, moved the
format note, added the publisher load segment, and emitted the final file.

## Decision

- Add a service-free controlled downstream constructor for the exact Linux
  publisher ELF.
- Consume the transient generic base application, admitted `WVCR`, successful
  `WVIO`, and exact `WVVP`; do not archive another large base binary.
- Mutate only the 4,096-byte ELF header page, then construct the result once in
  increasing file-offset order from immutable slices.
- Represent admitted 64-bit ELF values as checked low/high `u32` halves so the
  module remains on the baseline native compiler's supported scalar surface.
- Keep full digest admission in the existing upstream identity and completed-
  application admission stages rather than importing SHA into this bounded
  placement module.

## Evidence and consequences

The native front door builds a service-free 13,509-byte WVB with SHA-256
`dfaa0fda9f10843c757ac482ad5988ce79649bf7756a53647bc093b03d0cd089`.
The reviewed focused test passes 1/1 in 2.111 seconds. It pins the transient
249,856-byte base ELF identity, proves interpreter/native equality, and
reproduces the complete 254,917-byte canonical publisher with SHA-256
`de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a`.
Truncated envelope, base-header, failed-object, and wrong-metadata cases reject.

This removes the final Linux publisher byte writer from the remaining C#
semantic gap. Windows PE materialization, normal pipeline wiring, independent
Linux execution, grouped qualification, promotion, and recovery deletion
remain. No broad Seed, OS, Standard, Qualification, WebAssembly, QEMU, or
Linux process gate ran.

## Reconsideration triggers

Version the request or response if base ELF identity, program-header topology,
note placement, component placement, metadata offset, or final identity changes.
