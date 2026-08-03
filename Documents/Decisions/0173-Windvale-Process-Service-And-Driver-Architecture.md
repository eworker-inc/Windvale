# Decision 0173: Windvale process, service, and driver architecture

- Date: 2026-08-03
- Status: Accepted architecture direction; implementation remains incremental
- Refines: [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md), [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md), and [Decision 0171](0171-Future-Virtualization-And-Accelerator-Architecture.md)
- Informed by: [Decision 0165](0165-Contained-Windvale-Service-Failure.md) and [Decision 0172](0172-First-Kernel-Owned-Service-Endpoint.md)

## Context

Windvale has qualified two fixed protected processes, one thread in each process, generation-safe client-root reuse, immutable resource grants, a capacity-one copied-message channel, and exact cleanup after client exit, client fault, or one contained init-service fault. Cross-host-qualified Probe 37 adds a kernel-owned generation-safe endpoint between the process capability entry and that channel. This is meaningful isolation, capability, IPC, and lifecycle evidence, but it is not yet a process manager, scheduler, service manager, driver framework, dynamic object table, or application model.

The next process design must avoid two incompatible mistakes. Making applications, services, and drivers unrelated kernel process types would duplicate lifecycle, scheduling, IPC, and resource behavior. Treating every component as an undifferentiated process with ambient inherited authority would instead hide the security and operational differences among an ordinary application, a restartable service, a device driver, a runtime helper, and a future VMM.

Windvale also needs a path from its exact two-process coordinator to independently runnable services without prematurely adopting POSIX process inheritance, a global service namespace, a pure one-interface-per-process microkernel, a stable syscall ABI, or a complex multiprocessor scheduler.

## Decision

### Use one kernel process mechanism with explicit policy roles

The kernel has one process and thread model. `Application`, `helper`, `service`, `driver`, `runtime`, and `VMM` are launch, supervision, and resource-policy roles over that model; they are not incompatible kernel process classes. Platform scope, authority level, capability requirements, and optional capabilities remain separate metadata dimensions. A role label may support admission, diagnostics, and service-manager policy, but it grants no authority and implies no scheduler priority. Capabilities and verified executable admission remain the enforcement sources.

| Role | Durable meaning |
| --- | --- |
| Application | One main process and optional helpers under one aggregate approved-capability ceiling and resource domain; every member receives only its explicit grants. |
| Helper | A separately isolated process owned by an application or runtime domain; it receives only explicit grants and normally no authority beyond that domain's approved set. |
| Service | A supervised endpoint provider with exact capabilities, dependency bindings, limits, and lifecycle policy. Service status does not imply blanket trust. |
| Driver | An AOT system-profile component with explicit device-resource capabilities. It may run as an isolated process or remain behind a named kernel seam when boot, latency, or hardware evidence requires that boundary. |
| Runtime, verifier, compiler, or JIT | An in-process component or isolated authorized service according to its trust, reuse, and failure boundary. General JIT compilation never runs in the kernel. |
| VMM | A future isolated system service that owns guest-machine and device policy through privileged VM kernel objects accepted by Decision 0171. |
| Kernel worker | An internal privileged execution context used only for kernel mechanisms. It is not an application process or a discoverable service. |

Not every library or interface becomes another process. Capability-free deterministic libraries normally remain in their consumer. Interfaces may share a service process when they have the same authority, resource budget, update boundary, and failure/restart policy. They split when isolation, least authority, independent progress, or independent recovery provides measured value.

### Separate process protection from thread scheduling

A process is a protection and accounting domain containing:

- an isolated address space;
- one or more threads;
- a capability table;
- verified module, runtime, target, and publication identities;
- membership in one aggregate resource domain;
- explicit memory, CPU, thread, handle, endpoint, and other resource ceilings; and
- lifecycle, exit, fault, and diagnostic evidence.

A thread is a schedulable execution context containing registers, stack, thread-local runtime state, ready or wait state, and CPU accounting. Threads share the process capability table by default. Thread creation does not manufacture authority; a later scoped execution context may use a deliberately reduced view without creating an independent ambient authority model.

The current experimental process and thread states may retain their versioned encodings, but the general model must not equate a process being alive with one thread executing. Conceptually, a process progresses through construction/admission, alive, stopping, and terminal exited or faulted states. A thread independently progresses through new, ready, running, waiting, and terminal states. Exact state numbers remain deferred until measured implementation.

### Add flat aggregate resource domains before hierarchy

An application with helpers, a group of tightly coupled services, or a future VM needs limits and teardown above one process. Introduce a generation-safe resource-domain or job object that accounts aggregate processes, threads, memory, handles, endpoints, CPU time, pinned or DMA memory, and other authorized resources.

The first form is flat: every launched process belongs to exactly one domain, and terminating a domain can stop and reclaim all members in a defined order. Nested job trees, delegation across domains, desktop sessions, containers, and Unix-style process groups remain later policy. A resource domain is not an authority bundle; every process still receives exact capability references.

