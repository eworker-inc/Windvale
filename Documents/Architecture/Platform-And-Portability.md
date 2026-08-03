# Platform and portability model

## Status

Accepted architecture direction, refined by [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0140](../Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md). Current implementation status remains governed by the corresponding format, runtime, native-target, and OS specifications.

## Central distinction

Windows and Linux are hosts for the Windvale runtime; they do not define Windvale language behavior.

```text
Application
    |
Windvale language and library contracts
    |
Windvale module/runtime contract
    |
    +-- Windows adapter
    +-- Linux adapter
    `-- Windvale OS implementation
```

This keeps early host work useful after the new OS can run programs. It also prevents platform-specific process, path, permission, executable, and GUI rules from becoming accidental language semantics. An application or library is not required to run on every environment: each part may use shared Windvale contracts, target an explicit subset of environments, or depend on one named platform extension.

## Execution forms

Windvale source supports two durable publication levels and several execution times:

- Canonical verified bytecode for applications, tools, packages, caching, and experimentation. The WVB format remains a cross-host distribution contract, but a particular module may carry requirements implemented by every host, a subset of hosts, or one named platform. It may be interpreted, JIT-compiled, install-time compiled, or compiled ahead of time without changing its identity or semantics.
- Native code produced through the shared native backend for kernels, drivers, runtime internals, host tools, low-level libraries, and selected applications. Deterministic WVO/AOT and in-memory JIT publication share ABI, lowering, and typed relocation rules.

The frontend and semantic model are shared. Backend and execution-tier differences must not silently alter defined behavior. [Native-Execution-And-Dotnet-Retirement.md](Native-Execution-And-Dotnet-Retirement.md) owns the accepted interpreter/JIT/AOT continuum and .NET retirement boundary.

[Windvale-Os-Architecture.md](Windvale-Os-Architecture.md) owns the durable boundary for the third environment: a small capability-oriented kernel written primarily in system-profile Windvale, a bounded WVA machine layer, and isolated Windvale services. It fixes trust and ownership without prematurely freezing the syscall, IPC, scheduler, package, filesystem, or public user-ABI encodings.

## First OS boot environment

[Decision 0044](../Decisions/0044-First-X64-Uefi-Boot-Environment.md) accepts x86-64 with UEFI 2.11 as the first Windvale OS boot environment. QEMU `pc-q35-11.0` with exact EDK II firmware bytes and TCG acceleration is the primary automated VM; Hyper-V Generation 2 is the later Windows compatibility target. This fixes a reproducible experiment boundary without making QEMU devices, UEFI services, PE32+, or the x64 firmware convention part of portable language behavior.

[Decision 0045](../Decisions/0045-First-Uefi-Application-And-Boot-Probe.md) adds a narrow deterministic PE32+ target adapter and first firmware-entry probe. [Decision 0046](../Decisions/0046-Bounded-Uefi-Memory-Map-Probe.md) validates the exercised system/boot-services structure and acquires, walks, and frees a bounded UEFI memory map through ABI-correct calls. [Decision 0047](../Decisions/0047-Bounded-Exit-Boot-Services-Transition.md) retains the current map, bounds stale-key recovery, terminates boot services, and proves continued direct serial execution. [Decision 0048](../Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) separates loader and kernel-entry WVO objects behind [handoff version 1](../../Specifications/Windvale-Kernel-Handoff.md) and a position-independent linked call. [Decision 0049](../Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) replaces the handwritten entry with a WVO object compiled from system-profile `.wv` and keeps COM1 behind an explicit OS adapter. These remain system-profile bootstrap boundaries. The QEMU exit port and remaining raw loader/adapter instruction builder are test implementation details, not portable or hosted Windvale capabilities.

The cross-host-qualified probe-21 baseline boots through firmware shutdown, owns a 64 KiB arena and 8 KiB stack, activates one kernel W^X root, and admits one exact WVB in AOT Windvale before executing its derived ring-0 code. [Decision 0081](../Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) records historical probe 17; Decisions 0085 through 0090 record clean shutdown, normalized vector-6/vector-13 entry, kernel page tables, ABI 16, and fixed WVB admission through exact commit `860c69c`.

[Decision 0091](../Decisions/0091-First-Protected-Windvale-Process.md) advances probe 22 without changing portable semantics. It enlarges the explicitly versioned boot arena and kernel executable window, creates one separate CPL3 page-table root, runs the exact admitted Windvale AOT program behind a WVA user entry, uses one generation/rights-checked capability for bounded register send/receive, exits through `SYSCALL`, and contains a deliberate user general-protection fault. The x86-64 register assignment and process record are internal experiment contracts, not portable or hosted behavior. Its retained composition is cross-host qualified through probe 24.

[Decision 0092](../Decisions/0092-First-Windvale-Init-Resource-Service.md) advances probe 23 with two separate CPL3 roots and the first user-space service written in Windvale. Receive-only init blocks, a send-only client publishes one message, and the fixed coordinator wakes init even after the selected client fault. Its retained composition is cross-host qualified through probe 24 at `190174a`.

[Decision 0093](../Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) advances cross-host-qualified probe 24 without changing portable semantics. The client image is an AOT-built portable Windvale interpreter that executes the exact admitted WVB subset in user space; the program's host-built AOT derivative is absent from that path. The interpreter identity, program identity, runtime kind, RX extent, and NX stack are explicit internal OS contracts.

Qualified [Decision 0094](../Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) advances probe 25 by deriving the admitted module's seven section payloads and bounded function/export shape instead of depending on fixed serialized offsets. Its 32-page RX image and measured four-page stack remain internal process bounds. This is not a runtime-supplied loader, general interpreter, JIT, or stable process ABI.

Candidate [Decision 0085](../Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) adds the first real lifecycle adapter without changing portable semantics: a WVA-authored, independently verified function requests poweroff through the pinned Q35 PM control interface after the normal kernel path completes. This is deliberately target-specific; ACPI discovery, Hyper-V and physical-machine adapters, and process/service shutdown policy remain separate contracts.

Candidate [Decision 0086](../Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) adds the first reusable machine-entry shape without changing portable semantics. WVA-authored vector-6 and vector-13 stubs normalize CPU frames with and without error codes into one 40-byte ring-0 prefix. The current common handler remains a bounded Stage 0 terminal-policy seam; recovery, page faults, interrupt routing, and user-mode delivery remain separate contracts.

This is now a functioning but deliberately tiny two-process service and interpreter proof, not a complete kernel or general runtime/trap/process system. It has no general scheduler, preemption, capability transfer, general loader, JIT publication, resource namespace, page-fault policy, double-fault containment, IST, interrupt controller, recovery, or general platform lifecycle coordination. Windvale `WVR` runtime traps remain packed semantic statuses and are not redefined as CPU faults.

## Module scope, authority, and capability requirements

Portability is a property of an individual part and a derived property of the complete dependency graph. It is not a blanket requirement placed on every imported library. Four concerns remain independent:

- **Platform scope:** every environment, an explicit set such as Windows and Linux, or a named target constrained by OS, architecture, ABI, or execution environment when those details are observable.
- **Authority level:** ordinary application, trusted service, or system/driver code.
- **Required capabilities:** contracts that must be approved and bound before execution.
- **Optional capabilities:** extensions whose absence is visible before use and for which the application has an intentional fallback.

A reusable part may therefore be cross-platform, shared by only Windows and Linux, or specific to Windows, Linux, or Windvale OS. The build derives the final artifact's supported target set from every reachable part and rejects an empty or contradictory set. Target-specific ordinary application code does not become system code merely because it is target-specific, and system authority does not imply that a component is tied to one machine or OS.

Alternative platform implementations are selected before deriving the final graph. A shared WVB imports the shared semantic library and lets the runtime bind a provider; a target-specific build may select one explicit implementation part. Importing mutually exclusive Windows and Linux implementation parts into one static graph does not make that graph cross-platform.

The current Seed `portable`, `hosted`, and `system` profile byte remains an implemented validation boundary. Seed also currently requires imported source dependencies to be portable and capability-free. Those restrictions describe the present compiler, not the durable library architecture. A later source/module decision must encode or otherwise bind platform scope independently from privilege and permit capability-bearing dependencies without weakening deterministic composition.

Portable remains a useful positive promise. A portable part depends only on deterministic language behavior, Foundation, and shared Windvale contracts whose semantics are available on every target it claims. It does not receive ambient host paths, native handles, process state, privileged instructions, or undocumented host behavior. Platform-specific parts may use explicitly named extensions without pretending those extensions are portable.

System modules may use raw memory, architecture instructions, interrupts, device registers, kernel services, or other unsafe facilities. System-only behavior must remain visible in source, metadata, validation, and review regardless of platform scope.

### Requirement, approval, grant, and binding

A capability declaration is a requirement, not authority by itself. The durable chain is:

```text
library requirement
    -> application approval
    -> launcher or service-manager grant
    -> runtime provider binding
