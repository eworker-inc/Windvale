# Decision 0878: restore exact Slice 7 qualification ownership

## Status

Accepted implementation correction on 2026-08-29. Final paired-host evidence
remains pending.

## Context

The first complete Qualification run after split-compiler convergence proved
the independent WebAssembly and compiler-bootstrap jobs on both hosts, then
exposed five narrower defects in the qualification shards:

- generic nominal deferral also deferred a malformed application of a generic
  type parameter, so the dedicated generic program returned `7` instead of
  `42`;
- the database owner rebuilt the complete compiler build driver before testing
  storage, even though exact compiler reconstruction already belongs to the
  independent convergence jobs;
- Windows runner temporary paths could retain an 8.3 spelling while cache-key
  evidence required the canonical long spelling;
- the native console AOT check treated a localized display label as if it were
  serialized object identity;
- the hosted publisher owner retained exact identities from an older current
  compiler product, and the Linux memory owner omitted the required `.elf`
  suffix from one late self-test package output.

These failures do not justify changing the frozen Seed. Seed is the immutable
bootstrap and recovery oracle; current Language 1.0 behavior belongs in the
current split analyzer and emitter.

## Decision

1. Restrict generic nominal deferral to unresolved nominal families. An
   application such as `T<i32>` where `T` is a type parameter remains an exact
   semantic error rather than being hidden by nominal deferral.
2. Make database qualification consume the retained, digest-pinned build
   driver and lowerer. Compiler convergence owns rebuilding the current
   compiler; a storage owner must not reconstruct it as an unrelated prelude.
3. Canonicalize the temporary root before creating cache-key producer evidence.
   Keep the cache-key requirement for canonical, non-link paths unchanged.
4. Keep exact object bytes, digest, architecture, and verification result as the
   console AOT contract. Treat the human-readable `Verified object:` suffix as
   display text rather than serialized identity.
5. Refresh the exact current WVB publisher WVB, WVO, and linked-fragment
   identities, and report expected and observed byte lengths on future drift.
6. Give the Linux structured-task runtime self-test its explicit `.elf`
   extension so target and container output agree.
7. Establish analyzer and emitter fixed points dynamically, then compare those
   products with the promoted exact identities. Exact mismatch diagnostics must
   report expected and observed bytes and SHA-256 values.

## Consequences

- Seed remains unchanged and retains one narrow, reproducible role.
- Database qualification begins with two cheap exact tool checks and spends its
  remaining time on database behavior. A local retained-tool preparation check
  completes in about 190 milliseconds instead of failing after roughly 45
  seconds in unrelated compiler reconstruction.
- Compiler reconstruction remains cold, exact, and independently visible in
  the paired bootstrap jobs.
- Windows path safety is preserved; the repair removes spelling ambiguity
  before evidence is captured instead of weakening canonical-path validation.
- Local focused evidence passes 12 generic cases, one console AOT case, and all
  16 hosted publisher file-pipeline cases.
- The current analyzer is 1,515,281 bytes at SHA-256
  `a8687f5ec9337d95ea105b5b2d5feea453a11686251802c14110d1f171a3983a`.
  The current emitter is 1,523,514 bytes at SHA-256
  `61ebad24f080a78059bfe3c2812cdb04978873eb6891d063ac2090876dc06403`.
- The 408,545-byte current WVB publisher has SHA-256
  `525779efa3e19a3874919e1f51a5d33e93cdc825e67835b6ab5d9878d08e2275`.
  Its 3,317,775-byte WVO has SHA-256
  `0369818eed1af26a353da91167687d6ea29b564aaa84877120c4f7d27d8f7ec6`,
  and its 3,311,953-byte linked fragment has SHA-256
  `68b89b056159791b07e63d0dfeaf9f731069979f5749fa83e47ce50b68c5e7c4`.

## Reconsideration triggers

Rebuild compiler tooling inside a database owner only if storage semantics
begin to depend on a named compiler-construction behavior that is not already
proved by convergence. Relax canonical-path evidence only if Windvale adopts a
specified identity model that safely distinguishes links, aliases, and ordinary
path spellings on both hosts. Change Seed only through a separate recovery or
security decision with new immutable provenance.
