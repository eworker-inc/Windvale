# Decision 0154: First Windvale directory-service IPC

- Status: Implemented candidate with local Windows evidence; guest adoption and cross-host qualification pending
- Date: 2026-08-03
- Owners: Windvale OS runtime adapter and isolated filesystem-service boundary
- Contract: [`WVDQ 1` with exact `WVDR 1` replies](../../Specifications/Windvale-Directory-Service-Ipc.md)
- Advances: [Decision 0153](0153-First-Versioned-Read-Only-Directory-Capability.md) and [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Retains: ABI 22/context 7, Probe 34, `WVKMEM13`, `WVPROC13`, `WVCHAN03`, `WVRES005`, canonical WVB 1.6/1.7, `WVRS 1`, and every firmware identity

## Context

Decision 0153 defines one useful application-facing filesystem operation but leaves its Windvale OS service binding open. Its largest response is 3,096 bytes, which fits the kernel's existing 4,096-byte format-blind channel. Probe 34's current init and client reply windows are only 2,016 and 2,048 bytes, however, so directly adding a guest call would either truncate the accepted contract or overlap live runtime state.

Reusing `WVRQ 1` would confuse an opaque package-resource lookup with a typed directory read. Returning native paths or repurposing the init-owned `WVRS 1` boot store would collapse distinct namespaces. Enlarging the guest before independently checking the service protocol would mix wire semantics, page layout, capability lifetime, WVA mechanics, and QEMU evidence in one change.

## Decision

- Define one 28-through-283-byte `WVDQ 1` request containing exact total length, offset, maximum chunk, candidate-name length, reserved zero, and at most 255 candidate bytes. The endpoint selects the already bound directory instance; no path, handle, provider identifier, instance number, or kernel pointer enters the message.
- Return the exact existing 24-through-3,096-byte `WVDR 1` value without a wrapper. The semantic contract, statuses, maximal 3,072-byte chunk, and snapshot rules do not change.
- Treat changed identity, extent, reserved bytes, or coverage as structural protocol failure. The service publishes no reply, allowing the existing generation-safe peer-exit boundary to clear the call. Do not invent a filesystem status for malformed transport bytes.
- Preserve typed `Invalid_name` and `Invalid_limit` outcomes for representable semantic requests. Validate names before limits and never invoke the provider for either rejection.
- Add portable [`Directory-Service-Core.wv`](../../Operating-System/Services/Directory-Service-Core.wv). Windvale owns request parsing, semantic checks, local failure construction, and independent provider-response validation.
- Add hosted [`Directory-Service-Bridge.wv`](../../Operating-System/Services/Directory-Service-Bridge.wv) only as an integration adapter. It reads one opaque request and invokes one separately authorized `filesystem.directory_read_v1` instance after the portable core admits the request.
- Retain an independently implemented Stage 0 codec, provider oracle, and hostile-input verifier in [`Directory-Service-Ipc.cs`](../../Operating-System/Windvale.Bootstrap/Directory-Service-Ipc.cs).
- Extract the existing copied-message state machine into [`Bounded-Service-Exchange.cs`](../../Operating-System/Windvale.Bootstrap/Bounded-Service-Exchange.cs). Resource and directory services now demonstrably share one format-blind 4 KiB transport oracle rather than giving a resource-named class accidental format ownership.
- Do not change the compiler merely to land this slice. The existing Windvale type, byte, loop, checked-range, capability, and static-composition facilities express the service policy without a new language or backend feature.
- Keep Probe 34 and every generated firmware input unchanged. Guest adoption requires its own process/memory/resource version decision and pinned-QEMU evidence.

## Evidence

The focused Windows OS suite passes all 34 tests in 13.1 seconds after a zero-warning Release build. Three new cases prove deterministic request construction; the 283-byte request and 3,096-byte response boundaries; middle, end, beyond-end, missing, and non-file results; invalid name/limit non-invocation; structural no-reply behavior; exact response reconstruction; provider invariant rejection; 512 deterministic hostile inputs; peer-exit cleanup; independent capability authorization; missing request handling; and byte agreement between the Windvale service and Stage 0 oracle.

Repeated compilation produces an 8,389-byte portable core with SHA-256 `7433ffde4399862eb2cdf46c0ea43d8d39fc7aec56b2182d6e7789bcb29b2179` and an 8,492-byte hosted bridge with SHA-256 `465b66c8fd21683c33cb9157fa13830c655147a36af50cc0961331fb5967ba2a`.

This is proportional local protocol evidence. Cross-host verification, live guest execution, new firmware identities, and QEMU are pending until the candidate is committed and the separate guest-adoption slice is implemented.

## Consequences

Windvale now owns a strict OS-facing adapter protocol for its first typed filesystem capability. The kernel remains unaware of names and `WVDR`; the service remains unaware of native paths; and the application contract does not change between hosted and eventual Windvale OS execution.

[Decision 0155](0155-First-Immutable-Windvale-Directory-Snapshot.md) now defines the required provider value as one verified `WVDS 1` page distinct from the package-resource store. The remaining guest slice must add a dedicated page-sized reply window, map that init-owned snapshot, bind one rights-limited endpoint/generation, invoke the exact WVDQ/WVDR protocol from a checked runtime adapter, and prove success plus malformed request, service death, client death, cleanup, and repeatability in QEMU. The measured 3,096-byte response rules out reusing Probe 34's existing reply windows.

This decision does not implement that guest adapter, a filesystem root, enumeration, nested paths, handles, mutation, persistence, a block device, DMA, caching, service discovery, concurrent calls, or a general VFS.

## Reconsider when

- More than one call per endpoint generation requires correlation, queueing, cancellation, or fairness.
- A measured immutable read cannot fit one 4 KiB copied message.
- Multiple typed directory instances require an explicit transferable-capability or endpoint-discovery contract.
- A live or writable provider requires semantics that cannot honestly share the immutable `WVDR 1` identity.
