# Decision 0294: Static WebAssembly opcode-effect table

- Date: 2026-08-06
- Status: Implemented with focused Windows-local native and Node.js evidence
- Advances: [Decision 0289](0289-Bounded-WebAssembly-Interpreter-Warmup.md) and [Decision 0292](0292-Bounded-Direct-WebAssembly-Static-Descriptors.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The normal browser playground still executes the exact 919,577-byte portable
compiler as a guest of a generated Windvale interpreter. Every guest
instruction classified its static operand-stack effect through a balanced
branch tree over twenty-four packed `u32` words. That representation avoided an
outer WVB data section before Decision 0292, but repeated its selection and
bit-extraction work for all 1,183,292 compiler instructions.

Decision 0292 now admits bounded immutable `bytes` declarations in the direct
WebAssembly backend and lowers them into the reserved page-zero static range.
The interpreter has exactly 256 possible one-byte opcodes, and preflight
already rejects every opcode outside its admitted set before execution.

## Decision

- Replace the balanced packed-word selection with one immutable 256-byte
  `Stackˉeffects` declaration indexed by the verified opcode.
- Preserve the existing four-bit encoding: bits zero and one are the consumed
  cell count, bit two indicates a produced value, and bit three identifies its
  descriptor kind. Unused entries remain zero.
- Retain every module-dependent effect correction, verifier boundary, guest
  meter, call-frame rule, memory region, worker boundary, and execution ABI.
- Build the interpreter WVB through the pinned native source compiler and
  publish its WebAssembly through the pinned native backend. Neither normal
  operation requires .NET; Stage 0 remains only the explicit recovery path.
- Pin source commit `82b31fa5a2cf9e438c9cd7b02c4784e82c646164`
  as the package provenance boundary.

## Exact evidence

The normal native front door emits a 110,700-byte interpreter WVB with SHA-256
`325556e1a6bff318818877265207d4c0c5be1374439d5d021f38703805db076f`.
This is 1,516 bytes smaller than the preceding 112,216-byte WVB. The normal
native WebAssembly publisher emits 828,165 import-free ABI-3 bytes with SHA-256
`f3226906f1848cee60d4b25fe0ed4cf3710bd79bb55b12fe16620fc382756c72`,
10,939 bytes smaller than the preceding 839,104-byte module.

One exact Node.js run on the measured Windows host reports:

| Phase | Guest instructions | Previous outer instructions | Current outer instructions | Reduction |
| --- | ---: | ---: | ---: | ---: |
| Warmup | 20,000 | 17,005,452 | 15,154,202 | 10.89% |
| Compiler | 1,183,292 | 1,513,523,789 | 1,404,070,227 | 7.23% |
| Result execution | 4 | 8,679 | 8,309 | 4.26% |

The complete warmup, compilation, verification, and result execution path
finishes in 59.437 seconds. It publishes the same canonical 183-byte WVB with
SHA-256
`3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`,
then returns status zero and scalar result `42`. Guest instruction counts and
observable results are unchanged.

## Consequences

The hot path performs one bounded static byte read instead of a five-level
selection plus packed extraction. Package size and all three retained outer
instruction counts decrease without widening the interpreter profile or
changing browser authority.

This remains an interpreter optimization, not an interactive compiler tier.
The exact compile still costs 1.404 billion outer instructions, and the current
package has not yet received a fresh Chromium or cross-browser timing report.
Direct compiler WebAssembly or another verified native representation remains
the primary performance frontier.

## Reconsider when

- the WVB opcode space exceeds one byte;
- stack-effect encoding requires more than one byte per opcode;
- the verifier no longer proves every execution opcode before dispatch;
- page-zero static storage moves or becomes mutable; or
- a direct compiler representation removes this interpreter from the normal
  browser compilation path.
