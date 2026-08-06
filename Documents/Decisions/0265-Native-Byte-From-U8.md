# Decision 0265: Native byte construction from u8

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0237](0237-Bounded-Native-Bytes-Concatenation.md), and [Decision 0262](0262-Native-U32-Formatting.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After unsigned formatting entered the accepted subset, complete self-lowering advanced into function 1, `__WvM10F0(bytes, u8) -> bytes`, and stopped at `bytes.from_u8` at WVB offset `0x0019`. Stage 0 implements the operation as one checked byte of arena allocation, a complete bytes descriptor, and one exact byte store. The Windvale lowerer already owned the same arena and descriptor-lifetime contracts for dynamic byte concatenation but did not admit this constructor.

An initial focused module reproduced the machine sequence through Stage 0 but enlarged the pinned native compiler's source-binding surface and was rejected before WVB publication. The separate file duplicated the byte-module patch and relative-displacement helpers, so retaining it would not improve ownership.

## Decision

Admit `bytes.from_u8` as a one-byte descriptor operation. Require one `u8` operand, replace it with one bytes value during typed analysis, reserve the exact descriptor lifetime, check arena addition and capacity, publish the complete one-byte descriptor, store the source byte, and branch through the existing arena-exhaustion failure path.

Keep the 98-byte construction template beside the existing bounded dynamic-byte concatenation template and reuse that module's patch and relative-displacement functions. The complete per-instruction size is 108 bytes after the existing ten-byte descriptor-release sequence. Do not retain a new module whose only additional ownership is duplicated patch machinery.

Add a selectable fixture that constructs `255u8`, requires a one-byte result, reads the exact byte back, and returns 42. Stage 0 and both Windvale adapters must produce the same complete WVO without requiring a runtime service.

## Consequences

- The dedicated differential case passes in 1.742 seconds and produces a 405-byte WVB at SHA-256 `7fc617a827c75ab0d22bc37a3dfa4875624d61deccf0a712cba26f485ccc285c`. Its exact 2,714-byte WVO contains 2,641 code bytes at SHA-256 `65f3be6bd24bb6eb5b5114f74c74ffa5cec3aeb99090d0e581aaad9759dadf84`.
- The separate pinned-package case passes in 9.008 seconds. Both Release builds report zero warnings and errors.
- The core closure is 330,750 bytes at SHA-256 `c88321782dfe60224863eed5a8afb3bafd31b1c0bde1d12cc2a580d4c2de6d81`.
- The memory adapter is 325,839 bytes at SHA-256 `82d1df1e15aaf0b38facf00bde04e9fdd615fd36a62ee92d7eddc0ba8168ed2e`; the hosted tool is 326,867 bytes at SHA-256 `ef544dc1552d5c41603b9e7a5e8e55e3ea473091193760476193d6db764c65f9`. Both reproduce exactly through the pinned native source front door in 31.490 seconds.
- Current unpromoted packages are 4,522,496 Windows and 4,521,984 Linux bytes at SHA-256 `45c0aea665f139ffe0eb87dfa6f7305e5c86fda100e9d64d1c90ce62234dd6f7` and `77f0c6be7f8f84913575297fe89f5fcc8cf0c39c7f21824aff8e911012b6c0fc`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. It advances to `bytes.from_i32_little` in function 2, `__WvM10F1(bytes, i32) -> bytes`, at WVB offset `0x0019`; 32-bit little-endian byte construction is the next active slice.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit the constructor if ABI 22's arena, descriptor ownership, value limit, or failure-detail contract changes. Reconsider a separate construction module only when a cohesive family can own its machinery without duplicating helpers or exceeding the pinned native compiler's accepted binding surface.
