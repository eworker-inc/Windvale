# Decision 0770: Implement Language 1.0 floating point

- Status: Accepted
- Date: 2026-08-18

## Context

Decision 0767 freezes `f32` and `f64` as distinct portable IEEE 754 binary32
and binary64 values with explicit literals, same-type operations, deterministic
rounding, canonical NaN results, and no implicit numeric conversion. The
migration compiler reserved their source and binding identities but rejected
stored values until lexer, WIR, bytecode, verifier, and runtime contracts could
advance together.

Using decimal source text would require a second exact decimal-to-binary
conversion contract during the bootstrap. Reusing integer WVB tags would erase
the type and permit invalid operations. Relying on host floating-point
instructions in the portable scalar oracle would inherit host rounding modes and
NaN payload behavior. None satisfies reproducible cross-host semantics.

## Decision

Implement the first floating-point vertical checkpoint as follows.

1. Source admits hexadecimal literals of the form
   `0x<whole>[.<fraction>]p[+|-]<exponent>f32` or `f64`. At least one whole
   hexadecimal digit, a mandatory binary exponent, and the explicit lowercase
   suffix are required. A present dot requires at least one fractional digit.
   Decimal floating literals and an omitted suffix are rejected.
2. Literal conversion evaluates the exact hexadecimal value and rounds once to
   binary32 or binary64 using round-to-nearest, ties-to-even. Overflow produces
   infinity and underflow produces the correctly rounded subnormal or zero.
3. WVLB/WVIR shapes `14` and `15` mean `f32` and `f64`. WVIR operations `151`
   through `162` own constants, add, subtract, multiply, divide, negate,
   equality, inequality, and ordered comparison.
4. WVB 1.14 adds primitive type tags `18` and `19` and opcode `C2`. The opcode
   carries the type tag and selector; constants additionally carry exact raw
   little-endian bits. Any floating shape or operation selects WVB 1.14, while
   unaffected modules retain their prior lowest version and byte identity.
5. Runtime arithmetic preserves finite values, subnormals, infinities, and
   signed zero. Division by zero and invalid arithmetic produce IEEE results
   rather than Windvale traps. Every produced NaN is canonical quiet NaN
   `7FC00000` or `7FF8000000000000`. Positive and negative zero compare equal;
   NaN comparisons are false except inequality.
6. The shared scalar interpreter implements this contract with integer
   operations over raw bits. The portable oracle therefore does not depend on a
   host floating-point environment, NaN payload convention, or locale.
7. Verification rejects the new types or opcode under an earlier header,
   unknown selectors, wrong type tags, truncated or over-wide constants, and
   every operand or result-shape mismatch before execution.
8. There is no implicit conversion, decimal floating literal, remainder,
   bitwise operation, shift, truthiness, formatting, or Foundation conversion in
   this checkpoint.

## Evidence

`Tests/Fixtures/Language-1.0/Floating-Program.wv` covers both formats, arithmetic,
negation, all comparisons, signed zero, infinity, and NaN. The Windvale-written
compiler emits the same 2,809-byte WVB 1.14 module twice with SHA-256
`c783fd85deca397814da71a87ec543ec75f800d4ecd10549c53091d48fd54327`, and the current
source-built native runner returns `42`.

The focused Language 1.0 owner also rejects four invalid source programs and
eight independently mutated bytecode forms. A separate floating runtime
self-test exercises raw-bit arithmetic and scanner boundaries through the same
integer implementation. The native `u64` lowering oracle covers the complete
bitwise and shift support needed by that runtime, including a full-width
function result, and passes on the current Windows host. Cross-host CI remains
the owner of a paired-host conformance claim; no broad storage or full
qualification gate is claimed by this checkpoint.

The retained profile-5 runner was reconstructed from the same source closure:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB 1.14 | 183,537 | `1926cf33e359c56c8b457cbd96c685ffee052feb9f1330053c43d77e18f38d3e` |
| ABI-22 WVO | 1,808,213 | `dfcfb2360d496a5ab873539b4d6dbcdfe3824e8593dfe3e007cc71cd9bc55480` |
| Windows x64 | 1,822,208 | `7a2f245b405d01c1f0f9c7f2b9e9cbe0d88370232e8cf1843616207aa155e7bd` |
| Linux x64 | 1,822,720 | `7dac00ed67f7622af2fcd4c9ededd17afced3ad54ea309d749320249188b15b4` |

The segmented WVO staging producer was refreshed with the same lowering source
so large compiler builds do not fall back to the pre-`u64` staging subset. Its
WVB is 542,219 bytes at
`786e271556b141c476ef9ce32beb65acded5ce8b76a61f1ba295994b97272dc7`;
the paired Windows and Linux applications are 7,855,104 bytes at
`5303a5580831dad96c2f46a50aa9f0ce4c4c3dc70d4612dac8a03dc1c78b1aeb`
and 7,856,128 bytes at
`5ea8569ce076087aa3b11afc19ce492d0a062f96e872a52dd6a93b889860f3cb`.

## Non-decision

This checkpoint does not add implicit or explicit numeric conversions,
decimal-source floating literals, compile-time floating arithmetic and
comparison folding, mathematical-library functions, formatting, vector/SIMD or
reduced-precision AI types, native floating-point lowering, or complete
Language 1.0. It does not promote unrelated retained consumers or require WVB
1.14 for an unaffected module. Consumers that have not implemented WVB 1.14
keep their explicit narrower version boundary.

## Consequences

The portable compiler, verifier, and scalar runtime now preserve exact `f32`
and `f64` semantics through one versioned path. Deterministic software execution
is slower than a future native lowering but provides a simple correctness oracle
for JIT, AOT, GPU, and accelerator implementations. The focused verifier owns
the new boundary without repeatedly invoking unrelated storage qualification.

The remaining Language 1.0 migration checkpoints continue in frozen order.
Progress reporting must call this an implemented floating-point checkpoint, not
a complete Language 1.0 compiler.

## Reconsideration triggers

Reconsider this encoding or portable arithmetic only if paired Windows/Linux
evidence exposes a semantic disagreement, a proven IEEE edge case cannot be
represented, the compact `C2` envelope prevents bounded verification, or a
future hardware-backed implementation demonstrates equivalent raw-bit results
under an explicit and independently checked floating environment.
