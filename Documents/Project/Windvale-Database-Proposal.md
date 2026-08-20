# Windvale Database proposal

- Date: 2026-08-04
- Status: Historical implementation proposal; the bounded reader, hosted page seams,
  checked storage geometry, mutable storage-resource contract, `WVDS 1`
  superblock, `WVPG 1` page envelope, `WVCR 1` commit record, `WVTN 1` tree
  node, single-writer publication, repeated depth-two updates, deterministic
  internal branch split, and bounded owned-path updates for input depths two
  through eight with full split propagation, collection-scoped schemas, typed
  rows, bounded multi-record transactions, and portable secondary-index
  planning are implemented candidates, but no long-running server, query
  executor, hosted index-enforcement boundary, or product name was accepted by
  this document. Its external rewrite and parity framing is superseded by
  [Decision 0790](../Decisions/0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md);
  retain this file as implementation and provenance history, not active WVDB
  1.0 product direction
- Working name: Windvale Database
- Historical review inputs: EWDB source, performance evidence, and operational
  experience
- Active direction: [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md)
- Builds from: [bounded owned values](../Decisions/0137-Bounded-Owned-Values-Before-Dynamic-Collections.md), [conditional 64-bit scalars](../Decisions/0138-Conditional-Wvb-1-7-64-Bit-Scalars.md), [language and capability direction](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md), [payload variants and recoverable results](../Decisions/0199-Nominal-Payload-Variants-And-Recoverable-Results.md), [bounded sequences and builders](../Decisions/0200-Bounded-Sequences-Affine-Builders-And-For.md), and the [language-design guide](../Architecture/Language-Design.md)

## Purpose

This historical document records the early database exploration without
claiming that the current language, runtime, libraries, or operating system can
already support a complete durable database server. The native backend now executes the durable
superblock, physical-page, compact-log, publication-planning core, one
rights-limited storage writer/recovery shell, repeated depth-two updates, and a
bounded persistent multi-record transaction for input depths two through eight.
A full depth-four cascade now creates a depth-five root. Provider-driven path
discovery is implemented, and portable secondary-index planning can compose
primary and index changes. Hosted bundle discovery and unique checks, input
depth nine, reclamation, concurrent readers, a writer queue, query execution,
and service boundaries remain absent. This proposal records the candidate
mechanisms, missing prerequisites, and smallest useful implementation sequence
identified during that exploration. Decision 0790 and the WVDB 1.0
specification plan own the current product direction.

The intended result is one reusable database engine with distinct application
and service shells. WVDB is independently specified and implemented in
Windvale Language 1.0; it is not a translation project. Mutable application
storage remains a capability-backed resource with an independent lifecycle,
authority boundary, failure contract, and format version.

## Review basis

The initial review used these local source states:

- E-Worker Data Platform commit
  `52f06df77b6f751c967fd44fd113702fac9af2f1` as the EWDB extraction candidate;
- E-Worker v7 commit `52645fd6a818dc4d9a8d4242ef0607d8e862223d`
  for the retained benchmark and architecture evidence; and
- the EWDB performance architecture recorded on 2026-07-29.

These hashes record historical material reviewed; they do not import it into
Windvale, select it as an upstream, or make it a WVDB compatibility authority.

The current `WVDB 1` reader was newly authored from the candidate contract and
does not copy the reviewed source, fixtures, persisted bytes, or schemas. Any
future use of third-party or separately licensed material still requires an
explicit licensing and provenance decision.

The reviewed historical engine depended on several storage and graph
foundations in addition to its nominal core. That coupling reinforced the need
to specify WVDB behavior independently instead of transferring source
structure or framework boundaries.

## Product position

Decision 0790 accepts **WVDB 1.0** as the product and specification identity.
`Windvaleˉdatabase` remains a candidate module namespace and `wvdb` a
candidate tool name. Command names, durable format magic, and file extensions
remain specification questions. Existing experimental `WVDB` magic does not
become the production format identity automatically.

