# Standard byte-output capability

## Status and scope

`standard_output.write_v1(bytes) -> bytes` is the first semantic provider
binding for the portable standard byte-output core. It preserves arbitrary
bytes and does not append, decode, normalize, or transcode them. The capability
instance denotes one rights-limited standard-output stream; it is not a native
handle, host path, file resource, terminal, or general duplex stream.

The first hosted adapter admits one value of at most 65,536 bytes and a stream
lifetime total of at most 4 MiB. A capability call returns one provider-owned
`WVOW 1` response. The response borrow ends at the next call to the same
provider instance.

## `WVOW 1` response

All integers are little-endian. The response is exactly 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVOW` (`0x574f5657`) |
| 4 | 4 | version | `1` |
| 8 | 4 | response bytes | `32` |
| 12 | 4 | status | One value below |
| 16 | 4 | progress | Exact locally consumed prefix; no greater than the request |
| 20 | 4 | reserved | Zero |
| 24 | 8 | provider generation | Nonzero; rules below |

| Value | Name | Progress and generation rule |
| ---: | --- | --- |
| 0 | `Completed` | Complete request length; current generation |
| 1 | `Rejected` | Zero; current generation; no mutation dispatched |
| 2 | `Peer_closed` | Exact known prefix; current generation |
| 3 | `Provider_lost` | Exact known prefix; current generation |
| 4 | `Stale` | Zero; nonzero replacement generation different from current |
| 5 | `Revoked` | Zero; nonzero replacement generation different from current |
| 6 | `Indeterminate` | Exact known prefix; current generation |

`Completed` means the local semantic sink consumed the complete value; it does
not prove display, remote receipt, file durability, or application commit.
`Rejected`, `Stale`, and `Revoked` are the only pre-dispatch outcomes. Every
other non-complete outcome follows mutation dispatch. A caller must not retry
an indeterminate suffix automatically.

The hosted Windvale adapter validates every response field and translates the
result through `Windvaleˉstandardˉbyteˉoutputˉcore`. A malformed response is
conservatively treated as an indeterminate dispatched mutation. Native,
browser, and Windvale OS providers must preserve this contract rather than
define host-specific output semantics.

`Standard-Byte-Output-Response-Core.wv` owns the capability-free decoder. Its
20-case self-test accepts every defined status shape and rejects truncated,
wrong-magic, wrong-version, wrong-size, unknown-status, excessive-progress,
reserved-field, zero-generation, stale-generation, and completion mismatches.
The hosted Windows and Linux leaves then prove exact output through the ordinary
`file-read` application; neither leaf is a general file or terminal provider.
