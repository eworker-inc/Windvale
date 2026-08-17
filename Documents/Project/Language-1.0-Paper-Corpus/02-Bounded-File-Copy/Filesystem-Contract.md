# Workload 2 filesystem contract

## Status

This is the draft-reviewed paper contract selected by workload 2 and accepted
under
[Decision 0756](../../../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md).
It is not a published capability catalog, runtime ABI, host-path interface, or
implementation claim. Its two capability signature-set identities remain
provisional until later database and service workloads confirm the shared
filesystem/resource boundary.

## Authority split

The application requires two independent version-1 capability roots:

- `filesystem.copy.source` can acquire a read-only immutable source snapshot
  from one launcher-bound directory instance; and
- `filesystem.copy.destination` can create one new destination and mutate only
  that owned instance within its admitted maximum.

Neither root grants the other. Neither grants enumeration, ambient current
directory, absolute paths, traversal, links, deletion, rename, replacement,
metadata mutation, mapping, native handles, device access, or terminal I/O. The
launcher may bind the roots to the same semantic directory only when policy
allows it; source cannot discover that relationship.

Names are strict UTF-8 semantic single segments of 1 through 255 bytes. `/`,
`\`, NUL, `:`, `.` and `..` as complete names, native drive/device prefixes,
and platform normalization aliases reject before provider dispatch. Providers
compare the admitted canonical name bytes ordinally and never reinterpret a name
as a host path.

## Common values

`Platformˉfilesystem` supplies these exact paper values:

```text
export enum Ioˉfailureˉkind: u8 {
    Missing = 1u8;
    Alreadyˉexists = 2u8;
    Wrongˉkind = 3u8;
    Permissionˉdenied = 4u8;
    Unavailable = 5u8;
    Revoked = 6u8;
    Unsupported = 7u8;
    Invalidˉname = 8u8;
    Invalidˉlimit = 9u8;
    Invalidˉposition = 10u8;
    Invalidˉresponse = 11u8;
    Capacityˉexhausted = 12u8;
    Shortˉacceptance = 13u8;
    Cancelled = 14u8;
    Providerˉlost = 15u8;
    Providerˉrestarted = 16u8;
    Sourceˉchanged = 17u8;
}

export record Ioˉfailure {
    Kind: Ioˉfailureˉkind;
    Expectedˉgeneration: u64;
    Observedˉgeneration: u64;
}

export variant Readˉoutcome {
    Rejected(Error: Ioˉfailure);
    Completed(Completed: u64);
}

export variant Finishˉoutcome {
    Rejected(Error: Ioˉfailure);
    Completed(Length: u64);
    Indeterminate(Error: Ioˉfailure);
}
```

Generation zero means no replacement generation was observed. Failure kinds
whose meaning does not use either generation field require both fields to be
zero. `Invalidˉresponse` is a defense-in-depth library result for a runtime that
failed to enforce the provider boundary; a conforming runtime rejects malformed
provider values before source receives them.

The common record does not make every kind legal for every operation:

| Operation | Admitted ordinary kinds |
| --- | --- |
| Source open | `Missing`, `Wrongˉkind`, `Permissionˉdenied`, `Unavailable`, `Revoked`, `Unsupported`, `Invalidˉname`, `Invalidˉlimit`, `Cancelled`, `Providerˉlost`, `Providerˉrestarted` |
| Destination create | `Alreadyˉexists`, `Wrongˉkind`, `Permissionˉdenied`, `Unavailable`, `Revoked`, `Unsupported`, `Invalidˉname`, `Invalidˉlimit`, `Cancelled`, `Providerˉlost`, `Providerˉrestarted` |
| Read | `Permissionˉdenied`, `Unavailable`, `Revoked`, `Unsupported`, `Invalidˉposition`, `Cancelled`, `Providerˉlost`, `Providerˉrestarted`, `Sourceˉchanged` |
| Write | `Permissionˉdenied`, `Unavailable`, `Revoked`, `Unsupported`, `Invalidˉposition`, `Capacityˉexhausted`, `Shortˉacceptance`, `Cancelled`, `Providerˉlost`, `Providerˉrestarted` |
| Finish | `Permissionˉdenied`, `Unavailable`, `Revoked`, `Unsupported`, `Invalidˉposition`, `Cancelled`, `Providerˉlost`, `Providerˉrestarted` |

The runtime rejects an operation/kind combination outside this table as a
malformed provider result. `Invalidˉresponse` may be constructed only by the
defense-in-depth library after such bytes bypass that boundary.

## Source acquisition

The exact source-root call is:

```text
filesystem.copy.source.Openˉsnapshot(
    Name: borrow text,
    Maximumˉlength: u64,
    Maximumˉreadˉbytes: u64,
    Maximumˉoperations: u64,
) -> Result<Sourceˉfile, Ioˉfailure>
    effects(filesystem.copy.source, resource.acquire)
