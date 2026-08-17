# Decision 0756: Resolve the Language 1.0 file-copy findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0752](0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md),
[Decision 0754](0754-Resolve-First-Language-1.0-Paper-Findings.md), and
[Decision 0755](0755-Resolve-Language-1.0-Command-Workload-Findings.md).
It accepts the five owner resolutions from the bounded file-copy paper bundle.
It does not freeze source edition 1, change Windvale Seed, publish filesystem
capability signature-set identities, or claim a provider implementation on any
target.

## Context

The second mandatory paper workload expresses a bounded regular-file copy using
two rights-limited filesystem roots, move-only source and destination resources,
one budgeted mutable byte buffer, checked slices, explicit offsets, exact partial
progress, explicit durable finish, and locally infallible release. It covers
empty and maximum files, source growth, destination capacity, zero progress,
partial and indeterminate writes, finish failure, early propagation,
cancellation, provider loss, and provider restart.

The complete source needs no file syntax, native-path type, pointer, exception,
garbage collector, implicit close, command-specific WIR instruction, or parallel
compiler. Review exposed five decisions that later workloads should not answer
in incompatible ways:

1. fixed caller-owned byte buffers need an exact safe Foundation surface;
2. local `using` release and fallible semantic completion must remain separate;
3. known short progress needs an exact safe continuation rule distinct from
   uncertain mutation;
4. read-only source and create/write destination authority should remain
   independently grantable; and
5. synchronous provider cancellation can be exact without prematurely fixing a
   general source-visible cancellation-token API.

## Decision

### Fixed safe byte buffers

Accept `Foundationˉbytes.Byteˉbuffer` as a move-owned, fixed-length,
zero-initialized byte allocation and accept these version-1 signatures:

```text
export fn Constructˉbuffer(
    Budget: Memoryˉbudget,
    Length: u64,
) -> Result<Byteˉbuffer, Allocationˉfailure>
    effects(memory.allocate);

export fn Bufferˉlength(
    Buffer: borrow Byteˉbuffer,
) -> u64 effects();

export fn Borrowˉslice(
    Buffer: borrow Byteˉbuffer,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects();

export fn Borrowˉsliceˉmut(
    Buffer: borrow mut Byteˉbuffer,
    Start: u64,
    Length: u64,
) -> Mutableˉslice<u8> effects();
```

Construction consumes one rights-reduced memory budget. On success, the buffer
owns the transferred accounting and exactly `Length` initialized zero bytes. On
failure, construction consumes and locally releases the child budget before
returning `Allocationˉfailure`; it does not expose a partial buffer.

Both slice operations validate `Start + Length` with checked `u64` arithmetic
and require the complete range to lie within `Bufferˉlength`. Violation traps
before forming a borrow. The immutable or exclusive mutable result is tied to
the one buffer owner and cannot escape it. Normal borrow rules prevent overlap
with live mutable access. No safe uninitialized byte, unchecked Core/Hosted
slice, native address, capacity slack, or backing identity is exposed.

### `using` remains local release, not semantic completion

Retain the Language 1.0 rule that `using` invokes only
`Foundationˉresource.Localˉrelease<Self>`. It never implicitly flushes, finishes,
commits, publishes, rolls back, deletes, or reports a semantic completion.

For workload 2:

- a body failure skips destination finish and remains the returned failure;
- a successful body attempts `Finishˉdurable` exactly once;
- completed finish permits the success report;
- rejected finish becomes `Finishˉrejected`;
- uncertain finish becomes `Finishˉindeterminate` and is not retried; and
- destination/source release follows every ordinary path without replacing the
  body or finish result.

This policy makes body and finish results mutually ordered rather than silently
combining them. A future protocol that must complete after a body failure needs
an explicit named composition type capable of retaining both results; it cannot
change `using` into hidden exception-style precedence.

### Known partial progress may continue only from the proved boundary

Retain `Foundationˉresource.Mutationˉoutcome<E>` for destination writes.
`Rejected` proves zero progress. `Completed` must satisfy the operation's exact
complete-progress contract. `Acceptedˉpartial` proves an exact positive prefix
smaller than the request. `Indeterminate` provides no replayable count.

