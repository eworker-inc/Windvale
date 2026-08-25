# Windvale source function-type evidence

## Status and boundary

`Compilerˉsourceˉfunctionˉtypeˉlowering` defines the bounded canonical
compiler identity of a concrete structural function type. `WVFT 1.0` is
compiler-phase evidence. It distinguishes complete signatures without packing
an arbitrary signature into one `u32`, conflating it with a generic nominal
type, or exposing a compiler-private identity as a WVB runtime type.

The catalog format, immutable admission, accessors, and independent validation
are implemented and execute through the maintained native path. Connecting
source type uses, named function values, closures, captures, WVIR, and the
selected closed runtime representation is separate work in Language 1.0 Slice
6. This representation checkpoint does not make those consumers appear
implemented.

Every integer is unsigned little-endian. Lengths are exact. Admission returns a
replacement catalog and never publishes a partially appended entry.

## Structural identity

A function-type identity consists of:

- `async` and `unsafe` flags;
- the exact portable, hosted, or system language profile;
- the exact result shape and its value transfer mode;
- the canonical language-effect and capability-effect bit sets; and
- every parameter's exact shape and transfer mode in declaration order.

The current result mode is exactly by-value zero. Parameter modes are by-value
zero, immutable borrow one, and mutable borrow two. Parameter names, source
locations, implementation function identities, capture values, and estimated
emitted bytes are not type identity. Named-call labels therefore do not alter
function-value compatibility.

A function value may only use a WVFT identity after its effect masks have been
resolved through the canonical source-effect registry. WVFT preserves exact
resolved masks; it does not independently assign a meaning to an unknown bit.

## Limits and private shapes

| Boundary | Maximum |
| --- | ---: |
| Parameters per function type | 64 |
| Function-type instances per catalog | 256 |
| Retained catalog evidence | 1,048,576 bytes |
| Aggregate estimated emitted bytes | 16,777,216 bytes |

Within compiler evidence only, catalog instance `i` has shape
`0x80000100 + i`, for `i` from zero through 255. The preceding
`0x80000000..0x800000ff` range remains owned by WVGT concrete generic types.
WVFT shapes never enter WVB. A later phase must replace every retained WVFT
shape with the selected public callable representation or reject publication.

An entry may refer to an ordinary nonzero source shape, any valid WVGT private
shape, or an earlier WVFT instance. It may not refer to itself, a later WVFT
instance, or any other value in the compiler-private range. Dependency order is
therefore canonical and cyclic or forward structural evidence cannot be
presented as a complete type.

## WVFT 1.0 catalog

The catalog begins with this 24-byte header:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVFT`. |
| 4 | 2 | Major version `1`. |
| 6 | 2 | Minor version `0`. |
| 8 | 4 | Instance count. |
| 12 | 4 | Maximum observed parameter count. |
| 16 | 4 | Exact retained evidence bytes, including the header. |
| 20 | 4 | Aggregate estimated emitted bytes. |

Each instance has a 32-byte prefix:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Flags: bit zero `async`, bit one `unsafe`; all other bits zero. |
| 4 | 4 | Language profile: portable `1`, hosted `2`, or system `3`. |
| 8 | 4 | Exact result shape. |
| 12 | 4 | Exact result transfer mode; currently zero. |
| 16 | 4 | Canonical language-effect mask. |
| 20 | 4 | Canonical capability-effect mask. |
| 24 | 4 | Ordered parameter count. |
| 28 | 4 | Estimated emitted bytes for bounded accounting. |

The prefix is followed by one 8-byte entry per parameter:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | Exact parameter shape. |
| 4 | 4 | Transfer mode: value `0`, immutable borrow `1`, mutable borrow `2`. |

The size estimate is accounting evidence rather than structural identity. An
equal signature reuses its first instance before a later estimate is tested
against growth bounds. This matches WVGT's first-admission rule and prevents a
different diagnostic estimate from inventing a different program type.

## Validation and admission

Complete validation checks:

- exact magic, version, total length, retained-byte count, and aggregates;
- every count, cursor, multiplication, subtraction, and limit before a read;
- admitted flags, profile values, result mode, and parameter modes;
- nonzero ordinary shapes, the exact WVGT range, and earlier-only WVFT
  dependencies;
- duplicate structural identities;
- aggregate estimated-byte overflow; and
- exact end-of-evidence with no trailing bytes.

Admission validates its input catalog and complete parameter payload before it
searches for identity reuse. It reuses an equal instance before testing the
new-instance, retained-evidence, or estimated-growth limits. A new entry is
appended only after every bound succeeds, and the resulting catalog is
independently revalidated before it is returned.

## Runtime separation

WVFT is neither a pointer, function address, symbol spelling, ABI record, WVB
type entry, nor closure environment. It provides stable compiler reasoning
only. The source compiler may use it to prove assignments, arguments, returns,
effect compatibility, and indirect-call signatures. WVB publication must use a
separately specified portable representation whose verifier checks the same
signature and ownership facts. Native addresses remain backend details and can
never become portable source semantics.
