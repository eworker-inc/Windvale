# Seed verification throughput

## Status

Current post-retirement verification architecture. It incorporates
[Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md),
[Decision 0550](../Decisions/0550-Measured-Native-Retirement-Sharding.md),
[Decisions 0553 through 0555](../Decisions/0555-Content-Addressed-Project-Wvb-Development-Checkpoints.md),
and
[Decision 0557](../Decisions/0557-Separate-Development-Verification-From-Qualification.md).

Historical managed-suite optimization measurements remain in the dated
decisions, qualification evidence, and Git history. They do not define the
normal development path after .NET retirement.

## Purpose

Verification must answer the question being asked without turning every edit or
commit into a release qualification.

- Development asks whether the owners affected by a change still pass.
- Qualification asks whether one deliberately selected source state satisfies
  the complete independent Windows/Linux evidence contract.
- Recovery and differential work asks whether the frozen Stage 0 oracle still
  reconstructs or agrees at a named boundary.

These are different evidence modes. Running all of them for the same unchanged
state adds elapsed time without strengthening the selected claim.

## Normal verification modes

| Mode | Trigger | Work | Claim |
| --- | --- | --- | --- |
| Lightweight | Ordinary Markdown, root license, or editor-package-only changes | `git diff --check`, link/path review, and editor verification when relevant | Documentation or editor development feedback |
| Website | Static site, browser package, Cloudflare function, or website-tool changes | `Tools/Verify/Verify-Website.ps1` | Website development feedback |
| Development | Implementation or specification changes with mapped native owners | `Tools/Verify/Verify-Changed.ps1` selects affected owners in canonical order | Development feedback only |
| Qualification | Explicit workflow dispatch or an unresolved comparison that must fail closed | Complete cold native retirement shards, WebAssembly owner, and compiler convergence on Windows and Debian | Qualification for the selected source state |

The development planner refuses an uncovered path. It does not use the managed
Seed harness or the complete unfiltered native suite as an implicit fallback.
A missing mapping must gain a focused native owner or an explicit qualification
decision.

## Local workflow

While editing, inspect the selected work without running it:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -PlanOnly
```

After a coherent edit settles, run the change-aware verifier once:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

Rules:

1. Run a verifier after a coherent batch, not after every small edit or commit.
2. Reuse a passing owner result while its source, producer, fixture, tool, and
   expected-contract inputs remain unchanged.
3. After failure, rerun the narrowest failed or changed owner.
4. Do not rerun merely because the change is about to be committed or pushed.
5. Run at most one broader final gate when the claim or changed boundary
   requires it.
6. Do not execute changed-file, Fast, Development, Standard, and Qualification
   sequentially for one unchanged state.

## GitHub workflow

The `Verify` workflow classifies the exact base/head comparison.

- Pull requests and pushes to `main` use lightweight, website, or development
  scope according to changed paths.
- Development scope runs the affected native owners on Windows and Linux. These
  jobs do not create a conformance or qualification claim.
- Manual `workflow_dispatch` selects complete qualification.
- An empty path set, missing base, unresolved comparison, or explicit
  qualification request fails closed to qualification rather than guessing.
- The aggregate `Verification gate` remains stable for branch protection while
  requiring only the jobs selected by the classifier.
- Workflow concurrency cancels superseded runs for the same workflow and ref.

Complete qualification is appropriate for a release candidate, artifact
promotion, bootstrap or recovery claim, security boundary, ABI change, or a
deliberate cross-host conformance statement. It is not a per-commit gate.

## Native owner model

Every accepted implementation boundary has one or more focused native owners.
The changed-file planner maps source, specification, project, fixture, runtime,
tool, and workflow paths to those owners.

An owner should:

- name the contract or failure family it protects;
- declare every input that can change its result;
- use isolated temporary and output state;
- preserve exact input and destination behavior on rejection;
- report stable success totals separately from diagnostic timing;
- avoid rebuilding an unrelated product merely to run one behavior; and
- remain runnable independently through an exact filter.

Add cases to an existing owner when they protect the same contract. Create a new
top-level owner only for a genuinely independent boundary, resource profile, or
failure domain.

## Development checkpoints

Content-addressed checkpoints may accelerate deterministic construction during
development. A valid key includes all input digests, producing tool identity,
target/profile, relevant options, and the checkpoint format. Every hit must
revalidate the manifest, output size, digest, and required structural admission.

Checkpoints cache immutable products, not passing behavior. The owner reruns the
execution, recovery, denial, mutation, or other behavior affected by the edit.
Qualification ignores development checkpoint state and reconstructs its evidence
cold.

Implemented database-path checkpoints currently cover:

- source-built project WVB products;
- lowered WVO project objects; and
- packaged hosted applications with their producer closure.

On the measured Windows host, the warm two-case database path fell from the
1,111-second clean fourteen-case owner to about 71 seconds. The complete
change-aware front door, including planner contracts, fell to about 74 seconds.
These are diagnostic host measurements, not portable pass thresholds.

## Qualification sharding

The native retirement manifest assigns every suite exactly once to one of four
shards. Manifest order remains canonical inside each shard; no-argument local
execution remains the sequential oracle, and exact filters remain available for
focused work.

Decision 0550 qualified 52 suites and 3,287 cases per host. Four shards reduced
the observed complete workflow from about 40 minutes to about 15 minutes without
dropping a case or consulting a cache. WebAssembly and compiler convergence
remain separate independent qualification jobs.

Sharding reduces wall-clock time, not total evidence or necessarily total hosted
compute. Rebalance only from repeated dual-host measurements.

## Managed recovery verifier

`Tools/Verify/Verify-Seed.ps1` and `.sh` remain explicit Stage 0 recovery and
differential commands. Their Fast, Development, Standard, and Qualification
levels describe the frozen managed harness; they are not the normal post-
retirement workflow.

Use them only for a named recovery drill, security correction, or differential
question. Select the smallest applicable area/filter and state why managed
evidence was required. Do not run a managed broader level after a passing native
development result unless the selected claim specifically needs that independent
oracle.

## Next measured optimizations

1. Separate artifact construction from execution inside the WebAssembly owner so
   an engine-only change does not regenerate unchanged WVB and Wasm products.
2. Extend exact checkpoints to other repeatedly reconstructed compiler, lowerer,
   verifier, linker, and hosted-application products only when timing identifies
   them as a development bottleneck.
3. Replace ad hoc path lists with declarative per-owner dependency manifests when
   that representation can be verified against the current planner without
   losing fail-closed coverage.
4. Schedule independent development owners concurrently only with explicit CPU
   and memory bounds, isolated state, deterministic log collation, and a retained
   sequential equivalence oracle.
5. Treat compiler incrementality as a separate optimization: cache parsed
   modules, symbols, WIR, and native objects by complete dependency identity.

The working targets are a repeated affected-owner local run under two minutes
and ordinary pull-request feedback under five minutes, excluding runner queueing.
Targets are revised from measured Windows/Linux evidence; they are not reasons to
skip a required boundary.

## Evidence rules

- Timing is diagnostic host evidence and never enters portable conformance.
- Filtered, fail-fast, checkpointed, or development runs cannot write a
  qualification report.
- Exact byte comparisons remain mandatory where bytes are part of the contract.
- Malformed-input, containment, capability, publication, and teardown behavior
  remains owned even when construction is cached.
- A faster implementation may remove demonstrated overhead; it may not silently
  remove independent verification or change a failure contract.
- Cross-host qualification claims require reports from Windows and real Debian
  for the same selected source state.