### Spawn from an immutable verified launch plan; do not make `fork` foundational

Windvale's primary creation operation is a clean spawn, not an address-space clone. A launcher or service manager constructs an immutable launch plan that names:

- canonical WVB and selected runtime or authorized AOT identities;
- target, platform scope, authority profile, entry point, and arguments;
- required semantic capabilities and exact bound instances;
- memory, CPU, thread, handle, endpoint, and execution budgets;
- resource-domain membership; and
- service-manager-owned dependency, supervision, criticality, exit, and restart policy where applicable.

The kernel does not parse package names, provider policy, or restart rules. It consumes the checked mechanism portion of the plan, creates a non-running process, validates executable admission, mappings, budgets, object references, and grants, and makes the process runnable only after the complete initial state is valid. Failure leaves no partially visible child or grant.

Parent or supervisor relationships carry lifecycle observation and cleanup policy, not capability inheritance. A child receives only the explicit capabilities in its launch plan. POSIX `fork`, ambient file descriptors, implicit current directories, process-global native environment state, signals, and unrestricted handle inheritance are not Windvale foundations; a later compatibility service may emulate a deliberately bounded subset.

### Make init a minimal service manager, not a permanent universal provider

The first user process evolves into a small service manager and launcher. It reads the verified boot plan, starts authorized services, binds required dependencies before entry, observes terminal results, and applies explicit restart, degrade, recovery, or shutdown policy. It receives only the boot resources and management capabilities required for that work and should delegate provider authority rather than accumulate every service capability.

Required semantic interfaces are bound before process entry. Optional interfaces are reported absent before use. Later dynamic discovery belongs in a user-space registry or broker reached through a capability; the kernel knows endpoint identity, rights, generation, bounds, waiting, and peer lifecycle but not names such as `filesystem`, `display`, or `network`.

Service lifecycle policy distinguishes planned, starting, available, draining or stopping, exited, faulted, and unavailable outcomes without requiring those words to become kernel state encodings. A provider is not published as available until initialization and endpoint binding complete. Graceful stop prevents new work before bounded draining; forced stop fails remaining work and proceeds through kernel cleanup. A boot-critical service failure may select an explicit recovery or shutdown path, but it does not become a kernel invariant failure merely because the failed process had a service role.

A restarted service has a new process generation and provider binding. Clients observe peer loss and either receive an explicit replacement binding or fail according to their contract. The service manager never forges continuity by silently replaying an operation whose mutation may have completed indeterminately.

### Use endpoints for control and shared memory for measured data planes

Decision 0172's endpoint is the correct next kernel-object seam. General IPC evolves from it while retaining these rules:

- Small bounded copied messages carry control requests, replies, events, and diagnostics.
- Endpoint queues, message sizes, in-flight calls, and retained reply buffers have explicit bounds and backpressure.
- Calls use correlation and peer-generation evidence. Cancellation, deadlines, provider replacement, and multiple clients become separate measured revisions rather than overloaded error values.
- Capability transfer is explicit, generation-safe, and non-amplifying: the receiver obtains no rights beyond the sender's authorized source and may receive a strict subset. Copy and move behavior, including which party retains a reference after successful delivery, is part of the operation contract.
- Shared memory or bounded rings carry storage, network, display, VM, GPU, and accelerator data only after ownership, layout, notification, teardown, and resource limits are validated.
- The kernel remains format-blind. Runtime adapters and services validate application values and protocol envelopes before and after IPC.

Synchronous calls may later require measured priority donation to prevent inversion, but that mechanism does not make service role an automatic priority class.

### Introduce scheduling in bounded single-CPU stages

The durable scheduler is a kernel mechanism, not service policy and not part of portable application semantics. Begin with one CPU and one thread per process:

1. replace the exact machine sequence with a state-driven ready/wait dispatcher that selects a runnable thread after a syscall, block, exit, or fault;
2. add a monotonic kernel timer and fixed-quantum preemption so a non-cooperating user process cannot prevent service and recovery progress; and
3. account CPU use against explicit process and resource-domain budgets.

The first preemptive policy should be simple bounded round-robin without public priority or real-time promises. Application, service, and isolated-driver roles use the same base scheduling semantics. Priorities, reservations, affinity, IPC priority donation, per-CPU queues, SMP, and real-time classes require separate measured evidence. The WVB interpreter's semantic instruction budget remains distinct from wall-clock or CPU scheduler accounting.

### Isolate drivers through exact device capabilities

Drivers are deterministic AOT system-profile Windvale components. Device policy belongs in `.wv`; WVA owns only irreducible port, register, interrupt, context, and architecture instructions. An authorized driver receives exact instances for the MMIO ranges, port-I/O ranges, interrupt bindings, DMA mappings, IOMMU domains, memory objects, and kernel operations it needs.

