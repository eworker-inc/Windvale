# Windvale Language 1.0 migration evidence

## Status

Decision 0767 freezes the source design. This page records implementation and
measurement evidence outside that immutable identity. It must not be read as a
claim that the complete Language 1.0 compiler, Foundation, runtime, editor, or
any natural-language pack is implemented.

Migration Slices 1 through 4 are complete and Slice 5 is active. The existing
compiler admits an edition-1 source descriptor only through an explicitly
supplied, hash-pinned source-input lock and composite source profile. It
resolves the frozen `en@1` component chain, exposes the remaining bytes as an
immutable view, parses the required standalone module metadata, and compiles
one minimal Core program deterministically through WIR and WVB. Slice 2 adds
exact front-end identities for the frozen primitive value types and prevents
Seed-only `void` type syntax from crossing the edition-1 front door. Named
record update, fixed-width
`i8`/`i16`/`u16`, exact Unicode-scalar `rune`, `f32`/`f64`, ordinary one-value
`unit`, and return-only `never` now cross the compiler, verifier, and scalar
runtime together. Named zero-through-64-field variant construction and
destructuring now cross the compiler, compiler-aligned verifier, and
source-built native scalar runner through WVB 1.16. Contextual immutable fixed
arrays now cross the same compiler, independent verifier, and scalar runner
through WVB 1.17. Project 3 carries the
profile artifacts; Project 2 and
descriptorless Seed retain their prior behavior.

The compiler now also recognizes the frozen `borrow T` and `borrow mut T`
signature and expression surface. One call-scoped checkpoint proves explicit
immutable and mutable arguments, conservative Copy/shared read-through, and
direct mutable origins. It rejects omitted modes, immutable-to-mutable calls,
mutable borrowing from `let`, standalone borrow storage, and borrowed results
until provenance is represented. This is prerequisite evidence, not a claim
that Slice 5 ownership, moves, lifetime overlap, resources, or cleanup is
complete.

The Edition 1 front end now also recognizes bounded exact `effects(...)`
clauses and retains their source span and identity count. This is the syntax
prerequisite for allocation work, not yet canonical effect resolution,
inference, call checking, or serialized compiler evidence.

Value-producing `if` and exhaustive enum/variant `match` now cross the reference
compiler and scalar runtime, completing Slice 2's planned value-and-control
compiler surface. Slice 3 publishes the exact edition-1
`Foundationˉoption.Option<T>` and `Foundationˉresult.Result<T, E>` variant
identities, admits their full-arity type uses, and emits every used concrete
specialization as an ordinary canonical WVB variant. Value-producing `try`
extracts `Valid.Value` and permits `Result<T, E>` to propagate into
`Result<U, E>` by reconstructing only the failure case when `T` and `U` differ.
One fixture migrates a manual `Valid`/`Value`/`Error` record through an explicit
domain-error adapter. Generic declaration and explicit-call syntax now has a
bounded parser checkpoint. Canonical WVGS solution evidence and WVGC
specialization catalogs establish deterministic inference conflicts, reuse,
diagnostics, and growth limits before code generation. The direct-function
checkpoints now connect that identity to source symbols, concrete bindings,
monomorphic WIR, and ordinary WVB execution. They support inferred and
full-arity explicit calls, equal-instance reuse, and multiple bounded concrete
instances per declaration, including a type appearing only as the complete
result type. WVGT 1.0 now supplies the bounded concrete identity, exact nested
dependency order, and private compiler shapes required by general generic
records and variants. A focused recursive source binder now resolves complete
type lists and exact fixed-width literal constants into transactional WVGT
replacements. Field substitution and the deterministic materialization plan are
implemented. Main analysis now scans ordinary function signatures and explicit
locals, publishes non-empty WVGT evidence through WVLB 1.3, and retains its
private shapes in paired WVIR evidence without changing ordinary WVLB 1.1 or
function-only WVLB 1.2 semantics. The frozen broader
constant-expression contract, generic-function-context type uses, WVB Types and
operation emission, and package-visible template publication remain Slice 4
work.
Collections beyond the first structural bounded signature and the
repository-wide Seed-to-edition-1 source migration also remain Slice 4 or later
work. Final paired-host and broad integration evidence remains
deferred to the seven-slice integration gate. Localized token execution,
public-library vocabulary lookup, Unicode identifier admission, and paired-host
Language 1.0 qualification also remain pending.

## Frozen source identity

The verifier binds the exact replacement source frozen by
[Decision 0767](../Decisions/0767-Freeze-Windvale-Language-1.0-Source.md):

| Evidence | Exact value |
| --- | ---: |
| Freeze manifest bytes | 3,702 |
| Freeze manifest SHA-256 | `c9517841eae6b6e86778cb1dd88711feb38929dec8fe79e084eec44fa22c512a` |
| Frozen inputs | 250 |
| Frozen input bytes | 1,724,854 |
| Frozen aggregate SHA-256 | `fb918a763ae7c8c85dd1a2ffecee6587ab93bbf846ae31ae19b53509aed36a0a` |

The immutable Decision 0767 semantic identity above does not change. The
current migration-verifier closure additionally contains the implementation
catalog for the already frozen `Foundationˉresult` public identity and the
updated source-input lock. That closure contains 251 files and 1,726,783 bytes;
its entry stream is 46,260 bytes with SHA-256
`de39b8f4042c98d34ff3676ec111a7ffca6e91c529f0e40f2250a824c54ad415`.

`Tests/Native/Language-1.0-Fixture-Inventory.txt` further fixes 16 workload
bundles containing 72 `.wv` source fixtures and 482,325 source bytes. The
inventory records the exact source count, byte count, aggregate identity,
planned migration slices, and current standing of every bundle. The verifier
recomputes every identity and validates every fixture's UTF-8 encoding and
bounded ASCII descriptor.

These are executable identity and descriptor checks, not yet executable claims
for the fixtures' remaining Language 1.0 syntax or semantics. Their inventory
standing therefore remains explicitly `identity-only`.

## First compiler path

`Compiler/Windvale/Source-Descriptor-Core.wv` reads only the first physical
line and implements the frozen universal descriptor boundary:

- exact byte-zero `#!wv/1 ` admission;
- a 2-through-96-byte ASCII profile identity with the frozen component/atom
  grammar;
- a positive decimal `u32` profile version without a sign, suffix, separator,
  leading zero, or overflow;
- LF and CRLF support with no BOM or preceding bytes;
- a 128-byte maximum excluding the line ending; and
- structured status and byte offsets without allocation or an unbounded scan.

The 33-case self-test covers accepted English and Simplified-Chinese descriptor
shapes, the maximum profile version, missing/unsupported editions, malformed
profiles and versions, BOM/non-ASCII input, line-ending failures, and length
bounds. Profile selection is deliberately absent from this syntax-only reader.
It returns `42`; two builds compare byte-identically before execution. The
current deterministic test WVB is 12,633 bytes with SHA-256
`53de13cfb20e237e71d5e34e6010f193eccbe815cc58a214b8c5ee2acf76bcc2`.

`Compiler/Windvale/Source-Set-Core.wv` performs edition dispatch once per
source-set view. A descriptorless external WVSS 1 source remains Seed edition 0.
An edition-1 source must first pass the profile-aware compiler entry point; the
ordinary entry points reject it instead of obtaining an ambient binding. For an
admitted edition-1 module, the private source-set view starts at the descriptor's
line ending. `Bytesˉslice` retains the original immutable backing, avoids a
whole-source copy, and lets the existing lexer preserve the module header's
physical line 2. The view records the raw offset/length, edition, binding,
origin, and front-door failure offset.

The declaration parser accepts `profile core|hosted|system;` only in the
standalone edition-1 metadata position. `core` lowers to the current portable
WVB profile for this implemented subset. Source-set validation rejects that
standalone form in descriptorless Seed and rejects an edition-1 file that omits
it. The WVB metadata writer consumes the same header without creating a second
compiler path.

`Compiler/Windvale/Source-Profile-Core.wv` now owns the bounded artifact admission
boundary. The compiler receives exact `.wvlock` and `.wvsp` byte values plus the
expected lock digest; it neither discovers a file nor searches or downloads a
profile. It hashes the lock before parsing, selects the exact descriptor
identity/version, hashes the supplied profile against that locked row, checks its
identity/version/edition and fixed component chain, and publishes one resolved
binding only after all checks succeed. The implemented English profile digest is
`e678b1b5daae2c0d87179f2fcd162b1b002cebe8617fc0fb155a5b78a1bdaf27`
under lock digest
`9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e`.

The compiler then creates a private WVSS 2 view carrying the resolved edition,
binding, and descriptor-origin length for each module. Downstream graph, symbol,
WIR, and WVB phases neither reparse descriptors nor rehash profile artifacts.
WVSS 2 is not an external compiler input; public ordinary compilation continues
to require WVSS 1.

`Tests/Fixtures/Language-1.0/Minimum-Program.wv` is the first executable
edition-1 fixture. A source-built native compiler emits the same 221-byte WVB
twice, SHA-256
`25a18cf13d791db1e85fd6b237f89f21d4a0c7b9460b0a72db2da5e5deb205ae`;
the compiler-aligned metadata verifier accepts it and the ordinary runtime returns
`42`. An unsupported source profile, a missing edition-1 profile declaration, a
descriptorless edition-1 header, an absent ambient profile, a wrong lock digest,
and changed profile bytes all fail without publishing an output.

## Slice 2 primitive, unit, and never checkpoints

The shared lexer now assigns stable appended token identities to `unit`, `never`,
`i8`, `i16`, `u16`, `f32`, `f64`, `rune`, and the record-update word `base`.
The declaration parser and symbol/binding layers recognize the eight primitive
type identities without renumbering any Seed token, declaration type, or internal
shape. The new internal primitive shapes are deliberately not WVB type bytes:
backend admission remains closed until each value representation and operation is
specified, verified, and implemented across compiler, verifier, runtime, and
native lowering.

The source-set edition preflight rejects Seed-only `void` before ordinary semantic
analysis and retains its exact module-relative offset, line, and column. The
corresponding real Project 3 fixture is compiled through a rebuilt hosted compiler
and must fail without publishing a WVB. Descriptorless Seed continues to accept
`void` and rejects the new edition-1 primitive identities at the same boundary.
This preflight is an intermediate canonical-token guard; final profile-aware token
classification and localized spellings remain Slice 1 follow-through and must not
be inferred from these English-token tests.

The focused value-front-end self-test contains 39 assertions covering all appended
keyword and primitive-type identities, exact packed seven/eight-byte keyword and
near-miss classification, both edition directions, exact first invalid-token
offsets, and stable value-`if` and value-`match` expression-kind identities. It
compiles to a verified WVB and returns `42`; real parser behavior is covered by
the source-built compiler fixtures below rather than recursively interpreting
the parser inside the bounded scalar runner.

The body parser appends expression kind `Unit = 12` and recognizes only an empty
parenthesized expression as `()`. A nonempty parenthesized expression retains its
existing transparent grouping behavior. Binding a unit literal contributes no
name or call evidence; typed WIR gives it ordinary shape `9` and emits
`Unitˉconstant = 163`. Explicit `return;`, `return ();`, return of a unit-valued
call, and implicit fallthrough all produce and return that same one logical
value. Parameters, locals, assignment, and record fields preserve the unit type;
the runtime may represent its unobservable physical payload as a canonical zero
scalar cell. A non-unit expression returned from `unit`, `()` returned from
`i32`, or descriptorless Seed use of `()` is rejected without output. Seed
`void` retains shape zero and its no-value return behavior.

Internal shape `10` is the logical `never` result and is never a temporary,
parameter, local, record field, or serialized stack value. A call to a
never-returning function emits the ordinary physical call with no result,
terminates its WIR block with a verified self-loop, and propagates logical shape
`10` through its enclosing expression. It therefore satisfies an expected result
position without a conversion and makes a following statement unreachable.
Fallthrough or a value-return statement in a `never` function is rejected. The
compiler recognizes `while true` as non-fallthrough unless an admitted break path
reaches the loop's after-block; encoded WIR still retains canonical explicit
edges for independent validation.

WVB 1.15 adds type tags `20`/`21` for unit/never and opcode `C3` for a unit
constant. The compiler-aligned verifier restricts never to function results,
rejects any return instruction in a never function, and treats a call to never
as pushing no value. The scalar runtime executes unit through its ordinary
stack, local, record, call, and return path and applies the same never call
effect. `Tests/Fixtures/Language-1.0/Unit-Control.wv` compiles twice to the same
731-byte WVB with SHA-256
`f047706f0b4915e59120b54eef5746efe22eae9c2c658860082fe131fa85ad3c`.
`Never-Control.wv` compiles twice to the same 853-byte WVB with SHA-256
`955be78835ecec4bcd4be3b563932d5a933422c6ce1cbdd74ee928d4f9bf9a04`.
Both execute through the source-built runner and return `42`. Four source
fixtures reject never fallthrough, a return from never, a never parameter, and
a statement after a never call. The positive fixture also proves a never-valued
loop condition and a never right operand behind a finite Boolean short path.
Eleven independent WVB mutations cover version,
shape, opcode, type mismatch, forbidden never positions, and return behavior.

The next checkpoint appends body expression kind `Recordˉupdate = 13` for the
frozen `Qualifiedˉsourceˉname base Expression { Field: Value, ... }` form. Binding
visits the base once, visits replacements left to right, then resolves the target.
Typed WIR requires the base to have the target's exact nominal record shape,
rejects duplicate or unknown replacements, preserves every unreplaced field, and
uses only the existing record-field and record-create operations. A base expression
that contains its own top-level brace construction is parenthesized at this parser
checkpoint to expose the replacement-list boundary explicitly.

`Tests/Fixtures/Language-1.0/Record-Update.wv` compiles twice to the same
1,116-byte WVB, executes through the scalar runner, proves replacement plus field
preservation, and returns `42`. Separate cases reject a wrong-nominal base,
duplicate replacement, unknown replacement, and descriptorless Seed use without
publishing output. No WVIR operation, WVB opcode, value representation, or
serialized-format version changes.

## Slice 2 value-producing `if`

The body parser appends expression kind `If = 16` without renumbering retained
kinds. An edition-1 value `if` requires one Boolean condition, a braced then arm,
and an `else` arm that is either another value `if` or a braced value block.
Each value block contains zero or more ordinary statements and exactly one final
expression without a semicolon. The final expression is not reinterpreted as an
expression statement, and branch-local bindings end at the arm's closing brace.

Typed WIR evaluates the condition once, reaches exactly one arm, requires both
reachable arm values to have the same exact shape, and joins them through
`Valueˉphi = 64`. That operation is the generalized identity of the retained
Boolean short-circuit phi: validation now requires two same-shape operands and
the same non-void/non-never result shape, two distinct unconditional
predecessors, and no conditional or third predecessor. A `never` arm contributes
no value; the reachable arm continues without a conversion. The WVB backend
materializes the selected value in the phi-result local on the predecessor edge,
so no opcode, module representation, or WVB minor-version change is required.

`Tests/Fixtures/Language-1.0/Value-Control.wv` covers a Boolean local, two
branch-local declarations, mutation, a final name expression, and recursive
`else if`. It compiles twice to the same 529-byte WVB with SHA-256
`c9b5cecdfb26478844dc8c6e6e97683758693d419fab36b360705eb99ff5d0e8`
and executes through the source-built scalar runner with result `42`.
`Value-If-Lazy.wv` places an unbounded recursive call in the unselected arm,
compiles to 350 bytes with SHA-256
`d18209374d076eea7ff9eb3bde6b2a71e7c01999cb91a01e3d154818e18aa386`,
and also returns `42`, proving that the skipped arm has no runtime execution
path. Four source fixtures reject a missing `else`, a semicolon after an arm's
purported final value, an exact arm-type mismatch, and a non-Boolean condition
without publishing output. A fifth fixture proves that descriptorless Seed
rejects the value form while retaining its existing statement `if`.

The focused Windows `language-1-front-door` owner passed all 128 cases in
365.93 seconds. It rebuilt the compiler once, reused that image for the
value-control cases, and completed all retained fixed-integer, rune, floating,
unit/never, and named-variant phases. Heavy storage, broad OS, paired-host, and
complete Qualification gates did not run; the final Slice 2 integration gate
owns that broader evidence.

## Slice 2 value-producing `match`

The body parser appends expression kind `Match = 17` and reuses the same bounded
match/case view for statement and value forms. The ordinary expression parser
owns the selector boundary, including a brace-form nominal construction, rather
than a second delimiter scanner. Edition-1 arms are value blocks; descriptorless
Seed retains statement match and rejects the value form.

Typed WIR evaluates the selector once, requires an exact nominal enum or variant,
retains complete duplicate-free exhaustiveness, and binds named variant fields
only inside their selected arm. Reachable results must have the same exact shape
and join pairwise through the existing `Valueˉphi = 64`. Only the selected arm
has a runtime path, and a `never` arm contributes no value. No WVIR operation,
WVB opcode, module version, verifier rule, or scalar representation changes.

`Value-Match.wv` uses a call selector and three enum arms, selects the middle
arm, exercises two pairwise value joins, and compiles twice to the same
588-byte WVB with SHA-256
`320ae22a8f38aea54884cacb7c07be841bf6500f2527aadf58e2f24083c86226`.
It returns `42`. `Value-Match-Lazy.wv` places an unbounded recursive call in
the unselected arm; its deterministic 431-byte WVB has SHA-256
`358a533f491d7901c21ca66f653db33c1654b1730b4d2d4bf8e66cc6fd263a74`
and also returns `42`. `Value-Match-Never.wv` admits a `never`-typed arm without
inventing a value; its deterministic 422-byte WVB has SHA-256
`cfc597be4a1dc57ef6d52bb3ff61962680508d5f62b18c8057c0d231ffd1db73`
and returns `42` through the source-built WVB 1.15 scalar runner.
`Value-Match-Variant.wv` selects a brace-form two-field
variant construction, destructures fields by name in a different order, and
compiles twice to the same 634-byte WVB 1.16 with SHA-256
`db8b1cab5c672dfccc337fb7874a9b07f9b5e2fb6b4243cd8c1f0a35e70af2f6`;
the source-built WVB 1.16 scalar runner returns `42`. Separate cases reject a
missing case, a trailing semicolon, an exact arm-type mismatch, and
descriptorless Seed use without publishing WVB output.

The compacted 498-function compiler contains 961,629 code bytes and 1,161,873
module bytes. Native planning reports 33,408,489 machine-code bytes plus 2,440
relocation bytes. The unchanged segmented stage accepts a 33,438,278-byte
object package across 42 chunks with a 528-byte manifest; no construction or
chunk limit was widened. The focused Windows language owner passed all 136 cases
in 371.69 seconds, including every retained numeric, unit/never, and WVB 1.16
variant phase. Heavy storage, broad OS, paired-host, and complete Qualification
gates remain deferred to the final seven-slice integration gate.

## Slice 3 typed failure completion

The declaration parser accepts generic parameter lists on variants, but source
symbols admit specialization only for the two exact Foundation modules and
declarations. `Option` requires `<T>` and the exact ordered `Present(Value: T)`
and `Absent` cases. `Result` requires `<T, E>` and the exact ordered
`Valid(Value: T)` and `Failure(Error: E)` cases. Type uses require exact arity
and may currently substitute an implemented primitive or one of the first 1,024
ordinary record, enum, or variant identities. Bare use, extra or missing type
arguments, unsupported nested shapes, and structural lookalikes are rejected.

