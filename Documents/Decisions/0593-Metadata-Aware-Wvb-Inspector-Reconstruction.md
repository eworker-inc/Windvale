# Decision 0593: Metadata-Aware WVB Inspector Reconstruction

- Status: Implemented with current Windows and Debian WSL2 execution evidence; exact identities and promotion pending
- Date: 2026-08-15
- Advances: Decision 0592
- Contracts: [Hosted read-only applications](../../Specifications/Windvale-Hosted-Verifier-Application.md), [Seed bytecode](../../Specifications/Seed-Bytecode.md)

## Context

Decision 0592 established one portable WVB metadata normalizer and connected it
to the source verifier and compiler build driver. The profile-4 WVB inspector
still consumed only the retained absent form, so a valid metadata-bearing module
could be verified and lowered but not inspected through the current hosted
application path.

The historical `Package-Hosted-Wvb` profile argument is not the profile-4
read-only verifier ABI. Reusing it selected compiler-startup policy and produced
invalid evidence. Current construction also admitted profiles 2, 5, 6, 7, and 8
but omitted profile 4 from its container, platform, startup, bundle, metadata,
and runtime-header request models.

Trying a metadata-bearing multi-module application exposed an independent source
compiler defect: alias discovery expected imports immediately after the module
declaration and stopped at the independent metadata header. The source graph
accepted the modules, but imported types and calls were reported as unknown.

## Decision

Admit the canonical `wvb-inspector` label as profile 4 throughout the existing
native hosted-verifier construction pipeline. It uses the same eleven bounded,
read-only services as the WVO inspector and retains the existing profile-4
startup and application authority contract.

Make the canonical inspector project import
`Tools/Windvale.Verify/Wvb-Metadata-Normalization.wv`. File inspection first
validates the raw WVB envelope. When Module metadata is present, it validates and
normalizes the complete input before the existing inspector parses and reports
the executable view. Empty normalization is rejection; unvalidated metadata is
never treated as ignorable trailing data.

Add `wvb-inspector-reconstruction` as a live four-case native owner. On each host
it builds the six current construction tools, compiles and lowers the inspector,
composes the profile-4 application twice, requires identical bytes, runs its
self-tests, inspects an absent-form WVB, and inspects the exact 369-byte
metadata-present fixture with SHA-256
`94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa`.
The Linux owner explicitly restores and checks executable mode on the completed
application before execution; the deterministic container bytes do not encode a
host-filesystem mode bit.
The changed-file planner selects this owner for the inspector, metadata
normalizer, and shared hosted-verifier construction boundaries.

Change source-symbol alias discovery to skip validated header-metadata
declarations before continuing through imports. Pin a binding regression in
which an independent-metadata root imports a portable dependency and calls its
exported function.

## Consequences

- Current Windvale-authored profile-4 Windows and Debian WSL2 applications now
  inspect both retained and metadata-present WVB inputs without .NET and without
  a second metadata parser.
- Shared construction edits now re-prove profile 4 alongside the other hosted
  verifier consumers. This adds four bounded cases to the live registry and
  increases cold verification work, while cache reuse keeps repeat checks
  focused.
- The report describes normalized executable semantics. It does not yet expose
  the original platform, authority, or optional-requirement metadata as report
  fields.
- The alias fix removes a real blocker for metadata-bearing multi-module
  applications, but does not promote the pinned normal compiler front door.
- The same current source state passed the four-case Windows owner in 78.240
  seconds and the Debian WSL2 mirror in 93.000 seconds. These are local
  environment measurements, not release qualification or portable performance
  thresholds. Exact candidate identities and promotion must still complete
  before a normal-front-door claim.
- One complete metadata-bearing package build, install, launch, and execution
  path remains pending; repository-wide source migration is still prohibited.

## Reconsideration triggers

Reconsider when the inspector must report admission metadata directly, when the
normalizer gains typed diagnostics rather than empty-byte rejection, when the
ordinary compiler or inspector candidates are promoted, or when a complete
metadata-bearing package passes the same-commit Windows and Linux owners.
