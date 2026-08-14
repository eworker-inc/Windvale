# Windvale Database proposal

- Date: 2026-08-04
- Status: Active implementation proposal; the bounded reader, hosted page seams, checked storage geometry, mutable storage-resource contract, `WVDS 1` superblock, `WVPG 1` page envelope, `WVCR 1` commit record, `WVTN 1` tree node, single-writer publication, depth-two lookup, and first root split are implemented candidates, but no general transaction engine, server contract, SQL grammar, or product name is accepted by this document
- Working name: Windvale Database
- Informed by: EWDB source, performance evidence, and operational experience
- Builds from: [bounded owned values](../Decisions/0137-Bounded-Owned-Values-Before-Dynamic-Collections.md), [conditional 64-bit scalars](../Decisions/0138-Conditional-Wvb-1-7-64-Bit-Scalars.md), [language and capability direction](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md), [payload variants and recoverable results](../Decisions/0199-Nominal-Payload-Variants-And-Recoverable-Results.md), [bounded sequences and builders](../Decisions/0200-Bounded-Sequences-Affine-Builders-And-For.md), and the [language-design guide](../Architecture/Language-Design.md)

## Purpose

This document explores adding a database to Windvale without claiming that the
current language, runtime, libraries, or operating system can already support a
complete durable database server. The native backend now executes the durable
superblock, physical-page, compact-log, publication-planning core, one
rights-limited storage writer/recovery shell, and the first depth-two tree
generation. General transaction and service boundaries remain absent.
This proposal identifies what should be learned
from EWDB, where a Windvale database would belong, which prerequisites are
missing, and the smallest useful implementation sequence.

The intended result is one reusable database engine with distinct application
and service shells. It is not a line-by-line translation project and does not
make a database part of Windvale source-language semantics. Mutable application
storage remains a capability-backed resource with an independent lifecycle,
authority boundary, failure contract, and format version.

## Review basis

The initial review used these local source states:

- E-Worker Data Platform commit
  `52f06df77b6f751c967fd44fd113702fac9af2f1` as the EWDB extraction candidate;
- E-Worker v7 commit `52645fd6a818dc4d9a8d4242ef0607d8e862223d`
  for the retained benchmark and architecture evidence; and
- the EWDB performance architecture recorded on 2026-07-29.

These hashes record the material reviewed; they do not import it into Windvale
or select it as a permanent upstream. A later implementation proposal must name
the exact source, test, fixture, and benchmark revisions it consumes.

The current `WVDB 1` reader is newly authored from the candidate contract and
does not copy EWDB source, fixtures, persisted bytes, or schemas. EWDB remains
behavioral and architectural evidence for this slice. Any later derivation or
copying from EWDB still requires the explicit licensing and provenance decision
described below.

The reviewed EWDB engine is not only `EWDB.Core`. Its reusable implementation
also depends on Structured Store, Storage, and Graph foundations. In the
reviewed extraction those areas total approximately 48,000 lines of C# before
the server, protocol, client, tools, and product adapters. This size and
coupling make bulk transliteration especially risky.

## Product position

The working name is **Windvale Database**. `Windvaleˉdatabase` is the candidate
module namespace and `wvdb` is a possible tool name. Public naming, command
names, durable format magic, and file extensions remain review questions. The
reader experiment uses `WVDB` only as a replaceable candidate magic; it does
not reserve a production identity.

The database should serve typed Windvale applications and services. Its first
goal is not PostgreSQL SQL compatibility or PostgreSQL's complete join,
extension, replication, administration, and deployment surface. It should
begin with the smallest typed storage and transaction contracts required by a
real Windvale consumer, then grow only through measured needs.

Calling it EWDB 2 would incorrectly suggest API, protocol, query, or on-disk
compatibility. EWDB should remain the qualified operational engine while
Windvale Database develops through isolated fixtures and differential evidence.

## One engine, several roles

The database should not be classified only as an application or only as an
operating-system service. The reusable engine owns semantics; process shells
own placement and authority.

```text
Windvale Database engine libraries
  -> storage/page kernel
  -> transactions and committed snapshots
  -> indexes and query planning
  -> client and service contracts
       -> Windows/Linux hosted server application
       -> Windvale OS supervised database service
       -> administration, inspection, backup, and recovery tools
```

