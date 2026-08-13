# Windvale libraries

## Status

This tree owns reusable Windvale APIs and implementations. [Decision 0140](../Documents/Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) defines the durable platform/capability direction; [Decision 0145](../Documents/Decisions/0145-First-Capability-Bearing-Static-Library.md) implements the first bounded capability-bearing static-library slice; [Decision 0153](../Documents/Decisions/0153-First-Versioned-Read-Only-Directory-Capability.md) implements the first rights-limited read-only directory operation; [Decision 0210](../Documents/Decisions/0210-First-Hosted-Wvdb-Snapshot-Consumer.md) composes that operation with the experimental portable database reader; [Decision 0211](../Documents/Decisions/0211-U64-Database-Storage-Geometry.md) adds format-neutral checked `u64` page geometry; [Decision 0212](../Documents/Decisions/0212-First-Preopened-Random-Access-Storage.md) adds the first pre-opened mutable `u64` storage resource; and [Decision 0529](../Documents/Decisions/0529-Native-Capability-Bearing-Library-Composition.md) makes these capability-bearing compositions native-owned build inputs.

The current compiler still uses `portable`, `hosted`, and `system` as a coarse compatibility and authority boundary. Independent platform scope, optional capabilities, typed capability values, provider binding metadata, and runtime module linking are not implemented.

The [post-retirement language and library stage](../Documents/Project/Post-Dotnet-Retirement-Language-And-Libraries.md)
now has a native foundation: Project 2 manifests build each current library,
capability-bearing dependencies compose under an explicit root approval rule, and
the changed-file planner has a focused native library owner. The first exact
`local-source-1` package now composes the WVDB Query application from checked-in
source parts. General package resolution, binary library distribution, and runtime
WVB linking remain later contracts.

## Layers

- `Foundation/` contains deterministic capability-free algorithms and values. `Foundation/Resources/Resource-Store.wv` owns portable `WVRS 1` validation and lookup.
- `Database/` contains reusable capability-free database algorithms. `Storage-Geometry.wv` owns checked zero-based `u64` page-range arithmetic, while `Wvdb-Reader.wv` remains the bounded experimental snapshot reader.
- `Platform/` contains application-facing adapters over semantic capabilities. `Platform/Resources/Hosted-Resource-Store.wv` obtains store bytes through the bounded `file.read_bytes` bootstrap leaf and delegates format policy to Foundation. `Platform/Filesystem/Read-Only-Directory.wv` owns a typed 3 KiB read-at API over one pre-bound immutable directory instance. `Platform/Storage/Random-Access-Storage.wv` owns the typed mutable `u64` storage-resource boundary. `Platform/Database/Read-Only-Wvdb.wv` composes the immutable directory provider with the portable experimental WVDB reader while keeping storage and database failures distinct and owning the complete public failure vocabulary exposed by its facade.
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
| `Windvale-Library-Wvdb-Reader` | `Windvaleˉdatabaseˉreader` | portable / none |
| `Windvale-Library-Hosted-Resource-Store` | `Hostedˉresourceˉstore` | hosted / `file.read_bytes` |
| `Windvale-Library-Read-Only-Directory` | `Readˉonlyˉdirectory` | hosted / `filesystem.directory_read_v1` |
| `Windvale-Library-Random-Access-Storage` | `Randomˉaccessˉstorage` | hosted / `storage.random_access_v1` |
| `Windvale-Library-Read-Only-Wvdb` | `Readˉonlyˉwvdb` | hosted / `filesystem.directory_read_v1` |

`Tools/Native/Test-Libraries` builds all seven projects, builds one positive
capability-bearing importer and two database conformance applications, and rejects
missing root approval and incompatible profile imports. Its completion contract is
12 cases. The suite uses only the native Project 2 build and publication path.

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
The new mutable storage capability is deliberately format-neutral; the next
WVDB adapter must compose it with checked page geometry and validation.
Internal commit publication, recovery, mutation identities, page ownership,
transactions, and concurrency remain database-layer work.
