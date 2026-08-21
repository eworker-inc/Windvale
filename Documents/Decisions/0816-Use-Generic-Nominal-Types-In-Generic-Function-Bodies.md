# Decision 0816: Use generic nominal types in generic-function bodies

- Status: Accepted with current-Windows development evidence; independent Linux qualification pending
- Date: 2026-08-21
- Advances: [Language 1.0](../../Specifications/Windvale-Language-1.0.md), [generic type evidence](../../Specifications/Compiler-Source-Generic-Types.md), [source bindings](../../Specifications/Compiler-Source-Bindings.md), [source WIR](../../Specifications/Compiler-Source-Wir.md), [source WVB](../../Specifications/Compiler-Source-Wvb.md), and [Decision 0815](0815-Execute-Generic-Record-Construction-And-Fields.md)

## Context

Decision 0815 made an explicitly concrete `Box<Point>` constructible and
readable in ordinary function bodies. A generic function still could not write
the natural equivalent:

```wv
fn Wrap<T>(Value: T) -> Box<T> {
    let Wrapped: Box<T> = Box<T> { Value: Value };
    return Wrapped;
}
```

WVGC already retained the concrete specialization argument, and WVGT already
represented `Box<Point>`, but the type binder considered those catalogs
separately. It therefore treated the `T` nested in `Box<T>` as unresolved.
Inference also recognized a direct formal `T` and the two collection
descriptors, but could not derive `T` from an actual `Box<Point>`.

The fix must join existing bounded evidence rather than add runtime generics,
another compiler, result-context inference, overload search, or an unbounded
recursive solver.

## Decision

1. Give generic nominal type binding an optional immutable generic-function
   specialization context: source function identity, declared generic
   parameters, exact WVGC catalog, and selected instance.
2. When one applied nominal argument directly names a function type or constant
   parameter, substitute its exact WVGC structural argument before ordinary
   WVGT admission. Keep all existing arity, kind, width, value, depth, count,
   retained-byte, and emitted-size limits.
3. Use that binder for specialized signatures, parameters, explicit locals,
   construction targets, returns, and field reads. Publish only concrete
   ordinary or WVGT-private shapes in WVLB and WVIR; never serialize `T` or a
   transient WVGS solution as a value shape.
4. Permit argument-derived inference from an actual generic nominal only when
   it is an admitted private WVGT identity whose declaration and full arity
   exactly match the formal application. Contribute each direct formal
   parameter through the existing WVGS kind, width, shape, and equality rules.
5. Reject a different generic declaration, conflicting repeated contribution,
   or unsupported pattern as `Genericˉresolution`. Preserve field failures as
   the existing `Typeˉmismatch` or `Unknownˉfield` statuses and publish no
   partial analysis artifact family.
6. Reuse WVIR 1.4, `Recordˉcreate = 17`, `Recordˉfield = 18`, WVLB 1.3,
   WVGC 1.0, WVGT 1.0, and the existing materialization plan. WVB contains only
   ordinary monomorphic function signatures, locals, record operations, and
   Types entries.
7. Do not claim generic variants, applied generic fields inside a generic
   declaration's own layout, or deeper nested formal patterns. They remain
   separately bounded Slice 4 checkpoints.
8. Continue evolving this active Windvale-written compiler into Compiler 1.0.
   The immutable Stage 0 release remains recovery provenance; completion does
   not create a parallel replacement Seed compiler.

## Evidence

The active split route emits a 1,046,844-byte analyzer WVB with 508 functions
and 856,375 code bytes at SHA-256
`d65b1e159768ea7fa75775de1015f57ceb92af8ac76ae45e5dec301258d79d06`.
Its eight-fragment Windows application is 33,302,528 bytes at SHA-256
`7dd3682ac2072a8b0505b23afda5e0baef72fa2854ecaeda08afe27688fd081c`.
The 976,573-byte emitter WVB contains 532 functions and 808,976 code bytes at
SHA-256
`e0bcf72d6f04efbd9e24b139bb7a9db48dba7538571c4dda371292e5b1093230`.
Its six-fragment Windows application is 21,560,320 bytes at SHA-256
`d4cde64d604b7f59f00cb4b21cc550671d081a6162576cd63f510abe596d3182`.

`Generic-Nominal-Function-Body.wv` exercises inferred `Wrap(Point)`, inferred
`Read(Wrapped)`, `Box<T>` in both signatures, an explicit local, construction,
return, and field access. Analysis publishes exact 467-byte WVSS, 104-byte
WVCA, 504-byte WVLB 1.3, and 1,040-byte WVIR 1.4 products. Emission publishes
a 600-byte WVB 1.11 at SHA-256
`a27f28ed39ba407c196f461723d1232563372e7684203ee29e151fdb383dacc6`.
The 28-case inspector proves two exact function instances, one `Box<Point>`
instance, removal of both templates, concrete WVB signatures and operations,
and execution result `42`.

Three focused negative fixtures return exact `Typeˉmismatch`,
`Unknownˉfield`, and `Genericˉresolution` WIR statuses with no standard
output or partial artifacts. The compiler-scale generic-WIR fixture is
1,201,537 bytes at SHA-256
`b01236a677548399e0f2cc49410b459291d37fe433fd3f730ae671573a5d87d4`.
The coordinated Language 1.0 owner passes all 219 cases in 577,760
milliseconds, or 578,260 milliseconds including coordinator overhead. The
109-owner registry contains 4,966 cases at SHA-256
`b0a66a9bf4a8615755faaa5fec5ad78abc5987a92385f125e11a41a960f836e9`.
These are development results, not paired-host conformance or release
qualification.

## Consequences

Generic functions can now use a generic record as an ordinary monomorphic type
after specialization. Library authors may write one `Wrap<T>` or `Read<T>` and
receive concrete checked bodies for every admitted call. The runtime, verifier,
JIT, and AOT paths do not gain a runtime-generic mode.

The compiler joins WVGC and WVGT through explicit immutable context, increasing
the relevant compiler closures but not any semantic limit. Remaining nested
patterns should extend the same bounded evidence only when their dependency
order is explicit; they must not be hidden behind recursive retry or catalog
mutation.

## Reconsideration triggers

Revisit this decision if Language 1.0 adds higher-kinded parameters, partial
generic applications, result-context inference, overload sets, reified runtime
generics, or a canonical dependency-order rule that safely admits the excluded
nested declaration patterns.
