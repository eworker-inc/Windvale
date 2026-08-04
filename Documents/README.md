# Windvale documentation guide

Windvale separates current status, enduring architecture, accepted decisions, executable specifications, historical evidence, and operational records. Start with the smallest source that answers the question; dated decisions and evidence should not be read as the current status page by themselves.

## Current project state

- [Progress](Project/Progress.md) — concise phase indicators and working paths
- [Roadmap](Project/Roadmap.md) — phase gates, detailed sequence, and current transfer
- [Project vision](Project/Project-Vision.md) — purpose, intended stack, success principles, and non-goals
- [Open questions](Project/Open-Questions.md) — unresolved choices only
- [Windvale Database proposal](Project/Windvale-Database-Proposal.md) — EWDB-informed engine/application/service split, language-readiness boundaries, performance-transfer rules, and the implemented Stage 1 read-only experiment; no durable format or product direction is accepted
- [Accepted product-direction set](Decisions/0178-Project-Stewardship-Archives-And-Recovery.md) — stewardship and recovery, followed by linked language, runtime/toolchain, OS, browser, and product-lifecycle decisions 0179 through 0183
- [First timer-preemption proof](Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md) — cross-host-qualified HPET/local-APIC mechanics and bounded three-root preemption evidence
- [Accepted console, shell, and CLI direction](Decisions/0191-Windvale-Console-Shell-And-Cli-Architecture.md) — future device/terminal/shell/application split, explicit launch and stream bindings, small command language, and staged delivery
- [Accepted network-stack direction](Decisions/0192-Capability-Oriented-User-Space-Network-Stack.md) — future user-space protocol service, isolated NIC driver, semantic network capabilities, standards-based dual-stack path, and staged deterministic qualification
- [Accepted remote-terminal protocol direction](Decisions/0193-Simple-Windvale-Remote-Terminal-Protocol.md) — one authenticated secure connection, one bounded terminal session, typed control messages, explicit authority, and connection-owned teardown
- [First generation-safe non-tail memory objects](Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md) — cross-host-qualified Probe-40 bitmap/owner/object policy and WVA reclamation proof
- [Proposed next integrated defaults](Decisions/0198-Next-Integrated-Architecture-Defaults.md) — successor review set for resource domains, launch, streams, network devices, identity/trust, packages/releases, and language value contracts; not accepted or implemented
- [WebAssembly playground exploration](Project/WebAssembly-Playground-Exploration.md) — implemented Stage 0 host plus a Windvale-authored bounded metered-control-flow/direct-call backend, possible permanent browser target, constraints, and remaining decisions
- [Changelog](../CHANGELOG.md) — release-facing summary of accepted work

## Architecture

- [Seed implementation](Architecture/Seed-Implementation.md) — implemented Stage 0, compiler, bytecode, runtime, object, assembler, linker, and native ownership map
- [Language design](Architecture/Language-Design.md) — accepted future syntax, module, result, collection, resource-lifetime, and operator direction; distinct from implemented Seed
- [Platform and portability](Architecture/Platform-And-Portability.md) — per-part platform scope, authority, capability-provider, library, filesystem, and VM-provider boundaries
- [Windvale OS architecture](Architecture/Windvale-Os-Architecture.md) — durable kernel, process, capability, service, virtualization/accelerator, language-ownership, and bootstrap boundaries
- [Memory objects and resource domains](Architecture/Memory-Objects-And-Resource-Domains.md) — proposed physical-page ownership, memory-object, mapping, accounting, zeroing, and teardown defaults
- [Process launch and supervision](Architecture/Process-Launch-And-Supervision.md) — proposed semantic/kernel launch plans, atomic clean spawn, transfer, completion, and bounded restart defaults
- [Console, shell, and CLI architecture](Architecture/Console-Shell-And-Cli.md) — future terminal, session, command resolution, clean launch, standard-stream, pipeline, shell-language, and administration boundaries
- [Network-stack architecture](Architecture/Network-Stack.md) — future kernel/driver/protocol boundary, application capabilities, Internet protocols, packet data plane, virtio-net, security, testing, and delivery sequence
- [Remote-terminal protocol architecture](Architecture/Remote-Terminal-Protocol.md) — future `WVTS/1` secure carrier, authentication, authorization, framing, terminal messages, limits, lifecycle, compatibility adapters, and qualification
- [Identity, time, entropy, and trust](Architecture/Identity-Time-Entropy-And-Trust.md) — proposed provider separation, key custody, pinned mutual identity, authorization, trust generations, and secure-stream prerequisites
- [Packages, releases, updates, and recovery](Architecture/Packages-Releases-And-Recovery.md) — proposed package/lock/bundle/release split, immutable installation generations, 0.1 gate, and later A/B updates
- [Compiler bootstrap options](Architecture/Compiler-Bootstrap-Options.md) — bootstrap sequence and representation choices
- [Native execution and .NET retirement](Architecture/Native-Execution-And-Dotnet-Retirement.md) — interpreter/JIT/AOT destination and retirement gate
- [Exact compiler Stage 0 recovery](Runbooks/Exact-Compiler-Recovery.md) — clean-checkout reconstruction, identities, and archive provenance
- [Seed verification throughput](Architecture/Seed-Verification-Throughput.md) — performance evidence and verification strategy

## Contracts and decisions

- [Specification index](../Specifications/README.md) — current language, format, compiler, runtime, tool, native, and OS contracts
- [Accepted decisions](Decisions/) — dated architecture and policy records; later decisions can amend earlier ones
- [Agent handbook](../AGENTS.md) — durable contribution and verification rules for people and AI agents

## Development and operations

- [Seed development runbook](Runbooks/Seed-Development.md) — prerequisites, verification tiers, bootstrap convergence, CLI examples, assembly, and linking
- [Contributing guide](../CONTRIBUTING.md) — CLA acceptance, DCO sign-off, provenance, and pull-request requirements
- [Website guide](../Website/README.md) — local preview, progress data, support configuration, analytics, and publication

## Evidence and operations

- [Seed verification evidence](Project/Seed-Verification-Evidence.md) — exact cross-host qualification history and artifact identities
- [GitHub publication runbook](Project/GitHub-Publication-Runbook.md) — completed visibility procedure and remaining baseline follow-up
- [Bootstrap attribution migration](Project/Bootstrap-Attribution-Migration.md) — completed one-time identity-normalization evidence
- [Earliest-known claim evidence](Project/Earliest-Known-Claim-Evidence.md) — dated scope and comparison record for the project claim

Accepted decisions and qualification evidence are intentionally historical and cumulative. Update the progress page, roadmap, architecture, and specifications when the current contract changes; do not rewrite an accepted decision to make it look current.
