# Windvale Language 1.0 foreign-declaration catalog

## Status and scope

This document defines the implemented `WVFC 1.0` construction and structural
validation checkpoint selected by
[Decision 0886](../Documents/Decisions/0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md).
The format records one canonical source-ordered inventory of syntactic foreign
declarations. It is an internal compiler-phase value, not a package or runtime
format, and carries no authority.

This checkpoint implements the stable first ABI-contract registry identity,
fixed format construction, checked structural validation, exact failure
positions, byte-identical accepted output, and a compiler-owned canonical
producer over validated descriptor-free `WVSS 2` source snapshots. The
target-aware hosted `wvadmit` now composes that producer with exact WVTD
admission and WVAE construction, while the separately built `wvauth`
independently authenticates every source/catalog record and target predicate.
The proposed production-ingress candidate carries an authenticated empty
catalog through its private Analyzer handoff and stops a nonempty catalog at
exact `Foreignˉsemanticsˉpending` before launching the Analyzer. Exact local
Windows product pins and the complete focused owner pass; paired-host
acceptance, semantic FFI binding and lowering, native-symbol binding, runtime
containment, and foreign execution remain later boundaries.

## Encoding

All multibyte integers are unsigned little-endian. `WVFC 1.0` has one 48-byte
header followed by zero or more source-ordered 96-byte records. No padding or
trailing bytes are permitted.

