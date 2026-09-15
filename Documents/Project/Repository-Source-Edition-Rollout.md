# Repository source-edition rollout

> Status: Current completed source/package milestone; selected cross-host verification complete
> Authority: Informative; accepted source, project, capability, and bootstrap contracts remain authoritative
> Last reviewed: 2026-09-15

The maintainer approved expanding the package-parser migration to its shared
compiler/tool dependencies, starting with this bounded implementation and
qualification plan. The immediate delivery remains a coherent package migration
that can be committed and pushed to both configured remotes. Repository-wide
source conversion is enabling work, not a claim that Libraries 1.0 is complete.
The maintainer subsequently approved the time needed for this chat. Individual
runs remain bounded and reported; that session approval does not change the
repository's default ten-minute budget for future work.

## What is established

The maintained canonical decimal parser and borrowed package-lock reader work
through selected current products. Their exact tested, still-uncommitted inputs
are in the [lock-reader evidence](../Evidence/2026-09-15-Package-Lock-Borrowed-Content.json).
The shared source conversion and selected cross-host integration verification are
complete. The canonical Option parser and immutable-borrow lock consumer work
through the normal build and package paths; wider Option/Result operations and
installed-toolchain promotion remain separate milestones.
Completed cross-host package and reconstruction results, exact input identity,
resumed checks, and exclusions are in the
[integration evidence](../Evidence/2026-09-15-Source-Edition-Package-Integration.json).

The [existing migration plan](Windvale-Language-1.0-Migration.md) is an
identity-bound source-freeze input. Do not rewrite its historical status or
hash-bound bytes as this rollout advances. Its no-parallel-compiler, explicit
edition, semantic-review, and removal-checkpoint rules still apply. Current
compiler qualification is owned by the
[Slice 8 qualification decision](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md),
not the older plan's pre-implementation wording.

## Audit boundary, not a blind rewrite list

The 15 September audit started from
`Libraries/Package/Canonical-Package-Text.wv`, read the tracked `.wvproj` files
under `Projects/`, and repeatedly added every project declaring a selected source
and every source declared by those projects until no new entry appeared.
This is a conservative declared-inventory connection, not semantic import
resolution or proof that every listed file must change.

Of 626 tracked project manifests, that connection contains 394 projects and
593 sources. Before conversion, 569 had legacy module headers and 24 had source
descriptors. The legacy headers comprised 419 portable and 150 hosted modules. SHA-256 is
declared by 28 projects; the shared WVB semantic verifier by 14. Those shared
dependencies connect package work to compiler, lowerer, and publisher builds.

| Source owner | Connected sources | Legacy headers before conversion |
| --- | ---: | ---: |
| Compiler | 117 | 117 |
| Libraries | 96 | 85 |
| Linker | 88 | 88 |
| Runtime | 37 | 37 |
| Tools | 31 | 31 |
| Foundation | 8 | 8 |
| Applications / Examples | 26 | 26 |
| Assembler / Object-Model | 5 | 5 |
| Operating-System | 5 | 5 |
| Tests | 180 | 167 |

There are another 684 tracked `.wv` files outside this connected set, including
72 under `Documents/`. Audit their actual owners and generation paths separately.
The counts exclude the two existing untracked Project 4 manifest-test files and
do not count source text embedded in scripts or generated at test time.
Recompute the inventory after project changes; do not make these counts a
permanent golden test.

All 394 connected manifests now select Project 4 and all 593 sources declare
edition 1. Complete Windows and Debian native admission scans accept that set. This
checks source edition, profile, platform, authority, capabilities, and authenticated
source inventory; it does not claim execution of every application or test.
The conversion also makes implicit enum backing explicitly `i32` and changes
nine legacy inspector `void` return annotations to `unit`. The runner's private
diagnostic helper instead returns its existing failure exit code directly to
all three callers: this keeps the native runner within the supported lowering
subset without introducing a dummy enum or widening backend acceptance.
The sole consuming project is rechecked after that repair. Deliberate legacy
strings and frozen sources remain unchanged.

Every source must be classified before conversion: maintained implementation,
positive fixture, deliberate rejection fixture, generated source, or immutable
historical/frozen input. Record its project consumers, canonical module identity,
profile, actual platforms, authority, capabilities, producer, and verification
owner. Preserve frozen corpora. Migrate generators with their current outputs;
do not rewrite strings inside malformed-input tests blindly.

