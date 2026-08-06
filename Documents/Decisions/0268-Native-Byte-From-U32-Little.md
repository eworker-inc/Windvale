# Decision 0268: Native little-endian byte construction from u32

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0237](0237-Bounded-Native-Bytes-Concatenation.md), and [Decision 0267](0267-Native-Byte-From-I32-Little.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Stage 0 lowers `bytes.from_u32_little` and `bytes.from_i32_little` through the same exact 32-bit store. Their machine behavior differs only at typed analysis: the unsigned operation consumes `u32`, while the signed operation consumes `i32`. The Windvale lowerer already owns the checked four-byte allocation, complete descriptor, store, and failure sequence added for the signed constructor.

An initial constructor-only inspection identified function 6's `bytes.from_u32_little` as the next unsupported operation after the signed slice. A complete ordinal scan shows that function 3 reaches unsupported `u32.multiply` earlier. The unsigned constructor remains a coherent adjacent retirement item, but admitting it does not move that complete-tool frontier.

## Decision

Admit `bytes.from_u32_little` as a four-byte descriptor operation. Require one `u32` operand, replace it with one bytes value during typed analysis, reserve the exact descriptor lifetime, check four-byte arena addition and capacity, publish a complete length-four descriptor, store all 32 source bits in little-endian order, and branch through the existing arena-exhaustion failure path.

Share the 108-byte per-instruction size and machine emitter with `bytes.from_i32_little`; source signedness does not change the exact stored bit pattern. Keep the distinct opcode and typed-stack admission explicit. Extend the selectable byte-construction fixture with `2309737967u32`, require four bytes, read the exact unsigned little-endian value back, and retain the earlier signed and maximum-`u8` assertions.

## Consequences

- The final focused differential case passes in 3.944 seconds and produces an 881-byte WVB at SHA-256 `eec376d645a8169e2b979849724975b2d2287238ef3df9e0cbd63a26c1033154`. Its exact 6,782-byte WVO contains 6,709 code bytes at SHA-256 `faac9832dee443c321336d4642b7291003cf9a9ceac19d906dc3c308d11dbe9d`.
- The separate pinned-package case passes in 9.153 seconds. Both Release builds report zero warnings and errors.
- The core closure is 333,739 bytes at SHA-256 `1db014375c7af755f509fd42cca5badb49981a8890cdf3648b77b4e7399f3066`.
- The memory adapter is 328,828 bytes at SHA-256 `df4d0fb5795ddd9f191786425adf83d7f76189792c77874271f7ba7c4939533c`; the hosted tool is 329,856 bytes at SHA-256 `fb19c845c49305272a5ae5a54b955609d22c20b251a8df78bb0432e55ffc8180`. Both reproduce exactly through the pinned native source front door in 32.5 seconds.
- Current unpromoted packages are 4,558,336 Windows and 4,558,848 Linux bytes at SHA-256 `d0f023d3e4630f5588381effad7819eba7972fbfbec2077c4e90b33e474362eb` and `d8ef029a81c696ed86d24fbf41d112151f011f1e32407315f5a5a3d4a6343bf0`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. The first unsupported instruction is `u32.multiply` in function 3, `__WvM10F10(bytes, i32, u32) -> bytes`, at WVB offset `0x0233`; checked unsigned multiplication is the next active slice.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit the shared 32-bit emitter if ABI 22's arena, descriptor ownership, value limit, byte order, or failure-detail contract changes, or if a future target distinguishes signed and unsigned scalar storage at the machine boundary.
