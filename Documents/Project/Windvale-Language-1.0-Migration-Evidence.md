# Windvale Language 1.0 migration evidence

## Status

Decision 0767 freezes the source design. This page records implementation and
measurement evidence outside that immutable identity. It must not be read as a
claim that the complete Language 1.0 compiler, Foundation, runtime, editor, or
any natural-language pack is implemented.

Migration Slice 1 is complete and Slice 2 is active. The existing compiler admits
an edition-1 source descriptor only through an explicitly supplied, hash-pinned
source-input lock and composite source profile. It resolves the frozen `en@1`
component chain, exposes the remaining bytes as an immutable view, parses the
required standalone module metadata, and compiles one minimal Core program
deterministically through WIR and WVB. Slice 2 adds exact front-end identities
for the frozen primitive value types and prevents Seed-only `void` type syntax
from crossing the edition-1 front door. Named record update, fixed-width
`i8`/`i16`/`u16`, exact Unicode-scalar `rune`, `f32`/`f64`, ordinary one-value
`unit`, and return-only `never` now cross the compiler, verifier, and scalar
runtime together. Project 3 carries the profile artifacts; Project 2 and
descriptorless Seed retain their prior behavior.

These checkpoints do not complete Slice 2. Multi-field variant construction and
destructuring and value-producing control flow remain pending. Localized
token execution, public-library vocabulary lookup, Unicode identifier admission,
and paired-host Language 1.0 qualification also remain pending.

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
`4c5840af896924292a2ad3f3d5d986956211745a8e4a9bb60f0b45f10cecf9c3`.

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

The focused value-front-end self-test contains 23 assertions covering all appended
keyword and primitive-type identities, both edition directions, and exact first
invalid-token offsets. It compiles to a verified WVB and returns `42`.

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

The cross-host `language-1-front-door` owner reports 96 declared cases. Its
bounded checkpoints recompute the frozen identities, compare two descriptor-test
builds and execute them, build and execute the 23-assertion value-front-end test,
construct the changed compiler through the shared segmented backend, and retain
the minimum, unit, record-update, and 22-case fixed-integer evidence. Its 20 rune
cases compile the valid program twice, compare diagnostics and bytes, reject eight
source forms, admit the canonical module, reject six byte-level corruptions, and
execute both the reconstructed shared runner and focused runtime core. The 27
floating cases add deterministic compilation, four source rejections, eight
malformed-WVB rejections, current-runner execution, and a focused raw-bit runtime
self-test. Its 21 unit/never cases add deterministic compilation, four source
rejections, eleven malformed-WVB rejections, and two executions returning `42`.
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
runtime checkpoint. Its completion gate remains multi-field
variant/destructuring and value-producing control flow over this same compiler
architecture. Seed stays on that architecture until its named removal
checkpoint.
