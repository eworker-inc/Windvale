# Workload 3 typed database contract

## Contract boundary

The paper package requires `database.customer.transaction` version 1. The
launcher binds one rights-limited root for one named customer collection. The
root grants begin, lookup, one staged update, commit, and local release only. It
does not grant collection creation, schema mutation, arbitrary query text,
enumeration of other collections, host files, storage pages, native handles,
administration, or ambient retry.

The identity is a paper candidate, not a frozen capability-catalog entry.

## Explicit schema

The package plan binds exactly this schema, independently of source declaration
layout and provider implementation layout:

| Property | Contract |
| --- | --- |
| Collection identity | `windvale.paper.customer.v1` |
| Schema identity | `windvale.paper.customer.row.v1` |
| Schema version | `1u32` |
| Key | `Identifier: u64` |
| Field 1 | `Identifier: u64`, required, equal to key |
| Field 2 | `Email: text`, required, strict UTF-8, 1–128 bytes |
| Field 3 | `Credits: u64`, required |
| Maximum encoded row | 61,440 bytes at the generic provider boundary |
| Workload row maximum | 168 bytes including explicit envelope |
| Field order | Identifier, Email, Credits |

The provider adapter uses the existing versioned typed-row/logical-record
contracts or a byte-equivalent explicit codec. Encoding is defined by schema
identity, field tags, exact integer widths, little-endian integers, strict UTF-8,
checked lengths, and declaration order. It never walks source fields through
ambient reflection, host object metadata, or platform layout.

Malformed schema identity, version, field count, type, length, duplicate tag,
unknown required field, noncanonical ordering, key mismatch, or invalid UTF-8 is
rejected before the application receives a `Customerˉrow`.

## Exact typed surface

The provisional platform module exposes these semantic shapes:

```text
resource Transaction;

record Failure {
    Kind: Failureˉkind;
    Providerˉgeneration: u64;
}

record Existingˉrow {
    Row: Customerˉrow;
    Revision: u64;
}

variant Lookupˉoutcome {
    Missing;
    Found(Row: Customerˉrow, Revision: u64, Schemaˉversion: u32);
    Rejected(Error: Failure);
}

variant Commitˉoutcome {
    Committed(
        Commitˉidentity: u64,
        Previousˉgeneration: u64,
        Generation: u64,
        Sequence: u64,
    );
    Conflict(Expectedˉrevision: u64, Observedˉrevision: u64);
    Rejected(Error: Failure);
    Indeterminate(
        Commitˉidentity: u64,
        Expectedˉgeneration: u64,
        Observedˉgeneration: u64,
        Error: Failure,
    );
}

Begin(
    Expectedˉdatabaseˉgeneration: u64,
    Maximumˉoperations: u32,
    Maximumˉstagedˉrows: u32,
    Maximumˉretainedˉrowˉbytes: u64,
) -> Result<Transaction, Failure>
    effects(database.customer.transaction, resource.acquire);

Lookup(
    Transaction: borrow mut Transaction,
    Identifier: u64,
) -> Lookupˉoutcome effects(database.customer.transaction);

Stageˉupdate(
    Transaction: borrow mut Transaction,
    Expectedˉrevision: u64,
    Row: Customerˉrow,
) -> Result<unit, Failure> effects(database.customer.transaction);

Commit(
    Transaction: borrow mut Transaction,
) -> Commitˉoutcome
    effects(database.customer.transaction, resource.complete);
```

`Transaction` implements infallible `Localˉrelease`. Release invalidates the
local session and discards an uncommitted staged value. It does not commit,
flush, retry, recover, or overwrite the application result.

## Transaction state machine

The observable states are `Ready`, `Staged`, `Committed`, `Reopenˉrequired`, and
`Released`.

| State | Admitted operations | Transition |
| --- | --- | --- |
| Ready | lookup, stage, release | lookup retains Ready; successful stage enters Staged |
| Staged | commit, release | commit enters Committed or Reopen-required; conflict/rejection remains terminal to this source run |
| Committed | release | release enters Released |
| Reopen-required | release only | release enters Released; same-session query/retry is forbidden |
| Released | none | any access is a compile-time ownership error or terminal invalid-resource trap at a foreign boundary |

The workload performs at most one lookup, one stage, and one commit. A provider
counts acquisition and internal validation separately but may not exceed the
declared operation maximum.

## Commit meanings

- `Committed` proves the exact staged update is selected durably under the
  provider's admitted stability model. Generation and sequence are monotonic
  provider evidence, not host time.
- `Conflict` proves no commit by this transaction and reports the expected and
  observed row revisions.
- `Rejected` proves no commit by this transaction. It is safe to report but this
  source does not retry automatically.
- `Indeterminate` cannot prove whether the commit became selected. It supplies a
  stable commit identity and generation evidence and forbids replay.

Provider loss or cancellation before mutation dispatch may be `Rejected`.
After the durability boundary is dispatched, lack of proof is `Indeterminate`,
not a guessed rejection.

## Reopen and recovery

Every successful or uncertain test run closes the transaction and uses a new
launcher-owned recovery session. That independent session reopens the database,
validates storage, selects the durable generation, and reads the customer row by
key. It records the selected generation, commit identity when present, row
revision, schema identity, and row value.

For `Committed`, reopen must observe the new row and the returned generation and
sequence. For `Indeterminate`, reopen may prove either the prior row or the new
row; exactly one valid state must be selected. The harness then compares the
stable commit identity and never resubmits the mutation. A corrupt or ambiguous
recovery is a provider qualification failure, not an application result.

The existing
[persistent writer](../../../../Specifications/Windvale-Database-Persistent-Transaction-Writer.md),
[single-writer transaction](../../../../Specifications/Windvale-Database-Single-Writer-Transaction.md),
and [storage recovery](../../../../Specifications/Windvale-Database-Storage-Recovery.md)
contracts are the first concrete oracle for this behavior.

## Deterministic query rule

This workload performs a point lookup, so it observes no provider query order.
Any later multi-row query in this capability family must declare one typed total
order and return a finite admitted maximum; provider plan, hash seed, storage
layout, and input insertion order cannot affect the visible order. The parser's
map iteration is already ascending `Fieldˉkey` order under the one explicit
`Ordering<Fieldˉkey>` implementation.