The compiler carries each specialization as a compact typed shape through WVLB
and WVIR. WVB emission collects at most 256 distinct used specializations,
orders them by exact shape, and emits them as ordinary private variant types.
The canonical Foundation template declarations remain ordinary nominal types;
specialized private names are deterministic implementation identities and do
not replace the public source name. No WVB opcode, execution rule, or minor
version was added.

`try` evaluates its operand once, requires exact Foundation `Result` identities
on both sides, and compares the expanded error shapes exactly. The success edge
extracts `Valid.Value`. If the result shapes are identical, the failure edge
returns the original value. If only the success shapes differ, it extracts the
same `E` and constructs `Result<U, E>.Failure`. Statement `try` uses the same
branch and discards the success value. Different error domains require an
explicit source adapter and never receive inferred conversion or overload
selection.

`Foundation-Generic-Result.wv` exercises Option presence/absence, Result
construction and matching, same-error/different-success `try`, statement `try`,
a manual status-record adapter, an explicit domain-error adapter, and 16
concrete specializations that cross the rank-9/rank-10 private-name boundary.
Analysis publishes 3,169 source bytes, a 104-byte WVCA manifest, 900 binding
bytes, and 5,792 WIR bytes. Two emissions produce the same 3,383-byte WVB with
SHA-256
`64da5d52c01301c54f9391c9f8cdc3f7a8000e7c21694b06baa096354ba1d09f`.
The current compiler-aligned verifier accepts it and the source-built scalar
runner returns `42`. Four focused malformed cases reject wrong arity, an extra
argument, bare Result use, and mismatched `try` error types without publishing
analysis evidence or WVB. The Windows and Linux front doors encode this as an
eleventh visible phase. The complete 146-case Windows owner passes; the Linux
execution and paired-host claim remain final-integration work.

The current source-profile admission product contains 40 functions in an
82,781-byte WVB and packages as a 797,184-byte Windows x64 executable in one
fragment. The analyzer contains 403 reachable functions in a 976,748-byte WVB
and packages as a 31,013,888-byte executable in eight fragments. The emission
product contains 368 functions in a 775,522-byte WVB and packages as a
17,712,128-byte executable in five fragments. All remain inside the existing
segmented limits; no compiler, object, fragment, or runtime limit was raised.

The complete 524-function compiler still rebuilds as a 1,165,567-byte WVB, but
its native image crosses the unchanged monolithic staging ceiling. The focused
front door therefore retains that WVB rebuild as self-hosting evidence and
executes current source through admission, analysis, and emission products.
All 146 focused cases pass through that path. Broad storage and OS gates remain
deferred to final integration because none of these products changes them.

## Slice 3 compiler-capacity checkpoint

Decision 0777 consolidates token-to-type, binding-to-shape, declared-shape,
WIR-operation, WVB-opcode, and WVB-operation-length identity without changing
source or serialized semantics. Independent exhaustive comparisons preserve all
165 operation-to-opcode mappings and every operation-length result for no
shape, an ordinary shape, and the shape-15 floating-constant case.

The complete 505-function compiler rebuild contains 939,424 code bytes and
1,132,084 module bytes. Native planning reports 33,187,051 machine-code bytes
and 2,472 relocation bytes, removing 32,851 module bytes and 265,572 native
machine-code bytes relative to Decision 0776. The retained object envelope now
leaves an estimated 337,262 bytes below the unchanged 32 MiB limit. That margin
is not sufficient for the specialization table, substituted Foundation fields,
and deterministic type emission required by the remainder of Slice 3. A
versioned bounded source/type-analysis phase artifact and independently useful
WVB-emission phase are therefore the next measured capacity boundary; generic
semantics remain unclaimed here.

## Slice 3 analysis/emission phase checkpoint

Decision 0778 adds the fixed 104-byte `WVCA 1.0` manifest over separate
canonical WVSS, WVLB, and WVIR values. Every manifest count is independently
provable: the consumer rescans WVSS, reconstructs source symbols, validates the
complete WVLB directory, compares the WVIR header, and validates the complete
WVIR directory before prepared emission can begin. Unverified diagnostic and
performance counters are deliberately absent.

The WVB backend body is now `Compilerˉemitˉpreparedˉsourceˉwvb`. The retained
one-shot compiler constructs the same scan, symbols, bindings, and WIR summary
and delegates to that body, while the source-emission adapter accepts persisted
evidence only through the independent validator. Canonical WVB remains the
distribution contract; WVCA is an internal compiler-phase artifact.

The analysis core compiles to 952,903 bytes with 386 retained functions and
787,036 code bytes. Its deterministic and corruption fixture compiles to
957,810 bytes with 392 functions and 791,178 code bytes. The emission closure
compiles to 743,989 bytes with 349 functions and 615,041 code bytes, 34.3%
smaller than the one-shot compiler module before native packaging. The complete
compiler reconstructs with 506 functions, 939,530 code bytes, and 1,132,278 module bytes,
so the compatibility wrapper adds only 106 code bytes and 194 module bytes over
Decision 0777. The available scalar, general native, and WebAssembly execution
paths reject the compiler-heavy focused fixture at their documented operation,
module, or code boundaries; runtime execution is therefore not claimed by this
checkpoint. No storage, OS, or broad qualification gate was run.

Decision 0779 publishes independent hosted analyzer and emitter front doors plus
a development-only, target-aware Project 2 cache. The analyzer packages in
eight bounded fragments at 30,276,096 bytes and the emitter in five at
16,976,384 bytes without raising a tool limit. The retained one-shot path and
the split path produce the same 12,633-byte descriptor fixture with SHA-256
`53de13cfb20e237e71d5e34e6010f193eccbe815cc58a214b8c5ee2acf76bcc2`.
Small producer-identity manifests move roughly 47 MiB of executable hashing out
of cache hits; five Windows x64 warm samples have a 143.2-millisecond median,
down from the earlier approximately 285-millisecond hit.

That initial cache admitted only complete `portable-wvb-v1` Project 2 input.
Decision 0788 advances the fixed emitter target and isolated cache families to
`portable-wvb-optimized-v1`; neither version is qualification evidence. Project
3 profile-aware caching, general Slice 4
generic resolution and collections, repository-wide source migration,
paired-host equality, and final broad integration remain open.

## Slice 4 direct generic function lowering checkpoint

Decision 0786 adds a compact production WVGS/WVGC producer to the source-symbol,
binding, WIR, analysis, and emission closures. It accepts direct type parameters
as complete value-parameter or result types. Ordinary arguments infer a
declaration-ordered solution; `::` supplies the same full ordered solution
explicitly. Equal inferred and explicit uses reuse one catalog identity.
Conflicting contributions reject as `Genericˉresolution`, while a second
distinct specialization of the same declaration rejects as
`Genericˉspecialization` before evidence publication.

WIR generation performs one planning pass over ordinary functions, compiles
admitted generic bodies with concrete bindings, and repeats only when that body
discovers another generic instance. The fixed point is bounded at 32 passes.
Non-generic source retains one pass. The independent WVIR validator reconstructs
each selected solution from concrete parameter and result shapes and rechecks
the monomorphic function directory against source. WVGS and WVGC do not enter
WVIR or WVB.

The focused publisher accepts inferred-plus-explicit identity reuse and an
explicit result-only specialization. It rejects conflicting inference and a
second distinct identity. The published identity program contains two concrete
functions and executes through the unchanged emitter, verifier, and scalar
runtime with result `42` in 26 instructions. A same-length monomorphic oracle
produces byte-identical WVCA, WVLB, and WVIR, proving that source generic syntax
has disappeared before WVB emission.

The final publisher contains 452 functions, 864,885 code bytes, and 1,048,153
module bytes, SHA-256
`a2befed440f070ed934dd3ca783129cad30016ec2b46007548507f415cb3974a`.
The generic/oracle products share 104-byte WVCA SHA-256
`7c30318a94a9c16965347d17da358b309aefaa01519bafed80e48eb52b4a294a`,
148-byte WVLB SHA-256
`bda5d2ec661429a8649b3a23c905d1986fa5ad081b8c891c0283f5c534582a37`,
and 560-byte WVIR SHA-256
`dc3810d6b498fc2ff6d5676a584331df47292105daa13f5926dff309b1322be5`.
The unchanged emitter produces a 297-byte WVB with SHA-256
`cb7f970929bcdafa15c5f13b817f013ba30c033933d2988283b2e5c41ea316b3`.

The compiler-heavy focused publisher packages only through the existing
segmented native route; the monolithic route reaches its unchanged output limit.
No limit was widened. At this checkpoint general generic records and variants,
nested and constant arguments, phantom or template-only declarations, multiple
concrete specializations, and collections remained open Slice 4 work. Decision
0789 below removes the multiple-specialization restriction. Heavy storage, OS,
paired-host, and complete Qualification gates remain deferred to the final
seven-slice integration gate.

## Slice 4 bounded generic collection checkpoint

Decision 0787 extends the connected function subset to one structural bounded
collection signature. A formal `sequence<Type, Maximum>` or
`builder<Type, Maximum>` is matched as three facts: collection family, concrete
element shape, and exact maximum. The type and `const Maximum: u32` parameters
contribute independently to the canonical WVGS solution. Repeated inferred
evidence must agree; explicit `8u32` retains its exact width and unsuffixed `8`
does not convert implicitly.

The focused publisher rejects conflicting inferred capacities, an explicit
constant of the wrong width, and a builder supplied for a sequence formal. Its
successful source calls the same specialization by inference and explicitly,
publishes 104-byte WVCA, 184-byte WVLB, and 976-byte WVIR products, and returns
`42`. A same-length monomorphic oracle produces byte-identical artifacts with
SHA-256 identities
`debdc883ad8ebbde577589bc9248f58f79b70f5e7851409545b21be5282a73cb`,
`6df7f06016882fca5b38d909ca56136587a94975de60431daca96d13e9e35f4c`,
and
`c9a9299f223cae34887fd6788180f81b0b9a8d1499e99d5f81c2d053694361ab`.

The 1,065,397-byte publisher WVB has SHA-256
`ca1b50539ab3c53966fde062e8816b829d25b0dc0bd14bcb3374a813443ecc7a`.
Sharing immutable WIR state and generic-resolution paths reduces its selected
native image to 33,487,778 bytes, leaving 66,654 bytes below the unchanged
33,554,432-byte ceiling. The ordinary segmented hosted package completes and
returns `42`.

The retained driver emits the monomorphic oracle as a 466-byte WVB / SHA-256
`2d59187da5f16a3b275a6bbe96502ce1309f0ba8348e8a22da02097808c8b0c6`.
The pinned native verifier does not admit collection bytecode: it rejects both
that product and the pre-existing 809-byte monomorphic collection fixture at
the same semantic boundary. The fully current general emission driver builds
as a 1,268,289-byte WVB, but selects 37,097,130 native bytes and therefore was
not packaged by widening the fixed limit. Collection-capable native execution
remains downstream work.

Decision 0788 completed the initial target-aware validated-analysis emission
capacity checkpoint. The split emitter fixed its target to
`portable-wvb-optimized-v1`. The current product contains 402 reachable
functions and 687,924 code bytes in an 833,126-byte WVB / SHA-256
`be4a063cafe5b905ea2457e1c3c2ead36af2ecd4f9dd76a8a68a905dbf90a111`.
It packages through the unchanged profile-2 route in five fragments as a
19,005,440-byte Windows x64 application. The packaged current emitter reproduces
its own WVB byte-for-byte and emits the direct bounded-generic fixture and its
monomorphic oracle to the same 466-byte WVB above.

The split producer identity is now role-specific version 2 and the cache uses
isolated `project-analysis-wvca-v2` and
`project-split-wvb-optimized-v2` families. Its focused four-case owner checks
the adapter route, exact optimized and complete pruning oracles, and failed
cache-publication cleanup in under one second locally instead of rebuilding
three large compiler products already covered by the Language 1.0 front door.
That historical checkpoint retained a portable bootstrap pair: a 949,355-byte
analyzer and a 746,557-byte target-aware emitter at SHA-256
`a0fe54283ed51e1940bae837eb11bfb2d72f16dd91d7eb7022e51730eb0c5805`.
Decision 0789 keeps the old analyzer as recovery/provenance evidence but removes
it from the active front-door path. The gate now uses its already reconstructed
current analyzer with the bootstrap emitter, avoiding one redundant large
package and the old analyzer's exhausted instruction budget. No additional
native compiler executable is checked in.

Decision 0813 advances the active portable checkpoint to the current
992,412-byte analyzer and 895,787-byte emitter at SHA-256
`ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94`.
The pair consumes WVIR 1.3/1.4 and reproduces the emitter byte for byte. The
Language 1.0 gate also removes its unused monolithic `Compiler.wvb` build, so
current compiler-scale evidence comes from this one split reconstruction.

The Decision 0813 checkpoint published a 3,236-byte reachable Foundation generic
product, 147 bytes smaller than the historical complete product. Decision 0814
now omits the unused generic template entry and publishes 3,127 bytes while
preserving verifier admission and the exact typed result behavior.

Heavy storage, OS, paired-host, and complete Qualification gates remain
deferred to the final seven-slice integration gate.

## Slice 4 multiple concrete specialization checkpoint

Decision 0789 removes the temporary one-instance-per-declaration restriction
without adding runtime generics or raising a compiler limit. Source without a
generic instance retains WVLB 1.1/WVIR 1.3. Specialized analysis publishes an
inseparable WVLB 1.2/WVIR 1.4 pair: WVLB embeds the bounded WVGC catalog and maps
each concrete range to its source declaration, while WVIR appends one concrete
body per catalog instance. The ordinary generic declaration position remains a
zero placeholder.

Specialization indices begin after the complete WVSD entry directory rather
than after the source function count. `Generic-Multiple-Specializations.wv`
therefore places a record before `Identity<Type>`, infers distinct `i32` and
`u32` instances, and explicitly reuses the `i32` instance. Two current split
builds produce the same 473-byte WVB with SHA-256
`39811a38c92b8d4a6459750c64f85cf4e500bb4a2e4e83d31ab3bab626a70e12`.
The strict compiler-aligned verifier accepts it and the native scalar runner
returns `42`. Its three reachable WVB functions are `Main` plus the two concrete
bodies; the source generic placeholder is not emitted or exported. The current
optimized writer also removes the unused ordinary record declaration because
the complete retained WIR has no nominal use.

Independent validation now checks the exact embedded substitution, concrete
parameter and result signatures, catalog/declaration mapping, and specialized
call target before the emitter trusts cached analysis. The current local
analyzer is a 1,070,851-byte WVB with SHA-256
`7720b36a5c1f336ab26db4bc9a8e7eb1d3f0f686945f4d5f6627a5ad80d6f26c`.
Its 33,527,296-byte segmented Windows package stays 27,136 bytes below the
unchanged whole-image limit.

The cross-host Language 1.0 owner adds deterministic double compilation,
strict verification, and runtime execution for this boundary. The focused
Windows evidence above ran during implementation; paired-host results and the
heavy storage, OS, and complete Qualification gates remain deferred to the
final seven-slice integration gate.

## Slice 4 generic nominal type identity checkpoint

Decision 0804 adds WVGT 1.0 rather than extending the function-only WVGC
catalog or packing arbitrary arguments into the old Foundation-specialized
shape ranges. Each of at most 256 concrete record/variant instances receives a
private compiler shape `0x80000000 + instance`. Its identity retains the exact
WVSD declaration, nominal kind, and complete ordered type/constant argument
sequence, including phantom arguments.

Nested generic arguments reference only an earlier WVGT instance. Validation
recomputes exact depth, rejects forward or self references, and caps depth at
32. Admission validates the complete catalog, reuses equal identity before
growth checks, and rejects malformed lengths, aggregates, constants, duplicate
identities, a 257th instance, evidence beyond 1 MiB, or estimated emitted type
growth beyond 16 MiB. These private shapes are compiler evidence and do not
enter WVB.

The focused fixture covers ordinary, nested, constant, phantom-identity,
malformed, duplicate, limit, and reuse behavior. It builds to 65,457 WVB bytes
at SHA-256
`1387baaf0d9da4deed9ac5a7d37530f47c086c178461576e29f66168240e7d8b`.
Its 681,472-byte hosted Windows executable has SHA-256
`b6bf5abea06bf9ab2d6fc081742dc4c6812d0a3b80d149cb5bf733443ad7c924`
and returns `42` without output.

This is the representation foundation for general generic nominal source, not
the connected compiler claim. Declaration-parameter binding, recursive type-use
admission, substituted record/variant fields, typed WIR, reachable WVB type
materialization, and migration of the current Foundation special cases remain
the next Slice 4 checkpoints. Heavy storage, OS, paired-host, and complete
Qualification gates remain deferred to the final seven-slice integration gate.

## Slice 4 generic nominal declaration ownership checkpoint

Decision 0805 connects record and variant declarations to the existing bounded
generic parameter descriptor. Source Symbols now validates unique ordered type
and fixed-integer constant parameters for all three generic declaration kinds.
Generic record fields and variant payloads can retain an unresolved declared
type parameter. A constant in type position, a builder record field, and a bare
generic nominal template reject before symbol evidence is published. Parameters
nested inside collection or nominal type syntax remain owned by the subsequent
recursive type-use binder. Phantom parameters remain valid and participate in
the later WVGT identity.

The exact edition-1 Foundation `Option<T>` and `Result<T, E>` declarations now
pass the same parameter and payload validation rather than returning early from
variant validation. Their existing specialized use shapes remain unchanged.
General `Box<i32>` binding, recursive nested argument admission, field
substitution, WIR carriage, and WVB type materialization remain subsequent
connected checkpoints.

The 12-assertion focused fixture builds to a 604,172-byte WVB with SHA-256
`fa056f720caa741d3b3312e97ccd0b5dfce46559c07871f0e99eca229e06ca85`.
Its four-fragment 15,083,520-byte hosted Windows executable has SHA-256
`c8e87418ca758bab33f9f16ff93d36f74f1a8f2e5cb962a6bc56c1c29ab4d83a`,
returns `42`, and writes no output. The new independently runnable
`generic-nominal-declarations` owner reports visible build, package, and execute
phases and reuses content-keyed project and hosted-application checkpoints.
The broad Language 1, storage, OS, paired-host, and complete Qualification gates
remain deferred to the final seven-slice integration gate.

## Slice 4 recursive generic nominal type-binding checkpoint

Decision 0806 connects validated generic nominal declarations to WVGT without
yet mixing in field substitution or WVB emission. The focused binder gives the
ordinary type system first refusal, then parses an exact full-arity record or
variant use. Type arguments bind recursively, exact fixed-width literal
arguments satisfy constant parameters, nested instances enter WVGT before their
parents, and structurally equal uses reuse the first private shape. Phantom
arguments remain part of identity.

One outer use is transactional. A later arity, separator, contribution, or
admission failure restores the exact catalog status and evidence supplied by the
caller, including when a nested instance had already succeeded locally.
Malformed catalogs, a 33rd nesting level, mismatched type/constant kinds, and
wrong constant widths reject without publishing new evidence. Ordinary
nominals and the current Foundation-specialized `Option` and `Result` shapes do
not enter WVGT.

