# Decision 0129: Bounded resource-service request/reply

- Status: Implemented candidate
- Date: 2026-08-02
- Owners: Windvale operating-system IPC transport and user-space resource-service boundaries
- Contract: [`WVRQ 1` / `WVRY 1`](../../Specifications/Windvale-Resource-Service-Ipc.md)

## Context

[Decision 0126](0126-First-Read-Only-Resource-Store.md) adds strict dynamic lookup over one deterministic read-only store, but its hosted service calls lookup directly. Probe 32 still has one capacity-one `u32` result channel, a fixed two-name `WVBR002` leaf, paired resource lifetimes, and no byte-message or resource-service protocol.

Moving the complete store, a general filesystem, writable storage, new scheduling, and a block driver into the guest together would erase the boundary between transport, service policy, persistence, and device authority. The next independently checkable pressure is one page-bounded name request and inline reply whose transport remains ignorant of names and store structure.

## Decision

- Define strict versioned `WVRQ 1` lookup requests with a nonzero correlation identifier, client response ceiling, opaque strict-UTF-8 name, exact coverage, and zero reserved fields.
- Define canonical `WVRY 1` replies with explicit success/malformed/not-found/limit/invalid-store status, failure domain and offset, typed resource metadata, copied inline data, and lowercase ASCII SHA-256 identity.
- Bound every transport message to 4 KiB and successful inline data to 3,984 bytes. Keep the request maximum at 1,056 bytes.
- Add a capacity-one, format-blind Stage 0 exchange oracle with separate client and service endpoints, directional rights, copied message ownership, deterministic state transitions, peer-exit clearing, and explicit terminal close.
- Add portable [`Resource-Service-Core.wv`](../../Operating-System/Services/Resource-Service-Core.wv). It owns request parsing, complete `WVRS 1` validation, name lookup, response-limit policy, and response construction.
- Add hosted [`Resource-Service-Bridge.wv`](../../Operating-System/Services/Resource-Service-Bridge.wv). It declares only `file.read_bytes`, reads the store and request through two opaque names, and returns the response.
- Retain an independent Stage 0 codec, verifier, handler, and transport oracle, now preserved by the [managed recovery archive](../../Bootstrap/Stage0/README.md).
- Keep Probe 32, protected-process version 11, the current syscall and channel records, the boot image, and QEMU identities unchanged. Guest adoption requires its own ABI decision and boot-capability lifetime evidence.

## Candidate evidence

The local focused OS suite passes 31 of 31 tests. New coverage proves deterministic request bytes, strict independent request/response verification, exact one-page reply, first response-limit failure, missing name, invalid store, canonical failure domains, 512 deterministic hostile request/response inputs, endpoint authorization, capacity-one ordering, peer-exit clearing, explicit close, hosted-capability denial, missing store/request inputs, and an exact live hosted exchange for `boot:main.configuration`. The Windvale bridge and Stage 0 handler produce byte-identical responses for success, malformed request, missing name, response limit, and invalid store.

Pinned candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Portable resource-service core WVB | 19,515 | `f151fd559b607b3f0dd8b3ae06399c91b2864a9ce7b30a07da1cffa0dc75e129` |
| Hosted resource-service bridge WVB | 19,457 | `c13d94aa5fc02676ddbaac315c4b55f0c26dbfd28bbd4f821123f67112db1b3f` |

Cross-host Qualification, independent Linux execution, live guest integration, and new QEMU execution have not yet run. This decision remains an implemented candidate.

## Consequences

Windvale now owns both sides of a strict byte-level resource-service request/reply protocol, while the transport oracle knows only endpoint authority, message extent, ownership, and lifecycle. A returned inline snapshot can outlive service access to the store and is independently digest-checked by the client.

The next guest change is narrower: version the protected-process/channel ABI, add checked one-page user-buffer copies and wait/wake transitions, give the service an immutable `WVRS 1` boot capability, route the interpreter's configuration lookup through the service, and prove success, malformed request, service death, client death, and cleanup in QEMU. Direct module/budget boot admission and later block storage remain separate.

## Deliberate non-claims

This decision does not add a general IPC facility, live guest byte IPC, a filesystem, VFS, paths, directories, enumeration, handles, mutation, writes, block I/O, a disk driver, DMA, caching, crash consistency, atomic replacement, service discovery, arbitrary process creation, transferable capabilities, streaming, multi-page payloads, concurrent clients, or new QEMU evidence.

## Reconsideration triggers

Reconsider this protocol when:

- a measured resource cannot fit the one-page inline response and justifies page-backed immutable capabilities;
- concurrent clients require bounded queueing, cancellation, or fairness;
- independent service or client failure needs a richer terminal status;
- authentication requires signed resource metadata rather than store-local digest integrity; or
- a real writable or block-backed consumer supplies concrete durability and recovery pressure.
