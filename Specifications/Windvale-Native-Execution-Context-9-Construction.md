# Windvale native execution-context version 9 construction

## Status and scope

`WVXQ 2` and `WVXR 2` are implemented candidate runtime-private contracts for
constructing the planned 136-byte execution-context version 9. They retain every
context-7 field and append the allocator reservations from Decision 0151 plus the
capability-provider table pointer from Decisions 0537 and 0538.

This constructor does not replace the current [`WVXQ 1` / `WVXR 1`
constructor](Windvale-Native-Execution-Context-Construction.md), publish ABI 23,
or make an ABI-22 fragment read the added fields. The host must still validate
the complete `WVPT 1` table and its agreement with verified WVB identities before
using this candidate context.

All integers are little-endian. Pointers are opaque nonzero integers; portable
Windvale validates and copies them but never dereferences them.

## Request envelope: `WVXQ 2`

The request is exactly 144 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVXQ`, `0x51585657` |
| 4 | 4 | version | `2` |
| 8 | 4 | total bytes | `144` |
| 12 | 4 | flags | Bits 0 through 5 identify service, arguments, output, file input, file output, and provider table |
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
| 120 | 8 | allocator-state pointer | Required zero in this candidate |
| 128 | 8 | allocator-leaf pointer | Required zero in this candidate |
| 136 | 8 | capability-provider table pointer | Presence exactly matches flag bit 5 |

Bits above bit 5 are invalid. The allocator fields are explicit rather than
omitted or repurposed: nonzero values require the still-unpublished allocator
integration contract. The provider pointer may be absent for a capability-free
fragment; later ABI-23 admission must require it when a selected capability uses
provider dispatch.

## Response envelope: `WVXR 2`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVXR`, `0x52585657` |
| 4 | 4 | version | `2` |
| 8 | 4 | total bytes | `32` on failure or `168` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; `144` on success |
| 20 | 4 | context bytes | Zero on failure; `136` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by context format version `9`, structure bytes
`136`, and request bytes 16 through 143. This maps request offsets 120, 128, and
136 to context offsets 112, 120, and 128 respectively. Existing context offsets
0 through 111 remain byte-for-byte compatible with their version-7 meaning.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact initial context 9 follows |
| 1 | `Invalid_size` | Physical or declared request size differs |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_initial_state` | Flags, reserved, or mutable initial fields are invalid |
| 5 | `Invalid_budget` | Either execution budget is zero |
| 6 | `Invalid_service_table` | Service-table presence differs from flag bit 0 |
| 7 | `Invalid_record_arena` | Record arena target or bound is noncanonical |
| 8 | `Invalid_text_arena` | Text arena target or bound is noncanonical |
| 9 | `Invalid_arguments` | Argument target/count presence or limit is invalid |
| 10 | `Invalid_binding_table` | Output or file-table presence differs from its flag |
| 11 | `Invalid_allocator_reservation` | Either unpublished allocator field is nonzero |
| 12 | `Invalid_provider_table` | Provider-table presence differs from flag bit 5 |

Post-call mutability remains the context-7 set: record-arena used, text-arena
used, and service-failure detail. The allocator and provider pointers must remain
unchanged for the complete execution and teardown waits for all provider calls.

## Ownership and evidence

`Runtime/Windvale/Native-Execution-Context-9-Core.wv` owns validation and exact
construction. Its `Main(bytes) -> bytes` bridge is capability-free. The core WVB
is 5,986 bytes with SHA-256
`f2ae414fe2be7ed6ad25b555bd41ce2c71701017f2400bb835d274af19013ed8`;
the bridge is 5,979 bytes with SHA-256
`347ec3c7083493eb1b9f79967ab2109eb2bac4607180db6cf77b6cad2403bb5a`.

The focused self-test builds as a 13,833-byte WVB at SHA-256
`2da0ea6deb6a00d722300d05b6a10a46d2ae91b01a029807a3279fee71d69b17`
and lowers to a verified 140,298-byte ABI-22 test object at SHA-256
`2244b1a5fd398d933690187639a06e694ee8fef89ada5dfbc244bf45855b5ac7`.
Its 157,696-byte Windows package returns zero, and the same fragment constructs a
159,744-byte Linux package. The test exercises this constructor as portable
logic; it does not claim an ABI-22 application executes with context 9.

Main-lowerer selection, ABI-23 object identity, fragment-verifier admission,
host allocation/copy/post-call checks, and independent Linux execution remain
qualification requirements.
