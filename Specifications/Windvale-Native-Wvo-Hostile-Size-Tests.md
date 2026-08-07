# Windvale native WVO hostile-size tests

## Status and scope

This fixed contract transfers standard WVO input at the first byte beyond the
ordinary object/value limit to the native host-tool boundary. It proves that
every current normal WVO consumer contains the exact 4,194,305-byte value
without reading it into Windvale, changing the input, or mutating an existing
destination.

This is an outer immutable-snapshot test. It does not claim that the portable
WVO scanner received a value larger than Windvale's 4-MiB `bytes` limit, and it
does not replace explicit large-native 32-MiB segmented-object evidence.

## Input and provenance

Standard WVO admission is limited to 4,194,304 bytes. The retained Stage 0
linker test supplies one zero-filled `MAX_OBJECT_BYTES + 1` value and requires
`WVL1002`; direct Stage 0 object reading classifies the same outer size as
`WVO1001` before parsing magic.

`Oversized.wvo` therefore contains exactly 4,194,305 zero bytes at SHA-256
`95e441ca65cd41fa01b2a71799e79fd60db59ed34f13af32a91e85f90378676c`.
It is stored in one 4,178-byte gzip tar archive at SHA-256
`4c9e5ed9aa6a822c64e799378ede641d86c37a6cc639003286afd2277144ef89`.
The repository representation is 5,646 LF-only base64 bytes at SHA-256
`1d4816599e4b968bb2449f2285a953212bab8d8eb1e3ab2f04af5942b5ab68b6`.

Static review verifies the archive contains exactly one member named
`Oversized.wvo`. The permanent command verifies the complete archive digest and
the exact expanded length and digest. It generates nothing and invokes no
managed process.

## Native boundary

The native `file.read_bytes` service admits at most one 4,194,304-byte immutable
snapshot. Its platform leaves use bounded reads and classify a larger file as
service failure `WVR3025`. The current hosted-tool startup normalizes that
uncatchable service failure to process result `1` with empty standard output
and standard error; the Windvale command body is not entered past the failed
read.

`Tools/Native/Test-Wvo-Hostile-Size.cmd` and `.sh` require this exact behavior
through four digest-bound public launchers:

| Case | Launcher | Stage 0 provenance | Additional preservation |
| --- | --- | --- | --- |
| `verify` | `Verify-Wvo` | `WVO1001` | input |
| `inspect` | `Inspect-Wvo` | `WVO1001` | input |
| `link` | `Link-Wvo` | `WVL1002` | input and existing output |
| `publish` | `Publish-Wvo` | `WVO1001` | input, existing destination, and zero scratch |

Before the mutating cases, the command copies the fixed 479-byte WVO sentinel
at SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
Each call must return `1`, leave both output channels empty, and preserve every
applicable file byte-for-byte. Publication must leave no `.wvpublish-*` file.

Success ends with:

```text
Tests: 4, Passed: 4, Failed: 0
```

## Boundary

The empty process channels are intentional and must not be described as a WVO
diagnostic: admission failed earlier at the host byte-value boundary. A future
segmented WVO reader may expose a detailed portable size result, but it must
retain this no-overallocation, no-mutation evidence and requires a separate
accepted contract rather than silently widening `bytes`.
