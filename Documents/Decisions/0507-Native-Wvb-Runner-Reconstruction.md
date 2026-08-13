# Decision 0507: Native WVB-runner reconstruction

- Status: Current-host focused evidence complete; grouped qualification pending
- Date: 2026-08-10
- Scope: retained-WVB native reconstruction of the paired profile-5 runner applications
- Extends: Decisions 0217, 0220, 0497, and 0500

## Context

The ordinary WVB runner was already a digest-bound native executable, but its
paired applications retained historical Stage 0 container-construction
provenance. The current accepted-subset lowerer now admits the exact retained
runner WVB. The shared WVHV construction path also has profiles 6 and 7, making
profile 5 a bounded extension rather than a new container family.

Profile 5 has five capabilities and nine ordered services. It reuses the
inspector startup object, whose complete relocation table names eleven
services. The runner deliberately omits enum-name and text-quote, so exact
optional-service target admission is required for two positions and no others.

## Decision

Windvale adopts one exact retained-WVB reconstruction route:

1. require the retained 90,009-byte runner WVB;
2. lower it through the current native WVB-to-WVO candidate and require the
   exact ABI-22 WVO;
3. link `Main` at address 10,049 and require the exact flat fragment;
4. assemble the retained Windows and Linux inspector startup sources;
5. construct the profile-5 metadata, runtime, service bundle, startup, and
   complete container for both targets; and
6. require byte equality with a four-artifact candidate inventory.

The exact products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB | 90,009 | `3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626` |
| WVO | 761,854 | `e92eed5006a7a98609173c0ed73e66a7aec5e152d8556c9174cab928b946a505` |
| linked fragment | 761,278 | `d602b50d9057f0aad1bb7dca32e624cf78a78244e53ec1a053455caf66a02212` |
| Windows application | 778,240 | `578ddd302da5fbd8d8e14c9410787f5aa05378429a1aca738ee2057e2f9ac1a5` |
| Linux application | 778,240 | `16f39270c239609c6f58b086d0648609fad46860ba9bdd198fa7e6668b628047` |

The normal launchers now use this candidate rather than rewriting the
historical `Native-Front-Door` seed. The focused Windows owner passes 3/3:
inventory, exact paired reconstruction, and current-host execution plus
rejected-input preservation.

## Evidence boundary

The native Project front door still rejects `Projects/Tools/Windvale-Wvb-Runner.wvproj` at
source bindings. This decision therefore begins at the retained exact WVB and
does not claim source-to-WVB reconstruction. The Linux executable is a verified
cross-target product but has not been executed independently on Linux.

This evidence does not establish clean bootstrap, previous-release renewal,
grouped qualification, promotion, or deletion of recovery sources and tools.

## Consequences

- The paired runner containers no longer depend solely on a managed writer.
- The ordinary digest-bound runner launcher follows a current reconstructed
  candidate while the historical front-door seed remains intact.
- The fixed native retirement coordinator gains one three-case owner, reaching
  43 suites and 3,204 cases.
- Source-binding closure and dual-host qualification remain explicit next gates.

## Reconsider when

Reconsider this decision if the retained WVB, lowerer WVO, linked entry or
fragment, profile-5 service set, optional-target policy, shared tool
inventories, or either target application identity changes.
