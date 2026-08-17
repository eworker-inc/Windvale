# Decision 0748: Make Source Containment Compiler-Aware

- Status: Accepted
- Date: 2026-08-17
- Scope: ordinary compiler-source and direct-compiler-artifact verification

## Context

After compiler front-door admission became target-aware, source containment was
the largest common compiler owner. The source lane assigns each of 500 frozen
corpus inputs to two independent oracles:

1. a fresh instance of the direct compiler WebAssembly module; and
2. a separate native assembler process with input and destination preservation.

Compiler changes select this lane for the first oracle. They do not change the
native assembler, yet development launched it 500 times. The complete owner
measured 15,078.507 milliseconds on the Windows development host.

## Decision

- Add explicit `--compiler-only` parsing to the paired source-containment owner
  commands and the shared random-containment runner.
- In that mode, retain corpus admission, exact direct-compiler byte length and
  SHA-256, WebAssembly validation, import/export and ABI checks, fresh compiler
  instantiation, fixed memory bounds, the 2,000,000-instruction budget, and all
  500 exact compiler outcomes.
- Stop after those 500 compiler cases without admitting or launching the native
  assembler.
- Select this mode only for development-scoped compiler inputs and the exact
  direct compiler WebAssembly artifact.
- Force complete mode for changes to the corpus, containment host, corpus
  parser, source implementation, runner, paired owner commands, or containment
  specification.
- Keep no-argument owner execution, exact coordinator filters, qualification,
  the 500-case registry count, and both compiler and assembler oracles
  unchanged.

The mode has an explicit development dependency closure containing the direct
compiler artifact, frozen corpus, paired owner commands, and all shared source
containment producers. It is an affected-oracle selection, not corpus sampling:
no compiler case is removed.

## Evidence

Three consecutive compiler-only executions passed all 500 compiler cases in
9,171.520, 8,536.508, and 8,714.302 milliseconds. Their median was 8,714.302
milliseconds and their mean was 8,807.443 milliseconds. A complete no-argument
execution after the refactor passed all 500 dual-oracle cases in 15,078.507
milliseconds.

The development mode therefore removes 6,364.205 milliseconds, or 42.21
percent, from this owner on the measured host while retaining every compiler
input. Process-tree peak memory was not recorded because each compiler case
constructs a short-lived 163,643,392-byte WebAssembly memory and complete mode
also launches 500 bounded native child processes.

The same end-to-end changed-file command for
`Compiler/Windvale/Source-Lexer-Core.wv` measured 43,732.204 milliseconds before
compiler-only dispatch and 37,184.503 milliseconds afterward. It retained the
editor contract and all 550 selected owner cases. The measured end-to-end saving
is 6,547.701 milliseconds, or 14.97 percent, on top of target-aware front-door
selection.

Planner evidence must prove compiler-only selection for compiler paths and the
direct compiler artifact, complete selection for owner/corpus changes, zero
coverage gaps, and unchanged complete no-argument execution.

## Consequences

Ordinary compiler feedback no longer pays for an unchanged assembler oracle.
Assembler containment remains part of every complete source owner and
qualification run, and assembler changes retain their dedicated rejection,
golden, and differential owners.

The fresh 160-MiB-class compiler instance and explicit collection for each of
500 cases now dominate this lane. Reusing instances or collecting less often
would change isolation or peak-memory behavior and requires separate measured
evidence rather than being folded into this affected-oracle change.

## Reconsideration triggers

Reconsider if compiler and assembler source containment cease to be independent
oracles, if the compiler-only route omits a frozen compiler input, if the direct
artifact no longer selects the lane, or if complete containment stops running
in qualification.
