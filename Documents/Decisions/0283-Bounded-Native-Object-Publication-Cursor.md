# Decision 0283: Bounded native object-publication cursor

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0281](0281-Segmentable-Native-Object-Regions.md), [Decision 0282](0282-Bounded-Native-Function-Batches.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0282 made machine code and relocation generation independently
batchable, while Decision 0281 separated every non-code WVO region. No one
portable owner yet defined how those values form one exact object without
joining all code into a complete `bytes` value. Letting a hosted writer choose
the order or offsets would duplicate compiler layout policy at the host seam.

The existing `storage.random_access_v1` capability is deliberately not this
boundary. It binds an already-open storage object and explicitly excludes path
creation, rename, atomic replacement, and directory publication. Reusing it
would still leave the commit protocol undefined and would pull its `u64`
surface into a native backend that does not yet admit wide values.

## Decision

- Add a focused capability-free publication module rather than enlarging the
  already large lowering core. It consumes the core's immutable plan and uses
  the existing object-region planner and bounded function-batch emitter.
- Traverse every function batch once while planning. Require nonempty forward
  progress, exact artifact lengths, canonical relocation order, in-range data
  targets, nonoverlapping patch fields, and a zero relocation placeholder
  inside the code chunk that owns each field.
- Retain only bounded raw relocation entries while planning the object regions;
  do not retain or concatenate complete machine code. Preserve the 32 MiB WVO
  ceiling and WVO 1.0 bytes.
- Begin one immutable cursor at position zero. Each step yields one exact
  `(position, bytes)` value in this order: prefix, bounded code batches,
  alignment padding, optional read-only header, data, symbols, and relocation
  records. Code positions come from canonical function offsets.
- Carry the exclusive next-function ordinal and exact next position in every
  cursor. Reject invalid, skipped, repeated, or out-of-range cursor evidence;
  accept completion only at the planned object length.
- Treat the plan and region record as immutable in-process phase evidence, not
  as a serialized or host-supplied format. The later host owner must consume
  them without mutation and independently own resource lifetime and commit.
- Extend the focused ten-function adapter instead of adding another corpus. A
  1 KiB artifact ceiling forces multiple code batches, reconstructs the entire
  WVO from cursor steps, checks an invalid cursor, and returns compact evidence
  for independent Stage 0 comparison.

The future hosted owner must define a versioned resource protocol with unique
sibling creation, exact positioned writes, set-length behavior where needed,
partial and indeterminate mutation reporting, durable flush, atomic
replacement, and cleanup before publication. It must never silently retry an
indeterminate mutation. Those host semantics do not belong in this portable
compiler module.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 12.081 test seconds
  after a 17.42-second zero-warning Release build. No broader local
  verification level was run.
- The canonical return-42 WVO remains 479 bytes at SHA-256
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
- The native source front door compiles the 22-module publication closure to a
  384,821-byte WVB at SHA-256
  `1d95fbec5d6d52080eca1547a280b07183b666d90e48716267a5a08558159bad`.
  It compiles the 23-module test adapter closure to 388,511 bytes at SHA-256
  `1c5b2e08fe17c78368e3782bdd3e5ad09e79379592d48e17f7fd8bd88914f71e`.
- Existing unpromoted Windows and Linux WVB-to-WVO packages remain 5,348,864
  and 5,349,376 bytes at SHA-256
  `0e0d0c87f82f6576b11f888cfa26469f86f157064ea605a4bb188bcee5e3b280`
  and `c6ba202ffcb32a261bfd9c997e4bab754ab5a636e2d0b95e5de5f55e598c6358`.
- No C# product implementation or WebAssembly implementation changed. Stage 0
  remains the independent object oracle and recovery path.

This slice does not claim stateful or atomic host publication, complete-tool
self-lowering, ordinary-path cutover, artifact promotion, or .NET retirement.
Development, Standard, Qualification, Linux execution, WebAssembly
verification, and the complete grouped gate remain deferred.

## Reconsideration triggers

Revisit this cursor if WVO ordering changes, one admitted function cannot fit
in an ordinary bounded value, a serialized publication plan becomes necessary,
or the host publication protocol requires noncontiguous writes. The hosted
owner must not weaken exact positions, zero-placeholder validation, failure
cleanup, or atomic visibility merely to reuse an existing storage interface.
