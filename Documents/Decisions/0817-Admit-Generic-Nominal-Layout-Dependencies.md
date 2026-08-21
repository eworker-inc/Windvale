# Decision 0817: Admit generic nominal layout dependencies

- Status: Accepted with current-Windows development evidence; independent Linux qualification pending
- Date: 2026-08-21
- Advances: [Language 1.0](../../Specifications/Windvale-Language-1.0.md), [generic type evidence](../../Specifications/Compiler-Source-Generic-Types.md), [source bindings](../../Specifications/Compiler-Source-Bindings.md), [source WIR](../../Specifications/Compiler-Source-Wir.md), [source WVB](../../Specifications/Compiler-Source-Wvb.md), and [Decision 0816](0816-Use-Generic-Nominal-Types-In-Generic-Function-Bodies.md)

## Context

Decision 0816 made `Box<T>` usable inside a specialized generic function, but a
generic declaration could not use the same application in its stored layout:

```wv
record Box<T> {
    Value: T;
}

record Holder<T> {
    Wrapped: Box<T>;
}
```

Binding `Holder<Point>` solved `T = Point`, but the layout phase encountered
`Box<T>` after the outer instance had already entered WVGT. It could neither
publish the required `Box<Point>` before `Holder<Point>` nor mutate the
immutable catalog during layout, so later record construction failed as an
invalid record literal.

The fix must preserve deterministic dependency order, immutable phase evidence,
the existing generic bounds, monomorphic WVB, and the bootstrap compiler's
bounded staging envelope. It must also distinguish a generic-argument nesting
depth from a field-layout dependency and reject recursive value containment.

## Decision

1. Parse each validated generic record or variant declaration into one shared
   bounded field plan used by both binding and layout. Record plans contain at
   most 64 fields. Variant plans contain at most 256 cases and 64 fields per
   case. Every consumer validates the exact byte length before indexed reads.
2. After solving an outer declaration's complete generic arguments, substitute
   its direct type and constant parameters into every planned field. Recursively
   bind each applied generic field against a working catalog before admitting
   the outer declaration.
3. Preserve the first deterministic dependency order. For the example,
   `Box<Point>` precedes `Holder<Point>` in WVGT even though both applications
   have generic-argument depth one. WVGT depth remains argument nesting; catalog
   order additionally expresses layout availability.
4. Make recursive admission transactional. Any field, arity, kind, width,
   capacity, size, or depth failure returns the caller's original catalog. A
   failed declaration cannot publish an admitted dependency prefix.
5. Treat recursive value containment such as `Loop<T> { Next: Loop<T>; }` as
   `Genericˉresolution`. The existing maximum instantiation depth of 32 bounds
   discovery, and analysis publishes no partial WVSS, WVCA, WVLB, or WVIR
   artifact family.
6. Keep declaration lookup declaration-before-use. Dependency-first admission
   does not silently add forward type declarations or reorder the source graph.
7. Make layout read only: it consumes the same field plan and may reuse only
   WVGT identities already admitted by binding. A late missing dependency is an
   error, not permission to mutate evidence.
8. Reuse WVGT 1.0, WVLB 1.3, WVIR 1.4, and WVB 1.11. Materialization replaces
   every private dependency with an ordinary concrete record or variant type;
   no template, type parameter, solution, or runtime-generic object enters WVB.
9. Continue evolving the active Windvale-written compiler toward the first
   conforming Compiler 1.0. “Seed” names the temporary source subset accepted
   during migration, not a frozen compiler implementation that must later be
   discarded. The immutable Stage 0 release remains recovery provenance only.

## Evidence

The active split route emits a 1,055,646-byte analyzer WVB with 515 functions
and 863,823 code bytes at SHA-256
`b4f04ef8d843f1af0dcd405e3c19a02f8a11532ac9996745e8b3fc7b956c4e7d`.
Its eight-fragment Windows application is 33,436,160 bytes at SHA-256
`c5da4daf6aff6be48fd37e79bf5ac81ad8986ff9c796d54bf4905e6ea17ed847`.
The connected 985,374-byte emitter WVB contains 539 functions and 816,424 code
bytes at SHA-256
`a95c9c248554919bfedae75795077467570924157d0c59a22060f1c1617a50e6`;
its six-fragment Windows application is 21,693,952 bytes at SHA-256
`c17e10f3d1d6abde396fdc274627e3b6046af26d8671910481f525c3b7208804`.

`Generic-Nominal-Declaration-Dependency.wv` publishes exact 526-byte WVSS,
104-byte WVCA, 564-byte WVLB 1.3, and 1,168-byte WVIR 1.4 products. Emission
publishes a 668-byte WVB 1.11 at SHA-256
`5ec54be82a84a0bea60fd6cb8146c08ddf8fb934aaf9560734250eadd20ee046`.
The 32-case inspector proves dependency-ordered `Box<Point>` and
`Holder<Point>` instances, concrete WVB Types and field targets, removal of all
templates, compiler-aligned verification, and execution result `42`. The cycle
fixture adds one exact rejection case without partial outputs.

The focused generic nominal layout owner passes 21 cases and returns `42`.
The compiler-scale generic-WIR fixture is deterministic at 1,210,339 bytes and
SHA-256
`8587efd5dd86073ed0cdfe3c9a134bcb91c2b8316e82f9abfa800622c7530340`.
The coordinated Language owner passes all 252 cases in 587,790 milliseconds,
or 588,290 milliseconds including coordinator overhead. The 109-owner registry
contains 5,002 cases at SHA-256
`75649a099553cb3d11037dcfd83f9e36c464aa59a04307ab67187f8ff77dba9a`.
These are development results, not paired-host conformance or release
qualification.

## Consequences

Library and application types can compose generic records and variants through
their own direct parameters while remaining ordinary concrete types at runtime.
Binding owns dependency discovery; layout and emission become deterministic
consumers of immutable evidence. The shared plan removes duplicated record and
variant scans, while recursive failure remains bounded and atomic.

This does not add recursive value types, forward declarations, higher-kinded
types, partial generic application, implicit heap indirection, runtime generic
metadata, or broader generic-function pattern inference.

The new reachable compiler logic increases analyzer, emitter, and compiler-test
artifacts. Current hosted container construction remains a material development
cost; its cache and staging performance should be improved independently rather
than weakening the source contract or its verification.

## Reconsideration triggers

Revisit this decision if Language 1.0 adds explicit indirection for recursive
types, forward declaration units, higher-kinded parameters, lazy or erased
runtime generics, or a replacement canonical ordering that can preserve the
same deterministic and transactional guarantees.
