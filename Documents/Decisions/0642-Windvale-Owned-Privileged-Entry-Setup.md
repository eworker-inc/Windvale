# Decision 0642: Windvale-owned privileged entry setup

- Status: Implemented current-Windows-host native candidate; live guest qualification pending
- Date: 2026-08-16
- Advances: [Decision 0641](0641-Windvale-Owned-Client-Directory-Validation.md)
- Contract: [privileged-entry emission](../../Specifications/Windvale-Os-X64-Process-Privileged-Entry-Emission.md)

## Decision

Emit fixture offsets 11,442 through 12,082 as one privileged-entry boundary:
GDT/TSS construction, four IDT gates, descriptor-table activation, x86-64
syscall feature admission, and exact syscall MSR setup. Keep handler symbol
relocations and internal failure/entry targets explicit. Do not treat hosted
byte reproduction as proof that the processor executed these instructions.

## Evidence and consequences

The normalized slice SHA-256 is
`6ac9279ab67e1a6c3fe408cec86730b778b96d0cb8e205bf89917966b635cb32`.
The owner passes 138 cases across twenty-three projects with results 50 through
72. The retirement inventory is 70 suites and 3,702 cases. Windvale source owns
the first 12,083 process-machine bytes and 86 relocation fields.

This establishes exact syscall/exception entry prerequisites, but exception
handlers, syscall dispatch, timer programming, client publication, and a live
application run remain separate checkpoints.

## Reconsideration triggers

Another architecture or table layout may replace this fixed x86-64 encoding,
but must retain explicit privilege transitions, handler identities, feature
admission, per-CPU state, bounded failure, and live-hardware/emulator evidence.
