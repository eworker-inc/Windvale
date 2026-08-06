# Decision 0316: Exact compiler WebAssembly operation families

- Date: 2026-08-06
- Status: Implemented as lowering input with focused Windows-local evidence
- Advances: [Decision 0315](0315-General-Compiler-WebAssembly-Control-Flow.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The general function graph and basic-block directory describe which compiler
instructions execute and how control moves between them. Direct WebAssembly
emission also needs an explicit finite account of the operations present in the
exact browser compiler. Treating every bytecode opcode as one undifferentiated
case would conceal materially different scalar, managed-value, nominal-object,
call, and control requirements.

The exact compiler currently uses 58 distinct reachable opcodes. The composed
portable directory and control-flow tool uses 43. This is a bounded lowering
matrix, not an open-ended need to reproduce the complete language before direct
compiler WebAssembly can execute.

## Decision

- Add focused portable `WebAssembly-Operation-Families.wv` outside
  `WebAssembly-Core.wv`.
- Consume a strictly ascending byte set and emit one deterministic eight-byte
  entry per opcode: the zero-extended opcode followed by its lowering strategy.
- Use five explicit strategy ordinals:

  1. typed direct operations, including scalar constants, typed locals,
     arithmetic, comparisons, conversion, and stack discard;
  2. descriptor operations for immutable text and bytes values, validation,
     formatting, slicing, concatenation, reads, and construction;
  3. nominal arena operations for record and enum construction, projection,
     comparison, and names;
  4. direct calls using the typed call and executable-graph directories; and
  5. dispatcher control using the verified basic-block successors.

- Fail closed on an empty set, duplicate or descending opcode, or an opcode
  without an assigned strategy. Capability calls and wide-value families are
  not silently assigned because the exact portable compiler does not use them.
- Keep classification independent of Wasm encoding. The next modules can own a
  small binary encoder and each strategy's storage and emission rules without
  turning the classification table into another backend monolith.
- Keep all evidence capability-free and local. No server compiler, deployed
  browser change, or fallback path is introduced.

## Focused evidence

The composed 43-operation portable tool produces 344 deterministic entry bytes:
25 typed-direct operations, 12 descriptor operations, two nominal operations,
one call operation, and three control operations. Its complete self-analysis,
control comparison, and malformed-target contract remain intact.

An independent Stage 0 traversal starts at the exact compiler's real `Main`
ordinal 2, reaches 397 of 417 functions, and extracts its sorted 58-opcode set.
A tiny Windvale adapter maps only those 58 input bytes rather than interpreting
the 920 KiB compiler. All 464 output bytes agree entry by entry with an
independent expected strategy: 34 typed-direct, 14 descriptor, six nominal, one
call, and three control operations. Opcode `0xff` produces no entries.

The extended focused contract passed in 16.519 seconds after an 11.64-second
zero-warning Release build. This adds complete exact-compiler family coverage
for about 2.3 seconds over the preceding control-flow test instead of repeating
the billion-operation compiler path. No broader verifier was run because this
single contract is the narrowest affected gate.

## Consequences

Every operation currently reachable in the exact browser compiler now has an
explicit lowering owner before emission begins. Descriptor and nominal storage
cannot be accidentally treated as ordinary Wasm `i32` values, and call/control
lowering cannot be hidden in a scalar opcode switch.

This slice classifies operations but does not yet emit or execute direct
compiler WebAssembly, change the browser package, or improve measured browser
compile time. The next slice builds a focused binary encoder and proves the
typed-direct plus dispatcher-control families on a general basic-block graph,
then adds descriptor and nominal storage needed by the portable compiler tool.

## Reconsider when

- the exact compiler gains an opcode outside the retained matrix;
- capability-bearing compilation becomes part of the browser contract;
- wide values or collection opcodes become reachable in normal compilation;
- profiling justifies subdividing one storage/emission strategy; or
- a later WVB version publishes a different typed operation contract.
