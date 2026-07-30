# Decision 0027: Streaming statement and expression views before syntax collections

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The qualified declaration pass supplies exact immutable function-body byte spans but deliberately does not inspect their grammar. The next bootstrap requirement is to reproduce the Stage 0 statement and expression decisions in Windvale without assuming that a general syntax tree, token collection, arena, or diagnostic collection is already justified.

The declaration-parser pressure showed that offset-based views and bounded rescanning are sufficient for namespace discovery. Body syntax adds recursive structure, operator precedence, nested blocks, and ordered call arguments. It therefore needs child relationships, but it does not yet need durable ownership of every node.

## Decision

Create portable module `Compilerˉsourceˉbodyˉparser` above the qualified declaration parser. It validates function bodies and returns flat immutable statement and expression records. Parent views identify child expressions, blocks, names, types, and argument interiors by byte span. A later semantic pass can recurse into those spans; the parser retains no token, statement, expression, or tree collection.

Reproduce the implemented Stage 0 grammar exactly: immutable/mutable locals, simple-name assignment, `if`/`else`, `while`, return with or without a value, nested blocks, expression statements, integer/string/boolean literals, qualified names, two-part field access, calls, indexing, unary minus/not, and the existing equality/comparison/additive/multiplicative precedence. Parentheses reframe an expression span without manufacturing a syntax node.

`Compilerˉparseˉnextˉstatementˉvalidated` and `Compilerˉparseˉexpressionˉvalidated` are the streaming/view contracts. `Compilerˉparseˉbodyˉspanˉvalidated` validates one exact declaration body. `Compilerˉparseˉsourceˉbodies` walks the declaration stream once, derives each body position from the known source cursor, and aggregates every function without changing the qualified declaration record.

Preserve the existing UTF-16-compatible line/column model. Child views keep byte spans; `Compilerˉbodyˉpositionˉbetween` derives a child position from a known parent cursor without a global position table.

Limits are explicit: 4 MiB source and 262,144 lexer tokens from the lower layer; 64 statement nestings; 64 expression nestings/tree depth; 64 call arguments; 16 qualified-name parts; 4,096 statements per function body; 4,096 nodes per expression; and 262,144 expression nodes per body. The pass fails fast with lexical status, parser status, expected/found token kinds, and source coordinates.

The current Seed source-composition subset requires every dependency helper retained in a composed module to be exported. The body module therefore has a larger mechanical export surface than its conceptual public API. Do not add a new visibility feature solely to hide these helpers; revisit this when compiler/semantic modularity creates a second concrete need.

## Consequences

Windvale can now validate and structurally inspect the complete implemented Seed syntax in its lexer, declaration parser, and body parser while running as verified bytecode. The flat views supply the child seams required by semantic binding without choosing an allocation/ownership model prematurely.

The real sources demonstrate that the collection-free design is practical, but rescanning costs are visible. Conservative qualification ceilings are 100,000,000 instructions for the lexer and 160,000,000 each for the declaration parser and body-parser self pass. Future semantic pressure may justify a bounded packed node table or cursor stack; it must show a material correctness, ownership, or performance benefit.

This is not semantic binding. It does not resolve names, validate types or mutability, determine control-flow reachability, decode string values, retain recoverable diagnostics, construct WIR, emit WVB, or close the bootstrap.

## Verification gate

The exact candidate must pass the complete 42-test and native CLI verifiers on Windows and Debian. Tests cover every statement/expression form, precedence and child spans, exact body boundaries, lexical/declaration propagation, 65-argument rejection, 65-level expression and block rejection, and 4,097-statement rejection.

Both hosts must produce identical parser, demo, and hosted-tool WVB files. The hosted tool must emit identical aggregate reports for `Source-Lexer-Core.wv`, `Source-Declaration-Parser.wv`, and `Source-Body-Parser.wv`. Normalized conformance reports must match, and all previously qualified lexer, declaration, object, assembler, linker, object, image, and map identities must remain unchanged.
