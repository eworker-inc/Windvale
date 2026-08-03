# Decision 0171: Future virtualization and accelerator architecture

- Date: 2026-08-03
- Status: Accepted architecture direction; implementation deferred
- Refines: [Decision 0044](0044-First-X64-Uefi-Boot-Environment.md), [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md), and [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md)
- Retains: pinned QEMU/Q35/TCG as the reproducible Windvale OS guest lane, explicit per-part platform scope, capability authorization, and the small-kernel/isolated-service boundary

## Context

Windvale OS currently runs as a guest in one exact x86-64 UEFI machine environment. QEMU's TCG accelerator emulates guest CPU execution in software and requires no nested virtualization; Hyper-V Generation 2 remains a separate future compatibility target. This proves a reproducible boot and operating-system path. It does not make Windvale a virtual-machine host and does not establish VMX, SVM, EPT, NPT, IOMMU, virtual-device, GPU, or accelerator contracts.

An operating system does not need to host virtual machines to be complete. If Windvale later acts as a host, however, the design must distinguish four responsibilities that are often hidden behind the word virtualization:

1. a guest machine and firmware model;
2. an execution engine, either software emulation or hardware-assisted virtualization;
3. host enforcement over vCPUs, guest memory, interrupts, DMA, and resource budgets; and
4. user-space policy for lifecycle, images, virtual devices, consoles, storage, networking, graphics, and compute accelerators.

QEMU can combine software CPU emulation through TCG with an emulated machine, or hardware-assisted CPU virtualization through providers such as KVM or WHPX while still supplying virtual devices. Direct Hyper-V uses another firmware and device environment. These routes must not be described as interchangeable evidence merely because each can boot a VM.

GPU and AI-compute access introduce another independent choice. A device may be emulated, shared through a host-owned paravirtual service, divided by a hardware or vendor partition provider, or assigned exclusively through passthrough. Those modes differ in compatibility, authority, isolation, reset behavior, performance, migration, and failure containment. One broad `gpu` or `compute` flag would conceal differences that applications and operators need to know before launch.

## Decision

### Separate guest qualification from VM hosting

- Preserve the exact QEMU/Q35/TCG environment as the reproducible qualification oracle until a later decision deliberately versions or replaces it.
- Permit separate developer-speed and compatibility lanes. A Linux host may bind a QEMU/KVM provider; a Windows host may bind QEMU/WHPX or a direct Hyper-V provider. Every run reports the selected machine, firmware, execution engine, CPU profile, and provider identity.
- Prefer non-nested environments for accelerator and compatibility qualification: QEMU/KVM on a physical or root Linux host, QEMU/WHPX on a physical or root Windows host when useful, and Windvale as a direct Hyper-V Generation 2 guest rather than as a guest of a nested Hyper-V instance. Here a root host is the host operating system with direct access to the physical hypervisor, not an L1 guest. A development VM and the direct Windvale VM may be siblings under the same outer hypervisor.
- Treat nested WHPX, KVM, or Hyper-V as optional development facilities for the baseline guest and accelerator lanes. They can accelerate an inner QEMU lane and later let a Windvale development guest exercise its own VMX/SVM backend, but they are never prerequisites for building, verifying, or booting Windvale. Each nested report names the outer hypervisor, nesting level, exposed CPU profile, and inner provider and does not become a support, physical-hardware, or non-nested performance claim. A later decision may qualify nested operation when nesting itself is the explicit feature under test.
- Do not use wall-clock timing, host CPU details, or an implicitly selected accelerator as portable semantic evidence. Accelerated lanes may compare guest image identity, verifier result, defined output, diagnostics, lifecycle result, and specified resource counters.
- Treat browser execution separately. A browser that boots the x86-64 OS image performs machine emulation; a WebAssembly runtime that executes WVB or shared Windvale semantics is not an OS virtual-machine host.

### Use a provider-neutral management contract

