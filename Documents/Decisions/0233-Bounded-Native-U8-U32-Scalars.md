# Decision 0233: Bounded native u8 and u32 scalars

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0232](0232-General-Native-Call-Directory.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The Windvale-written x86-64 lowerer accepted eight functions but only `i32` and `bool` parameters/locals and only `i32` returns. Measuring existing compiler-produced fixtures showed that raising the function ceiling would unlock none of them. The four-function `Function-Only.wv` fixture was instead blocked by `u32` loop state and checked addition, one `u8` parameter and comparison, and a Boolean helper return.

ABI 22 already represents `i32`, `bool`, `u8`, and `u32` scalar values in one 16-byte frame cell and transfers the first four scalar arguments through the same dword registers. All four scalar result kinds return in `EAX`. The missing boundary was therefore typed verification and selection, not a new ABI.

## Decision

### Admit bounded u8 and u32 scalar shapes

Retain parameterless exported `Main() -> i32`, one through eight functions, at most four register parameters, existing frame/code/instruction/depth limits, general call order, and fail-closed WVB admission. Permit helpers to use `i32`, `bool`, `u8`, or `u32` parameters, locals, and returns.

Add exact lowering for the instructions exercised by the measured fixture: `u8.const`, `u32.const`, checked `u32.add`, `u32.less`, and `u8.equal`. Unsigned add branches on carry and preserves Stage 0's exact bytes. Other `u8` and `u32` operations remain fail-closed until a measured consumer and focused differential vector require them.

### Preserve one 16-byte call directory

Keep the internal directory entry at 16 bytes. Store machine offset and length in the first eight bytes, then one-byte parameter count, one-byte scalar return type, two reserved zero bytes, and four padded parameter-type bytes. Calls validate arguments against the complete signature and allocate their result in the return type's typed value-slot group.

Reuse value cells independently in canonical native type order: `i32`, `bool`, `u8`, then `u32`. The focused layout module owns both the packed directory projection and typed slot-offset calculation. The instruction core retains WVB parsing, stack analysis, exact machine-size accounting, and opcode selection; this follows a real ownership boundary without splitting the large core into numbered fragments.

### Prove the real compiler fixture

Extend the existing shared backend differential case with the canonical 816-byte `Function-Only.wv` WVB. Stage 0 interpretation and native execution return 6. The Windvale memory adapter, hosted tool, and generated native tool must reproduce Stage 0's exact 6,041-byte `.text` and 6,216-byte WVO.

Add one small source-defined scalar-return vector because `Function-Only.wv` exercises a Boolean helper result but not `u8` or `u32` results. Its `Byte() -> u8` and `Count() -> u32` calls must execute to 42 and reproduce Stage 0's exact 2,349-byte `.text` and 2,490-byte WVO through the hosted Windvale lowerer.

Keep identity pins after behavioral package assertions so a derived digest change cannot abort direct execution, deterministic repetition, malformed-output preservation, or CLR-absence checks and force a redundant rerun. Run only the reviewed shared-backend and package selections. Do not run Standard, Qualification, the full Seed/OS suites, Linux execution, GitHub verification, or artifact promotion before the grouped end-of-goal gate.

## Consequences

- The accepted lowerer now covers the existing compiler-produced scalar/control fixture rather than only purpose-built `i32`/`bool` vectors.
- Boolean, `u8`, and `u32` helper results use the existing scalar ABI and general call directory without a compatibility path.
- Every retained earlier differential WVO remains byte-identical.
- More than eight functions, the remaining `u8`/`u32` operations, wider integers, division/remainder/bitwise/shifts, text/bytes descriptors, multiple data declarations, nominal values, capabilities, and the complete backend remain outside this candidate.
- No artifact is promoted and no ordinary launcher changes in this decision.

## Reconsideration triggers

Add another scalar opcode family only against a measured blocked consumer and with its exact trap semantics. Extend descriptors through the shared ABI's ownership model rather than treating them as scalar cells. Reconsider the instruction-core boundary only when a cohesive parsed instruction or machine-selection model can move without duplicating verification state or obscuring metering and trap invariants.
