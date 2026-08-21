# Windvale source generic type evidence

## Status and boundary

`Compilerˉsourceˉgenericˉtypeˉlowering` defines the bounded canonical identity
of concrete generic record and variant instances. `WVGT 1.0` is compiler-phase
evidence: it lets symbol binding, typed WIR, and WVB emission refer to one
monomorphic type identity without packing arbitrary nested arguments into a
`u32` or adding runtime generics.

The catalog and the first source connection are implemented and executed.
Source Symbols now recognizes generic record/variant declarations, validates
their ordered parameter namespaces and structurally unresolved field uses, and
rejects a bare template where a concrete type is required. General full-arity
generic nominal use still does not bind, lower, or emit. That connection must
consume WVGT, validate every ordered argument against WVSD and source,
substitute fields exactly, and emit only reachable concrete types. The existing
private Foundation `Option` and `Result` shapes remain unchanged until that
integration deliberately migrates them.

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
`0x80000000..0x800000ff` never enter WVB. They are replaced by ordinary
concrete record or variant type identities during emission. All other values at
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
| 4 | 4 | WVSD declaration kind: record `4` or variant `8`. |
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
- record/variant declaration kinds and non-sentinel identities and origins;
- complete WVGS solutions with one through 32 valid ordered arguments;
- exact fixed-integer constant width and value bounds;
- private-shape range, earlier-instance ordering, and recomputed depth;
- duplicate structural identities; and
- exact end-of-evidence with no trailing bytes.

Admission validates its input catalog and solution before searching. It reuses
an equal instance before checking new-instance count or growth. A new entry is
appended only after its derived depth, retained bytes, and aggregate estimate
fit every bound.

## Integration contract

The remaining source integration must preserve these boundaries:

1. retain every already-validated generic nominal parameter in declaration
   order without assigning the template an ordinary concrete shape;
2. resolve a full-arity type use recursively, admitting inner concrete types
   before outer types;
3. substitute WVGT arguments while validating record fields and variant cases;
4. carry private shapes only through validated compiler artifacts that bind the
   exact WVGT evidence;
5. materialize reachable instances as ordinary monomorphic WVB types in
   canonical dependency order; and
6. reject an unused template only when a boundary requires a concrete value,
   while keeping uninstantiated source templates out of runtime artifacts.

The catalog does not define runtime type discovery, erased generics, dynamic
dispatch, reflection, an implicit allocation strategy, or a second backend.

## Verification

`Generic-Type-Catalog-Self-Test.wv` covers record and variant admission, exact
reuse, type-plus-constant identity, nested private shapes, accessors, malformed
and duplicate evidence, invalid declaration kind, rejected forward reference,
estimated-growth rejection, all 256 instance identities, a rejected 257th
instance, a depth-32 chain, and rejected depth 33.

The current fixture builds to a 65,457-byte compiler-aligned WVB with SHA-256
`1387baaf0d9da4deed9ac5a7d37530f47c086c178461576e29f66168240e7d8b`.
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