The database should serve typed Windvale applications and services. It has no
required SQL, API, protocol, file, runtime, or behavioral compatibility with
another database. It should implement accepted WVDB specifications in useful
vertical slices, then grow through stated product requirements and measured
workloads.

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

## Historical implementation lessons

The historical review identified mechanisms worth evaluating through WVDB
requirements, measurements, and conformance evidence:

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

The reviewed same-workload evidence also showed that relative performance
changed with the operation, durability mode, and concurrency profile. WVDB
should preserve reproducible workload evidence rather than make broad language
or product performance claims.

## Boundaries of the first engine slice

The first engine slice should exclude:

- service hosting, routing, and transport composition not required by the
  storage contract;
- application-specific account, workspace, schema-bundle, and authorization
  policy;
- deployment-specific backup placement and operator workflow, while retaining
  exact engine-level backup, restore, and durability requirements for later
  specification;
- graph declarations and traversal from the ordered-index implementation slice,
  while retaining the graph/relationship profile in WVDB 1.0 design;
- compatibility with obsolete development formats;
- an unbounded query-language surface or external wire-protocol compatibility
  in the storage-kernel slice; and
- behavior that has no accepted WVDB requirement, specification, and
  conformance owner.

Host-language and framework constructs are not WVDB design contracts. Windvale
Language 1.0 implementation must follow Windvale ownership, result, capability,
concurrency, and resource-accounting rules.

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
| Bounded sequences and builders | Ready for narrow algorithms | WVB 1.11 implements bounded immutable sequences, affine builders, and deterministic `for`; nested collections and general maps remain unavailable. Database pages now have explicit single-commit predecessor ownership, but no general allocator or reclamation collection. |
| General page and row collections | Not ready | Deterministic maps/page tables, nested or variable-size aggregates, exact allocation charging, and consuming database publication remain unimplemented. |
| Durable superblock and recovery selection | Implemented candidate | [`WVDS 1`](../../Specifications/Windvale-Database-Durable-Superblock.md) defines two checksummed 256-byte slots, checked committed length, generation selection, conflict rejection, and unpublished-tail reporting. It is the publication target and performs no I/O. |
| Durable page, compact log, and publication ordering | Implemented candidate | [`WVPG 1` and `WVCR 1`](../../Specifications/Windvale-Database-Durable-Commit.md) validate exact immutable pages and commit linkage; the pure planner enforces append, content-and-length flush, inactive-slot write, and content flush while mapping partial or indeterminate mutations to recovery. |
| Portable storage publication and reopen policy | Implemented candidate | The [storage recovery contract](../../Specifications/Windvale-Database-Storage-Recovery.md) maps publication to bounded 64 KiB actions and maps fresh superblock evidence plus an unpublished tail to exact resize and content-and-length flush actions. Uncertain mutations require reopen and are never silently replayed. |
| Native single-writer transaction | Focused Windows implementation; cross-host qualification pending | The [single-writer transaction](../../Specifications/Windvale-Database-Single-Writer-Transaction.md) constructs one root page, one compact log page, the inactive superblock, and a typed publication plan without I/O authority. Its hosted executor performs the four exact storage actions and preserves provider completion semantics. The builder remains payload-agnostic; the `WVTN 1` composition below supplies the first structured root. |
| Durable variable-key tree | Focused Windows bounded-path updates and deletes through input depth eight implemented; broader qualification pending | [`WVTN 1`](../../Specifications/Windvale-Database-Tree-Node.md) defines packed ordered byte keys, byte values, `u64` branch children, lookup, replacement, page-size-aware upsert, deterministic leaf split, and branch routing. The hosted reader proves stable provider identity, descending child identities, selected generation visibility, and inherited key ranges. The [depth-two transaction](../../Specifications/Windvale-Database-Depth-Two-Upsert.md), [depth-three growth](../../Specifications/Windvale-Database-Depth-Three-Root-Growth.md), and [existing depth-three transaction](../../Specifications/Windvale-Database-Depth-Three-Upsert.md) establish replacement, split, and height growth. The [bounded owned-path upsert](../../Specifications/Windvale-Database-Tree-Path-Upsert.md) propagates splits bottom-up for input depths two through eight, while the [focused delete transaction](../../Specifications/Windvale-Database-Tree-Path-Delete.md) rebuilds a present leaf and every ancestor without merging. Provider-driven upsert and delete publication are implemented; reclamation remains next work. |
| Physical leaf delete and bounded scan | Focused portable Windows native implementation; cross-host qualification pending | [Leaf operations](../../Specifications/Windvale-Database-Tree-Leaf-Operations.md) physically remove exact keys into canonical copy-on-write replacement leaves and return ascending inclusive/exclusive ranges of at most 500 rows as one borrowed contiguous slice. Reverse scans, merge, and reclamation remain unimplemented. |
| Durable full-path delete | Focused hosted Windows native implementation; cross-host qualification pending | [Tree-path delete](../../Specifications/Windvale-Database-Tree-Path-Delete.md) rebuilds a present key's selected leaf and every ancestor into one crash-safe commit. [Hosted delete publication](../../Specifications/Windvale-Database-Hosted-Tree-Delete.md) discovers the committed path through bounded provider reads, preserves changed-snapshot errors, publishes through the four-action protocol, recovers every interruption boundary, and performs no provider mutation for a missing key. |
| Committed-generation range cursor | Focused hosted Windows native implementation; cross-host qualification pending | [Durable range scan](../../Specifications/Windvale-Database-Durable-Range-Scan.md) returns one zero-copy packed leaf page at a time, resumes across separators without sibling pointers, and pins database plus provider identity so pages from different commits cannot mix. |
| Canonical logical record layer | Focused portable native implementation; cross-host qualification pending | [`WVKR 1` and `WVRD 1`](../../Specifications/Windvale-Database-Logical-Records.md) map nonzero collection identities, opaque bounded record identities, collection-scoped schema keys, nonzero schema identities, and payload bytes to exact `WVTN 1` keys and values. Typed get/put preparation is capability-free and deterministic; deletes, indexes, and queries remain unimplemented. |
| Persistent collection catalog | Focused portable native implementation; cross-host qualification pending | [`WVCL 1`](../../Specifications/Windvale-Database-Collection-Catalog.md) stores one exact UTF-8 name and primary-schema identity at each canonical collection key, prepares deterministic read/put bytes, and rejects key/value identity mismatch. Put retains current upsert semantics; atomic creation, name lookup/uniqueness, and schema migration remain unimplemented. |
| Bounded schemas and typed binary rows | Focused portable Windows native implementation; cross-host qualification pending | [`WVSC 1` and `WVTR 1`](../../Specifications/Windvale-Database-Typed-Rows-And-Schemas.md) persist 1 through 64 uniquely named Boolean, I64, U64, UTF-8 text, or bytes fields with exact nullability and byte limits. Typed put preparation rejects missing, extra, mismatched, oversized, or malformed values before producing `WVRD 1`; defaults, nested values, decimals, migration, and compatibility coercion remain unimplemented. |
| Strict bounded JSON value | Focused portable Windows native implementation; cross-host qualification pending | The [strict JSON value](../../Specifications/Windvale-Database-Json-Value.md) admits one 64 KiB UTF-8 document with exact depth, node, property, array, string, number, name, and escape limits; rejects comments, trailing commas, malformed Unicode, semantic duplicate names, and unsafe prototype names; and retains one owned exact spelling without allocating a second document tree. Protocol envelopes, typed-row conversion, canonical output, JSON indexes, and query semantics remain unimplemented. |
| Strict JSON protocol envelope | Focused portable Windows native implementation; cross-host qualification pending | The [version-1 JSON envelope](../../Specifications/Windvale-Database-Json-Protocol.md) strictly admits and deterministically emits bounded request/database IDs, seven operation families, deadlines, optional freshness sequences, response status and observed sequence, and object body spans without copying the body. Operation-specific bodies, transport, authentication, authorization, and server execution remain unimplemented. |
| Canonical transaction mutations | Focused portable Windows native implementation; cross-host qualification pending | [`WVTM 1`](../../Specifications/Windvale-Database-Transaction-Mutations.md) describes 1 through 32 strictly sorted, duplicate-free puts/deletes in at most 256 KiB. It validates the complete request before returning owned bytes. The persistent hosted writer now consumes it through one atomic publication. |
| Atomic transaction leaf rewrite | Focused portable Windows native implementation; cross-host qualification pending | The [transaction leaf rewrite](../../Specifications/Windvale-Database-Transaction-Leaf-Rewrite.md) decodes one `WVTM 1` set once, applies all operations to one in-memory `WVTN 1` leaf, reports exact no-ops and counts, and exposes no partial bytes on failure. Path grouping, leaf splitting, shared ancestor rewrite, and durable publication remain next. |
| Bounded transaction path set | Focused portable Windows native implementation; cross-host qualification pending | The [transaction path set](../../Specifications/Windvale-Database-Transaction-Paths.md) validates one snapshot-consistent root-to-leaf path per canonical mutation through depth eight, rejects wrong routing and conflicting shared-page bytes, and reports consecutive unique leaf groups under a 16 MiB ceiling. Bottom-up shared rewrite and one commit publication remain next. |
| Transaction leaf-group plan | Focused portable Windows native implementation; cross-host qualification pending | [`WVLG 2`](../../Specifications/Windvale-Database-Transaction-Leaf-Groups.md) groups sorted mutations by routed leaf and uses the bulk partitioner once per group. Every changed group carries one complete `WVLP 1` replacement plan, including two through 33 leaves when needed; temporary overflow followed by later deletes is evaluated correctly as one final state. Durable leaf-page allocation is implemented; shared ancestor replacement and one commit publication remain next. |
| Durable transaction leaf pages | Focused portable Windows native implementation; cross-host qualification pending | [`WVLD 1`](../../Specifications/Windvale-Database-Transaction-Leaf-Pages.md) assigns consecutive page identities to every changed `WVLG 2` replacement, encodes complete checksummed `WVPG 1` pages at one target generation and sequence, and carries a compact validated replacement map. Unchanged groups allocate nothing. Shared ancestor replacement and one commit publication remain next. |
| Bulk transaction branch partition | Focused portable Windows native implementation; cross-host qualification pending | [`WVCR 1`](../../Specifications/Windvale-Database-Transaction-Child-Replacements.md) carries one bounded tree level's old-to-new child mappings. [`WVBP 1`](../../Specifications/Windvale-Database-Transaction-Branch-Partition.md) applies every changed child to one shared parent exactly once and partitions the complete final branch with deterministic promoted separators. Parent grouping, durable branch pages, and recursive root completion are implemented; commit publication remains next. |
| Transaction parent groups | Focused portable Windows native implementation; cross-host qualification pending | [`WVPP 1`](../../Specifications/Windvale-Database-Transaction-Parent-Groups.md) joins allocated replacement leaves to their actual parents from the validated paths, groups every changed child under one parent, and invokes the bulk partitioner once per parent. Durable branch-page allocation and recursive root completion are implemented; one commit publication remains next. |
| Transaction tree completion | Focused segmented Windows native implementation; cross-host qualification pending | [`WVTC 1`](../../Specifications/Windvale-Database-Transaction-Tree-Completion.md) composes changed leaves, immediate parents, every remaining ancestor, and split-root growth for input depths two through eight into at most 792 consecutive durable pages and exactly one root. It binds mutations, exact paths, and pages and replays the complete plan during validation. Commit and persistent hosted publication are implemented. |
| Transaction commit publication | Focused segmented Windows native implementation; cross-host qualification pending | The [transaction commit coordinator](../../Specifications/Windvale-Database-Transaction-Commit.md) validates up to 792 consecutive tree pages and unique obsolete ownership, appends one compact commit-log page, constructs the inactive superblock, and returns the exact four-action durable publication state. Logical no-ops publish nothing. The persistent hosted writer now executes this state. |
| Persistent hosted transaction writer | Focused segmented Windows native implementation; cross-host qualification pending | The [persistent writer](../../Specifications/Windvale-Database-Persistent-Transaction-Writer.md) gathers one provider-backed path per canonical mutation, commits changes across multiple leaves atomically, requires reopen after commit or uncertainty, and records request, recovery, I/O, append, logical memory-bound, and explicit monotonic-tick counters. It is one serialized owner, not yet a concurrent queue or network server. |
| Bulk transaction leaf partition | Focused portable Windows native implementation; cross-host qualification pending | [`WVLP 1`](../../Specifications/Windvale-Database-Transaction-Leaf-Partition.md) merges one leaf with all of its sorted mutations as a final state and emits one through 33 ordered leaves. It handles later deletes after temporary overflow and rejects an individually oversized entry without partial output. Group integration and durable leaf-page allocation are implemented; shared ancestor replacement and publication remain next. |
| Persistent comparison benchmark | Planned after persistent server and batch commit | Run matched durable single-record, batched-write, point-read, range-read, and mixed WVDB workloads against selected representative systems only when the comparison answers a named design or performance question. Record exact versions, host/storage profiles, load, durability settings, warmup, throughput, latency percentiles, memory, storage growth, observed I/O, and raw results. Keep cold-process restart as a separate startup/recovery workload. |
| Bounded typed query IR | Focused portable Windows native implementation; cross-host qualification pending | [`WVQI 1`](../../Specifications/Windvale-Database-Query-Ir.md) binds one collection and schema to unique projections, up to 32 typed `AND` predicates and parameters, up to 8 sort fields, and a required 500-row ceiling. It rejects schema, type, null, ordering, unused-parameter, duplicate-field, framing, and size errors before execution. JSON lowering, plans, indexes, cursors, and execution remain unimplemented. |
| Parameterized SQL lowering | Focused portable Windows native implementation; cross-host qualification pending | The [first SQL subset](../../Specifications/Windvale-Database-Sql.md) accepts one bounded `SELECT` over an exactly bound collection and schema, parameter-only comparisons, null predicates, ordering, and a mandatory row limit, then lowers into `WVQI 1`. It intentionally excludes literals, joins, expressions, mutations, and comments. It is a candidate WVDB language surface, not an external SQL compatibility contract. |
| Deterministic durable bootstrap and create-or-open | Focused native Windows implementation; cross-host qualification pending | The [database bootstrap](../../Specifications/Windvale-Database-Bootstrap.md) constructs the canonical generation-1 empty `WVDS 1`/`WVPG 1` image and resumes only two byte-exact interruption states. The [engine lifecycle](../../Specifications/Windvale-Database-Engine-Lifecycle.md) composes bootstrap with open, validates expected identity/page size before tail recovery, and returns an engine-ready snapshot. Storage-object creation and server policy remain separate. |
| Capability-bearing mutation and crash recovery | Focused native Windows repeated restart recovery implemented; cross-host qualification pending | [`WVPT 1`](../../Specifications/Windvale-Native-Capability-Provider-Table.md) binds the exact storage target/state, and the [provider-call contract](../../Specifications/Windvale-Native-Provider-Call.md) preserves all ABI-23 budgets while returning strict `WVSA 1`. Native processes repair unpublished tails and interrupt either the initial root split or repeated depth-two update after zero through four completed actions; every restart selects and validates only the 4,608-byte bootstrap, 20,992-byte first depth-two, or 33,280-byte repeated-update generation before a stable reopen. General fragment/WVB admission, independent Linux execution, configurable server binding, and reclamation remain unimplemented. Native path replacement and directory durability remain separate future interfaces. |
| Concurrent readers, one hosted writer, and group commit | Partial: serialized persistent writer implemented | One hosted owner can now retain session state and publish bounded atomic transactions. Structured tasks, a writer queue, channels, cancellation, synchronization, group commit, and cross-task ownership remain future contracts. |
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
generation. [Decision 0551](../Decisions/0551-General-Depth-Two-Upsert-And-Obsolete-Ownership.md)
then adds repeated routed-leaf replacement, leaf split propagation into the
root, explicit borrowed-page copying, and unique obsolete-page ownership.
[Decision 0556](../Decisions/0556-Depth-Three-Root-Growth-And-Internal-Branch-Split.md)
adds deterministic full-branch splitting and the first depth-three root without
changing any durable format or publication action.
[Decision 0568](../Decisions/0568-Existing-Depth-Three-Upsert-And-Bounded-Cascade.md)
then updates a selected depth-three root/branch/leaf path and propagates one
split cascade through a new depth-four root.
[Decision 0569](../Decisions/0569-Bounded-Owned-Tree-Path-Upsert.md) replaces
the fixed-height composition with one owned exact-page path for input depths
two through eight and demonstrates a four-level cascade into depth five.
[Decision 0572](../Decisions/0572-Provider-Driven-Durable-Tree-Writer.md) then
adds one hosted mutation operation that discovers and copies the exact path,
rejects snapshot changes, invokes that portable transaction, and executes the
four-action publication with explicit active, committed, aborted, and recovery
outcomes. Deeper inputs and reclamation remain future engine work.
[Decision 0575](../Decisions/0575-Single-Writer-Database-Engine-Lifecycle.md)
adds one exact open/recovery snapshot shared by the hosted read and write
projections. Those projections remain separate native targets under the
ordinary object bound while exposing the same admitted current generation.
[Decision 0664](../Decisions/0664-Physical-Database-Leaf-Delete-And-Bounded-Scan.md)
adds exact physical leaf removal and one bounded ascending leaf scan without
tombstones or per-row copies. [Decision 0668](../Decisions/0668-Durable-Full-Path-Database-Delete.md)
then composes deletion through every selected ancestor into one deterministic
commit, gives missing deletion no publication, and admits a validated empty
durable leaf that the ordinary upsert path can refill.

