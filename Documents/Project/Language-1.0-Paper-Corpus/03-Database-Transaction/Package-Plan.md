# Workload 3 package and launch plan

## Package mapping

Package identity: `windvale.paper.database_transaction.v1`.

| Source file | Module identity | Profile | Authority | Platforms |
| --- | --- | --- | --- | --- |
| `Source/Transaction-Types.wv` | `Transactionˉtypes` | Core | library | Windows, Linux, Windvale |
| `Source/Transaction-Ordering.wv` | `Transactionˉordering` | Core | library | Windows, Linux, Windvale |
| `Source/Transaction-Parser.wv` | `Transactionˉparser` | Core | library | Windows, Linux, Windvale |
| `Source/Transaction-Application.wv` | `Transactionˉapplication` | Hosted | application | Windows, Linux, Windvale |

There is one application entry:

```text
Transactionˉapplication.Run(
    Configuration: Transactionˉtypes.Configuration,
    Budget: Foundationˉmemory.Memoryˉbudget,
) -> Result<Transactionˉreport, Transactionˉfailure>
```

The build plan binds `Transactionˉordering` as the unique visible
`Ordering<Fieldˉkey>` implementation. No import executes registration.

## Profile and authority closure

The Core parser has only `memory.allocate`. The Hosted application adds the one
required `database.customer.transaction` capability plus `resource.acquire`,
`resource.complete`, and `resource.release`. There are no optional capabilities.

The launcher approves exactly one customer collection, expected provider
generation, schema identity/version, cancellation generation, transaction
limits, and root memory budget. No environment, locale, wall clock, entropy,
filesystem, network, console, process, or administrator authority is implied.

## Input maxima

| Bound | Maximum |
| --- | ---: |
| Input fields | 3 |
| Input text bytes retained by launcher | 168 |
| Identifier decimal bytes | 20 |
| Credits decimal bytes | 20 |
| Email UTF-8 bytes | 128 |
| Parser iterations | 3 |
| Map items | 3 |
| Arena live nodes | 3 |
| Map comparisons per operation | published Foundation bound for maximum 3 |
| Transaction lookups | 1 |
| Staged rows | 1 |
| Commits | 1 |
| Transaction semantic operations | 3 |
| Provider retained row bytes | 61,440 generic ceiling; 168 workload ceiling |
| Application diagnostics | 1 terminal structured result |
| Recursion, tasks, queues | 0 |

The launcher rejects an absent, zero, or excessive transaction-operation limit
before acquisition. The reference plan supplies `Maximumˉtransactionˉoperations
= 3u32`.

## Memory plan

The launcher supplies a 16 KiB root budget. The application splits exactly two
4 KiB child budgets for the arena and ordered map. Up to 8 KiB remains for root
metadata and provider-call marshalling. Provider storage, cache, WAL, and
recovery memory belong to the provider resource domain and are limited by its
separate launch plan; they are not charged invisibly to the application arena.

The parser transfers each child budget into its collection owner. Return from
`Parse`, whether success or failure, releases both local owners and all nodes.
The shared `text` in the returned row may share the launcher input backing; the
existing root-domain retained-byte charge remains singular.

## Schema binding

The build plan binds the exact schema in [Database-Contract.md](Database-Contract.md).
The binding contains the collection identity, schema identity/version, field
table, maximum encoded row, codec identity, and digest. There is no package data
and therefore no shipped content object or deduplication entry in this bundle.

The final manifest digest is intentionally not assigned by paper design. Source
freeze must hash the canonical schema and capability signature set.

## Launch and recovery sessions

`windvale.launch.database_transaction.v1` supplies the typed configuration,
memory root, and rights-reduced transaction root. The normal application session
ends after its one commit outcome and local release.

Qualification then starts `windvale.launch.database_recovery_oracle.v1` in a
fresh provider/session generation. That launcher owns reopen and recovery; it
does not call the application again. The two session transcripts are correlated
by collection identity, input digest, expected generation, and commit identity.

## Artifact expectation

The four source modules require ordinary records, enums, variants, generics,
protocol selection, loops, match, ownership, borrowing, and capability calls.
No database-specific WIR opcode or parallel compiler is permitted. WVB carries
typed imports and calls; native modes lower the same verified semantics through
the shared backend. Provider serialization stays behind the capability adapter.
