# Decision 0269: Native checked u32 arithmetic

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0268](0268-Native-Byte-From-U32-Little.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Complete ordinal inspection after the fixed-width byte-construction slices identified checked `u32.multiply` as the first unsupported operation in the hosted lowerer closure. The Windvale lowerer already admitted `u32.add`, but not the adjacent `u32.subtract` and `u32.multiply` operations. Stage 0 defines all three as checked arithmetic and provides the independent exact machine oracle.

Unsigned addition and subtraction use the processor carry flag to identify overflow and borrow. Unsigned multiplication uses the full `EDX:EAX` product and rejects any nonzero high word. Treating the three operations as one typed family avoids another partial arithmetic boundary and preserves the established per-instruction metering and overflow status.

## Decision

Admit `u32.add`, `u32.subtract`, and `u32.multiply` as one checked binary family. Typed analysis consumes two `u32` values and produces one `u32` value. Addition emits `add eax, ecx`; subtraction emits `sub eax, ecx`; both branch through the existing unsigned-overflow path when the carry flag is set. Multiplication emits `mul ecx`, tests `edx`, and branches to the same `WVR3007` overflow tail when the high word is nonzero.

Retain Stage 0's exact 39-byte add/subtract and 41-byte multiply instruction envelopes, including metering, operand loads, overflow branch, and result store. Add a focused source fixture that forces all three operations over high unsigned values. Compare the complete WVO from the Windvale memory and hosted adapters with Stage 0, and retain a separate multiplication-overflow input that traps as `WVR3007` before exact-object comparison.

Keep the new focused C# assertion in its own partial test file rather than extending the already-large WVB-to-WVO test source. This follows the repository's reviewability guidance without splitting implementation into numbered fragments.

## Consequences

- The focused two-input selection passes in 2.586 seconds. Its successful fixture is a 471-byte WVB at SHA-256 `7ce9f43e05a16be16c5df444e4dc5ed80f0c206a6816cafa9a91466ebf1fa6fb`; the exact 3,362-byte WVO contains 3,255 code bytes and has SHA-256 `d342e76095e397686defcddaf68a122517fba1d54606cbc738165eda0cb71591`.
- The separate pinned-package case passes in 9.357 seconds. Both Release builds report zero warnings and errors.
- The core closure is 335,078 bytes at SHA-256 `ed4e1c0233fb2f5ad8f6af79b83f3e6fb415d1a0665cb27f4e97ba7a3d982f8d`.
- The memory adapter is 330,079 bytes at SHA-256 `28c5c6e66d02619550d7ea4a29e973463067818789253a0ae4196ad59ea0c4a3`; the hosted tool is 331,107 bytes at SHA-256 `80e7e875d57cfb65bb9ea66875e784512d6bda03fea674919a90d55456026ace`. Both reproduce exactly through the pinned native source front door in 32.7 seconds.
- Current unpromoted packages are 4,569,088 Windows and 4,571,136 Linux bytes at SHA-256 `f065af91d76cb50ea014c17a0c178a48f0b354b0414b4de4900c03f67c81adc2` and `dfbfaa68beafb60183734ae93fee681aed43bc145681562f31c2e7fdd990b452`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. A complete ordinal scan identifies the next unsupported instruction as `bytes.from_u16_little` in function 26, `__WvM12F4(bytes, u32, u32, u32, u8, bytes) -> bytes`, at function offset `0x0126`.
- No C# product implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit this lowering if WVB arithmetic stops being checked, ABI 22 changes its overflow status or metering contract, or a future target cannot reproduce the same exact unsigned overflow proofs.
