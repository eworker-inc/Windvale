# Windvale progress

> Status: Current project snapshot as of 4 September 2026
> Authority: Informative; linked specifications and evidence own exact contracts
> Last reviewed: 2026-09-04

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
| Libraries 1.0 | Active | Foundation memory-budget, collection, byte-buffer, and builder contracts have focused implementations and fixtures. Candidate WVB 1.39 publishes all three immutable Option/Result payload projections and preserves exact borrowed identity across direct helper calls. The focused Windows publication checkpoint passes 39 cases, including native planner tests, structural mutations, deterministic output, and unchanged large borrow-free functions. The current database remains a useful bounded byte-oriented consumer. | Add complete WVB verification and runtime execution, then reproduce on Linux. Complete Option/Result exclusive borrow, take, and mapping before primitive ordering, collection mutation/slicing, and bounded byte construction. Migrate and qualify required real consumers afterward; the current database's passing storage suite does not prove those APIs or unsafe-region adoption. |
| WVDB 1.0 | Candidate | Upper-layer identity, tables, typed relationships, indexes, queries, transactions, storage profiles, types, documents/graphs, and backup direction are accepted. Existing storage and service slices remain useful implementation evidence. | Finish normative storage, durability, backup/restore, service, operations, and conformance contracts, then reconcile the implementation against them. |
| Packages and services | Active | Immutable packages, release admission, installers, offline activation, rollback, command resolution, and rights-limited execution are established foundations. | Define and qualify the complete 1.0 service lifecycle, support, migration, update, compatibility, and recovery promises. |
| Windvale OS | Ongoing | Probe 40 qualifies protected processes, capability IPC, bounded preemption, generation-safe memory reuse, exact WVB portability, and growing source ownership of the fixed process machine. Filesystem work has bounded host and FAT32 foundations. | Bind a surviving consumer and FAT32 media, enter the ready filesystem provider, complete one bounded guest read with rollback and teardown, then advance networking without claiming arbitrary application launch. |
| Windvale Shell | Candidate | The Shell 1 parser and portable `echo` path have paired evidence. A hosted exact-byte output and file-read target exists. | Package and resolve file-read, route the Workbench `cat` command through verified WVB, and later add an interactive or in-OS host deliberately. |
| Compute and efficiency | Program | Performance and memory are repository-wide requirements with a separate 2027 program. | Add improvements only through named workloads, measurements, resource bounds, regression thresholds, and reproducible public evidence. |

## WVB version scopes

Two active tracks intentionally use different bytecode generations:

- Windvale Seed and its frozen bootstrap/recovery path emit and consume their
  qualified WVB 1.11 contract.
- The evolving Language 1.0 compiler uses later versioned WVB contracts, with
  the executable structured-task slice at WVB 1.32 and the contained unsafe
  memory operations at WVB 1.37. The qualified source compiler publishes the
  authenticated call as WVB 1.38, the complete verifier admits its
  exact registered binding, the scalar provider executes it against private
  logical heap state, and the native x64 lowerer executes the same exact binding
  through its typed SysV ABI provider on Windows and Linux. WebAssembly and
  other native targets retain narrower declared boundaries.
- The Libraries 1.0 track has a source-publication candidate at WVB 1.39 for
  immutable Option/Result payload borrowing. Its source writer and bounded
  independent reader pass on Windows. The complete verifier and all execution
  consumers reject 1.39 until complete typed-stack and lifetime verification
  and runtime support land. Direct-call identity has local publication evidence,
  not execution admission. The verifier's small typed-directory component now
  preserves distinct borrowed payload identities and bounds-checks shape and
  instruction decoding. The control-phase component also checks that each
  payload owner is initialized on every path and cannot be overwritten or
  consumed after borrowing, including loops. These are component results,
  not complete 1.39 admission or call-lifetime verification.

The Language 1.0 track does not silently redefine the frozen Seed recovery
contract. A current document must name the track when a WVB version matters.

## What is not complete

Windvale 1.0 is not released. The required Libraries profiles, WVDB 1.0,
integrated services, support policy, and final whole-product Windows/Linux
qualification remain open. The completed Language 1.0 compiler qualification
does not substitute for those product gates.

Windvale OS does not yet provide arbitrary application launch, a live general
filesystem provider, a complete network stack, a general scheduler, broad
hardware support, or a desktop. The browser playground is not a Windvale OS
boot. Proposed agent-runtime and Observatory work is not an active release
claim.

## Immediate next results

Ordinary front-end verification now shares exact build products and selects
affected test-project inputs while executing the behaviors afresh. The Windows
checkpoint passed all 329 development claims in 29.94 seconds warm, versus
225.71 seconds while creating its project/package checkpoints. A changed parser
defect was rebuilt and rejected in 14.63 seconds; restoring it passed the focused
254-claim selection in 2.10 seconds. These are development observations, not a
clean-machine or paired-host qualification claim. See the
[focused evidence](../Evidence/2026-09-04-Front-End-Development-Product-Reuse.json).

Borrow-planner development now has a separate 16-case current-source check:
10.75 seconds while creating its small native package and 1.69 seconds warm.
A changed-source defect rebuilt and failed at the expected case. See the
[bounded planner evidence](../Evidence/2026-09-04-Foundation-Borrow-Plan-Development.json).

The separate cross-call publication checkpoint now passes 39 cases in 15.35
seconds warm. Constructing missing compiler packages and running the preceding
37-case selection took 29.02 minutes under an approved one-hour cap. Complete
products were retained; the expanded run reused them and freshly executed its
cases. These are local observations, not clean-machine or cross-host claims.
See the [publication evidence](../Evidence/2026-09-04-Foundation-Borrow-Cross-Call-Publication.json).

Verifier-directory development adds 24 WV cases through the same existing test
owner, using `--foundation-borrow-directories`. The small current-source package
took 12.72 seconds to construct and test, and 1.27 seconds warm with fresh
execution. Changed-file dispatch completed in 3.76 seconds with result reuse
disabled. Boundary tests cover 8,192 slots, exact borrowed identities,
truncation, invalid offsets, and instruction boundaries; a seeded defect was
rebuilt and rejected. Full-verifier source changes retain broader routing.
See the [directory evidence](../Evidence/2026-09-04-Wvb-Typed-Directory-Development.json).

The next control-phase checkpoint adds 18 WV owner-flow case groups through
`--foundation-borrow-owners`. It tests path-dependent initialization, permanent
owner freezing, branch joins, loops, bounded work, and the actual published
candidate's control phase. See the
[owner-flow evidence](../Evidence/2026-09-04-Foundation-Owner-Flow-Development.json)
for exact timings, changed-source rejection, and the remaining admission limits.

1. Finish complete WVB verification and runtime
   execution for candidate WVB 1.39 immutable Option/Result payload borrowing;
   reproduce it on Linux, then complete exclusive borrow, take, and mapping.
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
   6,061 declared cases. The accepted 5,981-case paired-host baseline supplies all 252 owner timings;
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
