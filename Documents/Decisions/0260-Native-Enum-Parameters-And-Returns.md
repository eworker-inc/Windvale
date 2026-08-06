# Decision 0260: Native enum parameters and returns

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0238](0238-Bounded-Native-Enum-Lowering.md), and [Decision 0259](0259-Bounded-Native-Multi-Record-Call-Events.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The lowerer already admitted enum declarations, locals, constants, comparisons, field values, and name lookup, and ABI 22 already transports their signed 32-bit backing values through the scalar register and stack path. Complete self-lowering still failed during all-function signature preflight at function 117, `__WvM1F1(bytes, enum) -> record`, because enum parameters and returns were excluded from helper signatures.

Enum-returning calls also needed their result assigned to the existing enum value-slot group. No new machine representation or ABI path was required.

## Decision

Admit an existing bounded nominal enum identity as a helper parameter or return type. Preserve its exact nominal type byte in the call directory, use ABI 22's existing 32-bit scalar argument and return transport, and allocate a returned enum in the block's enum value-slot group.

Remove the obsolete analyzer-only one-record-parameter rejection superseded by Decision 0259. Retain exact call-signature identity checks, the 64-parameter bound, the existing nominal-table limits, the hard 2,048-cell frame check, and parameterless `Main() -> i32`.

Extend the focused enum fixture with `Keep(Weather) -> Weather`; its `Main` compares and names the returned value before returning 42. Stage 0 and both Windvale adapters must produce the same complete WVO.

## Consequences

- The focused native-lowering selection passes in 15.997 seconds, including enum parameter/return transport, Stage 0 native execution, and exact complete-WVO equality through both Windvale adapters. The rebuilt test project reports zero warnings and errors.
- The core closure is 326,360 bytes at SHA-256 `c8382621573f71770dfc4ab789a7e0938be3787eaddbb84ac333e554e05316ed`.
- The memory adapter is 321,449 bytes at SHA-256 `8a05ee5bad6367d98e886dc305c1628fe939052c484192bac52d2fe94c06bcef`; the hosted tool is 322,477 bytes at SHA-256 `bca63a986e4e14815a3fe83a0cafdb2fac20fa3c51bd419fc6381b017d50927d`. Both reproduce exactly through the pinned native source front door.
- Current unpromoted packages retain their 4,451,328 Windows and 4,452,352 Linux lengths at SHA-256 `96b30a5a0256e753774633063956f8db03e14d2feb5cf9c96212f5427d7061e4` and `1ce42f94519df8ad40e3b813c89ac5f30b7dd2d010af6270029f8c8f75f327d8`.
- Direct self-lowering advances from `Unsupportedˉfunction` to `Unsupportedˉcode`. The first unsupported instruction is `u32.format` in `Main` at WVB offset `0x01D1`; formatting is the next active slice.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Revisit enum transport only if the language changes enum backing width or ABI representation. Continue to reject nominally different enum identities even when their backing values happen to match.
