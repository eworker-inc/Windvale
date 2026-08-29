# Windvale hosted task scheduling

## Status and scope

This specification defines the first bounded parallel-capable scheduler policy
beneath the Language 1.0 structured-task contract. It complements the
deterministic sequential WVB 1.32 oracle; it does not replace that oracle,
change source syntax, add thread identities to source, or alter WVB verification.

The current implementation is
`Runtime/Hosted/Tasks/Bounded-Parallel-Task-Scheduler.mjs`. It runs explicitly
selected, isolated hosted executors on Node.js worker threads. Work is opaque to
the scheduler and is admitted only through bounded byte descriptors. A target
binding is responsible for selecting an executor that implements the verified
child-work contract. Executor selection is launcher/runtime policy, never a
source capability or ambient module lookup.

## Construction and limits

Construction requires all of the following finite limits:

- 1 through 64 accepted children;
- 1 through the child limit runnable children;
- at least the runnable limit and no more than the child limit retained
  completions;
- 1 through 1,048,576 retained scheduler bytes;
- 1 through 1,000,000 work units; and
- 1 through the runnable limit worker threads, each with a 1-through-300,000-ms
  host recovery ceiling.

It also requires one exact file-URL executor identity, one nonzero admitted task
runtime generation, and one observed generation. A zero observed generation
produces `Runtimeˉlost`; a distinct nonzero generation produces
`Runtimeˉrestarted`. Invalid construction values fail before any worker exists.

## Admission and ownership

One spawn accepts 1 through 65,536 opaque work bytes and a positive maximum work
count. Before accepting the work it reserves:

- one 56-byte task record;
- the complete work descriptor;
- one 65,536-byte maximum terminal value; and
- the child's declared work-unit ceiling.

The lower application or runtime limit wins. A closing scope, exhausted child
count, full runnable/completion reservation, insufficient retained bytes, or
insufficient work units rejects before dispatch and returns the exact work bytes.
Accepted work receives one nonzero creation identity and generation. The
resulting handle is consumed once; a forged, stale, or second observation fails.

The implementation conservatively reserves the child's complete declared work
allowance at admission. A future executor binding may share the scope meter more
precisely only if it preserves the same no-overcommit and pre-transfer failure
boundary.

## Worker and outcome protocol

Each dispatched worker receives only its immutable task identity, runtime
generation, limits, owned work bytes, shared cancellation word, optional bounded
coordination state, and exact executor URL. The wrapper imports that executor and
requires one `Executeˉboundedˉhostedˉtask` function.

The executor returns one of the seven existing task-outcome kinds, a value no
larger than 65,536 bytes, exact `u64` evidence, and a consumed work count no
greater than its admitted maximum. The scheduler revalidates the complete
record. Only `Valid` and typed `Failure` carry value bytes. Cancellation and
deadline carry no evidence. Runtime loss/restart carry exact generations, and a
contained trap carries one nonzero `u32` identity. A malformed response, worker
exception, worker exit, or worker-lifetime expiry is task-runtime loss; it cannot
be mislabeled as a source trap or typed child failure.

## Cancellation, completion, and teardown

The first cancellation request atomically closes the scope and marks one shared
word. Later requests are idempotent. Executors observe the word cooperatively;
there is no asynchronous source exception or forced replay. Teardown closes
spawn, optionally requests cancellation, joins every unconsumed child, and
requires live tasks, retained bytes, and reserved work units to return to zero.

Workers may finish independently. Completion records retain their original
creation identities, while source consumes handles in its chosen order. The
focused conformance policy is one complete four-slot permutation. It holds four
bounded results and publishes task slots `3, 1, 0, 2`; creation-ordered awaits
still return slots `0, 1, 2, 3` and the
canonical transcript remains:

```text
3
1
0
2
Result: 42
```

The completion policy is runtime evidence, not a portable scheduling promise.

## Parallel-capability evidence

The permanent executor fixture performs bounded work in four separate worker
threads. All four must reach one shared atomic rendezvous before any returns;
the scheduler must report peak active workers four and the rendezvous must record
four arrivals and four departures. This is real concurrent worker execution,
not a synthetic completion trace or an inference from host CPU count.

The same focused owner runs unchanged on Windows and Linux. Paired-host evidence
requires exact agreement on 49 cases, worker count four, completion order
`3,1,0,2`, join order `0,1,2,3`, result `42`, and transcript SHA-256
`ec46e8f65ee74954a51180781f35d37e1d1b36dda92e4c4ac2872a1b0eefb576`.

Commit `aef806b7` satisfies that focused paired-host contract in GitHub Actions
run `33235016333`: Windows and Linux report the same 49-case result and exact
observation identity, and the aggregate Development gate passes. This is the
required parallel-capability evidence; complete Slice 7 Qualification remains a
separate final integration gate.

## Boundaries

This first host policy does not expose threads, worker counts, scheduler events,
detached work, general task collections, channels, or races to Windvale source.
It does not make the scalar WVB interpreter's shared heap concurrent. Isolated
executors must return immutable bounded completion records; a future shared-heap
executor requires a separate data-race and merge proof. The current policy does
not add provider retry, replay, capability acquisition, or transparent restart.
