# Windvale source generic type evidence

## Status and boundary

`Compilerˉsourceˉgenericˉtypeˉlowering` defines the bounded canonical identity
of concrete generic record, variant, and compiler-supplied Foundation
collection instances. `WVGT 1.0` is compiler-phase
evidence: it lets symbol binding, typed WIR, and WVB emission refer to one
monomorphic type identity without packing arbitrary nested arguments into a
`u32` or adding runtime generics.

The catalog, declaration ownership, and recursive full-arity type-use binding
are implemented and executed. Source Symbols recognizes generic record/variant
declarations, validates their ordered parameter namespaces and structurally
unresolved field uses, and defers a parsed `<...>` application to the WVGT-aware
semantic phase rather than assigning it an ordinary template shape. The focused
binding phase resolves uses such as
`Choice<Box<i32>, text>`, admits inner WVGT identities before parents, and
returns the exact replacement catalog. The generic layout and materialization
phases now substitute fields and cases, assign ordinary output type indices,
and retain private nested-instance identity in one bounded dependency-order plan. Main
Source WIR now scans ordinary function signatures and explicit locals, retains
the resulting catalog in WVLB 1.3, and carries catalog-bounded private shapes in
paired WVIR evidence. Main Source WVB consumes that catalog, materializes every
retained instance, omits source templates from the WVB Types table, remaps
ordinary and private nominal identities, and emits the concrete entries before
all retained Foundation `Option` and `Result` instances through that same
ordinary `__WvY` sequence; no Foundation specialization suffix remains.
Applied record construction and field access consume those same substituted
layouts in source bodies and lower through the ordinary monomorphic WIR/WVB
record operations. Applied variant construction and matching likewise consume
one substituted variant layout and lower through the ordinary monomorphic
variant create, case-test, and field operations. A concrete generic-function specialization also supplies a
bounded substitution context for an applied nominal use in its signature or
body. Thus `Box<T>` in `Wrap<T>` becomes the same catalog identity as
`Box<Point>` when `Wrap` is specialized for `Point`; no unresolved `T` enters
WVLB, WVIR, or WVB. A generic record or variant declaration may likewise store
an applied generic type whose arguments use its own direct type or constant
parameters. Binding `Holder<Point>` for `record Holder<T> { Value: Box<T>; }`
admits `Box<Point>` first and `Holder<Point>` second, then layout consumes that
immutable dependency order without growing the catalog.

The current constant-argument connection accepts one exact suffixed
fixed-integer token of the declared shape. Evaluation of the broader frozen
Language 1.0 constant-expression production remains a subsequent migration
checkpoint; it is not redefined as literal-only semantics here.

Every integer is unsigned little-endian. Lengths are exact. A catalog is
immutable evidence: admission returns a replacement value and never publishes
a partially appended entry.

## Limits and private shapes

The type catalog shares the frozen generic bounds:

| Boundary | Maximum |
| --- | ---: |
| Generic parameters per type declaration | 32 |
| Concrete generic type instances per catalog | 256 |
| Nested instantiation depth | 32 |
| Retained catalog evidence | 1,048,576 bytes |
| Estimated emitted type bytes | 16,777,216 bytes |

Within compiler evidence only, catalog instance `i` has shape
`0x80000000 + i`, for `i` from zero through 255. Values in
`0x80000000..0x800000ff` never enter WVB. Records, variants, and fixed arrays
are replaced by their concrete WVB type identities during emission. Owned
vectors and immutable sequences remain private compiler identities until their
WVB 1.18 operation/runtime checkpoint is connected; a WVB publication attempt
that reaches one is rejected rather than erasing or guessing its meaning. All other values at
or above `0x80000000` are invalid type arguments in WVGT 1.0.

An entry may refer to an earlier WVGT shape as a type argument. It may not refer
to itself or a later instance. This canonical dependency order permits one
bounded forward scan, makes every nested dependency available before its
consumer, and prevents cyclic or forward evidence from masquerading as a
concrete solution.