## Ordered implementation and exit gates

| Phase | Bounded work | Exit gate |
| --- | --- | --- |
| 1. Inventory and bootstrap audit | Establish the connected set, exceptions, source producers, and current construction chain. | Completed for the 394-project connection; outside sources remain explicitly separate. |
| 2. Bootstrap-safe build plumbing | Bind profile/lock/target contents into project cache identity; make current compiler construction and convergence consume explicit authenticated Project 4 inputs without recursively asking the product being built to construct itself. | Focused cache/admission rejection tests pass; a recorded predecessor can construct the first migrated compiler with no warm-cache dependency. |
| 3. Source conversion | Convert reviewed closures in dependency order: Foundation and shared verifiers, compiler/admission/lowering/publishing, package/library consumers, applications, and remaining runtime/OS/test producers. | All participating project inventories select explicit inputs and targets; no accidental mixed-edition closure or undeclared capability remains. Intermediate connected edits stay uncommitted until coherent. |
| 4. Package workflow integration | Select the current lowerer in the existing package owners; preserve native output publication, exact failure progress, and finite runtime profiles. | Maintained parser, lock, manifest, consistency, resource admission, generation, bundle, and installation consumers build and run without ad hoc source rewriting or manual image assembly. |
| 5. Reconstruction and host qualification | Reconstruct the selected migrated compiler through two generations; execute causal package and compiler owners on Windows and Debian with independently constructed host products. | Same source, target, options, and producer generation yield identical WVB; required host behavior and rejection cases pass. Record measured time and memory where practical. |
| 6. Coherent commit and delivery | Review generated identities and dependency closure, reuse unchanged verification evidence, sign off the task-scoped commit, and push to both configured remotes. | Both remote branch tips contain the same commit; no unrelated work or local caches enter Git. Report CI separately from local qualification. |

These phases are dependencies, not estimates of equal effort. Do not report a
repository-wide completion percentage from the initial file count.

## Bootstrap safety comes before shared-header conversion

`Current-Split-Compiler-Cache-Core.mjs` constructs analyzer/emitter
products through `Build-Cached-Split-Project-Wvb.mjs`. Its explicit
Project 4 mode delegates to the existing authenticated coordinator and binds
all supplied predecessor executables into the cache key. The construction graph
now has an edition-1 branch and includes the four admission products in its
version-2 checkpoint. The shared compiler manifests are converted. Recorded-source
predecessor construction has completed on both hosts; migrated construction and
two-generation convergence are separate measured checks, not inferred from a
warm cache hit.

The audit found that the generic project key hashed manifest and source bytes,
but not the contents of declared source locks, profiles, or target descriptors.
The cache fix now includes those bytes in identity and unchanged-input
checks. Twenty-one focused cases pass independently on Windows and Debian,
including source-only key compatibility. See the
[cache-input evidence](../Evidence/2026-09-15-Source-Edition-Cache-Inputs.json).
A cache key is not admission: the native admission/authentication boundary must
still reject an incorrect lock digest, unsupported profile, or target mismatch.

Keep the exact predecessor compiler/admission/publisher inputs reconstructible
from their recorded source state and pinned artifacts. The first migrated build
must invoke those explicit predecessor products through the existing authenticated
split boundary, not recurse through `Build-Wvb-Project4.mjs` while rebuilding its
own prerequisites. Then reconstruct with the migrated products and compare.
Do not overwrite bootstrap pins, silently fall back, depend on a user's cache,
or restore managed Stage 0 to `main`. A changed bootstrap trust policy requires
its own decision; ordinary source conversion does not authorize it.

## Mechanical changes versus semantic review

Headers, explicit enum base types, and project directives may be mechanically
edited only after their exact replacement metadata is classified. Preserve
canonical names and serialized values. Do not assign every module all platforms
or infer authority from its directory alone. Hosted capability versions and
bindings must come from their existing accepted contracts.

Review constructors, `void`/`unit`, effects, mutation, overflow, and diagnostics
under the actual supported edition-1 compiler. A header change is not proof of
semantic equivalence. Do not broaden this batch into replacing every byte buffer,
status record, collection, resource API, or concurrency pattern. Record required
language-admission repairs separately from later library API work.

Keep one SHA-256 implementation and one WVB verifier. Do not strip descriptors,
copy shared sources into edition-specific libraries, introduce mixed-profile
guessing, relax borrow limits, or change verification acceptance to pass the batch.
Do not remove Seed parsing until the existing migration removal gate is satisfied;
the package milestone alone cannot satisfy it.