The first database can avoid requiring cross-platform atomic file replacement
by publishing commits inside one storage object: write new pages, flush their
content, write one checksummed alternate superblock/root record, then flush
again. Recovery selects the newest completely valid generation. Backup and
whole-file replacement remain later optional interfaces with their own
directory-durability contracts.

## Proposed implementation sequence

### Stage 0: specification, provenance, and performance ledger

Before implementation:

- identify the accepted WVDB requirement and specification for the slice;
- record exact source and evidence hashes for every reused or comparative
  artifact;
- inventory every selected optimization, invariant, benchmark, crash boundary,
  and malformed-input case; and
- classify application-specific or compatibility behavior that is intentionally
  excluded.

Any separately licensed material requires an explicit provenance and licensing
record. Comparative evidence alone does not authorize source transfer.

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

The storage substrate through the first depth-three Stage 3 vertical slice is
implemented. `WVDS 1` supplies dual checksummed root records and deterministic
recovery selection. `WVPG 1`, `WVCR 1`, and the pure publication planner add
immutable page/log bytes and an executable durable-before-publish sequence.
`WVTN 1` now adds variable-key leaf lookup, replacement, and insertion, and the
hosted executor publishes that structured root through every four-action crash
boundary. It splits one full root leaf, repeatedly rewrites the depth-two tree,
splits a full branch root into two branches beneath a new depth-three root, and
now accepts exact owned root-to-leaf page paths for input depths two through
eight. The bounded transaction validates top-down, rebuilds bottom-up, propagates
splits through every supplied ancestor, updates existing depth-four generations,
and can create a depth-five root. The persistent hosted writer now discovers
one provider-backed path per canonical mutation and publishes the whole set in
one commit. The first portable secondary-index contract now adds bounded compound
definitions and deterministic ordered keys for Boolean, I64, U64, and text
fields, including explicit null placement and safe unique-owner key shapes. A
bounded bundle and pure upsert planner now compose old-entry deletes, new-entry
puts, and the primary row into one sorted transaction while emitting explicit
unique-owner checks. The hosted writer does not yet execute those checks, discover
persisted bundles, accept input depth nine, reclaim pages, queue concurrent
requests, or manage concurrent readers.

