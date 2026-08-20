# Windvale source generic resolution evidence

## Status and boundary

`Compilerˉsourceˉgenericˉresolution` is the bounded semantic foundation for
Language 1.0 generic inference and specialization. It gives the compiler one
canonical representation for a solved generic argument list and one catalog for
deduplicating concrete specializations before ordinary monomorphic WIR and WVB
emission.

This checkpoint does not yet connect generic source syntax to source symbols,
WIR, or WVB emission. It therefore does not claim that general generic
functions, records, or variants compile. The declaration and call parsers admit
the frozen syntax; the next compiler phase must produce the evidence specified
here and then lower each admitted catalog entry through the existing
monomorphic backend.

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

The later source-symbol and WIR integration must:

1. create one solution in declaration-parameter order;
2. contribute explicit arguments and structurally inferred arguments with
   exact source origins;
3. finish the solution before catalog admission;
4. admit or reuse the canonical specialization before emitting code; and
5. substitute the concrete arguments into the ordinary monomorphic WIR path.

WIR and WVB remain monomorphic. Language 1.0 does not gain runtime-erased
generic values, a runtime specialization service, or a second generic backend.

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
