# Windvale database strict JSON value

## Status and scope

`Windvaleˉdatabaseˉjsonˉvalue` is a portable, capability-free JSON admission
boundary for database protocols and future JSON-typed columns. It validates one
complete UTF-8 document, reports its root kind and node count, and returns an
owned copy of the exact admitted bytes. It does not perform storage, network,
query, or authorization work.

The first boundary intentionally retains the admitted JSON spelling instead of
building a heap object graph. Consumers that need a string call the strict
decoder explicitly. This keeps successful admission to one syntax traversal,
one final document copy, and bounded property-name bookkeeping.

## Value model

The supported root and child kinds are null, false, true, number, string,
array, and object. Numbers use the JSON grammar: an optional minus, a canonical
integer part without a leading zero, an optional fraction, and an optional
base-10 exponent. A number retains its exact admitted spelling; numeric
coercion and equality belong to the future typed query boundary.

Strings accept unescaped strict UTF-8 and the JSON quote, reverse-solidus,
solidus, backspace, form-feed, newline, carriage-return, tab, and `\uXXXX`
escapes. UTF-16 high surrogates require one immediately following low
surrogate. Lone, reversed, malformed, or truncated surrogate sequences reject.
`Databaseˉjsonˉstringˉdecode` returns the exact decoded UTF-8 bytes.

Objects use decoded property names. Equal names reject even when their source
spellings differ, such as `"a"` and `"\u0061"`. The exact decoded names
`__proto__`, `prototype`, and `constructor` reject at every object depth.
Property order otherwise remains significant only as retained source spelling;
the value contract does not define map equality.

## Limits

| Resource | Maximum |
| --- | ---: |
| Complete JSON document | 65,536 bytes |
| Nesting depth, including the root | 16 |
| Value nodes | 4,096 |
| Properties in one object | 256 |
| Items in one array | 1,000 |
| Decoded property name | 128 bytes |
| Decoded string | 61,408 bytes |
| Number token | 128 bytes |
| Escapes in one string | 4,096 |

The escape limit bounds both decoding work and immutable intermediate
construction for deliberately escape-heavy strings. Property-name duplicate
checks retain at most 33,792 bytes and compare at most 256 admitted names.
There is no unbounded host collection, locale operation, or normalization.

## Strict admission

Admission rejects an empty or whitespace-only document, invalid UTF-8, a byte
order mark, comments, trailing commas, missing separators, multiple roots,
trailing non-whitespace bytes, invalid literals or numbers, raw string control
bytes, malformed escapes, excessive resources, unsafe property names, and
semantic duplicate names. Whitespace is only space, tab, line feed, or carriage
return in positions allowed by the JSON grammar.

Failure returns a typed reason and byte offset and no admitted document bytes.
Success returns the root kind, exact recursive node count, and an owned byte
copy. Equal inputs produce equal results on every host.

## Performance and memory

Syntax and node accounting are single-pass. Each object decodes each property
name once, then checks a bounded compact name directory; application string
values remain in the admitted document until explicitly decoded. This avoids a
second full-document tree allocation and makes the cost visible in input bytes,
nodes, properties, and escapes.

The focused fixture exercises maximum depth, exactly 4,096 nodes, exactly 256
properties, exactly 1,000 array items, a 61,408-byte string, and 4,096 escapes,
plus one-over-limit and malformed cases. These are admission limits, not claims
about a future server's request concurrency or total memory budget.

## Exclusions

This contract does not yet define protocol envelopes, canonical JSON output,
JSON numeric normalization, JSON-to-typed-row conversion, path evaluation,
JSON indexes, query semantics, schema defaults, or migration from EWDB. Those
must build on this admission boundary rather than bypass it.
