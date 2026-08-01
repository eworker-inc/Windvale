# Platform and portability model

## Status

Accepted architecture direction, refined by [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md). Current implementation status remains governed by the corresponding format, runtime, native-target, and OS specifications.

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

This keeps early host work useful after the new OS can run programs. It also prevents platform-specific process, path, permission, executable, and GUI rules from becoming accidental language semantics.

## Execution forms

Windvale source supports two durable publication levels and several execution times:

- Canonical verified bytecode for portable applications, tools, packages, caching, and experimentation. It may be interpreted, JIT-compiled, install-time compiled, or compiled ahead of time without changing its identity or semantics.
- Native code produced through the shared native backend for kernels, drivers, runtime internals, host tools, low-level libraries, and selected applications. Deterministic WVO/AOT and in-memory JIT publication share ABI, lowering, and typed relocation rules.

The frontend and semantic model are shared. Backend and execution-tier differences must not silently alter defined behavior. [Native-Execution-And-Dotnet-Retirement.md](Native-Execution-And-Dotnet-Retirement.md) owns the accepted interpreter/JIT/AOT continuum and .NET retirement boundary.

[Windvale-Os-Architecture.md](Windvale-Os-Architecture.md) owns the durable boundary for the third environment: a small capability-oriented kernel written primarily in system-profile Windvale, a bounded WVA machine layer, and isolated Windvale services. It fixes trust and ownership without prematurely freezing the syscall, IPC, scheduler, package, filesystem, or public user-ABI encodings.

## First OS boot environment

[Decision 0044](../Decisions/0044-First-X64-Uefi-Boot-Environment.md) accepts x86-64 with UEFI 2.11 as the first Windvale OS boot environment. QEMU `pc-q35-11.0` with exact EDK II firmware bytes and TCG acceleration is the primary automated VM; Hyper-V Generation 2 is the later Windows compatibility target. This fixes a reproducible experiment boundary without making QEMU devices, UEFI services, PE32+, or the x64 firmware convention part of portable language behavior.

[Decision 0045](../Decisions/0045-First-Uefi-Application-And-Boot-Probe.md) adds a narrow deterministic PE32+ target adapter and first firmware-entry probe. [Decision 0046](../Decisions/0046-Bounded-Uefi-Memory-Map-Probe.md) validates the exercised system/boot-services structure and acquires, walks, and frees a bounded UEFI memory map through ABI-correct calls. [Decision 0047](../Decisions/0047-Bounded-Exit-Boot-Services-Transition.md) retains the current map, bounds stale-key recovery, terminates boot services, and proves continued direct serial execution. [Decision 0048](../Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) separates loader and kernel-entry WVO objects behind [handoff version 1](../../Specifications/Windvale-Kernel-Handoff.md) and a position-independent linked call. [Decision 0049](../Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) replaces the handwritten entry with a WVO object compiled from system-profile `.wv` and keeps COM1 behind an explicit OS adapter. These remain system-profile bootstrap boundaries. The QEMU exit port and remaining raw loader/adapter instruction builder are test implementation details, not portable or hosted Windvale capabilities.

The qualified probe now boots through firmware shutdown, owns a 64 KiB arena and 8 KiB stack, and runs one ordinary portable WVB module through the shared AOT path before compiler-generated system source completes. [Decision 0081](../Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) records historical probe 17 and ABI 14/context 6 evidence. [Decision 0085](../Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) adds the WVA-owned Q35 shutdown path in probe 18; [Decision 0086](../Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) adds normalized vector-6/vector-13 entries in probe 19. [Decision 0087](../Decisions/0087-Native-Windows-And-Linux-File-Output.md) cross-host qualifies their probe-20 composition for ABI 15/context 7 with a zero file-output-table pointer at exact commit `12e9e2e`; all three exact images pass pinned QEMU. The handler's QEMU exit is negative test transport, not a platform lifecycle API.

Candidate [Decision 0085](../Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) adds the first real lifecycle adapter without changing portable semantics: a WVA-authored, independently verified function requests poweroff through the pinned Q35 PM control interface after the normal kernel path completes. This is deliberately target-specific; ACPI discovery, Hyper-V and physical-machine adapters, and process/service shutdown policy remain separate contracts.

Candidate [Decision 0086](../Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) adds the first reusable machine-entry shape without changing portable semantics. WVA-authored vector-6 and vector-13 stubs normalize CPU frames with and without error codes into one 40-byte ring-0 prefix. The current common handler remains a bounded Stage 0 terminal-policy seam; recovery, page faults, interrupt routing, and user-mode delivery remain separate contracts.

This is not yet a functioning kernel or general trap system. Other exception frames, page and double faults, TSS/IST, interrupt controllers, recovery, processes, general platform lifecycle coordination, and the in-guest WVB verifier/runtime remain separately specified and verified slices. Windvale `WVR` runtime traps remain packed semantic statuses and are not redefined as CPU faults.

## Capability profiles

### Portable

Portable modules depend only on deterministic language and foundation-library behavior. They do not receive ambient access to host paths, processes, devices, native libraries, or privileged memory.

### Hosted

Hosted modules may request declared services such as files, networking, windows, clocks, subprocesses, or native interoperability. Availability and authorization remain explicit.

The first accepted hosted boundary supplies an immutable launcher argument snapshot, a bounded opaque-name-to-bytes file read, deterministic standard output, and a separate diagnostic sink. Native adapters own path resolution and native errors. See `Specifications/Hosted-Resources.md`.

### System

System modules may use raw memory, architecture instructions, interrupts, device registers, kernel services, or other unsafe facilities. System-only behavior must be visible in source, metadata, validation, and review.

## Failure model

A valid program may still be unable to perform an operation in a particular environment. Windvale should distinguish at least:

- `Unsupported`: the environment does not implement the capability.
- `Permission denied`: the capability exists but the module is not authorized.
- `Unavailable`: the service exists but cannot currently complete the operation.
- `Invalid module`: the module violates bytecode, type, import, resource, or capability rules.

Unsupported capabilities should be detected at compile, package, or load time when the selected target profile makes that determination possible.

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

Portable code should consume Windvale contracts for these concepts. When a host-specific feature is needed, the module should import an explicitly host-specific capability rather than rely on conditional behavior scattered through ordinary code.

## Contract design principles

- Define fixed integer widths, overflow behavior, endianness, alignment, encoding, and module limits.
- Use separate concepts for package-internal paths and native host paths.
- Keep monotonic time separate from calendar time.
- Avoid exposing native handles through portable APIs.
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
