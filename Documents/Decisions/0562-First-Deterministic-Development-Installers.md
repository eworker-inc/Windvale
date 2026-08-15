# Decision 0562: First deterministic development installers

- Status: Qualified and implemented
- Date: 2026-08-15
- Advances: Milestone 3, Decisions 0183 and 0561
- Contract: [Windvale development installer 1](../../Specifications/Windvale-Development-Installer.md)
- Evidence: [paired-host Verify run 31881681424](https://github.com/eworker-inc/Windvale/actions/runs/31881681424)

## Context

Milestone 2 closed one useful package-backed application. Milestone 3 now needs
an actual product-shaped installation path, but signing, the release envelope,
rollback, and the final exact-state qualification are still open. Calling an
unsigned archive `v0.1.0` would weaken the release gate; postponing every
installation experiment until the full trust system exists would leave payload,
host-path, determinism, and uninstall risks unmeasured.

The repository already retains exact .NET-free Windows and Linux x64 compiler,
assembler, linker, runner, verifier, publisher, and inspector artifacts. They can
support a bounded development installer without generalizing package resolution
or adding an OS package-manager dependency.

## Decision

Adopt `0.1.0-dev.1` as the first development installer label. Construct one
stored deterministic ZIP for Windows x64 and one deterministic USTAR/gzip-store
archive for Linux x64 from the exact checked-in tool identities and canonical LF
license bytes. Do not check generated archives into the repository; a caller
selects an existing empty output directory, and a later release job may publish
the pinned bytes as assets.

Each archive carries an exact payload manifest, platform installer and
uninstaller, a bounded `wv` inspection client, an offline payload doctor, and the
seven native commands. Install into a payload-derived immutable per-user
generation, verify before and after copying, reuse only an exact existing
generation, and publish stable command shims without searching the ambient PATH.
Refuse pre-install and installed tampering. Require the exact installation record
before uninstall and preserve external or separately owned application data.

Keep this slice explicitly below the complete package-store architecture. It does
not create signed release metadata, capability approvals, automatic updates,
multi-generation activation, rollback, or garbage collection. Windows PATH
mutation is opt-in; Linux command links cannot replace unrelated entries.

Own the slice through one eight-case focused native suite. Ordinary changes to
the installer inputs select only that owner in addition to any owner of a changed
native producer. Pair its exact Windows and Linux reports before promoting the
candidate.

## Consequences

- Windows and Linux x64 users can install and inspect the native toolchain
  per-user from deterministic development archives; both transactions pass the
  permanent eight-case owner on their target host.
- The two archive and payload identities are exact inputs for the later release
  envelope and offline release verifier.
- Shell and PowerShell are bootstrap installer dependencies, not Windvale source
  semantics or runtime dependencies. Node.js is used only to construct and
  inspect repository release artifacts.
- Download compression, native launcher recovery, signing, threat modeling, and
  exact-state release qualification remain visible Milestone 3 work.

## Reconsideration triggers

Reconsider this decision when a signed release envelope selects these artifacts,
when a native recovery launcher can own activation, when a second installed
generation requires rollback, or when a measured download constraint justifies
one bounded deterministic compression method.
