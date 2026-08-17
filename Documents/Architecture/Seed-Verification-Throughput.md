# Seed verification throughput

## Status

Current post-retirement verification architecture. It incorporates
[Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md),
[Decision 0550](../Decisions/0550-Measured-Native-Retirement-Sharding.md),
[Decisions 0553 through 0555](../Decisions/0555-Content-Addressed-Project-Wvb-Development-Checkpoints.md),
[Decision 0557](../Decisions/0557-Separate-Development-Verification-From-Qualification.md),
and
[Decisions 0559 and 0560](../Decisions/0560-Linked-Image-Development-Checkpoints.md).

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
| Qualification | Explicit workflow dispatch or an unresolved comparison that must fail closed | Complete cold native verification-owner shards, WebAssembly owner, and compiler convergence on Windows and Debian | Qualification for the selected source state |

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
- Development jobs may restore a versioned host-specific checkpoint directory.
  Each run attempt writes a new immutable cache key and may restore an earlier
  key by prefix; qualification jobs never bind or restore that directory.
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
- lowered WVO project objects;
- flat linked images plus their exact link maps; and
- packaged hosted applications with their producer closure.

The database development owner additionally selects one exact target when the
changed paths resolve to one maintained test-project closure. Shared, ambiguous,
multi-target, and database-tool inputs fail closed to all fifteen targets.
Hosted selections retain their dependency closure rather than reusing passing
scenario output. Every progress record names its step, current item, requested
target, elapsed time, and checkpoint outcome.

On the measured Windows host, the warm two-case database path fell from the
1,111-second clean fourteen-case owner to about 71 seconds. The complete
change-aware front door, including planner contracts, fell to about 74 seconds.
These are diagnostic host measurements, not portable pass thresholds.

After the owner grew to fifteen targets, the measured all-hit Windows lifecycle
selection runs the `engine` closure in 85,390 ms: tool validation, host storage,
host tree reader, and engine. An independent logical-record selection completes
in 11,860 ms. Qualification remains cold and ignores this selection.

## Qualification sharding

The native verification-owner manifest assigns every owner exactly once to one
of four qualification shards. Manifest order remains canonical inside each
shard; no-argument local execution remains the sequential oracle, and exact
filters remain available for focused work.

Decision 0550 qualified 52 suites and 3,287 cases per host. Four shards reduced
the observed complete workflow from about 40 minutes to about 15 minutes without
dropping a case or consulting a cache. WebAssembly and compiler convergence
remain separate independent qualification jobs.

Sharding reduces wall-clock time, not total evidence or necessarily total hosted
compute. Rebalance only from repeated dual-host measurements.

## Archived managed evidence

The former managed Fast, Development, Standard, and Qualification harness is
preserved only in `stage0-recovery-e5a1a7473c57`. It is absent from `main` and
therefore cannot become an accidental fallback or another step in the ordinary
verification ladder. A named recovery, security, or historical differential
investigation restores that exact release in a separate workspace.

## Next measured optimizations

The measured 733,980 ms `seed-native-front-door` owner is now split. The ordinary
owner validates the pinned manifest and inventory, hashes all 18 artifacts, and
admits all six WVB modules in 13,900 ms on the same Windows host. The complete
105-artifact, 185-assertion audit remains the explicitly named
`seed-native-front-door-reconstruction` qualification owner. This is a 98.11%
wall-clock reduction for ordinary front-door feedback without deleting the
reconstruction evidence.

The WebAssembly owner is now split as well. The checked-in playground package
binds its direct compiler and scalar interpreter WVB/Wasm identities plus the
referenced native compiler, backend, and segmented-backend packages. Its
package-and-core engine checkpoint passes in 29,674 ms on Windows without
regenerating a product. Compared with the last recorded 1,619,500 ms complete
construction-and-engine command, that is a 98.17% development-path reduction;
the complete cold owner remains unchanged for explicit qualification.

The database development owner now checkpoints the six portable tree projects,
their linked images, and their current-host applications. It reports live
target/phase progress and does not repeat project-object admission already
performed by the checkpoint. The direct all-hit Windows owner takes 87,800 ms:
9,190 ms for tools, 24,290 ms for the six portable behaviors, 28,570 ms for
host storage, and 25,660 ms for the host tree reader. That is a 78.19% reduction
from 402,638 ms and crosses the two-minute working target without removing any
of the eight selected behaviors. A subsequent complete changed-file invocation
reported 89,530 ms for the same owner after also passing planner and workflow
policy checks. The complete owner remains cold, reconstructs both target
containers, and retains independent reproducibility and admission evidence.

