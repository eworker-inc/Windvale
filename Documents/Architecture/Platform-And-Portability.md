# Platform and portability model

## Status

Initial architecture direction. Details remain subject to format and capability specifications.

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

Windvale source is expected to support two primary forms:

- Portable bytecode for applications, tools, packages, and experimentation.
- Native code for the kernel, drivers, runtime internals, low-level libraries, and selected performance-sensitive programs.

The frontend and semantic model should be shared. Backend differences must not silently alter defined behavior.

## Capability profiles

### Portable

Portable modules depend only on deterministic language and foundation-library behavior. They do not receive ambient access to host paths, processes, devices, native libraries, or privileged memory.

### Hosted

Hosted modules may request declared services such as files, networking, windows, clocks, subprocesses, or native interoperability. Availability and authorization remain explicit.

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

- The Windvale SDK and build tools
- The bytecode runtime and application launcher
- Editors, debuggers, inspectors, and package tools
- Continuous integration and fuzzing
- Cross-host conformance tests
- Development of Windvale OS itself

The OS port adds another implementation of Windvale platform contracts; it does not replace the host ecosystem.
