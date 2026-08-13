# Windvale database random-access page reads

## Status and scope

This contract defines the implemented portable page-planning and response-
admission core plus its hosted adapter over `storage.random_access_v1`.
The source libraries and their deterministic native Project 2 builds are
implemented. The Windvale-native x64 backend now lowers the required checked
`u64` subset. A focused hosted application executes the portable page core over
real Windows file input and packages the same image for Linux through the
bounded native snapshot bridge described below. The closed ABI-22 service
table still has no direct `storage.random_access_v1` slot, so this document does
not claim native mutation or native execution of that capability dispatch.

The implementation is split at the authority boundary:

- `Libraries/Database/Storage-Page.wv` is portable and owns checked geometry
  plus page-response invariants.
- `Libraries/Platform/Database/Random-Access-Page.wv` is hosted, declares
  `storage.random_access_v1`, and owns provider dispatch and storage-failure
  mapping.

This is format-neutral storage machinery for a future database reader. It does
not define a durable page format, cache, mutation, transaction, write-ahead
log, recovery protocol, path API, or ambient filesystem access.

## Page plan

`Databaseˉstorageˉpageˉprepare` accepts a header size, page size, zero-based
page identifier, provider generation, and described storage length. It returns
either a complete immutable plan or one typed error.

The page starts at:

```text
offset = header_size + page_identifier * page_size
end_exclusive = offset + page_size
```

Both operations use checked `u64` arithmetic. Page size must be nonzero, must
not exceed the `storage.random_access_v1` 65,536-byte transfer limit, and the
complete half-open range must fit within the described storage length. A valid
plan retains the generation and storage length used to derive it.

Preparation failures are `Invalidˉpageˉsize`, `Transferˉlimit`,
`Arithmeticˉoverflow`, or `Outsideˉstorage`.

## Response admission

`Databaseˉstorageˉpageˉaccept` admits read bytes only when all of these facts
match the plan:

- the provider generation is unchanged;
- the reported storage length is unchanged;
- the echoed position equals the planned offset; and
- the payload length equals the planned page size exactly.

The corresponding failures are `Staleˉgeneration`, `Changedˉstorage`,
`Invalidˉposition`, and `Invalidˉlength`. Success returns the admitted bytes
with their generation, storage length, page identity, offset, and exclusive
end. The core never retries or performs I/O.

## Hosted adapter

`Databaseˉstorageˉreadˉpage(Headerˉsize, Pageˉsize, Pageˉidentifier)` performs
one bounded sequence against the single pre-bound storage instance:

1. call `Storageˉdescribe`;
2. prepare the page plan from that exact description;
3. call `Storageˉreadˉat` with the plan's generation, offset, and length; and
4. admit the response through the portable core.

The public result separates `Valid`, `Pageˉfailure`, and `Storageˉfailure`.
Storage failures preserve permission, availability, revocation, stale-provider,
peer-exit, outside-storage, unsupported, invalid-request, and invalid-response
classes. A provider status is never silently reclassified as a valid but empty
page, and the adapter does not retry a changed or stale read.

The root module must redeclare `storage.random_access_v1`; declaration is only
static approval. A launcher or service manager must still bind and authorize
one rights-limited provider instance.

## Native hosted snapshot bridge

`Nativeˉhostedˉsnapshotˉreadˉpage(Resource, Headerˉsize, Pageˉsize,
Pageˉidentifier)` is the first normal native execution provider for this core.
It reads one immutable snapshot through ABI 22's existing `file.read_bytes`
leaf, describes that snapshot with generation `1`, derives the page with
checked `u64` planning, slices only after the plan proves the range fits the
native snapshot, and admits the exact bytes through
`Databaseˉstorageˉpageˉaccept`.

The bridge deliberately accepts `u32` header and page identities because the
native file snapshot is already bounded to 4 MiB; it widens those values before
planning. The host-tool process argument supplies the resource binding. Source
never receives a file handle or descriptor, but this transition profile does
expose the opaque resource name to its root application and native file-service
failure remains a runtime service failure. It is therefore not a semantic
replacement for the pre-opened capability and is not suitable for mutable or
long-running database storage.

## Verification contract

The native library owner builds the portable core, hosted adapter, and an
importing application through the Project 2 source-to-WVB front door. Focused
conformance builds cover:

- a valid page plan at page zero;
- zero and above-limit page sizes;
- multiplication/addition overflow and outside-storage ranges;
- successful response admission; and
- stale generation, changed length, wrong position, and short payload.

The import smoke test calls the hosted facade and binds its nominal result,
proving that the adapter remains a composable library rather than only a valid
root module. The `native-u64-lowering` owner reconstructs the current lowerer,
requires the exact 8,032-byte wide-scalar WVO, lowers the snapshot-page
application to an exact 74,228-byte WVO, packages exact Windows and Linux
hosted applications, and executes the current-host application against a real
17-byte file to result 42. Independent execution on the other host remains a
qualification requirement.

## Next contracts

The next platform milestone should version and widen the closed native service
table so `storage.random_access_v1` can bind one pre-opened object directly,
return provider failures as values, and avoid whole-file snapshots. That is an
ABI/service-contract change, not an alias for `file.read_bytes`.

The next database layer may validate one admitted page's format and checksum.
It must remain separate from provider dispatch so malformed storage bytes can
be tested without filesystem authority. Mutation must wait for a separately
specified commit/recovery protocol that treats partial and indeterminate
completion as first-class outcomes.