```

`Maximumˉlength` is at most 1,048,576. `Maximumˉreadˉbytes` is 1 through
65,536. `Maximumˉoperations` is 1 through 2,097,152. The provider validates all
limits before acquiring a handle or exposing source metadata.

Success returns one move-only `Sourceˉfile` recording:

- exact canonical name identity;
- exact length not exceeding `Maximumˉlength`;
- nonzero provider and content generations;
- the three admitted limits; and
- a read-only snapshot guarantee for its resource lifetime.

The provider may satisfy the snapshot guarantee by immutable storage, bounded
copying charged outside source memory, a writer-exclusion lease, or an equivalent
unobservable strategy. If it cannot preserve or attest the original content and
length, it rejects `Unsupported`. A later detected source mutation, including
growth, shrink, or replacement, returns `Sourceˉchanged` with expected and
observed content generations before returning bytes.

These observations are total local descriptor reads:

```text
export fn Sourceˉlength(
    Source: borrow Sourceˉfile,
) -> u64 effects();

export fn Sourceˉgeneration(
    Source: borrow Sourceˉfile,
) -> u64 effects();
```

## Bounded reads

The read call is:

```text
filesystem.copy.source.Readˉat(
    Source: borrow mut Sourceˉfile,
    Position: u64,
    Target: Mutableˉslice<u8>,
) -> Readˉoutcome effects(filesystem.copy.source)
```

The mutable slice is a nonescaping exclusive borrow into caller-owned memory.
Its length is the request maximum and cannot exceed the source handle's admitted
read maximum. The provider cannot retain the borrow after return.

A completed read writes exactly `Completed` bytes into the target prefix and
leaves the remaining target bytes unchanged. It reports a value no greater than
the target length or snapshot bytes remaining. Short positive completion is
valid and the caller may issue the next read at the advanced explicit position.
At snapshot EOF a zero completion is valid. When position is before EOF and the
target is nonempty, completion must be positive. Rejection leaves the entire
target unchanged and reports no progress.

Every addition and range check uses checked `u64` arithmetic before provider
dispatch. Position beyond snapshot length returns `Invalidˉposition`. There is
no implicit cursor, read-ahead identity, or host buffer exposure.

## Destination acquisition

The exact destination-root call is:

```text
filesystem.copy.destination.Createˉexclusive(
    Name: borrow text,
    Maximumˉlength: u64,
    Maximumˉwriteˉbytes: u64,
    Maximumˉoperations: u64,
) -> Result<Destinationˉfile, Ioˉfailure>
    effects(filesystem.copy.destination, resource.acquire)
```

The call creates one new regular destination and rejects `Alreadyˉexists`
without opening or truncating an existing object. It never follows links. The
limits have the same ranges as source acquisition. Success returns one move-only
`Destinationˉfile` with a nonzero provider generation, zero initial logical
length, and no implicit durability claim.

The destination may become externally visible at acquisition. A later body,
finish, or release failure can therefore leave a partial new object. This
version provides no hidden deletion or rollback and never replaces an existing
name.

## Positioned writes and partial progress

The exact write call is:

```text
filesystem.copy.destination.Writeˉat(
    Destination: borrow mut Destinationˉfile,
    Position: u64,
    Value: Slice<u8>,
) -> Mutationˉoutcome<Ioˉfailure>
    effects(filesystem.copy.destination)
