# Decision 0161: Exact compiler service bundle and manifest

- Date: 2026-08-03
- Status: Implemented with focused local Windows evidence; cross-host qualification pending
- Adds: paired exact-compiler native service bundles and `WVHA 1`
- Retains: ABI 22, context 7, service table 5, WVB 1.6, WVO 1.0, standard 4 MiB admission, and every version-1/version-2 PE/ELF byte

## Context

Decision 0160 admits the exact 17.1 MiB compiler through one bounded WVO/link profile. The existing version-2 hosted applications bind only `console.write_line`, while the compiler declares six capabilities and requires several platform and intrinsic runtime services. Building a PE or ELF before fixing that authority and adapter evidence would make the startup contract implicit and would mix service selection with container layout debugging.

The ABI-22 table has twelve possible service slots. Measurement of the actual compiler, rather than the full table vocabulary, shows that it requires ten: it does not import `text.quote` or `i32.format`.

## Decision

- Build one canonical service bundle per platform from the already verified native fragment and its exact ordered required-service list.
- Reuse the existing service generators and their separate instruction-level verifiers. Reuse the qualified executable-publication placement plan rather than creating compiler-specific alignment rules.
- Keep the unchanged 17,130,441-byte compiler native image as the bundle prefix. Append only the ten required leaves.
- Assign explicit platform adapter identities to console output, diagnostics, file input, and file output. Assign shared adapter identities to argument snapshots, UTF-8, enum metadata, text concatenation, and u32 formatting.
- Serialize `WVHA 1` as one fixed 1,024-byte manifest. Bind target, proposed container format 3, ABI/context/table versions, the exact six capabilities, exact ten services, table slots, adapters, bundle offsets and sizes, native entry, arenas, flags, native digest, and every service-leaf digest.
- Distinguish authority-bearing services from intrinsic services. A service requirement is never an authority grant: each capability service points to its exact canonical capability identity.
- Reject noncanonical capabilities, reordered or incomplete services, platform/bundle disagreement, malformed fields, changed digests, changed bundle bytes, target mismatch, and nonzero reserved bytes.
- Pin both complete bundle identities and both manifest identities in the existing exact-compiler AOT transport test. Do not add another full compiler compilation or another malformed corpus.
- Do not expose format-3 CLI targets until their PE/ELF startup code initializes and binds every declared table and the outer independent verifier checks the complete container.

## Local evidence

The focused exact-compiler transport case passes after a zero-warning Release solution build. It compiles the exact compiler once, retains Decision 0160's WVO/link checks, builds both platform bundles, verifies every selected leaf, constructs and parses both manifests, and exercises shared corruptions against both targets.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows service bundle | 17,143,635 | `6d524aa9b96d0f624b0b449937ec6c0987a57e2c002af8276784c63a185efef6` |
| Windows `WVHA 1` | 1,024 | `635c432f6af1349c54fee66a43aab7e89471ff5a42a04e6d1cdb29718ebc217d` |
| Linux service bundle | 17,143,351 | `99da55911c81218ac74442a695d340ed440c74515b830bcc659bd4b7df7b2d4b` |
| Linux `WVHA 1` | 1,024 | `179887b6dec8fd987301a07c290cda93e0833c6827d73d5e62f1ebcf05007d69` |

Both bundles begin with the exact compiler native image SHA-256 `af8db63675a2441e57a763ca4caa411419a84879cf01a1eb62b4be7556487cab`. The target-neutral leaf identities and placements agree. Only console, diagnostic, file-input, and file-output adapters are platform-specific.

This is focused local Windows evidence. Exact identities are pinned so GitHub's independent Windows and digest-pinned Debian Qualification jobs must reproduce the same candidate bytes before a cross-host claim is made.

## Consequences

The next PE/ELF work no longer needs to guess which compiler services exist or how authority is represented. It can concentrate on bounded startup-owned tables, arenas, imports/syscalls, relocations, and complete outer-container reconstruction while preserving one shared native compiler image.

This decision does not produce or expose a compiler executable, initialize runtime tables, bind OS functions, directly execute the compiler, reproduce Stage 2 outside .NET, or satisfy the native-retirement gate. Format 3 is allocated only inside the implemented candidate manifest; version-1 and version-2 targets remain the complete public hosted applications.

## Rejected alternatives

Serializing all twelve ABI slots was rejected because it would overstate the exact program's dependencies and bind two unused services. Storing only platform imports was rejected because it would omit intrinsic semantic services and their code identities. Reusing `WVHC 1` was rejected because its fixed one-capability/one-service shape is already qualified and changing it would invalidate version-2 evidence.

## Reconsider when

- A rebuilt exact compiler changes its canonical capability or required-service list.
- A service cannot be represented as one verified leaf behind the current table contract.
- Startup measurement requires a different bundle order or exposes an unrepresented runtime-private table.
- Windows and Debian do not reproduce the pinned bundle and manifest identities.
