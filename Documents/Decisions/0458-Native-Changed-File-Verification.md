# Decision 0458: Native changed-file verification

- Status: Implemented Windows development front door; non-Windows dispatch qualification pending
- Date: 2026-08-09
- Advances: [Decision 0457](0457-Normal-Path-Dotnet-Audit.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native changed-file verification](../../Specifications/Windvale-Native-Changed-Verification.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0457 found one indirect normal dependency outside the four direct
managed verification files: `Verify-Changed.ps1` dispatched every
qualification-scoped implementation change to `Verify-Seed.ps1 -Level Fast`.
It also failed broad or unknown mappings to all managed Seed areas. That made
the recommended local front door a normal .NET dependency even though focused
native owners already exist for most maintained product boundaries.

Replacing that call with the unfiltered 3,147-case retirement coordinator would
violate the repository's verification rhythm and repeat long, unrelated loops.
Silently passing unknown paths would be worse because it would hide the exact
native evidence still missing.

## Decision

- Add a focused native planner that reads the canonical retirement-suite order,
  maps maintained boundaries to their owned suites, and deduplicates combined
  selections without changing manifest order.
- Model missing coverage as stable named gaps. Frozen managed compiler/runtime,
  object, assembler, linker, OS, and test changes report recovery-source gaps;
  database, GitHub qualification, unknown tools/specifications, and empty input
  report their own gaps.
- Never fall back to `.NET`, the managed Seed verifier, or the complete native
  coordinator. An unknown path is a request to add an owner, not permission to
  run everything.
- Make `Verify-Changed.ps1` run only planner verification and the selected
  filtered native suites. Preserve whitespace, editor, website, fail-fast, and
  optional timing-report behavior.
- Keep the paired `.cmd`/`.sh` suite commands as behavior owners. The PowerShell
  dispatcher chooses the host form; independent non-Windows execution remains
  part of the final host audit.

## Evidence and consequences

The focused planner test passes 27 existing general cases plus 13 new native
cases. The actual no-argument working-tree front door discovers all 13 task
paths, selects zero behavior suites, finds zero gaps,
and runs only plan/inventory verification. It does not load .NET.

This removes the sole indirect local managed dependency found by Decision 0457.
The four direct normal files remain: the paired broad Seed verifiers, the legacy
WebAssembly verifier, and the GitHub verification workflow. The explicit gap
set now supplies the authoritative work queue for evidence closure instead of
requiring a line-for-line port of the managed harness.

No broad Seed, OS, native retirement, WebAssembly, QEMU, Standard, Qualification,
or complete retirement gate ran. Existing passing process-object and Probe
evidence was not rerun because this slice changes only planning and dispatch.

## Reconsideration triggers

Change a mapping when suite ownership changes or a named gap gains a focused
native owner. Do not widen a common mapping merely to suppress a gap. Introduce
a versioned data manifest if the planner's focused mapping table becomes hard
to review as one cohesive source.
