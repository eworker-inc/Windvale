# Windvale source closure lowering catalog

## Status and boundary

`Compilerˉsourceˉclosureˉlowering` owns the compiler-private deterministic
catalog of source closure sites that will become synthetic physical functions.
It sits after explicit capture and callable-type analysis and before WVIR
publication. It does not parse a closure, infer captures, change ownership, or
publish a WVIR function by itself.

The catalog gives later lowering one stable identity rule:

```text
synthetic target = ordinary symbol entries + generic function instances + closure ordinal
```

Closure ordinals are assigned in first deterministic compiler traversal order.
Repeating the exact same site and semantic identity reuses its ordinal and does
not grow retained evidence. Repeating a site with a different callable-type
instance or capture count rejects instead of silently changing its meaning.

This catalog is compiler evidence, not a source, package, ABI, or runtime
format. No application can construct it to acquire authority.

## WVCL 1.0 evidence

The retained evidence begins with this 24-byte little-endian header:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVCL` |
| 4 | 2 | major version `1` |
| 6 | 2 | minor version `0` |
| 8 | 4 | closure entry count |
| 12 | 4 | exact maximum capture count |
| 16 | 4 | exact retained byte count |
| 20 | 4 | sum of admitted source-span byte lengths |

Each following fixed 24-byte entry contains:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | owning module index |
| 4 | 4 | parent physical function index |
| 8 | 4 | closure-expression source byte offset |
| 12 | 4 | closure-expression source byte length |
| 16 | 4 | exact function-type catalog instance |
| 20 | 4 | explicit capture count |

The source-site identity is the tuple `(module, parent function, offset,
length)`. No two retained entries may have the same tuple. The callable-type
instance and capture count are semantic assertions attached to that site, not
part of an alternate identity that could create two targets for one expression.

## Bounds and rejection

One catalog retains at most 256 entries, 8,192 evidence bytes, 64 captures per
entry, and 16,777,216 aggregate source-span bytes. Admission validates the
module, parent-function, source-span, function-type, and capture bounds before
any append. Target arithmetic rejects overflow.

Full evidence validation recomputes the exact retained length, maximum capture
count, aggregate span length, and uniqueness of every site. Admission scans at
most 256 entries. Full duplicate validation performs at most 32,640 pair
comparisons; the quadratic check is therefore explicitly bounded and occurs on
small compiler evidence, never on unbounded source data.

Malformed magic, version, counts, lengths, spans, type instances, capture
counts, aggregate accounting, duplicate sites, or a non-`Valid` status reject
as invalid evidence. An invalid catalog is never partially reused.

## Integration state

WVCL 1.0 supplies deterministic identities and bounded evidence for the next
Slice 6 step. The WVIR compiler still needs to admit a site while compiling the
parent expression, compile catalog entries after ordinary and generic
functions, publish the matching synthetic binding ranges, and validate the
catalog at the WVIR/WVB trust boundaries. Captured move invalidation, borrow
lifetime/escape proof, effectful callables, and the selected native callable
ABI remain later connected work.

## Focused evidence

The maintained self-test covers empty evidence, first admission, exact reuse,
stable ordering, every accessor, target arithmetic, invalid modules, parents,
spans, types, zero and excessive captures, conflicting site semantics, the
256-entry bound, the aggregate-span bound, truncation, bad magic, and duplicate
site evidence. Its packaged Windows development execution returns `42`.

The exact component is a 14,524-byte WVB at SHA-256
`5cf39d57dd9f69cc0e3e90ae20742c907527481cd3d43c77af6ba1c4f672b13d`.
The self-test is a 23,078-byte WVB at SHA-256
`adbb332cd832d69c06660688508d8d45ea981f00f9563497de574790c47d977d`.
These are current-host development identities, not paired-host qualification.
