# Workload 4 implementation responsibilities

| Contract | Owner | Evidence |
| --- | --- | --- |
| `::<...>` explicit generic-call syntax | lexer/parser/name resolver | grammar fixtures, exact arity/kind diagnostics, no comparison ambiguity |
| generic selection/limits | type checker/specialization cache | explicit vs inferred identity equality, 256/32 boundaries, retained-work limits |
| strict UTF-8/rune/source positions | Foundation text + compiler lexer | allocation/source-failure separation, malformed corpus, and byte/rune/line/column oracle |
| reserved vectors/sequences | Foundation collections/runtime | capacity, ownership-return failure, consuming freeze |
| empty ordered map/rank access | Foundation collections/protocol resolver | exact text order, rank/property tests, borrow lifetime |
| mutable/immutable typed arenas | Foundation collections/runtime + borrow checker | freeze identity, stale/wrong-arena/capacity/adversarial generation tests |
| recursive parser depth/work | compiler control/ownership analysis | recursion limit independent of host stack; exhaustion result |
| bounded diagnostic sink | compiler library | stable ordering, related spans, saturation at every 2–16 maximum |
| byte builder integer appends | Foundation bytes | exact little-endian bytes and atomic limit failure |
| phase publication | ownership checker/WIR lowering | consume mutable owner, no post-freeze mutation, handle preservation |
| canonical `WVFE 1` oracle | test/reference implementation | independent encoder/reader, exact 140-byte fixture |
| shared backend | WIR/WVB/native owners | no front-end-specific opcode or second compiler |

## Implementation sequence

1. Add grammar/editor recognition for qualified-name `::<...>` calls without
   changing ordinary comparison parsing.
2. Implement explicit parameter substitution and exact diagnostics before
   protocol/effect/ownership checks.
3. Add reserved vector, empty map/arena, rank observation, immutable-arena freeze,
   and integer-builder operations with reference models.
4. Turn each paper module into parser/type/ownership fixtures.
5. Run the deterministic paper front end under the reference runtime, then the
   same WVB through interpreter/JIT/cached/AOT modes.
6. Record Windows/Linux artifact equality and measured ceilings before revising
   any planning threshold.

## Current compiler relationship

The current `Compiler/Windvale/Source-*` modules and qualified 599,868-byte
self-reproduced compiler are implementation evidence for deterministic compiler
phases. They remain Seed source and packed/current models in places. This paper
bundle does not replace or fork them. After source freeze, migrate the same
compiler architecture in vertical slices and preserve its correctness oracle.

## Verification ownership

The changed-file planner must map edition-1 grammar, generic resolution,
Foundation collections/arenas/text/builders, source containment, compiler
frontend, and deterministic format fixtures to focused owners. Qualification is
dual host because portable semantic/output claims require Windows/Linux equality.
No complete Qualification gate is required for this paper-only documentation
change.
