# Decision 0285: Strict native object-staging manifest validation

- Date: 2026-08-06
- Status: Implemented candidate; native resource adapter and grouped dual-host qualification pending
- Advances: [Decision 0284](0284-Versioned-Native-Object-Staging-Manifest.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0284 defines and produces `WVOP 1`, but a platform adapter must not
trust its lengths, counts, positions, or derived chunk names merely because the
manifest appeared last. Implementing validation separately in Windows and
Linux assembly would duplicate format policy at the most security-sensitive
boundary. Deferring all checks until after sibling creation would also mutate
the publication domain before rejecting malformed staging evidence.

The staging serializer was initially local to the hosted tool. Leaving the
producer and eventual consumer with separate field definitions would allow
them to drift before the native commit adapter is implemented.

## Decision

- Add a focused capability-free staging-manifest module. It owns both canonical
  `WVOP 1` serialization and strict validation while remaining separate from
  the already-large lowering core and the hosted staging shell.
- Require exactly 24 header bytes plus 12 bytes per chunk, one through 518
  chunks, a nonzero final WVO no larger than 32 MiB, and the exact 4 MiB chunk
  ceiling. Bound the count before multiplying or indexing attacker-controlled
  values.
- Require exact magic and version, an exact declared manifest length,
  zero-based ordinal chunk indices, contiguous positions beginning at zero,
  nonzero chunk lengths no larger than the ceiling or remaining object extent,
  and an exact final position equal to the declared WVO length.
- Return named failure status and zeroed size/count evidence on rejection.
  Return the object length, chunk count, and ceiling only after the complete
  manifest is valid.
- Make the staging producer call the shared canonical builder. The builder
  validates its own result before the manifest-last write.
- Add small checked-in Project 1 manifests for the real staging tool and the
  focused validation adapter so the qualified native source front door can
  reproduce both graphs without an ad hoc inventory.
- Keep platform resource opening, identity retention, missing/changed chunk
  detection, WVO verification, sibling construction, replacement, and cleanup
  out of this portable format reader.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 16.494 test seconds
  after a 24.03-second zero-warning Release build. No broader local
  verification level was run.
- The test accepts the producer's exact three-chunk/60-byte return-42 manifest
  and rejects truncation, bad magic, bad version, false declared size, zero and
  oversized object extents, zero and oversized counts, a changed chunk ceiling,
  reordered or duplicate indices, gapped positions, zero and oversized chunks,
  incomplete final coverage, and trailing bytes.
- The qualified native source front door compiles the 24-module staging-tool
  graph to 394,780 bytes at SHA-256
  `77158b228c204b587dbf559621ad7c717d4eb5b418c32b783204cd350525ac76`.
  It compiles the two-module validation adapter to 6,728 bytes at SHA-256
  `0d343c22a2d33bf1d90dc71f055133fedb742d88e775aaf6c2f9d1f3542300c0`.
- The validator, hosted shell, and test adapter are focused files of 5,835,
  6,245, and 2,098 bytes. No large platform adapter or numbered source fragment
  was added.
- Existing unpromoted Windows and Linux WVB-to-WVO packages remain 5,348,864
  and 5,349,376 bytes at their previously pinned SHA-256 identities.

No C# product implementation or WebAssembly implementation changed. Stage 0
remains the independent object, runtime, and malformed-input oracle. This slice
does not claim native manifest-file admission, chunk identity or content
validation, reconstructed WVO verification, durable final-object construction,
atomic replacement, cleanup, complete-tool self-lowering, ordinary-path
cutover, artifact promotion, or .NET retirement. Development, Standard,
Qualification, Linux execution, WebAssembly verification, and the grouped gate
remain deferred.

## Reconsideration triggers

Revisit the reader if WVO or cursor limits change, if publication becomes
noncontiguous, or if the native adapter cannot preserve the exact validated
manifest snapshot while opening chunks. A later format version must use a new
major version when an old reader could otherwise accept a meaning it does not
understand.
