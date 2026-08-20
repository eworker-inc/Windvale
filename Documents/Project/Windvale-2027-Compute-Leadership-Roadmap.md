# Windvale 2027 compute leadership roadmap

## Status

Active strategic roadmap under
[Decision 0803](../Decisions/0803-Make-Compute-Performance-And-Efficiency-A-2027-Program.md).
The durable design is the
[compute and efficiency architecture](../Architecture/Compute-And-Efficiency.md).
Implementation, hardware selection, baselines, and quantitative leadership
claims remain pending.

This is a workstream and checkpoint plan, not a new release-stage ladder.
Windvale still targets `v1.0.0` directly. Compute work may ship incrementally
when qualified and runs alongside the Language 1.0, Libraries 1.0, WVDB 1.0,
package/service, and Windvale OS workstreams.

## End-of-2027 outcome

By the end of 2027, Windvale should have:

1. one reproducible benchmark, profile, and energy-evidence system spanning
   Windows, Linux, hosted accelerators, and selected Windvale OS mechanisms;
2. a measured optimized CPU path with target feature admission, vector
   execution, bounded parallelism, and memory-local allocation;
3. the same restricted accelerator workload running through the software oracle
   and qualified physical NVIDIA and AMD providers;
4. a high-throughput hosted networking provider plus measured Windvale OS
   syscall, IPC, scheduling, buffer, and packet-path mechanisms;
5. optimized WVDB and storage operations needed by the selected integrated
   workloads;
6. at least one package-backed inference workload and one network/data/compute
   service measured end to end for throughput, tail latency, memory, and energy;
   and
7. a public scorecard that states exactly where Windvale leads, matches, trails,
   or lacks evidence.

The year succeeds even if Windvale does not lead every workload. It fails if the
project accumulates optimized-looking subsystems without reproducible
end-to-end evidence or if performance requires weaker semantics or containment.

## Workstream dependency map

```text
measurement contracts and hardware inventory
  -> stable correctness oracles and comparison baselines
  -> compiler/runtime, accelerator, network, kernel, and data workstreams
  -> integrated workloads and energy evidence
  -> bounded public performance claims
```

The workstreams may execute concurrently after the measurement contract fixes
the inputs and result format. A backend can prototype before all baselines are
complete, but it cannot claim leadership without the complete comparison record.

## Preparation before 2027

Use the remainder of 2026 to remove measurement ambiguity:

- inventory available Windows/Linux hosts, CPU topology, memory, NVIDIA and AMD
  devices, NICs, storage, firmware, drivers, power controls, and measurement
  equipment;
- select one affordable repeatable hardware matrix, without claiming that it
  represents every deployment;
- freeze benchmark manifests, raw-result formats, timing rules, warm-up,
  repetitions, timeout, failure, memory, and energy fields;
- select reference implementations and capture untuned baselines before
  Windvale optimization;
- connect benchmark changes to focused verification owners and regression
  thresholds; and
- keep the software accelerator, interpreter, scalar native, ordinary network,
  and durable database paths as correctness oracles.

Recommended minimum lab coverage is one recent AMD x86-64 host, one recent Intel
x86-64 host, one NVIDIA GPU, one AMD GPU, and one multiqueue NIC usable from
Linux. Windows evidence may share the same dual-boot or equivalent hardware when
the resulting driver, firmware, and power-state differences are recorded.
Specific purchases remain a separate budget decision.

## Workstream A: measurement and regression control

### Deliverables

- Canonical benchmark-run and hardware manifests.
- Bounded timing, memory, counter, trace, and result readers.
- Repeated-run statistics with visible warm-up and outlier policy.
- Full-system energy input plus clearly labeled device telemetry.
- Before/after comparison and regression reports.
- A dashboard or static report generated entirely from retained raw evidence.
- CI smoke workloads distinct from longer lab qualification workloads.

### Gate

The same retained result can be independently inspected, hardware and tool
identity changes invalidate comparison automatically, and a deliberately noisy
or semantically incorrect candidate cannot earn a performance pass.

## Workstream B: compiler, CPU, runtime, and memory

### Deliverables

