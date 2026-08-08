# Windvale native entry-bridge construction

## Status and scope

`WVJQ 1` and `WVJR 1` are runtime-private contracts for constructing the
caller-owned descriptor bridge used by native `Main() -> bytes` and
`Main(bytes) -> bytes`. Portable Windvale owns the exact initial 16-byte result
cell and optional 16-byte immutable input descriptor. The host retains input
allocation, real-address projection, bridge allocation/copy, invocation,
post-call immutable-field verification, result-range admission, result copying,
and teardown.

The constructor treats the input pointer as an opaque little-endian integer and
never dereferences it. No process pointer or live input byte is serialized into
WVB, WVNF, WVO, or a native cache.

## Request envelope: `WVJQ 1`

The request is exactly 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVJQ`, `0x514A5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exactly `32` |
| 12 | 4 | flags | Bit 0 means byte input is present; all other bits are zero |
| 16 | 8 | input pointer | Nonzero opaque address exactly when input is present |
| 24 | 4 | input length | Zero when absent; at most 4 MiB when present |
| 28 | 4 | reserved | Zero |

A present zero-length byte input retains a nonzero pointer because it is a
distinct admitted entry shape with an execution-owned allocation.

## Response envelope: `WVJR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVJR`, `0x524A5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure, `48` or `64` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | Relevant request byte; request length on success |
| 20 | 4 | bridge bytes | Zero on failure; `16` without input or `32` with input |
| 24 | 8 | reserved | Zero |

On success, the header is followed by the bridge bytes published to native
code. The first 16 bytes are a zero result descriptor. When byte input is
present, the second 16 bytes contain its opaque pointer, length, and a zero
reserved word. The host independently verifies the initial bytes before
allocation and requires the complete second descriptor to remain byte-for-byte
unchanged after native return, including a trapped return.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact bridge follows |
| 1 | `Invalid_size` | Request or declared length is not exactly 32 |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_flags` | Flags or reserved bytes are invalid |
| 5 | `Invalid_presence` | Pointer presence does not match the input flag |
| 6 | `Invalid_length` | Absent input has bytes or present input exceeds 4 MiB |

## Windvale owner and retained artifact

`Runtime/Windvale/Native-Entry-Bridge-Core.wv` owns request validation and
exact bridge construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 3,385 bytes with SHA-256
`8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae`.
The retained bridge WVB is 3,401 bytes with SHA-256
`d66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1`.
The normal runtime embeds only its 37,374-byte WVNF 1 artifact with SHA-256
`2abde6462aa470f4037aa87ae486f16f2a106932d3022344e85fa5763d44623b`.

The service-free bootstrap lane retains one frozen Stage 0 bridge oracle solely
to execute this and other retained Windvale constructors without recursive
bridge construction. Ordinary application execution does not use that oracle.
Any entry shape, descriptor representation, byte limit, mutable-field rule, or
artifact change requires a new accepted contract version and Windows/Linux
qualification.
