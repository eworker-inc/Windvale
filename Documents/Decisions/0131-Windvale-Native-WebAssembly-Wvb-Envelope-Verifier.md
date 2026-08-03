# Decision 0131: Windvale-native WebAssembly WVB envelope verifier

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0128](0128-Bounded-WebAssembly-Runtime-Values.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 9 established bounded byte values and the fixed execution-ABI-3 memory contract, but it admitted only straight-line code. The first real WVB verifier needs a loop over section envelopes, conditional rejection paths, unsigned cursor arithmetic, and comparisons. Qualifying those requirements through a real Windvale program is stronger evidence than adding disconnected opcode fixtures.

The compiler emits nested source conditionals as canonical WVB basic blocks with absolute jumps. Directly recovering every nested source region would couple the backend to one parser layout and retain the older nonnested-region restriction. A bounded basic-block lowering can preserve the same verified WVB control graph without making Wasm the definition of Windvale control semantics.

## Decision

- Add experimental profile 10 for profile 9's single portable, capability-free `Main(Input: bytes) -> bytes` shape composed with checked `u32.add`, checked `u32.subtract`, all six unsigned `u32` comparisons, `u8` equality and inequality, `jump`, `branch.false`, and early `return`.
- Retain the 16,384-byte code, 4,096-instruction, 255-nonparameter-local, four-value operand-stack, fixed-memory, value-size, aggregate-arena, and exact instruction-budget bounds.
- Decode every admitted instruction before publication. Require each control target to be an instruction boundary and either function entry or immediately after a validated terminator. Require an empty operand stack at every jump boundary and a `bytes` value at every return.
- Lower control-capable functions through one private Wasm program-counter local and one Wasm dispatch loop. Each validated WVB basic block becomes one guarded Wasm case. Jumps select the next validated offset; `branch.false` selects fallthrough or its validated target. Synthetic dispatch operations do not consume WVB budget, while every executed WVB instruction retains one pre-execution charge.
- Preserve checked Windvale unsigned arithmetic. Addition overflow and subtraction underflow return `WVR3007`; no wrapping result is published.
- Add `Wvb-Envelope-Verify-Main.wv` as the first real consumer. It returns byte `1` only for a completely consumed WVB 1.6 envelope with the exact magic, version, seven canonical section envelopes in kind order, zero flags and reserved fields, and in-range payload extents. It returns byte `0` for a structurally invalid envelope.
- Keep this verifier's result distinct from selector admission. It validates the outer WVB envelope only; it does not yet validate section payload schemas, types, functions, exports, instructions, capabilities, or semantic control flow.

## Consequences

The Windvale-authored backend can now lower and execute nested compiler-produced control over primitive and byte values without .NET in the resulting module. The retained artifact is a real Windvale-native WVB verifier rather than an identity or operation demonstration, and its input is untrusted runtime data under the same fixed browser memory and budget contract.

Program-counter dispatch is intentionally an internal Wasm lowering. Canonical WVB remains the portable control-flow contract. The target verifier restricts blocks to the compiler's terminator-aligned layout, which keeps target validation bounded and makes malformed or crossing byte offsets fail before any Wasm bytes are published.

This does not yet replace .NET in the editable playground. Full WVB verification, source compilation, remaining compiler-required text/record/enum operations, and worker-contained Stage 0 execution remain open. The static page also remains on its smaller profile-8 artifact until a later browser-facing milestone intentionally advances it.

## Local evidence

The verifier compiles to 2,837-byte WVB SHA-256 `1362b2707a4ff442a1458e3f821e01108bb948858db21e022bfee05869c2fb86`. The selector emits a deterministic 14,902-byte import-free Wasm module with SHA-256 `f493777450b720ef786b60502528819969ad9e0322aa55a9c0259f6de20850fc` under unchanged execution ABI 3.

The reference runtime and Node.js return byte `1` for the verifier's own canonical WVB in exactly 2,206 instructions. Budget 2,205 returns `WVR3011` with empty output. Bad magic, an 11-byte truncated header, a hostile first-section length, and one trailing byte return byte `0` after 112, 92, 460, and 2,201 instructions respectively.

A 447-byte checked-`u32` fixture with WVB SHA-256 `d6ba02dfe12efdcb7c2f8ed6664551a776e79e3ff2c30134dc7e3642ee7ce743` emits 1,893-byte Wasm SHA-256 `f645c0ff095eb06c825fea056659545cc258d857da55fc9dfd1a928812373f61`. Node.js preserves `42` after checked add/subtract in 57 instructions, returns `WVR3007` for addition overflow after 37 and subtraction underflow after 47, and returns `WVR3008` for a truncated read after seven.

The C# conformance oracle independently reconstructs the admitted source types, opcodes, terminator-aligned targets, local layout, single Wasm dispatch loop, conditional routes, branch depths, program-counter accesses, complete target opcode stream, exact static meter count, fixed memory, ordered exports, output publication, and bulk-memory use. A branch target corrupted into an instruction operand is rejected without output. The focused Seed WebAssembly test and the 23-artifact-plus-input Node.js gate pass locally on Windows.

## Rejected alternatives

Matching only the exact verifier byte sequence was rejected because it would qualify a stencil rather than a reusable runtime boundary.

Recovering nested source regions into nested Wasm regions was deferred because WVB basic blocks are the verified input contract and a dispatcher handles nested compiler output without reconstructing source syntax.

Rescanning the complete instruction stream for every candidate block boundary was rejected after it exceeded the established 100,000,000-step selector gate. Requiring targets to follow decoded terminators provides a stronger canonical profile and keeps the real verifier within that gate.

## Reconsider when

- A broader compiler-produced control graph needs fallthrough targets that do not immediately follow terminators.
- Dispatch overhead materially limits a measured verifier or compiler workload and direct structured reconstruction can retain equal validation evidence.
- Full section-payload verification requires calls, text, records, enums, or a larger bounded temporary-memory policy.
