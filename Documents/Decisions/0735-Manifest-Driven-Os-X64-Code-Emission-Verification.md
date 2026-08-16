# Decision 0735: Manifest-driven OS x64 code-emission verification

- Date: 2026-08-16
- Status: Implemented with complete Windows execution evidence; independent Linux execution pending
- Extends: [Decision 0733](0733-Target-Aware-Os-X64-Code-Emission-Development-Verification.md)
- Preserves: the 56-project, 336-case complete owner and six-check focused targets

## Context

The paired OS x64 code-emission owners repeated the same build, lower, link,
package, execute, size, and SHA-256 operations for every project. The Windows
owner contained 1,494 lines and the Linux owner contained 917 lines. Adding a
target required copying evidence into both scripts as well as maintaining the
development-target inventory, which made host drift and partial updates likely.

The audit found one such drift: four Linux entries packaged and hashed their
ELF containers but did not execute them, although the owner contract reported
six checks and current-host execution for every target. Adjacent entries and
the Windows owner did execute those programs.

## Decision

- Version the canonical OS x64 code-emission target manifest at version 2.
- Store one row per target containing the project, artifact stem, expected local
  result, exact byte size and SHA-256 for WVB, WVO, linked image, Windows
  container, and Linux container, followed by the complete Project 2 source
  closure.
- Make each host owner a generic executor over those rows. Windows packages and
  executes PE before checking the ELF container; Linux packages and executes
  ELF before checking the PE container.
- Preserve `--development-target <target>`, unknown-target exit code 64, the
  focused six-check summary, and the complete 56-project, 336-case summary.
- Treat a manifest change like an owner-script change: run planner verification
  and the complete owner rather than trusting focused evidence derived from the
  same changed data.
- Require the planner verifier to validate the manifest version, field grammar,
  unique targets, projects, and artifacts, sequential expected results, existing
  inputs, exact project declarations, and the presence of one generic command
  pipeline in each paired owner.
- Execute every Linux target locally, including the four entries whose repeated
  bodies had omitted execution. This closes an implementation gap in the stated
  six-check contract rather than adding a seventh check.

## Evidence

Planner verification passes 24 general and 163 native cases. On the measured
Windows host, the first, middle, and final focused targets passed, unknown-target
selection returned 64, and Git Bash accepted the Linux script syntax. The
complete Windows owner passed all 56 projects and 336 checks in 129,638 ms with
the unchanged registered summary.

The paired owner implementation is now 263 lines instead of 2,411 lines, an
89.09 percent reduction of repeated script code. These counts and timings are
diagnostic maintenance evidence, not portable performance thresholds. Linux
syntax evidence does not substitute for independent Linux behavior execution.

## Consequences

One manifest row is now the source of truth for a target's dependency closure
and exact output evidence. New targets no longer require two copied command
bodies, and paired-host operation order is reviewable in two small executors.
The complete owner still rebuilds all projects.
[Decision 0736](0736-Reuse-Os-X64-Verification-Trust-Checks.md) subsequently
removes repeated trust and workspace checks around those builds without changing
the manifest or evidence boundary.

The manifest is wider and therefore requires strict validation. A manifest edit
selects planner verification and the complete owner, and the planner verifier
rejects malformed inventory before qualification.

## Reconsideration triggers

Reconsider this decision if the hosts require materially different case data,
manifest parsing becomes a measurable part of focused execution, a target cannot
express its evidence in the current row grammar, or a batched compiler interface
can consume several rows without weakening per-target diagnostics and hashes.
