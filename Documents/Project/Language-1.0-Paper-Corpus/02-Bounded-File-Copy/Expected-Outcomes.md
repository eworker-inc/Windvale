# Workload 2 expected semantic outcomes

## Observation rule

These are semantic provider/application transcripts, not host syscall traces.
Native handles, path spellings, scheduler events, caches, and filesystem-specific
error numbers are outside the observation.

## Empty file

Configuration uses maximum 0, chunk 4, and operation maximum 1. Source open
reports length 0. The exact transcript is:

```text
open-source -> valid(length=0)
create-destination(maximum-length=0) -> valid
finish-durable(expected-length=0) -> completed(length=0)
release-destination
release-source
result -> valid(source=0, copied=0, reads=0, writes=0, operations=0)
```

No read or write occurs. The destination name, zero length, and empty content are
durable under the admitted provider model.

## Complete ten-byte copy with four-byte chunks

Source bytes are the ten ASCII/UTF-8 bytes for `Windvale!` followed by LF.

```text
read-at(0, max=4) -> completed(4)
write-at(0, length=4) -> completed(4)
read-at(4, max=4) -> completed(4)
write-at(4, length=4) -> completed(4)
read-at(8, max=2) -> completed(2)
write-at(8, length=2) -> completed(2)
finish-durable(10) -> completed(10)
```

The result is `Valid` with source/copy bytes 10, three reads, three writes, and
six total operations. Destination bytes equal source bytes exactly.

## Short positive read and write

For a six-byte source, chunk maximum 4:

```text
read-at(0, max=4) -> completed(3)
write-at(0, length=3) -> partial(1, Short_acceptance)
write-at(1, length=2) -> completed(2)
read-at(3, max=3) -> completed(3)
write-at(3, length=3) -> completed(3)
finish-durable(6) -> completed(6)
```

The result reports two reads, three writes, five operations, and six copied
bytes. Destination position 0 is never submitted after its one accepted byte.

## Exact maximum

A 1,048,576-byte snapshot with 65,536-byte chunks and complete provider calls
requires 16 reads, 16 writes, 32 operations, and one finish. The result reports
exactly 1,048,576 source and copied bytes. One additional source byte causes open
rejection before destination creation.

## Source growth

After source acquisition records content generation 41 and length 8, an external
writer causes observed content generation 42 before the second read:

```text
read-at(0, max=4) -> completed(4)
write-at(0, length=4) -> completed(4)
read-at(4, max=4) -> rejected(Source_changed, expected=41, observed=42)
release-destination
release-source
```

The result is `Sourceˉchanged(position=4, copied=4)`. Finish is not attempted.

## Destination full after partial progress

The destination accepts two of four requested bytes, then proves capacity
exhaustion:

```text
write-at(position=8, length=4)
  -> accepted-partial(completed=2, Capacity_exhausted)
```

The terminal failure records position/copied bytes 10 and capacity exhaustion.
The two proved bytes are not replayed. Finish is not attempted.

## Zero progress

A read before snapshot EOF returns `Completed(0)`. Source returns
`Progressˉstalled(Read)` after one provider call. It performs no write or
finish, then releases destination and source. It cannot loop indefinitely.

## Operation limit

With a nonempty source and maximum operations 1, one read completes. Before the
first write, source returns `Operationˉlimit(completed=1, maximum=1, copied=0)`.
The buffer and destination contain no proved copied byte. Finish is not called.

## Indeterminate write

After eight proved copied bytes, the next write returns indeterminate provider
loss. The result is:

```text
Mutationˉindeterminate(
    position=8,
    copied-bytes=8,
    reason=Provider_lost,
    expected-generation=<expected>,
    observed-generation=0
)
```

The current suffix may or may not be visible. Source performs no further write,
finish, alternate-provider call, or automatic retry.

## Finish rejection

All bytes copy successfully, but the provider rejects combined durability as
unsupported. The result is `Finishˉrejected` with the exact copied length.
Release follows. The application does not claim the partial or completed file is
durable or deleted.

## Indeterminate finish

All bytes copy successfully and finish dispatches, then the provider restarts.
The result is `Finishˉindeterminate` with reason `Providerˉrestarted`, copied
length, and both expected and observed replacement generations. No second finish
occurs.

## Early `try` propagation

Destination creation rejects `Alreadyˉexists`. The source handle releases before
the buffer and root budget. The existing destination receives no write, truncate,
finish, or release-as-finish operation. The result retains the destination-open
stage and exact reason.
