# Decision 0672: Windvale-owned generation-2 return validation

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0671](0671-Windvale-Owned-Generation-Two-Reentry.md)
- Contract: [generation-2 return validation emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Return-Validation-Emission.md)

## Decision

Emit fixture offsets 25,954 through 26,964 as one resumed-handler validation
transaction. Preserve all sixty terminal branches as explicit internal fields,
and require processor, resource, generation, mapping, alias, and context-record
identity before the next dispatcher crossing.

## Evidence and consequences

The normalized payload SHA-256 is
`340f3a8475b659130200e9422629c51e2889a76b4e2c1ddf54f88f84b6146d97`.
The focused owner advances to forty-six projects and 276 cases with results 50
through 95. Windvale source owns the first 26,965 process-machine bytes and 223
internal or external relocation fields.

The first generation-2 return handler no longer trusts retained resource or
alias state implicitly. The following dispatcher crossing, subsequent re-entry,
later lifecycle, and live QEMU evidence remain separate.

## Reconsideration triggers

Another handler design must preserve generation-bounded resource references,
page-table and backing-object identity, context-record ownership, exact terminal
targets, and fail-closed validation before privileged dispatch resumes.
