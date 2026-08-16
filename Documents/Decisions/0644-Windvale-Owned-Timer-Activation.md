# Decision 0644: Windvale-owned timer activation

- Status: Implemented current-Windows-host native candidate; live IRQ proof pending
- Date: 2026-08-16
- Advances: [Decision 0643](0643-Windvale-Owned-Thread-And-Timer-State.md)
- Contract: [timer activation emission](../../Specifications/Windvale-Os-X64-Process-Timer-Activation-Emission.md)

## Decision

Emit fixture offsets 12,873 through 12,997 as one fail-closed timer activation
transaction: page-table admission, GS ownership binding, timer arming, rollback,
and timer-resume transfer. Keep all imported and internal targets explicit.

## Evidence and consequences

The normalized slice SHA-256 is
`3ac2dc5ef8642caba8671c0ee689008be5c4d2626355746406ca86931a83bcf4`.
The owner passes 150 cases across twenty-five projects with results 50 through
74. The retirement inventory is 70 suites and 3,714 cases. Windvale source owns
the first 12,998 process-machine bytes and 92 relocation fields.

Live timer delivery, handler execution, readiness publication, and application
execution remain mandatory later evidence.

## Reconsideration triggers

Another timer provider may replace these imports only while preserving checked
page-table activation, per-thread GS ownership, bounded arming failure,
rollback, and explicit resume authority.
