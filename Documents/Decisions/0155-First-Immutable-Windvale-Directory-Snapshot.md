# Decision 0155: First immutable Windvale directory snapshot

- Status: Implemented candidate with local Windows evidence; guest adoption and cross-host qualification pending
- Date: 2026-08-03
- Owners: Windvale OS init/service data boundary and read-only directory provider
- Contract: [`WVDS 1`](../../Specifications/Windvale-Directory-Snapshot.md)
- Advances: [Decision 0154](0154-First-Windvale-Directory-Service-Ipc.md) and [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Retains: ABI 22/context 7, Probe 34, `WVKMEM13`, `WVPROC13`, `WVCHAN03`, `WVRES005`, canonical WVB 1.6/1.7, `WVRS 1`, and every firmware identity

## Context

Decision 0154 proves the application and IPC semantics for one immutable directory read, but the next guest probe needs exact provider bytes. Reusing the init-owned `WVRS 1` package store would merge package identity with directory semantics. Giving init an ad hoc pointer and length without a self-validating format would make the first guest fixture part of an undocumented WVA convention. Building a filesystem image, VFS, block layer, or mutable provider now would add unrelated policy before one useful read is measured.

The required response pressure is already known: the selected guest file must return the maximum 3,072-byte chunk, producing a 3,096-byte `WVDR 1` message. A complete provider value must therefore be independently testable before new pages, process records, resource records, syscalls, and QEMU evidence depend on it.

## Decision

- Define `WVDS 1` as one exact, little-endian, 4,096-byte-bounded immutable value with a 32-byte header, one through 64 fixed 32-byte entries, packed ordinal ASCII names, zero alignment, and packed file bytes.
- Admit only two entry kinds: `file` and `other`. Do not encode paths, provider identities, kernel pointers, host handles, permissions, timestamps, or unused future policy.
- Require strictly increasing names and exact region coverage. Reject duplicate names, gaps, overlap, aliases, nonzero reserved or alignment bytes, trailing bytes, changed identity/version, invalid name grammar, and every unchecked extent.
- Keep `WVDS 1` distinct from `WVRS 1`. The former is directory-service-private read data; the latter remains a typed package-resource store. A kernel resource record may describe the immutable page without parsing either format.
- Add portable [`Directory-Snapshot.wv`](../../Operating-System/Services/Directory-Snapshot.wv) to own verification, lookup, and exact `WVDR 1` construction.
- Compose storage and transport in [`Directory-Snapshot-Service.wv`](../../Operating-System/Services/Directory-Snapshot-Service.wv). Do not add a snapshot import to the already-qualified directory protocol core; `WVDQ 1` remains provider-format-independent.
- Retain hosted [`Directory-Snapshot-Bridge.wv`](../../Operating-System/Services/Directory-Snapshot-Bridge.wv) solely for executable differential evidence. It declares only `file.read_bytes` and reads fixed opaque snapshot/request inputs.
- Retain an independently implemented Stage 0 writer, verifier, and provider oracle in [`Directory-Snapshot.cs`](../../Operating-System/Windvale.Bootstrap/Directory-Snapshot.cs). It is a bootstrap/recovery seam, not the permanent OS provider.
- Use the canonical two-entry, 3,184-byte fixture specified by `WVDS 1`. Its 3,072-byte `kernel.wv` entry forces the maximum `WVDR 1` response and its `folder` entry proves typed non-file behavior.
- Do not change the compiler for this slice. Existing Windvale records, enums, bytes, loops, checked ranges, static imports, and hosted input are sufficient.
- Keep Probe 34 and every firmware identity unchanged. Guest adoption is a separate versioned process/memory/resource slice.

## Evidence

The focused Windows OS suite passes all 37 tests after a zero-warning Release build. Three new cases prove input-order-independent construction; exact header, entry, name, alignment, and data layout; a 3,184-byte/one-page bound; exact maximal `WVDR 1`; missing, non-file, and invalid-offset results; every structural region class; writer rejection; 256 deterministic hostile images; missing/unauthorized hosted inputs; malformed snapshot/request no-reply behavior; repeated WVB construction; and byte agreement between Windvale and the independent Stage 0 oracle.

The canonical `WVDS 1` fixture has SHA-256 `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a`. Repeated Windvale compilation produces:

- 12,121-byte portable snapshot module, SHA-256 `7741b8a4005aff12276d3e151b2e991192d132585ceb758f8155d6eab098a137`;
- 20,061-byte portable composed service, SHA-256 `fc77da4c957dd7c44087012c3f911124b7904ec968ef3b21fc768ca0d6078316`;
- 20,294-byte hosted differential bridge, SHA-256 `3f45fffcc9aee26fec35661c8a4ecf1b7b27e4f0a3fb0f0027fed0a78580fa4b`.

This is proportional local format/service evidence. Cross-host verification, a guest mapping, process/memory/resource version changes, new firmware identities, and QEMU remain pending until the separate adoption slice.

## Consequences

The next guest probe has a small immutable provider contract rather than an informal byte page. Init can receive one independently mapped RO/NX snapshot, verify it before serving, and answer the existing WVDQ/WVDR exchange without host paths or kernel filesystem parsing. The kernel continues to own only pages, typed resource metadata, endpoint rights, copying, generations, and lifecycle cleanup.

The format deliberately does not grow into a filesystem. A future provider can implement the same directory capability from another verified snapshot, a service-owned cache, a block-backed filesystem, or a native host adapter without changing `WVDQ 1` or application-visible `WVDR 1` semantics.

This decision does not implement the Probe 35 mapping, directory capability grant, client adapter, additional service call, service/client death cases, filesystem root, enumeration, open handles, mutation, persistence, block I/O, drivers, DMA, caching, or concurrent calls.

## Reconsider when

- A measured immutable provider cannot fit one page.
- Directory enumeration or nested relative lookup needs a distinct bounded contract.
- Large or shared file content should be referenced by verified immutable extents rather than copied into one snapshot.
- A live or mutable provider requires generation, coherence, or revocation semantics absent from version 1.
- More entry kinds are required by implemented behavior rather than anticipated future use.
