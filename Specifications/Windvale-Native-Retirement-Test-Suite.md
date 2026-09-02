# Windvale native retirement test suite (historical)

## Status

Archived milestone contract. The .NET-retirement result is frozen at the
immutable `v0.1.0` tag and its release evidence; it no longer grows when the
compiler, operating system, database, packages, or libraries gain tests.

The repository originally used `Tests/Native/Retirement-Suite.txt` and
`Test-Retirement-Suite.cmd` / `.sh` to compose the transferred native evidence.
Those exact historical files remain available from the tag and Git history.

## Current work

Ongoing checks are maintained as
[native verification owners](Windvale-Native-Verification-Owners.md).
Development selects affected owners; explicit release qualification composes
the current registry into paired-host shards. The redundant retirement command
aliases and paired verification-owner coordinators are absent from `main`; use
`pwsh -NoProfile -File Tools/Verify/Invoke-WindvaleTests.ps1` for the live
runner. The old paths remain available from the immutable `v0.1.0` tag and Git
history and must not be described as a new retirement claim.
