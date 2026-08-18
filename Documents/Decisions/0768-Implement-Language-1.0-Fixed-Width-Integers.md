# Decision 0768: Implement Language 1.0 fixed-width integers

- Status: Accepted
- Date: 2026-08-18

## Context

Decision 0767 froze `i8`, `i16`, and `u16` as ordinary Language 1.0 primitive
values with exact same-type operands, checked arithmetic, explicit conversion,
unsigned-only Core bitwise operations, and no pointer-sized integer. The
migration compiler already reserved lexer, source-type, and private binding
identities for these types, but deliberately rejected their stored values until
the WIR, bytecode, verifier, and runtime contracts could advance together.

Reusing `i32` or `u32` bytecode tags would erase the named width at function,
local, field, payload, and verification boundaries. Always moving every module
to another WVB minor version would instead change byte identities for programs
that do not use the feature. Neither behavior matches the frozen language or
Windvale's reproducibility contract.

## Decision

Implement the first fixed-width-integer vertical checkpoint as follows.

1. The source lexer admits exact decimal suffixes `i8`, `i16`, and `u16` with
   positive token bounds 127, 32,767, and 65,535. Unary minus remains a separate
   checked expression.
2. WVLB/WVIR shapes `11`, `12`, and `13` mean `i8`, `i16`, and `u16`.
   WVIR operations `129` through `147` own their constant, checked arithmetic,
   comparison, signed-negation, `u16` bitwise, and `u16` shift families.
3. Named typed constants use the same checked evaluator and lower to the same
   fixed-integer WIR constant operation as an equivalent literal. They create no
   runtime storage or data identity.
4. WVB 1.12 adds primitive type tags `14`, `15`, and `16` and opcode `C0`.
   `C0` carries the type tag and one operation selector; a constant also carries
   one raw little-endian `u16`. The complete selector and type rules are frozen
   in `Specifications/Seed-Bytecode.md`.
5. A canonical writer emits the lowest required version. Any admitted fixed
   shape or operation selects WVB 1.12; a module without one remains exact WVB
   1.11. The compiler-aligned verifier and scalar runner accept both versions
   and reject every 1.12-only item under a 1.11 header; other consumers keep
   explicit narrower boundaries until implemented.
6. Arithmetic overflow traps with `WVR3007`, division by zero with `WVR3032`,
   and an invalid `u16` shift count with `WVR3033`. Signed minimum divided or
   remaindered by minus one is overflow. Signed bitwise/shift selectors and
   unsigned negation are malformed bytecode.
7. Keep one compiler and one shared scalar-interpreter path. The runner extracts
   the bounded fixed-integer instruction scan and execution logic into focused
   source modules; this is an internal size boundary, not another runtime.

## Evidence

The focused Language 1.0 front door owns valid source, deterministic A/B output,
six malformed-bytecode mutations, compile-time literal/type/operator rejection,
runtime success, and exact overflow/division/shift traps. The executable fixture
covers all three types, is 5,335 bytes with SHA-256
`b3cca3ae81dfadc78d45b1f83b5bdd7a3deaff1d42624e12c2a610bdb3f222a9`,
and returns `42`. The unchanged minimum edition-1 program
remains the exact 221-byte WVB 1.11 module with SHA-256
`25a18cf13d791db1e85fd6b237f89f21d4a0c7b9460b0a72db2da5e5deb205ae`.

The reconstructed Windows and Linux runner products remain digest-bound and are
verified by the independent runner reconstruction owner. Cross-host CI remains
the owner of a two-host conformance claim.

## Non-decision

This checkpoint does not implement `f32`, `f64`, `rune`, general conversions,
fixed-width formatting/Foundation APIs, localized numeric spelling, or complete
Language 1.0. It does not make WVB 1.12 mandatory for an unaffected module and
does not make the retained Stage 0 recovery compiler the oracle for the new
vocabulary.

## Consequences

Compiler phases, bytecode verification, and runtime execution now retain the
declared width rather than relying on a wider storage convention. Unaffected
projects preserve their existing bytes and caches. Any other WVB consumer must
either implement WVB 1.12 explicitly or reject it at its version boundary; it
may not accept the header while ignoring the new shapes or opcode.

The remaining Language 1.0 migration slices continue in their frozen order.
Progress reporting must call this an implemented fixed-integer checkpoint, not
a complete Language 1.0 compiler.

## Reconsideration triggers

Reconsider this encoding only if independent Windows/Linux evidence exposes a
semantic disagreement, the opcode envelope prevents bounded validation or
lowering, or a later complete scalar implementation proves that another compact
family materially simplifies the verifier without weakening exact width and
lowest-version guarantees.