The Windows/Linux application and Windvale OS service may use different
capability providers and process adapters, but they must not become separate
database engines. An embedded profile may later reuse the same verified core
when its ownership, durability, and single-owner restrictions are explicit.

Candidate repository ownership, created only when each area has real code or a
normative contract, is:

- `Specifications/` for accepted formats, validation, transactions, and service
  contracts;
- `Libraries/Database/` for reusable portable engine and client modules;
- `Libraries/Platform/` for rights-limited Windows, Linux, and later Windvale OS
  storage bindings;
- `Operating-System/Services/` for the eventual supervised database service;
- `Tools/Database/` for repository inspection, benchmarks, recovery, and
  qualification; and
- `Tests/Database/` for format, differential, malformed-input, crash, recovery,
  reproducibility, and performance coverage.

The implemented reader now gives `Libraries/Database/` a concrete owner and
uses a fixture under the existing `Tests/` tree. The other candidate areas are
still absent until they receive real code or a normative contract; empty
scaffolding would imply implementation that does not exist.

## EWDB knowledge to preserve

The port should preserve proven mechanisms and their evidence rather than C#
class shapes. The reviewed implemented EWDB performance work includes:

- synchronous writer allocation attribution and removal;
- adaptive isolated-versus-burst group commit;
- a bounded process-wide query-plan cache;
- a byte-budgeted process-wide page and index buffer;
- checksummed persisted B+tree root, branch, and leaf pages;
- append-only copy-on-write primary row pages;
- structurally shared committed roots instead of snapshot-sized copying;
- generation-pinned persisted primary and secondary read authority;
- snapshot-safe obsolete-page reclamation;
- bounded one-traversal mutation batches and smaller row/index page targets; and
- an exact committed point-read path that avoids redundant primary lookups,
  unnecessary secondary-root pins, repeated row-ID unions, and per-lookup
  cycle-detection allocation.

The corresponding design invariants are more important than the implementation
language:

- durable bytes precede publication of the root that makes them visible;
- a committed reader pins one exact root generation;
- mutation publication is atomic and failed preparation is not visible;
- page, cache, transaction, queue, and recovery work have explicit bounds;
- checksums, indices, sizes, offsets, generations, and graph structure are
  revalidated as untrusted input;
- cache keys include every generation that affects their validity;
- cache admission and eviction use byte budgets rather than database counts;
- uncertain mutations are never replayed without an idempotency contract; and
- performance claims retain the exact workload, host, storage, release, and
  durability conditions that produced them.

The reviewed same-workload QA evidence also prevents an oversimplified
language comparison. EWDB was materially slower than PostgreSQL for warm
indexed reads and sequential durable transactions, faster for mixed reads
during 32 durable writers, and slower for the corresponding writer burst. This
supports workload-specific engineering rather than a claim that C, .NET, or a
future Windvale implementation is universally faster.

## What should not be translated initially

The first engine slice should exclude:

- ASP.NET Core hosting, HTTP routing, and JSON transport composition;
- E-Worker account, workspace, schema-bundle, and authorization policy;
- product-specific backup placement and operations;
- graph declarations and traversal until the ordered-index core exists;
- compatibility with obsolete development formats;
- an unbounded SQL surface or PostgreSQL wire compatibility in the storage-kernel slice; and
- a second implementation of behavior still owned only by the qualified EWDB
  runtime.

.NET constructs such as exceptions, `Dictionary`, concurrent dictionaries,
channels, tasks, cancellation tokens, weak tables, locks, LINQ, `FileStream`,
and ASP.NET types identify required behavior but are not portable design
contracts. Windvale equivalents must follow Windvale ownership, result,
capability, concurrency, and resource-accounting rules.

## Windvale readiness

Windvale can now execute one bounded single-writer transaction over a hosted
mutable storage object and recover at every publication boundary. A general
key/value engine, transactional database process, and server are not ready.

