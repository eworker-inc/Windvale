# Decision 0484: Native WVHV publisher admission lowering

- Status: Implemented current-host candidate; hosted packaging pending
- Date: 2026-08-09
- Advances: [Decision 0483](0483-Native-WVHV-Publisher-Application-Admission.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [publisher application admission](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Application-Admission.md)

## Context

Decision 0483 established the non-circular admission source and reproducible
WVB, but the accepted native lowerer rejected that first command shape with
`Unsupportedˉcode function=0 detail=1`. The portable admission algorithm was
not the cause. `Main` used the newer `text.equal` opcode for its two platform
names, while accepted hosted tools already use bytewise text comparison.

## Decision

- Keep the exact publisher length and SHA-256 admission unchanged.
- Compare the two fixed platform names with the established bounded UTF-8 byte
  loop instead of expanding the native backend in this slice.
- Rebuild and natively lower the command through the digest-bound front doors.
- Pin both resulting WVB and WVO in the existing construction candidate.
- Extend the existing inventory case to reproduce both artifacts; do not add a
  second test case or rerun a broader verification level.

## Evidence and consequences

The native source front door now produces a 30,837-byte WVB with SHA-256
`f1e7497dc1acba1a08190021d4dac83ec65c3e6b58f80edb3bfcd62eeda55ed3`.
The accepted native lowerer reports ABI 22, 554,720 code bytes, and emits the
exact 556,273-byte WVO with SHA-256
`ac5972e8de83ad962874217ed6e0fba49586096df4c3b69d61abdf7509e2dff5`.

Version 9 of the candidate pins 44 artifacts, and the focused six-case native
lane rebuilds both new admission products before exercising the unchanged
publisher construction and execution cases. This closes the native lowering
gap without changing source-language semantics or adding C#.

It does not allocate a hosted application profile, package or execute paired
admission applications, perform durable publisher replacement, qualify Linux,
or promote the candidate.

## Reconsideration triggers

Use `text.equal` directly when that opcode joins the accepted native subset
under its own semantic and malformed-input evidence. Version the hosted profile
separately; do not reuse profile 7 or combine read-only admission with a
host-side copy.
