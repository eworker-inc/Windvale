# Windvale source body parser

## Status and purpose

`Compilerˉsourceˉbodyˉparser` is the third Windvale-written compiler slice. It parses the complete implemented Seed statement/expression grammar from the exact function-body spans supplied by the declaration pass.

The implementation is cross-host qualified under Decision 0027. It imports `Compilerˉsourceˉdeclarationˉparser`, which imports the qualified lexer and decimal parser.

## Structural boundary

The parser recognizes:

- `let` and `var` locals with one explicit non-void, non-array type and initializer;
- simple identifier assignment;
- `if` plus optional block `else`, `while`, nested blocks, return, and expression statements;
- integer, string-shape, and boolean literals;
- qualified names, two-part field access, one call or index postfix;
- unary `-` and `!`;
- `==`/`!=`, comparisons, `+`/`-`, and `*` with the Stage 0 precedence/associativity rules;
- parenthesized expressions without a synthetic group node.

This is syntax only. A string view retains its source bytes rather than decoding a durable text value. Assignment remains limited to the simple-name form accepted by Stage 0.

## Immutable views

`Compilerˉsourceˉstatement` records the statement kind, whole span, optional name/type/expression spans, first and second block spans, mutability, next streaming cursor, aggregate descendant counts/depths, and first failure evidence.

`Compilerˉsourceˉexpression` records the root kind/operator, whole span, literal/name/operator payload span, first and second child spans, call-argument interior/count, numeric classification/value, boolean value, next cursor, node count/tree depth, and first failure evidence.

These records are flat scalar/enum data. They do not own child records or runtime handles. To traverse a child, derive its position from a known parent position with `Compilerˉbodyˉpositionˉbetween`, then parse the bounded child span. Call arguments are streamed from the recorded interior rather than retained as a collection.

`Compilerˉsourceˉbodyˉsummary` reports functions, top-level statements, total statements, expression nodes, maximum statement/expression depths, final cursor, and failure evidence.

## Entry points

```text
Compilerˉparseˉexpressionˉvalidated(Input, Offset, Line, Column, Endˉoffset, Parentˉprecedence, Nesting)
    -> Compilerˉsourceˉexpression

Compilerˉparseˉexpressionˉspan(Input, Offset, Line, Column, Endˉoffset)
    -> Compilerˉsourceˉexpression

Compilerˉparseˉnextˉstatementˉvalidated(Input, Offset, Line, Column, Endˉoffset, Statementˉnesting)
    -> Compilerˉsourceˉstatement

Compilerˉparseˉblockˉvalidated(Input, Offset, Line, Column, Endˉoffset, Statementˉnesting)
    -> Compilerˉbodyˉparseˉstep

Compilerˉparseˉbodyˉspan(Input, Offset, Line, Column, Endˉoffset)
    -> Compilerˉsourceˉbodyˉsummary

Compilerˉparseˉsourceˉbodies(Input)
    -> Compilerˉsourceˉbodyˉsummary
```

Safe `span`/whole-source entries lexically preflight input. Validated entries are for a compiler pipeline that already performed the complete lexical/declaration pass. Every end offset is exclusive; an exact body span includes both braces and must end immediately after the closing brace.

## Status and limits

`Compilerˉbodyˉparseˉstatus` contains `Valid`, `Lexicalˉerror`, `Invalidˉspan`, `Expectedˉtoken`, `Expectedˉexpression`, `Invalidˉtype`, `Nestingˉlimit`, `Itemˉlimit`, and `Declarationˉerror`.

The pass enforces:

- the lower-layer 4,194,304-byte source and 262,144-token ceilings;
- at most 64 statement nestings;
- at most 64 expression nestings/tree depth;
- at most 64 call arguments and 16 qualified-name parts;
- at most 4,096 statements per function body;
- at most 4,096 nodes in one expression and 262,144 expression nodes in one body.

The first failure is deterministic and includes lexical status, expected/found token kinds, and byte/line/column position. Recoverable multi-error diagnostics remain deferred to semantic compiler pressure.

## Current candidate artifacts and evidence

- `Source-Body-Parser.wvb`: 167,777 bytes, SHA-256 `7b56ea4d25f2d13467d19123654bb8d617ae2e1b0dd43f2497e1ff9644cc3839`.
- `Source-Body-Parser-Demo.wvb`: 171,689 bytes, SHA-256 `07f9b2d94b4ebefaa3260d04b2cc7400b56007f664ef93a8db97207679039005`, result `0` under 30,000,000 instructions.
- `Source-Body-Parser-Tool.wvb`: 168,913 bytes, SHA-256 `9c8b88f9b6aaa27df5d39fc671319ed4890510535321f637a533cf2f01ddeadc`.

The real-source reports are:

```text
source bodies status=Valid functions=14 top-level=123 statements=567 expression-nodes=1579 statement-depth=17 expression-depth=5 offset=41737
source bodies status=Valid functions=24 top-level=232 statements=527 expression-nodes=2135 statement-depth=5 expression-depth=3 offset=64951
source bodies status=Valid functions=38 top-level=234 statements=519 expression-nodes=2500 statement-depth=5 expression-depth=3 offset=69023
```

These correspond to the lexer, declaration parser, and body parser. Their ceilings remain respectively 100,000,000, 160,000,000, and 160,000,000 instructions. The body parser was originally cross-host qualified at `ddfa9e3`; the current artifacts embed Decision 0042's lexer and await exact requalification.
