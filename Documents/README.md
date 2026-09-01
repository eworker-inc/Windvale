# Windvale documentation

> Status: Current documentation map
> Authority: Informative routing guide
> Last reviewed: 2026-08-31

Start with the smallest document that owns your question. Do not load the full
decision or evidence history for ordinary development.

## Fast reading path

1. Read the [root README](../README.md) for the public overview and working
   entry points.
2. Read [Progress](Project/Progress.md) for what works, what is missing, and the
   immediate next results.
3. Read [Roadmap](Project/Roadmap.md) only when the task depends on a forward
   product gate or workstream order.
4. Open the relevant [specification](../Specifications/README.md) for exact
   current behavior.
5. Open an architecture document when the task crosses an ownership or design
   boundary.
6. Open a runbook when you need commands or an operational procedure.
7. Read a decision for rationale and an evidence record for exact historical
   runs or artifact identities.

Repository documentation follows the
[documentation policy](Documentation-Policy.md). Work under this directory also
follows [Documents/AGENTS.md](AGENTS.md).

## Which document owns the answer?

| Question | Primary owner |
| --- | --- |
| What is Windvale and how do I try it? | [Root README](../README.md) |
| What works right now? | [Progress](Project/Progress.md) |
| What are we building next, and in what order? | [Roadmap](Project/Roadmap.md) |
| What behavior or binary format is required? | [Specifications](../Specifications/README.md) |
| Which component owns a responsibility? | [Architecture](Architecture/) |
| Why was a durable choice made? | [Decisions](Decisions/) |
| What exact run, host, measurement, or artifact was completed? | Evidence records under [Project](Project/) |
| How do I build, verify, publish, or recover something? | [Runbooks](Runbooks/) |
| What changed for release users? | [Changelog](../CHANGELOG.md) |

Progress is the only current-state dashboard. Roadmap owns future gates, not a
second activity diary. Specifications and architecture describe current
contracts and boundaries. Decisions and evidence are not current-state pages;
their explicit status controls how they may be used.

## Current project direction

- [Project vision](Project/Project-Vision.md) — purpose, assurance ambition,
  success principles, and honest non-goals.
- [Progress](Project/Progress.md) — concise current implementation and
  qualification snapshot.
- [Roadmap](Project/Roadmap.md) — direct Windvale 1.0 critical path and parallel
  OS work.
- [Windvale 1.0 product plan](Project/Windvale-1.0-Product-Plan.md) — complete
  release gate for Language, Libraries, WVDB, packages, services, support, and
  qualification.
- [Release names and tags](Project/Release-Names-And-Tags.md) — recovery,
  baseline, preview, and product tag meanings.

## Architecture entry points

- [Seed implementation](Architecture/Seed-Implementation.md) — current
  source-to-execution component ownership.
- [Platform and portability](Architecture/Platform-And-Portability.md) — shared
  contracts and explicit platform scope.
- [Language design](Architecture/Language-Design.md) — approachable source
  design and its safety boundaries.
- [Native execution and Stage 0 retirement](Architecture/Native-Execution-And-Dotnet-Retirement.md)
  — native destination, completed normal-path transition, and recovery role.
- [Windvale OS architecture](Architecture/Windvale-Os-Architecture.md) — boot,
  kernel, processes, services, and machine seams.
- [Product threat model](Architecture/Product-Threat-Model.md) — shipped trust
  boundaries, threats, and residual risks.
- [Packages, releases, and recovery](Architecture/Packages-Releases-And-Recovery.md)
  — immutable installation and recovery model.

Proposed architecture documents say so in their status. They are not current
implementation claims.

## Specification entry points

The [specification index](../Specifications/README.md) is the complete contract
catalog. Common starting points are:

- [Language 1.0](../Specifications/Windvale-Language-1.0.md),
  [grammar](../Specifications/Windvale-Language-1.0-Grammar.md), and
  [Foundation](../Specifications/Windvale-Language-1.0-Foundation.md);
- [Seed language](../Specifications/Seed-Language.md) and
  [Seed bytecode](../Specifications/Seed-Bytecode.md);
- [Windvale object format](../Specifications/Windvale-Object-Format.md),
  [assembly](../Specifications/Windvale-Assembly.md), and
  [linking](../Specifications/Windvale-Linking.md);
- [native verification owners](../Specifications/Windvale-Native-Verification-Owners.md);
- [Windvale OS boot environment](../Specifications/Windvale-Os-Boot-Environment.md)
  and [protected processes](../Specifications/Windvale-Protected-Process.md);
- [WVDB specification plan](Project/WVDB-1.0-Specification-Plan.md); and
- [browser playground](../Specifications/Browser-Playground.md).

## Development and operations

- [Seed development](Runbooks/Seed-Development.md) — ordinary development and
  focused verification rhythm.
- [Native source to WVB](Runbooks/Native-Source-To-Wvb.md) — build, verify,
  inspect, and run the native source path.
- [Native tests](Runbooks/Native-Tests.md) — focused owner and qualification
  execution.
- [Installer](Runbooks/Installer.md) — installer construction and checks.
- [Preview release ceremony](Runbooks/Preview-Release-Ceremony.md) — signed
  release procedure.
- [Stage 0 recovery](../Bootstrap/Stage0/README.md) — restore the immutable
  managed recovery release in a separate workspace.

## Status words

| Status | Meaning |
| --- | --- |
| Current | The document describes the active owner, contract, workflow, or plan. |
| Accepted | The decision or direction is approved. Implementation may still be incomplete. |
| Proposed | The material is open for review and must not be treated as accepted behavior. |
| Superseded | A later owner replaced the material; keep it only for history. |
| Historical | The document preserves a dated state, run, or rationale. |

Implemented, verified, qualified, and released are different evidence claims.
A document must state the narrowest claim it can support.

## Hashes and exact evidence

Current narrative pages do not copy raw artifact hashes. Exact identities live
in machine-readable manifests, signed release checksums, launchers, fixtures,
or named evidence records. See the
[hash ownership policy](Documentation-Policy.md#hash-ownership).

The large [Seed verification evidence](Project/Seed-Verification-Evidence.md)
and [Language 1.0 migration evidence](Project/Windvale-Language-1.0-Migration-Evidence.md)
records are append-oriented technical history. Read them only when a task needs
exact reproduction, qualification, or provenance detail.

## Historical catalogs and snapshots

- [Previous detailed documentation catalog](Documentation-Guide-History-2026-08-31.md)
- [Previous detailed progress snapshot](Project/Progress-History-2026-08-31.md)
- [Previous detailed roadmap and milestone audits](Project/Roadmap-History-2026-08-31.md)

These records remain searchable but are not default current context.
