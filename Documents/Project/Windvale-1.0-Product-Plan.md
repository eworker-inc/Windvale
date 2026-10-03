# Windvale 1.0 product plan

> Status: Current language and toolchain target; delivery and qualification incomplete
> Authority: Informative; accepted decisions and specifications own contracts
> Last reviewed: 2026-10-03

- Date: 2026-08-20
- Status: Active product target; implementation and qualification incomplete
- Direction: [Language and essential-library scope](../Decisions/0976-Focus-Windvale-1.0-On-The-Language-And-Essential-Libraries.md),
  amending [Target Windvale 1.0 directly](../Decisions/0800-Target-Windvale-1.0-Directly.md)
- Language contract: [Language 1.0 freeze](../Decisions/0767-Freeze-Windvale-Language-1.0-Source.md)
- Library program: [Windvale Libraries 1.0](Windvale-Libraries-1.0-Plan.md)
- Deferred database program: [WVDB 1.0](WVDB-1.0-Specification-Plan.md)
- Strategic performance program:
  [2027 compute leadership](Windvale-2027-Compute-Leadership-Roadmap.md)

## Product outcome

Windvale 1.0 should let a person install one supported toolchain, write ordinary
Windvale programs using essential libraries, build and run them on Windows and
Debian/Linux, and reconstruct the compiler from its declared inputs. The
language, compiler and libraries are the first useful product. The compiler and
its development tools are the primary maintained consumers; WVDB and secondary
applications are outside this delivery's critical path.

The next intended product tag is `v1.0.0`. There is no planned `v0.2.0` product
release. The version is earned by the complete gate below; it is not a label for
the current repository state.

## Planning model

The roadmap uses workstreams and gates. Workstreams may advance concurrently,
and each may use small implementation slices. Those slices are engineering
units, not public product stages and not compatibility levels.

The current dependency shape is:

```text
accepted Language 1.0 contract
  -> current compiler/runtime and complete ownership-to-storage memory
  -> essential libraries used by compiler and toolchain consumers
  -> deterministic self-hosting and clean Windows/Debian installation

explicit preparation, package, installer, security, and recovery work
  -> makes the selected toolchain usable and supportable

all required workstreams
  -> one cross-host Windvale 1.0 release gate
```

This is a dependency map, not a release sequence. A downstream specification can
advance while an upstream implementation slice is still being completed, but
qualification cannot claim behavior that the selected implementation lacks.

The broader compute, database, service and OS programs retain their own
contracts. They do not add automatic prerequisites to this language/toolchain
delivery. Admit an enabling improvement only for a named supported dependency
or a measured bottleneck.

## Required workstreams

### Language, compiler, runtime, and toolchain

The Language 1.0 source design is frozen by Decision 0767. Complete its bounded
implementation in the normal Windvale compiler and shared verified execution
path. Compiler slice numbers describe that implementation work; they do not
reopen the frozen design.

The [Slice 8 qualification](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md)
establishes the compiler within its exact qualified scope and targets. General
native memory management, complete libraries and installed promotion remain
open. Finish unique mutable ownership, shared immutable backing, borrowed
lifetimes, physical storage reuse, budgets, allocation leases and cleanup.
Bound interpreter and compiler working storage independently of guest accounting.
A source-rule change requires a named defect and a deliberate freeze amendment.

[Decision 0802](../Decisions/0802-Share-X64-Encoding-Without-Compiling-Through-WVA.md)
keeps native compilation direct: the compiler does not emit or parse textual
WVA. The WVA frontend and native lowerer share x86-64 encoding and WVO
construction only for selected real overlap. Windvale 1.0 does not require a
speculative target-neutral machine IR or migration of WVA-only operations; each
selected duplicated production behavior must either use its shared owner or
retain a documented reason plus exact differential evidence.

### Windvale Libraries 1.0

Complete essential value, numeric/ordering, memory, collection, bytes/text and
resource APIs, plus the explicit host file, directory and publication boundaries
needed by programs and the toolchain. Use compiler parsing, symbol/analysis
state, interpreter working state and bytecode/object output as maintained
consumers. Required package admission and publication remain supported; a typed
Package-Lock refactor or a database operation is not a mandatory memory consumer.

The [completion matrix](Compiler-Tools-And-Libraries-1.0-Matrix.md) distinguishes
accepted declarations, existing implementation and unresolved APIs. Reconcile
the finite delivery profile against the accepted Foundation registry before
qualification. An essential subset does not establish the complete suite.

Each module must have exact public names, types, limits, capabilities, mutation
completion behavior, portability scope, test oracles, and package identity.
Draft Data/Backend catalogs and broader hosted profiles remain proposed where
their contracts have not been accepted. They are separate delivery work.

### Packages, installation, self-hosting, and release

