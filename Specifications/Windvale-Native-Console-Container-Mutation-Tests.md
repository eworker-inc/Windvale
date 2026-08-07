# Windvale native console-container mutation tests

## Status and scope

This fixed contract transfers the ordinary version-1 PE and ELF malformed-
container cases from live Stage 0 orchestration to the Windvale-native console
publisher. It covers exact truncation, one-byte structural mutation, canonical
padding, startup, execution-context, relocation, and trailing-byte rejection.

The contract is separate from the 256-case arbitrary
[hostile-input corpus](Windvale-Native-Console-Container-Hostile-Input-Tests.md).
It does not replace the segmented larger-than-4-MiB boundary, hosted version-2
container mutations, successful construction/execution, or the final dual-host
qualification gate.

## Canonical bases and oracle provenance

The immutable cases derive from the two canonical `Sumˉdata` version-1
applications:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| `windows-x64-console-v1` | 5,120 | `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77` |
| `linux-x64-console-v1` | 8,304 | `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4` |

The exact operations come from the frozen managed console-application tests.
One reviewed Stage 0 program ran once against the completed corpus and confirmed
all recorded detailed codes. It passed 19/19 and was then moved outside the
repository with its build output. The permanent commands do not generate a
case, invoke .NET, or change an expected value after native execution.

## Corpus identity

`Manifest.txt` starts with
`windvale-console-container-mutation-corpus 1`. Every later LF-terminated row
contains these pipe-separated fields:

```text
filename|target|operation|offset|stage0-code|bytes|sha256
```

The operation is exactly `truncate-last`, `xor-one`, or `append-zero`. The
offset identifies the removed byte boundary, changed byte, or original append
boundary. The corpus contains:

- ten Windows cases: two `WVW2001` size failures and one case for every
  `WVW2002` through `WVW2009` family;
- nine Linux cases: two `WVL2001` size failures and one case for every
  `WVL2002` through `WVL2008` family;
- two truncations, fifteen one-byte mutations, and two trailing zero bytes; and
- 125,936 total input bytes.

The 2,626-byte manifest has SHA-256
`35794ce75d80a06b099f705a8c0fce91295a5d627cee2a76803617f372e13669`.
It and all 19 immutable candidates are stored in one 4,432-byte gzip tar archive
at SHA-256
`63b7d5187aa0f5407aa5a68be851c03fb0b64991c418f8c2407548f0ad6c89c9`.
The repository representation is 5,990 LF-only base64 bytes at SHA-256
`43988d27758031d577d1f27caf20b9f7e8076184334e5f379bebccf1c2f49825`.

Before native execution, independent static review reconstructed every archived
value from its named canonical base and manifest operation and required exact
byte-for-byte agreement.

## Native rejection contract

`Tools/Native/Test-Console-Container-Mutations.cmd` and `.sh` verify the
archive, manifest, target/suffix, operation inventory, detailed Stage 0 code
inventory, length, and SHA-256 of every candidate. The public current-host
`Publish-Console` launcher then routes `.exe` and `.elf` values through the
portable Windvale verifier without executing either container.

Before every call, the command copies the fixed 479-byte WVO sentinel at
SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`
to the same-suffix destination. Every native invocation must:

- return process exit `1`;
- write no standard output;
- emit exactly `publication status=Rejected phase=console-application` plus LF,
  whose SHA-256 is
  `39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f`;
- preserve the complete candidate and destination sentinel; and
- leave no `.wvpublish-*` scratch file.

Success prints the 19 manifest-ordered `PASS` lines followed by:

```text
Tests: 19, Passed: 19, Failed: 0
```

## Segmented boundary

The retained Stage 0 matrix also constructs one zero-filled value at
`MAX_APPLICATION_BYTES + 1` for each target. Those values are 4,196,353 and
4,202,609 bytes. The current public publisher intentionally admits one bounded
4,194,304-byte snapshot, while the portable verifier separately supports a
canonical two-chunk request. The two size cases therefore remain owned by a
future focused segmented-admission lane; replacing them with smaller inputs
would not transfer their contract.
