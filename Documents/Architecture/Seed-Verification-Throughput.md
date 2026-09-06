# Seed verification throughput

> Status: Current post-retirement verification architecture
> Authority: Informative
> Last reviewed: 2026-09-06

It incorporates
[Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md),
[Decision 0550](../Decisions/0550-Measured-Native-Retirement-Sharding.md),
[Decisions 0553 through 0555](../Decisions/0555-Content-Addressed-Project-Wvb-Development-Checkpoints.md),
[Decision 0557](../Decisions/0557-Separate-Development-Verification-From-Qualification.md),
and
[Decisions 0559 and 0560](../Decisions/0560-Linked-Image-Development-Checkpoints.md),
with exact database target-set selection under
[Decision 0944](../Decisions/0944-Select-Exact-Database-Development-Target-Sets.md)
and portable database packaging ownership under
[Decisions 0945 and 0946](../Decisions/0946-Delegate-Portable-Database-Reproducibility-To-Toolchain-Owners.md),
then extends the same evidence-graph direction to complete qualification under
[Decision 0947](../Decisions/0947-Treat-Complete-Qualification-As-One-Evidence-Graph.md)
and planner-proved development-result reuse under
[Decision 0948](../Decisions/0948-Reuse-Development-Owner-Results-Across-Unrelated-Source-Trees.md),
with the existing qualification runners balanced under
[Decision 0949](../Decisions/0949-Balance-Qualification-Shards-By-Declared-Cost.md).

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
| Qualification resume | Explicit workflow dispatch naming one shard and, optionally, its first owner | The selected cold shard or canonical shard tail on Windows and Debian | Supplemental evidence for owners not completed by an earlier qualification; never a complete qualification by itself |

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
  key by prefix. Restore and save are separate steps, so a late development
  assertion failure still preserves every completely published content-addressed
  checkpoint. Qualification jobs never bind, restore, or save that directory.
- Manual `workflow_dispatch` selects complete qualification by default. An
  explicit shard selects only that shard on both hosts; an optional canonical
  start owner resumes its tail. The selection is validated before runners
  start, and bootstrap plus WebAssembly remain complete-qualification jobs.
- An empty path set, missing base, unresolved comparison, or explicit
  qualification request fails closed to qualification rather than guessing.
- The aggregate `Verification gate` remains stable for branch protection and
  complete qualification. A resumed dispatch instead publishes a visibly
  distinct `Partial qualification gate` that requires both selected host jobs
  and cannot be presented as a complete release gate.
- Workflow concurrency retains one running run and only the latest pending run
  for the same workflow and ref. A new push replaces an older pending run but
  does not discard an in-flight compiler reconstruction or its eventual cache.

Complete qualification is appropriate for a release candidate, artifact
promotion, bootstrap or recovery claim, security boundary, ABI change, or a
deliberate cross-host conformance statement. It is not a per-commit gate.
Resume mode is appropriate only after a complete qualification stopped at a
known owner and the passing owners' complete declared inputs remain unchanged.
Its result must be composed with those retained results explicitly; it does not
convert a failed or incomplete workflow into a pass.

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
Qualification retains explicitly required independent constructions; a
checkpoint hit alone proves neither behavior nor reproducibility.

Segmented hosted packaging stores a profile-independent native image separately
from each profile's final application. The private `segmented-hosted-image-v1`
checkpoint binds the WVB bytes, host, complete producer graph, and validator
identity. It retains the existing WVLI manifest and at most sixteen 4 MiB
fragments; manifest bounds are checked before fragment reads. Producer and input
changes reject publication, and every hit rechecks identities and image
structure. Profiles 1 through 8 may share that image while keeping separate
container keys. Packaging copies the image into scratch space because service
construction appends chunks beside it. Independent reconstruction remains the
uncached wrapper path. See the
[image-reuse evidence](../Evidence/2026-09-06-Segmented-Image-Profile-Reuse.json)
for exact package comparisons and the limits of the measured workload.

Implemented database-path checkpoints currently cover:

- source-built project WVB products;
- lowered WVO project objects;
- flat linked images plus their exact link maps; and
- packaged hosted applications with their producer closure.

The database development owner additionally derives a deterministic target set
from every maintained test-project closure affected by the changed paths. A
versioned 53-case inventory maps selectors to portable and hosted cases; the
shared planner unions exact case labels, while ambiguous or complete selections
fail closed to `all`. Hosted selections retain their dependency closure rather
than reusing passing scenario output. Every progress record names its step,
current item, requested target set, elapsed time, and checkpoint outcome.

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

