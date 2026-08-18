# Workload 9 package and execution plan

## Mapping

Package identity: `windvale.paper.language1.package_graph` version 1.

| File | Module | Profile | Authority |
| --- | --- | --- | --- |
| `Source/Package-Graph-Package.wv` | `Packageˉgraphˉpackage` | Core | library |
| `Source/Package-Graph-Types.wv` | `Packageˉgraphˉtypes` | Core | library |
| `Source/Package-Graph-Ordering.wv` | `Packageˉgraphˉordering` | Core | library |
| `Source/Package-Graph-Parser.wv` | `Packageˉgraphˉparser` | Core | library |
| `Source/Package-Graph-Graph.wv` | `Packageˉgraphˉgraph` | Core | library |
| `Source/Package-Graph-Report.wv` | `Packageˉgraphˉreport` | Core | library |
| `Source/Package-Graph-Application.wv` | `Packageˉgraphˉapplication` | Core | application |

All modules target Windows, Linux, and Windvale. The launcher selects exported
`Packageˉgraphˉapplication.Run` with one owned memory budget and one limits
record. No capability root is bound.

## Exact package-data bindings

| Declaration | Type | Maximum | Exact bytes | SHA-256 | Content object |
| --- | --- | ---: | ---: | --- | --- |
| `Packageˉgraphˉpackage.Manifest` | text | 1,024 | 63 | `4efd42dd1e69bf39a237201fd8db7aa1b3e902b534e528b5cd41a0a4c96500ef` | `sha256:4efd42dd…00ef` |
| `Packageˉgraphˉpackage.Lock` | text | 2,048 | 111 | `7c01e9d4691a43650879dc3d6bb4d9cd2c17e1bbffcd86756ed932f8d3c9f3c5` | `sha256:7c01e9d4…f3c5` |
| `Packageˉgraphˉpackage.Noticeˉprimary` | bytes | 128 | 53 | `4acfdf3d97fb998f66f809cdbcb873ef85eded9c1d726601fbde0f78ca664708` | `sha256:4acfdf3d…4708` |
| `Packageˉgraphˉpackage.Noticeˉcopy` | bytes | 128 | 53 | `4acfdf3d97fb998f66f809cdbcb873ef85eded9c1d726601fbde0f78ca664708` | `sha256:4acfdf3d…4708` |

The canonical package table contains four typed declaration references and
three content objects. Unique shipped content is 227 bytes; naively duplicating
each declaration would ship 280 bytes. Both notice declarations validate their
own type, maximum, length, digest, and binding identity before publication, but
reference the same immutable content object.

Within one application resource domain, one admitted distinct content identity
has one retained-content charge. A second declaration reference adds only its
bounded reference metadata; it cannot create a second 53-byte payload charge.
Separate application/service domains each account for the content they admit;
deduplication cannot transfer authority or evade a domain limit.

## Bounds

| Limit | Reference | Hard paper ceiling |
| --- | ---: | ---: |
| manifest bytes | 63 | 1,024 |
| lock bytes | 111 | 2,048 |
| distinct packages | 4 | 64 |
| dependencies per package | 2 | 32 |
| identity bytes | 5 | 64 |
| graph edges | 4 | 2,048 |
| topology scans | 4 | 65 |
| retained cycle identities | 0 | 64 |
| diagnostics | 0 | 32 |
| report budget / exact output | 4,096 / 160 bytes | 4,096 |
| tasks / capabilities / recursion / unsafe calls | 0 | 0 |

The launcher supplies one root budget large enough for six exact children:
4,096 manifest-set bytes, 65,536 lock map/set bytes, 8,192 completed-set bytes,
8,192 order-vector bytes, 8,192 cycle-diagnostic bytes, and 4,096 report bytes.
The lock parser subdivides only its rights-reduced child. No maximum is derived
from file contents after allocation.

## Execution order

1. Validate all seven configuration limits.
2. Bind and validate the four package-data declarations before source starts.
3. Check manifest/lock byte maxima before allocation.
4. Parse the manifest and lock into bounded ordered sets/map.
5. Freeze collections and validate root version/dependencies and every edge.
6. Compute the dependency-first order with at most `Maximum_packages`
   successful selections plus one no-progress cycle scan.
7. Compare the two notice values without observing storage identity.
8. Render and freeze the exact report; compare it with the multiline oracle.
9. Publish immutable map, sequence, and text.

## Source record

The reviewed source contains 7 files, 1,478 LF-terminated lines, 60 top-level
declarations, and 48,475 UTF-8 bytes. The largest cohesive module is
`Package-Graph-Parser.wv` at 631 lines / 21,119 bytes. These are source facts,
not compiler/runtime performance claims. The parser remains one owner because
its cursor, lexical rules, format adapters, and preallocation checks share one
failure-atomic invariant; implementation may extract a reusable bounded text
scanner when another real parser supplies the boundary.

Implementation measurements must record tokens, phase time/peak memory, generic
instances, protocol solutions/comparisons, WIR blocks/operations, WVB/native
bytes, parse/report time, collection retained bytes, package-table/reference
bytes, distinct content-object bytes, and peak working set on both hosts.
