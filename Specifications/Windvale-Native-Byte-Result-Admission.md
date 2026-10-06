# Windvale native byte-result admission

## Status and scope

`WVRQ 1` and `WVRR 1` are runtime-private contracts for deciding whether a
native exported byte-result descriptor is contained by one verified source
owned by the current execution. Portable Windvale owns descriptor and range
admission. The host retains construction of range evidence from the verified
fragment and live allocations, real-memory copying after admission, and
teardown.

The constructor treats every address as an opaque pair of little-endian `u32`
limbs and never dereferences it. This preserves exact unsigned 64-bit range
arithmetic without requiring native `u64` lowering. No live address or result
byte is serialized into WVB, WVNF, WVO, or a native cache.

## Request envelope: `WVRQ 1`

The request is `64 + static count * 16` bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVRQ`, `0x51525657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact request length |
| 12 | 4 | static range count | At most 4,096 |
| 16 | 8 | result pointer | Opaque returned address; zero only for an empty result |
| 24 | 4 | result length | At most 4 MiB |
| 28 | 4 | result reserved | Zero |
| 32 | 8 | arena start | Nonzero execution-owned arena address |
| 40 | 4 | arena used | Committed prefix, at most 128 MiB |
| 44 | 4 | arena reserved | Zero |
| 48 | 8 | input start | Zero when no entry input exists; otherwise nonzero |
| 56 | 4 | input length | Zero when absent; at most 4 MiB when present |
| 60 | 4 | reserved | Zero |
| 64 | variable | static ranges | Exact `static range count` entries |

Each static range is one verified immutable data symbol:

| Relative offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 8 | start | Nonzero opaque address |
| 8 | 4 | available bytes | At most the 32 MiB native fragment limit |
| 12 | 4 | reserved | Zero |

Containment requires `pointer >= start`, an unsigned difference representable
in `u32`, `difference <= available`, and
`result length <= available - difference`. This admits exact-end zero-length
ranges and rejects arithmetic wraparound. A null pointer is admitted only with
zero length.

## Response envelope: `WVRR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVRR`, `0x52525657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on rejection or `48` on admission |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | Relevant request byte; request length on admission |
| 20 | 4 | descriptor bytes | Zero on rejection; exactly `16` on admission |
| 24 | 8 | reserved | Zero |

An admitted response appends the exact unchanged result descriptor. The host
verifies the response identity and descriptor equality before copying from the
admitted address.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact admitted descriptor follows |
| 1 | `Invalid_size` | Request or declared size is invalid |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_directory` | Count, extent, or header reserved state is invalid |
| 5 | `Invalid_descriptor` | Result reserved, length, or null relation is invalid |
| 6 | `Invalid_arena` | Arena address, committed length, or reserved state is invalid |
| 7 | `Invalid_input` | Input presence or length is invalid |
| 8 | `Invalid_static_range` | A static range is malformed |
| 9 | `Outside_owner` | The result is outside every admitted source |

Statuses 5 and 9 are ordinary rejection of untrusted native output. Other
nonzero statuses indicate invalid host-supplied evidence.

## Windvale owner and retained artifact

`Runtime/Windvale/Native-Byte-Result-Admission-Core.wv` owns validation and
containment. Its capability-free bridge exposes `Main(bytes) -> bytes`.

The core WVB is 7,078 bytes with SHA-256
`eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04`.
The retained bridge WVB is 7,057 bytes with SHA-256
`9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf`.
The normal runtime embeds only its 68,608-byte WVNF 1 artifact with SHA-256
`35c29fa9bbc41a00e8797f7812eb1bbf0f95c7f07b96227ca666cc5bf8fd38c2`.

The service-free bootstrap lane retains one frozen Stage 0 admission oracle so
the retained constructor can admit its own byte response without recursion.
Ordinary application results use Windvale. Any result representation, source
class, range bound, artifact, or response rule change requires a new accepted
version and Windows/Linux qualification.

## Candidate ABI-25 admission

[Decision 0980](../Documents/Decisions/0980-Bind-Native-Shared-Values-To-Budgets-And-Tool-Entries.md)
selects a separate WVRQ/WVRR version-two envelope for generation-checked shared
values. The current source parser implements that candidate alongside the
retained version-one path. The recorded artifacts above retain version-one
behavior; source changes do not update their identities or qualification.

WVRQ 2 is exactly `48 + 32 * BackingCount` bytes with at most 66 rows. The
header carries the unchanged sixteen-byte result descriptor, a nonzero domain
epoch and reserved zero bytes. Each row binds a full generation-checked handle
to an immutable pointer, logical length and admitted backing maximum. The
caller constructs those rows from complete live-domain validation and
inspection. The parser checks all rows before resolving the result's packed
identity and range. It does not prove operating-system mappings or pointer
authority.

A charged empty result retains its nonzero backing identity and requires its
live row. Its admitted maximum may be zero even when physical storage has
committed slack. The unbacked empty is the all-zero descriptor. Empty owning
ranges normalize their start to zero. Checked pointer addition rejects wrapping
backings and result addresses.

WVRR 2 has the existing 32-byte response header. Failure has no payload; success
has exactly 32 payload bytes: unchanged descriptor, resolved pointer and epoch.
The host rechecks descriptor and epoch equality, copies at most 4 MiB while
the domain remains live, then closes that same domain. The forty-case candidate
fixture covers these boundaries and a retained version-one empty response.
It is not yet execution or cross-host qualification evidence.

The candidate shared-compiler host calls this reader through the ordinary hosted
scalar entry in
[`Shared-Compiler-Byte-Result-Admission-Adapter.wv`](../Linker/Startup/Shared-Compiler-Byte-Result-Admission-Adapter.wv).
Its private `request` read and `response` write providers operate only on bounded
memory inside the older outer context. They permit one read followed by one
write, validate the exact resource names and extents, and copy the response
before the reader returns. The wrapper restores the previous provider table and
disables these bindings before admitting the response. This adapter changes
neither WVRQ/WVRR nor the historical descriptor-returning bridge; its packaging
does not establish qualification of the ABI-25 compiler.