This milestone needs no general query language, network protocol, or graph
layer.

### Stage 4: measured performance mechanisms

Add, one independently measured boundary at a time:

- generation-pinned committed snapshots and structurally shared roots;
- a process-wide byte-budgeted page buffer with exact pin and eviction evidence;
- hosted secondary-index bundle discovery and unique-owner check execution;
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

## Agent-runtime consumer profile

The proposed [Windvale agent runtime implementation plan](Windvale-Agent-Runtime-Implementation-Plan.md)
names a future durable-run consumer of the database contract. This does not change
the database's current milestones or claim that the consumer is implemented. The
agent's deterministic stages remain capability-free; persistence begins only after
the database writer and recovery surface they need is independently qualified.

The first durable agent profile requires:

- an append-only event stream per run with an expected prior revision;
- exact idempotency-key replay and same-key/different-body conflict outcomes;
- ordered bounded replay from a known revision;
- immutable checkpoint or snapshot publication tied to the admitted event prefix;
- stable references for context or evidence bodies too large for the bounded event
  record; and
- explicit stale generation, unavailable provider, partial progress, durable
  completion, and indeterminate completion outcomes.

The later functional-mind profile adds:

- one persistent agent-self root above episode/run roots;
- governance profile/charter and authority-manifest generations, role/roster
  assignments, exact-revision approvals, amendments, dissents, suspensions,
  transitions, exports, restores, succession, archival, and retirement evidence;
