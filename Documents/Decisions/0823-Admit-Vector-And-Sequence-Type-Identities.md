# Decision 0823: Admit Vector and Sequence type identities

## Status

Accepted on 2026-08-22.

## Context

Language 1.0 defines `Foundationˉcollections.Vector<T>` as a move-owned,
runtime-budgeted collection and `Sequence<T>` as its shared immutable
publication. The earlier lowercase `builder<T, N>` and `sequence<T, N>` types
encode a compile-time maximum in one packed shape and use frame-owned storage.
They cannot represent a runtime-admitted maximum, an allocation lease, retained
byte accounting, or observable ownership transfer without changing the frozen
contracts.

The compiler needs exact generic identities before operation lowering can type
construction, append, freeze, length, and indexed borrows. It must not treat a
lookalike user module as Foundation or let private generic evidence smuggle an
intrinsic owner. `Source-Wir-Core.wv` is also already a broad 12,177-line
orchestrator, so this checkpoint must not add a second collection subsystem to
that file merely for convenience.

## Decision

1. WVGT 1.0 reserves kind `11` for owned `Vector<T>` and kind `12` for
   immutable `Sequence<T>`. Their compiler-private shapes retain the same
   256-instance, depth-32, 1 MiB evidence, and 16 MiB estimated-growth bounds as
   every other admitted generic instance.
2. Admission requires a qualified name resolved through an import of the exact
   edition-1 module `Foundationˉcollections`. `Vector<T>` and `Sequence<T>`
   each require one explicit type argument; the existing optional trailing
   comma remains accepted. An unqualified name, wrong module, bare use, extra
   argument, or value argument is rejected deterministically.
3. The intrinsic layouts contain no forgeable source fields or cases. Layout
   validation rechecks the canonical Foundation module, arity, argument kind,
   dependency order, and element validity. It rejects `never`, capabilities,
   and the obsolete frame-owned builder as elements at this boundary.
4. Fixed-array layout evidence now receives the same canonical Foundation
   module check. This closes an older hostile-catalog path without changing
   valid `Array<T, N>` source.
5. Materialization retains fieldless kind-11 and kind-12 entries so later WIR
   and WVB phases consume one immutable type plan. Successful WVB publication
   remains unavailable until the runtime-backed WVB 1.18 shape and operations
   are implemented; the compiler rejects that incomplete path instead of
   erasing the type or lowering it as the legacy builder.
6. New collection representation and operation helpers will live in a focused,
   acyclic compiler module. `Source-Wir-Core.wv` keeps orchestration and the
   existing legacy encoding until that path is replaced; no cosmetic numbered
   file split is introduced.

## Consequences

- Source analysis can now identify, deduplicate, nest, validate, and
  materialize canonical Vector and Sequence types without claiming that their
  operations execute yet.
- The focused generic nominal type-binding owner grows from 45 to 59 cases. Its
  765,440-byte WVB has SHA-256
  `6c65821f1303782b820e87a191cc94c69dccf7529d76ae5498b24595a5c226b3`;
  its 18,334,720-byte Windows development application has SHA-256
  `a6d38b676e6856a584ccecc50e9e36cd66dc96333493431586b44254838d1d9e`,
  writes no output, and returns `42`.
- The registry remains 108 owners and advances to 5,147 declared cases at
  SHA-256
  `1e8b3cd06dd7038d2ec55607386bb7fabf1a1c90c8dd407ab966b1c96619856f`.
- No source grammar, WVB version, runtime behavior, editor token, or public
  compatibility promise changes in this checkpoint.

## Reconsideration triggers

Reconsider the internal kind numbers or fieldless materialization only if the
runtime-backed collection descriptor cannot carry the frozen Vector/Sequence
ownership and accounting contract, or if the WVB 1.18 design demonstrates that
a different acyclic compiler boundary is required. Do not reconsider merely to
reuse the legacy fixed-capacity builder encoding.
