# Decision 0312: General compiler WebAssembly executable graph

- Date: 2026-08-06
- Status: Implemented with focused Windows-local reference evidence
- Advances: [Decision 0309](0309-Typed-Compiler-WebAssembly-Call-Agreement.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The direct WebAssembly backend completely inventories and types the exact
portable compiler, but its retained executable selectors model only eight or
sixteen functions. They additionally place `Main` first or last and use a
single `u32` reachability mask or a direction restriction. The exact compiler
has 417 functions and 2,991 calls, including backward, forward, self, and
cyclic relationships. Increasing one selector constant would therefore not
produce a valid general execution model.

The first client-shaped portable directory tool already has 78 functions and
303 calls after composing the reusable validation modules. It reaches the same
structural selector boundary before operation lowering. A replacement must be
an immutable, bounded input to later lowering rather than another special-case
emitter inside `WebAssembly-Core.wv`.

## Decision

- Retain an independent eight-byte name entry beside each existing 32-byte
  function-directory entry. Name lookup resolves an arbitrary `Main` ordinal
  without changing or republishing the raw constant-time metadata directory.
- Add a focused portable `WebAssembly-Executable-Graph.wv` module. It consumes
  the validated function directory and code inventory, not the old small
  selector's internal tables.
- Encode one eight-byte graph entry per function: the byte offset of its first
  target and its direct-call count. Store every target as one `u32` in original
  function/instruction order.
- Derive a deterministic breadth-first reachable order from any supplied root.
  Membership is bounded by 512 functions and the inherited 4,096-call limit;
  forward, backward, self, mutually recursive, and cyclic graphs require no
  separate representation or direction rule.
- Preserve unreachable functions in the directory while listing only the
  root-reachable functions in execution order. Later lowering can choose
  whether to emit the complete directory or the reachable subset without
  rediscovering the graph.
- Make graph construction optional in the hosted evidence tool and mandatory
  in the capability-free `bytes -> bytes` memory tool. These remain local
  development/compiler components; no server-side compiler or remote fallback
  is introduced.
- Keep the graph independent of the established backend until a complete
  operation-lowering path can replace an executable selector.

## Exact evidence

The exact compiler resolves `Main` at function 2. Its graph has 417
eight-byte entries, 2,991 four-byte targets, and 397 root-reachable functions:
3,336 entry bytes and 11,964 target bytes. A separate Stage 0 traversal agrees
on the root and reachable count.

The composed portable memory tool resolves `Main` at function 1. Its current
78-function, 11,619-instruction, 303-call graph contains 74 reachable
functions, 624 entry bytes, and 1,212 target bytes. The existing direct
artifact compiler still rejects that valid graph as `Unsupportedˉcode`, which
confirms that operation/control lowering remains the next boundary rather than
silently selecting an old small profile.

The retained focused contract independently compares the exact graph with the
Stage 0 model and keeps the typed range containing the compiler's true maximum
stack depth of 34. It passed in 49.898 seconds; the separate focused Release
build passed in 9.29 seconds with zero warnings. Complete typed-call agreement
remains the eleven-shard evidence
recorded by Decision 0309 instead of being repeated in every graph check.

## Consequences

The direct compiler path no longer needs a sixteen-function mask, a fixed
`Main` position, or a call-direction restriction to describe what can execute.
Function signatures, nominal types, typed calls, and reachability now exist as
separate reusable Windvale-owned models outside the monolithic backend.

This slice does not emit compiler WebAssembly, change the deployed browser
package, reduce the measured 1.4-billion-operation interpreted compilation,
or remove Stage 0 recovery. The next slice maps the validated general graph's
operation and control families into a complete lowering representation, then
emits and executes the portable memory tool before attempting the full
compiler.

## Reconsider when

- the compiler exceeds 512 functions or 4,096 direct calls;
- indirect calls require a separately typed table;
- graph construction becomes material to compiled-Wasm startup and warrants a
  bounded bitmap instead of the simple immutable queue;
- dead-function elimination becomes a published reproducibility contract; or
- a complete direct compiler Wasm supersedes this development representation.