| Candidate work | Current readiness | Boundary |
| --- | --- | --- |
| Database architecture and format proposal | Ready | Documentation makes no execution claim. |
| Checksums, endian codecs, key comparison, and page validation | Ready in bounded slices | Current scalars and immutable `bytes` express the algorithms; canonical WVB 1.11 and native x64 now execute exact little-endian `u64` reads and writes. |
| One read-only B+tree lookup over a small in-memory fixture | Implemented experiment | [`WVDB 1`](../../Specifications/Windvale-Database-Reader.md) validates at most 64 256-byte pages and returns a typed exact `u32` to `i32` result. It is not an accepted durable format. |
| Rights-limited hosted snapshot lookup | Implemented candidate | [`Readˉonlyˉwvdb`](../../Libraries/Platform/Database/Read-Only-Wvdb.wv) composes the immutable directory provider and portable reader, assembles at most six chunks, and distinguishes provider failures from invalid database bytes. Independent Linux qualification remains pending. |
| Checked page identity and byte-range arithmetic | Implemented candidate | [`Windvaleˉdatabaseˉstorageˉgeometry`](../../Specifications/Database-Storage-Geometry.md) computes zero-based `u64` page ranges, widens `u32` page size explicitly, and returns typed invalid-size, overflow, or outside-storage outcomes without I/O authority. |
| Pre-opened mutable storage object | Focused native Windows execution implemented; Linux image constructed | [`storage.random_access_v1`](../../Specifications/Random-Access-Storage-Capability.md) defines one object with `u64` generation/positions/length, exact 64 KiB reads, positioned writes, resize, typed mutation completion, and two flush classes. The ABI-23 host derives one exact `WVPT 1` binding, owns the writer fence and page-probed response scratch, executes every operation on Windows, and builds the equivalent Linux syscall application. The fixed test-shell name is not yet an ordinary configurable server binding. |
| Bounded sequences and builders | Ready for narrow algorithms | WVB 1.11 implements bounded immutable sequences, affine builders, and deterministic `for`; nested collections, general maps, and database page ownership remain unavailable. |
| General page and row collections | Not ready | Deterministic maps/page tables, nested or variable-size aggregates, exact allocation charging, and consuming database publication remain unimplemented. |
| Durable superblock and recovery selection | Implemented candidate | [`WVDS 1`](../../Specifications/Windvale-Database-Durable-Superblock.md) defines two checksummed 256-byte slots, checked committed length, generation selection, conflict rejection, and unpublished-tail reporting. It is the publication target and performs no I/O. |
| Durable page, compact log, and publication ordering | Implemented candidate | [`WVPG 1` and `WVCR 1`](../../Specifications/Windvale-Database-Durable-Commit.md) validate exact immutable pages and commit linkage; the pure planner enforces append, content-and-length flush, inactive-slot write, and content flush while mapping partial or indeterminate mutations to recovery. |
| Portable storage publication and reopen policy | Implemented candidate | The [storage recovery contract](../../Specifications/Windvale-Database-Storage-Recovery.md) maps publication to bounded 64 KiB actions and maps fresh superblock evidence plus an unpublished tail to exact resize and content-and-length flush actions. Uncertain mutations require reopen and are never silently replayed. |
| Native single-writer transaction | Focused Windows implementation; cross-host qualification pending | The [single-writer transaction](../../Specifications/Windvale-Database-Single-Writer-Transaction.md) constructs one root page, one compact log page, the inactive superblock, and a typed publication plan without I/O authority. Its hosted executor performs the four exact storage actions and preserves provider completion semantics. The builder remains payload-agnostic; the `WVTN 1` composition below supplies the first structured root. |
| Durable variable-key tree | Focused Windows depth-two implementation; general propagation pending | [`WVTN 1`](../../Specifications/Windvale-Database-Tree-Node.md) defines packed ordered byte keys, byte values, `u64` branch children, lookup, replacement, page-size-aware upsert, deterministic leaf split, and branch routing. The hosted reader proves stable provider identity, descending child identities, selected generation visibility, and inherited key ranges. A four-page append publishes two leaves, one branch root, and one log without modifying the old generation. General child split propagation and reclamation remain unimplemented. |
| Capability-bearing mutation and crash recovery | Focused native Windows restart recovery implemented; cross-host qualification pending | [`WVPT 1`](../../Specifications/Windvale-Native-Capability-Provider-Table.md) binds the exact storage target/state, and the [provider-call contract](../../Specifications/Windvale-Native-Provider-Call.md) preserves all ABI-23 budgets while returning strict `WVSA 1`. Native processes now repair an unpublished tail and interrupt the first transaction after zero through four completed actions; every restart selects and validates only the old 4,608-byte or new 12,800-byte generation before a stable reopen. General fragment/WVB admission, independent Linux execution, configurable server binding, tree-node payloads, and reclamation remain unimplemented. Native path replacement and directory durability remain separate future interfaces. |
| Concurrent readers, one hosted writer, and group commit | Not ready | Structured tasks, channels, cancellation, synchronization, and cross-task ownership remain future contracts. |
| High-performance native database process | Not ready | General Windvale-owned native lowering, 64-bit backend coverage, memory management, optimization, and host services remain incomplete. |
| Windvale OS database service | Not ready | Persistent storage, general launch/supervision, resource domains, service bindings, and filesystem providers remain future work. |

