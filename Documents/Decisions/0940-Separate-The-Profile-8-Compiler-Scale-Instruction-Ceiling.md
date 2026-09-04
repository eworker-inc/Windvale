# Decision 0940: separate the Profile 8 compiler-scale instruction ceiling

## Status

Accepted implementation checkpoint on 2026-09-03. Profile 8 now carries a
finite `2^38` instruction ceiling for explicit compiler-scale analysis and
emission. Profile 7 remains at `2^37`; ordinary hosted applications are
unchanged. Current Windows self-host construction passes, while independent
Linux reconstruction and final paired-host qualification remain pending.

## Context

[Decision 0900](0900-Add-A-Compiler-Scale-Hosted-Geometry-And-Artifact-Readers.md)
separated Profile 8's compiler-analysis arena from Profile 7 but initially kept
their instruction ceilings equal. The growing current analyzer and emitter now
use Profile 8 as one bounded compiler-scale class. Keeping the larger arena
behind the same instruction ceiling left too little separation between ordinary
split emission and complete compiler reconstruction.

The correction must remain profile-scoped. Raising every hosted profile would
broaden unrelated applications without measurement, while removing the meter
would make compiler construction unbounded.

## Decision

1. Set Profile 8's instruction limit to exactly 274,877,906,944 (`2^38`).
2. Retain Profile 7's exact 137,438,953,472 (`2^37`) limit and every Profile
   1-through-6 limit.
3. Encode and independently revalidate the new low/high pair as `0, 64` in the
   hosted metadata constructor, metadata admission core, and runtime header.
4. Select Profile 8 only for the analyzer, emitter, and WVO staging producer.
   Keep compiler-image staging and canonical transport on Profile 6.
5. Keep Profile 8's existing 435,945,472-byte arena, 32 file inputs, name
   stride, target layout, and outer runtime extent unchanged.
6. Rebuild the three geometry-sensitive hosted tool families and require the
   checked inventory before packaging any compiler-scale application.

## Implementation standing

The rebuilt hosted tool inventory is 6,927 bytes at SHA-256
`1a17fa4ee16ba2f21613db6ac36bd7e8643d29a5a1cb26f42e322df19cdc9fd7`.
The self-hosted 1,557,114-byte emitter WVB packages as a 32,075,264-byte
Profile-8 Windows application at SHA-256
`eb939949b9c53d7239e15f4923aa464ab5bca84f342c96358d9c7adaf4a7fda6`.
Its embedded 1,024-byte metadata record contains target `1`, container `11`,
and the exact instruction pair `0, 64`.

That analyzer and emitter reproduce the canonical 1,040,878-byte WVB runner at
SHA-256
`4e50301efe5e2260608eb994f21ece89e83ad102aac28cebb705d35d06e3d86b`.
The resulting Profile-5 Windows runner is byte-identical to the retained
candidate and executes the Return-42 fixture in four WVB instructions.

The focused Language 1.0 memory-budget split owner passes all 172 borrow,
ownership, Vector, `using`, resource, asynchronous-call, and structured-task
cases through the current Profile-8 compiler path in 859,369 milliseconds on
the Windows development host. Because its cold path constructs and packages
multiple compiler-scale products, the owner is classified `very-slow` with a
900-second expected duration and a 3,600-second hard limit; completed package
caches remain reusable across interrupted attempts.

## Consequences

- Compiler-scale work retains an explicit finite meter with additional
  headroom instead of inheriting an unbounded execution path.
- Ordinary Profile-7 emission and all smaller hosted profiles preserve their
  limits and application geometry.
- Profile 8 remains development infrastructure until Linux reconstructs the
  same products and the paired qualification gate passes.

## Reconsideration triggers

Revisit this decision if a non-compiler application selects Profile 8, a
compiler-scale product approaches `2^38`, the meter representation changes, or
the same work can be completed under a lower measured ceiling without changing
semantics or deterministic bytes.
