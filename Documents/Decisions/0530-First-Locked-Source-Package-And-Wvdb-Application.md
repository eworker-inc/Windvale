# Decision 0530: First locked source package and WVDB application

- Date: 2026-08-13
- Status: Implemented candidate
- Requires: [Decision 0527](0527-Native-Only-Forward-Development-Boundary.md)
- Extends: [Decision 0528](0528-Workspace-Rooted-Project-2.md), [Decision 0529](0529-Native-Capability-Bearing-Library-Composition.md)

## Context

The post-retirement product lane called for one useful package-backed application
that composes portable and rights-limited hosted libraries. Project 2 and native
capability-bearing source composition supplied the build boundary, but the
repository had no deployable application owner, package manifest, deterministic
lock, or package-focused native verification owner.

The existing read-only WVDB snapshot composition was the strongest bounded
consumer. Its public result type nevertheless exposed a failure type owned by a
transitive implementation module, and no standalone command turned the composition
into useful observable behavior.

## Decision

Add `Applications/` for deployable Windvale entry points, `Projects/Applications/`
for cross-component Project 2 inputs, and `Distribution/` for checked-in package
metadata. Do not place ordinary project or package files back in the repository
root.

Select WVDB Query as the first application. It accepts a snapshot name and `u32`
key, reports found and missing results, and keeps storage, database, and usage
failures distinct. Its root explicitly approves console, diagnostic, process
argument, and read-only-directory capability requirements.

Define Package 1 and Lock 1 as separate canonical text artifacts. The package
names the root, parts, dependency edges, target, license, and capability closure.
The `local-source-1` lock pins the workspace, manifest, native compiler, Project 2
input, every source part, and expected output by byte count and SHA-256.

The first native package front door is deliberately instance-specific. It admits
only the checked-in WVDB Query manifest and lock identity, verifies all locked
resources before compilation, builds privately through Project 2, verifies the
exact result, and publishes through the pinned native publisher. A general parser,
resolver, bundle, store, installer, registry, and signature system remain future
contracts.

Make `Readˉonlyˉwvdb` own its exported database-failure status and record. Public
consumers no longer need the transitive `Windvaleˉdatabaseˉreader` module merely to
name the facade's failure payload.

## Evidence

The locked output is a 25,763-byte WVB with SHA-256
`063a23023d2baedbf21503ae43d4c3eac13a0e77bf87d0b41182518bf88f53f8`.
Inspection reports 26 functions, 19,804 code bytes, and exactly five capabilities.

The paired native package owner has eight cases: two builds, deterministic byte
comparison, capability inspection, three negative admission cases, and failed
output preservation. The changed-file planner maps the application, manifest,
lock, Project 2 input, package tools, and locked library parts to this owner.

## Consequences

- The repository has a useful application boundary and a reproducible offline
  source-package baseline without widening Project 2.
- Package metadata remains separate from build-input selection and cannot grant
  runtime authority.
- Library facades own the public failure vocabulary they expose.
- The exact initial front door is honest about its bounded scope; accepting a
  second package requires a real parser/resolver decision rather than copied hash
  logic.
- Native execution is not qualified by this slice. The current native runner does
  not bind `filesystem.directory_read_v1`, so cross-host execution, provider
  binding, and denial evidence remain follow-up work.

## Reconsideration triggers

Revisit the text formats when a second package requires general resolution, binary
or resource parts, target variants, optional capabilities, a content-addressed
store, signed bundles, or installed generations. Revisit the application profile
when the native runner can bind a rights-limited directory provider.
