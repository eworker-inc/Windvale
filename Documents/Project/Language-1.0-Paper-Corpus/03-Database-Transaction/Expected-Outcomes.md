# Workload 3 expected semantic outcomes

## Canonical successful input

The launcher supplies these three typed fields in any order:

```text
Identifier = "42"
Email = "new@example.test"
Credits = "9001"
```

It binds database generation 17. The deterministic provider begins with row 42
at revision 6 and generation 17. Commit identity 7001 selects generation 18 and
sequence 104.

The application returns:

```text
Valid Transactionˉreport {
    Identifier: 42u64,
    Priorˉrevision: 6u64,
    Commitˉidentity: 7001u64,
    Priorˉdatabaseˉgeneration: 17u64,
    Committedˉdatabaseˉgeneration: 18u64,
    Commitˉsequence: 104u64,
    Requiresˉindependentˉreopen: true,
}
```

The independent reopen transcript must then report schema version 1, generation
18, sequence 104, commit identity 7001, row revision 7, and exactly the typed row
`{ Identifier: 42, Email: "new@example.test", Credits: 9001 }`.

## Deterministic parser matrix

| Input | Expected parser result |
| --- | --- |
| all six field permutations | same `Customerˉrow`; canonical iteration Identifier, Email, Credits |
| Email exactly 128 UTF-8 bytes | valid |
| Email 129 bytes | `Textˉlimit` |
| Identifier `18446744073709551615` | valid `u64` maximum |
| Identifier `18446744073709551616` | `Aboveˉmaximum` |
| Identifier `42x` | `Trailingˉinput` at byte 2 |
| duplicate Email, missing Credits | `Duplicateˉfield` at duplicate index before missing-field check |

## Provider result matrix

| Injected event | Application result | Durable oracle |
| --- | --- | --- |
| row absent | `Missingˉrow(42)` | unchanged generation 17 |
| stored schema version 2 | `Existingˉschemaˉinvalid(42, 2)` | unchanged |
| stage storage-capacity rejection | `Stageˉrejected(Capacityˉexhausted, generation)` | unchanged |
| revision changes 6 to 7 | `Conflict(6, 7)` | competing row remains selected |
| commit rejected before dispatch | `Commitˉrejected(reason, generation)` | old row/generation selected |
| commit completes | valid report | new row/generation selected |
| commit loses acknowledgement after durable selection | `Commitˉindeterminate(7001, 17, 18, Providerˉlost)` | reopen selects new row/generation 18 |
| commit loses provider before selection | same indeterminate family | reopen selects old row/generation 17 |
| restart after stage before dispatch with proof | `Commitˉrejected(Providerˉrestarted, new generation)` | unchanged |

The two indeterminate rows deliberately share the same application failure
family. Only independent recovery can distinguish them.

## Cleanup observations

For every row in the matrix:

- arena live nodes return to zero;
- map items return to zero;
- both 4 KiB child budget charges return;
- transaction release count is one if and only if begin succeeded;
- commit count is zero or one and never two;
- no transaction handle survives the application call; and
- no recovery operation occurs through that released handle.

## Backend-independent comparison

Tests compare nominal case, named fields, integers, exact text/runes, operation
counts, provider generations, and recovery transcript. They do not compare host
exception text, addresses, object identity, map layout, WAL layout, system error
numbers, wall-clock timing, or native database-engine messages.

## Artifact evidence to record after implementation

Each Windows/Linux/Windvale execution mode records source module digests,
Foundation signature-set identity, capability signature-set identity, target,
WIR block/operation counts, WVB digest/size, native artifact digest/size where
applicable, peak application memory, provider peak memory, elapsed compiler time,
elapsed verification time, and the two bounded session transcripts.
