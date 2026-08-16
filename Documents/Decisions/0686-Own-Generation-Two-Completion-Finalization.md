# Decision 0686: Own generation-two completion finalization

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0685](0685-Adapt-Checked-Completion-Cleanup-For-Generation-Two.md)
- Contract: [generation-2 completion-finalize resume emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Completion-Finalize-Resume-Emission.md)

## Decision

Source-own fixture offsets 30,826 through 31,199 as the inseparable completion
finalization transaction. Keep selected-client re-entry, operation-6
validation, channel close and scrub, generation advancement, context restore,
and `sysretq` in one checked path.

## Evidence and consequences

The 374 normalized bytes have SHA-256
`a23e9c50df6109daf68c303ffa220a04274116aafa3a529c7ced520ec78b0cfe`.
Nine internal fields and external symbol 17 remain explicit. The focused owner
advances to fifty-five projects and 330 cases with results 50 through 104,
31,200 source-owned bytes, and 405 relocation fields.

This closes the generation-two client channel without treating cleanup as
reclamation. Provider shutdown, client memory release, and live QEMU execution
remain later boundaries.

## Reconsideration triggers

Any split must preserve fail-closed operation validation, prevent observers
from seeing a closed channel with retained transient state, and prove that the
generation advances exactly once before client resume.
