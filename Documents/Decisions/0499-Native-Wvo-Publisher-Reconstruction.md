# Decision 0499: Native WVO publisher reconstruction

- Status: Accepted
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the exact WVO publisher candidate
- Extends: Decisions 0308, 0475 through 0483, 0492, and 0497

## Context

The Windvale WVO publisher already owns complete portable WVO admission and
reuses the native durable publication transaction. Its exact WVB and paired
applications nevertheless retained Stage 0 application-construction
provenance. The ordinary lowerer launcher also invokes this publisher to
durably publish its output, so using that composite command to lower the
publisher itself would make the reconstruction circular at the target
application boundary.

The retained digest-bound raw lowerer can lower the exact publisher WVB without
invoking the publisher. The existing hosted-verifier publisher-construction
pipeline can then treat this WVO publisher as a distinct fourth role and reuse
the admitted startup, publication adapter, SHA-256 object, service bundle,
platform construction, and final materializers.

## Decision

Windvale adopts one bounded reconstruction route for the exact WVO publisher:

1. consume the WVB produced from `Projects/Tools/Windvale-Wvo-Publisher.wvproj` through the
   native project front door;
2. invoke the retained raw accepted-subset lowerer directly, avoiding the
   composite lower-and-publish wrapper;
3. require the complete lowered WVO to match the retained exact WVO oracle
   before linking;
4. link the admitted WVO once at base zero with exported entry `Main`;
5. construct the target base and the role-3 `WVPO 1` publication overlay
   through the retained native hosted-container and publisher-construction
   toolsets; and
6. require the completed Windows and Linux applications to match their exact
   identities before copying either result to a new destination.

The exact accepted artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| publisher WVB | 41,365 | `4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5` |
| publisher WVO oracle | 408,284 | `29c1cc269b9387944b4d43fe9215392044996ad47da55be45a1d177f26e5bafb` |
| Windows application | 430,080 | `ad4c2a05115b2acdb074c0f53b6d7470c8bcacfdfea86583043bdd0ff511188a` |
| Linux application | 426,949 | `4b0ce2d332648e3dd572596db4490748bf62ee4448a9550d83c152de60f7e51d` |

The target-specific intermediate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| linked publisher fragment | 406,840 | `591231b7900aecea5700e139dfd67e36afa3e04a68a87d255aa2be3eb852c828` |
| Windows base application | 422,912 | `1f9361126c368f133693222cbaa4c21e2d0948e79df7bf945b7b037ac815e884` |
| Linux base application | 421,888 | `af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7` |

## Evidence

The current Windows host reconstructs the exact WVO oracle and both target
applications through the retained native route without loading .NET or
invoking the Stage 0 application writer. The Linux result is cross-target
construction evidence only; it was not executed on a Linux host for this
decision.

The focused `wvo-publisher-reconstruction` retirement owner passes both fixed
cases in 30.9 seconds: exact candidate inventory, followed by native WVB and
paired-application byte equality. The existing shared
`hosted-verifier-publisher-files` owner also passes all 15 cases in 183.3
seconds, preserving the exact role-0 publisher, role-1 promoter, and role-2
WVB-publisher paths after adding role 3.

## Consequences

- The managed writer is no longer the only constructor for the exact current
  WVO publisher applications.
- Direct raw lowering prevents the WVO publisher from publishing the object
  used to construct itself.
- The route still consumes retained same-release compiler, lowerer, linker,
  hosted-container, and publisher-construction seeds. It is not clean-bootstrap
  or previous-seed renewal evidence.
- Independent Linux reconstruction and execution, grouped qualification,
  candidate promotion, and recovery deletion remain separate gates.
- This decision does not release or delete any C# Stage 0 implementation.

## Reconsider when

Reconsider this decision if the publisher source closure, exact WVB or WVO
oracle, raw lowerer, role-3 metadata, startup or publication objects, hosted
toolset, publisher-construction toolset, or either target container identity
changes.
