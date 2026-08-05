# Decision 0234: Bounded native scalar comparisons

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0233](0233-Bounded-Native-U8-U32-Scalars.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0233 admitted the scalar shapes needed by `Function-Only.wv`, measurement of the remaining compiler-produced fixtures found one common narrow blocker. `Data-And-Text.wv`, `Nominal-Types.wv`, and `Hosted-Capabilities.wv` all use `u32.not_equal`; the first two also use `u8.not_equal`. The lowerer already owned typed `u32.less` and `u8.equal` verification, value slots, and exact comparison emission.

Admitting descriptors, nominal values, capabilities, or another arithmetic family merely to reach those comparisons would combine unrelated ownership boundaries. Leaving the comparison selector partial would duplicate a complete stable WVB scalar family without reducing any other risk.

## Decision

### Complete the bounded u32 and u8 comparison families

Admit WVB 1.11 `u32.equal`, `u32.not_equal`, `u32.less`, `u32.less_equal`, `u32.greater`, `u32.greater_equal`, `u8.equal`, and `u8.not_equal`. Reuse the existing typed binary-comparison analysis and 39-byte comparison template.

Map equality to x86-64 `sete`/`setne` and unsigned ordering to `setb`, `setbe`, `seta`, and `setae`. ABI 22 stores both types zero-extended in dword cells, so the existing dword load and compare contract remains exact. Do not admit `u32` subtraction, multiplication, division, remainder, bitwise, shift, conversion, descriptor, nominal, or capability operations in this slice.

### Extend the reviewed scalar differential vector

Expand the small typed-return vector introduced by Decision 0233 so its `u32` result exercises all six comparison operators and its `u8` result exercises equality and inequality across true and false routes. Stage 0 interpretation and native execution return 42. The hosted Windvale lowerer must reproduce Stage 0's exact 5,263-byte `.text` and 5,404-byte WVO.

Keep derived module and package identity assertions after behavior. Run only the reviewed shared-backend and WVB-to-WVO package selections, transcribe their final identities after every behavioral assertion completes, and do not replay unchanged behavior. Standard, Qualification, the full Seed/OS suites, Linux execution, GitHub verification, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- Every bounded `u32` and `u8` comparison now has one shared typed verifier and x86-64 selector rather than a partial special case.
- One scalar blocker is removed from all three remaining compiler-produced fixtures, but none is claimed accepted: descriptors/data, nominal values, or capabilities still block each complete module.
- Retained WVO vectors, including `Function-Only.wv`, remain byte-identical.
- No new source module, assembly source, ABI, serialized format, artifact promotion, or ordinary launcher is introduced.

## Reconsideration triggers

Choose the next opcode or value family only after remeasuring complete real fixtures. Descriptor work must define value lifetime and ownership before selection; nominal work must preserve ABI 22's frame-owned layout; capability work must preserve explicit service authority and failure propagation.
