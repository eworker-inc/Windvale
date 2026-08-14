# Random-access storage capability

- Status: Semantic and portable-library contract implemented; focused native Windows provider execution and restart recovery implemented candidate; independent Linux execution and ordinary configurable binding pending
- Capability identity: `storage.random_access_v1`
- Source library: `Libraries/Platform/Storage/Random-Access-Storage.wv`
- Native binding contract: [`WVPT 1`](Windvale-Native-Capability-Provider-Table.md)
- Historical Stage 0 oracle: `Runtime/Windvale.Runtime/Random-Access-Storage.cs`

## Purpose and boundary

This capability binds one pre-opened mutable storage object. It is the first
hosted storage boundary suitable for a future Windvale Database page file. It
is not a native path API, directory capability, general filesystem handle,
database format, transaction interface, or write-ahead log.

Windvale source never receives the native path, file descriptor, Windows
handle, or provider object. The root module declares the capability, the
launcher separately authorizes it, and the launcher binds exactly one existing
ordinary file. A future Windvale OS provider may implement the same semantic
contract through a service endpoint instead of an in-process file.

The version suffix is the current WVB compatibility encoding. A later metadata
format may carry interface identity and major version separately without
changing version 1 semantics.

## Source signature

The current capability catalog declares:

```text
storage.random_access_v1(
    Operation: u32,
    Generation: u64,
    Position: u64,
    Control: u32,
    Value: bytes
) -> bytes
```

Applications use the typed `Randomˉaccessˉstorage` platform library rather
than dispatching operation numbers or decoding response bytes themselves. One
capability identity covers all operations so the generation and writer fence
refer to the same bound object.

## Limits

- Positions and storage lengths are unsigned 64-bit values.
- A read maximum or write value is at most 65,536 bytes.
- Every position-plus-length calculation is checked before provider dispatch.
- A provider may return `Unsupported` before mutation when its backing store
  cannot represent a valid `u64` request. The frozen Stage 0 oracle was limited
  by its signed 64-bit native file positions.
- Version 1 binds one storage instance per runtime context.

## Operations

| Number | Typed library operation | Request fields | Successful result |
| ---: | --- | --- | --- |
| 0 | `Storageˉdescribe()` | generation, position, control, and value are zero/empty | Nonzero provider generation and current `u64` length. |
| 1 | `Storageˉreadˉat(Generation, Position, Maximum)` | control is the bounded maximum; value is empty | Exactly `min(Maximum, Length - Position)` bytes. Position equal to length returns an empty success; position beyond length returns `Outsideˉstorage`. |
| 2 | `Storageˉwriteˉat(Generation, Position, Value)` | control is zero | No response payload and an explicit completed, exact-partial, or indeterminate mutation outcome. A positioned write may extend the object. |
| 3 | `Storageˉresize(Generation, Length)` | position carries the requested length; control and value are empty | Completed with exact resulting length, or indeterminate. |
| 4 | `Storageˉflush(Generation, Class)` | position and value are empty; control selects the flush class | Completed with the current length, or indeterminate. |

There is no implicit cursor. Zero-length reads and writes are valid. Version 1
does not expose append, create, rename, atomic replacement, directory
publication, sparse-allocation control, mapping, transactions, or close as a
source operation. Runtime teardown closes the bound provider.

## Generation and lifetime

`Describe` returns a nonzero generation. Every later operation supplies that
exact value. A different generation returns `Stale` and the provider's current
nonzero generation without performing I/O. `Revoked`, `Peerˉexited`, and
`Unavailable` remain distinct typed outcomes.

Generation is a fencing value within the bound provider lifetime. The historical
Stage 0 launcher bound one object for one process run, held one whole-file writer
lease, used generation `1`, and disposed the binding after execution. It did not
claim a persistent cross-restart mutation identity. A restartable service must
allocate a new generation before it can implement this contract.

The frozen Windows/Linux reference adapter opened the file for read/write, permitted
other readers, requests a whole-file lock, and retains the handle through
runtime teardown. Linux file locks are advisory against non-cooperating native
processes; therefore version 1's Stage 0 binding is a Windvale/provider writer
fence, not a claim that arbitrary host programs cannot modify the file. A
production database deployment must control the backing object and exclude
non-cooperating writers.

The focused native provider binds one fixed `Windvale-Database-Storage.bin`
object in the process working directory before calling Windvale `Main`.
Windows denies other writers and deleters while permitting readers. Linux uses
one nonblocking exclusive `flock`, which remains advisory against
non-cooperating native programs. The fixed name is test-shell policy, not a
source path API or the eventual configurable database-server launcher.

The focused owner has two evidence boundaries. Its no-argument form is the
clean nine-case owner and ignores local caches. `--development` is an explicit
non-qualification lane that uses the target-scoped [native tool
checkpoint](Windvale-Native-Tool-Checkpoint.md), assembles and verifies both
platform leaves, and executes the Windows create, tail-repair, and stable-reopen
lifecycle. `--prepare-development-tools` prepares or validates that checkpoint
without running the database case.

## Mutation outcomes

Mutation results use one of four completion values:

- `None`: no mutation was dispatched; used by reads and rejected operations.
- `Completed`: the requested mutation completed. Write progress equals the
  request length; resize length equals the requested length.
- `Partial`: a write accepted an exact positive prefix smaller than the request.
  Progress is authoritative. Resize and flush cannot return partial.
