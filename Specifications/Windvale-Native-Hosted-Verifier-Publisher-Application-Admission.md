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
| `1`, Windows x64 | 256,000 | `17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96` |
| `2`, Linux x64 | 254,917 | `babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97` |

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
`b4e0a2ee04de6cfff0efc723c57031bf5cfcd6706e3156525ce2157c5f287d07`.
It builds through the native Project 1 front door. The accepted native ABI-22
lowerer produces a 555,690-byte WVO with SHA-256
`88cc97665cfd0de14f2c9ac6c80dfd985edc508fccdc3d9b887da740cd034e23`.
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
| Windows x64 | 570,368 | `4742ee299759728be1b72fed3d3b42620c21b10f77aed12cf150c1549b177b53` |
| Linux x64 | 569,344 | `b03788fad58ce071788b2f30945ed1dc0992559bb04b6cad04e719ff1114dc0a` |

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