Checked `i64` and `u64` are implemented in Stage 0 and the Windvale-written compiler under
[Decision 0138](../Decisions/0138-Conditional-Wvb-1-7-64-Bit-Scalars.md),
and [Decision 0207](../Decisions/0207-U64-Binary-Fields-For-Durable-Storage.md)
adds exact little-endian `u64` field codecs. [Decision 0209](../Decisions/0209-Single-Current-Wvb-1-11-Format.md)
folds the complete surface into canonical WVB 1.11. Implemented-candidate
[Decision 0211](../Decisions/0211-U64-Database-Storage-Geometry.md) adds the
lossless `u32` to `u64` transition and the first checked portable page-range
module. Implemented-candidate
[Decision 0212](../Decisions/0212-First-Preopened-Random-Access-Storage.md)
then binds those widths to one format-neutral hosted storage object with typed
generation, lifecycle, mutation-completion, and flush results. Native,
WebAssembly, and Windvale OS execution profiles retain explicit narrower WVB
1.11/capability subsets.
[Decision 0200](../Decisions/0200-Bounded-Sequences-Affine-Builders-And-For.md)
now supplies the first bounded ownership and builder slice required by
[Decision 0137](../Decisions/0137-Bounded-Owned-Values-Before-Dynamic-Collections.md).
The reader uses the concurrently delivered payload-variant result surface; its
small recursive search path does not claim that general database collections
or ownership are complete.

## Durable integer-width direction

The experimental reader's `u32` fields remain intentionally local to its
16,416-byte immutable fixture. A future durable format uses distinct scalar
domains rather than treating every integer as interchangeable:

The implemented storage-geometry module now proves the shared arithmetic seam:
zero-based `u64` page identity, `u64` offset and exclusive end, a losslessly
widened `u32` page size, complete overflow preflight, and typed out-of-storage
failure. It does not select a page size, header size, durable format, provider,
or authority model.

| Domain | Selected durable direction | Reason |
| --- | --- | --- |
| Byte offset and file length | `u64` | Avoid the approximate 4 GiB `u32` byte-position ceiling. |
| Page identity and persisted child reference | `u64` | Preserve long-lived identity across growth, recovery, reclamation, and generations. |
| Generation, commit sequence, and WAL position | `u64` | Prevent practical rollover from aliasing durable history. |
| Mutation/idempotency identity | `u64` minimum | Make uncertain-completion detection explicit; final shape may be wider or structured. |
| Page size, chunk length, entry count, and bounded collection count | `u32` | These remain deliberately bounded in memory and at capability boundaries. |
| Status, version, flags, and algorithm identifiers | Small explicit widths | Their closed value spaces do not benefit from widening. |

A page identity is not a byte offset. Converting it to storage position uses
checked `u64` arithmetic for `header_size + page_id * page_size`, then validates
the complete requested range against the granted storage object's length and
configured growth ceiling. Reusing a physical page requires an explicit
generation/fencing rule; an equal numeric offset alone cannot prove that a
cached or logged reference is current.

Decision 0534 assigns `WVDS 1` only to the dual-superblock record and its
recovery selector. Decision 0535 separately assigns `WVPG 1` to the physical
page envelope and `WVCR 1` to the compact commit record. None silently
versions root, branch, leaf, row, index, transaction, or whole-database
compatibility. Those formats still require their own validation, recovery,
migration, and malformed-input rules.

