# Decision 0842: Execute fallible Vector construction as WVB 1.24

## Status

Accepted on 2026-08-23. Recoverable append, general owned calls and joins,
semantic `using`, reverse-order release, paired-host conformance, and a real
hosted resource consumer remain pending within Slice 5.

## Context

Decision 0840 binds the exact public
`Vectorˉconstructˉreserved::<T>(Budget, Maximumˉitems)` call as typed WVIR
operation 172. Decision 0841 proves the generation-safe transition from one
consumed budget to one allocation lease, including atomic refusal and exact
parent credit. The remaining connected boundary must lower that operation to
verified bytecode, allocate a private Vector backing, preserve typed failure,
and release the lease with the final descriptor without serializing a pointer,
lease token, or heap representation.

The first emitted fixture also exposed a latent ordering distinction. Generic
instances are materialized in dependency order, but WVB Types entries are
canonical by nominal category. `Result<Vector<T>, Allocationˉfailure>` therefore
needs Result serialized before Vector even though materialization discovered
Vector first. Weakening the verifier would make type ranges non-canonical and
complicate every nominal-shape check.

## Decision

1. WVB 1.24 is the lowest minor containing executable fallible Vector
   construction. Opcode `CF` (`207`) is nine bytes: `CF u32 budget-local, u32
   result-type`.
2. `CF` consumes one top-stack `u64 Maximumˉitems`, consumes the named available
   shape-25 budget on every ordinary Result path, and produces the exact affine
   `Result<Vector<T>, Allocationˉfailure>` named by the second immediate.
3. `Maximumˉitems == 0` violates the public constructor precondition and traps
   with `WVR3008`. Positive requests that the selected target cannot represent
   return `Failure(Targetˉunaddressable, Requestedˉbytes,
   Availableˉbytes)`. Requested-byte reporting uses saturating `u64`
   multiplication; it never wraps.
4. The verifier derives the exact Vector descriptor and scalar element from the
   Result Valid payload. Neither the instruction nor the module serializes a
   heap pointer, capacity address, allocation generation, or lease layout.
5. The reference runner converts the consumed budget to one private allocation
   lease, allocates one fixed reserved Vector backing, and stores the lease with
   that backing. Provider refusal releases whichever owner remains local before
   publishing Failure. The final descriptor release releases the lease exactly
   once and credits its parent.
6. The scalar runner retains its explicit 2,047-cell maximum backing profile.
   That is an implementation resource bound, not a portable Language 1.0
   maximum. A conforming target may accept a larger positive maximum or return
   the specified recoverable target failure.
7. WVB 1.24 extends the existing bounded forward-control ownership proof to
   constructor Result owners. It permits direct consumption of the entry budget
   parameter, forbids ordinary loads of affine results, and transfers a Valid
   Vector exactly once through `local.take` and variant matching.
8. Generic materialization keeps dependency order, but assigns a separate WVB
   output rank by the five admitted nominal categories. Generic serialization
   walks those categories and remaps every private nominal reference. For the
   exact fixture the canonical Types order is Allocation failure, allocation
   reason, Result, Vector; Result points forward to Vector.
9. Modules without opcode `CF` retain their lowest earlier WVB version. Every
   WVB 1.24 module contains at least one exact `CF` instruction.

## Consequences

- The successful fixture deterministically emits 747-byte WVB 1.24 at SHA-256
  `e25ff63b466d3e4a219afdc03a64c2ff53418dffc9039fea0678ff3328d2dcd1`.
  Success and ordinary allocation refusal both execute to 42. A zero maximum
  fails with `WVR3008` after four guest instructions.
- The existing Split owner is extended rather than duplicated. It now passes 32
  cases: five valid modules, 19 malformed mutations, deterministic Split and
  Vector builds, both typed provider outcomes, and the zero-precondition trap.
- The verifier now reports the rejected semantic substage—normalization,
  structure, module capabilities, data, functions, code/exports, or types—only
  after a semantic failure. Valid-module work is unchanged.
- Refactoring the scalar runner's extended-operation and descriptor-release
  paths keeps its largest native function below the existing 2,048-cell frame
  bound; the bound was not widened.
- The native verification registry remains 112 owners and advances from 5,344
  to 5,361 cases at SHA-256
  `7da8ebac77d31f21554b198e9ee90598280c31c72cf65c1c7344835eddc4b8a4`.
- This checkpoint fixes durable bytecode, ownership, failure, and teardown
  semantics. It does not micro-optimize the transitional seed compiler or freeze
  the scalar runner's heap representation.

## Reconsideration triggers

Change the private heap, descriptor, or lease representation when a target has
measured reason, provided exact Result behavior, stale-owner rejection,
failure-local cleanup, final credit, and deterministic output remain unchanged.
Broaden element support only with resource-aware destruction and tracing.
Replace the 2,047-cell scalar profile only with an explicit measured profile;
do not reinterpret it as a portable source limit. Do not weaken canonical type
ordering, expose lease bytes in WVB, copy an affine constructor Result, or make
local release depend on provider availability.
