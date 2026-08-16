# Windvale libraries

## Status

[Decision 0573](../Documents/Decisions/0573-First-Provider-Neutral-Model-Protocol.md)
adds the first capability-free external-model protocol and deterministic
scripted provider. [Decision 0583](../Documents/Decisions/0583-First-Native-Bound-Model-Provider.md)
adds the offline hosted facade and exact native provider binding.
[Decision 0585](../Documents/Decisions/0585-Catchable-Model-Provider-Lifecycle-Results.md)
adds catchable revoked, stale, peer-exited, and submission-indeterminate results
while retaining ABI failure for an untrustworthy bridge call.
[Decision 0587](../Documents/Decisions/0587-First-Bounded-Operation-Deadline-And-Cancellation-Core.md)
adds the shared capability-free operation, virtual deadline, cancellation,
bounded wait-batch, reserved close, and teardown core required before networking.
[Decision 0704](../Documents/Decisions/0704-First-Portable-Standard-Byte-Output-Core.md)
specializes that lifecycle into exact bounded standard-output bytes,
backpressure, peer consumption, and cleanup for the next shell slice.

This tree owns reusable Windvale APIs and implementations. [Decision 0140](../Documents/Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) defines the durable platform/capability direction; [Decision 0145](../Documents/Decisions/0145-First-Capability-Bearing-Static-Library.md) implements the first bounded capability-bearing static-library slice; [Decision 0153](../Documents/Decisions/0153-First-Versioned-Read-Only-Directory-Capability.md) implements the first rights-limited read-only directory operation; [Decision 0210](../Documents/Decisions/0210-First-Hosted-Wvdb-Snapshot-Consumer.md) composes that operation with the experimental portable database reader; [Decision 0211](../Documents/Decisions/0211-U64-Database-Storage-Geometry.md) adds format-neutral checked `u64` page geometry; [Decision 0212](../Documents/Decisions/0212-First-Preopened-Random-Access-Storage.md) adds the first pre-opened mutable `u64` storage resource; [Decision 0529](../Documents/Decisions/0529-Native-Capability-Bearing-Library-Composition.md) makes these capability-bearing compositions native-owned build inputs; [Decisions 0534](../Documents/Decisions/0534-First-Durable-Database-Superblock.md), [0535](../Documents/Decisions/0535-First-Durable-Database-Commit.md), and [0536](../Documents/Decisions/0536-Nested-Records-And-Database-Storage-Recovery.md) establish the first durable database metadata, publication, and reopen-repair slices; [Decision 0549](../Documents/Decisions/0549-Bounded-Durable-Tree-Reader-And-Root-Split.md) adds provider-backed depth-bounded lookup plus the first multi-page root split; [Decision 0551](../Documents/Decisions/0551-General-Depth-Two-Upsert-And-Obsolete-Ownership.md) adds repeated routed-leaf replacement, split propagation, and unique obsolete-page ownership; [Decision 0556](../Documents/Decisions/0556-Depth-Three-Root-Growth-And-Internal-Branch-Split.md) adds deterministic internal branch splitting and the first depth-three root; [Decision 0568](../Documents/Decisions/0568-Existing-Depth-Three-Upsert-And-Bounded-Cascade.md) adds updates inside an existing depth-three generation with one bounded internal split cascade; [Decision 0569](../Documents/Decisions/0569-Bounded-Owned-Tree-Path-Upsert.md) generalizes that transaction to an owned path of depth two through eight; [Decision 0572](../Documents/Decisions/0572-Provider-Driven-Durable-Tree-Writer.md) composes provider traversal, owned-path mutation, publication, and restart recovery; [Decision 0575](../Documents/Decisions/0575-Single-Writer-Database-Engine-Lifecycle.md) adds the shared exact open/recovery state for separate read and write projections; and [Decision 0600](../Documents/Decisions/0600-Focused-Logical-Multi-Level-Database-Writer.md) adds the focused logical write projection and restart-read evidence without merging near-limit native objects.

The current compiler still uses `portable`, `hosted`, and `system` as a coarse compatibility and authority boundary. Independent platform scope, optional capabilities, typed capability values, provider binding metadata, and runtime module linking are not implemented.

The [post-retirement language and library stage](../Documents/Project/Post-Dotnet-Retirement-Language-And-Libraries.md)
now has a native foundation: Project 2 manifests build each current library,
capability-bearing dependencies compose under an explicit root approval rule, and
the changed-file planner has a focused native library owner. The first exact
`local-source-1` package now composes the WVDB Query application from checked-in
source parts. General package resolution, binary library distribution, and runtime
WVB linking remain later contracts.

