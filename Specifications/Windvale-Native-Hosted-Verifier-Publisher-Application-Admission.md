# Windvale native hosted-verifier publisher application admission

## Status and scope

This contract gives Windvale source exact read-only ownership of the two
completed hosted-verifier publisher application identities. It deliberately
lives outside the publisher being measured: embedding either final publisher
digest in that publisher would change the subject and create an endless
self-pin cycle.

This slice provides the portable admission module, its hosted command source,
a natively built canonical WVB/WVO pair, and deterministic Windows/Linux
profile-8 applications. It does not provide a durable installation transaction.

## Portable admission

`Nativeˉhostedˉverifierˉpublisherˉapplicationˉverification(Input, Target)`
accepts only these exact values:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| `1`, Windows x64 | 256,000 | `2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12` |
| `2`, Linux x64 | 254,965 | `8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e` |

Every other target, length, or digest returns false. SHA-256 is computed over
the complete immutable byte value and compared as eight exact little-endian
words. The admission result is not a structural PE/ELF claim; the pinned bytes
already carry the independently established construction evidence.

## Command source

The hosted source command is:

```text
wvhostverifierpublisheradmit <windows|linux> <publisher.exe|publisher.elf>
```

It declares only console output, diagnostic output, file input, and the two
process-argument capabilities. Success reports one fixed validity line and
returns zero. Invalid arguments return 64; a wrong target, length, or digest
reports one rejection diagnostic and returns 2. It never writes a file.

The canonical command WVB is 30,778 bytes with SHA-256
`73c6bfb23c277b6e0384a79bb00a9631709f3d4e9c727e7c27eb9e5dcbbd97f9`.
It builds through the native Project 1 front door. The accepted native ABI-22
lowerer produces a 555,690-byte WVO with SHA-256
`e348c41dcd96dbacedcc1820d42013e3c19795d89f7183ac7bc64311612dd927`.
Both are pinned in version 15 of the publisher-construction candidate.

## Hosted profile and applications

`WVHV 1` profile `8` owns this exact read-only role. It reuses the five
capabilities, six ordered services, one immutable file-input snapshot, runtime
geometry, verifier startup objects, and outer format 4 of profile `2`, but the
expected profile is passed explicitly through metadata, runtime, layout,
platform, startup, and container admission. Existing commands remain strict
profile `2`; construction tools select profile `8` only through the literal
`publisher-admission` mode.

The paired applications are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 570,368 | `1407ed428387986e170b4d8394e9a0a6295408ef668d5d6e16d719102428dd4f` |
| Linux x64 | 569,344 | `27fff54e139228586a6948aa234de60e5d4f5439e6b0616a55c057d4ad8661c2` |

`Construct-Hosted-Verifier-Publisher-Admitter.cmd` and `.sh` reproduce those
bytes through the native linker and hosted-container tools. The current-host
`Admit-Hosted-Verifier-Publisher` launcher pins the admitter identity before
execution but does not pre-hash, copy, rename, or otherwise mutate the subject.

## Remaining boundary

No existing hosted profile was relabeled for this command. `WVHV` profile `7`
remains the console-application verifier, while the hosted-container segmenter
uses numeric profile `7` only inside its separate `WVHG` metadata family.

Read-only admission also must not be followed by a host-side copy or rename:
that would admit one snapshot and mutate from another. Decision 0486 adds the
distinct promoter source plus its native WVB/WVO. Decision 0487 extends the
publisher-specialized construction pipeline with an exact promoter role and
constructs paired promoter applications. The promoter remains separate from the
publisher it installs, and its executable identity remains launcher-pinned.
