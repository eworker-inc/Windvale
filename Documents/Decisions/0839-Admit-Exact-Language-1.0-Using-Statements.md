# Decision 0839: Admit exact Language 1.0 `using` statements

## Status

Accepted on 2026-08-23. Owned-resource classification, lexical binding,
exactly-once release, exit-path lowering, and a hosted resource consumer remain
pending within Migration Slice 5.

## Context

The frozen Language 1.0 grammar defines one resource-scope form:

```text
using Name = Expression { Statements }
```

Its acquisition expression produces one owned resource, and the block owns the
new binding. The eventual semantic compiler must release that resource exactly
once on every ordinary exit while leaving fallible finish, commit, flush, or
protocol completion explicit in source. That behavior cannot be implemented
coherently while `using` is still lexed as an ordinary identifier and the body
parser has no stable spans for the acquisition or scope.

The source parser already has flat immutable statement records, streaming block
parsing, and exact statement/expression limits. A new tree representation or a
second compiler would add no value at this boundary.

## Decision

1. Append token identity 103 for the exact lowercase canonical keyword `using`.
   Edition 1 admits the token and descriptorless Seed rejects it.
2. Append statement kind 14 and parse exactly one identifier, `=`, one ordinary
   expression, and one required brace-delimited block.
3. Retain the binding-name span, acquisition-expression span, complete body
   span, next cursor, descendant statement/expression counts, and maximum
   depths in the existing flat statement record.
4. Reuse the existing limits of 4,096 statements per function, 4,096 nodes per
   expression, and 64 statement/expression nestings. Do not allocate or retain
   a child-statement collection.
5. Do not yet claim resource semantics. This checkpoint does not introduce the
   binding to semantic scope, classify the expression result, prove moves or
   borrows, select a release protocol, emit cleanup edges, or change WVIR/WVB.
6. Keep full Unicode project-identifier admission in the separate normalized
   source-profile front door. The direct canonical parser test covers the
   already implemented ASCII/U+02C9 view and does not weaken the frozen Unicode
   requirement.
7. Give this boundary an independent 18-case verification owner. Build its WVB
   twice, compare exact bytes, package it through the maintained segmented
   native path, and execute the cases in two bounded parallel batches.

## Consequences

- The compiler can distinguish and retain the exact Language 1.0 resource-scope
  syntax without pretending that parser admission is ownership enforcement.
- Eighteen cases cover canonical recognition, `try` acquisition, nested scopes,
  trivia, retained spans and counts, missing components, unterminated input,
  contextual lookalikes, exact nesting boundaries, macron-separated names, and
  edition separation.
- The focused project rebuilds byte-identically as a 378,739-byte WVB at
  SHA-256
  `cab55c5abbf301fe1a9dbafe6566444d7cba9aee6d1d1eddaf23d26d3406847e`.
  Every packaged Windows execution returns 42.
- The verification registry advances to 112 owners and 5,316 cases at SHA-256
  `fc53fb21939dd854c4a7f3e8a46602a62dd04078444002723375d77b9c1f3e93`.
  Paired Linux execution remains pending.
- Direct scripting execution of the full body parser still reaches its existing
  bounded call-depth refusal. The test therefore uses the maintained native
  segmented package rather than increasing a runtime limit or weakening the
  parser case.

## Reconsideration triggers

Change the syntax only through a Language 1.0 source amendment. Change parser
limits only with representative source and verifier evidence. Do not add a
general `defer`, hidden fallible completion, or exception-style cleanup
precedence while connecting semantics. The next resource checkpoint must name
one exact owned resource protocol and prove release on fallthrough, `return`,
`break`, `continue`, and `try` propagation before broad source migration.
