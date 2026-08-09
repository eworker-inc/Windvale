# Decision 0433: Fixed native WVA positive matrix

- Status: Implemented current-host focused evidence; complete lane and Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0432](0432-Fixed-Native-Scalar-X64-Golden-Object.md), [Decision 0336](0336-Fixed-Native-Wva-Differential-Corpus.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native WVA differential tests](../../Specifications/Windvale-Native-Wva-Differential-Tests.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The managed typed-scalar assembler assertion retained seventeen distinct
positive vectors beyond the fixed examples: one source for each paired 8-bit
and 16-bit register, plus one complete narrow immediate/shift group. These are
stable accepted semantics, but retaining seventeen loose sources or creating a
parallel suite would weaken the existing cohesive WVA differential owner.

The established 200-case corpus primarily owns rejection diversity: only one
no-op mutation is accepted. It can absorb the positive matrix while preserving
its exact original mutation sequence and assignment distribution.

## Decision

Run the frozen Stage 0 assertion once and retain only one deterministic compact
archive containing:

- sixteen LF-terminated sources covering immediate move, same-register move,
  condition materialization, and 16-bit multiply for `al`/`ax` through
  `r15b`/`r15w`; and
- `Typed-Narrow-Groups.wva`, covering 8/16-bit immediate ALU, compare, test,
  rotate, logical shift, and signed shift boundaries.

The 17 sources total 4,123 bytes and produce 1,707 exact Stage 0 WVO bytes. The
5,080-byte manifest pins every source, object, assembler report, and native
verifier report at SHA-256
`81172a33451d422ccc1e6c2a418041d6fc6436ad801d15f1adda45afe685ce28`.
The deterministic 3,576-byte archive has SHA-256
`ebb9e8e4ae5d90ace39f828996ebab9b75fc66d78c62ac7c58e86cf05ba9ba00`;
its 4,769-byte LF-only base64 representation has SHA-256
`a2e6a55419d7b4aaa3d1dbb6f7101e3a02aefb27f7d1d7309280e3b73877970b`.

Extend the existing paired `Test-Wva-Differential` commands. Their unfiltered
contract remains the grouped lane and grows from 200 to 217 cases. Add
`--positive-only` as the narrow inner loop; it decodes only the new archive and
runs exactly 17 cases. Every case must preserve its source, reproduce the exact
Stage 0 WVO, match the complete native assembler report, and pass independent
native WVO verification with its exact digest-bearing report.

The 2,054-byte retirement plan remains 24 suites and grows from 3,049 to 3,066
cases at SHA-256
`521488bb63e001cccc673db3e41c6718b20313a11b28a9e9421d735c6b992f56`.

## Evidence and consequences

The focused managed assertion passes 1/1 in 527 ms. The temporary source/WVO
exporter was removed and `Program.cs` returned byte for byte to its committed
state. The generated staging directory was removed after the permanent archive
identity was independently compared.

The reviewed Windows command
`Test-Wva-Differential.cmd --positive-only` passes all 17 cases in 7.0 seconds
without starting .NET. The unchanged 200-case mutation corpus was not rerun;
the complete 217-case lane, other 23 retirement lanes, broad local verifier,
Linux execution, and grouped retirement gate remain deferred.

This removes the managed harness as the sole owner of the typed byte/word
positive register matrix without adding loose sources, a large source file, or
a new lane. No assembler, object model, linker, compiler, runtime, WebAssembly,
or product artifact changed.

## Reconsideration triggers

Revise the positive corpus identities if accepted WVA syntax, x86-64 encoding
policy, WVO serialization, report contracts, or the managed vector semantics
change. Preserve the original 200 mutation rows exactly unless their own named
contract changes. Do not generate positive sources during permanent execution.
