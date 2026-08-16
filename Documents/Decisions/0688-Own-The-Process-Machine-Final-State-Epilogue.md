# Decision 0688: Own the process-machine final-state epilogue

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0686](0686-Own-Generation-Two-Completion-Finalization.md)
- Contract: [final-state validation epilogue emission](../../Specifications/Windvale-Os-X64-Process-Final-State-Validation-Epilogue-Emission.md)

## Decision

Source-own terminal fixture offsets 31,200 through 33,825 as one final-state
validation epilogue. Keep supervisor, endpoint, channel, resource, mapping,
generation-two thread, memory-record, and return-state checks fail-closed
against the single terminal target.

## Evidence and consequences

The 2,626 normalized bytes have SHA-256
`6d7fa0c376583e5bba075b781b865b874cf123e9fb6021c102f5b8daa0cff591`.
All 164 branches remain explicit. The focused owner advances to fifty-six
projects and 336 cases with results 50 through 105. Source ownership now covers
all 33,826 process-machine bytes and 569 relocation fields.

This completes source ownership of the retained process-machine oracle. It
does not by itself prove an integrated boot image, live QEMU application
execution, or production filesystem/network services; those are the next
vertical-integration phases.

## Reconsideration triggers

Any split must retain one fail-closed terminal target and prove that no partial
final-state acceptance can reach the result-6 return epilogue.
