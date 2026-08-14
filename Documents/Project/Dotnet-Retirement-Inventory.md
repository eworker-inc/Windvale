# .NET retirement inventory

> Inventory snapshot: 14 August 2026

This is the current operational ledger for .NET after the completed retirement
gate. It records the normal-path result, the direct recovery commands, retained
managed source owners, and the policy for any future archival change.

The machine-readable
[companion inventory](Dotnet-Retirement-Inventory.json) is checked by
`Tools/Verify/Verify-Dotnet-Retirement-Inventory.ps1`. Historical transfer
detail remains in Decisions 0496 through 0526, the qualification evidence, and
Git history; it is not repeated in this live inventory.

## Completion dashboard

`████████████████████  8/8 retirement conditions qualified`

| Counter at the retirement release | Final evidence |
| --- | ---: |
| Normal managed entry points | **0** |
| Recovery-only managed entry points | **9** |
| Independent native matrix jobs | **6/6 passed** |
| Fixed native retirement suites | **45/45 per host** |
| Fixed native cases | **3,206/3,206 per host** |
| Exact selected-release recovery hosts | **2/2 passed** |
| Final recovery assets | **13/13 published and independently retained** |

[Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md)
qualified the accepted repository build, verification, test, packaging,
execution, WebAssembly, OS-image construction, and bootstrap paths without a
normal .NET dependency. Later feature or product candidates do not reopen that
gate merely because their broader profile is unfinished.

## Standing vocabulary

| Standing | Meaning |
| --- | --- |
| `native-qualified` | The accepted ordinary Windows/Linux route is native and has independent cross-host evidence. |
| `native-candidate` | A native forward feature or product slice has focused evidence but has not reached its own promotion gate. |
| `recovery-retained` | A managed source owner or command is intentionally available only for Stage 0 recovery, security correction, or a named differential question. |
| `missing` | A required ordinary route has no adequate native owner. Any such discovery reopens the affected retirement condition. |

There are no `managed-normal` or `missing` entries in the accepted retirement
subset.

## Accepted product surfaces

| Surface | Standing | Ordinary route |
| --- | --- | --- |
| Project source to canonical WVB | `native-qualified` | `Tools/Native/Build-Wvb.cmd` and `.sh` use digest-bound native compiler, verifier, and publisher tools. |
| WVB verification and inspection | `native-qualified` | Digest-bound `wvverify` and `wvdump` applications protect ordinary build and inspection. |
| Accepted-subset WVB execution | `native-qualified` | Paired native runners cover the fixed portable subset, results, traps, diagnostics, and budgets. |
| WVA assembly | `native-qualified` | Digest-bound native assemblers own accepted ordinary assembly, including the Probe 40 construction path. |
| WVO verification and inspection | `native-qualified` | Windvale-owned admission protects inspection, publication, linking, and execution boundaries. |
| Accepted standard WVO linking | `native-qualified` | Reconstructed native linkers own accepted flat and segmented link paths. |
| Accepted PE/ELF and hosted WVB packaging | `native-qualified` | Windvale-owned construction, admission, and durable publication own accepted container profiles. |
| Accepted-subset WVB-to-WVO lowering | `native-qualified` | The Windvale lowerer owns the operations and shapes exercised by accepted repository products. |
| Deterministic AOT and baseline JIT | `native-qualified` | Exact WVO/AOT construction and the representative typed W^X patch-plan path pass on both hosts. |
| Native test and differential gate | `native-qualified` | The fixed native retirement plan owns accepted portable, malformed, containment, execution, and artifact cases. |
| Windvale OS accepted image construction | `native-qualified` | Native owners construct the accepted Probe 40 objects, links, and EFI images. |
| Complete accepted WebAssembly generation | `native-qualified` | Native source-to-WVB and WVB-to-Wasm front doors plus the strict engine/probe owner run without .NET. |
| Compiler convergence and clean native bootstrap | `native-qualified` | Digest-bound native seeds reproduce byte-identical Stage 1 and Stage 2 compilers on Windows and Debian. |
| Final Stage 0 recovery archive | `native-qualified` | Release `stage0-recovery-e5a1a7473c57` contains the exact source, history, artifacts, dependencies, licenses, runbook, reports, and checksums. |

Current package, database, browser-promotion, and broader OS work is forward
product breadth. Each new accepted contract must gain a focused native owner;
it must not silently fall back to the managed harness.

## Direct managed entry points

The companion JSON records exactly nine direct managed entry points. All are in
the explicit recovery lane, and no ordinary or release-gating workflow invokes
them.

