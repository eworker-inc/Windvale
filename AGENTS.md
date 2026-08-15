# Agent Handbook

## Purpose

This document gives people and AI agents the durable rules needed to develop Windvale safely and coherently.

Windvale is a source-available experiment in constructing a small computing stack: language, compiler, bytecode, runtime, assembler, object model, linker, foundation library, tools, and operating system. The goal is not merely to generate code with AI. The result must be understandable, testable, reproducible, and useful independently at each layer.

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

## Durable project direction

- Windows and Linux are permanent Windvale hosts. They are not the semantic definition of the language.
- Windvale defines its own source semantics, bytecode, module format, runtime contracts, capability profiles, and standard-library behavior.
- The same source language should eventually support portable bytecode and native compilation.
- Canonical WVB remains the verified cross-host distribution contract while a shared Windvale-native backend serves interpreter, JIT, cached/install-time, and AOT execution without changing semantics. An individual WVB may carry explicit platform-scoped requirements.
- C# and .NET supplied the qualified Stage 0 bootstrap but are absent from `main` under Decision 0558. The immutable `stage0-recovery-e5a1a7473c57` release and `Bootstrap/Stage0/README.md` preserve the exact recovery provenance. New source-language semantics belong in `Compiler/Windvale`; any managed recovery or security correction begins in a separate restored workspace and requires a new decision before managed source or a direct `dotnet` entry point may return to `main`.
- Applications may target shared Windvale contracts, an explicit subset of environments, or one named platform extension. Portability is a per-part promise and a derived artifact property, not a blanket dependency requirement. Host adapters map shared contracts to Windows, Linux, and Windvale OS.
- Platform-specific capabilities remain explicit and must not leak into parts that claim portability.
- Running Windvale OS as a guest, accelerating that guest through a host hypervisor, and making Windvale OS a VM host are separate contracts. Preserve the pinned emulation oracle, report the selected engine/provider and full nested topology, prefer physical/root providers for baseline qualification, and keep nested virtualization optional and developer-oriented rather than a build or semantic dependency unless a named decision qualifies nesting itself.
- Future Windvale VM hosting keeps privileged guest-memory, vCPU, interrupt, DMA, accounting, and teardown enforcement in the kernel/WVA boundary while machine, firmware, device, GPU, compute, and lifecycle policy remains in isolated services.
- Future Windvale OS networking keeps interrupt, timer, memory, DMA/IOMMU, accounting, and teardown mechanisms in the kernel; isolated drivers own link-device mechanics; one bounded user-space service initially owns standards-based packet, route, UDP, and TCP processing; applications use semantic rights-limited capabilities rather than ambient sockets or raw service protocols.
- Future remote terminals use a small bounded session protocol over an authenticated secure ordered stream. The first real profile is one connection, one session, and one resource domain; identity and authorization remain separate; no production plaintext, replayable early data, implicit resume, ambient remote-root authority, custom cryptography, or terminal parsing in the kernel or shell is permitted.
- The OS is a vertical integration target, not a reason to postpone useful host tools and libraries.
- Keep bootstrap dependencies explicit. A bootstrap tool may be temporary, but its role and replacement path must be documented.
- Prefer a small coherent path over parallel compilers, runtimes, object models, or compatibility layers.
- Do not preserve obsolete experimental formats during early development unless a named compatibility case is explicitly required. Update fixtures and tests to the current contract.

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
- Treat guest images, firmware, VM state, page tables, shared queues, shaders, compute kernels, and device commands as untrusted input. Bound VM exits, interrupts, queues, pinned pages, work, diagnostics, and teardown time while reserving host recovery resources.
- A VM-management capability does not grant storage, network, display, GPU, accelerator, firmware, host-file, or passthrough authority. Bind each attachment separately and expose whether it is software, paravirtual shared, hardware-partitioned, or exclusive passthrough.
- Never permit device passthrough or guest/accelerator DMA without measured IOMMU, interrupt-remapping, topology, ownership, reset, range, generation, revocation, and teardown evidence. Disable the attachment when any required guarantee is unavailable.
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

