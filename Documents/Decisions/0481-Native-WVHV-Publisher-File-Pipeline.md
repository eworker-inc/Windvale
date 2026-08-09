# Decision 0481: Native WVHV publisher file pipeline

- Status: Implemented current-host candidate; native base construction and packaged execution pending
- Date: 2026-08-09
- Advances: [Decision 0480](0480-Native-WVHV-Publisher-Windows-Materialization.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher file pipeline](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-File-Pipeline.md)

## Context

Decisions 0475 through 0480 transferred exact publisher metadata, source and
object identity, layout, object instantiation, Windows imports, and final PE/ELF
mutation into service-free Windvale modules. Their focused evidence still
assembled request envelopes and passed byte values directly from the managed
test harness. No hosted Windvale process connected the records through files.

The metadata boundary also needed a non-circular producer. `WVPI` admits
`WVVP`, so deriving `WVPM` from `WVPI` would make reconstruction depend on the
metadata it was meant to create.

## Decision

- Add one metadata producer that admits the exact publisher WVB and target
  startup WVO, constructs `WVPM 1`, invokes the existing metadata constructor,
  and writes the final admitted `WVVP 1`.
- Add hosted wrappers that form `WVIX 1`, the fixed Windows import request, and
  bounded Windows/Linux materialization envelopes from prior versioned files.
- Preserve service-free constructors and their existing identities. The hosted
  tools are adapters, not duplicate semantic implementations.
- Reject byte-identical input/output path arguments before mutation and preserve
  destinations on semantic rejection. Treat the tools as private pipeline
  stages; final admission and durable replacement remain orchestration duties.
- Keep the modules focused. The seven new Windvale source files are 19 through
  117 lines; the integrated test is a separate focused owned file.

## Evidence and consequences

Five WVBs build through the digest-bound native front door and are pinned in
version 6 of the publisher-construction candidate manifest. The reviewed
focused test passes 1/1 in 12.523 seconds, after a one-time Release build, and
reproduces both canonical publisher applications byte for byte:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 256,000 | `7ba65ba1bd74511339a2fc6772ded8ad6b71fa7f1246b2b8e10f50c8a6f80d95` |
| Linux x64 | 254,917 | `de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a` |

The test additionally rejects a corrupt publisher WVB, a zeroed ordered target,
and an exact path-text alias while preserving the existing destination or input. The
metadata producer separately admits the startup WVO identity but writes the
digest of the instantiated five-byte startup into `WVPM`; confusing those two
identities was caught during pre-run review.

This closes the in-memory-only seam after generic base construction. It does
not yet close the complete normal pipeline: the focused test deliberately uses
the frozen managed builder to supply the generic six-service base application.
The next slice must compose that base through the existing native `WVHV`
request, runtime, bundle, startup, platform, and container processes, keep the
intermediates private, and run complete final application admission before
durable publication. No broad
Seed, OS, Standard, Qualification, WebAssembly, QEMU, or Linux process gate ran.

## Reconsideration triggers

Version the tools or records if publisher WVB identity, startup WVO or image
identity, ordered target ownership, input envelope geometry, final application
identity, or alias/rejection behavior changes. Combine stages only if the
result preserves reviewable ownership and stays within the source-WIR binding
contract without weakening admission.
