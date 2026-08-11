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
| `Console-Application-Verifier.wvb` | 105,006 | `1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c` |
| `Console-Application-Verifier.wvo` | 1,049,519 | `51292e4d300d4a6bb6ce4879915bba5304de70c9deafdf4eb6ff6a54a6dbf150` |
| `windows-x64-wvappverify.exe` | 1,063,936 | `a82027ab78ee5f4d7d9f34180392ee8b8364ea78616c11aeac1e684250fc3679` |
| `linux-x64-wvappverify.elf` | 1,064,960 | `c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a` |

The WVB rebuilds through the native Project 1 front door after canonicalizing
its order-independent source inventory. [Decision 0502](../Documents/Decisions/0502-Native-Console-Application-Verifier-Reconstruction.md)
records the current-Windows-host native cross-target reconstruction: the
retained raw lowerer produces the exact WVO oracle, the retained native linker
produces the 1,045,627-byte fragment with `Main` at offset 19,221, and the
profile-7 hosted-container path constructs both exact applications. The hosted
profile has exactly two immutable input snapshots and no file-write capability.
Existing verifier, inspector, and runner profiles retain one input snapshot and
their existing layouts. The construction route consumes retained same-release
seeds and does not claim independent Linux execution, clean bootstrap,
qualification, promotion, or recovery deletion.

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
objects, promote either candidate, prove a clean or previous-seed bootstrap,
release Stage 0 recovery, or complete the Decision 0057 retirement gate.