## Testing and verification

- Choose the narrowest reliable verifier for the changed behavior. Verification levels are alternatives, not a ladder: do not run changed-file, Fast, Development, Standard, and Qualification sequentially for the same source state. A passing broader level subsumes its narrower levels.
- Run a verifier after a coherent edit, not after every small edit. Reuse a passing result while the files relevant to that verifier remain unchanged, and do not rerun it merely because a commit or push is next. After a failure, rerun the narrowest affected selection; run at most one broader final gate when the resulting risk requires it.
- Prefer `Tools/Verify/Verify-Changed.ps1` for the Windows inner loop. Its change classifier uses a lightweight scope for ordinary documentation and editor-package-only work, a website scope for static site, browser packaging, Cloudflare function, and website-tool changes, a development scope for implementation and specification changes with mapped native owners, and qualification only when explicitly requested or when comparison cannot be resolved safely. Development-scoped changed-file verification maps maintained boundaries to focused native retirement suites in canonical order and refuses explicit uncovered gaps; it does not fall back to the complete unfiltered native gate. The website scope runs `Tools/Verify/Verify-Website.ps1`. Managed Stage 0 source and tests are absent from `main`; restore the exact recovery release in a separate workspace only for a named recovery, security, or historical differential investigation. Run focused native OS owners through their retirement-suite filters.
- Every parser and binary reader needs valid, boundary, truncated, oversized, inconsistent, and malicious-input coverage.
- Use golden byte fixtures only where exact bytes are part of the contract. Pair them with structural assertions so failures remain diagnosable.
- Use differential tests when a temporary C backend, reference VM, native backend, or host adapter should implement the same semantics.
- Test deterministic builds by comparing output bytes, not only behavior.
- Keep a reference implementation simple enough to act as an oracle even when faster implementations appear.
- Keep `Tools/Editors/Windvale/` synchronized with changes to the implemented `.wv` lexical surface, and run `Tools/Editors/Verify-Windvale-Editor.ps1` after changing its grammar or package metadata. Keep WVA textual assembly separate from Windvale source classification.
- Documentation-only changes normally require `git diff --check`, link/path inspection, and review of the changed Markdown.
- Code changes require the relevant package checks and focused conformance tests once those commands exist.
- State exactly which broader checks were not run and why.

Windvale Seed code changes normally require one local development verifier, selected in proportion to risk. For a focused change, use the change-aware verifier:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

For a coherent cross-area batch, run `Verify-Changed.ps1` once after the edit has settled; its native planner may select multiple focused owners in canonical order. A named gap must gain a native owner rather than trigger an unfiltered or managed fallback. Use the managed Development, Standard, or Qualification tiers only for explicit recovery/differential evidence or the final retirement gate. Do not run multiple verifier levels for the same unchanged tree. GitHub runs affected focused native owners on both hosts for ordinary implementation and specification pushes and pull requests. Invoke the independent complete dual-host Qualification gate explicitly through workflow dispatch for a release candidate, promotion, security boundary, or deliberate qualification claim; it is not a per-commit gate. Run local broad native, managed comparison, bootstrap, WebAssembly-engine, or live OS-boot gates only when the changed boundary or an explicit qualification claim requires them. Changes to portable semantics, bytecode, serialization, runtime behavior, or golden hashes require reports from both hosts before cross-host conformance is claimed.

## Documentation discipline

- Put enduring architecture under `Documents/Architecture/`.
- Put dated decisions under `Documents/Decisions/`, including status, context, decision, consequences, and reconsideration triggers.
- Put project framing and unresolved product questions under `Documents/Project/`.
- Distinguish accepted direction from proposals and experiments.
- Update documentation when semantics, formats, architecture, bootstrap stages, security boundaries, or durable workflows change.
- Do not record aspirational features as implemented behavior.
- Treat README overview images and similar graphics as dated editorial snapshots, not generated mirrors of the surrounding prose. Do not update an image merely because README wording or ordinary milestones change. A human or AI maintainer may refresh it periodically when the visual would otherwise become materially misleading; preserve an accurate snapshot date.

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