## Layers

- `Foundation/` contains deterministic capability-free algorithms and values. `Foundation/Resources/Resource-Store.wv` owns portable `WVRS 1` validation and lookup. `Foundation/Operations/Bounded-Operation-Core.wv` owns generation-bound operations, virtual monotonic deadlines, exact terminal results, and a bounded event queue whose normal traffic cannot consume its cancellation and teardown reservations.
- `Database/` contains reusable capability-free database algorithms. `Storage-Geometry.wv` owns checked zero-based `u64` page-range arithmetic, `Storage-Page.wv` owns immutable read plans and exact response admission, `Durable-Superblock.wv` owns `WVDS 1` encoding and dual-slot recovery selection, `Durable-Page.wv` owns `WVPG 1` physical pages plus explicit borrowed-to-owned copying, `Durable-Commit-Record.wv` owns `WVCR 1` commit linkage, `Commit-Publication.wv` owns the durable-before-publish state machine, `Commit-Batch.wv` validates and publishes up to 63 immutable data pages plus one log page with unique committed predecessors, `Storage-Publication.wv` maps that state to bounded storage actions, `Storage-Recovery.wv` selects and repairs an unpublished tail without uncertain mutation replay, `Tree-Node.wv` owns canonical `WVTN 1` leaf/branch operations, `Tree-Branch-Split.wv` owns deterministic full-branch split propagation, `Root-Split-Upsert.wv` owns the first leaf split and branch-root transaction, `Depth-Two-Upsert.wv` owns repeated routed-leaf replacement and split propagation, `Depth-Three-Root-Growth.wv` owns the first height increase, `Depth-Three-Upsert.wv` owns updates and one bounded split cascade inside depth three, `Tree-Path-Upsert.wv` owns bounded depth-two-through-eight split propagation, `Tree-Path-Delete.wv` owns bounded full-path physical deletion, and `Wvdb-Reader.wv` remains the bounded experimental snapshot reader.
- `Models/` contains the capability-free `WVMM 1`, `WVMQ 1`, `WVMC 1`, and
  `WVMG 1` model protocol plus a deterministic scripted provider. Its status
  vocabulary distinguishes temporary unavailability, revocation, stale
  generation, pre-dispatch peer exit, and indeterminate post-dispatch
  submission. It owns no
  credentials, network access, JSON, live provider adapter, or model-routing
  policy.
- `Network/` contains capability-free public network values and policy.
  `Address-Authority.wv` owns canonical IPv4/IPv6 values, prefixes, scoped
  endpoints, canonical service names, exact peer selectors, bounded grants,
  and fail-closed rights reduction. It owns no resolver, socket, TLS, HTTP,
  interface discovery, or host authority.
- `Platform/` contains application-facing adapters over semantic capabilities. `Platform/Streams/Standard-Byte-Output-Core.wv` owns the capability-free exact-byte lifecycle, accepted/consumed/released accounting, backpressure, peer close, provider invalidation, and cleanup shared by future host, browser, and OS standard-output providers. `Platform/Models/Bound-Model-Provider.wv` validates canonical requests before dispatch and independently admits catalog and inference responses from one bound provider. `Platform/Resources/Hosted-Resource-Store.wv` obtains store bytes through the bounded `file.read_bytes` bootstrap leaf and delegates format policy to Foundation. `Platform/Filesystem/Read-Only-Directory.wv` owns a typed 3 KiB read-at API over one pre-bound immutable directory instance. `Platform/Storage/Random-Access-Storage.wv` owns the typed mutable `u64` storage-resource boundary. `Platform/Database/Random-Access-Page.wv` composes that boundary with the portable page core while preserving distinct page and provider failures. `Platform/Database/Durable-Database-Engine.wv` owns exact header admission, current selection, bounded tail recovery, and reopen validation. `Platform/Database/Durable-Tree-Reader.wv` performs at most 32 provider-backed page visits while proving the selected generation, page graph, node kinds, and inherited key range. `Platform/Database/Durable-Tree-Writer.wv` performs one validated read per admitted level, closes provider-borrow and arena-tail ownership seams, invokes the portable transaction, and executes bounded publication without retrying uncertain mutation. `Platform/Database/Durable-Logical-Tree-Writer.wv` adds canonical logical write preparation without pulling read and session code into the near-limit writer object. `Platform/Database/Native-Hosted-Snapshot-Page.wv` is the narrow native transition provider: it obtains one bounded immutable host snapshot through `file.read_bytes`, then admits one checked page through the same portable core. `Platform/Database/Read-Only-Wvdb.wv` composes the immutable directory provider with the portable experimental WVDB reader while keeping storage and database failures distinct and owning the complete public failure vocabulary exposed by its facade.
- `Protocol/` is reserved until a reusable bounded provider/service wire contract moves here from a concrete implementation.
- `System/` is reserved until a reusable privileged kernel, driver, or machine API has an implemented owner and contract.

