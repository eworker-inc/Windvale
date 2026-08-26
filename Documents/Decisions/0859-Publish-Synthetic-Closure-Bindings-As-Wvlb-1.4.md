# Decision 0859: publish synthetic closure bindings as WVLB 1.4

## Status

Accepted on 2026-08-26.

## Context

Decision 0858 assigned deterministic WVCL ordinals and physical targets to
source closures. The closure capture analyzer can also reconstruct an exact
function-private binding phase from validated capture evidence. Ordinary WVLB
1.1 through 1.3 ranges are still declaration-derived, however: their parameter
prefix comes from a top-level WVSD function declaration. A closure target has
no WVSD symbol and has captured values before its public parameters.

Publishing that synthetic phase through an older range would either misclassify
captures as parameters, forge a source declaration, or weaken canonical range
validation. The compiler instead needs one explicit retained-evidence carrier
for the distinct physical signature.

## Decision

1. WVLB 1.4 is the binding carrier for a non-empty WVCL 1.0 catalog. It has a
   48-byte header, 32-byte physical-function ranges, and unchanged 36-byte
   binding entries.
2. The header retains exact optional WVGC and WVGT lengths, the required WVCL
   length, the base physical-function count, catalog layout `3`, and one
   reserved zero. Catalog order is WVGC, WVGT, then WVCL.
3. A range retains first/count, real source declaration, inherited generic
   instance, closure ordinal, capture count, public parameter count, and flags.
   Flags `0` describe an ordinary or generic physical function; flags `1`
   describe a synthetic closure target.
4. A synthetic range maps to its real parent declaration and module. It never
   creates a WVSD symbol. Its bindings are captures first and public parameters
   second. Capture entries are only `let` or `var`; public parameters are only
   `Parameter`; slots remain contiguous from zero.
5. The publisher requires valid source-set, symbol, WVGC, WVGT, and non-empty
   WVCL evidence before construction. It validates parent bounds, declaration
   kind and module, source spans, counts, payload length, checked target
   arithmetic, and the 4 MiB retained-directory limit.
6. An independent validator reconstructs all catalog boundaries and validates
   exact length, canonical gap-free ranges, inherited identities, entry shape
   and kind, identifier and scope spans, capture/parameter partitioning, and
   trailing-byte absence. Malformed input fails before unchecked slicing or
   range arithmetic.
7. WVLB 1.1 through 1.3 remain byte-for-byte unchanged when no source closure
   target is published. WVLB 1.4 is compiler evidence and adds no source syntax,
   runtime authority, package ABI, or compatibility promise for obsolete
   experimental inputs.
8. This checkpoint publishes and validates the synthetic binding directory.
   Connecting it to main WVIR construction, emitting `Closureˉcreate`, and
   compiling the synthetic body remain the next Slice 6 step.

## Consequences

Closure targets now have one exact binding identity without weakening ordinary
function validation. Diagnostics continue to name the real parent module and
source span, while downstream typed lowering can distinguish captured physical
parameters from the callable's public parameters.

The format carries three catalogs once rather than copying provenance into
each binding entry. The additional range width applies only when closures are
present, so existing ordinary and generic compilation outputs do not grow.

## Evidence

The focused WVLB 1.4 component builds as a 918,460-byte WVB at SHA-256
`ebd6876706711f9897d13a1e26cca0370e9efe766c4bfcda987527d43db5e6f5`.
The 1,006,982-byte closure-capture self-test at SHA-256
`cb8f61170a9d9a8b76cb4bf842424e65c09e8a95f9f9e314b19e5bbf7aefe298`
executes selector `j` with result `42`. It proves one copied `i32` capture,
one public `i32` parameter, the exact 48-byte header and range identities, full
directory validation, and rejection after the retained capture count is
changed from one to two.

The complete focused Windows owner reports:

```text
native language 1 callable semantics status=Passed cases=51 result=42 modules=9 wvb-bytes=4255417 evidence-sha256=1f1d0a180c1276404ff8b4dfaa7518d589f4d169a8678f20e9f0c140215a4bc7
```

Main WVIR/WVB integration, captured move and borrow enforcement, independent
Linux reproduction, and Qualification remain separate claims.

## Reconsideration triggers

Reconsider this decision if synthetic target ordering cannot remain append-only,
if closure parameters require a different physical prefix, or if a later
compiler evidence envelope can preserve the same independent validation with
less retained state. Any replacement must retain real source provenance,
checked arithmetic, exact capture/public-parameter separation, deterministic
bytes, and failure-closed malformed-input behavior.
