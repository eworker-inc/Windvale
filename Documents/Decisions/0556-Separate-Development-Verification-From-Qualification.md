# Decision 0556: Separate development verification from qualification

- Status: Implemented
- Date: 2026-08-14
- Revises: Decisions 0525, 0526, and 0550
- Scope: GitHub verification frequency for ordinary changes and explicit
  cross-host qualification

## Context

The native retirement gate proved that Windvale can build and verify its
accepted stack without a normal .NET dependency. Its six profiles deliberately
run the complete retirement manifest, WebAssembly owner, and compiler
convergence on both permanent hosts. The workflow continued to run that whole
gate for every implementation push and pull request.

That policy confused two different questions. Development needs prompt evidence
that the owners affected by a change still pass. Qualification needs a cold,
complete, independently hosted result for one deliberately selected source
state. Repeating the latter after every incremental commit consumes substantial
time and runner capacity without strengthening a release claim, because the
next commit immediately selects a different source state.

## Decision

Ordinary implementation and specification pushes and pull requests use the new
`development` classification. GitHub runs `Verify-Changed.ps1` against the exact
base/head comparison on Windows and Linux. Its native planner selects only the
affected retirement owners in canonical order, refuses uncovered gaps, and
retains the existing editor, whitespace, plan, WebAssembly, and workflow checks
when their paths require them. These jobs are development feedback and do not
create a qualification or conformance claim.

The complete dual-host qualification matrix runs only after an explicit
`workflow_dispatch`. Use it for a release candidate, artifact promotion,
security-boundary change, bootstrap or retirement claim, or another deliberate
cross-host qualification point. The dispatch still runs all four retirement
shards, the complete WebAssembly owner, and compiler convergence on Windows and
the pinned Debian host. It remains cold and cache-independent.

Documentation-only and website-only changes retain their existing lightweight
and website gates. An empty changed-path set, unresolved comparison, or explicit
qualification request continues to fail closed to the complete qualification
scope.

## Consequences

- Every ordinary implementation commit receives affected-owner feedback on
  both Windows and Linux without running unrelated database, WebAssembly,
  bootstrap, OS, or toolchain owners.
- A passing development gate permits continued integration but must not be
  cited as cross-host qualification.
- A milestone owner selects one source state and dispatches the complete gate
  once after the coherent batch is ready; failures are corrected and the gate
  is dispatched again for the replacement state.
- Branch protection can continue to require the aggregate `Verification gate`;
  that gate now accepts the exact lightweight, website, development, or
  explicit-qualification branch selected by the classifier.

## Reconsideration triggers

Revisit the split if affected-owner mapping misses a changed boundary, focused
dual-host feedback becomes too slow for ordinary development, a release process
cannot bind manual dispatch to an exact commit, or a security policy requires a
separate scheduled full gate. Do not restore per-commit complete qualification
merely to compensate for an unmapped focused owner.
