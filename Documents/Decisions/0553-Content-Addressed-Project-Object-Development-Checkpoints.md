# Decision 0553: Content-addressed project-object development checkpoints

Status: Accepted

Date: 2026-08-14

Defines: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

Advanced by: [Decision 0738](0738-Reuse-Project-Object-Checkpoint-Admission.md)

## Context

The settled Decision 0552 change-aware run spent 1,111.135 seconds in the
fourteen-case database-storage owner. The compiler, lowerer, and database code
were not individually that slow. Development feedback repeatedly reconstructed
large Project 2 WVB and WVO products and also ran qualification-oriented
duplicate and paired-target cases.

The existing Decision 0546 checkpoint safely reused one packaged build driver,
but the composed host-storage and tree-reader projects were still rebuilt and
lowered on every development run. Faster feedback must not admit stale source,
trust a cache filename, or convert a local cache hit into qualification
evidence.

## Decision

Add a host-local, content-addressed project-object checkpoint for development
verification:

- key the exact workspace, project manifest, declared root/source closure, and
  ordered build-driver and lowerer bytes through a versioned length-framed
  SHA-256 contract;
- publish the WVB, WVO, and their complete size/digest record into one immutable
  host-scoped entry;
- rehash the products, compare the complete manifest and materialized copies,
  and structurally admit the WVO on every hit;
- fail closed on missing, malformed, linked, oversized, stale, or corrupt cache
  state without implicitly overwriting the entry;
- use the checkpoint only in the two-case composed database development owner;
  and
- let changed-file planning select that owner only for covered database and
  checkpoint boundaries. Compiler, lowerer, specialized fixture, and other
  broader changes retain the complete fourteen-case owner.

The no-argument retirement owner, GitHub shards, and cross-host qualification
remain cold and cache-independent.

## Consequences

- A repeated database development verification falls from 1,111.135 seconds to
  190.863 seconds on the measured Windows host while continuing to execute the
  real storage, interruption, recovery, repeated depth-two update, and stable
  reopen scenarios.
- A source or producer change selects a miss automatically; an unchanged hit
  cannot silently use modified product bytes.
- Node.js becomes an explicit development-only key-framing dependency. It does
  not define Windvale semantics or enter product execution and qualification.
- Linking, hosted packaging, and database process execution still consume most
  of the warm 190.863 seconds and remain the next measured cache boundary.
- Cache eviction and concurrent same-key publication remain future tooling
  work. A partial `.new-` directory is never a hit.

## Reconsideration triggers

Reconsider this decision if a cache hit can evade a declared source or producer
change, if independent Linux behavior disagrees, if the planner selects the
development owner for an uncovered boundary, or if native incremental
compilation provides a smaller and equally auditable checkpoint.
