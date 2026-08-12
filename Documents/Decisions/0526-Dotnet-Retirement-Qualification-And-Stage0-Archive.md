# Decision 0526: .NET retirement qualification and final Stage 0 archive

- Status: Qualified and implemented
- Date: 2026-08-12
- Completes: Decisions 0057, 0178, 0213, 0457, 0458, and 0525
- Scope: the accepted repository build, verification, test, packaging,
  execution, WebAssembly, OS-image construction, and bootstrap paths on
  Windows and Linux

## Context

Decision 0057 requires eight conditions before C# and .NET can leave Windvale's
normal workflow. The migration deliberately accumulated focused native owners
rather than repeatedly running a broad managed gate after each small transfer.
Decision 0525 finally composed those owners into six independent Windows/Linux
jobs: the fixed native retirement suite, the complete WebAssembly owner, and
native compiler convergence on each permanent host.

The last qualification work also constructed the final Stage 0 recovery
archive required by Decision 0178. Independent generation showed that Git
2.55.0.windows.3 and Git 2.54.0 can choose different valid compressed pack
representations for the same complete history. The deterministic metadata,
commit, tree, source inventory, and artifact identities agree. Qualification
therefore tests one exact selected release bundle on both hosts rather than
mistaking Git's internal compression choice for source identity.

## Decision

.NET is retired from Windvale's normal workflow for the accepted repository
subset at commit
`e5a1a7473c57935c5dfcf09b78b18c3c099e70ef`, tree
`9950150f14cd4864b06c853ab6a716fa6e04495a`.

The ordinary Windows and Linux build, verification, test, packaging,
execution, WebAssembly, OS-image construction, and bootstrap routes use the
qualified native owners. The direct-entry audit contains zero normal managed
entry points and nine explicitly classified recovery-only entry points.

The C#/.NET source is not deleted. It remains a frozen, provenance-preserving
Stage 0 recovery and independent differential oracle. It may be invoked only
through the explicit recovery commands recorded by the retirement inventory
and final release. Forward source-language semantics continue exclusively in
`Compiler/Windvale` under Decision 0213. Restoring .NET to an ordinary or
required qualification path requires a later decision that names the missing
native contract; it must not be used as an implicit fallback.

The final immutable recovery release is
[`stage0-recovery-e5a1a7473c57`](https://github.com/eworker-inc/Windvale/releases/tag/stage0-recovery-e5a1a7473c57).
Its 13 assets include the complete Git history, exact source and artifact
inventories, dependency and license inventories, runbook, base checksums,
Windows and Linux recovery reports, cross-host report, and supplemental release
checksums. One independently held E-Worker copy matches all 13 published
assets; its private location remains outside the public repository.

## Qualification evidence

The exact release commit passed the independent six-job
[`Verification gate`](https://github.com/eworker-inc/Windvale/actions/runs/31608597009):

1. Windows passed all 45 native retirement suites and all 3,206 fixed cases.
2. Debian passed the same 45 suites and 3,206 cases.
3. Windows native compiler Stage 1/Stage 2 convergence passed.
4. Debian native compiler Stage 1/Stage 2 convergence passed.
5. The complete native WebAssembly owner passed on Windows.
6. The same WebAssembly owner passed on Linux.

The exact selected release bundle then passed the paired
[`Stage 0 recovery proof`](https://github.com/eworker-inc/Windvale/actions/runs/31609676682):

- Windows reconstructed the historical native compiler seed, admitted the
  archived native front door, and reached current native convergence in 344.2
  seconds.
- Linux repeated those checks in 332 seconds against the identical
  Windows-generated release bytes.
- Independent Windows and Linux archive generation produced byte-identical
  metadata, source inventory, artifact inventory, dependency inventory,
  license inventory, and runbook, while both host-specific Git bundles verified
  the exact commit, tree, history, and inventories.

The published source bundle SHA-256 is
`1830bf95b583267b69229125edb83521733a36f27a4d49fe371534734bcc0892`.
The supplemental checksum file SHA-256 is
`de18793e13fa4cf429070739708e2e3bebc4cebbd5eacde5832dca9781928267`.

## Decision 0057 condition closure

| Condition | Qualified evidence |
| --- | --- |
| 1. Compiler self-convergence | Native Stage 1 and Stage 2 convergence passed on Windows and Debian from the exact release commit. |
| 2. Native normal toolchain | The six-job gate exercised the accepted build, verify, test, assemble, lower, link, package, execute, OS-image, and WebAssembly paths with zero normal managed entry points. |
| 3. Native WVB verification | Digest-bound native verification protects every accepted native build and execution path; malformed and unsafe boundaries are in the fixed suites. |
| 4. CLR-independent runtime contracts | The accepted runner, hosted runtime/service tables, values, traps, capabilities, entry points, publication transactions, and host adapters execute through native owners on both hosts. |
| 5. Deterministic AOT and baseline JIT | Exact WVO/link/PE/ELF products and the source-to-AOT chain pass on both hosts; the six-case typed patch-plan and W^X publication owner qualifies the accepted baseline-JIT path. |
| 6. Differential agreement | Fixed WVB, WVO, WVA, container, malformed-input, containment, execution, AOT, and baseline-JIT cases pass through the 3,206-case native plan on both hosts. |
| 7. Clean native bootstrap | Digest-bound native seeds and provenance inventories reproduce byte-identical Stage 1/Stage 2 compilers on both hosts. |
| 8. Final Stage 0 archive | The immutable 13-asset release was reconstructed from the same selected bundle on Windows and Linux and retained independently of GitHub. |

This closes the retirement dependency gate for the accepted subset. It does not
claim that every future source feature, capability-bearing program, optimizer,
OS fault scenario, package service, browser, or library is already complete.
Those are forward product breadth and must gain focused native owners when their
contracts become accepted; they do not keep .NET in the normal workflow.

## Verification after retirement

Use `Tools/Verify/Verify-Changed.ps1` once after a coherent edit. Its native
planner selects only the affected focused owners and refuses uncovered gaps.
GitHub runs the six independent qualification jobs concurrently for
implementation and specification changes. Do not run the managed Seed harness
or several increasingly broad local levels for the same unchanged tree. Invoke
the managed recovery commands only for a named recovery drill, security fix, or
differential question.

## Consequences

- Language and library expansion can proceed without widening the frozen C#
  compiler or carrying a normal CLR dependency.
- Candidate labels that remain in the project ledger describe future feature or
  product breadth outside the accepted retirement subset, not an incomplete
  dependency cutover.
- The immutable recovery tag remains on the exact qualified implementation
  commit. Later documentation commits may describe that evidence but do not
  move or replace the release identity.
- Complete Git history, source, dependencies, licenses, build instructions, and
  paired-host reconstruction evidence remain recoverable without making Stage 0
  a maintained forward implementation.

## Reconsideration triggers

Revisit this decision if the published checksums fail, the recovery bundle can
no longer reconstruct on a documented supported host, an accepted normal path
is found to invoke .NET, or a security correction requires rebuilding Stage 0.
A future feature that lacks native coverage is a focused implementation gap, not
by itself a reason to reverse retirement.
