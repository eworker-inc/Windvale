# Decision 0112: Bounded exact-compiler record arena

- Date: 2026-08-02
- Status: Qualified at exact commit `bbec1aee901d6471b3ff0e56e65f656a4cd53ed1`
- Retains: Native ABI 20, execution-context version 7, and target `x86-64-wvb-baseline-v20`
- Refines: [Decision 0068](0068-Bounded-Native-Nominal-Values-And-Wvdump-Structural-Core.md) and [Decision 0111](0111-Bounded-Exact-Compiler-Fragment-Publication.md)

## Context

Decision 0068 introduced one fixed 1 MiB execution-scoped arena for immutable records. Construction advances a checked monotonic cursor in complete 16-byte field cells, and every record expires when `Main` returns. That capacity was sufficient for the structural `wvdump` consumer, but it was not derived from complete compiler execution.

Decision 0111 admitted the exact 4,556,121-byte compiler fragment through independent decoding and live W^X publication without changing ABI 20. Execution against the existing function-only source fixture then reached deterministic `WVR3017` before any output. The next decision therefore needed measured compiler allocation rather than an unbounded increase or speculative garbage collector.

A diagnostic execution using the same compiler fragment, source, four-billion-instruction ceiling, services, and context contract measured a 1,480,096-byte record-arena high-water mark and a 4,340,388-byte text-arena high-water mark. The existing 16 MiB text capacity is sufficient. Repeating the execution with a 2 MiB record arena produced the same measurements and completed successfully.

## Decision

- Raise only the current Windows/Linux host executor's hard record-arena capacity from 1 MiB to 2 MiB. The context already carries the arena pointer and runtime length, so its 112-byte layout and generated allocation checks do not change.
- Retain checked monotonic construction, immutable values, no individual free, no garbage collection, and whole-arena release after `Main`. Current evidence does not justify a more complex lifetime model.
- Expose execution high-water marks to the internal conformance suite through a Stage 0 measurement seam. Ordinary executor entry points and generated code remain unchanged.
- Require the exact compiler to consume exactly 1,480,096 record bytes and 4,340,388 text bytes while compiling `Tests/Fixtures/Source-Wvb/Function-Only.wv`.
- Require successful native output to equal the independent Stage 0 compiler's exact 815 WVB bytes, with stdout `source wvb status=Valid functions=4 code-bytes=532 module-bytes=815` plus LF and an empty diagnostic stream.
- Retain native ABI 20, execution context 7, service table 5, WVB 1.6, WVO 1.0, the 8 MiB fragment ceiling, and all selected machine bytes. The 2 MiB host capacity leaves 617,056 bytes of measured headroom.
- Keep Windvale OS execution contexts independently sized by their owning profiles. Probe 32's 1,024-byte in-page interpreter arena and its exact 528-byte use do not change.
- Keep the separate 4 MiB WVO/object and flat-linker limits unchanged. Native AOT publication of the exact compiler remains a later measured decision.

## Evidence

The focused exact-compiler test invokes the normal executor with the implemented 2 MiB capacity. The compiler returns zero, publishes the exact 815-byte Stage 0 result, emits the exact success report, emits no diagnostics, and reports the pinned record/text high-water marks above.

The existing exhaustion fixture now requests more than 2 MiB of record cells and still reaches `WVR3017`, proving that the revised bound remains enforced.

The exact selected compiler remains 4,556,121 bytes with SHA-256 `8e74707df03a535e3ef68cfcfc8da6fa68fda29ccf4344e272fc50c8a5845bab`. Windows Development passes a zero-warning Release build, all 67 regular Seed tests, and all 25 OS tests; the complete command takes 74.1 seconds. Windows Standard passes all 68 Seed tests and all 25 OS tests in 241.2 seconds wall time; Seed in-process time is 227.737 seconds, including 169.396 seconds for the golden compiler-reproduction contract.

Exact implementation commit `bbec1aee901d6471b3ff0e56e65f656a4cd53ed1` passes GitHub [Verify run 30769250223](https://github.com/eworker-inc/Windvale/actions/runs/30769250223). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 68 Seed tests, all 25 OS tests, retained planner reproduction, and the complete native CLI gate. The exact compiler case takes 1.678 seconds on Windows and 1.414 seconds on Linux. Windows Seed takes 227.437 seconds with a 169.266-second golden contract; Linux Seed takes 192.121 seconds with a 141.888-second golden contract. The complete jobs finish in 8m46s and 7m14s respectively. QEMU was not rerun because the fragment, ABI, generated machine bytes, and all OS inputs remain unchanged.

## Consequences

The exact Windvale compiler can now execute natively through the current Windows/Linux W^X host path and compile a real source file to byte-identical WVB. Its observed record-allocation blocker is closed without adding a general allocator or changing source, bytecode, machine, capability, or platform semantics.

Two MiB is a security and resource ceiling, not a general memory promise. It is adequate for this exact compiler/input evidence; larger programs or concurrent native tools may expose new pressure. Measurement remains test evidence rather than a portable API.

This does not yet prove native Stage 1 to Stage 2 reproduction, standalone PE/COFF or ELF tools, exact-compiler WVO/AOT output, a Windvale-owned general allocator, or .NET retirement.

## Reconsider when

- Measured exact-tool use approaches 2 MiB or a representative source workload exceeds it.
- Concurrent tools make the fixed per-execution reservation materially costly.
- Phase or region boundaries can reclaim meaningful compiler memory without weakening descriptor validity.
- Long-lived mutable graphs require allocation semantics that an execution-scoped immutable arena cannot provide.
- Windvale OS adopts the general host executor rather than an independently bounded system profile.
