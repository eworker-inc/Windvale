# Decision 0320: Metered checked scalar dispatcher WebAssembly

- Date: 2026-08-06
- Status: Implemented for the direct scalar/control proof with local engine evidence
- Advances: [Decision 0319](0319-General-Scalar-Dispatcher-WebAssembly.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The first general dispatcher proved that verified Windvale basic blocks can run
as direct WebAssembly, but it exported an unmetered result function and rejected
all checked arithmetic. That was enough to cross the execution boundary, not
enough to run compiler code safely or preserve Windvale runtime evidence.

Loops and calls must share one exact WVB instruction budget. Integer overflow
must report the Windvale status instead of inheriting WebAssembly's wrapping
arithmetic or engine traps. A failed execution must never publish a stale or
partial result.

## Decision

- Replace the proof's direct root export with a private deterministic wrapper:
  `Windvale.run(i32 budget) -> i32 status`.
- Export mutable `Windvale.result` and `Windvale.instructions` globals. Reset
  both, the private budget, and the private status before every run. Reject a
  non-positive budget as `3011` before charging a WVB instruction.
- Charge every dynamically selected WVB instruction exactly once before its
  operation, including calls, branches, jumps, and returns. Dispatcher
  comparisons and loop branches remain target plumbing and are not charged.
- Share the globals across every reachable Wasm function. A budget failure sets
  status `3011`, returns the scalar zero value or void from the current helper,
  and propagates immediately through every caller.
- Move scalar call results through one private scratch local before inspecting
  failure status. This satisfies Wasm structured-stack typing while ensuring a
  failed call cannot leave a value available to its caller.
- Lower checked `i32.add`, `i32.subtract`, `i32.multiply`, and `i32.negate`, plus
  checked `u32.add`, `u32.subtract`, and `u32.multiply`. Every overflow or
  unsigned underflow sets status `3007`, returns without publishing a result,
  and retains the count including the failing instruction.
- Use signed bit tests for `i32` addition/subtraction, signed `i64` extension for
  multiplication, an exact minimum-value test for negation, unsigned carry or
  borrow comparisons for `u32` addition/subtraction, and unsigned `i64`
  extension for multiplication.
- Keep this a capability-free generated module and a local proof contract. It
  does not yet claim execution ABI 3, browser package replacement, call-depth
  containment, descriptor memory, or complete compiler support.

## Focused evidence

The expanded compiler-produced success fixture has three reachable functions,
backward control, calls, all seven newly admitted checked operations, scalar
comparisons, locals, and explicit control. Its WVB has SHA-256
`e84b83c282a8e03847c46b624148726b204cc09f774c228da114a775d7573359`.
The capability-free dispatcher tool is WVB
`c862142402d52be2a3cfa0b802094d0845e5299f177d04d60d3f2cc5a6aca672`.
It emits a deterministic 3,646-byte module with SHA-256
`6281a72c8704997640be8164f5ec834a82006f8df33b5ab0084159275fb13767`.

Node 24.18 validates and executes that module to result 42 in exactly 116 WVB
instructions. Budget 115 returns `3011`, publishes result zero, and reports
exactly 115 instructions. A repeated budget-116 run resets and reproduces the
successful result and count.

The separate checked-add overflow fixture has WVB SHA-256
`ec785b6ad0fe3a72574a3e6587d32bad0719054a8f7d9481ba361a0857086450`.
It emits a deterministic 770-byte module with SHA-256
`263aa2bb5c0589f679df3f415b8e3c97eb809a87a0a756d3dc312f79956244cc`.
Node returns `3007`, result zero, and 14 charged instructions; budget 13 returns
`3011` with exactly 13 charged instructions instead. The reusable probe reports:

```text
dispatcher-engine status=Valid module-bytes=3646 result=42 instructions=116 limited-status=3011 overflow-status=3007 overflow-instructions=14
```

The focused Seed contract passes in 0.721 test seconds after a zero-warning
incremental Release build. No broad Seed or WebAssembly-engine verifier was run.

## Consequences

Direct scalar/control Wasm now preserves the two runtime properties most likely
to disappear in an optimization shortcut: exact work evidence and checked
integer failure. The module can stop a nonterminating loop at a deterministic
Windvale boundary, and arithmetic failure propagates across direct calls without
using a WebAssembly trap.

The private contract still lacks a call-depth budget, so untrusted recursive
graphs cannot use this path in the browser yet. It also lacks descriptor and
nominal storage, static data, capabilities, wide and collection values, and the
`bytes -> bytes` host transport required by the real compiler. No end-to-end
compiler speed claim follows from this focused artifact.

## Reconsider when

- call-depth accounting joins the wrapper and changes private globals;
- descriptor ownership needs a non-scalar failure return convention;
- execution ABI 3 replaces this proof wrapper;
- engine measurements justify a `br_table` dispatcher; or
- the exact compiler artifact supersedes the focused fixtures.
