# Localization workload 1 performance and cache contract

## Status

This file defines the measurement protocol and structural ceilings. It does not
claim host timing or memory results before a reference validator and compiler
front door exist on both permanent hosts.

## Reference input sizes

| Input | Exact bytes |
| --- | ---: |
| Eleven content artifacts | 12,895 |
| Artifact hash index | 1,214 |
| Selected `en@1` chain, lock, and one catalog | 8,695 |
| Selected `test-Unicode@1` chain, lock, and one catalog | 8,794 |
| English keyword rows | 66 |
| Synthetic keyword rows | 66 |
| Each `Foundationˉoption` catalog | 16 labels |

Shared byte totals count the Unicode profile and token registry once. A compiler
must not retain duplicate validated copies merely because two source profiles
reference them.

## Structural work requirements

- Descriptor scanning examines at most 128 bytes plus one line ending.
- An artifact is hashed once per private admission candidate and parsed in one
  forward pass after its external hash matches.
- Count and ordering validation is linear in records.
- Registry-to-lexicon comparison is linear in the 66 ordered rows.
- Catalog completeness uses the canonical interface identity set and must not
  perform pairwise comparison of all labels.
- Exact and skeleton collision checks use bounded indexed or sorted structures;
  an implementation must not hide quadratic work behind the current 16-label
  fixture.
- One malformed artifact produces one primary diagnostic and at most eight
  related structured fields.
- Failed private state is released before the request returns.

The general format maxima remain 64 KiB for profiles, registries, lexicons, and
vocabulary profiles; 1 MiB and 65,536 labels for one catalog; 1 MiB for one lock;
and 64 catalogs per source module. These are admission ceilings rather than
allocation instructions.

## Cache publication

The cache key is `(artifact format, exact SHA-256)`. Identity/version is retained
as validated metadata but never substitutes for the content hash.

Publication follows:

1. allocate one request-private candidate under an explicit byte/work budget;
2. hash and parse all selected components;
3. verify cross-references, counts, ordering, Unicode policy, and collisions;
4. build the immutable lookup representation;
5. atomically publish it to the current compiler-service generation; and
6. release equivalent losing candidates after a race.

A published composite entry retains component hashes, not private source spans
or request diagnostics. Request-owned source maps and diagnostics are rebuilt for
each raw source. Negative admissions are not durable shared cache entries.

## Required measurements

The first executable reference validator and compiler implementation must record
on Windows and Linux:

- host/CPU, tool commit, build profile, and cold/warm state;
- exact input byte and row counts;
- descriptor time;
- per-artifact hash, parse, validation, and table-construction time;
- complete selected-chain time;
- peak private allocation and published retained bytes;
- warm lookup time and allocation;
- concurrent same-hash publication behavior;
- malformed first-record, last-record, hash-mismatch, collision, and maximum-size
  failure time/memory; and
- cache-generation teardown time and remaining retained bytes.

Run at least 30 measured in-process iterations after five warmups for the small
reference chain. Report median and p95, not only the best result. Measure process
startup separately. Maximum-size fixtures use at least five iterations and must
remain bounded even when timing is noisy.

## Threshold gate

No wall-clock or retained-memory threshold is accepted yet because no
representative implementation measurement exists. Replacement source freeze
accepts the exact measurement protocol and structural linear/bounded-work
requirements. The first implementation must establish stable Windows and Linux
baselines, after which the owner accepts versioned ceilings with headroom as
release and regression gates.

The intended product requirement is that a warm profile admission performs no
artifact reread, rehash, or reparse, and that cold work remains linear and
negligible beside ordinary parsing/type analysis for a real module. Missing
measurements block implementation qualification and release, not the design
freeze; they do not justify guessing a threshold or running broad native
qualification for a paper-only change.
