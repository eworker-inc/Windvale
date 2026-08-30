# Windvale compiler admission evidence

## Status and ownership

This specification defines Windvale Admission Evidence 1.0 (`WVAE 1.0`) and
the first compiler-owned construction and authentication foundation. The
format is an internal compiler-phase value, not a package, executable, cache,
authority grant, or distribution format.

`Compiler/Windvale/Admission-Evidence-Core.wv` owns the portable format code.
`Compiler/Windvale/Admission-Evidence-Validator-Core.wv` owns the portable
post-read structural, cross-field, and digest orchestration shared by the
hosted shell and focused fixture.
`Compiler/Windvale/Admission-Source-Set-Core.wv` owns the independent small
WVSS 2 structural reader. The hosted `wvverify-admission-evidence` leaf
compiles those modules with the existing lightweight WVTD and WVFC structural
validators independently of the source analyzer. The complete independent
validator remains named `wvauth`. Portable in-memory `wvadmit` construction and
authenticated coordination are now implemented by
`Compiler/Windvale/Source-Admission-Coordinator-Core.wv` and
`Compiler/Windvale/Source-Target-Admission-Core.wv`; a hosted file publisher
and Analyzer integration remain later checkpoints.

## Fixed binary format

WVAE 1.0 is exactly 224 bytes. Multibyte integers are unsigned little-endian.

| Offset | Bytes | Field | Exact version-1 requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVAE` |
| 4 | 2 | major version | `1` |
| 6 | 2 | minor version | `0` |
| 8 | 4 | structure length | `224` |
| 12 | 4 | hash identity | `1`, SHA-256 |
| 16 | 4 | flags | zero |
| 20 | 4 | WVSS byte length | exact bound value length |
| 24 | 4 | WVTD byte length | exact bound value length |
| 28 | 4 | WVFC byte length | exact bound value length |
| 32 | 4 | WVSS module count | exact independently validated count |
| 36 | 4 | foreign-declaration count | exact independently validated count |
| 40 | 4 | admitted edition | `1` |
| 44 | 4 | admitted source-profile binding | `1`, the existing `Compilerˉsourceˉprofileˉbinding.English` identity |
| 48 | 16 | reserved | all zero |
| 64 | 32 | WVSS digest | raw SHA-256 of the exact WVSS bytes |
| 96 | 32 | WVTD digest | raw SHA-256 of the exact WVTD bytes |
| 128 | 32 | WVFC digest | raw SHA-256 of the exact WVFC bytes |
| 160 | 32 | lock digest | raw SHA-256 of the exact source-input-lock snapshot |
| 192 | 32 | profile digest | raw SHA-256 of the exact selected-profile snapshot |

An empty, truncated, or trailing-byte value is invalid. Unknown versions,
hash identities, flags, profile bindings, editions, or nonzero reserved bytes
are invalid. WVAE is not a container: WVSS, WVTD, WVFC, lock, and profile
snapshots remain separate immutable values.

## Bounded input geometry

Every limit is checked before hashing or result construction:

- WVSS is 37 through 4,194,304 bytes, contains one through 64 modules, and is
  at least `16 + 21 * module count` bytes so its 20-byte directory and one byte
  per nonempty source can fit;
- WVTD is 64 through 320 bytes and its length after byte 64 is divisible by 4;
- foreign count is at most 43,690 and WVFC length is exactly
  `48 + 96 * foreign count`, at most 4,194,304 bytes;
- the source-input-lock snapshot is 1 through 1,048,576 bytes; and
- the selected-profile snapshot is 1 through 65,536 bytes.

The six retained WVAE, WVSS, WVTD, WVFC, lock, and profile values have an
aggregate ceiling of exactly 9,503,264 bytes. The hosted reader snapshots and
checks them sequentially in WVAE, WVTD, lock, profile, WVSS, WVFC order. It
updates the checked cumulative total immediately after each accepted read and
does not read the next value after rejection. The maximum accepted prefix
before the final service read is 5,308,960 bytes; adding the service's maximum
4,194,304-byte value reaches exactly 9,503,264. This establishes the peak bound
before structural validation, count derivation, hashing, or other work.
The outer per-value checks retain the matching WVAE status and field offset for
absolute bounds: WVAE length uses `Invalidˉlength`; WVSS below 37 or above
4,194,304 uses `Invalidˉwvssˉlength` at 20; WVTD below 64 or above 320 uses
`Invalidˉwvtdˉlength` at 24; WVFC below 48 or above 4,194,304 uses
`Invalidˉwvfcˉlength` at 28; and lock/profile use their exact length statuses
at 160/192. In-range malformed WVTD and WVFC geometry is not relabeled as an
outer length failure: it is delegated to the existing structural validators
and retains the `Wvtd` or `Wvfc` validation phase, structural status, and exact
inner offset. Only a checked cumulative-total failure uses the separate
`Resourceˉlimit` validation phase and hosted diagnostic.

Count multiplication occurs only after the count bound is established.
The numeric geometry API accepts lengths and counts directly so maximum,
one-past-maximum, and inconsistent arithmetic can be tested without allocating
oversized values. The independent leaf then validates WVSS 2, WVTD, and WVFC
structure before it derives either count.

## Portable APIs

```text
Compilerˉvalidateˉadmissionˉevidenceˉinputˉshape(
    Wvssˉlength: u32,
    Wvtdˉlength: u32,
    Wvfcˉlength: u32,
    Wvssˉmoduleˉcount: u32,
    Foreignˉcount: u32,
    Lockˉlength: u32,
    Profileˉlength: u32
) -> Compilerˉadmissionˉevidenceˉresult

