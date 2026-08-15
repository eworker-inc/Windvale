# Windvale progress

> Status snapshot: 15 August 2026

<a href="Images/Windvale-Roadmap-August-2026.svg"><img src="Images/Windvale-Roadmap-August-2026.svg" alt="Dated August 2026 Windvale roadmap phase map" width="100%"></a>

This is the current-state dashboard. It records measured standing and immediate
work, not the chronological path used to reach it. The
[roadmap](Roadmap.md) owns forward milestones,
[qualification evidence](Seed-Verification-Evidence.md) owns exact historical
runs and identities, and accepted decisions own rationale.

The image is a dated editorial snapshot. Update the Markdown when current state
changes; refresh the image only when it becomes materially misleading.

The roadmap is dependency-based: Milestone 2 closed the package-backed host
application, Milestone 3 now owns the 0.1 preview, and OS-1 advances launch and
service composition in parallel. The former numeric order remains in history;
no completion gate was removed by the rebaseline.

## Indicators

| Indicator | Meaning |
| :---: | --- |
| ✅ Qualified | The named finite gate has reproducible evidence. |
| 🔵 Ongoing | Useful qualified slices exist; direct consumers may require more. |
| 🎯 Current | This is an immediate product or workflow boundary. |
| 🚧 Candidate | Focused implementation evidence exists, but the named gate is not qualified. |
| ○ Proposed | Direction exists without implementation evidence. |

## Current dashboard

