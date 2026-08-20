# Windvale compute and efficiency architecture

## Status

Accepted strategic direction under
[Decision 0803](../Decisions/0803-Make-Compute-Performance-And-Efficiency-A-2027-Program.md).
Implementation and quantitative leadership claims remain pending. The execution
plan is the
[2027 compute leadership roadmap](../Project/Windvale-2027-Compute-Leadership-Roadmap.md).

## Outcome

Windvale should become exceptionally fast and efficient on selected useful
workloads by treating performance as one end-to-end contract:

```text
Windvale source and libraries
  -> typed WIR, analysis, specialization, and optimization
  -> CPU native code | accelerator kernels | provider operations
  -> runtime memory, queues, scheduling, and asynchronous completion
  -> host providers or Windvale OS services and drivers
  -> CPU, GPU, memory, storage, and network hardware

measurement, correctness, bounds, energy, and reproducibility cross every layer
```

The project does not claim to be universally fastest. It aims to earn and retain
leadership on a published workload matrix while remaining competitive and
predictable on the rest of that matrix.

## Meaning of performance leadership

Every workload selects its primary and guardrail metrics before optimization:

| Dimension | Example measurements |
| --- | --- |
| Throughput | operations, requests, records, tokens, bytes, or packets per second |
| Latency | median plus p95 and p99 end-to-end completion time |
| Efficiency | joules per completed work unit and completed work per joule |
| Memory | peak resident bytes, retained bytes, allocation count, and bandwidth |
| Data movement | copied bytes, host/device transfers, DMA bytes, and cache or locality evidence |
| Scaling | useful speedup and efficiency across cores, queues, devices, and request concurrency |
| System cost | compile time, code size, startup, warm-up, and required reserved resources |

A throughput improvement does not pass if it violates the workload's
correctness, quality, tail-latency, memory, fairness, power, or teardown limits.
An optimized component result is useful evidence but not an end-to-end product
claim.

## Non-negotiable invariants

- Exact integer, overflow, ownership, bounds, capability, failure, and mutation
  semantics remain unchanged by optimization.
- Floating-point and quantized paths declare their exact strict or relaxed
  numeric mode. No target receives ambient fast math.
- Untrusted input is rejected before expensive allocation, compilation, device
  upload, queueing, or execution whenever the contract permits.
- Every queue, batch, graph, worker set, compilation unit, profile, diagnostic,
  and retained cache has a finite limit.
- Optimized implementations keep a simple correctness oracle and differential
  corpus.
- Unsupported hardware features reject or select an admitted fallback; they do
  not execute an assumed instruction.
- Performance never turns provider acceptance into remote receipt, durable
  completion, or application commit.
- Recovery capacity, cancellation, provider loss, reset, and bounded teardown
  remain available under load.

## Measurement and evidence plane

Performance work begins with one Windvale-owned benchmark and profiling plane.
Each run records:

- exact source, package, WVB, native, kernel, and configuration identities;
- compiler, runtime, provider, driver, firmware, operating-system, and tool
  versions;
- CPU, cache, memory, NUMA, accelerator, storage, NIC, topology, and link facts;
- enabled instruction, vector, accelerator, queue, offload, huge-page, affinity,
  and power features;
- dataset identity, input size, concurrency, warm-up, repetitions, and timeouts;
- raw samples plus median, tail, dispersion, and failure/drop counts;
- wall time, CPU time, peak or working-set memory, bytes moved, and available
  hardware counters; and
- full-system energy where practical, with device telemetry labeled separately.

Identical run manifests must produce comparable artifacts. A result is not
comparable when hardware, thermal state, power policy, driver, correctness mode,
input, or workload contract differs materially. Raw results and unsuccessful
experiments remain inspectable so tuning does not become publication bias.

Stable workloads gain regression thresholds only after enough measurements show
their ordinary noise. A candidate must exceed that noise and any declared
minimum practical improvement before a leadership claim or complexity increase
is accepted.

## Compiler and CPU execution

The compiler uses one typed source and WIR architecture for baseline,
interpreter, JIT, and AOT execution. Performance work may add explicit passes
and evidence models for:

- constant and range propagation, dead work removal, and checked simplification;
- escape, alias, ownership, liveness, and allocation analysis;
- inlining, specialization, devirtualization, and direct capability dispatch;
- loop canonicalization, invariant motion, unrolling, fusion, and tiling;
- bounds-check elimination only where retained proof makes the removal exact;
- scalar replacement and stack or region placement for non-escaping values;
- instruction selection, scheduling, register allocation, and call lowering;
- CPU vectorization and target multiversioning; and
- profile-guided or auto-tuned choices carried as explicit versioned build
  inputs.

Start with a clear baseline lowering that remains the oracle. Optimized code must
agree with the interpreter and baseline native path on exact modes. Feature
dispatch selects only a verified artifact compatible with the admitted CPU and
OS context.

