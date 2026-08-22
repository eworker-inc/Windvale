# Decision 0824: Classify Vector and Sequence function values

## Status

Accepted on 2026-08-22.

## Context

Decision 0823 gave canonical `Foundationˉcollections.Vector<T>` and
`Sequence<T>` bounded WVGT identities, but ordinary non-generic function
signatures still stopped in the earlier Source Symbols phase. Even if a private
shape reached call lowering, the borrow checker did not distinguish a
move-owned vector from a shared immutable sequence. Runtime operations cannot
be connected safely until signatures and ownership use those exact identities.

The compiler must not infer ownership from a private shape's numeric range or
accept a lookalike collection module. It must consume the same validated type
catalog that admitted the Foundation identity.

## Decision

1. The early Source Symbols pass admits the syntax of a canonical imported
   `Foundationˉcollections.Vector<T>` or `Sequence<T>` function type and defers
   its concrete shape. It does not assign an ordinary nominal shape.
2. Source WIR requests concrete binding and resolves that deferred type through
   the bounded generic-type catalog. The catalog supplies the exact private
   shape, element argument, kind, and dependency evidence.
3. Call ownership classifies WVGT kind `11` (`Vector<T>`) as owned and kind `12`
   (`Sequence<T>`) as shared immutable. A borrowed sequence may satisfy a
   by-value read-through parameter; a borrowed vector may not satisfy a
   consuming by-value parameter.
4. A private shape without matching catalog evidence receives no collection
   ownership classification. Unqualified, lookalike, malformed, or otherwise
   unresolved types remain rejected by their existing phase boundary.
5. This checkpoint does not add allocation, append, freeze, length, indexing,
   WVB 1.18 publication, or runtime support.

## Consequences

- Ordinary function signatures can now carry exact Vector and Sequence
  identities into typed WIR without weakening Source Symbols.
- The shared/owned distinction is executable compiler behavior rather than a
  documentation-only promise. A Sequence fixture publishes 424 bytes of WVIR;
  the corresponding Vector fixture fails exactly as `Invalidˉborrow` and
  publishes no WVIR.
- The current 1,100,197-byte analyzer WVB has SHA-256
  `f678904797a4b81f621a457f33dc57d83403c3f57273935ebe773b7e1ec3b3f3`.
  Its 34,632,192-byte Windows development application has SHA-256
  `f0f1d776b502600b69e415f1772bc4f7210fd19f25731619ae74600690fe5d8b`.
- The registry contains 108 owners and 5,149 declared cases at SHA-256
  `7506d35f266b8dfacf5288685dbada8468f34731046bdf289269f1aa975f88e9`.
  Changed-file planning passes 31 general and 194 native routing cases.
- `Source-Wir-Core.wv` is 12,198 lines. The focused changes remain local, but a
  later refactor should move a cohesive collection expression-lowering and
  validation pipeline together with its callers. Moving only widely shared
  helpers would increase cross-module binding traffic and is not the chosen
  boundary.
- No source grammar, WVB version, runtime behavior, editor token, or public
  compatibility promise changes in this checkpoint.

## Reconsideration triggers

Reconsider the phase split only if the generic-type catalog can no longer be
the immutable owner of concrete collection identity, or if the runtime-backed
WVB 1.18 design requires a stronger ownership class. Do not replace catalog
validation with numeric private-shape inference or reuse the legacy
fixed-capacity collection encoding.
