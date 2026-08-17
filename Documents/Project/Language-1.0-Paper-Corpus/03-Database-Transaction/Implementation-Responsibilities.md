# Workload 3 implementation responsibilities

## Responsibility map

| Contract | Owner | Required implementation evidence |
| --- | --- | --- |
| records, variants, match, borrowing, moves | Language compiler/type checker | accepted/rejected source and local diagnostics |
| argument-derived generics | type checker | `T`, `K`, and `V` solved only from first value/key/value arguments |
| `Ordering<Fieldˉkey>` | protocol resolver | one exact implementation; no import-order or overload choice |
| ordered map | Foundation collections/runtime | worst-case bound, ascending rank access/iteration, ownership-preserving rejection |
| typed arena and handle | Foundation collections/runtime | runtime capacity, opaque identity/index/generation, stale/wrong-arena tests |
| memory budgets | Foundation memory/runtime | two exact child charges and return on all paths |
| strict `u64` and text byte length | Foundation numeric/text | existing accepted exact semantics |
| resource release/completion split | language + Foundation resource | `using` releases exactly once and never commits |
| typed customer schema | package/build planner + database adapter | exact schema digest, explicit codec, malformed row corpus |
| transaction root and session | capability catalog/provider | reduced authority, operation/row limits, generation and revocation |
| point lookup and stage | provider | snapshot/revision semantics and bounded retained state |
| commit outcome | provider + storage engine | rejection/conflict/completed/indeterminate proof boundaries |
| reopen/recovery | separate launcher/provider session | old-or-new selection, corruption rejection, no replay |
| WVB/WIR lowering | shared compiler/backend | ordinary generic/variant/resource/capability lowering only |
| dual-host qualification | verification owners | deterministic oracle on Windows and Linux; Windvale when provider exists |

## Foundation work

Implement one `Arena<T>` whose maximum node count is stored in the owner and
checked before allocation, plus opaque `Handle<T>` validation. Implement the
accepted first-item constructors and insert/borrow/iterate families recorded in
Decision 0757. Preserve original owned inputs on rejected insertion. The
implementation may use arrays, trees, slots, or compact nodes; layout is not
observable.

The ordered map must publish and enforce a finite comparison bound derived from
`Maximumˉitems`. Tests use adversarial insertion order and an ordering
implementation that counts comparisons.

## Compiler work

No grammar change is required. The first-item calls solve all generic parameters
from explicit arguments under Decision 0754. The compiler must diagnose an
unsolved empty generic constructor; this bundle does not invent result-context
inference. Workload 4 must test whether first-item construction remains usable
for larger maps/arenas before source freeze.

Borrow checking must reject mutation while a map value borrow is live, a handle
borrow escaping the map owner, a node borrow crossing arena mutation, transaction
copy/use after release, and `using` resource escape. Diagnostics should name the
owner and conflicting borrow or move.

## Database/provider work

Start with a deterministic in-memory provider exposing the exact typed API. Pair
it with a fault script for every begin/lookup/stage/commit boundary. Then adapt
the existing Windvale persistent single-writer database contracts. The adapter
validates explicit schema bytes before constructing safe source values and
serializes named fields without reflection.

Commit proof must distinguish no-dispatch rejection, revision conflict, durable
completion, and uncertainty. A transport disconnect is not automatically proof
of rejection. Provider restart increments generation and never retargets the
old transaction.

## Launcher and recovery work

The application launcher binds the exact collection, schema, provider generation,
cancellation generation, operation limit, and memory budget. A second recovery
launcher reopens in a fresh session after success or uncertainty. It records a
bounded structural transcript and refuses duplicate commit identities.

Recovery must reserve resources independently of the failed transaction so a
capacity or provider fault cannot prevent teardown/evidence indefinitely.

## Verification ownership

Add focused owners for Language 1.0 collections/arenas, typed database adapter,
transaction provider, and recovery oracle when implementation begins. Changed
paper documents currently require documentation verification only; no current
Seed or provider source is changed by this bundle.

Cross-host conformance cannot be claimed until Windows and Linux reports agree
on source semantics and canonical WVB. Windvale OS evidence is required before
claiming that provider target, but it does not block paper source review.