| Header offset | Bytes | Field | Version-1 requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVFC`, integer `1128683095` |
| 4 | 2 | major version | `1` |
| 6 | 2 | minor version | `0` |
| 8 | 4 | complete byte length | exactly `48 + 96 * count` |
| 12 | 4 | foreign-declaration count | `0..43,690` |
| 16 | 4 | record size | `96` |
| 20 | 4 | record offset | `48` |
| 24 | 4 | WVSS module count | `1..64` |
| 28 | 4 | flags | zero |
| 32 | 16 | reserved | all zero |

The count is checked before multiplication. The largest representable
version-1 catalog contains 43,690 records and is 4,194,288 bytes. Count 43,691
would require 4,194,384 bytes and is rejected before multiplication or record
iteration. The general value ceiling remains 4,194,304 bytes; the record width
means no canonical WVFC occupies the final 16 bytes below that ceiling.

Zero declarations produce the canonical 48-byte header. The module count is
still required because the empty catalog is evidence about one exact WVSS, not
an ambient or missing source set.

Each record is:

| Record offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | WVSS module ordinal |
| 4 | 4 | source declaration ordinal |
| 8 | 4 | declaration flags |
| 12 | 4 | registered ABI-contract identity |
| 16 | 4 | declaration offset |
| 20 | 4 | declaration length |
| 24 | 4 | source-name offset |
| 28 | 4 | source-name length |
| 32 | 4 | ABI-literal offset |
| 36 | 4 | ABI-literal length |
| 40 | 4 | external-symbol-literal offset |
| 44 | 4 | external-symbol-literal length |
| 48 | 4 | signature offset |
| 52 | 4 | signature length |
| 56 | 4 | effect-clause offset |
| 60 | 4 | effect-clause length |
| 64 | 32 | SHA-256 of the exact admitted signature span |

Flag bit 0 is the required `unsafe` marker and flag bit 1 is `export`.
Therefore the only valid values are `1` and `3`; zero, a missing `unsafe` bit,
or any unknown bit is invalid.

All six spans are nonempty in this edition-1 foreign profile. Their unsigned
ends must not overflow and every child span must be contained in its
declaration span. Structural validation does not have WVSS module byte lengths,
so proving that the declaration itself is contained in the selected module is
part of later source authentication.

Records are ordered strictly by module ordinal and then declaration ordinal.
Within one module, declaration spans are nonoverlapping and advance in source
order. Module ordinals are less than the header's module count. Gaps in module
or declaration ordinals are structurally representable because modules may
contain nonforeign declarations; the later complete WVSS scan proves that no
foreign declaration was omitted or added.

Structure validation alone is not source authentication. Durable public target
readback reproduces the complete canonical catalog from supplied admitted WVSS
and requires byte identity before it walks foreign target predicates. A
structure-valid empty catalog, omitted/reordered record, remapped declaration
ordinal, or extra record therefore rejects. The first byte difference is the
catalog failure offset; reproduction failure uses outer target-admission status
`WVFC` and retains subordinate source-set, parser, and catalog evidence.

At exact maxima, public readback stably retains 4,194,304 WVSS bytes, 320 WVTD
bytes, a supplied WVFC up to the general 4,194,304-byte format ceiling, and a
canonical reproduced WVFC up to 4,194,288 bytes: 12,583,216 bytes.
Conservative simultaneously-live large immutable payload retention adds the
producer's 4,194,240-byte headerless record accumulator for 16,777,456 bytes.
This is not a process working-set bound and excludes runtime state and small
bounded temporaries. Reproduction and exact comparison are linear bounded
scans.

The raw digest field is always exactly 32 bytes because it occupies the fixed
record tail. The record constructor separately rejects any supplied digest
whose length is not exactly 32. Structural validation deliberately does not
reject an all-zero or otherwise unusual 32-byte value: the Analyzer must
recompute SHA-256 from the authenticated signature span, and only that
comparison proves digest content.

## ABI-contract registry

Zero is permanently unregistered. The first stable registry row is:

| Identity | Canonical source literal | Exact contract |
| ---: | --- | --- |
| 1 | `windvale.paper.buffer_source.sysv_amd64_c_v1` | the no-unwind scalar/pointer buffer-source contract for `linux.x86_64.sysv_amd64_c_v1` |

The implementation exports identity 1 and an exact numeric membership check.
The admission producer maps the authenticated ordinary text literal to this
row. A WVFC consumer accepts no other numeric identity in version 1.0.
The identity records requested syntax; it grants no capability, target match,
library handle, linker import, pointer, or execution authority.

## Construction and validation

`Compilerˉsourceˉforeignˉcatalogˉrecordˉconstruct` accepts one typed
record and exact WVSS module count. It requires a 32-byte digest, validates the
module, flags, registry identity, spans, and emits exactly 96 bytes.

`Compilerˉsourceˉforeignˉcatalogˉconstruct` accepts the exact module count
and an already source-ordered sequence of fixed records. It rejects the module
count, resource limit, nonmultiple record payload, record content, order, or
overlap before publishing the header and records.

`Compilerˉsourceˉforeignˉcatalogˉvalidateˉstructure` treats the complete
value as untrusted and validates in this deterministic order:

1. outer minimum and 4 MiB resource bound;
2. magic and exact major/minor version;
3. declared total length;
4. count bound, record width and record offset;
5. WVSS module-count range, header flags, and all reserved words;
6. checked count-derived complete length; and
7. every record's module, flags, ABI identity, nonoverflowing spans, strict
   order, and same-module declaration nonoverlap.

Success returns the accepted input bytes unchanged. Every failure returns an
empty byte value and the first status/offset under that order; it never repairs,
sorts, truncates, substitutes, or normalizes the input.

| Status | Value | Failure offset |
| --- | ---: | --- |
| `VALID` | 0 | 0 |
| `INVALID_INPUT` | 1 | 16 for a constructor record-payload width |
| `INVALID_SIZE` | 2 | 8 |
| `INVALID_MAGIC` | 3 | 0 |
| `INVALID_VERSION` | 4 | 4 |
| `INVALID_RECORD_SIZE` | 5 | 16 |
| `INVALID_RECORD_OFFSET` | 6 | 20 |
| `INVALID_MODULE_COUNT` | 7 | header 24 or record field 0 |
| `INVALID_FLAGS` | 8 | header 28 or record field 8 |
| `INVALID_RESERVED` | 9 | first nonzero word in 32..44 |
| `INVALID_ORDER` | 10 | record field 0, 4, or 16 |
| `UNKNOWN_ABI_CONTRACT` | 11 | record field 12 |
| `INVALID_SPAN` | 12 | first invalid offset/length pair |
| `INVALID_DIGEST` | 13 | record-constructor field 64 |
| `RESOURCE_LIMIT` | 14 | header 8 or count field 12 |

Record-field offsets are reported as absolute WVFC offsets when validating or
constructing a catalog. The standalone record constructor reports offsets
relative to its 96-byte record.

## Canonical producer

`Compilerˉsourceˉforeignˉcatalogˉproduce` consumes one immutable, validated,
descriptor-free `WVSS 2` snapshot. `WVSS 1`, an unknown WVSS version, a source
set larger than 4,194,304 bytes, zero or more than 64 modules, malformed module
content, and trailing source-set bytes are rejected before catalog publication.
Structurally recognized WVSS 1 returns producer status `ADMITTED_WVSS`;
an unknown version returns `SOURCE_SET` with underlying source-set status
`Unsupported_version`. This preserves the source-set scanner's earlier
structural diagnostic instead of relabeling it as a recognized legacy input.
The producer performs the established bounded source-set validation and one
independent source-ordered declaration scan. It does not mutate the input and
returns an empty `Value` on every failure.

The declaration ordinal is the zero-based ordinal of **every non-`End` entry**
returned by the validated declaration scan. Imports and ordinary declarations
therefore consume ordinals even though they produce no WVFC record. Records are
emitted in module order and declaration order without sorting.

For each foreign declaration, the producer requires the frozen edition-1
System profile and the parser-authenticated `unsafe` marker, exact ABI literal,
nonempty external-symbol literal, and complete `effects(ffi.call)` clause. The
parser's exact ABI literal maps directly to registry identity 1; this is a
mapping from already authenticated syntax, not a general membership test over
arbitrary text. The producer reuses
`Compilerˉsourceˉforeignˉdeclarationˉevidence` and the existing WVFC record and
catalog constructors. It does not carry a second grammar.

The source spans are exact UTF-8 byte ranges:

- declaration: optional `export` through the terminating semicolon;
- name: identifier bytes;
- ABI and external symbol: literal contents, excluding quotes;
- effect: the complete `effects(ffi.call)` clause; and
- signature: raw source bytes from the authenticated `fn` token through the
  final effect-clause byte, excluding ABI/external literals and `as`.

For the three frozen foreign layouts, the signature begins at
`Name_offset - 3` and ends at
`Effect_clause_offset + Effect_clause_length`. The producer computes those
bounds only after parser evidence succeeds and guards the subtraction,
addition, containment, and nonempty length. Any future foreign layout must
revise this explicit parser/producer invariant; it must not silently inherit
the current arithmetic. Trivia and layout are part of the raw signature, so
two otherwise equivalent declarations with different admitted bytes
intentionally have different signature digests.

The digest is the exact host-independent SHA-256 from
`Foundationˉsha256.Foundationˉsha256ˉhex`, decoded by a compiler-owned decoder
that accepts exactly 64 lowercase ASCII hexadecimal bytes and publishes exactly
32 raw bytes. The source-owned implementation remains the selected dependency
for the retained producer generation and its recorded evidence. The implemented
native x64 candidate now lowers the semantic `Bytesˉsha256ˉhex` opcode, but that
later backend support does not retroactively change this producer's WVB or
justify an unmeasured substitution. Any migration to the intrinsic requires a
named producer rebuild, focused current-host measurement, and independent Linux
evidence for that rebuilt producer. The registered backend owner has separately
passed all eight cases on exact-current Windows and local Debian 13.5 under
WSL. It proves exact empty and `abc` known answers, a `bytes` parameter, and an
owned `text` digest returned across a helper boundary; it has not migrated or
rebuilt this producer. The Debian/WSL run is Linux development evidence, not
paired-host CI qualification. This is an implementation choice, not a change
to digest semantics.

Records accumulate in at most 65,536 pending bytes before a complete-chunk
flush, then materialize once for the canonical catalog constructor. This keeps
the append path bounded at the 43,690-record ceiling and avoids one whole-value
concatenation per record. A 683-record case crosses the first flush boundary
and verifies the catalog count plus first and last declaration ordinals.

Producer status values are `VALID` 0, `SOURCE_SET` 1, `ADMITTED_WVSS` 2,
`PROFILE` 3, `DECLARATION` 4, `ABI_CONTRACT` 5, `SPAN` 6, `DIGEST` 7,
`RESOURCE_LIMIT` 8, and `CATALOG` 9. The result also carries bounded module,
declaration, offset, line, and column evidence plus the underlying source-set,
declaration-parser, and catalog statuses. Success publishes only the canonical
constructor result. Failure never repairs input or publishes a partial record
sequence.

`Failureˉoffset` has one deterministic domain per phase. Source-set scan,
WVSS-version, validated-summary, and final-catalog failures retain their
upstream container or already-normalized offsets. Direct module-loop
`PROFILE`, `DECLARATION`, and `RESOURCE_LIMIT` failures report the original
pre-descriptor source offset by adding the WVSS 2 entry's `Originˉoffset`.
Private-record `DECLARATION`, `ABI_CONTRACT`, `SPAN`, and `DIGEST` failures are
normalized at the same return boundary. A private record-constructor `CATALOG`
failure reports the exact canonical absolute WVFC field offset, `48 +
record_count * 96 + record_relative_offset`; the final catalog constructor
retains its own absolute WVFC offset. Line and column already account for the
retained descriptor line ending and are not translated. Container and catalog
offsets are never relabeled as source offsets.

This producer records syntax only. It does not infer semantic types or effects,
grant authority, bind declarations or external symbols, select overloads,
compare platform scopes with a selected WVTD, construct WVAE, or affect the
runtime or linker. The source-admission coordinator owns selected-WVTD scope
agreement and WVAE construction around this deliberately narrower producer;
`wvauth` owns the independent post-read source/catalog/target proof.

## Focused evidence and remaining proof

The focused owner builds twice and requires byte-identical WVB, then executes
40 isolated Windvale cases. They cover the exact empty, single, multiple,
exported and byte-identical forms; stable ABI identity; truncated and trailing
input; magic, version, length, width, offset, flags and reserved corruption;
module bounds; count arithmetic; duplicate, reordered and overlapping records;
unknown ABI identity; nonempty/overflowing/out-of-declaration spans; required
effect evidence; and short/long constructor digests.

The exact 43,690/43,691 count boundary is checked both in Windvale through the
overflow-safe size function and independently with host `BigInt` arithmetic.
The fixture intentionally does not retain or execute a 4 MiB catalog merely to
prove multiplication.

The separate canonical-producer owner builds twice, complete-verifies the
immutable WVB, and executes 25 isolated direct-native cases. They cover strict
lowercase digest decoding; empty, single, exported, trailing-comma, and paper
layouts; independent fixed spans and digest; import/ordinary/foreign ordinal
accounting; within-module and cross-module order; deterministic output; WVSS 1,
unknown-version, and trailing-input rejection; System/unsafe/ABI/symbol/effect
requirements; the
64/65-module boundary; maximum-record arithmetic; canonical result structure;
and 683 records crossing the 65,536-byte accumulator flush. The flush case
checks the catalog count and first/last declaration ordinals, so a lost,
duplicated, or reordered boundary record cannot pass.

The complete, separately composed `wvauth` consumer builds byte-identically in
two initially empty split-compiler caches as a 91,774-byte compiler-aligned
WVB at SHA-256
`88eec2e572e03cdd87de3bedc01c555da3a246fd2d160a62246da0d39331f580`.
The earlier 108,847-byte historical-monolithic product is not the
production-ingress pin. Actual-product cases accept the authentic paper
foreign set and reject missing or extra records, flags, ABI, symbol, signature,
effect, digest, and target mismatches at their bounded source/catalog offsets.
It neither invokes this canonical producer nor constructs a second complete
catalog.

A later producer/consumer capacity workload may retain a representative near-
maximum WVSS and catalog when it can measure peak memory and both compiler
products without weakening the ordinary value limit. Current focused evidence
does not claim Analyzer semantic consumption, foreign
types/effects/ownership lowering, runtime containment, a full-capacity retained
workload, production-ingress acceptance, or paired Windows/Linux
qualification.
