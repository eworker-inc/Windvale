# Decision 0259: Bounded native multi-record call events

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0240](0240-Bounded-Native-Record-Calls.md), and [Decision 0256](0256-Compact-Native-Record-Liveness.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The record scratch planner formerly encoded one optional record operand beside each instruction result. A direct call therefore rejected its second record parameter even though ABI 22, the typed call directory, and machine emission already support up to 64 parameters. The real hosted lowerer contains a function-18 call to a four-record-parameter helper, so this was a necessary independent closure gap.

The full tool does not yet reach that call in its own lowering pipeline. Its complete signature preflight first rejects function 117, `__WvM1F1(bytes, enum) -> record`, because enum parameters and returns remain outside the admitted subset. This corrects earlier progress wording that called function 18 the active self-lowering rejection.

## Decision

Encode each instruction's scratch-liveness event as a block index, optional record result, bounded record-use count, and ordered record-value identifiers. Direct calls append every record operand admitted by the existing 64-parameter signature limit instead of rejecting the second one.

During event replay, first require every listed operand to be live. Only after all operands pass validation may the planner release values whose last use is the current event. This preserves duplicate-operand validation and simultaneous input/result interference while retaining the existing 128 produced-record-values-per-block bound.

Add one compact source fixture whose `Sum` helper accepts four one-field records and whose `Main` returns 42. Stage 0 and both Windvale adapters must produce the same complete WVO, and Stage 0 native execution must return 42.

## Consequences

- Record calls now use the same zero-through-64 parameter bound as the shared call directory instead of a planner-specific one-record ceiling.
- The focused native-lowering selection passes in 16.202 seconds, including the new four-record call and exact complete-WVO equality through both Windvale adapters. The rebuilt test project reports zero warnings and errors.
- The core closure is 326,367 bytes at SHA-256 `dac68be2db94fbb47d7b046767239a2e3306647ba2ca8a844a27c560b80d87e0`.
- The memory adapter is 321,456 bytes at SHA-256 `66f587377a5cc6767dcbfcbce4ce1ad3f90cc339d159d3cefa72bb0f5a9f7453`; the hosted tool is 322,484 bytes at SHA-256 `a531f5a2d3d8aa8bb66e4481688344b65ef489d159c9bb8305052933c90baf11`. Both reproduce exactly through the pinned native source front door.
- Current unpromoted packages are 4,451,328 Windows bytes at SHA-256 `5c3082e718b1559a29bb53cfe9119899685fd63d4f5bf55138f1ed4a383902e6` and 4,452,352 Linux bytes at SHA-256 `5781a507f50a1f49a55b07d92dc4604e9933b72b4a3282f9353ba590e4acbb01`.
- Direct self-lowering remains fail-closed without output at function 117's enum-bearing signature. Enum parameter and return admission is the next active slice.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit sparse event encoding only if a qualified accepted module reaches the existing 64-parameter or 128-record-value bounds, or if profiling shows event construction materially dominates native lowering. Preserve per-use validation and release-after-validation ordering in any replacement.
