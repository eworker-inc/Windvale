# Decision 0500: Native WVO inspector reconstruction

- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the profile-6 WVO inspector candidate
- Extends: Decisions 0222, 0301, 0308, 0492, and 0497

## Context

The digest-bound WVO inspector applications already provide the fixed
read-only `verify` and `inspect` behavior on Windows and Linux. Their checked-in
candidate still has Stage 0 application-construction provenance, however, and
the existing native hosted-verifier path covered only the six-service profile 2
and profile 8 shapes.

The inspector needs five additional pure report services: `enum.name`,
`text.concat`, `text.quote`, `i32.format`, and `u32.format`. This requires an
explicit eleven-service profile 6 through bundle evidence, metadata, runtime,
layout, startup, platform, and container construction. It is not safe to treat
profile 6 as an alias of the six-service compiler verifier. The source project
also requires `Foundation` before the object modules and native-subset report
helpers with explicit `i32` results; neither change alters successful command
text or exit status.

## Decision

Windvale adopts one bounded reconstruction route for the exact WVO inspector:

1. build `Projects/Object-Model/Windvale-Wvo-Object.wvproj` once through the retained native project
   front door and require its complete WVB identity;
2. invoke the retained raw accepted-subset lowerer once and require the complete
   WVO identity before linking;
3. link that admitted WVO once at base zero with exported entry `Main`;
4. assemble the target-specific profile-6 inspector startup object from its
   reviewed WVA source;
5. construct the eleven-service bundle, profile-6 metadata and runtime header,
   target platform bytes, relocated startup, and final container only through
   the explicit `wvo-inspector` forms of the retained native tools; and
6. require exact Windows and Linux candidate identities before copying any
   result into a caller-owned empty output directory.

The profile-6 serialized shapes are exact: a 156-byte `WVPQ 1` publication
request inside the bounded `WVSQ 2` envelope, 572-byte `WVVE 1` evidence,
624-byte `WVVR 1` request, fixed 1,024-byte `WVHV 1` metadata with eleven
service records and a 112-byte zero tail, 4,096-byte `WVHR 1` runtime header,
and eleven startup service addresses. Profiles 2 and 8 retain their
six-service bytes and layouts.

The accepted startup objects are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows inspector startup WVA | 9,437 | `f706848709e9c217f31dce6733b8aa3e94518b6f371cbd5ccc8af63603edb495` |
| Linux inspector startup WVA | 5,214 | `01603c6b945b4e03ebef1d3d5bf691a5e05bf2e2630d6466e1db1028b8c9c005` |
| Windows inspector startup WVO | 3,927 | `1bb785d5a06c40b91e45ebdc26b33ae33cb8ee7b244daffaa30ee59b9509edf3` |
| Linux inspector startup WVO | 2,291 | `5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb` |

The exact reconstructed products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| inspector WVB | 61,008 | `a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db` |
| inspector WVO | 591,723 | `f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c` |
| linked inspector fragment, `Main` at 82,280 | 587,529 | `f318ee573b149aac169b67369e90dbacc6451fc129022bfb4e62b2ceff9cfba4` |
| Windows application | 606,208 | `bb39e58d51e7b6c3eab2690995ee52fc958557ab03cfcbcb9b5ef0f3070157d2` |
| Linux application | 606,208 | `bf94145cee63a4d7014bd7a31a40832017f025b7d8086a4ae3875385ba8345c1` |

## Evidence boundary

The profile-aware hosted-container toolset has an exact 72-entry, 6,927-byte
inventory with SHA-256
`e2d6f16ee17e7e3df890583eb0ed796582ef445f74c68db955cd89a7f99e39c4`.
The dependent publisher-construction toolset has an exact 49-entry, 5,064-byte
inventory with SHA-256
`8b752fd2c1b5afed4935453ee4d1f520d8807d439d7ad339f5f71a5ca30c05b1`.
Those inventories prove the construction tools selected by the route and the
final constructor independently verifies every inspector product above.

The focused `wvo-inspector-reconstruction` owner passes all three cases in 28.1
seconds on the current Windows host: candidate inventory, exact WVB/WVO plus
paired-application reconstruction, and current-host compatibility with profile
isolation. The Linux result remains cross-target construction evidence and was
not executed on a Linux host for this decision.

## Consequences

- Profile 6 is explicit at every public native construction command and cannot
  silently change profile-2 or profile-8 output bytes.
- The managed writer is no longer the sole constructor for this exact
  candidate.
- The route consumes retained same-release compiler, lowerer, linker,
  assembler, hosted-container, and service-leaf candidates. It is not a clean
  bootstrap or previous-seed renewal.
- Evidence is produced on the current Windows host. The Linux application is a
  cross-target product until independently reconstructed and executed on Linux.
- Grouped qualification, promotion, and removal or archival of Stage 0 remain
  separate gates.

## Reconsider when

Reconsider this decision if the WVO object source/project closure, native
lowerer service subset, exact WVB or WVO identity, profile-6 service order,
metadata/runtime shapes, inspector startup objects, hosted toolset inventory,
or either target container identity changes.
