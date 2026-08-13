# Decision 0529: Native capability-bearing library composition

- Date: 2026-08-12
- Status: Implemented candidate
- Requires: [Decision 0527](0527-Native-Only-Forward-Development-Boundary.md)
- Extends: [Decision 0145](0145-First-Capability-Bearing-Static-Library.md)

## Context

The current library tree already separates deterministic Foundation and database
algorithms from hosted adapters, but the ordinary native compiler rejected any
dependency that declared a capability. That made rights-limited platform libraries
individually expressible without allowing a hosted application library to compose
them through imports.

The Seed capability catalog also lacked the implemented read-only directory and
random-access storage signatures in the Windvale-owned compiler and verifier path.

## Decision

Permit a dependency to declare a catalog capability when its profile is compatible
with its importer. The root module must explicitly redeclare every capability in its
complete dependency closure. A library requirement is therefore visible at the
application boundary and does not become an implicit grant.

Only root declarations form the final canonical WVB capability table. Dependency
calls are rebound by ordinal capability name to the root-owned indices, independent
of dependency order and source declaration order.

Add these exact catalog signatures:

```text
filesystem.directory_read_v1(text, u32, u32) -> bytes
storage.random_access_v1(u32, u64, u64, u32, bytes) -> bytes
```

Keep profile compatibility monotonic: portable may import portable; hosted may
import portable or hosted; system may import any current profile. Reject a missing
root capability declaration and an incompatible import profile before WVB
publication.

## Repository ownership

Seven Project 2 manifests under `Projects/Libraries/` own the current library build
inventory. `Tools/Native/Test-Libraries` is the focused Windows/Linux owner. Its 12
cases build all seven libraries, one positive capability-bearing importer, two
database conformance applications, and two negative capability/profile fixtures.

The changed-file verification planner maps `Libraries/`, `Projects/Libraries/`, the
library fixtures, and their capability/database specifications to this native suite.
It replaces the former `database-native-tests` gap.

## Consequences

- Hosted libraries can compose other hosted libraries without hiding transitive
  authority.
- Final WVB capability order remains deterministic and root-owned.
- Capability approval at compilation remains distinct from launcher authorization
  and provider binding.
- Package resolution, binary library distribution, optional capability calls,
  multiple typed provider instances, and runtime WVB linking remain deferred.

## Evidence and qualification boundary

The focused Windows suite completes all 12 cases. The same checked-in shell owner
provides the Linux lane. Independent Linux execution from the final source state is
required before a cross-host qualification claim.

## Reconsideration triggers

Revisit this rule when module metadata represents capability identity and version
separately, when optional capability calls become executable, or when runtime-linked
binary libraries replace static source internalization.
