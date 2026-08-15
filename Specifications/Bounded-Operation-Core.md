# Bounded Operation Core

## Status

Implemented candidate under
[Decision 0587](../Documents/Decisions/0587-First-Bounded-Operation-Deadline-And-Cancellation-Core.md).

## Purpose and scope

`Windvaleˉboundedˉoperationˉcore` is the first capability-free asynchronous
operation contract shared by networking, files, processes, terminals, devices,
and package services. It defines virtual monotonic time, generation-bound
operation state, exact terminal outcomes, bounded event queues, wait batches,
reserved cancellation and teardown capacity, and deterministic queue closure.

The core performs no waiting and owns no threads, callbacks, sockets, native
handles, wall clock, TLS, HTTP, trust store, entropy, or credentials. A provider
drives the immutable model with explicit observations. A later native wait/timer
provider must preserve this contract rather than make host event-loop behavior
part of Windvale semantics.

## Identities and generations

Every accepted operation binds all of:

- a nonzero opaque provider identity and provider generation;
- a nonzero opaque operation identity and operation generation; and
- a nonzero monotonic-clock generation.

An event must match all provider and operation identity/generation fields before
it can affect state. A mismatched event returns `Invalidˉidentity` or
`Invalidˉgeneration`, is not applied, and leaves the supplied immutable state
unchanged. Clock observations from another generation return `Invalidˉclock`.

Provider restart and teardown require a nonzero replacement generation distinct
from the bound generation. They terminate old operations; they never retarget an
accepted operation to the new provider.

## Virtual monotonic time and deadlines

`Operationˉclockˉcreate` establishes an opaque generation and initial `u64`
tick. `Operationˉclockˉadvance` adds an explicit delta and rejects overflow.
Ticks have no fixed unit in this core. A concrete provider must publish the unit
and derive observations from monotonic time, never civil wall time.

An operation deadline is an absolute tick in the bound clock generation. A
deadline less than or equal to the begin tick produces an unaccepted terminal
`Timedˉout` state. For an accepted operation, each transition first compares the
observation tick with the deadline. At `tick >= deadline`, timeout wins over the
simultaneously presented completion, cancellation, provider-loss, restart, or
teardown observation. Before the deadline, the presented observation wins.
This explicit ordering makes deadline races reproducible. Every terminal state
also records its exact cause. A dispatched mutation whose deadline wins reports
`Submissionˉindeterminate` with cause `Deadline`, preserving both the trigger
and the prohibition on unsafe retry.

## Admission, phases, progress, and completion

`Operationˉbegin` produces an accepted `Queued` state after validating every
identity, generation, clock, and deadline. `Operationˉreject` produces an
unaccepted terminal `Rejected` state without dispatch. A provider may also
terminate an already accepted queued operation with a `Reject` event.

Accepted operations progress through:

1. `Queued`;
2. optionally `Dispatched`; and
3. exactly one `Terminal` state.

A `Complete` event is valid in either `Queued` or `Dispatched`, permitting a
provider to publish immediate completion without inventing a dispatch event.
`Progress` is valid only after dispatch. Its cumulative value must increase and
must not exceed the immutable progress limit. Completion progress must be at
least the last published progress and no greater than that limit. Progress is
local-provider evidence defined by the consuming contract; it never implies
remote receipt or application commit.

A transition presented after terminal completion is rejected as
`Invalidˉstate`; it cannot replace the first terminal outcome.

## Cancellation, loss, restart, and teardown

Cancellation before dispatch produces `Cancelled`. Cancellation of a dispatched
nonmutating operation also produces `Cancelled`. Cancellation of a dispatched
mutating operation produces `Submissionˉindeterminate`; the core does not assert
that the peer observed or committed nothing. The terminal cause remains
`Cancellation`.

Provider loss produces `Providerˉlost` unless a mutating operation was already
dispatched, in which case it produces `Submissionˉindeterminate`. Provider
restart produces `Stale` under the same rule. Direct teardown produces
`Tornˉdown` unless dispatched mutation makes completion indeterminate. No
indeterminate mutation is retried by this core. The distinct `Providerˉloss`,
`Providerˉrestart`, and `Teardown` causes remain present even when their common
effect outcome is indeterminate.

These are conservative shared semantics. A higher contract may prove stronger
pre-dispatch evidence and publish a narrower terminal result, but it must not map
uncertain post-dispatch mutation to known-not-sent.

## Bounded event queue and wait batch

An event queue binds one provider identity and generation and has a capacity from
3 through 64 events. Its internal immutable byte representation uses fixed
48-byte entries for this implementation; those bytes are not a serialized
interchange format and have no compatibility promise.

Ordinary events may fill only `capacity - 2` entries. One additional entry is
reserved for a single cancellation and the last entry is reserved for teardown.
A duplicate queued cancellation is rejected. Normal queue exhaustion therefore
cannot prevent either control operation.

`Operationˉqueueˉclose` appends one provider-wide teardown event containing the
replacement generation and permanently closes the old queue. No later event is
admitted. `Operationˉqueueˉwait` returns at most the caller's bounded maximum in
arrival order plus an immutable remaining queue. After the teardown event is
drained, every further wait reports `Closed` rather than `Empty`, so a waiter
cannot sleep again on an invalidated provider generation.

The core does not maintain an operation registry. A concrete provider remains
responsible for applying teardown to every accepted operation it owns before
releasing state. The closed wait result and provider-wide teardown event are the
common wake and invalidation evidence used for that enumeration.

## Limits and malformed-state handling

- Queue capacity: 3 through 64 events.
- Wait batch: 1 through 64 events.
- Cancellation events queued per provider queue: at most one.
- Teardown events queued per provider queue: at most one, created only by close.
- Progress and deadlines: checked `u64` values.

Every public queue operation revalidates capacity, byte length, entry kinds,
reserved fields, identities, generations, cancellation count, and closed-state
agreement before reading an entry. Invalid nominal values fail closed.

## Executable evidence

`Bounded-Operation-Core-Self-Test.wv` covers ten groups:

1. virtual-clock generation and overflow;
2. immediate rejection, expired admission, and immediate completion;
3. dispatch followed by queued completion;
4. increasing partial progress and limit rejection;
5. cancellation before and after mutating dispatch plus terminal race closure;
6. completion immediately before and exactly at a deadline;
7. stale events and provider restart;
8. provider loss before safe completion and after mutating dispatch;
9. queue exhaustion, the reserved cancellation slot, duplicate cancellation,
   reserved close capacity, and post-close rejection; and
10. bounded wait order, provider-wide teardown delivery, persistent closed wake,
    and indeterminate teardown after mutating dispatch.

The focused native owner builds the library and test twice, requires byte-identical
WVB and WVO outputs, executes the current-host image, and constructs the other
host image from the same linked native bytes.

## Secure-network consequence

This core completes network slice 1. It does not implement HTTPS. Secure HTTP
still requires the ordered address/authority, semantic stream, host resolver,
monotonic timer, entropy, trust, secure-stream, and HTTP framing slices. The
first HTTPS provider must require TLS 1.3, verify the service identity for the
bound origin, reject ambiguous HTTP framing, disable 0-RTT for version 1, and
preserve this core's no-replay result for uncertain mutation.
