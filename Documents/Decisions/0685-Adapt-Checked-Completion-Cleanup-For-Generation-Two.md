# Decision 0685: Adapt checked completion cleanup for generation two

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0683](0683-Own-Generation-Two-Directory-Reply-Lifecycle.md)
- Contract: [generation-2 completion-cleanup emission](../../Specifications/Windvale-Os-X64-Process-Client-Generation-Two-Completion-Cleanup-Emission.md)

## Decision

Derive fixture offsets 29,475 through 30,825 from the checked generation-one
completion-cleanup path. Change only the retained and selected client
generation bytes from 1 to 2; preserve all validation, cleanup ordering, and
sixty-one fail-closed branches.

## Evidence and consequences

The 1,351 normalized bytes have SHA-256
`26c87f2cb591290184621f423fe18a3ad39929f763c31f7735dd01fa85cf7d6e`.
All sixty-one failure fields resolve to the common failure target at fixture
offset 33,826. The focused owner advances to fifty-four projects and 324 cases
with results 50 through 103, 30,826 source-owned bytes, and 395 relocation
fields.

This adaptation avoids a second cleanup implementation while making the two
generation changes reviewable and testable. It does not yet own reclamation
preflight, later teardown, or live QEMU execution.

## Reconsideration triggers

Reconsider derivation if generation-specific cleanup semantics diverge, if the
common failure target changes, or if later teardown requires a separately
versioned state transition.
