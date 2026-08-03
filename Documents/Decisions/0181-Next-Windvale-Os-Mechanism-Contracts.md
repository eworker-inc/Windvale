# Decision 0181: Next Windvale OS mechanism contracts

- Date: 2026-08-03
- Status: Accepted architecture direction; exact layouts, hardware selections, and qualification remain incremental
- Refines: [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md), [Decision 0171](0171-Future-Virtualization-And-Accelerator-Architecture.md), [Decision 0173](0173-Windvale-Process-Service-And-Driver-Architecture.md), and qualified [Decision 0176](0176-Third-Protected-Service-And-Ready-Wait-Dispatcher.md)
- Retains: the qualified Probe-38 three-process/dispatcher baseline, pinned QEMU/TCG, and the small-kernel/isolated-service boundary

## Context

Decisions 0171 and 0173 establish the durable VM-host, accelerator, process, service, scheduler, and driver architecture. Decision 0176 has since qualified the first selected pressure: three protected processes, two independent endpoints, and a state-driven ready/wait dispatcher. The remaining questions concern the smallest successor records, mechanisms, filesystem operations, and evidence that should advance those decisions without freezing experimental encodings or selecting unavailable hardware by preference.

## Decision

### Keep one WVO and one linker path

Do not create a kernel-only WVO fork. Extend the common object model only with generally useful typed sections, permissions, alignment, address classes, relocations, and entry metadata. A target adapter owns physical and virtual placement, boot handoff, privileged entry, manifest construction, and UEFI or later container details. Kernel-owned data remains ordinary typed sections and symbols rather than embedded development-host addresses.

### Evolve qualified logical records, not current offsets

Probe 38's three-process and ready/wait-dispatcher implementation establishes bounded logical records for:

- process identity, generation, role, lifecycle, address space, capability table, resource domain, and terminal evidence;
- thread identity, register/stack state, ready or wait reason, accounting, and owning process;
- capability object identity, generation, rights, type, and close state;
- endpoint provider generation, semantic interface identity, bounded queue/in-flight state, peer state, and close reason; and
- immutable directory snapshot identity and version.

Current field offsets, fixed slot numbers, machine references, exact process IDs, and one-thread-per-process shape remain replaceable internal evidence. Probe 38 already proves stale-generation and wrong-provider rejection, independent resource and directory endpoint behavior, contained provider failure, exact wakeup, and complete generation-safe teardown. Timer preemption and later dynamic process creation must preserve those logical invariants without treating `WVPROC17` as a public run-queue ABI.

### Separate clocksource from clockevent

The first single-CPU preemptive scheduler uses:

- an invariant TSC clocksource only after feature and calibration evidence, with HPET as the initial calibration source and fallback;
- a local-APIC one-shot or deadline clockevent when supported;
- a private experimental fixed round-robin quantum, initially approximately five milliseconds, that is recorded in evidence but is not a public timing guarantee; and
- monotonic tick accounting separate from WVB semantic instruction budgets and civil wall time.

Qualification covers a CPU-bound thread that cannot starve another runnable thread, bounded wakeup, masked or delayed interrupts, no lost ticks, checked wrap/overflow, idle behavior, and exact terminal accounting. Provider-specific synthetic timers remain later adapters.

### Begin physical memory with a deterministic bitmap

Use a bounded bitmap first-fit physical-page allocator with explicit page state and owner evidence. A generation-safe memory object owns a checked page vector; virtual mappings, rights, pinning, DMA state, and executable publication are separate records. Zero pages before every grant or reuse. Permit a virtually contiguous object to use noncontiguous physical pages.

Add buddy allocation, slabs, NUMA policy, large pages, contiguous DMA pools, and SMP-local caches only from measured fragmentation, allocation-rate, device, or topology pressure.

### Isolate serial output before a general driver framework

The first isolated AOT driver is an output-only polled serial service for the exact configured COM1 port range. It receives a rights-limited checked port-I/O capability, accepts bounded write batches, and owns ordinary console output. It has no input, interrupt, DMA, discovery, or arbitrary-port authority.

The kernel retains a separate early-boot and panic serial sink. Driver failure closes or faults ordinary clients without removing that emergency path. Restart requires the old grant generation to be revoked, hardware quiescence to be established, and a fresh generation to be bound.

### Grow discovery and capabilities in measured stages

Use the immutable directory provider as the first discovery service. Before process entry, the service manager resolves a canonical semantic interface identity, major version, and selected instance to a rights-limited endpoint. Applications do not perform ambient global-name lookup. Provider replacement creates a new generation; existing endpoints close or report peer loss before explicit rebind.

Starting from the qualified second fixed endpoint, introduce capability behavior in this order:

1. copy one reduced-right, non-amplifying directory-call capability;
2. serve two clients through bounded queues with explicit backpressure;
3. add correlation plus cancellation or deadline for a read-only request;
4. prove provider replacement and new-generation binding;
5. qualify one versioned single-producer/single-consumer shared-memory data plane; and
6. add move semantics or more general delegation only for a measured ownership case.

Cancellation of transport waiting does not prove that a service mutation did not occur, and no indeterminate mutation is replayed automatically.

### Define the first filesystem core in Windvale terms

The source-facing API follows Windvale naming conventions, with operations such as `Open`, `Readˉat`, `Writeˉat`, `Setˉlength`, and `Close` on typed directory or file capabilities. Machine-facing capability and protocol identities retain their specified ASCII-safe convention.

- Open is relative to a granted directory and makes disposition, link-following policy, requested rights, and result type explicit.
- Read and write use checked explicit-width offsets and bounded byte chunks. The final public offset width waits for qualified `u64` support on every selected target.
- `Writeˉat` distinguishes rejection before mutation, exact partial progress, completion, and indeterminate completion. A library-level `Writeˉall` may repeat exact partial writes but never retries an indeterminate mutation without a specified idempotency contract.
- Length change and close are separate operations. Data flush, metadata or directory durability, append, enumeration, atomic replacement, rename, links, watches, permissions, sparse storage, memory mapping, and transactions are separate stronger interfaces.
- The granted directory instance defines or reports segment rules, comparison, normalization, collision behavior, limits, and traversal boundaries. Native Windows or Linux paths and handles do not cross the shared contract.
- Enumeration, when added, defines bounds, ordering, continuation identity, concurrent-mutation behavior, and whether it is a snapshot.

### Retain provider-specific virtualization evidence

- Pinned QEMU/Q35/TCG remains the deterministic emulation oracle.
- One physical or root Windows machine owns direct Hyper-V Generation 2 and optional WHPX evidence; one physical or root Linux machine owns KVM evidence. Reports include hardware, firmware, microcode, OS/kernel, hypervisor, IOMMU, CPU profile, topology, and nesting level.
- Nested virtualization remains developer evidence unless nesting itself is separately qualified. It does not replace physical baseline or performance evidence.
- Common qualification compares exact guest-image identity, verification, process result, fault containment, cleanup, and shutdown. It does not require identical firmware or device layouts.

The first Windvale VM-host backend remains provider-neutral at its internal contract and is selected as VMX or SVM from the most stable measured physical machine. Its minimal profile has one x86-64 vCPU, fixed private memory, exact reset and entry state, no firmware or devices, bounded exits, and one terminal result. A later performance profile adds negotiated paravirtual timer, console, immutable block, network, and shared queues. PC compatibility remains a separate profile.

### Start accelerator evidence with exclusive ownership

The first physical GPU or AI-accelerator proof uses a secondary non-display device in exclusive passthrough mode only after IOMMU-group isolation, interrupt remapping, ownership, reset, DMA revocation, fault teardown, and rebind are measured. A software or paravirtual implementation remains the semantic oracle. Hardware sharing or partitioning waits for hardware that can prove its stated isolation and reset guarantees.

Performance evidence records exact hardware, firmware, microcode, provider, topology, build, configuration, workload, and input identity. Suites cover VM entry/exit, interrupt latency, memory and TLB behavior, storage, network, graphics, and representative compute. Report median and tail distributions and establish per-machine regression thresholds only after measuring noise. Do not treat those thresholds as portable semantics or overcommit CPU or memory in the first profile.

## Consequences

The next OS steps remain small and attributable while establishing records that can survive dynamic process creation. Timer, memory, driver, discovery, and filesystem work each receive a narrow initial proof without pretending to implement a general scheduler, allocator, driver framework, registry, or VFS.

Filesystem APIs use Windvale source naming and semantics while provider protocols remain ASCII-safe boundaries. Windows, Linux, and Windvale OS can implement the same small core without hiding stronger instance-specific guarantees.

Virtualization and accelerator selections remain grounded in owned hardware. Nested development helps implementation speed but does not weaken baseline or performance claims.

No timer, allocator, isolated driver, filesystem, Hyper-V/KVM qualification, VMX/SVM backend, passthrough device, or performance budget is implemented by this decision. The third process and ready/wait dispatcher are qualified existing evidence from Decision 0176, not an implementation claim of this decision.

## Reconsider when

- the timer-preemption proof requires a record that cannot evolve without making `WVPROC17` a public ABI;
- TSC or local-APIC evidence is unreliable on the selected hardware or providers;
- bitmap allocation fails a measured fragmentation or allocation-rate bound;
- polled serial output prevents required progress or containment evidence;
- the common filesystem core cannot preserve identical observable outcomes across selected providers;
- physical inventory makes the proposed VM or device sequence unsafe or unavailable; or
- measured data-plane overhead justifies earlier shared-memory transport.