The 18-case fixture builds to a 649,494-byte WVB with SHA-256
`94a7b1672a846d329c9056f01539ca1d30499ddab0dc460862fd76a5855dfa9b`.
Its four-fragment 15,842,304-byte hosted Windows executable has SHA-256
`9071f7a16051ff46f422cb692d63a10103d3b36e57d0242198203548dc9c0e07`,
returns `42`, and writes no output. The independently runnable
`generic-nominal-type-binding` owner exposes build, package, and execute phases
and reuses the content-keyed project and hosted-application checkpoints.

General generic nominal use still does not reach application WIR or WVB. Exact
field/case substitution, private-shape carriage, reachable concrete type
materialization, and Foundation migration remain the next connected Slice 4
checkpoints. Storage, OS, broad Language 1, paired-host, and complete
Qualification gates remain deferred to the final seven-slice integration gate.

## Slice 4 generic nominal layout-substitution checkpoint

Decision 0807 binds each admitted WVGT instance back to its exact generic
record or variant declaration and substitutes its ordered direct type
parameters. The resulting layout exposes bounded record fields, variant cases,
and variant payload fields without publishing a second serialized catalog.
Declaration identity, origin, kind, parameter kinds, and parameter shapes must
match the WVGT entry before any layout is accepted.

Record-storage and variant-payload restrictions run again after substitution.
Consequently, a builder or capability cannot enter a record through `T`, and a
nested variant cannot enter a variant payload through `T`. Nested source uses
with adjacent closers now parse as type syntax without changing expression
`>>` tokenization. Nested template syntax inside a declaration field, WIR
carriage, canonical reachable-instance ordering, WVB materialization, and
Foundation migration remain subsequent checkpoints.

After the sequential-evidence refinement in Decision 0808, the 18-case fixture
builds to a 688,672-byte WVB with SHA-256
`55fe9cf4744cfe26f42900c85ad8eed9f6e0940cd7d6b533b7a6a94295c042b1`.
Its five-fragment 16,976,896-byte hosted Windows executable has SHA-256
`f28acda8fb1dc64da27e7e08d191ab637600e23c2e69505ee89aed40cc374f5c`,
returns `42`, and writes no output. The independent
`generic-nominal-type-layout` owner reports build, package, and execute progress
and reuses the content-keyed project and hosted-application checkpoints.
The broad Language 1, storage, OS, paired-host, and complete Qualification gates
remain deferred to the final seven-slice integration gate.

## Slice 4 generic nominal materialization-plan checkpoint

Decision 0808 converts admitted WVGT layouts into one bounded fixed-width
compiler plan. It traverses catalog instances in dependency order, assigns the
caller-selected contiguous ordinary Types range, indexes global case and field
records, and replaces every earlier private WVGT field shape with its ordinary
record or variant shape. Generic layout creation now retains each field and case
record once, so this sequential phase does not invoke a source-rescanning
random-accessor for every item.

Each evidence sequence is preflighted against the Foundation 4 MiB `bytes`
limit, the complete plan plus its catalog stays within 16 MiB, and the existing
1,024-type limit is checked before construction. Failure publishes no partial
derived evidence and reports the first failing instance. Full reconstruction
rejects caller-created type or field mutations.

The 20-case fixture builds to a 707,484-byte WVB with SHA-256
`0f91eb3d873f9dd9f5a68d53956b7be6f0ac7f62c70056241e99ea49ab47fe64`.
Its five-fragment 17,346,560-byte hosted Windows executable has SHA-256
`1989251b54de71bb6b7e69141e61529dd882a218bfd3892107ce4c6ff6f1e275`,
returns `42`, and writes no output. The independent
`generic-nominal-type-materialization` owner reports build, package, and execute
progress and reuses the content-keyed project and hosted-application caches.

This is the compiler materialization plan, not yet main WIR or WVB emission.
Connecting the plan to reachable source analysis, WVB Types and operations, and
then migrating Foundation special cases remain the next Slice 4 checkpoints.
The broad Language 1, storage, OS, paired-host, and complete Qualification gates
remain deferred to the final seven-slice integration gate.

## Slice 4 generic nominal WVB Types-serialization checkpoint

Decision 0811 consumes the fully reconstructed materialization plan and emits
each WVGT instance as one ordinary concrete WVB record or variant Types entry.
Catalog order becomes a contiguous suffix after declared nominals. Fixed-width
private names `__WvY0000` through `__WvY1023` keep output deterministic and
precede the existing `__WvZ000` Foundation suffix. Nested generic record and
variant fields carry their final ordinary type-table targets; source parameters
and private WVGT shapes are absent from the payload.

The focused serializer validates the exact declared-type base, the reconstructed
materialization, a unique at-most-256-entry Foundation first-use plan, the
1,024-type ceiling, nested nominal kinds, shapes, and its 4 MiB output bound.
Failure returns no partial payload. Existing record, variant, multi-field-case,
shape, and feature encodings are reused, so this checkpoint adds no WVB version
or runtime generic mechanism.

The expanded 30-case fixture builds to a 731,861-byte WVB with SHA-256
`c4283d87564abff8fe81d0d2fe6935745cbdc609dde20d2b97ad30d04f53c4c0`.
Its five-fragment 17,704,448-byte hosted Windows executable has SHA-256
`fdc0a0325e4d3e68ec133e7ad726c37f52f56c4a770e106c8593f9b85de8c14a`,
returns `42`, and writes no output. The focused
`generic-nominal-type-materialization` owner exposes its build, package, and
execute phases and reuses both content-keyed caches.

At the focused serializer checkpoint, main Source WIR did not yet carry retained
WVGT evidence into Source WVB. Decision 0812 now completes that analysis-side
carriage, while the complete backend still does not insert these entries or
remap their operation targets. This remains one exact connected checkpoint, not
a claim that general generic applications compile to WVB. Backend insertion,
Foundation migration, collections, paired-host evidence, and the final broad
gate remain.

## Slice 4 generic nominal WVLB-carrier checkpoint

Decision 0809 extends the existing binding artifact rather than creating a
fourth split-compiler product. WVLB 1.3 has a 40-byte header, retains the exact
optional WVGC and required non-empty WVGT byte lengths separately, and places
the two bounded catalogs between canonical 16-byte function ranges and binding
entries. Generic nominal instances do not create function ranges. A binding
may carry a private WVGT shape only when that instance exists in the retained
catalog.

Ordinary source continues to publish exact WVLB 1.1, and a program with only
generic function specializations continues to publish exact WVLB 1.2. The
focused generic-type publisher delegates to those existing forms when WVGT is
empty. `Source-Bindings-Core.wv` and every established compiler project closure
remain byte-for-byte unchanged. The extension validator reconstructs catalog
offsets, validates both catalogs, checks the function-range/WVGC relation,
bounds private shapes by WVGT instance count, and rejects cross-catalog length
confusion, short headers, truncation, trailing bytes, and reserved-field
mutations.

The fixture proves both legal layouts: a type-only WVLB 1.3 with no WVGC
bytes, and a combined directory with one retained WVGC specialization and one
retained WVGT instance. The latter also proves the appended 16-byte function
range maps back to its declaration and zero-based WVGC instance.

The 20-case fixture builds to a 796,891-byte WVB with SHA-256
`f0bfd9c749a380aca4efffb6ff61c6205d0a2b4bb94957149d7260d8db09add1`.
Its five-fragment 20,293,120-byte hosted Windows executable has SHA-256
`628c4c2ac2bf4029a767e821132209a406c52444486c7c61b1140b572a6bc52e`,
returns `42`, and writes no output. The focused
`generic-nominal-wvlb-carrier` owner reports build, package, and execute phases.

The large pre-existing Generic-WIR project was also probed after restoring its
project and source inputs exactly to `HEAD`. The immutable recovery Seed exits
at source binding 499, operation zero. A retained later compiler first
established that this is a bootstrap-capacity boundary rather than a failure of
the current source semantics. The independently reconstructed current split
analyzer/emitter compiles the ordered project to the exact 1,145,513-byte WVB
with SHA-256
`d56de5ae356a5e3dd6a36f3665792dce0e2c7ba968826c92e27ba0f4a046243e`.
The current analyzer publishes 104 WVCA bytes, 216,512 WVLB bytes, and 2,962,236
WVIR bytes; its WVSS byte length retains temporary source-path metadata and is
therefore not pinned. The emitter publishes 537 functions and 947,713 code
bytes. The independent compiler-aligned WVB verifier accepts the product. The
native staging route reaches its unchanged output limit, while the general
runner reaches its deliberately smaller call-depth limit; neither unrelated
bound is widened for this compiler-scale sentinel.

The analyzer source also now avoids moving its complete 22-field source-symbol
summary through private generic-parameter validation and declaration-type
binding paths that need only failure coordinates or lookup/count values. Shared
successful type-binding construction removes another repeated record path. The
resulting 478-function, 1,073,582-byte analyzer has a 33,545,634-byte complete
native object, 8,798 bytes below the unchanged 32 MiB limit, and preserves the
same language behavior under the complete 160-case owner.

Decision 0810 therefore makes this compiler-scale project a four-case sentinel
inside the existing Language 1.0 owner. It uses the analyzer/emitter already
constructed by that owner, compiles twice through the target-aware split cache,
requires byte identity plus the exact size and digest, and packages and executes
the result once. It does not rebuild the immutable Seed, create another compiler
tree, or add an independent heavy owner.

This checkpoint establishes durable split-compiler carriage and proves that the
current compiler-scale WIR closure is healthy through the supported split front
door. Decision 0812 now makes main WIR produce WVLB 1.3; consuming its
materialization plan in Source WVB remains the next connected checkpoint. Broad
storage, OS, paired-host, and complete Qualification gates remain deferred to
the final seven-slice integration gate.

## Slice 4 main generic nominal analysis checkpoint

Decision 0812 connects the accepted generic nominal components to the real
source analyzer. Source Symbols defers an already-parsed `<...>` application
instead of assigning a template shape. Main Source WIR scans ordinary function
signatures in canonical module/declaration order and explicit locals in source
order. The WVGT binder remains the single owner of family, arity, argument kind,
constant width, nesting, and immutable catalog admission.

WVGC and WVGT travel inside one bounded compiler-private `WVGI 1.0` build value.
When WVGT is empty, the prior raw WVGC bytes are retained exactly. A non-empty
WVGT selects WVLB 1.3; WVIR uses 1.3 when there are no function
specializations, but its function, operation, temporary, parameter, and local
shapes may carry only private identities bounded by the paired WVLB catalog.
Independent analysis validation uses the combined WVLB carrier.

`Generic-Nominal-Main-Pipeline.wv` produces exact 238-byte WVSS, 104-byte WVCA,
192-byte WVLB 1.3, and 320-byte WVIR 1.3 artifacts. The binding artifact retains
one 68-byte WVGT instance for `Box<i32>` plus parameter shape `0x80000000`; the
identity function's return and parameter-load operation retain the same shape.
The ordinary main function remains `i32` and carries constant `42`. A 12-case
artifact inspector pins those boundaries. The established generic-function
fixture remains byte-identical across all four analyzer products when WVGT is
empty.

Decision 0813 advances ordinary/specialized WVIR to 1.3/1.4 and reduces each
persisted operation from 40 to 32 bytes by dropping only its backend-unused
source span. On the exact 1,727,318-byte emitter source closure, predecessor
WVIR is 4,144,676 bytes and current WVIR is 3,526,316 bytes: 618,360 fewer bytes,
or 14.919 percent. Both paths emit the exact same 895,787-byte WVB at SHA-256
`ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94`.
Ordinary source also skips generic-nominal signature scanning when its validated
declaration directory contains no generic record or variant.

The final Windows analyzer contains 477 functions, 811,632 code bytes, and
992,412 WVB bytes, SHA-256
`26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120`.
Its eight-fragment profile-7 package is 31,740,416 bytes, SHA-256
`52c6cccdcaed1e99ea87759751d232e0f39bd1ed923d0555e4da5f4b236b442f`.
The paired 19,718,656-byte emitter package has SHA-256
`3f5d3d6baf9a41926b1e0c9068e31aea0612df51c5675ff69228f54874ab5347`
and reproduces the exact emitter WVB above. The closure exhausted the retained
64,000,000,000-instruction bound and succeeds under profile 7's measured
80,000,000,000 bound; profiles 1 through 6 remain at 64,000,000,000. A cached
repeat completes in 0.295 seconds on the current Windows host.

This is current-host development evidence. Main WVB materialization/remapping,
generic nominal construction and field operations, generic-function-context
type uses, Foundation migration, and paired-host qualification remain open.

## Slice 4 main generic nominal WVB materialization checkpoint

Decision 0814 connects the retained WVGT catalog to the real Source WVB backend.
Generic record and variant declarations remain source templates and receive no
runtime Types entry. The backend compacts concrete declared records, enums, and
variants into a bounded prefix, appends retained WVGT instances in catalog order,
then appends the existing concrete Foundation suffix. Explicit record and
variant target maps remap every affected shape and nominal operation.

Materialized fields retain an earlier instance's private WVGT identity until the
final WVB planning boundary. This is necessary because removing templates can
make an ordinary source nominal target numerically equal a generic output target.
The final serializer resolves that identity and validates its record/variant
kind; no private shape enters WVB. The packed materialization evidence now uses
bounded type-word and case-word accessors, which return the existing sentinel on
an invalid plan, index, or word.

The complete emission compiler initially reached 133 types against the retained
128-type native boundary. Five compiler-only wrapper records were removed rather
than widening the limit. The final 22-module analysis publishes 1,848,314 source
bytes, 283,268 WVLB bytes, and 3,739,652 WVIR bytes. Its optimized product has
529 functions, 798,745 code bytes, and exactly 128 Types entries in a
964,539-byte WVB at SHA-256
`9c11b7eb3b9e250817a0a763adf1fea8d7406bf6e2869247f4a7f84146307347`.
The existing profile-7 native path accepts it and publishes a six-fragment
21,254,144-byte Windows executable at SHA-256
`57c36ac13745b103fccbd677d4f54c3dbc112c739b520690b424b40bae491278`.

The strengthened `Generic-Nominal-Main-Pipeline.wv` deliberately combines
template `Box<T>` with concrete `Point` and materializes `Box<Point>`. Ordinary
source target 1 and generic output target 1 collide, proving that numeric range
tests are insufficient. The analyzer publishes exact 272-byte WVSS, 104-byte
WVCA, 208-byte WVLB 1.3, and 368-byte WVIR 1.3 artifacts. The new emitter
publishes a deterministic 252-byte WVB 1.11 at SHA-256
`8871f2876c9135e8f4f8740f7643d1ff5a5eb0e771da0dddd3357e1bed9d29aa`.
Its Types section is exactly `Point { X: i32 }` followed by
`__WvY0000 { Value: Point }`; it contains no `Box` template. The independent
compiler-aligned verifier accepts it and the native runner reports `Result: 42`.
The artifact inspector expands from 12 analysis cases to 20 analysis/WVB cases,
and the Language 1.0 owner adds independent verification and execution.

The focused generic nominal materialization owner still passes all 30 cases and
returns `42`. Its updated 734,722-byte WVB has SHA-256
`080990672a4f2912877ddae201c9fe0b35c858c40d51dc072567a3191e6e7757`.

This is current-Windows development evidence. It proves main WVB metadata,
template elision, target remapping, verifier admission, and executable
publication. Runtime construction and field access for general generic nominal
values, generic-function-context type uses, the remaining Foundation migration,
and paired-host qualification remain open.

## Slice 4 packed keyword dispatch checkpoint

Decision 0799 replaces repeated text materialization and byte-by-byte comparison
for the seven- and eight-byte English keyword groups with exact packed word
classification. Two bounded `u32` reads cover every byte of either token; fixed
length plus complete head/tail equality is collision-free and does not alter the
canonical token mapping. The other keyword lengths retain their existing exact
classification.

The profiling source uses the real lexer over the non-keywords `Compile` and
`Compiler`, which are common shapes in the compiler closure and force complete
rejection of the relevant keyword groups. Under the same current Windows
4,096-instruction scalar-runner bound, the original path completes two loop
iterations and rejects four at `WVR3011`; the candidate completes eight and
rejects sixteen at the same exact bound. The original micro WVB contains 10,221
code bytes in a 15,856-byte module; the candidate contains 10,727 code bytes in
a 16,096-byte module. These are bounded hot-path instruction observations, not
a claim that complete compiler wall time improved by the same ratio.

The representative analyzer uses the exact 14-source Project 2 analysis-driver
order on Windows 11 build 26200 and an AMD Ryzen 9 3900X. The retained baseline
median is 53,553.067 milliseconds over three runs. One packed-dispatch probe
took 55,178.412 milliseconds with a sampled 551,317,504-byte peak working set;
that noisy full-process result does not establish a complete-compiler speedup.
It does establish no broad timing claim while the exact output proof remains
strong: old and candidate analyzers publish byte-identical WVSS, WVCA, WVLB, and
WVIR artifacts for the changed source state. The remaining structural target is
repeated lexer/parser traversal and token-record pressure, not another increase
to an execution, evidence, or native-image limit.

The bounded value-front-end fixture exercises all ten packed spellings and four
near misses through the exported keyword classifier without raising the scalar
runner's instruction bound. The final Windows `language-1-front-door` owner
passed all 11 phases and 155 declared cases. Its deterministic target-aware
emitter is 838,654 bytes with SHA-256
`707c3aec27b481745ae599206960bc6f9c0be0053aaae73b359cd20cd2cc4876`.

## Slice 4 source-graph frontier checkpoint

Decision 0801 gives reachability state three private meanings: unseen, pending
expansion, and completely expanded. A successful expansion changes its current
module from pending to complete. Later fixed-point passes therefore parse only
newly discovered earlier-ordered modules instead of reparsing the complete
reachable prefix. The public graph summary, WVSS format, import lookup,
diagnostic order, module/pass bounds, adjacency construction, and cycle proof
remain unchanged.

The exact 14-module analysis-driver graph requires two reachability passes. Its
old implementation performs 27 module-expansion visits: 13 in the first pass
and all 14 again in the second. The completed-frontier implementation performs
14 visits: the same 13 followed only by the one newly pending module. This is a
48.1% reduction in expansion visits without retaining a parsed tree or raising
any memory bound.

Both focused tools were compiled by the same accepted analyzer and emitter.
The original has 149 functions, 295,816 code bytes, and 364,759 module bytes;
the candidate has 149 functions, 295,939 code bytes, and 364,903 module bytes.
They both report exactly:

```text
source graph status=Valid modules=14 imports=41 reachable=14
```

Four interleaved warmed Windows runs on an AMD Ryzen 9 3900X measured a
3,784.766-millisecond original mean and a 3,281.864-millisecond candidate mean,
a 13.3% reduction. Their medians are 3,773.672 and 3,286.330 milliseconds. This
is focused graph-phase evidence rather than a claim that complete compilation
improves by the same ratio.

The final 1,071,235-byte analyzer WVB has SHA-256
`52feeed48b2526441d36a2335e50ffe26b6974c82255f367a7f3f0e62e3e9cec`.
Its 33,531,904-byte Windows package has SHA-256
`cbe35be00d52d188459be25acdb337dc834e61315e530e32a84f9765150f8035`.
The analyzer deterministically produces the current 838,798-byte target-aware
emitter with SHA-256
`e40da70ba3cf1ef85193bd5b2fe2657faf0068d5951cb36f232d80ec7f7223fe`.
On the same current source input, the accepted and candidate analyzers publish
byte-identical WVSS, WVCA, WVLB, and WVIR artifacts. The source-graph demo adds
a canonical dependency order in which a later importer discovers an earlier
module and returns zero through the generated native executable. Heavy storage,
OS, paired-host, and complete Qualification gates remain deferred to the final
seven-slice integration gate.

