# Decision 0183: Product packaging, trust, and evolution

- Date: 2026-08-03
- Status: Accepted product direction; exact formats and release gates remain incremental
- Refines: [Project vision](../Project/Project-Vision.md), [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md), and [Decision 0173](0173-Windvale-Process-Service-And-Driver-Architecture.md)
- Retains: explicit external dependencies, canonical artifact identities, capability-based host access, experimental pre-1.0 contracts, and x86-64 as the first qualified architecture

## Context

The product architecture needs explicit answers for packaging, compatibility, trust, diagnostics, ambient host facilities, milestone identity, and future architecture expansion. Without these boundaries, implementation could accidentally create an authority-bearing package manager, silently reinterpret old artifacts, leak host time or networking into deterministic code, or call a collection of unrelated features a release.

## Decision

### Define the first product milestone as a vertical evidence slice

Windvale 0.1 requires a reproducible source-to-WVB-to-verification-to-execution workflow on Windows and Linux, one useful application using a real reusable library, explicit capability requirements and grants, documented recovery evidence, and release artifacts whose identities can be reconstructed. It does not require Windvale OS self-hosting, a desktop, broad hardware support, WebAssembly permanence, or a stable 1.0 compatibility promise.

The exact 0.1 release checklist is selected only after the native-retirement and package-manifest paths are close enough to estimate honestly. Milestone names never replace phase-specific qualification evidence.

### Start packages with immutable identities and lockfiles

- Package bundles are immutable and content-addressed.
- A canonical lockfile records exact package identities, versions, source or release origins, dependency graph, selected target parts, capability requirements, and integrity evidence.
- Resolution completes before compilation or launch and produces a deterministic graph. Dependencies cannot silently change authority.
- Package resources remain distinct from mutable application storage and native host files.
- Begin with explicit local or GitHub release sources. Do not create a central registry, dynamic runtime linker, or network resolver before package identity, signatures, update, and offline behavior are qualified.

### Version contracts independently

Version source-language editions, WVB, WVO, object/link recipes, package manifests, capability interfaces, service protocols, machine profiles, and target containers independently. A major version changes incompatible semantics; unsupported required versions fail explicitly rather than being reinterpreted.

Before 1.0, Windvale may replace experimental formats and APIs without compatibility, but release notes identify the affected contract and migration expectations. A later 1.0 decision selects the actual source, package, runtime, and support commitments.

### Make time, entropy, and network authority explicit

- Monotonic time, civil wall time, timers, and scheduler accounting are separate contracts.
- Secure entropy is a capability and never falls back silently to a deterministic or weak generator. Deterministic test entropy is a separately named provider.
- Name resolution, connection, listening, packet access, and network configuration are separate capabilities. A broad socket or DNS API is not ambient process state.
- Tests can bind deterministic time, entropy, and network providers without changing application source semantics.

### Separate local development from trusted releases

Unsigned local builds and packages remain allowed. Official releases publish exact content hashes, manifests, provenance, dependency/license inventories, signatures or attestations, and offline-verification instructions. Trust policy is attached to the launcher, package source, or release channel rather than embedded as an ambient language rule.

Rollback protection, key rotation, revocation, staged deployment, and Windvale OS A/B updates require later contracts and persistent-storage evidence. No updater may make an indeterminate mutation appear committed.

### Maintain one explicit threat model

Create and maintain a project threat model that links the normative validators and capability contracts. It covers malicious source, WVB, WVO, packages, objects, relocations, debug data, service messages, browser input, VM state, firmware, shared queues, device commands, DMA, denial of service, supply-chain inputs, and the host recovery reserve.

The threat model identifies trust boundaries, assets, attacker control, required validation, budgets, containment, teardown, and residual risk. It summarizes and routes to specifications; it does not replace their exact rules.

### Use bounded structured observability

Diagnostics carry stable codes, component and phase identity, source location when available, correlation identity, bounded structured fields, and explicit truncation. Human text may improve without changing the stable code.

Logs, metrics, traces, crash dumps, and external export are distinct facilities with explicit budgets and capabilities. Sensitive source, paths, package data, secrets, guest memory, and device buffers are not emitted by default. A process or service fault produces bounded terminal evidence even when external export is unavailable.

### Qualify x86-64 before adding a second architecture

Keep WIR, WVB, WVO, ABI policy, object records, OS mechanisms, and capability contracts architecture-neutral where possible, while x86-64 remains the first qualified native and OS architecture. Begin ARM64 or another architecture only after the shared native backend and first process, scheduler, memory-object, and isolated-driver paths are stable enough that a second backend tests portability rather than duplicating a moving bootstrap.

## Consequences

The product receives a coherent path from development builds to reproducible releases without requiring a registry, universal signing, updater, or second CPU architecture immediately. Package resolution and authority become auditable before dynamic linking or network discovery.

Time, entropy, networking, observability, and crash reporting cannot become ambient host behavior. This costs explicit binding but preserves deterministic tests and least authority.

The 0.1 milestone remains useful and attainable while 1.0 compatibility, OS self-hosting, browser permanence, and broad hardware support stay separate claims.

No package format, lockfile, registry, signature root, release, updater, threat-model document, diagnostic protocol, network API, or second architecture is implemented by this decision.

## Reconsider when

- content-addressed immutable packages make development or coordinated updates impractical;
- a public registry is required for real dependency discovery;
- a supported ecosystem needs stronger pre-1.0 compatibility guarantees;
- an application cannot use time, entropy, or networking through the separated capability families;
- release or OS recovery requirements demand a concrete updater earlier; or
- a second architecture offers enough immediate hardware or verification value to justify parallel backend work.
