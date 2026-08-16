# Decision 0670: Windvale-owned generation-2 endpoint rebind

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0669](0669-Reuse-Checked-Image-For-Generation-Two-Client.md)
- Contract: [generation-2 endpoint-rebind emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Endpoint-Rebind-Emission.md)

## Decision

Emit fixture offsets 25,065 through 25,512 as one fail-closed endpoint-rebind
transaction. Validate both complete closed generation-1 endpoint records and
their provider/channel relationships before changing either client identity to
generation 2.

## Evidence and consequences

The normalized payload SHA-256 is
`0bfe36edc975de32420bf9a13e985f0f218138d523a8df615047562d116880bc`.
The focused owner advances to forty-four projects and 264 cases with results 50
through 93. Windvale source owns the first 25,513 process-machine bytes and 149
relocation fields.

Stale generation-1 endpoint state cannot survive into generation 2 silently.
Resource validation, aliases, readiness, context completion, and re-entry remain.

## Reconsideration triggers

Another rebind design must preserve exact closed-state, provider, channel,
generation, rights, close-evidence, and zero-transient validation before either
new client reference is visible.
