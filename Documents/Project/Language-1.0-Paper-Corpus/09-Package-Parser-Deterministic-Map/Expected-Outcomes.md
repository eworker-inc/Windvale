# Workload 9 expected semantic outcomes

## Valid reference

1. All four package-data bindings validate before `Run` begins.
2. The two notice declarations compare equal; no storage identity is observable.
3. Manifest parsing yields root `app`, version 1, dependencies `{codec, util}`.
4. Lock parsing accepts shuffled input and yields four canonical map ranks:
   `app`, `codec`, `core`, `util`.
5. Dependency ranks are canonical; app becomes `codec`, `util` despite the lock
   spelling `util,codec`.
6. All dependency identities resolve to one map key.
7. Topology publishes `core`, `codec`, `util`, `app`.
8. The report matches the 160-byte multiline oracle and exact SHA-256.

## Input permutation oracle

Every permutation of the four lock package lines and every permutation of each
dependency list must produce identical immutable map ranks, dependency ranks,
topological order, report bytes, and SHA-256. A conforming tree, compact sorted
vector, or other representation must produce the same result under the
published comparison-work bound.

## Ownership outcomes

- successful collection insertion accepts each owned key/value once;
- duplicate or rejected insertion returns the exact original owned inputs;
- immutable publication consumes the mutable collection once;
- rank observation borrows only the published owner;
- failed parsing/topology/reporting publishes no partial successful result; and
- all child budget authority returns during lexical failure teardown.

## Content-object outcome

The package manifest has four declaration-reference records. The content table
has three objects with exact lengths 63, 111, and 53. The two notice references
select the same 53-byte object. Unique payload bytes are exactly 227. A package
containing two physical copies or a loader charging two notice payloads in the
same domain fails the nonduplication evidence even if source output matches.

## Compiler planning ceilings

The future executable fixture admits at most 48 generic instances, 320 WIR
blocks, 2,560 WIR operations, 24 call-depth units, 32 diagnostics, and 768 KiB
retained compiler evidence. These are admission ceilings to test, not measured
claims. A compiler exceeding one must reject with the named limit instead of
silently retaining more work.