```

The immutable slice is a nonescaping borrow and cannot exceed the handle's
admitted write maximum. Position plus length is checked and cannot exceed the
destination maximum.

- `Rejected` proves zero bytes accepted.
- `Acceptedˉpartial` proves an exact positive prefix smaller than the request.
- `Completed` must equal the complete request length.
- `Indeterminate` proves no replayable progress and must not be retried.

`Shortˉacceptance` is the only partial reason for which workload 2 continues.
It advances both buffer and destination positions by the proven count and sends
only the unaccepted suffix. That continuation is not a retry of accepted bytes.
Any other partial reason terminates copying after recording the exact accepted
prefix. `Capacityˉexhausted` may reject at zero or terminate after an exact
partial prefix. A zero-byte value may complete with zero, although this workload
never issues one.

## Durable finish

The exact completion call is:

```text
filesystem.copy.destination.Finishˉdurable(
    Destination: borrow mut Destinationˉfile,
    Expectedˉlength: u64,
) -> Finishˉoutcome
    effects(filesystem.copy.destination, resource.complete)
```

The call first requires the destination's proved logical length to equal
`Expectedˉlength`. A completed result proves, within the provider's admitted
stable-storage model, durability of:

- all completed content writes;
- the exact logical length; and
- the newly created name in the bound destination directory.

It returns that exact length. A provider that cannot offer this combined
content/length/name guarantee rejects `Unsupported` before dispatch. Rejection
does not claim the already written destination disappeared. `Indeterminate`
means the requested durability transition may or may not have completed and is
never retried by this workload.

Finish is attempted exactly once and only after the complete copy body succeeds.
A body failure is returned directly and finish is not attempted. A successful
body followed by rejected or indeterminate finish returns that finish outcome.
Thus neither result is hidden or overwritten by automatic cleanup.

## Cancellation, restart, and loss

The named launcher profile binds one cancellation generation into both provider
roots. Acquisition, read, write, and finish are cancellation points. This
synchronous workload does not acquire a task or poll ambient process state.

Cancellation before mutation dispatch is a known `Cancelled` rejection.
Cancellation after a write or finish was dispatched returns `Indeterminate` with
kind `Cancelled` unless the provider can prove exact progress. Nonmutating read
cancellation rejects with an unchanged target.

`Providerˉlost` means the expected generation became unavailable without an
observed replacement. `Providerˉrestarted` carries the distinct nonzero
replacement generation. Neither retargets a live handle. A dispatched write or
finish that loses its provider returns indeterminate; a read or pre-dispatch
mutation returns the known failure.

Later concurrent workloads own the decision whether general Language 1.0 needs
a source-visible cancellation-token interface. This workload accepts only the
explicit launcher/provider cancellation contract above.

## Local release

Both file types implement `Foundationˉresource.Localˉrelease<Self>`. Release:

- consumes and invalidates the one local handle;
- runs on every ordinary `using` exit, including `try` propagation and return;
- returns local handle-table and resource-domain capacity;
- never performs finish, durability, deletion, rollback, or retry; and
- remains locally infallible when the provider is lost or restarted.

Provider-visible release retains the corresponding filesystem capability effect
in addition to `resource.release`. A runtime may perform bounded best-effort
remote notification, but source observes no new semantic result from release.

## Deliberately excluded

This contract does not define replacement, atomic rename, delete-on-failure,
recursive paths, links, metadata copying, permissions, timestamps, sparse files,
mapping, append, concurrent writers, resumable mutation identities, checksums,
encryption, compression, directory enumeration, or a general stream API.
