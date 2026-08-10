# Decision 0486: Native WVHV publisher promoter source

- Status: Implemented portable source and native object; application construction pending
- Date: 2026-08-09
- Advances: [Decision 0485](0485-Native-WVHV-Publisher-Admission-Applications.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native publisher promotion](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Promotion.md)

## Context

Decision 0485 closes exact read-only admission for the completed publisher
applications. Installing a file after a separate admission process would read a
second snapshot and reopen the race that the existing durable publisher adapter
already prevents. Adding the completed publisher digests to the publisher
itself would instead create a self-digest cycle.

## Decision

- Add a distinct Windvale promoter source named
  `wvhostverifierpublisherinstall`.
- Import the exact publisher-application admission module and the existing
  publication transaction bridge; do not add a new state machine or assembly
  adapter.
- Read the candidate once in Windvale and return admission status before the
  injected native adapter begins the sibling/reread/atomic-replacement sequence.
- Retain the private `Applicationˉpublicationˉpublisherˉbegin/apply` ABI used by
  the existing publisher specialization.
- Pin the natively built WVB and natively lowered WVO in the publisher
  construction candidate. Defer the role-aware PE/ELF construction extension
  rather than cloning the complete publisher pipeline.

## Evidence and consequences

The native Project 1 front door produces a 41,268-byte WVB with SHA-256
`30eb1e8c93b01266592b322b9c5154b27782ea6c7cd2b6522a10781bf935bec9`.
The accepted native lowerer produces a 660,123-byte WVO with SHA-256
`6f20c95c4c09958dcc09ee35b8f7a3a0330d67f26446206be5bdd85cd8cb042d`.
Native linking places `Main` at 1,178 and produces a 658,339-byte flat fragment
with SHA-256
`a7c0ef19de332e00dcae74c9ab8c25b16b1e1ca73169d4485c85575412a28ed8`.
The transaction apply/begin entry points remain at 0/789, and the object has no
imports.

Version 11 of the publisher-construction candidate contains 24 canonical
WVB/WVO artifacts plus the unchanged 22 paired construction-tool packages. Its
46-entry inventory is 4,812 bytes with SHA-256
`3e8f91bfdb305ef0652036b12a63adf88920483ce5b6e2ca6622c3311fbd0d11`.
The existing focused inventory owns native rebuild, lowering, linking, and byte
equality. The reviewed `hosted-verifier-publisher-files` filter passes all nine
cases locally (`Suites: 1, Passed: 1, Failed: 0, Cases: 9`) in 65 seconds. The
changed-file planner contract passes all 27 general and 14 native cases. No C#
source or Stage 0 target is added.

This slice does not claim an executable promoter or durable installation yet.
The current publisher-construction formats pin the original 29,170-byte WVB,
233,804-byte WVO, and completed application geometry, so silently feeding the
larger promoter into them would be invalid.

## Reconsideration triggers

Extend the construction records with an explicit exact role only when both the
original publisher remains byte-identical and the promoter receives independent
Windows/Linux identities. Do not implement promotion as read-only admission
followed by host copying, and do not allow caller-supplied expected digests.
