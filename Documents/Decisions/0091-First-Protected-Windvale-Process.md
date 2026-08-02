# Decision 0091: First protected Windvale process

- Date: 2026-08-01
- Status: Accepted and implemented; focused Windows and pinned-QEMU evidence recorded, cross-host qualification pending
- Implements: Step 4 of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Contract: [Protected process version 1](../../Specifications/Windvale-Protected-Process.md)

## Context

Qualified probe 21 already owns a bounded ring-0 W^X page-table root and admits one exact canonical WVB in AOT Windvale code before executing its derived native form. It still executes all code at CPL0, has no process or capability state, and treats processor faults as kernel-terminal.

Decision 0084 deliberately deferred detailed syscall, IPC, process-record, and user ABI choices until one bounded implementation could supply evidence. The smallest coherent next proof is not a scheduler or a general loader. It is one known module crossing a real hardware privilege boundary under a separate root, using a capability-checked message path, returning to the kernel, and demonstrating that one user fault does not become a kernel panic.

Current system-profile Windvale cannot yet write arbitrary 64-bit machine records, construct descriptor tables, program MSRs, or mutate process state through bounded unsafe memory. WVA can own fixed instruction sequences but does not yet express the conditional dispatcher and table-construction loops. The slice therefore needs a small named Stage 0 machine seam while preserving Windvale ownership of the policy and WVA ownership of instruction-level entry mechanics.

## Decision

- Define protected-process contract version 1 as exactly one process identifier 1, one thread identifier 1, one separate x86-64 page-table root, three user pages, one capability, one capacity-one register channel, and three system calls.
- Bind the process to the exact admitted WVB SHA-256 `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2`. Run an immutable Windvale policy module before process allocation; only policy token 91 admits construction.
- Allocate seven zeroed pages: four table pages followed by user code, stack, and data. Copy the kernel hierarchy into a new root, add user permission only along the required path, map code user RX, map stack/data user RW/NX, retain supervisor RW/NX elsewhere, retain the null guard, and reject every writable-executable leaf.
- Store one internal `WVPROC01` record with explicit module identity, addresses, budgets, capability slot/generation/rights, channel state, syscall count, lifecycle state, saved transition state, result, and fault evidence.
- Use capability slot 0, generation 1, send/receive rights value 3, and experimental reference 65536. No ambient pointer, raw handle, or implicit authority enters the syscall path.
- Add WVA statement `syscall`, encoded as x86-64 `0F 05`. Keep its semantics mechanical; the process contract owns validation and state transitions.
- For this experiment only, place syscall number in `EBX`, capability reference in `ESI`, and message/result in `EAX`. Number 1 sends, 2 receives, and 3 exits. Treat this assignment as versioned experimental evidence, not a stable public user ABI.
- Build the admitted Windvale AOT module into a one-page CPL3 image behind a WVA entry. The normal image sends 29, receives 29, and exits 29. Accept exit only after exactly three calls and an empty capacity-one channel.
- Build a separate evidence image that sends and receives 29, then executes WVA `disable_interrupts` at CPL3. Require CPU general protection vector 13/error 0, record the process and thread as faulted, and resume the kernel. Preserve the existing terminal behavior for the same faults originating at CPL0.
- Construct a private GDT/TSS, extend the existing kernel-owned IDT through vector 14, configure `EFER.SCE`, `STAR`, `LSTAR`, `FMASK`, and `KERNEL_GS_BASE`, and use `SWAPGS` plus `SYSRETQ` for the bounded entry/return mechanics.
- Advance kernel memory to version 2 with a 32-page/128 KiB arena, and kernel paging to version 2 with a 128 KiB executable window. Use new `WVKMEM02` and `WVKPAG02` identities so older record bytes cannot be silently reinterpreted under larger bounds.
- Advance the composed WVB-admission bridge target to version 2 because the admitted program now executes inside the protected process rather than directly from the bridge. Preserve its verify-before-execute token and exact module identity.
- Advance the firmware probe to version 22 with normal, invalid-opcode, general-protection, and user-fault scenarios. Require exact deterministic artifacts plus live pinned-QEMU transcripts.
- Keep the page/descriptor constructor, GDT/TSS/IDT/MSR setup, unsafe record mutation, syscall dispatcher, and process return machine inside one explicitly temporary C# Stage 0 object. Replace policy and validation with system-profile Windvale and fixed architecture sequences with WVA as those facilities become expressible.

## Evidence

Focused Windows testing passes all 25 OS tests, including deterministic page-table and process records, W^X/user-bit invariants, malformed planner inputs, exact Windvale/WVA/WVO/user-image/process-machine identities, and all four reproducible firmware images. The focused assembler gate passes all six assembler tests and verifies identical Stage 0 and Windvale-written `syscall` encoding.

Probe 22's normal, invalid-opcode, and general-protection images are each 74,752 bytes; the user-fault image is 75,264 bytes. Their candidate SHA-256 values are recorded in [the firmware-probe contract](../../Specifications/Windvale-Os-Boot-Probe.md). All four live pinned-QEMU scenarios pass with their exact transcript, image identity, and expected host code. Cross-host Seed/OS qualification remains a later promotion step.

## Consequences

Windvale OS now has a real protection boundary rather than a process-shaped simulation. Compiler-generated Windvale executes at CPL3 under a distinct CR3, invokes a capability-checked syscall path, and can exit or fault without falling through to arbitrary kernel execution. The kernel continues after both clean exit and the admitted user-fault scenario.

The process policy is already Windvale source, the user entry and exception normalization are WVA, and the user computation follows canonical WVB through the shared native backend. C# remains the temporary owner only where Windvale/WVA cannot yet express safe table construction and state mutation. This advances the non-.NET OS direction without pretending the bootstrap retirement gate is closed.

The exact record layout, register assignment, and numbers now have implementation evidence, but they remain an internal version-1 experiment. A later public ABI can change them without preserving compatibility until a decision explicitly freezes it.

Memory and paging version 2 intentionally reject version-1 identities rather than accepting old bytes under new arena or executable-window meanings. Historical probe-20/21 evidence remains valid for its recorded contracts.

## Deliberate limits

This decision does not add a scheduler, preemption, multiple processes, process creation, wait, capability transfer/reduction/revocation, generation rollover, a general channel queue, arbitrary module loading, semantic admission of unknown WVB, user heaps, teardown, page release, shared memory, demand paging, a general trap dispatcher, interrupts, timers, device services, init, packages, filesystems, networking, Hyper-V evidence, or physical-hardware evidence.

The capacity-one channel loops back through the kernel to the same process solely to prove rights and state transitions. It is not yet inter-process communication between independent peers.

## Reconsider when

- the first init/resource service requires a different creation, wait, transfer, or channel shape;
- a second architecture cannot implement the semantic operations without exposing the x86-64 experiment;
- user-pointer or larger-message evidence requires copy descriptors, shared regions, or different backpressure;
- timer-driven scheduling requires a different saved-register or kernel-stack contract;
- system-profile Windvale gains bounded unsafe memory and can replace one or more named Stage 0 machine owners; or
- cross-host or live-hardware qualification exposes an assumption not covered by the current exact QEMU boundary.
