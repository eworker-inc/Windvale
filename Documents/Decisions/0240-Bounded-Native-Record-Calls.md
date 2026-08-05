# Decision 0240: Bounded native record calls

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0239](0239-Bounded-Direct-Record-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0239 transferred deterministic frame-owned storage for direct record construction, local copying, and field reads, but deliberately excluded record parameters, returns, and calls. The next real compiler-produced nominal fixture passes one record to `Keep`, receives a record result, and also declares enums whose first backing values are not zero. A focused one-block fixture can transfer the reusable ABI mechanics independently from that fixture's remaining multi-block record liveness.

ABI 22 passes an ordinary record argument as its field-range handle and returns a record into caller-owned field storage addressed by a hidden `RAX` pointer. The callee must copy complete 16-byte field cells, preserve the dynamic arena checkpoint used by descriptor fields, and restore the shared instruction and call-depth state on every exit.

## Decision

### Admit a bounded record-call shape

Permit record identities in the existing zero-through-four register parameter signatures and as helper return types. A call may transport at most one record argument; until scalar-returning record consumers are transferred, a call with a record argument must also return a record. Record-bearing functions remain one-block and retain Decision 0239's limits. Record parameters are immutable handles in this slice and cannot be assigned through `local.store`.

Allocate every record call result from the existing deterministic scratch interference plan. Before the call, load record handles into ABI 22's 64-bit register positions and place the caller-owned result field range in `RAX`. A record-returning callee saves that pointer in one hidden frame cell, records the arena checkpoint in the cell's second half, and copies every result field into the caller's range before returning a zero status. Non-descriptor record returns restore the saved arena checkpoint directly; descriptor-bearing records retain the existing ownership path.

Move scalar and record call emission into `Native-X64-Lowering-Call-Instructions.wv`. This is a cohesive extraction from the already-large lowering core and keeps the generated Windvale tool below the fixed 2,048-cell native frame limit without raising that safety bound.

### Admit valid nonzero enum baselines

Remove the lowerer's obsolete requirement that an enum's first member equal zero. Canonical WVB already serializes explicit unique signed backing values, and locals still have WVB's ordinary zero-initialized semantics whether or not zero names a member. Apply the same two-line admission correction to the C# Stage 0 backend and fragment verifier so they remain an independent oracle for the existing WVB 1.11 contract; this is a bounded Stage 0 correction, not forward source-language expansion.

### Require focused differential evidence

Add `Wvb-To-Wvo-Record-Calls.wv`, which declares a nonzero-first enum, constructs a two-field `Pair`, passes it through `Keep(Value: Pair) -> Pair`, reads both returned fields, and returns 42. The reference interpreter and Stage 0 native backend must return 42. The Windvale memory adapter and hosted tool must reproduce Stage 0's complete WVO byte for byte, and the generated Windvale tool must remain natively compilable under the existing frame bound.

The affected shared-backend and standalone WVB-to-WVO package tests are the local evidence for this slice. Local Standard, Qualification, GitHub verification, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- One-block record parameters, caller-owned record returns, and bounded nominal record calls now have reusable Windvale-owned x86-64 lowering.
- Valid enum tables no longer need a zero-valued first member in either the accepted Windvale subset or the Stage 0 oracle.
- Compiler-produced `Nominal-Types.wv` still requires multi-block record liveness and a scalar-returning record consumer before it is admitted as a whole.
- The current tool is 256,184 WVB bytes and lowers through Stage 0 to 3,690,036 code bytes and a 3,700,230-byte WVO. Current paired package measurements are 3,708,416 bytes on Windows and 3,706,880 bytes on Linux. These are unpromoted candidate measurements, not optimization promises.
- No normal .NET dependency is removed by this local proof. The only production C# changes are the bounded enum-admission oracle correction permitted by Decision 0213.

## Reconsideration triggers

Extend record liveness across control-flow blocks and admit scalar-returning record consumers only as required by compiler-produced `Nominal-Types.wv`. Do not broaden multiple record arguments, mutable record parameters, stack-passed records, nested records, payload variants, or unrelated capability transport without a measured fixture and an explicit ownership boundary.
