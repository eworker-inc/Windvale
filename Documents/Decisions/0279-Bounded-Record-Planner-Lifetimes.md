# Decision 0279: Bounded record-planner lifetimes

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0150](0150-Bounded-Native-Dynamic-Value-Lifetimes.md), and [Decision 0276](0276-Capability-Aware-Record-Storage.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0276 removed the remaining unsupported-code boundary from the
Windvale-written WVB-to-WVO lowerer. Its packaged self-lowering attempt then
exited 1 without a diagnostic or output, leaving instruction exhaustion and
native resource exhaustion indistinguishable.

An in-process execution of the exact hosted tool under its normal
48-billion-instruction ceiling reports `WVR3018`: the 128 MiB native text arena
is exhausted and no WVO is published. The supplemental record planner returns
descriptor-bearing records and constructs dense bounded maps through repeated
immutable byte concatenation. ABI 22 cannot discard a descriptor-record
callee's temporary dynamic values at that return boundary. Large compiler
functions also expose quadratic one-element fill and liveness-map construction
inside one call frame.

## Decision

- Return record-storage phase evidence as one direct `bytes` value instead of
  a descriptor-bearing record. The private packed value begins with `WVSR`,
  format version 1, exact scalar counts, persistent/scratch byte lengths, and
  the two offset payloads. Decode only after validating the magic, version,
  field/value limits, four-byte alignment, and exact total length.
- Put per-function measurement behind a scalar-return helper. Put per-function
  code and relocation production behind one direct-byte `WVFA` version-1
  envelope. These return shapes let ABI 22 discard control-analysis scratch
  while preserving only the measured scalar or final function artifacts.
- Construct repeated zero and `u32` maps by bounded doubling rather than one
  immutable append per element. The resulting bytes and limits are unchanged,
  while total copied fill bytes become linear in the requested result size.
- Construct each liveness iteration through direct-byte per-block helpers.
  Preserve the existing dense use, definition, live-in, and live-out semantics,
  bounded iteration count, and interference allocator; only temporary lifetime
  and construction grouping change.
- Keep the large functions under their existing owners for this slice. The
  repository's reviewable-file guidance still applies: the measured
  `emit.control.code` boundary is a concrete candidate for cohesive extraction
  or chunked emission, not for numbered fragments or an arbitrary line limit.

## Evidence and consequences

- The affected focused test was reviewed and expanded before execution. It
  validates valid and malformed `WVSR` evidence in the reference runtime and
  Stage 0 native executor, retains the hosted capability/record differential
  fixture, and exercises a 1,032-instruction, 130-block, 129-record-local
  envelope through both Windvale adapters. It passes 1/1 in 47.737 test seconds
  after correcting two stale fixture assumptions (`call().field` syntax and an
  unsupported filler `pop`).
- The separate pin-sensitive package selection passes 1/1 in 10.360 test
  seconds. Both Release builds report zero warnings and errors.
- The current core closure is 353,068 bytes at SHA-256
  `e5b47b3186dff2e69d0d307fad85ae64f73e642ea21b94af3c7eaeb6dc3b99d1`.
  The memory adapter is 347,939 bytes at SHA-256
  `e766f51e59564f51c64158cfaf0dfe156daf3eba44869e59fcf1452d6c7edb59`;
  the hosted tool is 348,967 bytes at SHA-256
  `13642a39dc3c7074eeb36d1a4cab897a171152bd5a5c4df2ab8076c06f0bd5b0`.
- Current unpromoted packages are 4,828,672 Windows and 4,829,184 Linux bytes
  at SHA-256
  `2dd0e91cf4e67466b68ebf7a67d9b29d4d69f8481efd9bf763b1d838aec7fdd5`
  and `41f46e75efdec87920d020c9c85b505ba238c7eb07556bb2f914e6a62ea06206`.
- One exact native self-lowering probe remains fail-closed as `WVR3018` after
  0.806 seconds and publishes no object. A separate reference-runtime probe
  reaches its explicit 100,000,000-instruction ceiling in 9.8 seconds. It
  reports 418,105,387 constructed dynamic bytes but only 221,418 peak live
  bytes; 345,206,567 constructed bytes belong to the machine-code
  `bytes.concat` emission helper alone. The next native slice is therefore
  chunked machine emission or integration of the already-designed reclaiming
  ownership allocator, not another arena increase.
- No C# product implementation or WebAssembly implementation changed. Stage 0
  remains the independent oracle and recovery path.

Local Development, Standard, Qualification, the full Seed/OS suites, Linux
execution, WebAssembly verification, GitHub verification, artifact promotion,
and ordinary-path cutover remain deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Revisit the packed phase evidence if Windvale gains a native-supported bounded
builder or a non-descriptor phase-result type with the same explicit limits.
Revisit dense liveness if valid inputs approach its remaining bounded work or
arena envelope. Replace the checkpoint wrappers when a qualified owner-token
allocator lowers the complete verified ownership plan across generated code
and runtime services.
