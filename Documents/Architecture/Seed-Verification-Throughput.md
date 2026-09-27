# Seed verification throughput

> Status: Current post-retirement verification architecture
> Authority: Informative
> Last reviewed: 2026-09-26

The [agent handbook](../../AGENTS.md#testing-and-verification) and
[verifier guidance](../../Tools/Verify/AGENTS.md) own current execution policy.
This page explains ownership, caching and qualification boundaries. Completed
optimization reports and their dated inventories are retained in
[Git history](../Git-History.md#superseded-browser-and-verification-notes); they are not current timing guarantees.

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
- Development scope runs affected native owners on Linux and adds Windows for
  Windows command, PowerShell, platform or binary changes. Mixed website changes
  retain website verification alongside native owners. These jobs do not create
  a conformance or qualification claim.
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

The current split-compiler coordinator now acquires the complete analyzer/emitter
pair before building requested projects. Its key binds both complete compiler
source closures and every construction producer; its two applications and two
identities are validated together. A hit needs no pinned or intermediate
construction. The
[split-cache contract](../../Specifications/Compiler-Split-Development-Cache.md#reusable-current-compiler-pair)
defines bounds, invalidation and failure handling; cache correctness cases run
inside the existing split-development owner on both hosts.

Content-addressed checkpoints may accelerate deterministic construction during
development. A valid key includes all input digests, producing tool identity,
target/profile, relevant options, and the checkpoint format. Every hit must
revalidate the manifest, output size, digest, and required structural admission.

Checkpoints cache immutable products, not passing behavior. The owner reruns the
execution, recovery, denial, mutation, or other behavior affected by the edit.
Qualification retains explicitly required independent constructions; a
checkpoint hit alone proves neither behavior nor reproducibility.

Acquire construction dependencies only when the requested product is missing.
The [split compiler cache](../../Specifications/Compiler-Split-Development-Cache.md)
derives both current request keys and validates its final WVB first. A valid
final checkpoint needs no retained analysis or symbol files; eviction must not
restart those phases. Corrupt final evidence fails before reconstruction.

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

Hosted producer contexts retain 40-byte fingerprints: an eight-byte little-endian
size followed by SHA-256, rather than complete producer files. All 72 toolset
entries are still read and checked against their inventory, in batches of at
most four; field order follows the inventory regardless of completion order.
Version-2 hosted application keys also bind the cache-key implementation and
Node runtime version. The shared reader allocates only the admitted file size
plus one byte, rejects a changed opened file identity or length, and closes its
handle on every path. The focused cache owner includes the existing hosted-session
checks, so session and key-tool edits select that owner instead of all database
behavior. A 4 KiB producer-payload bound protects the retained context in its
focused test. See the
[producer-context evidence](../Evidence/2026-09-06-Hosted-Producer-Fingerprints.json)
for timing, memory scope, and the unchanged package comparisons.

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

WebAssembly and compiler convergence remain separate independent qualification
jobs. Current owner and case counts come from the
[owner registry](../../Tests/Native/Verification-Owners.txt), not an old timing
snapshot.

Sharding reduces wall-clock time, not total evidence or necessarily total hosted
compute. Rebalance only from repeated dual-host measurements.

## Archived managed evidence

The former managed Fast, Development, Standard, and Qualification harness is
preserved only in `stage0-recovery-e5a1a7473c57`. It is absent from `main` and
therefore cannot become an accidental fallback or another step in the ordinary
verification ladder. A named recovery, security, or historical differential
investigation restores that exact release in a separate workspace.

## Next measured optimizations

Current development work uses exact affected-owner selection and declared
checkpoint dependencies. The [throughput plan](../Project/Verification-Throughput-Plan.md)
owns remaining priorities. Keep these boundaries while optimizing:

- Behavior owners validate retained tool identities and construct their own
  products; dedicated reconstruction owners own compiler reconstruction.
- Database and library selectors preserve affected dependency unions and
  independently owned cases. Do not widen to unrelated regression suites.
- Construction may be shared only where the declared product and evidence
  contracts agree. Preserve distinct logical cases and failure attribution.
- Cache hits preserve malformed-input, publication, host execution, restart,
  denial and cleanup checks owned by the changed boundary.
- Qualification scheduling uses measured per-host costs. Projected shard
  improvements remain projections until a paired run measures them.

The detailed before/after timing diary is in [Git history](../Git-History.md#superseded-browser-and-verification-notes).
The remaining optimization directions are:

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
