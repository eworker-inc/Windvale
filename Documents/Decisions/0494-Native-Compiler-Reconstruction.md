# Decision 0494: Native current-compiler reconstruction

- Status: Accepted current-host candidate
- Date: 2026-08-10
- Scope: current compiler WVB, segmented native image, paired hosted applications, and fixed reconstruction evidence
- Extends: [Decision 0491](0491-Build-Driver-Profile-Capacity.md), [Decision 0492](0492-Hosted-Container-Toolset-Reconstruction.md), and [Decision 0493](0493-Native-WVHV-Publisher-Overlay-Reconstruction.md)
- Retains: the last qualified native compiler seed, frozen Stage 0 recovery evidence, and the pending grouped dual-host retirement gate

## Context

Decision 0491 advanced the current compiler source inventory to a 921,640-byte
WVB and proved current-host build-driver self-convergence. Its exact native
compiler closure still carried 419-function pins from the preceding candidate,
and the paired compiler applications predated the shared startup, file-input,
and hosted-container reconstruction. Those stale values made the extended
recovery oracle fail before it could describe the current source state.

The retained `Native-Compiler-Seed` is qualified bootstrap provenance. Replacing
that inventory with a current, Windows-only reconstruction would confuse a
reproducible seed with an unqualified candidate.

## Decision

- Measure the current native closure once through the independent managed
  recovery oracle while skipping both long Stage-2 executions.
- Advance exact record-storage, descriptor-ownership, native image, WVO, link,
  bundle, metadata, runtime-header, verifier, and paired application expectations
  to the measured 427-function closure.
- Reconstruct the canonical WVB and both target applications through the
  digest-bound native seed, segmented lower/link/transport path, and Decision
  0492 hosted-container toolset.
- Retain those three products under a distinct native compiler reconstruction
  candidate. Do not overwrite or relabel the qualified seed.
- Add one fixed native retirement-suite owner for candidate inventory, usage
  rejection, and exact paired reconstruction. Keep full compiler execution in
  the grouped final gate rather than repeating it during artifact repinning.

## Current-host evidence

The recovery oracle's artifact-only filter completed in 55.752 seconds. It
measured 27,635,298 native image bytes at SHA-256
`80a3ebd54244487bdeafac7b6ebd6c11e1bd839c068405b6507aed83748ff3eb`,
a 27,657,722-byte WVO at
`e0a334a805883fe443ed0c7a95b578a076104ea691e29c4e6ed87bf7af63108b`,
and the current record/descriptor accounting. A second combined accounting-only
filter reused one native compilation, passed in 12.891 seconds, and did not run
Stage 2. Temporary measurement hooks were removed immediately afterward.

The native source bootstrap and segmented lower/link/transport path then emitted
the same 921,640-byte WVB and 27,635,298-byte image with entry offset 43,146 and
seven canonical chunks. Without repeating that work, the retained chunks were
packaged for both targets. The final Windows application is 27,666,432 bytes at
`c1be8bd7e2c9496fee0cd3e486348804469d72621bcf45e30d8b6e8a1814da9c`;
the Linux application is 27,668,480 bytes at
`25905e75e836ad8015a851aa6a52531bf5ab73c9dd97596628c2226740f37a34`.
Both match the independent recovery oracle exactly.

The permanent three-case reconstruction owner was reviewed but not rerun after
the equivalent outputs had already been produced once and retained as the
candidate. The long current-compiler Stage-2 execution, broad Seed, independent
Linux execution, Standard, Qualification, and the grouped retirement gate did
not run.

## Consequences

The current compiler now has one explicit .NET-free construction path and a
candidate inventory distinct from its qualified seed. Exact native-closure tests
describe the current WVB rather than a preceding 419-function module. The
retirement coordinator grows to 33 suites and 3,171 fixed cases.

This is reconstruction, not promotion. The exact full-source text-arena use and
Stage-2 output are still final-gate evidence; the retained prior qualified seed
and Stage 0 recovery archive remain unchanged.

## Reconsideration

Reconsider this decision if the independent Linux path cannot reproduce the
three candidate identities, if the native segmented image differs from the
recovery oracle, if the current full Stage 2 does not reproduce the 921,640-byte
WVB, or if a later source state cannot consume the promoted prior seed.
