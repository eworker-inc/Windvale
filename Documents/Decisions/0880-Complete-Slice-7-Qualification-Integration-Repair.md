# Decision 0880: complete Slice 7 qualification integration repair

## Status

Accepted implementation correction on 2026-08-29. Final paired-host evidence
for the corrected source state remains pending.

## Context

Qualification run 33249865070 against Decision 0879 passed both native
bootstrap jobs, both native WebAssembly jobs, and both first native shards. It
also reduced the model-provider owner from roughly 24 to 29 minutes to 13
seconds on Linux and 21 seconds on Windows while retaining all 11 cases.

The six failing native shards exposed four remaining integration defects:

- Windows Node cache producers canonicalized the temporary directory through
  the native operating-system path API, while five caller wrappers used the
  compatibility `realpath` implementation. A runner path containing the 8.3
  spelling `RUNNER~1` therefore differed from the producer's long spelling.
- four ordinary application and library owners still rebuilt the obsolete
  monolithic compiler build driver and lowerer before testing unrelated
  behavior. The current source could no longer be compiled through that stale
  build-driver path.
- the Linux front-door wrapper accepted at most two dependency arguments even
  though its Windows peer and one sequence-read fixture supplied three. It
  omitted the lookalike module and correctly received `Missingˉimport` instead
  of the intended `Invalidˉargument` rejection.
- the package-bundle self-test, writer, and verifier identities predated the
  current compiler product. Their enlarged WVB programs also exceeded the
  monolithic lowerer's bounded output envelope even though the existing
  segmented route packaged the same writer and verifier correctly elsewhere.

These are harness, ownership, and product-identity defects. They do not justify
changing the frozen Seed or the implemented Language 1.0 semantics.

## Decision

1. Canonicalize cache-facing temporary roots and allocated children with
   `realpathSync.native`. Cleanup remains limited to the exact locally allocated
   child under its derived canonical parent.
2. Make `bounded-operation-core`, `file-read-application`,
   `network-address-authority`, and `network-connect-stream-core` consume the
   digest-pinned retained `wvbuild` and `Wvb-To-Wvo` candidates. These owners
   continue to compile twice, compare exact outputs, lower, link, package, and
   execute their own products.
3. Keep compiler reconstruction in the dedicated convergence and reconstruction
   owners. Keep the historical standard-byte-output recovery owner bound to its
   exact restored recovery commit. Do not make unrelated qualification owners
   reconstruct compiler tools.
4. Admit the third dependency in the Linux multi-dependency negative-test
   helper so both hosts test the same complete source set and intended semantic
   rejection.
5. Refresh the package-bundle self-test to 558,336 bytes with SHA-256
   `6523933e61896df401d1c0115c6023fc48d8fbfce9c9f486ac9feb4eb9de46e9`,
   the writer to 510,498 bytes with SHA-256
   `7bc577ac157fc20c301699e5cd08286b736017922871f5206b045d6c46b93a1d`,
   and the verifier to 529,791 bytes with SHA-256
   `218e8939a6e0686c6d2086e2ce977c405abb77728280b332bdf15277f8fa606b`.
6. Package those enlarged tools and the bundle self-test through the existing
   bounded segmented route. Retain ordinary monolithic packaging for the small
   generation verifier, resolver, applications, and other products that remain
   inside its explicit envelope.

## Consequences

- the frozen Seed remains the bootstrap and recovery oracle. Migration or
  replacement waits for an independently qualified 1.0 compiler.
- the four ordinary retained-tool owners pass 67 focused Windows cases without
  rebuilding unrelated compiler tools.
- the package-bundle, installation-command-dispatch, and offline-package-stage
  owners pass 12, 9, and 8 focused Windows cases respectively. Application and
  bundle payload identities remain unchanged.
- the segmented package products use two native fragments. The monolithic
  lowerer's output ceiling remains intact instead of being widened to hide
  compiler-scale growth.
- the Linux three-dependency repair still requires same-state Debian execution;
  Windows cannot substitute for that host-specific wrapper evidence.
- final qualification remains the only evidence that may close Slice 7.

## Reconsideration triggers

Rebuild compiler tooling inside an ordinary behavior owner only if that owner is
explicitly redefined to protect compiler construction. Change the frozen Seed
only through a separate recovery or security decision with new immutable
provenance. Revisit segmented packaging when a measured simpler lowerer can
admit these exact programs without weakening output, memory, or diagnostic
bounds.
