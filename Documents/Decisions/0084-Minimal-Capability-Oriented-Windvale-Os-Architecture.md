# Decision 0084: Minimal capability-oriented Windvale OS architecture

- Date: 2026-08-01
- Status: Accepted direction; implementation is incremental and not yet qualified as a complete kernel or process system
- Refines: [Platform and portability](../Architecture/Platform-And-Portability.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Architecture: [Windvale OS architecture](../Architecture/Windvale-Os-Architecture.md)

## Context

Windvale already has an x86-64 UEFI/QEMU path that exits firmware services, owns a bounded memory arena and stack, runs compiler-generated Windvale code, executes one host-built AOT portable module, and reaches one deterministic terminal CPU-exception destination. The guest does not yet load or verify WVB, own page tables, isolate processes, provide IPC, schedule threads, or shut down cleanly.

[Decision 0083](0083-Windvale-Owned-Native-Publication-Lifetime.md) cross-host qualifies a Windvale-owned executable-publication state graph for Windows and Linux while isolating raw platform memory authority in one internal C# owner. That is a useful ownership transfer, but it does not make C# a durable OS or runtime layer.

Continuing one hardware slice at a time without a durable destination would risk accidental long-term choices: a C# kernel, a second kernel-only language, JIT compilation in privileged mode, host handles in application contracts, an ever-growing monolith, or a rigid microkernel split chosen before measurement. Conversely, fixing detailed syscall and object encodings now would turn untested guesses into compatibility debt.

## Decision

- Build a small capability-oriented kernel surrounded by isolated Windvale services. This fixes the mechanism and trust boundary without requiring a pure microkernel or monolithic label.
- Write durable kernel policy, state, validation, runtime support, services, and drivers primarily in system-profile Windvale (`.wv`). Use WVA (`.wva`) only for bounded architecture mechanics that Windvale cannot safely express, including privileged entry and exit, register frames, context switching, and required machine instructions.
- Keep C# and .NET as Stage 0 build, reference, independent verification, and recovery tools only. They are not part of the permanent OS. Any temporary C# machine emitter, target adapter, or kernel helper requires a named `.wv` or `.wva` replacement seam.
- Keep canonical verified WVB as the portable program identity on Windows, Linux, and Windvale OS. Native images are derived execution products. Use the shared native ABI/backend rather than a kernel-specific compiler or language dialect.
- AOT-compile the kernel, low-level drivers, and initial trusted verifier/runtime components. Keep general JIT compilation outside the kernel in an ordinary process or isolated authorized service.
- Make the kernel own privileged mechanisms: exceptions and interrupts; physical and virtual memory isolation; processes, threads, and minimum scheduling; capability and kernel-object lifetime; bounded IPC; executable-publication enforcement; essential timers and device bindings; panic and clean shutdown.
- Put package, filesystem, network, compiler, JIT, shell, GUI, general runtime, and most device policy in isolated services when practical. Boot-critical adapters may begin in the kernel behind named seams and move only when process/IPC evidence demonstrates a better boundary.
- Define a process conceptually as an isolated address space, threads, a capability table, verified module/runtime identity, explicit resource budgets, and lifecycle/fault/result state.
- Define capabilities conceptually as unforgeable rights-limited references with stale-reference protection. Do not expose raw host handles or persistent kernel pointers through portable or service contracts.
- Keep the syscall and IPC semantics small, versioned, bounded, and architecture-neutral. WVA owns the x86-64 calling mechanics; `.wv` owns validation and state transitions. Defer exact numbering, register assignment, encoding, and copy-versus-map policy.
- Enforce verify-before-execute and writable-or-executable publication. CPU exceptions remain distinct from Windvale `WVR` runtime traps.
- Bootstrap the first in-guest verification with an AOT Windvale verifier embedded in the boot image. It validates canonical WVB before the first ordinary module executes. Later isolation of that trusted verifier must preserve the kernel's executable-admission enforcement.

## Consequences

Windvale OS has a durable implementation language and trust direction without claiming that its process system, verifier, or service model is implemented. The existing C# and raw-machine emitters remain valid temporary bootstrap components only within their documented replacement seams.

The kernel remains small by responsibility, not by a promised line count or ideology. A specific driver or early service may remain in the kernel when that is the only coherent first implementation, but its authority and future boundary must be explicit.

The same WVB can remain the application artifact across Windows, Linux, and Windvale OS while execution tiers differ. Kernel and driver AOT does not create different language semantics.

System-profile Windvale will need explicit unsafe, memory, atomic, concurrency, and capability facilities as real kernel slices demand them. These facilities must not leak into portable modules.

## Rejected alternatives

- **A permanent C#/.NET kernel or runtime:** conflicts with the owned-stack goal and Decision 0057's retirement gate.
- **A separate kernel-only Windvale dialect or compiler:** risks semantic and backend divergence from the shared language.
- **General JIT compilation in the kernel:** expands the privileged parser, compiler, allocator, and executable-memory attack surface.
- **An all-in-kernel service architecture:** makes files, packages, networking, UI, and driver policy part of the largest failure domain.
- **A pure microkernel split immediately:** requires process, IPC, and driver evidence that does not yet exist and could replace working bounded progress with ideology.
- **Host handles, paths, structs, or ABIs as system contracts:** would make Windows, Linux, UEFI, or x86-64 define portable behavior.
- **Freezing detailed syscall, IPC, scheduler, package, or filesystem formats now:** creates long-lived compatibility obligations without implementation evidence.

## Implementation sequence

1. Complete the bounded single-address-space machine foundation: normalized essential traps, page-table ownership, deterministic panic, and clean shutdown.
2. Transfer remaining machine mechanics to WVA and kernel policy to `.wv` as each exact seam becomes expressible.
3. Boot an AOT Windvale decoder/verifier and validate one embedded canonical WVB inside the guest.
4. Add one protected process, thread, capability table, bounded resource budget, and IPC channel.
5. Start the first Windvale init/resource service and execute the verified module outside the kernel.
6. Move the interpreter and later JIT to user or isolated service space behind verified W^X publication.
7. Add device and resource services one qualified boundary at a time, then prove one exact WVB across all three environments.

Candidate [Decision 0090](0090-First-In-Guest-Wvb-Admission.md) implements the deliberately fixed first part of step 3: AOT Windvale code validates one exact WVB 1.6 identity before its separately AOT-compiled form executes. General semantic verification, loading, and isolation remain open. The next architecture slice is step 4.

## Reconsider when

- Capability identity, reduction, transfer, or revocation cannot meet a measured sharing requirement without ambient authority.
- Process or IPC evidence shows that a named mechanism is safer or materially simpler on the other side of the kernel boundary.
- The shared native backend cannot serve both AOT system code and hosted execution without semantic or safety divergence.
- Windvale's system profile cannot express kernel policy without an unacceptably broad unsafe surface.
- A second architecture cannot implement the semantic process, capability, and syscall boundary.
- Native recovery evidence requires a maintained non-Windvale implementation rather than an archived Stage 0 release.

Changing this direction requires a later decision with implementation and qualification evidence. Detailed experimental ABIs may evolve without revisiting the decision so long as these ownership and trust invariants remain intact.
