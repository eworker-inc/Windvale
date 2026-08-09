# Decision 0429: Fixed native assembler golden objects

- Status: Implemented current-host focused evidence; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), [Decision 0125](0125-Typed-Wva-Byte-Word-And-Terminal-Migration.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native retirement test suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The native retirement suite already owns every stable WVA rejection family and
a frozen 200-value differential corpus. Positive exact-byte encoding evidence
for the canonical Hello, expanded-x64, and typed-scalar-x64 sources still
primarily entered through the managed Seed harness. Losing that evidence when
the managed harness eventually becomes recovery-only would leave a gap between
parser rejection coverage and useful object production.

These three repository-maintained sources are stable golden contracts. They can
be checked directly without porting the C# assembler test runner or embedding a
second object decoder in host scripts.

## Decision

Add paired focused `Tools/Native/Test-Assembler-Golden.cmd` and `.sh` commands.
For each exact source, they:

1. admit the complete source identity;
2. assemble it twice through the digest-bound native assembler;
3. require the exact two-line success report;
4. require the exact WVO byte length and SHA-256;
5. independently admit the first result through the native WVO verifier; and
6. require complete equality between the two generated objects.

The accepted products are:

| Source | Source SHA-256 | WVO bytes | WVO SHA-256 |
| --- | --- | ---: | --- |
| `Hello-Object.wva` | `a88f748ba87df1a291752ee8bda896279edd8d9f8a7811692c2229bbaba8cea0` | 218 | `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85` |
| `Expanded-X64.wva` | `27a324b5c26c1e6a982c6f02b0a157ccfdcbb7500521dd8c95a381aa2ed20646` | 238 | `678551e9936ca1c901e2dc5ec129d2add73427edb1ea3d086bb4badbf1b6e4ad` |
| `Typed-Scalar-X64.wva` | `a66a36a06ac6375da7ed5287fe6fdae55901f5b8b236c3098723e7a6f856a4ef` | 396 | `860680074517025c69a2a6edf1dd9ff196475e05f9c50f95b53480c848c650c5` |

Add the command as one cohesive `assembler-golden` retirement-suite lane rather
than splitting three tiny commands. The complete plan becomes 24 suites and
3,038 fixed cases.

## Evidence and consequences

After reviewing the new host scripts and expected products, the Windows command
`Test-Retirement-Suite.cmd --filter assembler-golden` passes the selected suite
and all three cases in 2.1 seconds. It starts no managed process, builds each WVO
twice, admits each first result independently, and leaves no private scratch.

The command compares exact logical report lines while permitting the host's
native line termination. Product identity and determinism remain complete-byte
checks. No assembler, WVO, or native artifact changed in this slice.

This transfers three positive golden products, not every dynamic register,
relocation, linker, or malformed-source assertion in the managed suite. Those
remaining cases stay independent recovery evidence until separately transferred
or judged redundant. Linux execution and the unfiltered grouped gate remain
deferred to the goal-end qualification.

## Reconsideration triggers

Update source and product identities together when an accepted WVA encoding
contract changes. Add a case to this lane only when it is a stable positive
golden object; keep malformed inputs, differential corpora, and large segmented
objects in their existing focused owners.
