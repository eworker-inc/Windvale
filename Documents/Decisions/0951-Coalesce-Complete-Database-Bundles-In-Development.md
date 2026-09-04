# Decision 0951: coalesce complete database bundles in development

- Date: 2026-09-04
- Status: Implemented candidate with focused Windows execution evidence; Linux
  syntax verification passed and Linux execution remains pending
- Extends: [Decision 0944](0944-Select-Exact-Database-Development-Target-Sets.md)
  and [Decision 0950](0950-Bundle-Compatible-Database-Ancestor-Cases.md)
- Extended by: [Decision 0952](0952-Rank-And-Trial-Overlapping-Database-Products.md)
- Preserves: exact changed-case selection, all 53 development behaviors,
  case-level failure attribution, and qualification freshness

## Context

Four compatible database case pairs already shared one product during
qualification. Their development selectors correctly chose both logical cases,
but the development owner still dispatched the two original projects. A bundle
edit therefore appeared focused while retaining the duplicate compiler,
lowerer, linker, packaging, and process work that the bundle was meant to
remove.

Development progress also counted selected cases as if each case necessarily
owned one execution. That made the planned work and the executed work diverge
as soon as a product covered more than one behavior.

## Decision

1. Extend the versioned database development inventory with an optional bundle
   label for a portable case. The development planner must reject a bundle
   unless its ordered member list exactly matches the corresponding portable
   qualification step.
2. Report logical cases and physical executions separately. The planner emits
   the selected cases, selected execution count, complete bundle labels, and
   bundled member names.
3. Coalesce a bundle only when every member is selected. A one-case selection
   continues to build and execute the original one-case project.
4. Make the Windows and Linux development owners dispatch each selected bundle
   once and skip its constituent project calls. Progress and terminal evidence
   report the physical execution count while retaining every logical case name.
5. Keep development checkpoints content-addressed. Qualification does not read
   or publish these cached development results and remains a fresh evidence
   path.

## Evidence

The all-development plan now reports 53 logical cases in 49 executions. The
`publication+recovery` target reports two cases in one execution, while the
`publication` target reports one case in one execution and no selected bundle.
The planner binds all four bundle member lists to the qualification inventory.

Focused Windows development execution produced the following results:

- the first `publication+recovery` bundle checkpoint completed in 44,850 ms;
- the unchanged warm bundle completed in 2,130 ms, including 560 ms of tool
  preparation, with project, link, and hosted-application cache hits;
- the partial `publication` selection completed independently in 2,100 ms and
  reported only `Publication`;
- the partial `recovery` selection completed independently in 1,980 ms and
  reported only `Recovery`.

The owner-plan verifier passed all 31 general and 277 native routing cases. The
Linux owner passed Bash syntax validation. Complete database development and
paired-host execution were not run.

## Consequences

- Full database development performs four fewer compiler-product executions
  without removing a behavior.
- Bundle maintenance and exact pair selectors reach the seconds-scale warm path
  instead of dispatching both original projects.
- A future bundle must be declared once in qualification and referenced by its
  development cases; divergent membership is a planning failure.
- Case count can grow independently from execution count, so future test growth
  no longer implies one new compiler process per assertion group.

## Reconsideration triggers

Split a development bundle if its members acquire different profiles,
capabilities, mutable state, limits, or isolation requirements. Replace this
pair-level mechanism with a general product/case graph when another owner needs
the same distinction or the database owner accumulates enough bundles that
hard-coded dispatch becomes the next material maintenance cost.
