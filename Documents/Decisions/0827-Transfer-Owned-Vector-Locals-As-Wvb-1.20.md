# Decision 0827: Transfer owned Vector locals as WVB 1.20

## Status

Accepted on 2026-08-22.

## Context

Decision 0826 made scalar Vector and Sequence backings executable, but its
linear Vector evidence existed only on the operand stack. Ordinary
`local.load` retains a descriptor and deliberately produces a shared value, so
a compiler could not store a newly constructed Vector in a local and later
recover the unique evidence required by append or freeze. Treating an ordinary
load as a move would either create two mutable owners or require hidden
copy-on-write allocation.

Source-level ownership and borrowing are wider than this backend checkpoint.
The bytecode needs one exact transfer primitive before source lowering can
select collection operations without embedding runtime representation rules in
the compiler.

## Decision

1. WVB 1.20 adds opcode `CD`, `local.take u32 local-index`. It accepts only an
   exact kind-23 non-parameter local, transfers that local's unique evidence to
   the operand stack, and makes the source local unavailable. Parameter slots
   remain shared until move-at-call is specified and are rejected here.
2. Runtime transfer copies the eight-byte descriptor, replaces the source with
   an eight-byte zero cell, and neither retains nor releases the backing. A
   later unique `local.store` may initialize the local again.
3. `local.load` remains a retaining shared read and never creates unique Vector
   evidence. In WVB 1.20, a Vector `local.store` consumes a unique Vector. WVB
   1.19 retains its earlier local-store typing for compatibility.
4. The verifier proves definite availability for every Vector local in a
   function that uses `local.take`. Parameters begin initialized for shared
   loads but cannot be taken; locals begin unavailable. Unique store makes a
   local available; load requires but preserves availability; take requires and
   consumes it. Forward joins use intersection so every incoming path must own
   the local.
5. The first ownership-flow profile is explicitly bounded to 64 Vector slots
   and 4,096 instructions per function. It rejects backward control flow in a
   function using `local.take`; loop ownership fixed points remain later work.
6. The transfer is a WVB/runtime ownership primitive, not a new source keyword.
   Source move, borrow, fallible construction, `Memoryˉbudget`, and compiler
   selection remain owned by their Language 1.0 slices.

## Consequences

- The compiler-aligned verifier publishes 239,824 WVB bytes at SHA-256
  `bfb60c8f80856c15399b457ab8c471e0e600492e0ffc39d34a718d0cb45e0a5b`.
  Uninitialized and repeated takes reject during control/ownership analysis;
  non-Vector operands and non-unique WVB 1.20 stores reject during typed
  execution.
- The scalar runner publishes 228,106 WVB bytes at SHA-256
  `63b8c862372e619bc9472d85ce850e7d621ed2106950b3e2ddaf801eaa6c78ee`.
  The transfer reuses its existing local frame and descriptor/collection/Vector
  stack flags and performs no heap allocation.
- The deterministic WVB 1.20 fixture is 1,156 bytes at SHA-256
  `baa69aadf3b9c65900110d9aa3372989e051045e30207a87b720dbc0a663dd25`.
  It transfers each Vector through locals before two appends and freeze across
  six 16-KiB allocation cycles. Twelve cases cover valid execution, version,
  type and local-index corruption, copied ownership, uninitialized and repeated
  transfer, capacity exhaustion, and Sequence bounds.
- `Source-Wir-Core.wv` remains unchanged. The checkpoint supplies a backend
  primitive that later collection lowering can select; it does not add another
  responsibility to the large source-WIR orchestrator.
- The verification registry retains 108 owners and advances to 5,167 declared
  cases at SHA-256
  `40e0baf6e1db78464fd72313e22a05e8a9df065e18128ec26c269f5be239b085`.
- General source ownership, loop-aware move analysis, borrows, non-scalar
  collection elements, native lowering, WebAssembly qualification, and the
  public fallible Foundation surface remain later checkpoints.

## Reconsideration triggers

Reconsider the opcode shape if source move/borrow lowering cannot express exact
ownership without additional typed transfer operations, or if measured real
programs require loop ownership before the first collection source migration.
Do not weaken definite availability or introduce hidden retention, copying, or
allocation merely to admit a control-flow shape.