- Define a small versioned VM-management capability whose common semantics cover creation of a stopped VM from exact machine/image identities, declared vCPU and memory ceilings, attachment authorization, start, wait, guest-shutdown request, forced termination, terminal reason, and diagnostics. Put pause/resume in a separately bound lifecycle-control interface whose presence is known before use.
- Make execution policy explicit: require software emulation, require hardware virtualization, or prefer hardware virtualization with the selected result reported. Qualification never uses an implicit automatic choice.
- Keep software emulation, Windows hardware virtualization, Linux hardware virtualization, and future Windvale OS hardware virtualization as separate provider parts. Shared libraries may expose exact common management semantics; provider-specific controls remain platform-scoped extensions.
- Initially prefer launching the already bounded external QEMU machine provider on Windows and Linux. Direct KVM, WHPX, or Hyper-V bindings require a measured need for lower overhead or stronger lifecycle control and must not redefine the shared contract.
- A VM-management requirement is not authority to access firmware, images, host files, block devices, networks, displays, GPUs, accelerators, or passthrough devices. The application approves and the launcher grants each attachment separately.

### Keep the Windvale kernel mechanism-only

- Add hardware virtualization only after physical-memory ownership and reclamation, essential page-fault and interrupt handling, timers, scheduling, service lifecycle, and physical-machine evidence can support it safely.
- Let the kernel own feature discovery; VMX or SVM enablement; generation-safe guest-memory objects; EPT or NPT validation; vCPU creation, entry, exit, scheduling, affinity, and accounting; interrupt injection; bounded exit publication; and final teardown enforcement.
- Keep VMX and SVM as separate x86-64 machine backends behind one internal semantic contract. Implement the first backend only on measured available hardware; the other backend must not be invented by analogy without hardware evidence.
- Let WVA own irreducible privileged instructions and register transitions. Let system-profile Windvale own validation, state machines, budgets, diagnostics, and policy.
- Do not expose raw VMCS, VMCB, host pointers, physical addresses, or native file/device handles through application-facing contracts. Vendor-specific exit evidence may be retained in a bounded privileged diagnostic extension without changing normalized common outcomes.
- Do not place firmware, general device emulation, storage/network policy, GPU scheduling, or image management in the kernel.

### Put the VMM and devices in isolated services

- Run a protected Windvale VM-manager/VMM service that owns machine configuration, firmware selection, image loading, virtual devices, console policy, lifecycle coordination, and translation of normalized VM exits.
- Isolate complex device models so a malformed guest command or device-service failure terminates or disconnects the affected device or guest rather than panicking the kernel.
- Use versioned bounded shared-memory queues for high-throughput storage, networking, display, graphics, and compute. Separate the control plane from the data plane; batch submissions and notifications; enforce queue, byte, descriptor, time, and in-flight-operation limits; and define backpressure and peer-loss behavior.
- Reuse established firmware, bus, and paravirtual-device standards where they fit the required contract. Imported firmware or device-model components remain explicit external dependencies with provenance, license, update, validation, and replacement boundaries; they do not define Windvale language semantics.

### Build more than one machine profile deliberately

- The first Windvale-hosted proof uses a minimal Windvale VM profile: one x86-64 vCPU, private fixed memory, no PCI, storage, network, GPU, passthrough, or general firmware, and one defined terminal exit. The profile is first exercised by a tiny diagnostic guest; a later bounded slice boots a prepared Windvale guest.
- A performance-oriented profile later adds paravirtual timer, console, block, network, display, graphics, and compute devices one measured contract at a time. It avoids legacy emulation on the data path.
- A PC-compatibility profile later owns UEFI, ACPI, interrupt-controller, timer, PCIe, boot-storage, network, and display compatibility needed by ordinary Linux and eventually Windows guests. Compatibility evidence remains separate from the minimal and performance profiles.
- A machine profile and each device interface have explicit versions and negotiated feature sets. An unknown or unsupported required feature rejects the configuration before guest execution.

### Make GPU and accelerator attachment explicit

