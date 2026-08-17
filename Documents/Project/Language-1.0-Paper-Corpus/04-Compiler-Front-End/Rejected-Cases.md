# Workload 4 rejected and boundary cases

| Case | Boundary | Expected result |
| --- | --- | --- |
| 1 | malformed/truncated UTF-8 | one decode diagnostic at exact byte offset; no text/tokens/artifact |
| 2 | source 65,537 bytes, or decoded rune 101 with a selected rune maximum of 100 | `Sourceˉlimit` before decode or `Sourceˉruneˉlimit` during strict bounded decode |
| 3 | U+00AF/U+203E macron lookalike in name | unexpected-rune/identifier diagnostics; never treated as U+02C9 |
| 4 | name ending `ˉ` or containing `ˉˉ` | `Invalidˉidentifier` |
| 5 | decimal `18446744073709551616` | `Numericˉoverflow`; invalid token; no wrap |
| 6 | token 8,192 would displace End | `Tokenˉlimit`; End retained as final token |
| 7 | missing identifier/equal/expression/semicolon | stable parse diagnostic plus bounded synchronization |
| 8 | 65 nested parentheses with max 64 | `Nestingˉlimit`; no host-stack dependence |
| 9 | node 4,097 | node capacity failure; no stale/partial arena publication |
| 10 | declaration 513 | `Declarationˉlimit`; later recovery remains bounded |
| 11 | duplicate declaration | `Duplicateˉsymbol` with first declaration span as related evidence |
| 12 | unknown/self/forward name | `Unknownˉsymbol`; no implicit global lookup |
| 13 | stale or wrong-arena node handle fixture | recoverable arena validation failure before borrow |
| 14 | diagnostic issue 16 after 15 ordinary issues | final `Diagnosticˉlimit`; every later issue ignored |
| 15 | work charge 200,001 | exact `Workˉexhausted(200000,200000)`; no artifact |
| 16 | output maximum one byte below required | `Outputˉlimit`; private prefix destroyed, artifact absent |
| 17 | mutated ordered-map comparator violates total order | protocol/collection qualification failure, never nondeterministic output |
| 18 | generic instance 257 or depth 33 while compiling paper source | bounded compile diagnostic before artifact publication |
| 19 | same input with CRLF versus LF bytes | distinct spans/input identity but deterministic result for each; no host newline rewriting |
| 20 | all valid declarations but missing return | `Expectedˉreturn`; error node published only inside failed phase result |
| 21 | source budget is admitted but physical UTF-8 text allocation fails | typed source-text allocation failure; no decode diagnostic or partial text |

## Source rejections

### Result-context empty construction

```text
let Nodes: Collections.Arena<Types.Node> =
    Collections.Arenaˉconstruct(Budget: Budget, Maximumˉnodes: 4096u64);
```

Rejected because the call supplies no `T` evidence and has no explicit generic
arguments. The accepted spelling is
`Collections.Arenaˉconstruct::<Types.Node>(...)`.

### Partial explicit generic list

```text
Collections.Mapˉconstruct::<text>(Budget: Budget, Maximumˉitems: 512u64)
```

Rejected: explicit generic calls supply every type/constant parameter in
declaration order. There is no partial inference.

### Ambiguous borrowed result

```text
let Value = Collections.Mapˉborrowˉexisting(
    Map: borrow Bindings,
    Key: borrow Name,
);
```

Rejected by the revised Foundation surface because the signature cannot show
whether the result is tied to Map or Key without lifetime syntax. Source first
obtains an owned rank, then calls `Mapˉborrowˉat(Map, Rank)`, whose only borrowed
parameter is the owner.

### Mutation after freeze

```text
let Published = Collections.Arenaˉfreeze(Arena: Nodes);
Collections.Arenaˉinsert(Arena: borrow mut Nodes, Value: Node);
```

Rejected: freeze consumes `Nodes`; handles remain observations of `Published`,
not permission to mutate the moved owner.

### Borrow across arena mutation

```text
let Node = Collections.Arenaˉborrowˉvalidated(Arena: borrow Nodes, Handle: H);
let Other = Collections.Arenaˉinsert(Arena: borrow mut Nodes, Value: Value);
use Node;
```

Rejected because the immutable node borrow overlaps exclusive arena mutation.

## Determinism cases

Repeated compilation, alternate host, alternate map balancing, and alternate
native execution mode must preserve token spans, diagnostic order, canonical
symbol order, operation order, output length, and SHA-256. Address, pointer size,
host newline, locale, hash seed, and traversal implementation are excluded.
