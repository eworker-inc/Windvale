# Windvale documentation guide

Windvale separates current status, enduring architecture, accepted decisions, executable specifications, historical evidence, and operational records. Start with the smallest source that answers the question; dated decisions and evidence should not be read as the current status page by themselves.

## Where updates belong

Each kind of information has one primary home. Link to that source instead of copying a changing status paragraph into several documents.

| Information | Primary home | Update when |
| --- | --- | --- |
| Stable public introduction and working entry points | [Root README](../README.md) | The project's purpose, supported entry points, or top-level navigation changes |
| Current implemented and qualified state, immediate transfer, and working paths | [Progress](Project/Progress.md) | The measured project state changes |
| Forward phase gates, sequencing, and next deliverables | [Roadmap](Project/Roadmap.md) | The route, phase gate, or intended order changes |
| Current normative behavior and formats | [Specifications](../Specifications/README.md) | A contract changes |
| Durable component ownership and design boundaries | [Architecture](Architecture/) | An enduring boundary changes |
| Accepted rationale and consequential choices | [Decisions](Decisions/) | A decision is accepted, superseded, or materially amended |
| Exact completed qualification runs and artifact identities | [Seed verification evidence](Project/Seed-Verification-Evidence.md) | New reproducible evidence completes |
| Release-facing summary of accepted changes | [Changelog](../CHANGELOG.md) | Accepted work should be visible to release readers |

The Progress page is the single current-state dashboard. The Roadmap can report whether a gate is open or complete, but it should not become a second activity diary. Evidence is historical and append-oriented; specifications and architecture describe the current contract rather than daily progress.

## Current project state