Every GPU, NPU, FPGA, or other compute attachment selects one visible mode:

| Mode | Durable ownership and use |
| --- | --- |
| Software implementation | A service implements the virtual device or compute contract without hardware acceleration; shareable and suitable for fallback or oracle evidence, but not a performance claim. |
| Paravirtual shared device | A Windvale service owns the physical device and grants each host process or guest bounded queues, memory, execution, display, and feature budgets. |
| Hardware/vendor partition | A provider grants one hardware partition or virtual function while retaining physical-device management. Availability, isolation, reset, driver, and migration guarantees are provider-specific. |
| Exclusive passthrough | One guest receives the complete device and native-driver path. The host and other guests cannot use it concurrently; reclaim requires quiescence, DMA revocation, reset, and revalidation. |

- Keep display/presentation, accelerated graphics, portable compute, native-device extensions, hardware partitions, and exclusive passthrough as separate capability interfaces.
- A shared GPU service validates or delegates validation of guest-visible queues and commands, limits device memory and execution, schedules fairly according to an explicit policy, and attributes faults where the hardware permits. A shared physical device or firmware may still create a common failure or side-channel domain; do not claim hostile-tenant isolation without hardware and provider evidence.
- Passthrough requires a separately qualified IOMMU and interrupt-remapping boundary, safe device topology, exclusive ownership, reliable reset, bounded MMIO mappings, and generation-safe DMA teardown. It remains disabled when any condition is unavailable. Passing the only display GPU requires another host console path.
- AI-compute accelerators follow the same four assignment modes. A portable Windvale compute interface may expose an exact common operation set, while vendor instruction sets, native APIs, model formats, precision modes, and partition controls remain optional or platform-scoped interfaces.

### Design the fast path without weakening containment

- Prefer hardware-assisted CPU execution with EPT/NPT for production performance while retaining software emulation as a fallback, debugging environment, cross-architecture route, and differential oracle.
- Represent a vCPU as a schedulable kernel object and keep it in guest execution until a real exit is required. Use a bounded shared run page or ring for normalized exits and responses rather than copying unbounded records.
- Validate and seal invariant VM/vCPU configuration before first entry. Fast paths may rely on that immutable evidence but must revalidate every mutable address, generation, right, size, and security-sensitive transition.
- Allocate guest RAM through explicit memory objects, preserve NUMA locality, make large pages optional, pin only accounted DMA memory, and disable dirty logging unless an authorized snapshot or migration contract needs it.
- Use asynchronous queues, batching, notification coalescing, and checked zero- or single-copy ownership transfer. Do not require a syscall, VM exit, or service round trip for every packet, block, display update, or compute command.
- Define vCPU affinity, reservations, caps, virtual topology, monotonic time, interrupt moderation, and host-reserved recovery capacity before supporting overcommit. The first production profile does not overcommit CPU or memory.
- Keep performance and compatibility profiles distinct. Performance measurements record hardware, firmware, provider, topology, configuration, and workload; they are regression evidence, not portable semantics.

### Make teardown and failure exact

- Treat guests, firmware, machine images, device state, snapshot/migration data, page tables, descriptor rings, shaders, compute kernels, and accelerator commands as untrusted input.
- Version and fuzz the kernel/VMM boundary independently. Bound exit rates, queues, copied bytes, pinned pages, interrupts, diagnostics, and service work so an exit or interrupt storm cannot starve host recovery.
- Reserve host memory, CPU, console, and diagnostic resources before launch. A guest or device service cannot consume the last resources required to stop it.
- Keep clean guest shutdown, timeout, pause completion, provider loss, contained guest fault, device removal/reset, service failure, and forced termination as different outcomes.
- Teardown stops and joins every vCPU, blocks new submissions, drains or fails outstanding queues, revokes interrupt and DMA access, resets assigned devices, invalidates generations, unmaps guest memory, and only then releases resources. An indeterminate external device mutation is never reported as a cleanly completed operation.
- Snapshot, migration, memory overcommit, deduplication, confidential-VM support, nested virtualization, hot device reassignment, and live passthrough migration remain separate later decisions.