The final Windows `language-1-front-door` owner passed all 11 phases and 155
declared cases with the exact inline analyzer and target-aware emitter above.

The compiler-source sentinel was also attempted for both this candidate and the
clean upstream tip. Both reach native staging and fail its existing 4 MiB object
limit. The clean tip reports 600 functions, 1,048,036 code bytes, and a
1,262,814-byte WVB; the inline candidate reports the same function count,
1,048,159 code bytes, and a 1,262,958-byte WVB. No limit was raised and this is
not recorded as a pass. Repairing or replacing that already-overflowing staging
gate is separate verification-infrastructure work.

## Slice 2 named variant fields

The edition-1 declaration parser admits zero through 64 uniquely named fields
per variant case and rejects an empty parenthesized list. Named construction
uses `Type.Case { Field: Value, ... }`; it requires every declared field exactly
once, evaluates source expressions left to right, and supplies WIR constructor
operands in declaration order. A no-data case uses `{}` and emits zero operands.
Descriptorless Seed retains its zero/one positional payload syntax.

Named match patterns use `case Type.Case { Field: Binding, Other: _ }`. Every
declared field appears exactly once in any order, `_` creates no binding, and
other immutable bindings are scoped to the selected arm. WIR operation
`Variantˉfield = 164` consumes the exact nominal variant and carries the nominal
index plus packed `case * 64 + field` identity. The retained `Variantˉcreate =
65` now validates exact zero-through-64 operand arity; the older
`Variantˉpayload = 67` remains restricted to exactly one field.

WVB 1.16 adds Types marker `2`, followed by a canonical field count and named
shapes, plus opcode `C4` for exact field extraction. Marker `0` and marker `1`
remain byte-identical for zero-field and one-field cases. Either marker `2` or
opcode `C4` selects 1.16. This includes a one-field named pattern, which retains
marker `1` but still requires the new opcode and version.

`Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv` compiles twice to the same
918-byte WVB 1.16 module with SHA-256
`f3ceb596f1bcedda877ceea5aeb99aff1d5bcfa3b984fdae0e16eb21570562d1`.
Its empty `if` and `else` blocks also prove that a one-part condition followed
by a block is not reinterpreted as zero-field named construction.
`Named-Variant-Field.wv` isolates instruction-driven version selection in a
428-byte WVB 1.16 module with SHA-256
`2dea4aa515633e85863e51279f320d53f09c2bf4628b72d93fdc79559479209f`.
Both pass the compiler-aligned verifier and execute through the source-built
native scalar runner with result `42` in 76 and 26 guest instructions,
respectively. Nine source fixtures reject duplicate
declarations, empty payload declarations, missing/duplicate/unknown constructor
fields, type mismatch, and missing/duplicate/unknown pattern fields. Nine WVB
mutations reject version, marker, field-count, nominal, case, field, type, and
truncation corruption. A tenth mutation preserves an in-range but inconsistent
runtime case path: the verifier rejects it and direct scalar execution fails
with `WVR3017` after 32 guest instructions.

The runtime stores the first field slot and the exact nominal/case owner in one
eight-byte cell. Variants share the existing fixed 768-cell immutable aggregate
arena with records and allocate exactly their declared fields. Stack aggregate
flags, active locals, and saved frame locals are roots. The bounded collector
uses the selected record or variant field directory to mark nested record and
variant values, sweeps unreachable spans, releases descriptor fields, and
retries once.
The separate 512-byte WVB 1.11 one-field pressure oracle performs 900 variant
replacements, forces collection beyond capacity, and returns `42` after 26,134
guest instructions. Direct native lowering, browser packaging, and Windvale OS
execution remain narrower consumers.

## Slice 2 fixed integers, runes, and floating point

The fixed-width checkpoint admits `i8`, `i16`, and `u16` literals, typed
constants, parameters, results, and locals with exact same-type checked
operations. Internal shapes `11` through `13`, WVIR operations `129` through
`147`, WVB 1.12 tags `14` through `16`, and opcode `C0` preserve the named width
through every boundary. The deterministic 5,335-byte fixture has SHA-256
`b3cca3ae81dfadc78d45b1f83b5bdd7a3deaff1d42624e12c2a610bdb3f222a9`
and returns `42`; malformed selectors, shapes, widths, overflow, division by
zero, and invalid shifts fail at their named boundaries.

The rune checkpoint appends lexer token `Runeˉliteral = 98`, body expression
kind `Rune = 14`, internal shape `16`, and WVIR operations `148` through `150`.
A literal holds exactly one direct strict-UTF-8 scalar, admitted simple escape,
or braced Unicode escape. Empty, multiple, unterminated, unsupported, surrogate,
and above-`10FFFF` forms reject compilation. There is no numeric, text, locale,
or normalization conversion.

WVB 1.13 adds tag `17` and opcode `C1` for an exact scalar constant, equality,
and inequality. The compiler chooses 1.13 only when rune evidence exists, while
unaffected 1.11 and fixed-only 1.12 output remains stable. The verifier rejects
version downgrade, unknown selectors/envelopes, non-scalars, shape mismatch, and
truncation. The shared interpreter executes the family through a focused core;
the reconstructed native runner executes the actual compiler-produced module and
returns `42`.

`Tests/Fixtures/Language-1.0/Rune-Program.wv` compiles twice to the same
1,148-byte WVB 1.13 module with SHA-256
`116ff74b5b9c18a76af21785b7aa9017fe4f0c4ff73fa363dfa72898cf9d3dde`.
It covers ASCII, Japanese, emoji, all admitted simple escapes, braced escapes,
typed constants, parameters, locals, results, equality, and inequality.

The floating-point checkpoint adds strict suffixed hexadecimal literals,
internal shapes `14` and `15`, WVIR operations `151` through `162`, and WVB 1.14
type tags `18`/`19` plus opcode `C2`. The source lexer converts the exact
hexadecimal value directly to binary32 or binary64 with round-to-nearest,
ties-to-even. The compiler preserves raw bits through literals, locals,
parameters, results, arithmetic, unary negation, and every comparison without an
implicit conversion.

The shared scalar interpreter implements the two IEEE formats with integer
operations over raw bits. It preserves subnormals, infinities, and signed zero;
canonicalizes every NaN result; and therefore does not inherit a host
floating-point mode. `Tests/Fixtures/Language-1.0/Floating-Program.wv` compiles
twice to the same 2,809-byte WVB 1.14 module with SHA-256
`c783fd85deca397814da71a87ec543ec75f800d4ecd10549c53091d48fd54327`
and executes with result `42` through the current source-built runner.

## Focused verification owner

The maintained `language-1-front-door` owner reports 172 declared cases. Its
bounded checkpoints recompute the frozen identities, compare two descriptor-test
builds and execute them, build and execute the 39-assertion value-front-end test,
construct the changed compiler through the shared segmented backend, and retain
the minimum, unit, record-update, 12-case generic nominal main-analysis, and
22-case fixed-integer evidence. Its 20 rune
cases compile the valid program twice, compare diagnostics and bytes, reject eight
source forms, admit the canonical module, reject six byte-level corruptions, and
execute both the reconstructed shared runner and focused runtime core. The 27
floating cases add deterministic compilation, four source rejections, eight
malformed-WVB rejections, current-runner execution, and a focused raw-bit runtime
self-test. Its 21 unit/never cases add deterministic compilation, four source
rejections, eleven malformed-WVB rejections, and two executions returning `42`.
Its 25 named-variant-field cases add deterministic multi-field compilation, one
isolated one-field version-selection module, nine source rejections, two valid
verifier admissions, ten malformed-WVB rejections, two valid WVB 1.16 scalar
executions, an explicit `WVR3017` case-mismatch execution, and the bounded
900-replacement pressure execution.
Value-producing `if`, value-producing enum/variant `match`, and the first
identity-checked concrete Result `try` checkpoint reuse that one rebuilt compiler
and the same verifier/runtime artifacts. The typed-failure phase adds one
deterministic success/failure program and four source rejections.
The report keeps nested assertions separate from the top-level owner count.

Frozen design inputs, descriptor files, edition-1 fixtures, and the integrated
compiler boundaries map to this owner. Compiler WVB/image construction and
hosted application packaging use content-keyed cross-host caches: the first run
earns full native evidence, while unchanged repeats materialize validated cache
hits instead of rebuilding the 29 MiB compiler application. This is development
evidence, not yet a paired-host conformance claim. With the Windows compiler and
application checkpoints populated, the prior seventeen-case owner passed in
19,190 milliseconds. The first twenty-two-case Windows run rebuilt both changed
compiler cache products in 343,830 milliseconds; all child checks passed and only
the then-stale registry summary rejected the owner. After sharing the field parser
between construction and update, the final registered cold run passed all 22 cases
in 344,610 milliseconds. Its immediately following warm-cache run passed in
20,810 milliseconds, including 20,350 milliseconds in the owner itself.

## Pre-slice compiler baseline

The reference source state is commit
`44f677d6853ffd2abebd3533cabe8e91b8a6fc28`, immediately after the source
freeze and before the descriptor component. Measurements ran on Windows NT
10.0.26200.0 x64 with an AMD64 Family 23 Model 113 processor. The compiler
application was the 27,467,776-byte
`Artifacts/Native-Compiler-Seed/windows-x64/wvcompiler.exe`, SHA-256
`344940f66b26b516b8b4e10a712a6b2c01cbff95aa7ff18aac0789ba9197f970`.

The exact `Projects/Examples/Windvale-Compiler.wvproj` order contains 13 modules
and 1,161,243 input bytes. Three optimized direct invocations, without
`--complete`, produced:

| Sample | Elapsed milliseconds | WVB bytes | WVB SHA-256 |
| ---: | ---: | ---: | --- |
| 1 | 46,143.804 | 959,320 | `e177e418bfd8fdcbe40cfac513ce40e58b95ba5b88a8a1d1db9fe280ae81dbfb` |
| 2 | 46,187.198 | 959,320 | `e177e418bfd8fdcbe40cfac513ce40e58b95ba5b88a8a1d1db9fe280ae81dbfb` |
| 3 | 47,100.433 | 959,320 | `e177e418bfd8fdcbe40cfac513ce40e58b95ba5b88a8a1d1db9fe280ae81dbfb` |

The mean is 46,477.145 milliseconds. A fourth identical invocation took
46,183.648 milliseconds while 50-millisecond process sampling observed a peak
working set of 141,778,944 bytes. Every output reported 445 functions, 790,934
code bytes, and 959,320 module bytes.

The matching source-WIR inspection reports:

```text
source wir status=Valid modules=13 functions=533 blocks=11356 operations=52969 temporaries=48243 operands=42028 directory-bytes=2823444
```

The representative Echo package remains the independently pinned 927-byte WVB,
SHA-256
`b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713`.
It is recorded here from its existing exact application owner; the expensive
cross-target package/execution owner was not rerun merely to restate that
unchanged identity.

## Slice 1 compiler measurement

The Windows Slice 1 candidate was constructed from the changed tree through the
shared native backend. Its build-driver WVB contains 454 functions, 803,758 code
bytes, and 975,403 module bytes, SHA-256
`f4609cdc5d25850a418b1497879e07b3ec5013b134e3e92e3f93997537b54595`.
The current-host compiler application is 29,161,984 bytes, SHA-256
`0b78368eb9d3e5347986eda1d5b4763782479eda5baf5b4f3570dc9ee8531279`.

That candidate compiled the current 14-source compiler project twice in
optimized mode:

| Sample | Elapsed milliseconds | Peak working set | WVB bytes | WVB SHA-256 |
| ---: | ---: | ---: | ---: | --- |
| 1 | 44,161.410 | 120,414,208 | 951,241 | `8b53dc43d80a78ad7f3ee6f8fa2235d7966041d23a6cebfe341ac78184b61b89` |
| 2 | 44,320.267 | 124,043,264 | 951,241 | `8b53dc43d80a78ad7f3ee6f8fa2235d7966041d23a6cebfe341ac78184b61b89` |

The 44,240.839-millisecond mean is 4.812% below the pre-slice mean despite the
additional source-profile module. The largest sampled working set is 12.509%
below the pre-slice observation. These are useful regression signals, not a
causal speedup claim: the samples use the new compiler application, have only
two repetitions, and do not replace a Linux baseline.

## Current-driver bootstrap boundary

The qualified semantic-freeze front door predates the current compact WIR
implementation. It rejects the enlarged 22-module build-driver source at its
older retained-evidence bound, so it is not used to disguise forward-language
source as a semantic-freeze artifact. The explicitly unqualified current
candidate driver accepts the same exact project.

On Windows the current driver deterministically emitted a 1,259,719-byte WVB
containing 562 functions and 1,057,737 code bytes, SHA-256
`3e84e6dc8e646f7cde061e21fdbff7850e83e9faa83114d810b70297a445f949`.
The independently reconstructed staging producer accepted it as 32,003,453
object bytes across 40 chunks with a 504-byte manifest. The complete four-case
`segmented-compiler-toolset-reconstruction` owner passed in 322,210
milliseconds. No candidate container or qualified front-door identity was
promoted or repinned by this development checkpoint.

## Measurement limitation and next checkpoint

`Tools/Native/Measure-Source-Wvb-Compilation.ps1` currently forces paired
optimized and `--complete` runs. The current native compiler completed the
optimized sample, but its complete mode exited 1 without a diagnostic, so that
driver could not retain this baseline. The direct optimized measurements above
kept exact input order, artifact identity, and temporary cleanup, but peak
working set is a sampled observation rather than an allocation proof.

Before performance comparisons are promoted, the measurement driver still needs
an explicit mode selection, retained host metadata, and bounded live memory
sampling. Linux needs the same exact baseline and owner result before paired-host
performance or Language 1.0 conformance is claimed.

Migration Slice 1 is complete: source-profile locks and composite profiles are
explicit Project 3/build inputs, their pinned chain controls English token
resolution, and Project 2 remains stable. Slice 2 now includes primitive
front-end identities, edition separation, named record update, fixed-width
integer execution, exact rune execution, strict floating point, and complete
unit/never semantics over the compiler-aligned WVB 1.15 verifier and scalar
runtime checkpoint. Named variant construction and destructuring now reach the
compiler-aligned WVB 1.16 verifier and source-built native scalar runner with a
shared bounded aggregate collector. Value-producing `if` and exhaustive
enum/variant `match` now complete Slice 2's planned control-value compiler
surface over the same architecture. Seed stays on that architecture until its
named removal checkpoint; final paired-host and broad qualification evidence is
deferred to the seven-slice integration gate.

## Slice 4 executable generic record checkpoint

Decision 0815 connects applied generic record values to ordinary body lowering.
The source spelling is `Box<Point> { Value: ... }`; its following brace makes the
type application unambiguous without weakening the `Name::<T>(...)` rule for
explicit generic-function calls. Typed analysis admits the full application
through WVGT, reconstructs its substituted record layout, and reuses WIR
operations `17` and `18` for construction and field reads. Main WVB maps those
private operation targets through the existing materialization plan, so runtime
and native execution see only ordinary monomorphic record operations.

The frozen grammar had accepted generic record declarations but omitted an
applied construction production. The named 3,822-byte amendment manifest at
SHA-256
`57cd5ccb710ca504b55644194cfa20a576bc0fd8ebd33247ef232c30d0d84162`
preserves the original freeze manifest and binds the corrected 251-input
effective source identity at SHA-256
`16cd7aeddb876d58f63c2ebf14016e74f19c2e3b2ff25e36c09e671837faaec7`.

The active split route builds a 1,032,689-byte analyzer WVB with 505 functions
and 844,526 code bytes at SHA-256
`41b3a9c71dde657168929a1ba860e98c0b5fa27408d0f71add741d6fb49b94e5`.
Its 32,928,256-byte Windows application has SHA-256
`0e706c345152b10e24d436e82e23a1950a9a0897988e4bd434fa24eecc1e9a2b`.
The connected emitter contains 531 functions and 805,158 code bytes in a
972,044-byte WVB at SHA-256
`8e1a28be1cd492f42ca77df720f67d4b699407b3ad6482ebba4773a999d78140`.

The strengthened `Generic-Nominal-Main-Pipeline.wv` constructs a nested
`Box<Point>`, passes it through an exact `Box<Point>` identity function, and
returns `Wrapped.Value.X`. Analysis publishes exact 377-byte WVSS, 104-byte
WVCA, 244-byte WVLB 1.3, and 640-byte WVIR 1.3 artifacts. Emission publishes a
441-byte WVB 1.11 at SHA-256
`71c8e08b2a736ebbc2042f4188c8ed813091dfd72ced93226f5467bd507e73ed`.
The compiler-aligned verifier accepts it and execution reports `Result: 42`.
The 20-case inspector proves both nominal Types entries plus the ordinary
construction, private construction, call, private field read, and nested
ordinary field read.

Four companion fixtures reject a missing field, duplicate field, mismatched
field type, and unknown field with their exact one-line standard-error WIR
status, no standard output, and no partial WVSS/WVCA/WVLB/WVIR publication. The Language 1.0 owner therefore
expands its generic nominal pipeline from 22 to 26 cases and its complete
inventory from 182 to 186. The changed generic-WIR fixture builds
deterministically to 1,187,360 bytes at SHA-256
`f4ac7f82d79072bdc83c450d8ae4f9cab89550cf39efde2f7e96b56686b9eccd`.
The older fixed native front-door verifier silently exhausts its execution
budget on that compiler-scale module. The owner now checks its exact bytes in
phase 4 and performs the one semantic verification with the current source-built
native verifier in phase 5; that verifier accepts the module as
compiler-aligned. This keeps the recovery-era front door immutable and removes
a redundant known-undersized verification attempt.

This is current-Windows development evidence. It does not claim paired-host
qualification. Generic variants, template-dependent generic-function bodies,
and remaining Foundation-special planning are still open Slice 4 work.

The final coordinated Language 1.0 owner passes its complete 186-case inventory
in 660,130 milliseconds, or 660,650 milliseconds including coordinator
overhead. The run includes all eleven named phases and the current-native
generic-WIR verification. Broad storage, OS, paired-host, and Qualification
owners remain intentionally deferred because this checkpoint changes none of
their contracts.

## Slice 4 generic nominal function-body checkpoint

Decision 0816 makes an instantiated generic function's exact WVGC arguments
available to generic nominal type binding. `Box<T>` in `Wrap<T>` or `Read<T>` is
therefore admitted as the same `Box<Point>` WVGT identity when those functions
specialize for `Point`. This applies to signatures, explicit locals, record
construction, returns, and field reads. Inference also decomposes an exact
actual `Box<Point>` only when the formal uses the same generic declaration and
full arity; it does not add result-context inference or overload search.

The active split route produces a 1,046,844-byte analyzer with 508 functions
and 856,375 code bytes at SHA-256
`d65b1e159768ea7fa75775de1015f57ceb92af8ac76ae45e5dec301258d79d06`.
Its eight-fragment Windows application is 33,302,528 bytes at SHA-256
`7dd3682ac2072a8b0505b23afda5e0baef72fa2854ecaeda08afe27688fd081c`.
The connected emitter contains 532 functions and 808,976 code bytes in a
976,573-byte WVB at SHA-256
`e0bcf72d6f04efbd9e24b139bb7a9db48dba7538571c4dda371292e5b1093230`;
its six-fragment Windows application is 21,560,320 bytes at SHA-256
`d4cde64d604b7f59f00cb4b21cc550671d081a6162576cd63f510abe596d3182`.

