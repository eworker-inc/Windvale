# Windvale source generic resolution evidence

## Status and boundary

`Compilerˉsourceˉgenericˉresolution` is the bounded semantic foundation for
Language 1.0 generic inference and specialization. It gives the compiler one
canonical representation for a solved generic argument list and one catalog for
deduplicating concrete specializations before ordinary monomorphic WIR and WVB
emission.

The connected lowering checkpoints admit direct generic function type
parameters plus one structural bounded-collection form. A type parameter may
appear as a complete value-parameter type, the complete result type, or the
element of `sequence<Type, Maximum>` or `builder<Type, Maximum>`. A `const`
parameter with its exact declared fixed-integer shape may occupy the collection
maximum. Calls may
infer the structural arguments from a concrete collection or supply the full
ordered list with `::`. Every admitted solution is cataloged before the
function body is lowered through the ordinary monomorphic backend.

This checkpoint does not claim general generic records or variants, nested
generic type expressions or collections, constant generics outside the one
collection-maximum position, phantom parameters, or an uninstantiated
template-only declaration. A supported generic function may have multiple
concrete specializations in one compilation, bounded by the shared 256-instance
catalog limit. Those are explicit implementation boundaries, not changes to
the frozen Language 1.0 design.

`Compiler-Source-Generic-Types.md` separately defines WVGT 1.0, the first
bounded concrete identity for generic records, variants, nested arguments, and
phantom arguments. WVGT's executable catalog is implemented, but its source
binding, substitution, WIR, and WVB connections remain later Slice 4 work; this
function-resolution document does not imply those connections.

The artifacts are internal compiler-phase evidence. They are not source syntax,
package identity, distributable bytecode, or a runtime generic representation.
All integers are unsigned little-endian and all byte lengths are exact.

## Limits

| Boundary | Maximum |
| --- | ---: |
| Generic parameters per declaration | 32 |
| Concrete specialization instances per catalog | 256 |
| Instantiation depth | 32 |
| Retained catalog evidence | 1,048,576 bytes |
| Estimated specialized code | 16,777,216 bytes |

The resolver checks malformed lengths before indexed reads. Catalog admission
checks reuse before growth bounds, so an already admitted specialization remains
usable when the catalog is full. New publication is all-or-nothing.

## WVGS 1.0 solution evidence

A generic solution starts with this 20-byte header:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVGS` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Declared parameter count |
| 12 | 4 | Solved parameter count |
| 16 | 4 | Entry size, exactly `24` |

One 24-byte entry follows for each declared parameter:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Kind: type `1` or constant `2` |
| 4 | 4 | Fixed-integer shape, or zero for a type |
| 8 | 4 | Type identity or low constant bits |
| 12 | 4 | High constant bits, or zero for a type |
| 16 | 4 | First contributing argument index |
| 20 | 4 | First contributing source-byte offset |

An unsolved entry has zero value fields and sentinel origin fields. A solved
type has a nonzero identity. A constant must fit the exact fixed-width integer
shape declared by its parameter.

The first contribution retains its origin. A later structurally equal
contribution is accepted without replacing that origin. A conflicting
contribution reports both the current and retained related origins. Finishing
an incomplete solution reports the first unsolved parameter.

The public solution record contains a status, WVGS evidence, and a fixed
28-byte diagnostic record. Accessors derive the declared and solved counts from
valid evidence and use the diagnostic record when no evidence can be retained.
No separately mutable counter is authoritative.

## WVGC 1.0 specialization catalog

A specialization catalog starts with this 24-byte header:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVGC` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Instance count |
| 12 | 4 | Maximum observed instantiation depth |
| 16 | 4 | Exact retained evidence bytes |
| 20 | 4 | Aggregate estimated specialized code bytes |

Each entry has a 24-byte prefix:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Declaration identity |
| 4 | 4 | First-admission instantiation depth |
| 8 | 4 | First-admission origin module identity |
| 12 | 4 | First-admission source-byte offset |
| 16 | 4 | First-admission estimated code bytes |
| 20 | 4 | Generic argument count |

The prefix is followed by one 16-byte structural identity per argument:
kind, fixed-integer shape, low value, and high value. A specialization's
identity is its declaration identity plus this ordered structural argument
sequence. Origin, depth, and code estimate are diagnostic and accounting
evidence, not identity. Explicit and inferred calls therefore reuse the same
specialization when their solved arguments are equal.

