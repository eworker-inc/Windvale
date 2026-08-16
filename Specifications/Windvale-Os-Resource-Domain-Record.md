# Windvale OS resource-domain record

## Status and scope

Resource-domain record 1 is the fixed 64-byte `WVDOM001` kernel-state record
used by the current Probe 40 filesystem provider. It durably identifies one
alive domain and its exact committed process, user-page, and endpoint charge.
It is not a public syscall ABI, general object table, capability, allocator,
dynamic membership interface, or replacement for the immutable transition
rules in [Windvale-Os-Resource-Domain-Policy.md](Windvale-Os-Resource-Domain-Policy.md).

The first instance occupies state-page bytes `0x860..0x89F`, after exact
validation of the terminal directory endpoint formerly stored there. The
retained directory channel at `0x7F0` is not reinterpreted. Reuse is legal only
because the endpoint is closed, has its final provider/client identities and
resolution counts, and has no remaining capability authority.

## Encoding

All integers are unsigned little-endian `u32` values.

| Offset | Bytes | Field | Filesystem value |
| ---: | ---: | --- | ---: |
| `0x00` | 8 | magic | ASCII `WVDOM001` |
| `0x08` | 4 | version | `1` |
| `0x0C` | 4 | record bytes | `64` |
| `0x10` | 4 | lifecycle | `0` (`Alive`) |
| `0x14` | 4 | domain reference | `65538` |
| `0x18` | 4 | owner process reference | `196610` |
| `0x1C` | 4 | process limit | `1` |
| `0x20` | 4 | user-page limit | `81` |
| `0x24` | 4 | endpoint limit | `1` |
| `0x28` | 4 | committed processes | `1` |
| `0x2C` | 4 | committed user pages | `81` |
| `0x30` | 4 | committed endpoints | `1` |
| `0x34` | 4 | reserved processes | `0` |
| `0x38` | 4 | reserved user pages | `0` |
| `0x3C` | 4 | reserved endpoints | `0` |

The domain reference is generation 1, identifier 2. The owner is process
generation 3, identifier 2. The page count excludes the provider's four
kernel-only paging pages and charges its 81 user-owned pages. The single
endpoint charge covers provider-side endpoint reference `131072`; client 0
still grants no consumer authority.

The selected filesystem consumer uses the same versioned encoding with these
field substitutions:

| Field | Consumer value |
| --- | ---: |
| domain reference | `65540` |
| owner process reference | `131075` |
| process limit / committed | `1 / 1` |
| user-page limit / committed | `6 / 6` |
| endpoint limit / committed | `0 / 0` |

The consumer owns a rights-limited capability to the provider-owned endpoint;
it does not own or charge the endpoint object. Its kernel-state storage is not
yet selected or published. Live construction must validate a terminal record
slot before writing these bytes.

## Publication and lifetime

Construction clears the retired 64-byte slot and writes every non-magic field
while the record is private. The final commit writes the low magic word and
then the high magic word. Readers accept the record only when both words and
all fixed fields validate, so the second word is the publication point in the
current single-CPU boot transcript. The domain is published before endpoint,
thread, and process ready states.

The record remains live while the filesystem process, its 81 user pages, or
its endpoint remains committed. A later sequential network launch may reclaim
this slot only after checked filesystem stop, zero committed use, endpoint
closure, memory release, and exact terminal-record validation. Generation and
owner identities must change rather than silently treating the filesystem
record as a network record.

## Evidence and limits

[`Resource-Domain-Record.wv`](../Operating-System/Kernel/Resource-Domain-Record.wv)
constructs and validates the exact record. The `os-resource-domain` owner
builds the policy and record as separate Project 2 modules and executes both
exact filesystem records, cross-profile identity rejection, and truncated,
oversized, wrong-magic, version, header, identity, limit, reservation, and
committed-use cases alongside the policy lifecycle cases.
The boot construction independently preflights the retired endpoint and writes
the same bytes in the live kernel state page.

Version 1 has no peak counters, stop reason, thread/handle/CPU/DMA accounting,
concurrent mutation protocol, or public discovery. Those require a successor
record and measured lifecycle operations; they must not be inferred from this
fixed boot-owned ledger.
