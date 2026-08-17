# Decision 0747: Make Compiler Front-Door Admission Target-Aware

- Status: Accepted
- Date: 2026-08-17
- Scope: ordinary changed-file verification for compiler sources, compiler
  service projects, and the native source-compiler product launcher

## Context

A representative compiler source change selected six common native owners. On
the Windows development host their exact coordinator invocations measured:

| Owner | Elapsed |
| --- | ---: |
| `seed` | 8,937.272 ms |
| `seed-native-front-door` | 14,735.859 ms |
| `unsafe-wvb` | 9,443.176 ms |
| `source-containment` | 17,434.616 ms |
| `lowerer-rejections` | 1,577.314 ms |
| `console-packager-source-reconstruction` | 7,485.437 ms |
| **Total** | **59,613.674 ms** |

The front-door owner hashes 18 checked-in artifacts totaling 74,827,370 bytes
and admits six checked-in WVB modules. None of those inputs is produced from the
working compiler source during that owner invocation. A separate measurement
showed the 18 hashes themselves take about 132 milliseconds when executed in
one process, while six independent current-host WVB admissions take about 11.5
seconds. Hash memoization therefore would optimize the smaller mechanism while
retaining a stale affected-owner selection.

The compiler plan already runs behavioral owners over the source language,
malformed WVBs, source containment, lowerer rejection, and packager-source
reconstruction. The source-compiler product launcher additionally selects the
compiler reconstruction owner. Re-admitting the unchanged pinned distribution
does not add compiler-change evidence.

## Decision

- Remove `seed-native-front-door` from the maintained compiler-service source,
  project, and example mapping.
- Remove it from the native source-compiler product-launcher mapping.
- Select `seed-native-front-door` directly for every path under
  `Artifacts/Native-Front-Door/`, including its exact manifest and checksum
  inventory.
- Preserve the owner command, its 18-artifact and six-module checks, registry
  entry, qualification shard, and no-argument qualification behavior.
- Preserve every existing compiler behavioral owner. This decision removes one
  independent pinned-artifact admission; it does not replace or weaken a
  source-semantic, malformed-input, containment, lowering, or reconstruction
  check.

This is affected-owner selection, not a trust cache. The planner never infers
artifact integrity from timestamps or mutable metadata, and an actual
front-door artifact edit still runs the complete admission owner.

## Consequences

The profiled compiler loop removes 14,735.859 milliseconds, or 24.72 percent of
its six-owner elapsed time, without changing any executed compiler behavior.
The front-door owner is now aligned with the checked-in family it validates.

Other historical source-transfer mappings still select the front-door owner.
They remain candidates for the same dependency audit, but this decision does
not remove them without first confirming their independent behavioral owners.

## Evidence

The changed-file planner proves that a representative compiler core path and
the source-compiler product launcher no longer select
`seed-native-front-door`, while both front-door manifest inputs select it with
zero gaps. The complete planner self-test passed 24 general and 166 native
routing cases, and all 62 declared development-owner dependencies passed.

An end-to-end changed-file run for `Compiler/Windvale/Source-Lexer-Core.wv`
selected exactly `seed`, `unsafe-wvb`, `source-containment`,
`lowerer-rejections`, and `console-packager-source-reconstruction`. It passed
all 550 owner cases plus the editor contract in 43,732.204 milliseconds. No
front-door admission owner was invoked.

## Reconsideration triggers

Reconsider if the front-door owner begins constructing products from current
compiler sources, if one of the retained compiler owners ceases to execute the
changed boundary, or if qualification stops independently admitting the pinned
front-door family.
