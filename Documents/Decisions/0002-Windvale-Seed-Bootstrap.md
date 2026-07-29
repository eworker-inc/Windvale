# Decision 0002: Windvale Seed bootstrap

- Date: 2026-07-29
- Status: Accepted

## Context

Windvale needs the shortest credible path from an existing host environment to an owned source language and portable execution contract. The first milestone must be useful on Windows and Linux without pre-emptively building the assembler, linker, native backend, or operating system.

## Decision

- Name the first executable milestone **Windvale Seed**.
- Use C# and a pinned .NET 10 SDK as the Stage 0 implementation environment.
- Keep the Stage 0 implementation free of external NuGet dependencies.
- Compile a deliberately small Windvale source subset through a typed, stack-independent Windvale IR into typed stack bytecode.
- Use indexed locals in bytecode and require static stack verification before execution.
- Define `i32`, `bool`, immutable UTF-8 `text`, and immutable `[i32]` module data in Seed.
- Use a sectioned, little-endian `.wvb` module format with canonical declaration ordering.
- Expose `compile`, `inspect`, `verify`, and `run` through one `windvale` command-line host.
- Require explicit authorization when running modules that import hosted capabilities.
- Keep the C# implementation as the reference and recovery bootstrap when self-hosting work begins.

These are implementation decisions, not compatibility promises. Seed formats may change without backward readers while Windvale remains in active early development.

## First conformance programs

- A portable program loops over immutable integer data and returns its sum.
- A hosted program writes immutable text through the declared `console.write_line` capability and returns zero.

## Consequences

- Compiler semantics do not depend on C#, the CLR, Windows, or Linux behavior.
- The bytecode verifier is a mandatory trust boundary rather than an optional diagnostic pass.
- The internal IR remains suitable for later C and native backends without making bytecode the compiler's private IR.
- The initial interpreter favors clarity and determinism over optimization.
- Cross-host claims require the same conformance suite and golden module evidence on both Windows and Linux.

## Reconsider when

- Static stack verification makes ordinary control flow materially more complex than a small register bytecode.
- A required semantic operation cannot be represented without inheriting CLR behavior.
- The section format prevents bounded validation or deterministic serialization.
- A bootstrap dependency materially reduces reproducibility or prevents one of the required hosts from participating.