Do not add empty scaffolding for a planned layer. Add a directory only with its first owned implementation and specification.

## Current build inventory

The checked-in Project 2 manifests under `Projects/Libraries/` are the canonical
native build inputs for the current reusable modules:

| Project | Root module | Profile / direct capability |
| --- | --- | --- |
| `Windvale-Library-Resource-Store` | `Resourceˉstore` | portable / none |
| `Windvale-Library-Database-Storage-Geometry` | `Windvaleˉdatabaseˉstorageˉgeometry` | portable / none |
| `Windvale-Library-Database-Storage-Page` | `Windvaleˉdatabaseˉstorageˉpage` | portable / none |
| `Windvale-Library-Database-Durable-Superblock` | `Windvaleˉdatabaseˉdurableˉsuperblock` | portable / none |
| `Windvale-Library-Database-Durable-Page` | `Windvaleˉdatabaseˉdurableˉpage` | portable / none |
| `Windvale-Library-Database-Durable-Commit-Record` | `Windvaleˉdatabaseˉdurableˉcommitˉrecord` | portable / none |
| `Windvale-Library-Database-Commit-Publication` | `Windvaleˉdatabaseˉcommitˉpublication` | portable / none |
| `Windvale-Library-Database-Commit-Batch` | `Windvaleˉdatabaseˉcommitˉbatch` | portable / none |
| `Windvale-Library-Database-Storage-Publication` | `Windvaleˉdatabaseˉstorageˉpublication` | portable / none |
| `Windvale-Library-Database-Storage-Recovery` | `Windvaleˉdatabaseˉstorageˉrecovery` | portable / none |
| `Windvale-Library-Database-Single-Writer-Commit` | `Windvaleˉdatabaseˉsingleˉwriterˉcommit` | portable / none |
| `Windvale-Library-Database-Single-Leaf-Upsert` | `Windvaleˉdatabaseˉsingleˉleafˉupsert` | portable / none |
| `Windvale-Library-Database-Tree-Node` | `Windvaleˉdatabaseˉtreeˉnode` | portable / none |
| `Windvale-Library-Database-Tree-Branch-Split` | `Windvaleˉdatabaseˉtreeˉbranchˉsplit` | portable / none |
| `Windvale-Library-Database-Root-Split-Upsert` | `Windvaleˉdatabaseˉrootˉsplitˉupsert` | portable / none |
| `Windvale-Library-Database-Depth-Two-Upsert` | `Windvaleˉdatabaseˉdepthˉtwoˉupsert` | portable / none |
| `Windvale-Library-Database-Depth-Three-Root-Growth` | `Windvaleˉdatabaseˉdepthˉthreeˉrootˉgrowth` | portable / none |
| `Windvale-Library-Database-Depth-Three-Upsert` | `Windvaleˉdatabaseˉdepthˉthreeˉupsert` | portable / none |
| `Windvale-Library-Database-Tree-Path-Upsert` | `Windvaleˉdatabaseˉtreeˉpathˉupsert` | portable / none |
| `Windvale-Library-Local-Database-Contracts` | `Windvaleˉdatabaseˉlocalˉcontracts` | portable / none |
| `Windvale-Library-Local-Database-Session` | `Windvaleˉdatabaseˉlocalˉsession` | portable / none |
| `Windvale-Library-Local-Database-Put` | `Windvaleˉdatabaseˉlocalˉput` | portable / none |
| `Windvale-Library-Local-Database-Get` | `Windvaleˉdatabaseˉlocalˉget` | portable / none |
| `Windvale-Library-Local-Database-Control` | `Windvaleˉdatabaseˉlocalˉcontrol` | portable / none |
| `Windvale-Library-Wvdb-Reader` | `Windvaleˉdatabaseˉreader` | portable / none |
| `Windvale-Library-Hosted-Resource-Store` | `Hostedˉresourceˉstore` | hosted / `file.read_bytes` |
| `Windvale-Library-Read-Only-Directory` | `Readˉonlyˉdirectory` | hosted / `filesystem.directory_read_v1` |
| `Windvale-Library-Random-Access-Storage` | `Randomˉaccessˉstorage` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Random-Access-Database-Page` | `Randomˉaccessˉdatabaseˉpage` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Storage-Executor` | `Durableˉstorageˉexecutor` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Database-Engine` | `Durableˉdatabaseˉengine` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Tree-Reader` | `Durableˉtreeˉreader` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Root-Writer` | `Durableˉrootˉwriter` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Root-Split-Writer` | `Durableˉrootˉsplitˉwriter` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Local-Open` | `Durableˉlocalˉopen` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Local-Root-Put` | `Durableˉlocalˉrootˉput` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Local-Get` | `Durableˉlocalˉget` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Tree-Writer` | `Durableˉtreeˉwriter` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Durable-Logical-Tree-Writer` | `Durableˉlogicalˉtreeˉwriter` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Native-Hosted-Snapshot-Page` | `Nativeˉhostedˉsnapshotˉpage` | hosted / `file.read_bytes` |
| `Windvale-Library-Read-Only-Wvdb` | `Readˉonlyˉwvdb` | hosted / `filesystem.directory_read_v1` |
| `Windvale-Library-Bounded-Operation-Core` | `Windvaleˉboundedˉoperationˉcore` | portable / none |
| `Windvale-Library-Standard-Byte-Output-Core` | `Windvaleˉstandardˉbyteˉoutputˉcore` | portable / none |
| `Windvale-Library-Standard-Byte-Output-Response-Core` | `Windvaleˉstandardˉbyteˉoutputˉresponseˉcore` | portable / none |
| `Windvale-Library-Standard-Byte-Output` | `Windvaleˉstandardˉbyteˉoutput` | hosted / `standard_output.write_v1` |
| `Windvale-Library-Network-Address-Authority` | `Windvaleˉnetworkˉaddressˉauthority` | portable / none |
| `Windvale-Library-Model-Protocol` | `Windvaleˉmodelˉprotocol` | portable / none |
| `Windvale-Library-Scripted-Model-Provider` | `Windvaleˉscriptedˉmodelˉprovider` | portable / none |
| `Windvale-Library-Bound-Model-Provider` | `Boundˉmodelˉprovider` | hosted / `model.catalog_v1`, `model.inference_v1` |

