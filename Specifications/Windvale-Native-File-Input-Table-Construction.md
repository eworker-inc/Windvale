# Windvale native file-input-table construction

## Status and scope

`WVNQ 1` and `WVNR 1` are runtime-private contracts for constructing the exact
136-byte [`WVFI 1`](Windvale-Native-Execution-Context.md#runtime-private-file-input-table)
initial binding table from already allocated snapshot, name, data, and path
scratch ranges plus already resolved platform function targets. Portable
Windvale owns the immutable table layout. The host retains arena allocation,
Windows export resolution, native table allocation/copy, post-execution
snapshot verification, and teardown.

This constructor owns only the generic 1,048,576-byte name-stride instance. It
rejects the hosted build-driver's 8,192-byte value; that profile constructs and
admits its embedded table through its separate hosted metadata, runtime-header,
and container boundary without widening this request or response contract.

All integers are little-endian. Unknown versions, nonzero initial count or
reserved fields, truncation, trailing bytes, incorrect capacities, and invalid
platform-function combinations are rejected.

## Request envelope: `WVNQ 1`

The request is exactly 136 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVNQ`, `0x514E5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `136` |
| 12 | 4 | platform | `1` Windows or `2` Linux |
| 16 | 8 | snapshot-table pointer | Opaque nonzero execution-owned target |
| 24 | 4 | snapshot capacity | `64` |
| 28 | 4 | initial snapshot count | Zero |
| 32 | 8 | name-arena pointer | Opaque nonzero execution-owned target |
| 40 | 4 | name stride | `1,048,576` for this generic constructor; the hosted build-driver runtime owns the separately admitted `8,192` specialization |
| 44 | 4 | name reserved | Zero |
| 48 | 8 | data-arena pointer | Opaque nonzero execution-owned target |
| 56 | 4 | data stride | `4,194,304` |
| 60 | 4 | maximum data bytes | `4,194,304` |
| 64 | 8 | path scratch pointer | Opaque nonzero execution-owned target |
| 72 | 4 | path scratch bytes | Windows `2,097,154`; Linux `1,048,577` |
| 76 | 4 | reserved | Zero |
| 80 | 56 | Windows functions | Seven nonzero opaque targets on Windows; all zero on Linux |

The seven function ranges retain their `WVFI` order: UTF-8 conversion, open,
size, read, close, commit, and last-error. Windvale validates only the required
zero/nonzero shape and copies each opaque eight-byte range; portable source
does not allocate, acquire, or dereference a host pointer.

## Response envelope: `WVNR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVNR`, `0x524E5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `168` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; `136` on success |
| 20 | 4 | table bytes | Zero on failure; `136` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by the exact initial `WVFI 1` table. The
managed adapter independently checks every immutable field against its
allocated arenas and resolved function list before copying. After execution it
permits only the bounded snapshot-count mutation and verifies every published
snapshot record and name.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact initial table follows |
| 1 | `Invalid_size` | Request length or declared length is not 136 |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_reserved` | Initial count or a reserved field is nonzero |
| 5 | `Invalid_platform` | Platform is not Windows or Linux |
| 6 | `Invalid_snapshot` | Snapshot target or capacity is invalid |
| 7 | `Invalid_name_arena` | Name target or stride is invalid |
| 8 | `Invalid_data_arena` | Data target, stride, or maximum is invalid |
| 9 | `Invalid_scratch` | Scratch target or platform capacity is invalid |
| 10 | `Invalid_functions` | A Windows function is absent or a Linux function is present |

## Windvale owner and retained artifact

`Runtime/Windvale/Native-File-Input-Table-Core.wv` owns request validation and
exact initial `WVFI` construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 5,078 bytes with SHA-256
`0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438`.
The retained bridge WVB is 5,084 bytes with SHA-256
`e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9`.
The normal runtime embeds only its 52,334-byte WVNF 1 artifact with SHA-256
`378240d8f8770a4707d7f2ae86daae24036fc2eb9fd273d5ab737c9c03e3e70d`.

Any change to this generic constructor's format, exact capacities, function
ordering, platform identity, artifact, or bootstrap path requires a new
accepted contract version and Windows/Linux qualification.
