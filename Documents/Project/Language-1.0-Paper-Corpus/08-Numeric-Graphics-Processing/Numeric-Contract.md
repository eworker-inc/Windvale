# Workload 8 strict numeric contract

## Scalar profile

Every f32 operator is IEEE 754 binary32 with roundTiesToEven, preserved
subnormals, canonical arithmetic NaN, and no contraction or reassociation.
There is no ambient floating environment, exception flag, host rounding mode,
flush-to-zero switch, locale, or fast-math compiler option in portable source.

`Fusedˉmultiplyˉaddˉf32` is a different named operation: it computes an
infinitely precise product and sum and rounds once. Its invalid/NaN result is
canonical `0x7fc00000`. Bit reinterpretation performs no arithmetic and therefore
preserves any original sign/payload.

## Eight-lane fixture

For every index, the operation is `fma(Left, Right, Addend)`:

| Lane | Left bits | Right bits | Addend bits | Fused result | Separate result where relevant |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | `3f800001` | `3f7ffffe` | `bf800000` | `a8800000` | `00000000` |
| 1 | `40000000` | `40400000` | `40800000` | `41200000` | same |
| 2 | `c0000000` | `40400000` | `40800000` | `c0000000` | same |
| 3 | `00000001` | `3f800000` | `00000000` | `00000001` | same |
| 4 | `7f7fffff` | `40000000` | `00000000` | `7f800000` | same |
| 5 | `7f800000` | `00000000` | `3f800000` | `7fc00000` | canonical NaN |
| 6 | `80000000` | `3f800000` | `80000000` | `80000000` | same |
| 7 | `00000000` | `3f800000` | `80000000` | `00000000` | same |

Lane zero uses `(1 + 2^-23) * (1 - 2^-23) - 1`. The exact fused result is
`-2^-46`; the separately rounded product becomes `1`, so ordinary addition
produces positive zero.

## Observation

IEEE `==` remains numeric: both zeros compare equal and every NaN comparison is
unordered. Named observation supplies:

- exact bit reinterpretation;
- nine-way sign/category classification;
- all-bit equality; and
- IEEE `totalOrder`, including `-0 < +0` and exact NaN ordering.

These calls do not make `f32` a map key or implement the ordinary reflexive
`Equality<f32>`/`Ordering<f32>` protocols. A later map requires an explicit
nominal canonical/bitwise wrapper.

## Conversions

The reference checks:

- `16777217u32 → f32 nearest` is `16777216` / `0x4b800000`;
- the exact form returns `Inexact`;
- f64 `0x1.000001p+0` narrows nearest-even to f32 `1.0` and exact rejects;
- f32 `-42.75` truncates toward zero to i32 `-42`;
- NaN and infinity to i32 report their distinct failures; and
- f32 `1.5` widens exactly to f64 bits `0x3ff8000000000000`.

No operation saturates, wraps, truncates bits, or changes type without saying so
in its name.

## Canonical formatting

Fixed bit formatting emits eight lowercase hex digits. Canonical f32 formatting
uses lowercase `nan`, `inf`, `-inf`, `0`, `-0`, or the shortest bounded decimal
that round-trips under the canonical nearest-even parser. The tie rules and
24-byte maximum are in Foundation. Formatting never observes a host locale or
floating formatter.

## Parallel equivalence

The reference loop is sequential and ordered. Each lane reads immutable inputs
and replaces one disjoint output index, so a future bounded library may schedule
lanes concurrently only if the final ordered eight f32 bits and report are
identical. This evidence does not authorize parallel reductions, reassociation,
different accumulators, or nondeterministic publication.
