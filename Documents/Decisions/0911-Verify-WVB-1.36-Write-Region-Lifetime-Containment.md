# Decision 0911: verify WVB 1.36 write-region lifetime containment

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision admits the candidate WVB 1.36 write-region instruction to the
compiler-aligned verifier. It does not admit WVB 1.36 to a scalar runtime,
native lowerer, launcher, package execution path, browser, WebAssembly host,
or Windvale OS consumer. It does not expose a pointer, complete Slice 8, or
claim paired-host qualification.

## Context

[Decision 0910](0910-Represent-Mutable-Write-Region-Borrowing-In-Candidate-Wvb-1.36.md)
selected the deterministic `DE` representation but kept the complete verifier
and all execution consumers closed. Its independent byte reader proved the
header, sections, width, and immediate categories, but it did not prove the
canonical Foundation layouts, typed stack, affine region Result, exclusive
scratch use, or control-flow containment.

Opening a provider before those proofs would let execution reconstruct foreign
memory authority from incompletely checked bytes. The next coherent boundary
is therefore complete compiler-aligned verification while the region payload
and every execution path remain closed.

## Decision

1. Extend the compiler-aligned structural, semantic, typed-stack,
   control-reachability, and ownership verifier through WVB minor `36` while
   preserving every inherited WVB 1.11-through-1.35 rule.
2. Require WVB 1.36 metadata to select System profile `3`. Profile selection
   remains a classification and does not grant authority.
3. Recognize opcode `DE` (`222`) only under minor `36`, with exact width `13`,
   three ordered `u64` stack operands, and three valid `u32` immediates.
4. Require the scratch immediate to name an available exact
   `Foreignˉscratch<Abi>` owner or a compiler-authenticated shape-`28` mutable
   borrowed parameter. Mark that scratch unavailable at the instruction and
   preserve unavailability through branch merges, loops, and function exit.
5. Require the Result immediate to name the exact materialized
   `Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>` layout. Verify
   the synthesized generic identities, distinct opaque scratch and region
   nominals, Result cases and fields, and all seven pointer-failure cases,
   field names, field counts, and scalar widths.
6. Represent the produced Result with verifier-internal affine kind `37`.
   Permit only affine local move/store/take, discard, and case observation.
   Reject ordinary Result construction, payload or field extraction, direct
   or indirect call transfer, function parameters or results, and return.
7. Require the ABI immediate to name a kind-`2` or kind-`7` enum. Treat the
   instruction as the explicit serialized scratch/region/ABI binding. Reject
   inconsistent reuse of either nominal and reject disagreement with a
   scratch-construction binding when that construction is present.
8. Bound one module to at most 4,096 `DE` instructions and 256 distinct
   scratch/region/ABI bindings, occupying at most 3,072 relation bytes. Require
   at least one `DE` in every WVB 1.36 module.
9. Keep the region payload inaccessible and all execution consumers closed.
   Range, address-width, alignment, exact failure construction, release,
   pointer derivation, and provider state remain later execution semantics.
10. Extend the focused write-region oracle so the source-built verifier accepts
    canonical candidates; rejects byte-level, canonical-layout, and payload-
    extraction forgeries; and rejects analyzer-representable region escape and
    scratch reuse. Re-run the complete inherited unsafe-scratch oracle against
    the same verifier.

## Implementation standing

The source-built compiler-aligned verifier is a 473,392-byte WVB with SHA-256
`e7392f22668c53551141cbd8865c362e67038819f984e07373742c6b25810d5c`.
Its packaged Windows executable is 3,814,912 bytes with SHA-256
`937ca25af77b17eed141493a4bfb88583a6f01615eb538d710276d1648333c0b`.

The focused write-region oracle covers 13 source cases, seven malformed WVIR
mutations, five malformed WVB mutations, and four semantic WVB forgeries. The
compiler verifier accepts the two canonical instruction-shape candidates and
rejects two additional analyzer-accepted candidates that attempt region escape
and scratch reuse. The inherited WVB 1.33/1.35 unsafe-scratch oracle also
passes all 20 compiler-verifier cases.

The existing execution front door continues to reject every WVB 1.36
candidate. That fail-closed behavior is intentional.

## Consequences

- The next scalar/provider checkpoint can consume one completely verified
  WVB shape rather than repeating source-level inference.
- Verifier acceptance proves containment but creates no pointer and grants no
  runtime authority.
- Scratch unavailability through function exit is deliberately more
  conservative than the eventual lexical region lifetime. A later release
  rule may shorten the borrow only with equally explicit control-flow proof.
- WVB-generated type names erase source generic arguments. The verifier checks
  exact layouts and consistent explicit instruction bindings; it does not
  claim to recover generic arguments from synthesized names.
- Existing WVB 1.11-through-1.35 semantics remain unchanged.
- The next Slice 8 checkpoint is bounded scalar/provider region construction,
  exact validation failures, private release, and teardown. Native lowering,
  pointer derivation, and authenticated Foreign calls follow separately.

## Reconsideration triggers

Reconsider the conservative function-exit lifetime only when an explicit
region-consumption operation or structured lexical proof can release the
scratch earlier without alias ambiguity. Reconsider the relation directory if
real bounded workloads require more than 256 distinct bindings. Do not open
execution by weakening canonical layout, affine ownership, or fail-closed
version checks.