`Generic-Nominal-Function-Body.wv` publishes exact 467-byte WVSS, 104-byte
WVCA, 504-byte WVLB 1.3, and 1,040-byte WVIR 1.4 products, followed by a
600-byte WVB 1.11 at SHA-256
`a27f28ed39ba407c196f461723d1232563372e7684203ee29e151fdb383dacc6`.
The 28-case structural inspector proves two function specializations, one
generic type instance, concrete parameter/local/return shapes, construction,
field access, two exact calls, removal of both templates, and three emitted
functions. The compiler-aligned verifier accepts the module and execution
returns `42`. Three companion fixtures reject mismatched construction,
substituted unknown-field access, and a different generic template used for
inference with exact one-line diagnostics, empty standard output, and no WVSS,
WVCA, WVLB, or WVIR publication.

The generic-WIR compiler fixture remains deterministic at 1,201,537 bytes and
SHA-256
`b01236a677548399e0f2cc49410b459291d37fe433fd3f730ae671573a5d87d4`.
The Language owner registers 33 new function-body cases and grows from 186 to
219 cases. Its complete current-Windows run passes in 577,760 milliseconds, or
578,260 milliseconds including coordinator overhead. The 109-owner registry
contains 4,966 cases at SHA-256
`b0a66a9bf4a8615755faaa5fec5ad78abc5987a92385f125e11a41a960f836e9`.
No storage, OS, paired-host, or Qualification owner is required for this
compiler-only development checkpoint.

This closes direct `Box<T>`-style use in generic-function bodies, not every
recursive generic pattern. Generic variants, an applied generic field inside a
generic declaration's own layout, deeper nested formal applications, and the
remaining Foundation-special plan remain explicit Slice 4 work.

## Slice 4 generic nominal declaration-layout dependency checkpoint

Decision 0817 makes generic nominal binding discover stored layout dependencies
before admitting the enclosing concrete instance. For
`Holder<T> { Wrapped: Box<T>; }`, specializing `Holder<Point>` now admits
`Box<Point>` first and `Holder<Point>` second. Both have WVGT application depth
one; their catalog order separately proves that the stored field's concrete
layout is available before its consumer. Layout consumes the same bounded field
plan read only and cannot append a late instance.

The plan covers records and variants, direct type parameters, and direct
fixed-width constant parameters. Dependency admission is transactional: a
failure restores the exact input catalog. Recursive value containment is
bounded by the existing depth-32 rule, reports `Genericˉresolution`, and
publishes no partial analysis artifact family. Source declaration lookup remains
declaration-before-use; this checkpoint does not add forward declarations.

The active split route produces a 1,055,646-byte analyzer with 515 functions
and 863,823 code bytes at SHA-256
`b4f04ef8d843f1af0dcd405e3c19a02f8a11532ac9996745e8b3fc7b956c4e7d`.
Its eight-fragment Windows application is 33,436,160 bytes at SHA-256
`c5da4daf6aff6be48fd37e79bf5ac81ad8986ff9c796d54bf4905e6ea17ed847`.
The 985,374-byte emitter contains 539 functions and 816,424 code bytes at
SHA-256
`a95c9c248554919bfedae75795077467570924157d0c59a22060f1c1617a50e6`;
its six-fragment Windows application is 21,693,952 bytes at SHA-256
`c17e10f3d1d6abde396fdc274627e3b6046af26d8671910481f525c3b7208804`.

`Generic-Nominal-Declaration-Dependency.wv` publishes exact 526-byte WVSS,
104-byte WVCA, 564-byte WVLB 1.3, 1,168-byte WVIR 1.4, and 668-byte WVB 1.11
products. The WVB SHA-256 is
`5ec54be82a84a0bea60fd6cb8146c08ddf8fb934aaf9560734250eadd20ee046`.
Its 32-case inspector proves the two dependency-ordered WVGT instances, five
concrete bindings, three concrete WVB types, exact nested field targets, removal
of both templates, compiler-aligned verification, and execution result `42`.
The exact cycle fixture adds one negative case.

The independent generic nominal layout owner grows from 18 to 21 cases and
passes with result `42`. The generic-WIR compiler fixture is deterministic at
1,210,339 bytes and SHA-256
`8587efd5dd86073ed0cdfe3c9a134bcb91c2b8316e82f9abfa800622c7530340`.
The Language owner grows from 219 to 252 cases, and the 109-owner registry grows
from 4,966 to 5,002 cases at SHA-256
`75649a099553cb3d11037dcfd83f9e36c464aa59a04307ab67187f8ff77dba9a`.
The coordinated Language owner passes all 252 cases in 587,790 milliseconds,
or 588,290 milliseconds including coordinator overhead.

This closes applied generic fields in generic record and variant declarations;
generic variant value construction, deeper nested generic-function formal
patterns, broader constant-expression evaluation, collection implementation,
and the remaining Foundation-special plan remain explicit Slice 4 work. The
active Windvale-written compiler continues toward Compiler 1.0; the Seed name
describes the shrinking source subset, while immutable Stage 0 remains recovery
provenance rather than a maintained parallel compiler.

## Slice 4 generic nominal variant execution checkpoint

Decision 0818 connects general generic variants to ordinary source construction,
matching, WIR validation, WVB materialization, verification, and execution.
Construction spells the applied owner after the selected case, for example
`Outcome.Value<Point> { Item: Value, Attempts: Count }`. A match arm remains
`case Outcome.Value { ... }`: the selector's exact `Outcome<Point>` WVGT shape is
the single specialization authority. Decision 0815's accepted construction
grammar already admits this form, so the frozen grammar and its 251-input
identity do not change.

The implementation shares nominal field construction, consumes immutable
validated generic layouts, and avoids rebuilding record or variant plans in the
successful hot path. Generic construction uses private `Variantˉcreate = 65`;
matching uses private `Variantˉcase = 66` and `Variantˉfield = 164`. Each named
binding receives its substituted concrete shape. WVB maps all three targets to
the ordinary materialized `__WvY` type, leaving no template, argument vector,
private shape, or runtime-generic lookup.

The active split route produces a 1,062,350-byte analyzer with 522 functions and
869,515 code bytes at SHA-256
`518f8a08a67a83f3338ff9bd5994afdc616c7750e6ab77e077d3f8bd32f16666`.
Its eight-fragment Windows application is 33,452,032 bytes at SHA-256
`d73c61b713067180029edf0112e6beddf02be35ffce58a03c9dce7da583ebbd5`,
102,400 bytes below the unchanged 32 MiB product payload limit. The connected
990,701-byte emitter contains 542 functions and 821,031 code bytes at SHA-256
`05d130390f82b07a5d760f503fcc97b5157c0d5ed343d57bc8e04eb4bf4c1665`;
its six-fragment Windows application is 21,803,520 bytes at SHA-256
`31e0f1828f14cf4143ef9bafa68cdedb37c4b164d5793321b0976aad7afe62c4`.

`Generic-Nominal-Variant.wv` publishes exact 771-byte WVSS, 104-byte WVCA,
316-byte WVLB 1.3, 1,828-byte WVIR 1.3, and 947-byte WVB 1.16 products. Their
source, WVB, and complete artifact-family identities are independently pinned.
The 94-case inspector proves one `Outcome<Point>` WVGT entry, exact substituted
`Item`, `Attempts`, and `Code` bindings, the private operations, ordinary
multi-field materialization, case/field order, and template removal. The current
source-built verifier accepts the WVB and the runner returns `42`. Three
companion fixtures reject construction type mismatch, a missing construction
field, and pattern type mismatch with exact one-line diagnostics and no partial
downstream products.

The generic-WIR compiler fixture is deterministic at 1,217,428 bytes and
SHA-256
`e7e991248d658e7b6551137f75f3782c4b977c8d9be35c158a26d5c815035b2a`.
The Language owner grows from 252 to 349 registered cases, including 97 for this
checkpoint, and source fixtures grow from 74 to 78. Its complete
current-Windows run passes in 582,970 milliseconds, or 583,670 milliseconds
including coordinator overhead. The 109-owner registry grows
from 5,002 to 5,099 cases at SHA-256
`1db0080019c37a83b25a025c4171fd902a5a28dba6012b14803f30ca5208ff83`.
The directly changed generic type-binding, layout, materialization, and WVLB
carrier owners pass 18, 21, 30, and 20 cases respectively, each with result
`42`.
These are focused current-Windows development results; paired-host conformance
and release qualification remain independent gates.

This closes ordinary construction and exhaustive named matching for general
generic variants. Deeper generic-function formal patterns, broader constant
expressions, collection implementation, and the remaining Foundation-special
plan remain explicit Language 1.0 migration work.

## Slice 4 Foundation generic unification checkpoint

Decision 0819 completes the Foundation migration rather than preserving the
temporary specialization described by earlier checkpoints. `Option<T>` and
`Result<T, E>` now bind into ordinary WVGT, materialize beside user-defined
generic variants, and serialize only as `__WvY` Types entries. The packed
Foundation shape ranges, first-use plan, `__WvZ` suffix, and their dedicated
WIR/WVB validation branches are deleted.

`try` remains exact Foundation Result semantics, but it now inspects the
ordinary materialized layout, requires matching error shapes, and selects the
general variant fields. Foundation constructors require explicit complete type
arguments like every other generic nominal constructor. One new rejection
proves the retired inferred spelling publishes no WVB; the wrong-error fixture
uses explicit construction so it continues to isolate `try` compatibility.
The frozen grammar and its 251-input identity do not change.

The clean analyzer publishes exact 3,233-byte WVSS, 104-byte WVCA, 1,796-byte
WVLB, and 5,144-byte WVIR products for the complete Foundation fixture. The clean
emitter publishes a 3,143-byte WVB 1.16 at SHA-256
`fb3d07717252b60dcbcd6da1a95dbf6bccb8b85ba79d3a08c5e0e6306b722a81`.
It contains `__WvY`, contains no `__WvZ`, and returns `42` in 360 scalar-runner
instructions.

The optimized emission compiler falls from 542 to 530 functions and from
987,682 to 974,837 WVB bytes. Its Windows development package falls from
21,718,016 to 21,490,688 bytes while preserving the unchanged 128-type native
profile. At that Foundation checkpoint, the analyzer is 1,055,866 bytes at SHA-256
`2edf577a8b549fff0f351264e814e25783011a94942c904780a57be6ec1194b7`,
and the current fixed-array emitter is 998,402 bytes at SHA-256
`53b22d621cd3d169a69deb99bed0c4c5f9f1a15c11bac189076916625cef9743`.
The compiler-scale Generic-WIR fixture builds twice to identical 1,236,227-byte
WVBs at SHA-256
`37f6a8eeefb522e18685e3d96cfc9b27ee77e07698cd77f184cbb38280d59868`.

The segmented-toolset owner now stages the immutable 992,412-byte bootstrap
analyzer instead of asking the recovery Seed to rebuild evolving compiler
source. Its four cases still prove exact toolset reconstruction and
compiler-scale staging while avoiding a duplicate, architecturally obsolete
compiler build. The materialization owner retains 28 general-plan cases after
deleting the two grouped side-plan cases. The Language owner grows to 350 cases
and 79 source fixtures, including six Foundation cases. It subsumes the retired
five-case partial monolithic compiler-source sentinel: current compiler
reconstruction, deterministic compilation, verification, and execution remain
in one owner. The 108-owner registry totals 5,093 cases at SHA-256
`30f7a2130f41b18e5b4ca38e46775bb0ca4cbaef8add0cdf77e06589f4c660de`.
The complete changed-file plan selects 11 focused owners with no gaps and passes
all 31 general and 192 native routing cases.

This closes the remaining Foundation-special plan. Deeper generic-function
formal patterns, broader constant expressions, collection implementation,
paired-host conformance, and release qualification remain independent work.

## Slice 4 fixed-array and WVB 1.17 checkpoint

Decision 0821 implements the numeric/graphics workload's contextual fixed-array
surface without changing the frozen grammar. `Foundationˉcollections` owns
the canonical intrinsic `Array<T, N>` identity. The compiler requires an exact
expected type, exact `T` elements, and exact `u64` `N`; types admit zero through
4,095 while one literal is bounded to 64 element expressions by the existing
parser/WIR item limit. Construction evaluates left to right once, and indexing
uses a checked `u64`.

The internal generic catalog uses kind `10`. WVIR operations `165` and `166`
carry fixed-array construction and element access. WVB 1.17 adds Types kind `4`,
shape `22`, `C5` construction, and `C6` access. The concrete
`Collections.Array<i32, 3u64>` fixture publishes a 375-byte WVB at SHA-256
`e2125aba54aca71af5d10a6c7c4228460f2de28230503ad61b0b2877e8b593a7`.
Its function has a declared maximum stack of three, one exact kind-4
`Array<i32, 3>` descriptor, one `C5`, and one `C6`.

The current source-built verifier WVB is 205,363 bytes at SHA-256
`c1befdbebd700504192fb305492f080709885335617855c0897c953cfa3fade6`;
its Windows profile-2 application is 1,719,808 bytes at SHA-256
`a45a46a8e33d74e1278c53579c26461f1f2e010029cf4af754729644abf6d545`.
It accepts the exact fixture. The current source-built runner is 205,945 WVB
bytes at SHA-256
`04782c2fe6897c0678e6c7b9b57dbef3f87f58f1f10899915855c80be7f8f75f`;
its Windows profile-5 application is 2,052,096 bytes at SHA-256
`72d9e2ecb30943d8bcdde7b351f539ffc41c383bd91f10a202c1934240acd7cf`.
Execution reports `Result: 42`.

Six focused WVB cases prove exact verification/execution, deterministic
out-of-bounds code `3008` (`WVR3008`), rejection when the same extension is
relabeled 1.16, rejection of count 4,096, and rejection of an unknown `C5` type
index. The runner stores array cells in its existing traced 768-cell immutable
aggregate arena and now follows nested array/record/variant descriptors during
collection. The 4,095 serialized length is a format/type bound; the fixed arena
is an explicit consumer resource bound rather than hidden source capacity.

Compiler packaging now admits a bounded 64 MiB hosted product split across at
most sixteen fragments. The current analyzer is 1,077,512 WVB bytes at SHA-256
`9fa2a7a7b37329b399252eaa353a43599bd393f2c29dd1deb351b2bf1b512068`.
Its current Windows application is 33,997,312
bytes at SHA-256
`87ba2718b9f219a69f9e102045bcbb3331c37c96f1923eb605652fc9e0896e4f`;
the current emitter application is 21,970,432 bytes at SHA-256
`5224d55da8b201515dc7f15394cc3e7b21950a90242d2157b85d78f55241cfc1`.
The analyzer requires profile 7's 80-billion-instruction packaging envelope;
profile 6 stops normally at its lower 64-billion bound.

The split cache also advances to version-3 families and orders bounded
dependency snapshots by declared module identity rather than filename. Its
two-case focused test proves semantic ordering and forced-failure temporary
cleanup. Rebuilding the runner through the corrected family produces the exact
same 205,945 WVB bytes.

The complete current-Windows Language owner passes all 356 declared cases. The
108-owner registry contains 5,114 declared cases at SHA-256
`c57a0d8bca9f940392a192aff978f7716cbc1356c36d0a36e3d61c280fc1674e`;
changed-file planning has no uncovered paths and its regression suite passes 31
general plus 193 native routing cases.

This remains current-Windows development evidence. Direct native array
lowering, browser and Windvale OS consumers, paired-host conformance, and
release qualification remain separate gates.

## Borrow parser and call-scoped semantic checkpoint

Decision 0822 implements the frozen `borrow` and `mut` keywords without changing
the Language 1.0 grammar. Declaration parsing accepts `borrow T`, `borrow mut T`,
and borrowed results while preserving the exact underlying type location. Body
parsing represents immutable and mutable borrow unary expressions and rejects a
bare `mut`. The focused parser owner adds 12 cases and still returns `42`.

Source Symbols and WVLB bind the underlying shape and retain their serialized
formats. Typed WIR rereads the validated signature, keeps parameter and result
modes only as compiler facts, and erases them before publication. Exact borrow
modes must agree at a call. A borrowed actual may satisfy a by-value position
only for a conservatively proven Copy or shared immutable shape. `borrow mut`
requires a direct `var` or a parameter already declared `borrow mut`.
Standalone borrow storage and all borrowed results currently report
`Invalidˉborrow`; one-owner result provenance remains later work.

The executable six-function fixture publishes an 857-byte WVB at SHA-256
`deef20a9559e7930d37eb62d973e2e95a4e0e328d8dfdb0837321d389985ed69`.
The compiler-aligned verifier accepts it and execution reports `Result: 42`.
Six separate malformed semantic fixtures reject omitted explicit borrowing,
immutable-to-mutable use, mutable borrowing from `let`, local borrow escape, and
borrowed return, plus owned by-value read-through from a borrowed parameter,
with the exact `Invalidˉborrow` status.

The current analyzer is 1,088,695 WVB bytes at SHA-256
`4b5692c0caa9b53126b5461cc1c09fedcd7a716d4ed7f14f28abc9d80248ce58`;
its Windows development application is 34,402,816 bytes at SHA-256
`e9e8de91115175ebe0e7e081e4dac97adb2a46b00031d9d975f6c95f24230763`.
The current emitter is 1,002,147 WVB bytes at SHA-256
`5601ff3d80f8babcc8ef3ecd5615e56729d4905ae7884606b61270d0efc3ecdc`;
its Windows development application is 22,080,512 bytes at SHA-256
`b15dda33d7312c4f68c321e92877a34a0e32e9be30f5e1ed01a206529b6eebd8`.
The emitter remains exactly at the native lowerer's 128-type bound. Two
implementation-only enums initially raised it to 130 and caused
`Unsupportedˉmodule`; representing those modes as named bounded integers
restored bootstrap compatibility without changing source semantics.

Direct borrowed-parameter call arguments recover their mode from the existing
binding declaration offset. An initial implementation reparsed the complete
current signature at each such call and failed the large emitter self-hosting
path; the bounded declaration-offset lookup keeps the common path local and the
complete 1,940,645-byte emitter source set now publishes analysis successfully.

The verification registry now contains 108 owners and 5,133 declared cases at
SHA-256
`37b044d13ba09b34e9cc4d38dbf7e41fb190b84e773579af47352598fa921737`.
The seven semantic cases are registered in the cross-host Language 1.0 front door,
and changed-file planning passes 31 general plus 194 native routing cases. The
borrow parser fixture selects only `generic-nominal-type-binding`; the semantic
fixtures select only `language-1-front-door`. This section records focused
current-Windows evidence only. The complete front door, paired-host conformance,
full ownership checker, and release qualification remain separate gates.

## Slice 4 Vector and Sequence type-identity checkpoint

Decision 0823 admits the canonical edition-1
`Foundationˉcollections.Vector<T>` and `Foundationˉcollections.Sequence<T>`
type identities without changing the frozen grammar or disguising the legacy
fixed-capacity `builder<T, N>` and `sequence<T, N>` types as the Language 1.0
collections. The compiler binds only the exact Foundation module identities,
requires one type argument, retains bounded structural evidence for reuse, and
supports nesting with the already implemented generic nominal types.

