# Workload 3 rejected and boundary cases

Each case is a semantic oracle. Source snippets are illustrative candidate
edition-1 forms; provider faults are injected by the deterministic oracle.

| Case | Boundary | Expected result |
| --- | --- | --- |
| 1 | zero input fields | `Parse.Noˉfields`; no collection allocation |
| 2 | four fields | `Parse.Tooˉmanyˉfields(4, 3)` before value parsing |
| 3 | Identifier appears twice among three fields | `Duplicateˉfield(Identifier, index)` before duplicate node allocation |
| 4 | Credits absent | `Missingˉfield(Credits)`; arena/map release |
| 5 | empty Email | `Emptyˉtext(Email)` |
| 6 | Email is 129 UTF-8 bytes | `Textˉlimit(Email, 129, 128)` |
| 7 | Identifier contains `+1`, whitespace, locale digits, or trailing text | exact `Numericˉparseˉfailure`; no conversion or prefix acceptance |
| 8 | Identifier exceeds `u64` | `Aboveˉmaximum`; no wrap |
| 9 | arena/map budget cannot admit first item | allocation/collection failure returning original ownership |
| 10 | direct arena fixture inserts a fourth live node | `Capacityˉexhausted`; fourth value returned, first three unchanged |
| 11 | direct map fixture inserts an equal key | `Duplicate`; original key/value returned, map unchanged |
| 12 | removed arena handle is looked up after slot reuse | `Staleˉgeneration`; it never aliases the replacement |
| 13 | handle from a second arena is used | `Wrongˉarena` |
| 14 | database key missing | `Missingˉrow(identifier)`; no stage/commit |
| 15 | stored row has wrong schema version or invalid value | `Existingˉschemaˉinvalid` or provider `Invalidˉschema` |
| 16 | generic provider returns a 61,441-byte row | rejected before row allocation/decoding |
| 17 | expected row revision changed before commit | `Conflict(expected, observed)`; no retry |
| 18 | commit rejects before durable dispatch | `Commitˉrejected`; provider proves no commit |
| 19 | failure after commit dispatch cannot prove selection | `Commitˉindeterminate` with stable identity; no replay |
| 20 | provider restart between stage and commit | rejection if pre-dispatch is proved, otherwise indeterminate; live transaction never retargets |
| 21 | cancellation before commit dispatch | typed rejection and local release |
| 22 | cancellation after commit dispatch | indeterminate unless provider proves the exact outcome |
| 23 | reopen sees corrupt or two selectable generations | recovery-oracle failure; application success is not qualified |
| 24 | successful reopen sees old row after `Committed` | provider qualification failure |

## Ownership rejection examples

### Copying a transaction

```text
let Duplicate = Transaction;
database.customer.transaction.Commit(Transaction: borrow mut Transaction);
```

Rejected because `Transaction` is move-only; the first binding would move it,
leaving no value to borrow.

### Use after `using`

```text
using Transaction = try database.customer.transaction.Begin(...) {
    let Saved = borrow Transaction;
}
database.customer.transaction.Lookup(Transaction: Saved, Identifier: 1u64);
```

Rejected because the borrow cannot outlive the lexical resource owner.

### Commit hidden in release

```text
using Transaction = try database.customer.transaction.Begin(...) {
    try database.customer.transaction.Stageˉupdate(...);
}
return Success;
```

This compiles only as an explicit rollback/discard-on-release path. It cannot be
interpreted as a committed success because `using` never commits.

### Mutable map alias

```text
let Rank = try Collections.Mapˉfindˉrank(Map: borrow Fields, Key: borrow Key);
let Existing = Collections.Mapˉborrowˉat(Map: borrow Fields, Index: Rank);
Collections.Mapˉinsert(Map: borrow mut Fields, Key: Other, Value: Handle);
use Existing;
```

Rejected because an immutable borrow into the map remains live across exclusive
mutation.

### Handle used without arena

```text
let Number: u64 = Handle;
```

Rejected. A handle is opaque, not convertible to its slot/index or to an
integer. Only the owning arena can validate liveness.

## Determinism cases

All six permutations of the three input fields produce the same typed row and
the map iterator produces Identifier, Email, Credits. A provider query plan,
host hash seed, page placement, or insertion order cannot enter the report.

The maximum Email case uses exactly 128 UTF-8 bytes and succeeds. A 128-scalar
string whose UTF-8 encoding exceeds 128 bytes fails, proving the bound is bytes,
not host characters. A row at exactly 61,440 provider bytes is admitted only if
its explicit schema and internal length fields are valid.

## Reopen evidence cases

After `Committed`, a fresh recovery session must observe the reported generation,
sequence, commit identity, revision increment, and exact row. After
`Indeterminate`, it may select the old or new row, but must select exactly one
valid durable generation. The harness records which and never calls
`Stageˉupdate` or `Commit` again for the same commit identity.
