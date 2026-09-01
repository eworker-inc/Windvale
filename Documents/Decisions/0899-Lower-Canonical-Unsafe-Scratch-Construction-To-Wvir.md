# Decision 0899: lower canonical unsafe scratch construction to WVIR

## Status

Accepted and implemented as a focused local Windows checkpoint on 2026-08-31.
This decision does not complete Slice 8 or claim paired-host qualification.
WVB representation, verifier/runtime/native execution, Foreign-call lowering,
containment, and one migrated runtime or OS boundary remain pending.

## Context

Decision 0898 published the exact compiler-owned Foundation unsafe identities
but deliberately left them unproducible. The smallest next vertical slice is
the safe `Constructˉscratch` producer: it consumes explicit memory authority,
accepts an exact size and alignment, and returns a typed failure instead of
exposing an address. It does not require an unsafe lexical block because it
does not itself let source observe or manipulate an address.

Treating the call as an ordinary function would lose its owned result, budget,
ABI, and later containment requirements. Reusing an existing memory opcode by
shape would also erase the distinction between ordinary managed allocation and
ABI-scoped foreign scratch. The typed IR therefore needs one exact operation
before an executable representation is selected.

## Decision

1. Recognize `Constructˉscratch` as a compiler intrinsic only through a
   qualified alias of the exact edition-1 System module
   `Foundationˉunsafe`. Record arity three and operation identity `186`.
2. Require the explicit call form `Constructˉscratch::<Abi>`, with exactly one
   declared enum type as the ABI identity. Do not infer the ABI from the result.
3. Require the contextual result to be the exact canonical
   `Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>` identity. Reject
   lookalike modules, records, failures, and mismatched ABI arguments.
4. Require named by-value arguments `Budget`, `Length`, and `Alignment`.
   `Budget` is one available exact `Memoryˉbudget` slot; the other two values
   are exact `u64` temporaries.
5. Emit WVIR operation `186` with two ordered operands, the result's private
   generic shape, the budget slot in `Target`, and the declared ABI enum shape
   in `Auxiliary`.
6. Select WVIR 1.23 without generic specializations and WVIR 1.24 with them.
   These versions retain the callable/task-family header geometry so one
   validator and consumer can compose mixed feature sets.
7. Classify the result as affine owned evidence and assign
   `memory.allocate`. Preserve the existing prohibition on ordinary
   construction and field observation of every compiler-owned unsafe value.
8. Independently validate the exact Foundation identities, operand count and
   shapes, budget-slot availability, ABI enum range, temporary sequence,
   ownership, and feature-to-version correspondence before publication.
9. Keep positive length, power-of-two alignment, budget capacity, provider
   allocation, zero initialization, and target addressability as typed runtime
   result conditions. The compiler does not accept only literal values or
   silently turn these failures into traps.
10. Deliberately reject otherwise valid operation `186` at the source-WVB
    boundary with `Unsupportedˉoperation`. A later decision must add its WVB
    version/opcode, complete verifier, runtime, native ABI lowering, and
    containment contract together.

## Evidence

The current Profile-8 Windows Analyzer is 52,659,712 bytes at SHA-256
`211caf31790087d81537be5a29700097e57ed87333d7696691cbbb83dd3c3ac0`.
The optimized emitter source set publishes 1,557,184 source bytes, a 104-byte
analysis manifest, 273,740 binding bytes, and 3,556,880 WVIR bytes. Emission
contains 738 functions and 1,249,322 code bytes and produces a 1,479,716-byte
WVB at SHA-256
`8d2ca39f2792210699a2ae11be33b28f44136722b606459b9e1a7fc86d2b98c1`.

The current reconstructed verifier accepts that emitter WVB as
`compiler-aligned`. The older pinned front-door verifier rejects the same valid
module without a diagnostic, so the ordinary cached package front door was not
used to imply a promotion. After current-verifier admission, the existing
bounded producer directly constructs a 30,899,712-byte local emitter
application at SHA-256
`713007426ffca090f1981647b09d22464138f849f5513990ec8ed979b5682c53`.
This is local implementation tooling, not a promoted verifier or distribution
identity.

`Tools/Native/Test-Language-1.0-Unsafe-Scratch-Wir.mjs` injects that retained
Analyzer and emitter into two valid and seven rejected source cases. It then
corrupts the valid WVIR nine ways: old minor, unknown operation, primitive
result, missing operand, invalid budget slot, invalid ABI shape, wrong width for
each scalar operand, and result-temporary shape mismatch. The focused run
reports:

```text
native language 1 unsafe scratch WVIR status=Passed cases=9 valid=2 rejected=7 malformed=9 operation=186 effect-check=emitter
```

## Consequences

- The first canonical Foundation unsafe value now has a real typed producer.
- Source cannot select the producer by spelling, infer its ABI from an expected
  result, forge its result record, or bypass the explicit memory budget.
- The runtime contract remains visible as unfinished work rather than being
  hidden behind an existing allocation opcode.
- The next semantic step is the exact executable scratch representation and
  provider containment boundary, followed by write-region borrowing and one
  migrated real consumer.

## Reconsideration triggers

Reconsider the operation shape if the executable representation cannot preserve
the exact ABI identity, budget accounting, zeroing, alignment, generation,
failure, and teardown contracts; if `Memoryˉbudget` cannot remain an explicit
owner; or if an ordinary source operation can construct, copy, inspect, or leak
the opaque scratch carrier. Any such case must fail closed before WVB admission.
