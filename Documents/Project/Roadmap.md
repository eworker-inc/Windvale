# Windvale development roadmap

> Status: Active dependency-based plan for the direct Windvale 1.0 product
> target accepted on 20 August 2026. Earlier numbered milestones remain
> historical evidence rather than the forward planning model.

## Current planning boundary

Preserve the signed 0.1 preview and every qualified package, installation,
network, compiler, and OS foundation while executing the
[Windvale 1.0 product plan](Windvale-1.0-Product-Plan.md). Decision 0800 targets
`v1.0.0` directly; no `v0.2.0` product release is planned. Active work is grouped
by product workstream and completion gate, not by numbered release stages.

Language compiler slices remain bounded implementation and evidence units under
the exact source design frozen by Decision 0767. They do not reopen the freeze
or define public maturity levels. WVDB is the Windvale-owned database specified
by Decision 0790, not an EWorker rewrite or external parity exercise. OS-1
continues independently; a complete general-purpose OS does not block the
Windows/Linux host product.

Windows and Linux remain permanent hosts. Windvale OS remains the vertical
integration target. Portable WVB remains the shared distribution contract, and
native execution remains a derived form over the same verified semantics.

The [cross-border product assurance and standardization strategy](Cross-Border-Product-Assurance-And-Standardization.md)
is a long-term product and ecosystem direction, not an added Windvale 1.0 gate.
Its first evidence should reuse existing package, release, conformance, OS, and
AI-workload boundaries. A normative assurance format, external standardization
submission, or certification claim requires a later decision.