- bounded episode membership and autobiographical links;
- prospective intentions, eligibility conditions, satisfaction, cancellation,
  expiry, and wake identities;
- recurrent workspace-cycle, selection, prediction, and prediction-error
  evidence;
- working, episodic, semantic, procedural, prospective, and autobiographical
  memory records with consolidation/reconsolidation lineage; and
- derived world, belief, self-model, salience, and simulation indexes that can be
  rebuilt from admitted evidence.

The self and each episode retain explicit revision owners. One self may contain
many episodes without implying concurrent multi-writer mutation of one root.
Wake admission and intention arbitration require idempotency and expected
revision just as model calls and actions do.

Later memory and retrieval stages may add indexes over attributed claims,
provenance, support state, contradiction, dependent artifacts, source and
projection generation, action identity, review state, recency, expiry, and access
scope. Functional-mind pressure may later add self, episode, intention,
prospective condition, wake, memory-kind, belief-support, calibration, and
prediction-error indexes. Those indexes must remain derived from canonical admitted records and
rebuildable without changing their meaning. The first durable product profile
also requires a backup manifest that binds its event prefix, snapshots,
idempotency evidence, and large-body references, plus a restore result that
rejects missing or changed dependencies.

The first profile does not require SQL, a network server, distributed consensus,
multi-writer runs, vector search, or database-owned agent policy. The database
stores and retrieves bounded state and evidence-bearing records; it does not
decide what a model may believe, which intention deserves attention, which
projection is sufficient, what should be consolidated, who constitutionally
owns a self field, or what an agent may do. The proposed
[persistent-self governance architecture](../Architecture/Persistent-Self-Ownership-And-Governance.md)
owns that later semantic direction; this proposal supplies storage only after a
consumer freezes exact bounded records.