The first x86-64 vector work should cover coherent families needed by measured
library and database kernels: loads/stores, integer and floating arithmetic,
comparisons, masks, shuffles, reductions, and selected cryptographic or checksum
operations. SSE2, AVX, AVX2, AVX-512, and later extensions remain explicit target
features rather than ambient host assumptions. WVA and the reusable encoder may
advance ahead of production use when an accepted benchmark, compiler pass, OS
path, or hardware experiment supplies the owner and test plan.

## Runtime, memory, and scheduling

Most useful compute is limited by movement, waiting, or allocation before it is
limited by arithmetic. Runtime work therefore prioritizes:

- bounded arenas, slabs, pools, and reusable buffers for known lifetimes;
- move-owned buffers and views that avoid defensive copying;
- cache-aware layouts and compact representations measured against clarity and
  mutation cost;
- NUMA discovery, local allocation, affinity, and explicit cross-node costs;
- optional huge-page profiles for stable large working sets;
- bounded worker pools, structured parallelism, work partitioning, and
  backpressure;
- asynchronous operations that overlap independent CPU, device, storage, and
  network work;
- cancellation and deadline observation without per-item global contention;
  and
- reserved recovery and control capacity so saturation remains stoppable.

Schedulers may expose throughput, latency, and energy profiles. A low-latency
profile may reserve cores or poll bounded queues; an efficiency profile may
prefer interrupt-driven batching and fewer active workers. The selected behavior
and cost must be visible.

## Accelerator execution

The accelerator path retains the four-layer design in
[Windvale accelerator compute and AI](../Project/Windvale-Accelerator-Compute-And-AI-Design.md):
portable host logic, portable operations, target-scoped kernels, and providers.

The execution order is:

1. a deterministic software oracle;
2. one restricted verified accelerator representation;
3. SPIR-V/Vulkan physical evidence on NVIDIA and AMD hardware;
4. vendor-library operations where they preserve the portable contract; and
5. CUDA/PTX, HIP/ROCm, or future native device lowering for named capability or
   measured performance gaps.

Optimization focuses on end-to-end device use: keep reusable data resident,
batch transfers and launches, overlap independent transfer and compute, coalesce
memory access, reuse on-device storage, fuse operations when observable
semantics permit it, control register and shared-memory pressure, and select
dispatch geometry from measured target evidence.

PTX, cubin, fat binary, SPIR-V, and AMDGPU code objects remain provider or
backend artifacts. Windvale does not initially implement NVIDIA native machine
assembly or an AMDGPU assembler. A native device encoder requires a measured
need that the verified intermediate and bounded vendor providers cannot meet.

## Networking data plane

The semantic network contracts remain the same for ordinary and fast paths.
Performance profiles may add different provider mechanisms while keeping
address authority, completion meaning, cancellation, peer loss, and mutation
uncertainty exact.

The network path advances through:

- per-queue ownership and generation-safe descriptor rings;
- batching and vectorized packet or record processing;
- reusable fixed buffer pools and checked scatter/gather views;
- receive-side scaling, multiqueue steering, CPU/NIC locality, and affinity;
- interrupt moderation plus bounded polling or hybrid modes;
- checksum, segmentation, encryption, and other offloads only after exact
  feature discovery and fallback behavior;
- zero- or single-copy transfer where ownership and device support make it
  provable; and
- optional privileged kernel-bypass or direct-queue profiles with separately
  granted DMA, memory, CPU, and device authority.

The ordinary user-space protocol service remains the correctness and security
path. A fast provider may specialize established flows, buffers, or protocols,
but it does not move general packet parsing or policy into the kernel. DPDK,
AF_XDP, io_uring zero-copy receive, and other host facilities are comparative or
provider mechanisms, not Windvale application semantics.

## Windvale OS fast mechanisms

The kernel should make isolated high-performance services possible without
absorbing their policy. Its compute and networking priorities are:

- low-overhead validated syscall and IPC entry;
- per-CPU state, queues, allocators, and accounting where shared mutation is
  unnecessary;
- scheduler affinity, reservations, topology awareness, and bounded wake-up;
- precise monotonic timers, interrupt routing, moderation, and recovery budgets;
- page, memory-object, mapping, pinning, and optional huge-page mechanisms;
- IOMMU-scoped DMA ownership, descriptor validation, revocation, reset, and
  teardown;
- versioned shared rings with batch publication and notification coalescing;
  and
- hardware counter and trace access through a privileged, bounded diagnostic
  capability.

WVA may grow coherent instruction families needed for these mechanisms and for
profiling or hardware research. System-profile Windvale continues to own state,
validation, scheduling policy, budgets, and diagnostics. The kernel does not
gain TCP, TLS, database, model, tensor, or vendor-runtime policy.