[Decision 0751](../Decisions/0751-Accept-Windvale-Language-1.0-Direction.md)
starts a horizontal Language 1.0 specification track, and
[Decision 0752](../Decisions/0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md)
completes its ordered-set and package-data boundaries, while
[Decision 0753](../Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md)
requires an eleventh local AI accelerator workload before source freeze without
accepting accelerator syntax or implementation, and
[Decision 0754](../Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md)
resolves the first reviewed bundle's five general source-contract questions, and
[Decision 0755](../Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md)
accepts the command workload's general Foundation and launcher findings while
keeping its stream operation identities provisional, and
[Decision 0756](../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md)
accepts the file-copy workload's buffer, completion, progress, authority, and
lifecycle findings while keeping filesystem catalog identities provisional, and
[Decision 0757](../Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md)
accepts the database workload's arena, collection-observation, typed-schema,
commit, and recovery findings while keeping its capability/schema identities
provisional, and
[Decision 0758](../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md)
accepts the compiler workload's explicit generic construction, one-owner
collection observation, immutable arena, scalar-position, diagnostic, and byte
publication findings, and
[Decision 0759](../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md)
accepts the HTTP workload's checked slices, strict slice decode, byte decimal
formatting, opaque operation context, and exact reliable-stream progress
findings while leaving the service catalog identity provisional,
[Decision 0760](../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md)
accepts the concurrent-service workload's task, context, cancellation, and
asynchronous-provider boundaries, and
[Decision 0761](../Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md)
accepts the retained-GUI workload's typed-arena mutation, semantic event,
parent-only state application, and exact frame-publication boundaries, and
[Decision 0762](../Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md)
accepts contextual fixed arrays, checked mutable slices, strict numeric
operations/conversions/formatting, and bit-identical parallel policy, and
[Decision 0763](../Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md)
accepts complete deterministic collection publication, explicit bounded
parsing, package-content dedup/accounting, and canonical graph ordering, and
[Decision 0764](../Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md)
accepts the concrete System ABI target, registered ABI contract, pointer/scratch
surface, containment boundary, and safe-publication rules. The
[command-line application bundle](Language-1.0-Paper-Corpus/01-Command-Line-Application/README.md),
[bounded file-copy bundle](Language-1.0-Paper-Corpus/02-Bounded-File-Copy/README.md),
[database-transaction bundle](Language-1.0-Paper-Corpus/03-Database-Transaction/README.md),
[compiler-front-end bundle](Language-1.0-Paper-Corpus/04-Compiler-Front-End/README.md),
[HTTP request-handler bundle](Language-1.0-Paper-Corpus/05-Http-Request-Handler/README.md),
[concurrent-service bundle](Language-1.0-Paper-Corpus/06-Concurrent-Hosted-Service/README.md),
[retained-GUI bundle](Language-1.0-Paper-Corpus/07-Gui-Retained-State/README.md),
[numeric/graphics bundle](Language-1.0-Paper-Corpus/08-Numeric-Graphics-Processing/README.md),
[package-parser bundle](Language-1.0-Paper-Corpus/09-Package-Parser-Deterministic-Map/README.md),
[System/FFI bundle](Language-1.0-Paper-Corpus/10-System-Ffi-Boundary/README.md),
and
[local AI accelerator bundle](Language-1.0-Paper-Corpus/11-Local-AI-Accelerator-Inference/README.md)
are now draft reviewed. All eleven original required bundles are reviewed, and
the [complete-suite source-freeze packet](Windvale-Language-1.0-Source-Freeze-Review.md)
preserves the reconciled pre-localization grammar, Foundation, corpus, migration,
and historical candidate identities. The project owner held that candidate and
selected [one universal explicit source descriptor, stored localized keywords,
exact public-library source vocabularies, and Unicode project identifiers](Windvale-Semantic-Source-Views-And-Localization.md).
[Decision 0766](../Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
now accepts all five localization workloads and reconciles their exact formats,
conversion/tooling, Unicode/security, shipment/cache, cross-host, and
measurement contracts into the
[replacement specification addendum](../../Specifications/Windvale-Language-1.0-Localized-Source.md).
The [replacement manifest](Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt)
records 250 identity inputs and aggregate SHA-256
`fb918a763ae7c8c85dd1a2ffecee6587ab93bbf846ae31ae19b53509aed36a0a`.
The `zh-Hans@1` terminology remains an honest draft pending native and executable
qualification; that blocks official Chinese shipment, not the generic design
identity. Source implementation begins only after the separate named replacement
source-freeze decision. [Decision 0767](../Decisions/0767-Freeze-Windvale-Language-1.0-Source.md)
now freezes that exact identity and authorizes Migration Slice 0. Seed remains
the implemented language until focused executable gates advance each edition-1
slice; the draft Chinese pack retains its independent qualification path. The
[migration evidence](Windvale-Language-1.0-Migration-Evidence.md) fixes all 16
workload bundles and 72 source fixtures and records the pre-slice
compiler/WIR/memory/artifact baseline. Migration Slice 1 now connects the bounded
descriptor reader to the existing compiler, admits the exact `en@1` component
chain only through hash-pinned Project 3 lock/profile inputs, uses a no-copy body
view, parses the standalone Core header, and compiles one minimal edition-1 program
deterministically through WIR and WVB. Project 2 and descriptorless Seed remain on
their prior path; no ambient profile fallback or parallel compiler was added.
Slices 2 and 3 cover the frozen scalar/value surface, named record and variant
behavior, typed failure, generic Foundation `Option`/`Result`, canonical generic
syntax and resolution, and bounded generic-function specialization. Their
implemented WVB 1.12 through 1.16 paths cross the shared compiler, verifier, and
source-built scalar runner. Direct native lowering, browser, and OS consumers
remain explicitly narrower where their declared subsets have not advanced.

Slice 4 now includes generic nominal materialization, contextual immutable fixed
arrays, executable Vector/Sequence metadata and runtime operations, owned Vector
local transfer, exact Foundation Sequence reads, Vector length/freeze, and exact
freeze result contexts. [Decision 0832](../Decisions/0832-Bind-The-Canonical-Foundation-Memory-Budget-Identity.md)
adds the canonical qualified `Memoryˉbudget` source identity and owned WIR
classification. [Decision 0834](../Decisions/0834-Transfer-The-Launcher-Memory-Budget-As-Wvb-1.21.md)
begins Slice 5 by transferring that opaque owner
from the launcher into the WVB 1.21 entry parameter and releasing it on
invocation teardown.
[Decision 0835](../Decisions/0835-Represent-U8-Backed-Enums-As-Wvb-1.22.md)
then gives retained `u8`-backed enums an exact WVB 1.22 descriptor and connected
execution path. [Decision 0836](../Decisions/0836-Bind-Memory-Budget-Split-In-Wvir-1.5.md)
binds the exact mutable-parent Split call and its affine evidence as WVIR
1.5/1.6; the focused front door declares 478
cases. A separate 29-case executable accounting oracle now proves bounded child
reservation, failure atomicity, generation-safe reuse, deferred parent release,
recursive credit, malformed-state rejection, and teardown in about thirteen
seconds locally. Its fixed 65-entry state is an internal implementation model,
not the public `Memoryˉbudget` representation.
[Decision 0837](../Decisions/0837-Execute-Memory-Budget-Split-As-Wvb-1.23.md)
connects operation 171 to exact WVB 1.23, bounded forward-control ownership,
the verifier, and the source-built runner. Its 15-case owner proves both
successful and refused splits plus nine malformed modules.
[Decision 0838](../Decisions/0838-Admit-Exact-Language-1.0-Effect-Clauses.md)
then admits bounded exact `effects(...)` syntax and retains its declaration
span/count evidence without claiming semantic resolution.
[Decision 0839](../Decisions/0839-Admit-Exact-Language-1.0-Using-Statements.md)
then admits the bounded `using Name = Expression Block` syntax and retains its
binding, acquisition, and body spans without claiming resource semantics.
[Decision 0840](../Decisions/0840-Bind-Fallible-Vector-Construction-In-Wvir.md)
then binds exact fallible Vector construction as operation 172,
[Decision 0841](../Decisions/0841-Prove-Generation-Safe-Allocation-Leases.md)
proves generation-safe budget-to-lease transfer, and
[Decision 0842](../Decisions/0842-Execute-Fallible-Vector-Construction-As-Wvb-1.24.md)
connects the operation to canonical WVB 1.24, compiler-aligned verification,
typed target refusal, physical scalar backing, and descriptor-owned release.
The combined owner retains Split and adds the executable Vector success,
failure, zero-precondition, and malformed-module matrix.
[Decision 0843](../Decisions/0843-Execute-Recoverable-Vector-Append-As-Wvb-1.25.md)
adds exact recoverable append, [Decision 0844](../Decisions/0844-Prove-Owned-Vector-Calls-And-Forward-Joins-In-Wvir.md)
proves general Vector calls and forward joins in WVIR, and
[Decision 0845](../Decisions/0845-Execute-Owned-Vector-Calls-As-Wvb-1.26.md)
executes them with signature-encoded transfer modes and reverse-slot callee
cleanup. [Decision 0847](../Decisions/0847-Lower-Semantic-Using-And-Prove-Loop-Ownership.md)
lowers semantic `using`, reverse nested cleanup, and exact loop-ownership fixed
points. [Decision 0848](../Decisions/0848-Execute-Transactional-Vector-Growth-As-Wvb-1.27.md)
then executes explicit reserved Vector growth as a strong transaction through
WVB 1.27. [Decision 0850](../Decisions/0850-Own-Vector-Containing-Aggregates-As-Wvb-1.28.md)
then recursively moves records, variants, and fixed arrays containing Vector,
confines field/element views, and deterministically releases nested descriptors
through WVB 1.28. [Decision 0851](../Decisions/0851-Transfer-A-Rights-Limited-Source-File-As-Wvb-1.29.md)
completes Slice 5 by transferring one immutable, rights-limited source snapshot
through exact `Main(Sourceˉfile) -> i32`, WVB 1.29 shape `34`, opcode `D2`,
generation validation, and deterministic `using` release. The combined owner
now passes 113 cases. Acquiring additional budget authority from a provider
remains an explicit later hosted capability rather than ambient allocation.
[Decisions 0852](../Decisions/0852-Represent-Exact-Structural-Function-Types-With-Wvft.md)
through
[0856](../Decisions/0856-Execute-Noncapturing-Callable-Values-As-Wvb-1.30.md)
begin Slice 6 with exact structural function types, explicit capture checking,
bounded transitive effects, concrete callable cataloging, WVIR 1.17/1.18
function-reference and indirect-call operations, and WVB 1.30 verified scalar
execution. [Decision 0857](../Decisions/0857-Execute-Plain-Capture-Closure-Environments-As-Wvb-1.31.md)
adds WVIR 1.19/1.20 and WVB 1.31 immutable environments for copied inline
scalars and enums under a 4 MiB execution bound. Source closure-body lowering,
captured move/borrow and escape enforcement, effectful callable values, and
native callable ABI lowering are the next connected work in Slice 6.
[Decision 0858](../Decisions/0858-Catalog-Deterministic-Source-Closure-Targets.md)
now supplies bounded deterministic WVCL source-site ordinals for those
synthetic targets. [Decision 0859](../Decisions/0859-Publish-Synthetic-Closure-Bindings-As-Wvlb-1.4.md)
adds WVLB 1.4 physical ranges that separate each synthetic target's capture
prefix from its public parameters without forging a source symbol. Connecting
that carrier to final WVIR functions is the active next checkpoint.

Remaining collection
families, general ownership, localized token execution, direct
native/browser/OS collection execution, and paired-host conformance remain
later gates.

## How to read this roadmap

This file contains forward product workstreams, their dependency order,
completion gates, and the standing of already-earned evidence. It is not an
implementation diary or a public numbered maturity ladder.

- [Progress.md](Progress.md) records current measured state.
- [Seed-Verification-Evidence.md](Seed-Verification-Evidence.md) records exact
  historical qualification evidence.
- `Documents/Decisions/` records accepted rationale and supersession.
- `CHANGELOG.md` records notable implementation changes.

Completed migration detail remains available through Decisions 0057, 0178,
0213, 0525, and 0526 and Git history. It is not repeated here.

Historical decisions and commits keep their original milestone wording and
numbers. Decision 0800 does not renumber or erase that evidence; it ends their
use as the active forward product structure.

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

## Qualified foundations and active product target

| Work | Standing | Dependency |
| --- | :---: | --- |
| Predictable development feedback | ✅ Qualified foundation | Preserve its bounded workflow. |
| Native-only baseline promotion checkpoint | ✅ Qualified state / optional tag | The `v0.1.0` commit passed explicit post-archive dual-host qualification; a second checkpoint tag is optional. |
| Package-backed host application | ✅ Qualified foundation | Both permanent owners pass on Windows and Linux under Decision 0561. |
| Windvale 0.1 preview | ✅ Published history | The signed `v0.1.0` tag and immutable preview release retain the final installers, envelope, and paired qualification. |
| Offline package lifecycle | ✅ Qualified foundation | Two-package admission, immutable generations, activation/recovery, verified dispatch, rollback, and recoverable data-preserving uninstall pass on Windows and Linux in run `31906316540`. |
| Windvale 1.0 host product | 🎯 Active target | Complete the required Language, Libraries, WVDB, package/service, operations, support, and integrated qualification gates under Decision 0800. |
| Windvale OS | 🔵 Independent workstream | Shares portable contracts where useful and retains its own exact qualification boundary. |

The completed 0.1 product path was:

`Windows/Linux installers and core-tool packages → release envelope → one final release qualification → v0.1.0`

A completed focused gate remains complete while its relevant inputs and contract
stay unchanged. Later integrated evidence may consume that result; commits and
pushes do not by themselves require it to be rerun.

## Historical completion record: offline package lifecycle

[Decision 0590](../Decisions/0590-Offline-Package-Lifecycle-And-Generation-Activation-1.md)
selects installer/tool users as the primary audience and an offline
install/activate/recover/rollback demonstration as the finish. Networking, new
language semantics, and broader host authority were excluded. Completion closed
that bounded historical gate without publishing another product release.

### Selection record

| Choice | Advantages | Costs and risks | Standing |
| --- | --- | --- | --- |
| **Offline package lifecycle — selected** | Makes installation maintainable; gives `wv install`, generations, recovery, and rollback a real product boundary; reuses the completed 0.1 trust and installer work; remains testable without public infrastructure. | Requires generalizing the bounded package/store path and designing durable activation carefully; does not by itself advance Windvale OS. | Completed historical gate. |
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

### Completion evidence

The portable Windvale implementation parses bounded Generation 1 and Activation
1 records, validates ordered package/command closure, and plans idempotent
activation and rollback. The first host adapter now compare-publishes exact
caller-validated Activation 1 bytes, recovers an interrupted digest-named
candidate without changing the public record, rejects stale state, and publishes
rollback on both host paths without changing `v0.1.0`. WVDB Query and the WVB
inspector now rebuild into two exact admitted bundles that coexist in one
immutable store and share only the canonical license object. Both have exact
portable approvals and measured Windows/Linux launch records; the inspector's
exact installed host executes on each platform. A signed `stage` channel now
binds both bundles, approvals, provenance records, target launch records, the
shared license, and the offline verifier in one deterministic directory without
publishing a release or tag. The next stage revision also carries exact
Windows/Linux Generation 1 command inventories and immutably publishes the
current host's validated record. Windvale-native resolution also proves that
the active generation selects the exact `wvdump` or `wvquery` package, part,
approval, and target launch without guessing. Paired-host evidence for those
selection revisions is complete in GitHub run `31903569891`. The bounded host
dispatcher now locally executes both real commands only after reverifying their
bundle, approval, launch, target, and host identities and rejecting seven unsafe
or invalid selections. Dispatch passed both hosts in GitHub run `31904886608`.
The composed lifecycle publishes one- and two-package generations,
recovers an interrupted update without changing the old activation, activates
the expanded generation, and rolls back at serial 3 while retaining both
immutable records. It then durably uninstalls only package-owned state while
preserving application data, unrelated files, and the installation root. A
separate 13-case owner covers interrupted-uninstall recovery and five safety
rejections. Exact implementation commit `df2d15dad0434182b74ad7ae357b4596d4aef82d`
passed both owners on Windows and Linux in GitHub run `31906316540`; the focused
jobs took 1 minute 11 seconds and 38 seconds respectively, and the aggregate gate
passed. The offline lifecycle gate is closed without a new product tag or release. Signed
revocation/minimum-version policy belongs to the later security/update boundary
and is not silently approximated by this offline gate.

## Windvale 1.0 host product — active target

[Decision 0800](../Decisions/0800-Target-Windvale-1.0-Directly.md) supersedes the
future `v0.2.0` product selection in Decision 0595. The next intended product
tag is `v1.0.0`, and it waits for the complete
[Windvale 1.0 product gate](Windvale-1.0-Product-Plan.md).

The required workstreams are:

1. implement and qualify the frozen Language 1.0 contract through the shared
   compiler, verified representation, runtime, and native paths;
2. specify and implement the required Windvale Libraries 1.0 Foundation, Data,
   and Backend modules for ordinary bounded data services;
3. complete WVDB 1.0 as a Windvale-owned production-usable database with its
   accepted table and typed-relationship profiles, transactions, queries,
   indexes, durability, full backup/restore, service, operations, and
   conformance;
4. integrate immutable packages, signed release admission, installers,
   service lifecycle, safe upgrade/rollback/removal, and recovery; and
5. pass the exact Windows/Linux security, resource, determinism, compatibility,
   support, and release qualification gates.

Workstreams may advance in parallel through focused slices. An external-model
gateway remains a useful optional consumer of networking and backend libraries,
not an automatic 1.0 release requirement. Windvale OS retains an independent
qualification path and does not become an undocumented host-product claim.

## Strategic 2027 lane: compute leadership and efficiency

[Decision 0803](../Decisions/0803-Make-Compute-Performance-And-Efficiency-A-2027-Program.md)
accepts performance and efficiency as a cross-cutting 2027 program. Its durable
boundaries are in the
[compute and efficiency architecture](../Architecture/Compute-And-Efficiency.md),
and its workstreams, quarterly checkpoints, scorecard, and public-claim gate are
in the
[2027 compute leadership roadmap](Windvale-2027-Compute-Leadership-Roadmap.md).

The program connects compiler optimization, CPU vector and parallel execution,
runtime memory and scheduling, NVIDIA and AMD accelerators, high-performance
networking, Windvale OS mechanisms, WVDB/storage, profiling, and full-system
energy evidence. It aims to earn leadership on exact named workloads rather
than claim universal performance.

This lane may advance alongside every 1.0 workstream and may supply qualified
1.0 improvements. It does not silently add complete accelerator or Windvale OS
support to the 1.0 release gate. Every shipped component still receives
performance and memory regression coverage appropriate to its contract.

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
| Same canonical host identities | ✅ | WVB `61f7b9d…`, bundle `3d7f035…`, hardened no-link Windows application `198d44b…`, and Linux application `b21095d…` are pinned and reported by the paired owners. |
| Rights-reduced success and denial | ✅ | The ABI-23 five-entry provider table binds only `filesystem.directory_read_v1` to one fixed immutable object. The current hardened leaves add regular-file and no-link/reparse enforcement and a sixth traversal-denial case; the earlier paired [Verify run 31872429140](https://github.com/eworker-inc/Windvale/actions/runs/31872429140) remains the five-case cross-host qualification until the hardened owner runs on Linux. |
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
| Reproducible release artifacts and installers | ✅ | Decision 0562 records the exact qualified `0.1.0-dev.1` artifacts; Decision 0565 pins the stable `0.1.0` Windows/Linux identities; Decision 0749 advances the unsigned successor to compressed `0.2.0-dev.1` candidates. Decisions 0563 and 0566 own the published envelope and protected-key custody for the [`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0). |
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
| Atomic launch transaction | 🔵 | The current Probe 40 candidate routes both sequential generations of one fixed known child through immutable reserve/private-construct/publish transactions, admits distinct bounded machine layouts with W^X/capability checks, and proves failed-construction rollback. `WVSR 1` independently checks the first exact 64-byte application request. User-memory copy/syscall entry, arbitrary admitted images, variable charges, and runtime allocation/mapping remain open. |
| Isolated provider serving two clients | 🔵 | Bounded queues, two endpoints, explicit peer loss, generation-safe rebind, and teardown are qualified. The current candidate pins the filesystem, operation-queue, and network authority envelope, boot-embeds separate deterministic filesystem and network user images, and admits their exact failure-atomic resource/mapping/publication/teardown transaction. Checked Windvale x86-64 emission now begins replacement of the fixed three-process machine boundary, but the privileged machine still does not allocate, map, launch, or bind those providers, or serve two live clients through a general boundary. |
| Structured supervision | 🔵 | Contained service failure and deliberate terminal behavior are qualified foundations; a supervisor, generic completion record, and bounded restart policy are not implemented. |
| Exact WVB across hosts and guest | ✅ | Decisions [0101](../Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) and [0103](../Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md) qualify exact canonical application WVBs across Windows, Linux, and the guest. A future WVDB-specific guest composition is consumer integration, not a reason to reopen this portability proof. |

OS-1 has one completed portability gate and four substantially implemented or
qualified foundations. The current candidate also admits a versioned
generation-safe executable-publication request before both fixed launch
transactions, independently validates its `WVSR 1` serialization, and requires
the fixed boot-service envelope before token 97. User-memory copy/syscall entry,
dynamic allocation, filesystem/network
provider launch, IPC/resource binding, and supervision remain active work that
may advance without delaying the host-product critical path.

## Proposed future product lane: agent runtime and digital subconscious

The proposed [agent runtime architecture](../Architecture/Agent-Runtime-And-Digital-Subconscious.md)
defines one durable agent identity with a foreground reasoning plane and a bounded
digital-subconscious support plane. The companion
[implementation plan](Windvale-Agent-Runtime-Implementation-Plan.md) stages the
work from deterministic records and transitions through hosted model invocation,
durable continuity, retrieval, governed action, and eventual supervised service
placement. The
[verified AI workload and agent-aware inference proposal](Verified-AI-Workloads-And-Agent-Aware-Inference-Proposal.md)
keeps that durable agent above replaceable local and remote inference providers,
including eligible NVIDIA Dynamo, vLLM, SGLang, TensorRT-LLM, llama.cpp, and
other integrations. Agent-aware scheduling is an active upstream field;
Windvale's proposed differentiation is exact workload identity, authority,
provider-neutral continuity, and execution/effect evidence rather than a
competing provider cache format. These systems are candidate integration targets;
this proposed lane does not claim that the integrations are implemented. The
[persistent-self governance architecture](../Architecture/Persistent-Self-Ownership-And-Governance.md)
adds a named E-Worker development/test authority for early fixtures and a later
constitutional-stewardship profile for advanced operation.
The
[executive-function and qualification architecture](../Architecture/Agent-Executive-Function-And-Qualification.md)
adds the deliberation contract and a provider-neutral Mandate-to-Milestone test
of whether the agent can turn direction into verified progress without routine
human supervision.

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

The same corpus later receives a model-assisted executive pass. It presents a
high-level mandate, several plausible milestones, one valid bounded next step,
one scope expansion, one blocked choice, one lower-value option, a source change,
a failed check, and separate safe versus approval-required operations. The agent
must select, deliberate, recover, verify, continue safe work, and stop at the
real authority boundary. Semantic conformance remains exact; usefulness and
capability realization use a separate named outcome rubric.

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

Development and qualification may place most self-governance decisions with a
versioned E-Worker Development Authority so experiments can create, revise,
break, restore, migrate, and retire synthetic test selves. That arrangement is
not the product destination. Production-like pilots introduce real principals,
domain/data owners, and bounded E-Worker roles; advanced operation requires a
qualified transition that removes test bypasses and broad developer access and
instantiates the complete authority matrix.

## Proposed future product lane: organizational Observatory

The proposed
[organizational Observatory architecture](../Architecture/Organizational-Observatory-And-Epistemic-Infrastructure.md)
defines internal organizational intelligence as epistemic infrastructure: one
product composed from independently owned observation, evidence, epistemic-
state, deliberation, verification, knowledge-admission, decision-support, and
action systems. Its
[implementation plan](Windvale-Organizational-Observatory-Implementation-Plan.md)
begins with a synthetic, read-only organizational-readiness brief rather than a
live connector, continuous model cluster, general knowledge graph, or action
engine.

This lane is documented for product and dependency planning; it is not an
active milestone. Stages 0 through 3 are capability-free and deterministic:
they freeze the organizational vocabulary and corpus, preserve source and
provenance identity, maintain typed epistemic state and invalidation, and
produce a scripted evidence-backed readiness brief with disagreement and
uncertainty visible. Later stages may consume the qualified agent runtime,
provider-neutral model protocol, database, filesystem, identity, network,
connector, package, and OS contracts without redefining them.

The product maintains a continuously revisable account of what admitted
evidence supports; it is not a source of truth and must not silently convert
observations, reports, calculations, hypotheses, recommendations, decisions, or
commitments into one undifferentiated fact. Search remains one observation
sense among many. A later cross-organization **Windvale Constellation** remains
research until one real federation problem and its disclosure, trust,
provenance, revocation, and consensus boundaries are named.

## Workstream rules

### Language and compiler

- New source semantics belong only in `Compiler/Windvale`.
- Complete the accepted Language 1.0 semantic, grammar, Foundation, paper-corpus,
  and migration contracts before staged implementation. A source-freeze rule
  needs a named corpus or architecture requirement; an implementation slice
  needs the corresponding frozen rule and consumer.
- Do not allow implementation order, one compiler workload, or one target to
  redefine the complete accepted source design.
- Keep portable semantics, WVB, native lowering, and WebAssembly profiles
  explicit; a narrower target does not redefine the source language.
- Keep native compilation direct under
  [Decision 0802](../Decisions/0802-Share-X64-Encoding-Without-Compiling-Through-WVA.md):
  never format and reparse WVA as an ordinary compiler intermediate.
- Extract or extend shared x86-64 encoding and WVO construction for a current
  consumer or an accepted near-term OS, compute, hardware-research, tooling, or
  profiling plan with a named owner, bounded surface, hypothesis or expected
  benefit, work budget, stop condition, and executable evidence. Exploratory
  work need not guarantee production adoption. Preserve byte identity,
  diagnostics, bounds, and focused cross-host evidence.
- Update pinned tool products in deliberate promotion batches instead of
  turning every implementation commit into a repository-wide artifact refresh.

### Database and storage

- Use the accepted [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md)
  and its decision register as the product authority.
- Complete entity/table, relationship, index, query, transaction, storage,
  durability, backup/restore, service, operations, and conformance work only
  through explicit WVDB contracts with finite limits and owners.
- Treat established databases and earlier implementations as comparative
  evidence only. Do not silently inherit an external file format, wire protocol,
  runtime, feature inventory, or compatibility promise.

### Agent runtime

- Keep one durable agent identity; the digital subconscious is a support plane, not
  a second principal, hidden objective, or independent authority source.
- Treat each run as one episode beneath a long-lived self; values, commitments,
  intentions, autobiography, and skill evidence require explicit owners above
  individual episode state.
- Use Profile D only for bounded, visibly non-production development/testing;
  record every role E-Worker occupies, all test bypasses, scope, expiry, audit,
  disposal, and the prohibition on silent promotion.
- Treat advanced governance as a separate migration gate with a primary
  principal, constitutional stewards, domain/data owners, runtime custodian,
  capability owners, audit/appeal, recovery/succession, and expired test access.
- Make every context item, cognitive operation, memory proposal, action proposal,
  lease, and checkpoint bounded, versioned, attributable, and inspectable.
- Compile one deliberation contract for each material work sequence so a
  high-level mandate becomes an explicit selected problem, strategy, evidence
  set, capability allocation, success/stop rule, verification route, and
  escalation boundary.
- Measure unplanned human intervention separately from legitimate approval:
  fewer questions are useful only when routine authorized work continues and
  consequential ambiguity still stops correctly.
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

### Organizational Observatory

- Describe the product as evidence-backed epistemic infrastructure, never as a
  universal source of truth or an infallible organizational authority.
- Begin with the static synthetic, read-only readiness scenario; add durable
  state, live sources, drafts, and actions only at their named gates.
- Keep observations, source reports, extractions, calculations, claims,
  hypotheses, predictions, recommendations, decisions, commitments, and
  accepted organizational knowledge distinct and attributable.
- Keep domain meaning and knowledge admission with qualified domain and
  institutional owners; shared machinery may transport evidence but must not
  erase legal, financial, technical, operational, or governance boundaries.
- Keep organization-owned evidence, knowledge, policy, decisions, commitments,
  and workplace state outside agent-private memory; persistent-self continuity
  cannot bypass source, retention, correction, revocation, or deletion owners.
- Exclude hidden employee surveillance, behavioral scoring, inferred intent,
  and cross-context profiling from the foundation product.
- Prove the first deliberation with scripted jobs and explicit challenge before
  introducing model-assisted or distributed reasoning.
- Add indexes only from measured records and workloads; do not begin with a
  general graph, vector store, or distributed consensus system.
- Route model, database, filesystem, connector, identity, network, compiler,
  library, package, and OS requirements to their owning documents and decisions.
- Defer Windvale Constellation until a real cross-organization consumer
  justifies its disclosure, trust, provenance, revocation, and consensus model.

### Operating system

- Keep policy in isolated services and privileged mechanism in the kernel/WVA
  boundary.
- Replace the remaining process architecture fixture in cohesive source-owned
  slices: the checked emitter and exact 1,119-byte process-entry/dispatcher are
  implemented, and the following 309-byte coordinator initialization/relocation
  surface plus 444-byte channel/endpoint region are explicit; checked allocation
  and returned-extent validation plus the complete 462-byte init process record
  are source-owned, and the following retained-table copy and W^X/null-safe init
  paging are exact through byte 2,948. Four bounded private input copies, their
  typed relocations, native context, and store descriptor now extend ownership
  through byte 3,097. Checked non-tail recyclable-client reservation now extends
  ownership through byte 3,215, and checked directory-provider allocation now
  extends it through byte 3,322, and complete directory record construction from
  measured inputs reaches byte 3,784, and directory-private null-safe W^X
  paging reaches byte 4,224, and bounded measured provider inputs, native
  context, and snapshot descriptor now reach byte 4,340. Exact recyclable-client
  record construction from admitted interpreter/program identities, fixed
  budgets, and rights-limited resource/directory capabilities reaches byte
  4,858. Client-private null-safe W^X mappings and two post-extent guard entries
  now reach byte 9,606. The bounded interpreter copy, typed relocation, and
  private execution context reach byte 9,682. The first generation-one program
  resource reaches byte 9,930 and its separate generation-two budget resource
  reaches byte 10,159, and the generation-three immutable store resource reaches
  byte 10,398, and the generation-four read-only directory resource reaches byte
  10,637. Exact validation of the immutable store and its private W^X mapping
  reaches byte 11,031 with twenty-two additional explicit failure branches;
  directory snapshot validation reaches byte 11,441 with twenty-three more.
  Privileged GDT/TSS, four IDT gates, feature admission, and syscall-MSR setup
  then reach byte 12,082; three private thread records plus bounded timer state
  reach byte 12,872. Timer activation and rollback reach byte 12,997, and the
  guarded provider user-context transfer reaches byte 13,168, and checked
  provider-return/init transfer reaches byte 13,447. Checked init return and
  generation-one program-resource validation reach byte 13,786. Budget-resource
  and retained store/directory backing validation now reach byte 14,402.
  Guarded client user-context transfer reaches byte 14,576, and checked client
  return/init resume reaches byte 14,907. Init's checked 116-byte reply
  publication and zero-result resume reach byte 15,243, exact client reply
  delivery reaches byte 15,574, and checked transfer of the first 37-byte
  directory request reaches byte 15,905. Checked publication and provider
  resume for the 3,096-byte reply reach byte 16,241, and exact client delivery
  reaches byte 16,572, completing the first directory round trip.
  Checked generation-1 client completion, endpoint-alias removal, complete IPC
  scrubbing, and endpoint closure reach byte 17,923. A fail-closed retained-state
  reclamation preflight reaches byte 19,525. Checked release, allocator-state
  restoration, and same-root generation-2 allocation now reach byte 19,741.
  Private generation-2 client-record reconstruction now reaches byte 20,240.
  Exact checked paging reuse now reaches byte 24,988. Exact interpreter
  copy/context-seed reuse now reaches byte 25,064. Endpoint rebinding, the final
  resource-state transition, and re-entry remain at that boundary.
  Checked two-endpoint generation-2 rebinding now reaches byte 25,512.
  Checked memory/resource transition and generation-2 re-entry now execute the
  first generation-2 `sysretq` and reach byte 25,953.
  Resumed processor/resource/mapping/alias validation now reaches byte 26,964,
  immediately before the next dispatcher crossing.
  The generation-adapted checked client transfer now executes the second
  generation-2 `sysretq` and reaches byte 27,138.
  The generation-adapted client return now delivers result 55 to init and
  reaches byte 27,469 through the next `sysretq`.
  The adapted init reply for operation 7 and retained generation 2 now returns
  zero to the client and reaches byte 27,805.
  The adapted client reply delivery for operation 8 and generation-2 retained,
  thread, and selected state now returns result 116 and reaches byte 28,136.
  The adapted directory-request delivery now selects provider generation 2,
  returns the exact 37-byte request result, and reaches byte 28,467.
  The adapted provider reply now validates operation 4 and generation 2,
  publishes the exact 3,096-byte reply, and reaches byte 28,803.
  The checked endpoint lifecycle then publishes resolution, scrubs the request,
  advances bounded state, delivers 3,096 bytes to client generation 2, and
  reaches byte 29,474.
  Generation-two completion cleanup reuses all checked teardown validation,
  advances only retained and selected generations to 2, and reaches byte
  30,825.
  Completion finalization then validates operation 6, closes and scrubs the
  channel, advances its generation, resumes the selected client, and reaches
  byte 31,199.
  The final-state epilogue validates every remaining terminal invariant and
  completes source ownership of all 33,826 process-machine bytes.
  Syscall/exception handler bodies, teardown, and live QEMU evidence remain.
- Complete the application-start boundary first. The typed source admission and
  executable-publication check, exact serialized decoder, immutable user-window
  copy, independent caller check, x86-64 page-access adapter, operation-8
  syscall entry, and fixed checked child publication are implemented. Init
  invocation and dynamic executable/object allocation remain before arbitrary
  applications can rely on it; the fixed publication is sufficient to begin
  launching the already boot-linked isolated providers.
- Then advance the selected [filesystem plan](Windvale-Filesystem-Implementation-Plan.md):
  shared semantics, Windows/Linux host adapters, bounded Windvale OS service
  IPC, and one FAT32-backed guest provider before optional NTFS/ext4 adapters or
  a new native disk format.
- Advance the selected [networking plan](Windvale-Networking-Foundation-Implementation-Plan.md)
  through shared operation/address contracts, deterministic link/packet tests,
  Windows/Linux providers, and then the isolated guest link/transport service.
- Advance one resource-domain, launch, filesystem, network, driver, or teardown
  invariant at a time through a real consumer.
- Preserve pinned QEMU/Q35 as the reproducible oracle; physical or accelerated
  providers remain separately reported evidence.

### Browser, shell, and virtualization

Hosted resolver, secure-stream, and HTTP work supplies reusable Windvale 1.0
Backend and package-service contracts. An external-model gateway may consume
those contracts but is not itself a release gate. Browser Workbench may consume
only a local gateway and never owns provider credentials. Shell, virtualization,
and the Windvale OS packet/device path retain their independent architecture
gates and do not become release blockers without a direct product or recovery
need.

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
