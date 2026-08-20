# Windvale Language 1.0 migration evidence

## Status

Decision 0767 freezes the source design. This page records implementation and
measurement evidence outside that immutable identity. It must not be read as a
claim that the complete Language 1.0 compiler, Foundation, runtime, editor, or
any natural-language pack is implemented.

Migration Slices 1 through 3 are complete and Slice 4 is next. The existing compiler admits
an edition-1 source descriptor only through an explicitly supplied, hash-pinned
source-input lock and composite source profile. It resolves the frozen `en@1`
component chain, exposes the remaining bytes as an immutable view, parses the
required standalone module metadata, and compiles one minimal Core program
deterministically through WIR and WVB. Slice 2 adds exact front-end identities
for the frozen primitive value types and prevents Seed-only `void` type syntax
from crossing the edition-1 front door. Named record update, fixed-width
`i8`/`i16`/`u16`, exact Unicode-scalar `rune`, `f32`/`f64`, ordinary one-value
`unit`, and return-only `never` now cross the compiler, verifier, and scalar
runtime together. Named zero-through-64-field variant construction and
destructuring now cross the compiler, compiler-aligned verifier, and
source-built native scalar runner through WVB 1.16. Project 3 carries the
profile artifacts; Project 2 and
descriptorless Seed retain their prior behavior.

Value-producing `if` and exhaustive enum/variant `match` now cross the reference
compiler and scalar runtime, completing Slice 2's planned value-and-control
compiler surface. Slice 3 publishes the exact edition-1
`Foundationˉoption.Option<T>` and `Foundationˉresult.Result<T, E>` variant
identities, admits their full-arity type uses, and emits every used concrete
specialization as an ordinary canonical WVB variant. Value-producing `try`
extracts `Valid.Value` and permits `Result<T, E>` to propagate into
`Result<U, E>` by reconstructing only the failure case when `T` and `U` differ.
One fixture migrates a manual `Valid`/`Value`/`Error` record through an explicit
domain-error adapter. General generic functions, generic user declarations,
collections, and the repository-wide Seed-to-edition-1 source migration remain
Slice 4 or later work. Final paired-host and broad integration evidence remains
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

The focused value-front-end self-test contains 24 assertions covering all appended
keyword and primitive-type identities, both edition directions, exact first
invalid-token offsets, and the stable value-`if` expression-kind identity. It
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

The cache admits only unoptimized `portable-wvb-v1` Project 2 input and is not
qualification evidence. Project 3 profile-aware caching, general Slice 4
generic resolution and collections, repository-wide source migration,
paired-host equality, and final broad integration remain open.

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

The cross-host `language-1-front-door` owner reports 141 declared cases. Its
bounded checkpoints recompute the frozen identities, compare two descriptor-test
builds and execute them, build and execute the 25-assertion value-front-end test,
construct the changed compiler through the shared segmented backend, and retain
the minimum, unit, record-update, and 22-case fixed-integer evidence. Its 20 rune
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