`Tools/Native/Test-Libraries` builds the sixteen reusable projects accepted by
the ordinary pinned source front door, three positive capability-bearing
importers, and eight conformance applications, then rejects missing root
approval and incompatible profile imports. Its completion contract is 29
cases. Newer durable-composition manifests that require the current nested-
record/storage compiler closure are owned by `Test-Database-Storage`, which
reconstructs that compiler natively before building them. Promoting that
verified compiler generation into the ordinary front door is a separate
product-identity milestone. The bounded operation core uses that current
nested-record closure and has its own deterministic native owner,
`Test-Bounded-Operation-Core`. The first network value/permission module also
uses that current closure and is owned by `Test-Network-Address-Authority`.
All four suites avoid .NET and C#.

## Module names and local namespaces

Repository paths are the current durable ownership hierarchy: layer first,
then a focused domain and implementation. Source modules remain explicit named
units rather than mirroring a long filesystem path. An importer assigns a
local alias and uses that alias as its namespace, for example:

```windvale
import Randomˉaccessˉstorage as Storage;

let Result: Storage.Storageˉresult = Storage.Storageˉreadˉat(
    Generation,
    Offset,
    Maximum
);
```

This keeps call sites concise while the compiler enforces module ownership and
qualified access. Folder names are not silently converted into source names,
and moving a source file does not by itself redefine its module identity.
Package 1 now gives the selected distribution a globally unique package identity
and exact part graph, but source imports still name explicit modules internalized by
Project 2. A future general package resolver must map those imports unambiguously;
it should not require every source reference to repeat a broad global namespace.
Module names still use Windvale source naming, import aliases define the local
vocabulary, and capability/package/ABI identities retain their separately
specified ASCII-safe forms.

The post-retirement proposal retains this rule: prefer a focused facade only when
it owns a small coherent family of operations. Do not turn `Platform/Filesystem`
into one broad `System.IO` equivalent; directories, random-access storage,
durability, watch, permissions, and native extensions have different capability,
failure, and portability contracts.

## Static capability rules

Imported libraries remain statically internalized into one canonical WVB. Every library function must be exported at its source boundary, imported libraries may not own module data in this slice, and the root alone determines final WVB exports.

Profile import compatibility is currently monotonic: portable imports portable; hosted imports portable or hosted; system imports any current profile. Every module must explicitly redeclare every capability required by its complete dependency closure. This is compile-time approval, not a runtime grant. The launcher or service manager must still authorize every final WVB capability before execution.

## Filesystem libraries

The existing `file.read_bytes` and `file.write_bytes` leaves are bounded host-tool resource adapters. They accept opaque host-resolved names and are not the future filesystem API.

