# Decision 0782: WVDB Workbench and browser application framework

- Date: 2026-08-16
- Status: Accepted experimental product slice
- Draws conventions from: E-Worker 7 browser applications
- Does not accept: direct browser access to database files or credentials, a live WVDB service contract, or the browser host as Windvale OS

## Context

Windvale needs independently deployable web applications in addition to the
public website and the compiler playground. The first requested application is
a database administration environment for the developing WVDB stack. It needs
the desktop-workbench layout already proven in E-Worker applications: command
ribbon, server explorer, tabbed workspace, assistant surface, resizable console,
and a bottom status bar.

The WVDB storage engine and typed query IR are still advancing. A browser client
must not invent a database wire protocol, receive native database paths or
credentials, or imply that synthetic records are live engine results. The UI
architecture can nevertheless become useful now if the first slice makes those
boundaries visible and leaves a typed seam for the future service.

## Decision

Add an installable experimental PWA named **WVDB Workbench for Windvale**, with
`WVDB Workbench` as its product name, `WVDB` as its short installed name, and
`wvdb-workbench` as its application identity. Keep the generic **Windvale
Workbench** name for the existing browser compiler and shell workspace.

Place the deployable application under `Applications/Web/Wvdb-Workbench/` and
place reusable browser-native TypeScript under `Libraries/Web/`. The browser
framework follows the E-Worker 7 component rules while remaining Windvale-owned:

- thin application hosts compose inert feature manifests;
- one explicit state owner publishes snapshots and typed change sets;
- commands reach that owner rather than mutating unrelated components;
- render schedulers invalidate registered component boundaries, not the entire app;
- lifecycle scopes own cleanup in reverse registration order;
- themes and localization packs are scoped dependencies;
- reusable component CSS uses semantic `--wv-*` tokens and `wv.*` cascade layers.

The first WVDB Workbench slice is a synthetic, read-only product preview. It has
light and dark themes, English and French message packs, a resizable/collapsible
server explorer, a ribbon, tabbed central work, a resizable/collapsible local
assistant preview, a resizable console, and a bottom-most status surface. Query
validation and assistant replies are deterministic browser demonstrations. They
do not call a database or model service and they perform no database mutation.

The preview may persist presentation preferences, panel sizes, and up to eight
future-server connection profiles in browser storage. Profile records contain
only an identifier, display name, validated HTTPS endpoint (or HTTP loopback),
and default database name. They contain no credentials and saving one never
attempts a connection. Assistant messages remain session-only. Logs retain at
most 200 entries and assistant history at most 32 entries.

A future live client must call an authenticated, rights-limited Windvale service
or gateway. That service owns canonical request validation, authorization,
database access, query bounds, mutation identities, cancellation, audit, and
typed results. The PWA receives neither database file paths nor provider secrets.
Service DTOs are untrusted at both ends and require a later versioned contract.

The service worker may cache only versioned application-shell assets. API paths,
credentials, database data, query results, and mutations are never placed in the
offline cache. There is no offline mutation queue or silent replay.

## Consequences

Windvale gains a first real browser application and a small reusable component
foundation without coupling product UI to the public playground. The shell can
be installed, inspected in both themes and locales, and evolved alongside WVDB
while remaining honest about the absence of a live service.

Its Noto UI and code font stacks, semantic typography sizes, ribbon behavior,
editor toolbar, native dialogs, and workbench density follow the same UI roles
as E-Worker 7 without importing E-Worker runtime code or third-party font files.

The initial framework is intentionally small. It establishes composition, state,
invalidation, lifecycle, theme, localization, and workbench-shell contracts; it
does not establish a general widget catalog, router, server protocol, plugin
marketplace, or compatibility promise for arbitrary E-Worker components.

The synthetic records and deterministic assistant are presentation fixtures,
not database or model conformance evidence. Create, edit, execute, and durable
administration operations remain unavailable until their service, authority,
failure, cancellation, and uncertain-mutation contracts exist.

## Reconsider when

- the first versioned WVDB administration service protocol is ready;
- authenticated remote server discovery, connection profiles, or secret storage
  require durable contracts;
- multiple Windvale PWAs prove that packaging or routing belongs above individual
  application projects;
- reusable components need a compatibility/versioning policy; or
- offline read data or mutation support can state exact cache, revocation,
  conflict, replay, and durability behavior.