```

A dependency must not silently expand an application's authority. The root source or a later package manifest approves the exact transitive required-capability set; changing a dependency to require another capability must therefore fail the build or package check until the application accepts it. The launcher or Windvale OS service manager separately decides which approved requirements receive concrete grants.

Required capabilities are resolved before the application starts whenever the target and package are known. Optional capabilities are queried and bound as typed extensions before their first operation; absence is ordinary control flow rather than a surprise after partial mutation. Capabilities remain small semantic interfaces rather than a registry of arbitrary string-dispatched OS calls.

A capability interface requires a canonical identity: semantic name, major contract version, exact parameter and result shapes, limits, and failure behavior. A provider may add another optional interface without changing the existing identity, but it must not attach stronger, weaker, or otherwise different semantics to the same identity. Minor implementation revisions remain invisible only when observable behavior is unchanged.

Successful binding proves initial availability, not permanent availability. Revocation, process shutdown, service failure or restart, device removal, and provider teardown may invalidate a required or optional binding later. Every stateful interface must define closed, revoked, stale-generation, peer-exited, and temporarily unavailable outcomes where applicable. A caller cannot infer that authority or service lifetime continues merely because preflight succeeded.

The first accepted hosted boundary supplies an immutable launcher argument snapshot, a bounded opaque-name-to-bytes file read, deterministic standard output, and a separate diagnostic sink. Native adapters own path resolution and native errors. See [Hosted-Resources.md](../../Specifications/Hosted-Resources.md). These current tool-oriented leaves are bootstrap evidence, not the complete future application or filesystem API.

## Library and provider layers

Reusable application-facing facilities belong in distinct layers:

- **Foundation** contains deterministic, capability-free algorithms and values.
- **Platform libraries** expose typed application APIs for output, resources, filesystems, clocks, networking, windows, input, and lifecycle. A platform library may be shared or explicitly target-specific.
- **Protocol libraries** encode and strictly validate bounded service messages used between a provider and an OS service. Ordinary applications should not manipulate those wire bytes.
- **System libraries** expose privileged kernel, driver, and machine facilities and require system authority.

An application calls a platform library. The library reaches a small semantic capability. Windows and Linux may bind that capability to an in-process or native adapter; Windvale OS may bind it to a checked runtime adapter that communicates with an isolated service through bounded IPC. The app-facing contract remains independent of the provider mechanism.

Static internalization into one canonical WVB is the first implementation direction because it preserves deterministic self-contained artifacts. Dynamic package linking, service discovery, and provider replacement remain later choices. Capability-bearing static composition must merge exact requirements canonically while retaining explicit application approval. A derived native or AOT container must preserve and verify the canonical module identity, platform scope, and complete capability requirements; an unannotated machine image must not become the authority source.

## Filesystem contract family

Windvale should define a small common filesystem contract without making Win32, POSIX, or the first Windvale OS filesystem the universal model. A process receives a rights-limited filesystem or directory capability; it does not receive ambient access to a host filesystem.

The common core should be limited to operations whose signatures and semantics can be stated exactly across providers, such as opening relative to a granted directory, bounded reads and writes at explicit offsets, length changes, basic type and size queries, directory operations, flush, rename, removal, and close. Open and create dispositions, link-following policy, access rights, and collision behavior must be explicit. Implicit shared file cursors, native sharing flags, advisory locks, and mandatory locks are not assumed common semantics. Package resources, mutable application storage, and native host files remain different concepts even when one host adapter uses files to implement more than one of them.

Path values must use Windvale-defined segment, encoding, and traversal rules. Native Windows or Linux path strings belong only in explicit host-tool or platform-extension contracts. Case sensitivity, Unicode normalization, valid segment repertoire, maximum lengths, and collision behavior belong to the granted filesystem instance and must be queryable or fixed by its exact interface; callers must not infer them from the OS name. A provider must not allow a relative operation to escape its granted root through `..`, links, reparse behavior, mount traversal, or another native mechanism unless that behavior is part of an explicitly granted extension.

A single ambiguous `file.write` is not an adequate contract. The family should distinguish at least:

- a bounded write-at operation that reports the exact completed byte count and status;
- a library-level write-all operation that repeats partial writes and preserves partial-progress evidence on failure;
- append from offset-based writing;
- whole-file replacement from in-place mutation;
- data flush from data-and-metadata or directory durability; and
- atomic replacement from replacement that may expose a partial or missing destination.

Core reads and writes operate on bounded chunks so large files do not require one whole-value allocation. File offsets, lengths, and resulting sizes require an explicitly selected unsigned width and checked arithmetic; adopting `u64` for the public contract waits until every selected execution target supports that value shape. A zero-length operation, end-of-file result, short read, short write, and maximum chunk must each have one specified meaning.

Mutating operations must report whether they were rejected before change, completed, partially completed with exact progress, or left indeterminate by a provider or transport failure. A library must not automatically retry an indeterminate mutation unless the operation is defined as idempotent or carries a provider-validated idempotency identity. A service restart must not turn an uncertain write, append, rename, or replacement into an unnoticed duplicate or second mutation.

The same operation name may be implemented by multiple providers only when they promise the same observable behavior. A provider that cannot honor a guarantee must not silently weaken it. Additional feature groups remain separate versioned capabilities, for example atomic replacement, watching, links, permissions, memory mapping, sparse storage, or transactions. Windows reparse points and alternate streams, Linux-specific ownership or notification details, and Windvale OS-native snapshots or object capabilities may remain in explicit platform modules.

Feature availability belongs to the granted filesystem instance, not merely the OS name. A local volume, network share, removable volume, and package store on the same host may provide different guarantees. Required interfaces are bound before execution; optional interfaces are discovered before use. Instance-dependent failures still return stable operation results. Directory enumeration must define its bounds, ordering, continuation identity, consistency under concurrent mutation, and whether it is a snapshot; provider-native enumeration order is not a reproducible application contract.

Filesystem, directory, file, and watch references should become unforgeable typed capability values when the language and WVB value model admit them. A provider may internally use a Windows handle, Linux file descriptor, or Windvale kernel/service reference, but no native identifier crosses the application contract. References require rights, generation-safe identity, deterministic close/revocation behavior, and explicit resource budgets.

## Failure model

A valid program may still be unable to perform an operation in a particular environment. Windvale should distinguish at least:

- `Unsupported`: the environment does not implement the capability.
- `Permission denied`: the capability exists but the module is not authorized.
- `Unavailable`: the service exists but cannot currently complete the operation.
- `Invalid module`: the module violates bytecode, type, import, resource, or capability rules.

Unsupported required capabilities should be detected at compile, package, or load time when the selected target and provider set make that determination possible. An absent optional interface is a query result before use. Expected filesystem and service outcomes such as not found, already exists, permission denied, no space, quota, busy, end of file, closed, revoked, stale handle, peer exit, partial completion, or indeterminate mutation should be recoverable typed results once the source value model supports them; traps remain appropriate for violated runtime contracts, malformed provider replies, invalid bounds, and corrupted modules.

## Sources of host incompatibility to control

- Executable and object formats such as PE/COFF and ELF
- Calling conventions and native ABIs
- Filesystem path syntax and case behavior
- Permissions, identities, and sandbox policy
- Process, signal, and application lifecycle models
- Windowing and input systems
- Dynamic-library loading
- Clock, locale, and environment behavior
- Threads, atomics, scheduling, and memory ordering
- Executable-memory and code-signing policy

Shared code should consume Windvale contracts for these concepts. When a platform-specific feature is needed, the owning part should import an explicitly scoped capability or platform library rather than rely on conditional behavior scattered through ordinary code. The resulting artifact is then honestly platform-scoped rather than incorrectly described as portable.

## Contract design principles

- Define fixed integer widths, overflow behavior, endianness, alignment, encoding, and module limits.
- Use separate concepts for package-internal paths and native host paths.
- Derive artifact compatibility from all reachable parts instead of imposing portability on every dependency.
- Keep authority level independent from platform scope.
- Require exact application approval before a dependency may expand the transitive capability set.
- Give every capability interface a canonical semantic identity and version; do not change behavior behind an existing identity.
- Keep monotonic time separate from calendar time.
- Avoid exposing native handles through application or service APIs.
- Give operations distinct names when atomicity, durability, partial progress, link behavior, or failure semantics differ.
- Never retry a mutating operation whose completion is indeterminate without a specified idempotency contract.
- Group optional platform facilities into small versioned capability interfaces rather than one large interface with unpredictable unsupported operations.
- Make permissions and capabilities inspectable before execution.
- Validate bytecode and modules before allocating unbounded resources or executing instructions.
- Keep host adapters thin enough that conformance tests can run identically against every host.

## Permanent value of host ports

Windows and Linux should remain first-class environments for:

- The Windvale-native SDK and build tools
- The verified interpreter, JIT/AOT runtime, and application launcher
- Editors, debuggers, inspectors, and package tools
- Continuous integration and fuzzing
- Cross-host conformance tests
- Development of Windvale OS itself

The OS port adds another implementation of Windvale platform contracts; it does not replace the host ecosystem.

C# and .NET provide the current Stage 0 implementation on both hosts. They are not permanent host requirements. Normal development moves to qualified Windvale-native tools only after the explicit cross-host native-retirement gate; bootstrap history and the final recovery release remain documented rather than silently discarded.
