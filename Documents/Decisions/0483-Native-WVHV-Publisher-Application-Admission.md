# Decision 0483: Native WVHV publisher application admission

- Status: Implemented source candidate; hosted execution and durable promotion pending
- Date: 2026-08-09
- Advances: [Decision 0482](0482-Native-WVHV-Publisher-Base-Construction.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [publisher application admission](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Application-Admission.md)

## Context

Decision 0482 removed the managed base builder from exact publisher
construction, but the command still ended with a host-side length/digest check
and a non-durable copy. The existing console publisher cannot close that gap:
it admits version-1 and hosted format-2 applications, while these publisher
subjects are specialized format-4 containers.

The completed publisher cannot safely admit its own digest. Adding that digest
to its source changes the resulting application and therefore changes the
digest again.

## Decision

- Add a distinct portable Windvale admission module that pins the exact
  256,000-byte Windows and 254,917-byte Linux publisher applications.
- Add one read-only hosted command source with no mutation capability.
- Build and pin its canonical WVB through the native compiler in the existing
  publisher-construction candidate.
- Do not reuse an unrelated hosted profile. Profile 7 is already owned by the
  hosted-container segmenter.
- Do not compose a separate admission process with a host copy or rename.
  Admission and durable replacement must later share one immutable snapshot in
  a distinct installer application.

## Evidence and consequences

The native source front door produces a 30,325-byte WVB with SHA-256
`cdcda2e2bcdb7915a769ab9a79f7434e2b26bfbf4e0412a183bd7525769ef954`.
Version 8 of the construction candidate pins that WVB in its 43-entry
inventory, and the existing focused native lane rebuilds it byte for byte as
part of the inventory case. One bounded recovery-interpreter execution accepts
the exact 256,000-byte Windows publisher, and a target swap rejects before
hashing the mismatched-length input.

The current accepted native lowerer still reports `Unsupportedˉcode` for the
new WVB. Therefore this decision establishes exact Windvale semantic ownership
and reproducible WVB construction, but it does not claim a native admission
process, durable installation, Linux execution, promotion, or qualification.
No broad verifier is part of this slice.

## Reconsideration triggers

Repin this contract if either completed publisher identity changes. Version the
hosted application profile when the accepted lowerer can construct the command.
Replace this standalone read-only boundary with the later installer only when
the same immutable snapshot drives admission, staging, reread, and atomic
replacement.
