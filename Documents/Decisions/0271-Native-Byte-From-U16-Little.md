# Decision 0271: Native byte from u16 little

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0269](0269-Native-Checked-U32-Arithmetic.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Complete ordinal inspection after the checked-`u32` arithmetic slice identified `bytes.from_u16_little` as the first unsupported operation in the hosted lowerer closure. The canonical operation consumes a `u32`, accepts values through 65,535, returns an owned two-byte little-endian descriptor, and traps as `WVR3016` when narrowing would lose information.

The existing dynamic-byte module already owns one- and four-byte construction, exact arena accounting, descriptor publication, and fail-closed runtime tails. The two-byte constructor can extend that cohesive owner without a runtime service or a new allocation path.

## Decision

Admit opcode `0x79` (`bytes.from_u16_little`) through typed descriptor analysis. Consume one `u32`, produce one owned `bytes` descriptor, and reserve exactly two bytes in the ABI-22 dynamic arena. Compare the input with 65,535 before allocation, branch to the existing runtime failure target with detail 12 on overflow, publish length two and reserved zero, and store the low word with the target's native little-endian `AX` store.

Retain Stage 0's exact 130-byte operation template plus the shared ten-byte instruction charge. Patch only source and target frame offsets and the two external failure branches. Extend the existing focused byte-construction fixture with the maximum accepted value and require complete WVO equality through both Windvale adapters. The exact object comparison includes the narrowing-failure branch; the already-owned Stage 0 range test remains the explicit `WVR3016` oracle.

## Consequences

- The focused construction selection passes in 4.767 seconds. Its 1,107-byte WVB has SHA-256 `aa3736cee76c6aaf7e19e7eb36c715ce1614d8b007db03da7229b253784f305f`; the exact 8,657-byte WVO contains 8,584 code bytes and has SHA-256 `973a428de734f5414c33c1f5f91dd3b3943110ea747eb7565b34f269382802f4`.
- The separate pinned-package case passes in 10.082 seconds. Both Release builds report zero warnings and errors.
- The core closure is 336,925 bytes at SHA-256 `d9cbc92ab06f4e67ced19f85fd35e1a16d0a43a39a14ec9ac0dc59f4133d2a5d`.
- The memory adapter is 331,926 bytes at SHA-256 `5a388b1684b6f8eb6c4d47ee95ad0d75c0bc3015f5b9093b10032ae70db71de3`; the hosted tool is 332,954 bytes at SHA-256 `5b79bbd499c65a02a4d59dac28c4bfb897aeda66f95825f06c57aedb4c047bfd`. Both reproduce exactly through the pinned native source front door in 33.0 seconds.
- Current unpromoted packages are 4,588,032 Windows and 4,587,520 Linux bytes at SHA-256 `a2d1d452179e1305b365724d8b30c829e2a3b948d9651ce81f253df560dfbf8d` and `a1857c475f336820962e2f064c98e540663c6feb5a2e6e8df37eef09881561ba`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. A complete scan of the first 1,917 instructions identifies the next unsupported operation as `u32.from_u8` in function 29, `__WvM12F7(bytes, u32) -> u32`, at function offset `0x005D`.
- No C# product implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit this lowering if WVB changes the narrowing input or failure contract, ABI 22 changes descriptor ownership or arena accounting, or a target cannot preserve the exact little-endian two-byte representation.
