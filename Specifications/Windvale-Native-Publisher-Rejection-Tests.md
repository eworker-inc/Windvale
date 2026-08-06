# Windvale native publisher rejection tests

## Status and scope

This fixed candidate contract exercises pre-replacement rejection through the
digest-bound console-application and WVO publisher launchers. It transfers one
permanent no-.NET admission and cleanup slice; it does not duplicate the shared
publication transaction's hard-link, concurrency, injected-fault, replacement,
directory-durability, or indeterminate-completion matrix.

## Exact inputs

`Tools/Native/Test-Publisher-Rejections.cmd` and `.sh` decode two existing
repository fixtures:

| Role | Decoded bytes | SHA-256 |
| --- | ---: | --- |
| Invalid candidate, `Bad-Magic.wvo.b64` | 479 | `0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288` |
| Destination sentinel, `Return-42.wvo.b64` | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |

The same bytes receive `.exe`/`.elf` names for console-application admission and
`.wvo` names for WVO admission. Extensions select the public launcher contract;
they do not reinterpret or mutate the fixture bytes.

## Rejection contract

The ordered cases are:

| Case | Launcher | Complete diagnostic | Report SHA-256 |
| --- | --- | --- | --- |
| `console-application` | `Publish-Console.cmd` / `.sh` | `publication status=Rejected phase=console-application` plus LF | `39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f` |
| `wvo` | `Publish-Wvo.cmd` / `.sh` | `publication status=Rejected phase=wvo` plus LF | `e7a127a800310d9fbaf8b511b20c7b8184159521dec1be56b641793939a5c69f` |

For each case the coordinator must:

1. verify the current-host publisher digest through its public launcher;
2. require process result `1` and empty standard output;
3. require the complete diagnostic SHA-256 above;
4. require the destination sentinel's complete identity to remain unchanged;
5. require no `.wvpublish-*` scratch file to remain; and
6. remove only its named candidate, destination, report, and fixture copies.

Success prints:

```text
PASS  console-application
PASS  wvo
Tests: 2, Passed: 2, Failed: 0
```

The command invokes no .NET process and does not rebuild publisher artifacts.
Successful replacement remains covered by the existing AOT/package/lowerer
composition and managed independent-evidence tests until the grouped gate.
