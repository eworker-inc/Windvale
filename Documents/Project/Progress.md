# Windvale progress

> Status: Current project snapshot as of 3 September 2026
> Authority: Informative; linked specifications and evidence own exact contracts
> Last reviewed: 2026-09-03

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
| Language 1.0 | Candidate | The frozen design covers values, control flow, typed failure, generics, collections, ownership, borrowing, elastic memory budgets, hosted source access, callables, closures, and bounded structured tasks. The unsafe-memory path executes exact write-region validation and contained write-pointer derivation through candidate WVB 1.37 in the bounded scalar provider and native x86-64 lowering, without forming a host address. The authenticated three-argument Foreign invocation reaches deterministic [candidate WVB 1.38 publication](../Evidence/2026-09-03-Paired-Foreign-Wvb-1-38.json) as registered opcode `E0`; the complete compiler-aligned verifier admits it with affine-pointer containment; and the [source-built bounded scalar provider](../Evidence/2026-09-03-Authenticated-Foreign-Scalar-Execution.json) now executes both success and stale-generation outcomes on Windows. | Lower the verified WVB 1.38 call through the native ABI path, migrate one real boundary, and complete paired-host qualification. |
| Slice 8 source admission | Candidate | The target-aware admitter and independent authenticator validate the source, target, catalog, admission evidence, and no-retain/no-unwind facts. The generic-aware Analyzer now runs before the focused production builder constructs WVFB and pairs it with typed operation `190`; the coordinator independently validates, retains, and re-pairs that carrier before the emitter rechecks it and publishes candidate WVB 1.38. The complete verifier reconstructs the registered binding and consumes the pointer affinely. The production owner compiles authenticated generic source against the real Foundation Memory, Result, and Unsafe modules, then the bounded scalar provider executes both registered outcomes. The ordinary front door passes on Windows and Linux; the newer path passes locally on Windows. | Add native ABI execution, migrate a real system boundary, and reproduce the newer path on Linux. Decisions [0893](../Decisions/0893-Authenticate-Production-Source-Analysis-Ingress.md) and [0895](../Decisions/0895-Bind-Authenticated-Foreign-Declarations-In-A-Private-Compiler-Phase.md) remain Proposed. |
| Unsafe Foundation slice | Candidate | Canonical unsafe value types, scratch construction, immutable observation, affine mutable-region containment, exact write-region validation, and contained `Writeˉpointer::<Abi>` derivation execute through candidate WVB 1.37 in the scalar provider and native x86-64 lowering. The pointer remains a private logical descriptor with affine non-escape. Candidate WVB 1.38 consumes it in registered binding `1`, and the bounded scalar provider validates the live allocation and exact capacity without forming a native address. | Lower the registered call through the native ABI, migrate a real boundary, execute on Linux, and complete paired-host qualification. |
| Compiler scale | Candidate | The promoted segmented toolset, WVB-to-WVO lowerer, and WVB runner reconstruct their current Windows and Linux candidates byte for byte on Windows. Relocation-free terminal publication and the 50,761,605-byte compiler-scale object are covered. Ordinary native-lowerer edits now build the current 648-function compiler once and run a 13-case current-source development gate in about 32 seconds on a warm Windows cache. Split-project compilation also preserves an independently validated [source/symbol checkpoint](../Evidence/2026-09-03-Resumable-Compiler-Symbol-Checkpoint.json) when later analysis fails, so a safe retry need not repeat completed symbol work. | Reproduce the current candidates on Linux, complete source-compiler convergence, profile and improve remaining cold analysis/emission latency, and run paired-host qualification. |
| Libraries 1.0 | Active | Foundation memory-budget, collection, byte-buffer, and builder contracts have focused implementations and fixtures. An unqualified source checkpoint provides canonical Option/Result case predicates and adds direct-owner immutable payload borrowing through compiler-only WVIR 1.33/1.34 views. The validator freezes the owner, propagates non-owning payload provenance, and rejects escape, duplication, mutation, spoofed identities, and serialization. The current database remains a useful bounded byte-oriented consumer and recompiles through the current compiler. | Reconstruct and execute the checkpoint through the complete front door, then specify and implement its verified WVB/runtime representation. Complete Option/Result take and mapping operations before moving through primitive ordering, collection mutation/slicing, and bounded byte construction. Migrate and qualify required real consumers afterward; the current database's passing storage suite does not prove those APIs or unsafe-region adoption. |
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
  the executable structured-task slice at WVB 1.32 and the current unsafe-memory
  native execution checkpoint at candidate WVB 1.37. The compiler-aligned
  scalar and native x86-64 paths execute bounded write-region validation and
  contained pointer derivation without forming an address. The source compiler
  additionally publishes authenticated and paired calls as candidate WVB 1.38,
  the complete verifier admits the exact registered call with affine-pointer
  containment, and the source-built bounded scalar provider executes binding
  `1` against private logical heap state. Native x86-64, WebAssembly, and other
  consumers retain narrower declared boundaries.

The Language 1.0 track does not silently redefine the frozen Seed recovery
contract. A current document must name the track when a WVB version matters.

## What is not complete

Windvale 1.0 is not released. Language 1.0, the required Libraries profiles,
WVDB 1.0, integrated services, support policy, and final Windows/Linux
qualification remain open.

Windvale OS does not yet provide arbitrary application launch, a live general
filesystem provider, a complete network stack, a general scheduler, broad
hardware support, or a desktop. The browser playground is not a Windvale OS
boot. Proposed agent-runtime and Observatory work is not an active release
claim.

## Immediate next results

1. Lower the verified WVB 1.38 call through the native ABI without pointer
   escape or implicit authority.
2. Migrate one real system boundary and reproduce the newer Slice 8 path on
   Linux.
3. Continue the required Libraries and WVDB 1.0 specifications through useful
   consumers.
4. Run one real bounded Windvale OS filesystem-provider request with complete
   rollback and teardown.
5. Preserve predictable development feedback and reserve full qualification for
   deliberately selected release, security, bootstrap, ABI, or conformance
   states.

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
