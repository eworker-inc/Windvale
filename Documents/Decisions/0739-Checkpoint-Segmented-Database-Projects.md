# Decision 0739: Checkpoint segmented database projects

- Date: 2026-08-17
- Status: Implemented with Windows development evidence; independent Linux execution pending
- Advances: [Decision 0738](0738-Reuse-Project-Object-Checkpoint-Admission.md)
- Contract: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

After project-object admission reuse, the complete all-hit database development
owner still took 500,610 ms. Three portable cases and three host components
retained a separate segmented construction path that compiled their project and
ran stage, link, and transport on every invocation.

A representative `TransactionTreeCompletion` construction spent 11,230 ms in
the build driver, 15,588 ms in segmented WVO staging, 338 ms in staged linking,
and 194 ms in canonical image transport. Compile plus staging owned 97.9
percent of the 27,350 ms construction boundary. The six repeated segmented
constructions accounted for most of the remaining avoidable database-owner
time, while their executions, provider overlays, and failure scenarios were
independent behavior that still had to run.

## Decision

Add one cross-host `segmented-project-v1` checkpoint behind paired `.cmd` and
`.sh` wrappers and use it only in database development verification.

Derive the key from the exact workspace, project manifest, ordered source
closure, build driver, current-host segmented WVO producer, staged image linker,
image transport, and checkpoint driver. Verify all three host producer digests
before accepting a hit or miss. The checkpoint driver bytes bind the admission
procedure and expected identities.

On a miss, build the exact WVB, stage and link its segmented objects, transport
the canonical image, and validate the `WVLI 1` header, entry, image bounds,
fragment count, contiguous extents, product sizes, and SHA-256 identities.
Publish only the WVB, canonical manifest, one through eight canonical fragments,
and complete record. Intermediate objects and staged images are never cache
products.

On a hit, reject unexpected entries, links, aliases, malformed records,
manifest disagreement, or product corruption. Rehash the immutable entry, copy
every product to private owner paths, and rehash each copy. Own and remove only
the exact `.new-<key>-<pid>-<nonce>` candidate under the canonical checkpoint
family in `finally`, including producer failure and lost publication races.
Validate the race winner before returning `Hit`.

Keep no-argument and qualification verification cache-independent. They retain
duplicate project compilation, byte comparison, fresh stage/link/transport,
both-host packaging, and execution. Development hits still run the current-host
application. Host cases still compose fresh platform provider overlays and run
their restart, persistence, and interruption scenarios.

## Evidence

The bounded regression passes fresh creation, a validated hit, corruption
rejection with sentinel WVB, manifest, and fragment outputs unchanged, forced
build failure with no temporary debris, and a four-way same-key race with one
`Created`, three `Hit`, byte-identical products, and zero `.new-*` entries.

The two-case Windows tree-completion owner passes creation in 64,680 ms and an
all-hit rerun in 7,170 ms for the portable section, an 88.9 percent reduction.
The host-tree-writer step passes both application and logical-put creation in
55,640 ms, then passes both cache hits, provider overlays, restart behavior, and
all interruption scenarios in 16,020 ms. The persistent writer passes creation
in 46,390 ms and a hit in 2,520 ms. Transaction commit passes creation in
37,730 ms and a hit in 4,490 ms. These are single-host diagnostics, not portable
timing claims.

The final change-aware gate passes all 24 general and 164 native planner cases,
then all 50 database cases with every checkpoint reporting `Hit`. The complete
database owner takes 323,820 ms, down from the preceding 500,610 ms: 176,790 ms
or 35.31 percent less wall time and a 1.55-fold speedup. Its portable section
falls from 198,870 ms to 115,980 ms, a 41.68 percent reduction. The unchanged
100-owner registry remains 4,626 cases across four qualification shards.

## Consequences

An unchanged segmented project no longer pays roughly eleven seconds of
compilation and sixteen seconds of staging merely to recreate bytes already
admitted under the same complete dependency identity. Link and transport are
also reused because their exact producers and outputs share the same immutable
boundary.

The cache stores up to one WVB and eight bounded image fragments per project
identity. Eviction remains external policy. Corrupt entries fail closed rather
than being deleted or repaired implicitly. Node.js remains a development-tool
dependency and does not define Windvale bytecode or image semantics.

## Reconsideration triggers

Reconsider this decision if a source, project, workspace, build driver,
segmented producer, transport policy, or checkpoint driver change can select an
old entry; manifest validation accepts noncontiguous or mismatched fragments; a
failed or racing producer leaves debris; a hit changes owner output on
rejection; qualification consults cache state; or independent Linux behavior
disagrees.
