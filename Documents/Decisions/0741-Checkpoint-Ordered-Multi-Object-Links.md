# Decision 0741: Checkpoint ordered multi-object links

- Date: 2026-08-17
- Status: Implemented with Windows development evidence; independent Linux execution pending
- Advances: [Decision 0740](0740-Reuse-Hosted-Application-Producer-Trust.md)
- Contract: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

After hosted-application producer trust reuse, the warm Windows 50-case
database owner took 281,240 ms. Its host-root-writer section alone took 61,810
ms even though every project and application checkpoint reported `Hit`.

Phase measurement rejected the initial process-startup hypothesis. Twelve
fresh main-case publication, replay, interruption, and recovery executions
took 980 ms. Preparation took 17,030 ms: project admission was 310 ms,
application materialization was 190 ms, and the direct three-object link was
16,460 ms. The related root-fill, root-split, and read links took 15,180,
15,650, and 10,370 ms; their project and application hits remained below 310
ms and 190 ms respectively. Other host database cases repeated the same
`application WVO, shared storage WVO, current-host provider WVO` link shape.

The existing `linked-image-v1` checkpoint accepts one WVO. Collapsing three
objects into an invented combined object would change linker evidence, input
order, symbol ownership, and relocation diagnostics. Removing fresh execution
would weaken recovery and process-lifecycle coverage.

## Decision

Add `Build-Cached-Linked-Image-Set.mjs` and cache family `linked-image-v2` for
one through 64 ordered WVO inputs. Derive a length-framed SHA-256 key from the
format and namespace, current host, base address, entry symbol, canonical input
count, every exact WVO in command order, the producer script, current-host
`Link-Wvo` front door, and current-host native linker.

Bound each WVO and the aggregate snapshot by the 32 MiB large-native linker
admission limit. Retain the input buffers used by the key. On a miss, write
those buffers to private snapshot files and link the snapshots rather than
rereading mutable owner paths. Before publication, require the script, front
door, and linker to remain byte-exact. Publish one immutable directory with the
image, canonical map, and an eight-line record containing the key, input count,
entry offset, and both product sizes and digests.

Every hit rejects links and unexpected entries, reparses the exact requested
map entry, rehashes the image and map, reconstructs the record, copies both
products to private owner paths, and rehashes the copies. A publisher owns one
exact `.new-<key>-<nonce>-*` sibling; its `finally` boundary removes that path
after build, measurement, record, or lost-race failure. A race loser accepts a
winner only after complete validation. Cache-root output aliases are rejected.

Use version 2 only for current-host multi-object database development links.
Keep portable single-object version-1 hits unchanged. No-argument and
qualification execution continue to call the direct linker and retain both-
host construction, byte comparison, and failure evidence.

## Evidence

The bounded regression passes a four-process same-key cold publication race
with exactly one creator, byte-identical outputs, an exact warm hit, distinct
keys for reversed input order, corruption rejection with both sentinel outputs
unchanged, failed-link temporary cleanup, and noncanonical-count rejection.

The cold full database owner publishes every distinct real three-object link
and passes all 50 cases in 269,620 ms. The final change-aware all-hit run passes
in 101,370 ms, down from 281,240 ms. This saves 179,870 ms or 63.96 percent and
is 2.77 times faster for this slice. Host-root-writer falls from 61,810 ms to
3,560 ms, host storage from 24,620 ms to 8,140 ms, and host-local-service from
29,010 ms to 1,450 ms. Relative to the earlier 500,610 ms project-object-v2
result, the combined development loop saves 399,240 ms or 79.75 percent and is
4.94 times faster. These are Windows host diagnostics, not portable timing
claims.

## Consequences

Ordinary database development no longer reruns a 10-to-16-second native link
when the exact ordered objects and producers already have a validated result.
Different paths with identical bytes can share a key, while input reordering,
producer changes, and target-host changes cannot.

Cold misses intentionally retain the complete linker cost and validation. The
new family consumes cache space for an image and map per distinct ordered set;
version 1 remains readable and unchanged. Independent Linux execution remains
required before any cross-host performance or lifecycle claim.

## Reconsideration triggers

Reconsider this decision if version-2 bytes or entry offsets differ from direct
linking; input order does not select another key; a mutation can cross the
snapshot boundary; a failed build or publication race retains unbounded debris;
corruption changes an owner output; qualification consults the cache; Linux
behavior differs; or repeated in-memory hashing and Node startup become the
next measured linked-image bottleneck.