Validation rejects malformed headers, invalid identities, duplicate
specializations, inconsistent aggregates, trailing bytes, and every declared
bound violation. Admission performs a deterministic linear reuse search. Full
catalog validation performs a bounded pairwise duplicate check over at most
256 entries. Hash-table order and collision behavior are not semantic inputs.

The public catalog record contains a status, WVGC evidence, and a fixed 12-byte
diagnostic record. Its counts and aggregate measurements are derived from the
canonical evidence. Admission additionally reports the selected instance and
whether it reused an existing entry.

## Compiler integration contract

Source-symbol and WIR integration:

1. create one solution in declaration-parameter order;
2. contribute explicit arguments and structurally inferred arguments with
   exact source origins;
3. finish the solution before catalog admission;
4. admit or reuse the canonical specialization before emitting code;
5. discover calls from ordinary functions in a planning pass, then compile
   admitted generic bodies to a bounded fixed point of at most 32 passes; and
6. substitute the concrete arguments into the ordinary binding and monomorphic
   WIR path.

A collection contribution first requires the same sequence-or-builder family,
then contributes the concrete element shape to the formal type parameter and
the concrete maximum to the formal constant parameter. A literal formal
element or maximum must equal the concrete descriptor. Repeated structural
contributions retain the ordinary first-origin and conflict behavior. Explicit
constant arguments require an integer token whose numeric shape exactly equals
the declared fixed-integer shape; `8` and `8u32` are therefore intentionally
different arguments for `const Maximum: u32`.

WIR and WVB remain monomorphic. Language 1.0 does not gain runtime-erased
generic values, a runtime specialization service, or a second generic backend.
WVGS is transient construction evidence. WVGC is retained inside specialized
WVLB 1.2 so an emitter can validate each appended concrete WVIR body against
its exact declaration and ordered substitution; neither evidence format enters
the emitted WVB.

WVLB 1.2 and WVIR 1.4 reserve one zero placeholder at the source declaration's
ordinary symbol-directory position and append one concrete function range/body
per WVGC instance in catalog order. An appended function identity is the full
WVSD entry count plus its zero-based catalog instance, not the number of source
function declarations. This remains correct when records, enums, variants,
data, capabilities, or fields precede a generic function in the symbol
directory.

The independent WIR validator treats the embedded catalog entry as the
canonical solved substitution. It validates every argument against the source
generic declaration, then checks that concrete parameter bindings and the WVIR
result shape exactly match the substituted signature. For collections this
includes the exact family, element, and maximum. Call targets are validated
against the complete specialized function range and mapped back to their source
declaration before arity and dynamic operand/result checks. A parameter that
appears nowhere in the admitted signature positions remains unsupported because
the current source path cannot derive a specialization for it.

The production compiler uses the compact
`Compilerˉsourceˉgenericˉlowering` producer for the connected function subset.
The
larger `Compilerˉsourceˉgenericˉresolution` module remains the canonical
correctness oracle for complete WVGS/WVGC validation, diagnostics, constant
argument identities, and limit behavior. Both implement the same evidence
identity. Keeping the oracle out of the production closure avoids duplicating
an ownership-heavy implementation in every compiler executable.

## Verification

`Generic-Resolution-Self-Test.wv` covers limit identities, solution
construction, first-origin retention, equal repeated contributions,
conflicting and unsolved diagnostics, first publication, origin-independent
reuse, distinct specialization admission, depth and code-growth rejection, and
catalog validation.

The complete fixture builds to a 53,164-byte WVB with SHA-256
`f275d1873ef38740e964181931cee03f6d84e300884f9b104e13725e4d05f967`.
The hosted native package returns `42`. The pinned scalar runner does not expose
the ownership-heavy record and byte-service closure used by this compiler
phase, so the focused owner executes the fixture through the hosted native
path. That runner-profile limitation is not treated as a semantic failure or
as evidence for general generic-source integration.

