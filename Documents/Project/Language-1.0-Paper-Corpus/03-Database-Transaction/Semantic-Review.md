# Workload 3 semantic review

## Value and ownership inventory

| Value | Class and owner | Borrow/move path | End of life |
| --- | --- | --- | --- |
| `Configuration` | immutable entry value | application borrows `Fields`; copies scalar limits | application return |
| root budget | move-owned | split through one exclusive borrow | application return |
| arena/map child budgets | move-owned | consumed by first-item constructors | construction failure or collection destruction |
| `Arena<Parsedˉvalue>` | move-owned parser local | exclusive insert, immutable lookup | parser return on every path |
| parsed node | owned by arena | read through generation-checked handle | arena destruction or explicit removal test |
| `Handle<Parsedˉvalue>` | Copy, non-owning | stored in map and borrowed for arena lookup | semantically invalid after arena destruction/removal |
| ordered map | move-owned parser local | exclusive insert, immutable lookup/iteration | parser return |
| `Customerˉrow` | immutable value; text backing may be shared | moved to application, then staged | application/provider ownership contract |
| transaction | move-only resource | one mutable borrow at lookup, stage, commit | infallible local release after `using` body |
| staged row | transaction/provider-owned after successful stage | commit consumes semantic intent, not handle | commit resolution or release |
| report/failure | immutable return value | moved to launcher | launcher domain release |

There is one mutable application value at each layer: root budget during split,
arena/map during parse, and transaction during provider calls. No node address,
provider page, native handle, or mutable alias enters safe source.

## Mutation and order

Input fields are inspected from index zero upward. Duplicate detection precedes
allocation of the duplicate value. The map's total order is Identifier, Email,
Credits; iteration must produce that order regardless of input order or internal
balancing. Numeric parsing consumes complete ASCII decimal text and is locale
independent.

The provider mutation order is lookup, stage, commit. `Stageˉupdate` changes only
transaction-local/provider-staged state. `Commit` is the only externally durable
mutation boundary.

## Effect and capability closure

| Function/module | Effects |
| --- | --- |
| types and ordering | none |
| field parsing and typed extraction | none |
| parser | `memory.allocate` |
| application | `memory.allocate`, `database.customer.transaction`, `resource.acquire`, `resource.complete`, `resource.release` |

The capability root is a launcher-bound module singleton. The owned transaction
is a lexical local. It cannot be captured, copied, returned, or retained after
release by this source.

## Cleanup walkthrough

1. A failed first memory split returns its exact allocation failure; root budget
   is released by the entry frame.
2. A failed second split releases its rejected child and the first unused child
   through lexical ownership.
3. A parse failure destroys any constructed map and arena. Their children and
   nodes release exactly once.
4. A begin failure occurs before `using`; no transaction exists.
5. Lookup missing/rejection, invalid existing schema, or stage rejection exits
   the body. `using` releases the transaction and discards any uncommitted stage.
6. Conflict or commit rejection becomes the returned failure, then local release
   invalidates the session. No commit retry occurs.
7. Committed and indeterminate outcomes are captured as exact result values,
   then local release runs. Release cannot change either result.
8. Cancellation is observed only at invoked provider operations. Before commit
   dispatch it is rejection; after dispatch without proof it is uncertainty.
9. Provider loss/restart never retargets a live transaction. The handle releases
   and a fresh recovery launcher/session is required.

No ordinary `try`, `return`, match arm, provider result, or cancellation path can
bypass local release.

## Failure and trap boundary

Recoverable results cover malformed input, allocation/capacity, duplicate or
missing field, collection construction/insertion, invalid/stale handle, begin,
missing row, invalid stored schema, lookup/stage rejection, conflict, commit
rejection, and commit uncertainty.

Terminal traps remain programming defects after static checking: sequence access
outside a previously checked count, impossible map iterator overrun, use after
move/release, borrow violation, arithmetic impossible under admitted constants,
or a provider violating a verified in-process ABI. Untrusted provider bytes and
responses are validated into recoverable failures before safe values exist.

## Quantitative source and compiler plan

The bundle has four modules, 825 physical source lines, 26 top-level
declarations, and 27,254 source bytes. `Run` is the largest function at 161
physical lines. `Transactionˉreport` is the widest record at seven fields. The
source contains one required capability declaration, zero optional capabilities,
zero unsafe blocks, zero tasks, seven named failure adapters, and explicit
borrows at every collection/resource observation or mutation. Semantic planning
expects:

- 1 `Ordering<Fieldˉkey>` implementation;
- 1 `Arena<Parsedˉvalue>` instance;
- 1 `Map<Fieldˉkey, Handle<Parsedˉvalue>>` instance;
- 3 immutable `Sequence` observations;
- 2 strict `u64` parser uses sharing one generic implementation;
- no recursive instantiation, closure, task, unsafe block, or dynamic protocol;
- at most 52 candidate WIR blocks and 430 candidate WIR operations; and
- no new WIR instruction family.

These are preimplementation planning ceilings, not measurements. The migration
gate must record actual compiler time, peak memory, WIR, WVB, and native object
sizes and fail if the implementation exceeds an accepted revised ceiling.

## Resource accounting

Application-retained collection memory is at most 8 KiB. Launcher input is at
most 168 UTF-8 bytes plus three fixed field records. The transaction retains one
row under a 61,440-byte generic provider limit, though the bound schema admits at
most 168 encoded bytes. There are no tasks, queues, recursion, package data,
output builder, or diagnostic list. Work is finite: three parse iterations,
ordered-map work for maximum three keys, and three transaction operations.

## Backend independence

Interpreter, JIT, cached/install-time, and AOT modes must return the same nominal
outcomes and canonical values. Windows, Linux, and Windvale hosts may implement
durability differently, but cannot change conflict, rejection, uncertainty,
release, schema, or recovery meanings. Exact storage bytes are provider-format
evidence and not source-language semantics.
