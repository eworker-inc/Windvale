# Decision 0905: transfer affine memory budgets through ordinary calls

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision closes the by-value callable-budget gap left by
[Decision 0904](0904-Execute-Wvb-1.33-Unsafe-Scratch-In-A-Bounded-Scalar-Provider.md).
It does not encode borrowed `Memoryˉbudget` calls in WVB, add native lowering,
publish a front-door artifact, or claim Linux or paired-host qualification.

## Context

WVB 1.33 and the scalar provider already supported a budget-bearing entry that
constructed scratch directly. The existing WVB local-take and direct-call
instructions could also carry an owned budget into a helper. The source-WIR
budget validator nevertheless activated only around named memory operations
and allowed a budget temporary only to enter a local. A child budget extracted
from `Split` was therefore rejected before WVB emission when its next use was
an ordinary call.

The scalar envelope had a second version-local gap. It recognized a
`Memoryˉbudget` field inside the older `Split` result variants but did not admit
that same affine field in a WVB 1.33 type directory. The executor and budget
provider did not require a new operation or representation.

## Decision

1. Activate the source-WIR budget ownership pass whenever a function produces
   private budget shape `805306368`, as well as for the existing split,
   collection, task, and scratch operations.
2. Record the source slot of each budget temporary. A local load proves the
   slot is live but does not decide ownership transfer prematurely.
3. Permit a budget temporary to enter a direct call only when the callee has a
   real corresponding parameter mode. A by-value parameter consumes the
   recorded source slot. A borrow mode ends only the temporary view and leaves
   the source slot live. The general owned-value validator remains the
   independent rule that rejects a borrowed value supplied to a consuming
   parameter.
4. Continue to reject a second by-value use, a dead source slot, a temporary
   from another basic block, an unknown callee parameter, and every unsupported
   budget use before bytecode emission.
5. Reuse the existing WVB 1.33 local-take and direct-call encodings. No source,
   WVIR, WVB, runtime ABI, or package-format version changes.
6. Permit shape `25` as a field of a WVB 1.33 variant so the canonical
   `Result<Memoryˉbudget, Allocationˉfailure>` returned by `Split` reaches the
   existing executor. Retain all other type, count, and ownership checks.
7. Execute three new by-value cases in the focused scalar oracle: root budget
   to helper, split child to helper, and a 32-byte child asked for a 64-byte
   scratch allocation. The refusal must report `Budgetˉexhausted`, 64 requested
   bytes, and 32 available bytes.
8. Keep ordinary immutable budget borrowing valid through source and WIR.
   WVB emission still returns exact `Unsupportedˉshape` for such a call because
   WVB 1.33 has no borrowed-budget call encoding. Do not silently lower it as a
   move.

## Implementation standing

Implementation commit `102e179ffbef10e587c892f8d50f205432795ee9`
builds a 1,482,938-byte emitter WVB at SHA-256
`c0289da13703a975511cca7c6768dc62e6804cc39c981c8becd96d22f4c45824`
and a 498,552-byte scalar-runner WVB at SHA-256
`12cf9cf519b31ab73ae77bf33c1adfe85ac4d5456fedf03159177799ad534898`.

The focused oracle passes eleven source/WIR cases, nine malformed WVIR cases,
seven malformed WVB cases, three semantic WVB mutations, eleven compiler
verifier cases, six runtime cases, and one runtime malformed case. Duplicate
budget transfer is rejected before WVB publication. Both successful transfer
programs return `42`; the refusal program returns `42` only after checking the
exact nested allocation failure.

## Consequences

- An entry budget or a rights-reduced child budget can now be delegated once
  to an ordinary helper without widening its byte or child limits.
- Helper decomposition no longer forces unsafe construction into the entry
  function, so later containment code can keep policy and mechanics separate.
- Affine ownership remains explicit: a call transfers the budget rather than
  copying it, and duplicate use remains invalid.
- Exact child-budget refusal is now executable evidence rather than an
  implemented but unforced branch.
- Borrowed-budget calls remain a named WVB lowering task. Native containment,
  pointer and write-region operations, authenticated Foreign calls, a migrated
  real boundary, and paired-host qualification also remain pending.

## Reconsideration triggers

Add a borrowed-budget WVB call shape only with an exact verifier, runner, and
native-lowering contract that distinguishes a temporary view from ownership
transfer. Reconsider the direct-call-only boundary when indirect calls can
carry affine budget modes without ambiguity. Do not add an implicit copy,
ambient allocation authority, or a second budget representation for
convenience.
