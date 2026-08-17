# Workload 2 package and launch plan

## Status and identity

This is the exact paper package plan for canonical package identity
`windvale.paper.language1.file_copy` version 1. It binds four source modules and
no package data. It performs no build-script execution, path search, dynamic
import, or runtime package-name lookup.

## Module mapping

| Canonical module | Source | Profile | Target scope |
| --- | --- | --- | --- |
| `Copyˉtypes` | [`Source/Copy-Types.wv`](Source/Copy-Types.wv) | Core | Windows, Linux, Windvale |
| `Copyˉvalidate` | [`Source/Copy-Validate.wv`](Source/Copy-Validate.wv) | Core | Windows, Linux, Windvale |
| `Copyˉengine` | [`Source/Copy-Engine.wv`](Source/Copy-Engine.wv) | Hosted library | Windows, Linux, Windvale |
| `Copyˉapplication` | [`Source/Copy-Application.wv`](Source/Copy-Application.wv) | Hosted application | Windows, Linux, Windvale |

The build supplies the accepted normative-candidate Foundation signatures and
the paper `Platformˉfilesystem` module recorded in
[Filesystem-Contract.md](Filesystem-Contract.md). These are canonical module
dependencies, not source searched from a host directory.

## Launcher profile

The package selects launcher profile `windvale.launch.file_copy.v1` and exported
entry `Copyˉapplication.Run` with this exact signature:

```text
fn Run(
    Configuration: Copyˉtypes.Configuration,
    Budget: Foundationˉmemory.Memoryˉbudget,
) -> Foundationˉresult.Result<
    Copyˉtypes.Copyˉreport,
    Copyˉtypes.Copyˉfailure
>
```

`Run` is an ordinary source name. The launcher:

1. admits the exact package, modules, source edition, target, entry signature,
   and launcher profile;
2. constructs one strict typed configuration from deployment metadata;
3. validates each name as a semantic segment before binding either root;
4. creates and transfers one 65,536-byte-maximum root memory budget;
5. approves exactly two required capabilities and binds independently
   rights-limited source and destination directory roots;
6. binds one nonzero cancellation generation into both roots;
7. reserves local teardown capacity for two provider resources; and
8. invokes no source if any prior admission or binding step fails.

Configuration text is already strict Windvale `text`; no host path, command-line
quoting, environment, current directory, native encoding, or file descriptor
enters the function.

## Capability closure

| Requirement | Authority | Bound instance |
| --- | --- | --- |
| `filesystem.copy.source` version 1 | Acquire and read immutable source snapshots only. | One semantic directory root and provider generation. |
| `filesystem.copy.destination` version 1 | Create one new regular file, write within its admitted maximum, and durably finish it. | One semantic directory root and provider generation. |

No transitive dependency requires console, package-data, general filesystem,
network, clock, entropy, process, environment, or unsafe authority. Binding does
not imply grant; grant does not select a provider; both must be present.

## Configuration limits

| Field | Admitted range |
| --- | ---: |
| Source name | 1–255 canonical UTF-8 bytes, one semantic segment |
| Destination name | 1–255 canonical UTF-8 bytes, one semantic segment |
| Maximum source/copy bytes | 0–1,048,576 |
| Chunk bytes | 1–65,536 |
| Total read/write operations | 1–2,097,152 |

The launcher may impose smaller deployment maxima. Source revalidates the three
numeric values before allocation or provider acquisition. Capability roots
independently revalidate names and every operation bound.

## Resource-domain plan

### Source-visible memory budget: at most 65,536 bytes

The application creates one rights-reduced child whose exact maximum equals
`Configuration.Chunkˉbytes`, then consumes it into one zero-initialized
`Byteˉbuffer` of the same length. It creates no collection, text builder, byte
builder, task, queue, recursion, or retained diagnostic value.

### Application-domain ceiling: 98,304 bytes

| Charge | Maximum bytes |
| --- | ---: |
| Root memory accounting and one byte buffer | 65,536 |
| Configuration text and typed values | 1,024 |
| Source handle/provider state | 4,096 |
| Destination handle/provider state | 4,096 |
| Capability bindings and cancellation evidence | 4,096 |
| Bounded result/diagnostic evidence | 4,096 |
| Reserved teardown capacity | 15,360 |
| **Total** | **98,304** |

Provider-private snapshot storage is separately admitted by the bound source
provider and cannot be charged to or accessed through the application buffer.
The provider must reject acquisition if it cannot preserve the snapshot within
its own declared limits.

## Operation and work bounds

| Measure | Maximum |
| --- | ---: |
| Source and destination resource instances | 1 each |
| Mutable source-visible buffers | 1 |
| Live mutable slices | 1 |
| Live immutable write slices | 1 |
| Bytes per provider transfer | 65,536 |
| Source/copy length | 1,048,576 |
| Read plus write calls | 2,097,152 |
| Finish calls | 1 |
| Automatic mutation retries | 0 |
| Tasks, queues, timers, recursion | 0 |

One-byte read and write progress over the maximum file requires exactly
2,097,152 transfer calls. A launch may set a smaller operation maximum and
receive `Operationˉlimit` rather than allowing pathological provider behavior to
consume unbounded time.

## Result and process-status translation

The launcher retains the exact structured `Result` in a bounded application
completion record and maps it to process status only after source returns and the
application resource domain tears down:

| Result | Status |
| --- | ---: |
| Valid report with exact completed finish | 0 |
| `Invalidˉconfiguration` | 2 |
| `Allocation` | 3 |
| source `Openˉrejected` | 4 |
| destination `Openˉrejected` | 5 |
| known `Transferˉrejected`, `Progressˉstalled`, or `Operationˉlimit` | 6 |
| `Cancelled` | 7 |
| `Providerˉlost`, `Providerˉrestarted`, or `Sourceˉchanged` | 8 |
| `Mutationˉindeterminate` or `Finishˉindeterminate` | 9 |
| `Finishˉrejected` | 10 |
| `Invalidˉprogress` | 11 |

Status translation does not discard the structured fields from the bounded
completion record. This profile performs no diagnostic output and acquires no
output capability.

## Acquisition and publication order

1. Validate configuration numbers in Core source.
2. Split and commit the exact byte-buffer budget.
3. Construct the zero-initialized buffer.
4. Acquire the read-only source snapshot and observe its exact length.
5. Create the destination exclusively with that length as its maximum.
6. Copy through explicit read and write positions.
7. Attempt durable finish exactly once after body success.
8. Release destination, source, buffer accounting, and root accounting in
   reverse lifetime order.
9. Publish the structured result and mapped process status after teardown.

A failure at any step publishes no later step. In particular, destination
acquisition failure releases the already acquired source; body failure skips
finish; finish failure remains the returned result; and release cannot replace
any result.

## Reproducibility

Identical source, module identities, Foundation identities, capability catalog,
launcher profile, configuration, provider transcript, target descriptor, and
compiler options must produce identical WVB and identical structured semantic
results. Native object/container bytes may differ by declared target format but
must preserve the same capability calls, positions, progress, and result cases.
