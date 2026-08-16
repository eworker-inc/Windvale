# Decision 0645: Windvale-owned provider user-context transfer

- Status: Implemented current-Windows-host native candidate; live readiness pending
- Date: 2026-08-16
- Advances: [Decision 0644](0644-Windvale-Owned-Timer-Activation.md)
- Contract: [provider user-transfer emission](../../Specifications/Windvale-Os-X64-Process-Provider-User-Transfer-Emission.md)

## Decision

Emit fixture offsets 12,998 through 13,168 as the guarded directory-provider
user transition. Require role, generation and page-table validation; bind GS;
record the kernel continuation; publish only the running thread state; and load
the admitted user context immediately before `sysretq`.

## Evidence and consequences

The normalized slice SHA-256 is
`97a5e196a8b8be65b8d580bcc9b781610fee15d660532f7dbb0d3ce51b269d09`.
The owner passes 156 cases across twenty-six projects with results 50 through
75. The retirement inventory is 70 suites and 3,720 cases. Windvale source owns
the first 13,169 process-machine bytes and 99 relocation fields.

Provider readiness, syscall return behavior, client transfer, handlers, and
live QEMU execution remain separate evidence.

## Reconsideration triggers

Another context-switch design must retain role/generation checks, admitted page
tables, per-thread GS ownership, exact continuation state, and fail-closed user
entry.
