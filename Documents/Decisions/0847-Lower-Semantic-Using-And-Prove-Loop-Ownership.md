# Decision 0847: Lower semantic using and prove loop ownership

## Status

Accepted on 2026-08-24 with paired Windows/Linux focused development evidence.
This completes the exact-Vector `using` and loop-ownership checkpoint, not
aggregate-owned fields, general destructors, a hosted resource consumer, or
elastic growth.

## Context

[Decision 0839](0839-Admit-Exact-Language-1.0-Using-Statements.md) admitted and
retained `using Name = Expression Block` syntax but deliberately promised no
cleanup behavior. [Decision 0845](0845-Execute-Owned-Vector-Calls-As-Wvb-1.26.md)
made exact Vector transfer executable but rejected every backward edge in an
owned function. Slice 5 now needs one real resource class whose successful
acquisition has deterministic cleanup through normal and early exits, including
loops, without hidden exception machinery or a runtime destructor registry.

Treating cleanup as an ordinary source call would permit omission, duplication,
or aliasing. Releasing all active locals at every `break` or `continue` would
also be wrong for a resource whose scope contains the loop. The compiler must
represent the exact scopes exited and the verifier must independently prove
that each owner is available at its release.

## Decision

1. `using` initializers are evaluated exactly once before their name becomes
   visible. The name receives immutable WVLB binding kind `4`, a monotonically
   assigned non-parameter slot, and scope limited to the owned body.
2. The first semantic resource class is only the canonical Foundation kind-11
   `Vector<T>` identity. Scalars, shared Sequences, lookalike records, and every
   other value reject with `Invalidˉresource = 44`. This does not infer a
   destructor convention from a name or shape range.
3. Typed lowering emits `Releaseˉlocal = 174` on normal fallthrough, `return`,
   failed `try` propagation, `break`, and `continue`. It releases only `using`
   scopes actually exited. Nested resources are emitted in reverse binding and
   slot order.
4. Operation 174 has shape zero, the no-result sentinel, zero operands, one
   direct non-parameter exact-Vector slot in `Target`, and zero `Auxiliary`. It
   consumes the available owner. A move or freeze before the implicit release
   therefore invalidates the WVIR rather than suppressing or duplicating
   cleanup.
5. Operation 174 does not select a new WVIR feature family or WVB minor. Source
   WVB lowers it to the existing six-byte sequence `local.take <slot>; pop`.
   The scalar runtime's ordinary descriptor release remains the one physical
   cleanup mechanism.
6. The WVIR and WVB ownership proofs remain bounded to 64 blocks, 64 owned
   slots, and 4,096 operations or instructions per affected function. Forward
   joins retain their conservative agreement/intersection rule. Every backward
   edge must exactly equal the already established target-header ownership
   state. A loop may preserve ownership, but cannot gain, lose, or change owner
   class per iteration.
7. The verifier retains one bounded ownership state per instruction entry and
   compares the two-word state for each owner at a backedge. It does not execute
   an unbounded dataflow fixed-point, follow source scopes, or trust analyzer
   intent.

## Consequences

- Four positive `using` fixtures prove fallthrough, nested return, failed `try`
  propagation, and loop `break`/`continue`. They contain seven exact release
  operations. The fallthrough fixture emits a deterministic 1,211-byte WVB at
  SHA-256
  `f541cd186564d1e696820a53c4a17baf50ba0d393dbb4bc8b1c381960b595257`,
  passes the compiler-aligned verifier, executes the release, and returns `42`.
- A non-resource initializer rejects with exact `Invalidˉresource`. Moving a
  Vector before its implicit release publishes provisional analysis but the
  independent emitter rejects exact `Invalidˉwir`.
- An ownership-invariant Vector loop is admitted. A source loop whose backedge
  changes the Vector state rejects, and a byte-level mutation that removes the
  release before a backedge also rejects in the WVB verifier.
- The combined focused owner advances from 58 to 70 cases: 11 valid products,
  38 malformed modules, four retained owned-call cases, 12 `using` cases, seven
  releases, and executable result `42`.
- The native registry remains 112 owners and advances to 5,399 cases. Its
  17,351 LF-only bytes have SHA-256
  `75683af614bde5f4d6b8aa4c7439bf7c1a0b7df5c3160553900ab2173af5f6e7`.

## Reconsideration triggers

Admit another resource class only with an exact canonical identity, one-owner
state transition, deterministic release behavior, and matching analyzer,
emitter, verifier, runtime, malformed-input, and exit-path evidence. Do not add
ambient destructors, finalizers, exception unwinding, best-effort cleanup, or
an unbounded verifier worklist as shorthand for that contract.
