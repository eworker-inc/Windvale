# Workload 9 implementation responsibilities

## Ownership matrix

| Contract | Primary owner | Required implementation evidence | Not a language feature |
| --- | --- | --- | --- |
| Literal families | lexer/parser/constant evaluator/editor | exact delimiters, escapes, LF rules, byte/scalar limits, diagnostics | runtime string parser |
| Package data | source graph/package builder/loader | typed reference binding, early maximum/digest/UTF-8 rejection | filesystem or dynamic import |
| Content dedup | canonical package format/publisher/loader/accounting | four references, three objects, one per-domain notice payload charge | observable interning/pointer identity |
| Text cursor/ranges | Foundation text/borrow checker | byte/rune distinction, checked shared ranges, no owner escape | host substring/layout |
| Whole numeric parser | Foundation numeric | strict ASCII, whole input, limit/overflow failures | locale conversion |
| Map completion | Foundation collections/runtime | replace/remove/freeze plus immutable canonical observation | host map exposure |
| Set completion | Foundation collections/runtime | construct/insert/remove/freeze/immutable rank and ownership outcomes | compiler primitive or hash set |
| Ordering resolution | type/protocol checker | one argument-derived exact solution, law fixtures, finite comparison work | overload search/collation |
| Bounded topology | ordinary Core library/source | finite scans, lexical tie, sorted cycle diagnostic | recursion or scheduler intrinsic |
| Canonical report | Foundation text + application source | exact 160 bytes across maps/hosts/targets | reflection serialization |
| Verification | focused Language 1.0/package owners | valid, permutation, malformed, allocation, law, bytes/hash cases | paper-only pass claim |

## Likely WIR/backend work

No package-parser-specific WIR operation is justified. Map/set/tree operations,
shared immutable text ranges, builder appends, loops, comparisons, records,
variants, and generics should lower through ordinary typed calls and control
flow. Package data likely requires a versioned typed content-reference table in
WVB or its package envelope; the content payload must not be copied into every
module declaration.

The compiler may specialize the exact map/set instances but cannot select an
implementation from result context, import order, or a host collection. An
optimized collection remains differential-tested against a simple ordered
reference implementation with comparison counting.

## Verification slices after source freeze

1. all literal grammar/editor valid and malformed cases;
2. package-data source binding and strict text validation;
3. canonical content table/reference/dedup/accounting cases;
4. map/set construction, ownership, mutation, freeze, rank, and law cases;
5. bounded text/numeric parser cases and preallocation rejection order;
6. graph unknown-edge/cycle/permutation cases;
7. report exact bytes/hash across reference collection representations;
8. compiler generic/protocol ceilings and deterministic specialization cache;
9. cross-host WVB/package/report identity comparison.

One passing broader gate subsumes narrower checks on an unchanged tree.
Qualification records host, target, tool/package identities, elapsed time, peak
memory, comparison count, retained bytes, content-object/reference sizes, report
bytes, and hash.

## Performance record

Implementation must measure rather than assume:

- parse and report throughput at reference and hard maxima;
- map/set comparisons and worst-case insertion/removal time;
- peak/transient/retained collection and text bytes;
- distinct content-object versus declaration-reference shipment bytes;
- compiler specialization count, phase time, and retained evidence; and
- WIR/WVB/native code and data size.

Optimization may use a balanced tree, sorted compact representation, or shared
content mapping only while exact ownership, bounds, diagnostics, iteration,
bytes, accounting, and the simple reference oracle remain intact.
