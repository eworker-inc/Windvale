# Decision 0677: Adapt checked init reply for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0676](0676-Adapt-Checked-Client-Return-For-Generation-Two.md)
- Contract: [generation-2 init reply-publication resume emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Init-Reply-Publish-Resume-Emission.md)

## Decision

Derive fixture offsets 27,470 through 27,805 from the existing checked init
reply-publication/resume constructor. Change only the returned operation from 3
to 7 and retained client-generation state from 1 to 2, then supply the later
fixture position's exact internal displacements.

## Evidence and consequences

The normalized payload differs from generation 1 only at bytes 44 and 112 and
has SHA-256
`1bf543cae5f5e9696415ab7cda696fce0945c6c31132c2e531f9d431b8e4deaf`.
The focused owner advances to forty-nine projects and 294 cases with results
50 through 98. Windvale source owns the first 27,806 process-machine bytes and
260 internal or external relocation fields.

The generation-two init reply returns zero to the client through the same
channel, page-table, GS, continuation, and `sysretq` contract. The following
client handler, later lifecycle, and live QEMU evidence remain.

## Reconsideration triggers

Another reply path must preserve both explicit state differences, channel-state
clearing, dispatcher and continuation targets, external page-table activation,
zero completion, and the exact client `sysretq` boundary.
