# Windvale native execution-context construction

## Status and scope

`WVXQ 1` and `WVXR 1` are runtime-private contracts for constructing the
initial 112-byte [native execution context](Windvale-Native-Execution-Context.md#execution-context-memory-layout)
from already allocated execution resources. Portable Windvale owns the exact
version-7 bytes and validates the relationships among budgets, arenas,
arguments, and optional binding tables. The host retains allocation, native
pointer acquisition, copying into execution-owned memory, invocation,
post-execution mutation checks, and teardown.

All integers are little-endian. Unknown versions, truncation, trailing bytes,
zero budgets, noncanonical arena bounds, inconsistent optional pointers, and
nonzero initial mutable or reserved fields are rejected. Windvale treats all
pointers as opaque integers and never dereferences them.

## Request envelope: `WVXQ 1`

The request is exactly 120 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVXQ`, `0x51585657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `120` |
| 12 | 4 | flags | Bits 0 through 4 identify service, argument, output, file-input, and file-output table presence |
| 16 | 8 | instruction budget | Nonzero |
| 24 | 8 | call-depth budget | Nonzero |
| 32 | 8 | service-table pointer | Presence exactly matches flag bit 0 |
| 40 | 8 | record-arena pointer | Nonzero opaque target |
| 48 | 4 | record-arena bytes | Exactly 2 MiB |
| 52 | 4 | record-arena used | Zero initially |
| 56 | 8 | text-arena pointer | Nonzero opaque target |
| 64 | 4 | text-arena bytes | Exactly 128 MiB |
| 68 | 4 | text-arena used | Zero initially |
| 72 | 4 | service-failure detail | Zero initially |
| 76 | 4 | reserved | Zero |
| 80 | 8 | argument-table pointer | Presence exactly matches flag bit 1 |
| 88 | 4 | argument count | Zero when absent; 1 through 67 when present |
| 92 | 4 | argument reserved | Zero |
| 96 | 8 | output-table pointer | Presence exactly matches flag bit 2 |
| 104 | 8 | file-input-table pointer | Presence exactly matches flag bit 3 |
| 112 | 8 | file-output-table pointer | Presence exactly matches flag bit 4 |

Bits above bit 4 are invalid. The request mirrors context offsets 8 through
111 after its 16-byte envelope prefix. A table pointer is optional because its
corresponding service family may be absent; both arenas remain allocated at
the current normal-host bounds even when one execution does not consume them.

## Response envelope: `WVXR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVXR`, `0x52585657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `144` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; `120` on success |
| 20 | 4 | context bytes | Zero on failure; `112` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by exact context version `7`, size `112`, and
request bytes 16 through 119. The host independently checks every field before
copying the context. After execution, only record-arena used at context offset
44, text-arena used at offset 60, and service-failure detail at offset 64 may
change. Both counters must remain within their supplied arenas, the detail
must remain in the defined zero-through-twelve range, and every other byte
must still equal the admitted initial context.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact initial execution context follows |
| 1 | `Invalid_size` | Request length or declared length is not 120 |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_initial_state` | Flags, reserved fields, or mutable initial fields are invalid |
| 5 | `Invalid_budget` | Either execution budget is zero |
| 6 | `Invalid_service_table` | Service-table presence differs from flag bit 0 |
| 7 | `Invalid_record_arena` | Record-arena target or bound is noncanonical |
| 8 | `Invalid_text_arena` | Text-arena target or bound is noncanonical |
| 9 | `Invalid_arguments` | Argument target/count presence or limit is invalid |
| 10 | `Invalid_binding_table` | Output or file-table presence differs from its flag bit |

## Windvale owner, retained artifact, and bootstrap seam

`Runtime/Windvale/Native-Execution-Context-Core.wv` owns request validation
and exact initial-context construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 5,530 bytes with SHA-256
`dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b`.
The retained bridge WVB is 5,531 bytes with SHA-256
`86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68`.
The normal runtime embeds only its 58,363-byte WVNF 1 artifact with SHA-256
`acdfc7d71b5fc2f0c1cfd76242fddc59db2563a4026ac286313711f0e2eb05de`.

Ordinary application execution uses that Windvale constructor. Executing the
constructor itself still needs a context, so the explicitly service-free
bootstrap path uses one frozen Stage 0 byte writer. Focused evidence requires
its output to match Windvale byte for byte. That oracle is a temporary
bootstrap/recovery seam, not normal application ownership, and keeps the
complete .NET-retirement gate open until a later bootstrap-host slice replaces
it.

Any context version, size, arena bound, presence rule, mutable-field set,
artifact, or bootstrap change requires a new accepted contract version and
Windows/Linux qualification.
