# Decision 0296: Bounded direct WebAssembly nominal tables

- Date: 2026-08-06
- Status: Implemented with focused Windows-local native and reference evidence
- Advances: [Decision 0292](0292-Bounded-Direct-WebAssembly-Static-Descriptors.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The normal browser playground still executes the 919,577-byte portable
compiler as a guest of the generated Windvale interpreter. Decision 0292
removed static data as the first direct-lowering blocker. The exact compiler
then reached its 82 nominal record and enum declarations before the direct
backend inspected its 417-function executable graph.

The WVB type section is untrusted serialized input even when a particular
direct-lowering subset does not execute nominal values. Skipping it or accepting
only an empty table would either weaken validation or prevent primitive modules
from carrying harmless unused nominal declarations.

## Decision

- Completely consume zero through 1,024 nominal type declarations before
  function and code selection. Each declaration has a one-through-255-byte
  name and exact payload extent.
- Admit only record kind 1 and enum kind 2. Reject unknown kinds, truncated or
  oversized extents, trailing bytes, and empty member lists.
- Admit at most 64 record fields and 256 enum members. Every member name has a
  one-through-255-byte extent.
- Validate every record-field value shape. Primitive kinds 1 through 8 require
  no nominal index; record and enum kinds 9 and 10 require an in-range type
  index. Enum backing values are consumed exactly.
- Keep executable nominal shapes and operations outside this slice. A type may
  be declared but must remain unused by the retained primitive function,
  local, stack, call, and code subset.
- Preserve the emitted WebAssembly identity for inputs whose executable subset
  is unchanged. Nominal declarations alone do not create target types, memory,
  data, imports, or code.
- Reconstruct and pin both native `wvwasm` containers from source commit
  `5a1a2392a5c001166e573a9cde1f78a09826f2fe`. Stage 0 remains the explicit
  recovery constructor; normal use and WebAssembly publication remain .NET-free.

## Exact evidence

The focused retained test compares a primitive two-function `bytes -> bytes`
module with the same module plus one unused enum and one unused record. Both
lower to byte-identical WebAssembly. It also accepts exactly 64 record fields,
256 enum members, and 1,024 types, then rejects 65 fields, 257 members, 1,025
types, an invalid kind, a hostile unsigned name extent, an empty enum, a
truncated inner payload, and a trailing inner byte. The test passed in 1.430
seconds of execution on the measured Windows host.

The rebuilt Windows native backend preserves both retained external identities:

- the current 110,700-byte interpreter WVB lowers to the exact 828,165-byte
  WebAssembly SHA-256
  `f3226906f1848cee60d4b25fe0ed4cf3710bd79bb55b12fe16620fc382756c72`;
- the Decision 0292 static-descriptor fixture remains 2,339 WebAssembly bytes
  with SHA-256
  `2246c38d2cbc765271926c5f709e8a13cd062d82ee529e5a22dd346206a1772c`.

The exact portable compiler now clears both static-data and nominal-table
validation. It returns `Unsupportedˉcode`, writes no output, and exposes the
next measured boundary: 417 functions with general signatures, calls, control,
and nominal operations exceed the retained sixteen-function primitive graph.
This is frontier evidence, not a claim that the compiler itself is direct
WebAssembly.

The pinned artifact compiler WVB is 334,640 bytes with SHA-256
`d2955805a6275b1c9c7d6d3443e04168a19812cb1d234eedde04181ac17e0019`.
Its paired recovery containers are:

- Windows: 5,371,904 bytes, SHA-256
  `f41a911175209d150c01dd825e6b8ad89b6fb69894dbe55b7a2a477a63d3d9d4`;
- Linux: 5,373,952 bytes, SHA-256
  `9e430c52233cbedd972f02641ebbfd8a8c276eb76a687ba10881d203e58a900a`.

The Linux container is constructed and digest-pinned but has not received a
fresh independent Linux execution report in this slice.

## Consequences

The direct backend now validates and admits the compiler's complete declaration
inventory without weakening WVB parsing or changing execution ABI 3, host
authority, fixed memory, or browser packaging. Small primitive tools may carry
shared unused record and enum declarations without becoming unlowerable.

This slice does not improve browser compilation time by itself. The normal
browser still interprets the compiler for 1,404,070,227 outer instructions.
Direct compiler WebAssembly next requires a bounded representation for the
417-function table, general typed signatures and call graph, control, and the
nominal operations actually used by those functions.

## Reconsider when

- the canonical WVB nominal limits change;
- direct lowering needs recursive or cyclic nominal storage;
- nominal declarations require target-visible runtime metadata;
- type names become observable during direct execution; or
- the compiler's executable graph selects a different verified representation.
