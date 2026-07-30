# Decision 0001: Repository and foundation

- Date: 2026-07-29
- Status: Accepted foundation with explicitly proposed technical details

## Context

Windvale began as an idea for demonstrating AI-assisted construction of a small assembler, linker, programming language, portable bytecode environment, foundation library, and operating system.

The project needs useful intermediate outcomes and must avoid rewriting every tool when Windvale OS becomes capable of hosting them.

## Accepted decisions

- Use **Windvale** as the umbrella project and repository name.
- Establish the project with open-source intent; select the exact license before public source release.
- Use a shared bare Git repository and isolated development-lane layout consistent with the surrounding E-Worker environment.
- Keep Windows and Linux as permanent development and runtime hosts.
- Define Windvale semantics and platform contracts independently from host operating-system behavior.
- Plan for one source language with portable-bytecode and native-code execution forms.
- Keep portable, hosted, and system capabilities explicit.
- Preserve assembler, linker, runtime, compiler, and library usefulness independently of the future OS.
- Document bootstrap dependencies and replacement stages honestly.

## Repository workflow

- Local worktree and mirror locations are environment-specific and are not part of the public project contract.
- Each checkout uses its configured repository remote.
- The default branch is `main` until a later branching decision changes it.

The initial environment used a shared Windows bare repository and isolated development lanes. Those operational details are intentionally not public hosting requirements.

## Proposed, not yet accepted

- C# as the Stage 0 compiler/tool implementation language
- A typed Windvale IR
- A custom verifiable Windvale bytecode
- Restricted C as a transitional native backend
- x86-64 with UEFI as the first OS target
- QEMU as the primary automated VM and Hyper-V as a compatibility target

Decision 0002 subsequently accepted C#, typed WIR, custom verified bytecode, and the first Seed semantic subset. The remaining native and OS choices above are still proposals.

## Consequences

- Early Windows and Linux work remains part of the finished Windvale ecosystem.
- The project needs conformance tests that compare hosts and backends.
- Windvale must specify behavior that C, Windows, Linux, and future native backends might otherwise leave undefined.
- Host adapters and capability declarations become architectural components rather than scattered conditional code.
- Self-hosting can happen incrementally without requiring the OS to be finished first.

## Reconsider when

- A selected bootstrap language materially delays Windows or Linux development.
- The bytecode and native semantic models cannot remain meaningfully aligned.
- A second architecture or non-desktop host becomes a near-term requirement.
- The repository moves to public hosting or accepts outside contributions.
