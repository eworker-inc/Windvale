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
positions, and byte-identical accepted output. It does **not** yet construct the
catalog in `wvadmit`, authenticate spans against WVSS, recompute signature
digests, prove catalog completeness, produce WVAE, feed the Analyzer, perform
semantic FFI validation, bind a native symbol, or execute foreign code. Those
are subsequent producer/consumer migration boundaries.

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
The future admission producer maps the authenticated ordinary text literal to
this row. A WVFC consumer accepts no other numeric identity in version 1.0.
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
prove multiplication. A later producer/consumer capacity workload must add a
representative large WVSS and catalog when it can measure peak memory and both
compiler products without weakening the ordinary value limit.

This structural evidence does not prove WVSS span authenticity, literal text,
signature SHA-256 content, declaration completeness, target agreement, System
profile, semantic types/effects/ownership, Analyzer capacity, native lowering,
runtime containment, or Windows/Linux qualification.
