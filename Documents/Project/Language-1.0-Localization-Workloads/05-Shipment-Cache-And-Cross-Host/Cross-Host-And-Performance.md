# Cross-host and performance qualification protocol

## Equality matrix

Windows and Linux use the same portable localization bytes. A paired report
compares:

| Evidence | Required equality |
| --- | --- |
| Profile/component/catalog/lock inputs | Exact byte count and SHA-256 equal. |
| Descriptor and malformed-input admission | Same success/failure, phase, canonical diagnostic identity, and structured values. |
| Unicode/normalization/script/number/confusable cases | Same scalar spans, table identity, skeleton collision result, and bound. |
| Canonical keyword/declaration projection | Byte-identical. |
| Workload 3 conversion output/report semantic fields | Byte-identical except explicitly host-local output path/timing fields. |
| Portable AST/semantic/WIR/WVB sections | Byte-identical after excluding named raw-source/debug provenance. |
| Cache logical events | Same miss/admit/publish/hit/retire result for the deterministic schedule. |
| Online/offline portable store objects and selection | Same ordered hashes and logical package graph. |

Host-native compiler/installer bundles, target IDs, absolute paths, process IDs,
wall-clock timings, allocation implementations, and installation-generation
bytes that contain host target objects may differ. A report must not call those
portable-equal; it compares their declared host-specific evidence separately.

## Benchmark states

- **Process cold:** new compiler-service process/generation, localization cache
  empty, normal host filesystem cache unchanged.
- **Service warm:** same process/generation and exact inputs after successful
  content/profile publication.
- **Storage cold:** optional controlled qualification with documented host cache
  preparation; never approximated by deleting unrelated caches.
- **Install cold/warm:** absent versus already-admitted immutable content objects
  in a fresh bounded test installation root.

Process startup is reported separately from profile admission and compilation.

## Required workloads

1. Exact minimal English fixture: 8,099 unique semantic bytes.
2. Exact English+Chinese fixture: 12,288 unique semantic bytes.
3. Paired Workload 2 English/Chinese source with equal canonical projection.
4. Workload 3 three-profile conversion and round trip.
5. Workload 4 multilingual accepted/rejected/confusable/bidi matrix.
6. One representative real module importing the complete release Foundation
   catalog set.
7. Valid high-bound chain with 64 catalogs and separately bounded maximum-size
   artifacts, without allocating from declared maxima before reading lengths.
8. Malformed first/last record, hash mismatch, stale interface, collision,
   oversize, and cancellation paths.
9. Concurrent same-hash admission with 1, 2, and 8 requesters.
10. Generation update, in-flight old request, new request, retirement, rollback,
    and cache-budget pressure.

## Measurements

Each report records host/CPU/OS, commit, exact tool/profile build, input hashes and
sizes, process/service/cache state, and iteration policy. Measure:

- descriptor scan;
- content hash, parse, validation, collision/skeleton, and table construction per
  artifact;
- complete selected-chain admission;
- source lexing/public-label resolution;
- canonical projection and total compile elapsed time;
- private allocation, peak working set where reliable, published retained bytes,
  request-owned bytes, and bytes after generation retirement;
- cache hit/miss/publication/eviction counts;
- install download/read, verify, store-publication, reuse, activation, rollback,
  and cleanup bytes/time; and
- bounded diagnostic count/output bytes and failure cleanup time.

For the small reference chains, run five warmups then at least 30 in-process
measured iterations and report median/p95. Run maximum-size/concurrency cases at
least five times. Do not report only the fastest observation. Preserve raw
bounded result records beside the summary.

## Performance acceptance timing

Source freeze accepts exact input/algorithm/memory bounds and this reproducible
protocol. It does not invent milliseconds or retained-memory ceilings before a
representative Language 1.0 front door exists.

The first implementation qualification performs the paired measurements, records
host-specific absolute ceilings with explicit headroom, and establishes named
regression workloads. Release qualification then enforces those ceilings. Any
relative regression rule includes an absolute floor so sub-millisecond noise does
not fail a build, and any threshold change is reviewed evidence rather than an
automatic baseline reset.

Regardless of later numeric ceilings, these structural requirements already
hold:

- warm content/profile hits read, hash, and parse zero unchanged artifact bytes;
- work is linear or indexed, never pairwise across all labels/modules;
- the same content hash has one published immutable entry per service generation;
- cache retention and diagnostics remain explicitly bounded;
- retiring a generation eventually returns its retained localization state to
  zero, excluding deliberately shared immutable entries still referenced by a
  current generation; and
- localized source must not change portable semantic output or introduce runtime
  localization bytes.
