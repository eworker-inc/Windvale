# Decision 0841: Prove generation-safe allocation leases

## Status

Accepted on 2026-08-23. Connection to WVB, physical allocation, Vector
construction, provider refusal, collection-owned teardown, paired-host
conformance, and semantic effect enforcement remain pending within Slice 5.

## Context

Decision 0840 gives fallible reserved Vector construction an exact source and
typed-WVIR identity, but deliberately leaves executable allocation closed. The
next runtime boundary needs to convert one consumed `Memoryˉbudget` into a
move-only `Allocationˉlease`, make the old budget unusable, retain exact maximum
and current bytes plus alignment, and credit the parent when the collection is
released.

Embedding a heap pointer or the scalar runner's allocation table in the
language would freeze a transitional representation. Treating a lease as the
same opaque token as a budget would be unsafe: the internal Split operation
could then accept collection-owned allocation authority as a parent budget.

The existing fixed-capacity memory-accounting oracle already has one provider
generation per domain, stale-token rejection, recursive credit, and bounded
teardown. It can distinguish the two owner states without enlarging its private
2,616-byte state.

## Decision

1. Active budget-owner generations are odd. Active allocation-lease
   generations are even. Generation zero is uninitialized and
   `4294967295` retires a slot after its final owner is released.
2. Reusing an inactive slot selects the next greater odd generation. Converting
   a live budget to a lease increments its odd generation once. The old budget
   token therefore becomes stale atomically before the lease is published.
3. The current reference lease token is 28 bytes: domain identity, generation,
   maximum retained bytes, current retained bytes, and alignment ceiling. It is
   private bounded runtime evidence, not source-visible data, a serialized
   format, WVB shape, native ABI, or required target representation.
4. Lease construction requires a valid available budget, positive maximum,
   current bytes not exceeding that maximum, a power-of-two alignment from 1
   through 4,096, and a maximum no greater than the domain's unreserved bytes.
5. Invalid internal inputs return invalid evidence without mutation. Budget
   exhaustion returns recoverable reason 1 with exact requested and available
   bytes and preserves the budget and state. Generation exhaustion returns
   reason 3 and likewise preserves state.
6. Success changes only the owner generation and publishes the exact lease
   evidence. The caller remains responsible for physical allocation. If that
   later phase refuses, public constructor lowering must release the lease or
   still-owned budget locally before returning `Allocationˉfailure`.
7. Lease validation requires the exact token-carried maximum, current, and
   alignment values; a caller cannot substitute different release metadata.
   Release invalidates the owner, finalizes an unowned child, and recursively
   credits its exact maximum and child count to the parent.
8. Deterministic teardown clears both odd budget and even lease owners under the
   existing 65-domain and fixed-pass bounds. A stale lease cannot release a
   reused domain.
9. This checkpoint changes no source syntax, public Foundation signature, WVIR,
   WVB, or physical collection backing. Opcode and provider work must reuse the
   oracle behavior without copying its private token layout into the language.

## Consequences

- The original 17 budget cases remain, with reusable budget generations now
  always odd. Twelve added cases cover exhaustion atomicity, invalid alignment,
  exact successful transfer, old-budget rejection, exact metadata validation,
  wrong-metadata refusal, parent credit, stale release, live-lease teardown,
  and generation-safe reuse.
- The focused self-test remains one deterministic portable program. Its WVB
  grows from 24,825 to 35,799 bytes and is byte-identical at SHA-256
  `3f156ef17f29c5673c0d383c713e04814327243783d2047cce2fc8fe6be117fb`.
- The private budget state remains exactly 2,616 bytes: 16 header bytes plus 65
  fixed 40-byte domains. Lease metadata travels with the private lease token
  rather than widening every inactive domain.
- The existing owner grows from 17 to 29 cases. The registry remains 112
  owners and advances from 5,332 to 5,344 cases at SHA-256
  `b34bd9e5ce73255db7da366b908dda29249df9514aff6f7dbb1918ce4d4489e1`.
- The shared executable Split owner still passes 15 cases: two valid modules
  return 42 and nine malformed WVB mutations reject. The canonical 752-byte
  Split module retains SHA-256
  `5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53`.
- This work fixes durable ownership and accounting semantics before
  self-hosting. It does not optimize or freeze the transitional compiler's heap
  strategy.

## Reconsideration triggers

Replace the private token layout when a real runtime needs a different bounded
representation, provided stale-owner, exact-accounting, failure, and release
behavior remain unchanged. Widen the 65-domain or 4,096-alignment oracle only
with a named profile and measured need. Do not serialize the token, permit a
lease in Split, preserve the old budget after successful conversion, or make
local release depend on provider availability.
