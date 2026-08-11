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
| Admission WVB | 18,091 | `ffba52552f843a50efe00443492aa38116dd10d70149bb23025b8d488f43421e` |
| Windows verifier | 1,004,032 | `5f0a83681f54c7e047d6b68c86f71767d6c3584330bef1e68108f9b3465167a7` |
| Linux verifier | 1,003,520 | `824e90ae07e82af3d6d0b4cf23bc4d3327fc3367684215171247fa71ab274982` |
| Publisher WVB | 29,170 | `7ecbd7f0b11bdd7ce0ab578767b1d697bc16653e4f8182858e0ad8b8d808fb9e` |
| Windows publisher | 256,000 | `2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12` |
| Linux publisher | 254,965 | `8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e` |

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
