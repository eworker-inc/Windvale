# Decision 0803: Make compute performance and efficiency a 2027 program

- Date: 2026-08-20
- Status: Accepted strategic direction; implementation and quantitative claims
  pending
- Architecture: [compute and efficiency](../Architecture/Compute-And-Efficiency.md)
- Roadmap: [2027 compute leadership](../Project/Windvale-2027-Compute-Leadership-Roadmap.md)

## Context

Windvale intends to support useful databases, backend services, local AI,
data-parallel applications, networking, and its own operating system. Those
products will not become compelling through language design alone. Their
compiler, runtime, memory paths, accelerator providers, network data planes, and
kernel mechanisms must use modern hardware efficiently.

“Fastest” cannot be an unqualified project promise. Results vary by workload,
hardware, driver, operating system, correctness policy, latency target, power
limit, input size, and comparison implementation. Optimizing one microbenchmark
can also make an end-to-end application slower, less safe, or less predictable.

Windvale nevertheless needs an ambitious performance program. A small stack can
gain an advantage by carrying typed ownership, exact bounds, explicit
capabilities, immutable build evidence, and end-to-end resource accounting
through every layer instead of treating optimization as a late backend patch.

## Decision

Make 2027 the first Windvale compute-performance and efficiency program. Run it
as a cross-cutting product lane spanning:

- compiler analysis, optimization, native lowering, and target code generation;
- runtime scheduling, allocation, memory locality, and asynchronous execution;
- CPU vector and parallel execution;
- NVIDIA and AMD accelerator providers;
- high-throughput and low-latency networking;
- Windvale OS scheduling, IPC, interrupt, memory, DMA, and device mechanisms;
- WVDB and storage paths that feed or consume compute; and
- reproducible profiling, benchmarking, energy measurement, and regression
  control.

The program's north star is to earn leadership on named public workloads. Every
performance claim must identify the exact workload, correctness contract,
hardware, firmware, driver, operating system, tool identities, configuration,
power policy, comparison systems, repetitions, uncertainty, elapsed time, and
peak or working-set memory. Energy claims use measured full-system energy when
practical and distinguish it from device telemetry or rated power.

Windvale may say that a result is **leading** only when it exceeds every
predeclared qualified comparison on the named primary metric by more than the
measured noise while meeting the same correctness, latency, memory, safety, and
resource limits. It may say that a result is **efficiency-leading** only when
the same evidence also leads the selected energy-per-work or work-per-joule
metric. The comparison set is fixed before tuning; exclusions require a recorded
reason. No repository document claims universal or unmeasured performance
leadership.

Use one simple correctness oracle for every optimized path. Optimize data
movement, allocation, batching, locality, concurrency, and end-to-end scheduling
before assuming that more instructions or more parallel workers are better.
Retain clear baseline implementations for differential tests.

Keep the following boundaries:

- Windvale source semantics and safety do not change by target or optimization
  level;
- CPU WVA remains separate from accelerator device representations;
- the same Windvale compiler frontend and semantic analysis admit
  target-scoped kernels;
- vendor formats and libraries remain backend or provider mechanisms, not
  Windvale semantics;
- the kernel owns privileged enforcement and fast mechanisms, while protocol,
  accelerator, storage, and product policy stays in isolated services; and
- a fast path is a separately admitted capability or profile when its memory,
  polling, DMA, isolation, power, or fairness contract differs from the normal
  path.

Assembler and encoder work is evidence-directed rather than restricted to an
already blocked call site. A current consumer, accepted near-term architecture,
hardware experiment, diagnostic tool, or performance investigation may justify
a coherent bounded instruction family with a named owner and executable test
plan. This does not authorize an indiscriminate machine catalog.

The program advances alongside the direct Windvale 1.0 workstreams. It does not
silently make complete accelerator or Windvale OS support a 1.0 release
requirement. Performance and memory regression gates remain required for every
shipped 1.0 component; a later decision may admit a finite accelerator or
compute profile into the product gate when its contract and implementation are
ready.

## Consequences

- Performance becomes an architecture property with owners and evidence, not a
  cleanup activity after correctness work.
- Compiler, library, database, networking, and OS slices may prioritize work
  that removes a measured end-to-end bottleneck or enables the accepted
  measurement program.
- The first cross-vendor accelerator path can use software and SPIR-V/Vulkan;
  CUDA/PTX, HIP/ROCm, and vendor libraries may follow from measured gaps.
- A hosted Windows/Linux provider can qualify real hardware before Windvale OS
  owns a complete GPU or physical-NIC driver.
- Some peak-performance profiles will require affinity, reserved cores, pinned
  memory, polling, huge pages, or device-specific artifacts. Those requirements
  remain explicit and are never presented as portable defaults.
- Build time, code size, memory use, tail latency, fairness, and energy can veto
  an optimization whose throughput result looks attractive in isolation.
- Public comparison results must remain reproducible and must name where
  Windvale is slower as well as where it leads.

## Reconsideration triggers

Revisit this decision if measurement shows that the selected workload suite is
not representative, if one shared semantic contract prevents competitive
implementations on two important providers, if an optimization boundary weakens
correctness or containment, if energy measurement is not reproducible, or if
the program materially delays a more important product requirement without
producing transferable compiler, library, runtime, networking, or OS value.

Any revision must change the named workloads, claims, architecture boundary, or
resource allocation explicitly. A vendor benchmark, peak hardware number, or
single microbenchmark must not silently become Windvale's definition of useful
compute.
