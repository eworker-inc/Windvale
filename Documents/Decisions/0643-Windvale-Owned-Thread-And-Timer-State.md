# Decision 0643: Windvale-owned thread and timer state

- Status: Implemented current-Windows-host native candidate; live scheduling pending
- Date: 2026-08-16
- Advances: [Decision 0642](0642-Windvale-Owned-Privileged-Entry-Setup.md)
- Contract: [thread and timer-state emission](../../Specifications/Windvale-Os-X64-Process-Thread-Timer-State-Emission.md)

## Decision

Emit fixture offsets 12,083 through 12,872 as one private scheduler-state
boundary: exact init, directory-provider, and recyclable-client thread records
plus the first bounded timer record. Keep all records private until their owner,
generation, saved context, page table, and readiness transition are admitted.

## Evidence and consequences

The slice SHA-256 is
`387d6b045d79ba4b4312dedba27acf1d642773b44308f3637b98e48d7c7bd286`.
The owner passes 144 cases across twenty-four projects with results 50 through
73. The retirement inventory is 70 suites and 3,708 cases. Windvale source owns
the first 12,873 process-machine bytes and 86 relocation fields.

Timer arming, context switching, handler bodies, readiness publication, and
live application execution remain separate checkpoints.

## Reconsideration triggers

A dynamic scheduler may replace fixed records only while preserving explicit
ownership, generation safety, bounded budgets, private construction, exact
saved context, and readiness-only publication.
