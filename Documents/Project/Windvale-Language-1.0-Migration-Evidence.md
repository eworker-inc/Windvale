# Windvale Language 1.0 migration evidence

## Status

Decision 0767 freezes the source design. This page records implementation and
measurement evidence outside that immutable identity. It must not be read as a
claim that the complete Language 1.0 compiler, Foundation, runtime, editor, or
any natural-language pack is implemented.

The current checkpoint completes the first vertical Slice 1 path. The existing
compiler now admits an edition-1 source descriptor, resolves only its pinned
bootstrap `en@1` binding, exposes the remaining bytes as an immutable view,
parses the required standalone module metadata, and compiles one minimal Core
program deterministically through WIR and WVB. Seed remains available as the
temporary descriptorless migration path. External lock/profile artifact loading,
localized lexicons and vocabularies, Unicode identifier admission, most
Language 1.0 semantics, and paired-host Language 1.0 qualification remain
pending.

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

The 37-case self-test covers accepted English and Simplified-Chinese descriptor
shapes, the maximum profile version, missing/unsupported editions, malformed
profiles and versions, BOM/non-ASCII input, line-ending failures, and length
bounds. It also proves that exact `en@1` resolves to the sole English bootstrap
binding while a well-formed `zh-Hans@17` descriptor remains unsupported. It
returns `42`. Two builds compare byte-identically before execution. The current
deterministic test WVB is 14,183 bytes with SHA-256
`d48115061763bc8b6137f1e389b5aa13334308968fe71cca9c178eee50d2c73e`.

`Compiler/Windvale/Source-Set-Core.wv` now performs edition dispatch once per
source-set view. A descriptorless source remains Seed edition 0. A source that
begins with `#` must pass the bounded descriptor reader and resolve to the exact
English binding; malformed descriptors and every other profile fail before the
general lexer. For admitted edition 1, the view starts at the descriptor's line
ending. `Bytesˉslice` retains the original immutable backing, avoids a
whole-source copy, and lets the existing lexer preserve the module header's
physical line 2. The view records the raw offset/length, edition, binding,
origin, and front-door failure offset.

The declaration parser accepts `profile core|hosted|system;` only in the
standalone edition-1 metadata position. `core` lowers to the current portable
WVB profile for this implemented subset. Source-set validation rejects that
standalone form in descriptorless Seed and rejects an edition-1 file that omits
it. The WVB metadata writer consumes the same header without creating a second
compiler path.

The current profile resolver is intentionally a built-in bootstrap binding. It
pins profile artifact
`e678b1b5daae2c0d87179f2fcd162b1b002cebe8617fc0fb155a5b78a1bdaf27`
under lock artifact
`4c5840af896924292a2ad3f3d5d986956211745a8e4a9bb60f0b45f10cecf9c3`.
It does not search the host or claim that external `.wvlock`/`.wvsp` admission
is implemented.

`Tests/Fixtures/Language-1.0/Minimum-Program.wv` is the first executable
edition-1 fixture. A source-built native compiler emits the same 221-byte WVB
twice, SHA-256
`2f080e3bb2b43b3da2da1d3c9aea4b7d3e3e3a23432cc39ed189c553da4e1d2a`;
the ordinary runtime returns `42`. An unsupported source profile, a missing
edition-1 profile declaration, and a descriptorless edition-1 header all fail
without publishing an output.

## Focused verification owner

The cross-host `language-1-front-door` owner has seven declared cases:

1. recompute the complete frozen identity and fixture inventory;
2. build the descriptor self-test twice and compare exact WVB bytes; and
3. execute the descriptor self-test and require the sole output `Result: 42`;
4. construct the changed compiler through the shared segmented backend;
5. compile the minimal edition-1 program twice, require byte identity, and run it;
6. reject unsupported `zh-Hans@1` without publication; and
7. reject both missing and descriptorless edition-header mismatches without
   publication.

Frozen design inputs, descriptor files, edition-1 fixtures, and the integrated
compiler boundaries map to this owner. Compiler WVB/image construction and
hosted application packaging use content-keyed cross-host caches: the first run
earns full native evidence, while unchanged repeats materialize validated cache
hits instead of rebuilding the 29 MiB compiler application. This is development
evidence, not yet a paired-host conformance claim. After the Windows checkpoints
were populated, the complete owner passed in 3,212.894 milliseconds, including
descriptor builds, cached compiler materialization, two minimal compiles,
execution, and all three negative admissions.

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
implementation. It rejects the enlarged 20-module build-driver source at its
older retained-evidence bound, so it is not used to disguise forward-language
source as a semantic-freeze artifact. The explicitly unqualified current
candidate driver accepts the same exact project. Independent source-WIR
inspection reports 630 functions, 15,768 blocks, 66,960 operations, 60,075
temporaries, 51,780 operands, and a 3,597,612-byte directory, leaving 596,692
bytes below the 4 MiB value ceiling.

On Windows the current driver deterministically emitted a 1,182,549-byte WVB,
SHA-256
`1c2fa49bdd35a12125072b361b244521d2a0f22ccb432c99f701d1f2c229ff6a`.
The independently reconstructed staging producer accepted it as 31,025,972
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

The next semantic checkpoint is Slice 2: make source-profile locks and profile
artifacts explicit project/build inputs, validate the pinned artifact chain, and
move English token resolution behind that admitted profile without changing the
canonical parser/IR. Seed stays on the same compiler architecture until the
named removal checkpoint.