## Required enabling contracts

The first writable database milestone requires at least:

1. payload variants and typed recoverable results for expected storage and
   provider outcomes; the reader and first mutable storage library now exercise
   this part;
2. bounded immutable sequences and uniquely owned builders are now present,
   and one focused executor consumes bounded database publication;
   deterministic maps or page tables and exact allocation charges remain
   required;
3. scoped capability-resource ownership with explicit close, stale-generation,
   and provider-loss behavior; the current binding supplies generation/loss
   results and launcher-owned teardown, while source-scoped typed close remains;
4. `i64` and `u64` support, including the canonical WVB 1.11 binary codecs, through the
   selected compiler, verifier, interpreter, native ABI, and backend profile;
5. a rights-limited storage capability with exact random-access reads,
   positioned writes, partial-progress reporting, flush classes, fencing,
   bounded requests, and one root-publication recovery path is now implemented;
   durable mutation identity and broader recovery policies remain, while native
   replacement and directory durability stay separate;
6. monotonic time for deadlines and measurements without making wall-clock time
   part of database ordering;
7. structured tasks, bounded channels, cancellation, synchronization, and
   explicit ownership transfer for group commit and background maintenance; and
8. byte-budgeted native memory suitable for page frames, pins, eviction,
   pressure admission, and deterministic teardown.

The storage capability should bind pre-authorized storage roots or objects, not
grant ambient native paths. Its operations must distinguish rejection, exact
partial progress, completion, and indeterminate mutation completion. A flush
must state exactly which data and metadata durability boundary it proves.

### First database-storage interface — implemented candidate

Implemented-candidate [Decision 0212](../Decisions/0212-First-Preopened-Random-Access-Storage.md)
binds one pre-opened storage object behind one capability identity and one
provider generation. The typed source library exposes:

```text
Storage_describe() -> typed generation and u64 length result
Storage_read_at(Generation: u64, Offset: u64, Maximum: u32) -> exact chunk result
Storage_write_at(Generation: u64, Offset: u64, Value: bytes) -> typed progress result
Storage_resize(Generation: u64, Length: u64) -> typed mutation result
Storage_flush(Generation: u64, Class) -> typed durability result
```

The exact source names use Windvale macron separators; the abbreviated shapes
above emphasize the boundary. `Describe` must complete before later operations.
`Read_at` has no implicit cursor. `Write_at` may extend the object and
distinguishes rejected-before-change, exact partial progress, completion, and
indeterminate completion. Resize remains a separate metadata mutation.
`Content` and `Content_and_length` flushes do not imply atomic native-path
replacement or directory-entry durability.

The provider generation fences the current binding. After an indeterminate
result, the engine must recover from checksummed storage state and must not
blindly replay the mutation. Loss of the writer fence makes the instance stale.
Revocation, provider exit, and stale generation are ordinary typed outcomes;
the Stage 0 launcher owns close at runtime teardown. Version 1 deliberately
does not claim a persistent mutation identity across provider restart. Add one
only with a query/recovery contract that makes retries safer than storage-based
recovery.

Implemented-candidate [Decision 0544](../Decisions/0544-First-Native-Durable-Storage-Provider.md)
now executes every version-1 operation through the generated ABI-23 provider
call. Its execution-owned provider opens one fixed object behind a writer
fence, publishes a durable empty database, repairs one deterministic
unpublished tail after restart, and proves a stable third reopen on Windows.
The Linux application is constructed from its syscall leaf pending independent
execution. The ordinary hosted packager still lacks exact WVB-to-provider
admission and configurable binding, so this remains a focused shell rather than
a product launcher.

[Decision 0546](../Decisions/0546-First-Native-Verification-Tool-Checkpoint.md)
adds a non-qualification cached development lane for that composed lifecycle.
It reduced the measured Windows feedback loop from 658.777 seconds for the
clean nine-case owner to 100.179 seconds while keeping clean reconstruction and
cross-target construction as the final owner boundary.

[Decision 0547](../Decisions/0547-First-Native-Single-Writer-Transaction.md)
adds the first consuming transaction over that provider: portable root/log/
superblock construction, one hosted action executor, and process restart after
zero through four publication actions. It remains a bounded storage-kernel
slice rather than a server or general transaction API.