- `Indeterminate`: the provider cannot prove whether or how much mutation
  reached the backing object. Progress and reported length are zero because
  neither is authoritative.

`Permissionˉdenied`, `Unavailable`, `Revoked`, `Stale`, `Peerˉexited`, and
`Unsupported` mean the mutation was rejected before change. Once a provider
dispatches a mutation and then loses certainty, it must return `Indeterminate`
instead of one of those rejection statuses. Callers must not automatically
retry an indeterminate mutation.

Version 1 does not yet carry a durable mutation identity or query-by-mutation
operation. A database must recover its logical state from checksummed on-disk
records after an indeterminate result.

## Flush classes

`Content` requests durability for completed content writes according to the
provider's admitted stable-storage failure model. It does not assert that a
new logical length or native directory entry survives failure.

`Contentˉandˉlength` requests durability for completed content writes and the
storage object's logical length. This is the class required after extending or
resizing a single-file database object.

Neither class publishes a path, flushes a parent directory, provides atomic
replacement, or defines behavior for hardware that falsely acknowledges
stable writes. Those guarantees require separate provider evidence and a
  separate capability. The historical Stage 0 Windows/Linux adapter implemented
  both classes with a stable-file flush over an already-open existing ordinary
  file; it made no directory-publication claim and is not the forward provider.

## Response envelope

The capability returns one little-endian response of 48 through 65,584 bytes:

| Offset | Width | Field |
| ---: | ---: | --- |
| 0 | 4 | Magic `WVSA` (`0x41535657`) |
| 4 | 4 | Version `1` |
| 8 | 4 | Echoed operation |
| 12 | 4 | Status |
| 16 | 8 | Generation |
| 24 | 8 | Storage length |
| 32 | 8 | Echoed position |
| 40 | 4 | Exact progress |
| 44 | 4 | Completion |
| 48 | variable | Read payload only |

Statuses are `Valid` (0), `Permissionˉdenied` (1), `Unavailable` (2),
`Revoked` (3), `Stale` (4), `Peerˉexited` (5), `Outsideˉstorage` (6),
`Unsupported` (7), and runtime-owned `Invalidˉrequest` (8). The Windvale
decoder adds `Invalidˉresponse` (9), which is never a provider status.

The runtime rejects invalid requests before provider dispatch and constructs
status 8 itself. It validates every provider result before serialization and
validates the complete envelope before returning it to source. A provider
exception, uninitialized value, inconsistent length/progress, invalid status,
wrong generation, malformed completion, or payload mismatch traps as
`WVR3031`. The Windvale library independently validates the envelope and
returns `Invalidˉresponse` if bytes reach it through another host.

## Historical Stage 0 launcher binding

The retired reference launcher syntax was:

```text
windvale run Application.wvb \
  --allow storage.random_access_v1 \
  --bind-random-access-storage Database.wvdb
```

Binding does not grant authority, and authorization does not select a native
path. Both are required. The binding accepts an existing ordinary non-link,
non-device file. It does not create a missing file or follow an admitted
reparse-point binding.

## Focused native provider

`Runtime/Native/X64-Random-Access-Storage-Host.wva` derives context 9, constructs
the exact one-entry `WVPT 1` table, revalidates all five argument cells, and
serializes every `WVSA 1` result from page-probed execution-owned scratch. The
scratch is not stored in the RX application fragment and survives exactly until
`Main` returns. Stale generation, outside-storage reads, malformed requests,
unsupported signed host positions, rejected operations, exact completion, and
indeterminate mutations remain distinct.

The Linux leaf owns `openat`, `flock`, `lseek`, `pread64`, `pwrite64`,
`ftruncate`, `fsync`, and `close`. The Windows leaf reuses the hosted
container's already admitted file-function tables. It resolves only
`SetFilePointerEx` and `SetEndOfFile` from the same bounded PE image that owns
the admitted `CreateFileW` address; PE headers, image ranges, export tables,
name counts, exact names, ordinals, and forwarded exports are checked before a
resolved address can be called.

The focused Windvale program creates a `WVPG 1` root page, durably flushes its
length, publishes a `WVDS 1` superblock, reopens the object, and validates both
formats. The host test extends the closed file from 4,608 to 4,625 bytes to
model an unpublished tail. A second native process selects the committed
superblock, resizes and flushes back to 4,608 bytes, and a third process proves
byte-stable reopen. This is deterministic restart recovery evidence, not a
power-loss or arbitrary hardware-failure claim.

## Excluded claims and next contracts

The native [`WVPT 1`](Windvale-Native-Capability-Provider-Table.md) constructor
binds the exact capability identity and signature to opaque rights-limited
target/state pairs without changing ABI 22. The [native provider-call
candidate](Windvale-Native-Provider-Call.md) emits and independently admits the
exact five-cell x64 call, and an actual storage instruction now selects ABI 23.
A focused host now executes every version-1 operation on Windows and constructs
the equivalent Linux application. Independent Linux execution remains required
before cross-host conformance is claimed. The candidate does not provide an
ordinary configurable container binding, Windvale OS or WebAssembly provider,
power-loss rig, page cache, WAL, transactions, concurrent clients, durable
mutation identities, or service restart. The next database slice can build on
this real single-object executor without widening it into ambient filesystem
authority.
