# Workload 2 rejected and boundary cases

## Compile-time ownership and effect rejection

### Copying a file handle

```text
let Alias = Source;
filesystem.copy.source.Readˉat(Source: borrow mut Source, ...);
```

Reject because assigning move-only `Sourceˉfile` transfers the only owner.
`Source` cannot be used afterward.

### Returning a buffer slice

```text
fn Leak(Buffer: borrow Bytes.Byteˉbuffer) -> Bytes.Slice<u8> {
    return Bytes.Borrowˉslice(Buffer: Buffer, Start: 0u64, Length: 1u64);
}
```

Reject because the returned borrow would outlive the borrowed buffer parameter.

### Mutating while an immutable slice is live

```text
let Value = Bytes.Borrowˉslice(
    Buffer: borrow Buffer,
    Start: 0u64,
    Length: 4u64,
);
filesystem.copy.source.Readˉat(
    Source: borrow mut Source,
    Position: 4u64,
    Target: Bytes.Borrowˉsliceˉmut(
        Buffer: borrow mut Buffer,
        Start: 0u64,
        Length: 4u64,
    ),
);
filesystem.copy.destination.Writeˉat(
    Destination: borrow mut Destination,
    Position: 0u64,
    Value: Value,
);
```

Reject because the immutable buffer borrow remains live when overlapping mutable
access is requested.

### Hiding a capability effect

```text
export fn Read(
    Source: borrow mut Filesystem.Sourceˉfile,
    Target: Bytes.Mutableˉslice<u8>,
) -> Filesystem.Readˉoutcome effects() {
    return filesystem.copy.source.Readˉat(
        Source: Source,
        Position: 0u64,
        Target: Target,
    );
}
```

Reject because the exported empty effect set omits `filesystem.copy.source`.

### Finishing through local release

```text
using Destination = try Openˉdestination(...) {
    try Copyˉbody(...);
}
return Success;
```

Reject the claimed success during review: leaving `using` invokes only local
release and provides no durable-finish evidence.

### Reusing an indeterminate suffix

```text
case Resource.Mutationˉoutcome.Indeterminate { Error: _ } {
    continue;
}
```

Reject because the next loop iteration would replay bytes whose visibility is
unknown.

### Native path or handle escape

```text
filesystem.copy.source.Openˉsnapshot(
    Name: borrow "C:\\data\\source.bin",
    ...
);
```

Reject name validation before provider dispatch. A capability name is one
semantic segment, not a host path.

## Configuration boundaries

| Case | Expected result before provider acquisition |
| --- | --- |
| maximum bytes = 0 | Valid; only an empty source can open successfully |
| maximum bytes = 1,048,576 | Valid exact maximum |
| maximum bytes = 1,048,577 | `Invalidˉconfiguration`, field 1 |
| chunk bytes = 0 | `Invalidˉconfiguration`, field 2 |
| chunk bytes = 1 | Valid; bounded by operation limit |
| chunk bytes = 65,536 | Valid exact maximum |
| chunk bytes = 65,537 | `Invalidˉconfiguration`, field 2 |
| operations = 0 | `Invalidˉconfiguration`, field 3 |
| operations = 1 | Valid; may later terminate before a nonempty copy completes |
| operations = 2,097,152 | Valid exact maximum |
| operations = 2,097,153 | `Invalidˉconfiguration`, field 3 |

Names are independently rejected for empty value, more than 255 canonical UTF-8
bytes, separators, NUL, colon, complete `.` or `..`, or any representation that
would require platform normalization to identify the bound object.

## Acquisition boundaries

| Case | Result and side effect |
| --- | --- |
| source missing | Source `Openˉrejected(Missing)`; no destination call |
| source wrong kind or link | Source `Openˉrejected(Wrongˉkind)` |
| source longer than maximum | Source `Openˉrejected(Invalidˉlimit)` with no exposed handle |
| source provider lacks stable snapshot | Source `Openˉrejected(Unsupported)` |
| destination already exists | Destination `Openˉrejected(Alreadyˉexists)`; existing object unchanged |
| destination permission denied | Destination `Openˉrejected(Permissionˉdenied)` |
| cancellation before either open dispatch | `Cancelled` at the exact open stage |
| provider unavailable without replacement | `Providerˉlost`, observed generation zero |
| provider restarted before acquisition completes | `Providerˉrestarted`, observed replacement generation nonzero |