Layout and private-shape materialization recognize the two new compiler-supplied
intrinsics as fieldless identities. Publication deliberately remains closed:
the WVB planner rejects kinds 11 and 12 until WVB 1.18 defines their dynamic
descriptor and the runtime implements construction, append, freeze, length, and
indexed-borrow semantics. This checkpoint therefore proves type identity only;
it does not claim executable collection operations.

The focused `generic-nominal-type-binding` owner now reports 59 cases. Its
765,440-byte WVB has SHA-256
`6c65821f1303782b820e87a191cc94c69dccf7529d76ae5498b24595a5c226b3`,
and its 18,334,720-byte Windows development application has SHA-256
`a6d38b676e6856a584ccecc50e9e36cd66dc96333493431586b44254838d1d9e`;
the application exits with the expected result 42. The verification registry
contains 108 owners and 5,147 declared cases at SHA-256
`1e8b3cd06dd7038d2ec55607386bb7fabf1a1c90c8dd407ab966b1c96619856f`.
Changed-file verification planning passes 31 general and 194 native routing
cases against that identity.

`Source-Wir-Core.wv` is 12,177 lines at this checkpoint. The type-identity work
adds no lines to it. Runtime-backed collection representation and helpers will
live in a focused acyclic compiler module, preserving Source WIR as orchestration;
any later extraction from the large core will follow a cohesive ownership
boundary rather than create numbered or mechanically split fragments.

## Slice 4 Vector and Sequence function-value checkpoint

Decision 0824 connects the canonical Vector and Sequence identities to ordinary
function signatures and call ownership. The early Source Symbols phase admits
only an exact imported `Foundationˉcollections.Vector<T>` or `Sequence<T>`
type and defers its shape. Source WIR then resolves that type through the same
bounded WVGT catalog that owns its kind, element, identity, and dependency
evidence. No private numeric range is treated as sufficient proof.

Call checking classifies kind 11 Vector as move-owned and kind 12 Sequence as
shared immutable. The focused `Borrow-Sequence-Read-Through.wv` source publishes
valid symbol, binding, and 424-byte WVIR evidence when a borrowed sequence is
passed through a by-value read position. The equivalent
`Borrow-Vector-Owned-Read-Through.wv` source publishes valid symbol and binding
evidence, then fails exactly as `Invalidˉborrow` without a WVIR artifact. These
two cases extend the Language 1.0 front-door owner from 363 to 365 cases and its
borrow group from seven to nine.

The current analyzer is 1,100,197 WVB bytes at SHA-256
`f678904797a4b81f621a457f33dc57d83403c3f57273935ebe773b7e1ec3b3f3`;
its 34,632,192-byte Windows development application has SHA-256
`f0f1d776b502600b69e415f1772bc4f7210fd19f25731619ae74600690fe5d8b`.
The registry contains 108 owners and 5,149 declared cases at SHA-256
`7506d35f266b8dfacf5288685dbada8468f34731046bdf289269f1aa975f88e9`,
and changed-file planning passes 31 general plus 194 native routing cases.

`Source-Wir-Core.wv` is 12,198 lines at this checkpoint. Its changes are limited
to the ownership classifier and signature-binding calls. A safe later refactor
will extract the cohesive collection expression-lowering and validation
pipeline with its callers; a helper-only or numbered mechanical split is not a
stable module boundary. WVB 1.18 representation and collection construction,
append, freeze, length, indexed borrow, and runtime execution remain the next
connected Slice 4 work.

## Slice 4 Vector and Sequence WVB 1.18 type checkpoint

Decision 0825 gives the already admitted collection identities an exact
portable representation without reusing the obsolete fixed-capacity
collection types. WVB 1.18 Types kind `5` represents
`Foundationˉcollections.Vector<T>` and kind `6` represents `Sequence<T>`.
Each descriptor contains its canonical private name and exact element shape;
neither contains a source maximum, backing capacity, allocator, or authority.
Function and other value metadata use shapes `23` and `24` followed by the
matching Types index.

The exported-signature fixture publishes a deterministic 436-byte WVB at
SHA-256
`c51529baa7fb7b5cfb24e2508520044cce9f2661b9fb1dccb2321b5e122ec73d`.
Its two descriptors retain exact `i32` elements, its Vector and Sequence
parameters point to different matching kinds, and its independent `Main`
returns `42`. The focused six-case test proves compiler-aligned verification,
that scalar execution, and exact rejection of a pre-1.18 header, invalid
element shape, descriptor-kind confusion, and Types-index confusion. This is
metadata support only; no collection value is allocated or executed.

The current verifier is 222,399 WVB bytes at SHA-256
`9424d62eba7f5efb37363bcef439afeb198c943a1439703bb3492378310a24d0`;
its Windows profile-2 application is 1,827,840 bytes. The current scalar runner
is 209,917 WVB bytes at SHA-256
`62c9a42433e4e14a984fd42a9ce4db6c6d303677a09de21849b4418952cf5215`;
its Windows profile-5 application is 2,077,184 bytes. The verifier's structural
type scan and type-range discovery now have focused helpers, while executable
shape encoding has its own helper so the native function remains below the
fixed frame-local bound.

Prepared emission is now separate from direct compilation and source-profile
admission. Removing the direct/profile dependency from the emitter leaves its
1,899,183-byte source set at 3,780,080 WVIR bytes, safely below the fixed 4 MiB
bootstrap product limit. The current 1,013,482-byte emitter WVB has SHA-256
`3fb526c3298406a3ba71df5e074d58d000532e80640421fc4d665389d7a0ea0d`;
its profile-7 Windows application is 22,320,128 bytes. The current analyzer
publishes 3,287,604 WVIR bytes and a 1,098,751-byte WVB at SHA-256
`4e24d6312b01efbd8caeb155ed1a0ce4339f4debe3cf2d77e300798e11ccd68b`;
its profile-7 application is 34,622,976 bytes. Rebuilding both products through
that pair is byte-identical.

The lexer also uses one bounded two-limb decimal accumulator for every integer
literal, removing the duplicate Foundation decimal-parsing module from compiler
closures without changing the literal contract. `Source-Wir-Core.wv` remains a
large orchestration module, but this checkpoint keeps serialization and runtime
metadata changes outside it. Collection construction, append, freeze, length,
indexed borrow, ownership execution, allocation limits, and reclamation remain
the next connected Slice 4 work.

The 108-owner verification registry now declares 5,155 cases at SHA-256
`7f102e24a7035aab8c0c7c135e9df44bfc864fc2e772e66fe8f85ad1108afc72`.
The dual-host Language 1.0 owner declares 371 cases, including six exact WVB
1.18 type-representation cases, and both host scripts share the nine-case
Vector/Sequence-aware borrow checkpoint.

## Slice 4 Vector and Sequence WVB 1.19 runtime checkpoint

Decision 0826 gives the WVB 1.18 identities their first owned executable
backing without reviving exact-capacity source types. WVB 1.19 adds reserved
construction, append after a proved capacity check, consuming freeze,
Vector/Sequence length, and checked Sequence element access. The independent
verifier requires exact kind-5/kind-6 Types indices and exact scalar element
shapes through every stack transition.

The scalar runner stores one descriptor per value in its existing reclaiming
64 KiB heap. A backing contains current length, retained maximum, and fixed
eight-byte scalar cells. The current profile admits at most 2,047 cells, or 16
KiB, for one backing. Local loads retain the allocation; stores release the
replaced owner; length and element operations preserve their collection owner
on the operand stack; explicit pop and function teardown release owned
descriptors. Freeze shares the backing without allocation and prevents later
mutation through the verified Sequence shape.

The deterministic runtime fixture is 971 bytes at SHA-256
`14c8f442c499669139b5106d62bf4687450a6b4537b5e224f637fbecc4ada251`.
It executes all six operations across six 16-KiB allocation cycles and returns
`42`; completing those cycles requires inactive allocation metadata and heap
spans to be reused. Four version/type corruptions reject semantically, one
ordinary Vector-local copy rejects during typed execution, and two otherwise
valid modules fail capacity or index access exactly as `WVR3008`.

The current verifier is 232,414 WVB bytes at SHA-256
`27941493d2c818d67da8cffbcb686de32517ac46a2a659b3a5e5884e2d59fb7e`.
The current runner is 226,540 WVB bytes at SHA-256
`a3b63a20d7a360889477346d970490c2f1139be8687add203955271844bc92f9`.
The runner's collection operations live in a new focused module, and backing
initialization grows its zero-cell block logarithmically. `Source-Wir-Core.wv`
is unchanged; its size does not impede this runtime slice.

The verifier carries one non-serialized linear Vector marker from reserved
construction through append and observation into consuming freeze. The runner
mirrors it with bounded stack flags. This prevents ordinary local loading from
creating a second mutable owner without adding copy-on-write allocation to the
hot append path.

This is a low-level backend checkpoint, not the complete Foundation contract.
Source `Memoryˉbudget` construction and `Result` lowering, recoverable append,
borrowed indexed access, compiler opcode selection, non-scalar elements, native
lowering, and WebAssembly qualification remain explicit next work. The
Language 1.0 owner advances from 371 to 380 cases.

The focused WVB 1.19 build, package, verification, execution, corruption, and
terminal-failure evidence passes all nine cases. Changed-file Development
verification also passes planning and the `seed` and `seed-native-front-door`
owners, then stops in the pre-existing WVB-to-WVO reconstruction identity:
the retained lowerer candidate was last refreshed under the floating-point
checkpoint, while `Native-X64-Lowering-Core.wv` changed later for fixed arrays.
The reconstruction still produces matching sizes and exact Return-42 and
metadata results, but its lowerer and hosted-package hashes no longer match the
retained candidate. That independent native-candidate promotion is not claimed
or repinned by this scalar-runner checkpoint.

## Slice 4/5 owned Vector local transfer checkpoint

Decision 0827 advances the collection backend to WVB 1.20 with `CD`
`local.take`. The instruction transfers one available exact Vector descriptor
from a non-parameter local to the operand stack, zeros the source cell, and
preserves the allocation reference count. Parameter transfer waits for the
move-at-call contract. This bridges Slice 4 collection
lowering to Slice 5 ownership without pretending that ordinary retaining
`local.load` can recover a mutable owner.

The verifier performs bounded definite-availability analysis only in functions
that use the new operation. Vector parameters are initialized for shared loads
but cannot be taken; locals begin unavailable, unique stores restore
availability, loads preserve it, takes consume it, and forward joins intersect
it. The initial profile admits at most 64 Vector slots and 4,096 instructions
and rejects backward control flow until loop ownership fixed points are
implemented.

The current verifier is 239,824 WVB bytes at SHA-256
`bfb60c8f80856c15399b457ab8c471e0e600492e0ffc39d34a718d0cb45e0a5b`.
The current runner is 228,106 WVB bytes at SHA-256
`63b8c862372e619bc9472d85ce850e7d621ed2106950b3e2ddaf801eaa6c78ee`.
The deterministic 1,156-byte WVB 1.20 fixture has SHA-256
`baa69aadf3b9c65900110d9aa3372989e051045e30207a87b720dbc0a663dd25`.
It transfers the Vector before each append and freeze across six 16-KiB
allocation cycles and returns `42`.

All 12 focused cases pass. In addition to the WVB 1.19 evidence, an
out-of-range local index rejects semantically, while an uninitialized take and
a repeated take reject during control/ownership analysis. WVB 1.19 execution
remains accepted. The Language 1.0 owner advances from 380 to 383 cases; the
three additions are malformed ownership-flow cases, not repeated broad builds.

The 108-owner registry now declares 5,167 cases at SHA-256
`40e0baf6e1db78464fd72313e22a05e8a9df065e18128ec26c269f5be239b085`.

`Source-Wir-Core.wv` remains unchanged. Its size is a future cohesion/refactor
signal, but it is not on this runtime/verifier path and does not impede the
checkpoint. Source/WVIR selection, public fallible `Memoryˉbudget`
construction, recoverable append, loop ownership, and borrow lowering remain
the next connected work.

Changed-file planning has no coverage gaps, and its 31 general plus 194 native
routing cases pass after synchronizing the verification registry identity. The
`seed` owner passes 26 cases and `seed-native-front-door` passes its focused
case. The broader gate then reaches the retained WVB-to-WVO reconstruction and
stops after three passing checks at the pre-existing lowerer-candidate identity
drift documented by the WVB 1.19 checkpoint. A direct Language 1.0 retry passes
frozen-fixture and descriptor phases, then the retained pre-split `Run-Wvb`
candidate rejects the existing generic-calls value-front-end fixture before the
owner builds the changed split-compiler runner. Neither broader failure reaches
or contradicts the passing WVB 1.20 verifier/runtime oracle; no qualification,
candidate promotion, or cross-host conformance is claimed here.

## Slice 4 exact Foundation Sequence read checkpoint

Decision 0828 connects the public immutable Sequence read surface to the
already verified WVB 1.19 runtime operations. Only a qualified alias whose
target module header is exactly `Foundationˉcollections` can select
`Sequenceˉlength` or `Sequenceˉat`; a fake module with the same member spelling
is rejected even when its argument is a real Foundation Sequence.

The compiler infers `T` from validated WVGT kind 12, accepts only the
resource-free Copy scalar subset, and lowers the Foundation borrowed element
result as its equivalent copied scalar value. WVIR operations 167 and 168 retain
the exact private Sequence target. WVB emission writes `CB` and `CC` with the
planned kind-6 Types index, stores each scalar result, and then pops the
preserved shared owner.

The focused source fixture publishes a 472-byte WVB 1.19 module at SHA-256
`8f8cb926df946bff3b254b37304ac7cf8ffa744ccea963703cfcfebfdf7e1831`.
The current verifier accepts it and the runner returns 42. Four byte-level
corruptions reject the version, both Types immediates, and descriptor kind;
four source cases reject the owner, index, element, and lookalike boundaries.
These ten cases join the existing Vector/Sequence phase without repeating the
heavy compiler bootstrap.

The 108-owner registry now declares 5,177 cases at SHA-256
`2c4b82a7381d33509a64d1bd0ff057c7871408d2478ae5ff7326e7cb78602ea5`.

The split compiler independently accepts the complete 1.64-MiB WVIR compiler
source and its 242,736-byte binding evidence, then emits a 1,278,211-byte
compiler module. The complete source-to-WVB closure likewise analyzes and emits
successfully. The older monolithic builder still reports its retained
order-sensitive post-binding failure at the total function count; that result
does not contradict the accepted split-compiler evidence.

The current split analyzer is 1,104,336 WVB bytes at SHA-256
`55c08703e4b4a93904e21ec82a9305adcf895290f6540c55262b115c69565b97`
and packages as a 34,725,376-byte Windows application. The current target-aware
emitter is 1,019,952 WVB bytes at SHA-256
`9d53ba13e68c186a0092a2f77c6fc22071b128dc6c629d5f010a7a7b8ab1bdc3`
and packages as a 22,391,808-byte Windows application. The front-door scripts
pin those exact current WVB identities before using either product.

`Source-Wir-Core.wv` is 12,343 lines after this checkpoint. The size is not a
compilation or verification blocker, but it is a maintainability signal. The
later refactor boundary should own collection call signatures, lowering, and
validation together; a numbered or helper-only mechanical split would obscure
the invariants and is not recommended.

During host reconstruction, Decision 0829 also corrected an independent WVB
1.18 regression in the profile-admission closure. The admission source had
been changed from the portable Foundation SHA-256 function to VM opcode `7D`,
which the current native x64 backend does not lower. The portable function is
now retained in all affected compiler project closures. The current admission
product is 82,924 WVB bytes at SHA-256
`7a7da249ff51647e2c279a9d06c05897f071683991aca0748ad6f40e02887512`;
its one-fragment Windows package is 797,184 bytes at SHA-256
`8307a87aa7f70cc9519ade98140554db9e5b6de834d39c86149ec8441624b8d6`.
That freshly reconstructed tool admits the exact Sequence fixture used by this
checkpoint. No native opcode or packaging bound changed.

## Slice 4/5 exact Foundation Vector read and freeze checkpoint

Decision 0830 connects `Vectorˉlength` and `Vectorˉfreeze` to the exact
`Foundationˉcollections` owner. Length requires an explicit immutable borrow of
a direct owned non-parameter local and preserves it. Freeze requires that local
as a value, consumes it, and returns the exact same-element Sequence declared by
the enclosing function. Parameters, mutable borrow, borrowed freeze, indirect
expressions, resource-bearing elements, and use after freeze reject.

The implementation adds WVIR operations 169 and 170. Their target is the source
local slot, their auxiliary shape is the validated WVGT Vector identity, and
freeze's result is the validated WVGT Sequence identity. Independent WVIR
validation reconstructs both element shapes. The fixture exposed and fixed a
general catalog-admission omission: the optimized signature pre-scan considered
only user-declared generic nominals, so a compiler-supplied collection used only
as a function return could receive a private shape without a retained WVGT
entry. The pre-scan now also activates for the exact Foundation collections
module and admits parameter and return types before body lowering.

Emission uses WVB 1.20 `local.take` for Vector stores, Vector returns, and both
new operations. Length takes the owner, executes `CA`, stores the scalar result,
and restores the same unique Vector local. Freeze takes the owner and executes
`C9` without restoring it. The compiler-aligned verifier now requires a WVB 1.20
Vector-returning function to return unique evidence and models its call result
as unique; the runtime representation remains unchanged.

The positive two-module source fixture publishes a 288-byte WVLB, a 640-byte
WVIR, and a deterministic 546-byte WVB 1.20 module at SHA-256
`fc51afb9c7b8a17dd9fd044e971f22944e0d96ec872de910de3f0114d066e20f`.
The 240,230-byte verifier candidate at SHA-256
`ec612f4b1950121ce1c2d519472c24399a13975502bb690c49549d4c2460e833`
accepts it, and the unchanged 228,106-byte WVB 1.20 runner returns 42. Its
collection functions are retained as compiler/verifier lowering oracles but are
not called by `Main`; the existing direct WVB 1.20 fixture remains the runtime
oracle until fallible source Vector construction lands.

Eight harness cases cover the verifier, runner, old minor, shared-return and
shared-read substitutions, and three type-immediate corruptions. Five source
cases reject use after freeze, wrong borrow modes, parameters, and unsupported
elements. The 13 additions reuse the existing Vector/Sequence phase rather than
repeat compiler bootstrap, verifier construction, or runner construction.

The 108-owner verification registry now declares 5,190 cases at SHA-256
`e4e10295a6ebe799ebd86bbe649569bbda9bb7c8ee5371a370d3b5de81f84d66`.

The first source ownership profile is intentionally straight-line: one basic
block and at most one outstanding consumed Vector local per function.
Multiple outstanding moves, branch and loop fixed points, parameter transfer,
general expected-type propagation, fallible construction, recoverable append,
and non-scalar elements remain explicit next work.

`Source-Wir-Core.wv` is now a stronger cohesion signal, not an evidence-limit
failure. The maintained analyzer and emitter split projects still compile under
the fixed four-MiB WVIR product bound. The recommended later extraction owns
collection signatures, lowering, ownership validation, and dynamic WVIR checks
together; it must not create numbered fragments or hide cross-phase invariants.

## Slice 4/5 exact collection result-context checkpoint

