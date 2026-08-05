# Decision 0232: General native call directory

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0228](0228-Bounded-Acyclic-Native-Call-Directory.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The Windvale-written x86-64 lowerer admitted only calls to a lower function ordinal and required exported `Main` to be the final function. That kept its original one-pass measurement simple, but excluded forward calls, self-recursion, cycles, and otherwise valid compiler-produced layouts. A reviewed three-function oracle is emitted as `Alpha, Main, Zeta`; `Main` therefore calls a later function even before a verified same-signature mutation connects both recursive helpers into a mutual cycle.

ABI 22 already provides the missing dynamic safety boundary. Every function entry decrements the shared call-depth budget and traps with `WVR3004` before an over-depth frame can execute. Every WVB instruction, including a call, consumes the shared instruction budget. Rejecting cycles statically is therefore unnecessary for this bounded native profile.

## Decision

### Admit any in-range scalar call graph

Retain the one-through-eight-function limit, exact scalar signatures, zero-through-four register arguments, `i32` returns, frame limits, instruction metering, and parameterless exported `Main() -> i32`. Permit the one export to identify `Main` at any function ordinal. Permit every direct call target whose ordinal is in range and whose argument types exactly match the complete function directory, including forward, self-recursive, mutually recursive, and cyclic edges.

No call graph is expanded during admission. Dynamic instruction and call-depth budgets remain the termination and resource boundary, and propagated failure is still checked immediately after every call.

### Build signatures before measuring code

Make function processing explicitly three-pass:

1. validate every adjacent function declaration and code range while collecting a complete immutable signature directory;
2. analyze each function against all signatures and construct the final machine-offset/length directory; and
3. emit each function against that complete directory.

This preserves the existing 16-byte layout entry and exact call encoding. Existing decreasing-ordinal and code-only WVO bytes remain unchanged.

### Preserve canonical object symbol order

Pass the exported `Main` ordinal explicitly to the focused object writer. Emit every local `$function_NNNN` symbol in ordinal/name order while skipping `Main`, then emit exported `Main` last so WVO binding/name ordering remains canonical. Add `$function_0007`, which becomes necessary when `Main` is not the eighth function in a full eight-function module.

The already-large instruction core owns the bounded multi-pass orchestration because function parsing and control/type analysis are its existing invariants. The focused 83-line layout module retains directory access and naming, while the 172-line object module retains WVO symbol projection; no numbered source fragment or duplicate parser is introduced merely to reduce line count.

### Extend the affected differential proof

Review the shared backend and package identity tests before execution. Derive a verifier-approved three-function graph with a real forward edge and same-signature forward/back mutual recursion. Stage 0 interpretation and native execution return 42 at call depth five and trap with `WVR3004` at four. The Windvale memory adapter, hosted tool, and generated native tool must reproduce Stage 0's exact 4,350-byte `.text` and 4,491-byte WVO. An out-of-range target remains `Unsupportedˉcode`.

Run only those focused Fast selections. Refresh the derived module and paired package identities once, without running Standard, Qualification, the full Seed/OS suites, Linux execution, or GitHub verification before the grouped end-of-goal gate.

## Consequences

- The accepted native backend now owns bounded general scalar call order and recursion rather than requiring a topological function order.
- Export order no longer leaks into machine entry or WVO symbol assumptions.
- Existing code-only, data, control, and decreasing-call differential objects remain byte-identical.
- The C# backend remains the frozen complete recovery and differential oracle. More than eight functions, stack-passed or descriptor parameters, non-`i32` returns, multiple/text/bytes data, nominal values, capabilities, and the complete backend remain outside this candidate.
- No artifact is promoted and no ordinary launcher changes in this decision.

## Reconsideration triggers

Increase the function bound only for a measured real input and with explicit aggregate code, directory, instruction, and depth limits. Extend call signatures through the shared ABI rather than a second convention. Reconsider the core boundary only when function admission, control analysis, or machine selection can move behind a cohesive model without duplicating parsing or obscuring metering and trap invariants.
