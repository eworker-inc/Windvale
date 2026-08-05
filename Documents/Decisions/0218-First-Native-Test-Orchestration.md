# Decision 0218: First native test orchestration

- Date: 2026-08-05
- Status: Implemented candidate; dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0217](0217-Windvale-Sha256-And-Native-Wvb-Runner-Profile.md)
- Contract: [Windvale native test plan](../../Specifications/Windvale-Native-Test-Plan.md)

## Context

The Seed C# runner contains more than one hundred conformance and integration
cases. Translating that monolith line for line would duplicate test policy, retain
one oversized coordinator, and obscure which native contracts are actually ready.
The bounded native runner instead gives the project a smaller cutover unit: stable
self-checking WVB fixtures with exact build identities and results.

## Decision

Add one digest-bound `WVNT 1` candidate containing two scalar tests. The calls and
control fixture covers multi-function execution, loops, branches, and arithmetic.
The scalar-core fixture additionally covers signed and unsigned comparisons,
booleans, and `u8`. Both must compile to their exact recorded WVB bytes and execute
to `42` through the pinned native runner.

Use thin `cmd.exe` and Bash launchers to bind repository paths, temporary files,
host SHA-256 utilities, and process results. Keep test semantics in Windvale source
and the common plan. Do not add a second C# implementation of these tests or a
general plan parser before a real dynamic-plan requirement exists.

## Consequences

- `Tools/Native/Test-Seed.cmd` and `.sh` execute this accepted subset without .NET.
- The full Stage 0 suite remains the qualification and differential lane while
  untransferred compiler, backend, assembler, linker, packaging, OS, and recovery
  boundaries still need its independent oracles.
- New native tests are transferred as focused fixtures with explicit expected
  identities and outcomes, not by growing one broad source file.
- The plan becomes an ordinary native smoke gate only after the exact candidate
  passes on Windows and Linux. It is not yet a substitute for `Verify-Seed`.

## Reconsideration triggers

Replace the fixed plan with a versioned dynamic bundle only when native
orchestration needs runtime selection, malformed fixture sets, multiple execution
modes, or richer structured reports. At that point, parsing and policy belong in a
focused Windvale module; platform scripts remain narrow adapters.
