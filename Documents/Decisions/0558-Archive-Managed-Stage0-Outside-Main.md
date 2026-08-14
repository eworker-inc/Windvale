# Decision 0558: Archive managed Stage 0 outside main

- Status: Implemented; post-archive dual-host qualification pending
- Date: 2026-08-14
- Revises: Decisions 0526 and 0527
- Scope: repository maintenance and Stage 0 recovery custody

## Context

Decision 0526 qualified the native Windows and Linux workflow and published an
immutable, independently retained Stage 0 recovery release. Decision 0527 then
froze the managed implementation in `main`. Keeping 365 tracked C# source,
project, solution, and SDK files plus nine executable recovery entry points in
the active tree made that historical implementation look current, kept obsolete
paths in verification planning, and invited accidental maintenance of two
toolchains.

The qualified release already preserves the exact source, history, dependencies,
licenses, artifacts, runbook, reports, and checksums. Recovery is more coherent
when it begins from that exact state in a separate checkout instead of assuming a
future `main` tree remains compatible with the frozen projects.

The existing `stage0-recovery-e5a1a7473c57` tag is the final pre-removal
managed release. It is deliberately not called `v0.1.0` or `v1.0.0`: the former
remains the first product-preview gate and the latter requires a future stability
decision.

## Decision

Remove managed Stage 0 source, managed projects, solution and SDK metadata,
managed tests, Blazor host files, and direct managed recovery commands from
`main`. Preserve Windvale source, WVA/WVO/WVB/WVNF products, native tools, the
static playground, and historical decisions.

`Bootstrap/Stage0/README.md` is the in-tree recovery pointer. The machine-readable
retirement inventory records the exact release, commit, tree, and checksums. The
retirement verifier fails if tracked managed source/build metadata or a direct
managed invocation returns to an operational path.

Managed recovery work must start from the immutable release in a separate
workspace. Reintroducing managed source or a `dotnet` entry point to `main`
requires a later decision naming the failed native or recovery contract. Ordinary
development, affected-owner verification, packaging, execution, and release
preparation remain native-only.

This change does not rewrite the historical claims in Decisions 0526 and 0527:
they accurately describe the repository when accepted. It changes the current
custody model from a frozen live copy to an immutable external archive.

## Verification

The implementing commit must pass the repository inventory/planning guards and a
focused native front-door check. One explicit complete Windows/Linux
qualification dispatch is required before the resulting commit is cited as a
post-archive qualified state or tagged `native-only-baseline-<commit12>`. The
dispatch is not restored as a per-commit gate.

## Consequences

- The active tree has one implementation language path for forward compiler,
  runtime, object, linker, package, browser, and OS work.
- Cloning `main` no longer implies an SDK installation or exposes stale managed
  commands as if they were maintained.
- Historical managed differential tests are available only by restoring the
  qualified release; current native fixtures own forward evidence.
- Recovery custody depends visibly on release availability, checksum integrity,
  and the independently retained copy.

## Reconsideration triggers

Revisit this decision if the published assets or independent copy fail checksum
verification, the documented release cannot reconstruct on either supported
host, or a security defect makes the archived recovery procedure unsafe.
Convenience, an unmapped native test, or a desire for implementation parity is
not sufficient reason to restore managed source to `main`.
