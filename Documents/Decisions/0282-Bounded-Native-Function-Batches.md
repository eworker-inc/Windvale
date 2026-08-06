# Decision 0282: Bounded native function batches

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0160](0160-Bounded-Large-Native-Object-And-Link-Admission.md), [Decision 0280](0280-Bounded-Native-Analysis-And-Artifact-Aggregation.md), [Decision 0281](0281-Segmentable-Native-Object-Regions.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0281 separated the canonical WVO regions needed by a future
large-object publisher, but the lowering core still emitted every function's
relocations and machine code into one `WVFA 1` value. A publisher could not
consume the object regions safely while code production still required a
complete value, and re-running all WVB admission and machine-layout analysis
for every arbitrary chunk would duplicate work and weaken the phase boundary.

## Decision

- Split WVB admission and machine measurement from emission. A successful
  immutable lowering plan owns the canonical function directory, exact
  function-record cursors, per-function relocation lengths, aggregate lengths,
  and the validated data, type, and capability evidence.
- Measure relocation counts from the already validated instruction stream
  during the machine-layout pass. Retain exact machine offsets and canonical
  relocation order; do not derive either from a later output buffer.
- Let a batch request choose the largest non-empty contiguous function range
  whose complete `WVFA 1` header, relocations, and code fit a caller-provided
  ceiling between 16 bytes and the ordinary 4 MiB value limit.
- Emit the selected range through the existing balanced range emitter. Require
  its actual relocation and code lengths to equal the plan and return an
  exclusive next-function ordinal so forward progress is explicit.
- Keep the ordinary complete-object entry byte compatible: it requests one
  4 MiB batch and requires that batch to cover every function. It does not join
  several batches or claim large-object publication.
- Add a focused Windvale test adapter that forces the ten-function envelope
  across multiple 1 KiB batches. Verify contiguous progress, per-batch size,
  artifact accounting, total function-symbol code, total relocations, and the
  retained complete Stage 0 WVO agreement.

The already large lowering core remains the owner because the plan and batch
emitter depend on its private WVB function, control-analysis, and instruction
selection records. This follows the repository's reviewable-file guidance:
the later stateful publication protocol should be a focused module with a real
ownership boundary, rather than splitting this core into numbered fragments
or duplicating its invariants.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 10.610 test seconds
  after an 8.28-second zero-warning Release build. No broader local
  verification level was run.
- The canonical return-42 WVO remains 479 bytes at SHA-256
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
- Direct native source construction produces a 377,219-byte core closure at
  SHA-256
  `71b86801e1754f423a5428c5b588e1b6e2c9b722cde53fabcbfd654e9c2dd48c`,
  a 371,486-byte memory adapter at
  `0e2b509b54b9320efe7d70080ea74afdce771d01ade33c3d915ae088d9d4b278`,
  and a 372,514-byte hosted tool at
  `f2283d33fdcae404a6dd15f6a888c3d1efa359328110fca6d54be1aa67cc1d5c`.
- Current unpromoted packages are 5,348,864 Windows and 5,349,376 Linux bytes.
  Their SHA-256 identities are
  `0e0d0c87f82f6576b11f888cfa26469f86f157064ea605a4bb188bcee5e3b280`
  and `c6ba202ffcb32a261bfd9c997e4bab754ab5a636e2d0b95e5de5f55e598c6358`.
- No C# product implementation or WebAssembly implementation changed. Stage 0
  remains the independent oracle and recovery path.

This slice does not claim complete-tool self-lowering, stateful or atomic
large-object publication, ordinary-path cutover, artifact promotion, or .NET
retirement. Development, Standard, Qualification, Linux execution,
WebAssembly verification, and the complete grouped gate remain deferred.

## Reconsideration triggers

Revisit the batch artifact only if one admitted function can no longer fit in
an ordinary value or if a canonical relocation representation changes. The
next publication owner must preserve batch order, verify every relocation's
zero placeholder within its owned code chunk, define exact total length and
commit behavior, distinguish partial from indeterminate mutation, and clean up
failed state before it can replace the current whole-value tool.
