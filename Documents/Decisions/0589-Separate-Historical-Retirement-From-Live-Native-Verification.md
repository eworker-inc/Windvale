# Decision 0589: Separate historical retirement from live native verification

- Date: 2026-08-15
- Status: Accepted and implemented
- Supersedes the live naming in: [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Current contract: [native verification owners](../../Specifications/Windvale-Native-Verification-Owners.md)

## Context

The manifest-driven suite began as evidence for removing .NET from `main`.
After that milestone closed and `v0.1.0` froze its recovery and release evidence,
the same manifest kept growing with compiler, OS, database, model, package, and
installer checks. Calling all of them retirement suites made a current owner
registry look like unfinished historical work. It also encouraged people to
confuse one affected development owner with complete release qualification.

At this decision the live manifest had 70 owners and 3,568 declared cases while
its copied documentation contained several stale totals.

## Decision

- Freeze .NET retirement as historical `v0.1.0` evidence. Do not expand that
  milestone when current native code changes.
- Rename the evolving manifest to `Tests/Native/Verification-Owners.txt` and its
  coordinator to `Test-Verification-Owners.cmd` / `.sh`.
- Keep the old coordinator paths as quiet compatibility aliases. They do not
  define the live plan and new references must not use them.
- Treat exact `--filter` selection as development evidence.
- Treat no-argument and `--shard` composition as explicit qualification, never
  as an automatic per-commit fallback.
- Keep one owner implementation shared by both modes. Separation of purpose
  must not duplicate tests or add another verification ladder.
- Keep the detailed inventory canonical in the digest-bound manifest. The
  contract records only checked totals and shard allocation rather than a
  second 70-row list.

## Consequences

Compiler and product work can add or update native owners without reopening the
.NET-retirement milestone. Ordinary changes still run only mapped owners.
Release qualification remains complete, paired-host, deliberate, and visibly
named in CI. Existing local scripts using the old command continue to work.

## Reconsideration triggers

Revisit the registry format if qualification needs host-specific ownership, if
four shards no longer balance the slowest host, or if a stable machine-readable
separation between development and release-only owners becomes necessary.
