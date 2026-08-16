# Decision 0683: Own generation-two directory-reply lifecycle

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0681](0681-Adapt-Checked-Directory-Reply-For-Generation-Two.md)
- Contract: [generation-2 directory-reply lifecycle emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Directory-Reply-Lifecycle-Emission.md)

## Decision

Source-own fixture offsets 28,804 through 29,474 as one checked lifecycle
transaction. Keep endpoint validation and publication, channel scrubbing and
generation advancement, dispatcher crossing, page-table activation, context
restore, and exact 3,096-byte reply delivery inseparable.

## Evidence and consequences

The 671 normalized bytes have SHA-256
`4b33029c820d77e83ee01d79154d5dc4fdeaede72b25d245b8da256092ccf777`.
Twenty-eight internal fields and external symbol 17 remain explicit. The
focused owner advances to fifty-three projects and 318 cases with results 50
through 102, 29,475 source-owned bytes, and 334 relocation fields.

This stronger generation-two path does not collapse to the earlier delivery
constructor: exact endpoint publication and channel lifecycle mutation are now
part of the checked transaction. Later client completion, reclamation, and live
QEMU evidence remain.

## Reconsideration triggers

Any split must prove that no observer can see a published endpoint with stale
request state, that all counters and generations remain bounded, and that
failure before `sysretq` leaves no ambiguous mutation.
