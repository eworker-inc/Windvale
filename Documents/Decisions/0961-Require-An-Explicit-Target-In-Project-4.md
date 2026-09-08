# Require an explicit target in Project 4

## Status

Accepted on 2026-09-08 by maintainer approval. Implementation and qualification
remain separate gates.

## Decision

Introduce `windvale-project 4`, preserving Project 3's source inventory, lock
digest, and single source profile, and requiring exactly one
`target-descriptor "<workspace-relative-path.wvtd>"` directive.

Reuse the native project parser and the existing authenticated split compiler
admission pipeline. The selected target is an explicit bounded input, not a
host default. Existing target and foreign admission must run before publication,
including authentication of an empty foreign catalog where applicable.

The target path obeys existing lexical, containment, identity, and output
separation rules. Exact target bytes participate in build evidence and any
reusable result identity. Missing, unsupported, malformed, or substituted input
must fail without changing a previously published output.

Project 2 and Project 3 retain their existing contracts and reject the new
directive. Their descriptorless/profile-only paths do not become Language 1.0
production admission. This decision does not approve mixed Seed/Language 1.0
closures, alternate parsers, duplicated public libraries, or new WVTD encoding.

## Rationale and consequences

Project 3 cannot select the explicit target required by
[mandatory target and foreign admission](0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md).
A CLI-only target would leave the project insufficient to select its complete
build inputs. A separately versioned manifest makes the requirement explicit
without silently changing old projects.

The [integration plan](../Project/Target-Aware-Project-Admission-Plan.md) owns the
remaining implementation and verification gates. Parser acceptance alone does
not establish filesystem containment or compiler admission. The build provider
must prove those boundaries before Project 4 is a supported production path.
