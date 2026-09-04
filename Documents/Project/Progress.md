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
| Libraries 1.0 | Active | Foundation memory-budget, collection, byte-buffer, and builder contracts have focused implementations and fixtures. An unqualified source checkpoint provides canonical Option/Result case predicates and adds direct-owner immutable payload borrowing through compiler-only WVIR 1.33/1.34 views. The validator freezes the owner, propagates non-owning payload provenance, and rejects escape, duplication, mutation, spoofed identities, and serialization. The current compiler passes the focused 172-case ownership, Vector, `using`, resource, asynchronous-call, and structured-task execution owner. The current database remains a useful bounded byte-oriented consumer and recompiles through the current compiler. | Reconstruct and execute the checkpoint through the complete front door, then specify and implement its verified WVB/runtime representation. Complete Option/Result take and mapping operations before moving through primitive ordering, collection mutation/slicing, and bounded byte construction. Migrate and qualify required real consumers afterward; the current database's passing storage suite does not prove those APIs or unsafe-region adoption. |
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

1. Finish the verified WVB/runtime representation for direct-owner immutable
   Option/Result payload borrowing, then complete take and mapping operations.
2. Continue required Libraries 1.0 through primitive ordering, collection
   mutation and slicing, bounded byte construction, and real consumers.
3. Advance the remaining WVDB 1.0 specifications and reconcile its useful
   existing implementation against them.
4. Run one real bounded Windvale OS filesystem-provider request with complete
   rollback and teardown.
5. Preserve predictable development feedback and reserve full qualification for
   deliberately selected release, security, bootstrap, ABI, or conformance
   states.
6. Reduce the `database-storage` qualification owner's cold cost. Its repaired
   57-case path passed on both hosts in the final gate, but took about 21 minutes
   on Debian and 41 minutes on Windows. Correctness is qualified; removing or
   caching redundant work and keeping active-case timeout evidence remain a
   verification-workflow performance task.

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
