# Compiler source Foreign lowering carrier

## Status

Candidate private compiler-phase contract, WVFB 1.0, on 2026-09-02. The
compiler-owned binder constructs and independently validates this carrier after
complete source, target, catalog, symbol, and body binding. No production
coordinator or emitter consumes it yet, so authenticated Foreign-call lowering
and execution remain unavailable.

## Purpose and authority

WVFB carries the exact normalized callable facts needed by the later typed
Foreign-call lowering phase without importing the full catalog and semantic
closure into the size-constrained Analyzer. It is an internal compiler value,
not a distributable module, authentication certificate, capability grant,
provider handle, native address, or execution authority.

The private binder constructs WVFB only while it holds the WVSS, WVTD, and WVFC
values that it completely validated. A valid WVFB by itself proves only its own
shape. Any later cross-process consumer must bind it to the coordinator's
retained authenticated inputs and independently match its records to the paired
WVLB/WVIR identities before it may guide lowering.

All integers are unsigned little-endian values. All reserved fields must be
zero. The complete value is exactly `56 + RecordCount * 80` bytes and is bounded
to 5,176 bytes.

## Header

| Offset | Size | Field | Required value |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | ASCII `WVFB` |
| 4 | 2 | Major | `1` |
| 6 | 2 | Minor | `0` |
| 8 | 4 | Header bytes | `56` |
| 12 | 4 | Record bytes | `80` |
| 16 | 4 | Record count | `1..64` |
| 20 | 4 | Total bytes | Exact complete length |
| 24 | 4 | Build target | Linux x86-64 SysV AMD64 C v1 (`4`) |
| 28 | 4 | Environment | Linux (`2`) |
| 32 | 4 | Architecture | x86-64 (`1`) |
| 36 | 4 | Native ABI | SysV AMD64 C v1 (`2`) |
| 40 | 4 | Address bits | `64` |
| 44 | 4 | Byte order | little-endian (`1`) |
| 48 | 4 | No-unwind scalar-pointer ABI major | `1` |
| 52 | 4 | Reserved | `0` |

## Record

Records follow source/catalog order. Module and directory identities are
strictly increasing. The exact registered Foreign name is unique in each
module, and the binder rejects a Foreign count greater than the module count,
so WVFB 1.0 contains at most one record per module. Module is less than `64`,
declaration is less than `4,096`, and catalog record equals the zero-based
record position.

| Relative offset | Size | Field | Required value |
| ---: | ---: | --- | --- |
| 0 | 4 | WVSS module | Bound source identity |
| 4 | 4 | Declaration ordinal | Bound source identity |
| 8 | 4 | WVSD directory entry | Bound symbol identity |
| 12 | 4 | WVFC record | Zero-based record position |
| 16 | 4 | Foreign ABI contract | Buffer SysV AMD64 C v1 (`1`) |
| 20 | 4 | Source-name identity | Buffer read (`1`) |
| 24 | 4 | External-symbol identity | Buffer read v1 (`1`) |
| 28 | 4 | Required profile | System (`3`) |
| 32 | 4 | Language effects | `ffi.call` only (`256`) |
| 36 | 4 | Parameter count | `3` |
| 40 | 4 | Destination kind | Foreign pointer (`4`) |
| 44 | 4 | Destination element | `u8` (`1`) |
| 48 | 4 | Destination ABI contract | `1` |
| 52 | 4 | Capacity kind | `u64` (`2`) |
| 56 | 4 | Expected-generation kind | `u64` (`2`) |
| 60 | 4 | Return kind | `i64` (`3`) |
| 64 | 4 | Flags | bit 0 unsafe, bit 1 no-retain, bit 2 no-unwind; exactly `7` |
| 68 | 12 | Reserved | Three zero `u32` values |

The record deliberately contains semantic identities instead of source text,
native symbol bytes, a host address, or a library path. The binder has already
matched those identities to the exact WVFC record and WVSD entry; a later
lowerer must still match the paired compiler products.

## Validation and failure order

The validator rejects in this order:

1. an outer length below 56 or above 5,176;
2. magic;
3. version;
4. fixed header geometry or total length;
5. zero, over-64, or length-inconsistent record count;
6. the exact target tuple;
7. the header reserved field;
8. each record's source bounds, catalog position, and normalized facts;
9. each record's reserved fields; and
10. strict module and directory order.

Validation reports the status, declared record count when available, failing
record, and byte offset. An invalid result returns an empty value. Successful
validation returns the unchanged input bytes, record count, record-count
sentinel as the failure record, and zero failure offset.

## Current containment

The current binder returns WVFB only on complete success and returns an empty
carrier for every failure. The existing `wvbind` driver still publishes only
its retained-input digest line, and the production coordinator still stops at
`Foreignˉloweringˉpending`. WVFB 1.0 does not add a WVIR operation, WVB import,
runtime operation, native thunk, symbol resolution, dynamic-library load, or
provider call.
