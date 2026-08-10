# Decision 0487: Native WVHV publisher promoter applications

- Status: Accepted
- Date: 2026-08-09

## Context

Decision 0486 established a separate Windvale promoter source and exact WVB/WVO
identities so a publisher would never need to contain its own completed digest.
The native publisher-container pipeline still admitted only the smaller
hosted-verifier publisher geometry and final identities.

## Decision

The publisher-construction records now carry an explicit exact role: 0 is the
existing hosted-verifier publisher and 1 is its durable promoter. Reserved
fields `WVPM[36]`, `WVVP[112]`, `WVPS[120]`, and `WVCR[28]` carry that role.
Identity construction infers it from the admitted WVB/WVO pair rather than
trusting caller input. Existing role-0 records and final publisher applications
remain byte-identical.

Role 1 reuses the same target startup, publication adapters, SHA object, and
immutable-snapshot durable transaction, with exact promoter module geometry,
placements, imports, metadata, and final identities. The paired applications
are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 681,472 | `9cb234a57c9ff71b6ee44a0d687521e6fd7ccf82784b369e5e65b8ed40666069` |
| Linux x64 | 680,901 | `9406a1e2610db48e744a0912ab4abb2281856e92f7a0d870292c16105d9b9af0` |

Their identities live only in the external candidate manifest and launchers.
The promoter admits and installs the exact publisher subjects; it never admits
or installs itself. No new C# target, alternate transaction adapter, or generic
hosted profile is introduced.

## Evidence

The 46-entry construction inventory is 4,812 bytes with SHA-256
`76c8eebd5d5f426c496beda5f7338ee3dcad4c27edeea9e9d5de49acd236cad2`.
The focused file-pipeline owner passes 12 of 12 cases, including exact paired
construction, current-host promoter installation of both publisher subjects,
and an installed publisher-to-verifier chain. The publisher rejection owner
passes 4 of 4 cases with candidate/destination preservation and zero scratch.
The changed-verification planner contract passes 27 general and 15 native
dispatch cases. No unfiltered or broad qualification gate was run for this
slice.

## Consequences

Durable publisher installation no longer depends on a managed writer or a host
copy following read-only admission. The remaining retirement work is
independent Linux execution, grouped dual-host qualification, promotion, and
release integration. Reconsider the role fields if a third publisher-style
specialization appears or if its geometry can no longer be represented by the
current exact record family without weakening admission.
