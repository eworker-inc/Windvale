# Decision 0742: Unify database linked-image checkpoints

- Date: 2026-08-17
- Status: Implemented with Windows development evidence; independent Linux execution pending
- Advances: [Decision 0741](0741-Checkpoint-Ordered-Multi-Object-Links.md)
- Contract: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

After ordered multi-object checkpointing, the all-hit Windows database owner
took 101,370 ms. Its portable section still took 74,110 ms even though every
project, single-object link, and hosted-application checkpoint reported `Hit`.

A measured TreeNode case spent 220 through 240 ms in project-object
materialization, 580 through 630 ms in the version-1 linked-image wrapper, 80
ms parsing and copying its image, 160 through 170 ms in hosted-application
materialization, and 340 through 350 ms in fresh execution. A controlled
identical-input benchmark measured five version-1 hits at a 641.6 ms mean and
five version-2 hits at a 107.0 ms mean. The older wrapper repeatedly entered a
batch or shell front door, started a Node key process, started host hash tools,
and compared copied products. Version 2 already accepted one input and did the
same bounded work in one cross-host Node process.

Repository search found no remaining consumer of `Build-Cached-Linked-Image`
or `Get-Native-Linked-Image-Cache-Key` after switching the database owner.
Keeping both checkpoint implementations would retain dead Windows and Linux
paths without a named compatibility requirement.

## Decision

Use `Build-Cached-Linked-Image-Set.mjs` and `linked-image-v2` for every ordinary
current-host database development link, including one-input portable images
and ordered multi-object host images. Keep explicitly segmented projects on
their distinct canonical transport checkpoint.

Retire the unreferenced version-1 Windows wrapper, Linux wrapper, key helper,
planner mappings, and database dependency declarations. Existing on-disk
version-1 entries are inert and may be removed by an external cache policy;
the repository neither reads nor mutates them. Git history remains the recovery
record. No-argument and qualification owners continue to invoke the direct
linker and retain both-host construction, byte comparison, malformed-input,
and execution evidence.

## Evidence

The version-2 regression now proves an exact one-input creation and hit in
addition to its four-process multi-input race, order-distinct keys, corruption
preservation, failed-link cleanup, and malformed-count rejection. The focused
TreeNode case falls from 1,410 through 1,490 ms to 940 through 960 ms on hits.

A coherent population run publishes 37 new ordinary single-input links, reuses
one existing single-input key, retains the three segmented cases, and executes
all 50 cases in 411,770 ms. A following all-hit run passes in 83,810 ms, and
the final change-aware all-hit run passes in 85,010 ms, down from 101,370 ms.
The final gate saves 16,360 ms or 16.14 percent and is 1.19 times faster.
Its portable section falls from 74,110 ms to 58,410 ms, saving 15,700 ms or
21.18 percent and becoming 1.27 times faster. Relative to the earlier 500,610
ms project-object-v2 result, the combined development loop saves 415,600 ms or
83.02 percent and is 5.89 times faster. These are Windows host diagnostics,
not portable timing claims.

## Consequences

Database development has one linked-image cache contract, implementation, and
failure model instead of parallel single- and multi-object paths. Exact input
count and order remain part of the key. Cold misses retain the real linker and
are reported separately from all-hit feedback.

The next measured portable costs are fresh application execution, project-
object materialization, and hosted-application materialization. A persistent
session is justified only if it preserves per-request input hashing, complete
checkpoint validation, private output verification, bounded protocol state,
clean teardown, and direct qualification independence.

## Reconsideration triggers

Reconsider this decision if a one-input version-2 image or map differs from the
direct linker, a version-1 consumer is discovered with a named compatibility
requirement, cache corruption changes private owner outputs, input count or
order fails to select another key, Linux behavior differs, qualification reads
development cache state, or session startup and repeated stable-producer
hashing again dominate measured feedback.
