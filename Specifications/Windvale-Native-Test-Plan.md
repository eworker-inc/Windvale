# Windvale native test plan

## Status and scope

`WVNT 5` is the fixed repository test inventory executed entirely through
the pinned Windvale-native source-to-WVB, WVB-runner, WVB-verifier, and
WVO-verifier front doors. It is an implemented candidate pending exact-commit
Windows/Linux qualification.

This contract transfers one bounded orchestration slice. It does not replace the
complete Seed, OS, golden, differential, bootstrap, or packaging suites. Its
malformed-WVB and WVO inventories are fixed and representative rather than
exhaustive. It is not a general test-description language.

## Exact inventory

[`Tests/Native/Plan.txt`](../Tests/Native/Plan.txt) is exactly 4,742 UTF-8/LF bytes
with SHA-256
`6ad262319aad1b9df3c9e211fd1e01ed509d8e00beff0de8004642e2928457de`.
Its complete version-5 inventory is:

| Test | Input kind and path | Expected input SHA-256 | Expected outcome |
| --- | --- | --- | --- |
| `calls-control` | project `Projects/Tests/Windvale-Native-Test-Calls-Control.wvproj` | `04282e1d570bb68a24d9f7e531882ca192c52f8af6840f96d5380e7f9a6354e6` | `result 42` |
| `scalar-core` | project `Projects/Tests/Windvale-Native-Test-Scalar-Core.wvproj` | `e790d2162d1223f68bc374ee3c27720e1c660a4e8a2be906aade28cabc5f7713` | `result 42` |
| `function-only` | project `Projects/Tests/Windvale-Native-Test-Function-Only.wvproj` | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` | `result 6` |
| `data-text` | project `Projects/Tests/Windvale-Native-Test-Data-And-Text.wvproj` | `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc` | `result 13` |
| `nominal-types` | project `Projects/Tests/Windvale-Native-Test-Nominal-Types.wvproj` | `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b` | `result 11` |
| `invalid-utf8` | project `Projects/Tests/Windvale-Native-Test-Invalid-Utf8.wvproj` | `6ed7d7dfe3e4443c2943c56c0a8afe6c61c5ffae275e808f1be08fac9266317c` | `failure 3014:11` |
| `range-failure` | project `Projects/Tests/Windvale-Native-Test-Range-Failure.wvproj` | `e57e6183e121d3215beb854c8edb6aebd118480d0991917790fe7c569f8af794` | `failure 3008:14` |
| `u16-failure` | project `Projects/Tests/Windvale-Native-Test-U16-Failure.wvproj` | `971e2296743b5dd1f3838a68c1401c18a869124f4b47c96b85ea3acbbb2e672e` | `failure 3016:4` |
| `malformed-bad-magic` | base64 fixture `Tests/Native/Malformed-Wvb/Bad-Magic.wvb.b64` | `20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e` | `verify-failure semantic` |
| `malformed-bad-version` | base64 fixture `Tests/Native/Malformed-Wvb/Bad-Version.wvb.b64` | `4f0cc323d4eb6713405a3e92f7c885b358aa1efc5fa08e2ecca7be7e17287614` | `verify-failure semantic` |
| `malformed-bad-utf8` | base64 fixture `Tests/Native/Malformed-Wvb/Bad-Utf8.wvb.b64` | `d7e26806542fc5a924193f7d42a229b8e86aaab78c7ba9f845f0b51dcc655c55` | `verify-failure semantic` |
| `malformed-truncated` | base64 fixture `Tests/Native/Malformed-Wvb/Truncated.wvb.b64` | `c795b6a811dac439e60775dfc1c48b23b6a594e203639bc660f974d41dc4073f` | `verify-failure semantic` |
| `malformed-trailing` | base64 fixture `Tests/Native/Malformed-Wvb/Trailing.wvb.b64` | `7121dfd48f36433738e81c9328e54b86ac3e26aa5a61c89f5bef6469df0481c1` | `verify-failure semantic` |
| `malformed-typed-operator-stack-kind` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Operator-Stack-Kind.wvb.b64` | `c6a5431f2f79165294b23409212d5c30cc6dc191051f248561af7fb2c919fcbb` | `verify-failure typed-execution` |
| `malformed-typed-local-store-kind` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Local-Store-Kind.wvb.b64` | `bd5b097c685065a16adafad9c1e84bbfe010016648251964dfd9edc0d4a482df` | `verify-failure typed-execution` |
| `malformed-typed-call-argument-identity` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Call-Argument-Identity.wvb.b64` | `790846e2f2cba9df0c96a4c513cf6771935c18ec11ad6420dfdf71277d7b9a26` | `verify-failure typed-execution` |
| `malformed-typed-record-receiver-identity` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Record-Receiver-Identity.wvb.b64` | `699c2e735ca84621aa7170dd6befacdfb7f38480a0ba16b6968dea3024f68440` | `verify-failure typed-execution` |
| `malformed-typed-enum-operand-identity` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Enum-Operand-Identity.wvb.b64` | `1d5f4418769be1aaebbf791dc683f7cbdb1773d3f6d51ceb10e80f38149b5c09` | `verify-failure typed-execution` |
| `malformed-typed-branch-condition-kind` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Branch-Condition-Kind.wvb.b64` | `2110116c3d542df9b716977dcd877e39e1192ea036664f2ae5defa8b9de13e40` | `verify-failure typed-execution` |
| `malformed-typed-declared-maximum-stack` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Declared-Maximum-Stack.wvb.b64` | `e4ed0f9aa8ee47de4fb22e89227d9367f1179a93581a88e269b51c607953d673` | `verify-failure typed-execution` |
| `malformed-typed-capability-argument-kind` | base64 fixture `Tests/Native/Malformed-Wvb/Typed-Capability-Argument-Kind.wvb.b64` | `e0204e16f5d64e559f15ab0cbb21b578f12177c98464d778085d7ac7b5d78acc` | `verify-failure typed-execution` |
| `malformed-control-unreachable-instruction` | base64 fixture `Tests/Native/Malformed-Wvb/Control-Unreachable-Instruction.wvb.b64` | `4a76e7dbd5057efbf26b47c7edfb928eebc611da9857081bb8c03ed1b5f6c20c` | `verify-failure control-reachability` |
| `wvo-return-42` | base64 fixture `Tests/Native/Wvo/Return-42.wvo.b64` | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` | accepted report SHA-256 `0aad5d459a627ab68aaecb7f927a913178628db4e6d0ba27be8cbb7c55d27c3d` |
| `wvo-bad-magic` | base64 fixture `Tests/Native/Wvo/Bad-Magic.wvo.b64` | `0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288` | rejected report SHA-256 `2e53f573d1e94159c58368c4d9ebcba284d6c13f63a286bd75264bc837a162e4` |
| `wvo-truncated` | base64 fixture `Tests/Native/Wvo/Truncated.wvo.b64` | `6f120ce6b833f781ab014844af535b25fe28eb2d565afa2b2f4360c7a0c99371` | rejected report SHA-256 `9b45f12022ab0ba549e6c2ffa49cb15673d96c8f58efd5d6d9c2def87097aedb` |
| `wvo-trailing` | base64 fixture `Tests/Native/Wvo/Trailing.wvo.b64` | `3ca5e84240e8f12be84fdb957df37f8162e74415417cd7009f92698e683ee981` | rejected report SHA-256 `3cdcb2fa62f4fc698e9624e68dc10dbf95e7363cf0332b280066083cc1783711` |

The first line is exactly `windvale-native-tests 5`. Each remaining line contains
the exact ASCII test name, closed input kind, repository-root-relative input path,
lowercase decoded/input SHA-256 identity, expectation kind, and expectation
value separated by `|`. `project` selects a Project 1 build; `fixture-base64`
selects exact text decoding. `result` carries one signed decimal value. `failure`
carries the unsigned status code and executed guest-instruction count separated by
`:`, while `verify-failure` carries one exact verifier phase. `wvo-fixture-base64`
selects exact WVO text decoding. `wvo-valid` and `wvo-invalid` carry the SHA-256
of the verifier's complete success or diagnostic report so the launchers do not
need host text decoding or a live C# result generator.

Version 5 is a digest-bound fixed value rather than an extensible parser surface.
The launchers reject every insertion, deletion, reordering, path change, field
change, line-ending change, truncation, or appended byte before consuming a field.
A later dynamic plan format requires its own bounded parser, limits, malformed-input
coverage, and version decision.

The version-1, 1,074-byte version-2, 1,983-byte version-3, and 3,906-byte
version-4 predecessors added by Decisions 0226, 0227, 0229, and 0230 were local
unqualified candidates. Version 5 supersedes them before the grouped gate; no
distributed consumer or compatibility promise depends on them.

## Execution contract

The Windows and Linux launchers perform the same ordered steps for each entry:

1. verify the exact plan digest;
2. for `project`, build through the pinned native build driver, verifier, and
   publisher; for `fixture-base64` or `wvo-fixture-base64`, decode the fixed
   repository text into a caller-private WVB or WVO;
3. require the complete decoded/input WVB or WVO SHA-256 identity recorded above;
4. send `result` and `failure` inputs to the pinned native runner, or send
   `verify-failure` inputs to the pinned native WVB verifier, or send WVO inputs
   to the pinned native WVO verifier;
5. for `result`, require process result `0`, empty standard error, and exact
   standard output `Result: <value>` plus LF; for `failure`, require process
   result `1`, empty standard output, and exact standard error
   `wvb run status=Failed code=<code> instructions=<count>` plus LF; for
   `verify-failure`, require process result `1`, empty standard output, and exact
   standard error `wvb status=Invalid phase=<phase>` plus LF; for `wvo-valid`,
   require process result `0`, empty standard error, and the exact complete
   standard-output report digest; for `wvo-invalid`, require process result `2`,
   empty standard output, and the exact complete standard-error report digest;
   and
6. print one `PASS  <name>` line.

Success prints `Tests: 26, Passed: 26, Failed: 0` plus LF and returns `0`. The first
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
rejection through the qualified native semantic verifier. Eight additional fixed
corruptions cover operator stack kind, local storage, call and nominal identities,
branch condition kind, declared maximum stack, and capability arguments through
the native typed-execution phase. One fixed changed jump target proves unreachable
code rejection through the native control-reachability phase. One canonical WVO
and three fixed WVO corruptions independently prove complete-object acceptance,
bad-magic rejection, truncation rejection, and trailing-byte rejection through
the native object verifier. Randomized malformed data, remaining semantic and
structural limits, the broader unsafe-bytecode and WVO corpora, and tests outside
this subset remain in the explicit Stage 0 lane. Qualification of this plan
advances Decision 0057's native test condition but does not complete it.
