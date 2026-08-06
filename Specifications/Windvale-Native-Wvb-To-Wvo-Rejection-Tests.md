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
| Valid unsupported WVB, `Decimal-Parsing.wvb` | 1,698 | `bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37` |
| Destination sentinel, decoded `Return-42.wvo.b64` | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |

The malformed fixture fails WVB 1.11 admission. The decimal-parsing fixture is
a valid module whose function shape is outside the lowerer's current accepted
subset. It is not recompiled during the test.

## Rejection contract

The ordered cases are:

| Case | Complete diagnostic | Report SHA-256 |
| --- | --- | --- |
| `malformed` | `native x64 status=Invalidˉwvb` plus LF | `6dc739ce9e8c752efe41fbede32d6c373ea33e1c22159faf86772a4cc94ff323` |
| `unsupported-function` | `native x64 status=Unsupportedˉfunction` plus LF | `fc854d5370fe6da10243d8e28663f932baa4d7c30402488f5193d0a3dad77ded` |

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
