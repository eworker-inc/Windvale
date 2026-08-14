# Windvale development roadmap

> Status: Active forward plan, reset after the completed .NET retirement gate on
> 14 August 2026.

## Active goal

Turn the qualified Windvale-native compiler and toolchain into a useful,
inspectable product path: fast ordinary development, one deterministic
package-backed application on Windows and Linux, the next bounded Windvale OS
service slice, and an honest 0.1 preview.

Windows and Linux remain permanent hosts. Windvale OS remains the vertical
integration target. Portable WVB remains the shared distribution contract, and
native execution remains a derived form over the same verified semantics.

## How to read this roadmap

This file contains only forward milestones, their order, and their completion
gates. It is not an implementation diary.

- [Progress.md](Progress.md) records current measured state.
- [Seed-Verification-Evidence.md](Seed-Verification-Evidence.md) records exact
  historical qualification evidence.
- `Documents/Decisions/` records accepted rationale and supersession.
- `CHANGELOG.md` records notable implementation changes.

Completed migration detail remains available through Decisions 0057, 0178,
0213, 0525, and 0526 and Git history. It is not repeated here.

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

## Milestone 1: predictable development feedback

### Outcome

Ordinary work receives focused feedback quickly enough that verification does
not dominate implementation.

### Current baseline

[Decision 0557](../Decisions/0557-Separate-Development-Verification-From-Qualification.md)
separates affected-owner development checks from complete qualification.
[Decisions 0553 through 0555](../Decisions/0555-Content-Addressed-Project-Wvb-Development-Checkpoints.md)
add content-addressed development checkpoints for the current database path.
The warm two-case database owner is approximately 71 seconds on the measured
Windows host. Complete qualification remains cold and explicit.

The managed-source archival audit measured the old `seed-native-front-door` at
733,980 ms because it reconstructed 105 artifacts. The ordinary owner now binds
all 18 pinned front-door identities and admits all six WVB modules in 13,900 ms
on the same Windows host. The full 185-assertion reconstruction remains a
separately named explicit-qualification owner.

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

## Milestone 2: useful package-backed application

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

## Milestone 3: Windvale OS launch and service slice

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

## Milestone 4: Windvale 0.1 preview

Windvale 0.1 is the first inspectable product release, not the first complete
operating-system release. Its gate is defined in
[Packages-Releases-And-Recovery.md](../Architecture/Packages-Releases-And-Recovery.md#recommended-windvale-01-gate).
Recovery and repository-baseline tags remain separate from product versions as
defined by [Release-Names-And-Tags.md](Release-Names-And-Tags.md).

The preview requires:

1. the completed .NET-free normal workflow and recoverable Stage 0 lineage;
2. the useful package-backed application from Milestone 2;
3. reproducible source, package, tool, license, provenance, and qualification
   artifacts;
4. explicit capability approval, rights-reduced binding, and denial evidence;
5. a public threat model for shipped parsers, verifiers, providers, packages,
   and recovery paths;
6. a first release-signing and offline verification policy; and
7. one deliberately selected source state passing explicit dual-host
   qualification.

Windvale OS distribution, a public package registry, automatic updates, ARM64,
desktop graphics, a general network stack, and 1.0 compatibility are not 0.1
requirements.

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

### Operating system

- Keep policy in isolated services and privileged mechanism in the kernel/WVA
  boundary.
- Advance one resource-domain, launch, service, driver, or teardown invariant at
  a time through a real consumer.
- Preserve pinned QEMU/Q35 as the reproducible oracle; physical or accelerated
  providers remain separately reported evidence.

### Browser, networking, shell, and virtualization

These remain accepted or proposed future lanes in their architecture documents.
They may receive bounded experiments, but they do not displace the four active
milestones without a direct product or recovery need.

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
