# Decision 0850: Own Vector-containing aggregates as WVB 1.28

## Status

Accepted on 2026-08-24 with paired Windows/Linux focused development evidence.
This completes the aggregate-owned-field checkpoint for records, variants, and
fixed arrays containing Vector values. It does not add borrowed aggregate
parameters/results, owned phis, user-defined destruction, resource-bearing
Vector elements, or the remaining hosted resource consumer.

## Context

WVB 1.26 could move an exact `Vector<T>` through ordinary calls and returns,
and WVB 1.27 could grow that Vector transactionally. A record, variant, or
fixed array containing the same Vector was still treated as an ordinary
copyable aggregate. That gap could duplicate the hidden descriptor, release a
lease twice, permit use after move, or leak the nested allocation when a frame
returned.

The source language already needs ordinary aggregate composition. Requiring
every resource to remain a top-level local would make useful records unnatural,
but exposing pointers or a general destructor protocol would widen Language
1.0 substantially. The next checkpoint therefore needs recursive ownership
without general aliasing or hidden runtime authority.

## Decision

1. A concrete or materialized generic record, variant, or fixed array is owned
   when its validated layout recursively contains an exact Foundation
   kind-11 `Vector<T>`. Classification follows at most 64 ancestor types and
   reuses bounded, acyclic nominal evidence.
2. The exact affine Result shapes produced by memory Split and fallible Vector
   construction retain their existing specialized proof. Direct Vector values
   retain the WVB 1.20/1.26 proof. Neither is reclassified as a new aggregate.
3. Aggregate construction consumes every owned field or element. A unique
   local store, by-value call, or owned return moves the entire aggregate and
   invalidates its originating local. Duplicate transfer, use after move,
   asymmetric joins, owned phis, partial field moves, and record updates that
   could leave hidden ownership reject before publication.
4. Field and element observation borrows the parent rather than moving it. An
   owned selected field is non-owning evidence and cannot satisfy a by-value
   consumer. An explicit mutable field borrow additionally requires the parent
   to resolve to a mutable binding.
5. WVB 1.28 is the lowest minor selected by a Vector-containing aggregate.
   Whole aggregate parameters, returns, and ordinary locals keep
   their existing shapes. Borrowed aggregate parameters and returns are not
   admitted in this profile.
6. Three local-only serialized view shapes represent generated observations:
   `28` for a record, `29` for a variant, and `30` for a fixed array, each
   followed by the exact nominal Types index. They are invalid in parameters,
   returns, source-declared locals, fields, payloads, collection elements, and
   Types entries.
7. Each view is confined to exactly `local.load owner; local.store view;
   local.load view; observer`. The compiler-aligned verifier requires matching
   owner/view nominal identity, rejects taking or escaping the view, and derives
   whole-value ownership recursively from Types. Its transfer tags `31` through
   `33` are verifier-internal and are never serialized.
8. Recursive verifier classification uses an explicit bounded work stack: at
   most 8,192 steps, 4,096 pending frames, and a 64-type ancestor path.
   Malformed indices, cycles, or bound exhaustion reject conservatively.
9. The scalar runtime normalizes a view to the parent's ordinary aggregate
   representation. Construction retains descriptor-bearing fields. Return and
   top-level teardown run a bounded deterministic aggregate mark/sweep whose
   roots exclude departing locals but include the remaining stack, the return
   value, and caller frames; swept aggregates recursively release nested Vector
   descriptors and allocation leases exactly once.
10. Existing 768-cell aggregate-arena, 64 KiB descriptor-heap, 64-owned-slot,
    4,096-instruction, and compiler native-frame limits remain unchanged. No
    host allocation, pointer value, ambient authority, or unbounded tracing is
    introduced.

## Consequences

- `Owned-Aggregate-Vector-Executable.wv` emits byte-identically twice as a
  1,538-byte WVB 1.28 module at SHA-256
  `b9810655b33c79cf980ea05f7fbca5511d3c34219f37e1b6a046a630a3e1c395`.
- Its `Workˉqueue<Vector<i32>>` performs immutable and mutable field
  observation, moves the whole record through an ordinary call, releases the
  nested Vector exactly once, and returns `42`.
- Four source failures cover use after whole-value move, duplicate move,
  partial owned-field move, and mutable borrow from an immutable parent. Six
  byte-level corruptions cover version, borrowed parameter, nominal identity,
  owner/view substitution, premature take, and view take.
- The combined focused owner passes 101 cases: 13 valid modules, 59 malformed
  modules, five aggregate source cases, retained owned-call/`using` evidence,
  and result `42`.
- The exact 101-case summary and all portable WVB identities match on Windows
  and Linux. This is focused development conformance, not the broad repository
  Qualification gate or promoted runner-candidate repinning.
- The registry remains 112 owners and advances to 5,430 cases; its 17,601
  LF-only bytes have SHA-256
  `5e9d388aa6c744f1f865af15386ae0c652bb1768b3c7e8b434fcd555dc3acd87`.
- Aggregate observation has a deliberately mechanical serialized form. A later
  optimizer may remove redundant physical retains only if the verifier-visible
  ownership and teardown contract remains equivalent.

## Reconsideration triggers

Broaden aggregate borrowing only with explicit lifetime/provenance evidence that
cannot escape its owner. Add owned phis, partial moves, record updates,
resource-bearing Vector elements, or user-defined destruction only with exact
initialization-state, rollback, trace, and teardown proofs. If the fixed arena
or verifier bounds become measured product blockers, replace them through a
separately versioned bounded representation rather than weakening validation or
silently allocating from ambient host memory.
