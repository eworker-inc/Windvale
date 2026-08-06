# Decision 0267: Native little-endian byte construction from i32

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0237](0237-Bounded-Native-Bytes-Concatenation.md), and [Decision 0265](0265-Native-Byte-From-U8.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After one-byte construction entered the accepted subset, complete self-lowering advanced into function 2, `__WvM10F1(bytes, i32) -> bytes`, and stopped at `bytes.from_i32_little` at WVB offset `0x0019`. Stage 0 implements the operation with the same checked arena allocation and complete bytes descriptor as `bytes.from_u8`, but reserves four bytes and performs an exact 32-bit little-endian store.

The Windvale lowerer already owns the matching 98-byte fixed-width construction template and its arena-exhaustion path. Treating the signed source as its exact 32-bit representation requires no arithmetic conversion and no runtime service.

## Decision

Admit `bytes.from_i32_little` as a four-byte descriptor operation. Require one `i32` operand, replace it with one bytes value during typed analysis, reserve the exact descriptor lifetime, check four-byte arena addition and capacity, publish a complete length-four descriptor, store all 32 source bits in little-endian order, and branch through the existing arena-exhaustion failure path.

Keep the operation beside byte concatenation and one-byte construction. Reuse the common one-byte and four-byte patch helpers and relative-displacement calculation, while retaining explicit four-argument emitters that stay within the pinned bootstrap source surface. The complete per-instruction size is 108 bytes after the existing ten-byte descriptor-release sequence.

Extend the selectable byte-construction fixture with `-7`, require a four-byte result, read the exact signed little-endian value back, and retain the earlier maximum-`u8` assertions. Stage 0 and both Windvale adapters must produce the same complete WVO without requiring a runtime service.

## Consequences

- The final focused differential case passes in 2.893 seconds and produces a 655-byte WVB at SHA-256 `a7d05bf1057ba47defac824650c26342921a0179fe095645c825755d489fb877`. Its exact 4,884-byte WVO contains 4,811 code bytes at SHA-256 `cd6ec0d5f132d90c44278c46c2080a9325ad3e29c25303d0e0647f7397871208`.
- The separate pinned-package case passes in 8.451 seconds. Both Release builds report zero warnings and errors.
- The core closure is 333,205 bytes at SHA-256 `f0d93cc849e3f3246e695872cd61b91f9c8d8ee58285b5f2b9b2e92a04a786d2`.
- The memory adapter is 328,294 bytes at SHA-256 `bb69b780a71d2510944edc38ec30aa828b3e4a9cd2dd242215c621114315f406`; the hosted tool is 329,322 bytes at SHA-256 `2b0679507a01a5dd0c290f2c873c8360b52bf6b1699c4afb0b93cad3d25a267f`. Both reproduce exactly through the pinned native source front door in 32.2 seconds.
- Current unpromoted packages are 4,554,240 Windows and 4,554,752 Linux bytes at SHA-256 `b0e1e17198494397b646adcac37ce815622e6e194cb9cc0a3dcd322fc75bcc97` and `6cd60b082ebbe8ef0b15bdcd3d6b58d190e029abc4c39b1ec9b04ee90d4e167a`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. A complete ordinal scan corrects the initial constructor-only inspection: functions 1 and 2 clear, then function 3, `__WvM10F10(bytes, i32, u32) -> bytes`, reaches unsupported `u32.multiply` at WVB offset `0x0233` before the later unsigned-constructor helper.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit the constructor if ABI 22's arena, descriptor ownership, value limit, byte order, or failure-detail contract changes. Reconsider further abstraction when a cohesive fixed-width constructor family can share emission without exceeding the pinned native compiler's accepted source surface or obscuring width-specific invariants.
