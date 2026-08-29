# Decision 0875: add a bounded parallel hosted-task scheduler

## Status

Accepted implementation checkpoint on 2026-08-29. Focused Windows evidence is
complete. Independent Linux evidence and the final Slice 7 Qualification gate
remain pending.

## Context

The promoted WVB 1.32 runner is the deterministic single-thread correctness
oracle. Its complete interpreter state includes one heap, record table, memory
budget table, task table, output buffer, and active continuation. Mutating those
structures from host threads without an ownership and merge protocol would
introduce races and would no longer implement the verified sequential model.

Language 1.0 nevertheless requires one parallel-capable hosted policy on both
permanent hosts. The policy must remain below source and bytecode semantics,
run real child work concurrently, preserve creation identities and consuming
awaits, bound every retained resource, and produce the canonical observation
already established by the sequential oracle.

## Decision

1. Keep the source-built scalar WVB 1.32 runner as the portable correctness
   oracle. Do not add a thread switch to its shared mutable interpreter state.
2. Add one hosted scheduler whose admitted work is an opaque bounded byte
   descriptor and whose executor is an exact launcher-selected file identity.
   Run accepted work on at most the declared number of Node.js worker threads.
3. Reserve the complete 56-byte task record, owned work bytes, maximum terminal
   bytes, and declared work units before transfer. Preserve the existing finite
   64-child, 1-MiB retained-state, and 1,000,000-work-unit ceilings.
4. Retain nonzero creation identities and affine generation-one handles.
   Completion may publish out of order; observation consumes each exact handle
   once. Teardown joins or cancels and joins all remaining work and releases all
   reservations.
5. Use one shared atomic cancellation word. The request is idempotent and is
   observed cooperatively. Worker protocol failure becomes explicit task-runtime
   loss, not a fabricated child trap or typed application failure.
6. Prove real parallel capability with four worker threads that all reach one
   atomic rendezvous before any completes. Publish the same `3,1,0,2` completion
   order as the sequential fixture while creation-ordered joins remain
   `0,1,2,3` and return `42`.
7. Keep executor work isolated in this first profile. A concurrent binding to
   the scalar interpreter's shared heap requires a later explicit ownership and
   merge design; it is not smuggled into this checkpoint.

## Evidence

The focused Windows owner passes 49 cases. Four workers are simultaneously live,
all four arrive at and leave the shared rendezvous, completion order is
`3,1,0,2`, join order is `0,1,2,3`, and the final result is `42`. The exact
transcript SHA-256 is
`ec46e8f65ee74954a51180781f35d37e1d1b36dda92e4c4ac2872a1b0eefb576`.

The owner also covers invalid limits and executor identities, duplicate
completion policy, child/queue/memory/work refusal before transfer, returned
work identity, live cooperative cancellation, repeated cancellation, spawn
after closing, typed child failure, double await, runtime loss/restart, contained
trap identity, worker-lifetime containment, malformed worker-protocol loss, and
complete teardown accounting.

Independent Linux execution and the complete final qualification remain required
before this decision closes Slice 7.

## Consequences

- Windvale gains a real parallel-capable host policy without adding threads to
  the language or destabilizing the portable oracle.
- The host scheduler is small and independently verifiable; edits do not require
  rebuilding the full compiler-scale structured-task owner.
- Executor selection remains an explicit runtime binding. It cannot be obtained
  from source text or used to widen capabilities.
- Isolated work and immutable outcomes provide a safe first parallel boundary.
  Shared mutable runtime heaps remain sequential until separately proved.

## Reconsideration triggers

Replace the isolated-executor boundary when a native WVB continuation executor
can prove disjoint ownership or deterministic bounded merge. Reconsider the
fixed worker policy if measured hosted workloads need a reusable pool, but keep
admission-before-transfer, affine handles, explicit generations, cooperative
cancellation, exact outcomes, and bounded teardown.
