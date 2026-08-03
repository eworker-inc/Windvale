# Decision 0126: First read-only resource store

- Status: Implemented candidate
- Date: 2026-08-02
- Owners: Windvale operating-system resource-service and portable format boundaries
- Contract: [`WVRS 1`](../../Specifications/Windvale-Resource-Store.md)

## Context

Protected-process version 11 exposes two fixed init-owned resources through exact names and one atomic `WVBR002` publication. The durable OS architecture places resource, package, and filesystem policy in isolated services, but the current channel carries one `u32`, resource lifetimes are paired with one fixed borrower, and there is no dynamic namespace or general filesystem contract.

Adding a disk driver, path model, writable filesystem, general IPC, and service discovery together would make failures difficult to attribute. The smallest useful pressure is a third typed immutable input selected by an opaque dynamic name from one deterministic read-only image. The portable lookup policy can be Windvale-owned and useful on Windows and Linux before the live guest depends on it.

## Decision

- Define `WVRS 1` as a maximum-4-MiB image with a 32-byte header, 96-byte directory entries, packed strictly ordered names, packed resource data, explicit kinds/attributes, and lowercase ASCII SHA-256 identities.
- Keep resource names opaque strict UTF-8. Reject NUL, duplicates, host path interpretation, Unicode normalization, and ambient directory rules.
- Require canonical entry order by unsigned encoded-name bytes, consecutive name/data cursors, exact image coverage, unique nonzero identifiers, known kinds, immutable/read-only/no-execute attributes, zero reserved fields, and digest equality.
- Add the independent Stage 0 writer and verifier in [`Resource-Store.cs`](../../Operating-System/Windvale.Bootstrap/Resource-Store.cs). The writer reparses every output through the verifier.
- Add portable [`Resource-Store-Core.wv`](../../Operating-System/Services/Resource-Store-Core.wv), which validates the complete image and all resource digests before returning one name result.
- Add hosted [`Resource-Store-Service.wv`](../../Operating-System/Services/Resource-Store-Service.wv), declaring only `file.read_bytes`, reading `boot:resources.wvrs`, and resolving third resource `boot:main.configuration` as identifier `3`, kind `opaque-bytes`, attributes `7`, and bytes `[3,5,8,13]`.
- Keep current Probe-32 process, channel, resource records, WVA lookup leaf, firmware bytes, and QEMU evidence unchanged. Moving `WVRS 1` into the guest waits for a separately specified bounded IPC descriptor and independently lived resource capability rather than expanding the fixed syscall in place.

## Candidate evidence

The local focused suite passes 28 of 28 tests. New coverage proves deterministic encoding from reversed caller orders, the exact 4-MiB boundary and first oversized image, independent strict verification, exact lookup of a real third typed resource, authorization and missing-store rejection, malformed header/identifier/kind/attribute/name/order/data/digest failures through Windvale, Stage 0 malformed and oversized coverage, and 256 deterministic hostile images. Change-aware qualification-scope development verification also completes a zero-warning Release build and passes all 75 selected Seed tests, including the golden contract; that gate explicitly remains development feedback rather than qualification evidence.

Pinned candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Portable resource-store core WVB | 12,612 | `46350c610db3e1a2a445e0ee839bd6a7ffc37dc17afec6e1d21c086d25e78dc6` |
| Hosted resource-store service WVB | 13,629 | `3e366ad9888674188ca679c0c10ca5583478d2a382b6b756bd4013b30a1b73e1` |
| Three-resource `WVRS 1` fixture | 556 | `ee2ee737db4f4ab480430616032c0d71e6eec1ee66dc2ee33b1d22ac5b3cde2f` |

Cross-host Qualification, retained artifact comparison, and live QEMU integration have not yet run. This decision remains an implemented candidate until that evidence exists.

## Consequences

Windvale now owns a strict dynamic lookup algorithm over a deterministic resource image, and Stage 0 supplies an independent construction/verification oracle. A third resource pressures names and types without making host paths or a disk format portable semantics.

The next guest slice is more precise: add bounded request/reply IPC and a resource-service capability, pass or map `WVRS 1` as an immutable boot input, convert service outcomes to the existing `file.read_bytes` classifications, and prove cleanup for an independently lived result. That work may replace the fixed two-name WVA leaf only after the new boundary is independently checked.

## Deliberate non-claims

This decision does not add a general filesystem, VFS, paths, directories, enumeration, file handles, mutation, writable storage, block I/O, a disk driver, DMA, caching, crash consistency, atomic replacement, a package installer, a general loader, arbitrary process creation, general IPC, service discovery, transferable capabilities, or new QEMU evidence.

## Reconsideration triggers

Reconsider this format when:

- a real consumer needs more than 64 entries or a store larger than the Seed 4-MiB byte envelope;
- lookup measurements justify an index rather than bounded linear traversal;
- independently lived mappings require per-entry capability or page-granular publication data;
- package authenticity requires a signed manifest rather than per-resource integrity only; or
- the first writable or block-backed use case supplies concrete allocation and recovery pressure.