Integrate immutable packages, approvals, generations, activation, rollback,
recoverable uninstall, and signed release admission into an ordinary installed
product. Supply supported per-user toolchain installation on Windows and
Debian/Linux, using the selected current compiler and runtime generation.

The release must define safe install, upgrade, rollback, package-format admission,
data preservation, uninstall, offline verification, and recovery. Demonstrate
deterministic self-host reconstruction from declared inputs on both hosts.
An official connected source may improve delivery, but downloaded and offline
admission must select the same signed immutable objects. No transport location is
an authority or artifact identity.

### Security, operations, and evidence

Close the public threat model for every shipped parser, binary format, provider,
credential and package boundary. Publish finite defaults and
configurable ceilings for input, memory, storage, concurrency, work, queues,
deadlines, diagnostics and recovery for the shipped surface.

### Simplification and retirement

Select one ordinary build, verify, run, inspect, assemble, link and package path.
Keep expensive preparation explicit; reuse valid products by complete input
identity and measure ordinary edit/build/run feedback.

The [supported-toolchain map](Compiler-Tools-And-Libraries-Completion-Plan.md#supported-toolchain-and-retirement-map)
classifies current, required-bootstrap, superseded, recovery-only and parked
components. Retire superseded active source and compatibility after the
supported paths no longer depend on them. Secondary applications may break and
be migrated later. Preserve immutable release/recovery inputs and exact earlier
evidence. Host orchestration remains an explicit dependency until Windvale can
replace it usefully.

The selected release state must have deterministic builds where promised,
hostile-input coverage, interruption and crash evidence, bounded soak and
resource workloads, cross-host conformance, release provenance, signed artifacts,
and an independently usable offline verification path.

## Windvale 1.0 release gate

The `v1.0.0` tag waits until all of the following are true:

| Gate | Required evidence |
| --- | --- |
| Contract | Accepted source semantics and a finite, versioned language/library/toolchain target matrix with exact inclusions, exclusions and limits. |
| Implementation | One maintained implementation for every shipped promise, including ownership, accounting, storage reuse and bounded runtime working state. |
| Usefulness | Clean Windows and Debian installations build and run ordinary library programs and the maintained compiler/toolchain consumers. |
| Self-hosting | Declared bootstrap inputs reconstruct the selected Windvale compiler deterministically on both hosts. |
| Safety and authority | Capabilities are exact and rights-limited; untrusted input is bounded; uncertain mutations, revocation, failure, teardown, and recovery are explicit. |
| Compatibility | The 1.0 stability, support, deprecation, file-format, package, and migration promises are written before release. |
| Qualification | The exact release commit and artifacts pass the selected Windows/Linux conformance, determinism, performance, memory, recovery, security, and release gates. |
| Distribution | Identified source, tools, libraries, installers, documentation, provenance, signed artifacts and offline verification support one immutable release. |

Passing one row does not authorize the tag. Product progress should report each
row independently and name gaps without translating a percentage into a
compatibility claim.

## Explicitly outside the automatic 1.0 gate

- WVDB, secondary applications, the broad Data/Backend catalog, network/TLS/HTTP
  services and privileged service installation;
- browser/editor expansion, 2.0 proposals and complete 2027 compute goals;
- a complete general-purpose Windvale OS, desktop, or broad hardware catalog;
- wire, SQL, file, API, or behavioral compatibility with another database;
- .NET, Java, ASP.NET, E-Worker, or another framework/runtime dependency;
- distributed consensus, clustering, automatic failover, or multi-region WVDB;
- every optional library, database profile, browser application, model gateway,
  agent system, accelerator, or virtualization feature; and
- preservation of obsolete development formats without a named migration case.

These retain their own contracts and can be revisited after the usable language
and libraries. A named required toolchain dependency may remain without
reopening an entire deferred product lane.

## Existing versions and artifacts

The signed `v0.1.0` preview and its exact evidence remain published history.
Completed milestone records remain useful provenance but no longer organize the
forward roadmap. Checked-in `0.2.0-dev.1` installer/repository candidates keep
their exact historical names and hashes; they are implementation inputs, not a
selected `v0.2.0` release. Select any new 1.0 development artifact identity by
an explicit release or format decision rather than renaming immutable bytes.

## Immediate planning work

1. Finish the finite toolchain/dependency audit and make ordinary development
   predictable, with explicit preparation and one supported generation.
2. Complete ownership-to-storage memory and essential library APIs using the
   compiler and tools as maintained consumers.
3. Demonstrate self-hosting and clean-install use, promote the selected generation
   after its exact qualification gate, then retire replaced active paths.
4. Apply the accepted [1.x stability and support policy](Windvale-1.0-Stability-And-Support-Policy.md)
   to the reconciled shipped API/target matrix before selecting a release candidate.
