# Workload 2 semantic review

## Review status

Draft-reviewed under
[Decision 0756](../../../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md).
The source is coherent under the candidate grammar and the paper filesystem
contract. Its general byte-buffer and resource-completion rules are accepted.
The two capability signature-set identities remain paper contracts pending
later filesystem, transaction, and concurrent-service pressure.

## Module metadata

| Module | Profile | Authority | Platforms | Required capabilities |
| --- | --- | --- | --- | --- |
| `Copyˉtypes` | Core | Library | Windows, Linux, Windvale | None |
| `Copyˉvalidate` | Core | Library | Windows, Linux, Windvale | None |
| `Copyˉengine` | Hosted | Library | Windows, Linux, Windvale | source and destination roots |
| `Copyˉapplication` | Hosted | Application | Windows, Linux, Windvale | source and destination roots |

No module uses unsafe, foreign declarations, target-specific branches, optional
capabilities, dynamic loading, or a reverse Hosted-to-Core import.

## Values, ownership, and borrows

| Value | Class and owner | Movement or borrowing | Terminal state |
| --- | --- | --- | --- |
| Configuration | Shared immutable record containing two shared texts and three `u64` limits | Passed to entry; borrowed by validation; fields borrowed by acquisition | Released with application domain |
| Root budget | Move-owned accounting domain | Transferred by launcher, mutably borrowed once for split | Released after buffer and handles |
| Buffer budget | Move-owned child budget | Consumed by `Constructˉbuffer` | Accounting transferred to buffer or locally released on failure |
| Byte buffer | Move-owned, zero-initialized, fixed length | Exclusively borrowed for each read; immutably borrowed for each write slice | Released on every entry exit |
| Source file | Move-only resource | Acquired once; held by outer `using`; mutably borrowed per read | Locally released exactly once |
| Destination file | Move-only resource | Acquired once; held by inner `using`; mutably borrowed per write and finish | Locally released exactly once |
| Mutable target slice | Exclusive nonescaping buffer borrow | Exists for one `Readˉat` call | Ends before immutable write slice exists |
| Immutable write slice | Nonescaping buffer borrow | Exists for one `Writeˉat` call | Ends before buffer can be mutably borrowed again |
| Report | Copy record of five bounded `u64` values | Produced only after the transfer body succeeds | Returned after finish succeeds |
| Failure | Closed nominal variant with bounded fields | Propagated through exact `Result` | Retained by launcher completion record |

There is never a simultaneous mutable and immutable borrow of the byte buffer.
Provider calls cannot retain either slice. File handles cannot escape their
lexical `using` scopes or be copied into the result.

## Visible mutation

Mutation is confined to:

1. root-budget accounting during one child split;
2. the fixed byte buffer prefix written by each completed read;
3. five `u64` loop/progress counters;
4. provider-private source-handle observation state;
5. the destination bytes and logical length through explicit positioned writes;
   and
6. the explicit durable finish transition.

No source variable aliases destination state. Read failure leaves the buffer
unchanged. A completed read changes only its exact prefix. A rejected write
changes no destination bytes; partial progress names its exact accepted prefix;
indeterminate progress is never guessed.

## Effect closure

| Boundary | Exact effects | Reason |
| --- | --- | --- |
| Configuration validation | Empty | Pure comparisons of bounded values |
| Budget split | `memory.allocate`, `resource.acquire` | Commit one rights-reduced accounting child |
| Byte-buffer construction | `memory.allocate` | Allocate and zero the fixed caller-owned buffer |
| Source acquisition/read | `filesystem.copy.source`, plus `resource.acquire` for open | Use only the approved read root and owned source instance |
| Destination acquisition/write | `filesystem.copy.destination`, plus `resource.acquire` for create | Use only the approved create/write root and owned destination instance |
| Durable finish | `filesystem.copy.destination`, `resource.complete` | Make semantic completion visible and fallible |
| Lexical release | `resource.release` and the corresponding provider effect | Invalidate each owned local resource and return capacity |

The exported entry lists the exact union. Core modules remain capability-free.
Importing a module does not bind or acquire anything.

## Transfer invariants

At every loop boundary:

- `0 <= Copiedˉbytes <= Readˉposition <= Sourceˉbytes`;
- no byte at a destination position below `Copiedˉbytes` is submitted again;
- the live buffer content from zero through `Readˉbytes` is the most recent
  completed source prefix at `Readˉposition - Readˉbytes`;
