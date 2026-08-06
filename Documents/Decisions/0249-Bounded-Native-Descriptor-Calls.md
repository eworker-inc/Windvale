# Decision 0249: Bounded native descriptor calls

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0248](0248-Measured-Native-Lowering-Module-Envelope.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0248, the real compiler-produced hosted lowerer crosses the prototype table bounds and reaches helper signatures containing `text` and `bytes`. The Windvale lowerer already owns descriptor locals, static borrowed values, service-produced values, text operations, and bytes concatenation, but direct helper calls still reject descriptor parameters and returns. Keeping that gap would force otherwise Windvale-owned value behavior back through Stage 0 and prevent the candidate from reaching the next measured call shape.

Descriptors are 16-byte address/length cells whose backing may be static, borrowed from a hosted service, or owned by ABI 22's execution arena. Passing only the address or copying an arena-owned return without its lifetime transition would be incorrect even when a small fixture happened to return the expected scalar.

## Decision

### Pass descriptor cells through the existing register-call envelope

Admit `text` and `bytes` in helper parameter and return signatures while retaining the existing zero-through-four-parameter limit and `Main() -> i32` export. For each descriptor argument, pass the address of the caller's complete 16-byte cell in the corresponding 64-bit ABI register. The callee copies both address and length into its ordinary parameter cell. Scalar and record arguments retain their existing representations.

For a descriptor-returning call, the caller places the address of its result cell in `RAX`. The callee saves that destination plus the current arena checkpoint in one hidden frame cell. A borrowed or externally owned return is copied directly. An arena-owned return is validated against the checkpoint and current arena range, compacted to the checkpoint when necessary, and published into the caller-owned descriptor cell before the callee restores its frame. Overflow, invalid ownership range, and runtime-service failures retain the existing packed ABI 22 status path.

Keep stack-passed fifth and later arguments outside this slice. The signature directory still carries at most four encoded parameter types, so widening calls remains one separate measured contract rather than an accidental ABI extension.

### Keep descriptor-call emission focused

Place descriptor parameter, result, checkpoint, and return emission in `Native-X64-Lowering-Descriptor-Calls.wv`. The module is a cohesive 437-line machine-emission boundary. Core analysis and call dispatch retain policy and delegate descriptor-specific byte sequences instead of further enlarging the already-large lowering core. This follows the repository's reviewable-file guidance without splitting code into numbered fragments or hiding shared invariants.

### Require owned and borrowed descriptor evidence

Add `Wvb-To-Wvo-Descriptor-Calls.wv`. Its helpers pass and return borrowed text, borrowed bytes slices, and arena-owned concatenation results, including a returned value that survives later allocation. Require result 42 through the reference interpreter and Stage 0 native execution, then require the memory adapter and hosted Windvale lowerer to reproduce Stage 0's complete WVO byte for byte.

Retain the reviewed broad package test, updating only the deterministic tool and package identities caused by this source growth. The hosted tool's internal WVO now exceeds the standard 4 MiB object-admission profile, so this already-designated large native artifact uses the explicit `Large_native` profile without changing WVO 1.0 or ordinary object limits.

## Evidence and consequences

- The focused shared-backend case completed descriptor signature, interpretation, native execution, memory-adapter, hosted-tool, and exact-WVO comparisons. Its only later failure was the intentionally stale lowerer identity; the measured pins were then updated without restarting the unchanged behavior work.
- The focused package case completed construction and execution before reaching its intentionally stale package-size assertion. The updated unpromoted packages are 4,366,336 bytes on both hosts: Windows SHA-256 `d812ac375e9a4373c9bcbd73a9ea1187155037951927a456c82379cb664236df` and Linux SHA-256 `561341765954ab808dee30ae3bebd78f9dedf2cbaa29c73a6b0d149a23c721ce`.
- The current core, memory-adapter, and hosted-tool WVB hashes are `b2b7574f65e15ce8a0d80c608f9873010148702c809b6e8f15c3657d05c7b0f5`, `40b94897fceb0d56d75c4a19582149759327b6ec4b8ba1f37e378ea9cadef627`, and `e19bc8f8a18d75ece8929f86a0f8c60aeca3e68866aeb31b98145aef04864584`. The latter two contain 314,568 and 315,596 bytes and reproduce exactly through the pinned native build driver.
- The hosted tool now contains 22 immutable data declarations, 29 nominal types, and 321 functions. Stage 0 lowers it to 4,348,070 code bytes and a 4,360,170-byte WVO.
- The first remaining real-tool signature boundary is a six-parameter helper. Stack-passed arguments, enum parameters and returns, multiple record arguments, broader nominal shapes, and remaining instructions stay explicit later slices.
- No C# implementation changed. Stage 0 remains the independent differential and recovery lane until the grouped dual-host and complete Decision 0057 gates pass.
- Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Revisit this call representation if the shared ABI changes descriptor ownership, if values cease to use complete 16-byte cells, or if a measured stack-passed call requires a new directory version. Do not widen the directory or silently transfer ownership merely to admit a larger signature.
