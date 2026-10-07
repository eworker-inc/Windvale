# Compiler, tools, and libraries completion plan

> Status: Current language, toolchain and essential-library delivery plan
> Authority: Informative; accepted specifications and decisions own contracts
> Last reviewed: 2026-10-07

Deliver one usable Windvale language, compiler/runtime and essential library set
on Windows and Debian/Linux. The maintainer selected this scope on 3 October
2026 in the [language and essential-library decision](../Decisions/0976-Focus-Windvale-1.0-On-The-Language-And-Essential-Libraries.md).
The compiler and its tools are the primary maintained consumers. WVDB,
secondary applications and broader platform proposals are outside the critical
path; they may be migrated or rebuilt later.

The [product plan](Windvale-1.0-Product-Plan.md) owns release gates. The
[completion matrix](Compiler-Tools-And-Libraries-1.0-Matrix.md) tracks accepted
Foundation declarations, proposed APIs and target/evidence gaps. The
[throughput plan](Verification-Throughput-Plan.md) owns feedback measurements.
This plan does not accept draft signatures or rewrite frozen source semantics.

## What works and what remains

| Area | Current evidence | Delivery gap |
| --- | --- | --- |
| Language/compiler | [Slice 8 qualification](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md) establishes the exact source compiler and target subsets. Current analysis/emission and admission tools are written in Windvale. | Complete later compiler/runtime integration and promote one selected generation; the qualified source scope is not general memory or installed qualification. |
| Values and collections | The maintained package parser uses canonical Option and immutable borrowing. Candidate interpreter support adds Copy-record collections and owned-budget helpers. | General owner-bearing aggregates, wider mutation/borrowing, consuming value operations and complete public collection APIs remain open. |
| Native memory | Candidate physical storage, budgets, scalar Vectors and owned-console startup have focused Windows/Debian evidence. The WVB 1.44/1.45 path now connects reserved byte builders, shared values, supported aggregate/call cleanup and the compiler serializer through ABI 25/context 11; both compiler hosts have development reconstruction evidence and the selected source/serializer/staging checks pass on both hosts. | Finish compiler-scale self-lowering, then close broader scope exits, arbitrary owner aggregates, collection/view APIs, hosted startup and interpreter working storage. Pinned tools retain their declared older profiles. |
| Tools | Native build, admission, execution, assembler, linker and packaging paths exist; prepared current compiler products can be reused. Current-source verification has explicit preparation and an ordinary command that refuses construction. | Inspection/execution still select pinned generations. Publisher/lowerer preparation and affected compiler rebuild cost need a single explicit workflow. |
| Libraries | Accepted Foundation declarations and reusable implementation leaves exist. | Finish the selected essential API/target mapping and drive integration with maintained compiler/runtime consumers. A filename or isolated fixture cannot close a row. |
| Delivery | The released preview has installation, offline verification and package lifecycle evidence. | The newer compiler/runtime is not the preview installation. Self-host and qualify the selected current generation, then prove clean-install use. |

The current compiler checkpoint connects its retained compilation plan to native
chunked output. Its own object is about 55 MiB, beyond the single byte-result
entry's 4 MiB response ceiling. The candidate
[retained publication session](../../Specifications/Windvale-Native-Compiler-Publication.md)
keeps one plan alive, copies bounded pieces and releases each temporary result.
Small Windows and real Debian execution now produce the same object bytes as
ordinary output, pass independent linker admission and check malformed refusal,
write-failure cleanup and exact input-path collision. Both source reconstructions
produce matching portable compiler bytecode and object chunks. Selected source
memory, serializer and staging checks now pass on Windows and real Debian,
using resumable verification with preserved intermediate products. A
compiler-scale native run reached its 15-minute deadline before publishing a
chunk; preparation performance and exact compiler-scale self-lowering remain
open. This does not complete the broader memory, essential-library, installation
or retirement work below.

## Supported toolchain and retirement map

This map records inspected source entry points at the start of the new scope.
It is a dependency audit, not fresh qualification of every listed artifact.
Current means the forward owner; required bootstrap means a construction input
that must be replaced before deletion. A superseded active command may still
need migration even though it is no longer the selected forward implementation.

