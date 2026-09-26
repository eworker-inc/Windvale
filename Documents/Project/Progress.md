# Windvale progress

> Status: Current project snapshot as of 26 September 2026
> Authority: Informative; linked specifications and evidence own exact contracts
> Last reviewed: 2026-09-26

<a href="Images/Windvale-Roadmap-August-2026.svg"><img src="Images/Windvale-Roadmap-August-2026.svg" alt="Dated August 2026 Windvale roadmap phase map" width="100%"></a>

Windvale is building directly toward the integrated 1.0 host product. The signed
`v0.1.0` preview remains the completed public foundation; no `v0.2.0` product
release is planned. Windvale OS continues on its own qualification path and is
not an undeclared requirement for the Windows and Linux 1.0 product.

This page answers three questions: what works now, what is still missing, and
what result comes next. The [roadmap](Roadmap.md) owns forward gates. The
[verification evidence](Seed-Verification-Evidence.md) and
[Language 1.0 migration evidence](Windvale-Language-1.0-Migration-Evidence.md)
retain exact runs, hosts, artifact sizes, and hashes. The
[Language 1.0 Slice 8 qualification record](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json)
owns the final paired-host compiler result. The
[historical progress snapshot](Progress-History-2026-08-31.md) retains the
earlier detailed implementation diary.

The image is an editorial snapshot, not a generated status report. Update this
page whenever standing changes; refresh the image only when it becomes
materially misleading.

## What works today

- Windvale Seed source compiles to canonical WVB, which is verified and runs on
  Windows and Linux. The native toolchain also assembles WVA, verifies WVO,
  links images, and packages supported native applications.
- The Windvale-written Seed compiler reaches byte-identical Stage 1 and Stage 2
  results on Windows and Debian from its committed source inventory.
- Interpreter, deterministic AOT, baseline-JIT, WebAssembly, object, linker,
  hosted-container, and OS execution paths support their documented subsets.
- The repository's normal build and verification workflow is native-only. The
  qualified managed Stage 0 survives only in its immutable recovery release.
- The signed `v0.1.0` preview provides installers, an offline verifier, release
  evidence, explicit capability approval, and a package-backed WVDB Query
  application.
- The offline package lifecycle admits two packages, activates an immutable
  generation, recovers an interrupted update, rolls back, and removes
  package-owned state while preserving application data.
- Two canonical portable WVB applications have qualified execution evidence on
  Windows, Linux, and Windvale OS.
- The static browser playground compiles, verifies, and runs supported source
  entirely in the browser through the pinned WebAssembly path.

## Current work

