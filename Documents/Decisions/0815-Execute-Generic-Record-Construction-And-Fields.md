# Decision 0815: Execute generic record construction and fields

- Status: Accepted with current-Windows development evidence; independent Linux qualification pending
- Date: 2026-08-21
- Advances: [Language 1.0](../../Specifications/Windvale-Language-1.0.md), [generic type evidence](../../Specifications/Compiler-Source-Generic-Types.md), [source WIR](../../Specifications/Compiler-Source-Wir.md), [source WVB](../../Specifications/Compiler-Source-Wvb.md), and [Decision 0814](0814-Connect-Generic-Nominal-Materialization-To-Main-Wvb.md)

## Context

Main analysis and emission could already retain `Box<Point>` as one bounded
WVGT identity, materialize it as an ordinary WVB record, and remove its source
template. The executable fixture did not construct that type in a function body:
it returned an unrelated constant, so successful execution proved metadata and
target translation only.

Language 1.0 needs generic records to behave like ordinary monomorphic records
after exact substitution. It also needs construction syntax that remains
distinct from explicit generic-function calls and relational expressions. The
implementation must reuse the existing WIR, WVB, verifier, runtime, and native
record machinery rather than introduce runtime generics or a parallel compiler.

Implementation exposed one omission in the frozen source identity: the prose
accepted generic records, but `Nominalˉconstruction` admitted only a qualified
name and therefore provided no grammar for applying the record's required type
arguments. Decision 0767 requires a named reconsideration and a new exact
manifest identity rather than an undocumented implementation spelling.

## Decision

1. Accept the exact 3,822-byte
   `Windvale-Language-1.0-Source-Amendment-0815-Candidate.txt` manifest at
   SHA-256
   `57cd5ccb710ca504b55644194cfa20a576bc0fd8ebd33247ef232c30d0d84162`.
   It preserves the original 3,702-byte freeze manifest as immutable provenance,
   replaces only the main specification and its human/machine grammar identities
   for this semantic amendment, and binds the effective 251-input candidate at
   SHA-256
   `16cd7aeddb876d58f63c2ebf14016e74f19c2e3b2ff25e36c09e671837faaec7`.
2. An applied generic record construction uses
   `Qualifiedˉname<Typeˉarguments> { Field: Value, ... }`. The required following
   brace disambiguates the type application. `::` remains exclusive to explicit
   generic-function calls, so `Box::<Point> { ... }` is invalid.
3. The body parser retains the complete applied type as one construction target.
   Typed analysis resolves it through the existing full-arity generic binding
   and immutable WVGT catalog. A bare template is not a value type.
4. Reconstruct and independently validate the concrete record layout before
   accepting fields. Preserve ordinary left-to-right value evaluation and
   declaration-order operands. Reject unknown, duplicate, missing, or
   mismatched fields without publishing partial analysis artifacts.
5. Reuse `Recordˉcreate = 17`. For a generic record, its result and target are
   the catalog-bounded private shape and its auxiliary value is the template's
   WVSD declaration identity. Reuse `Recordˉfield = 18` for reads; its target is
   the private receiver shape and its result is the substituted field shape.
6. Independent WIR validation reconstructs the same layout and proves the
   instance, record kind, declaration, arity, field identity, operands, and
   result. The existing 256-instance, 32-depth, 64-field, retained-evidence, and
   emitted-size bounds are unchanged.
7. Source WVB maps private operation targets through the same materialization
   plan that emits `__WvY` Types entries. Published bytecode contains ordinary
   record operations and ordinary Types indices only. The verifier, interpreter,
   runtime, JIT, and AOT paths gain no runtime-generic branch.
8. Continue evolving the active Windvale-written Seed/compiler sources as the
   Language 1.0 implementation. When that compiler implements the accepted 1.0
   contract and reproducibly builds itself, it is the Windvale Compiler 1.0;
   no parallel “new Seed” compiler is created. The immutable Stage 0 release
   remains separate recovery provenance.

## Evidence

The active split route builds a 1,032,689-byte analyzer WVB with 505 functions
and 844,526 code bytes at SHA-256
`41b3a9c71dde657168929a1ba860e98c0b5fa27408d0f71add741d6fb49b94e5`.
Its eight-fragment Windows application is 32,928,256 bytes at SHA-256
`0e706c345152b10e24d436e82e23a1950a9a0897988e4bd434fa24eecc1e9a2b`.
The resulting 972,044-byte emitter WVB has 531 functions and 805,158 code
bytes at SHA-256
`8e1a28be1cd492f42ca77df720f67d4b699407b3ad6482ebba4773a999d78140`.

`Generic-Nominal-Main-Pipeline.wv` constructs
`Box<Point> { Value: Point { X: 42 } }`, passes it through
`Identity(Value: Box<Point>) -> Box<Point>`, and returns
`Wrapped.Value.X`. Analysis publishes exact 377-byte WVSS, 104-byte WVCA,
244-byte WVLB 1.3, and 640-byte WVIR 1.3 products. WIR contains the ordinary
`Point` construction, private `Box<Point>` construction, exact call, private
`Box.Value` read, and ordinary `Point.X` read. Emission publishes a 441-byte
WVB 1.11 at SHA-256
`71c8e08b2a736ebbc2042f4188c8ed813091dfd72ced93226f5467bd507e73ed`.
Its Types section contains `Point` and `__WvY0000`; the independent verifier
accepts it and execution returns `42`.

Four focused negative fixtures return exact `Missingˉrecordˉfield`,
`Duplicateˉrecordˉfield`, `Typeˉmismatch`, and `Unknownˉfield` WIR statuses,
write one exact diagnostic line to standard error with no standard output, and
publish none of WVSS, WVCA, WVLB, or WVIR. The
generic-WIR compiler fixture remains deterministic at 1,187,360 bytes and
SHA-256
`f4ac7f82d79072bdc83c450d8ae4f9cab89550cf39efde2f7e96b56686b9eccd`.
That compiler-scale WVB exhausts the older fixed native front-door verifier's
execution budget and produces no diagnostic there. The Language owner therefore
retains its exact size/hash and byte-determinism checks, then verifies it once
with the current source-built native verifier when that verifier becomes
available in phase 5. The current verifier accepts it as compiler-aligned; this
avoids repeating a known undersized interpreter path or widening the historical
front-door artifact.

These are current-Windows development results, not paired-host conformance or
release qualification.

The coordinated `language-1-front-door` owner passes all 186 registered cases
in 660,130 milliseconds (660,650 milliseconds including coordinator overhead).
It covers the 251-input amended freeze identity, 72 frozen source fixtures, the
26-case generic nominal pipeline, the current-native generic-WIR verification,
and every subsequent scalar, control-value, variant, typed-failure, and
Foundation-generic phase. No storage, OS, paired-host, or Qualification owner was
run for this focused language/compiler checkpoint.

## Consequences

General generic record values now cross ordinary function, local, construction,
and field-read boundaries while using one monomorphic runtime representation.
The source language gains the amended construction spelling without weakening
the explicit generic-call syntax. Invalid field sets fail at typed analysis and
leave no partial artifact family.

Generic variants, template-dependent operations inside generic-function bodies,
the remaining Foundation special planning, and paired-host qualification remain
later work. Adding the canonical generic layout owner increases compiler/test
closures; future capacity work should use reachable-code reduction or cohesive
module extraction before widening native limits.

## Reconsideration triggers

Revisit this decision if Language 1.0 admits inferred nominal construction type
arguments, templates acquire runtime identity, WVB gains reified generics, the
private-shape range changes, or representative compiler closures cannot remain
within their explicit type, code, and instruction bounds after measured
refactoring.