## WVGT 1.0 catalog

The catalog begins with this 24-byte header:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVGT`. |
| 4 | 2 | Major version `1`. |
| 6 | 2 | Minor version `0`. |
| 8 | 4 | Instance count. |
| 12 | 4 | Maximum observed exact nesting depth. |
| 16 | 4 | Exact retained evidence bytes, including the header. |
| 20 | 4 | Aggregate estimated emitted type bytes. |

Each instance has a 28-byte prefix:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Canonical WVSD declaration identity. |
| 4 | 4 | Concrete kind: record `4`, variant `8`, fixed array `10`, owned vector `11`, or immutable sequence `12`. |
| 8 | 4 | Exact nesting depth. |
| 12 | 4 | First-admission origin module identity. |
| 16 | 4 | First-admission source-byte offset. |
| 20 | 4 | Estimated emitted type bytes. |
| 24 | 4 | Ordered generic argument count. |

The prefix is followed by one 16-byte structural identity for every declared
parameter:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Argument kind: type `1` or compile-time constant `2`. |
| 4 | 4 | Zero for a type; exact declared fixed-integer shape for a constant. |
| 8 | 4 | Exact type shape or low constant bits. |
| 12 | 4 | Zero for a type or high constant bits. |

The entry identity is declaration, declaration kind, and the complete ordered
argument sequence. Origin, depth, and size estimate are diagnostic and
accounting fields, not identity. Equal arguments therefore reuse the first
instance even when a later origin supplies an excessive size estimate. A
phantom parameter remains part of identity even when no field mentions it.

Depth is derived, not trusted. A type with no WVGT argument has depth one. A
type containing earlier instances has one plus the maximum referenced depth.
Admission rejects missing, forward, out-of-range, or depth-33 dependencies.

## Validation and admission

Complete validation checks:

- exact magic, version, total length, retained-byte count, and aggregate values;
- every count and limit before an indexed read or allocation;
- admitted declaration/intrinsic kinds and non-sentinel identities and origins;
- canonical edition-1 `Foundationˉcollections` module identity for fixed arrays,
  owned vectors, and immutable sequences;
- complete WVGS solutions with one through 32 valid ordered arguments;
- exact fixed-integer constant width and value bounds;
- private-shape range, earlier-instance ordering, and recomputed depth;
- duplicate structural identities; and
- exact end-of-evidence with no trailing bytes.

Admission validates its input catalog and solution before searching. It reuses
an equal instance before checking new-instance count or growth. A new entry is
appended only after its derived depth, retained bytes, and aggregate estimate
fit every bound.

## Declaration-layout dependency plan

Generic nominal binding and layout share one bounded field plan rather than
parsing record and variant fields independently. The internal byte value begins
with a field count and variant-item count. It then carries one 16-byte item
entry per variant case and one 20-byte field entry per record field or variant
payload field. Item entries retain the case source range plus its first field
and field count. Field entries retain the name source range and the exact type
source position. The value is not serialized into WVLB, WVIR, or WVB.

The plan accepts at most 64 record fields or 256 variant cases with at most 64
fields each. Consumers validate the exact byte length before reading an entry.
For one complete generic declaration solution, binding substitutes direct type
and constant parameters into each planned field and recursively admits any
applied generic dependency before admitting the declaration itself. Any field,
limit, arity, kind, width, or recursive-depth failure returns the original
catalog, so a failed outer instance cannot publish an admitted prefix.

Value-containment cycles are not concrete layouts. The existing maximum
instantiation depth of 32 bounds their discovery and reports
`Genericˉresolution` without publishing analysis artifacts. Source declaration
lookup remains declaration-before-use; this dependency rule does not introduce
forward type declarations. WVGT application depth continues to describe nested
generic arguments, while catalog ordinal order also carries field-layout
dependencies. These are separate properties.

## Materialization plan

The materialization phase consumes exact Source Set, Source Symbols, and WVGT
evidence. It creates each concrete layout once in catalog order and assigns
instance `i` the ordinary Types index `First-type + i`. The complete range must
fit the existing 1,024-type compiler bound.

Each type entry is 36 bytes: private WVGT shape, ordinary output Types index,
declaration kind, declaration identity, module identity, first case, case
count, first field, and field count. Each case entry is 16 bytes: source name
offset, source name length, first global field, and field count. Each field
entry is 16 bytes: source name offset, source name length, output shape, and
nominal declaration kind.

An earlier WVGT reference retains its private shape in materialized field
evidence. This prevents an ordinary source nominal target from being confused
with an equal numeric output Types index after templates are removed. Final WVB
shape planning resolves the private identity to `65536 + output-index` for a
record, `196608 + output-index` for a variant, or the exact fixed-array type.
Vector and Sequence materialization entries deliberately have no fields or
cases and remain unsupported for WVB publication until their runtime-backed
shape and operations are connected. No private shape survives successful WVB
serialization. Each evidence stream is at most
4 MiB; all three streams plus the retained catalog are at most 16 MiB. Capacity
is checked before concatenation, and a failed plan publishes empty derived
evidence plus the first failing instance rather than a prefix.

The packed type entry exposes words zero through eight in the order documented
above; the packed case entry exposes words zero through three. A bounded word
accessor returns the existing `u32` sentinel for an invalid plan, index, or word.
This avoids allocating transient compiler-only materialized-type and case
records while keeping the byte layout and validation boundary explicit.

## Integration contract and progress

The connected source integration preserves these boundaries:

1. **Implemented:** retain every already-validated generic nominal parameter in
   declaration order without assigning the template an ordinary concrete shape;
2. **Implemented:** resolve a full-arity type use recursively, admitting inner
   concrete types before outer types and rolling the complete catalog back when
   the outer use fails;
3. **Implemented for direct parameters:** substitute WVGT arguments while
   validating record fields and variant cases, including concrete builder,
   capability, and nested-variant restrictions after substitution. In a
   concrete generic-function specialization, one applied generic nominal whose
   argument is a direct function type or constant parameter is resolved through
   that function's exact WVGC identity before WVGT admission. In a generic
   record or variant layout, applied fields using direct declaration type or
   constant parameters are recursively admitted in dependency order before the
   enclosing instance;
4. **Implemented:** carry private shapes through main WVLB/WVIR analysis only
   when the paired artifact binds exact WVGT evidence, retain unambiguous private
   references through materialization, and resolve them at WVB planning;
5. **Implemented:** serialize the plan through the main Source WVB entry point as
   ordinary monomorphic Types entries, omit templates, and remap function,
   field, temporary, and nominal-operation targets through one canonical plan;
6. **Implemented for records and variants:** parse an explicit applied
   construction target, validate its complete substituted layout, lower record
   construction and chained field reads through operations `17` and `18`, lower
   variant construction, case tests, and named field extraction through
   operations `65`, `66`, and `164`, and map every private target to the
   materialized ordinary WVB type. Variant construction spells the application
   after the selected case, as `Outcome.Value<Point> { ... }`; match patterns
   remain `case Outcome.Value { ... }` because the selector already has one
   exact concrete `Outcome<Point>` shape;
7. **Implemented for generic record formals:** infer a generic-function
   argument by decomposing an exact actual WVGT identity only when its generic
   declaration and full arity match the formal application. A different
   template, conflicting repeated contribution, or unsupported deeper pattern
   is rejected as `Genericˉresolution` rather than guessed;
8. reject an unused template only when a boundary requires a concrete value,
   while keeping uninstantiated source templates out of runtime artifacts.

The catalog does not define runtime type discovery, erased generics, dynamic
dispatch, reflection, an implicit allocation strategy, or a second backend.

## Verification

`Generic-Type-Catalog-Self-Test.wv` covers record and variant admission, exact
reuse, type-plus-constant identity, nested private shapes, accessors, malformed
and duplicate evidence, invalid declaration kind, rejected forward reference,
estimated-growth rejection, all 256 instance identities, a rejected 257th
instance, a depth-32 chain, and rejected depth 33.

The current fixture builds to a 66,384-byte compiler-aligned WVB with SHA-256
`21007237c68c3c74d165d5237eb6daaf112f81cca4a198ad690f6f1947a53006`.
Its 681,472-byte hosted Windows executable has SHA-256
`b6bf5abea06bf9ab2d6fc081742dc4c6812d0a3b80d149cb5bf733443ad7c924`
and returns `42` without output. Paired-host evidence is owned by the maintained
Language 1.0 verifier rather than claimed from this Windows implementation run.

`Generic-Nominal-Declaration-Self-Test.wv` adds 12 Source Symbols assertions:
generic records, variants, and phantom parameters; duplicate and
kind-mismatched parameters; rejected type-parameter application, builder
storage, and bare templates; ordinary nominal regression; and edition-1 WVSS 2
imports of the exact
Foundation `Option` and `Result` declarations. Its 604,172-byte WVB has SHA-256
`fa056f720caa741d3b3312e97ccd0b5dfce46559c07871f0e99eca229e06ca85`.
The four-fragment 15,083,520-byte hosted Windows executable has SHA-256
`c8e87418ca758bab33f9f16ff93d36f74f1a8f2e5cb962a6bc56c1c29ab4d83a`,
returns `42`, and writes no output. The independent
`generic-nominal-declarations` owner reproduces this focused evidence through
content-keyed compiler and hosted-application caches.

`Generic-Nominal-Type-Binding-Self-Test.wv` adds 18 focused assertions over
direct, repeated, nested, type-plus-constant, phantom, trailing-comma, and
ordinary nominal uses. It rejects wrong arity, argument-kind and constant-width
mismatches, bare templates, arguments on non-generic nominals, malformed
catalogs, and partial nested admission after a failed outer use. Its
649,494-byte WVB has SHA-256
`94a7b1672a846d329c9056f01539ca1d30499ddab0dc460862fd76a5855dfa9b`.
The four-fragment 15,842,304-byte hosted Windows executable has SHA-256
`9071f7a16051ff46f422cb692d63a10103d3b36e57d0242198203548dc9c0e07`,
returns `42`, and writes no output. The independent
`generic-nominal-type-binding` owner reproduces this boundary through the
content-keyed project and hosted-application caches.

`Fixed-Array-Type-Binding-Self-Test.wv` now also admits the canonical
`Foundationˉcollections.Vector<T>` and `Sequence<T>` identities as WVGT kinds
`11` and `12`. Fourteen added groups cover direct and repeated admission,
nested dependency order, exact element evidence, trailing commas, wrong arity
and argument kind, bare use, validated fieldless layouts and materialization,
rejection of a lookalike module, and rejection of hostile catalog evidence that
names that lookalike as an intrinsic owner. Together with the existing generic,
array, parser, and borrow groups, the focused owner reports 59 cases and returns
`42` without output. This checkpoint does not claim Vector operations or WVB
runtime support.

Canonical Vector and Sequence uses in ordinary function signatures are now
admitted through the early symbol pass as deferred types and resolved to their
exact private shapes by the WVGT-aware WIR phase. Borrow-mode checking consumes
the validated catalog kind: kind 11 Vector is move-owned and kind 12 Sequence
is shared immutable. Consequently, a borrowed Sequence may satisfy a by-value
read-through parameter, while a borrowed Vector cannot satisfy a consuming
by-value parameter. This connection still does not publish either type to WVB.

`Generic-Nominal-Type-Layout-Self-Test.wv` adds 21 focused assertions over
concrete record and variant layouts. It verifies field and case order, total
payload counts, private and ordinary substituted shapes, missing indices,
source/catalog identity mismatch, missing declarations, malformed evidence,
and caller-created layout tampering. It also proves dependency-first record and
variant layout, propagation of a direct constant parameter, and the resulting
private field links. Post-substitution checks reject builder record storage and
nested variant payloads. The nested rejection also exercises adjacent type
closers in `Choice<Choice<i32, text>, text>` while preserving expression `>>`
as a distinct lexer token. The independent `generic-nominal-type-layout` owner
reproduces the build, package, and execution boundary through content-keyed
caches and returns `42` without output.

`Generic-Nominal-Type-Materialization-Self-Test.wv` now carries 28 focused cases
over four dependency-ordered record and variant instances. It verifies assigned
ordinary type indices, fixed-width type/case/field ranges, record and variant
private-shape replacement, missing indices, an empty plan, the type limit,
invalid or missing catalog declarations, builder and nested-variant
rejection, and tampered-evidence reconstruction. The retired Foundation side-plan
admission and corruption cases are absent because Foundation `Option` and `Result`
now enter this same materialization plan as ordinary generic variants.

Its current 734,188-byte WVB has SHA-256
`bca0964398015bdda80f136b0ea5c1c6fd09283f5357cd24117a2bf562138646`.
It returns `42` and writes no output. The independent
`generic-nominal-type-materialization` owner reproduces this boundary through
content-keyed caches. Main Source WIR carriage, WVB insertion, template elision,
nominal target remapping, runtime generic-record construction/field use, and
runtime generic-variant construction/matching are connected. Applied generic
fields inside a generic declaration's own layout are also connected. Deeper
nested generic-function patterns and broader constant arguments remain later
checkpoints.

`Generic-Nominal-Main-Pipeline.wv` now covers the complete executable
analysis-to-WVB path. Its 20-case inspector requires exact 377-byte WVSS,
104-byte WVCA, 244-byte WVLB 1.3, 640-byte WVIR 1.3, and 441-byte WVB 1.11
products. It constructs `Box<Point>`, passes it through an exact generic-record
function boundary, and reads `Wrapped.Value.X`. The colliding ordinary source
and generic output targets are both one. WVB retains concrete `Point` at index
zero, emits `__WvY0000 { Value: Point }` at index one, and contains no `Box`
template. The compiler-aligned verifier accepts it and the runner returns `42`;
no private shape enters WVB. Four companion fixtures prove deterministic
missing, duplicate, mismatched, and unknown-field rejection without publishing
partial analysis artifacts; each writes its one exact diagnostic to standard
error and no standard output.

`Generic-Nominal-Function-Body.wv` additionally specializes `Wrap<T>` and
`Read<T>` for `Point`. It uses `Box<T>` in generic signatures, an explicit local,
record construction, return, field read, inferred direct call, and inferred
generic-nominal formal decomposition. The retained WVGC contains the two
function instances, WVGT contains one `Box<Point>`, WVB contains three concrete
functions and two ordinary record types, and execution returns `42`. Companion
fixtures reject mismatched construction, an unknown substituted field, and a
different generic template during inference without publishing partial
analysis products. A generic declaration whose own fields contain an applied
generic type and general generic variants are covered by the following
dependency and variant checkpoints; deeper nested formal patterns remain
separate work.

`Generic-Nominal-Variant.wv` covers an `Outcome<Point>` instance with a
two-field `Value`, one-field `Failure`, and zero-field `Empty` case. Construction
uses `Outcome.Value<Point>`, while the exact `Outcome<Point>` selector lets each
match arm use the non-applied case name. Its independent inspector proves the
WVGT instance, substituted WVLB bindings, private WIR operations `65`, `66`, and
`164`, ordinary materialized WVB variant metadata, template removal, and the
947-byte executable result. Execution returns `42`. Three companion fixtures
reject construction type mismatch, a missing construction field, and a pattern
binding type mismatch with exact diagnostics and no partial downstream
artifacts.