| Boundary | Standing | Evidence today | Immediate next result |
| --- | :---: | --- | --- |
| Seed through assembler and linker | ✅ | Source semantics, WVB, verification, runtime, object, assembly, and link foundations have Windows/Linux evidence. | Preserve their contracts as later products consume them. |
| Windvale-written compiler | ✅ | The committed twelve-module inventory produces byte-identical 599,868-byte Stage 1 and Stage 2 compilers on Windows and Debian. | Add forward semantics only for named consumers; do not widen Stage 0. |
| Shared accepted-subset native backend | 🔵 | Interpreter, deterministic AOT, baseline JIT, hosted execution, and native object/link/container profiles are qualified for their documented subsets. | Broaden only where the package application, tools, or OS require an unsupported operation or ownership shape. |
| Native-only repository | 🚧 | [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md) closed all eight retirement conditions; [Decision 0558](../Decisions/0558-Archive-Managed-Stage0-Outside-Main.md) removes all tracked managed source and direct managed entry points from `main`. | Pass one explicit post-archive dual-host qualification, then tag the exact commit as a native-only baseline. |
| Development verification | ✅ | Milestone 1 is complete under [Decision 0560](../Decisions/0560-Linked-Image-Development-Checkpoints.md): front-door feedback takes 13,900 ms, the pinned WebAssembly engine path takes 29,674 ms, and the all-hit database owner fell from 402,638 ms to 87,800 ms with all eight behaviors. Exact warm [GitHub run 31852544894, attempt 2](https://github.com/eworker-inc/Windvale/actions/runs/31852544894/attempts/2) passes end to end in 1m42s on Windows and 1m15s on Linux; three owner closures and five checkpoint families are machine-checked. | Preserve the bounded path and add another checkpoint or owner only from new measured product pressure. |
| Native qualification | ✅ | [Decision 0550](../Decisions/0550-Measured-Native-Retirement-Sharding.md) qualified 52 suites and 3,287 cases per host in four shards; the complete workflow took about 15 minutes. | Run this gate only for a selected release, promotion, bootstrap, security, ABI, or conformance state. |
| Package-backed application | ✅ | Milestone 2 is complete under [Decision 0561](../Decisions/0561-First-Admitted-Bundle-Store-And-Rights-Reduced-Wvdb-Query.md). Paired [Bundle 1/store run 31872089188](https://github.com/eworker-inc/Windvale/actions/runs/31872089188) and [capability run 31872429140](https://github.com/eworker-inc/Windvale/actions/runs/31872429140) retain the exact WVB, bundle, package, application, capability, success, denial, and offline-rebuild evidence. | Preserve these results while their declared inputs remain unchanged; use the completed package path as an installer and release-envelope consumer in Milestone 3. |
| Development/stable installers | ✅ | [Decision 0562](../Decisions/0562-First-Deterministic-Development-Installers.md) retains the paired exact `0.1.0-dev.1` archives. [Decision 0565](../Decisions/0565-First-Stable-Preview-Installers.md) pins separate `0.1.0` Windows/Linux archives and stable internal metadata while reproducing the development bytes unchanged. The combined owner passes on both hosts in [Verify run 31885759856](https://github.com/eworker-inc/Windvale/actions/runs/31885759856). | Select only the stable identities in the owner-signed release envelope. |
| Durable database | 🚧 | The bounded database path includes `u64` geometry, dual superblocks, immutable pages/logs, single-writer publication, variable-key tree nodes, routed updates, internal branch splitting, depth-three root growth, provider-backed reads, and interruption recovery. | Let the package-backed application select the minimum repeated depth-three, reclamation, and recovery behavior needed for a useful workload. |
| Windvale OS | 🔵 | OS-1 already owns the qualified three-environment WVB portability gate plus bounded endpoint, peer-loss, teardown, and contained-failure foundations. Probe 40 qualifies protected processes, capability IPC, bounded preemption evidence, and generation-safe non-tail memory objects. | In the parallel OS track, add one flat resource domain and one atomic launch/supervision slice before broad shell, networking, or driver work. |
| WebAssembly playground | 🔵 | The static Monaco playground and complete source/WVB-to-Wasm verification route run without .NET. A separate pinned-package engine checkpoint proves compile/verify/execute, capability denial/grant, and bounded output without rebuilding WVB/Wasm. | Keep complete construction cold for qualification; improve package size or browser coverage only when product measurements justify it. |
| Public project and release foundation | 🚧 | Licensing, contribution, security, governance, authorship, public repository policies, the product threat model, exact WVDB approval/launch records, and Release Envelope 1 creator/verifier now exist. Their focused owners pass on Windows and Linux in Verify run 31883543587. The exact Stage 0 recovery release is published and independently retained. | Perform the project-owner root ceremony, select and qualify the final state, and publish the signed `v0.1.0` envelope. |

## .NET retirement result

`████████████████████  8/8 conditions qualified — complete`

| Evidence counter at the retirement release | Result |
| --- | ---: |
| Normal .NET entry points | **0** |
| Explicit recovery-only .NET entry points | **9** |
| Native Windows/Linux qualification jobs | **6/6 passed** |
| Native retirement suites | **45/45 per host** |
| Fixed native cases | **3,206/3,206 per host** |
| Exact selected-release recovery | **2/2 hosts** |
| Published and independently retained recovery assets | **13/13** |

The later 52-suite/3,287-case plan is forward native coverage added after the
retirement release. It does not reopen the completed dependency gate.

## Current dependency-based work

### 1. Milestone 2 complete: package-backed host application

The active product slice is WVDB Query from
[Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md).
The completed manifest/lock closure and offline locked rebuild remain credited.
The completed package path contains:

1. one deterministic admitted bundle;
2. one content-addressed local publication path;
3. one complete capability closure and approval record;
4. one successful rights-reduced hosted execution;
5. one denied or unsupported execution with no ambient fallback; and
6. matching canonical WVB/package identities on Windows and Linux.

All six items have local and paired-host evidence. Bundle 1/store run 31872089188
passes on Windows and Linux at commit `d9795e0e15944b3342ea7c4a42105eee38420708`;
WVDB capability run 31872429140 passes on both hosts at commit
`204e8082fdaabbc7333ac40ed6ca7ff8564de123`. Milestone 2 is closed and no new
feature belongs in it.

SQL, a registry, a network resolver, broad concurrency, and a database server
are not required for this slice.

### 2. Active Milestone 3: Windvale 0.1 preview

Retain the qualified `0.1.0-dev.1` development artifacts from Decision 0562 and
consume the separately pinned `0.1.0` stable Windows/Linux installers from
Decision 0565. Their base payload is Windvale plus the bounded launcher/client,
offline payload verifier, compiler, assembler, linker, runtime, publisher, and
core inspection tools. A stable label alone is not the `v0.1.0` release.
WVDB Query is a separate example/application package; database servers and other
applications remain separate packages or projects. The shipped-product threat
model, first signing/offline-verification policy, bounded envelope creator and
independent verifier, and exact WVDB approval and target launch records are
implemented under Decisions 0563 and 0564. The 13-case envelope-format core and
8 approval-record cases pass on both hosts in Verify run 31883543587. Decision
0566 expands the envelope owner to 16 cases for passphrase-protected key custody,
passing on both hosts in Verify run 31888902259. The remaining sequence is the
project-owner root ceremony, one selected-state complete qualification,
reproducible envelope creation, offline verification, and the signed `v0.1.0`
tag/publication.

OS-1 is not a prerequisite for `v0.1.0`.

### Independent promotion checkpoint: native-only repository baseline

The archival decision is implemented in the working repository: the immutable
pre-removal release remains authoritative, the in-tree pointer preserves exact
checksums and recovery custody, and guards require zero tracked managed files and
zero direct managed entry points. The remaining step is one explicit complete
Windows/Linux qualification on the committed post-archive state; ordinary
per-commit full qualification remains disabled.

### Parallel track OS-1: launch and service composition

Preserve the already-qualified three-environment WVB, endpoint, bounded queue,
peer-loss, teardown, contained-failure, and memory-object evidence. The open
composition mechanisms are one flat resource domain, one atomic launch
transaction, a provider serving two live clients, and generic structured
supervision with a bounded restart or deliberate terminal result.

### Preserve bounded development feedback

- Use `Tools/Verify/Verify-Changed.ps1` once after a coherent edit.
- Treat a passing affected-owner result as reusable until that owner's declared
  inputs change.
- Use development checkpoints only for immutable generated products; always
  rerun the behavior being changed.
- Keep complete qualification cold, explicit, and bound to one selected source
  state.
- Promote related compiler and tool identities in coherent batches rather than
  repinning the entire product family on every implementation commit.

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows and Linux.
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image.
- ✅ Canonical WVB → shared accepted-subset lowering → native WVO/AOT → Windows PE or Linux ELF.
- ✅ Native compiler seed → Stage 1 → byte-identical Stage 2 without loading .NET.
- ✅ Hosted console application → explicit capability metadata → deterministic Windows/Linux application.
- ✅ Portable Windvale modules → bounded WebAssembly compiler/interpreter → static browser worker.
- ✅ Canonical WVB → admitted guest resource → protected Windvale OS execution for the qualified examples.
- ✅ Package manifest and lock → admitted immutable bundle/store → rights-limited WVDB Query execution on Windows and Linux.
- 🚧 Probe 40 → flat resource domain → atomic launch → supervised isolated provider.
- ○ Qualified product artifacts → signed release envelope → offline-verifiable Windvale 0.1 preview.

## Reading the evidence

- [Development roadmap](Roadmap.md) defines the active milestones and gates.
- [.NET retirement inventory](Dotnet-Retirement-Inventory.md) records the current
  native-only boundary and immutable managed recovery identity.
- [Qualification evidence](Seed-Verification-Evidence.md) preserves exact runs,
  reports, and artifact identities.
- [Changelog](../../CHANGELOG.md) summarizes notable implementation changes.
- Dated decisions preserve the detailed historical sequence without making that
  sequence the current plan.
