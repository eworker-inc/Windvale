# Language 1.0 paper workload 8: numeric/graphics processing

## Status

Draft reviewed after the project owner accepted all six findings on 2026-08-17
under
[Decision 0762](../../../Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md).
This is paper Language 1.0 source. Current Seed tools do not accept it, and it
does not implement floating-point support or freeze edition 1.

## Result

Six Core modules express one bounded deterministic numeric transform that:

1. constructs four exact `Array<u32, 8>` fixtures with contextual array
   literals;
2. converts exact f32 bits explicitly;
3. creates checked immutable array slices and one exclusive mutable vector
   slice;
4. applies eight explicit fused multiply-add lanes in index order;
5. freezes the vector into an immutable sequence;
6. compares every result bit with a fixed oracle;
7. checks canonical NaN, infinities, signed zero, minimum subnormal, overflow,
   total order, bitwise equality, widening, narrowing, nearest, exact, and
   truncating conversion; and
8. emits one invariant 328-byte text report with SHA-256
   `25f308384b0a6ad088039cb3a65f5cf6eb928148b0f2cc9b18b2e2ca7c6ead2a`.

Lane zero proves why fusion is explicit: fused result `0xa8800000` differs from
ordinary `A * B + C`, which is `0x00000000` after two roundings.

## Source modules

| Module | Responsibility |
| --- | --- |
| `Numericˉgraphicsˉtypes` | Limits, failures, audit, and published result. |
| `Numericˉgraphicsˉfixture` | Four fixed arrays of exact f32 bit patterns. |
| `Numericˉgraphicsˉtransform` | Generic checked slices, exclusive vector fill, explicit FMA, freeze. |
| `Numericˉgraphicsˉaudit` | Bit oracle and strict special/conversion checks. |
| `Numericˉgraphicsˉreport` | Bounded canonical numeric text. |
| `Numericˉgraphicsˉapplication` | Limit validation, budget split, orchestration, publication. |

Every module is Core and targets Windows, Linux, and Windvale. There is no
capability requirement, task, FFI call, GPU dependency, allocator global, or
host numeric library in source semantics.

## Evidence index

- [numeric contract](Numeric-Contract.md)
- [package and execution plan](Package-Plan.md)
- [semantic review](Semantic-Review.md)
- [rejected and boundary cases](Rejected-Cases.md)
- [expected outcomes](Expected-Outcomes.md)
- [implementation responsibilities](Implementation-Responsibilities.md)
- [review findings](Review-Findings.md)

## Acceptance answer

Language 1.0 is practical for exact bounded f32 lane work without implicit
conversion, operator overloading, fast-math defaults, host formatting, unsafe
pointers, or new parallel syntax. Contextual fixed-array literals and exact
Foundation operations close the missing usability surface.

## Nonclaims

This is not an image codec, renderer, SIMD API, GPU API, tensor library,
arbitrary-precision package, decimal type, interval arithmetic system, or proof
that every hardware target implements strict floats. Those remain separately
versioned libraries/targets over the accepted scalar contract.