| Component | Classification and observed role | Replacement or retirement condition |
| --- | --- | --- |
| `Compiler/Windvale` and the Project 4 analysis/emission/admission projects | Current source-language implementation. | Extend this owner; no parallel forward compiler. Reconstruct and promote its selected generation. |
| `Build-Wvb-Project4.mjs` and `Build-Current-Split-Project-Wvb.mjs` | Current development path. Prepared native Windvale tools perform compilation/authentication; Node owns orchestration and cache handling. Project builds refuse a missing compiler checkpoint. | Make all required tool preparation explicit and select the same generation through ordinary commands. |
| `Current-Split-Compiler-Cache-Core.mjs`, `Source-Edition-Predecessor-Core.mjs` and `Direct-Condition-Analyzer-Intermediate-Core.mjs` | Required bootstrap construction. Project 4 reconstruction uses the exact recorded predecessor Git tree, builds the bounded direct-condition Analyzer intermediate, then builds full current analysis/emission and admission products. [The cache contract](../../Specifications/Compiler-Split-Development-Cache.md#reusable-current-compiler-pair) owns the adaptation, identities and deadlines. | Retire these construction edges only after an independently reconstructed successor builds the full current source without them. Keep exact recovery provenance until then. |
| `Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap` and `Artifacts/Native-Segmented-Compiler-Toolset-Candidate` | Required pinned construction inputs. Packaging/staging/link/transport tools participate in current compiler reconstruction and cache identity. | Replace and qualify their exact construction edges before removing an artifact family or its wrapper. |
| `Build-Wvb.cmd/.sh`, `Build-Current-Wvb.cmd/.sh` | Mixed active front doors. Project 4 dispatches to current-source orchestration; older project manifests select different pinned build drivers. | Unify supported forward dispatch and document explicit bootstrap/recovery use. Retire redundant wrappers only after their supported callers migrate. |
| `Verify-Wvb.mjs --current` | Current-source admission using prepared Windvale verifier products. [The runbook](../Runbooks/Native-Source-To-Wvb.md#current-source-verification) separates bounded preparation from ordinary checking. | Qualify installed delivery and migrate supported callers before removing a required pinned verifier. |
| `Verify-Wvb.cmd/.sh`, `Inspect-Wvb.cmd/.sh`, `Run-Wvb.cmd/.sh` | Pinned admission/inspection/runner paths retained for named bootstrap and qualification callers. Current inspection and interpreter delivery still need integration. | Select current ordinary inspection/execution products for the promised version/target scope before retiring older defaults. Preserve named construction and historical qualification cases. |
| Current `Lower-Wvb-To-Wvo.mjs` and `Package-Console --owned` | Current candidate lowering/startup for declared subsets; older pinned lower/package wrappers remain construction inputs. | Promote only after declared memory, ABI, host and reconstruction gates. The owned console profile does not supply hosted services or immutable byte/text backing. |
| Historical byte/text arenas and checkpoint/loop compaction | Active limited storage mechanics, shared by pinned tools and parts of current native lowering. | Replace general lifetime management with correct ownership/accounting. Keep arenas where their bounded scratch or typed-arena contract is appropriate; do not delete them by name or reset live aliases. |
| `Foundation/` leaves and `Libraries/Foundation/` modules | Mixed reusable implementation leaves and canonical forward public owners. | Inventory by accepted operation; reuse or extract one implementation, then remove superseded duplicates after compiler/tool consumers migrate. |
| Managed Stage 0 | Recovery-only; C#/.NET source is absent from `main`. | Preserve the immutable [Stage 0 recovery procedure](../../Bootstrap/Stage0/README.md); do not restore managed semantics to the active compiler. |
| WVDB, secondary applications, full Backend/OS/2.0 lanes | Parked outside this delivery. Existing contracts/evidence remain. | Revisit after language/library delivery; application compatibility does not justify keeping obsolete active implementations. |

The forward completion scope retires the unused monolithic source-memory
adapter/project and the two byte-packet staging test adapters/projects. Their
only memory-compiler regeneration caller reads the historical browser compiler
source from Git; the pinned browser artifacts and that regeneration route remain
unchanged. The current split compiler is the maintained source-compilation path.
The portable compiler-core APIs remain available to their supported callers.

The older memory-adapter section of the identity-preserved
[source-to-WVB specification](../../Specifications/Compiler-Source-Wvb.md#portable-in-memory-adapter)
describes the historical WVSS1/WVCO consumer, rather than a forward Project4
entry point. The two retained scalar staging fixtures keep their six Relocations
and seven Symbols assertions under the existing mixed native owner. They require
explicit current-product preparation and the true compiler host before ordinary
console execution; the typed staging parsers, SHA helper and Relocations bridge
remain maintained construction dependencies. Passing parser construction alone
does not establish execution of those thirteen assertions.

The immediate audit found delivery generations and older storage mechanics,
not an active C# compiler competing with the Windvale compiler. Host PowerShell,
command/shell and Node tooling are explicit development dependencies. Replacing
all host orchestration before useful Windvale libraries exist is not a delivery
requirement. Do not treat browser JavaScript as source-language semantics.

## Ordered delivery phases

### 1. Align scope and finish the finite dependency audit

Update the handbook, README, product/roadmap and API inventory to the accepted
scope. Trace normal commands and their compiler, runtime, package and bootstrap
inputs. For each retirement candidate identify its supported caller, successor
and focused evidence needed before removal. Keep this map actionable rather
than creating another speculative architecture backlog.

Exit: one direction and an explicit supported/current-versus-bootstrap map,
with WVDB and secondary consumers removed from automatic completion gates.

### 2. Establish ordinary development feedback

Use the selected current compiler through a documented normal workflow. Prepare
compiler, verifier/interpreter, publisher and lowerer products separately with
finite deadlines; ordinary commands consume prepared inputs and give actionable
cache-miss diagnostics. Reuse valid caches and unaffected analysis. Introduce
component-only rebuilding only where dependencies and measurements justify it.

Exit: record cold preparation separately from warm edit/build/verify/run cost
for a small ordinary source program and a representative compiler edit. The
normal path must execute current Windvale components without hidden compiler
reconstruction. Keep security, identity and native publication checks intact.

### 3. Complete ownership-to-storage memory

The maintainer's 27 September memory priority remains. The
[handbook](../../AGENTS.md#memory-management-current-limitations-and-target)
owns the exact rules. Current byte/text call-return and eligible loop compaction
are bounded mitigations, not last-share ownership. The
[native lowering specification](../../Specifications/Windvale-Native-X64-Lowering.md)
owns their conservative shapes and bounds.

The candidate [owned-storage provider](../../Specifications/Windvale-Native-Owned-Storage.md),
[budget adapter](../../Specifications/Windvale-Native-Budgeted-Storage.md),
[collections](../../Specifications/Windvale-Native-Owned-Collections.md),
[fresh domain](../../Specifications/Windvale-Native-Owned-Domain.md) and
[ordinary console startup](../../Specifications/Windvale-Native-Owned-Console-Application.md)
are integration groundwork. Scalar Vector helper reads, replacement and
reserved append use the same physical and budget domain. These bounded profiles
still lack general owner aggregates, shared immutable backing and the hosted
storage needed by the compiler/interpreter.

Carry ownership and cleanup through calls, assignments, last uses, aggregate
transfers, failure propagation and terminal domain teardown. Connect physical
reuse to budgets/leases and preserve every owner on allocation refusal. Bound
native scratch and interpreter working state separately from guest allocation.

The maintainer selected larger coherent implementation paths on 3 October 2026.
Source analysis is a development checkpoint within a memory change, not its
completion milestone. The next path connects reserved byte construction and
shared immutable backing through the existing compiler, verifier and execution
owners, then migrates an actual compiler serializer and its budget-bearing
entry/callers. An unused helper or a separate probe does not establish consumer
adoption. Preserve owning byte slices, ordinary alias survival, aggregate fields,
charged empty values, refusal and failure cleanup while replacing the storage.

During this path, use the narrow checks needed to diagnose implementation defects
and preserve valid construction checkpoints. After the compiler/runtime/consumer
change is coherent, run one causal Windows and Debian final plan. Repeated
selection of the same bundle and checks that execute unaffected pinned tools do
not add evidence for the changed path. Later memory APIs reuse the same ownership,
storage and accounting machinery; the full memory exit below remains required.

Exit: maintained compiler/parser/collection consumers use the path. Repeated
allocation/release, alias survival, refusal and failure cleanup pass on Windows
and real Debian. Measure live, reusable, cumulative and budget-charged storage
separately from process peak memory. Fixed-live-state workloads stabilize at a
justified bound as iterations increase.

### 4. Deliver essential libraries through toolchain consumers

Finish accepted Option/Result operations, numeric/ordering, collections,
bytes/text builders and resources required by the selected delivery profile.
Use compiler lexing/parsing, symbol and analysis collections, interpreter state
and bytecode/object writing as maintained consumers. Keep scalars and suitable
records inline; use owned buffers/builders for mutable working state and bytes
for actual serialized formats.

For each API identify accepted declaration, implementation, package/identity,
consumer, target and evidence in the matrix. Reuse existing leaves rather than
adding duplicate general parsers or writers. Supply explicit file/directory and
publication boundaries needed by the compiler; wider database/network/service
APIs remain deferred. Resolve a draft API at its concrete dependency boundary.

Exit: normal compiler/toolchain workloads exercise the selected APIs on both
hosts with finite time/memory, malformed-input, deterministic-output, refusal,
borrow-invalidation and cleanup evidence. Required package functionality stays
usable; the typed Package-Lock refactor is a possible later consumer.

### 5. Demonstrate self-hosting and clean-install use

Select one integrated compiler/runtime/library generation. Reconstruct it
independently and deterministically on Windows and Debian from declared inputs.
Run its required promotion qualification before updating ordinary installed
identities. Prove installation, build, verification, inspection, assembly/link,
packaging, execution, update and recovery without development-checkout
conventions. Keep editor/browser expansion outside this gate.

Exit: the selected generation is usable through the same documented commands
on clean supported hosts, with exact API/target and dependency promises.

### 6. Retire superseded active paths and close the product gate

After each successor is usable and verified, remove redundant source, wrappers,
obsolete compatibility and application-specific implementations no longer
needed by the supported toolchain. Preserve immutable recovery and named exact
qualification inputs. Secondary applications may be rebuilt later.

Reconcile support/security/compatibility and the selected package/target matrix.
Qualify the exact product state and prepare identified distribution artifacts.
Release signing/publication is separate from implementation authorization.
Completion means the [product release gate](Windvale-1.0-Product-Plan.md#windvale-10-release-gate)
passes; neither the audit nor a narrow memory fixture establishes that result.

## Completed package checkpoint and deferred consumer

<a id="active-milestone-package-parser-with-immutable-borrowing"></a>

The maintained package parser already uses canonical `Option<u64>` and
immutable payload borrowing without changing decimal syntax, overflow failure
or package bytes. Its selected source/publication/consumer gates are recorded
in the [package integration evidence](../Evidence/2026-09-15-Source-Edition-Package-Integration.json).
The [projected Vector evidence](../Evidence/2026-09-22-Borrowed-Vector-Payloads.json),
[Copy-record collection evidence](../Evidence/2026-09-25-Copy-Record-Collections.json)
and [owned-budget helper checkpoint](../Evidence/2026-09-26-Development-Stabilization.json)
retain their exact implemented scopes.

The typed Package-Lock directory is not delivered and is no longer the mandatory
next consumer. Its saved probes and deferred-write measurements do not prove
native memory reuse or maintained delivery. Earlier detailed planning is
retrievable from the [pre-scope completion plan](https://github.com/eworker-inc/Windvale/blob/f618a3fb2a06347d2460e4aa06998a486752e48e/Documents/Project/Compiler-Tools-And-Libraries-Completion-Plan.md).
Preserve package functionality needed by the toolchain; return to this refactor
only when its supported library path and benefit are clear.

## Verification rhythm

Keep one feature chunk active and distinguish implementation, focused checks,
paired-host qualification, installed promotion and release. Extend existing
focused owners for distinct failure signals rather than adding replay wrappers.
Inspect `Verify-Changed.ps1 -PlanOnly` after a coherent edit and execute one
causal final selection. Documentation-only scope changes need documentation,
link/path and whitespace checks, not compiler or database reconstruction.

Preserve passing evidence by complete declared inputs; rerun only invalidated
owners. Count compilation/preparation in the ten-minute ordinary feedback
budget. Longer justified local runs have
[standing maintainer approval](../../AGENTS.md#testing-and-verification): state
the exact command, purpose, expected duration and finite maximum before launch,
stop at the deadline and report incomplete work honestly. Duration approval does
not accept draft contracts or waive qualification.

Before a selected compiler/runtime promotion, run its independent paired-host
qualification against exact inputs. Before release, close and qualify the final
product gate. Do not repeat broad checks merely for documentation, commits or
pushes; commit coherent verified changes and push both configured remotes.
