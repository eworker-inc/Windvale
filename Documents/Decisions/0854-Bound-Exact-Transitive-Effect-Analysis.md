# Decision 0854: Bound exact transitive effect analysis

- Status: Accepted
- Date: 2026-08-25

## Context

Language 1.0 requires an exported function to declare its exact direct and
transitive language and capability effects. Source parsing already retained an
effect clause, but no compiler phase resolved canonical identities, inferred
private-function effects, or rejected an explicit clause that omitted or added
an effect. A naive fixed point bounded only by the function count could also
turn a very deep hostile call graph into an excessive verification workload.

## Decision

1. Resolve the eight frozen language-effect identities to exact bits and every
   other effect identity to one declared capability symbol.
2. Rank at most 32 capability symbols by canonical source-name order so symbol
   insertion order cannot change masks or output.
3. Infer direct effects from typed WVIR operations and propagate them through
   exact direct-call targets.
4. Require every exported function to carry an explicit clause and require an
   explicit clause to equal, rather than merely contain, the inferred set.
5. Publish compiler-private WVEF 1.0 with checked header, entry, aggregate, mask,
   target, and capability-symbol evidence.
6. Retain the current 87,380-function and 4 MiB WVIR ceilings, but stop
   propagation after 256 complete changing passes with exact
   `Evidenceˉlimit`. Do not make verifier time depend on an arbitrarily deep
   call chain.
7. Keep WVEF out of WVB. A requirement is not authority, and runtime capability
   binding remains a separate launcher/service-manager responsibility.

## Consequences

Exact effects are now structural compiler evidence available to function-type
and closure analysis. Missing, extra, duplicate, and unknown effects fail
closed, and recursive call groups converge deterministically when they fit the
explicit pass ceiling.

The 256-pass limit may reject a valid but exceptionally deep call graph. That is
preferable to an unbounded development or verification delay at this
checkpoint. A bounded reverse-edge work list or strongly connected component
algorithm may replace the round-based oracle later and lift the depth ceiling
without weakening exactness.

This decision does not make function values or closures executable. Typed
callable WVIR/WVB values, indirect calls, closure environments, verifier rules,
and runtime/native execution remain later Slice 6 checkpoints.

## Evidence

The focused effect fixture covers canonical resolution, direct and transitive
inference, recursive convergence, exported-clause enforcement, exact mismatch,
duplicate and unknown identities, allocation operations, and WVEF count bounds.
It executes within the callable-semantics owner. Exact current-host owner bytes,
digest, and registry identity are recorded in the migration evidence after the
coherent Slice 6 checkpoint is rebuilt.

## Reconsideration triggers

Replace the round-based implementation if representative modules approach the
pass ceiling or effect propagation materially contributes to compilation time.
Any replacement must keep an explicit time/work bound, exact source identities,
deterministic output, malformed-evidence rejection, and a simple differential
oracle.
