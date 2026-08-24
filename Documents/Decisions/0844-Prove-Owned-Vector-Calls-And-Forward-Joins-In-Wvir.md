# Decision 0844: Prove owned Vector calls and forward joins in WVIR

## Status

Accepted on 2026-08-24 with current-Windows development evidence. Paired Linux
execution remains pending before a cross-host conformance claim. This is a
bounded Slice 5 ownership checkpoint, not completion of general aggregate
ownership, WVB call transfer, semantic `using`, deterministic reverse release,
or the hosted resource migration.

## Context

The compiler already distinguished by-value, immutable-borrow, and mutable-
borrow call arguments during source lowering, and specialized intrinsics had
bounded affine proofs. Ordinary WVIR calls still lacked an independent move
proof. A by-value `Vector<T>` could therefore be represented as a normal call
operand without a verifier-owned rule that invalidated its source slot across
later operations and joins.

Serializing that call immediately would be premature. WVB 1.25 does not carry
parameter transfer modes, and its function contract does not yet state when a
callee releases a transferred owner on every exit. Adding a version number
without those two rules would freeze an incomplete ownership ABI.

## Decision

1. Independent WVIR validation classifies exact private WVGT kind-11
   `Vector<T>` parameters, locals, and temporaries as owned values. It does not
   infer ownership from a numeric private-shape range.
2. Each owned by-value parameter begins available; a borrowed Vector parameter
   begins non-owning. Explicit Vector locals begin unavailable and become
   available only through one unique store.
3. A Vector local load retains exact source-slot provenance. A by-value ordinary
   call, unique store, or owned return consumes the temporary and invalidates
   that originating slot. A borrowed call consumes only the transient argument
   use and preserves its source owner.
4. The validator reconstructs each formal mode from the validated source
   declaration and bindings. Borrow syntax and modes remain absent from the
   serialized WVIR operation.
5. The first proof admits at most 64 blocks, 64 parameter/local slots, and 4,096
   operations per affected function. Temporaries remain within their producing
   block, Vector phis and backward control are rejected, and no unbounded fixed
   point is attempted.
6. Forward joins retain a slot state only when all incoming states agree. A
   value moved on one path and preserved on another is unavailable after the
   join. Equal moves on both paths remain valid.
7. The proof covers exact Vector values. Ownership recursively nested in a
   record, variant, Result, array, or another aggregate remains a separate
   checkpoint requiring aggregate field provenance and cleanup rules.
8. Valid owned-Vector calls are not serialized yet. The emitter returns exact
   `Unsupportedˉshape` without a WVB product. Invalid move evidence reaches the
   same emitter validation boundary and returns exact `Invalidˉanalysis` with
   `Invalidˉwir`.
9. WVB remains 1.25. No opcode, borrow handle, owner bit, source slot, runtime
   object, or implicit cleanup rule is introduced by this decision.

## Consequences

- `Owned-Vector-Calls-And-Joins-Wir.wv` proves borrow-then-transfer, owned
  results and returns, and equal consumption on two forward paths. Its WVIR has
  exactly six ordinary calls and two branches before the WVB boundary refuses
  publication.
- Three focused fixtures prove borrow-after-move, duplicate transfer, and
  asymmetric-join reuse are rejected by independent WVIR validation.
- The combined memory-budget and Vector owner passes 51 cases while preserving
  the exact 752-byte Split, 1,107-byte Vector construction, and 3,096-byte
  Vector append products and their prior SHA-256 identities.
- The native verification registry remains 112 owners and advances to 5,380
  cases. Its 17,078 LF-only bytes have SHA-256
  `832449b3d8cce925d5cd34ef6c0e478ce7b0d95aa8603a63f60df08c3d1e3b0c`.
- The source analyzer's product mode remains distinct from the emitter's trust
  boundary: provisional WVIR may be published for inspection, but no WVB is
  published without complete independent validation.
- This checkpoint is durable self-hosting architecture. It deliberately does
  not micro-optimize the transitional Seed implementation.

## Reconsideration triggers

Add WVB call transfer only with exact parameter modes, caller/callee ownership
agreement, all-exit cleanup, verifier dataflow, malformed-input coverage, and
runtime teardown. Broaden WVIR ownership only with field-level aggregate
provenance, loop fixed points, and deterministic release ordering. Do not make
borrow syntax a runtime pointer, copy a Vector owner, infer cleanup from host
behavior, or treat provisional analyzer output as executable authorization.
