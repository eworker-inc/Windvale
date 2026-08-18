# Windvale source body parser

## Status and purpose

`Compilerˉsourceˉbodyˉparser` is the third Windvale-written compiler slice. It parses the complete implemented Seed statement/expression grammar from the exact function-body spans supplied by the declaration pass.

The implementation is cross-host qualified under Decision 0027. It imports `Compilerˉsourceˉdeclarationˉparser`, which imports the qualified lexer and decimal parser.

## Structural boundary

The parser recognizes:

- `let` and `var` locals with an optional explicit non-void, non-array type and one required initializer;
- simple identifier `=`, `+=`, `-=`, and `*=` assignment;
- `if` plus optional recursive block-form `else if` and final block `else`, `while`, bounded `for`/`in`, exhaustive `match`/`case`, narrow `try` propagation, `push`, nearest-loop-shaped `break` and `continue`, nested blocks, return, and expression statements;
- `i32`, `i64`, `u8`, `u32`, and `u64` integer literals plus string-shape and Boolean literals;
- the Language 1.0 empty-parentheses unit literal `()`;
- qualified names, field access, one call or index postfix, named record literals, Language 1.0 named record updates, variant constructors, and bounded builder construction;
- unary `-`, `!`, `~`, and consuming `freeze`;
- `||`, `&&`, bitwise `|`/`^`/`&`, equality, comparisons, shifts, addition/subtraction, and multiplication/division/remainder with the Stage 0 precedence and left-associativity rules;
- nonempty parenthesized expressions without a synthetic group node.

This is syntax only. A string view retains its source bytes rather than decoding a durable text value. Assignment and builder mutation remain limited to simple-name targets. Statement kinds append `Match = 10`, `Push = 11`, `For = 12`, and `Try = 13`; expression kinds append `Builder = 11`, `Unit = 12`, and `Recordˉupdate = 13`. A unit expression spans both parentheses, owns one node at depth one, and has no child or payload. A record update retains the target name, one base-expression span, and the nonempty replacement-field interior/count. A base expression containing its own top-level brace construction is parenthesized at the current parser boundary so the replacement list remains lexically unambiguous. Edition admission, base nominal identity, field checks, and exact evaluation semantics belong to the later typed-WIR phase. A `try` statement records the one expression between its keyword and required semicolon. Semantic lowering proves its exact result contract as well as exhaustiveness, payload binding, collection types, affine builder use, and loop placement.

An inferred local publishes `Unknown` as its syntax type kind and zero type-span length. This is an explicit parser representation of an omitted annotation, not a semantic value shape; typed WVIR construction resolves it from the initializer. Comma-separated parameter, call-argument, positional-constructor, named-record-field, and static-data lists accept one final trailing comma under the Seed grammar. `Namedˉrecord = 10` is appended to the expression-kind contract. The named form is recognized only when a qualified name is followed by `{ Identifier :`, so an ordinary condition followed by its block remains unambiguous.

## Immutable views

`Compilerˉsourceˉstatement` records the statement kind, whole span, optional name/type/expression spans, first and second block spans, mutability, next streaming cursor, aggregate descendant counts/depths, and first failure evidence.

`Compilerˉsourceˉexpression` records the root kind/operator, whole span, literal/name/operator payload span, first and second child spans, call-argument interior/count, numeric classification plus low/high `u32` value limbs, Boolean value, next cursor, node count/tree depth, and first failure evidence.

These records are flat scalar/enum data. They do not own child records or runtime handles. To traverse a child, derive its position from a known parent position with `Compilerˉbodyˉpositionˉbetween`, then parse the bounded child span. Call arguments, named-record field/value pairs, and record-update replacements are streamed from the recorded interior rather than retained as a collection. An `else if` stores the nested `if` statement as its second span; consumers accept that one-statement span recursively as well as ordinary brace-delimited block spans.

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
- at most 64 call arguments, named-record fields, or record-update replacements and 16 qualified-name parts;
- at most 4,096 statements per function body;
- at most 4,096 nodes in one expression and 262,144 expression nodes in one body.

The first failure is deterministic and includes lexical status, expected/found token kinds, and byte/line/column position. Recoverable multi-error diagnostics remain deferred to semantic compiler pressure.

## Current deterministic artifacts and retained evidence

- `Source-Body-Parser.wvb`: 249,214 bytes, SHA-256 `6a0cd2ea9987778490ac15321d3604d06e49a2e327502f02070c8f83ff1089fb`.
- `Source-Body-Parser-Demo.wvb`: 256,044 bytes, SHA-256 `08e3d5ceeada8f4361e953c5270efb4870fd53525dd5bf5ed58c2e3ad2f94654`, result `0` under 30,000,000 instructions.
- `Source-Body-Parser-Tool.wvb`: 248,436 bytes, SHA-256 `2ecc13dbc108befc33dd86851caab804d87576443c507d13959b5232ad76b2c6`.

Decision 0516 makes the native Project 1 front door the ordinary constructor
for all three WVBs and binds the core's exact portable type/export surface
through native inspection. The demo and capability-bearing hosted-tool runs
remain in the managed differential lane because the scalar native runner stops
the demo at runtime code `3004` and does not bind the tool's console,
diagnostic, file, and process capabilities. This is local Windows transfer
evidence, not a new cross-host qualification claim.

These are the current local deterministic identities after initializer inference, named-record parsing, recursive `else if`, loop-control statements, short-circuit operators, and compound assignment. Cross-host qualification remains required before they replace the retained qualified baseline claim.

The real-source reports are:

```text
source bodies status=Valid functions=19 top-level=131 statements=749 expression-nodes=2153 statement-depth=17 expression-depth=5 offset=56313
source bodies status=Valid functions=32 top-level=365 statements=921 expression-nodes=3601 statement-depth=12 expression-depth=5 offset=112568
source bodies status=Valid functions=48 top-level=339 statements=812 expression-nodes=3607 statement-depth=7 expression-depth=3 offset=110706
```

These correspond to the lexer, declaration parser, and body parser. Their ceilings remain respectively 100,000,000, 160,000,000, and 160,000,000 instructions. The body parser was originally cross-host qualified at `ddfa9e3`; Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`. Decision 0055's reuse and containment implementation is cross-host qualified at `1a4fca7`.