- Baseline and optimized lowering agreement.
- WIR analysis for ranges, aliases, ownership, liveness, and escape.
- Bounded inlining, specialization, loop, allocation, and bounds-proof passes.
- Target-feature records and deterministic CPU multiversion selection.
- The first coherent x86-64 SIMD encoder and lowering families.
- Vectorized byte, text, hash/checksum, comparison, scan, and numeric kernels
  selected from measured library and WVDB consumers.
- Bounded parallel execution, topology discovery, NUMA-local allocation, buffer
  reuse, and optional huge-page profiles.
- Source/WIR/native profile correlation and stable regression workloads.

### Gate

Optimized paths agree with the interpreter and scalar native oracle, reject
unsupported features before execution, and improve an end-to-end workload
without exceeding its build-time, code-size, memory, or tail-latency budget.

## Workstream C: NVIDIA and AMD acceleration

### Deliverables

- Complete software-provider correctness and failure evidence.
- A restricted verified accelerator representation.
- Deterministic SPIR-V emission and independent validation.
- A hosted Vulkan compute provider on Windows or Linux.
- The same fixed workload on one NVIDIA and one AMD device.
- Device-resident buffers, batched operations, asynchronous transfer/compute,
  and bounded completion queues.
- Profiling adapters that report dispatch, transfer, memory, occupancy or
  utilization, and failure/reset evidence without defining semantics.
- One CUDA/PTX, HIP/ROCm, or vendor-library experiment only when the common path
  exposes a named feature or performance gap.

### Gate

Both physical providers agree with the software oracle under the exact numeric
contract, expose their feature and resource limits, survive rejected input and
provider loss, release all retained resources, and publish end-to-end rather
than kernel-only measurements.

Native NVIDIA machine encoding, an AMDGPU assembler, multi-GPU execution,
training, and distributed collectives remain outside this gate unless a measured
consumer separately admits them.

## Workstream D: networking and secure service data paths

### Deliverables

- Ordinary host network provider baselines for connection, stream, datagram,
  HTTP, and secure-stream workloads.
- Buffer pools, checked scatter/gather, batching, backpressure, and exact partial
  progress.
- Multiqueue/RSS-aware placement and CPU/NIC NUMA locality.
- Measured interrupt, polling, and hybrid profiles.
- One zero- or single-copy receive/transmit experiment with exact ownership and
  fallback evidence.
- Comparative Linux provider experiments using ordinary sockets and, where
  justified, io_uring, AF_XDP, or DPDK-style queue ownership.
- Vectorized checksum, parse, copy, or cryptographic work selected by profiling.
- End-to-end TLS/HTTP or equivalent secure service goodput, p99 latency, CPU,
  memory, drop, and energy evidence.

### Gate

The fast profile preserves the normal semantic capability and failure contract,
reports whether copying, offload, polling, and kernel bypass are active, remains
bounded under overload, and can be revoked and torn down without leaking DMA,
buffers, queues, or authority.

## Workstream E: Windvale OS compute and packet mechanisms

### Deliverables

- Measured syscall, IPC, context-switch, wake-up, timer, mapping, and
  memory-object primitives.
- Per-CPU or per-queue state where it removes measured shared contention.
- Scheduler affinity and reservations with retained recovery capacity.
- Shared-ring batch publication and notification coalescing.
- One multiqueue virtual NIC path after the single-queue correctness profile.
- Physical interrupt, DMA/IOMMU, memory ownership, reset, and teardown evidence
  before a physical fast device becomes usable.
- Bounded hardware-counter and sampling support for privileged diagnostics.
- Evidence-directed WVA and x86-64 encoder additions for planned kernel,
  vector, profiling, and driver work.

### Gate

Each optimized mechanism retains exact rejection and cleanup behavior, remains
measurable under load, cannot consume the last recovery resources, and improves
one service or integrated workload rather than only a synthetic loop.

The 2027 program may qualify hosted acceleration and networking without waiting
for native Windvale OS GPU drivers. A Windvale OS physical accelerator or NIC is
earned only after its prerequisite device and containment boundaries exist.

## Workstream F: WVDB, storage, and integrated compute

### Deliverables

- Measured WVDB page, index, comparison, scan, update, commit, and recovery
  paths.
- Vectorized or batched encoding, checksumming, comparison, scan, and aggregation
  where semantics permit.
- Asynchronous storage and explicit queue-depth evidence.
- A package-backed local inference workload with retained weights and bounded
  device memory.
- A network request that parses bounded input, performs a WVDB lookup or scan,
  executes CPU or accelerator compute, and returns a secure response.
