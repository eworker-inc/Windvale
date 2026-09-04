# Decision 0945: separate database behavior from cross-target packaging

- Date: 2026-09-04
- Status: Implemented candidate for the portable database lane; hosted migration
  and paired-host qualification remain pending
- Extends: [Decision 0557](0557-Separate-Development-Verification-From-Qualification.md)
- Extended by: [Decision 0946](0946-Delegate-Portable-Database-Reproducibility-To-Toolchain-Owners.md)
- Extended by: [Decision 0950](0950-Bundle-Compatible-Database-Ancestor-Cases.md)

## Context

The database qualification owner historically compiled and lowered each test
project twice, linked it, executed the current-host application, and also built
an application for the other host that it could not execute. This coupled a
database behavior claim to the complete generic packaging pipeline for every
case. The complete qualification workflow already runs the database owner on
Windows and Linux, and focused console/container/package owners independently
exercise cross-target PE and ELF construction.

The coupling consumes material time while providing no additional database
execution. As database cases accumulate, it repeats the same generic packaging
claim once per domain behavior.

## Decision

1. A database qualification step retains independent A/B source-to-WVB and
   WVB-to-WVO construction wherever that step currently owns deterministic
   compilation and lowering.
2. Each host packages and executes its own native database application. Windows
   executes the Windows application and Linux executes the Linux application.
3. The database owner no longer builds the other host's unused application for
   every portable case. Cross-target PE/ELF construction remains owned by the
   focused console packager, container reconstruction, and package-format
   qualification owners.
4. Complete database qualification requires the database owner on both hosts.
   Complete repository qualification composes those results with the focused
   generic packaging owners. A single-host database result is not paired-host
   qualification.
5. Database summaries state `current-host-behavior=Verified` and
   `cross-target-packaging=Delegated`; they no longer claim that every database
   step independently verified both target images.
6. A platform-specific database adapter or package-layout contract must declare
   its host or packaging owner explicitly. It cannot rely on this delegation if
   its failure is not observable through current-host behavior or a focused
   packaging owner.
7. Focused `--qualification-step` execution is diagnostic evidence for one
   complete cold node. It never becomes a complete qualification claim.

## Evidence

Before this separation, the first two-case portable bundle passed focused
Windows qualification with independent A/B compilation and lowering, WVO
admission, native link, Windows execution, and both target packages. A direct
clean comparison measured the two separate products at 71,830 ms and the
combined product at 40,608 ms, a 43.47 percent reduction.

After the separation, the same publication/recovery node passed focused Windows
qualification in 46,150 ms and reported both logical cases, independent A/B
construction and lowering, WVO admission, native linking, and current-host
behavior. Planner, dependency, documentation, and shell-contract verification
passed after the change. Linux and the complete 57-case paired gate remain
pending and no qualification promotion is claimed here.

## Consequences

- Portable database elapsed time no longer includes one unused cross-target
  package per construction step.
- Database behavior remains native on both required hosts in complete
  qualification.
- A generic packaging regression is reported by its focused owner instead of
  being rediscovered across many unrelated domain cases.
- The hosted database lane retains its current cross-target construction until
  its provider and platform-object ownership is separated with equivalent
  evidence.

## Reconsideration triggers

Reintroduce a target package to a database step only when that exact package
contains database-specific target behavior not exercised on the native host or
owned by a focused packaging verifier. Reconsider the split if the aggregate
qualification planner can omit a required packager owner, if paired host
composition is not fail-closed, or if native-host outputs cease to bind the
exact portable WVB/WVO identities used by the database behavior.
