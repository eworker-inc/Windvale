# Decision 0559: Checkpoint portable database development targets

- Status: Implemented
- Date: 2026-08-14
- Extends: Decisions 0553, 0554, 0555, and 0556
- Scope: repeated affected-owner feedback for the native database path

## Context

The eight-case database development owner already reused the compiler tool,
project-WVB, host-storage project/object, and two native-host application
checkpoints. Its six portable tree targets still rebuilt, lowered, admitted,
linked, and packaged unchanged products. The measured warm Windows owner took
402,638 ms, with those six targets accounting for nearly all of the interval.

The existing project/object and hosted-application checkpoints already bind the
complete source and producer identities needed by these products. Adding a new
cache format would duplicate their validation contracts. Development also needs
only the current host's application container: GitHub runs the affected owner
independently on Windows and Linux, while complete qualification retains paired
cross-target construction.

## Decision

- Route all six portable database development targets through the existing
  content-addressed project/object checkpoint.
- Route each target's current-host application through the existing hosted
  application checkpoint and execute it on every run. Do not construct the
  non-host container during development; retain both Windows and Linux
  construction in the unchanged complete owner.
- Treat successful project/object checkpoint admission as the one development
  WVO admission for that copied product. Do not immediately repeat the same
  admission in the parent owner. Complete reconstruction retains its independent
  deterministic builds, comparisons, and admission.
- Report live `START` and `PASS` records for tool preparation, every portable
  target, host storage, and the host tree reader. Keep the final structured
  status record last for existing consumers.
- Report tool, portable-target, host-storage, host-tree-reader, and total elapsed
  milliseconds. Report every portable project and application checkpoint result.

## Evidence

The cache-population Windows run passed all eight cases in 703,310 ms. It created
the compiler tool, all six portable project/object checkpoints, all six Windows
and six Linux application checkpoints used by the initial implementation, and
the two native-host application checkpoints.

The first all-hit run took 152,320 ms. Restricting development packaging to the
current host and removing duplicate parent admission produced a second all-hit
run in 135,670 ms:

| Phase | Elapsed |
| --- | ---: |
| Tool preparation | 9,010 ms |
| Six portable targets | 71,880 ms |
| Host storage | 29,240 ms |
| Host tree reader | 25,450 ms |
| Total | 135,670 ms |

Every reported project and current-host application checkpoint was `Hit`, and
all eight behavioral cases executed successfully. Compared with 402,638 ms,
the repeated owner is 66.30% faster. It remains 15,670 ms above the roadmap's
two-minute working target, so this decision does not close Milestone 1.

## Consequences

- Repeated database work now has visible progress instead of one silent interval.
- Current-host development avoids constructing a product it cannot execute;
  independent Windows/Linux development and cold cross-target qualification
  retain the platform evidence boundaries.
- Cache population remains expensive and is not release evidence. Qualification
  ignores development cache state.
- The next reduction must address measured repeated admission/link work or use
  explicitly bounded independent execution. It must not skip the cache trust
  boundary or remove behavioral cases merely to cross the timing target.

## Reconsideration triggers

- A development environment needs to inspect the non-host container from the
  same run.
- Current-host checkpoint validation ceases to include a complete dependency
  identity or required structural admission.
- Repeated measurements show that batching dependency admission or bounded
  concurrency cannot close the remaining local-time gap safely.