Decision 0831 replaces freeze's enclosing-return special case with one bounded
contextual-call contract. An ordinary call can receive an exact shape from a
declared local initializer, assignment target, enclosing return, or parameter
of an already selected non-generic fixed-signature function. The context cannot
select a declaration, solve a generic argument, or infer an untyped local.

`Vectorˉfreeze` accepts only the canonical same-element
`Foundationˉcollections.Sequence<T>` context. The source compiler rejects a
missing inferred result, a mismatched declared Sequence element, and a nested
call whose selected parameter has the wrong Sequence element before WVIR
publication. Independent WVIR validation remains unchanged and reconstructs
the same Vector and Sequence element evidence from WVGT.

The expanded source fixture contains nine functions and publishes a 584-byte
WVLB, a 1,792-byte WVIR, and a deterministic 1,199-byte WVB 1.20 module at
SHA-256
`c73f2e77aa4208a74385046a27beba7dea42e4cece730bfd9ac0ac61ca7a77bc`.
The current verifier accepts it and the unchanged runner returns 42. Eleven
harness cases plus eight source rejections make the focused Vector read/freeze
phase 19 cases.

The exact current split analyzer is 1,112,436 WVB bytes at SHA-256
`d294003a2cb37c33475c384de71f34d95a18254d34116b8883c576ac416bbbcb`
and packages as a 35,059,200-byte Windows application at SHA-256
`f12e299b1c9787538b3c6bc41a2a1d0538c2eacebcd3b770849c630867c5b879`.
Its 1,673,348 source bytes produce 3,335,328 WVIR bytes and 910,794 code bytes,
remaining below the maintained four-MiB WVIR product bound.

The Language 1.0 owner advances from 406 to 412 cases. The 108-owner registry
now declares 5,196 cases at SHA-256
`b3db60ae871d308c36cf17cc02639efd3a8a7b6d4a107bafb646af1abe6e690c`.

`Source-Wir-Core.wv` is 12,611 lines. This checkpoint adds one coherent expected-
shape path; it does not make file length a semantic or evidence failure. A later
refactor should extract a named contextual-expression or collection-lowering
owner with an explicit phase contract, not mechanically divide the file.

The next connected checkpoint remains canonical `Memoryˉbudget` identity,
ownership, allocation effect, and launcher transfer before public fallible
Vector construction and recoverable append. No budget-free or unbounded
constructor is introduced here.

## Slice 4/5 canonical Memory budget identity checkpoint

Decision 0832 connects the exact qualified edition-1
`Foundationˉmemory.Memoryˉbudget` identity to source binding and WIR ownership.
The alias must target an edition-1 module whose exact header name is
`Foundationˉmemory`; unqualified use and the same member spelling in a different
module fail as `Unknownˉtype`. The Foundation source remains an intrinsic holder
and publishes no forgeable record or constructor. Project admission and its
source lock, not the header spelling alone, select the trusted dependency.

The compiler carries the opaque type as private fixed shape `805306368` with an
internal named/end sentinel. Function signatures may receive it, immutable
borrows may observe it, and WIR classifies it as owned. A borrowed budget cannot
satisfy a consuming by-value parameter and fails as `Invalidˉborrow`. Record
fields reject the shape until opaque storage and destruction are represented.
No new public declaration kind, WVIR operation, or language ABI number is added.

WVB emission explicitly returns
`source emission status=Valid analysis-status=Valid wvb-status=Unsupportedˉshape`
with no output module. Runtime representation, moves and destruction, launcher
or parent-domain transfer, `Split`, allocation effects, exact enum backing,
`Allocationˉfailure`, leases, provider accounting, and public fallible Vector
construction remain connected later checkpoints.

The maintained 992,412-byte analyzer producer compiles the current
1,114,218-byte analyzer at SHA-256
`640ba1a9714979927433fa4936c73fa164b83f33ad22c794e86092ee8e17faa8`.
That analyzer compiles the current 1,029,551-byte emitter at SHA-256
`a3bdffe028b2d4268358324a9b9a13aba2841730dd8f7334c4512a4f312827eb`.
The positive two-module fixture publishes 104 manifest bytes, 120 binding bytes,
and 424 WVIR bytes. Four negative/publication cases cover owned read-through,
unqualified and lookalike identity, and unsupported WVB output.

The focused Language 1.0 owner advances from 412 to 417 cases. The 108-owner
registry advances from 5,196 to 5,201 declared cases at SHA-256
`bdff820b2e13034763962928b1c162e22f9852102ccb60dd5bb04f525c4c173d`.
`Source-Wir-Core.wv` reaches 12,612 lines, but this checkpoint adds only its
fixed-shape validity and ownership checks. The later maintainability refactor
should extract a named phase with type/ownership or collection-lowering
invariants rather than numbered file fragments.

## Slice 4 exact enum-backing and bounded compiler-capacity checkpoint

Decision 0833 implements the frozen edition-1 enum header and tag rules. An enum
must name one of the eight fixed signed or unsigned integer backings. The lexer
retains an exact two-limb nonnegative magnitude; the symbol pass owns suffix,
sign, range, signed-minimum, and normalized duplicate checks. The three appended
statuses are `Missingˉenumˉbacking`, `Invalidˉenumˉbacking`, and
`Invalidˉenumˉvalue`; the retained `Duplicateˉenumˉmember` and
`Duplicateˉenumˉvalue` statuses continue to own uniqueness failures. Existing
descriptorless compiler source retains its
implicit nonnegative `i32` compatibility form without becoming Language 1.0
syntax.

WIR carries a nominal enum and its declaration-order member identity rather than
a possibly truncated numeric tag. The current WVB 1.20 output contract emits
only exact signed `i32` backing, including negative tags as canonical
two's-complement bits. Any enum with one of the other seven backings reaches
valid source/WIR analysis and then returns `Unsupportedˉshape` without output.
This is an explicit output-format boundary, not an implicit conversion.

The all-width fixture publishes 848 WVSS bytes, 96 WVLB bytes, and 544 WVIR
bytes before exercising that boundary. Five negative cases cover missing
backing, mismatched suffix, out-of-range magnitude, unsigned negative, and a
duplicate signed value. `Enum-I32-Negative-Main.wv` produces a 427-byte WVB;
the current compiler-aligned verifier accepts it and the runtime returns `42`.

`Foundationˉmemory` now uses the frozen declarations directly:
`Allocationˉreason: u8` has `Budgetˉexhausted = 1u8`,
`Targetˉunaddressable = 2u8`, `Providerˉunavailable = 3u8`, and
`Fragmented = 4u8`; `Allocationˉfailure` carries that reason plus exact
`Requestedˉbytes: u64` and `Availableˉbytes: u64`. Runtime allocation authority,
budget transfer, effects, and fallible Vector construction remain later
connected work.

The enlarged compiler exposed a bootstrap execution-capacity boundary. Exact
profile-7 probes exhausted instructions through 120,259,084,288 and reached
text-arena exhaustion at 124,554,051,584. Profile 7 therefore advances to the
finite `2^37` instruction limit and 224 MiB text/byte arena while retaining its
1 MiB name stride. Profiles 1 through 6 remain unchanged. This is a measured
migration unblocker; broad optimization remains deferred until the Language 1.0
compiler becomes the active seed.

The exact self-emission accepts a 1,953,683-byte compiler source set and produces
104 manifest bytes, 293,664 binding bytes, 3,902,856 WIR bytes, and a
1,046,456-byte WVB with 552 functions and 869,476 code bytes at SHA-256
`92fa90b0d942cbe5a74861af49f680efe3c69b19466a237893e21ad0dff3cd66`.
The independent native WVB verifier accepts it. The maintained analyzer is
1,132,570 bytes at SHA-256
`e3eef9e462f47cb88d4de174eb1e714106b346137538d9e6b396361b834d8471`
and packages to 35,597,824 Windows bytes at SHA-256
`21d6ace08354a2b4154d8356ca9255fd288d2ae5c7c7d0292b0c90538270705a`.
The emitter package is 22,945,280 bytes at SHA-256
`51980614da75ef5e8e33cdd33fef91fa0cf74d7ee02cf1d978e2e14cf05f3701`.

The profile geometry change rebuilds the complete 72-entry hosted-container
candidate. Its 6,927-byte inventory has SHA-256
`40af573f510861b375b1dac5216e5e622b6539656dfec188f6f4079f33040239`.
The publisher applications retain their immutable atomic-publication shell and
were reconstructed in an isolated, digest-verified checkout of
`stage0-recovery-e5a1a7473c57`; only binary products return to `main`. The
Language 1.0 owner advances from 417 to 427 cases, and the 108-owner registry
advances from 5,201 to 5,211 cases at SHA-256
`39df2841962a0efa20541c5b2b2ecf5e15ec514d709756107f8bd5c8c5ef899b`.
The accepted source identity advances without rewriting the prior freeze:
the 3,778-byte 0833 amendment manifest has SHA-256
`1a48d58136e5200cdb6f5ae1e15638f554854a3764f61ce1f0d2222d9d8e0c13`
and binds the same 251 inputs, now totaling 1,728,883 bytes at aggregate
entry-stream SHA-256
`a6e6bf3617049a987b545a78e5f3fcef28b24a3fc2b82c45d620e58baed73fc9`.
The generic-call parser self-test already exceeded the retained scalar runner's
fixed one-million guest-instruction budget before this slice. The owner keeps
that global bound and executes the same compiler-aligned WVB through the
existing bounded three-fragment profile-1 native route, which returns `42`.
The borrow-call fixture likewise retains independent WVB verification. Its WVB
1.20 ownership operations are newer than the last promoted native scalar-runner
application, so the owner executes it through the bounded import-free
WebAssembly scalar interpreter. The module returns `42` in 93 guest instructions.
The generic-WIR closure grows deterministically to 1,295,691 WVB bytes at
SHA-256 `6afc2f4574158d5b151c7d4c0ec85eca132e26f88187f8d5fda8b2c866be9e6b`;
two independent productions are byte-identical.

The settled Windows `language-1-front-door` owner passes all 427 declared cases
in 648,030 ms. Its long negative groups now publish bounded item progress. The
Windows batch sources retain required CRLF, and macron-bearing diagnostics use
exact one-line comparisons rather than code-page-sensitive `findstr` patterns.
The independent verification planner passes 31 general and 194 native routing
cases, including explicit coverage of this 0833 amendment manifest.

## Slice 5 launcher memory-budget transfer checkpoint

Decision 0834 advances the canonical
`Foundationˉmemory.Memoryˉbudget` identity across bytecode verification and
source-built execution without pretending that general resource operations are
complete. WVB 1.21 appends private shape byte `25`. Its only valid placement is
the sole parameter of exported `Main(Memoryˉbudget) -> i32`; it cannot appear in
another signature, local, temporary, aggregate, collection, operation, call,
constructor, move, load, store, or return.

At launch, the scalar runner creates a fresh opaque identity/generation token
and transfers it into the entry parameter. Completed top-level return verifies
the token, zeros the cell, releases the invocation ownership exactly once, and
then publishes the result. Failure tears down the invocation domain. Budget
capacity is deliberately unobservable until `Split`, allocation leases, and
exact provider accounting exist; this checkpoint adds no public constructor or
fallible collection API.

The exact `Memory-Budget-Entry-Main.wv` fixture emits a deterministic 242-byte
WVB with 16 code bytes at SHA-256
`499c59fa1207917fd64ee0703569d3dc4a80c5075fc99923e657adc5e4f9ed65`.
The compiler-aligned verifier accepts it and the source-built runner returns
`42`. An independent bounded verifier accepts the valid module and rejects
nine malformed variants covering version, parameter shape/count, entry name,
return and local placement, load, store, and a missing export.

One conservative emitter optimization removes a connected blocker. When a
complete WIR-closure scan sees no nominal use in signatures, locals,
temporaries, operations, aggregates, variants, or collections, optimized output
emits an empty Types section. If any nominal use exists, the complete table and
existing indices remain unchanged. The all-width declaration-only enum fixture
therefore emits a 217-byte executable and returns `42`; an actually used `u8`
enum still returns `Unsupportedˉshape`. This does not implement partial type
pruning or widened enum execution.

The maintained analyzer remains 1,132,570 bytes at SHA-256
`e3eef9e462f47cb88d4de174eb1e714106b346137538d9e6b396361b834d8471`.
The current emitter is 1,054,673 bytes at SHA-256
`2b5b4af681a36569b39be9dd46999af5b7babbc5cff53e6d3aec5227590a7e8b`.
Exact self-emission consumes 1,961,550 source bytes and produces 104 manifest
bytes, 294,832 binding bytes, 3,926,604 WIR bytes, 554 functions, and 876,881
code bytes. The independent native verifier accepts the resulting WVB. The
current source-built runner is 230,259 WVB bytes at SHA-256
`d1393ec3cb83d95cf86902768893846e4dc0e5a742b46363c86e19712ec674ba`.

The focused Language 1.0 owner advances from 427 to 440 cases. Its 12 budget
cases cover deterministic compilation, one valid and nine malformed verifier
cases, and runtime transfer/release; the added enum case preserves the used-u8
rejection. The 108-owner registry advances from 5,211 to 5,224 cases at
SHA-256
`78dcce3ba389c2e265c1601bbe32f84e873e8742c795ab1a243317013301b0db`.
This remains an early Slice 5 checkpoint. General moves and borrows, `using`,
reverse-order release, budget splitting/accounting, and one real hosted resource
consumer remain before Slice 5 is complete.

The final Windows child run completed all 13 phases and printed the exact
440-case passing summary in 727,310 ms. Its coordinator then rejected only the
registered terminal text because that registry still named the pre-optimization
498-byte generic-specializations artifact; the run had produced the correct
473-byte artifact above. The registry and coordinator digest were corrected to
the exact observed value. No compiler, verifier, runtime, fixture, or owner-child
input changed afterward, so the passing child work was retained rather than
rerun solely for terminal-string comparison; the independent frozen-input and
verification-plan contracts were rerun against the corrected metadata.

## Slice 5 exact `u8` enum representation checkpoint

Decision 0835 advances one actually used fixed-width enum backing through the
connected compiler, WVB verifier, and source-built runner. WVB 1.22 adds kind
`7` without changing historical kind-2 `i32` enum bytes. Its descriptor carries
the exact source backing identity `6` plus one byte per member value. Both kinds
share the ordinary enum value shape, instruction family, and one name-sorted
enum category.

`Enum-U8-Used-Main.wv` now emits a deterministic 415-byte WVB 1.22 at SHA-256
`961ba417955a523b9fc21e0b71df7a8d99613252b7450700dd4381aa94e825ed`.
The descriptor encodes `Deliveryˉstate`, `Pending = 1`, and `Complete = 2`;
the independent verifier accepts it and the source-built runner returns `42`.
Nine bounded mutations reject old/future version selection, wrong backing,
duplicate or truncated values, a missing kind-7 feature, an unknown type kind,
and an out-of-range enum shape index. Retained wider enum backings remain closed
without narrowing or partial output.

The current emitter is 1,055,285 bytes at SHA-256
`bd87930696685475920bdc73dcf72dde01ae0eb5dae94579e28b9a79d018d606`
with 554 functions and 877,444 code bytes. The current compiler-aligned verifier
is 248,741 bytes at SHA-256
`f401d89796c48b4d6890a465d6f47c1a21c864cb48383ce54c8ec9bc1a0c3e08`.

Adding the descriptor path exposed existing per-function bytecode and native
2,048-cell limits in the source-built runner. Focused helper extraction keeps
both bounds unchanged: the rebuilt 257,017-byte runner has SHA-256
`269130ea87bba7504af0d7d8337a7d1b8748d61671611ffb816d7ca5f7fa2e02`,
98 functions, and 232,834 code bytes, and lowers to a 2,842,043-byte native
object. This is measured migration-blocker removal, not the deferred broad
optimization of the transitional compiler.

The focused Language 1.0 owner advances from 440 to 449 cases. The 108-owner
registry advances from 5,224 to 5,233 declared cases at SHA-256
`e40651f750eddb420500561ad0969cec233261f2666c47f383e958e28744a5b8`.
The final Windows child passes all 13 phases and 449 cases in 745,820 ms; its
owner coordinator completes in 746,630 ms. Candidate toolset and paired-host
qualification remain pending. Budget splitting,
allocation leases/effects/accounting, public fallible Vector construction,
general moves, `using`, reverse-order release, and one hosted resource consumer
remain before Slice 5 is complete.

## Slice 5 memory-budget Split WVIR checkpoint

Decision 0836 binds the first exact budget-splitting boundary without claiming
an executable allocation provider. Edition-1 source resolves only canonical
`Foundationˉmemory.Split`; it requires `borrow mut` of a directly named mutable
`Memoryˉbudget` local, exact `u64` byte and `u32` child limits, and the canonical
materialized `Result<Memoryˉbudget, Allocationˉfailure>`. The failure identity
is structural as well as nominal: its three declaration-order fields must be
the same-module `Allocationˉreason`, `Requestedˉbytes: u64`, and
`Availableˉbytes: u64`.

Typed lowering publishes operation `171`. Its two operands are the evaluated
numeric limits, its target is the parent local slot, and its auxiliary value is
the Foundation memory module. Split selects WVIR 1.5 without specialization and
1.6 with specialization; programs without it retain their prior 1.3/1.4 header
and bytes. A conservative single-block affine proof tracks live budget slots
and moved temporaries, rejects duplicate ownership and use after move, and
requires temporary owners to be consumed. The proof is extracted behind one
focused function after the inline form reproducibly exceeded the current native
emitter's record-storage analysis boundary.

The positive three-module fixture publishes 568 WVIR bytes: five function
entries, two blocks, six operations, five temporaries, three operands, and one
operation 171. The current WVB emitter returns `Unsupportedˉshape` at function
2, operation 6 and publishes no bytecode. Seven byte-level mutations reject an
older minor, operation 172, a primitive result, missing operand, consumed parent
slot, wrong module, and swapped numeric operands. Four source cases reject an
immutable borrow, wrong limit width, wrong result identity, and a same-name
Foundation failure record whose `Availableˉbytes` is `u32`.

The source-built analyzer is 1,144,757 bytes at SHA-256
`384cb966d9b8718fda0c2e7bf3863ae168ce7d9fcb911d076b87d5e33400b0e3`,
with 549 functions and 938,146 code bytes. The source-built emitter is 1,078,300
bytes at SHA-256
`215034c1149ee898ae4a9980bbe82326cb0d2a82fe7939e6191af64972a9af50`,
with 561 functions and 897,085 code bytes. Both native packages complete under
the retained bounds.

The first complete owner run passed the 13 new cases, then exposed one stale
pre-Split compiler-generic-WIR golden. Its two outputs were already identical at
1,315,395 bytes and SHA-256
`1da34176e4e17f395fadccfff9fe4f7f5e346ec2c919658744915ca86b7d6c19`.
After updating that exact size/hash oracle, the current native verifier accepted
the artifact and the repeated focused owner passed all 13 phases and 462 cases
in 725,200 ms; the owner coordinator completed in 725,980 ms. The 108-owner
registry now declares 5,246 cases at SHA-256
`824e1b4fb800916b3f149c235e16d366c3213f25faa7f71fc35bce498b52fd18`.

