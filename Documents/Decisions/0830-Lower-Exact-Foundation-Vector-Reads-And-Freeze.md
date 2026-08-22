# Decision 0830: Lower exact Foundation Vector reads and freeze

## Status

Accepted on 2026-08-22.

## Context

Decisions 0826 and 0827 made scalar Vector backings executable and added the
WVB 1.20 `local.take` ownership transfer. Decision 0828 then connected exact
Foundation Sequence reads to source. Source could carry `Vector<T>` and
`Sequence<T>` identities, but it still could not observe a Vector length or
freeze an owned Vector through the public Language 1.0 surface.

The next source checkpoint must preserve the backend's unique-owner proof. It
must not treat retaining `local.load` as a move, permit a parameter to become
unique without an argument-transfer contract, or infer a Sequence result from
an unrelated catalog entry.

## Decision

1. Bind `Vectorˉlength` and `Vectorˉfreeze` only when a qualified alias resolves
   to the exact `Foundationˉcollections` module. They use WVIR operation
   identities 169 and 170. Lookalike modules cannot acquire either intrinsic.
2. Admit only a direct owned non-parameter local whose exact WVGT identity is
   kind 11 `Vector<T>` with a resource-free Copy scalar element.
   `Vectorˉlength` requires explicit immutable `borrow` and preserves the local;
   freeze requires a value expression and consumes it. Mutable borrow, borrowed
   freeze, indirect expressions, parameters, and unsupported elements reject.
3. Operation 169 returns `u64`. Operation 170 returns the enclosing function's
   declared exact kind-12 `Sequence<T>` shape. Independent WVIR validation proves
   that its element equals the consumed Vector element, so a missing,
   out-of-catalog, or mismatched return type cannot publish.
4. Generic-type signature scanning runs when the source set contains the exact
   Foundation collections module as well as when it contains user-declared
   generic nominals. Parameter and return-only collection instances therefore
   enter WVGT before bodies are lowered.
5. Emit operation 169 as `local.take`, `vector.length`, scalar result store, and
   unique Vector store back to the same local. Emit operation 170 as
   `local.take`, `vector.freeze`, and Sequence result store. Both select WVB 1.20.
6. In WVB 1.20, a function declared to return Vector must return unique Vector
   evidence, and `call` to that declaration produces unique evidence. Compiler-
   generated stores and returns of Vector temporaries use `local.take`.
7. Keep this source ownership checkpoint straight-line: one function may have
   one outstanding consumed Vector local, and a function containing freeze has
   one basic block. Multiple simultaneous moves, branch/loop ownership,
   parameter transfer, general expected-type propagation, fallible construction,
   recoverable append, and non-scalar elements remain later work.

## Consequences

- The source fixture publishes a deterministic 546-byte WVB 1.20 module at
  SHA-256
  `fc51afb9c7b8a17dd9fd044e971f22944e0d96ec872de910de3f0114d066e20f`.
  The current verifier accepts it and the source-built WVB 1.20 runner returns
  42 from the independent `Main`.
- Six malformed-bytecode cases reject an old minor, shared values substituted
  for the required unique return/read, and invalid Vector/Sequence type
  immediates. Five source cases reject use after freeze, invalid borrow modes,
  parameter access, and an unsupported element. These 13 cases reuse the
  existing Vector/Sequence front-door phase and its already-built tools.
- The function-return catalog omission exposed by freeze is fixed at its general
  admission boundary. A return-only collection instance no longer needs an
  unrelated parameter or local to enter WVGT.
- `Source-Wir-Core.wv` remains within the maintained split compiler's fixed
  evidence bounds, but its collection signatures, call lowering, ownership
  checks, and validation now form a cohesive future extraction boundary.
- The 108-owner verification registry advances to 5,190 declared cases at
  SHA-256
  `e4e10295a6ebe799ebd86bbe649569bbda9bb7c8ee5371a370d3b5de81f84d66`.
- General source ownership, public fallible Vector construction, recoverable
  append, borrowed non-Copy elements, native lowering, WebAssembly execution,
  and cross-host qualification remain later checkpoints.

## Reconsideration triggers

Replace the straight-line source restriction when the general ownership phase
can compute branch and loop fixed points or transfer multiple owners without
weakening WVB definite availability. Replace enclosing-return contextualization
when general expected-type propagation can resolve freeze in local, argument,
and nested expression positions with the same exact WVGT proof.
