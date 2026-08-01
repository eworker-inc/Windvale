# Windvale documentation guide

Windvale separates current status, enduring architecture, accepted decisions, executable specifications, historical evidence, and operational records. Start with the smallest source that answers the question; dated decisions and evidence should not be read as the current status page by themselves.

## Current project state

- [Progress](Project/Progress.md) — concise phase indicators and working paths
- [Roadmap](Project/Roadmap.md) — phase gates, detailed sequence, and current transfer
- [Project vision](Project/Project-Vision.md) — purpose, intended stack, success principles, and non-goals
- [Open questions](Project/Open-Questions.md) — unresolved choices only
- [Changelog](../CHANGELOG.md) — release-facing summary of accepted work

## Architecture

- [Seed implementation](Architecture/Seed-Implementation.md) — implemented Stage 0, compiler, bytecode, runtime, object, assembler, linker, and native ownership map
- [Platform and portability](Architecture/Platform-And-Portability.md) — portable, hosted, system, and platform boundaries
- [Compiler bootstrap options](Architecture/Compiler-Bootstrap-Options.md) — bootstrap sequence and representation choices
- [Native execution and .NET retirement](Architecture/Native-Execution-And-Dotnet-Retirement.md) — interpreter/JIT/AOT destination and retirement gate
- [Seed verification throughput](Architecture/Seed-Verification-Throughput.md) — performance evidence and verification strategy

## Contracts and decisions

- [Specification index](../Specifications/README.md) — current language, format, compiler, runtime, tool, native, and OS contracts
- [Accepted decisions](Decisions/) — dated architecture and policy records; later decisions can amend earlier ones
- [Agent handbook](../AGENTS.md) — durable contribution and verification rules for people and AI agents

## Evidence and operations

- [Seed verification evidence](Project/Seed-Verification-Evidence.md) — exact cross-host qualification history and artifact identities
- [GitHub publication runbook](Project/GitHub-Publication-Runbook.md) — completed visibility procedure and remaining baseline follow-up
- [Bootstrap attribution migration](Project/Bootstrap-Attribution-Migration.md) — completed one-time identity-normalization evidence
- [Earliest-known claim evidence](Project/Earliest-Known-Claim-Evidence.md) — dated scope and comparison record for the project claim

Accepted decisions and qualification evidence are intentionally historical and cumulative. Update the progress page, roadmap, architecture, and specifications when the current contract changes; do not rewrite an accepted decision to make it look current.
