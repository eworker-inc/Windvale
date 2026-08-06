# Decision 0276: Capability-aware record storage

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0274](0274-Native-U32-Division.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The hosted lowerer uses a supplemental record-storage pass for functions that
contain record locals or temporary record values. That pass follows bytecode
control flow and scalar-versus-record stack shapes so it can plan persistent
local fields and scratch record fields before machine lowering.

Decision 0274 completed the top-level opcode surface required by the hosted
lowerer, but direct self-lowering still stopped at function 0. Its first
instruction is `call.capability`, and the supplemental pass did not receive the
already-validated capability directory. Its local instruction table also
lagged the accepted top-level scalar surface, so clearing the first instruction
alone would only move the same failure to later functions.

## Decision

Pass the capability-kind bytes produced by the existing strict capability
reader into record-storage analysis. Admit `call.capability` only when its
index selects one of those validated kinds. Apply the canonical arity and
return shape for all six supported capabilities, reject record arguments, and
treat text, bytes, and integer values as scalar storage for this pass. This
does not re-parse names or signatures and does not grant a capability; the
top-level capability reader and launcher retain those responsibilities.

Synchronize the supplemental instruction-size and stack-effect tables with the
complete scalar operation set currently accepted by the top-level lowerer,
including the missing `u32` arithmetic, comparison, conversion, formatting,
byte-construction, division, and remainder operations. Keep record creation,
field reads, direct calls, branches, returns, local liveness, and scratch-field
allocation under their existing record-specific rules.

Add one hosted fixture that obtains `process.argument_count`, stores it in a
record, applies subtraction, multiplication, and addition, formats the result,
checks its UTF-8 byte, and returns 42 for two arguments. Require the reference
runtime and Stage 0 native executor to agree, then require both Windvale
adapters to emit the exact Stage 0 WVO.

## Consequences

- The reviewed focused selection passes in 2.640 seconds. Its 637-byte WVB has
  SHA-256
  `8e458246eda42b3525ae0aa17b5db268bfdd0b366f244bf8bc59ae7ff6132d46`;
  the exact 4,820-byte WVO contains 4,747 code bytes and has SHA-256
  `c006214deed22a2e8765609d77c95990120042eec0e8245bfeee6c0dd13ef9f1`.
- The separate pin-sensitive package case passes in 9.535 seconds. Its Release
  build reports zero warnings and errors.
- The core closure is 346,868 bytes at SHA-256
  `1bb2cf8768cd91ae6edc4dce360505948dec93144c6af8a5a2a2c6fcec81b62b`.
  The memory adapter is 341,809 bytes at SHA-256
  `c42b38dd7092fe4e1c2b113a356b796e124aed96a3726b9f85ad865f060ee6eb`;
  the hosted tool is 342,837 bytes at SHA-256
  `24d4044ccf6d1f409a2343abf38558cea9f1ff829b3100893d8a941186e975eb`.
  The pinned Windows native source front door reproduces the hosted tool
  exactly in 17.9 seconds.
- Current unpromoted packages are 4,744,192 Windows and 4,743,168 Linux bytes
  at SHA-256
  `26a01262284d0ce8a8f7e647c66d1ed3529818928bf7651af1652696e72fc279`
  and `14a3e79efb9faed4cd0f719c68f618131c2a8219ca5ec6d993969fa289fdcda3`.
- Direct self-lowering no longer reports `Unsupportedˉcode`, including at the
  previously observed function-0 and function-16 boundaries. A reference run
  reaches its explicit 100,000,000-instruction limit in 8.0 seconds without
  output. The packaged native tool, whose fixed hosted budget is 48 billion
  instructions, exits 1 without a diagnostic or partial WVO. That result does
  not distinguish instruction exhaustion from another native resource trap,
  so a bounded measurement of the exact failing status is the next slice; a
  long complete proof remains deferred to the grouped end-of-goal gate.
- No C# product implementation or WebAssembly implementation changed. Stage 0
  remains the independent oracle and recovery path until the grouped dual-host
  and complete retirement gates pass.

Local Development, Standard, Qualification, the full Seed/OS suites, Linux
execution, WebAssembly verification, GitHub verification, artifact promotion,
and ordinary-path cutover remain deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Revisit this pass if capability signatures stop being fixed by canonical kind,
capabilities accept or return record values, the top-level lowerer admits a new
opcode without a synchronized record-storage stack effect, or record storage
moves to shared typed control-flow evidence.
