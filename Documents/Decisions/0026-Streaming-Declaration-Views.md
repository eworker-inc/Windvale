# Decision 0026: Streaming declaration views before statement trees

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified

## Context

The qualified Windvale lexer made the first parser pressure concrete. A direct copy of the Stage 0 syntax tree would require token, declaration, parameter, field, member, statement, expression, and diagnostic collections at once. Most of those collections are not needed for the compiler's first pass, whose job is to discover module identity and declaration shapes before binding bodies.

The qualified assembler and linker already demonstrate a useful alternative: validate immutable input once, expose offset-based views, and stream or rescan those views in later semantic passes. Applying that pattern to source declarations advances self-hosting without prematurely committing Windvale to a general heap or collection design.

## Decision

Create portable module `Compilerˉsourceˉdeclarationˉparser` above `Compilerˉsourceˉlexer`. It validates one complete source with the lexer, parses the module header and every top-level declaration, and returns flat immutable records containing byte spans, counts, kinds, cursors, and first-failure evidence.

The declaration pass parses imports, qualified capabilities, all three module-data forms, record fields, enum members, function export/name/parameter/return signatures, and the balanced outer function body span. It does not parse statements or expressions inside that body yet. Balanced token scanning is intentional: later body parsing receives an exact independently bounded span after the declaration namespace is known.

`Compilerˉparseˉheaderˉvalidated` and `Compilerˉparseˉnextˉdeclarationˉvalidated` form the streaming contract for a caller that validated the source once. `Compilerˉparseˉsource` produces a whole-module count summary without retaining declarations. `Compilerˉparseˉdeclarationˉat` is a verification convenience; production semantic passes should keep the returned cursor instead of repeatedly indexing from the beginning.

The pass is fail-fast and reports the lexer status, parser status, expected and found token kinds, and byte/line/column failure coordinates. Recoverable multi-error diagnostics remain deferred until body and semantic parsing show the necessary ownership model.

Limits are explicit: 4 MiB source, 262,144 lexer tokens, 4,096 declarations, 64 record fields, 256 enum members, 64 function parameters, 16 qualified-name parts, and 64 nested body braces. The parser allocates no token or declaration collection.

Add a thin hosted tool only for explicit file input and deterministic reporting. The portable parser remains capability-free. The verifier uses the tool to parse the real Windvale lexer and the declaration parser's own source.

## Consequences

Windvale can now discover its own compiler declarations as verified bytecode. The declaration pass can feed namespace and signature binding directly through streaming cursors and immutable source spans.

No general collection is justified yet. Statement and expression parsing may still prove that a bounded packed collection, an offset table, or structured diagnostic sequence is worthwhile; that decision will be based on the next implementation rather than guessed here.

This is not a complete Seed parser. Balanced bodies may contain invalid statement syntax that this pass accepts for later rejection. It does not decode string values, bind names or types, construct statement/expression trees, recover after errors, build WIR, emit WVB, or close the bootstrap.

## Verification gate

The portable demo must cover every declaration form, data literals, qualified names, types, parameter/field/member counts, export and body spans, import ordering, expected-token failures, profiles, invalid data/type/literal cases, unexpected declarations, missing braces, and lexical propagation.

The exact candidate must pass the complete 41-test and native CLI verifiers on Windows and Debian. Both hosts must produce identical parser, demo, and hosted-tool WVB files; the tool must emit identical reports while parsing `Source-Lexer-Core.wv` and `Source-Declaration-Parser.wv`; normalized conformance reports must match; and all prior artifacts must remain unchanged.

Candidate `fc87a3e` satisfied this gate on Windows and Debian GNU/Linux 12 x64 with zero build warnings/errors and all 41 tests. The normalized reports matched; the parser, demo, hosted tool, lexer, lexer demo, object core, assembler, linker, assembled object, linked image, and map were directly byte-identical. The hosted tool reported the same declaration counts and source endpoints for the real lexer and its own declaration source on both hosts.
