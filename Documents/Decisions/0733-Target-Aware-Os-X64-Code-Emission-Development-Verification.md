# Decision 0733: Target-aware OS x64 code-emission development verification

- Date: 2026-08-16
- Status: Implemented with focused Windows execution evidence; independent Linux execution pending
- Extends: [Decision 0557](0557-Separate-Development-Verification-From-Qualification.md)
- Preserves: the complete 56-project, 336-case owner and cold qualification

## Context

The OS x64 code-emission owner contains 56 independent project closures. Its
complete route builds a WVB, lowers a WVO, links an image, packages Windows and
Linux containers, checks every exact byte identity, and executes the current-host
container for every project. The changed-file planner selected that complete
owner even when one changed source, fixture, or project belonged to one exact
closure.

An audit of the 109 commits made on 2026-08-16 found the owner selected by 30
commits. Repeating 336 cases for a leaf change made local verification dominate
implementation time without adding evidence about the other 55 unchanged
closures.

## Decision

- Add a canonical manifest mapping each of the 56 target names to its project,
  root fixture, and complete declared source closure.
- Select one development target only when every changed OS x64 code-emission
  input maps to that same target.
- Fail closed to the complete owner for a shared input, multiple targets, an
  owner-script change, or any ambiguous closure. Reject malformed target data
  during planner verification.
- Add `--development-target <target>` to the paired owner scripts. A focused
  execution retains six checks: exact WVB, WVO, linked-image, Windows-container,
  and Linux-container identities plus current-host execution.
- Keep no-argument owner execution and qualification unchanged at 56 projects
  and 336 cases.
- Validate target uniqueness, project declarations, repository inputs, paired
  owner selectors, planner selection, multi-target fallback, and owner-change
  fallback in the verification-plan owner.

## Evidence

Planner verification passes 24 general and 158 native cases. Windows focused
execution passed the first, middle, and final target in 3,476 ms, 3,622 ms, and
3,041 ms respectively. The complete changed-file front door selected the middle
target and passed in 4,866 ms. An unknown target rejected with exit code 64, and
the Linux owner passed Git Bash syntax validation. The complete Windows owner
then passed all 56 projects and 336 cases in 115,333 ms. Compared with the
3,622 ms middle-target run, exact owner feedback is 31.84 times faster, a 96.86
percent wall-clock reduction for an eligible leaf edit.

These timings are diagnostic host evidence, not portable thresholds. Independent
Linux behavior execution remains required before claiming paired-host execution
evidence for this development route.

## Consequences

Leaf OS x64 emission edits now execute one project and six cases instead of 56
projects and 336 cases. Shared code and qualification retain the full evidence
boundary. The manifest adds one small maintained mapping surface, but removes
the need for the planner to reread all project files on every selection.

The owner scripts remain mechanically repetitive. A later refactor may make the
target manifest drive one generic project executor, but that refactor is not
required to obtain the bounded development-path reduction.

## Reconsideration triggers

Reconsider this decision if a project dependency can change without appearing
in the manifest, a focused run differs from the corresponding complete-owner
case, shared inputs select one target, Linux target execution differs, or a
generic manifest-driven owner can replace the paired repeated scripts without
weakening exact-byte evidence.
