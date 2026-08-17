# Decision 0738: Reuse project-object checkpoint admission

- Date: 2026-08-16
- Status: Implemented with Windows development evidence; independent Linux execution pending
- Advances: [Decision 0553](0553-Content-Addressed-Project-Object-Development-Checkpoints.md)
- Contract: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

The complete all-hit database development owner still took 708,690 ms. Tool
preparation was only 2,110 ms, but the portable cases took 345,980 ms and the
host cases took another 360,600 ms. Larger ordinary project hits reported
5,000 through 13,800 ms even though their project object, linked image, and
hosted application were unchanged.

An isolated `TransactionParentGroups` project-object hit averaged 10,339.75 ms
over three runs. Running `Check-Wvo` alone over the same materialized WVO
averaged 9,164.48 ms. Process creation was not the cause: after warm-up, an
invalid current build-driver invocation took 19 through 23 ms, while real
three-, fifteen-, and eighteen-module compilations took 1,583 through 8,012 ms.

Decision 0553 admitted every WVO before immutable checkpoint publication, then
rehash-validated the checkpoint and its private materialization, but also ran
the complete structural admission process again on every hit. The key did not
bind the inspector, so simply deleting that second admission from the existing
version would have trusted entries admitted by an unspecified producer.

## Decision

Introduce `database-project-object-v2` and `project-object-v2` through one
cross-host Node driver behind the retained `.cmd` and `.sh` entry points.

The version-2 key retains the exact workspace, project manifest, ordered source
closure, build driver, and lowerer. It additionally binds the exact checkpoint
driver bytes. Those bytes carry the host-specific expected inspector digest
and the admission procedure, so a driver or inspector-policy change selects a
new key. The driver verifies the current-host inspector digest before either a
hit or a miss.

On a miss, launch independent build, lowering, and WVO-inspector processes.
Publish only after successful admission and exact product measurement. On a
hit, rehash both immutable products, compare the complete version-2 record,
copy to the private owner paths, and rehash both copies. Do not rerun the WVO
inspector over bytes already proved identical to the admitted immutable
checkpoint.

Own and remove only exact `.new-<key>-<pid>-<nonce>` candidates whose canonical
parent is the selected checkpoint family. Cleanup runs in `finally` after
build, lowering, admission, measurement, manifest, or publication-race
failure. A same-key race loser accepts the winner only after complete
checkpoint validation.

Keep the cache development-only. No-argument database verification and
qualification continue to build, lower, admit, reproduce, package both hosts,
and execute without consulting project-object checkpoints.

## Evidence

The first isolated Windows version-2 creation took 23,257.74 ms and performed
fresh compilation, lowering, and WVO admission. Its next two validated hits
took 264.10 and 250.11 ms, averaging 257.11 ms. Against the 10,339.75 ms
version-1 hit, this saves 97.51 percent and is 40.2 times faster at this cache
boundary while preserving identical WVB and WVO bytes.

The bounded checkpoint regression passes fresh creation, a validated hit,
corruption rejection with both sentinel outputs unchanged, failed-producer
cleanup, and a four-way same-key race with exactly one `Created`, three `Hit`,
byte-identical products, and no `.new-*` debris. The focused `tree-node` owner
passes both creation and hit paths; its all-hit invocation takes 4,700 ms,
including 1,890 ms of unchanged tool preparation. Node syntax, paired Git Bash
syntax, the 43-declaration dependency closure, and all 24 general plus 164
native planner cases pass locally. Independent Linux runtime evidence remains
pending and no cross-host timing claim is made.

The complete changed-file gate passes those planner contracts and all 50
database cases while populating the remaining version-2 entries in 950,050 ms,
versus the preceding 1,495,600 ms cold reference. The following all-hit owner
passes the same 50 cases in 500,610 ms, down from 708,690 ms: 208,080 ms or
29.36 percent less wall time and a 1.42-fold speedup. Its portable section falls
from 345,980 ms to 198,870 ms. All ordinary project objects, linked images, and
Windows applications report `Hit`; the explicitly segmented cases report their
unchanged rebuilt/segmented status and remain the next measured boundary.

## Consequences

Warm verification no longer converts an immutable content proof back into an
expensive structural re-admission. Cache creation remains intentionally
expensive because it preserves the full producer and admission boundary.

The driver also replaces duplicated command-shell cache parsing, hashing,
publication, and cleanup policy with one cross-host implementation. Node.js
remains a development-only dependency and does not define WVB or WVO semantics.
Version-1 entries are ignored rather than migrated or deleted.

Segmented database cases still rebuild their WVB and staging composition, and
all ordinary cases still launch separate linked-image and hosted-application
checkpoint wrappers. Those are separate measured boundaries.

## Reconsideration triggers

Reconsider this decision if a changed project, source, build driver, lowerer,
checkpoint driver, or inspector policy can select an old entry; a corrupt
product reaches an owner path; a failed or racing producer leaves unbounded
temporary state; current-host execution differs; qualification consults cache
state; or independent Linux behavior disagrees.
