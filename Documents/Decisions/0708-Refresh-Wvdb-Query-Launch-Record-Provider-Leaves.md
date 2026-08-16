# Decision 0708: Refresh WVDB Query launch-record provider leaves

- Date: 2026-08-16
- Status: Implemented with local Windows and Debian WSL2 evidence
- Advances: [Decision 0564](0564-First-Installed-Capability-Approval-And-Launch-Records.md)
- Contract: [capability approval and launch records](../../Specifications/Windvale-Capability-Approval-And-Launch.md)

## Context

The maintained WVDB Query native-capability owner already reconstructs and pins
the current Windows and Linux host applications. Launch Record 1 still named the
older platform-leaf, linked-image, and host-application identities, so installed
command dispatch correctly denied the rebuilt host as a substitution.

No retained old host application exists in the repository. Reconstructing the
current path shows that the portable WVB, directory-host WVO, ABI 23 entry,
provider table, argument contract, authority, bundle, approval, and final host
sizes are unchanged. The target platform leaves and their derived images are the
stale boundary.

## Decision

- Rebind both WVDB Query Launch Record 1 files to the platform leaves, linked
  images, and host applications already pinned by the current native-capability
  owner.
- Preserve record version 1, approval, bundle, WVB, directory-host WVO, ABI,
  entry address, provider-table grammar, argument limits, and explicit denials.
- Refresh offline-stage, generation, resolution, dispatch, release-verifier, and
  specification consumers to the two new launch-record identities.
- Treat this as an exact-record repair discovered by the Echo metadata migration's
  change-aware gate, not as an expansion of WVDB authority.

## Evidence

| Target | Platform leaf | Linked image | Host application | Launch Record 1 |
| --- | --- | --- | --- | --- |
| Windows x64 | 1,951 bytes / `d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34` | 238,536 bytes / `fe51adddc364f9ec32d9ae0a7925417e1fa6304e930fd42ec9106f31f73d35bc` | 258,048 bytes / `198d44b49db6765792c835c6419da88f0cbcc0de0422748b0d15cb4ae5e6ba32` | 1,315 bytes / `213a59ecf1f9bde65ce596e2627bce1add249f936fc781b71dcba1eb88bcefe7` |
| Linux x64 | 681 bytes / `0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86` | 237,517 bytes / `cae8aee6da474d2acb0a976047c689511a22269377b58114a56e8616fecc708d` | 258,048 bytes / `b21095d6ab62209b67053b7dfe1cf5a2f0130b3722a09a8e48284fc1aa988b3f` | 1,310 bytes / `8ff3152ad30951235abb3504a372c57b2cb1bbff1410bb47933136645580ab88` |

Both images retain `Directory_host_entry` at 235,440 and the exact 2,010-byte
directory-host WVO at SHA-256
`7ab58a817fe5dbc8e8f91b910654487ba62e10bc5aa5d1ae74b6bb07f2f6ca09`.
The derived Generation 1 identities are
`db46b880a6fbff8d60d4dd19b9e6318ca1657dde5f932bc257dc916c7df67a14`
for Windows and
`9bc748c74f93bbdea9ad6bafc58a4bb1b14ab3511e852fa5fe48b158114bf3f1`
for Linux.
The native-capability owner already executes both host applications and checks
their cross-host behavior. The installed-command owner additionally executes the
selected host through the exact launch record and exercises its rejection cases.
Both hosts pass command resolution, dispatch, offline staging, and the 13-case
approval/launch-record owner with the refreshed identities.

## Consequences

- Current WVDB hosts can be selected through installed-command dispatch without
  weakening host identity checks.
- Approval and capability authority do not change.
- Offline generation and release verification now agree with the maintained
  native provider leaves.

## Reconsideration triggers

Reconsider this decision if the read-only-directory provider ABI, entry address,
argument contract, capability closure, or target application layout changes.
