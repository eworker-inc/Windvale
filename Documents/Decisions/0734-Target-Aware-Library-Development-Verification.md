# Decision 0734: Target-aware library development verification

- Date: 2026-08-16
- Status: Implemented with focused Windows execution evidence; independent Linux execution pending
- Extends: [Decision 0557](0557-Separate-Development-Verification-From-Qualification.md)
- Preserves: the complete 29-project library owner and cold qualification

## Context

The library owner had grown to 19 reusable/import projects, eight conformance
projects, and two negative projects. Every changed library source, fixture, or
project selected all 29 builds even when the affected contract occupied one
small dependency closure. On the measured Windows host, the complete owner took
26.24 seconds before this change and 26.348 seconds after it.

The projects form seven coherent clusters rather than 29 independent contracts:
resource store, storage geometry, page/storage, durability, read-only WVDB,
models, and capability rejections. Some sources are shared across clusters.
For example, `Foundation/Byte-Construction.wv` participates in durability and
WVDB reader projects and therefore cannot safely select only one cluster.

## Decision

- Add a canonical manifest assigning every one of the 29 projects to one of
  seven development targets and to its project, conformance, or negative
  evidence kind.
- Derive each target's input closure from the `root` and `source` declarations
  in its Project 2 files. A project declaration change therefore updates the
  planner closure without requiring a second copied source list.
- Select one development target only when every path that selects the library
  owner maps to that same target. Shared inputs, multiple targets, owner-script
  changes, and unrecognized inputs that select the owner fail closed to the
  complete route.
- Add `--development-target <target>` to the paired library owners. The focused
  route builds every positive and conformance project assigned to the target
  and retains failed-output preservation for assigned negative projects.
- Keep no-argument execution and qualification unchanged at 19 projects,
  eight conformance builds, two negative builds, and 29 total cases.
- Validate the manifest shape, target set, evidence totals, project uniqueness,
  declared input existence, paired owner support, exact selection, same-target
  composition, multi-target fallback, and owner-change fallback in the native
  verification-plan contract.

## Evidence

Planner verification passes 24 general and 163 native cases. On Windows, the
three-case `models` target passed in 4.32 seconds, and the largest nine-case
`page-storage` target passed in 5.878 seconds. The complete 29-case owner passed
in 26.348 seconds, making those focused routes about 6.10 and 4.48 times faster
respectively. The two-case `capability-rejections` target passed, an unknown
target rejected with exit code 64, and the Linux owner passed Git Bash syntax
validation.

These timings are diagnostic host evidence, not portable thresholds.
Independent Linux behavior execution remains required before claiming paired-
host execution evidence for this development route.

## Consequences

Eligible library edits now rebuild one coherent dependency cluster rather than
all 29 projects. The complete owner remains the fallback and qualification
oracle. The manifest is also the shared inventory used by both focused owner
scripts, replacing target-specific command duplication.

Changes to modern library areas outside this 29-project owner continue to use
their existing focused owners. This decision does not claim that the generic
library owner verifies projects absent from its complete inventory.

## Reconsideration triggers

Reconsider this decision if Project 2 gains dependency declarations that the
planner does not parse, a project belongs to more than one evidence target, a
focused target differs from the corresponding complete-owner subset, Linux
execution differs, or measurements show that process startup dominates even the
smallest clusters.
