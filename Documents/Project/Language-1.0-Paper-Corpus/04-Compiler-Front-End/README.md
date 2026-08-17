# Language 1.0 paper workload 4: compiler front end

## Status

Complete first-author bundle, draft reviewed by the project owner on
2026-08-17. [Decision 0758](../../../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md)
accepts the explicit generic-construction, borrowed-observation, immutable-arena,
source-position, diagnostic, and phase-publication findings.

This is paper Language 1.0 source. It is not the current Windvale compiler, is
not accepted by Seed tools, and does not freeze or implement edition 1.

## Result

Eight Core modules implement a small complete deterministic compiler front end:

1. admit bounded input bytes and decode strict UTF-8;
2. lex official macron-separated identifiers, two keywords, `u64` literals, and
   expression punctuation while retaining byte/rune/line/column spans;
3. parse declarations and a return expression with multiplication/addition
   precedence and bounded parenthesized recursion;
4. publish tokens, declarations, and the typed node arena immutably;
5. bind names through an ordered map and emit duplicate/unknown diagnostics;
6. publish canonical symbols and stack operations; and
7. emit one versioned canonical byte artifact only when diagnostics are empty.

The representative input language is deliberately smaller than Windvale. It is
large enough to test real lexer/parser/binder ownership and recovery without
claiming that this paper module replaces the qualified Seed compiler.

## Source modules

| Module | Responsibility |
| --- | --- |
| `Frontˉendˉtypes` | Positions, tokens, recursive nodes, phase records, diagnostics, limits, failures, report. |
| `Frontˉendˉwork` | One shared exact work meter. |
| `Frontˉendˉdiagnostics` | Reserved diagnostic vector and deterministic saturation. |
| `Frontˉendˉlexer` | Strict scalar-order lexing and token publication. |
| `Frontˉendˉparser` | Recursive precedence parser, typed arena, declarations, recovery. |
| `Frontˉendˉbinder` | Ordered symbol map, generation-safe node traversal, immutable bound operations. |
| `Frontˉendˉencoder` | Version-1 canonical byte encoding. |
| `Frontˉendˉapplication` | Limit validation, nine budget splits, phase orchestration, publication. |

All modules are Core, portable to Windows, Linux, and Windvale, and require no
capability.

## Evidence index

- [Front-end contract](Front-End-Contract.md)
- [Output format](Output-Format.md)
- [Package plan](Package-Plan.md)
- [Semantic review](Semantic-Review.md)
- [Rejected cases](Rejected-Cases.md)
- [Expected outcomes](Expected-Outcomes.md)
- [Implementation responsibilities](Implementation-Responsibilities.md)
- [Review findings](Review-Findings.md)

## Acceptance answer

The source materially improves on packed offsets and wide flat records. Tokens,
spans, nodes, declarations, bindings, operations, failures, and diagnostics are
nominal. Recursive edges are checked handles rather than pointers. Every mutable
phase owner becomes immutable before the next phase observes it. The visible
ownership annotations identify real budgets, mutation, or borrows; they do not
dominate the successful path.

## Nonclaims

The bundle does not define the complete edition-1 grammar, module graph, type
checker, ownership checker, WIR, WVB writer, optimizer, native backend, editor,
or package build. It adds no compiler capability, ambient file access, macro,
reflection, host string indexing, database, class, exception, or tracing GC.
