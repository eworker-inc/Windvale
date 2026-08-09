# Decision 0434: Expanded native WVA positive matrix

- Status: Implemented current-host focused evidence; complete lane and Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0433](0433-Fixed-Native-Wva-Positive-Matrix.md), [Decision 0336](0336-Fixed-Native-Wva-Differential-Corpus.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native WVA differential tests](../../Specifications/Windvale-Native-Wva-Differential-Tests.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0433 transferred the managed typed byte/word positive matrix. The
expanded-x64 assertion still exclusively owned 52 distinct accepted sources:
the combined expanded register operations, every paired 32/64-bit register,
local and definition-scoped labels, all sixteen branch conditions, all sixteen
condition materializations, and the complete RIP-relative load/store/address
and link shape.

These cases belong in the same compact positive corpus. Adding loose fixtures,
one generated mega-source, or another suite would obscure their shared oracle
and permanent native execution contract.

## Decision

Advance the positive corpus from version 1 to version 2 while retaining all 17
Decision 0433 rows. Add the 52 expanded-x64 sources and their exact Stage 0 WVO,
assembler-report, and verifier-report identities. The resulting 69
LF-terminated sources total 13,274 bytes and produce 6,238 exact WVO bytes.

The version-2 manifest is 20,206 bytes at SHA-256
`fdf5c5e63cf323fee11a4ac08e0786e5167acbfa8a63e1fc245659936026fde2`.
The deterministic 12,906-byte archive has SHA-256
`c17bb829636608f8d38b983d5d5979f64c24bfc4b9b3a4d753fdf1620425aaab`;
its 17,209-byte LF-only base64 representation has SHA-256
`595c405e54c4eda6ebe0bc14e4174ffae2ba34c5621aa4929639ef336fc426ff`.

Keep `--positive-only` as the narrow inner loop, now selecting all 69 positive
rows. The unfiltered WVA differential lane grows from 217 to 269 cases: 200
frozen mutations plus 69 exact positives. The 2,054-byte retirement plan
remains 24 suites and grows from 3,066 to 3,118 cases at SHA-256
`b561c70dfff8ddf495849ebbdd1bed2a1067033d00bea0245b8e82556ab4ceeb`.

## Evidence and consequences

After one temporary exporter-name compile correction, the focused managed
expanded-x64 assertion passes 1/1 in 586 ms. The exporter was removed and
`Program.cs` returned byte for byte to its committed state. The generation
directory was removed after the permanent archive identity and every archived
source were checked.

The reviewed Windows command
`Test-Wva-Differential.cmd --positive-only` passes all 69 cases in 25.7 seconds
without starting .NET. Every source remains unchanged, every object matches its
Stage 0 identity, and every object passes native verification with its exact
digest-bearing report. The unchanged 200-case mutation corpus, complete
269-case lane, other 23 retirement lanes, broad local verifier, Linux execution,
and grouped retirement gate were not run.

This removes the managed harness as the sole owner of the expanded-x64 positive
register, condition, label, and RIP-relative matrices without adding a new lane
or large source file. No assembler, object model, linker, compiler, runtime,
WebAssembly, or product artifact changed.

## Reconsideration triggers

Advance the corpus version if accepted WVA syntax, x86-64 encoding policy, WVO
serialization, report contracts, or any retained managed vector changes.
Preserve the original 200 mutation rows and the version-1 positive rows unless
their own named contracts change.
