# Native test runbook

This runbook owns the first .NET-free repository test slice accepted by
[Decision 0218](../Decisions/0218-First-Native-Test-Orchestration.md). Its exact
inventory and non-claims are defined by the
[native test-plan contract](../../Specifications/Windvale-Native-Test-Plan.md).

## Candidate commands

On Windows x64:

```bat
Tools\Native\Test-Seed.cmd
```

On Linux x64:

```sh
./Tools/Native/Test-Seed.sh
```

The command verifies the fixed test plan, builds both projects through the pinned
native source-to-WVB tools, compares their complete WVB identities, and executes
them through the pinned native runner. Success is:

```text
PASS  calls-control
PASS  scalar-core
Tests: 2, Passed: 2, Failed: 0
```

No .NET process is required by this command. The host dependencies are `cmd.exe`
and `certutil` on Windows, or Bash, `sha256sum`, `cmp`, and core utilities on Linux.

## Current boundary

This is a candidate scalar smoke gate, not the complete normal repository verifier.
Continue to select one appropriate Stage 0 verifier for changes outside these two
transferred fixtures. Do not run this candidate and progressively broader local
levels merely as a checklist; use the narrowest gate that owns the changed behavior.

For changes to the native plan, launchers, or its two projects, run the focused Seed
test named `native test orchestration builds and runs the pinned scalar plan` once.
GitHub owns the independent Windows and pinned-Debian Qualification run for a final
committed candidate.
