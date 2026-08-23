# Decision 0837: Execute memory-budget Split as WVB 1.23

## Status

Accepted on 2026-08-23. Paired-host conformance and downstream native,
browser, and Windvale OS consumers remain pending.

## Context

Decision 0836 gives the exact Foundation Split call a typed WVIR 1.5/1.6
operation but deliberately leaves WVB closed. The following checkpoint adds a
fixed-capacity accounting oracle with atomic reservation, generation-safe
ownership, recursive credit, and deterministic teardown. Together those
boundaries are sufficient to make Split executable without exposing a token
layout or pretending that collection allocation and leases already exist.

The first Split source program crosses match branches. Keeping the earlier
single-block ownership proof would make a valid ordinary Result match
unrepresentable. Raising compiler limits or broadly tuning the transitional
compiler would not solve that semantic gap.

## Decision

1. WVB 1.23 is the lowest minor containing executable budget Split. Opcode
   `CE` has fixed width nine: one opcode byte, a `u32` parent-local index, and a
   `u32` exact Result-type index.
2. The stack contains `u64` maximum bytes followed by `u32` maximum children.
   Split consumes both values, preserves the available parent owner, and
   produces one affine exact
   `Result<Memoryˉbudget, Allocationˉfailure>` value.
3. Shape `25` remains representation-hidden. WVB 1.23 permits it only as the
   exact launcher parameter and non-parameter locals of exported `Main`.
   `local.take` moves a budget or exact Split Result; ordinary load cannot copy
   either, and store cannot overwrite an available affine owner.
4. The verifier identifies the result by its machine contract, not unstable
   compiler-generated names: a two-case variant whose Valid case contains one
   shape-25 value and whose Failure case contains the exact three-field
   allocation-failure record and four-value `u8` reason enum.
5. The source ownership proof becomes bounded forward-CFG dataflow. It admits
   at most 64 blocks and 64 owned slots, intersects availability at joins,
   requires temporary owners to be consumed, and rejects backward control.
   The WVB verifier independently retains its 4,096-instruction owned-function
   bound.
6. The reference runner binds a 98,304-byte, 64-child root budget. Success
   atomically debits byte and child limits and returns one child token. Refusal
   preserves state and returns exact requested/available evidence. Top-level
   teardown releases all surviving owners deterministically.
7. WVB 1.23 contains at least one opcode `CE`; modules without Split retain
   their previous lowest minor. Kind-7 enums remain valid in 1.23 without
   changing their WVB 1.22 representation.
8. Broad optimization of the transitional compiler remains deferred until the
   Language 1.0 compiler becomes the active seed. Current work may retain
   durable caches, focused verification, explicit bounds, and measured blocker
   fixes that survive self-hosting.

## Consequences

- `Memory-Budget-Split-Executable.wv` produces deterministic 752-byte WVB 1.23
  at SHA-256
  `5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53`.
- A second source program requests more than the root maximum and proves the
  typed refusal branch. Both programs pass the current verifier and return 42
  through the source-built runner.
- Nine mutations reject version downgrade, unknown opcode, unavailable or
  non-budget parent, missing or wrong result type, primitive Valid payload, and
  unauthorized budget placement or wrong allocation-failure field width.
- The current analyzer remains 1,144,757 bytes at SHA-256
  `384cb966d9b8718fda0c2e7bf3863ae168ce7d9fcb911d076b87d5e33400b0e3`.
  The emitter is 1,084,963 bytes at SHA-256
  `694aa254b7147f2964d7cab3f7dba96e1076509c8ec3c91768e3c529b2ae71a4`.
- The source-built verifier is 263,234 bytes at SHA-256
  `5f8e8c93818bc64a1360e9b20d3893edddea3854b6d618d52d16bf3488bde468`;
  the runner is 282,833 bytes at SHA-256
  `2e37fc47eb61b8420bc9d30d24385a9427815f55c735d76adaff51ebb68e0f95`.
- The new 15-case focused owner advances the registry to 110 owners and 5,278
  cases at SHA-256
  `90cf308458315c105b3f735217a54bb9cc189d23099e9587b88d31998007178a`.
  Its first Windows pass includes cold/warm tool packaging evidence; paired
  Linux execution remains pending.

## Reconsideration triggers

Add loop ownership only with a terminating fixed-point algorithm and explicit
diagnostic/resource bounds. Change the runner root limits only as a named
profile decision, never as portable semantics. Add allocation leases, public
fallible collection construction, or general owned calls only with exact
failure, teardown, and verifier contracts. Reprofile compiler internals when
the 1.0 compiler becomes the seed, or earlier only for a measured blocker.
