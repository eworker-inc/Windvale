# Decision 0330: Manifest-driven native retirement test suite

- Date: 2026-08-06
- Status: Implemented current-host focused evidence; complete Windows and Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), and [Decision 0305](0305-Digest-Bound-Native-Aot-Chain-Test.md)
- Contract: [Native retirement test suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)

## Context

Ten fixed native commands now own 74 deterministic result, failure, malformed,
read-only, assembler, lowerer, linker, packager, publisher, and AOT-chain cases.
Each command can run without .NET, but Qualification still reaches the commands
through individual managed Seed wrappers. That leaves no single direct
Windows/Linux test boundary to replace those wrappers at the retirement gate.

Porting the managed test runner would preserve the dependency being retired.
Allowing a coordinator to discover commands or infer their expected output
would also let the candidate define its own acceptance conditions.

## Decision

- Add one versioned, LF-only manifest that fixes the ordered command stems,
  declared case counts, and exact terminal success summaries for all ten lanes.
- Pin the complete 787-byte manifest at SHA-256
  `4ffd86e1d5891c8968dadde7c52e745f2695cf11b309d597696b943e06b098e0`
  in paired Windows and Linux coordinators.
- Require every selected child to exit `0`, write no standard error, and end
  standard output with its manifest-owned exact summary before the coordinator
  reports that suite as passed.
- Make no arguments select all 74 cases in manifest order. Add exact
  `--filter <suite-name>` selection for the narrow development check that owns
  a coherent edit.
- Do not add another managed Seed wrapper for the coordinator. Reserve its
  unfiltered run for the final grouped retirement gate instead of adding a new
  verifier-ladder step.

## Evidence and consequences

- Static review confirms all ten entries have paired `.cmd` and `.sh` commands,
  and their declared counts total exactly 74. The Linux coordinator passes Bash
  syntax validation.
- After reviewing the affected scripts and manifest, the Windows command
  `Test-Retirement-Suite.cmd --filter unsafe-wvb` passes its one selected suite
  and all five underlying cases in 2.624 seconds.
- The complete 74-case command was deliberately not run locally. Linux
  execution and the grouped end-of-goal Windows/Linux gate remain deferred.
- This creates a direct .NET-free suite boundary without changing any product,
  WVB, WVO, native artifact, or WebAssembly implementation. It does not promote
  a candidate or authorize deletion of Stage 0.
- A future transferred test belongs in an existing cohesive child command when
  possible. Add a new suite entry only when it owns a distinct execution or
  failure boundary; do not split tests into numbered fragments merely to keep
  individual files small.

## Reconsideration triggers

Revise the manifest version and digest when a child inventory, ordering, or
terminal summary changes. Replace a host script with Windvale-owned
orchestration when the required process and file capabilities exist, while
preserving the same fail-closed manifest and channel contract. Do not widen this
fixed suite into seeded randomized, OS-boot, or bootstrap work until those lanes
have explicit native owners and independent cross-host acceptance contracts.
