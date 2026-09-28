# Agent Handbook

## Purpose

This document gives people and AI agents the durable rules needed to develop Windvale safely and coherently.

Windvale is a source-available AI-led research and development project constructing a small computing stack: language, compiler, bytecode, runtime, assembler, object model, linker, foundation library, tools, and operating system. The goal is not merely to generate code with AI. The result must be understandable, testable, reproducible, and useful independently at each layer.

The signed `v0.1.0` preview completed Windvale's initial feasibility phase. Do not describe the project as an experiment. Individual proposals, prototypes, profiles, or measurements may still be experimental when their exact status and limits are stated.

## Quick start

- Read this file before making non-trivial changes.
- Follow `CONTRIBUTING.md` for Contributor License Agreement acceptance, DCO sign-off, provenance, and pull-request requirements.
- Use the active checkout and its configured repository remote; local worktree and mirror paths are environment-specific.
- Use `main` as the default branch until the repository establishes a different branching policy.
- Before changes, run `git status --short --branch` and `git pull --ff-only`.
- Preserve unrelated work and stage only files belonging to the current task.
- Do not add implementation merely to make a design document appear complete.
- Do not commit secrets, private keys, credentials, local SDK installations, build caches, virtual disks, firmware images, or machine-specific configuration.

## Progress reporting

- Give a concise progress update before starting tool-driven work and at least once every 60 seconds while long-running work continues.
- When the work has a known set of phases, report progress as `current/total` and an approximate percentage, such as `phase 4/6 (67%)`, and name the active phase.
- When the remaining work cannot be estimated honestly, report the current named phase, the completed phases, and the next checkpoint instead of inventing a percentage.
- Long-running repository scripts should emit bounded phase or item progress before buffered summaries so a person can distinguish active work from a stalled process. Prefer stable `step=<name>`, `item=<current>/<total>`, or percentage fields that automation can ignore safely.
- Update the phase count when scope changes materially, and make clear whether a failure is local to the current phase or invalidates earlier completed evidence.

## Communication clarity

- Lead with the practical outcome in plain language before implementation detail.
- Explain unavoidable technical terms when they first appear, and use a concrete example or short analogy when it makes the boundary easier to understand.
- Clearly separate what already works, what is still missing, and what will be built next.
- Present complex work in layers: first the product behavior, then the safety or performance reason, and finally the internal mechanism for readers who need it.
- Keep explanations simple without weakening exact contracts, evidence, limitations, or failure behavior.

## Performance and memory discipline

- Treat performance and memory use as product requirements across Windvale-generated code, compilers, runtimes, verifiers, development workflows, tests, repository tools, and applications.
- Keep time, allocation, buffer, collection, recursion, queue, diagnostic, and retained-state bounds explicit. Reject oversized or malformed untrusted input before expensive work or allocation whenever the contract permits.
- Measure representative hot paths and long-running workflows before and after material optimization work. Record the input size, host and profile, elapsed time, and peak or working-set memory when practical so claims remain reproducible.
- Protect development feedback time: avoid redundant compilation and verification, preserve valid intermediate evidence and caches, select the narrowest reliable verifier, and keep long operations visibly progressing.
- Give performance and memory regressions named workloads and enforceable thresholds when stable measurement is available. Tests and verifiers themselves must remain bounded and must not hide accidental superlinear work behind small fixtures.
- Prefer clear data ownership, streaming or bounded buffering, and predictable allocation over avoidable copying and unbounded retention. Applications must expose resource limits and failure behavior appropriate to their profile.
- Never trade correctness, safety, deterministic output, portability contracts, or maintainability for an unevidenced micro-optimization. Document deliberate tradeoffs and keep a simple correctness oracle where optimized implementations need comparison.

## Durable project direction

