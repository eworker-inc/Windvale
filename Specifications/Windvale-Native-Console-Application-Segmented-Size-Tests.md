# Windvale native console-application segmented-size tests

## Scope

This contract transfers the two version-1 console-application maximum-size
rejections from normal managed testing to one fixed, read-only Windvale-native
command on Windows and Linux. It covers the real two-chunk acquisition boundary;
it does not weaken either case to a smaller malformed application.

The managed target-specific verifiers remain recovery evidence. The permanent
command does not load .NET or invoke a live managed oracle.

## Verifier application

`Windvale-Console-Application-Verifier.wvproj` builds module
`Windvaleˉconsoleˉapplicationˉverifierˉtool`. `Main` requires exactly two paths,
reads both inputs once through `file.read_bytes`, and passes them to the shared
portable console-application admission function.

The candidate manifest pins:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Console-Application-Verifier.wvb` | 103,424 | `5894bb7180597945f4e4d49e87ae954fb3c2bba84cde4b9cb549a2f168006a91` |
| `windows-x64-wvappverify.exe` | 1,041,408 | `ebc6f54884e3d93ee1fb1f3658a9062167294f3d0e936554cadc499b83bd8111` |
| `linux-x64-wvappverify.elf` | 1,040,384 | `5dbd78b3f67cc179e9848eacca6627a03f5f44ddecc6480d2e9ab98d073f792e` |

The hosted profile has exactly two immutable input snapshots and no file-write
capability. Existing verifier, inspector, and runner profiles retain one input
snapshot and their existing layouts. Construction is recorded as
`stage0-recovery`; normal use of the pinned applications is .NET-free.

Wrong arity returns `64` with usage on standard error. A rejected application
returns `1`, writes no standard output, and writes exactly:

```text
console application status=Rejected code=<status> offset=<offset> target=<target> bytes=<application-bytes>
```

The report terminates in one LF byte on both host applications.

## Fixed corpus

`Tests/Native/Console-Application-Segmented-Size-Boundaries/Corpus.tar.gz.b64`
decodes to an 8,909-byte gzip archive with SHA-256
`d0e9aa4f6e31d3bd28fb0468606f43b275c320adb470e4d3b78034d440573200`.
Its 569-byte LF-only manifest has SHA-256
`50c1c87ac9dcaaccbd5036c2d67677dde044a6b24f11fe78149784741c72ca29`.
The archive contains only that manifest and these four files:

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| `Windows-First.bin` | 4,194,304 | `f89f9f52fa123d6aef2f6233ce328f2067556d94479441b8480232b19f02b33d` |
| `Windows-Second.bin` | 2,049 | `5373c2d1dc4c5333681ef9fccfe13fcb842c4779960359570e994a864145c2d2` |
| `Linux-First.bin` | 4,194,304 | `1284d5b90b95c7e0c7786f9dceb0c3ba56e79000d2f8ce313a55a2e913aea103` |
| `Linux-Second.bin` | 8,305 | `36ed1705d3e80aa3a43adee79841efcc0e3c552986e77153528a352210d4307e` |

The first Windows chunk begins `MZ`; the first Linux chunk begins ELF magic.
All remaining bytes are zero. Those target markers let the one portable
verifier identify a platform. The old target-specific zero-array checks are
therefore recovery provenance, not a claim that the permanent inputs are
byte-identical to the managed inputs.

## Exact cases

The runner owns exactly two cases:

| Case | Managed provenance | Native result | LF report SHA-256 |
| --- | --- | --- | --- |
| `windows-max-plus-one` | `WVW2001` | code `2`, offset `4196353`, target `1`, bytes `4196353` | `d0b1304c62778d71c7df11b2c9d3759139810b0acca3115e77bb44aae1b052ba` |
| `linux-max-plus-one` | `WVL2001` | code `1`, offset `4194304`, target `0`, bytes `4202609` | `9b8b2d84bdb475db94d5a0e1be47a73f12d9663e966c2c8708ce4f556aacb1d2` |

For Windows, the two accepted chunk lengths form one application byte beyond
the 4,196,352-byte target limit, so target selection precedes `Invalidˉsize`.
For Linux, the 8,305-byte second chunk exceeds the shared 8,304-byte chunk
limit, so `Invalidˉchunk` occurs before target inspection. That ordering is the
portable two-chunk contract and intentionally differs from the target-specific
managed `WVL2001` surface.

For each case the runner verifies both input sizes and hashes before execution,
requires exit `1`, empty standard output, and the exact rejection report, then
rechecks both hashes. The command creates no destination or publication scratch.
A complete success ends with:

```text
Tests: 2, Passed: 2, Failed: 0
```

## Boundary

This contract does not qualify Linux execution, validate a maximum-size valid
PE or ELF, widen the ordinary byte-value limit, transfer large-native segmented
objects, promote either candidate, replace Stage 0 construction, or complete
the Decision 0057 retirement gate.