`Invoke-WindvaleTests.ps1 -Shard <1-4> -StartAtOwner <owner>` selects the named
owner and every later owner in that shard's canonical order. The runner rejects
a missing shard, a malformed or unknown owner, and an owner assigned to another
shard. Its structured mode records the shard and start owner. This preserves
the cold qualification behavior for the selected tail while avoiding replay of
unaffected shards after a late deterministic failure.

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

The former measured 733,980 ms Seed front-door reconstruction mixed immutable
Seed admission with exact hashes for mutable current source. It is retired, not
repinned. The remaining `seed-native-front-door` owner validates the pinned
manifest and inventory, hashes all 18 immutable artifacts, and admits all six
WVB modules in 13,900 ms on the same Windows host. The separate
`seed-native-console-aot` owner reconstructs the canonical source-to-WVB,
WVB-to-WVO, link, package, and execution chain. Current compiler fixed points
belong to split compiler convergence. This removes the redundant 105-artifact,
185-assertion hash farm from every complete qualification without reducing an
owned current or immutable boundary.

The checked-in WebAssembly playground package binds its direct compiler and
scalar interpreter WVB/Wasm identities plus the referenced native compiler,
backend, and segmented-backend packages. Its package-and-core engine checkpoint
passes in 29,674 ms on Windows without regenerating a product. It is the
independent blanket qualification contract on both hosts. The old 1,619,500 ms
cold command attempted to compile current Language 1.0 fixtures through frozen
Seed and regenerate an untracked historical compiler workload; it is not a
valid general qualification boundary. Full current-source WebAssembly
reconstruction remains an explicit WebAssembly promotion task and must migrate
to the current split compiler before it can make that stronger claim.

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

After the database development owner expanded to 50 cases, a complete all-hit
Windows run still took 708,690 ms. Project-object version 1 was a false hit at
the trust boundary: it rehashed and byte-compared an immutable admitted WVO,
then ran the complete structural WVO inspector again. For the fifteen-module
`TransactionParentGroups` project, the wrapper averaged 10,339.75 ms and the
redundant admission alone averaged 9,164.48 ms. Version 2 binds the exact cache
driver and its digest-pinned inspector policy into the project key, retains
admission before immutable publication, and proves hits through the complete
record plus rehashed private copies. Its first fresh creation took 23,257.74 ms;
the next two hits took 264.10 and 250.11 ms, a 97.51 percent boundary reduction
and 40.2-fold speedup. The complete all-hit 50-case owner falls from 708,690 ms
to 500,610 ms, saving 208,080 ms or 29.36 percent for a 1.42-fold speedup;
portable-case time falls from 345,980 ms to 198,870 ms. Even the first coherent
version-2 population gate falls from the preceding 1,495,600 ms cold reference
to 950,050 ms because each new WVO is admitted once rather than again after
materialization. Qualification remains cache-independent.

The remaining database segmented path is checkpointed at its measured stable
boundary. A representative project spent 11,230 ms compiling, 15,588 ms in
segmented WVO staging, 338 ms linking, and 194 ms in canonical transport;
compile plus staging therefore owned 97.9 percent of construction time.
`segmented-project-v1` binds the complete project closure, build driver, all
three digest-pinned segmented producers, and checkpoint driver, then stores the
exact WVB plus the structurally admitted canonical manifest and fragments.
Hits rehash the immutable entry and every private materialization. The two-case
tree-completion section fell from 64,680 ms during population to 7,170 ms on
hits, an 88.9 percent reduction. The composed host-tree-writer step fell from
55,640 ms during population to 16,020 ms on hits, and the persistent writer
from 46,390 ms to 2,520 ms. All current-host executions, provider overlays,
restart checks, and interruption cases still run. Cold duplicate compilation
and both-host packaging remain qualification-only evidence. The final all-hit
50-case owner takes 323,820 ms, down from 500,610 ms: 176,790 ms or 35.31
percent less wall time for a 1.55-fold speedup. Its portable section falls from
198,870 ms to 115,980 ms, a 41.68 percent reduction.