Destination failure occurs inside the source `using` scope, so the acquired
source releases during `try` propagation.

## Read boundaries

| Case | Result |
| --- | --- |
| empty snapshot | No read call; proceed directly to empty destination finish |
| exact maximum snapshot | Copy exactly 1,048,576 bytes or hit the selected operation limit |
| short positive read | Advance source position by exact count and continue |
| completed zero before snapshot EOF | `Progressˉstalled(Read)`; no retry |
| count greater than target | `Invalidˉprogress`; no slice constructed from it |
| source grows, shrinks, or is replaced | `Sourceˉchanged` with both generations |
| cancellation | `Cancelled(Read)` and target unchanged |
| provider loss | `Providerˉlost(Read)` and target unchanged |
| provider restart | `Providerˉrestarted(Read)` and live handle never retargeted |
| rejected read after copied prefix | failure retains exact already copied bytes |

A malformed response claiming both rejection and buffer mutation is rejected at
the runtime boundary and cannot be represented as `Readˉoutcome`.

## Write boundaries

| Outcome | Required source behavior |
| --- | --- |
| `Rejected(Error)` | Record zero new progress and return mapped failure |
| positive `Acceptedˉpartial(Shortˉacceptance)` | Advance by exact prefix and continue with suffix only |
| positive partial with capacity exhaustion | Advance proved prefix, then return terminal failure |
| partial count = 0 | `Progressˉstalled(Write)` defense; provider contract violation |
| partial count >= request | `Invalidˉprogress` |
| `Completed` equal request | Advance complete request |
| `Completed` unequal request | `Invalidˉprogress` |
| `Indeterminate(Error)` | Return `Mutationˉindeterminate`; no write or finish retry |

If the destination fills after accepting three of eight requested bytes, the
failure reports the prior copied total plus three. Those three bytes are never
submitted again. The partial destination remains owned until local release and
may remain externally visible afterward.

## Work and arithmetic boundaries

- Operation admission is checked before every read and write.
- Reaching the maximum returns `Operationˉlimit` without another provider call.
- Provider counts are bounded before position addition.
- Buffer slice start plus length is checked before a borrow is formed.
- Destination position plus request cannot exceed the source length admitted at
  destination creation.
- No loop depends solely on provider willingness to make progress.

## Finish boundaries

| Case | Result |
| --- | --- |
| body fails | Finish not called; body result preserved |
| expected length differs from proved destination length | rejected before durability dispatch |
| complete content/length/name durability | Valid report |
| unsupported combined durability | `Finishˉrejected(Unsupported)` |
| cancellation before dispatch | `Cancelled(Finish)` |
| cancellation after dispatch | `Finishˉindeterminate(Cancelled)` |
| provider loss before dispatch | `Providerˉlost(Finish)` |
| provider loss after dispatch | `Finishˉindeterminate(Providerˉlost)` |
| provider restart after dispatch | `Finishˉindeterminate(Providerˉrestarted)` |
| completed length differs from expected | `Invalidˉprogress(Finish)` |

No finish failure is converted to success by release. No indeterminate finish is
retried. A rejected or uncertain finish does not claim the new object was
deleted or rolled back.

## Release boundaries

- Source releases if destination creation fails.
- Destination releases before source for every inner-scope exit.
- Buffer accounting releases after both file resources.
- Local release remains one consuming operation after provider loss or restart.
- Release cannot throw, allocate unbounded state, finish output, or replace the
  body/finish result.
- A stale copied handle cannot be released because no copy is legal.

## Determinism cases

Given the same configuration and provider transcript, every backend must produce
the same call positions, slice lengths, result case, counts, and release order.
Provider chunking may differ only when the transcript differs; it cannot change
the final bytes or permit an operation beyond the declared maximum.
