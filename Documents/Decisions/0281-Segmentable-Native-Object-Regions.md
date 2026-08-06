# Decision 0281: Segmentable native object regions

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0160](0160-Bounded-Large-Native-Object-And-Link-Admission.md), [Decision 0280](0280-Bounded-Native-Analysis-And-Artifact-Aggregation.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0280 removed measured analysis amplification, but complete-tool
self-lowering still cannot publish its WVO through one ordinary Windvale
`bytes` value. Decision 0160 already establishes the correct product boundary:
large-native admission retains canonical WVO 1.0 bytes while ordinary values
remain limited to 4 MiB. The Windvale object writer nevertheless still joined
every object-owned region before returning, so a later producer could not
stream the same canonical object without duplicating its header, symbol, and
relocation rules.

## Decision

- Give the focused Windvale object writer one validated region plan containing
  the WVO/header-plus-text prefix, optional read-only section header, canonical
  symbols, canonical relocation records, and exact final object length.
- Keep machine code, text padding, and immutable data as separately owned
  spans. Canonical order is prefix, code, padding, read-only header, read-only
  data, symbols, then relocation records.
- Validate function/data/directory counts, padding, relocation ordering,
  relocation targets, and relocation ranges while planning. Retain the final
  zero-placeholder check in the complete object emitter because only that
  caller currently owns the complete code value.
- Reconstruct the ordinary WVO through the same region plan and require its
  actual length to equal the planned length. Exact WVO bytes do not change.
- Do not add a second object format, raise the ordinary 4 MiB value limit,
  reinterpret `file.write_bytes`, or claim that region planning alone
  publishes a large object.

This is a real ownership boundary inside the existing focused object module.
Function-code batching and a versioned stateful publication owner remain the
next slice; they can consume these regions without moving object serialization
back into the already large lowering core.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 9.770 test seconds
  after an 8.37-second zero-warning Release build. It retains exact Stage 0
  agreement, deterministic repetition, malformed-input rejection, PE/ELF
  packaging, and absence of CLR modules. No broader local verification level
  was run.
- The canonical return-42 object remains 479 bytes at SHA-256
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
- The object module is 21,117 bytes at SHA-256
  `20de442e9a8aa64f957e7d5f353ff62f8e43b2fce74e0aad572a1b8259ad9918`.
  The current core closure is 365,441 bytes at SHA-256
  `c084922554535592d047b559ba59e0eb7824e3c4832e0d1ee275c672e337b74a`;
  the memory adapter is 360,099 bytes at SHA-256
  `43666ab10aac0c12d67ffda54fe7f4b04ff6d37efef689e3b755f84fe12f0758`;
  and the hosted tool is 361,127 bytes at SHA-256
  `88648ea76f05bf441232747f97f33be87324027fb4ac03e1cc045249d45c62f0`.
- Current unpromoted packages are 5,021,184 Windows and 5,021,696 Linux bytes.
  The Windows SHA-256 is
  `292ef7b86d7462f5763032ba82c453ac67e7f9f9da84d3d5bca8fff68a7cc702`;
  the Linux SHA-256 is
  `2b0ec426fe2b3263a41549b323f9e0c79613923584699db35c46a3fd1bea095e`.
- No C# product implementation or WebAssembly implementation changed. Stage 0
  remains the independent oracle and recovery path.

This slice does not claim complete-tool self-lowering, large-object
publication, ordinary-path cutover, artifact promotion, or .NET retirement.

## Reconsideration triggers

Revisit the region set if a canonical WVO section can itself exceed one
ordinary value or if bounded function batching reveals a different natural
publication unit. Any stateful publisher must specify exact ordering, total
length, partial and indeterminate mutation behavior, final verification, and
cleanup before replacing the current whole-value output path.
