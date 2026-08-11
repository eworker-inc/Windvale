# Windvale native hosted-verifier application publisher

## Status and scope

This contract admits and durably publishes the two completed format-4 compiler
verifier applications. The Windvale admission boundary accepts only the exact
Windows or Linux release candidate length and SHA-256. These pins are the
deterministic outputs of the structurally qualified native composer; they are
release admission identities, not a second PE/ELF parser.

The publisher reuses the version-1 native publication transaction. It does not
lower WVB, compose a verifier, or duplicate structural container verification.
Changing the verifier WVB, runtime, startup, service bundle, or platform layout
requires focused composer qualification and an explicit repin here.

## Commands

The standalone admission project builds `wvhostverifieradmit`:

```text
wvhostverifieradmit <windows|linux> <application.exe|application.elf>
```

Valid input reports `native hosted verifier application status=Valid` and
returns 0. Invalid input reports `Invalid` and returns 2. Usage errors return 64.

The durable publisher accepts two distinct, same-kind paths:

```text
wvhostverifierpublish <candidate.exe|candidate.elf> <destination.exe|destination.elf>
```

Admission failure reports
`publication status=Rejected phase=native-hosted-verifier-application`, returns
1, and does not begin mutation. Success uses the shared snapshot, durable sibling
write, exact reread, atomic replacement, directory-durability, and cleanup path.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Admission WVB | 18,091 | `382f0e23711400d94a843324a34b43347a782a893b1f13d4f417ee20554fad17` |
| Windows verifier | 1,004,032 | `aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a` |
| Linux verifier | 1,003,520 | `26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b` |
| Publisher WVB | 29,170 | `77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f` |
| Windows publisher | 256,000 | `17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96` |
| Linux publisher | 254,965 | `510f5ce5d2a494eacf0adc7a613581bc2371c4ad0f5f985f501381edc1632fac` |

`Tools/Native/Publish-Hosted-Verifier-Application.cmd` and `.sh` digest-check
the current-host publisher before execution. The exact publisher applications
are constructed through the digest-bound native hosted-container and
publisher-overlay tools. The frozen Stage 0 C# writer remains only recovery and
differential evidence. The target names are construction-contract identifiers,
not ordinary `windvale aot` targets.

## Evidence and remaining gate

The focused Seed contract rebuilds both WVBs through the native front door,
checks both publisher identities, publishes the current-host verifier, executes
the installed verifier against canonical WVB, observes no CLR/hostfxr/hostpolicy,
and proves corruption preserves candidate, destination, and scratch state. The
native publisher-rejection suite owns the permanent launcher rejection case.

The separate [publisher-metadata contract](Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.md)
now owns construction and admission of the exact `WVVP 1` record without
creating a publisher-WVB digest self-reference.

Independent Linux execution, grouped native retirement qualification, ordinary
path promotion, release integration, and final retirement of the retained Stage
0 recovery writer remain.
