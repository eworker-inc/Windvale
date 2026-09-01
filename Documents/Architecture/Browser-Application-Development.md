# Browser application development

> Status: Current browser application architecture
> Authority: Normative for repository-owned browser applications and libraries
> Last reviewed: 2026-08-31

Windvale browser applications use ordinary browser boundaries deliberately.
They may present or exercise Windvale contracts, but the browser is still a
host: it does not silently gain Windvale OS authority, native paths, or local
credentials.

## Ownership and layout

- Put independently deployable browser applications under `Applications/Web/`
  and reusable browser-native framework or component code under
  `Libraries/Web/`. Keep the public website and developer playground under
  their existing owners.
- Keep application hosts thin. Compose inert manifests, explicit contracts, and
  focused components; importing a module must not register global behavior.
- Give durable application state one explicit owner. Commands request changes
  from that owner, which publishes immutable snapshots and typed change sets.
  Registered render boundaries update only the affected surface.

## Lifecycle and presentation

- Dispose listeners, observers, timers, workers, and other resources through an
  explicit lifecycle scope. A component must not leave work behind after its
  host is removed.
- Treat theme and localization data as scoped inputs. Use semantic design
  tokens, CSS cascade layers, logical layout properties, stable message IDs,
  and a documented fallback locale rather than component-global mutable state.
- Keep application CSS in the `wv.*` cascade layers and custom properties in
  the `--wv-*` namespace. Reusable components must not depend on application
  selectors.

## Mutation and trust

- Model drafting, validation, execution, and durable save as separate states.
  Do not turn field edits into hidden database or network writes, and never
  retry an indeterminate mutation without an idempotency contract.
- Treat every browser/server data-transfer object and persisted browser value as
  untrusted input. Validate it on both sides of the boundary.
- Keep credentials, native paths, database files, and ambient authority out of
  browser bundles. Bind only the narrow service or capability the application
  actually needs.
- An installable progressive web application is still a browser host. Offline
  support, background work, caching, and installability are declared per
  application and do not imply unavailable server capabilities or Windvale OS
  semantics.