- [Progress](Project/Progress.md) — authoritative current-state snapshot, immediate measured transfer, and working paths
- [Roadmap](Project/Roadmap.md) — forward phase gates, detailed sequence, and next deliverables
- [.NET retirement inventory](Project/Dotnet-Retirement-Inventory.md) — enforced direct-entry-point inventory, product-surface ledger, and retained recovery owners
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
- [Stage 0 semantic freeze and native front door](Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md) — qualified WVB 1.11 semantic freeze and ordinary native project source-to-WVB path, with Stage 0 retained as the explicit recovery/differential lane
- [Exact native WVB publication step](Decisions/0214-Exact-Native-Wvb-Publication-Step.md) — first cross-host-qualified native publisher profile, shared verify-snapshot-and-replace logic, and remaining extended fault/concurrency hardening
- [Fixed native staged-WVO publication](Decisions/0299-Fixed-Native-Staged-Wvo-Publication.md) — bounded multi-snapshot admission, native file-identity checks, and fixed Windows/Linux sibling-and-replace candidates
- [Native staged-WVO producer/publisher composition](Decisions/0300-Native-Staged-Wvo-Producer-Publisher-Composition.md) — exact native staging-producer packages and a current-host .NET-free two-process publication path
- [Digest-bound native WVO candidate launchers](Decisions/0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md) — pinned Windows/Linux object verification and inspection entry points awaiting grouped promotion
- [Digest-bound native WVO linker candidate](Decisions/0302-Digest-Bound-Native-Wvo-Linker-Candidate.md) — clean exact linker artifacts and a pinned current-host entry point awaiting grouped promotion
- [Digest-bound native console-packager candidate](Decisions/0303-Digest-Bound-Native-Console-Packager-Candidate.md) — exact bounded PE/ELF materializer packages and candidate launchers with Stage 0 construction still explicit
- [Digest-bound native WVB-to-WVO candidate](Decisions/0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md) — current accepted-subset lowerer packages, fixed vector, and candidate launchers without a managed runtime
- [Digest-bound native AOT-chain test](Decisions/0305-Digest-Bound-Native-Aot-Chain-Test.md) — permanent fixed-vector source-to-executable verification through pinned native tools and result 42
- [Native console-application publication](Decisions/0307-Native-Console-Application-Publication.md) — portable PE/ELF admission plus reused native atomic replacement for digest-bound packaging
- [Native WVO publication](Decisions/0308-Native-Wvo-Publication.md) — shared portable WVO admission plus reused native atomic replacement for accepted-subset lowering
- [Fixed native WVO test cases](Decisions/0310-Fixed-Native-Wvo-Test-Cases.md) — one accepted object and three structural rejections in the digest-bound .NET-free native plan
- [Fixed native linker rejections](Decisions/0311-Fixed-Native-Linker-Rejections.md) — initial invalid-base, missing-entry, and malformed-object reports with output preservation
- [Fixed native console-packager rejections](Decisions/0313-Fixed-Native-Console-Packager-Rejections.md) — exact entry and empty-image rejection reports with destination preservation
- [Fixed native publisher rejections](Decisions/0314-Fixed-Native-Publisher-Rejections.md) — invalid console-application and WVO admission with destination preservation and zero scratch
- [Fixed native WVB-to-WVO rejections](Decisions/0317-Fixed-Native-Wvb-To-Wvo-Rejections.md) — malformed and valid-but-unsupported lowerer failures with exact reports and destination preservation
- [Fixed native WVA assembler rejection families](Decisions/0321-Fixed-Native-Wva-Assembler-Rejection-Families.md) — one exact output-preserving case for every stable WVA diagnostic family
- [Fixed native WVO read-only rejection families](Decisions/0322-Fixed-Native-Wvo-Read-Only-Rejection-Families.md) — identical exact verifier and inspector reports for all thirteen stable WVO status families
- [Expanded native linker rejection families](Decisions/0325-Expanded-Native-Linker-Rejection-Families.md) — exact output-preserving evidence for every externally driven `WVL1001` through `WVL1010` family
- [Fixed native linker map limit](Decisions/0327-Fixed-Native-Linker-Map-Limit.md) — compact generated-fixture evidence for exact `WVL1012` rejection at 16,384 definitions
- [Fixed native WVB unsafe rejections](Decisions/0329-Fixed-Native-Wvb-Unsafe-Rejections.md) — five immutable unsafe instruction-stream cases through both native read-only launchers
- [Manifest-driven native retirement test suite](Decisions/0330-Manifest-Driven-Native-Retirement-Test-Suite.md) — one digest-bound direct coordinator, currently covering 20 native suites and 3,024 transferred cases
- [Fixed native linker hostile-input corpus](Decisions/0332-Fixed-Native-Linker-Hostile-Input-Corpus.md) — 200 immutable bounded inputs through exact native `WVL1002` containment
- [Fixed native console-container hostile-input corpus](Decisions/0334-Fixed-Native-Console-Container-Hostile-Input-Corpus.md) — 256 immutable bounded PE/ELF candidates through the native publisher with preservation and zero-scratch evidence
- [Fixed native WVO differential corpus](Decisions/0335-Fixed-Native-Wvo-Differential-Corpus.md) — 128 valid-shaped mutations and 128 arbitrary values agree with frozen Stage 0 acceptance through the native verifier
- [Fixed native WVA differential corpus](Decisions/0336-Fixed-Native-Wva-Differential-Corpus.md) — 200 exact seeded source mutations agree on acceptance, rejection code, and successful WVO bytes through the native assembler
- [Fixed native random-containment corpus](Decisions/0337-Fixed-Native-Random-Containment-Corpus.md) — the exact continued 2,000-value Stage 0 source/WVB/WVO sequence is frozen behind focused native lanes
- [Fixed native console-container mutations](Decisions/0338-Fixed-Native-Console-Container-Mutations.md) — 19 canonical PE/ELF mutations retain detailed Stage 0 provenance and fail safely through the native publisher
- [Fixed native WVO hostile size](Decisions/0339-Fixed-Native-Wvo-Hostile-Size.md) — verify, inspect, link, and publish contain the exact first-byte-over-4-MiB input at the native snapshot boundary
- [Windvale-native hosted-console admission](Decisions/0340-Windvale-Native-Hosted-Console-Admission.md) — portable format-2 PE/ELF admission plus two valid bases and thirteen fixed mutations through the native publisher
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
- [Native source-to-WVB runbook](Runbooks/Native-Source-To-Wvb.md) — ordinary no-.NET project build, pinned artifact verification, atomic publication, limits, and Stage 0 reconstruction
- [Progress comic publishing runbook](Runbooks/Progress-Comic-Publishing.md) — local asset ownership, responsive exports, homepage accessibility, archive direction, review, and publication
- [Contributing guide](../CONTRIBUTING.md) — CLA acceptance, DCO sign-off, provenance, and pull-request requirements
- [Website guide](../Website/README.md) — local preview, featured progress stories, social and favicon assets, support configuration, analytics, and publication

## Evidence and operations

- [Seed verification evidence](Project/Seed-Verification-Evidence.md) — exact cross-host qualification history and artifact identities
- [GitHub publication runbook](Project/GitHub-Publication-Runbook.md) — completed visibility procedure and remaining baseline follow-up
- [Bootstrap attribution migration](Project/Bootstrap-Attribution-Migration.md) — completed one-time identity-normalization evidence
- [Earliest-known claim evidence](Project/Earliest-Known-Claim-Evidence.md) — dated scope and comparison record for the project claim

Accepted decisions and qualification evidence are intentionally historical and cumulative. Put current-state changes in Progress, route changes in the Roadmap, durable boundaries in Architecture, and normative behavior in Specifications. Do not duplicate the same progress narrative across those documents or rewrite an accepted decision to make it look current.
