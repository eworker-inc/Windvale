# Windvale directory-service IPC

## Status and purpose

`WVDQ 1` is the implemented-candidate request contract owned by [Decision 0154](../Documents/Decisions/0154-First-Windvale-Directory-Service-Ipc.md). A successful service exchange returns the exact `WVDR 1` response already defined by the [read-only directory capability](Read-Only-Directory-Capability.md); it does not add a wrapper or a second filesystem result vocabulary.

This protocol binds one call to one already selected, rights-limited, immutable directory snapshot. The endpoint capability selects the instance. The message contains no host path, mount name, file descriptor, native handle, provider identifier, kernel pointer, or ambient namespace.

The protocol runs above the existing capacity-one, 4 KiB, format-blind bounded service exchange. User space owns every byte-level rule below. The kernel owns only endpoint identity, directional rights, bounds, generation, copied message state, peer lifecycle, and cleanup.

## `WVDQ 1` request

Every integer is unsigned little-endian. The complete request is a 28-byte header followed immediately by zero through 255 candidate name bytes:

| Offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Magic `WVDQ` |
| `4` | 4 | Version `1` |
| `8` | 4 | Exact complete request bytes |
| `12` | 4 | Read offset |
| `16` | 4 | Maximum returned chunk bytes |
| `20` | 4 | Candidate name bytes |
| `24` | 4 | Reserved zero |
| `28` | variable | Exact candidate name bytes |

The complete extent is 28 through 283 bytes. There is no padding, trailing data, operation field, instance field, or correlation identifier. Version 1 has one operation and one in-flight call per generation. The rights-limited endpoint identifies the bound instance, while the channel generation and capacity-one state prevent a reply from being confused with another call.

A semantically valid request carries the exact name, offset, and maximum from `filesystem.directory_read_v1`. The name is one 1-through-255-byte case-sensitive ordinal ASCII segment under the application-facing contract. The maximum is `0…3072`, and `offset + maximum` must fit `u32`.

The trusted runtime adapter should reject names that cannot fit the bounded request before IPC and construct the existing typed `WVDR Invalid_name` result locally. The service still treats every received byte as untrusted: a representable empty, dot, dot-dot, separator-bearing, colon-bearing, NUL, non-ASCII, or otherwise invalid candidate produces `WVDR Invalid_name`; an invalid maximum or checked range produces `WVDR Invalid_limit`. Name rejection has precedence when both are invalid, matching the Windvale library and Stage 0 runtime.

## Structural rejection

An extent outside 28 through 283 bytes, changed magic or version, inconsistent complete/name length, nonzero reserved field, or trailing bytes is structurally malformed. It is not a filesystem outcome and therefore has no invented `WVDR` status.

The service returns no reply for a structurally malformed request or an invalid provider response. In the live OS binding, the service endpoint must terminate or close the generation, causing the kernel's existing peer-exit cleanup to clear all retained message and destination state. A future checked runtime adapter maps an observed terminal service loss to the already defined `WVDR Peer_exited` result. The current candidate proves deterministic no-reply behavior and transport cleanup; it does not yet claim that guest adapter.

## Exact `WVDR 1` reply

A completed semantic request returns exactly 24 through 3,096 bytes under [Read-only directory capability version 1](Read-Only-Directory-Capability.md):

```text
u32 magic          0x52445657 (`WVDR` bytes)
u32 version        1
u32 status
u32 file_length
u32 returned_offset
u32 chunk_length
bytes chunk
```

No service header surrounds this value. The maximum reply is 3,096 bytes, leaving 1,000 bytes below the existing 4,096-byte transport limit. The service independently revalidates identity, status, offset, extent, exact successful chunk length, failure payload rules, and the caller's maximum before publication. Statuses `Invalid_name` and `Invalid_limit` are constructed locally and never invoke the bound provider.

## Windvale-owned service

[`Directory-Service-Core.wv`](../Operating-System/Services/Directory-Service-Core.wv) owns request parsing, name and checked-range validation, typed local rejection, and defense-in-depth `WVDR 1` validation. Structurally invalid requests and provider responses produce an empty service result so they cannot enter the bounded reply transition.

[`Directory-Service-Bridge.wv`](../Operating-System/Services/Directory-Service-Bridge.wv) is a temporary hosted integration adapter. It reads one opaque `ipc:directory-read.wvdq` input through `file.read_bytes`, invokes the separately authorized `filesystem.directory_read_v1` instance only for a valid request, and returns the core-approved bytes. It exists to prove the Windvale service policy against an independent Stage 0 oracle; it is not the Windvale OS provider or permanent host path.

[`Directory-Service-Ipc.cs`](../Operating-System/Windvale.Bootstrap/Directory-Service-Ipc.cs) independently implements construction, parsing, provider invariants, response verification, and hostile-input containment for Stage 0 recovery and differential evidence. [`Bounded-Service-Exchange.cs`](../Operating-System/Windvale.Bootstrap/Bounded-Service-Exchange.cs) is the shared format-blind transport oracle used by both resource and directory protocols.

## Current evidence and limits

The Windows focused OS suite proves deterministic WVDQ construction, the 283-byte request edge, exact 3,096-byte WVDR success, zero-length end reads, invalid offsets, missing and non-file entries, invalid name/limit non-invocation, malformed request no-reply behavior, malformed response rejection, provider invariant failure, 512 deterministic hostile request/response inputs, peer-exit cleanup, capability denial, missing request input, repeatable WVB construction, and byte-identical Windvale/Stage 0 results.

Probe 34, ABI 22, `WVKMEM13`, `WVPROC13`, `WVCHAN03`, `WVRES005`, firmware bytes, and QEMU identities remain unchanged. [Decision 0155](../Documents/Decisions/0155-First-Immutable-Windvale-Directory-Snapshot.md) now defines the separate one-page [`WVDS 1`](Windvale-Directory-Snapshot.md) provider value. Live guest adoption still requires a new measured probe with a dedicated page-sized client reply window, an init-owned RO/NX mapping, a checked runtime adapter, and service-failure mapping. This protocol adds no enumeration, nested paths, open handles, links, mutation, persistence, block storage, caching, concurrency, cancellation, transferable capability, or service discovery.

Reconsider version 1 when more than one call can be in flight, a reply cannot fit one bounded message, multiple instances share an endpoint, cancellation or fairness becomes measurable, or the provider needs semantics beyond an immutable read-at snapshot.