- `Chunkˉwritten <= Readˉbytes <= Chunkˉbytes`;
- `Operations == Readˉcalls + Writeˉcalls`; and
- `Operations <= Maximumˉoperations` before every provider call.

The source maximum of 1,048,576 and operation maximum of 2,097,152 make all
counter increments safe after their preceding comparisons. Provider-reported
counts are checked against the current request before any addition or new slice.

## Read progress

Each read targets the smaller of remaining source bytes and the configured chunk
size. Positive short reads advance the explicit source position and are valid.
A zero completion before known snapshot EOF returns `Progressˉstalled`; it never
spins. A count larger than the target returns `Invalidˉprogress` without creating
an out-of-range slice.

Rejection returns the source position and already proved copied length. A source
change, cancellation, loss, and restart preserve separate top-level failure
cases. No source read exposes an implicit cursor.

## Write progress

A completed write must equal the entire supplied slice. A positive partial write
first advances by the proven prefix. Only `Shortˉacceptance` continues with the
unaccepted suffix; every other reason terminates with the new proved total. This
is continuation after exact progress, not replay.

Rejected means zero progress. Indeterminate means the current suffix may have
changed the destination but no additional count is authoritative; source returns
immediately, attempts neither another write nor finish, and locally releases the
handle.

## Completion versus release

`using` owns local release only. It never calls `Finishˉdurable`. Source calls
finish exactly once after `Engine.Copy` returns a valid report. The cases are:

- body failure: return that failure, skip finish, release both handles;
- body success plus finish completion: return the report, release both handles;
- body success plus finish rejection: return `Finishˉrejected`, release both;
- body success plus uncertain finish: return `Finishˉindeterminate`, do not
  retry, release both.

Because finish is conditional on body success, there is no path with two
competing body and finish failures. Neither can be overwritten by implicit
release. The partial destination may remain after any post-create failure; this
contract has no deletion or rollback authority.

## Early-propagation cleanup walkthrough

### Invalid configuration

No allocation or provider call occurs. The transferred root budget releases when
entry returns.

### Buffer split or construction failure

No provider resource exists. Any consumed child accounting is released locally,
then the root releases.

### Source acquisition failure

The buffer and root release. No destination operation occurs.

### Destination acquisition failure

The source `using` scope releases the source during `try` propagation. The
buffer and root then release. An existing destination is never truncated because
creation is exclusive.

### Read or write body failure

The inner destination scope releases first, then the source, buffer, and root.
Finish is not called. Known copied progress remains in the failure record;
indeterminate progress is explicitly marked.

### Finish failure

The body report remains available until `Mapˉfinish` constructs a failure with
the exact copied length. Destination release follows without replacing that
result, then source, buffer, and root release.

### Success

Finish proves exact content, length, and created-name durability. Destination and
source release in reverse acquisition order. The report contains no handle,
borrow, native identity, or mutable state.

## Cancellation and provider lifecycle

The launcher profile supplies a named cancellation generation to both roots.
Each provider call is a cancellation point. A known pre-dispatch cancellation is
`Cancelled`; a cancellation after mutation dispatch is an indeterminate mutation
whose reason remains `Cancelled`.

Loss without a replacement and restart with a nonzero replacement generation are
different cases. No live handle retargets to the replacement. Local release is
still infallible. This workload starts no task, owns no timer, and makes no claim
about general source-visible cancellation tokens.

## Terminal traps

Expected failures are typed. A terminal trap is reserved for:

- malformed nominal values that bypass required runtime validation;
- a provider retaining or writing outside a borrowed slice;
- impossible ownership/borrow evidence;
- arithmetic after a violated checked invariant; or
- a runtime returning a capability value that cannot inhabit the declared
  result type.

The source independently turns validly shaped but unusable zero or excessive
progress into typed failures, providing bounded defense even before a runtime
hardens that provider check.

## Common corpus review answers

- Every acquisition, move, borrow, mutation, completion, and release is visible.
- Every growing or externally retained value has a maximum before provider work.
- Source and destination authority are independently approved and bound.
- `try` propagation releases all live resources in reverse lexical order.
- Partial progress advances only by an authoritative count.
- Indeterminate mutation is never retried or converted to rejection.
- Cancellation, source change, provider loss, and provider restart are distinct.
- Windows, Linux, and Windvale use identical source semantics; providers map
  their native behavior to the same contract.
- No new grammar production, WIR family, pointer, or unsafe operation is needed.
- The workload exposes one general Foundation byte-buffer signature group and
  one clarification of explicit completion versus local release.