The kernel retains exception and interrupt entry, page and DMA ownership enforcement, context switching, IOMMU enforcement, terminal panic, and an emergency diagnostic path. A boot-critical driver or adapter may remain in the kernel until isolation has real IPC, scheduling, and teardown support, but it requires a documented retention or extraction boundary.

Driver failure teardown proceeds in security order: stop new interrupt delivery and submissions; revoke DMA and IOMMU access; reset or quarantine the device when possible; close service endpoints and fail waiters; invalidate generations and mappings; then release memory and consider restart. No driver restart occurs while stale device access can survive.

The recommended first isolated driver slice is ordinary console/serial output. The kernel retains its minimal COM1 panic and early-boot diagnostic path, while a separately supervised AOT service receives the normal port-I/O authority. This proves driver authority and containment without introducing DMA. Timer and interrupt-controller mechanisms remain in the kernel initially because the scheduler depends on them. Storage and networking follow only after interrupt, shared-memory, DMA, and cleanup evidence exists.

### Preserve the application-facing contract across environments

Applications call typed Windvale libraries, not raw syscalls or service wire formats. Windows and Linux may bind a semantic capability to an in-process adapter, native host process, or supervised service. Windvale OS may bind the same capability to a runtime adapter and protected endpoint. The provider mechanism and process placement may differ without changing the declared semantic contract.

Platform-specific applications, services, and drivers remain valid when their scope is explicit. The common process architecture does not restore a blanket portability requirement and does not make Windows, Linux, or POSIX process behavior the Windvale definition.

## First measured implementation sequence

1. Retain Decision 0172's cross-host-qualified exact `WVENDP01` as the baseline and do not broaden its record without the second endpoint or another concrete consumer.
2. Split the immutable directory provider out of the current combined init/resource/directory process into one statically constructed third process. Give the client a separate resource endpoint and directory endpoint, move the directory snapshot capability to the new provider, and replace the fixed machine sequence with the smallest state-driven ready/wait dispatcher. Do not add dynamic names or restart in this slice.
3. Add a monotonic timer interrupt and single-CPU fixed-quantum preemption while retaining one thread per process and exact deterministic lifecycle evidence.
4. Replace exact tail-only process allocation with checked independently lived memory objects and a general bounded physical-page reclamation strategy sufficient for more than one process lifetime.
5. Add dynamic process and address-space creation from immutable launch plans, starting each process only after complete admission and binding.
6. Generalize capability-table allocation, rights reduction, explicit transfer, endpoint creation, events or timers, deadlines, and shared memory one measured consumer at a time.
7. Extract the service-manager role from the combined init/resource provider; add exit observation, dependency policy, and generation-safe replacement without transparent mutation replay.
8. Move ordinary console/serial output to the first isolated driver service while retaining the kernel emergency sink.
9. Add one bounded shared-memory device data plane, then storage or networking with measured interrupt, DMA, IOMMU, reset, and teardown behavior.
10. Add multiple threads, SMP, priorities, affinity, or real-time behavior only after single-CPU lifecycle, revocation, allocator, and page-table invalidation evidence is sound.

Each step is a pressure test, not permission to stabilize its first record layout or syscall encoding as a public ABI.

## Consequences

- Windvale gains one coherent isolation and lifecycle model for applications, helpers, services, drivers, runtimes, and future VMMs.
- Security authority remains in explicit capability grants rather than role labels, parentage, inherited handles, or scheduler class.
- Process and thread state can scale beyond one thread without conflating protection-domain life with CPU execution.
- Flat resource domains provide aggregate containment and teardown without requiring a complete container or session hierarchy.
- Clean spawn and pre-entry binding make launch reproducible and prevent ambient process state from becoming a language or OS contract.
- Service restart and provider replacement remain observable and generation-safe; uncertain mutations are not replayed as though nothing happened.
- Control-plane IPC remains simple while high-throughput services and virtualization can use checked shared-memory data planes.
- User-space drivers become possible without forcing every early boot mechanism out of the kernel before the required scheduler, interrupt, and teardown evidence exists.
- This decision implements no third process, scheduler, timer, dynamic allocator, process creator, supervisor, driver service, capability transfer, shared-memory transport, SMP, or stable public syscall ABI.

## Reconsideration triggers

Reconsider a boundary or sequence when:

- measured service or driver transitions cost more than their containment benefit for a named workload;
- a required device cannot be reset, isolated, or revoked safely outside the kernel;
- one process per selected failure domain creates unacceptable memory or scheduling overhead;
- clean spawn cannot support a concrete compatibility requirement that a bounded compatibility service could not supply;
- a second architecture cannot implement the same semantic process, capability, endpoint, and lifecycle model;
- scheduler evidence requires a different first fairness mechanism or timer source;
- aggregate resource domains cannot express required cleanup without safe hierarchy; or
- service replacement cannot preserve exact failure and mutation-completion evidence.

Any revision must name the affected mechanism, authority, failure domain, performance evidence, and compatibility contract. It must not silently turn a role label, parent process, service name, scheduler class, native handle, or device identity into ambient authority.
