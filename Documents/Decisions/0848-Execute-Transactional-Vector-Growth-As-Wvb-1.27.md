# Decision 0848: Execute transactional Vector growth as WVB 1.27

## Status

Accepted on 2026-08-24 with paired Windows/Linux focused development evidence.
This completes the explicit fallible Vector-growth checkpoint, not hosted
provider expansion, aggregate-owned fields, general element destruction, or a
hosted resource consumer.

## Context

Reserved construction and recoverable append gave a Vector one fixed maximum.
Applications that discover a larger bounded need would otherwise have to build
an unrelated Vector and manually coordinate two budgets, copying, cleanup, and
failure rollback. Hidden append-time reallocation would be simpler to call but
would erase the allocation boundary, introduce ambient authority, and make an
ordinary append's cost and failure behavior unpredictable.

The Language 1.0 direction instead calls for one explicit growth operation with
an explicit mutable budget and typed refusal. Failure must not leave a partially
copied Vector or consume accounting authority. The current scalar runtime also
cannot safely support general resource-bearing element relocation yet.

## Decision

1. Foundation collections major 1 adds
   `Vectorˉgrowˉreserved<T>(Vector: borrow mut Vector<T>, Budget: borrow mut
   Memoryˉbudget, Newˉmaximumˉitems: u64) -> Result<unit,
   Allocationˉfailure> effects(memory.allocate)`. This changes the exact
   Foundation-collections signature-set identity.
2. The call is selected only through the canonical Foundation module as
   intrinsic identity `12`. It requires explicit `::<T>`, one direct mutable
   non-parameter `Vector<T>` local, one distinct mutable exact budget slot, an
   exact `u64` new maximum, and exact contextual
   `Result<unit, Allocationˉfailure>`. The first executable profile admits
   resource-free scalar `T` only.
3. WVIR operation `175` carries the new-maximum temporary as its sole operand,
   the Vector slot in `Target`, and the budget slot in `Auxiliary`. WVIR 1.15 is
   the non-specialized growth envelope and WVIR 1.16 is its specialized pair.
   Independent validation reconstructs the exact Vector, unit Result, and
   Allocation-failure identities and proves both owners available.
4. WVB 1.27 is the lowest and only minor that can contain opcode `D1` (`209`).
   The instruction is thirteen bytes:
   `D1 u32 vector-local, u32 budget-local, u32 result-type`. The sole stack
   operand is exact `u64 Newˉmaximumˉitems`. Every WVB 1.27 module contains
   at least one exact `D1`.
5. A new maximum less than or equal to the Vector's current maximum violates
   the precondition and traps with `WVR3008` before allocation. A positive
   maximum beyond the scalar target's 2,047-cell bound returns
   `Targetˉunaddressable`; requested-byte arithmetic saturates rather than
   wraps.
6. Growth is a strong transaction. The runtime reserves one child allocation
   lease for the complete replacement while the old backing remains live,
   allocates and zero-initializes the replacement, copies exactly the
   initialized prefix, attaches the new lease, releases the old descriptor and
   lease, then swaps the Vector local once. Success preserves length and order.
7. Any budget, target, provider, fragmentation, lease, or heap refusal before
   the swap returns exact `Allocationˉfailure` and leaves the Vector owner,
   length, contents, capacity, maximum, and supplied-budget accounting and
   generation unchanged. The temporary peak may contain both allocations; the
   request therefore charges the full replacement size rather than a delta.
8. The bytecode verifier admits at most 64 growth instructions, validates all
   three indices and the thirteen-byte boundary, and preserves Vector and
   budget ownership at joins. Fifteen mutations cover version, opcode,
   Vector/budget indices and identity, Result layout, allocation fields, and
   truncated width.
9. Existing native frame and record-storage limits remain hard safety bounds.
   The new WVIR and WVB rules are extracted into focused predicates so the
   already-large validators stay below those bounds; no backend check is
   weakened or ceiling increased.
10. Provider-backed acquisition of additional budget authority remains a
    separately named future hosted capability. `D1` can consume only authority
    already present in its explicit budget and cannot reach ambient OS memory.

## Consequences

- `Vector-Grow-Reserved-Executable.wv` emits byte-identically twice as a
  3,628-byte WVB 1.27 module at SHA-256
  `30de39bdd12ad7718ad1fb465b14bc42f8463b6ecfc6ba1f10494cb6e67c5b59`.
- A 40-byte replacement request against 24 available bytes returns exact
  `Budgetˉexhausted`, reports both values, and leaves length `1`. A later
  24-byte request grows maximum `1` to `2`; append accepts the second item and
  the source-built runner returns `42`.
- The combined focused owner passes 88 cases: 12 valid products, 53 malformed
  modules, deterministic Split/construction/append/growth publication, owned
  calls, semantic `using`, typed success/refusal, and precondition traps.
- The exact 88-case summary, portable WVB sizes, and SHA-256 identities match on
  Windows and Linux. This is focused development conformance, not the broad
  repository Qualification gate or promoted runner-candidate repinning.
- Append remains allocation-free and never grows implicitly. Programs choose
  when to pay the full replacement peak and can distinguish refusal from
  capacity exhaustion.
- The first scalar implementation deliberately copies only resource-free
  elements. General move/destruction-aware backing transfer must preserve the
  same public transaction semantics when added.

## Reconsideration triggers

Broaden element support only with exact move, destruction, trace, rollback, and
teardown proofs. Add provider expansion only through an explicit capability and
typed revocation/retry contract. A future lower-peak growth operation must have
a distinct name and exact partial-progress or in-place guarantees. Do not turn
append into hidden allocation, retry an indeterminate mutation, expose backing
identity, weaken the verifier, or silently consume budget authority on refusal.
