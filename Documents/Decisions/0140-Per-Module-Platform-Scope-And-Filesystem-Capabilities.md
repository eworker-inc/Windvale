# Decision 0140: Per-module platform scope and filesystem capabilities

- Date: 2026-08-03
- Status: Accepted architecture direction; first bounded static-library candidate implemented
- First implementations: [Decision 0145](0145-First-Capability-Bearing-Static-Library.md) adds bounded Stage 0 capability-bearing composition; [Decision 0153](0153-First-Versioned-Read-Only-Directory-Capability.md) adds one versioned rights-limited immutable-directory read while retaining the current coarse profiles
- Refines: [Platform and portability](../Architecture/Platform-And-Portability.md), [Decision 0039](0039-Capability-Profiles-In-The-Windvale-Backend.md), [Decision 0040](0040-Static-Multi-Module-Windvale-Backend.md), and [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Retains: Canonical WVB as the verified distribution contract, explicit capability authorization, bounded host adapters, and the capability-oriented Windvale OS kernel/service boundary

## Context

Seed source composition currently requires every imported dependency to be portable, data-free, and capability-free. That restriction made the first deterministic static composition proof small, but it cannot support ordinary application libraries that wrap console, resource, filesystem, window, clock, network, or lifecycle capabilities. Requiring every reusable part to be portable would also prevent honest Windows-, Linux-, or Windvale OS-specific libraries.

The existing `portable`, `hosted`, and `system` profiles combine concerns that need to evolve independently. A library can be ordinary unprivileged application code while targeting only one OS. A system component can implement architecture-neutral policy. A shared library can require an external capability without being tied to one provider implementation. Portability, platform compatibility, privilege, and authority are therefore not one dimension.

Filesystem APIs expose the problem clearly. Windows, Linux, and Windvale OS can implement a useful common set, but individual providers, volumes, network shares, and package stores differ in atomic replacement, durability, path behavior, links, watching, permissions, sparse storage, mapping, and transactions. A single broad interface would either reduce every provider to the weakest behavior or expose methods that fail unpredictably after an application has begun mutating state.

## Decision

- Drop the durable architectural requirement that every imported library be portable. Each part owns its platform scope, authority level, required capabilities, and optional capabilities.
- Retain portable as a positive promise for a part whose declared semantics and dependencies are available on every target it claims. Do not use portable as a blanket dependency rule.
- Derive the final artifact's compatible target set from every reachable part. A dependency may narrow that set by OS, architecture, ABI, execution environment, or another declared target property. Reject contradictory graphs.
- Select alternative platform implementation parts before deriving the final static graph. Shared WVB imports a shared semantic interface and binds a provider at runtime; importing mutually exclusive implementation parts does not produce portability.
- Keep platform scope independent from authority. Ordinary application libraries may be target-specific; trusted services and system/driver code must declare their authority even when their policy is shared across platforms.
- Treat capability acquisition as four separate steps: a library states a requirement, the application approves the exact transitive set, the launcher or service manager grants a rights-limited instance, and the runtime binds the semantic interface to a provider. A dependency update must not silently add authority.
- Identify each semantic capability interface by a canonical name, major contract version, exact parameter and result shapes, limits, and failure behavior. Add optional functionality through another interface rather than changing semantics behind an existing identity.
- Resolve required capabilities before process entry whenever the target and provider set are known. Represent optional facilities as separately bound typed interfaces whose absence is visible before use. Do not make a large interface depend on per-method `Unsupported` failures.
- Treat successful binding as initial evidence rather than a lifetime guarantee. Stateful contracts must define revocation, stale generation, close, service exit or restart, device removal, and temporary unavailability where applicable.
- Permit capability-bearing static libraries as the first implementation direction. Merge exact transitive requirements canonically into the self-contained WVB while retaining root or package approval. Do not add runtime module linking merely to introduce the first platform libraries.
- Require every derived native or AOT container to preserve and verify the canonical module identity, platform scope, and complete capability requirements. Do not treat an unannotated machine image as the authority source.
- Separate reusable code into Foundation algorithms, app-facing platform libraries, bounded provider/service protocol libraries, and privileged system libraries. Applications call typed platform APIs; they do not call raw syscalls or manipulate general IPC envelopes.
- Let Windows and Linux bind a semantic capability to a native or in-process adapter. Let Windvale OS bind the same shared capability, or an explicit Windvale extension, to a checked runtime adapter and an isolated service over bounded IPC. The kernel owns capability objects, isolation, transfer, bounds, waiting, peer lifecycle, and cleanup but does not interpret filesystem or application policy.
- Define filesystem authority as a rights-limited instance such as a filesystem root, directory, file, or watch capability. Do not expose ambient current directories, unrestricted host paths, native handles, file descriptors, or kernel pointers through app-facing contracts.
- Keep package resources, mutable application storage, and native host files as separate concepts. A host may use files to implement more than one concept, but that implementation choice does not merge their semantics.
- Define a small versioned filesystem core only from operations with exact shared semantics. Put atomic replacement, watching, links, permissions, memory mapping, sparse storage, transactions, and irreducibly native behavior in separate optional or platform-scoped interfaces.
- Bind filesystem features to a granted instance rather than an OS label. Two filesystems on the same host may provide different guarantees.
- Make open/create disposition, link-following policy, rights, name comparison, Unicode normalization, segment limits, and collision behavior explicit in the interface or queryable from the granted instance. Do not infer them from Windows, Linux, or Windvale OS as a whole.
- Require directory enumeration to define bounds, ordering, continuation identity, concurrent-mutation behavior, and snapshot status. Native enumeration order is not a reproducible application contract.
- Do not define one ambiguous `file.write`. Distinguish bounded write-at with exact partial-progress evidence, library-level write-all, append, whole-file replacement, flush strength, and atomic replacement. A provider that cannot satisfy a named guarantee must not silently weaken it.
- Use bounded chunk operations for large files and checked explicit-width offsets and sizes. Selecting `u64` for the public filesystem contract waits until every intended execution target supports it.
- Require a mutating operation to distinguish rejection before change, exact partial progress, completion, and indeterminate completion. Do not automatically retry an indeterminate mutation without specified idempotency semantics or a provider-validated idempotency identity.
- Return expected operational outcomes through typed results once the value model supports them. Reserve traps for contract violations, invalid bounds, malformed provider replies, and corrupted execution state.
- Keep the current Seed profile bytes, portable dependency restriction, WVB capability grammar, and hosted resource leaves unchanged until focused source, module, runtime, and verification decisions implement replacements. This decision does not claim that capability-bearing imports, typed capability handles, optional binding, or a filesystem API already exist.

## Consequences

Windvale applications can be genuinely cross-platform, intentionally shared by a subset of hosts, or explicitly OS-specific. Platform-specific functionality no longer needs to masquerade as a portable contract or force a fork of the language.

The same canonical WVB format can carry modules with different compatibility scopes. Shared application bytes remain possible when every dependency and required provider contract is shared; importing a platform extension intentionally narrows the artifact. Native and AOT containers must eventually preserve and verify the capability and target requirements derived from the canonical module rather than treat an unannotated native image as the authority source.

Platform libraries can grow without adding every convenience operation to the runtime capability ABI. Pure validation, formatting, retries, write-all loops, and fallback policy remain ordinary library code. Runtime capabilities stay small and semantic, while OS service protocols remain bounded implementation contracts hidden below the app-facing API.

Capability preflight improves startup diagnostics but does not eliminate runtime lifecycle failures. Applications and libraries that retain stateful providers must handle revocation and peer loss explicitly, and mutation protocols must preserve enough evidence to prevent unsafe replay after uncertain completion.

Filesystem evolution becomes additive by interface rather than additive by an ever-growing table. A common core can be qualified across Windows, Linux, and Windvale OS, while a Windows, Linux, Windvale, volume-specific, or future provider extension remains honest about its scope.

Explicit application approval adds one more build/package check, but it prevents a dependency update from acquiring file, network, window, process, or device authority unnoticed.

## Rejected alternatives

- **Require every dependency to be portable:** prevents capability-bearing libraries and honest target-specific APIs.
- **Use the lowest common denominator for every filesystem function:** discards valuable guarantees and still fails to model differences between filesystem instances on one OS.
- **Expose one broad interface with optional methods:** makes support discoverable too late and encourages partial mutation before `Unsupported` is observed.
- **Give the same `file.write` name to different guarantees:** hides partial progress, replacement, atomicity, and durability differences behind one misleading contract.
- **Expose raw syscalls or generic IPC as the application API:** couples apps to transport and kernel mechanics and duplicates validation across consumers.
- **Expose native paths and handles:** leaks provider identity, authority, and lifetime rules into application semantics.
- **Retry every failed mutation:** can duplicate or reorder writes, appends, renames, and replacements after an uncertain transport or service failure.
- **Add dynamic runtime linking immediately:** adds package resolution and loader complexity before the static capability-bearing library boundary is measured.

## Implementation sequence

1. Specify source/module metadata for platform scope, authority, canonical capability interface identity/version, required capabilities, and optional capabilities while preserving exact current Seed behavior until the new format is selected.
2. Extend deterministic static composition for one capability-bearing platform library, canonical transitive requirement merging, and explicit root approval.
3. Prove a small console library through Windows, Linux, and Windvale OS providers without exposing provider mechanics to the application.
4. Define the first bounded filesystem/resource core from the implemented immutable resource-service pressure, including typed recoverable results and service-death behavior.
5. Add unforgeable typed capability values only when a concrete filesystem, storage, window, or network instance requires multiple handles, delegation, revocation, or independent lifetimes.
6. Add optional and platform-specific extension interfaces only from measured provider behavior and application demand.

## Reconsider when

- Static internalization makes capability requirements, code size, package updates, or provider replacement materially impractical.
- A required shared filesystem operation cannot be implemented with the same observable semantics by Windows, Linux, and Windvale OS providers.
- Typed handle lifetime, transfer, or revocation cannot be verified without moving part of the application-facing contract into the kernel.
- Optional interface binding creates unacceptable state-space or compatibility complexity compared with a smaller versioned family.
- Real applications require asynchronous I/O, cancellation, mapping, or transactions that cannot compose with the proposed core without semantic ambiguity.
- Service restart, revocation, or indeterminate mutation cannot be represented safely without a different request or handle-lifetime model.
