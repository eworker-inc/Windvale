# Decision 0228: Bounded acyclic native call directory

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0206](0206-Register-Scalar-Calls-In-Windvale-Native-X64-Lowering.md)
- Builds on: [Decision 0224](0224-First-Native-Wvb-To-Wvo-Front-Door.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The Windvale-written x86-64 lowerer accepted one exported `Main` or one helper followed by `Main`. Calls were hard-coded to helper index zero, so the accepted subset could not lower the existing three-function `Add -> Build -> Main` control fixture even though the complete backend and reference runtime already shared its semantics. Adding special cases for another helper would enlarge an already substantial source file without establishing a reusable function-layout boundary.

## Decision

### Admit one bounded decreasing-ordinal graph

Accept one through eight functions with exported parameterless `Main() -> i32` last. Every other function returns `i32` and has zero through four register scalar `i32` or `bool` parameters. A call target must be strictly lower than its caller's canonical function ordinal. This permits calls from helpers and calls to different earlier functions while rejecting forward edges, self-calls, cycles, and recursion by construction.

Keep the existing per-function code, local, stack, frame, metering, and aggregate 32 MiB output limits. Calls still share ABI 22's instruction and call-depth budgets, test the packed status after return, and consume the callee's exact declared scalar signature.

### Extract function layout from instruction lowering

Add `Compiler/Windvale/Native-X64-Lowering-Layout.wv`. Its fixed 16-byte internal entry stores machine offset, machine length, parameter count, and four padded parameter-type bytes. It owns deterministic helper symbol names and canonical WVO function-symbol emission. The instruction core retains WVB admission, control/type analysis, machine selection, and branch/call patching.

The lowerer makes two bounded passes. The first parses adjacent functions, admits only calls to directory entries already constructed, and fixes every machine offset and length. The second emits code using that complete immutable directory. Existing one- and two-function WVO bytes must remain unchanged.

This is a cohesive source boundary, not a numbered fragment or a line-count rule. The core falls from 2,979 to 2,887 lines; the new 146-line module has one independently named responsibility.

### Extend the existing differential proof once

Review the affected shared-backend test before execution. Retain every existing exact one- and two-function comparison and add the existing `Calls-With-Control-Main.wv` fixture. It requires three functions, calls both earlier ordinals, returns 42 in the reference runtime, and must produce a WVO byte-identical to the frozen Stage 0 backend through both interpreted and native execution of the Windvale-written tool. A changed middle-function target proves self/forward calls fail closed.

Run that one focused case locally. Do not run Standard, Qualification, the full Seed/OS suites, or another GitHub loop. The grouped final gate still begins from refreshed upstream state and runs once on Windows and Linux.

## Consequences

- The accepted native lowering subset now includes bounded multi-helper control programs rather than only the return-42 single-function profile.
- Call signatures, sizes, and relative targets come from one explicit directory instead of global helper parameters and offset zero.
- Existing generated WVO bytes remain stable, while source-closure and packaged-tool identities change because the implementation is modularized.
- Static data, nominal values, descriptor and stack-passed parameters, non-`i32` returns, recursion, general call graphs, capabilities, and relocations remain outside the subset.
- C# remains the frozen complete recovery and differential backend until the complete Decision 0057 gate and final recovery archive succeed.

## Reconsideration triggers

Reconsider the 16-byte directory or decreasing-ordinal rule when a real accepted program requires more than eight functions, forward acyclic calls, non-scalar signatures, descriptor ownership, separate compilation, or relocatable calls. Do not relax acyclicity or bounds without an explicit graph-verification and resource contract.
