# Decision 0062: Dynamic native instruction budgets and backward control flow

- Date: 2026-07-31
- Status: Implemented and validated on Windows x64; exact-commit Debian qualification pending
- Refines: [Decision 0061](0061-Typed-Native-Blocks-And-Forward-Control-Flow.md)'s forward-only control boundary

## Context

Decision 0061 deliberately rejects backward branches because a byte-safe native fragment could otherwise run forever inside the host process. Windvale's reference interpreter charges every executed WVB instruction, permits exactly the configured positive maximum, and raises `WVR3011` before the next instruction. The shared native JIT/AOT fragment needs the same dynamic boundary without embedding one compile-time limit, patching code per execution, or generating different bytes for Windows and Linux calling conventions.

## Decision

- Advance the internal experimental target to `x86-64-wvb-baseline-v4` and ABI version `4`. There is no compatibility reader for version 3; Decision 0061's exact version-3 evidence remains historical at its qualified commit.
- Add an explicit `Nativeˉinstructionˉcharge` before every lowered WVB instruction. Semantic operations consume their preceding charge; explicit jump, branch, and return terminators consume a final charge; an implicit next-block fallthrough does not invent a WVB instruction.
- Pass one positive signed-64-bit maximum dynamically at execution. The executor supplies three unsigned machine arguments `(0, budget, budget)`: Windows x64 places the second argument in `RDX`, while System V x86-64 places the third there. The shared fragment therefore receives the same value in `RDX` on both hosts and copies it into reserved register `R11` without host-specific code generation.
- Emit each charge as an exact decrement of `R11` followed by an unsigned-underflow branch to `$instruction_limit`. Starting with maximum `N` permits exactly `N` charges and traps before charge `N + 1`, matching the reference interpreter. Packed status 2 maps to recoverable `WVR3011`; status 1 remains checked i32 overflow `WVR3007`.
- Admit bounded backward and self targets in typed native block graphs. Every selected branch target must land on an instruction-charge group, so entering any block through fallthrough, a forward edge, or a backward edge charges its first WVB instruction.
- Remove the special six-byte constant selector. Constants use the same budgeted typed-frame shape as every other admitted program, preventing an unmetered alternate execution path.
- Independently decode the exact budget-source prologue, both trap epilogues, every charge, every semantic group, and the complete cyclic control graph. Require strict charge/semantic alternation, exact instruction-limit targets, all control targets on charge boundaries, complete reachability, complete byte consumption, minimal initialized frames, and balanced exits before WVO or executable-memory publication.

## Initial evidence

The constant path permits its exact four WVB instructions and traps at three. A six-iteration loop executes exactly 157 WVB instructions and returns `42` in the reference interpreter, W^X JIT fragment, and WVO-linked AOT image when given 157; all three reject 156 as `WVR3011`. A genuinely nonterminating verified loop is stopped by the interpreter and native executor at a 50-instruction limit.

The deterministic loop fragment is 1,178 bytes with SHA-256 `8b4cde486874a5dcd7eded18e0d3fcb208e244de0499e868dc8e50501bd21139`; its 1,318-byte WVO has SHA-256 `7b0d7898a83eb87818b08dfcb4081ce6ff05e91f5dd2958ba0e869ebcae735ac`. The version-4 constant, arithmetic, and forward-control fragments are also pinned exactly. Corruption evidence rejects the budget source, decrement amount, limit target, limit status, non-charge control target, and all prior frame, comparison, overflow, and instruction-shape attacks as `WVN3030`.

The focused Windows test passes in approximately 0.17 seconds with a zero-warning build. Development passes all 48 regular tests in 47.721 seconds. Standard passes all 49 tests in 201.986 suite seconds and 205.709 wall-clock seconds. Exact-commit Windows/Debian Qualification remains required.

## Consequences

The admitted native subset now includes real loops without allowing generated code to outlive its explicit execution budget. JIT and linked AOT execution still consume identical verified bytes, and callers can choose a budget per run without rewriting those bytes.

One exact ten-byte charge sequence per WVB instruction is intentionally simple rather than compact. Its code-size and runtime cost are baseline measurements, not a permanent encoding promise. A later verified stencil or grouped-charge optimization may reduce overhead only if it preserves observable limits and cannot cross a trap, capability, allocation, call, or other future safe-point boundary.

`R11` is reserved for the current single-function budget. Native calls must preserve one shared remaining budget through an explicit ABI rule rather than silently resetting it or trusting caller-saved register behavior. This decision does not claim calls, static data, capabilities, asynchronous cancellation, allocation/GC safe points, PE/ELF hosts, native compiler execution, Windvale-written runtime ownership, or .NET retirement.

## Reconsider when

- Shared-ABI calls require the remaining budget to cross function boundaries.
- Runtime services require separate allocation, cancellation, scheduling, or reclamation safe points.
- Measurements justify a compact charge representation without weakening exact instruction-limit behavior.
