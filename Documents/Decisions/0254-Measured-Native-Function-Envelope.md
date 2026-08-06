# Decision 0254: Measured native function envelope

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0251](0251-Bounded-Native-Wide-Calls.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After wide-call transport, the real 330-function hosted lowerer first failed its own general function admission at function ordinal 88. The Windvale selector still allowed only 1,024 combined parameters/locals, 8,192 code bytes, and 1,024 instructions per function. Those prototype ceilings were below already verified Stage 0 native input and frame contracts.

Direct inspection of the current tool found maxima of 1,717 combined parameters/locals, 26,980 code bytes, 5,929 decoded instructions, and operand-stack depth 18. Independent Stage 0 lowering measured the tightest projected native frame at 1,999 of the unchanged 2,048 cells. Raising the hard frame bound would hide pressure rather than transfer a missing admission boundary.

## Decision

Admit fewer than 2,048 combined parameters and declared locals, at most 32,768 code bytes, and at most 8,192 decoded instructions per function. Retain the existing one-through-1,024 declared operand-stack bound and the exact final 2,048-cell frame check over parameters, locals, reused typed value groups, record storage, and any hidden result.

Add one compact test-built WVB module with 1,025 `i32` locals, 10,246 code bytes, and 2,050 decoded instructions. It copies the default zero value through every local and returns the final cell. Stage 0 and both Windvale adapters must produce the same complete WVO. Constructing the repetitive canonical bytecode in the test keeps the boundary explicit without adding an artificially large source fixture.

Do not widen the separate record planner's 128-local, 128-block, 128-record-value, or 1,024-instruction ceilings in this decision. Do not add enum parameters or returns, multiple record arguments, new instructions, or other nominal shapes.

## Consequences

- The real hosted tool crosses its general local, code, and instruction guards while remaining below the unchanged native frame ceiling.
- A subsequent direct self-lowering run crosses the general envelope and fails during complete signature preflight at function ordinal 117's enum parameter. Static inspection also identifies a later multiple-record-argument accounting rejection in function ordinal 18 and the former record-planner capacity at ordinal 88; these are independent boundaries even though their ordinals are lower.
- The core closure is 322,860 bytes at SHA-256 `80066bc87774c70b4d2cc0b60b5605573480d5eea10a695f584a5d210a3f3c81`.
- The memory adapter is 317,949 bytes at SHA-256 `f3003741b9d5003575cfdadd611534ee6ba3aa7aa936b2c73184d05f023e72ff`; the hosted tool is 318,977 bytes at SHA-256 `450c1eb86d5ff564b04cdbd00f3919cce2c0372acdca32fbe1a5e30e0c05c414`.
- Current unpromoted packages remain 4,406,272 Windows bytes at SHA-256 `8bfc95be0d722d3b849956c2878c9f053b4242d78242b6c4fe5dc4782e86e660` and 4,407,296 Linux bytes at SHA-256 `6cd55057461252b6c36a1165e7e39167a50c9db31afb460be24b36e0db93d6e5`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Measure enum signatures, record calls, and record-planner boundaries independently. Revisit the general envelope only if a qualified accepted module reaches one of these bounds, and preserve the fixed native frame limit unless a separate ABI decision changes it.
