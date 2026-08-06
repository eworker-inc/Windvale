# Decision 0315: General compiler WebAssembly control flow

- Date: 2026-08-06
- Status: Implemented with focused Windows-local reference evidence
- Advances: [Decision 0312](0312-General-Compiler-WebAssembly-Executable-Graph.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The general executable graph removes the old sixteen-function and directed-call
selector limits, but it does not describe control inside each reachable
function. The retained direct WebAssembly profiles reconstruct a few recognized
structured shapes. That approach cannot represent the exact compiler's nested,
backward, cyclic, and early-returning control without adding another growing
pattern matcher to the 10,000-line `WebAssembly-Core.wv` module.

The direct backend instead needs one deterministic lowering input that preserves
verified WVB instruction boundaries and explicit successors. It must remain
bounded, reject malformed targets before emission, and be reusable by a later
compiled-Wasm emitter without changing Windvale semantics.

## Decision

- Add the focused portable `WebAssembly-Control-Flow.wv` module outside the
  existing backend monolith.
- Consume only the validated function directory and general executable graph.
  Unreachable functions retain zero-count function entries and do not acquire
  executable blocks.
- Encode one eight-byte entry per function: its first global block ordinal and
  local block count.
- Encode every reachable basic block as six little-endian `u32` fields: source
  byte offset, byte length, instruction count, terminator kind, first successor,
  and second successor. The 24-byte records use local successor ordinals and
  `0xffffffff` where no successor exists.
- Use terminator kinds zero through three for fallthrough, unconditional jump,
  false branch, and return. A branch records fallthrough first and its explicit
  false target second.
- Discover explicit targets and post-terminator boundaries independently of
  source-level structured regions. A later per-function dispatcher can execute
  forward, backward, nested, and cyclic graphs uniformly.
- Bound the directory to 512 functions, 200,000 reachable instructions, 32,768
  blocks in aggregate, and 4,096 blocks in one function.
- Validate the sorted target set with one linear instruction-boundary pass.
  Only the malformed path rescans to recover the exact originating function,
  instruction offset, and opcode diagnostic.
- Retain a sorted reachable opcode set as the input to the next operation-family
  lowering slice.
- Keep this module and its evidence tool capability-free (`bytes -> bytes`). No
  server compiler, remote fallback, or deployed browser change is introduced.

## Focused evidence

The composed portable memory tool now contains 103 functions. Starting at its
real `Main` ordinal 1, an independent Stage 0 traversal agrees that 88 functions
and 15,085 instructions are reachable. It independently reconstructs 1,764
blocks, 1,043 unconditional jumps, 419 false branches, 302 returns, and 43
distinct reachable opcodes. The Windvale directory therefore contains exactly
42,336 block bytes.

The same focused contract changes a false-branch operand to point inside its
own five-byte instruction. The Windvale module rejects it with the exact
function, instruction offset, and opcode 49 rather than accepting a byte-range
target as an instruction boundary.

The first focused self-analysis test passed in 26.509 seconds. Replacing
per-target function rescans with the linear valid-path boundary pass preserved
both comparisons and the malformed diagnostic while reducing the same test to
14.174 seconds, about 46 percent. The accompanying Release build passed in
8.03 and 7.97 seconds respectively with zero warnings. No broader verifier was
run because this isolated extended contract is the narrowest affected gate.

## Consequences

The direct compiler path now has separate immutable models for function
metadata, types, typed calls, reachability, and intrafunction control. The
remaining lowering work can dispatch over verified blocks instead of inferring
structured source regions or imposing a forward-only graph.

This slice does not yet emit or execute direct compiler WebAssembly, change the
browser package, reduce the measured browser compilation time, or retire the
interpreter. The next slice maps the finite reachable opcode set to scalar,
descriptor, nominal, call, and dispatcher-control lowering families before
emitting the portable tool and then the exact compiler.

## Reconsider when

- the compiler exceeds any retained function, instruction, or block bound;
- indirect calls require typed table successors;
- exception-like control adds edges that are not explicit WVB terminators;
- compiled-Wasm profiling shows that a different immutable block layout is
  materially better; or
- a complete direct compiler Wasm supersedes this development representation.
