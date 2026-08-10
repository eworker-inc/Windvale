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
| `1`, Windows x64 | 256,000 | `735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6` |
| `2`, Linux x64 | 254,917 | `de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a` |

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
`c6ba933fa0ea1068f02235f75ed251655b10b43d64f8984d22b548f01608af0d`.
It builds through the native Project 1 front door. The accepted native ABI-22
lowerer produces a 555,690-byte WVO with SHA-256
`722d819152d8415487c1cf111474fd11dd0ab89a863e33ab84c865a2e3e13771`.
Both are pinned in version 12 of the publisher-construction candidate.

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
| Windows x64 | 570,368 | `7f58a5e321d1b4baa16ba673b3e0e1c21c9acd040cba92dae0f180d629c63e6b` |
| Linux x64 | 569,344 | `9bfe16fa751e21a32847f5534eff7de18ba74cfe5b714c63fb6a6589d30d7cad` |

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
