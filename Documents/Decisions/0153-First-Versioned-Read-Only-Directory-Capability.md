# Decision 0153: First versioned read-only directory capability

- Date: 2026-08-03
- Status: Cross-host-qualified application contract; Windvale OS guest binding pending
- Advances: Stage 0 capability catalog/reference runtime and `Libraries/Platform/Filesystem`
- Retains: Canonical WVB 1.6/1.7 encoding, current profile bytes, ABI 22, Probe 34, `WVRS 1`, hosted resource leaves, and no runtime module linker
- Refines: [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md), [Decision 0145](0145-First-Capability-Bearing-Static-Library.md), and [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Contract: [Read-only directory capability version 1](../../Specifications/Read-Only-Directory-Capability.md)

## Context

Decision 0145 proves transitive authority approval but deliberately leaves the first filesystem interface open. Reusing `file.read_bytes` would expose an opaque launcher-resolved name rather than a rights-limited instance; accepting native paths would let Windows or Linux define Windvale semantics; and adding mutable state would combine names, allocation, caching, partial progress, and crash recovery before any read-only contract exists.

The current WVB format also lacks independent platform scope and an explicit capability-major field. Decision 0145 permits one bounded compatibility encoding before those larger metadata changes. The smallest useful operation is therefore an exact chunk read from one pre-bound immutable directory snapshot.

## Decision

- Add catalog identity `filesystem.directory_read_v1` with signature `(text, u32, u32) -> bytes`. The `_v1` suffix temporarily carries the major contract version without changing WVB bytes. Do not introduce a second major interface until module metadata carries name and version separately.
- Bind exactly one rights-limited directory instance per authorization. Source receives no root selector, path, native identifier, or ambient current directory.
- Restrict names to one 1–255-byte case-sensitive ordinal ASCII segment and reject `.`, `..`, separators, colons, NUL, non-ASCII, and other bytes before provider invocation.
- Define the bound instance as an immutable snapshot. File names, types, lengths, and contents do not change for the capability lifetime, making offset reads deterministic across hosts.
- Limit a read to 3,072 bytes and `u32` checked offset arithmetic. Require exact maximal chunks, an empty success at end of file, and typed invalid-offset evidence beyond it.
- Return the fixed 24-byte `WVDR 1` header plus the chunk. Represent not found, not file, permission, temporary unavailability, revocation, stale generation, peer exit, invalid offset, invalid name, and invalid limit as typed results.
- Make the runtime validate the envelope and provider invariants before Windvale observes it. Malformed or inconsistent providers trap as `WVR3030`.
- Implement the application-facing decoder and result record in Windvale. Keep the C# owner limited to provider invocation, strict envelope construction/validation, and Stage 0 reference execution.
- Keep `WVRS 1`, package resources, opaque hosted resource names, native files, and the new directory instance distinct.
- Do not change the native service table, selector, console containers, Probe 34, kernel formats, or firmware in this slice.

## Evidence

The focused fixture statically internalizes the Windvale library, preserves one canonical capability requirement, and reproduces identical WVB bytes. An explicitly authorized provider supplies exact middle, tail, and end-of-file chunks; not-found and beyond-end reads return typed results; invalid names and limits do not reach the provider. Missing authorization fails as `WVR3010`, missing transitive approval fails as `WVC0013`, and inconsistent providers plus malformed raw capability responses fail as `WVR3030`.

After integration with Decision 0152, the final expanded fixture passes in 522 milliseconds after a zero-warning Release build. Neighboring hosted-resource and transitive-approval fixtures pass in 144 and 169 milliseconds after zero-warning builds. Earlier change-aware Windows verification before the independent Decision 0152 rebase passes all 85 then-selected Seed tests in 375.273 suite seconds, including the required 224.556-second golden contract.

Exact commit `2fcd66a15d10a88d9c6644c78909ab52b57c526e` passes GitHub [Verify run 30800812738](https://github.com/eworker-inc/Windvale/actions/runs/30800812738). Independent Windows and digest-pinned Debian jobs complete the zero-warning build, all selected Seed and OS tests, golden compiler contract, and native CLI gate. The exact test and broader verification identities are recorded in [Seed verification evidence](../Project/Seed-Verification-Evidence.md). This qualifies the application-facing contract and reference provider boundary across both hosts; native lowering and live Windvale OS guest adoption remain pending.

## Consequences

Windvale now owns the first reusable filesystem-shaped application API and typed failure vocabulary without inheriting native paths or making the kernel a filesystem. The interface is intentionally a snapshot read, not a disguised live directory or package store.

The compatibility name is temporary debt, not the final module model. Independent platform scope, canonical capability name/major metadata, optional interfaces, and multiple typed instances remain required. The Windvale-written compiler still owns only its qualified seven-capability baseline and must adopt the new catalog entry before native self-hosting can consume this library.

[Decision 0154](0154-First-Windvale-Directory-Service-Ipc.md) now binds this exact semantic operation to an independently verified Windvale-owned `WVDQ 1` / `WVDR 1` service protocol while preserving format-blind IPC. Live guest adoption, page mappings, endpoint lifetime, and QEMU evidence remain a separate slice; it must not repurpose the existing package-resource `WVRS 1` name space as a filesystem root.

## Reconsider when

- A conforming Windows, Linux, or Windvale OS provider cannot preserve the immutable snapshot and exact maximal-chunk rules.
- The 3,072-byte payload cannot fit the independently measured OS request/reply envelope without a different bounded chunk.
- Independent capability-version metadata lands before the first OS binding and can replace the `_v1` compatibility identity without preserving obsolete experimental bytes.
- A measured consumer needs multiple directory instances, live change visibility, enumeration, or wider offsets.
