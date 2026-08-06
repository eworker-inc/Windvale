# Decision 0323: Complete compiler u32 WebAssembly operations

- Date: 2026-08-06
- Status: Implemented for the direct scalar/control proof with local engine evidence
- Advances: [Decision 0320](0320-Metered-Checked-Scalar-Dispatcher-WebAssembly.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The metered dispatcher admitted checked addition, subtraction, multiplication,
and negation but still rejected eight typed-direct operations used by the exact
portable compiler: unsigned divide, remainder, bitwise AND, OR, XOR, complement,
and left and right shift. The exact reachable compiler contains 24 divides, 28
remainders, three instances of each bitwise operation, and one of each shift: 66
dynamic instruction sites in total.

These operations are small compared with descriptor and nominal lowering, but
leaving them inside the dispatcher would grow that control owner toward another
monolith. Division and shifts also need explicit Windvale failures before Wasm's
own wrapping or trapping behavior can occur.

## Decision

- Add focused portable `WebAssembly-Scalar-Operations.wv` as the owner of the
  remaining exact-compiler 32-bit scalar instructions. The general dispatcher
  supplies scratch-local and private-status-global indices explicitly; the new
  module does not own control-flow layout or a hidden execution ABI.
- Lower `u32.divide` and `u32.remainder` to Wasm unsigned division/remainder only
  after storing both operands and rejecting a zero divisor with status `3032`.
- Lower `u32.bitwise_and`, `u32.bitwise_or`, and `u32.bitwise_xor` directly.
  Lower complement as XOR with an exact signed-LEB `-1` constant.
- Lower `u32.shift_left` and `u32.shift_right` only after storing both operands
  and rejecting every count greater than or equal to 32 with status `3033`.
  Wasm's implicit modulo-width shift behavior must not define Windvale syntax.
- Use the same early-return convention as metering and checked arithmetic.
  Failure publishes no result and retains the instruction count including the
  failed operation. A one-instruction-short budget still wins as `3011` before
  the operation-specific validation.
- Keep descriptor, nominal, wide, and collection operations fail-closed. This
  slice completes the exact compiler's admitted 32-bit scalar family, not the
  complete compiler module.

## Focused evidence

The expanded three-function success fixture has WVB SHA-256
`6b8ee8e5e3707203891840157547fa1cf88368447b493015b9d0f9e48bbb69d2`.
It now executes unsigned divide, remainder, all four bitwise operations, both
shifts, the previously admitted checked arithmetic, calls, backward control,
comparisons, and locals. The capability-free dispatcher tool has WVB SHA-256
`1089e906ae75395d0fd1d114196d33da4e0d7059ba7ac558d27a03ff74e8338c`.
It emits deterministic 5,881-byte Wasm with SHA-256
`3f90a6641648ae55a3b4ddf3a50ae2d2ad7d52ae434bf15c1718076f04232e79`.

Node 24.18 validates and executes the module to result 42 in exactly 200 WVB
instructions. Budget 199 returns `3011` with no result. A divide-by-zero fixture
emits 1,203 bytes, returns `3032`, and charges 14 instructions; its WVB/Wasm
SHA-256 values are respectively
`9bfee4a2bfd7be72406b6e83539417bc3eafe513f4340862e9f3a0b7713972b5`
and `5639230029a5b02e063ffd196d95f9b1b1cce5b3d131045b2cb039734debd6ef`.
An invalid-shift fixture emits 1,205 bytes, returns `3033`, and also charges 14;
its WVB/Wasm values are
`eafd95c3ce2cd75c1a2fdd3e6f58c6d4b36b90730aadc25b80079d88309106b5`
and `1c339d9798c73c30dd052ee88f915df7edfbd926427f955af2f0aece8983853b`.
The reusable probe reports:

```text
dispatcher-engine status=Valid module-bytes=5881 result=42 instructions=200 limited-status=3011 overflow-status=3007 overflow-instructions=14 divide-zero-status=3032 divide-zero-instructions=14 shift-failure-status=3033 shift-failure-instructions=14
```

The one focused Seed contract passes in 1.390 test seconds after its incremental
Release compile. No broad Seed or WebAssembly-engine verifier was run.

## Consequences

No 32-bit scalar operation reachable from the exact portable compiler remains
assigned to the interpreter. Descriptor and nominal values are now the next
semantic boundary, rather than a hidden arithmetic gap. Keeping the added logic
in a focused module also holds the dispatcher at its control/layout role.

This does not yet make the compiler executable as direct Wasm. Its `bytes` and
`text` data, record and enum values, static data, recursive call-depth budget,
and `bytes -> bytes` host transport remain unresolved. No browser-performance
claim follows from the scalar fixture.

## Reconsider when

- signed divide/remainder becomes reachable in the selected compiler;
- a shared scalar-operation module should absorb the earlier checked arithmetic;
- execution ABI 3 replaces the private failure convention; or
- the exact compiler artifact supersedes these focused fixtures.
