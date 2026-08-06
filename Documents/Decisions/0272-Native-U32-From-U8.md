# Decision 0272: Native u32 from u8

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0271](0271-Native-Byte-From-U16-Little.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Complete ordinal inspection after the two-byte construction slice identified `u32.from_u8` as the first unsupported operation in the hosted lowerer closure. The canonical operation consumes one `u8`, produces the same numeric value as `u32`, cannot lose information, and has no failure or capability path.

Stage 0 represents both values in canonical 32-bit scalar slots. Its exact operation body therefore loads the source slot and stores the same 32 bits in a newly selected `u32` slot. The existing descriptor-instruction state already owns the stack-slot replacement needed by this conversion. Reusing that owner avoids adding conversion locals to the large lowering core, which must retain the 2,048-cell frame ceiling.

## Decision

Admit opcode `0x76` (`u32.from_u8`) through typed analysis. Require one `u8` on the operand stack, replace it with one `u32`, reserve the next `u32` value slot, and charge the shared ten-byte instruction meter plus Stage 0's exact 14-byte machine body.

Emit the ordinary seven-byte 32-bit source-slot load followed by the seven-byte target-slot store. Patch only those two frame offsets. Do not add a runtime service, trap branch, narrowing check, or alternate representation. Keep this small emitter with the existing descriptor-instruction state because it performs the same stack-type and stack-slot transition; do not expand the core or create a one-operation module solely to reduce line count.

Add one focused fixture that converts both `255u8` and `0u8` through a helper and compare the complete WVO with Stage 0 through both Windvale adapters.

## Consequences

- The focused conversion selection passes in 1.946 seconds. Its 420-byte WVB has SHA-256 `981a9a104e69bea9f0ef808f9107e3c0dc06da40b8b595140152d056bcbcc782`; the exact 2,546-byte WVO contains 2,439 code bytes and has SHA-256 `2ae4cfff8e8de357177710238a9fea7d2f33ec8049d1d66ee009c5debf5bc5cc`.
- The separate pinned-package case passes in 8.839 seconds. Both Release builds report zero warnings and errors.
- The core closure is 339,930 bytes at SHA-256 `0f882a38caed33bbe9752302da70d51ebb030c3cc5e167363d167aab9537fe88`.
- The memory adapter is 334,931 bytes at SHA-256 `8bf3e469e26d7d3ec3d933094080d472bbae9acaeb015ca5ab2a82fbe668ed9a`; the hosted tool is 335,959 bytes at SHA-256 `a650aa965028c91e2d71a21e0f43ce480e26276933eac81c5a6ff39d45379dc3`. The pinned native source front door reproduces the hosted tool exactly in 17.4 seconds.
- Current unpromoted packages are 4,647,424 Windows and 4,648,960 Linux bytes at SHA-256 `18fd0fd179a649d245dd9e9163d006d0c2e6d6995c5ea992fe5e1de256fb613d` and `87d5efffa225bf65b2e6f04036777dedc3646a041f31b6ed437c2844f97181f3`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. A complete scan identifies the next unsupported operation at instruction ordinal 2,602: `u32.remainder` in function 36, `__WvM13F4(bytes, bytes, u32, u32, __WvM6R0, bytes) -> bytes`, at function offset `0x0267`.
- No C# product implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit this lowering if WVB changes the widening contract, scalar slots stop carrying canonical zero-extended `u8` values, or ABI 22 changes scalar slot size or frame addressing.
