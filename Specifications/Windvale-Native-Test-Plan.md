# Windvale native test plan

## Status and scope

`WVNT 3` is the fixed repository test inventory executed entirely through
the pinned Windvale-native source-to-WVB, WVB-runner, and WVB-verifier front
doors. It is an implemented candidate pending exact-commit Windows/Linux
qualification.

This contract transfers one bounded orchestration slice. It does not replace the
complete Seed, OS, golden, differential, bootstrap, or packaging suites, and it
transfers only a fixed malformed-WVB envelope subset. It is not a general
test-description language.

## Exact inventory

[`Tests/Native/Plan.txt`](../Tests/Native/Plan.txt) is exactly 1,983 UTF-8/LF bytes
with SHA-256
`1b5dc525a2a5fc8883e21cbd0502bb2c3af1cb93c32fec11f5379e9f624fd870`.
Its complete version-3 inventory is:

| Test | Input kind and path | Expected WVB SHA-256 | Expected outcome |
| --- | --- | --- | --- |
| `calls-control` | project `Windvale-Native-Test-Calls-Control.wvproj` | `04282e1d570bb68a24d9f7e531882ca192c52f8af6840f96d5380e7f9a6354e6` | `result 42` |
| `scalar-core` | project `Windvale-Native-Test-Scalar-Core.wvproj` | `e790d2162d1223f68bc374ee3c27720e1c660a4e8a2be906aade28cabc5f7713` | `result 42` |
| `function-only` | project `Windvale-Native-Test-Function-Only.wvproj` | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` | `result 6` |
| `data-text` | project `Windvale-Native-Test-Data-And-Text.wvproj` | `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc` | `result 13` |
| `nominal-types` | project `Windvale-Native-Test-Nominal-Types.wvproj` | `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b` | `result 11` |
| `invalid-utf8` | project `Windvale-Native-Test-Invalid-Utf8.wvproj` | `6ed7d7dfe3e4443c2943c56c0a8afe6c61c5ffae275e808f1be08fac9266317c` | `failure 3014:11` |
| `range-failure` | project `Windvale-Native-Test-Range-Failure.wvproj` | `e57e6183e121d3215beb854c8edb6aebd118480d0991917790fe7c569f8af794` | `failure 3008:14` |
| `u16-failure` | project `Windvale-Native-Test-U16-Failure.wvproj` | `971e2296743b5dd1f3838a68c1401c18a869124f4b47c96b85ea3acbbb2e672e` | `failure 3016:4` |
| `malformed-bad-magic` | base64 fixture `Tests/Native/Malformed-Wvb/Bad-Magic.wvb.b64` | `20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e` | `verify-failure semantic` |
| `malformed-bad-version` | base64 fixture `Tests/Native/Malformed-Wvb/Bad-Version.wvb.b64` | `4f0cc323d4eb6713405a3e92f7c885b358aa1efc5fa08e2ecca7be7e17287614` | `verify-failure semantic` |
| `malformed-bad-utf8` | base64 fixture `Tests/Native/Malformed-Wvb/Bad-Utf8.wvb.b64` | `d7e26806542fc5a924193f7d42a229b8e86aaab78c7ba9f845f0b51dcc655c55` | `verify-failure semantic` |
| `malformed-truncated` | base64 fixture `Tests/Native/Malformed-Wvb/Truncated.wvb.b64` | `c795b6a811dac439e60775dfc1c48b23b6a594e203639bc660f974d41dc4073f` | `verify-failure semantic` |
| `malformed-trailing` | base64 fixture `Tests/Native/Malformed-Wvb/Trailing.wvb.b64` | `7121dfd48f36433738e81c9328e54b86ac3e26aa5a61c89f5bef6469df0481c1` | `verify-failure semantic` |

The first line is exactly `windvale-native-tests 3`. Each remaining line contains
the exact ASCII test name, closed input kind, repository-root-relative input path,
lowercase decoded/input WVB SHA-256 identity, expectation kind, and expectation
value separated by `|`. `project` selects a Project 1 build; `fixture-base64`
selects exact text decoding. `result` carries one signed decimal value. `failure`
carries the unsigned status code and executed guest-instruction count separated by
`:`, while `verify-failure` carries one exact verifier phase.

Version 2 is a digest-bound fixed value rather than an extensible parser surface.
The launchers reject every insertion, deletion, reordering, path change, field
change, line-ending change, truncation, or appended byte before consuming a field.
A later dynamic plan format requires its own bounded parser, limits, malformed-input
coverage, and version decision.

The version-1 and 1,074-byte version-2 predecessors added by Decisions 0226 and
0227 were local unqualified candidates. Version 3 supersedes both before the
grouped gate; no distributed consumer or compatibility promise depends on them.

## Execution contract

The Windows and Linux launchers perform the same ordered steps for each entry:

1. verify the exact plan digest;
2. for `project`, build through the pinned native build driver, verifier, and
   publisher; for `fixture-base64`, decode the fixed repository text into a
   caller-private WVB;
3. require the complete decoded/input WVB SHA-256 identity recorded above;
4. send `result` and `failure` inputs to the pinned native runner, or send
   `verify-failure` inputs to the pinned native WVB verifier;
5. for `result`, require process result `0`, empty standard error, and exact
   standard output `Result: <value>` plus LF; for `failure`, require process
   result `1`, empty standard output, and exact standard error
   `wvb run status=Failed code=<code> instructions=<count>` plus LF; for
   `verify-failure`, require process result `1`, empty standard output, and exact
   standard error `wvb status=Invalid phase=<phase>` plus LF; and
6. print one `PASS  <name>` line.

Success prints `Tests: 13, Passed: 13, Failed: 0` plus LF and returns `0`. The first
failure prints a stable `FAIL` reason and nonzero summary to standard error, returns
`1`, and does not execute later entries. Temporary outputs are caller-private and
removed on completion.

The launchers invoke no .NET command. Windows currently depends on inbox `cmd.exe`
and `certutil`; Linux depends on Bash, `sha256sum`, `base64`, `cmp`, and ordinary
core utilities. These host adapters do not define Windvale language or WVB
semantics.

## Retirement boundary

This plan proves native deterministic compilation and interpreted execution for
the runner's accepted portable subset. The first two cases cover calls, loops,
branches, signed and unsigned arithmetic, comparisons, booleans, and `u8`. The
additional cases cover typed constants, a six-function graph, static scalar/text/
byte data, immutable byte operations, records, enums, nominal calls, and enum-name
recovery. The three failure cases prove exact native status and guest-instruction
boundaries for invalid UTF-8, an out-of-range byte slice, and a truncated
little-endian `u16` read. The five malformed fixtures independently fix bad magic,
bad format version, invalid module-name UTF-8, truncation, and trailing-data
rejection through the qualified native semantic verifier. Typed-execution,
control-reachability, randomized malformed data, unsafe bytecode, and tests outside
that subset remain in the explicit Stage 0 lane until the relevant native tool,
fixture, and oracle are available. Qualification of this plan advances Decision
0057's native test condition but does not complete it.
