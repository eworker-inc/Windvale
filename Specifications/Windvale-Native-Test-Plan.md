# Windvale native test plan

## Status and scope

`WVNT 1` is the fixed repository test inventory executed entirely through
the pinned Windvale-native source-to-WVB and WVB-runner front doors. It is an
implemented candidate pending exact-commit Windows/Linux qualification.

This contract transfers one bounded orchestration slice. It does not replace the
complete Seed, OS, golden, malformed-input, differential, bootstrap, or packaging
suites, and it is not a general test-description language.

## Exact inventory

[`Tests/Native/Plan.txt`](../Tests/Native/Plan.txt) is exactly 635 UTF-8/LF bytes
with SHA-256
`79294d8e1a08325cd41042e6068b4a6bc9f3c15bd05372ad4bb7eda268a47b73`.
Its complete version-1 inventory is:

| Test | Project | Expected WVB SHA-256 | Expected result |
| --- | --- | --- | ---: |
| `calls-control` | `Windvale-Native-Test-Calls-Control.wvproj` | `04282e1d570bb68a24d9f7e531882ca192c52f8af6840f96d5380e7f9a6354e6` | 42 |
| `scalar-core` | `Windvale-Native-Test-Scalar-Core.wvproj` | `e790d2162d1223f68bc374ee3c27720e1c660a4e8a2be906aade28cabc5f7713` | 42 |
| `function-only` | `Windvale-Native-Test-Function-Only.wvproj` | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` | 6 |
| `data-text` | `Windvale-Native-Test-Data-And-Text.wvproj` | `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc` | 13 |
| `nominal-types` | `Windvale-Native-Test-Nominal-Types.wvproj` | `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b` | 11 |

The first line is exactly `windvale-native-tests 1`. Each remaining line contains
the exact ASCII test name, repository-root-relative Project 1 path, lowercase
SHA-256 identity, and signed decimal expected result separated by `|`.

Version 1 is a digest-bound fixed value rather than an extensible parser surface.
The launchers reject every insertion, deletion, reordering, path change, field
change, line-ending change, truncation, or appended byte before consuming a field.
A later dynamic plan format requires its own bounded parser, limits, malformed-input
coverage, and version decision.

## Execution contract

The Windows and Linux launchers perform the same ordered steps for each entry:

1. verify the exact plan digest;
2. build the project through the pinned native build driver, verifier, and publisher;
3. require the complete WVB SHA-256 identity recorded above;
4. execute that WVB through the pinned native runner;
5. require process result `0`, empty standard error, and exact standard output
   `Result: <expected>` plus LF; and
6. print one `PASS  <name>` line.

Success prints `Tests: 5, Passed: 5, Failed: 0` plus LF and returns `0`. The first
failure prints a stable `FAIL` reason and nonzero summary to standard error, returns
`1`, and does not execute later entries. Temporary outputs are caller-private and
removed on completion.

The launchers invoke no .NET command. Windows currently depends on inbox `cmd.exe`
and `certutil`; Linux depends on Bash, `sha256sum`, `cmp`, and ordinary core
utilities. These host adapters do not define Windvale language or WVB semantics.

## Retirement boundary

This plan proves native deterministic compilation and interpreted execution for
the runner's accepted portable subset. The first two cases cover calls, loops,
branches, signed and unsigned arithmetic, comparisons, booleans, and `u8`. The
additional cases cover typed constants, a six-function graph, static scalar/text/
byte data, immutable byte operations, records, enums, nominal calls, and enum-name
recovery. Tests outside that subset remain in the explicit Stage 0 lane until the
relevant native runtime, tool, fixture, and oracle are available. Qualification of
this plan advances Decision 0057's native test condition but does not complete it.
