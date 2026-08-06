# Decision 0262: Native u32 formatting

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0236](0236-Bounded-Native-Text-Services.md), and [Decision 0260](0260-Native-Enum-Parameters-And-Returns.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After enum parameters and returns entered the accepted subset, complete self-lowering reached code analysis and stopped at `u32.format` in `Main` at WVB offset `0x01D1`. ABI 22 already defines a bounded `u32`-formatting runtime service that returns an owned text descriptor. The Windvale lowerer did not yet analyze or emit that operation even though its generated hosted package already carried the service.

The existing text-service fixture covered concatenation, UTF-8 validation/conversion, and quoting but did not prove the unsigned boundary value or assert the complete required-service set.

## Decision

Admit `u32.format` as a one-byte descriptor operation. Require one `u32` operand, replace it with one text value during typed analysis, reserve the exact descriptor lifetime, and emit ABI 22's existing service call at byte offset 88. Load the unsigned input into `R8D`, place the destination text-descriptor address in `R9`, and reuse the shared runtime-service failure tail.

Extend the focused text fixture with `U32ˉformat(4294967295u32)` and require the ten-byte decimal result. Add one selectable differential test that compiles the shared lowerer once for each Windvale adapter and compares the complete WVO with Stage 0. Keep the larger shared-backend test, but do not require it for this isolated slice.

## Consequences

- The dedicated differential case passes in 3.459 seconds and proves exact complete-WVO equality through the Windvale memory and hosted adapters. The separate pinned-package case passes in 9.070 seconds. Both Release builds report zero warnings and errors.
- The updated text-service fixture produces a 773-byte WVB at SHA-256 `93f59f977c0266a08d3763314f6f8ab962ec443a98c3440e9ea734fc45cfa611` and an exact 6,388-byte WVO with 6,160 code bytes at SHA-256 `48669652cfd36e82ac4cab82dacdcdc1a326e92057104bf1e14927eda9c2a830`.
- The core closure is 327,854 bytes at SHA-256 `808b7f72ed31a35d52985643df31d8dafaf255c46f3026dbf2ea168afe1ec7cf`.
- The memory adapter is 322,943 bytes at SHA-256 `93553ce7cc00a0c2ec73bf6f8862b5a5a5d4c203658b37ff99f7c9f0ba50cc8e`; the hosted tool is 323,971 bytes at SHA-256 `0fcb9201e91f38e200d5208b042deeba8f85104c957802ba270dc08ebecf952c`. Both reproduce exactly through the pinned native source front door in 31.403 seconds.
- Current unpromoted packages are 4,469,248 Windows and 4,468,736 Linux bytes at SHA-256 `2b4b8dd1877d2714d5bb86e6b7526048568918fe0551cf7a4ab891f9b46293ee` and `1215b8e6d9d01f7220f72215f0e9d08e28f1617e8ca98f1ea1d317c6010bc49b`.
- Direct self-lowering remains fail-closed as `Unsupportedˉcode` without publishing output. It advances to `bytes.from_u8` in function 1, `__WvM10F0(bytes, u8) -> bytes`, at WVB offset `0x0019`; byte construction is the next active slice.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit this lowering if ABI 22's unsigned-formatting service signature, ownership, output limit, or failure contract changes. Do not replace the explicit service with host-specific formatting behavior.
