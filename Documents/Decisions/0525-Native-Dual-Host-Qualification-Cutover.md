# Decision 0525: Native dual-host qualification cutover

- Status: Implemented candidate; first independent native matrix pending
- Date: 2026-08-12
- Advances: Decisions 0057, 0457, 0458, 0504, and 0524
- Scope: normal GitHub qualification orchestration on Windows and Linux

## Context

The normal source build, verification, inspection, execution, assembly,
lowering, linking, packaging, publication, website, and bootstrap front doors
no longer invoke .NET. After the paired WebAssembly owner closed, three files
still represented the normal managed verification dependency: the main GitHub
workflow and the paired broad Seed scripts.

The replacement evidence already has three different runtime profiles. The
fixed native retirement coordinator owns 45 suites and 3,206 cases; the
WebAssembly owner is a separate 23-to-27-minute Node.js engine workload; and
the compiler bootstrap owns native Stage 1/Stage 2 self-convergence. Serializing
all three profiles once per host would make the wall time approximately their
sum and would obscure which contract failed.

## Decision

For every qualification-scoped GitHub change, run one fail-closed native matrix
with six independent jobs:

1. execute the complete fixed retirement coordinator on Windows;
2. execute the same coordinator in the pinned Debian 12 container;
3. execute the complete native WebAssembly owner on Windows;
4. execute the same WebAssembly owner on Linux;
5. execute native compiler self-convergence on Windows; and
6. execute the same self-convergence contract in the pinned Debian 12
   container.

The fixed-suite jobs pin Node.js 24 for their random-containment owners. The
WebAssembly jobs independently pin Node.js 24 and use the host's PowerShell 7
to dispatch the paired native `.cmd` or `.sh` front doors. The bootstrap jobs
invoke only `Verify-Bootstrap.cmd` or `.sh`. All six jobs run in parallel and
the existing required `Verification gate` succeeds only if every selected job
succeeds. Lightweight and website classifications retain their smaller gates.

Remove the .NET environment and `actions/setup-dotnet` steps from normal GitHub
automation. Retain `Verify-Seed.ps1` and `.sh` as explicitly classified
recovery/differential commands until the final recovery archive owns their
source, dependency inventory, instructions, and exact evidence. No normal
workflow invokes them.

## Evidence boundary

Before publication, the direct-entry audit reported eleven direct managed entry
points: zero normal and eleven recovery. The final Stage 0 archive work later
removed the paired obsolete front-door rebuilders, leaving zero normal and nine
recovery entries. The audit independently discovers the current set, and the
workflow contains no `setup-dotnet`, `dotnet`, or `DOTNET_` entry.
Planner and inventory verification, workflow structure checks, changed-file
whitespace verification, and documentation review are the proportional local
checks for this orchestration-only change.

The first GitHub run from the exact committed workflow is the independent
Windows/Linux qualification evidence. Until all six jobs and the required gate
pass, this decision remains a candidate and does not promote T1, T2, G1, or the
complete retirement gate.

## Consequences

- Qualification becomes one user-visible workflow run whose independent
  profiles execute concurrently rather than one serial managed harness per
  host.
- A failed profile can be rerun or diagnosed at its exact owner; passing sibling
  profiles are not manually repeated.
- The normal GitHub path no longer downloads, installs, or invokes .NET.
- The managed broad Seed harness remains available only for named recovery or
  differential questions and does not define normal acceptance.
- Decision 0057 condition 8, the final digest-bound Stage 0 recovery archive,
  remains mandatory before complete retirement is declared.

## Reconsideration triggers

Revisit the job split if one profile begins sharing mutable evidence with
another, a job exceeds its bounded timeout, a host stops providing the required
PowerShell or Node runtime, or a new permanent normal product surface is not
owned by one of the selected contracts. Do not restore a managed normal gate to
mask a missing native owner.