[Decision 0548](../Decisions/0548-First-Durable-Tree-Node-And-Upsert.md) replaces
the opaque test payload with `WVTN 1`: variable byte keys and values, strict
ordering, lookup, replacement, insertion, and two-generation copy-on-write
composition. [Decision 0549](../Decisions/0549-Bounded-Durable-Tree-Reader-And-Root-Split.md)
adds bounded provider-backed traversal, global range/graph proofs, deterministic
leaf split, a reusable multi-page commit batch, and the first branch-root
generation. General non-root split propagation remains next.

The first database can avoid requiring cross-platform atomic file replacement
by publishing commits inside one storage object: write new pages, flush their
content, write one checksummed alternate superblock/root record, then flush
again. Recovery selects the newest completely valid generation. Backup and
whole-file replacement remain later optional interfaces with their own
directory-durability contracts.

## Proposed implementation sequence

### Stage 0: provenance and performance ledger

Before source translation:

- resolve whether selected EWDB material is copied under its existing license,
  expressly relicensed by its copyright owner, or used only as a behavioral
  reference;
- record exact source and evidence hashes;
- inventory every retained optimization, invariant, benchmark, crash boundary,
  and malformed-input case; and
- classify product-specific behavior that is intentionally excluded.

The E-Worker Data Platform and Windvale repositories currently use different
source-available licenses. Common organizational ownership does not remove the
need for an explicit provenance and licensing record.

### Stage 1: smallest executable read path — implemented experiment

The experiment specifies and implements only:

```text
small immutable database bytes
  -> validate header, version, sizes, and checksum
  -> validate one B+tree root and search path
  -> perform one exact key lookup
  -> return one typed inspection result
```

The implemented [`WVDB 1` reader experiment](../../Specifications/Windvale-Database-Reader.md)
compiles, verifies, and runs through the reference runtime. Its independently
constructed Stage 0 fixtures cover valid, boundary, truncated, oversized,
inconsistent, cyclic, depth-exhausted, padding, ordering, child, and checksum-
corrupt inputs. The test pins deterministic library bytes and registers the
same source for byte-identical Stage 0 and Windvale-written compiler output.
It remains a format-reader experiment, not an accepted database format.

Implemented-candidate [Decision 0210](../Decisions/0210-First-Hosted-Wvdb-Snapshot-Consumer.md)
adds the first hosted consumer path. It obtains one named immutable snapshot
through the rights-limited directory capability, checks the reader's exact
16,416-byte ceiling, assembles no more than six bounded chunks, and returns
typed storage-versus-database outcomes. It does not change any `WVDB 1` byte or
claim durable storage.

### Stage 2: language and storage prerequisites

Use concrete pressure from the reader and first writer to advance the smallest
owned sequence/builder, typed result, 64-bit backend, resource, and storage-
capability contracts. Do not add a broad collection or filesystem surface only
because a C# API exists.

### Stage 3: durable single-writer storage kernel

Build a single-process, single-writer ordered key/value kernel with:

- fixed-size checksummed pages and checked 64-bit identities;
- persisted B+tree roots and bounded search depth;
- a compact binary write-ahead log;
- durable-before-publish root replacement;
- reopen, tail truncation, and crash recovery;
- deterministic compaction or obsolete-page reclamation; and
- fault injection before and after every durable transition.

The storage substrate and first depth-two Stage 3 vertical slice are
implemented. `WVDS 1` supplies dual checksummed root records and deterministic
recovery selection. `WVPG 1`, `WVCR 1`, and the pure publication planner add
immutable page/log bytes and an executable durable-before-publish sequence.
`WVTN 1` now adds variable-key leaf lookup, replacement, and insertion, and the
hosted executor publishes that structured root through every four-action crash
boundary. It now splits one full root leaf, publishes a two-leaf branch root,
and performs bounded provider-backed point lookup. The engine does not yet
propagate a non-root split, rewrite an existing branch, reclaim pages, or manage
concurrent transactions.

This milestone needs no general query language, network protocol, or graph
layer.

### Stage 4: retained EWDB performance mechanisms

