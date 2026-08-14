# Windvale Seed development

This runbook describes the current native-only development loop. Historical
managed commands belong to the immutable Stage 0 recovery release, not this
checkout.

## Prerequisites

- Windows x64 with the inbox command processor, or Linux x64 with Bash and
  `sha256sum`;
- PowerShell 7 for repository verification orchestration; and
- Node.js 24 only for website or WebAssembly owners selected by a change.

The active repository contains no managed projects and requires no .NET SDK.
Native launchers verify the exact checked-in tools before use.

## Development rhythm

After a coherent edit, run one change-aware verifier:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

Use `-PlanOnly` when you only need to inspect the affected owners. The planner
selects focused native suites in canonical order and refuses unmapped active
boundaries. It does not fall back to an unfiltered or managed gate.

Verification levels are alternatives:

- do not run changed-file, focused owner, shard, and complete qualification
  sequentially against one unchanged tree;
- reuse a passing result while that owner's inputs remain unchanged;
- after a failure, rerun the narrowest affected owner; and
- run at most one broader final gate when the risk or claim requires it.

Ordinary GitHub pushes and pull requests run affected owners on Windows and
Linux. The complete four-shard native suite, WebAssembly owner, and compiler
convergence run only through explicit workflow dispatch for a selected release,
promotion, bootstrap, security, ABI, or conformance state.

To run one named retirement-suite owner directly:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter seed-native-front-door
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter seed-native-front-door
```

Do not run the complete manifest locally merely because a commit or push is
next.

## Build, verify, inspect, and run WVB

Build the portable example on Windows:

```bat
Tools\Native\Build-Wvb.cmd Examples\Seed\Sum-Data.wvproj Artifacts\Sum-Data.wvb
Tools\Native\Verify-Wvb.cmd Artifacts\Sum-Data.wvb
Tools\Native\Inspect-Wvb.cmd Artifacts\Sum-Data.wvb
Tools\Native\Run-Wvb.cmd Artifacts\Sum-Data.wvb
```

Or on Linux:

```sh
./Tools/Native/Build-Wvb.sh Examples/Seed/Sum-Data.wvproj Artifacts/Sum-Data.wvb
./Tools/Native/Verify-Wvb.sh Artifacts/Sum-Data.wvb
./Tools/Native/Inspect-Wvb.sh Artifacts/Sum-Data.wvb
./Tools/Native/Run-Wvb.sh Artifacts/Sum-Data.wvb
```

The runner prints `Result: 29`. Build publication uses a private candidate,
revalidates it, and atomically replaces only an admitted destination. Project 1
manifests enumerate their roots and explicit source closure; they do not grant
capabilities or discover ambient source files.

The detailed command contract and limits are in
[Native-Source-To-Wvb.md](Native-Source-To-Wvb.md).

## Lower, verify, and inspect WVO

Lower an accepted WVB to a native object on Windows:

```bat
Tools\Native\Lower-Wvb-To-Wvo.cmd Artifacts\Sum-Data.wvb Artifacts\Sum-Data.wvo
Tools\Native\Verify-Wvo.cmd Artifacts\Sum-Data.wvo
Tools\Native\Inspect-Wvo.cmd Artifacts\Sum-Data.wvo
```

On Linux:

```sh
./Tools/Native/Lower-Wvb-To-Wvo.sh Artifacts/Sum-Data.wvb Artifacts/Sum-Data.wvo
./Tools/Native/Verify-Wvo.sh Artifacts/Sum-Data.wvo
./Tools/Native/Inspect-Wvo.sh Artifacts/Sum-Data.wvo
```

Treat every WVB and WVO as untrusted input. The launchers verify their pinned
applications and the object publisher admits the exact candidate before durable
replacement.

## Assemble and link WVA

Assemble the canonical example and provider on Windows:

```bat
Tools\Native\Assemble-Wva.cmd Examples\Assembler\Hello-Object.wva Artifacts\Hello-Object.wvo
Tools\Native\Assemble-Wva.cmd Examples\Linker\Console-Provider.wva Artifacts\Console-Provider.wvo
Tools\Native\Verify-Wvo.cmd Artifacts\Hello-Object.wvo
Tools\Native\Link-Wvo.cmd 1048576 Main Artifacts\Hello-Linked.bin Artifacts\Hello-Object.wvo Artifacts\Console-Provider.wvo
```

On Linux:

```sh
./Tools/Native/Assemble-Wva.sh Examples/Assembler/Hello-Object.wva Artifacts/Hello-Object.wvo
./Tools/Native/Assemble-Wva.sh Examples/Linker/Console-Provider.wva Artifacts/Console-Provider.wvo
./Tools/Native/Verify-Wvo.sh Artifacts/Hello-Object.wvo
./Tools/Native/Link-Wvo.sh 1048576 Main Artifacts/Hello-Linked.bin Artifacts/Hello-Object.wvo Artifacts/Console-Provider.wvo
```

WVA assembly, WVO verification, and linking are distinct contracts. A successful
assembly does not bypass object admission, and a linked flat image is not
implicitly a PE, ELF, or Windvale OS image.

## Compiler convergence

Run compiler/bootstrap convergence only when the compiler inventory, bootstrap
seed, construction path, or an explicit qualification claim changes:

```bat
Tools\Verify\Verify-Bootstrap.cmd
```

```sh
./Tools/Verify/Verify-Bootstrap.sh
```

The verifier admits the digest-bound native seed and publisher, constructs Stage
1 from the exact project inventory, uses that product to construct Stage 2, and
requires byte identity. “Stage 1” and “Stage 2” describe compiler convergence;
they are not product release numbers.

## OS, WebAssembly, and website owners

Use the focused owner selected by `Verify-Changed.ps1`:

- OS image, boot, firmware, or kernel-seam changes use the mapped OS suite and
  the relevant live boot gate when that claim is made;
- WebAssembly generator, ABI, engine, or fixture changes use
  `Tools/Verify/Verify-WebAssembly.ps1`; and
- static site, playground, function, or browser packaging changes use
  `Tools/Verify/Verify-Website.ps1`.

These owners are not a ladder. A change that affects only one boundary should
not pay for unrelated OS, browser, database, or bootstrap work.

## Stage 0 recovery

The final managed state is the immutable release
`stage0-recovery-e5a1a7473c57`. Follow
[`Bootstrap/Stage0/README.md`](../../Bootstrap/Stage0/README.md) and restore it
in a separate workspace for a named recovery, security, or historical
differential investigation.

Do not paste archived commands into current runbooks or reintroduce managed
source to make an old comparison convenient. Current behavior must gain a
focused native owner. A managed correction requires a new decision explaining
why the qualified archive is insufficient.

## Before committing

1. Review the exact changed paths and preserve unrelated work.
2. Run one proportional verifier after the edit settles.
3. Record the focused checks and any broader gate intentionally not run.
4. Stage only task files and use a DCO sign-off.
5. Do not rerun a passing verifier solely because commit or push is next.