Hosted-application hits now reuse producer trust within the same bounded
database-owner invocation. Before this change, one unchanged 5.65 MiB hit took
1,573 through 2,393 ms, including 447 through 930 ms to reopen and revalidate
the same 72-artifact, 21.7 MiB producer closure. The owner-session service
validates that closure once, retains its exact buffers, and reconstructs the
unchanged version-1 key for each independently hashed WVB and fragment set.
The same hit takes 129 through 165 ms through the session. Misses still use the
standalone full-validation publisher; corruption never falls back. The all-hit
change-aware 50-case owner falls from 323,820 ms to 281,240 ms, saving another
42,580 ms or 13.15 percent. Its portable section falls from 115,980 ms to
81,940 ms, a 29.35 percent reduction. Relative to the earlier 500,610 ms
project-object-v2 result, the two subsequent changes save 219,370 ms or 43.82
percent and make the owner 1.78 times faster.

The next profile separated host-root execution from preparation. Twelve fresh
publication, replay, interruption, and recovery processes took 980 ms in the
main case, while its direct three-WVO link took 16,460 ms; project admission
took 310 ms and application materialization 190 ms. The related root-fill,
root-split, and read links took 15,180, 15,650, and 10,370 ms, versus 210 through
260 ms for project hits and 140 through 170 ms for application hits. Fresh
process semantics were therefore not the bottleneck and remain unchanged.

Ordered `linked-image-v2` checkpoints now hash one through 64 exact WVO buffers
in command order, snapshot them before cold linking, include all current-host
producer bytes, and publish an immutable image/map/record directory. Current-
host database development initially used this path for every eligible multi-
object link; direct qualification paths stay unchanged. The final change-aware
all-hit Windows owner falls from 281,240 ms to
101,370 ms, saving 179,870 ms or 63.96 percent for a 2.77-fold speedup in this
slice. Host-root-writer falls from 61,810 ms to 3,560 ms, host storage from
24,620 ms to 8,140 ms, and host-local-service from 29,010 ms to 1,450 ms.
Relative to the earlier 500,610 ms project-object-v2 result, the combined
development loop saves 399,240 ms or 79.75 percent and is 4.94 times faster.

The remaining portable single-input path repeated a batch front door, Node key
process, several `certutil` hashes, and copy comparisons on every hit. A
controlled identical-input comparison measured a 641.6 ms version-1 mean and
a 107.0 ms version-2 mean. A live TreeNode case separated 220 through 240 ms
of project materialization, 580 through 630 ms of version-1 linking, 80 ms of
map/copy work, 160 through 170 ms of hosted-application materialization, and
340 through 350 ms of fresh execution. Database development now uses the
single version-2 producer for all 38 ordinary portable single-object links and
every eligible host multi-object link; three segmented portable cases retain
their separate transport checkpoint. The obsolete version-1 wrappers and key
helper have no consumer and are removed.
The coherent population run retained 37 real new links and all 50 executions
in 411,770 ms. The final change-aware all-hit owner takes 85,010 ms, down
16,360 ms or 16.14 percent from 101,370 ms; the portable section falls from
74,110 ms to 58,410 ms, saving 15,700 ms or 21.18 percent. Relative to the
earlier 500,610 ms project-object-v2 result, the combined loop is 5.89 times
faster.

The remaining project-object hit still started Node and rehashed the same build
driver, lowerer, checkpoint driver, and workspace for every project. Ten empty
Node invocations averaged 54.4 ms and the old project-key command averaged
124.9 ms. Project-key format 2 places that common producer closure before each
project closure, streams it once into a clonable hash context, and lets the
existing bounded owner session serve read-only project-object hits. Eight
controlled standalone hits averaged 149.0 ms; eight session hits averaged 98.0
ms. The representative TreeNode case fell from 940 through 960 ms to 810 ms
without removing fresh application execution. The same context is reused across
all selected OS x64 project-WVB keys. Cold publishers recheck their complete
keyed input evidence before immutable publication, and a miss retains the
standalone publisher.

The accepted session design retains no producer file buffers. On the measured
Windows host it used 73.78 MiB working set and 83.14 MiB private memory, versus
69.84 MiB and 80.32 MiB for the hosted-only session. A rejected whole-buffer
prototype used 107.45 MiB and 117.61 MiB. Producer count, producer aggregate,
project-input count, and project aggregate are explicitly bounded. The format
change makes older project-key entries inert and caused one deliberate cold
migration; it does not change checkpoint products, application execution, or
qualification boundaries. The final change-aware warm database owner takes
81,910 ms, down 3,100 ms or 3.65 percent from 85,010 ms; its portable section
takes 55,340 ms, down 3,070 ms or 5.26 percent from 58,410 ms.

`Tests/Native/Development-Owner-Dependencies.txt` now declares the source,
producer, and artifact closures for the measured front-door, WebAssembly, and
database owners plus all six database checkpoint families. Its verifier
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

