# Workload 8 implementation responsibilities

## Ownership matrix

| Contract | Primary owner | Required implementation evidence | Not a language feature |
| --- | --- | --- | --- |
| Contextual array literal | Lexer/parser/type/constant evaluator/editor | exact expected type/count, left-to-right evaluation, malformed/trailing-comma diagnostics | dynamic list literal or repetition |
| Fixed arrays | Type/layout/WIR/backends | inline finite layout, checked index, cross-target values | heap identity |
| Checked slices | Foundation collections + borrow checker | overflow/range failure, one-owner lifetime, no escape/overlap | pointer arithmetic |
| Mutable replacement | Foundation + ownership analysis | old/replacement ownership, in-range mutation, exclusive view | unchecked store |
| Strict scalar floats | Type checker/WIR/verifier/interpreter/native backends | every special case, no contraction/reassociation, subnormal preservation | host default float mode |
| Named FMA/classification/total order | Foundation numeric + optional intrinsics | simple bit/reference oracle and exact target lowering | operator overload |
| Conversion matrix | Generated Foundation surface/compiler/runtime | every pair/policy/failure identity and boundary | generic cast |
| Canonical formatting | Foundation text + reference algorithm | shortest round-trip/tie rules, 24-byte proof, exhaustive/random bit corpus | host locale formatter |
| Parallel lane library | Future bounded library/task owners | disjointness, lane-order publication, exact sequential differential | keyword or implicit tasks |
| Verification | Native owner registry and both-host workflows | parser, semantic, WIR, runtime, bit/hash, malformed and target rejection | paper-only pass claim |

## Likely WIR/backend work

Exact scalar operations may require versioned WIR operations for f32/f64
arithmetic, comparison, bit reinterpretation, classification, conversions, and
FMA. Array literal lowering and slices should use ordinary typed aggregates,
checked arithmetic, borrows, and indexed operations; they do not require raw
addresses in source.

An intrinsic implementation is permitted only beside a simple reference oracle
and differential coverage. Unsupported hardware may use a correct bounded
software helper. If neither preserves the strict contract, the target rejects
the strict floating profile.

## Verification slices after source freeze

1. grammar/editor array literal acceptance and malformed cases;
2. exact type/count/constant evaluation and ownership diagnostics;
3. array/vector/slice Foundation reference behavior;
4. interpreter scalar special values and conversions;
5. WIR verifier and native lowering per operation;
6. canonical formatting against an independent bit-oracle corpus;
7. cross-host exact lane/report/hash comparison; and
8. optional vector/parallel differential comparison with the sequential oracle.

One passing broader gate subsumes its narrower checks for an unchanged tree.
Qualification records target, host, tool identity, strict mode, elapsed time,
peak memory, output bits, and report hash.

## Performance record

Implementation must measure rather than assume:

- scalar versus admitted vector/parallel lane time;
- formatting throughput and maximum temporary/retained bytes;
- compiler specialization count and numeric lowering phase time;
- WIR operation/block count and WVB/native bytes; and
- any software-emulation cost on targets without direct FMA.

Optimization may improve those values only while exact outputs, bounds,
diagnostics, portability, and the reference path remain intact.
