# WVDB Workbench frontend improvement plan

- Status: Current tranche implemented
- Updated: 2026-08-20
- Product: WVDB Workbench for Windvale
- Boundary: browser-only interaction and presentation; no WVDB or model service

## Goal

Make the synthetic workbench useful as a polished desktop application shell
while WVDB service contracts are still being designed. Every item in the
current tranche must work without credentials, database files, network calls,
or mutation authority.

## Current tranche

The current frontend tranche is complete when the workbench provides:

1. hierarchical server, database, and collection disclosure with expand-all,
   collapse-all, filtering, correct `aria-expanded` state, and stable selection;
2. independent explorer, assistant, console, and ribbon collapse controls;
3. persisted panel visibility and resizer dimensions with a safe layout reset;
4. a searchable command palette opened from the title bar or `Ctrl+K`;
5. keyboard commands for query validation and the primary panel toggles;
6. functional Console, Activity, and Problems tabs plus local log clearing; and
7. equivalent English and French copy in light and dark themes;
8. native connection-profile and settings dialogs, with at most eight validated
   non-secret profiles persisted in the browser;
9. an E-Worker-aligned editor toolbar and typography roles using Noto UI and
   code stacks with local system fallbacks;
10. a session-only assistant with bounded history, context controls, suggestion
    prompts, and a clear no-model-call boundary; and
11. a distinctive generated PWA icon at the installed sizes.

Acceptance remains explicit: query validation and assistant responses are
deterministic local demonstrations, disabled commands stay visibly disabled,
and no action implies that a WVDB or model request occurred.

## Later browser-only tranches

- Multiple local query drafts with close, duplicate, rename, dirty, and restore
  behavior that never silently claims a durable database save.
- Parameter editing, bounded-history inspection, and
  explicit export/import of non-secret workspace preferences.
- Complete arrow-key tree navigation, focus restoration, screen-reader audit,
  high-contrast tokens, reduced-motion behavior, and an RTL pseudo-locale.
- Responsive drawer behavior for narrower screens and touch-sized resize or
  collapse alternatives.
- Install/offline diagnostics that distinguish cached application-shell assets
  from live database availability.
- Connection-profile import/export without secrets, profile ordering, and
  connectivity tests once the server DTO exists.
- Reusable notifications, confirmation surfaces, empty states, property panels,
  and split editors as additional applications prove their contracts.

## Backend-gated work

The following work does not enter a browser-only tranche:

- connection authentication, secret storage, or server discovery;
- live catalogs, schemas, records, indexes, plans, metrics, or logs;
- query execution, cancellation, timeout, or result streaming;
- create, edit, delete, import, backup, restore, or other mutation;
- live model inference or database-aware assistant tools; and
- offline database data or mutation replay.

Those features require versioned service DTOs, exact authority, validation,
resource limits, cancellation, audit, revocation, and uncertain-mutation
behavior before their UI commands can be enabled.