For the file-copy destination contract, `Shortˉacceptance` is the only partial
reason that permits continuation. Source advances buffer and destination
positions by the accepted prefix and submits only the unaccepted suffix. A
partial capacity, cancellation, loss, restart, or other terminal reason records
its proved prefix and stops. An indeterminate write stops immediately without
write retry, finish, alternate provider, or fallback.

A zero read before known snapshot EOF or zero partial write cannot drive a loop;
it is a bounded progress-stalled/provider-defect result. Every provider call is
also bounded by the launch's exact total-operation maximum.

### Independent source and destination filesystem authority

Accept `filesystem.copy.source` version 1 and
`filesystem.copy.destination` version 1 as independent required capability
candidates for this paper workload.

The source root acquires one read-only immutable snapshot with exact maximum
length, transfer size, operation count, provider generation, and content
generation. A provider unable to preserve or attest the snapshot rejects rather
than exposing live mutable host-file behavior. Later growth, shrink, or
replacement returns `Sourceˉchanged` before returning bytes.

The destination root creates one new regular object exclusively, never truncates
or replaces an existing object, enforces exact length/transfer/operation maxima,
and owns positioned writes plus one explicit combined durable finish. That
finish proves completed content, logical length, and created-name durability
under the provider's admitted stable-storage model or returns a typed rejection
or uncertainty.

Neither root grants enumeration, traversal, links, native paths, native handles,
source mutation, destination read, replacement, rename, deletion, metadata,
mapping, append, or a general filesystem object. A grant of one never grants or
substitutes for the other.

The authority split and workload behavior are accepted. Final catalog and
signature-set identities remain provisional until database, HTTP/service, and
system-boundary workloads test owned directory instances, transactions,
deadlines, recovery, and provider teardown.

### Synchronous cancellation profile

Accept one explicit nonzero cancellation generation as launcher metadata bound
into both filesystem provider roots for
`windvale.launch.file_copy.v1`. Acquisition, read, write, and finish are named
cancellation points.

Cancellation before mutation dispatch is a known typed rejection. Cancellation
after write or finish dispatch is indeterminate unless the provider can prove
exact progress. Nonmutating read cancellation leaves the caller buffer
unchanged. Provider loss without a replacement and restart with a nonzero
replacement generation remain distinct; no live handle retargets.

This is not ambient catchable interruption: source observes cancellation only
through an invoked capability operation and handles it as a nominal result. It
does not yet establish a general `Cancellationˉtoken`, polling operation,
deadline type, or task interaction. Workloads 5 and 6 must decide that shared
surface using asynchronous and concurrent evidence.

## Consequences

The bounded file-copy application becomes a draft-reviewed corpus row. Three of
eleven workloads are now draft reviewed and eight remain.

The byte-buffer calls and release/completion clarification become
normative-candidate Foundation/language contracts available to later workloads.
Later workloads may add operations but cannot silently expose uninitialized
memory, make `using` finish resources, replay uncertainty, or merge independent
authority. A contradiction requires a named reconsideration and coherent corpus
update.

The two filesystem roots remain accepted paper capability candidates rather
than published catalog entries. Provider implementation must begin from a
deterministic in-memory oracle and retain the existing portable filesystem
semantics as relevant evidence, but neither current Seed APIs nor host syscalls
define Language 1.0 behavior.

The decision adds no grammar production, implicit conversion, exception,
pointer, hidden allocation, file-specific WIR operation, or implementation
claim. Current compilers and libraries continue implementing Windvale Seed.

## Reconsideration triggers

Reconsider `Byteˉbuffer` only if another mandatory workload proves the fixed
zero-initialized value cannot support safe bounded I/O or decoding. Preserve
explicit ownership, initialization, checked slicing, and allocation accounting.

Reconsider release/completion composition only when a mandatory protocol must
attempt completion after body failure. Any replacement must retain both results
explicitly and keep local handle invalidation guaranteed.

Reconsider partial continuation if a provider cannot distinguish resumable short
acceptance from terminal known progress. Do not solve that by retrying an
indeterminate mutation.

Reconsider a filesystem operation signature when later workloads require an
owned directory/session, deadline, cancellation token, transaction, idempotency
identity, or recovery query. Preserve independent read and write authority and
version the interface rather than broadening it in place.

Reconsider the launcher-bound cancellation generation after workloads 5 and 6
define the general task/provider cancellation contract. The replacement must
keep pre-dispatch cancellation distinct from post-dispatch uncertainty and from
provider loss or restart.