## Storage and WVDB participation

Compute leadership includes supplying and retaining data efficiently. WVDB,
filesystem, and block providers should measure:

- page-cache and buffer-pool locality;
- batching, prefetch, asynchronous reads/writes, and queue depth;
- checksumming, compression, encoding, comparison, and scan vectorization;
- index traversal, range scans, joins, aggregation, and transaction contention;
- durable-write latency separately from accepted or buffered completion; and
- ingest-to-query or ingest-to-inference end-to-end cost.

Storage durability, recovery, and transaction semantics are never weakened to
win a throughput comparison. Read-only analytical or ephemeral compute profiles
may select different explicit contracts.

## Profiling, tuning, and artifact identity

Profiling must connect source locations, WIR operations, native instructions,
kernel dispatches, provider queues, syscalls, IPC, network flows, and storage
operations without retaining unbounded trace state. Sampling is preferred when
complete tracing would distort the workload.

Auto-tuning may choose tile sizes, vector widths, queue depths, batch sizes,
worker counts, fusion plans, or device kernels. The tuning search is bounded,
its inputs and objective are explicit, and its selected result becomes a
versioned artifact keyed by compatible hardware and provider identities.
Production execution does not perform hidden unbounded experimentation.

## Initial workload matrix

The first suite should contain all three evidence scales:

| Scale | Workloads |
| --- | --- |
| Micro | scalar/vector arithmetic, copy, hash, parse/encode, allocation, queue, syscall, IPC, packet, and page/index primitives |
| Component | compiler build, WVB execution, WVDB scan/index/update, HTTP/TLS service, network forwarding, accelerator operation, and custom kernel |
| End to end | package-backed local inference; network request through parse, WVDB lookup, compute, and response; sustained ingest plus indexed query |

Each workload fixes correctness, data, bounds, primary metric, guardrails, and
comparison set before optimization. Microbenchmarks explain a result; component
and end-to-end workloads decide whether it matters.

## Claim and promotion gates

An optimized path advances through:

1. semantic and malformed-input agreement with the reference;
2. stable micro evidence identifying the expected bottleneck;
3. component evidence with time and memory regression limits;
4. end-to-end evidence under representative concurrency and data size;
5. Windows/Linux and selected physical-provider comparison where promised; and
6. energy and Windvale OS evidence only when those claims are made.

“Leading” means the result beats all predeclared qualified comparisons on the
primary metric by more than the measured noise while satisfying every guardrail.
The comparison set is fixed before tuning, and every exclusion has a recorded
reason. A regression elsewhere is reported, not hidden by a composite score.
Results name the hardware class and date; they expire when a materially newer
comparison or driver/tool generation makes the claim misleading.

## External design evidence

The architecture follows established measured principles without adopting an
external semantic definition:

- NVIDIA's
  [CUDA best-practices guide](https://docs.nvidia.com/cuda/cuda-c-best-practices-guide/)
  emphasizes minimizing host/device transfers, coalesced access, parallel
  execution, and measured occupancy;
- AMD's
  [HIP performance guidelines](https://rocm.docs.amd.com/projects/HIP/en/latest/how-to/performance_guidelines.html)
  emphasize parallel execution, batching, memory locality, coalescing, streams,
  resource pressure, and profiling;
- the Linux kernel documents
  [network scaling](https://docs.kernel.org/networking/scaling.html),
  [AF_XDP rings and zero-copy admission](https://docs.kernel.org/networking/af_xdp.html),
  and
  [io_uring zero-copy receive](https://docs.kernel.org/networking/iou-zcrx.html);
- DPDK documents
  [poll-mode, batching, queue ownership, and NUMA locality](https://doc.dpdk.org/guides/prog_guide/ethdev/ethdev.html);
  and
- MLCommons publishes
  [inference performance and full-system power methodology](https://mlcommons.org/benchmarks/inference-datacenter/).

These sources inform measurement and provider design. CUDA, HIP, Linux, DPDK,
Vulkan, MLPerf, and their formats or APIs do not define Windvale semantics or
create compatibility claims.

## Deliberately open selections

The roadmap must select, from measured inventory and available resources:

- the first exact CPU, NVIDIA GPU, AMD GPU, NIC, storage, and power-measurement
  matrix;
- the initial comparison implementations and workload datasets;
- the first SIMD families and optimized library kernels;
- whether the first physical accelerator provider is Vulkan compute alone or
  includes one vendor library operation;
- the first normal and fast networking profiles;
- the first Windvale OS physical compute or NIC device; and
- the threshold at which native GPU lowering, multi-device execution, RDMA, or
  another advanced mechanism becomes worth its permanent cost.

Those selections require recorded evidence and budget; their absence does not
block the architecture, but no hardware-specific claim exists before them.
