# Windvale native capability-provider table

## Status and scope

`WVPQ 1`, `WVPR 1`, and `WVPT 1` are implemented candidate runtime-private
contracts for binding rights-limited native provider instances to an already
admitted WVB capability table. The portable constructor validates and copies
opaque native targets and instance-state addresses; it never dereferences them.

This contract does not change ABI 22, execution-context version 7, service-table
version 5, WVB 1.11, or WVO 1.0. It establishes the bounded table referenced by
the separately specified [provider-call candidate](Windvale-Native-Provider-Call.md).
It does not itself open a resource, grant authority, or publish an OS handle to
source.

All integers are little-endian. Every size and offset is relative to the
containing request or table and is checked before use.

## Capability identity

Each identity is the complete existing WVB capability record:

```text
u32      capability-name byte length
bytes    lowercase ASCII capability name
u32      parameter count
u8[]     parameter value types
u8       return value type
```

The name is 3 through 256 bytes, contains at least one dot, and consists of
dot-separated segments. A segment begins with `a` through `z`; later bytes may
also be digits or underscore. Empty segments are invalid. Identities are
strictly ordered by ordinal name and therefore cannot repeat.

Version 1 accepts zero through 64 parameters from the current scalar and
descriptor type codes: `i32` (1), `bool` (2), `text` (3), `u8` (4), `u32` (5),
`bytes` (6), `i64` (9), and `u64` (10). A return may additionally be `void` (0).
Nominal and collection-shaped provider signatures require a later table version.

The host derives these bytes from the independently verified WVB capability
section and must require byte-for-byte agreement before table publication. A
provider registry cannot substitute a different signature for the same name.

## Construction request: `WVPQ 1`

The 32-byte request header is followed by 1 through 32 fixed entries and their
contiguous identity bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPQ`, `0x51505657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact request length; at most 11,328 |
| 12 | 4 | capability count | 1 through 32 |
| 16 | 4 | provider mask | Nonzero; no bit at or above count |
| 20 | 4 | entry bytes | `24` |
| 24 | 4 | identity bytes | Exact trailing byte count |
| 28 | 4 | reserved | Zero |

Each request entry is:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | identity length | Exact next identity length |
| 4 | 4 | reserved | Zero |
| 8 | 8 | provider target | Opaque nonzero native target when its mask bit is set; otherwise zero |
| 16 | 8 | provider state | Opaque nonzero rights-limited instance state when its mask bit is set; otherwise zero |

The mask selects identities dispatched through this provider table. An admitted
application may retain separately specified fixed runtime services for other
capabilities during migration. The execution boundary must nevertheless prove
that every capability call has exactly one authorized binding; neither an
absent provider entry nor a fixed service is an implicit grant.

Requiring both target and state for a selected entry prevents a stateless code
pointer from becoming ambient authority. A semantically stateless provider
still receives a nonzero execution-owned authorization record. Targets and
state never enter WVB, WVO, package metadata, diagnostics, or source values.

## Construction response: `WVPR 1`

Every response begins with 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPR`, `0x52505657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure; header plus table on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; request length on success |
| 20 | 4 | table bytes | Zero on failure; exact `WVPT` bytes on success |
| 24 | 8 | reserved | Zero |

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact provider table follows |
| 1 | `Invalid_size` | Physical or declared size, identity coverage, or trailing bytes differ |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_count` | Count is zero or above 32 |
| 5 | `Invalid_mask` | Mask is empty or names an entry outside count |
| 6 | `Invalid_layout` | Entry width or a reserved field differs |
| 7 | `Invalid_identity` | Name, signature, type, or identity extent is invalid |
| 8 | `Invalid_order` | Identity names are not strictly increasing |
| 9 | `Invalid_binding` | Target/state presence differs from its mask bit |

## Provider table: `WVPT 1`

The successful table repeats the 32-byte header shape using magic `WVPT`
(`0x54505657`), then contains `count * 24` entries and the exact copied identity
stream. Header fields at offsets 12 through 28 retain count, mask, entry width,
identity bytes, and zero reserved respectively.

Each table entry contains:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | identity offset | Relative to the start of `WVPT`; points after all entries |
| 4 | 4 | identity length | Exact bounded identity bytes |
| 8 | 8 | provider target | Exact opaque input value |
| 16 | 8 | provider state | Exact opaque input value |

The table owner keeps it immutable for one native execution. Provider state may
refer only to an independently admitted, rights-limited object whose lifetime
contains every call and whose teardown waits for all calls to finish. Generated
code selects by its verified capability ordinal; it does not search names or
retain target/state addresses after return.

## Random-access storage binding

The first planned stateful consumer is `storage.random_access_v1`. Its provider
state will own exactly one pre-opened mutable object, a nonzero generation, one
whole-object writer fence, bounded transfer scratch, revocation state, and a
test-only failure-injection policy. The eventual target must validate the five
typed arguments again before host I/O and return one independently checked
`WVSA 1` envelope.

The binding table does not weaken the storage contract: no native path or handle
is supplied by generated code, stale generations cannot mutate, partial writes
report exact progress, and uncertainty after mutation becomes `Indeterminate`.
The generated call emission, main-lowerer selection, and one describe-only
execution probe are implemented candidates. The probe constructs the exact
identity and target/state pair in execution-owned memory and performs no host
I/O. Product capability admission, Windows and Linux file providers, open/lock
lifecycle, flush implementation, and process/power-failure evidence remain the
next implementation boundary.

## Ownership and evidence

`Runtime/Windvale/Native-Capability-Provider-Table-Core.wv` owns validation and
construction. Its capability-free bridge exposes `Main(bytes) -> bytes` for
artifact reconstruction. The focused native database-storage owner compiles the
constructor and executes a self-test as a native Windows application while also
constructing the exact Linux application.

The test covers a selected stateful provider beside an unselected fixed-service
identity, exact output offsets and copied signatures, deterministic repeated
construction, the maximum 32-entry mask, and malformed size, magic, version,
count, mask, layout, reserved, name, order, signature-type, target, and state
cases. An eighth case executes the exact storage call against the bounded
describe probe on Windows and constructs its Linux package. Independent Linux
execution and real platform provider leaves remain qualification requirements.