Qualification owners follow the same ownership boundary even though they run
cold. An ordinary application, library, or provider owner verifies the exact
retained compiler and lowerer candidates, then reconstructs and exercises its
own product. It does not rebuild compiler tooling unless compiler construction
is the behavior it owns. Dedicated convergence and reconstruction owners retain
that independent evidence, and a historical recovery owner remains bound to its
exact restored commit. Applying this rule to model-provider qualification cut
the measured owner from roughly 24 to 29 minutes to 13 seconds on Linux and 21
seconds on Windows while preserving all 11 cases. Five other ordinary owners
now use the same retained-tool boundary while preserving duplicate compilation,
byte comparison, lowering, linking, cross-target packaging, and current-host
execution of their actual products. The fifth is the durable-database behavior
owner, which no longer rebuilds a lowerer before exercising its 12 cases.

Qualification scheduling now retains the complete paired-host owner timings from
the accepted Language 1.0 run. The 126-owner baseline showed that equal sums of
coarse duration profiles still projected a 6,547,869 ms critical shard. Moving
six independent owners without changing their commands, cases, profiles, or
timeouts produces historical projections of 4,655,707 ms on Windows and
4,521,081 ms on Linux. The planner validates all 252 timing values and reports
declared timeout policy separately from observed scheduling work; a new paired
run is still required before the projection becomes a measurement.

Database target selection now preserves independent unions instead of turning
every multi-closure change into the complete development owner. The current
planner selects three cases for `Local-Database-Put.wv`, four for
`Durable-Tree-Reader.wv`, and 34 for the broadly shared `Durable-Page.wv`.
Publication, recovery, and single-writer commit are now included in the 53-case
development inventory; cold qualification remains a separate 57-case route.
The version-3 development inventory also identifies the six qualification-
validated bundle memberships. A complete pair is one physical development
execution while its two logical cases remain visible; a partial pair stays on
the original one-case project. The all-development plan therefore contains 53
behaviors in 47 executions. On Windows the publication/recovery bundle took
44,850 ms while creating its content-addressed checkpoints and 2,130 ms on an
unchanged warm run with project, link, and application hits. Independent warm
publication and recovery selections took 2,100 and 1,980 ms, confirming that a
partial selection does not consume the combined product.
On the measured Windows host, three portable cases with two fresh products
passed in 77,030 ms, and the portable plus hosted local-service closure passed
its exact three cases in 193,520 ms. The latter plan reports 245 expected
seconds and a 570-second safety bound rather than the qualification owner's
2,700/3,600-second profile.

Cold database qualification now reads one versioned inventory on both hosts.
It preserves 57 logical cases while six compatible pairs share products, so the
current graph contains 54 execution steps, 58 project references, 673 declared
root/source references, and 146 unique source paths. Pairing every construction
would produce 1,346 source visits per host. The 42 ordinary portable steps now
use one admitted construction and delegate 385 duplicate visits to focused
reproducibility owners; their remaining 4.61-fold cross-project manifest overlap
is the next construction target. Portable and hosted steps execute the
current-host image and delegate generic opposite-host packaging to focused
packager owners; paired Windows/Linux database behavior remains mandatory for
complete qualification. Five ordinary hosted construction functions also share
one fresh source-build, lowering, and admission routine per host. The paired
`StorageLowering` case and common host-adapter assembly comparisons retain their
focused reproducibility claims. See the
[hosted construction evidence](../Evidence/2026-09-06-Hosted-Database-Current-Host-Packaging.json)
for the measured improvement and remaining qualification limits.
The first two-case bundle reduced direct clean Windows work from 71,830 to
40,608 ms. Its focused node fell from 46,150 to 38,310 ms after both portable
delegations. Two further bundles preserve the ancestor-groups/depth-four and
ancestor-pages/intermediate case labels; their focused Windows nodes passed in
51,640 and 61,150 ms respectively. A source-overlap planner now ranks
non-identical portable pairs without treating them as safe automatically. Its
first retained trial combines transaction leaf groups and leaf pages, sharing
10 of 23 declared inputs; the focused Windows node passed in 56,140 ms without
raising any compiler or lowerer limit. Its development path took 59,540 ms while
creating checkpoints and 2,990 ms with project, link, and application hits.
The next retained overlap trial combines root split and depth two; its focused
Windows qualification node passed in 59,990 ms, and its development path took
58,470 ms while creating checkpoints and 2,860 ms on the unchanged warm path.

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
