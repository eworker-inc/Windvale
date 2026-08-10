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
- [Native publisher promoter applications](Decisions/0487-Native-WVHV-Publisher-Promoter-Applications.md) — separate non-circular durable publisher installation through paired role-aware native applications
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
- [Manifest-driven native retirement test suite](Decisions/0330-Manifest-Driven-Native-Retirement-Test-Suite.md) — one digest-bound direct coordinator, currently covering 40 native suites and 3,195 transferred cases
- [Fixed native assembler golden objects](Decisions/0429-Fixed-Native-Assembler-Golden-Objects.md) — three exact positive WVA products with repeated native assembly and independent WVO admission
- [Fixed native typed WVB rejections](Decisions/0430-Fixed-Native-Typed-Wvb-Rejections.md) — six compact single-byte typed/control/nominal mutations through both native readers
- [Compact native WVB rejection closure](Decisions/0431-Compact-Native-Wvb-Rejection-Closure.md) — four final compact stack, receiver, and nominal-kind mutations plus explicit ownership of excluded cases
- [Fixed native scalar-x64 golden object](Decisions/0432-Fixed-Native-Scalar-X64-Golden-Object.md) — exact immediate, multiply, shift, rotate, and indexed-memory source through repeated native assembly
- [Fixed native WVA positive matrix](Decisions/0433-Fixed-Native-Wva-Positive-Matrix.md) — every paired 8/16-bit register plus typed narrow immediate/shift groups in one compact exact corpus
- [Expanded native WVA positive matrix](Decisions/0434-Expanded-Native-Wva-Positive-Matrix.md) — every paired 32/64-bit register, condition code, label scope, and RIP-relative vector in the same compact owner
- [Digest-bound OS boot execution](Decisions/0435-Digest-Bound-Os-Boot-Execution.md) — supplied-image QEMU verification without Stage 0, with image construction retained explicitly as recovery
- [Windvale-native UEFI application construction](Decisions/0436-Windvale-Native-Uefi-Application-Construction.md) — portable canonical EFI construction, independent verification, and native Project 1 front doors
- [Native linker to UEFI packaging](Decisions/0437-Native-Linker-To-Uefi-Packaging.md) — real digest-bound flat-link output and entry evidence through a hosted Windvale EFI packager
- [Retained native UEFI packager containers](Decisions/0438-Retained-Native-Uefi-Packager-Containers.md) — paired native-built PE/ELF tools, digest-bound launchers, and a permanent three-case native lane
- [Native UEFI recovery-packaging cutover](Decisions/0439-Native-Uefi-Recovery-Packaging-Cutover.md) — real Probe 40 linked payload through the retained native packager, with managed object production/linking still explicit
- [Probe 40 object-inventory boundary](Decisions/0440-Probe-40-Object-Inventory-Boundary.md) — fourteen verified WVO containers expose the native-link transfer boundary
- [Scale-safe native Wv-Linker relocation emission](Decisions/0441-Scale-Safe-Native-Wv-Linker-Relocation-Emission.md) — resolves the measured 128 MiB arena blocker without changing link bytes or maps
- [Native Probe 40 recovery linking cutover](Decisions/0442-Native-Probe-40-Recovery-Linking-Cutover.md) — removes the managed linker from the normal recovery command while retaining Stage 0 object production
- [Native Probe 40 top-level WVA assembly](Decisions/0443-Native-Probe-40-Top-Level-Wva-Assembly.md) — moves three exact OS shim objects to the qualified native assembler and focuses inventory ownership
- [Native Probe 40 inner process-image WVA handoff](Decisions/0444-Native-Probe-40-Inner-Process-Wva-Handoff.md) — feeds four exact native-assembled WVOs into Stage 0 process-image composition while retaining its frozen fallback
- [Digest-bound native Probe 40 object seed](Decisions/0445-Digest-Bound-Native-Probe-40-Object-Seed.md) — constructs the exact normal EFI through native Windows/Linux launchers without invoking .NET while retaining explicit Stage 0 provenance
- [Native Probe 40 Windvale source producer](Decisions/0446-Native-Probe-40-Windvale-Source-Producer.md) — compiles and lowers the canonical native-probe source in the ordinary build and removes its exact WVO from the frozen seed
- [Native Probe 40 admission source producer](Decisions/0447-Native-Probe-40-Admission-Source-Producer.md) — adds a verified native WVO export rename, builds admission from Windvale source, and reduces the frozen seed to nine objects
- [Native Probe 40 exception object producer](Decisions/0448-Native-Probe-40-Exception-Object-Producer.md) — adds verified WVO construction, builds the x64 exception installer through Windvale-native tooling, and reduces the frozen seed to eight objects
- [Native Probe 40 admission bridge producer](Decisions/0449-Native-Probe-40-Admission-Bridge-Producer.md) — consolidates the focused object producer, builds the WVB admission bridge natively, and reduces the frozen seed to seven objects
- [Native Probe 40 native bridge and support producer](Decisions/0450-Native-Probe-40-Native-Bridge-And-Support-Producer.md) — adds the two-section bridge/support recipe to the focused producer and reduces the frozen seed to six objects
- [Native Probe 40 paging object producer](Decisions/0451-Native-Probe-40-Paging-Object-Producer.md) — builds the exact paging installer through the focused native producer and reduces the frozen seed to five objects
- [Native Probe 40 memory-object producer](Decisions/0452-Native-Probe-40-Memory-Object-Producer.md) — builds the normal memory object through a separate focused producer and reduces the frozen seed to four objects
- [Native Probe 40 loader-object producer](Decisions/0453-Native-Probe-40-Loader-Object-Producer.md) — rebuilds the normal loader object from a pinned architecture fixture and reduces the frozen seed to three objects
- [Native Probe 40 architecture-fault scenarios](Decisions/0489-Native-Probe-40-Architecture-Fault-Scenarios.md) — constructs byte-identical invalid-opcode and general-protection images through the ordinary .NET-free path
- [Native Probe 40 system-kernel target](Decisions/0454-Native-Probe-40-System-Kernel-Target.md) — compiles the canonical system source to WVB, lowers it through Windvale, and reduces the frozen seed to two objects
- [Native Probe 40 process-policy source path](Decisions/0455-Native-Probe-40-Process-Policy-Source-Path.md) — composes the general native builder, lowerer, and export renamer, reducing the frozen seed to one object
- [Fixed native linker hostile-input corpus](Decisions/0332-Fixed-Native-Linker-Hostile-Input-Corpus.md) — 200 immutable bounded inputs through exact native `WVL1002` containment
- [Fixed native console-container hostile-input corpus](Decisions/0334-Fixed-Native-Console-Container-Hostile-Input-Corpus.md) — 256 immutable bounded PE/ELF candidates through the native publisher with preservation and zero-scratch evidence
- [Fixed native WVO differential corpus](Decisions/0335-Fixed-Native-Wvo-Differential-Corpus.md) — 128 valid-shaped mutations and 128 arbitrary values agree with frozen Stage 0 acceptance through the native verifier
- [Fixed native WVA differential corpus](Decisions/0336-Fixed-Native-Wva-Differential-Corpus.md) — 200 exact seeded source mutations agree on acceptance, rejection code, and successful WVO bytes through the native assembler
- [Fixed native random-containment corpus](Decisions/0337-Fixed-Native-Random-Containment-Corpus.md) — the exact continued 2,000-value Stage 0 source/WVB/WVO sequence is frozen behind focused native lanes
- [Fixed native console-container mutations](Decisions/0338-Fixed-Native-Console-Container-Mutations.md) — 19 canonical PE/ELF mutations retain detailed Stage 0 provenance and fail safely through the native publisher
- [Fixed native WVO hostile size](Decisions/0339-Fixed-Native-Wvo-Hostile-Size.md) — verify, inspect, link, and publish contain the exact first-byte-over-4-MiB input at the native snapshot boundary
- [Windvale-native hosted-console admission](Decisions/0340-Windvale-Native-Hosted-Console-Admission.md) — portable format-2 PE/ELF admission plus two valid bases and thirteen fixed mutations through the native publisher
- [Fixed native console segmented-size rejections](Decisions/0341-Fixed-Native-Console-Segmented-Size-Rejections.md) — two exact larger-than-4-MiB two-chunk inputs through a pinned read-only Windvale verifier
- [Native segmented console-application construction](Decisions/0342-Native-Segmented-Console-Application-Construction.md) — maximum valid PE/ELF construction as bounded chunks plus an independently verified `WVCS 1.0` manifest
- [Native console-packager source reconstruction](Decisions/0343-Native-Console-Packager-Source-Reconstruction.md) — both console-packager WVBs rebuilt exactly through the digest-bound .NET-free Project 1 front door
- [Native console-packager WVO reconstruction](Decisions/0344-Native-Console-Packager-Wvo-Reconstruction.md) — both source-built packager closures lowered to exact WVO without widening the bounded native emitter
- [Native enum-service fragment reconstruction](Decisions/0408-Native-Enum-Service-Fragment-Reconstruction.md) — focused enum-metadata helpers let the pinned native lowerer and linker reproduce the exact service WVO and raw fragment without .NET
- [Native hosted segment iteration control](Decisions/0413-Native-Hosted-Segment-Iteration-Control.md) — admitted native count modes keep both bounded loops out of PowerShell/Bash binary parsing
- [Native segmented compiler toolset reconstruction](Decisions/0496-Native-Segmented-Compiler-Toolset-Reconstruction.md) — current-Windows-host native cross-target reconstruction of three exact WVBs and six paired applications, with retained-seed circularity explicit
- [Native WVB-to-WVO reconstruction](Decisions/0497-Native-Wvb-To-Wvo-Reconstruction.md) — current-Windows-host native reconstruction of the exact accepted-subset lowerer WVB, paired applications, and unchanged fixed vector through the retained segmented toolset
- [Native console-packager application reconstruction](Decisions/0498-Native-Console-Packager-Application-Reconstruction.md) — current-Windows-host native reconstruction of both ordinary and segmented paired application candidates through the retained hosted toolset
- [Native WVO publisher reconstruction](Decisions/0499-Native-Wvo-Publisher-Reconstruction.md) — current-Windows-host native cross-target reconstruction through an exact WVO oracle and role-3 publisher pipeline without target self-publication
- [Native WVO inspector reconstruction](Decisions/0500-Native-Wvo-Inspector-Reconstruction.md) — current-Windows-host native cross-target reconstruction of the profile-6 WVO inspector through retained compiler and hosted-container toolsets
- [Native Wv-Linker reconstruction](Decisions/0501-Native-Wv-Linker-Reconstruction.md) — current-Windows-host native cross-target reconstruction through a raw WVO oracle and distinct segmented image path without target self-linking
- [Native console-application-verifier reconstruction](Decisions/0502-Native-Console-Application-Verifier-Reconstruction.md) — current-Windows-host native cross-target reconstruction of the exact profile-7 two-snapshot verifier through retained compiler and hosted-construction toolsets
- [Native console-application-publisher reconstruction](Decisions/0503-Native-Console-Application-Publisher-Reconstruction.md) — current-Windows-host native cross-target reconstruction through a raw WVO oracle and role-4 publisher overlay without target self-publication
- [Native WebAssembly generation and verification](Decisions/0504-Native-WebAssembly-Generation-And-Verification.md) — complete current-Windows source/WVB-to-Wasm generation, strict engine verification, and probes without a normal .NET invocation
- [Native Seed front-door qualification smoke](Decisions/0505-Native-Seed-Front-Door-Qualification-Smoke.md) — five fixed native build/verify/inspect cases replacing nine managed calls in each broad Seed qualification script
- [Native Seed console AOT qualification smoke](Decisions/0506-Native-Seed-Console-Aot-Qualification-Smoke.md) — exact native lower/verify/link/package and current-host execution replacing two more managed calls in each broad Seed script
- [Native WVB-runner reconstruction](Decisions/0507-Native-Wvb-Runner-Reconstruction.md) — retained-WVB native lower/link and exact paired profile-5 construction with current-host execution evidence
- [Native Seed WVB execution qualification smoke](Decisions/0508-Native-Seed-Wvb-Execution-Qualification-Smoke.md) — three exact representative WVB executions transferred from each broad Seed script to the current native runner
- [Native WVB-runner source reconstruction and step reporting](Decisions/0509-Native-Wvb-Runner-Source-Reconstruction-And-Step-Reporting.md) — complete native source-to-WVB reconstruction, exact paired applications, and overall instruction reporting transferred from each broad Seed script
- [Native Foundation build, inspect, and execution transfer](Decisions/0510-Native-Foundation-Build-Inspect-And-Execution-Transfer.md) — component-local Foundation projects, exact native build/inspection, three supported demo executions, and fifteen more managed calls removed from each broad Seed script
- [Native service-source build and inspection transfer](Decisions/0511-Native-Service-Source-Build-And-Inspection-Transfer.md) — component-local native-stencil and runtime-service projects, exact native build/inspection, and fifteen more managed calls removed from each broad Seed script
- [Native I/O-service build and inspection transfer](Decisions/0512-Native-Io-Service-Build-And-Inspection-Transfer.md) — component-local output, file-output, and file-input projects, exact native build/inspection, and fourteen more managed calls removed from each broad Seed script
- [Native fixed-service and publication build/inspection transfer](Decisions/0513-Native-Fixed-Service-And-Publication-Build-Inspection-Transfer.md) — component-local fixed-service, enum-metadata, and publication projects, two genuine service-bundle aggregates, and twenty-three more managed calls removed from each broad Seed script
- [Native runtime-table build and inspection transfer](Decisions/0514-Native-Runtime-Table-Build-And-Inspection-Transfer.md) — component-local runtime-table, execution-context, entry, and byte-result-admission projects, exact native build/inspection, and twenty-four more managed calls removed from each broad Seed script
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
