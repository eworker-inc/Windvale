# Decision 0237: Bounded native bytes concatenation

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0236](0236-Bounded-Native-Text-Services.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0236 transferred the four ABI 22 text-service operations, `bytes.concat` was the sole unsupported instruction in compiler-produced `Data-And-Text.wv`. Unlike the service-backed text operations, byte concatenation owns allocation and descriptor-generation policy directly in generated code. The Stage 0 instruction body is 976 machine-code bytes because it covers overflow, bounded arena allocation, owned-buffer validation, tail reuse and growth, copying, generation publication, and two explicit runtime failures.

Adding those bytes to the already-large orchestration core would obscure both the instruction's ownership boundary and the core's control-flow responsibilities. Special-casing `Data-And-Text.wv` as a whole program would also violate the selected algorithmic-lowering contract.

## Decision

### Admit one generic bounded instruction

Admit opcode `bytes.concat` for two exact bytes descriptors. Stack analysis consumes both inputs, allocates one bytes result cell, and accounts for the instruction's 986 emitted bytes: the normal ten-byte WVB instruction charge followed by the 976-byte concatenation body.

The body preserves ABI 22's existing dynamic-byte contract. It rejects `u32` length overflow and combined values above 4 MiB, uses the bounded text/byte arena already owned by the execution context, validates any claimed left-side owner header and generation, reuses or grows the current tail when safe, otherwise allocates an exact small result or a capacity-bearing owned result, copies the left and right ranges in order, and publishes the complete result descriptor only after the selected allocation path is valid. Byte-value overflow reports service-failure detail 11; arena exhaustion reports detail 2; both branch to the existing runtime-service status tail.

Keep the instruction-local machine template in the focused `Native-X64-Lowering-Bytes-Concatenation.wv` module. Its fixed internal branches remain part of the reviewed template. Lowering patches forty frame-slot displacement fields for the actual left, right, and result cells plus the two function-relative runtime-failure targets. This is a reusable opcode implementation for every admitted module, not a whole-program stencil or fixture-specific result.

### Require real compiler-produced evidence

Use the existing compiler-produced `Data-And-Text.wv` fixture as the focused success vector. Its exact 1,652-byte WVB at SHA-256 `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc` combines three functions, immutable i32/text/bytes data, static descriptors, all four transferred text operations, and `bytes.concat`. Interpretation and native execution both return 13. The Windvale memory adapter and hosted tool reproduce Stage 0's exact 15,123-byte WVO at SHA-256 `cc987e81e8f8dfb8d19b13e91e5f259c86b3f5eb8a6ae79db66ed6ef3dca4263`.

Mutating its bytes concatenation to text concatenation must fail typed analysis before publication. The existing independent Stage 0 fragment verifier must also accept the exact lowered concatenation structure; result-only comparison is insufficient.

The reviewed shared-backend and WVB-to-WVO selections remain the only local verifiers for this coherent slice. Standard, Qualification, Linux execution, GitHub verification, and artifact promotion stay deferred to the grouped end-of-goal gate.

## Consequences

- The complete current `Data-And-Text.wv` source/WVB surface now has Windvale-owned accepted-subset WVO lowering.
- Dynamic byte ownership, arena bounds, generation publication, failure detail, and machine bytes remain exactly aligned with Stage 0 for the admitted instruction.
- The focused source module prevents the 976-byte template and its patch inventory from enlarging the orchestration core.
- The current hosted tool is 167,172 WVB bytes and lowers through Stage 0 to 2,257,076 code bytes and a 2,263,326-byte WVO.
- Current paired package measurements are 2,275,328 bytes on Windows and 2,277,376 bytes on Linux; they remain unpromoted candidate evidence.
- No normal .NET dependency is removed by this local proof.

## Reconsideration triggers

Measure the nominal record/enum operations required by compiler-produced `Nominal-Types.wv` next. Do not broaden byte builders, descriptor calls/returns, or general allocation merely because they are adjacent to concatenation; each needs a concrete remaining fixture and its own ownership evidence.
