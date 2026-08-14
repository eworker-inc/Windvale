# .NET retirement and archival inventory

> Inventory snapshot: 14 August 2026

The accepted Windows and Linux workflow is native, and managed Stage 0 source is
now archived outside `main`. The machine-readable
[inventory](Dotnet-Retirement-Inventory.json) and
`Tools/Verify/Verify-Dotnet-Retirement-Inventory.ps1` enforce that boundary.

## Current boundary

| Counter | Current value |
| --- | ---: |
| Tracked managed source/project/build files | **0** |
| Direct managed operational entry points | **0** |
| Normal managed entry points | **0** |
| Qualified Stage 0 recovery releases | **1** |
| Independently retained recovery copies at qualification | **1** |

The verifier examines tracked files rather than local ignored build caches. It
rejects C#, F#, Visual Basic, Razor, managed project/solution metadata, SDK/NuGet
root metadata, and direct managed invocations in workflows, verification tools,
recovery tools, or package scripts.

## Accepted native surfaces

Decision 0526 qualified the accepted source-to-WVB, verification, inspection,
execution, assembly, object, link, PE/ELF and hosted-WVB packaging, WebAssembly,
OS-image, and compiler-convergence routes on Windows and Linux. Later package,
database, browser, and OS breadth must gain focused native owners; it does not
silently fall back to the archived harness.

Ordinary commands include:

- `Tools/Native/Build-Wvb.cmd` and `.sh`;
- `Tools/Native/Verify-Wvb.cmd` and `.sh`;
- `Tools/Native/Inspect-Wvb.cmd` and `.sh`;
- `Tools/Native/Run-Wvb.cmd` and `.sh`;
- the paired WVA, WVO, link, lower, package, WebAssembly, OS, and bootstrap
  owners selected by changed-file verification.

## Immutable Stage 0 recovery identity

| Identity | Value |
| --- | --- |
| Release/tag | `stage0-recovery-e5a1a7473c57` |
| Commit | `e5a1a7473c57935c5dfcf09b78b18c3c099e70ef` |
| Tree | `9950150f14cd4864b06c853ab6a716fa6e04495a` |
| Source bundle SHA-256 | `1830bf95b583267b69229125edb83521733a36f27a4d49fe371534734bcc0892` |
| Supplemental checksums SHA-256 | `de18793e13fa4cf429070739708e2e3bebc4cebbd5eacde5832dca9781928267` |
| Recovery pointer | [`Bootstrap/Stage0/README.md`](../../Bootstrap/Stage0/README.md) |

The release contains complete history, source and artifact inventories,
dependencies, licenses, runbook, checksums, Windows/Linux reconstruction reports,
and cross-host evidence. Managed recovery work begins from that release in a
separate workspace.

## Verification rhythm

- Run `Tools/Verify/Verify-Changed.ps1` once after a coherent edit.
- Reuse passing affected-owner evidence while its inputs remain unchanged.
- After a failure, rerun the narrowest affected owner.
- Ordinary GitHub changes run affected owners on Windows and Linux.
- Dispatch the complete cold dual-host gate only for a selected release,
  promotion, bootstrap, security, ABI, or conformance state.
- After this archival change is committed, dispatch that gate once before
  applying a `native-only-baseline-<commit12>` checkpoint tag.

Do not run a ladder of progressively broader verifiers for the same unchanged
tree. The full gate is a deliberate qualification event, not per-commit feedback.

## History and custody

- Decision 0213 froze Stage 0 source semantics and established the native front
  door.
- Decisions 0496 through 0525 record the incremental native ownership transfer.
- Decision 0526 records the completed retirement qualification and immutable
  recovery release.
- Decision 0527 established native-only forward development.
- Decision 0557 separated ordinary affected-owner feedback from explicit full
  qualification.
- Decision 0558 removes the frozen live managed copy from `main`.

Historical decisions may accurately mention files and commands that existed at
their acceptance date. Current runbooks and contribution guidance must point to
native commands or the immutable recovery release instead.
