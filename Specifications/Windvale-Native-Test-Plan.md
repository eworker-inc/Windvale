# Windvale native test plan

## Status and scope

`WVNT 2` is the fixed repository test inventory executed entirely through
the pinned Windvale-native source-to-WVB and WVB-runner front doors. It is an
implemented candidate pending exact-commit Windows/Linux qualification.

This contract transfers one bounded orchestration slice. It does not replace the
complete Seed, OS, golden, malformed-input, differential, bootstrap, or packaging
suites, and it is not a general test-description language.

## Exact inventory

[`Tests/Native/Plan.txt`](../Tests/Native/Plan.txt) is exactly 1,074 UTF-8/LF bytes
with SHA-256
`619b22496a6999b80ed25f601066c1fa07162dad52c6b6c79b9d836a1d46df62`.
Its complete version-2 inventory is:

| Test | Project | Expected WVB SHA-256 | Expected outcome |
| --- | --- | --- | --- |
| `calls-control` | `Windvale-Native-Test-Calls-Control.wvproj` | `04282e1d570bb68a24d9f7e531882ca192c52f8af6840f96d5380e7f9a6354e6` | `result 42` |
| `scalar-core` | `Windvale-Native-Test-Scalar-Core.wvproj` | `e790d2162d1223f68bc374ee3c27720e1c660a4e8a2be906aade28cabc5f7713` | `result 42` |
| `function-only` | `Windvale-Native-Test-Function-Only.wvproj` | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` | `result 6` |
| `data-text` | `Windvale-Native-Test-Data-And-Text.wvproj` | `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc` | `result 13` |
| `nominal-types` | `Windvale-Native-Test-Nominal-Types.wvproj` | `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b` | `result 11` |
| `invalid-utf8` | `Windvale-Native-Test-Invalid-Utf8.wvproj` | `6ed7d7dfe3e4443c2943c56c0a8afe6c61c5ffae275e808f1be08fac9266317c` | `failure 3014:11` |
| `range-failure` | `Windvale-Native-Test-Range-Failure.wvproj` | `e57e6183e121d3215beb854c8edb6aebd118480d0991917790fe7c569f8af794` | `failure 3008:14` |
| `u16-failure` | `Windvale-Native-Test-U16-Failure.wvproj` | `971e2296743b5dd1f3838a68c1401c18a869124f4b47c96b85ea3acbbb2e672e` | `failure 3016:4` |

The first line is exactly `windvale-native-tests 2`. Each remaining line contains
the exact ASCII test name, repository-root-relative Project 1 path, lowercase
SHA-256 identity, expectation kind, and expectation value separated by `|`.
`result` carries one signed decimal value. `failure` carries the unsigned status
code and executed guest-instruction count separated by `:`.

Version 2 is a digest-bound fixed value rather than an extensible parser surface.
The launchers reject every insertion, deletion, reordering, path change, field
change, line-ending change, truncation, or appended byte before consuming a field.
A later dynamic plan format requires its own bounded parser, limits, malformed-input
coverage, and version decision.

The 635-byte version-1 predecessor added by Decision 0226 was a local unqualified
candidate. Version 2 supersedes it before the grouped gate; no distributed consumer
or compatibility promise depends on version 1.

## Execution contract

The Windows and Linux launchers perform the same ordered steps for each entry:

1. verify the exact plan digest;
2. build the project through the pinned native build driver, verifier, and publisher;
3. require the complete WVB SHA-256 identity recorded above;
4. execute that WVB through the pinned native runner;
5. for `result`, require process result `0`, empty standard error, and exact
   standard output `Result: <value>` plus LF; for `failure`, require process
   result `1`, empty standard output, and exact standard error
   `wvb run status=Failed code=<code> instructions=<count>` plus LF; and
6. print one `PASS  <name>` line.

Success prints `Tests: 8, Passed: 8, Failed: 0` plus LF and returns `0`. The first
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
recovery. The three failure cases prove exact native status and guest-instruction
boundaries for invalid UTF-8, an out-of-range byte slice, and a truncated
little-endian `u16` read. Tests outside that subset remain in the explicit Stage 0
lane until the relevant native runtime, tool, fixture, and oracle are available.
Qualification of this plan advances Decision 0057's native test condition but does
not complete it.