The first standard byte-output interface is
[`standard_output.write_v1`](../Specifications/Standard-Byte-Output-Capability.md):
one rights-limited stream, exact byte prefixes, typed pre-dispatch and
post-dispatch failures, generation checks, and no native handle or text
conversion. Its portable response decoder and output state machine remain
separate from the hosted capability adapter.

The first filesystem interface is [`filesystem.directory_read_v1`](../Specifications/Read-Only-Directory-Capability.md): one rights-limited immutable directory snapshot, strict single-segment names, `u32` offsets, exact chunks of at most 3,072 bytes, typed lifecycle failures, and no native paths or handles. The reference `windvale run` launcher can bind a bounded eager Windows/Linux snapshot with `--bind-read-only-directory <path>`; the capability still requires a separate `--allow`. Its `_v1` suffix is a temporary compatibility encoding until WVB carries independent capability-name/version metadata.

Further filesystem libraries must preserve versioned semantic capability interfaces and rights-limited instances. Shared contracts must define path segments, traversal, name comparison, normalization, collision behavior, operation bounds, offsets, partial progress, close/revocation, provider loss, and recoverable results. Atomic replacement, durability strengths, watching, links, permissions, mapping, sparse storage, transactions, and native host behavior belong in separate optional or platform-scoped interfaces.

The first mutable storage interface is
[`storage.random_access_v1`](../Specifications/Random-Access-Storage-Capability.md):
one pre-opened object, `u64` generation/positions/length, exact bounded reads,
positioned writes, resize, explicit mutation completion, and content or
content-and-length flush. The reference launcher binds one existing ordinary
Windows/Linux file with `--bind-random-access-storage <path>` while retaining a
separate `--allow`. It is an object-storage contract for applications such as
WVDB, not an ambient native filesystem API or a database-specific interface.

Package resources, immutable `WVRS 1` images, mutable application storage, and native host files remain distinct concepts even when one provider stores them on the same host filesystem. Ordinary applications should call typed platform libraries, not raw IPC envelopes, syscalls, native paths, handles, or file descriptors.

The hosted `Readˉonlyˉwvdb` adapter remains a bounded consumer of the immutable
directory contract. Its 16,416-byte ceiling comes from the experimental reader.
The mutable storage capability remains format-neutral. The checked
[`Databaseˉstorageˉpage`](../Specifications/Windvale-Database-Storage-Page.md)
core and hosted `Randomˉaccessˉdatabaseˉpage` facade now compose it into one
exact page read while preserving generation, stable length, and typed failure
invariants. Native x64 now lowers the required checked `u64` subset. The
transition `Nativeˉhostedˉsnapshotˉpage` provider executes the same core over
the existing bounded Windows/Linux native file-input leaves, with the resource
name supplied by the host-tool argument binding. It is an immutable snapshot
bridge, not the eventual pre-opened `storage.random_access_v1` native service.
Page-format validation, a widened native service table, internal commit
publication, recovery, mutation identities, page ownership, transactions, and
concurrency remain database-layer work.

The portable [`Databaseˉsuperblock`](../Specifications/Windvale-Database-Durable-Superblock.md)
contract now fixes the first two-slot durable publication target and recovery
selector. The connected [durable commit contract](../Specifications/Windvale-Database-Durable-Commit.md)
adds `WVPG 1` page bytes, `WVCR 1` commit linkage, and the exact append/flush/
inactive-slot/flush planner. The [storage recovery contract](../Specifications/Windvale-Database-Storage-Recovery.md)
then maps publication to bounded 64 KiB storage actions and maps reopen evidence
to exact tail resize/flush actions. Partial, indeterminate, stale-generation,
and changed-storage observations enter reopen instead of silently retrying.
The tree layer now publishes the first depth-two generation, repeatedly
replaces or splits a routed leaf while preserving untouched children, and
grows a full branch root to depth three through a deterministic internal split.
Multi-page hosted
mutation uses the durable-page owned-copy operation before another provider
call invalidates its borrowed response. Each replacement page uniquely names
the old committed page it makes obsolete; reclamation and repeated depth-three
updates are not yet implemented.

These portable modules have no I/O authority. The focused ABI-23 host now binds
one rights-limited `storage.random_access_v1` object, a process-lifetime writer
fence, and exact Windows/Linux leaves. Windows executes create, publish,
reopen, deterministic tail repair, and byte-stable reopen; the equivalent Linux
application is constructed pending independent execution. This remains a fixed
test shell, not the configurable database-server binding. The focused native
owners pin the deterministic WVB, WVO, linked-image, and Windows/Linux hosted
artifacts.
