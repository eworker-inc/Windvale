# Decision 0590: Offline package lifecycle and Generation 1 / Activation 1

- Status: Accepted
- Date: 2026-08-15

## Context

Milestones 1 through 3 are complete and the signed `v0.1.0` preview is
published. Installer 1 already verifies a payload and publishes it beneath an
immutable generation-derived directory, but its command shims select that one
generation directly. It has no portable generation inventory, durable activation
record, interrupted-activation recovery, or rollback contract.

The credible next product lanes were OS-1 composition, an offline package
lifecycle, and a durable application/database increment. Separate agents may
continue OS and database work, but neither needs to become a dependency of the
host package lifecycle. Publishing `v0.2.0` now would also imply a product
promotion before enough additional work has accumulated.

## Decision

Select the offline package lifecycle as product Milestone 4. Do not assign its
current development slices to `v0.2.0`. Keep networking, OS-1, database breadth,
new source syntax, and automatic update discovery outside its completion gate.

Freeze the first portable installed-state contracts in
[`Specifications/Windvale-Installation-Generation.md`](../../Specifications/Windvale-Installation-Generation.md):

- Generation 1 names an exact target, ordered admitted package/lock/bundle
  identities, installed commands, approval identities, and launch identities.
- The SHA-256 of canonical Generation 1 bytes is its immutable identity.
- Activation 1 names one current and optional previous generation with a
  monotonic nonzero `u64` serial.
- Effective activation and rollback increment the serial; selecting the current
  generation is idempotent; serial overflow never wraps.
- Rollback swaps current and previous without rewriting package content.
- Host publication uses private-write, flush, reread, atomic replace, directory
  durability where available, and explicit indeterminate-completion handling.

Implement parsing, bounded views, cross-record package references, and pure
activation/rollback planning in portable Windvale source first. Host adapters
later consume that semantic result and own only native filesystem/durability and
process-binding mechanics.

Milestone 4 completes only when Windows and Linux install two real packages from
one offline release directory, construct and atomically activate an immutable
generation, execute exact approved commands, recover an interrupted activation,
roll back without rewriting content, and uninstall package-owned files without
deleting separately owned application data.

## Consequences

- The existing published `v0.1.0` installer inputs and assets remain immutable.
- The first slice can use the existing package-format owner and does not require
  registry, TLS, DNS, HTTP, civil time, or new compiler semantics.
- General package resolution, command dispatch, multi-package installation, and
  uninstall reachability remain implementation work after the portable contract.
- The first host activation publisher now implements compare-before-write,
  durable private publication, atomic public replacement, explicit
  indeterminate completion, and bounded interruption recovery on Windows and
  Linux without changing the published `v0.1.0` installers. Command dispatch,
  multi-package installation, and uninstall reachability remain open.
- OS-1 and database work may continue independently and later consume package
  identities without changing this milestone gate.
- A later release decision chooses whether accumulated work merits `v0.1.x`,
  `v0.2.0`, or another preview label; Milestone 4 completion alone does not
  publish a version.

## Reconsideration triggers

Reconsider this contract if durable host replacement cannot preserve its
completion distinctions, if a real two-package generation cannot express its
approval/launch closure, if security rollback policy requires more than a
separate high-water constraint, or if multiple writers become a required first
profile.