## Organizational-Observatory consumer profile

The proposed
[Windvale Observatory architecture](../Architecture/Organizational-Observatory-And-Epistemic-Infrastructure.md)
and
[implementation plan](Windvale-Organizational-Observatory-Implementation-Plan.md)
name a second future consumer of the database contract. Stages 0 through 3
remain capability-free, use a static synthetic corpus, and do not require
durable storage. A durable mapping begins only after the Observatory freezes
exact observation, evidence, claim, support, invalidation, decision, and
artifact records and the database writer and recovery surface are independently
qualified.

The first durable Observatory profile likely requires:

- expected-revision append for each organization or bounded knowledge domain;
- immutable observation, evidence, source-revision, and large-body records;
- exact idempotency replay/conflict outcomes, ordered replay, and snapshots;
- measured indexes for source/revision, claim/support/status, dependency,
  validity interval, workplace, subject, decision, artifact, job, and revisit
  membership;
- deterministic invalidation and review queues when a source, policy,
  calculation, or admitted dependency changes; and
- backup and restore evidence binding canonical records, derived indexes,
  snapshots, idempotency state, and referenced bodies.

Those lookups are planning pressure, not permission to add a general graph,
unbounded query language, distributed writer, or vector-search contract. Exact
records and measured fixtures must select each index.

