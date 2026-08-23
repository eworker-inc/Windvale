# Decision 0834: Transfer the launcher memory budget as WVB 1.21

## Status

Accepted on 2026-08-23. Candidate toolset qualification remains pending.

## Context

Decision 0832 gave `Foundationˉmemory.Memoryˉbudget` one canonical source and
WIR identity but deliberately stopped before bytecode publication. The next
connected ownership checkpoint needs the launcher to transfer one non-forgeable
root budget into the application, prove that bytecode cannot copy or construct
it, and release it when the entry invocation ends. It must not imply that
`Split`, allocation leases, general owned calls, or public fallible collection
construction already exist.

The compiler also imports `Foundationˉmemory`, whose exact Language 1.0
declarations include the currently non-executable `Allocationˉreason: u8` enum.
Rejecting a module merely because an unused declaration has no WVB
representation prevents the budget entry from compiling. Selectively pruning
and renumbering individual nominal types would be a larger type-graph
optimization with more correctness risk than this checkpoint needs.

Broad optimization of the transitional compiler is still likely to be wasted
when the Language 1.0 compiler becomes the active seed. Work before then remains
limited to measured blockers, verification waste, bounded-resource failures,
and clear accidental complexity that directly impedes the migration.

## Decision

1. WVB 1.21 appends private shape byte `25` for the exact
   `Foundationˉmemory.Memoryˉbudget` identity. It may appear exactly once: as
   the sole parameter of exported `Main(Memoryˉbudget) -> i32`.
2. Shape 25 is forbidden in every other function signature, local,
   temporary, record or variant field, collection element, operation shape,
   load, store, call, constructor, move, or ordinary return position. There is
   no forgeable scalar encoding for a budget.
3. At launch, the source-built scalar runner creates one fresh opaque token
   with checked identity and generation, transfers it into the entry parameter,
   and owns its teardown. A completed top-level return verifies the token,
   zeros the parameter cell, releases it exactly once, and only then publishes
   the `i32` result. Failed execution tears down the complete invocation domain.
4. The token proves ownership transfer and lifetime only. The launcher's byte
   maximum and accounting remain outside bytecode and unobservable until
   `Memoryˉbudget.Split` and allocation-lease operations are implemented.
5. Optimized WVB emission may publish an empty Types section only when a
   complete WIR-closure scan finds no nominal shape use in signatures, locals,
   temporaries, operations, record or variant targets, or collection targets.
   If any nominal use exists, the complete Types table and its existing indices
   are emitted unchanged. Partial type pruning and index remapping are not part
   of this decision.
6. A declaration-only unsupported enum therefore need not block output, while
   an actually used non-`i32` enum still fails as `Unsupportedˉshape`. This is
   dead declaration elimination, not widened enum execution.
7. Compiler-emission failures include exact function, operation, and source-line
   coordinates so a long focused verifier exposes the failing boundary without
   a second diagnostic run.

## Consequences

- `Memory-Budget-Entry-Main.wv` emits deterministic WVB 1.21: 242 bytes,
  16 code bytes, and SHA-256
  `499c59fa1207917fd64ee0703569d3dc4a80c5075fc99923e657adc5e4f9ed65`.
  The compiler-aligned verifier accepts it and the source-built runner returns
  `42` after exact launcher-to-entry transfer and release.
- An independent bounded verifier accepts the canonical module and rejects
  nine malformed variants covering version, parameter shape/count, entry name,
  return/local placement, load, store, and a missing export.
- The declaration-only all-width enum fixture now emits a 217-byte executable
  and returns `42`. `Enum-U8-Used-Main.wv` proves that a used `u8` enum still
  stops at `Unsupportedˉshape` with exact function/operation coordinates.
- The retained generic-specializations oracle keeps its three executable
  functions while dropping one unused ordinary record Types entry, reducing
  deterministically from 498 to 473 bytes at SHA-256
  `39811a38c92b8d4a6459750c64f85cf4e500bb4a2e4e83d31ab3bab626a70e12`.
- The current analyzer remains 1,132,570 bytes at SHA-256
  `e3eef9e462f47cb88d4de174eb1e714106b346137538d9e6b396361b834d8471`.
  The current emitter is 1,054,673 bytes at SHA-256
  `2b5b4af681a36569b39be9dd46999af5b7babbc5cff53e6d3aec5227590a7e8b`.
- Exact compiler self-emission consumes 1,961,550 source bytes and produces
  104 manifest bytes, 294,832 binding bytes, 3,926,604 WIR bytes, and a
  1,054,673-byte WVB with 554 functions and 876,881 code bytes. The independent
  native verifier accepts it.
- The current source-built runner is 230,259 WVB bytes at SHA-256
  `d1393ec3cb83d95cf86902768893846e4dc0e5a742b46363c86e19712ec674ba`.
  Its promoted paired-host profile remains separately pinned.
- The focused Language 1.0 owner advances from 427 to 440 cases. The 108-owner
  registry advances from 5,211 to 5,224 declared cases at SHA-256
  `78dcce3ba389c2e265c1601bbe32f84e873e8742c795ab1a243317013301b0db`.

## Reconsideration triggers

Replace the entry-only shape rule when general owned call transfer, explicit
moves, `using`, or resource-bearing locals have a complete verifier and runtime
contract. Make budget capacity observable only with `Split`, allocation leases,
and exact accounting/failure behavior. Revisit conservative all-or-nothing type
emission only with a typed dependency graph, deterministic index remapping, and
a simple complete-table correctness oracle. Reprofile or optimize the compiler
after it becomes the active seed, or earlier only from reproducible migration or
verification pressure.