## Verification plan and time bounds

Inspect `pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -PlanOnly` before
each coherent changed-state selection. Use the existing owners, adding focused
cases and selectors where a distinct changed contract requires them.

- Cache changes: mutate each declared profile/lock/target independently; require
  a changed key and stale-request rejection. Include missing, malformed,
  duplicate, escaping, and linked inputs, and preserve source-only identity.
- Build plumbing: authenticated source/profile/target admission, wrong-lock
  rejection, no publication on rejection, deterministic output, and cold
  predecessor construction. Existing production-admission and split-compiler
  owners remain responsible; do not add a second coordinator.
- Source closures: use the narrow owner for each changed contract, including
  SHA-256 known answers and malformed WVB cases when those sources change.
- Package integration: select `package-format`, `package-bundle`,
  `offline-generation-lifecycle`, `installation-command-resolution`, and
  `offline-package-stage` only for their actual changed boundaries. Preserve
  prior passing results when their complete inputs are unchanged.
- Final bootstrap: adapt and use the existing split-compiler convergence gate
  for migrated source. Compare identical target descriptors across hosts;
  Windows-target and Linux-target artifacts are not assumed byte-identical.
  Independent host construction is distinct from running a Windows-built ELF
  on Debian. OS changes also follow `Operating-System/AGENTS.md`.

The full source-admission scan took 567 seconds on Windows and 817 seconds on
Debian under WSL2. These measure source admission, not complete compilation or
execution of every project. Compiler preparation, native packaging, and fresh
two-generation reconstruction are separate bounded operations. The maintainer's
time approval covers this session; future longer runs still require the normal
advance command/budget approval. Reuse exact completed caches and preserve
phase artifacts when a later check fails.

The generic owner-duration labels are planning hints, not permission to exceed
the total ten-minute local budget. In particular, the pending package plan's
660-second estimate already exceeded it. Keep construction evidence and caches
on timeout and report incomplete results honestly.

## Current checkpoint

The 394-project / 593-source connection is converted and passes complete Windows/Debian
native source admission. The explicit-predecessor Project 4 cache has thirteen
passing native cases on each host; its expanded cache/construction sentinel has
thirty-nine passing cases on each host. The current analyzer, emitter, and four
admission WVBs match across Windows and Debian. Windows fresh-generation builds
produce identical analyzer and emitter bytes. Profile 2 exhausted its runtime
budget on the large products; separately resumed Profile 7 verification accepts
both unchanged second-generation products and rejects malformed input. This is
combined Windows evidence, not a claim that the first gate invocation passed.
The complete Linux two-generation gate passes in 1,478 seconds with identical
portable WVB identities and independent host-native compiler products.

Package-format execution passes 82 groups on each host. Native borrow/pointer lowering
passes 43 cases, command resolution passes eight cases, and generation lifecycle
composition passes 27 cases on both hosts. Bundle, offline-stage, and
command-dispatch checks pass twelve, eight, and nine cases respectively on each
host. The ordinary Project 4 launcher passes nine publication/replacement and
rejection-preservation cases on each host.
Historical application-package reproduction passes
eleven cases on each host, and the shipped browser compiler reproduces its exact
locked bytes on each host from its recorded historical sources. These historical
commands do not build current source or promote new distribution artifacts.

`Build-Wvdb-Query-Package` reproduces its released lock from explicit recorded
sources through the pinned native compiler. `Tools/WebAssembly/Build-Compiler-Wvb.mjs`
likewise reproduces the shipped browser compiler from its manifest's historical
source commit, including the root-era Project 1 inventory. Both require the
recorded Git objects locally and exact output identity before native publication.
Use ordinary Project 4 builds for current sources; these reproduction commands
do not silently update their distribution locks.

Large bundle tools exceed the single-buffer lowerer's existing 4 MiB bound and
therefore use the current segmented staging path. That path, selected cross-host
package integration, and compiler verification have passed. The full production
admission owner also passes all forty cases on each host: twenty-four ingress
cases and sixteen Project 4 cases, using the repaired current runner.
The normal Project 4 front door now reuses the authenticated emission cache and
retains native final publication plus unchanged-input checks.

No Seed removal, installed promotion, full Libraries 1.0 completion, full
repository qualification, or release qualification is claimed by this checkpoint.