The database stores and retrieves canonical and derived records. It does not
decide which source has authority, whether evidence supports a claim, whether a
claim becomes accepted organizational knowledge, which professional judgment
applies, how deliberation reaches a recommendation, or whether an organizational
action is authorized. Organization-owned Observatory roots and agent-owned
persistent-self roots remain distinct even if one engine stores both; a storage
path or index cannot transfer ownership between them.

## Differential and performance evidence

WVDB conformance is defined by its specifications and simple Windvale-owned
oracles. A selected external system may participate in an isolated comparative
workload when the comparison answers a named question, but it is never the
semantic oracle or a hidden runtime dependency. Production dual-write is not
implied.

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

The first goals are exact semantics, crash safety, bounded resource use,
reproducibility, and a coherent native path. Numeric performance targets should
be approved only after a complete WVDB path can run representative workloads.

External comparisons are optional and question-driven. A comparison may select
one or more representative relational, embedded, document, graph, or analytical
systems appropriate to the workload; the selected products do not become a
fixed parity set. Each result must name durability settings, connection and
process shape, warm-up, data size, key/value distribution, concurrency, host,
storage, and software versions. Reports retain throughput, median and tail
latency, peak memory, database bytes, and observed I/O. Default safe settings
are reported separately from tuned profiles, and results from different host or
storage profiles are never pooled.

## Non-goals of this proposal

Under the active WVDB direction, this historical proposal does not:

- define the complete WVDB 1.0 specification or repository layout;
- select a complete on-disk format, query grammar, isolation level, or wire
  protocol;
- establish data, API, query, protocol, file, or operational parity with an
  external database;
- claim replication, clustering, failover, or distributed consensus;
- make a host language, framework, operating system, or filesystem behavior
  the database definition; or
- authorize `.wv` source that cannot compile, verify, and execute within its
  stated resource bounds.

## Current disposition

Decision 0790 accepts WVDB as a Windvale-owned system implemented in Windvale
Language 1.0. Decision 0791 accepts one explicit primary identity per entity
set. The [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md) owns the
current design sequence, and the
[upper-layer decision register](WVDB-1.0-Upper-Layer-Decision-Register.md)
presents the unresolved choices with alternatives and tradeoffs.

The implementation inventory in this proposal remains evidence about current
candidate mechanisms and missing lower-layer work. It is not the product
definition, compatibility baseline, or active implementation roadmap.
