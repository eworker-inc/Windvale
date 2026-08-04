# Decision 0208: Native read-only directory snapshot binding

- Date: 2026-08-04
- Status: Implemented candidate on Windows; Linux qualification pending
- Advances: [Decision 0153](0153-First-Versioned-Read-Only-Directory-Capability.md)
  and the reference `windvale run` launcher
- Retains: `filesystem.directory_read_v1`, its `u32` offset and length contract,
  explicit capability authorization, and no native handles or paths in source

## Context

The application-facing Windvale library, capability catalog, provider
interface, response verifier, and conformance oracle already implement one
rights-limited immutable directory snapshot. The ordinary reference launcher
did not bind that interface, so a compiled application could exercise it only
through tests or a custom host.

## Decision

- Add `windvale run --bind-read-only-directory <path>` as the one explicit
  reference-launcher binding for `filesystem.directory_read_v1`.
- Keep provider binding separate from authority. The module must declare the
  capability and the launcher invocation must still include
  `--allow filesystem.directory_read_v1`.
- Resolve the native path only in the launcher. Windvale receives only the
  contract's validated single ASCII segment names.
- Materialize every queryable immediate entry before process entry into an
  ordinal, case-sensitive immutable snapshot. Subsequent host-file changes are
  not observable through the bound capability.
- Bound one reference snapshot to 4,096 queryable entries and 64 MiB of regular
  file bytes. Reject binding before process entry when a regular file exceeds
  the `u32` length contract or the snapshot exceeds either host bound.
- Treat directories, reparse points, and device entries as `Not_file`. Ignore
  names that the capability grammar can never request. Do not follow links.
- Use the same .NET provider implementation on Windows and Linux. Cross-host
  qualification remains pending until the independent Linux gate reports.

## Evidence

The focused test captures a regular file and directory, proves ordinal name
matching, `Not_file`, `Not_found`, and invalid-offset results, changes the host
file after binding and observes the original bytes, then runs a compiled
Windvale application through the real CLI with separate binding and grant
options.

## Consequences

Windows and Linux now have a concrete reference-host path for the first real
filesystem-shaped Windvale library without making native paths part of source
semantics. The eager snapshot intentionally spends bounded memory to provide
the deterministic immutable behavior already required by version 1.

This is not the future mutable storage provider. It does not provide handles,
enumeration, writes, durability, atomic replacement, large-file offsets,
Windvale OS IPC, native AOT service leaves, or WebAssembly bindings.

## Reconsider when

- A consumer needs a snapshot larger than the reference binding bounds.
- Measured provider behavior requires a manifest or open-handle snapshot instead
  of eager materialization.
- Multiple typed directory instances replace the current one-binding
  compatibility model.
