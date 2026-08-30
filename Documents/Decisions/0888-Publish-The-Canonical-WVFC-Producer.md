# Decision 0888: publish the canonical WVFC producer

## Status

Accepted on 2026-08-29 and implemented on 2026-08-30 for the focused Windows
development checkpoint. Linux execution remains required before a paired-host
conformance claim. This decision implements the bounded producer slice of
[Decision 0886](0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md);
it does not yet integrate WVTD, WVFC, or WVAE into the admission coordinator.

## Context

The implemented WVFC 1.0 format has canonical record and catalog constructors,
but it previously had no compiler-owned producer from admitted source. A useful
producer must authenticate the frozen System foreign syntax through the one
declaration parser, preserve exact source spans and order, compute the signature
digest, and remain bounded for 64 modules, a 4 MiB source set, and 43,690
records.

Repeatedly concatenating a growing record sequence would be quadratic at that
ceiling. Deriving expected spans in the owner exclusively through the same
parser would also allow one shared offset defect to pass. Both risks need
explicit evidence.

Decision 0886 expected this producer to use the `Bytesˉsha256ˉhex` semantic
intrinsic. The current direct-native backend does not implement that operation.
Executing this compiler-scale fixture inside the scalar interpreter nested in
a native application retained more than the current 224 MiB text arena, even
though the guest result was otherwise valid. Increasing the arena or weakening
the fixture would hide the boundary rather than qualify the producer.

The existing source-owned `Foundation/Sha256.wv` implementation is exact,
host-independent SHA-256 and is already used by compiler source-profile code.
It is therefore a suitable implementation dependency for the independently
packaged producer owner. This does not change WVFC digest semantics or authorize
a host hashing service.

## Decision

Add `Compilerˉsourceˉforeignˉcatalogˉproduce` as the sole canonical WVFC
producer for validated descriptor-free `WVSS 2` snapshots. It rejects WVSS 1
and unknown versions and publishes either one complete canonical WVFC or an
empty value with exact bounded diagnostics.

Keep version diagnostic layering exact: recognized-but-not-admitted WVSS 1
returns producer status `ADMITTED_WVSS`; structurally unknown WVSS 3 returns
`SOURCE_SET` with the source-set scanner's `Unsupported_version` status.

The producer performs the existing source-set validation and one independent
source-ordered declaration scan. The declaration ordinal is the zero-based
ordinal of every non-`End` declaration-scan entry, including imports and
ordinary declarations. Only a foreign entry emits a record. This preserves the
source ordinals consumed by later authentication without introducing a second
parser or grammar.

Each record requires the edition-1 System profile, parser-authenticated unsafe
foreign evidence, the exact registered ABI literal mapped to identity 1, and
the exact nonempty name, ABI-literal-content, external-symbol-content,
signature, and effect-clause spans. The signature is the raw admitted source
from `fn` through the final byte of `effects(ffi.call)`. For the three frozen
layouts, its guarded bounds are `Name_offset - 3` through
`Effect_clause_offset + Effect_clause_length`. Any expanded layout must revise
that parser/producer invariant explicitly.

Compute the digest through
`Foundationˉsha256.Foundationˉsha256ˉhex` and decode exactly 64 lowercase ASCII
hexadecimal bytes into 32 raw bytes. Add the Foundation SHA source to only the
producer projects that need it. Keep the strict compiler-owned decoder and the
unchanged canonical WVFC constructors.

Accumulate fixed 96-byte records in a pending chunk no larger than 65,536
bytes. Flush a complete chunk into the bounded complete sequence and
materialize the final record payload once before the catalog constructor. The
focused 683-record case must cross the first flush boundary and verify count,
structure, and first/last declaration ordinals.

Execute the producer owner as a complete-verified direct-native profile-7
application. Build its WVB twice, require byte identity and its exact written
identity, use the repository's content-addressed segmented hosted-WVB cache for
stage, link, transport, and package acquisition, then run one selector per
bounded process with exact exit 42 and no output. The owner has Windows and
Linux launchers; this decision records only the host actually run.

The producer does not infer semantic types or effects, grant authority, bind a
symbol, compare source platform scopes with a selected WVTD, produce WVAE, or
change the analyzer, runtime, or linker. Those remain later coordinator and
consumer checkpoints.

## Evidence

The focused Windows owner built two byte-identical 503,800-byte WVB values with
SHA-256
`d647180c2015c236c6b1cbbade40d2aff25959c74936174e11d2d771723addc1`.
The complete verifier accepted the immutable candidate. All 25 isolated
selectors `a` through `y` returned exact exit 42 with no standard output or
error. They cover strict digest decoding, canonical empty and nonempty
catalogs, the three frozen layouts, import/ordinary/foreign ordinal accounting,
module and declaration order, deterministic output, WVSS 1, unknown-version,
and trailing-input rejection, profile/unsafe/ABI/symbol/effect rejection, the
64/65-module boundary, maximum-count arithmetic, canonical structure, and the
683-record accumulator flush.

The canonical single-record case independently fixes UTF-8 byte spans at
declaration 91/253, name 156/21, ABI literal contents 107/44, external symbol
contents 312/30, signature 153/154, and effects 290/17, plus a hard-coded
signature digest. The final flush case completed in 546 ms on the Windows
development host. The hardened cache cold acquisition completed in 121,681 ms
with key
`8e742e55d181bd581b6c4d798564ce3553986044aacf6c1fe75c15101248ab63`.
Its 12,554,752-byte profile-7 application has SHA-256
`4fdaf4baa1eb6d9f022063233691bbb360115770c7e0281f61212250579d2cbb`.
Application packaging measurements describe this host and cache generation;
they are not portable semantics.

## Consequences

- WVFC now has one bounded compiler-owned producer over authenticated admitted
  source instead of only caller-supplied format constructors.
- Exact trivia and layout intentionally affect the signature digest because the
  digest authenticates raw admitted bytes.
- Record production remains bounded without one whole-catalog append per
  declaration.
- Direct-native execution avoids nested-interpreter retained text without
  widening a product arena or changing ordinary script call-depth policy.
- Decision 0886's intrinsic requirement is superseded for this producer only;
  future consumers must choose their own measured implementation within their
  retained compiler budget.
- The admission coordinator, selected-target agreement, WVAE publication,
  Analyzer authentication, and paired-host qualification remain unimplemented.

## Reconsideration triggers

Reconsider this decision if the foreign declaration layout expands, WVSS
advances beyond version 2, the ABI registry gains another source literal, the
record/source-set bounds change, a current native SHA semantic operation is
implemented and measured, the producer moves into a product whose retained
source closure cannot include Foundation SHA, or paired-host evidence finds a
byte or diagnostic difference.
