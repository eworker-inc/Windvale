# Decision 0309: Typed compiler WebAssembly call agreement

- Date: 2026-08-06
- Status: Implemented with focused Windows-local reference evidence
- Advances: [Decision 0306](0306-Compiler-WebAssembly-Function-Directory.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0306 gives every compiler function one immutable constant-time
directory entry, but an in-range call target is not necessarily compatible
with the values at a call site. Direct compiler WebAssembly must reject a
malformed WVB whose call index names a function with a different parameter or
result signature. Re-running the existing bootstrap interpreter is not that
proof and remains the performance problem this work is intended to remove.

The exact compiler has 82 nominal types, 417 functions, 157,844 instructions,
2,991 direct calls, and declared maximum stack depth 34. A verifier for this
boundary must remain bounded, preserve nominal identity, and avoid allocating
an immutable byte sequence for every stack operation.

## Decision

- Add a separate immutable 12-byte type-directory entry for each record, enum,
  or variant declaration. It records kind, item count, and item offset while
  completely validating the bounded declaration payload.
- Validate direct calls with a typed operand-stack simulation independent of
  `WebAssembly-Core.wv`. Every call pops the target's exact parameters in
  reverse order and pushes its exact result.
- Encode each primitive or nominal stack value in one byte. Nine `u32` words
  retain up to 36 values without per-instruction byte allocation; the exact
  compiler remains inside that bound at depth 34.
- Store ordinary opcode effects in one 179-entry static table and handle
  locals, records, enums, calls, and control terminators through focused
  routines. Nominal codes retain the globally unique type index and resolve
  its declaration kind through the type directory.
- Expose a bounded function-range validator. The complete compiler can be
  checked in independently reproducible shards, while the ordinary validation
  entry still covers the complete directory.
- Report the exact function, function-relative bytecode offset, and opcode of
  the first typed failure. An in-range but incompatible target must fail before
  directory publication.
- Add both a hosted file tool and a portable `bytes -> bytes` memory tool. They
  are development tools, not server-side compilation services. No Cloudflare
  function, application server, or remote compiler participates in this path.

## Exact evidence

The exact 417-function compiler was validated in eleven bounded ranges:
`0+60`, `60+60`, `120+60`, `180+60`, `240+60`, `300+10`, `310+5`, `315+5`,
`320+10`, `330+30`, and `360+57`. Every range passed. Their typed-call counts
sum to exactly 2,991, matching the independent code inventory, and their
maximum declared stack is 34.

A portable 66-function memory-tool composition validates its own 10,056
instructions and 270 calls with maximum stack 14. Mutating one valid call from
`Failure(text) -> bytes` to the in-range but incompatible
`Range(u32, u32, u32) -> bool` target is rejected at function 1, offset 97,
opcode 64.

At this checkpoint the focused test checked four representative exact-compiler
ranges, including the depth-34 range and the densest call-heavy functions. It
compared their call counts and maximum stacks with the independent Stage 0
model, checked deterministic directory bytes, retained the 512-function
boundary, and performed an in-range incompatible-target mutation. It passed in
50.446 seconds; the
complete focused wrapper, including its zero-warning Release build, completed
in 62.8 seconds on the measured Windows host. Complete all-function evidence
remains sharded rather than turning one long reference-interpreter run into a
routine development gate.

Decision 0312 later retains two non-redundant ranges: the general graph proof
and the range containing the true depth-34 maximum. The complete typed result
remains this decision's eleven-shard evidence rather than being repeated by
every successor contract.

The portable memory tool currently reaches the established direct backend's
`Unsupportedˉcode` boundary. That is the next executable-representation gap;
it is not replaced with server execution or silently routed through .NET.

## Consequences

The direct compiler path now has Windvale-owned, bounded proof that all 2,991
exact calls agree with their target signatures. Function and nominal metadata
are reusable, and malformed in-range calls fail with precise evidence.

This slice does not yet emit the compiler as WebAssembly, alter the browser
package, reduce the measured 1.4-billion-operation playground compilation, or
remove Stage 0 recovery. The next slice is the first general executable
representation that consumes these directories and supports the operations
used by the portable memory tool.

## Reconsider when

- the canonical WVB shape or nominal declaration encoding changes;
- the compiler exceeds 128 nominal types or stack depth 36;
- collection values require a wider runtime stack code;
- indirect calls require a separate table and signature proof; or
- the complete direct compiler Wasm replaces these development probes.