- Windows and Linux are permanent Windvale hosts. They are not the semantic definition of the language.
- Windvale defines its own source semantics, bytecode, module format, runtime contracts, capability profiles, and standard-library behavior.
- The same source language should eventually support portable bytecode and native compilation.
- Canonical WVB remains the verified cross-host distribution contract while a shared Windvale-native backend serves interpreter, JIT, cached/install-time, and AOT execution without changing semantics. An individual WVB may carry explicit platform-scoped requirements.
- C# and .NET supplied the qualified Stage 0 bootstrap but are absent from `main` under Decision 0558. The immutable `stage0-recovery-e5a1a7473c57` release and `Bootstrap/Stage0/README.md` preserve the exact recovery provenance. New source-language semantics belong in `Compiler/Windvale`; any managed recovery or security correction begins in a separate restored workspace and requires a new decision before managed source or a direct `dotnet` entry point may return to `main`.
- Applications may target shared Windvale contracts, an explicit subset of environments, or one named platform extension. Portability is a per-part promise and a derived artifact property, not a blanket dependency requirement. Host adapters map shared contracts to Windows, Linux, and Windvale OS.
- Platform-specific capabilities remain explicit and must not leak into parts that claim portability.
- OS, VM-hosting, guest-networking, and remote-terminal work also follows the
  focused rules in `Operating-System/AGENTS.md`.
- The OS is a vertical integration target, not a reason to postpone useful host tools and libraries.
- Keep bootstrap dependencies explicit. A bootstrap tool may be temporary, but its role and replacement path must be documented.
- Prefer a small coherent path over parallel compilers, runtimes, object models, or compatibility layers.
- Do not preserve obsolete experimental formats during early development unless a named compatibility case is explicitly required. Update fixtures and tests to the current contract.

## Current baseline and Language 1.0 direction

Use these distinctions when choosing implementation work or describing progress:

- The signed `v0.1.0` preview and Windvale Seed are historical baselines, not the
  target for new memory-management work. Preserve their exact recovery evidence;
  do not add permanent Seed compatibility to the forward compiler.
- [The Language 1.0 source-freeze decision](Documents/Decisions/0767-Freeze-Windvale-Language-1.0-Source.md)
  and its accepted amendments establish the source contract. Some identity-bound
  specifications and manifests preserve pre-freeze status text. Read their
  promotion/amendment decisions before interpreting that text as current status;
  do not edit frozen bytes merely to modernize a heading.
- [The Slice 8 qualification decision](Documents/Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md)
  establishes the forward Language 1.0 compiler within its exact qualified scope
  and target subsets. It does not establish complete Libraries 1.0, general
  runtime memory management, installed delivery, or a released 1.0 product.
- The [completion plan](Documents/Project/Compiler-Tools-And-Libraries-Completion-Plan.md)
  and [1.0 completion matrix](Documents/Project/Compiler-Tools-And-Libraries-1.0-Matrix.md)
  distinguish maintained consumers from candidate probes and unfinished APIs.
  Keep implemented, verified, qualified, and released claims separate.

### Memory management: current limitations and target

The maintainer selected completing the Language 1.0 ownership-to-storage path
as the priority on 27 September 2026. Close that path before further consumer
expansion that depends on it. This direction is accepted; the complete native
implementation and its qualification remain work to do.

Current native execution still uses bootstrap-era byte/text arena mechanics:
checked allocation, restricted buffer reuse, and function-return checkpoints.
The current source lowerer compacts the byte/text ranges reachable from record
and variant returns, reclaiming other allocations above the call's entry
checkpoint. This does not release obsolete values below that checkpoint or
provide general last-share ownership. Eligible immutable byte/text loops also
compact storage reachable from live locals at loop entry, including nested
records and active variants. The lowering contract bounds that analysis and
retains storage conservatively outside its supported shapes and limits. This
does not implement owned mutable storage, allocation leases, or the full 1.0
release contract. Pinned bootstrap artifacts retain their
recorded behavior until separately rebuilt and qualified. The interpreter
repeatedly replaces immutable byte representations of working state. Its guest allocation
accounting and reclamation are separate from the native storage running that
interpreter. A guest release therefore does not prove native memory reuse.
Deferred frame writes and fewer copies reduce pressure but do not complete
reclamation. A fixed arena ceiling, including the hosted runner's 128 MiB
profile, bounds exhaustion; it is neither a portable language limit nor proof
of efficient memory use.

