# Decision 0924: use one PowerShell verification-owner runner

- Date: 2026-09-02
- Status: Accepted and implemented
- Refines: [Decision 0589: Separate historical retirement from live native verification](0589-Separate-Historical-Retirement-From-Live-Native-Verification.md)
- Current contract: [native verification owners](../../Specifications/Windvale-Native-Verification-Owners.md)

## Context

Native verification had one registry but two independently maintained top-level
coordinators: a Windows command file and a Linux shell script. Both parsed the
same owner inventory, repeated its byte size, digest, owner count, case total,
and shard totals, then implemented the same selection, streaming, summary, and
timing behavior separately. An intentional registry update therefore required
editing unrelated orchestration constants, and coordinator drift could fail
otherwise-correct code for reasons unrelated to the changed contract.

PowerShell 7 already owns changed-file verification on both development hosts.
The individual native owners still use host-specific command and shell scripts,
so replacing every test body at once would combine orchestration cleanup with a
much larger evidence migration.

## Decision

- Use `Tools/Verify/Invoke-WindvaleTests.ps1` as the sole cross-host entry point
  for named owners, qualification shards, and deliberate complete runs.
- Remove the paired `Test-Verification-Owners.cmd` and `.sh` coordinators.
- Validate the registry grammar, uniqueness, bounded numeric fields, four-shard
  coverage, current-host command files, and Linux executable modes at runtime.
  Calculate inventory totals from that validated source instead of duplicating
  mutable byte, digest, owner, case, and shard constants in coordinators.
- Preserve the existing bounded live-output helper, failure rules, exact terminal
  summary checks, and individual owner scripts during this transition.
- Install a pinned, checksum-verified PowerShell runtime in the pinned Debian
  qualification container so Windows and Linux invoke the same coordinator.
- Keep changed-file special development modes explicit until their leaf owners
  expose equivalent focused modes through the shared registry contract.

## Consequences

Registry growth no longer requires synchronized edits to two parsers and two
sets of copied totals. Local development and paired-host qualification select
owners through the same interface, while existing native test behavior remains
unchanged behind it. PowerShell and Node remain bootstrap dependencies for this
first framework slice; Node continues to provide bounded child-output streaming.

This decision does not claim that all test bodies are now PowerShell or
Windvale-native, and it does not make complete qualification an ordinary
per-change gate. Historical evidence continues to name the commands that
actually produced it.

## Reconsideration triggers

Revisit this boundary when Windvale can host the runner itself, when structured
owner-result records can replace terminal-summary parsing, or when a focused
development mode can be expressed without bypassing the registry entry point.
