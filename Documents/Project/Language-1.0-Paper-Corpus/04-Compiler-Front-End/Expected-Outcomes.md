# Workload 4 expected semantic outcomes

## Canonical success

Input (ASCII, 71 bytes when spelled exactly as below without a final newline):

```text
let Total = 2 + 3 * 4; let Doubled = Total + Total; return Doubled * 5;
```

The semantic counts are:

| Measure | Value |
| --- | ---: |
| tokens including End | 22 |
| AST nodes | 11 |
| declarations/symbols | 2 / 2 |
| bound operations | 16 |
| diagnostics | 0 |
| artifact bytes | 140 |

The canonical symbol order is `Doubled` ordinal 1, then `Total` ordinal 0.
Expression precedence represents `2 + (3 * 4)` and the emitted stack operations
are the exact sequence documented in the output format.

Artifact SHA-256 is
`498f869576f2c38970fa252962a3cf1e5a460d4a8a1e0e5df75d31378e898259`.
The complete 140-byte lowercase hex oracle is:

```text
57564645303030310100000002000000000000001000000000000000010000000700000000000000446f75626c6564000000000500000000000000546f74616c01000000000202000000000000000203000000000000000204000000000000000504060000000001010000000300000000030000000004060100000003010000000205000000000000000507
```

## Diagnostic examples

| Input/event | Ordered diagnostic codes | Artifact |
| --- | --- | --- |
| invalid byte `FF` at offset 3 | `Invalidˉutf8` | absent |
| decoded rune 101 with selected rune maximum 100 | `Sourceˉruneˉlimit` | absent |
| `let X = 1 return X;` | `Expectedˉsemicolon` | absent |
| `let X = Y; return X;` | `Unknownˉsymbol(Y)` | absent |
| `let X = 1; let X = 2; return X;` | `Duplicateˉsymbol(second X, related first X)` | absent |
| 20 independent invalid runes, diagnostic max 16 | 15 `Unexpectedˉrune`, then `Diagnosticˉlimit` | absent |
| valid source, output maximum 139 | `Outputˉlimit(140 needed, 139 admitted)` | absent |

## Position examples

For UTF-8 text `let Aˉβ = 1;` the Greek beta is not admitted by identifier
syntax. Its start byte offset accounts for two-byte U+02C9, while rune offset and
column each advance by one scalar for that macron. The diagnostic identifies the
beta scalar's half-open byte/rune span and never reports UTF-16 code units or
display cells.

LF advances line. CRLF advances column for CR then line for LF; input is not
normalized. U+02C9 is accepted only between valid ASCII segments. U+00AF and
U+203E are distinct unexpected runes.

## Cleanup oracle

For every success, diagnostic result, and typed failure, instrumentation records
zero live mutable vectors, maps, arenas, builders, and child budgets after the
returned report/failure is released. A successful report retains only artifact
and diagnostic immutable backing. A diagnostic report retains diagnostics and
no artifact. No phase has a hidden global cache.

## Later implementation evidence

Record source/compiler version, exact generic instances, compiler steps/time,
verification time, peak compiler/application memory, WIR blocks/operations, WVB
size/digest, native object size/digest where applicable, and results on Windows
and Linux. Cross-host equality is required before claiming portable conformance.
