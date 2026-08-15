# Windvale development roadmap

> Status: Active dependency-based forward plan, rebaselined after completed
> Milestones 1 through 3 and the signed `v0.1.0` preview on 15 August 2026.

## Current planning boundary

Preserve the completed 0.1 product path while implementing Milestone 4's offline
package lifecycle. OS-1 continues as an independent launch-and-service track;
database work may also advance independently. Neither becomes a package-lifecycle
dependency merely because the work overlaps in time.

Windows and Linux remain permanent hosts. Windvale OS remains the vertical
integration target. Portable WVB remains the shared distribution contract, and
native execution remains a derived form over the same verified semantics.

## How to read this roadmap

This file contains forward milestones, their dependency order, completion
gates, and the standing of already-earned evidence. It is not an implementation
diary or a numbered maturity ladder.

- [Progress.md](Progress.md) records current measured state.
- [Seed-Verification-Evidence.md](Seed-Verification-Evidence.md) records exact
  historical qualification evidence.
- `Documents/Decisions/` records accepted rationale and supersession.
- `CHANGELOG.md` records notable implementation changes.

Completed migration detail remains available through Decisions 0057, 0178,
0213, 0525, and 0526 and Git history. It is not repeated here.

The 14 August 2026 rebaseline preserves every accepted gate while correcting
the earlier implied sequence. Former Milestone 4 is now product Milestone 3
because it depends on Milestone 2 but not on the former Milestone 3. The former
Milestone 3 is now parallel track OS-1. Historical decisions and commits keep
their original milestone wording; this mapping prevents renumbering history.

## Completed foundation

| Boundary | Standing |
| --- | --- |
| Seed language, WVB, verifier, runtime, object model, assembler, and linker foundation | Qualified on Windows and Linux. |
| Windvale-written compiler | Qualified Stage 1/Stage 2 convergence from the committed twelve-module source inventory. |
| Shared accepted-subset native backend | Qualified interpreter, AOT, baseline-JIT, object, link, package, and execution evidence for its documented profiles. |
| Native-only host repository | Qualified normal workflow under [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md); managed source archived outside `main` under [Decision 0558](../Decisions/0558-Archive-Managed-Stage0-Outside-Main.md). |
| Windvale OS vertical proof | Qualified through Probe 40, including protected processes, capability-mediated IPC, services, bounded preemption evidence, and generation-safe non-tail memory objects. |
| Static WebAssembly playground | The normal browser build and native generation/verification route are .NET-free; WebAssembly remains an interoperability lane rather than a permanent-platform commitment. |

Completion means that later work preserves the named contract. It does not
mean that every future language feature, optimizer, package service, database
operation, device, or operating-system mechanism already exists.

## Milestone 1: predictable development feedback — complete

### Outcome

Ordinary work receives focused feedback quickly enough that verification does
not dominate implementation.

### Current baseline

[Decision 0557](../Decisions/0557-Separate-Development-Verification-From-Qualification.md)
separates affected-owner development checks from complete qualification.
[Decisions 0553 through 0555](../Decisions/0555-Content-Addressed-Project-Wvb-Development-Checkpoints.md)
add content-addressed development checkpoints for the current database path.
[Decision 0559](../Decisions/0559-Checkpoint-Portable-Database-Development-Targets.md)
extends those checkpoints across all six portable tree targets.
[Decision 0560](../Decisions/0560-Linked-Image-Development-Checkpoints.md)
adds exact linked-image/map checkpoints. The measured direct all-hit eight-case
Windows owner is 87,800 ms, down from 402,638 ms, and reports live target plus
phase progress. Complete qualification remains cold and explicit.

The managed-source archival audit measured the old `seed-native-front-door` at
733,980 ms because it reconstructed 105 artifacts. The ordinary owner now binds
all 18 pinned front-door identities and admits all six WVB modules in 13,900 ms
on the same Windows host. The full 185-assertion reconstruction remains a
separately named explicit-qualification owner.

The WebAssembly development engine checkpoint now consumes the pinned browser
package directly and completes in 29,674 ms on the measured Windows host. The
last recorded complete construction-and-engine owner took 1,619,500 ms and
remains cold, cache-independent explicit qualification evidence.

