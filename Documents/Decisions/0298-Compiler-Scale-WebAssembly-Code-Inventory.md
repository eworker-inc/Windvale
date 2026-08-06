# Decision 0298: Compiler-scale WebAssembly code inventory

- Date: 2026-08-06
- Status: Implemented with focused Windows-local native and reference evidence
- Advances: [Decision 0297](0297-Compiler-Scale-WebAssembly-Function-Inventory.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0297 completely consumes the exact portable compiler's 417-function
directory but intentionally stops before code. The retained direct emitters
decode only their narrow executable subsets and reject the compiler at their
sixteen-function or signature gates. A compiler-scale direct route needs a
separate code inventory that can bound the complete instruction stream and
represent general direct-call targets without the root-first `u32` mask or
forward-only ordering.

The exact compiler contains 759,232 code bytes, 157,844 instructions, and 2,991
direct calls. Its calls include 488 forward, 2,471 backward, and 32 self edges.
Those edges are valid input to eventual WebAssembly direct calls, but accepting
their serialized operands is distinct from proving typed call agreement,
control flow, or recursion resource behavior.

## Decision

- Decode every function body independently through the complete current WVB
  1.11 opcode-width table, covering opcodes 1 through 191 only where the format
  assigns them an exact one-, two-, five-, or nine-byte encoding.
- Admit at most 200,000 aggregate instructions and 4,096 direct calls inside
  Decision 0297's one-MiB aggregate code envelope.
- Require Boolean constants to contain only zero or one; local loads and stores
  to reference a declared parameter or local; jump and false-branch operands to
  remain inside their declaring function; and every direct-call target to be
  below the admitted function count.
- Keep function metadata and code status separate. A malformed function
  directory returns `Unsupportedˉfunction`; an invalid code inventory returns
  `Unsupportedˉcode`. Neither path publishes output.
- Do not yet prove that a control target is an instruction boundary or follows
  a terminator. Do not type the operand stack, check call arity/signature
  agreement, establish reachability, select a recursion budget, lower nominal
  values, or emit general instructions in this slice.
- Reconstruct and pin both native `wvwasm` containers from source commit
  `cfe2781f016037b35ffb5e97263b7aa185858a8d`. Stage 0 remains the explicit
  recovery constructor; normal use and WebAssembly publication remain .NET-free.

## Exact evidence

The focused retained test independently reconstructs the compiler's exact
417-function, 24-parameter, 1,408-local, 21,875-function-code-byte, stack-34,
157,844-instruction, and 2,991-call maxima and totals. The direct backend admits
that complete function and code inventory before returning
`Unsupportedˉcode` with no output at the later executable selector.

A generated 512-function module with one direct call also clears both inventory
passes. A 513-function module, hostile function-name extent, invalid signature
shape, inconsistent code range, and stack depth 257 fail the function boundary.
An unknown opcode and a call target equal to the function count fail the code
boundary. The focused test passed in 8.673 seconds of execution on the measured
Windows host.

The rebuilt Windows native backend scans the exact compiler and reaches the
retained executable-code boundary in 151 ms. It writes no output. The same tool
preserves the current interpreter identity: the 110,700-byte interpreter WVB
lowers to the exact 828,165-byte WebAssembly SHA-256
`f3226906f1848cee60d4b25fe0ed4cf3710bd79bb55b12fe16620fc382756c72`.

The pinned artifact compiler WVB is 347,729 bytes with SHA-256
`8a71377dc22f77747f3c04f1ff3a323ef9e5a48f90a4732bfbb5ebf2cc8a84b1`.
Its paired recovery containers are:

- Windows: 5,476,352 bytes, SHA-256
  `b5359908928770140ef54c1d757b64c50cf00df06e2768a640d01287ccc34041`;
- Linux: 5,476,352 bytes, SHA-256
  `ea3cd335094a6d2ee237c346a6134cdda6415f33a40f87f0507e52938030f87f`.

The Linux container is constructed and digest-pinned but has not received a
fresh independent Linux execution report in this slice.

## Consequences

The direct backend can now bound and walk the complete compiler code stream and
represent every direct call target without imposing call direction or a
32-function mask. The next rejection is no longer an opaque count or serialized
code boundary; it is the executable semantic work itself.

This slice does not improve browser compilation time by itself. The browser
still interprets the compiler for 1,404,070,227 outer instructions. The next
direct slice should establish typed signature and call agreement over a
representation that supports all 308 signature families, then advance control
and nominal-value lowering without publishing partial or stub WebAssembly.

## Reconsider when

- the WVB opcode space, instruction widths, or serialized operands change;
- the compiler exceeds 200,000 instructions or 4,096 direct calls;
- indirect calls or function values require a typed table contract;
- control-boundary validation moves into a reusable immutable directory; or
- a verified runtime-compilation representation supersedes direct static Wasm
  emission.
