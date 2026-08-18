# Decision 0769: Implement Language 1.0 runes

- Status: Accepted
- Date: 2026-08-18

## Context

Decision 0767 freezes `rune` as one exact Unicode scalar value rather than a
numeric alias, UTF-8 byte, UTF-16 code unit, or one-character text allocation.
The migration compiler already reserved the keyword and internal shape, but
deliberately rejected stored rune values until the lexer, WIR, bytecode,
verifier, runtime, and native runner could advance together.

Encoding a rune as `u32` would erase its type at function, local, field, payload,
and verification boundaries. Encoding it as text would make equality and storage
allocation-dependent and would admit zero or multiple scalars unless every
consumer repeated source rules. Neither matches the frozen language contract.

## Decision

Implement the first rune vertical checkpoint as follows.

1. A source rune literal contains exactly one direct strict-UTF-8 scalar, one of
   the simple escapes `\\`, `\'`, `\"`, `\n`, `\r`, `\t`, `\0`, `\{`, or
   `\}`, or `\u{H}` with one through six hexadecimal digits. Empty,
   multi-scalar, unterminated, unsupported, surrogate, and above-`10FFFF`
   forms reject compilation.
2. WVLB/WVIR shape `16` means `rune`. WVIR operations `148`, `149`, and `150`
   mean rune constant, equality, and inequality. A constant carries one exact
   scalar; comparisons require two runes and produce `bool`.
3. Named typed rune constants use the same checked evaluator and create no
   runtime storage or data identity.
4. WVB 1.13 adds primitive type tag `17` and opcode `C1`. Selector `0` is
   followed by one little-endian `u32` scalar; selectors `1` and `2` are equality
   and inequality with no immediate payload.
5. A canonical writer emits the lowest required version. Any rune shape or
   operation selects WVB 1.13; modules without rune evidence remain WVB 1.11 or
   1.12 according to their existing vocabulary. A 1.13 reader admits the 1.12
   fixed-integer vocabulary as a proper subset.
6. Verification rejects a rune item under an earlier header, an unknown
   selector, a truncated immediate, a surrogate, a value above `10FFFF`, or an
   operand/shape mismatch. Control-boundary evidence explicitly represents the
   six-byte constant instruction.
7. The shared scalar interpreter and reconstructed native runner execute the
   same `C1` family. The focused rune core is an internal size boundary, not a
   second runtime or alternate semantic path.

## Evidence

`Tests/Fixtures/Language-1.0/Rune-Program.wv` covers ASCII, Japanese, emoji,
simple escapes, braced Unicode escapes, typed constants, parameters, locals,
results, equality, and inequality. The source-built compiler emits the same
1,148-byte WVB 1.13 module twice, SHA-256
`116ff74b5b9c18a76af21785b7aa9017fe4f0c4ff73fa363dfa72898cf9d3dde`,
and the reconstructed runner returns `42`.

The focused Language 1.0 owner also rejects eight invalid source forms and six
malformed bytecode mutations. The reconstructed runner executes the actual
compiler-produced WVB 1.13 module, while a smaller core oracle covers scanner and
stack-transition boundaries. The owner passes 64 declared cases, including 20
rune cases, on the current Windows host.
Cross-host CI remains the owner of a paired-host conformance claim.

The reconstructed runner identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB | 151,488 | `e5948f52146a5c3be9901e2dc8c3b9e4f1ba7b2fdc75624c43f2a3a7b807d264` |
| ABI-22 WVO | 1,371,883 | `f482eface9f6857e6a851a4503b343c6c848aa99fdbe28385aa951bc8e463905` |
| Windows x64 | 1,387,008 | `57b91dae115d14da470b265f3ce1f59a44fe94c06f0de4ae99b1c13418118ae4` |
| Linux x64 | 1,388,544 | `b6914c6b4d5c3bb069b219ce2cb329b179faf032c8b204648628775fbdfbd25e` |

## Non-decision

This checkpoint does not add implicit conversion between `rune`, integers, and
`text`; rune ordering, arithmetic, formatting, normalization, case folding, or
locale behavior; broader Unicode identifiers; localized source tokens; `f32` or
`f64`; or complete Language 1.0. It does not require a WVB 1.13 header for an
unaffected module and does not widen the retained Stage 0 recovery compiler.

## Consequences

Compiler phases, bytecode verification, and runtime execution preserve Unicode
scalar identity without allocation or host-text assumptions. Direct non-ASCII
source remains strict UTF-8, while the braced escape provides an ASCII spelling
for every scalar. Any other WVB consumer must either implement WVB 1.13 exactly
or reject it at its version boundary.

The remaining Language 1.0 migration checkpoints continue in frozen order.
Progress reporting must call this an implemented rune checkpoint, not a complete
Language 1.0 compiler.

## Reconsideration triggers

Reconsider this encoding only if paired Windows/Linux evidence exposes a
semantic disagreement, the compact `C1` envelope prevents bounded validation or
lowering, or a future scalar/text Foundation proves that another representation
materially simplifies all consumers without erasing scalar identity.
