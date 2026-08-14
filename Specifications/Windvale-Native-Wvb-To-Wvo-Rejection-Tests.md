# Windvale native WVB-to-WVO rejection tests

## Status and scope

This fixed candidate contract exercises malformed-input and accepted-subset
rejection through the digest-bound WVB-to-WVO launcher. It transfers one
permanent no-.NET failure slice without rebuilding the lowerer, linking an
image, packaging an application, or deriving expectations from the managed
backend.

## Exact inputs

`Tools/Native/Test-Lowerer-Rejections.cmd` and `.sh` use these complete fixed
inputs:

| Role | Bytes | SHA-256 |
| --- | ---: | --- |
| Malformed WVB, decoded `Bad-Magic.wvb.b64` | 174 | `20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e` |
| Valid unsupported WVB, decoded `Unsupported-Function.wvb.b64` | 183 | `605a2528ebad0fc418e9cb1ab8738c6e3a9b2e58cb9e0897cb0bc93fececaf91` |
| Destination sentinel, decoded `Return-42.wvo.b64` | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |

The malformed fixture fails WVB 1.11 admission. The unsupported-function
fixture is the exact compiled form of
`Tests/Fixtures/Native-X64/Wvb-To-Wvo-Unsupported-Function.wv`; it is a valid
portable module whose exported `Main(i32) -> i32` shape is outside the
lowerer's accepted entry subset. It is not recompiled during the test.

## Rejection contract

The ordered cases are:

| Case | Complete diagnostic | Report SHA-256 |
| --- | --- | --- |
| `malformed` | `native x64 status=Invalidˉwvb plan-status=Invalidˉwvb function=4294967295 detail=4294967295` plus LF | `cb4866cce34d859dabe8d8823f7ad391daed579cdcd61fd6ecbd4e5c324d78dc` |
| `unsupported-function` | `native x64 status=Unsupportedˉfunction plan-status=Unsupportedˉfunction function=4294967295 detail=4294967295` plus LF | `0e5a4dc04f822ab0afe79fe48d5126ffbdc825a2abe47f7b7a0bfd67b12830e5` |

For each case the coordinator must:

1. invoke only the public digest-bound lowerer launcher;
2. require process result `1` and empty standard output;
3. require the complete diagnostic SHA-256 above;
4. require the destination sentinel's complete identity to remain unchanged;
5. isolate launcher temporary work and require it to be empty afterward; and
6. remove only its named input copies, destination, reports, and empty temporary
   directories.

Success prints:

```text
PASS  malformed
PASS  unsupported-function
Tests: 2, Passed: 2, Failed: 0
```

The command invokes no .NET process. Successful lowering remains covered by the
fixed return-42 front door and AOT composition. Broader valid-but-unsupported
profiles and malformed-WVB categories remain in managed independent evidence
until separately transferred or the final grouped gate qualifies their native
equivalents.
