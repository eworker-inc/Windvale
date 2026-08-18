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
deterministically through WIR and WVB. The first Slice 2 checkpoint adds exact
front-end identities for the frozen primitive value types and prevents Seed-only
`void` type syntax from crossing the edition-1 front door. Project 3 carries the
profile artifacts; Project 2 and descriptorless Seed retain their prior behavior.

This checkpoint does not complete Slice 2. Literal decoding, exact scalar
operations, WVB/runtime representation, ordinary `unit` and `never` control
semantics, named update, multi-field variant construction and destructuring, and
value-producing control flow remain pending. Localized token execution,
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

## Slice 2 primitive front-end checkpoint

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

## Focused verification owner

The cross-host `language-1-front-door` owner reports thirteen declared cases. Its
bounded checkpoints recompute the frozen identities, compare two descriptor-test
builds and execute them, build and execute the 23-assertion value-front-end test,
construct the changed compiler through the shared segmented backend, compile the
minimal edition-1 program twice through the exact lock/profile inputs, require byte
identity and result `42`, and exercise the unsupported-profile, missing-profile,
descriptorless-header, Seed-only-`void`, no-ambient-profile, wrong-lock-digest, and
changed-profile rejection boundaries. The report separately states its 33
descriptor assertions, four profile-admission outcomes, 23 value-front-end
assertions, and eight compiler outcomes rather than presenting those nested
assertions as extra owners.

Frozen design inputs, descriptor files, edition-1 fixtures, and the integrated
compiler boundaries map to this owner. Compiler WVB/image construction and
hosted application packaging use content-keyed cross-host caches: the first run
earns full native evidence, while unchanged repeats materialize validated cache
hits instead of rebuilding the 29 MiB compiler application. This is development
evidence, not yet a paired-host conformance claim. With the Windows compiler and
application checkpoints populated, the current thirteen-case owner passed in
17,070 milliseconds, including both self-tests, cached compiler materialization,
two minimal compiles, execution, and all seven negative admissions.

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
resolution, and Project 2 remains stable. Slice 2 has begun with primitive
front-end identities and edition separation. Its completion gate remains exact
literal/value execution plus named update, multi-field variant/destructuring, and
value-producing control flow over this same compiler architecture. Seed stays on
that architecture until its named removal checkpoint.