| Boundary | Standing | What works | What is missing or next |
| --- | :---: | --- | --- |
| Language 1.0 | Qualified | The frozen design covers values, control flow, typed failure, generics, collections, ownership, borrowing, elastic memory budgets, hosted source access, callables, closures, bounded structured tasks, contained unsafe memory, and authenticated Foreign calls. [Decision 0943](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md) accepts the exact compiler state after all 126 native owners and 5,981 cases passed on each host, deterministic reconstruction passed on Windows and Debian, and both declared WebAssembly subsets passed. | The frozen compiler track is complete. New semantics or wider target promises require a new versioned contract; the product critical path now continues in Libraries 1.0. |
| Slice 8 source admission | Qualified | The target-aware front door authenticates, analyzes, pairs, emits, verifies, lowers, assembles, links, packages, and executes registered Foreign calls without pointer escape or ambient authority. The real Linux system-profile record consumer uses the canonical Foundation Memory, Result, and Unsafe modules and passes within the [final paired-host gate](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json); separate native ABI cases qualify the Windows path. | Complete. Preserve this evidence unless a declared source, WVB, containment, ABI, or qualification input changes. Decisions [0893](../Decisions/0893-Authenticate-Production-Source-Analysis-Ingress.md) and [0895](../Decisions/0895-Bind-Authenticated-Foreign-Declarations-In-A-Private-Compiler-Phase.md) remain historical proposals rather than alternate compilers. |
| Unsafe Foundation slice | Qualified | Canonical unsafe value types, scratch construction, immutable observation, affine mutable-region containment, exact write-region validation, and contained `Writeˉpointer::<Abi>` derivation execute through WVB 1.37 and are consumed immediately by registered WVB 1.38 bindings. The real record consumer preserves the exact binding, target, lifetime, and authority boundary through native execution. | The bounded compiler/runtime contract is complete. Future library APIs must reuse it without widening authority. |
| Compiler scale | Qualified | The promoted segmented toolset, WVB-to-WVO lowerer, and WVB runner reconstruct byte for byte. Relocation-free terminal publication and the 50,761,605-byte compiler-scale object are covered. The self-hosted analyzer and emitter reproduce the exact WVB runner and application, resumable symbol checkpoints fail closed, and the final gate reconstructs the compiler independently on Windows and Debian. | Correctness and deterministic reconstruction are complete for Language 1.0. Cold analysis, emission, and qualification latency remain performance work, not an open compiler-semantic gate. |
| Libraries 1.0 | Active | The package parser and immutable-borrow consumer are delivered. Candidate WVB 1.42 adds focused Windows/Debian evidence for Copy-record collections and owned-budget helpers. | Complete the typed Package-Lock consumer, then the remaining accepted library rows and native/installed qualification gates. The [completion matrix](Compiler-Tools-And-Libraries-1.0-Matrix.md#current-checkpoint-and-next-action) tracks scope; the [completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md) owns current blockers. The [six-item simplification goal](Verification-Throughput-Plan.md#six-item-simplification-goal) addresses diagnostics, feedback time and maintenance overhead. |
| Project 4 build integration | Development | The selected package parser/lock source closure now builds through explicit-target Project 4 manifests and current-source native publication. Focused Windows/Debian launcher, replacement, malformed-output, and consumer cases are recorded in the [integration evidence](../Evidence/2026-09-15-Source-Edition-Package-Integration.json). | Installed compiler and publisher promotion, full transaction fault-injection qualification, and complete suite migration remain open; the selected package-parser delivery gate is closed. |
| WVDB 1.0 | Candidate | Upper-layer identity, tables, typed relationships, indexes, queries, transactions, storage profiles, types, documents/graphs, and backup direction are accepted. Existing storage and service slices remain useful implementation evidence. | Finish normative storage, durability, backup/restore, service, operations, and conformance contracts, then reconcile the implementation against them. |
| Packages and services | Active | Immutable packages, release admission, installers, offline activation, rollback, command resolution, and rights-limited execution are established foundations. | Define and qualify the complete 1.0 service lifecycle, support, migration, update, compatibility, and recovery promises. |
| Windvale OS | Ongoing | Probe 40 qualifies protected processes, capability IPC, bounded preemption, generation-safe memory reuse, exact WVB portability, and growing source ownership of the fixed process machine. Filesystem work has bounded host and FAT32 foundations. | Bind a surviving consumer and FAT32 media, enter the ready filesystem provider, complete one bounded guest read with rollback and teardown, then advance networking without claiming arbitrary application launch. |
| Windvale Shell | Candidate | The Shell 1 parser and portable `echo` path have paired evidence. A hosted exact-byte output and file-read target exists. | Package and resolve file-read, route the Workbench `cat` command through verified WVB, and later add an interactive or in-OS host deliberately. |
| Compute and efficiency | Program | Performance and memory are repository-wide requirements with a separate 2027 program. | Add improvements only through named workloads, measurements, resource bounds, regression thresholds, and reproducible public evidence. |

## WVB version scopes

Name the track when a bytecode version matters:

- Seed and its frozen bootstrap/recovery path retain qualified WVB 1.11.
- The qualified Language 1.0 source compiler includes authenticated Foreign
  calls through WVB 1.38. Other execution targets retain their declared subsets.
- Libraries 1.0 development extends immutable borrowing and owned collections
  through candidate WVB 1.42. Focused compiler, verifier and interpreter evidence
  does not establish native lowering or installed-toolchain promotion for those
  later versions. The [completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md)
  owns the exact current boundary and next consumer.

The [dated library history](Library-Development-History-2026-09-26.md) preserves
the earlier 1.39 checkpoint details and delivery measurements. Later development
does not silently redefine frozen Seed or Language 1.0 qualification identities.

## What is not complete

Windvale 1.0 is not released. The required Libraries profiles, WVDB 1.0,
integrated services, and final whole-product Windows/Linux
qualification remain open. The completed Language 1.0 compiler qualification
does not substitute for those product gates.

[Support policy accepted](Windvale-1.0-Stability-And-Support-Policy.md): each
official minor line receives 12 months of fixes through its latest patch.

Windvale OS does not yet provide arbitrary application launch, a live general
filesystem provider, a complete network stack, a general scheduler, broad
hardware support, or a desktop. The browser playground is not a Windvale OS
boot. Proposed agent-runtime and Observatory work is not an active release
claim.

## Immediate next results

The [six-item simplification goal](Verification-Throughput-Plan.md#six-item-simplification-goal)
is improving diagnostics, preparation reuse, code structure and maintenance.
Use that plan for its implementation status and the [native test runbook](../Runbooks/Native-Tests.md)
for current verification commands. Earlier feedback timings and component case
inventories are preserved in the [dated history](Library-Development-History-2026-09-26.md#earlier-development-feedback-checkpoints).

1. Complete the typed Package-Lock consumer after the delivered parser gate.
   Record-vector observation and owned-budget helpers have focused evidence;
   the [completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md)
   owns the remaining emission blocker and integration work. Preserve lock
   bytes, failure order and resource bounds. Native lowering, installed
   promotion and independent qualification remain separate gates.
2. Continue required Libraries 1.0 through primitive ordering, collection
   mutation and slicing, bounded byte construction, and real consumers.
3. Advance the remaining WVDB 1.0 specifications and reconcile its useful
   existing implementation against them.
4. Run one real bounded Windvale OS filesystem-provider request with complete
   rollback and teardown.
5. Execute the [verification throughput redesign](Verification-Throughput-Plan.md):
   make ordinary affected feedback complete in seconds where practical, make
   cold qualification scale with unique construction plus behavior rather than
   cases multiplied by the complete pipeline, and reserve full qualification
   for deliberately selected release, security, bootstrap, ABI, or conformance
   states. The new complete-work planner inventories all 126 native owners and
   6,193 declared cases. The accepted 5,981-case paired-host baseline supplies all 252 owner timings;
   it revealed that equal profile totals still projected a 6,547,869 ms critical
   shard. Six scheduling-only owner moves reduce that historical projection to
   4,655,707 ms, 28.90 percent lower and 4.01 percent above the arithmetic lower
   bound, without changing an owner, case, command, profile, or timeout. A new
   paired run must measure that projection. Consolidation proceeds by unique
   failure signal and measured critical-path contribution.
   Compatible-state development reuse now preserves an unaffected passing owner
   across planner-proved unrelated trees: a six-case owner executed in 15.68
   seconds, then reused that receipt after four documentation-only changes.
   Qualification remains fresh.
6. Finish the cold `database-storage` qualification-workflow repair. Development
   now uses a shared 53-case inventory and exact multi-target unions: the
   publication/recovery/single-writer set passed three cases in 77.03 seconds,
   while the portable and hosted local-service closure passed its three cases in
   193.52 seconds. Oversized focused plans now stop at the ten-minute development
   budget unless a longer run is explicit. Cold qualification now has one shared
   host inventory with 57 logical cases in 54 execution steps; six safe case
   pairs share products, and portable steps delegate unused opposite-host
   packaging plus private A/B construction. The publication/recovery product
   passed the resulting current-host path in 38.31 seconds, down from 46.15
   seconds before construction delegation. The ancestor-groups and ancestor-pages
   bundles also passed their two retained cases in 51.64 and 61.15 seconds.
   Development binds all six bundle memberships to qualification and plans all
   53 behaviors as 47 executions. The publication/recovery pair took
   44.85 seconds to create its development checkpoints and 2.13 seconds on the
   unchanged warm path; one-member selections remained independent. The first
   non-identical overlap bundle now combines transaction leaf groups and pages;
   it passed the focused qualification path in 56.14 seconds and its warm
   development path in 2.99 seconds without raising a capacity limit. The
   root-split/depth-two bundle passed qualification in 59.99 seconds and its warm
   development path in 2.86 seconds, also without raising a capacity limit.
   Build-once dependency reuse, hosted
   ownership, capacity-aware bundling, complete Windows/Linux bounds, and
   structured timeout results remain.

## How to verify ordinary work

After a coherent edit, run one change-aware verifier:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

Use the focused owner selected for the changed boundary. Do not run development
and complete qualification as a ladder against the same unchanged source.

## Evidence and history

- [Exact Seed and release evidence](Seed-Verification-Evidence.md)
- [Language 1.0 migration evidence](Windvale-Language-1.0-Migration-Evidence.md)
- [Detailed progress history through this reorganization](Progress-History-2026-08-31.md)
- [Release naming and recovery policy](Release-Names-And-Tags.md)
- [Windvale 1.0 product gate](Windvale-1.0-Product-Plan.md)
