# Decision 0297: Compiler-scale WebAssembly function inventory

- Date: 2026-08-06
- Status: Implemented with focused Windows-local native and reference evidence
- Advances: [Decision 0296](0296-Bounded-Direct-WebAssembly-Nominal-Tables.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The direct WebAssembly backend now completely consumes the exact portable
compiler's static and nominal declaration tables. Its older executable paths
then reject the compiler's function count before parsing the function
directory: those paths support at most sixteen uniform descriptor functions,
while the compiler contains 417 functions and 308 distinct signatures.

Blindly increasing sixteen is unsafe and insufficient. Root-first reachability
uses one `u32` mask, current call emission assumes one descriptor signature
family, and the supported graphs exclude the compiler's recursion and backward
edges. A separate bounded inventory pass is required before those executable
semantics can be expanded.

## Decision

- Completely consume one through 512 function declarations before selecting
  an executable lowering profile.
- Admit at most 64 parameters, 8,191 nonparameter locals, 131,072 code bytes
  per function, 1,048,576 aggregate code bytes, and declared stack depth one
  through 256.
- Require one-through-255-byte function names, structurally valid primitive or
  record/enum parameter and local shapes, a valid return shape including
  `void`, and every nominal shape index to reference the admitted type table.
- Require function code ranges to begin at zero, remain contiguous and
  nonoverlapping in declaration order, and consume the Code payload exactly.
- Return `Unsupportedˉfunction` for an inventory outside this envelope. An
  admitted inventory continues into the existing executable selectors; if no
  selector supports its instructions or graph, return `Unsupportedˉcode`
  without publication.
- Do not decode, type-check, or emit the compiler's code in this slice. Call
  agreement, control flow, reachability, recursion, nominal value storage, and
  instruction lowering remain later verified boundaries.
- Reconstruct and pin both native `wvwasm` containers from source commit
  `9887d6561845e3bb5c0e4992c6eed3e0503a7bb4`. Stage 0 remains the explicit
  recovery constructor; normal use and WebAssembly publication remain .NET-free.

## Exact evidence

The 919,577-byte portable compiler fits the new inventory with:

| Measure | Exact value | Retained bound |
| --- | ---: | ---: |
| Functions | 417 | 512 |
| Distinct signatures | 308 | structurally bounded per function |
| Maximum parameters | 24 | 64 |
| Maximum nonparameter locals | 1,408 | 8,191 |
| Maximum function code | 21,875 bytes | 131,072 bytes |
| Aggregate code | 759,232 bytes | 1,048,576 bytes |
| Maximum declared stack | 34 | 256 |

The native inspector also reports 157,844 instructions and 2,991 direct calls:
488 forward, 2,471 backward, and 32 self-calls. Those measurements explain why
the existing forward-only sixteen-function emitter is not a direct compiler
solution.

The focused retained test admits the exact compiler through the inventory and
observes `Unsupportedˉcode` with no output. It accepts a generated 512-function
boundary, rejects 513 functions as `Unsupportedˉfunction`, and rejects a
hostile name extent, invalid value shape, inconsistent first code offset, and
stack depth 257. It passed in 2.718 seconds of test execution on the measured
Windows host.

The rebuilt Windows native backend preserves the current interpreter identity:
the 110,700-byte interpreter WVB lowers to the exact 828,165-byte WebAssembly
SHA-256
`f3226906f1848cee60d4b25fe0ed4cf3710bd79bb55b12fe16620fc382756c72`.
The same native tool admits the exact compiler inventory, returns
`Unsupportedˉcode`, and creates no output file.

The pinned artifact compiler WVB is 340,435 bytes with SHA-256
`c195e42a0ff7814dbc2af0c0128e9ee28ffe857fe7d43a3611cda3ccb7dcd804`.
Its paired recovery containers are:

- Windows: 5,420,544 bytes, SHA-256
  `23c0675c56542cc619bb58ca359f5e5127227a9b253faf0e79ce73e9fc27d245`;
- Linux: 5,419,008 bytes, SHA-256
  `01f2e2210cbdffa9eac85467072846636c0447aa91e67f7b5eef7e3a084d0811`.

The Linux container is constructed and digest-pinned but has not received a
fresh independent Linux execution report in this slice.

## Consequences

The direct backend now parses and bounds the compiler's complete function
directory before rejecting unsupported execution. This separates serialized
inventory safety from code-generation capability and replaces the misleading
idea that the next step is merely changing a sixteen-function constant.

This slice does not improve browser compilation time by itself. The browser
still interprets the compiler for 1,404,070,227 outer instructions. The next
direct slice must validate and represent the general signature and call graph
without the `u32` reachability mask or forward-only ordering; executable
nominal values and the compiler's broader opcode families remain subsequent
work.

## Reconsider when

- the canonical WVB function, local, code, or stack limits change;
- a direct representation can safely stream function metadata instead of
  rescanning the immutable directory;
- the compiler exceeds the measured 512-function or 1-MiB code envelope;
- nominal collections or variants enter the browser compiler inventory; or
- a verified runtime-compilation representation supersedes direct static Wasm
  emission.