## First implementation sequence

1. Preserve pinned QEMU/TCG and add optional explicitly reported QEMU/KVM and QEMU/WHPX smoke lanes using the same generated guest image. Prefer physical/root hosts for baseline qualification; nested runs are developer-speed evidence unless a later decision makes nesting the explicit subject of qualification.
2. Qualify the same `BOOTX64.EFI` as a direct Hyper-V Generation 2 guest under the outer Windows host, separate from any development VM and as a distinct machine/firmware contract.
3. Add read-only CPU virtualization feature discovery to Windvale OS; absence remains an ordinary reported result.
4. Define normalized internal guest-memory, vCPU, entry, exit, budget, and teardown models without exposing vendor control structures.
5. Use nesting for rapid development of one VMX or SVM backend when available, but qualify that backend on one measured physical Windvale machine by executing one vCPU with private memory and one terminal exit without devices or an outer hypervisor.
6. Move lifecycle and exit policy into one isolated Windvale VMM service and boot the minimal Windvale VM profile.
7. Add shared-memory transport and one paravirtual console/timer contract, then one immutable block source.
8. Add IOMMU ownership and teardown evidence before attempting one dedicated secondary-device passthrough proof.
9. Add shared graphics and compute only after the physical GPU/accelerator driver can isolate contexts, memory, budgets, faults, and reset behavior.
10. Add the second CPU-vendor backend and broader PC/Linux/Windows compatibility only from measured cases.

This sequence is a future branch, not the next mandatory Windvale OS milestone. Current scheduler, memory, interrupt, driver, filesystem, and physical-hardware work retains priority according to measured pressure.

## Consequences

- Windvale can expose one coherent VM-management concept across Windows, Linux, and Windvale OS without pretending that TCG, KVM, WHPX, Hyper-V, VMX, and SVM provide identical machines or evidence.
- The Windvale kernel gains only privileged enforcement mechanisms; firmware, device models, accelerators, and lifecycle policy remain replaceable and containable services.
- Minimal, performance, and PC-compatibility profiles can evolve independently rather than burdening the first proof with a complete emulated PC.
- Shared and exclusive GPU/AI-compute paths are both supported, but attachment mode, authority, isolation, reset, migration, and performance guarantees remain explicit.
- Passthrough can provide near-native performance but conflicts with simultaneous sharing and commonly constrains snapshot, migration, host display, and reset behavior.
- Shared accelerators improve utilization but enlarge the trusted driver/service and shared-hardware failure or side-channel domain.
- High performance depends on measured hardware support, low exit frequency, memory locality, paravirtual queues, bounded batching, and resource reservation; it is not obtained by weakening validation or allowing ambient DMA.
- No VM-hosting, hardware-virtualization, IOMMU, GPU-sharing, passthrough, accelerator, snapshot, migration, or performance behavior becomes implemented by this decision.

## Reconsideration triggers

Reconsider the placement or sequence when:

- measured vCPU or device-service transitions make the proposed split impractical;
- a required architecture lacks an equivalent safe guest-memory and vCPU mechanism;
- a sustained hostile-multitenant requirement justifies a smaller hypervisor beneath Windvale with Windvale OS as a privileged management guest;
- a well-bounded existing VMM, firmware, or device-model component substantially reduces risk after license, provenance, update, and containment review;
- hardware partitioning supplies stronger measured isolation than the shared-service model;
- a required snapshot or migration contract cannot coexist with the selected memory, timing, device, or passthrough model; or
- physical evidence shows that a normalized common contract would hide a security- or correctness-relevant VMX/SVM, GPU, IOMMU, or provider difference.

Any revision must identify the exact machine profile, execution engine, provider, device attachment, authority, and evidence being changed. It must not silently turn one host API, virtual machine monitor, GPU vendor, or accelerator stack into Windvale semantics.
