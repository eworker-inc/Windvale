# Decision 0681: Adapt checked directory reply for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0680](0680-Adapt-Checked-Directory-Request-Delivery-For-Generation-Two.md)
- Contract: [generation-2 directory reply-publication resume emission](../../Specifications/Windvale-Os-X64-Process-Directory-Generation-Two-Reply-Publish-Resume-Emission.md)

## Decision

Derive fixture offsets 28,468 through 28,803 from the checked directory reply
publication/resume constructor. Change operation 2 to 4 and selected provider
generation 1 to 2, then supply the later position's exact displacements.

## Evidence and consequences

The normalized payload differs only at bytes 44 and 112 and has SHA-256
`7ad00a77679e7a99cd216ccbb9aca3b48f0b83fd5bcf590214dca1dea8f015c6`.
The focused owner advances to fifty-two projects and 312 cases with results 50
through 101. Windvale source owns the first 28,804 machine bytes and 305
relocation fields.

The isolated provider publishes the exact 3,096-byte reply and resumes with
zero through the same checked machine contract. Client delivery, later
lifecycle, and live QEMU evidence remain.

## Reconsideration triggers

Another provider reply path must preserve operation/generation state, exact
reply size, channel clearing, all dispatcher and continuation targets, external
page-table activation, zero completion, and the provider `sysretq` boundary.
