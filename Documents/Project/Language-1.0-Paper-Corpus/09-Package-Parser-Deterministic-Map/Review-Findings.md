# Workload 9 review findings

## Status

First-author review is complete. The project owner authorized direct acceptance
of all recommended correctness/completeness findings on 2026-08-17; all six
findings are accepted under
[Decision 0763](../../../Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md).
They are normative-candidate/source-freeze inputs, not implementation or final
freeze claims.

## Finding 1: mutable map promises need exact completion/publication calls

The candidate promised replacement, removal, and immutable publication but
fixed only construction, insertion, and mutable-owner observation signatures.
Accept ownership-preserving replace/remove, consuming `Mapˉfreeze`, and the
immutable map length/contains/rank/key/value observations. Workload 9 directly
uses freeze and immutable ranks; source freeze cannot leave the other promised
map outcomes unnamed.

## Finding 2: deterministic sets need a complete callable surface

Decision 0752 accepted sets semantically but no exact function signatures.
Accept construction, optional first item, insert, length, contains, rank,
borrow-at, remove, consuming freeze, and immutable observations. Duplicate or
rejected insertion returns its original owned value. No set literal, compiler
primitive, host hash set, or randomized iteration is added.

## Finding 3: collection identity equality is the resolved total order

Accept the existing rule that uniqueness is exactly `Ordering.Compare == Equal`
and require law/equality-conflict tests. One nominal package-identity order is
resolved from argument types; result context and import order cannot select it.
The workload's ordinal implementation is explicit source, not a built-in locale
or ambient text collation.

## Finding 4: existing text ranges and numeric parsing are sufficient

The bounded cursor uses rune observation and shared ranges after an initial
UTF-8 byte-limit check. The whole-u64 parser supplies exact numeric behavior.
Do not add regex syntax, split allocation, parser combinators, preprocessing,
automatic schemas, or reflection. A later reusable scanner is a library choice
only after multiple real parsers show a common bounded contract.

## Finding 5: duplicate package content is one unobservable content object

Accept four independently validated typed declaration references to three
content objects. Within one resource domain, one distinct admitted content
identity has one payload charge; aliases add bounded reference metadata only.
Separate domains keep separate accounting and authority. Source can compare
values but cannot observe interning, mapping, address, or reference identity.

## Finding 6: canonical topology and cycle evidence derive from ordered ranks

Accept repeated ascending-rank scans as the simple deterministic oracle. It
publishes dependency-first order with lexical ties and stops after a bounded
no-progress pass. Cycle evidence contains the remaining identities in canonical
order. Faster graph libraries are permitted only with exact differential output
and published work/memory bounds; no unordered queue or recursive diagnostic is
implicit.

## Quantitative record

| Measure | Recorded value |
| --- | --- |
| Source | 7 files; 1,478 lines / 48,368 UTF-8 bytes; 60 top-level declarations; largest 631 lines. |
| Package data | 4 declarations / 3 content objects / 227 unique bytes / 280 logical declaration bytes. |
| Collections | 1 four-item immutable map; 5 dependency/completed sets; 1 four-item order sequence. |
| Execution | 4 packages / 4 edges / 4 topology selections / no recursion. |
| Report | 160 UTF-8 bytes / 7 LF-terminated lines / one SHA-256. |
| Failure surface | 51 named package/parser/collection/graph/literal/generic cases. |
| New general surface | exact map completion/publication and set APIs; no grammar, capability, unsafe, WIR, or serialization syntax. |

## Owner resolution

The owner accepted all six recommendations. Workload 9 is draft reviewed. The
Foundation and package-data candidates carry the corresponding rules. No
current compiler/runtime/package implementation, performance result, or
source-freeze claim follows.