| Path | Recovery owner |
| --- | --- |
| `Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1` | Windows Stage 0 native compiler seed reconstruction. |
| `Tools/Recovery/Rebuild-Native-Compiler-Seed.sh` | Linux Stage 0 native compiler seed reconstruction. |
| `Tools/Recovery/Rebuild-WebAssembly-Native-Compiler.ps1` | WebAssembly native compiler package reconstruction. |
| `Tools/Recovery/Rebuild-WebAssembly-Native-Backend.ps1` | WebAssembly backend package reconstruction. |
| `Tools/Recovery/Verify-Managed-Bootstrap.ps1` | Windows managed compiler convergence recovery. |
| `Tools/Recovery/Verify-Managed-Bootstrap.sh` | Linux managed compiler convergence recovery. |
| `Tools/Recovery/Rebuild-Os-Probe.ps1` | Windvale OS Stage 0 probe-image reconstruction. |
| `Tools/Verify/Verify-Seed.ps1` | Windows managed Seed recovery and differential evidence. |
| `Tools/Verify/Verify-Seed.sh` | Linux managed Seed recovery and differential evidence. |

The inventory verifier searches website package commands, GitHub workflows,
`Tools/Verify`, and `Tools/Recovery`. A new direct .NET invocation in those
scopes must fail verification unless the same accepted change gives it an
explicit recovery classification. Restoring .NET to an ordinary path requires
a new decision naming the missing native contract.

## Retained managed source owners

| Owner | Retained responsibility |
| --- | --- |
| `Compiler/Reference` | Frozen source compiler oracle and Stage 0 recovery compiler. Forward language semantics are prohibited. |
| `Runtime/Windvale.Bytecode` and `Runtime/Windvale.Runtime` | Frozen decoding, verification, and interpreter oracle. |
| `Compiler/Native` and `Runtime/Windvale.Native` | Stage 0 lowering, ABI, publication, and host-execution provenance. |
| `Assembler/Reference`, `Object-Model/Windvale.ObjectModel`, and `Linker/Reference` | Independent managed object, assembly, link, and historical CLI oracles. |
| `Tools/Windvale.Project` and `Tools/Windvale.Tool` | Historical managed project parsing and orchestration. |
| `Tests/Windvale.Seed.Tests` and `Tests/Windvale.Os.Tests` | Broad historical conformance, malformed-input, OS, artifact, and differential evidence. |
| `Operating-System/Windvale.Bootstrap` | Host-side Stage 0 Probe construction oracle. |
| `Tools/Windvale.Playground` and `Tools/Windvale.Playground.Engine` | Historical managed browser host/engine; the static application is the normal product. |

Their presence in `main` is a recovery-policy choice, not a normal dependency.
The exact qualified recovery release already preserves their source and
provenance independently of the live tree.

## Post-retirement verification rhythm

- Run `Tools/Verify/Verify-Changed.ps1` once after a coherent edit.
- Reuse passing affected-owner evidence while that owner's inputs remain
  unchanged.
- Rerun the narrowest failed or changed owner after a correction.
- Ordinary GitHub implementation/specification changes run affected native
  owners on Windows and Linux and produce development feedback only.
- Dispatch complete cold qualification for a release, promotion, bootstrap,
  security, ABI, or explicit conformance claim.
- Invoke managed commands only for a named recovery drill, security correction,
  or differential question.

Do not run changed-file, Fast, Development, Standard, and Qualification levels
sequentially for the same source state.

## Possible live-source archival

Removing the retained C# implementation from `main` is not required to claim
.NET dependency retirement. It is a separate repository-maintenance decision.
If selected, that change must:

1. preserve the immutable recovery release, checksums, complete history, and
   independent retained copy;
2. add a small in-tree restoration pointer and digest-verification procedure;
3. remove or relocate all managed projects, SDK/solution entry points, and the
   nine direct recovery commands coherently;
4. change the machine-readable inventory and verifier so managed source or a
   direct `dotnet` invocation cannot return silently;
5. perform a clean native dependency audit; and
6. run one explicit dual-host qualification from the resulting committed state.

Until such a decision is accepted and implemented, the retained owners above
remain frozen recovery evidence and must not receive forward product work.

## Historical evidence

- [Decision 0213](../Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
  owns the Stage 0 semantic freeze.
- Decisions 0496 through 0525 record the incremental transfer into focused
  native owners.
- [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md)
  records the final dependency-retirement and recovery-release evidence.
- [Seed-Verification-Evidence.md](Seed-Verification-Evidence.md) preserves the
  exact reports and artifact identities.