Compilerˉadmissionˉevidenceˉretainedˉtotalˉisˉbounded(...lengths) -> bool

Compilerˉadmissionˉevidenceˉretainedˉadditionˉisˉbounded(
    Current: u32,
    Additional: u32
) -> bool

Compilerˉvalidateˉadmittedˉsourceˉsetˉstructure(Input: bytes)
    -> Compilerˉadmissionˉsourceˉsetˉresult

Compilerˉvalidateˉadmissionˉevidenceˉshape(Input: bytes)
    -> Compilerˉadmissionˉevidenceˉresult

Compilerˉdecodeˉadmissionˉsha256ˉhex(Input: bytes)
    -> Compilerˉadmissionˉevidenceˉresult

Compilerˉconstructˉadmissionˉevidence(...)
    -> Compilerˉadmissionˉevidenceˉresult

Compilerˉauthenticateˉadmissionˉevidence(...)
    -> Compilerˉadmissionˉevidenceˉresult

Compilerˉvalidateˉadmissionˉevidenceˉsnapshots(...)
    -> Compilerˉadmissionˉevidenceˉvalidationˉresult
```

Shape validation is pure structural validation and performs no hashing.
Authentication first validates the complete WVAE shape and bounded snapshot
geometry, requires exact length and count agreement, computes all five digests,
then compares them in field order. A digest match proves only set binding; it
does not prove the syntax or semantics of any bound value.

Construction and authentication compute every digest through the semantic
`Bytesˉsha256ˉhex` intrinsic. `Textˉtoˉutf8` exposes its exact 64 ASCII bytes to
one fixed decoder. The decoder accepts only `0` through `9` and lowercase `a`
through `f`, converts each pair to one raw byte, and rejects short, long,
uppercase, or nonhexadecimal input at the first exact offset. This module does
not import `Foundation/Sha256.wv`, accept caller-supplied hashes, or invoke a
host hashing service.

Success returns the accepted input bytes unchanged for shape validation and
authentication. Failure always returns an empty `Value`.

## Status and offset contract

| Status | Exact failure offset |
| --- | ---: |
| `Invalidˉlength` | actual length when below 224; otherwise 224 |
| `Invalidˉmagic` | first differing magic byte, 0 through 3 |
| `Unsupportedˉversion` | 4 for major, 6 for minor |
| `Invalidˉstructureˉlength` | 8 |
| `Unsupportedˉhash` | 12 |
| `Invalidˉflags` | 16 |
| `Invalidˉwvssˉlength` | 20 |
| `Invalidˉwvtdˉlength` | 24 |
| `Invalidˉwvfcˉlength` | 28 |
| `Invalidˉmoduleˉcount` | 32 |
| `Invalidˉforeignˉcount` | 36 |
| `Invalidˉedition` | 40 |
| `Invalidˉprofileˉbinding` | 44 |
| `Invalidˉreserved` | first nonzero byte, 48 through 63 |
| `Invalidˉdigestˉencoding` | first invalid decoder byte, or 64 for excess input |
| `Digestˉmismatch` | 64, 96, 128, 160, or 192 for the first mismatching digest |
| `Invalidˉlockˉlength` | 160 |
| `Invalidˉprofileˉlength` | 192 |

## Independent validator boundary

`wvverify-admission-evidence` accepts exactly six explicit snapshot resource
names in WVAE, admitted WVSS 2, WVTD, WVFC, lock, and profile order. Before
deriving counts it checks all per-value and aggregate retention limits, then:

1. validates WVSS magic, exact version 2.0, one through 64 entries, exact
   20-byte directory entries, edition 1, English binding 1, origin range 1
   through 129, nonempty payloads, and canonical contiguous layout;
2. invokes the existing lightweight WVTD and WVFC structural validators;
3. derives the module and foreign counts, requires the WVFC module count to
   equal the WVSS count, and authenticates every WVAE field and digest.

A structurally valid WVFC whose module count differs from the structurally
valid WVSS fails in `Crossˉfield` phase with structure status 1, offset 24,
and an empty value.

It publishes no certificate or successor artifact and reports only success or
bounded failure control flow. The coordinator retains all six exact immutable
private snapshots. A future Analyzer handoff may invoke internal admitted mode
only after the complete `wvauth` succeeds.

## Authenticated source-admission coordinator

`Compilerˉsourceˉadmissionˉcoordinate` accepts WVSS 1 source input, locked
source-profile snapshots, the exact 64-byte lowercase lock digest, and one
WVTD. It rejects outer geometry first, admits descriptor-free WVSS 2, checks
every platform scope against WVTD, produces canonical source-ordered WVFC,
independently validates the three formats and counts, checks each foreign
module's System profile and exact concrete predicate, and constructs WVAE.
Success returns WVSS, unchanged WVTD, WVFC, and WVAE as four separate immutable
values. Failure returns all four values empty.

The input-retention ceiling is 5,308,800 bytes. The conservative result ceiling
is 8,389,152 bytes, and combined retained input plus result is at most
13,697,952 bytes. These are retained-value bounds, not a compiler process
working-set claim. A WVSS above 4,194,304 bytes rejects before source scanning.
The arithmetic APIs cover exact maxima and one-past rejection without allocating
oversized fixtures.

The exported foreign-catalog target readback is independently safe. After
WVSS, WVTD, WVFC, and module-count validation, it reproduces WVFC from supplied
WVSS and requires exact bytes before target admission. Stable retention is at
most 12,583,216 bytes: supplied WVFC retains the general 4,194,304-byte format
ceiling while reproduced canonical WVFC is at most 4,194,288 bytes.
Conservative simultaneously-live large immutable payload retention additionally
holds the bounded 4,194,240-byte headerless record accumulator, totaling
16,777,456 bytes. This is not a process working-set bound and excludes runtime
state and small bounded scalar/view temporaries. Reproduction and comparison
are linear in the bounded source/catalog sizes.

Producer failure maps to outer target-admission status `WVFC` and preserves the
subordinate source-set, parser, catalog, source module, and source offset. A
structure-valid byte mismatch also reports `WVFC` at the first differing
catalog offset. The coordinator's earlier production phase retains the more
specific foreign-producer status. Generic `linux`, and a list mixing generic
`linux` with the exact ABI predicate, cannot authorize foreign source; only
the exact concrete predicate alone can do so.

No public analyzer mode may bypass that control flow. A cache key, path,
producer identity, file existence, or forgeable certificate cannot replace
validation. The analyzer remains responsible for independent WVSS, WVTD, and
WVFC structure validation plus semantic and source/catalog consistency.

The current admission-evidence leaf has fixed development ceilings of 262,144
WVIR bytes and 262,144 WVB bytes. The refined Windows capacity case publishes
190,524 WVIR bytes and a valid deterministic 72,060-byte WVB. These ceilings do
not set the eventual complete `wvauth` product ceiling. Cross-host equality
remains required before qualification is claimed.

This is an intermediate format and validator-foundation checkpoint, not a
completed hosted-validator execution claim. The implemented native x64
lowering candidate now recognizes WVB opcode `0x7D` (`bytes.sha256_hex`) and
retains the intrinsic's exact bounded result contract. Its registered owner
passes eight cases on exact-current Windows and local Debian 13.5 under WSL,
including exact known answers, owned-text return, and 64/63-byte arena
behavior. That removes the backend opcode-level cause of the prior
`Unsupportedˉmodule` gap, but it is not packaging or execution evidence for
the complete `wvverify-admission-evidence` application. That
application's packaged execution and paired-host CI qualification remain
pending. The local Debian/WSL result is Linux development evidence, not that
qualification. The script runner's separate hosted-profile envelope boundary
likewise does not prove the packaged native application. This module continues
to use the semantic intrinsic; it does not
substitute Foundation SHA-256 or a host hashing service.
