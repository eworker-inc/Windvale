# Decision 0649: Windvale-owned init return and program validation

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0646](0646-Windvale-Owned-Provider-Return-And-Init-Transfer.md)
- Contract: [init-return and program-validation emission](../../Specifications/Windvale-Os-X64-Process-Init-Return-Program-Validation-Emission.md)

## Decision

Emit fixture offsets 13,448 through 13,786 as one fail-closed init-return and
program-resource validation transaction. Require the returning init thread and
process to retain their admitted states, then reacquire the exact generation-one
program resource and prove its identity, geometry, rights, publication state,
owner generation, and private page-table linkage before client activation can
continue.

## Evidence and consequences

The normalized slice SHA-256 is
`18cab9dafda9e6619822969c036d304b5cfc025aeab91be77b86379071ee1d74`.
The focused owner advances to twenty-eight projects and 168 cases with results
50 through 77. Windvale source owns the first 13,787 process-machine bytes and
100 external relocation fields.

Budget, store, and directory validation, client transfer, syscall and exception
handler bodies, context switching, and live QEMU application execution remain
separate evidence.

## Reconsideration triggers

Another return or client-activation design must retain explicit init-state and
generation checks, exact program-resource identity and rights, private
page-table validation, and a common fail-closed rejection boundary.
