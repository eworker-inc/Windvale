# Decision 0906: represent immutable borrowed memory-budget calls in WVB 1.34

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision closes the immutable borrowed-budget publication gap retained by
[Decision 0905](0905-Transfer-Affine-Memory-Budgets-Through-Ordinary-Calls.md).
It does not add mutable budget borrowing, indirect borrowed-budget calls,
pointer or write-region borrowing, a public native ABI token, Linux evidence,
or paired-host qualification.

## Context

Source and WVIR already distinguish an observing immutable borrow from an
ownership transfer. WVB 1.33 had only shape `25`, whose calls consume the
opaque `Memoryˉbudget` owner. Encoding an immutable borrow as shape `25` would
therefore either move the caller's authority incorrectly or require downstream
consumers to guess a source-level mode that WVB did not carry.

The runtime and native backend already represent the opaque budget token in one
bounded scalar cell. The missing contract was not another token or provider;
it was a non-owning bytecode shape plus exact confinement rules that prevent
that scalar representation from becoming a copy operation.

## Decision

1. Reserve WVB minor `1.34` and value-shape byte `36` for an immutable borrowed
   view of the exact shape-`25` `Memoryˉbudget` owner.
2. Admit shape `36` only as an immutable borrowed function parameter or a
   compiler-generated non-parameter local. It is invalid as a result, nominal
   field or payload, collection element, callable-descriptor shape, mutable
   borrow, or owned value. Every WVB 1.34 module contains at least one
   shape-`36` parameter.
3. Retain shape `25` as the sole affine owner. Construct a temporary view only
   through the canonical adjacent sequence `local.load owner`, `local.store
   view`, `local.load view`, followed by zero through 64 ordinary local loads
   and one direct call whose corresponding parameter is shape `36`.
4. The view cannot be taken, returned, embedded, retained, or used by an
   indirect call. Loading a shape-`36` parameter is also invalid in this first
   profile; the callee may accept the observation boundary but no budget-query
   operation is yet defined.
5. A view operation never changes the owner-availability bit. A later by-value
   call or unsafe-scratch construction may consume the same still-available
   shape-`25` owner. Duplicate ownership transfer remains invalid.
6. The scalar interpreter and native x86-64 backend may carry both shapes in
   the same opaque `u64` cell, but their verifiers retain identities `25` and
   `36` independently and prove the canonical view sequence before lowering or
   execution. No raw address, provider object, or forgeable budget value is
   exposed.
7. WVB 1.34 inherits WVB 1.33's System profile, callable/task encodings, exact
   unsafe-scratch operation when present, bounds, and malformed-input checks.
   Unlike WVB 1.33, a borrow-only 1.34 module need not contain opcode `DC`.

## Implementation standing

The focused Windows oracle accepts the formerly unsupported immutable borrow,
executes a program that observes the budget and then transfers that same owner
to a 64-byte scratch allocation, and returns `42` through both the scalar
runner and native x86-64 output. All eight runtime programs lower and execute
natively. Six byte-level corruptions cover the old minor, owner/view shape
substitution, unknown view shape, view result, and `local.take`; both the
compiler-aligned verifier and native lowerer reject every corruption before
publication.

## Consequences

- Helper APIs may accept immutable budget authority without consuming it.
- Ownership remains explicit in bytecode even though execution uses the same
  bounded scalar cell for the owner and its temporary view.
- The first profile is intentionally observation-only: it establishes safe
  call representation before adding budget-query operations.
- Mutable borrowing remains separate because native packed-token mutation
  requires an exact write-through alias contract rather than a copied cell.

## Reconsideration triggers

Add a budget-query operation only with an exact result, failure, and revocation
contract. Add mutable or indirect borrowing only with write-through aliasing,
escape, callable-mode, verifier, runner, and native-lowering rules that preserve
one owner. Do not reinterpret shape `36` as an owner or make its scalar carrier
part of the public ABI.
