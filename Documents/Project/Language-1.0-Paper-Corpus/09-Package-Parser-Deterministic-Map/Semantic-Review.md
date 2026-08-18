# Workload 9 semantic review

## Profile and authority

All seven modules are Core. Package data is supplied by the admitted package
plan before execution and grants no path, filesystem access, loader handle, or
dynamic-import authority. The only effects are bounded memory allocation and
rights-reduced budget acquisition/release.

## Literal and parser review

The source exercises ordinary text/byte, multiline text, raw text, and raw byte
literals. Literal kind is explicit and never selected from contents. Package
inputs remain external exact bytes; the multiline report is a source oracle,
not an implicit parser schema.

Line and word cursors count Unicode scalar indices while each resource limit is
checked in canonical UTF-8 bytes. `Shareˉrange` creates immutable shared text
ranges; no substring builder or native pointer is needed. Identity validation
accepts only the declared ASCII grammar. Numeric parsing consumes the entire
word under the exact u64 parser.

The first input byte-limit check precedes map/set construction. Each following
item/count check precedes insertion. A declared length field in a future format
must be checked with the same order: parse bounded digits, compare with maximum,
checked-add enclosing geometry, then allocate/read. The workload records the
malicious-length rejection even though its two line formats carry no length
prefix.

## Generic and protocol resolution

There is one nominal `Ordering<Packageˉidentity>` implementation. Generic
`Map`, `Set`, and their immutable observations resolve from explicit key/value
arguments or full type arguments. Result context, import order, map layout, and
host collection choice do not participate.

The order compares scalar values lexicographically and then length. It is
deterministic, locale-free, compatible with exact text equality, and bounded by
the 64-byte identity maximum. A protocol law test rejects an implementation
whose comparison says Equal for two observably different identities; the
collection cannot silently choose one.

## Ownership and publication

Map, sets, vectors, builders, and budgets are move-owned while mutable. Every
successful insertion accepts its owned value once; duplicate/capacity/allocation
rejection returns the original owner. `Mapˉfreeze`, `Setˉfreeze`, vector freeze,
and text freeze consume their mutable owners and publish shared immutable values
without allocation or failure.

Rank-derived borrows never coexist with mutation. The graph uses only immutable
map/set ranks. Topology output constructs new identity records that share
immutable text; it does not move keys out of the frozen map.

## Determinism

The lock deliberately lists `util`, `app`, `core`, `codec` and lists app
dependencies as `util,codec`. Map/set iteration publishes `app`, `codec`,
`core`, `util` and `codec,util`. Topology repeatedly scans that canonical map,
so the dependency-first order is `core,codec,util,app`.

No randomized hash seed, insertion order, balancing layout, address, filesystem
enumeration, reflection field order, scheduler, or host locale can influence
the result. A different conforming ordered-tree representation must emit the
same 160 bytes.

## Cycle and diagnostic bounds

The algorithm is iterative. At most 64 successful selections plus one
no-progress cycle scan each visit at most 64 packages and at most 32
dependencies per candidate in the hard profile. A no-progress scan stops and
copies at most 64 remaining identities in canonical order into one bounded
diagnostic sequence. There is no recursive DFS depth, unbounded path retention,
or repeated diagnostic growth.

## Package-data deduplication

Both notice declarations have exact independent typed bindings but the same
content identity. The package publisher stores one payload and two references.
The loader validates each reference, maps one immutable content object into the
resource domain, and publishes two equal values. Source cannot observe whether
the implementation maps, copies, interns, or shares that content.

This is content deduplication, not authority sharing. Separate domains retain
separate accounting, revocation, and teardown. Digest equality alone is not a
grant, and a declaration with the wrong type/maximum/identity still fails even
when matching bytes exist elsewhere.

## Failure atomicity

| Boundary | Failure | Observable state |
| --- | --- | --- |
| package binding | missing/duplicate/type/length/digest/UTF-8 | application never starts |
| input maximum | observed and admitted bytes | no parser collection allocated |
| identity/version | exact line/column or numeric reason | no item inserted |
| set/map insert | duplicate/capacity/comparison/allocation | collection unchanged; owner returned |
| freeze | none | mutable owner consumed once |
| reference validation | root/version/dependency mismatch | no order/report publication |
| topology | unknown edge or sorted remaining cycle set | no success order/report |
| report append | exact output limit | failing append changes no bytes; no text published |

## Acceptance matrix

| Pressure | Evidence | Standing |
| --- | --- | :---: |
| raw/multiline/byte/text literals | all four literal families in the report module | Pass |
| numeric/text parsing | scalar cursor/ranges plus whole-u64 parsing | Pass |
| map/set/sequence/builders | mutable construction, consuming freeze, immutable ranks | Pass with accepted Foundation completion |
| duplicate-content package data | four references, three objects, 227 unique bytes | Pass |
| explicit versions/no reflection | three exact magic/version grammars and handwritten serializer | Pass |
| generic/protocol determinism | one nominal Ordering solution, explicit generic construction | Pass |
| bounded cycles | iterative no-progress detector and 64-item sorted diagnostic | Pass |