This is an early Slice 5 compiler/WVIR checkpoint. Executable provider
debit/credit, failure atomicity, allocation leases and effects, control-flow
ownership joins, fallible Vector construction, `using`, reverse-order release,
and one hosted resource consumer remain. Broad optimization of the transitional
compiler remains deferred until Language 1.0 becomes the active seed; only
measured migration blockers and verification/caching costs are optimized during
the slices. Candidate toolset and paired-host qualification remain pending.

## Slice 5 bounded memory-budget accounting oracle

The next checkpoint implements the provider-independent accounting behavior as
portable Windvale source without yet making WVIR operation 171 executable. The
model has one root plus at most 64 child-domain slots. Its 2,616-byte internal
state consists of a 16-byte header and 65 fixed 40-byte entries. An 8-byte
identity/generation token owns one live entry. This byte layout is a bounded
implementation oracle for the current compiler and runner; it is not a public
serialized format or the normative representation of `Memoryˉbudget`.

`Splitˉmemoryˉbudget` validates the complete state and parent token before
mutation. Success atomically reserves the child's maximum bytes and one child
slot, advances the reused slot generation, and returns the child token. Byte or
per-parent child exhaustion returns allocation reason 1 with exact requested and
available bytes; global slot or generation exhaustion returns reason 3. Every
failure preserves the original state and publishes no token. The target-
unaddressable reason remains for the later physical allocation boundary rather
than being fabricated by pure accounting.

Release first removes the owner. A domain with live descendants remains active,
so its maximum stays reserved in its parent. Releasing the last descendant
finalizes the unowned ancestor chain and recursively credits exact maxima and
child counts. Stale tokens fail through generation comparison. Teardown removes
all owners and finalizes every domain under fixed pass, depth, and capacity
bounds. Complete validation rejects malformed headers, flags, parent chains,
cycles, overflow, reserved totals, and child counts before accepting state.
The straightforward validator is quadratic in the fixed capacity; broad tuning
is deferred until the real self-hosted compiler/runtime is profiled.

The self-test exercises 17 success, failure, reuse, malformed-input, and teardown
outcomes and returns 42. The deterministic WVB contains 34 functions, 21,533
code bytes, and 24,825 module bytes at SHA-256
`4d4214dd2e1ebf9b2864e1ef07d51dac48d569fc99b3989368b2a95e42c7d9b5`.
The Windows focused owner builds it twice, compares exact bytes, packages it as
a hosted native application, executes it, and passes in about 13 seconds. Its
Linux counterpart has the same bounded phases; cross-host execution remains
pending.

The accounting files route to their own focused owner rather than the 462-case
Language front door. The verification registry therefore advances from 108
owners and 5,246 cases to 109 owners and 5,263 cases at SHA-256
`826da92d26ba1e58be07cd99bf1b995a7d9331d8a23381fad4655a3095a8c846`.
This is a verification-feedback improvement worth retaining now; it does not
optimize compiler internals that may be replaced at self-hosting. WVB operation
171 connection, launcher profile selection, allocation leases/effects, and
public fallible Vector construction remain the next implementation checkpoints.

## Slice 5 executable memory-budget Split checkpoint

[Decision 0837](../Decisions/0837-Execute-Memory-Budget-Split-As-Wvb-1.23.md)
connects WVIR operation 171 to WVB 1.23 opcode `CE` and the bounded accounting
provider. The nine-byte instruction carries the mutable parent-local index and
the exact materialized Result type. It consumes `u64` maximum bytes followed by
`u32` maximum children, preserves the parent owner, and produces one affine
`Result<Memoryˉbudget, Allocationˉfailure>`.

WVB 1.23 permits shape `25` only in the launcher entry and non-parameter locals
of `Main`. `local.take` moves budget and exact Split-result owners without a
copy. The verifier recognizes the result by its exact machine layout rather
than compiler-generated private names. Its Valid case contains shape `25`; its
Failure case contains the exact record, `u8` reason enum, and two `u64` fields.

The source ownership proof now performs bounded forward-CFG dataflow across at
most 64 blocks and 64 owned slots. Incoming ownership is intersected at joins,
temporary budget owners must be consumed, and backward control remains closed.
This admits an ordinary Result match without introducing an unbounded loop
fixed point.

The success fixture deterministically emits 752-byte WVB 1.23 at SHA-256
`5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53`.
A second fixture requests 100,000 bytes from the reference runner's 98,304-byte
root and proves the typed failure branch. The current verifier accepts both,
nine version/opcode/local/type/layout mutations reject, and the source-built
runner returns 42 for both the successful and refused split.

The rebuilt analyzer remains 1,144,757 bytes at SHA-256
`384cb966d9b8718fda0c2e7bf3863ae168ce7d9fcb911d076b87d5e33400b0e3`.
The emitter is 1,084,963 bytes at SHA-256
`694aa254b7147f2964d7cab3f7dba96e1076509c8ec3c91768e3c529b2ae71a4`;
the verifier is 263,234 bytes at SHA-256
`5f8e8c93818bc64a1360e9b20d3893edddea3854b6d618d52d16bf3488bde468`;
and the runner is 282,833 bytes at SHA-256
`2e37fc47eb61b8420bc9d30d24385a9427815f55c735d76adaff51ebb68e0f95`.

The focused 15-case owner passes on Windows and advances the registry to 110
owners and 5,278 cases at SHA-256
`90cf308458315c105b3f735217a54bb9cc189d23099e9587b88d31998007178a`.
Its warm run shows compiler-product cache hits are quick while native packaging
still dominates elapsed time; that is a later measured workflow target. Broad
transitional-compiler tuning remains deferred until self-hosting. Allocation
leases/effects, public fallible Vector construction and recoverable append,
general owned calls, `using`, reverse release, and one hosted consumer remain.

## Slice 5 effect-clause front-end prerequisite

[Decision 0838](../Decisions/0838-Admit-Exact-Language-1.0-Effect-Clauses.md)
adds the source boundary needed before allocation can carry an honest
`memory.allocate` effect. The lexer appends token identity 102 for the exact
`effects` keyword. Edition 1 admits it while descriptorless Seed rejects it.
The declaration parser accepts the frozen empty, comma-separated, and
trailing-comma forms immediately after a function return type.

Effect identities are canonical lowercase ASCII segments. The implementation
bounds a clause to 32 identities, 16 segments per identity, 128 canonical
identity bytes, and 16,384 source bytes including exact lexer whitespace and
line-comment trivia. The function declaration record retains clause presence,
exact source offset/length, and identity count; an omitted local clause remains
distinguishable for later inference.

The first token-record parser made the correct grammar unnecessarily expensive
at its own declared maxima. The retained implementation scans canonical
identity bytes directly while reusing the lexer's whitespace, UTF-8 width, and
identifier-component primitives. This makes the 32-identity, 16-segment, and
128-byte boundary cases executable under the current bounded verifier without
raising a runtime limit. It is a durable blocker fix, not broad tuning of the
transitional compiler.

One hosted test WVB uses the bounded scripting profile to select exactly one
case per fresh execution. The focused owner builds twice, compares exact bytes,
validates the pinned host runner, and runs all 20 selectors. The 373,281-byte
module is byte-identical at SHA-256
`0a6e703cbb9b0536addaad8211c82d6d99ffddb9cf999d61ae6d45910b53153c`;
all Windows selectors return 42. The registry advances to 111 owners and 5,298
cases at SHA-256
`eb61fe17b976553df0e53564b625aa2b37f9ec802cc5772d7740b06eeb2eb7ed`.

The immutable Seed rejects the complete compiler project at its pre-existing
compiler-scale boundary (`Sourceˉbindings`, function sentinel 647, operation
zero); the unchanged parent revision rejects at sentinel 646. This is not an
effect-clause semantic failure. The maintained split analyzer accepts the
1,717,674-byte source set and publishes 251,336 binding bytes plus 3,433,904
WVIR bytes. Its paired emitter publishes the 612-function, 1,342,735-byte WVB
at SHA-256
`7f4d93fc7f427f5f9f75b4813baed92c00284c5d5ef4f5c996ca0fa0539dd69f`.
Compiler-scale development therefore continues through the split compiler
under Decision 0810 rather than widening or tuning the immutable recovery Seed.
Paired Linux execution remains pending.

This is deliberately not complete effect semantics. Canonical registry
resolution, explicit exported empty sets, local inference, call/capture
compatibility, function values, and WVIR/WVB/package evidence remain Slice 6
work. Slice 5 next connects the `memory.allocate` identity far enough to verify
allocation leases, then adds public fallible Vector construction and
recoverable append.

## Slice 5 `using` statement front-end checkpoint

[Decision 0839](../Decisions/0839-Admit-Exact-Language-1.0-Using-Statements.md)
admits the exact frozen `using Name = Expression Block` source shape without
claiming resource semantics. The lexer appends token identity 103. Edition 1
admits it while descriptorless Seed rejects it. The body parser appends statement
kind 14 and retains the binding-name, acquisition-expression, and complete body
spans in its existing flat immutable record.

The parser counts the acquisition and body descendants under the existing
4,096-statement, 4,096-node-per-expression, and depth-64 limits. It allocates no
child-statement collection. The checkpoint does not yet add the lexical binding
to semantic scope, classify an owned resource, prove moves or borrows, select a
release protocol, lower cleanup edges, or alter WVIR/WVB. Full Unicode project
identifier admission remains a separate normalized source-profile front-door
qualification; this direct canonical-parser owner covers the already implemented
ASCII/U+02C9 view without changing that frozen requirement.

The focused owner builds its hosted fixture twice, compares exact WVB bytes,
packages the module through the maintained segmented native path, and executes
18 cases in two bounded parallel batches. Cases cover the keyword, ordinary and
`try` acquisitions, nested scopes, comments and line tracking, retained spans
and counts, every missing structural component, unterminated input, edition
separation, contextual `usingx`, exact accepted/rejected nesting boundaries,
and macron-separated identifiers. The 378,739-byte module is byte-identical at
SHA-256
`cab55c5abbf301fe1a9dbafe6566444d7cba9aee6d1d1eddaf23d26d3406847e`;
all packaged Windows selectors return 42.

One rerun exposed a real Windows development-wrapper race: simultaneous
`Build-Wvb.cmd` processes can begin with identical `%RANDOM%` candidates and
previously used a check-then-create sequence. The wrapper now treats directory
creation as the atomic claim and retries at most 32 times. This is a measured
workflow reliability fix worth retaining before self-hosting; it is not broad
tuning of the transitional compiler.

The new focused owner advances the registry to 112 owners and 5,316 cases at
SHA-256
`fc53fb21939dd854c4a7f3e8a46602a62dd04078444002723375d77b9c1f3e93`.
The complete body-parser fixture exceeds the small scripting runner's existing
bounded call-depth profile, so executable evidence uses the maintained native
segmented package rather than widening the runner. Paired Linux execution
remains pending.

This remains a Slice 5 front-end checkpoint. Allocation leases and honest
`memory.allocate` enforcement, public fallible Vector construction/recoverable
append, general owned calls and control-flow joins, semantic `using`, reverse
successful-acquisition release, and one real hosted resource consumer remain
before Slice 5 is complete. Canonical effect resolution, inference, function
values, and capture enforcement remain Slice 6 work.

## Fallible Vector-construction typed-WVIR evidence

[Decision 0840](../Decisions/0840-Bind-Fallible-Vector-Construction-In-Wvir.md)
adds the first public fallible Vector-construction compiler boundary without
claiming executable allocation. The canonical
`Foundationˉcollections.Vectorˉconstructˉreserved::<i32>` fixture is admitted
with exact Foundation Collections, Memory, and Result dependencies. The current
split analyzer publishes:

```text
source analysis status=Published source-bytes=2085 manifest-bytes=104 binding-bytes=284 wir-bytes=456
```

The directory is exact WVIR 1.5: five function entries, two blocks, three
operations, three temporaries, and one operand. Exactly one operation is 172.
Its result is the private `Result<Vector<i32>, Allocationˉfailure>` instance,
its sole operand is exact `u64`, its target is budget slot zero, and its
auxiliary is the canonical Foundation Memory module. The present emitter proves
the closed executable boundary:

```text
source emission status=Valid analysis-status=Valid wvb-status=Unsupportedˉoperation function=0 operation=1 source-line=0
```

The independent verifier accepts that one boundary and rejects eight exact
mutations: WVIR 1.4, unknown operation 173, Vector instead of Result, no maximum
operand, a non-budget target, the Collections module instead of Memory, a `u32`
maximum, and the result temporary used as its own maximum. Five source programs
reject inferred generic arguments, wrong maximum width, wrong result, wrong
budget, and a lookalike allocation-failure declaration. A use-after source
program publishes provisional analysis, but independent ownership validation
rejects its second operation 172 and no WVB is created.

The maintained products are the 83,055-byte admitter at SHA-256
`aefe1711155aa74bd6f1ac188e778aaf94d5e9f603434d0ce737858f9543cd04`,
1,165,611-byte analyzer at SHA-256
`351368e34169c8f4c92992f924df0d39bab13168b012e92e943130cd93b80010`,
and 1,101,122-byte emitter at SHA-256
`b0b4f7cd12e7ef90abf61b125c53a05dd13af26eba6b93b13313b599aca35046`.
The front-door contract advances to 478 cases and 114 source fixtures. The
112-owner registry contains 5,332 cases at SHA-256
`ae29842cedcd3eda416b8008cf77e03b9346faba5bf0779d4ebacc7468be51f0`.
Focused Windows evidence is present; the complete front-door rerun and paired
Linux execution remain the later qualification gate.

## Generation-safe allocation-lease accounting evidence

[Decision 0841](../Decisions/0841-Prove-Generation-Safe-Allocation-Leases.md)
extends the fixed-capacity memory oracle without changing its private 2,616-byte
state. Every available budget token now has an odd generation. Converting it to
one allocation lease advances to the following even generation, so Split and
budget release reject the old token immediately. A released slot selects the
next greater odd generation when reused; generation `4294967295` stays retired.

The current lease evidence is a private 28-byte value containing the domain and
generation plus exact maximum-retained, current-retained, and alignment fields.
Construction accepts power-of-two alignment from 1 through 4,096, positive
maximum bytes, ordered current bytes, and a maximum within the budget's
unreserved authority. Exhaustion reports reason 1 with exact requested and
available bytes while preserving the state. Invalid metadata is rejected before
mutation. Successful conversion makes the budget stale, and release accepts
only the exact token-carried metadata before recursively crediting the parent.

The focused self-test preserves the original 17 budget cases and adds 12 lease
cases. Two independent builds match as a 35,799-byte WVB at SHA-256
`3f156ef17f29c5673c0d383c713e04814327243783d2047cce2fc8fe6be117fb`;
the packaged Windows execution returns 42. The owner summary is:

```text
native language 1 memory budget accounting status=Passed cases=29 result=42 state-bytes=2616 capacity=65 lease-token-bytes=28 wvb-bytes=35799
```

The registry remains 112 owners and contains 5,344 cases at SHA-256
`b34bd9e5ce73255db7da366b908dda29249df9514aff6f7dbb1918ce4d4489e1`.
This is an accounting oracle, not an executable operation-172 or physical-
allocation claim. The current 15-case Split owner also passes unchanged: both
typed modules execute to 42 and all nine bytecode corruptions reject. Paired
Linux execution remains required evidence before promotion.

## Executable fallible Vector-construction evidence

[Decision 0842](../Decisions/0842-Execute-Fallible-Vector-Construction-As-Wvb-1.24.md)
connects WVIR operation 172 to exact WVB 1.24 opcode `CF`. Its first immediate
is the consumed shape-25 budget slot, its second is exact
`Result<Vector<i32>, Allocationˉfailure>`, and its sole operand is `u64
Maximumˉitems`. The Types directory is canonically ordered as allocation-
failure record, allocation-reason enum, Result variant, and Vector descriptor;
the Result Valid payload points forward to the Vector type.

The successful source emits twice to identical 747-byte WVB at SHA-256
`e25ff63b466d3e4a219afdc03a64c2ff53418dffc9039fea0678ff3328d2dcd1`.
The ordinary target-unaddressable fixture emits 756 bytes, and the zero-
precondition fixture emits 753 bytes. The compiler-aligned verifier accepts all
three. The source-built native runner returns 42 for success and typed refusal;
zero reports `WVR3008` after four guest instructions.

The combined owner preserves the 752-byte Split oracle and rejects 19 exact
mutations: nine Split and ten Vector version, opcode, local, type, Result,
Vector, and allocation-failure corruptions. Its terminal evidence is:

```text
native language 1 memory budget and vector execution status=Passed cases=32 valid=5 malformed=19 result=42 split-wvb-bytes=752 split-sha256=5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53 vector-wvb-bytes=747 vector-sha256=e25ff63b466d3e4a219afdc03a64c2ff53418dffc9039fea0678ff3328d2dcd1
```

The verifier now reports the exact semantic rejection stage on invalid input.
The scalar runner retains its 2,048-cell native frame ceiling; cohesive extended
execution and descriptor-release extraction brought its largest function below
that bound instead of widening it. The registry remains 112 owners and contains
5,361 cases at SHA-256
`7da8ebac77d31f21554b198e9ee90598280c31c72cf65c1c7344835eddc4b8a4`.
Paired Linux execution and promoted-runner repinning remain required before a
cross-host conformance claim.

## Executable recoverable Vector-append evidence

[Decision 0843](../Decisions/0843-Execute-Recoverable-Vector-Append-As-Wvb-1.25.md)
connects the frozen all-or-nothing Foundation append contract to typed WVIR
operation 173 and exact WVB 1.25 opcode `D0`. The instruction's first immediate
is one direct mutable non-parameter `Vector<i32>` local; its second is exact
`Result<unit, Vectorˉappendˉfailure<i32>>`; its sole stack operand is the item.
The verifier reconstructs the unit Valid payload, returned-item failure record,
canonical Collection failure, Vector identity, and exact scalar element.

This checkpoint also corrects generic nominal serialization. Dependency order
remains compiler planning evidence, while the final Types directory is grouped
by semantic category and ordinal name. The append fixture therefore contains a
concrete record, a generic record, one enum, three variants, and one Vector in
canonical category order; all generic and public nominal references are remapped
to those final indices. No private shape or template enters WVB.

The 5,553-byte source publishes 5,704-byte typed WVIR 1.8. Two independent
compilations produce identical 3,096-byte WVB 1.25 at SHA-256
`6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f`.
The source-built runner appends `7`, refuses the attempted `9` at capacity,
returns that `9` with exact `Capacityˉexhausted` maximum `1`, and returns `42`.
The compiler-aligned
verifier accepts the module and rejects twelve append-specific version, opcode,
local, type, payload, failure, element, and canonical-order corruptions.

The combined focused owner preserves the Split and fallible-construction
oracles and reports:

```text
native language 1 memory budget and vector execution status=Passed cases=47 valid=6 malformed=31 result=42 split-wvb-bytes=752 split-sha256=5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53 vector-wvb-bytes=1107 vector-sha256=881bcbabc9620188964a63601490ad81acf63587f70501443d97447cdd45f7c5 append-wvb-bytes=3096 append-sha256=6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f
```

The native registry remains 112 owners and advances to 5,376 cases at SHA-256
`cf78e39ec42551a9fc1715e4582a1a0971aeb35ad2e547a1f7587c0d72da267d`.
The runner envelope now accepts the already specified ordinary variant shape
`11` in record fields, matching its decoder and the compiler-aligned verifier.
This is current-Windows development evidence. Paired Linux execution remains
required before cross-host conformance is claimed.
