# Decision 0121: Deterministic WVA scalar and memory operands

- Date: 2026-08-02
- Status: Implemented; Windows qualification passed; cross-host qualification pending

## Context

Decision 0119 established typed general registers, local control flow, stack and indirect control, and RIP-relative symbols. Useful scalar routines and migration of existing machine emitters still required immediate arithmetic, multiplication, shifts, and addresses derived from runtime base/index registers. Adding opaque byte escape hatches or adopting an expression parser would weaken WVA's semantic validation and reproducibility boundary.

The next contract must remain small enough for the Windvale-written assembler and independent C# oracle to implement separately. Encoded width choices must not depend on host assemblers, optimization, or displacement relaxation.

## Decision

- Retain WVA version 1 and WVO version 1.0.
- Add `add_i32`, `subtract_i32`, `and_i32`, `or_i32`, `xor_i32`, `compare_i32`, and `test_i32` for typed 32/64-bit registers. The exact signed `imm32` bit pattern is used directly for 32-bit operands and sign-extended by x86-64 for 64-bit operands.
- Add same-width two-operand signed `multiply` through `IMUL`, writing the low-width product to the destination and retaining architectural overflow flags.
- Add immediate `rotate_left`, `rotate_right`, `shift_left`, `shift_right`, and `shift_right_signed`. Counts must be below the operand width so WVA never relies on x86's implicit count mask.
- Add typed 32/64-bit `load_memory` and `store_memory` forms over one explicit `Base64`, `Index64|none`, scale 1/2/4/8, and signed 32-bit displacement.
- Always encode memory through SIB plus `disp32`; do not shorten zero or small displacements. `none` requires scale 1. `rsp` is not an index; `r12` is admitted through REX.X. Any 64-bit register is a base.
- Keep memory safety, alignment, provenance, bounds, aliasing, capability, and ABI proofs outside the assembler. Unsafe or system owners must establish them before execution.
- Keep division, variable-count shifts, 8/16-bit operands, 64-bit immediates, expression syntax, displacement relaxation, and raw executable-byte directives outside this change.

## Consequences

WVA can express ordinary 32/64-bit scalar loops over stack, structure, and array addresses with deterministic bytes. Existing sources remain byte-identical, existing relocations and the linker remain unchanged, and the C# implementation stays independently encoded for differential value.

The explicit five-operand memory syntax is intentionally mechanical. It exposes every encoded address component, avoids punctuation parsing during bootstrap, and can later be mapped from a friendlier frontend without changing WVO. Fixed `disp32` costs bytes but removes base-register exceptions and host-dependent relaxation.

The Windvale implementation still uses repeated immutable source passes and concatenation. This change does not disguise that performance work as a higher source limit; bounded tables and a measured byte builder remain separate follow-up architecture.

## Verification

Acceptance requires exact low/extended REX.W/R/X/B, ModRM, SIB, `imm32`, and `imm8` bytes; all immediate operation groups; both multiply widths; all rotate/shift groups; no-index and indexed memory; all four scales; `r12` versus no-index evidence; signed displacement boundaries; stable malformed immediate/count/width/base/index/scale/displacement/context diagnostics; complete Windvale/C# WVO equality; independent WVO verification; deterministic linking; and the repository assembler and qualification gates.

Cross-host qualification requires the same committed archive and portable artifacts to pass on Windows and Debian. No cross-host result is claimed until both reports exist.

The 2026-08-02 Windows assembler candidate passed the focused assembler gate (8/8), change-aware qualification scope (72/72), full Seed/native CLI qualification (72/72), and OS suite (25/25), including zero-warning builds, golden identities, and composed assembler WVB SHA-256 `6c96bc45cd9ce17016773391e1b39da27953355d2d462c29d90887f0b510a0fc`. After rebasing unrelated Windows-console, verifier-throughput, and line-ending-policy work through `50742b7`, the integrated focused assembler gate passed again (8/8); the assembler implementation and exact WVB identity were unchanged by that rebase.

## Reconsider when

- A migrated emitter requires byte/word memory, division, variable shifts, or implicit-register instructions.
- A frontend needs structured address expressions that lower unambiguously to this exact operand model.
- Measured code size justifies a deterministic displacement-relaxation contract.
- Repeated immutable construction, rather than encoding coverage, becomes the dominant normal-tool limit.
