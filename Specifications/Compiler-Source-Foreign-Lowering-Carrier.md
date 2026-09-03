# Compiler source Foreign lowering carrier

## Status

Candidate private compiler-phase contract, WVFB 1.0, implemented locally on
Windows on 2026-09-03. The
generic-aware Analyzer completes body binding first. A focused compiler-owned
builder then constructs the carrier from validated source, target, catalog, and
symbol facts and pairs it with the typed WVIR before publishing only to a
coordinator-selected private path. The coordinator independently validates,
retains, and re-pairs that exact file before WVB emission. The complete verifier
and source-built bounded scalar provider now accept registered WVB 1.38 binding
`1`; native and other execution consumers remain closed.

## Purpose and authority

WVFB carries the exact normalized callable facts needed by the later typed
Foreign-call lowering phase without importing the full catalog and semantic
closure into the size-constrained Analyzer. It is an internal compiler value,
not a distributable module, authentication certificate, capability grant,
provider handle, native address, or execution authority.

The private builder constructs WVFB only while it holds the WVSS, WVTD, WVFC,
and typed WVIR values that it completely validated or paired. A valid WVFB by
itself proves only its own shape. The implemented cross-process pairer binds it
to a symbol directory reconstructed from the retained WVSS and matches every
typed Foreign call in the paired WVIR. The coordinator retains the
authenticated inputs and rechecks their exact bytes after pairing.

All integers are unsigned little-endian values. All reserved fields must be
zero. The complete value is exactly `56 + RecordCount * 80` bytes, is at least
136 bytes, and is bounded to 5,176 bytes.

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
matched those identities to the exact WVFC record and WVSD entry; the pairing
phase matches that entry to the typed compiler product before WVB emission.

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

## Authenticated analysis pairing

The private pairing function consumes one structurally valid WVFB, one valid
source-symbol summary reconstructed from the retained WVSS, and one Analyzer-
produced WVIR. It returns only status plus the carrier-record and Foreign-call
counts; it publishes no transferable certificate or successor file.

The pairer requires conditional WVSD 1.2 with its exact 16-byte header,
24-byte entries, complete length, and one or more kind-9 Foreign entries. Those
entries must match the WVFB records one for one and in source order by module,
WVSD directory index, and fixed arity three. It accepts only WVIR 1.31 or 1.32,
bounds the 48-byte function, 28-byte block, and 28-byte operation tables before
reading them, and scans at most the declared operation count. Every operation
`190` target must identify one retained WVFB record. One WVIR is limited to
4,096 typed Foreign calls; a 4,097th call fails with `Callˉlimit`.

Failures distinguish invalid carrier, symbol, or WVIR structure; carrier/symbol
count or record disagreement; an unmatched Foreign operation; and the call
limit. This phase proves correlation only. It does not authenticate a source
set by itself, validate every unrelated WVIR field, grant a capability, resolve
a native symbol, or authorize WVB publication.

## Hosted publication and current containment

The production lowering builder returns WVFB only on complete success and
returns an empty carrier for every failure. After the Analyzer completes typed
and generic-aware analysis, the hosted driver accepts exactly:

```text
wvbind --internal-bind-analyzed <input.wvss> <input.wvtd>
    <input.wvfc> <input.wvir> <output.wvfb>
```

All five paths must be distinct. The driver validates the source, target,
catalog, symbols, and registered callable facts without repeating body or
generic binding. It then pairs the candidate carrier with the supplied typed
WVIR. Only after complete construction and pairing does it write the carrier
and one exact standard-output line:

```text
foreign binding status=Published source-bytes=<u32> source-sha256=<hex> target-bytes=<u32> target-sha256=<hex> catalog-bytes=<u32> catalog-sha256=<hex> carrier-bytes=<u32> carrier-sha256=<hex> foreign-count=<u32>\n
```

The line is at most 447 UTF-8 bytes under the current input bounds. It names the
bytes consumed and produced by that invocation but remains non-authoritative.

The production coordinator supplies a new path inside its private phase
directory. After the Analyzer and construction form of `wvbind` exit
successfully, it requires one ordinary,
single-link, 136-through-5,176-byte file and makes it read-only. It independently
checks the complete WVFB header and record geometry; exact retained WVTD target
tuple; record count; WVFC module, declaration, and record mapping; strictly
increasing module and WVSD directory identities; and every fixed callable fact.
It snapshots the carrier, constructs the expected evidence line from its own
retained WVSS, WVTD, WVFC, and WVFB bytes, requires byte-for-byte equality, and
then rechecks the original six authenticated snapshots plus WVFB identity and
bytes. Any missing, aliased, linked, malformed, substituted, or changed carrier
fails closed and the private tree is removed.

The coordinator has already required the Analyzer's WVSS copy to equal the
retained source set and retained its WVCA, WVLB, and WVIR files. It now
re-enters the existing binder executable for an independent pairing check:

```text
wvbind --internal-pair-analysis <input.wvss> <input.wvfb> <input.wvir>
```

All three paths must be distinct. The driver validates source symbols from WVSS,
applies the pairing contract above, writes no file, and emits exactly:

```text
foreign pairing status=Validated records=<u32> calls=<u32>\n
```

Only after that process succeeds does the coordinator recheck the original six
authenticated snapshots and retained WVFB again. It then invokes the emitter's
private paired-Foreign form with the retained WVSS, WVFB, and Analyzer products.
The emitter independently reconstructs source symbols, validates the exact
WVFB/WVSD/WVIR pairing, and emits candidate WVB 1.38 only on complete agreement.
The coordinator validates the private product, rechecks all six authenticated
snapshots plus WVFB after emission, and only then publishes it atomically.

The ordinary emitter form rejects Foreign-bearing source, and direct use of
`wvbind`, the private emitter switch, or WVFB cannot establish the preceding
authentication relationship or grant authority. Candidate WVB 1.38 records
registered binding identity `1`; it does not embed WVFB, grant a capability,
resolve a native symbol, create a native thunk, or load a dynamic library. The
complete verifier admits the exact call, and the source-built bounded scalar
provider executes identity `1` against private logical heap state. Native and
all other execution consumers remain closed to minor 38.