Add, one independently measured boundary at a time:

- generation-pinned committed snapshots and structurally shared roots;
- a process-wide byte-budgeted page buffer with exact pin and eviction evidence;
- secondary indexes and bounded mutation batches;
- normalized query planning and a generation-keyed plan cache;
- a bounded hosted-writer queue and adaptive group commit; and
- committed point/range read fast paths with exact I/O and allocation evidence.

Correctness, recovery, and invalidation tests accompany every performance
change. A microbenchmark improvement alone cannot replace the complete-path
workload.

### Stage 5: application and service products

After the kernel is stable:

- define a versioned typed client/service protocol independent from host wire
  encoding;
- add a bounded SQL parser, binder, planner, and diagnostic surface for people;
  SQL translates into the same typed query/transaction model and is not the
  engine's internal API or service wire contract;
- package a Windows/Linux hosted server application;
- add inspection, backup, restore, repair, and qualification tools; and
- bind the same engine and semantic capabilities into a supervised Windvale OS
  service when OS storage and service lifecycle contracts are qualified.

## Differential and performance evidence

EWDB should remain an oracle, not a hidden runtime dependency. Candidate
Windvale Database tests should replay the same isolated logical workloads into
separate stores and compare declared results, failure outcomes, generations,
and recovery state. Production dual-write is not implied.

The evidence suite should include:

- valid, boundary, truncated, oversized, inconsistent, cyclic, and malicious
  format inputs;
- deterministic artifact and report bytes across Windows and Linux;
- crash injection around append, flush, root publication, checkpoint,
  compaction, and recovery;
- old-snapshot isolation and stale-generation failures;
- exact cache entry, byte, pin, eviction, and pressure ceilings;
- exact queue admission, batching, cancellation, and uncertain-completion
  behavior; and
- workload-specific latency, throughput, allocation, memory, I/O, and durable-
  flush observations on named storage and host profiles.

The first goal is not to beat PostgreSQL. The first goals are exact semantics,
crash safety, bounded resource use, reproducibility, and a coherent native path.
Performance targets should be approved only after the first complete path can be
measured against EWDB and relevant external comparators.

## Non-goals of this proposal

This proposal does not:

- accept the working product name or a repository layout;
- select a complete on-disk format, SQL grammar, isolation level, or wire protocol;
- promise EWDB data, API, query, or operational compatibility;
- retire EWDB or move an existing authority;
- claim PostgreSQL parity;
- claim general SQL or PostgreSQL compatibility, replication, clustering,
  failover, or distributed consensus;
- make .NET, C, POSIX, Windows, or Linux behavior the database definition; or
- authorize dead `.wv` source that cannot compile, verify, and execute in a
  bounded milestone.

## Questions before an implementation decision

- Is **Windvale Database** an acceptable working identity, and which public
  name, module namespace, CLI name, and format identity should be reserved?
- Is the first consumer a compiler/tool catalog, package index, OS service
  registry, application data store, or an isolated database demonstration?
- Which EWDB source may be copied or derived, under which license and
  attribution, and which parts remain behavioral reference only?
- Should the first candidate format be deliberately new, or is one named EWDB
  read-only import case valuable enough to specify separately?
- Which storage capability is the smallest exact contract that Windows, Linux,
  and Windvale OS can all implement without reducing durability to a host
  default?
- Which transaction isolation and single-writer guarantees are required by the
  first real consumer?
- Which bounded SQL subset best serves administrators and interactive users
  without becoming the typed client protocol?
- Which first benchmark workload and storage profile justify an initial numeric
  performance budget?
- Which implemented slice is useful enough to package before network service
  support exists?

## Recommended next decision

Decisions 0534 through 0549 now supply the dual superblock, immutable page and
tree-node envelopes, compact commit record, portable publication/recovery
actions, exact ABI-23 storage call, one real provider pair, root-leaf lookup and
upsert, and deterministic interruption after every publication action. The next
database decision should add general depth-two upsert, child split propagation,
and explicit obsolete-page ownership without changing the four-action publish
protocol. Independent Linux execution and ordinary configurable ABI-23 binding
remain parallel qualification work. Catalog, network listener, and SQL
execution should wait until repeated multi-page growth is real.
