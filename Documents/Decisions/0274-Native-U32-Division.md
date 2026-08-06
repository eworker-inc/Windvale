# Decision 0274: Native u32 division and remainder

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0272](0272-Native-U32-From-U8.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Complete ordinal inspection after the `u32.from_u8` slice identified
`u32.remainder` as the first unsupported operation in the hosted lowerer
closure. Division and remainder share the same unsigned x86-64 operation and
the same divide-by-zero contract, so implementing them as one family avoids
duplicating analysis, status layout, and machine templates.

Stage 0 loads the left operand into `EAX` and the right operand into `ECX`,
rejects a zero divisor as `WVR3032`, clears `EDX`, and executes `div ECX`.
The quotient is already in `EAX`; remainder lowering additionally copies
`EDX` to `EAX`. Both operations store the result in a newly selected `u32`
slot. A function needs the division status tail only when its verified code
contains one of these operations.

## Decision

Admit opcodes `0xA1` (`u32.divide`) and `0xA2` (`u32.remainder`) through
typed analysis. Require two `u32` operands, replace them with one `u32`,
reserve the next `u32` value slot, and charge the shared ten-byte instruction
meter. Emit Stage 0's exact 33-byte quotient template or 35-byte remainder
template after that charge.

Patch only the left, right, and result frame offsets plus the relative
divide-by-zero branch. Append status 9 only to functions that contain this
family. Keep the operation-specific templates and stack transition in the
existing descriptor-instruction state, which already owns scalar stack types
and slots. Extract the repeated common status-tail sequence from the large
lowering core into one focused helper so the complete hosted tool remains
within the 2,048-cell native frame contract.

Teach record-storage analysis that both instructions are one-byte binary
scalar operations. This is required when division appears in a function that
also owns record locals; it does not add record semantics to the operations.

Add one focused record-bearing fixture that exercises the maximum unsigned
input, both quotient and remainder, both Windvale adapters, exact complete-WVO
equality with Stage 0, and the exact `WVR3032` divide-by-zero behavior.

## Consequences

- The focused selection passes in 2.462 seconds. Its 541-byte WVB has SHA-256
  `135892131fe0fd055d97530b4e6d5a055deb700729081db28f903c396b97750c`;
  the exact 4,088-byte WVO contains 4,015 code bytes and has SHA-256
  `cf45c322014c14b81fb8dca21e14ab40a1f0ba6655a87b6ba5e21657c9c89047`.
- The separate pinned-package case passes in 8.764 seconds. Both Release builds
  report zero warnings and errors.
- The core closure is 345,343 bytes at SHA-256
  `03da73d82695bd9ab6db3f781c9b426bd04e22c045788b97edeb1122de18280b`.
- The memory adapter is 340,284 bytes at SHA-256
  `093962a628ed013029b8c40f47e4a9771c18f08b9c441e79be9c72d6c8bd3d36`;
  the hosted tool is 341,312 bytes at SHA-256
  `ee1e6bc3306801fcf9258a1dd31df39a2a136cf4920d13f134619b0e16b09660`.
  The pinned Windows native source front door reproduces the hosted tool
  exactly in 17.6 seconds.
- Current unpromoted packages are 4,730,368 Windows and 4,730,880 Linux bytes
  at SHA-256
  `629b76bfbdf75060a72dc4162860de29b71e9675ed0cc26594a7a95c824eb9b3`
  and `ba95f29d2467206de3a3cd008301eb0171106108bd61d2d505830b3f3c1446cf`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` and publishes
  no output. The top-level opcode surface is now complete for this tool, but
  function 0 (`Main`) requires record-storage analysis and begins with
  `call.capability` at bytecode offset `0x0000`. That supplemental analyzer
  does not yet model capability signatures, making capability-aware record
  storage the next native-retirement slice.
- No C# product implementation changed. Stage 0 remains the independent oracle
  and recovery path until the grouped dual-host and complete retirement gates
  pass.

Local Standard, Qualification, the full Seed/OS suites, Linux execution,
GitHub verification, artifact promotion, and ordinary-path cutover remain
deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Revisit this lowering if WVB changes unsigned divide-by-zero behavior, ABI 22
changes scalar slot representation or status ownership, or x86-64 is no longer
the selected native backend for this target.