`Generic-Analysis-Publication-Self-Test.wv` exercises the connected compiler
path. It accepts inferred and explicit calls that reuse one identity, accepts an
explicit result-only specialization, rejects conflicting inference, and now
publishes two distinct specializations from one declaration as WVLB 1.2 and
WVIR 1.4. It checks the exact two-instance count before publishing its artifact
set, rejects a corrupted embedded catalog, and rejects a mismatched WVIR
specialization count. The current 1,068,726-byte publisher WVB has SHA-256
`d9179596701a415c4fd2105ca3f2a56c043ce46e2e4ff6878514cf271bd26f09`
and its segmented Windows package returns `42`. It writes a 360-byte WVLB 1.2
with SHA-256
`1189e27c21bf2281b59ecf9fdd8f8efa1d9671b049e17e9bbdb9e4baf390c74d`
and a 1,100-byte historical WVIR 1.2 with SHA-256
`3a07f3f96dda4d7be6b07636e312575b533664cea4c70a37c4b3991f89f71928`.
No runtime generic mechanism is involved.

The following single-specialization identities are retained as the historical
Decision 0786 checkpoint, rather than current multiple-specialization output:
`7c30318a94a9c16965347d17da358b309aefaa01519bafed80e48eb52b4a294a`
for 104-byte WVCA,
`bda5d2ec661429a8649b3a23c905d1986fa5ad081b8c891c0283f5c534582a37`
for 148-byte WVLB, and
`dc3810d6b498fc2ff6d5676a584331df47292105daa13f5926dff309b1322be5`
for 560-byte WVIR. The unchanged emitter produces a 297-byte WVB with
SHA-256
`cb7f970929bcdafa15c5f13b817f013ba30c033933d2988283b2e5c41ea316b3`;
the pinned scalar runner returns `42` in 26 instructions.

`Generic-Multiple-Specializations.wv` places a record before the generic
declaration, infers `Identity<i32>` and `Identity<u32>`, and explicitly reuses
the first instance. The current split compiler deterministically emits a
473-byte WVB with SHA-256
`39811a38c92b8d4a6459750c64f85cf4e500bb4a2e4e83d31ab3bab626a70e12`.
Strict compiler-aligned verification accepts it and the native scalar runner
returns `42`. The WVB contains three reachable functions: `Main` and the two
concrete specializations; the generic source placeholder is absent. Its unused
ordinary record declaration is also absent under the optimized writer's
all-or-nothing no-nominal-use rule.

`Generic-Collection-Analysis-Publication-Self-Test.wv` extends that source
analysis proof to structural type-plus-constant inference. It rejects a
repeated maximum conflict, an explicit constant with the wrong width, and a
builder supplied for a sequence parameter. Its successful program calls
`First<Type, const Maximum: u32>(sequence<Type, Maximum>)` once by inference
and once as `First::<i32, 8u32>`. Under the current contract the generic source
publishes specialized WVLB 1.2/WVIR 1.4, so its retained catalog and appended body
are intentionally not byte-identical to a monomorphic WVLB 1.1/WVIR 1.3 oracle.

The following publisher and oracle identities are retained as historical
Decision 0787 evidence. The focused publisher is 1,065,397 WVB bytes with SHA-256
`ca1b50539ab3c53966fde062e8816b829d25b0dc0bd14bcb3374a813443ecc7a`.
Its ordinary segmented native package stays below the unchanged 33,554,432-byte
whole-image ceiling at 33,487,778 selected machine bytes and returns `42`.
The generic/oracle products share 104-byte WVCA SHA-256
`debdc883ad8ebbde577589bc9248f58f79b70f5e7851409545b21be5282a73cb`,
184-byte WVLB SHA-256
`6df7f06016882fca5b38d909ca56136587a94975de60431daca96d13e9e35f4c`,
and 976-byte WVIR SHA-256
`c9a9299f223cae34887fd6788180f81b0b9a8d1499e99d5f81c2d053694361ab`.

That checkpoint's retained emission driver emitted the monomorphic oracle as a 466-byte WVB
with SHA-256
`2d59187da5f16a3b275a6bbe96502ce1309f0ba8348e8a22da02097808c8b0c6`.
Direct execution is not claimed at this checkpoint: the current pinned native
verifier rejects both this product and the unchanged 809-byte non-generic
collection fixture at its target-specific semantic boundary. The fully current
general emission-driver WVB builds successfully at 1,268,289 bytes, but its
37,097,130 selected native bytes exceed the unchanged whole-image ceiling, so
it was deliberately not packaged. A target-aware validated-analysis emission
split is the next downstream capacity checkpoint.
