# Windvale source generic resolution evidence

## Status and boundary

`Compilerˉsourceˉgenericˉresolution` is the bounded semantic foundation for
Language 1.0 generic inference and specialization. It gives the compiler one
canonical representation for a solved generic argument list and one catalog for
deduplicating concrete specializations before ordinary monomorphic WIR and WVB
emission.

The first lowering checkpoint connects direct generic function type parameters
to source symbols, bindings, WIR, and WVB emission. A type parameter may appear
as a complete value-parameter type, the complete result type, or both. Calls may
infer it from value arguments or supply the full ordered list with `::`. Every
admitted solution is cataloged before the function body is lowered through the
ordinary monomorphic backend.

This checkpoint does not claim general generic records, variants, nested
generic type expressions, constant generic parameters, phantom parameters, or
multiple concrete specializations of one declaration. A supported generic
declaration must be instantiated by the current compilation and currently has
exactly one concrete specialization. Those are explicit implementation
boundaries, not changes to the frozen Language 1.0 design.

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

WIR and WVB remain monomorphic. Language 1.0 does not gain runtime-erased
generic values, a runtime specialization service, or a second generic backend.
WVGS and WVGC are transient compiler evidence and are not embedded into the
published WVIR or WVB.

The independent WIR validator reconstructs a specialization solution from the
concrete parameter and result shapes recorded by monomorphic binding and WIR
evidence. It then revalidates every parameter and the result against the source
declaration. A parameter that appears nowhere in those direct signature
positions remains unsupported because its specialization identity cannot be
proven from the published monomorphic product.

The production compiler uses the compact
`Compilerˉsourceˉgenericˉlowering` producer for the direct-type subset. The
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
explicit result-only specialization, rejects conflicting inference, and rejects
a second distinct specialization without publishing partial evidence. Its
successful identity program publishes ordinary WVSS, WVCA, WVLB, and WVIR only.
The emitted WVB is byte-identical to a same-length hand-written monomorphic
oracle and executes with result `42`; no runtime generic mechanism is involved.
The focused publisher is a 1,048,153-byte WVB with SHA-256
`a2befed440f070ed934dd3ca783129cad30016ec2b46007548507f415cb3974a`
and its segmented hosted package returns `42`.

The equal generic/oracle evidence identities are
`7c30318a94a9c16965347d17da358b309aefaa01519bafed80e48eb52b4a294a`
for 104-byte WVCA,
`bda5d2ec661429a8649b3a23c905d1986fa5ad081b8c891c0283f5c534582a37`
for 148-byte WVLB, and
`dc3810d6b498fc2ff6d5676a584331df47292105daa13f5926dff309b1322be5`
for 560-byte WVIR. The unchanged emitter produces a 297-byte WVB with
SHA-256
`cb7f970929bcdafa15c5f13b817f013ba30c033933d2988283b2e5c41ea316b3`;
the pinned scalar runner returns `42` in 26 instructions.
