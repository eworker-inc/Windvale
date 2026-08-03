# Windvale resource-service IPC

## Status and purpose

`WVRQ 1` and `WVRY 1` are the implemented-candidate bounded request/reply contracts owned by [Decision 0129](../Documents/Decisions/0129-Bounded-Resource-Service-Request-Reply.md). [Decision 0135](../Documents/Decisions/0135-Bounded-Guest-Resource-Request-Reply.md) adopts one exact exchange in the live Probe-33 guest through `WVCHAN02`. They carry one opaque resource-name lookup to a user-space service and return at most one 4 KiB message. The kernel transport copies bytes and enforces endpoint rights and lifecycle; only user space interprets the resource name or response format.

This is a narrow resource-service protocol, not a general IPC ABI or filesystem. It provides no host paths, directories, enumeration, handles, mutation, storage device, service discovery, transferable capability, or ambient namespace.

## Limits and encoding

Every integer is unsigned little-endian. Names are opaque strict UTF-8, 1 through 1,024 encoded bytes, with NUL forbidden. A name is not split, normalized, case-folded, or interpreted as a native path.

| Limit | Value |
| --- | ---: |
| Complete request | 32 through 1,056 bytes |
| Complete response or transport message | 1 through 4,096 bytes |
| Encoded resource name | 1 through 1,024 bytes |
| Inline response data | 0 through 3,984 bytes |
| Response digest | 64 lowercase ASCII SHA-256 characters |
| In-flight messages | One |

The 3,984-byte data limit leaves the 112-byte `WVRY 1` header inside one 4 KiB message. Version 1 supports only lookup operation `1`.

## `WVRQ 1` request

The complete request is a 32-byte header followed immediately by the exact encoded name. There is no padding or trailing data.

| Offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Magic `WVRQ` |
| `4` | 4 | Format version `1` |
| `8` | 4 | Exact complete request bytes |
| `12` | 4 | Nonzero client-chosen request identifier |
| `16` | 4 | Operation, exactly `1` for lookup |
| `20` | 4 | Maximum accepted response-data bytes, `0…3984` |
| `24` | 4 | Encoded name bytes, `1…1024` |
| `28` | 4 | Reserved zero |
| `32` | variable | Opaque strict-UTF-8 resource name |

The total must equal `32 + name bytes`. Malformed size, magic, version, identifier, operation, response limit, name extent, reserved field, UTF-8, NUL, or trailing data is rejected before store lookup.

## `WVRY 1` response

The response is a 112-byte header followed by zero through 3,984 inline data bytes. Successful data is a copied immutable snapshot; it does not borrow the service's store mapping.

| Offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Magic `WVRY` |
| `4` | 4 | Format version `1` |
| `8` | 4 | Exact complete response bytes |
| `12` | 4 | Echoed request identifier |
| `16` | 4 | Response status |
| `20` | 4 | Failure-offset domain |
| `24` | 4 | Failure offset within that domain |
| `28` | 4 | Resource identifier on success, otherwise zero |
| `32` | 4 | Resource kind on success, otherwise zero |
| `36` | 4 | Resource attributes on success, otherwise zero |
| `40` | 4 | Inline data bytes |
| `44` | 4 | Reserved zero |
| `48` | 64 | Lowercase ASCII SHA-256 on success, otherwise all zero |
| `112` | variable | Exact inline resource data |

Successful responses require status `0`, nonzero request and resource identifiers, known `WVRS 1` kind `1…3`, attributes exactly `7`, failure domain and offset zero, exact extent coverage, and digest equality.

Failure responses have no resource metadata or data and carry an all-zero digest. Statuses and their only valid failure domains are:

| Status | Meaning | Domain |
| ---: | --- | ---: |
| `0` | Success | `0` none |
| `1` | Malformed request | `1` request |
| `2` | Name not found | `0` none |
| `3` | Resource exceeds the request's response limit | `1` request; offset `20` |
| `4` | Invalid complete `WVRS 1` store | `2` store |

A malformed request may produce request identifier zero when no valid nonzero identifier was available. Every other response requires a nonzero echoed identifier. The invalid-store response exposes the first `WVRS 1` validation offset but not partially validated resource data.

## Transport ownership and lifecycle

The Stage 0 `Resourceˉserviceˉexchange` is a format-blind oracle for the intended bounded transport. It owns one copied message and two generation-1 endpoints:

- client endpoint `0x00010000`: `send-request` and `receive-reply`;
- service endpoint `0x00010001`: `receive-request` and `send-reply`.

The only successful normal sequence is:

```text
empty → request-ready → service-processing → reply-ready → completed → closed
```

The capacity-one boundary rejects a second send, receive-before-ready, reply-before-request consumption, replay, close-before-terminal, wrong endpoint, missing right, empty message, and a message above 4 KiB. Peer exit clears the retained bytes and moves to `peer-exited`; explicit close then completes cleanup. The transport does not parse magic, request identifiers, names, kinds, or store bytes. Version 1 defines deterministic failure rather than blocking or an unbounded queue; kernel scheduling and wait/wake adoption remain a separate guest ABI slice.

## Windvale-owned service

[`Resource-Service-Core.wv`](../Operating-System/Services/Resource-Service-Core.wv) parses `WVRQ 1`, invokes the complete portable `WVRS 1` validator and lookup, applies the requested inline-data ceiling, and constructs canonical `WVRY 1`. It never returns a resource from a partially validated store.

[`Resource-Service-Bridge.wv`](../Operating-System/Services/Resource-Service-Bridge.wv) is the hosted integration adapter. It declares only `file.read_bytes`, reads opaque inputs `boot:resources.wvrs` and `ipc:resource-request.wvrq`, and returns the response bytes. The OS suite sends the request through the bounded exchange, runs this service through the reference runtime, sends its output back through the exchange, and verifies exact byte agreement with the independent Stage 0 handler for success, malformed request, missing name, response limit, and invalid store.

## Implementation evidence and limits

The focused OS suite proves deterministic construction, the exact one-page response boundary, the first oversized resource result, strict request and response verification, canonical failure envelopes, 512 deterministic hostile request/response inputs, transport authorization and lifecycle, peer-exit cleanup, capability denial, missing inputs, and one live lookup of `boot:main.configuration` returning identifier `3`, kind `opaque-bytes`, attributes `7`, and bytes `[3,5,8,13]`.

Probe 33 advances to `WVPROC12` and `WVCHAN02`, adds checked one-page user-buffer copy rules and synchronous wait/wake behavior, and proves the exact configuration request/reply twice in pinned QEMU before the existing terminal cleanup. Request sources are RX, destinations are registered RW/NX windows, and endpoint rights remain directional.

The guest service response is currently a fixed WVA-owned canonical envelope for `boot:main.configuration`; the complete portable `WVRS 1` validator and dynamic handler still run only in hosted evidence. The next filesystem slice must give the guest service an independently lived immutable `WVRS 1` capability, execute dynamic lookup there, and define service-death propagation without teaching the kernel resource-name semantics.
