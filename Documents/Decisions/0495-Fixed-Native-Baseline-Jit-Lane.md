# Decision 0495: Fixed native baseline-JIT lane

- Status: Accepted current-host fixed evidence
- Date: 2026-08-10
- Scope: `WVJP 1` construction/admission and the bounded `WVLT 1` W^X publisher
- Extends: [Decision 0424](0424-Paired-Native-Baseline-Jit-Publication.md), [Decision 0458](0458-Native-Changed-File-Verification.md), and [Decision 0494](0494-Native-Compiler-Reconstruction.md)
- Retains: the managed complete backend, recovery-only Windows import constructor, and pending final retirement gate

## Context

The paired baseline-JIT patch-plan and publisher launchers already build, admit,
publish, execute, and release their bounded candidates without loading .NET.
Decision 0424 records paired Windows/Debian execution. These exact checks were
not owned by the manifest-driven retirement coordinator, so changes to their
sources, adapters, fixtures, projects, artifacts, or specifications selected
generic compiler evidence or an unmapped gap instead of their direct native
owner.

The candidate is deliberately not a general JIT. The Windows launcher rebuilds
and verifies the unpatched console base but executes the retained import-bound
PE; adding those import-directory entries remains an explicit recovery
PowerShell operation.

## Decision

- Add one paired `Test-Baseline-Jit` wrapper that requires the exact terminal
  result of the existing patch-plan and publisher launchers.
- Count one aggregate producer/verifier self-test plus the publication
  contract's five explicit corrupted-plan, result, permission-transition, and
  release behaviors.
- Register those six cases as one fixed `baseline-jit` retirement-suite owner.
- Route the exact compiler/runtime/WVA sources, fixture, projects, candidate
  artifacts, child launchers, and specifications to that owner.
- Keep general lowering, complete runtime integration, code-cache policy,
  paired-host renewal, promotion, and the managed complete backend open.

## Current-host evidence

The Windows retirement coordinator ran only:

```text
Test-Retirement-Suite.cmd --filter baseline-jit
```

It passed one selected suite and all six cases in eight seconds:

```text
PASS  suite baseline-jit cases=6
Suites: 1, Passed: 1, Failed: 0, Cases: 6
```

No compiler reconstruction, Stage 2, broad Seed/OS, Standard, Qualification,
Linux execution, or unfiltered retirement suite ran.

## Consequences

The fixed native coordinator grows to 34 suites and 3,177 cases. Baseline-JIT
changes now select their direct .NET-free evidence owner instead of a generic
compiler lane or named gap. This is an evidence transfer: it removes no direct
managed entry from the retirement inventory and does not advance N2 beyond its
bounded constant-return profile.

## Reconsideration

Reconsider this decision if either child launcher stops reproducing its exact
intermediates, if the two host adapters no longer share the same `WVJP 1` and
`WVLT 1` admission, if Windows final import construction becomes native, or if
the profile expands enough to require independently named lowering, cache, or
concurrency owners.