- A sustained ingest plus indexed-query workload showing backpressure,
  durability, memory stability, and recovery.

### Gate

The integrated workloads retain their correctness, durability, security,
authority, cancellation, and teardown contracts and publish component plus
end-to-end time, memory, data movement, and energy evidence.

## 2027 checkpoints

The dates are review checkpoints, not public compatibility stages.

### January–March: establish truth

- Complete the measurement plane and hardware inventory.
- Freeze the first workload and comparison manifests.
- Capture scalar CPU, interpreter, host network, software accelerator, and WVDB
  baselines.
- Add source-to-native profiling and choose the first SIMD and allocation
  consumers from evidence.
- Publish the first honest scorecard with no leadership claim required.

### April–June: remove CPU and data-movement bottlenecks

- Qualify the first SIMD/vector families and optimized library/WVDB kernels.
- Add bounded parallel execution, topology, NUMA-local buffer reuse, and
  asynchronous overlap.
- Complete deterministic SPIR-V emission and the hosted physical-provider
  harness.
- Measure normal and batched hosted network paths, including tail latency and
  overload behavior.
- Promote only improvements that change a component or end-to-end result.

### July–September: prove physical acceleration and fast queues

- Qualify the fixed accelerator workload on NVIDIA and AMD hardware.
- Compare SPIR-V/Vulkan with one vendor-native or vendor-library experiment if a
  measured gap justifies it.
- Qualify one zero- or single-copy hosted networking experiment and multiqueue
  locality.
- Measure Windvale OS syscall, IPC, scheduler, shared-ring, and virtual-NIC
  improvements.
- Start the integrated inference and network/data/compute workloads.

### October–December: integrate, challenge, and publish

- Stabilize the selected end-to-end workloads under sustained concurrency,
  cancellation, failure, and teardown.
- Measure full-system energy and performance operating points.
- Run exact comparison systems on the same hardware and publish raw evidence.
- Record where Windvale leads, is competitive, trails, or lacks support.
- Turn stable workloads into regression gates and select the next measured
  bottlenecks rather than automatically broadening the machine or device scope.

## Initial scorecard

| Area | Required primary evidence | Required guardrails |
| --- | --- | --- |
| CPU/library | operations or bytes per second | exact result, memory, code size, compile time |
| Accelerator | end-to-end requests or work per second | numeric quality, transfer bytes, p99, retained device memory |
| Networking | goodput or packets per second | p99, drops, CPU, copied bytes, overload and teardown |
| Kernel | operation latency and scalable throughput | isolation, fairness, recovery reserve, bounded cleanup |
| WVDB/storage | transactions, rows, or bytes per second | durability mode, p99, memory, recovery, write amplification |
| Integrated service | completed requests or tokens per second | quality, p99, memory, energy, failure and cancellation |

The first baselines set numeric regression limits. The roadmap does not invent
percentage targets before measuring the hardware and comparison noise.

## Work-selection rules

- Fix the largest measured end-to-end constraint that has an owned contract.
- Prefer reducing bytes moved, allocations, queue crossings, synchronization,
  and idle time before adding opaque complexity.
- Permit preparatory assembler, encoder, profiler, or hardware work for an
  accepted near-term consumer with a bounded test plan; an already blocked call
  site is not required.
- Retain a simple oracle and make optimized paths replaceable.
- Use external compilers, drivers, libraries, profilers, or benchmark suites as
  explicit providers and evidence sources when they accelerate learning.
- Do not implement a complete instruction set, native GPU assembler, kernel
  bypass, RDMA stack, multi-GPU runtime, or custom physical driver merely because
  high-performance systems sometimes contain one.
- Stop or redesign an optimization that weakens exact behavior, produces no
  representative gain, increases memory or energy disproportionately, or makes
  failure and teardown unbounded.

## Public reporting gate

A public performance report must include the run manifest, raw samples,
comparison selection, correctness evidence, primary metric, all guardrails,
measurement uncertainty, hardware/software identities, and reproduction steps.
It must disclose unsupported cases and meaningful regressions.

The words “fastest,” “leading,” “most efficient,” and “best” require the exact
scope and date in the same statement. Until such evidence exists, documents use
“target,” “design,” “planned,” or “measured improvement,” never an achieved
leadership claim.
