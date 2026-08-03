# Decision 0188: First HPET-calibrated local-APIC preemption proof

- Date: 2026-08-03
- Status: Qualified
- Advances: [Decision 0181](0181-Next-Windvale-Os-Mechanism-Contracts.md)
- Retains: qualified Probe 38, protected-process version 17, the three-record ready/wait policy, and the private Stage 0 replacement seam

## Context

Probe 38 qualifies three protected processes and a state-driven ready/wait dispatcher, but every transition remains cooperative. Decision 0181 selects an invariant-TSC-or-HPET clocksource, an HPET-calibrated local-APIC clockevent, and an approximately five-millisecond private quantum as the next direction. A PIT prototype proved the architecture-neutral interrupt frame and context-switch core, but PIT does not satisfy that accepted machine direction and is not retained.

The smallest useful successor must prove involuntary progress across the existing three process roots without freezing `WVPROC17` as a scheduler ABI, claiming a general timer framework, or moving privileged hardware access into ordinary Windvale source.

## Decision

Firmware Probe 39 adds one bounded single-CPU preemption experiment before the retained Probe-38 workload:

- Portable `Process-Foundation.wv` owns the exact four-tick, three-switch policy model: directory to init, init to client, client back to directory, then terminal accounting. The quantum is 5,000 microseconds of private experiment evidence, not a public timing guarantee.
- Each existing user image exports one exact 88-byte WVA preemption probe. It initializes all fifteen saved general-purpose registers to process-specific sentinels, enables interrupts, and remains CPU-bound. The process-image builder resolves and verifies the exact linked function range; the machine builder revalidates those bytes and bounds before using its address.
- A private 224-byte `WVTHR001` context record for each participant retains identity, state, accounting, and a normalized 176-byte privilege-transition frame. One private 96-byte `WVTIME01` record retains clocksource, clockevent, vector, quantum, tick/switch/EOI counts, cursor, active identity, measured provider evidence, and terminal state. These offsets and record shapes remain replaceable internal evidence.
- The WVA timer seam masks both legacy PICs, requires architectural APIC support, reads `IA32_APIC_BASE`, requires enabled xAPIC at `0xFEE00000`, rejects x2APIC for this machine profile, validates the local-APIC version and enabled spurious-vector register, and owns vector 32.
- The pinned Q35 contract requires a 64-bit HPET at `0xFED00000` with period `10,000,000` femtoseconds. HPET is the first monotonic clocksource. Exactly 500,000 HPET ticks calibrate the local-APIC divide-by-16 initial count over approximately five milliseconds; a zero result is rejected.
- The local APIC runs one-shot mode. Every admitted interrupt writes EOI exactly once; each nonterminal switch rearms the measured initial count. The fourth interrupt records completion and stops HPET and the local-APIC event.
- The interrupt entry normalizes vector/error, saves all fifteen GPRs, uses the checked `SWAPGS` contract to acquire the current process record, validates the complete live frame, copies it into the outgoing context, activates the next process root, and resumes through `IRETQ`. Only controlled RFLAGS safety bits are exact; unrelated live arithmetic flags are preserved rather than guessed.
- HPET and local-APIC supervisor-only RW/NX uncached mappings advance paging to version 5. The additional page-table page advances the exact boot arena from 156 to 157 pages and memory state to version 16.
- Common WVA 1 gains exact no-operand `cpuid`, `read_tsc`, `read_msr`, `swap_gs`, and `interrupt_return` operations. `read_tsc` exists as a general measured primitive, but Probe 39 deliberately does not select TSC: a feature bit without calibration is insufficient under Decision 0181.

The normal, contained-user-fault, and contained-service-fault success transcripts add `timer-preemption=pass` only after the complete kernel path returns successfully. Terminal invalid-opcode and general-protection transcripts retain their earlier panic shape and do not claim the later success marker.

## Consequences

One CPU-bound participant cannot retain the CPU across this exact sequence: all three protected roots execute, all sentinel registers and privilege-frame boundaries survive, and directory resumes once before the bounded experiment ends. The retained service, resource, interpreter, fault-containment, reuse, and shutdown behavior then runs unchanged.

The hardware split is explicit. Portable Windvale owns the policy invariant; WVA owns measured privileged mechanics; Stage 0 still emits the private dispatcher/context orchestration and will be replaced incrementally. HPET is both source and calibration evidence for this first machine profile. TSC selection, x2APIC, deadline mode, provider abstraction, and physical-machine selection remain later contracts.

This is not complete Decision-0181 scheduler qualification. It does not yet prove delayed or coalesced interrupt handling, checked counter wrap, idle entry/wakeup, runnable wake latency, lost-tick recovery, priorities, multiple threads, dynamic run queues, SMP, physical APIC/HPET behavior, or a public scheduling ABI.

## Current evidence

- All 39 focused OS tests pass on Windows, including deterministic timer/context codecs, malformed and truncated records, exact WVA linkage, process-root preservation, and all five reproducible firmware artifacts.
- All 13 focused assembler tests pass, including reference/Windvale byte equality and malformed operand boundaries. The Windvale-owned assembler WVB is `e69b4ddf632ab21aba06aa79ad7c2e6c0d1f80f684ad904d9f80e12a7f1f783f`.
- The five diagnostic-free firmware images have the exact identities in `Specifications/Windvale-Os-Boot-Probe.md`; normal is 665,088 bytes with SHA-256 `415304780f360508f11cba337638aac4434746ee2e4a08133b06bf4a7f6e01df`.
- All five pinned Windows QEMU 11.0/Q35/TCG scenarios pass with exact serial and host-exit evidence.
- Exact implementation commit `6a250c86c30e8921d6bf9244a27d0fd763716cb0` passes GitHub [Verify run 30847279400](https://github.com/eworker-inc/Windvale/actions/runs/30847279400). Windows and digest-pinned Debian each complete a zero-warning Release build, all 87 Seed tests including the golden compiler contract, all 39 OS tests, and the complete native CLI gate. Linux finishes in 12m25s and Windows in 13m13s.

## Reconsider when

- HPET or local-APIC evidence differs on another selected provider;
- the initial count cannot remain nonzero and bounded over repeated calibration;
- delayed interrupts or counter wrap require a different accounting model;
- idle or wakeup evidence requires separating this fixed experiment from the retained dispatcher more sharply;
- invariant-TSC calibration justifies a clocksource change; or
- moving the context-switch policy into Windvale exposes a smaller durable record than this private Stage 0 layout.
