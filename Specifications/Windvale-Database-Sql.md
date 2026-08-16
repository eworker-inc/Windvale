# Windvale database parameterized SQL subset

## Status and purpose

This specification defines the first SQL input accepted by Windvale Database.
It is a bounded, read-only front end for `WVQI 1`, not a second query model. A
successful parse produces exactly one admitted typed query IR. Execution,
planning, indexes, authorization, transport, and result encoding remain outside
this contract.

## Input contract

The input is valid UTF-8 and at most 16,384 bytes. The grammar is:

```text
query      = SELECT projection FROM collection [WHERE predicates]
             [ORDER BY orders] LIMIT row-count
projection = "*" | field {"," field}
predicates = predicate {AND predicate}
predicate  = field compare parameter | field IS [NOT] NULL
compare    = "=" | "!=" | "<>" | "<" | "<=" | ">" | ">="
parameter  = "$" decimal-index
orders     = order {"," order}
order      = field [ASC | DESC]
```

Keywords are ASCII case-insensitive. Space, horizontal tab, carriage return,
and line feed separate tokens where separation is needed. Identifiers are
case-sensitive ASCII `[A-Za-z_][A-Za-z0-9_]*` values of at most 128 bytes. The
collection identifier must equal the separately bound collection name, and
field identifiers must exactly match names in the bound `WVSC 1` schema.
Keyword-shaped field and collection names are reserved in positions where the
grammar would treat them as keywords.

Parameters are one-based `$1` through `$32` references into the separately
supplied canonical `WVQI 1` parameter section. Decimal parameter and limit
numbers have no sign and no redundant leading zero. The supplied parameter
count is at most 32. `LIMIT` is mandatory and ranges from 1 through 500.

The parser admits at most 64 explicit projection fields, 32 predicates, and 8
order fields. `SELECT *` is represented by zero projections. Predicates retain
source order and are joined only by `AND`; order fields retain source order and
default to ascending.

## Shared typed validation

The lowerer resolves every name to a schema field index, encodes projections,
predicates, order items, and parameters, and calls the canonical `WVQI 1`
encoder. The query encoder remains authoritative for:

- collection and schema identity;
- duplicate projection and order fields;
- parameter framing, use, kind, and null behavior;
- comparison and ordering support by field kind; and
- complete query size and section framing.

SQL failures distinguish syntax/name/limit failures from a typed query rejection
and retain the exact `WVQI 1` error when lowering reached that boundary. Equal
SQL meaning, bound schema, identities, and parameter bytes produce equal query
IR bytes regardless of keyword case, accepted whitespace, `ASC` omission, or
the `!=`/`<>` spelling choice.

## Explicit exclusions

The first subset has no literals, quoted or qualified identifiers, comments,
semicolons, multiple statements, mutations, `OR`, `NOT`, joins, aliases,
expressions, functions, aggregates, grouping, offsets, cursors, subqueries,
text search, JSON paths, or DDL. Unsupported text rejects; it never bypasses the
typed query IR or falls through to host SQL behavior.

The subset is not PostgreSQL or SQLite compatibility. Those databases are
future performance comparison peers after Windvale has a persistent server and
batch transactions; they do not define Windvale SQL semantics.
