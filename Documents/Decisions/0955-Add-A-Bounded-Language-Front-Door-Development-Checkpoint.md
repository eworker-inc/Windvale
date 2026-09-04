# Decision 0955: add a bounded language front-door development checkpoint

- Date: 2026-09-04
- Status: Implemented candidate with focused Windows execution evidence; paired
  CI measurement and complete qualification remain pending
- Extends: [Decision 0947](0947-Treat-Complete-Qualification-As-One-Evidence-Graph.md)
- Preserves: the no-argument 492-case owner, exact qualification output, fresh
  qualification, host-specific execution, and every existing case

## Context

The `language-1-front-door` qualification owner took 2,761,285 ms on Windows
and 2,362,071 ms on Linux in the completed paired-host baseline. Phase timing
shows that its compiler-scale slice alone took 2,173,270 ms on Windows and
1,891,090 ms on Linux. Running that complete path for an ordinary front-end
edit therefore exhausts the ten-minute development budget before it reaches
most downstream phases.

The first three phases already form a useful and attributable development
boundary. They verify frozen migration inputs, deterministic descriptor
construction and execution, value-front-end behavior, and the initial generic
declaration, call, resolution, and type-catalog products. Downstream numeric,
sequence, ownership, Foundation, and compiler-convergence evidence remains
necessary for qualification but is not the narrowest feedback for every edit.

## Decision

1. Add one explicit `--development` invocation to both host wrappers. It stops
   only after the first three existing phases have passed and reports 329 cases.
2. Keep the no-argument invocation and its 492-case summary unchanged as the
   complete qualification contract.
3. Make the changed-file dispatcher select `--development` only in development
   scope. Qualification orchestration continues to invoke the owner without an
   argument.
4. Replace the owner's 900-second expected and 3,600-second maximum planning
   values with 240 and 600 seconds only for that development dispatch. Retain
   the registered qualification duration profile unchanged.
5. Declare the checkpoint's fixed artifacts, producers, project inputs, and
   fixture inventory in the development dependency registry. A change to that
   closure invalidates reusable development evidence.
6. Preserve all omitted phases and cases. This checkpoint is selection, not
   sampling, deletion, or qualification evidence.

## Evidence

The exact Windows development command passed all three selected phases and
reported 329 cases in 235,533 ms. The retained input identity and limitations
are recorded in the [focused evidence record](../Evidence/2026-09-04-Language-Front-Door-Development-Checkpoint.json).

Compared with the historical 2,761,285 ms complete Windows owner, the focused
path is 91.47 percent shorter and 11.72 times faster. This comparison explains
the development benefit; it is not a claim that the current complete owner was
rerun or that the checkpoint meets the three-minute clean target.

Static verification requires the development flag, the 329-case and 492-case
summaries, exact changed-file dispatch, adjusted planning bounds, and a complete
development dependency closure. The shell wrapper also passes syntax checking.

## Consequences

- A front-door-only changed path now fits the ten-minute local development
  budget and returns useful language feedback in a few minutes.
- The checkpoint still spends most of its time constructing generic native
  products. Product sharing, rather than removing cases, is the next reduction.
- A passing development receipt must be named as development evidence. It does
  not make the compiler or Language 1.0 qualified.

## Reconsideration triggers

Expand the checkpoint only when a new case has a direct front-end failure signal
and the total remains within the development budget. Narrow or split it if
paired measurements exceed ten minutes, and retire the special mode when shared
incremental products make the complete owner comparably inexpensive.