One canonical declaration now records the source, producer, and artifact
closures for those three measured owners plus all five database checkpoint
families. Its verifier rejects missing, linked, noncanonical, duplicate,
unsorted, planner-gapped, or owner-disconnected entries.

GitHub [Verify run 31852544894, attempt 2](https://github.com/eworker-inc/Windvale/actions/runs/31852544894/attempts/2)
restored the separately populated Windows and Linux checkpoint directories and
passed the same 12-path affected-owner plan. The complete Windows development
job took 1 minute 42 seconds and Linux took 1 minute 15 seconds, including
checkout, Node setup, cache restoration, planner/workflow checks, all eight
behaviors, and post-job cleanup. The affected-owner steps took 66 and 49 seconds;
their database owners reported 57,870 and 43,000 ms respectively. Qualification
jobs remained skipped.

### Completion gate

1. A repeated affected-owner local run normally completes within two minutes
   when its declared products are unchanged.
2. Ordinary pull-request feedback runs only affected owners and normally
   completes within five minutes, excluding runner queueing.
3. Every expensive owner reports phase timings and has one declared source,
   producer, and artifact dependency closure.
4. Development cache hits revalidate identities and behavior; qualification
   ignores development cache state.
5. Complete dual-host qualification runs only for an explicit release,
   promotion, bootstrap, security, ABI, or conformance claim.

These are workflow targets, not semantic limits. A security or malformed-input
owner may remain slower when its complete boundary cannot be divided honestly.

### Current gate audit

| Gate | Standing | Evidence |
| --- | :---: | --- |
| Repeated affected owner under two minutes | ✅ | The direct all-hit Windows database owner passes all eight behaviors in 87,800 ms; the coherent changed-file invocation reports 89,530 ms. |
| Ordinary pull-request feedback under five minutes | ✅ | The exact warm dual-host workflow takes 1m42s on Windows and 1m15s on Linux end to end. |
| Phase timings and declared dependency closure | ✅ | The measured owners report phases; the canonical 33-entry declaration verifies three owner closures and five checkpoint families against the planner. |
| Cache hits revalidate identities and behavior | ✅ | Implemented checkpoints validate manifests, sizes, digests, structural admission where required, and rerun affected behavior. |
| Complete qualification is explicit only | ✅ | Decision 0557 reserves the cold dual-host matrix for deliberate qualification points. |

Milestone 1 is closed. Ordinary development now has a measured bounded path;
Milestone 2 subsequently closed the package-backed application, and Milestone 3
closed with the signed `v0.1.0` preview. Full qualification remains a separate
deliberate selection for later releases and promotions.

## Completed milestones and active track

| Work | Standing | Dependency |
| --- | :---: | --- |
| Milestone 1: predictable development feedback | ✅ Complete | Closed foundation; preserve its bounded workflow. |
| Native-only baseline promotion checkpoint | ✅ Qualified state / optional tag | The `v0.1.0` commit passed explicit post-archive dual-host qualification; a second checkpoint tag is optional. |
| Milestone 2: package-backed host application | ✅ Complete | Both permanent owners pass on Windows and Linux under Decision 0561. |
| Milestone 3: Windvale 0.1 preview | ✅ Complete | The signed `v0.1.0` tag and immutable preview release retain the final installers, envelope, and paired qualification. |
| Milestone 4: offline package lifecycle | 🎯 Current | Generation/Activation semantics, durable host publication/recovery, and two real source-package candidates exist; the shared offline directory, command dispatch, and end-to-end evidence remain. |
| Parallel track OS-1: launch and service composition | 🔵 Ongoing | Shares portable contracts where useful and remains independent of the completed host preview. |

The completed 0.1 product path was:

`Windows/Linux installers and core-tool packages → release envelope → one final release qualification → v0.1.0`

A completed focused gate remains complete while its relevant inputs and contract
stay unchanged. Later integrated evidence may consume that result; commits and
pushes do not by themselves require it to be rerun.

## Milestone 4: offline package lifecycle — active

[Decision 0568](../Decisions/0568-Offline-Package-Lifecycle-And-Generation-Activation-1.md)
selects installer/tool users as the primary audience and an offline
install/activate/recover/rollback demonstration as the finish. Networking, new
language semantics, and broader host authority are excluded. Completion is a
milestone checkpoint and does not publish `v0.2.0`.

### Selection record

| Choice | Advantages | Costs and risks | Standing |
| --- | --- | --- | --- |
| **Offline package lifecycle — selected** | Makes installation maintainable; gives `wv install`, generations, recovery, and rollback a real product boundary; reuses the completed 0.1 trust and installer work; remains testable without public infrastructure. | Requires generalizing the bounded package/store path and designing durable activation carefully; does not by itself advance Windvale OS. | Current Milestone 4. |
| Complete OS-1 composition | Advances Windvale's defining vertical-integration goal and closes already-started launch, provider, and supervision contracts. | Larger integration/debugging surface and less immediate value to installer users. | Continue as parallel OS-1 work. |
| Durable application/database increment | Produces another useful workload and can select the smallest necessary language/library improvements. | Can become an open-ended database project without a sharply bounded user scenario. | Continue independently when a named workload supplies the gate. |

### Completion gate

1. One offline release directory admits at least two real packages and their
   exact lock, bundle, approval, launch, target, and command identities.
2. Windows and Linux publish the same logical immutable Generation 1 inventory.
3. Activation publishes one validated Activation 1 record atomically and command
   dispatch observes either the complete old or complete new generation.
4. Recovery handles an interrupted pre-publication candidate without guessing or
   changing the active generation.
5. Rollback increments the activation serial and selects the retained previous
   generation without rewriting content or lowering release freshness policy.
6. Denied, unavailable, corrupt, stale, overflow, and indeterminate cases fail
   explicitly; uninstall preserves separately owned application data.

### Current slice

The portable Windvale implementation parses bounded Generation 1 and Activation
1 records, validates ordered package/command closure, and plans idempotent
activation and rollback. The first host adapter now compare-publishes exact
caller-validated Activation 1 bytes, recovers an interrupted digest-named
candidate without changing the public record, rejects stale state, and publishes
rollback on both host paths without changing `v0.1.0`. WVDB Query and the WVB
inspector now rebuild into two exact admitted bundles that coexist in one
immutable store and share only the canonical license object. Both have exact
portable approvals; the inspector still needs measured target launch records.
Release-envelope inventory, complete launch closure, active-generation command
dispatch, and the complete installation demonstration remain open.

## Milestone 2: package-backed host application — complete

### Outcome

One useful application builds, verifies, installs, inspects, and runs from
immutable package inputs on Windows and Linux without .NET.

The selected application is the WVDB Query path from
[Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md).
It composes portable decimal/database code with a rights-limited hosted storage
or directory provider. Database and package work advance together only where
this application supplies direct pressure.

### Completion gate

1. A canonical package manifest and lock select the complete source, resource,
   dependency, platform, and capability closure.
2. A deterministic bounded bundle carries the locked immutable content and is
   independently admitted before installation or execution.
3. A content-addressed local store publishes admitted objects without rewriting
   existing identities.
4. Windows and Linux construct the same canonical application WVB and report
   the same package and capability identities.
5. The application executes through a rights-reduced provider and proves both
   success and denied/unsupported behavior without ambient filesystem access.
6. A clean offline rebuild succeeds from the locked objects and documented
   native tool identities.

Do not add a public registry, general network resolver, dynamic linker, or SQL
surface to complete this milestone.

### Rebaseline audit

| Gate | Standing | Evidence or remaining boundary |
| --- | :---: | --- |
| Canonical manifest and lock closure | ✅ | Package 1 and Lock 1 select the exact WVDB Query source, resource, project, platform, license, tool, output, and capability closure under [Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md). |
| Deterministic independently admitted bundle | ✅ | Distinct Windvale-written writer and verifier implementations construct and admit the exact 43,725-byte Bundle 1 identity under [Decision 0561](../Decisions/0561-First-Admitted-Bundle-Store-And-Rights-Reduced-Wvdb-Query.md); paired [Verify run 31872089188](https://github.com/eworker-inc/Windvale/actions/runs/31872089188) owns valid, boundary, malformed, ordering, geometry, digest, target, and executable rejection cases. |
| Immutable content-addressed local store | ✅ | The admitted bundle publisher creates five digest-derived objects plus one bundle through private reread-verified publication and proves an idempotent second publish with zero rewrites on both hosts. This bounded host publisher closes Milestone 2 without claiming the later general activation/store service. |
| Same canonical host identities | ✅ | WVB `61f7b9d…`, bundle `3d7f035…`, Windows application `7cd6086…`, and Linux application `29b4d4d…` are pinned and reported by the paired owners. |
| Rights-reduced success and denial | ✅ | The ABI-23 five-entry provider table binds only `filesystem.directory_read_v1` to one fixed immutable object. Paired [Verify run 31872429140](https://github.com/eworker-inc/Windvale/actions/runs/31872429140) executes positive, negative-value, missing-key, unauthorized-name, and unavailable-provider cases on Windows and Linux. |
| Clean offline locked rebuild | ✅ | The selected package rebuilds from checked-in locked resources and documented native tool identities without .NET. Reconfirm it inside the final integrated package evidence without reopening the implementation gate. |

Milestone 2 is closed at implementation/evidence commit
`204e8082fdaabbc7333ac40ed6ca7ff8564de123`. Installer, registry, signing,
activation, SQL, and server work do not reopen it; the bounded installer and
release-envelope work now belongs to Milestone 3.

## Milestone 3: Windvale 0.1 preview — complete

Windvale 0.1 is the first inspectable product release, not the first complete
operating-system release. Its gate is defined in
[Packages-Releases-And-Recovery.md](../Architecture/Packages-Releases-And-Recovery.md#recommended-windvale-01-gate).
Recovery and repository-baseline tags remain separate from product versions as
defined by [Release-Names-And-Tags.md](Release-Names-And-Tags.md).

The preview requires:

1. the completed native normal workflow and recoverable Stage 0 lineage;
2. the useful package-backed application from Milestone 2;
3. reproducible source, package, tool, license, provenance, and qualification
   artifacts;
4. explicit capability approval, rights-reduced binding, and denial evidence;
5. a public threat model for shipped parsers, verifiers, providers, packages,
   and recovery paths;
6. a first release-signing and offline verification policy; and
7. one deliberately selected source state passing explicit dual-host
   qualification.

The first downloadable product shape is a per-user Windows installer and a
per-user Linux installer. Each installs the small Windvale launcher/client,
offline verifier, compiler, assembler, linker, runtime, and core inspectors.
WVDB Query may be offered as a separately installable example package; a
database server and other applications are separate packages or projects and
do not enlarge the base installer.

Windvale OS distribution, a public package registry, automatic updates, ARM64,
desktop graphics, a general network stack, and 1.0 compatibility are not 0.1
requirements.

### Rebaseline audit

| Gate | Standing | Evidence or remaining boundary |
| --- | :---: | --- |
| Native normal path and recoverable Stage 0 | ✅ | The normal repository is native-only and the exact pre-archive Stage 0 recovery release is published and independently retained. |
| Useful package-backed application | ✅ | Milestone 2 is complete with paired Bundle 1/store and rights-reduced capability reports under Decision 0561. |
| Reproducible release artifacts and installers | ✅ | Decision 0562 retains the exact qualified `0.1.0-dev.1` artifacts; Decision 0565 pins the stable `0.1.0` Windows/Linux identities; Decisions 0563 and 0566 own the envelope and protected-key custody. The official envelope is published with the [`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0). |
| Capability approval, binding, and denial | ✅ | Decision 0564 fixes one approval and two target launch records around the existing five-capability closure. The independent eight-case owner passes on Windows and Linux in Verify run 31883543587. |
| Public product threat model | ✅ | [Product-Threat-Model.md](../Architecture/Product-Threat-Model.md) covers the shipped source, compiler, WVB/WVO, bundle, installer, release, capability, and recovery boundaries with explicit limits and residual risks. |
| Release signing and offline verification policy | ✅ | The project-owner ceremony established the public root and release delegation; the published root policy, signed manifest, public keys, and independent verifier retain the offline verification path. |
| Exact-state dual-host release qualification | ✅ | [Qualification run 31889107326](https://github.com/eworker-inc/Windvale/actions/runs/31889107326) passed exact commit `c1d350949207c7ee6f82ed2c399b748e188bf949` before the signed tag and release were published. |

Milestone 3 closed on 15 August 2026 with signed tag `v0.1.0`, manifest digest
`2e28f45c668be869b17cc5547ee0865a0365417f442ecc49c0e481c696b6d85a`,
and the immutable public preview release. Future fixes or preview milestones use
new versions; they do not replace these assets.

## Parallel track OS-1: Windvale OS launch and service slice

This is the former Milestone 3. The rebaseline changes its scheduling relation,
not its outcome or completion gate.

### Outcome

Advance Probe 40 into one cleanly launched and supervised service/application
composition without expanding the kernel into a package manager, shell, or
policy engine.

### Completion gate

1. One flat resource domain owns explicit process, memory, capability, and work
   limits.
2. One immutable launch plan reserves, constructs, and publishes a process
   atomically, rolling back every unpublished resource on failure.
3. One isolated normal console or storage provider serves at least two clients
   with bounded queues, explicit peer loss, and generation-safe teardown.
4. Supervision reports structured completion and performs one bounded restart
   or deliberate terminal failure without ambient authority.
5. The exact application WVB used on Windows and Linux is admitted and executed
   in the guest where its capability profile is supported.

Dynamic discovery, general scheduling, a shell, networking, and multi-user
policy remain later milestones unless this slice produces a measured need.

### Rebaseline audit

| Gate | Standing | Evidence or remaining boundary |
| --- | :---: | --- |
| Flat resource domain | 🔵 | The current native Probe 40 candidate reserves and commits exactly 3 processes, 144 ordinary pages, and 2 endpoints before publication, retains peaks across client reuse, rejects post-stop work, and finishes at zero charge. Dynamic membership plus capability and work budgets remain open. |
| Atomic launch transaction | ○ | The immutable launch-plan contract is designed; dynamic reserve/construct/publish and complete rollback are not implemented. |
| Isolated provider serving two clients | 🔵 | Bounded queues, two endpoints, explicit peer loss, generation-safe rebind, and teardown are qualified. The fixed proof still serves sequential client generations rather than two live clients through a general provider boundary. |
| Structured supervision | 🔵 | Contained service failure and deliberate terminal behavior are qualified foundations; a supervisor, generic completion record, and bounded restart policy are not implemented. |
| Exact WVB across hosts and guest | ✅ | Decisions [0101](../Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) and [0103](../Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md) qualify exact canonical application WVBs across Windows, Linux, and the guest. A future WVDB-specific guest composition is consumer integration, not a reason to reopen this portability proof. |

OS-1 has one completed portability gate, three substantially implemented or
qualified foundations, and one unimplemented atomic-launch composition mechanism. Work may advance when it is the
selected consumer without delaying the host-product critical path.

## Proposed future product lane: agent runtime and digital subconscious

The proposed [agent runtime architecture](../Architecture/Agent-Runtime-And-Digital-Subconscious.md)
defines one durable agent identity with a foreground reasoning plane and a bounded
digital-subconscious support plane. The companion
[implementation plan](Windvale-Agent-Runtime-Implementation-Plan.md) stages the
work from deterministic records and transitions through hosted model invocation,
durable continuity, retrieval, governed action, and eventual supervised service
placement.

This lane is documented for dependency planning; it is not an active milestone and
does not displace the current compiler, database, package, or OS gates. Its first
candidate gate is deliberately capability-free and portable:

1. freeze one versioned corpus for agent, run, event, typed claim,
   context-manifest, context-diff, checkpoint, and operation-result records,
   including the three change clocks and closed attention states;
2. implement deterministic bounded transition, validation, claim-support,
   context-selection, and influence-inspection functions over supplied
   identities and time;
3. prove exact bytes and reports on Windows and Linux; and
4. keep model providers, clocks, entropy, durable storage, filesystems, and action
   capabilities outside that first semantic kernel.

Database append/replay and snapshot work may later satisfy the durable-run profile,
but the agent lane must consume the qualified database contract rather than define
a competing store. Compiler and library additions likewise require a measured
agent corpus plus at least one independent consumer before they widen shared
language semantics.

Later book-completeness gates add source/projection invalidation, evidence
bundles and sufficiency, reviewed memory, append-only action evidence,
cross-workspace/data-placement controls, backup/restore, emergency pause and
quotas, transparent human-facing vocabulary, and separate software and proposal
reference workflows. Those product gates do not enlarge the first portable
kernel.

After the governed single-episode product qualifies, a separate functional-mind
sequence may add a persistent agent self above runs, multi-episode intentions,
a recurrent global workspace, revisable world/belief/self models, bounded
salience and counterfactual simulation, layered consolidation, and authorized
event-driven subconscious wakeups. This later sequence retains one identity and
all existing evidence and authority boundaries; it does not claim consciousness
or authorize a continuously running model.

## Workstream rules

### Language and compiler

- New source semantics belong only in `Compiler/Windvale`.
- Add syntax or ABI breadth only for a named application, library, tool, or OS
  consumer.
- Keep portable semantics, WVB, native lowering, and WebAssembly profiles
  explicit; a narrower target does not redefine the source language.
- Update pinned tool products in deliberate promotion batches instead of
  turning every implementation commit into a repository-wide artifact refresh.

### Database and storage

- Use the package-backed application as the priority selector.
- Complete repeated depth-three operation, reclamation, and recovery only to
  the extent needed for a durable useful workload.
- Do not begin SQL, a server protocol, broad concurrency, or a public product
  identity before the bounded storage and capability contracts are useful end
  to end.

### Agent runtime

- Keep one durable agent identity; the digital subconscious is a support plane, not
  a second principal, hidden objective, or independent authority source.
- Treat each run as one episode beneath a long-lived self; values, commitments,
  intentions, autobiography, and skill evidence require explicit owners above
  individual episode state.
- Make every context item, cognitive operation, memory proposal, action proposal,
  lease, and checkpoint bounded, versioned, attributable, and inspectable.
- Keep claims typed and source-linked; projections, summaries, and derived
  memories remain rebuildable and lose eligibility when their permitted lineage
  becomes stale or unavailable.
- Keep the first portable kernel capability-free. Model calls, clocks, entropy,
  persistence, retrieval, filesystem access, and external effects enter only
  through separately granted semantic capabilities.
- Do not add peer agents, implicit background mutation, autonomous authority, or
  general concurrency merely to imitate an existing agent framework.
- Permit later bounded event-driven cognition only through admitted wake sources,
  one executor generation, explicit rate/cycle budgets, checkpointed outcomes,
  and clean return to dormancy.
- Route measured language, database, filesystem, package, runtime, and OS needs to
  their owning contracts before implementation crosses those boundaries.
- Treat account/workspace isolation, data placement, backup/restore, quotas,
  telemetry redaction, and emergency pause as named product gates rather than
  invisible deployment assumptions.

### Operating system

- Keep policy in isolated services and privileged mechanism in the kernel/WVA
  boundary.
- Advance one resource-domain, launch, service, driver, or teardown invariant at
  a time through a real consumer.
- Preserve pinned QEMU/Q35 as the reproducible oracle; physical or accelerated
  providers remain separately reported evidence.

### Browser, networking, shell, and virtualization

These remain accepted or proposed future lanes in their architecture documents.
They may receive bounded experiments, but they do not displace the product
critical path or parallel OS-1 track without a direct product or recovery need.

## Verification policy

Development and qualification answer different questions.

- Run `Tools/Verify/Verify-Changed.ps1` once after a coherent local edit.
- Reuse a passing result while the relevant owner inputs remain unchanged.
- Rerun only a failed or changed owner after correction.
- Ordinary GitHub changes run affected native owners; they do not create a
  qualification claim.
- Complete dual-host qualification is an explicit workflow dispatch for a
  selected source state.
- Managed Stage 0 evidence is restored from its immutable release only for a
  named recovery, security, or historical differential investigation.

Do not run changed-file, Fast, Development, Standard, and Qualification levels
sequentially for the same source state.

## Decision threshold

A numbered decision is required for a durable semantic or serialized-format
change, public capability or ABI contract, security or authority boundary,
bootstrap/recovery policy, qualification-model change, or another choice that
would be difficult to reverse silently.

Routine implementation checkpoints, fixture additions, artifact refreshes,
performance measurements, cache extensions, and test reorganizations normally
belong in code, specifications, the changelog, or the progress dashboard. They
should not receive a numbered decision unless they change one of the durable
boundaries above.

## Replanning rule

At a milestone boundary, keep, revise, or replace the proposed mechanism using
measured implementation evidence. Do not silently lower an accepted gate or
describe a narrower demonstration as completion of the original milestone.
