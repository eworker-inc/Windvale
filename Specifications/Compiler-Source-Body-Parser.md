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

Compilerˉparseˉsourceˉbodiesˉfromˉdeclarations(Input, Declarationˉsummary)
    -> Compilerˉsourceˉbodyˉsummary
```

Safe `span`/whole-source entries lexically preflight input. The checked body-span boundary also performs an iterative block-shape preflight so an over-deep span returns `Nestingˉlimit` without recursive host-stack exhaustion. Validated entries are for a compiler pipeline that already performed the complete lexical/declaration pass. `Compilerˉparseˉsourceˉbodiesˉfromˉdeclarations` reuses the caller's accepted declaration summary and does not repeat that pass. Every end offset is exclusive; an exact body span includes both braces and must end immediately after the closing brace.

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

## Qualified milestone artifacts and evidence

- `Source-Body-Parser.wvb`: 175,055 bytes, SHA-256 `3df42c7b6e81343194340b8f6f44e44fb83f3d6f18c249c9d9ed4e58df69ec73`.
- `Source-Body-Parser-Demo.wvb`: 179,955 bytes, SHA-256 `afa07f843679e89f84a5a55887af834575d43d4a3ac3f1a76cd4395a103e62b6`, result `0` under 30,000,000 instructions.
- `Source-Body-Parser-Tool.wvb`: 176,131 bytes, SHA-256 `342fadc0886e5b8b2910cb65c8495730a902364a526fd34df58c574a32a91890`.

The real-source reports are:

```text
source bodies status=Valid functions=17 top-level=111 statements=602 expression-nodes=1670 statement-depth=17 expression-depth=5 offset=45589
source bodies status=Valid functions=25 top-level=245 statements=615 expression-nodes=2366 statement-depth=12 expression-depth=4 offset=70592
source bodies status=Valid functions=39 top-level=237 statements=523 expression-nodes=2520 statement-depth=5 expression-depth=3 offset=69903
```

These correspond to the lexer, declaration parser, and body parser. Their ceilings remain respectively 100,000,000, 160,000,000, and 160,000,000 instructions. The body parser was originally cross-host qualified at `ddfa9e3`; Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`. Decision 0055's reuse and containment implementation is cross-host qualified at `1a4fca7`.
