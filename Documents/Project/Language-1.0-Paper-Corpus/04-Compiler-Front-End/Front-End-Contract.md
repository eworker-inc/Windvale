# Workload 4 front-end contract

## Representative input language

The paper compiler accepts strict UTF-8 bytes for this exact grammar:

```text
Module         ::= Declaration* Returnˉstatement End
Declaration    ::= "let" Identifier "=" Expression ";"
Returnˉstatement ::= "return" Expression ";"
Expression     ::= Multiplicative ("+" Multiplicative)*
Multiplicative ::= Primary ("*" Primary)*
Primary        ::= Unsignedˉdecimal | Identifier
                 | "(" Expression ")"
```

Whitespace is space, tab, CR, or LF. It is discarded while updating positions.
There are no comments, signs, separators, implicit semicolons, locale digits,
or numeric prefixes. Decimal values must fit `u64`.

Identifiers use the Language 1.0 ASCII-segment/U+02C9 form. Each segment begins
with ASCII letter or underscore and continues with ASCII letter, digit, or
underscore. Macron cannot begin/end a name or appear twice without a new segment.
The exact lowercase scalar sequences `let` and `return` are keywords.

Declarations are visible after their initializer. A self-reference is unknown.
Every unique declaration name reserves one deterministic ordinal even if another
diagnostic later prevents artifact publication. Duplicate detection compares
exact Unicode scalar sequences under Foundation's total text order.

## Positions and spans

`Sourceˉposition` contains zero-based byte offset, zero-based rune offset,
one-based line, and one-based scalar column. LF increments line and resets column
to one. CR is ordinary whitespace and increments column. Tabs occupy one scalar
column; display width and grapheme count are not source positions.

Every token uses a half-open `[Start, End)` span. The End token is zero width at
input end after a complete lex; after token-limit saturation it is instead the
zero-width synchronization point where lexing stopped. Source decode failures
guarantee only their exact byte offset; rune/line/column remain the predecode
sentinel `(0,1,1)` and cannot be mistaken for fully decoded position evidence.
Physical decode allocation failure is a separate typed outer failure without a
source diagnostic.

## Phase boundaries

| Producer | Mutable owner | Published result |
| --- | --- | --- |
| decoder | reserved UTF-8 construction | shared immutable `text` |
| lexer | `Vector<Token>` | `Sequence<Token>` including End |
| parser | `Vector<Declaration>`, `Arena<Node>` | `Sequence<Declaration>`, `Immutableˉarena<Node>` |
| binder | `Map<text,Binding>`, vectors | canonical `Sequence<Symbolˉrecord>` and `Sequence<Boundˉoperation>` |
| encoder | reserved bytes builder | immutable `bytes` |
| diagnostic sink | reserved vector | immutable diagnostic sequence at return |

Freeze consumes the mutable owner. No later phase observes hidden mutation or
retains a mutable borrow into a published phase model.

## Recursive syntax and handles

`Node` is a recursive variant whose Add/Multiply cases contain two
`Handle<Node>` values. The parser owns one runtime-bounded `Arena<Node>`.
Insertion produces generation-checked handles; immutable freeze preserves arena
identity, live slot generations, and handle validity while removing mutation.

Binder validation is recoverable for wrong/stale/corrupt handles. A validated
borrow is tied to the immutable arena and cannot escape traversal. The maximum
parenthesis and traversal depth is 64, but a package may select a smaller
positive limit. The host stack is not the semantic limit.

## Diagnostics and recovery

Diagnostics are stable structured records, not formatted host messages. The
sink retains at most the selected maximum, 2–16. It accepts at most
`Maximum - 1` ordinary diagnostics. The next issue becomes the one final
`Diagnosticˉlimit` record and saturates the sink; later issues retain no state.

Lexer recovery consumes one invalid rune or invalid token and continues. Parser
primary recovery consumes one unexpected token unless it is End, semicolon, or
right parenthesis. Missing semicolon recovery advances to semicolon, `let`,
`return`, or End. Binder records duplicates/unknown names while continuing
bounded traversal. Any diagnostic suppresses artifact publication.

Allocation, collection corruption, checked handle failure, and work exhaustion
remain typed outer failures when continuation cannot preserve a valid phase
model.

## Work and generic limits

One `Workˉmeter` spans lexer, parser, binder, and encoder. Each inspected rune,
consumed token, constructed/traversed node, declaration, symbol, and emitted
operation charges at least one. Its accepted maximum is 200,000. Exhaustion
returns the exact completed/maximum evidence; it never publishes a partial
artifact.

The source itself creates seven exact generic collection instances. The build
plan admits at most 256 total generic instances and depth 32 for this package.
A compiler exceeding either produces a bounded compilation diagnostic before
running the paper application.

## Determinism

Lexing and parsing are source order. Binding ordinals are declaration order.
Symbol publication is ascending exact text order. Operations are deterministic
postorder under fixed precedence and associativity. Output uses explicit tags,
little-endian widths, canonical UTF-8, and no host padding. Diagnostics are phase
and encounter order with stable saturation.