`Tests/Native/Development-Owner-Dependencies.txt` now declares the source,
producer, and artifact closures for the measured front-door, WebAssembly, and
database owners plus all five database checkpoint families. Its verifier
requires canonical ordering, ordinary repository files, complete closure kinds,
the exact checkpoint-family set, no planner gaps, and selection of the declared
owner.

GitHub Verify run 31852544894 first populated separate host caches, then exact
attempt 2 restored both. The complete development jobs passed in 1m42s on
Windows and 1m15s on Linux, including checkout, runtime setup, restore, planner
and workflow checks, all eight database behaviors, and cleanup. The selected
scope skipped qualification. This closes the five-minute ordinary-feedback
target without turning a cached development result into qualification evidence.

The OS x64 code-emission development path is target-aware as well. Its canonical
manifest maps 56 independent project closures. One leaf source, fixture, or
project selects one six-check target; shared inputs, multiple targets, owner
changes, and qualification retain the complete 56-project, 336-case owner. On
the measured Windows host, first, middle, and final targets completed in 3,476
ms, 3,622 ms, and 3,041 ms, and the complete changed-file front door for the
middle target completed in 4,866 ms. Linux target execution remains independent
host evidence rather than an inference from Windows; the paired shell passed
syntax validation. The complete Windows owner passed all 56 projects and 336
cases in 115,333 ms, making the measured 3,622 ms middle-target owner 31.84
times faster without changing the complete route.

That owner is also manifest-driven. The versioned row for each target owns its
project closure, artifact stem, local result, and exact WVB, WVO, linked-image,
Windows-container, and Linux-container identities. The paired host scripts each
contain one generic pipeline instead of 56 copied pipelines. This reduces the
two scripts from 2,411 lines to 263 lines and makes target additions single-row
changes while retaining the complete owner. The generic Linux path also executes
all 56 local containers; it closes four copied-body omissions that had packaged
and hashed an ELF without performing the declared current-host execution.

One owner invocation now treats those tools as a bounded verified session. It
stages and checks seven private native tool snapshots and verifies workspace
containment once, then gives every target independent compiler, lowerer, linker,
packager, and publisher processes plus separate candidate paths. This retains
immutable publication and per-target exact hashes without repeating 504 Windows
tool hashes and 56 workspace scans. The measured complete Windows owner fell
from 129,638 ms to 82,557 ms, a 36.32 percent reduction and 1.57-fold speedup.
The development path now also reuses deterministic compiler products through
the existing Project 2 content identity. One batch process derives and validates
all selected keys, while a miss retains a separate native compiler process and
all later phases remain fresh. The complete all-hit Windows development owner
takes 74,729 ms, saving another 7,828 ms or 9.48 percent from the session-only
result. The focused `code` target takes 4,076 ms instead of 4,524 ms. A rejected
per-project wrapper design took 94,799 ms because 56 Node and command-shell
hashing lifecycles outweighed compilation; batching the cache boundary is what
makes reuse beneficial. No-argument and qualification execution remain cold.

The library development owner now selects one of seven dependency clusters from
a canonical 29-project manifest. The planner derives each cluster's source
closure from its Project 2 declarations; shared, multi-cluster, owner, and
otherwise ambiguous changes retain the complete route. On the measured Windows
host, the three-case `models` target completed in 4,320 ms and the largest
nine-case `page-storage` target completed in 5,878 ms, compared with 26,348 ms
for the unchanged complete owner. That is a 6.10-fold speedup for the model
cluster and 4.48-fold for the page/storage cluster without removing the full
29-case qualification route.

Library ownership is inventory-bounded as well as target-aware. Modern database,
network, and other library projects outside those 29 cases remain with their
actual focused owners instead of also invoking an unrelated library regression
set. Replaying the 112 commits from 2026-08-16 reduced library-owner selection
from 31 commits to four, avoiding about 711 seconds of measured Windows work
while retaining two focused selections and two legitimate complete selections.

1. Schedule independent development owners concurrently only with explicit CPU
   and memory bounds, isolated state, deterministic log collation, and a retained
   sequential equivalence oracle.
2. Extend compiler incrementality beyond whole-project WVB checkpoints only
   after measuring a stable phase boundary: cache parsed modules, symbols, WIR,
   or native objects by complete dependency identity.

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
