# Decision 0948: reuse development-owner results across unrelated source trees

- Date: 2026-09-04
- Status: Implemented candidate with focused cache and changed-file evidence
- Extends: [Decision 0557](0557-Separate-Development-Verification-From-Qualification.md)
- Preserves: fresh qualification, current planner ownership, exact host and
  repository identity, and fail-closed cache fallback

## Context

The first persistent development-result cache keyed every passing owner receipt
to the complete non-ignored Git tree. It could resume an interrupted run or
survive a commit that changed no bytes, but any source-tree difference created a
new state and made every earlier receipt invisible. An unrelated document edit
therefore forced the same compiler or database owner to run again even when the
changed-file planner proved that owner was unaffected.

That policy was safe but defeated the purpose of incremental verification. It
also encouraged developers either to rerun disproportionate gates or to skip
verification manually.

## Decision

1. Exact-state result reuse remains the first and cheapest lookup.
2. After an exact miss, development verification may inspect at most fifteen
   retained result states from the same canonical repository and exact host,
   boot, environment, and host-tool identity. Qualification, sharded runs,
   direct owners, and non-development execution never use this path.
3. Every cache state records its Git tree, repository identity, and host
   identity in a bounded ordinary state record. A missing, malformed, linked,
   oversized, cross-repository, or cross-host record is not a candidate. State
   preparation accepts a tree only when source sentinels measured immediately
   before and after it are equal.
4. For a candidate receipt with the same owner action, enumerate every path
   changed between its Git tree and the current measured Git tree. The current
   native changed-file planner must accept the complete list with no coverage
   gaps and must not select that owner. Otherwise the receipt is a miss.
5. The owner action binds the exact registered owner row in addition to suite,
   command, arguments, and scope. A changed expected summary or owner profile
   therefore cannot reuse an earlier action receipt accidentally.
6. Changes to the owner registry, duration profiles, changed-file planners,
   development dispatcher, owner coordinator, result-cache implementation, or
   process-stream boundary disable compatible-state reuse. These are global
   proof inputs and may use only an exact-state receipt.
7. Revalidate an exact source sentinel immediately before exact reuse. For
   compatible reuse, revalidate the candidate receipt and publish the
   planner-proved result into the current state only if the tracked/untracked
   source sentinel still matches. If the tree changed during either proof, run
   the owner normally.
8. Cache discovery, Git-tree lookup, decoding, planner execution, or publication
   failure degrades to an ordinary owner run. A missing Git object is a miss;
   it is never reconstructed from untrusted cache content.
9. Report compatible reuse explicitly with the prior state prefix and changed
   path count. It remains development feedback and is not qualification
   evidence.

## Evidence

The focused `verification-owner-stream` test covers state-record publication,
same-repository and same-host candidate selection, host/repository rejection,
exact Git-tree changed-path enumeration, corrupt-result rejection, concurrent
publication, retention bounds, source-tree preparation bracketed by stable
sentinels, exact-state sentinel confirmation, and process timeout cleanup. All
six focused cases passed after those checks were added.

An end-to-end changed-file run executed all six focused owner cases in 15,682 ms
and stored a receipt. After four documentation-only paths changed, the same
request reused that result as
`source-state=Compatible changed-paths=4 from-state=933d59cd5667`. The current
planner passed all 31 general and 273 native routing cases before both runs;
the expanded contract now passes 277 native cases after adding all four combined
database-project routes.
Qualification did not consume this result and no qualification claim was made.

A third source state changed the result-cache implementation itself. Although
compatible receipts existed, the global proof-input barrier rejected them and
the owner executed all six cases again in 16,188 ms. This demonstrates both the
positive unrelated-tree path and the required exact-state fallback when cache
safety code changes.

A fourth state changed the selected owner's own test module without changing a
global barrier. The current delta plan selected `verification-owner-stream`, so
compatible reuse was rejected and all six cases executed in 17,319 ms. This
separately proves the ordinary affected-owner invalidation path.

## Consequences

- A passing compiler or database development owner can survive unrelated
  documentation, website, or independently owned source changes.
- Reuse cost is bounded by fifteen candidate states and current planner calls,
  normally seconds rather than the retained owner's minutes.
- Planner coverage becomes an executable cache-safety boundary. New or unknown
  paths fail closed instead of inheriting a receipt.
- The first compatible lookup after a tree change performs the proof; later
  lookups use the newly published exact-state receipt.
- Existing exact-state receipts remain structurally valid, but the version-2
  owner action intentionally prevents old receipts that omitted the registered
  owner contract from crossing source states.

## Reconsideration triggers

Replace planner-delta proof with a direct owner dependency digest when every
owner has a complete validated transitive input declaration. Reconsider the
candidate bound when measurement shows more than fifteen retained states are
needed, but do not increase it without retaining bounded directory traversal.
Disable compatible reuse for an owner whose dynamic or ambient input cannot be
represented by the host identity, owner action, changed-tree delta, and current
planner ownership.