The runtime-private [owned-storage leaf](Specifications/Windvale-Native-Owned-Storage.md)
now adds committed capacity, generation-checked handles, zeroing, exact physical
charges, non-tail reuse, and domain teardown over the historical allocator.
The [budgeted adapter](Specifications/Windvale-Native-Budgeted-Storage.md) now
binds this fixed 64-slot profile to canonical budget/lease accounting, preserving
both domains on refusal and crediting parents on release. The candidate
[native collection path](Specifications/Windvale-Native-Owned-Collections.md)
now connects single-Main scalar Vector reservation, append, length, indexed
reads, explicit replacement growth, scope release and terminal teardown through
ABI 24/context 10. Growth funds the full replacement while the old backing is
live and preserves both owners on refusal. Owned helper/aggregate transfer,
normal launchers and interpreter working state still
need integration. Connect those lifetimes and charges before describing the
1.0 storage path as complete.

Implement the existing [Language 1.0 allocation and release rules](Specifications/Windvale-Language-1.0.md#allocation-and-release)
and [Foundation memory domains](Specifications/Windvale-Language-1.0-Foundation.md#memory-domains-and-allocation):

- Keep scalars and suitable records in registers, stack, or inline storage;
  ordinary value updates must not require rebuilding an entire heap-backed frame.
- Give mutable vectors, buffers, builders, and maps unique ownership, exclusive
  borrowed mutation, explicit limits, and deterministic release.
- Track shared immutable byte/text/sequence backing through copies, slices,
  aggregate fields, calls, and returns. Preserve its charge while a semantic
  share remains; final release must make storage reusable. Reference counting
  is a compatible implementation candidate, not a newly frozen source rule.
- Keep borrowed views within their owner's lifetime. Recursive graphs use owned
  typed arenas with generation-checked non-owning handles and bounded teardown.
- Connect physical storage to budgets and allocation leases. Preserve typed
  allocation refusal, reservation guarantees, unchanged-on-refusal behavior,
  and the specified accounting transfer on move, growth, freeze, and release.
- Bound and reclaim runtime scratch and interpreter working storage separately
  from guest accounting. Use typed owned state and reserved buffers/builders as
  their native support becomes available; bytes remain appropriate for actual
  serialized formats.

### Implementation and completion rules

- Extend the existing compiler and shared native runtime. Carry ownership and
  cleanup through normal exits, failure propagation, and aggregate transfers;
  terminal teardown must reclaim the enclosing resource domain. Do not bypass
  alias safety by resetting an arena while returned references remain live.
- The [descriptor allocator leaf](Compiler/Native/Allocator/Descriptor-Allocator.wva)
  is candidate groundwork, not an integrated 1.0 allocator. Evaluate it against
  the full ownership and accounting contracts. Version ABI or format changes
  through the existing decision process before claiming the successor selected.
- Treat larger arenas, deferred writes, and local copying reductions as bounded
  mitigations with explicit limits. They do not close the 1.0 memory gate.
- Measure live storage, reusable storage, cumulative allocation, budget charges,
  and process peak memory separately. Source-code size and process RSS alone
  cannot establish the live working set or attribute a leak.
- Require fixed-live-state workloads to stabilize within a justified memory
  bound as iterations increase, plus repeated allocation/release reuse, alias
  survival, failure cleanup, and a maintained parser/collection consumer. Extend
  existing focused owners and enforce named workload thresholds. One saved
  probe finishing below an arena ceiling is insufficient completion evidence.
- Qualify the changed path on Windows and real Debian against exact declared
  inputs. Preserve unaffected earlier evidence and state remaining target/API
  gaps; do not infer complete 1.0 delivery from a narrow passing selection.

## Architecture boundaries

Keep these responsibilities distinct even if early prototypes temporarily share a project or process:

- `Specifications/` defines source semantics, bytecode, modules, object records, ABI rules, and platform contracts.
- `Compiler/` owns parsing, semantic analysis, Windvale IR, and code-generation orchestration.
- `Assembler/` owns textual assembly parsing and instruction encoding.
- `Object-Model/` owns structured sections, symbols, relocations, and serialization contracts.
- `Linker/` owns symbol resolution, layout, relocation, and final image production.
- `Runtime/` owns bytecode loading, validation, execution, memory/runtime services, and host adaptation.
- `Libraries/` owns reusable Windvale APIs and their shared, platform-scoped, capability-specific, protocol, or system implementations.
- `Operating-System/` owns boot, kernel, drivers, processes, and Windvale-native platform services.
- `Tools/` owns repository development, inspection, generation, and verification utilities.
- `Tests/` owns conformance, differential, integration, malformed-input, and reproducibility coverage.
- `Documents/` owns architecture, decisions, project direction, and runbooks.

Create these source areas only when implementation begins; do not add empty directory scaffolding without an owner or contract.

## Compiler and format rules

- Treat source syntax, semantic IR, distributable bytecode, native machine IR, object files, and executable images as different contracts.
- Keep JIT and AOT as output modes over shared verified semantics, native ABI rules, machine lowering, and typed relocation contracts rather than parallel compilers.
- Do not use a backend format such as C, LLVM IR, WebAssembly, PE, or ELF as the implicit definition of Windvale semantics.
- Give every serialized format an explicit version, validation boundary, size limits, and malformed-input tests.
- Bytecode verification happens before execution. Revalidate indices, offsets, lengths, types, capabilities, imports, and control-flow targets.
- Compiler phases consume and produce explicit models. Avoid hidden cross-phase mutation and global compiler state.
- Prefer immutable evidence between phases. Diagnostics must identify the phase, source location when available, and violated rule.
- The compiler and assembler should converge on one structured native object model and object writer.
- Keep architecture-specific instruction selection, encoding, relocation, and ABI policy behind explicit contracts.
- Reproducible output is a product feature: identical inputs, tool versions, and options should produce identical bytes.

## Capability and safety rules

- Classify each part by platform scope, authority level, required capabilities, and optional capabilities before implementation. Until source/module metadata separates those dimensions, retain the implemented portable, hosted, or system profile and document any narrower platform scope explicitly.
- Portable code must not depend on native paths, host handles, ambient process state, privileged instructions, or undocumented host behavior.
- Hosted and system operations require declared capabilities. Unsupported and unauthorized operations must fail explicitly.
- A library requirement is not a grant. The application must approve its exact transitive capability set, and the launcher or service manager must bind rights-limited provider instances separately.
- Give semantic capability interfaces canonical names, major contract versions, exact signatures, limits, and failure behavior. Binding proves initial availability, not permanent availability; revocation, stale handles, peer exit, and provider restart must remain explicit.
- Keep shared filesystem semantics small and exact. Put stronger or platform-specific behavior in separate capability interfaces, and never use one operation name for different partial-write, atomicity, durability, path, or failure guarantees.
- Mutating I/O must distinguish rejection, exact partial progress, completion, and indeterminate completion. Never retry an indeterminate mutation without a specified idempotency contract.
- A network stream write reports exact local-provider acceptance, not remote receipt or application commit. Datagram acceptance does not imply delivery. Reconnection must not silently replay an uncertain application mutation.
- OS-specific guest, virtualization, DMA, and passthrough safety rules live in
  `Operating-System/AGENTS.md` and apply before work crosses those boundaries.
- Unsafe operations must be syntactically and contractually visible; do not allow safety-sensitive behavior through ordinary convenience APIs.
- Treat every loaded module, object file, package, symbol table, relocation, and debug record as untrusted input.
- Use checked arithmetic for file offsets, memory sizes, indices, and address calculations.
- Define integer widths, overflow behavior, byte order, alignment, text encoding, and concurrency semantics rather than inheriting host defaults.

## Code style and naming

Carry the established [E-Worker](https://eworker.ca) host-code convention into Windvale and repository tooling where the implementation language supports it:

- TypeScript identifiers controlled by Windvale use the macron separator `ˉ` (U+02C9) between semantic words and start with capital case, such as `Moduleˉreader` and `Readˉsection`.
- Do not leave camelCase islands inside macron-separated identifiers.
- External APIs, wire formats, standard-library members, and persisted schemas keep required external casing only at the boundary. Map them to internal names.
- Constants use `ALL_CAPS_WITH_UNDERSCORES`.
- File and folder names capitalize the first word and use `-` between words, such as `Object-Model` and `Module-Reader.cs`.
- C, assembly, exported ABI symbols, and generated identifiers use a deliberately specified ASCII-safe convention for toolchain portability.
- Official Windvale source follows `Specifications/Source-Naming.md`: capitalized identifiers, U+02C9 semantic-word separators, immutable `let`, mutable `var`, and ASCII-safe machine namespaces.
- Prefer explicit types and contracts over loosely shaped objects.
- Prefer focused modules named after their owned capability over broad `Helpers`, `Utils`, `Common`, or numbered-part files.
- Prefer focused source files of reviewable size. A very large file should prompt consideration of cohesive extraction into clearly owned modules or files when a real boundary exists; this is guidance rather than a line limit, and code should not be split into numbered fragments or have invariants obscured merely to reduce file size.
- Add succinct comments only for invariants, format rules, unsafe reasoning, or other non-obvious logic.
- Keep dependencies explicit; importing a declaration must not perform global registration or other runtime work.
- Repository-maintained text uses LF line endings except Windows command files. Keep text and binary classifications explicit in `.gitattributes`; do not rely on a contributor's global `core.autocrlf` setting.

## Browser web applications

Follow the shared
[browser-application architecture](Documents/Architecture/Browser-Application-Development.md)
and the nearest scoped `AGENTS.md`. Independently deployable applications belong
under `Applications/Web/`; reusable browser framework and component code belongs
under `Libraries/Web/`; the public website and playground keep their existing
owners. Browser state, lifecycle, mutation, input-validation, styling, and
capability boundaries must remain explicit.

## Testing and verification

- Local development verification has a default total wall-clock budget of ten
  minutes, including cache preparation, compilation, packaging, reconstruction,
  test execution, and cleanup. Before executing changed-file verification,
  inspect its proposed plan with `Verify-Changed.ps1 -PlanOnly` and do not start
  an owner whose known or reasonably expected cold duration exceeds the
  remaining selected budget.
- Standing maintainer approval, granted on 22 September 2026, covers longer
  local builds, compilation, packaging, reconstruction, tests, benchmarks, and
  Windows/Debian verification needed for the requested Windvale work. Do not
  ask for run-by-run approval solely because a run exceeds ten minutes. This
  approval persists across tasks and sessions until the maintainer revokes it.
- Before a longer run, state its purpose, exact command, expected cold duration,
  and finite maximum duration. Select the narrowest causal plan and use the
  existing explicit long-run switches where required. The ten-minute default
  remains a feedback-time target, not an approval blocker for a justified run.
  Stop at the selected deadline or when the estimate proves materially wrong;
  preserve completed evidence and caches and report the incomplete result.
  After diagnosis or a coherent fix, a newly reported bounded continuation is
  covered by the same standing approval. Do not silently reset an active
  deadline, retry unchanged failures, or disguise cumulative time as a new run.
- This standing approval concerns local execution time only. It does not grant
  new product scope, accept draft contracts, waive qualification evidence, or
  authorize release signing/publication, privileged installation, destructive
  operations, or external spending. Those actions retain their own authority
  requirements. A long run alone must not block the task awaiting permission.
- A verifier is justified only when its failure could reveal a defect introduced
  by the change. Before starting it, name the changed contract and the failure
  signal the check can detect. File-based routing is a conservative hint, not a
  reason to run unrelated implementation suites.
- If a selected plan is disproportionate or loses that causal link, stop before
  or during the run, preserve already completed evidence, and correct the scope,
  routing, or document ownership. Do not continue merely because the check was
  selected automatically or has already consumed time.
- Add only the minimum verification needed to protect a distinct contract. Before
  adding a verifier, prove that an existing focused owner cannot cover the
  behavior by adding cases. Every retained verifier must own unique behavior,
  failure evidence, or a required host boundary; merge or remove entry points
  that merely replay another verifier without adding coverage.
- Treat implementation and final verification as separate phases. During active
  implementation, run only the narrow checks required to develop or diagnose
  the changed behavior. After the coherent change is complete, run one selected
  final verification plan; do not keep restarting broad verification after each
  edit, artifact refresh, commit, or push.
- Make final verification parallel and resumable where independence permits.
  Cache each owner result by its complete declared inputs, tool identities,
  command, host, and verifier version. Reuse a passing result when none of those
  dependencies changed, and rerun only invalidated owners. Never reuse evidence
  across an undeclared dependency, semantic change, malformed-input boundary,
  host requirement, or qualification identity.
- Choose the narrowest reliable verifier for the changed behavior. Verification levels are alternatives, not a ladder: do not run changed-file, Fast, Development, Standard, and Qualification sequentially for the same source state. A passing broader level subsumes its narrower levels.
- Run a verifier after a coherent edit, not after every small edit. Reuse a passing result while the files relevant to that verifier remain unchanged, and do not rerun it merely because a commit or push is next. After a failure, rerun the narrowest affected selection; run at most one broader final gate when the resulting risk requires it.
- Every parser and binary reader needs valid, boundary, truncated, oversized, inconsistent, and malicious-input coverage.
- Use golden byte fixtures only where exact bytes are part of the contract. Pair them with structural assertions so failures remain diagnosable.
- Use differential tests when a temporary C backend, reference VM, native backend, or host adapter should implement the same semantics.
- Test deterministic builds by comparing output bytes, not only behavior.
- Keep a reference implementation simple enough to act as an oracle even when faster implementations appear.
- Documentation-only changes normally require `git diff --check`, link/path inspection, and review of the changed Markdown.
- Code changes require the relevant package checks and focused conformance tests once those commands exist.
- State exactly which broader checks were not run and why.

For an ordinary coherent change, first inspect the change-aware plan:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -PlanOnly
```

Execute the proposed plan only after confirming that every selected owner is
causal and the complete cold plan fits the selected budget. If it does not fit
the ten-minute default, select the exact focused owner, correct the routing, or
declare a bounded longer run under the standing approval above instead of
launching the plan mechanically or requesting redundant approval.

Detailed verifier routing, native-owner selection, editor synchronization, broad
gates, recovery-only managed checks, and cross-host qualification rules live in
[`Tools/Verify/AGENTS.md`](Tools/Verify/AGENTS.md).

## Documentation discipline

- Follow `Documents/Documentation-Policy.md` for document ownership, status
  metadata, plain-language structure, hash placement, decision identifiers, and
  active-context budgets.
- Ordinary changes record verification in the commit or pull-request summary;
  they do not require a new evidence JSON or per-file hash inventory. Identify
  tracked source by its exact tested revision and reference existing artifact
  manifests. Keep hashes that enforce release, bootstrap, cache integrity or
  exact-byte contracts; avoid duplicating them in routine documentation.
- Work under `Documents/` also follows `Documents/AGENTS.md`.
- Obsolete narrative history belongs in Git, not working-tree archives. Keep
  current contracts and active plans concise; retrieve a specific old record
  through `Documents/Git-History.md` only when the task needs it.
- Update documentation when semantics, formats, architecture, bootstrap stages,
  security boundaries, or durable workflows change. Never describe a proposal
  or aspiration as implemented behavior.
- Treat overview images as dated editorial snapshots, not generated mirrors of
  surrounding prose. Refresh one only when it becomes materially misleading and
  preserve its snapshot date.

## Git workflow

Before changes:

```powershell
git status --short --branch
git pull --ff-only
```

After changes:

```powershell
git status --short --branch
git add <task-files>
git commit -m "<task-scoped message>"
git push
```

If the remote moved, use `git pull --rebase` and then push. Do not overwrite shared history.
