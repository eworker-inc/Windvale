# Windvale split-compiler development cache

## Status and scope

`Build-Cached-Split-Project-Wvb` is a development-only coordinator for the
independent analyzer and emitter products specified by
[the source-analysis phase artifact](Compiler-Source-Analysis.md). It admits
one canonical Project 2 manifest, one private `.wvb` output, one analyzer plus
identity, and one emitter plus identity. Its explicit `--authenticated-project4`
mode additionally requires an admitter, authenticator, manifest reader, and
foreign binder. It delegates Project 4 parsing and authenticated compilation to
`Run-Split-Compiler.mjs`; it does not reconstruct these supplied producers or
guess an edition. Project 3 and optimization options remain outside this route.
The analyzer target is `source-analysis-v1`; the emitter and
published product target are `portable-wvb-optimized-v1`.

This cache is not a release, qualification, or cross-host conformance boundary.
Split output must remain byte-identical to the one-shot compiler's normal
optimized output for the same accepted Project 2 input. The one-shot
`--complete` mode remains the explicit diagnostic and differential oracle; it
is not a hidden split-cache option.

## Prepared-product execution

### Compiler selection and explicit preparation

`Build-Current-Split-Project-Wvb.mjs` and `Build-Wvb-Project4.mjs` require
an existing validated compiler checkpoint for ordinary project builds. Neither
reconstructs compiler tools on a miss. The default selects the key derived from
current compiler sources and construction inputs; a miss exits `64` with
preparation instructions before invoking a compiler product.

An explicit `WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT=<sha256-key>` may select a
prepared profile-8 hosted image of the preserved complete verifier when its
pinned host image cannot finish a larger compiler input within its execution
limits. The selected checkpoint must belong to this host's
`segmented-hosted-wvb-v1` family and bind the exact immutable
`Artifacts/Native-Front-Door/Wvb/Compiler-Wvb-Verifier.wvb`. Preparation first
packages that WVB with profile 8 while this variable is absent, then records
and selects the returned key. There is no automatic fallback or reconstruction
on a missing or invalid selection. Without this variable the pinned verifier
remains selected.

The compiler and hosted-product construction keys bind the selected checkpoint,
semantic input, executable size and digest. Complete verification still runs
before new hosted products are published, with the same exact successful report,
bounded deadline, and input/producer rechecks. Selecting a larger host profile
does not extend the preserved verifier's supported WVB versions; current-format
consumers require their current complete verifier. This development selection
does not establish current compiler qualification.

For selected compiler-construction owners, `Verify-Changed.ps1
-PreparationOnly` prepares or admits this profile-8 product within the shared
absolute preparation deadline. It atomically records the full identity in
`Bootstrap-Complete-Verifier-Selection.<windows-x64|linux-x64>.json` at the native
cache root. The descriptor is canonical UTF-8 JSON with LF, bounded to 4,096
bytes, and carries format `windvale-bootstrap-complete-verifier-selection-1`.
Its identity contains the checkpoint key, host, profile, input size and digest,
and executable size and digest.

The corresponding `-UsePreparedProducts` execution admits that descriptor and
its actual product before deriving compiler or verification-result cache keys.
An explicit environment selection takes precedence. A missing, corrupted or
wrong-host descriptor refuses execution without preparing a replacement.
Verification-result identity rechecks the selected executable on each source
state measurement; an unchanged key cannot preserve evidence after a product
changes. These records are machine-local preparation state and are not tracked
qualification evidence.

Both commands accept `--compiler-checkpoint <sha256-key>` to deliberately use
one existing checkpoint in the current host's `current-split-compiler-v2`
family. The Project 4 publication command requires this option before the
project/output pair. The key is exactly 64 lowercase hexadecimal characters;
there is no newest, closest, or automatic fallback selection. The existing
bounded checkpoint inventory, host, identity records and executable digests
are validated and rechecked before accepting completion (before transactional
publication in the Project 4 front door). No new cache format is introduced.

Explicit selection does not derive its identity from the current source tree.
It reports `selection=explicit-checkpoint construction=disabled` and proves
use of the selected products, not verification of newer compiler source or
release qualification. Project admission, authentication and source/product
cache identities still bind the actual selected producers. Source edits must
invalidate their affected analysis and output products.

Compiler construction belongs to the existing development builder's explicit
`--prepare-only --deadline-ms <absolute-unix-ms>` mode. Construction owners that
also build target projects may instead select `--prepare-compiler` with the
same mandatory deadline. The latter conflicts with prepared-only restrictions,
`--prepare-only`, and explicit checkpoint selection. Ordinary development
builds default to a total ten-minute deadline, including cleanup, unless a
deadline is supplied. The existing `--prepared-compiler-only` flag remains
accepted. Publisher preparation is still a separate remaining cost in the
transactional Project 4 front door; selecting a compiler does not imply a
prepared publisher.

### Bounded construction diagnostics

The current split-project CLI preserves the original failure exit status and
the existing 1 MiB UTF-8 diagnostic limit. When available, it reports error code
and syscall fields of at most 64 characters and a path of at most 1,024 characters,
with line breaks and tabs replaced by spaces. It includes at most 8 stack frames
of 256 characters each, selected from the first 16,384 stack characters. Aggregate
causes and cleanup failures remain bounded by the existing 32-value traversal.
This adds diagnostic context without retrying or preparing a failed operation.

### Prerequisite check

The `--preflight-only` option checks prerequisites and returns before work-directory
creation, full current-key derivation, prepared-product admission or construction.
Ordinary builds run the same preliminary phase and then retain all compiler and
product admission checks. Readiness does not establish source semantics or
qualification.

The phase uses the existing project-cache reader for declared input identities
and the existing artifact generator's `--check` for generated readers. Preparation
checks all six current tools; a target checks when its admitted closure contains
the canonical generated marker or a maintained
`Compiler/Windvale/*-Artifact*.wv` input. A missing generated marker therefore
still triggers the check. No second source parser or generator catalog is added.

The phase targets 30 seconds with a 60-second ceiling inside the inherited work
deadline. Existing complete source hashing and filesystem operations can exceed
the target; the outer workflow owns the hard process deadline. Node and Git
versions, canonical filesystem parents, and free bytes are prerequisite
observations. Temporary and output paths require positive space and one-byte
write/cleanup checks because they materialize files even in prepared mode. A
prepared-only cache requires an ordinary readable path, permits zero free space,
and receives no write probe. A cache that may publish requires positive space and
writable checks. Failed required writes, cleanup or tool checks refuse.
The 2 GiB scratch and 1 GiB cache alerts are advisory cold-profile warnings, not
space reservations or justified hard thresholds; warm reuse remains allowed
below them. Readiness does not infer WVIR headroom or process-memory safety.

The existing cache-test `--construction-readiness` selection runs 23 focused
readiness controls with mocked compiler/child dependencies and the unchanged 13
construction CLI process actions. Its existing process group also checks exact
package deadline selection and inherited clipping. This selection does not run
the memory integration owner or the broader cache suite. Actual generated-source
analysis and independent WIR/WVB verification still follow the coherent compiler
build.

### Reuse of all prepared products

`WINDVALE_PREPARED_PRODUCTS_ONLY=1` selects reuse without construction in
`Build-Current-Split-Project-Wvb.mjs`, `Build-Cached-Split-Project-Wvb.mjs` and
`Build-Cached-Segmented-Hosted-Wvb.mjs`. Other defined values are rejected.
The current compiler, final project WVB and final hosted application must each
have an exact valid checkpoint. A missing checkpoint exits with status 64 and
preparation instructions before launching a producer or allocating a candidate.
An invalid existing checkpoint fails closed. Hits retain complete validation,
input-identity checks and private output materialization; missing intermediate
analysis or image checkpoints do not require reconstruction when the final
product is valid. A miss leaves previous owner output and completed cache entries
unchanged. Without this variable, requested project and application products
may be constructed; compiler construction still requires explicit preparation.

Preparation runs the same builders without this variable in a separately
declared budget. This mode does not freeze unrelated repository tools or replace
the selected owner's fresh behavior execution. It is a prerequisite for CI phase
separation; merely enabling it does not establish complete CI coverage.

## Producer identity

`Write-Split-Compiler-Producer-Identity.mjs` hashes a bounded ordinary analyzer
or emitter executable once at packaging and publishes this exact six-line ASCII
identity for the current `win32-x64` or `linux-x64` host:

```text
windvale-split-compiler-producer 2
role <analyzer-or-emitter>
target <source-analysis-v1-or-portable-wvb-optimized-v1>
host <host-family>
bytes <canonical-positive-decimal>
sha256 <64-lowercase-hex>
```

The analyzer identity requires `source-analysis-v1`; the emitter identity
requires `portable-wvb-optimized-v1`. An existing identity may be reused only
when every byte is equal. A different producer requires a new identity path or
explicit removal by its packaging owner; the writer does not overwrite an
identity in place.

## Keys and checkpoints

The analysis cache uses namespace `project-analysis-wvca-v3` and binds the
workspace marker, analyzer-identity bytes, project identity and bytes, and the
ordered root/source closure through the shared length-framed Project 2 key. Its
checkpoint records exact size and SHA-256 evidence for WVSS, the fixed 104-byte
WVCA, WVLB, and WVIR.

The coordinator validates the strict Project 2 directives independently of the
key builder, places the declared root first, reads every bounded source snapshot,
and sorts dependencies by the ordinal UTF-8 bytes of their declared module
identities before invoking the analyzer. A filename such as `*-Main.wv` is not
used as a proxy for that identity. Therefore Project 2's semantically irrelevant
directive order never changes source-module meaning or successful WVB bytes. The
version-3 family prevents a pre-fix path-ordered checkpoint from being reused.

On a miss, the coordinator passes those root-first canonical source paths to
the retained descriptorless Project 2 Analyzer route. The Analyzer owns its
bounded reads and publishes WVSS 1 beside WVCA, WVLB, and WVIR. The coordinator
does not construct an "admitted" WVSS, invoke the removed public
`--admitted-source-set` option, or enter the private authenticated Language 1.0
sequence. Project 2 remains development-only and its Analyzer precheck rejects a
`System` profile plus every platform or foreign declaration before analysis.

The optional private `--symbol-checkpoint` route divides that same miss into
two analyzer invocations. The first publishes the canonical WVSS plus a bounded
`WVSY 1.0` symbol checkpoint into the separate
`project-symbols-wvsy-v1` cache family. Its manifest binds the complete analysis
request key and exact size and SHA-256 evidence for both values. The second
copies those validated values into its private candidate, independently admits
the checkpoint against the unchanged WVSS, revalidates its directory, lookup,
visibility, and all aggregate counts, and publishes WVCA, WVLB, and WVIR. The
coordinator compares both copied values before and after consumption and removes
the private copies before final atomic cache publication. A retry can therefore
reuse completed symbol work after a later-phase interruption without admitting
stale source, producer, project, or dependency state. Final analysis cache hits
still consume only the same validated WVSS/WVCA/WVLB/WVIR products; `WVSY` is
internal resumable evidence, not a distributable compiler format or additional
compiler.

The emission cache uses namespace `project-split-wvb-optimized-v3`, binds both
producer identities and the same closure, and records the exact analysis key
beside the WVB size and hash. The fixed optimized target therefore participates
in both the producer identity and cache-family name. Both cache families are
additionally separated by the current host family.

## Hit, miss, and publication behavior

A finished-product hit validates the WVB record, its exact analysis request key,
and its bounded product bytes before consulting intermediate analysis or symbol
checkpoints. It does not read, validate, or reconstruct those intermediate
products; they may have been evicted independently. It still derives both keys
from the current complete source closure and producer identities. Invalid final
records or product bytes fail closed without falling back to reconstruction.
Only a Project 2 final-product miss acquires analysis, ordering the bounded source modules
and validating or constructing its phase products as needed. A hit does not
read or execute either large compiler product in Project 2 mode.
On a miss, the coordinator hashes the selected executable against its identity
both immediately before and after execution, rechecks the complete key input
set, syncs the candidate files, and atomically publishes the directory.
Analyzer and emitter launches report `Started`, one bounded `Active` heartbeat
every 30 seconds, and `Complete`, so a long producer is distinguishable from a
stalled coordinator without changing machine-significant result lines.

A race loser accepts only a completely valid canonical checkpoint. A `finally`
boundary removes only the exact locally allocated `.new-<key>-*` directory
after a producer, measurement, manifest, or lost-race failure, after proving
that the candidate remains a direct child of the selected family. A successful
rename clears the temporary path and is preserved.

## Authenticated Project 4 construction

The `project-authenticated-split-wvb-v1` family is separate from descriptorless
analysis and emission caches. It binds all declared source, lock, profile and
target bytes, the workspace and project, analyzer/emitter identity records,
the loaded coordinator implementations, host, and role-ordered executable
content identities for Node and all six supplied native producers. Each producer
is a nonempty ordinary file of at most 128 MiB, hashed with a 1 MiB streaming
buffer. There are exactly seven executable inputs; content hashes, not mutable
cache locations, identify the predecessor products.

A miss invokes the existing authenticated coordinator with its native manifest
reader and explicit workspace. The resulting WVB is a private cache candidate,
not application output publication. Its checkpoint uses the existing emission
record shape, with the complete authenticated request key also occupying the
analysis-key field.
The outer sequence has a fifteen-minute bound; individual native phases retain
the existing five-minute ceiling. Authenticated Project 4 accepts an optional
`--source-emission-deadline-ms <absolute-unix-ms>` for source emission alone.
It must be a future safe integer at most 600,000 milliseconds after coordinator
start; duplicate, malformed, expired and non-Project-4 use rejects before any
producer starts. The phase uses only the remaining absolute interval. Its
success is rechecked against that same deadline, and the lower-only test timeout
still shortens it. Other phases and commands without the option keep five
minutes. Diagnostics remain bounded to 64 KiB.

After fresh admission and authentication, `Run-Split-Compiler.mjs` can reuse
an `authenticated-analysis-v1` checkpoint. Its key binds the six exact admitted
source-set, target, catalog, evidence, lock and profile byte sequences, analyzer
content identity, normal or Foreign analysis mode, host, Node identity, and
loaded coordinator/cache/reader implementations. Emitter identity is excluded
from this phase key because emission occurs after analysis. Binding and pairing
still execute after analysis for a Foreign catalog. Authentication is never
replaced by an analysis-cache hit.

The checkpoint has exactly four analysis products and the bounded successful
phase report, plus its digest record. Each phase value is at most 4 MiB; the
manifest is exactly 104 bytes and the report is at most 64 KiB. The analyzed
source must equal the admitted source before checkpoint publication. Temporary
publication is atomic, concurrent results must agree, and hits copy validated
bytes into newly owned private files. Corruption, unexpected inventory, links,
oversized input and changed producers fail closed. An emission failure keeps
completed analysis available. Prepared-product mode permits hits and rejects
misses before analysis. This cache owns no admission or emission result.

Both hits and misses remeasure all producer bytes and project inputs before
copying a completed WVB to the caller's private output. Wrong digests, changed
profiles or targets, invalid identities and failed admission cannot reuse an
earlier accepted key. Corrupt checkpoints fail closed. The final application
destination still belongs to the native transactional publisher in the ordinary
Project 4 launcher; this development cache does not replace that boundary.

The ordinary `Build-Wvb-Project4.mjs` front door uses this authenticated cache
for both the requested project and its current publisher. Before invoking the
native publisher, it rechecks the compiler checkpoint identity, its own loaded
coordinator bytes, and both complete project requests. A cache hit therefore
avoids compilation, not final native admission or transactional publication.

## Qualified two-file bootstrap

The Language 1.0 front door retains the qualified current 1,552,090-byte
analyzer and 1,556,434-byte target-aware emitter under
`Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/`. Their manifest binds
the Decision 0896 promotion, current Project 2 roots, source identities, sizes,
and digests. The descriptorless construction branch packages that pair with
normal role-specific version-2 identities. Edition-1 construction first uses
the recorded-source predecessor described below; it does not feed modern
sources directly to a descriptorless bootstrap route.

The checkpoint is not another compiler source tree, native executable, release
artifact, source-admission claim, or fallback compiler. Both promoted products
consume the current compact WVIR format. The former Decision 0813 pair and
Decision 0846 bridge remain historical provenance only; keeping them in the
active inventory would preserve a transition that the current pair no longer
needs. The gate also omits the former monolithic compiler-source-set build
because its result was never consumed; compiler-scale evidence comes from the
current split analyzer/emitter reconstruction and fixed point.

`Build-Current-Split-Project-Wvb.mjs` accepts one through eight ordered
`<project.wvproj> <output.wvb>` pairs. Its explicit preparation mode packages
and identifies the pinned pair and reconstructs the current analyzer/emitter
pair once. Ordinary builds require the prepared set and build every requested
target through that same validated selected identity.
The current Analyzer WVB is packaged once under Profile 7 for ordinary target
analysis and once under Profile 8 for the larger artifact-reader emitter
closure. Both packages contain the same WVB and have separate executable
identities. Descriptorless emitter and requested-target analysis use the internal
symbol checkpoint route. Edition-1 projects instead use the explicit authenticated
Project 4 route, without a separately reusable symbol checkpoint.
Output paths must be distinct bounded `.wvb` targets with existing canonical
parents. A single pair retains the original result line; a multi-project run
reports each product's size and digest plus one aggregate completion line. This
bounded batching is a verification-time optimization only: it changes neither
the per-project split-cache key nor the resulting bytes.

The focused development owner validates the adapter's fixed optimized route,
requires the exact 308-byte reachable pruning oracle and its exact 395-byte
complete counterpart, and executes the existing cache sentinel. The sentinel
proves module ordering, identity publication, failure cleanup, replacement and
quarantine race safety, primary-plus-cleanup diagnostics, the root-first raw
Project 2 argument and WVSS/WVCA/WVLB/WVIR output order, resumable WVSY reuse
after a later analysis failure, fail-closed WVSY corruption handling, finished
product reuse after intermediate eviction, final product and analysis-key
corruption rejection before construction, producer-change invalidation, two-branch construction ordering, peer completion
after either branch fails, aggregate errors, admission-product corruption,
Project 2 and explicit-predecessor Project 4 construction graphs, and rejection
before publication. It also covers exact current-product acquisition for the
Foundation borrow owner, joined packaging failures, changed input/product
rejection, and deadline admission and exit-status propagation. The native
`--project4` selector adds thirteen cases for
authenticated construction, independent-cache determinism, hits, lock/profile/
target invalidation, explicit producers, and preservation of existing output
on rejection, including preservation of hosted versioned metadata. It
deliberately does not rebuild three large compiler products already covered by
the Language 1.0 front door.
Compiler analysis/emission core changes select that broader semantic gate once;
split adapter, identity, and cache changes select this focused owner. Full
storage, OS, complete native, and paired-host qualification remain final
integration gates unless this boundary changes them directly.

## Reusable current compiler pair

`Build-Current-Split-Project-Wvb.mjs` acquires the current analyzer and emitter
and four admission products as one development construction product before
building requested projects. A validated set skips pinned compiler packaging
and all intermediate construction. The Project 2 path uses twenty preparation
steps and publishes only the completed set; existing intermediate caches remain independently
reusable. This does not make a genuinely cold compiler preparation fit the local
development budget or turn cached construction into qualification evidence.

The construction core first completes the pinned pair, current Analyzer WVB,
and its Profile 8 package and identity. It then runs exactly two independent
branches: Profile 7 Analyzer packaging/identity, and Emitter build/package/identity.
The Profile 8 package publishes the shared native image before either branch
starts, so parallel execution does not duplicate native staging. Profile-specific
memory, instruction, file-count, and output limits remain unchanged. Both branches
settle before final-product copying or temporary cleanup; failures prevent pair
publication and retain each branch error. The coordinator uses the existing
bounded process-tree command runner and a 1 MiB combined diagnostic bound.
Ordinary target builds and non-package commands retain a ten-minute command
ceiling. Only cold compiler construction callbacks under explicit
`--prepare-only` or `--prepare-compiler` use a 1,050-second ceiling for the exact
`Package-Segmented-Compiler-Wvb` and
`Build-Cached-Segmented-Hosted-Wvb.mjs` package commands. This ceiling allows
the existing 120-second complete verification, shared 900-second image/container
construction deadline and 30 seconds for settlement. It does not renew those
internal deadlines or retry a failure. Ordinary target builds following
`--prepare-compiler` retain the ten-minute ceiling.
The `--deadline-ms` argument supplies
an absolute Unix-millisecond deadline across the whole construction request;
individual commands use the earlier of that deadline and their existing ceiling.
The builder reserves thirty seconds for its cleanup, and predecessor construction
reserves a further thirty seconds for exact-path Git worktree removal. Invalid
or expired deadlines reject before work-directory creation. Construction requires
this option; ordinary builds default to a ten-minute total deadline. Timeout status `124` is retained
through command, construction, and caller failures rather than becoming an
assertion failure. An enclosing owner may impose an earlier total deadline.
Each progress line retains its assigned step number during overlapping work.

On Linux, cancellation freezes each still-owned ancestor before enumerating its
threads' children, so detached descendant process groups are included. Inspection
is bounded to 256 processes, 4,096 enumerated thread entries, 4 KiB stat records,
64 KiB child lists, and three seconds. Exact PID/start-time identities are
rechecked before termination; collected processes are killed leaves first and
must be gone or zombies before cleanup is considered confirmed. Pipe settlement
has a separate five-second bound. Unconfirmed termination reports framework
failure `2` and preserves affected work for diagnosis; it never establishes
passing evidence. This is lifecycle control for trusted build wrappers that keep
their child ancestry alive, not containment of arbitrary programs that orphan
themselves before observation. Windows retains its process-tree termination path.

The `current-split-compiler-v2` family is separated by `win32-x64` or
`linux-x64`. Its key binds the workspace marker, both compiler and all four
admission project manifests and their complete declared input closures,
pinned analyzer/emitter WVB bytes,
the coordinator, command lifecycle and split-cache implementations, source ordering, producer
identity writing, the source-edition predecessor constructor, all segmented staging/linking/transport/admission producers,
the direct-condition Analyzer intermediate owner,
the hosted packager's complete producer context, and the Node version and
executable identity. Requested test projects are separate downstream products.
Every input is remeasured before accepting a hit and before publishing a miss.

Each checkpoint contains `Analyzer`, `Emitter`, `Reader`, `Admitter`,
`Authenticator`, and `Binder` executables (`.exe` on Windows, `.elf` on Linux),
the analyzer/emitter `.identity` files, and
`Checkpoint.json`. Each executable is nonempty and at most 64 MiB; each identity
is at most 1 KiB and must exactly describe its corresponding executable and
role. The record is at most 4 KiB of canonical UTF-8 JSON with a final LF:
`format`, `key`, `host`, and eight ordered `products` containing `name`, `bytes`,
and `sha256`. The format is `windvale-current-split-compiler-checkpoint-2`.
The implementation compares canonical record bytes rather than accepting extra
fields or alternate serialization.

Paths must be canonical ordinary files/directories without links; Linux
executables must retain an execute permission bit. Directory
inventory is bounded before reading product bytes. Missing, additional,
corrupt, oversized, wrong-role, or wrong-host products fail before use; a corrupt
hit never silently starts reconstruction. Hashing uses 1 MiB stream buffers.
Production uses a private sibling directory and atomic directory publication.
Concurrent writers must produce identical complete records. Failure cleanup
removes only its own private candidate and retains completed checkpoints owned
by other stages. Forced process termination may leave an unpublished candidate;
lookup never treats that private directory as a completed checkpoint.

The existing split-cache sentinel also covers current-pair reuse, corruption,
identity mismatch, size and link rejection, changed inputs and keys,
interruption cleanup, concurrent publication, and excess inventory. The ordinary
coordinator route keeps that owner and the foreign-binding integration owner;
it no longer rebuilds the WVB runner merely to replay compiler preparation.

For edition-1 compiler sources, all six construction projects must explicitly
select Project 4. The constructor uses a task-owned detached checkout of the
recorded pre-migration Git source state to reconstruct predecessor products
through that tree's existing qualified WVB pins. It verifies the recorded tree,
rejects links or unsupported inventory entries, and bounds the checkout to
8,192 files and 768 MiB of tracked payload. Git must already contain that commit;
missing history fails explicitly without network fetching or a cache-only
fallback. The checkout is removed after construction. A bounded removal failure
reports and preserves the exact checkout and containing work directory for
recovery; cleanup diagnostics do not replace the primary failure. Ordinary intermediate
native caches remain reusable. No managed Stage 0 or second maintained source
implementation is introduced, and bootstrap executable pins do not change.

Project 4 construction first builds a direct-condition Analyzer with the actual
recorded predecessor Analyzer, Emitter and four admission roles. This is one
temporary source adaptation in the existing construction graph: three exact,
hash-bound selections remove scalar-pool finalization from the current Analyzer
closure, reducing it from 24 modules to 23. The adaptation restores the original
bytes exactly; unrelated source stays unchanged. The resulting Analyzer then
builds the full current Analyzer, including scalar finalization. The original
predecessor Emitter and admission roles remain selected for that first build;
later steps retain the existing current-tool graph. Project 2 is unchanged.

The intermediate uses the host-separated `direct-condition-analyzer-v1` family
and record `windvale-direct-condition-analyzer-checkpoint-1`. Its key binds the
current construction request, actual predecessor revision/tree/key and all eight
role/identity inputs, original and adapted source closures, selector version,
loaded helper and Node identities, selected complete verifier, Profile 8, and
existing source/package/identity commands. The caller must supply explicit
preparation context and its unchanged-input guard. Both prepared-only environment
restrictions refuse construction. An alternate project location must contain
the exact current repository Analyzer manifest.

The direct-condition Analyzer intermediate supplies this emission option as the
earlier of its inherited deadline and invocation time plus 600,000 milliseconds.
The enclosing source command retains its existing ten-minute ceiling; the
option does not renew that command or the total preparation deadline.

One inherited absolute deadline covers source adaptation, all commands, input
rechecks and publication. The intermediate owns only an Analyzer WVB 1.11,
native image and canonical identity; it never publishes the final current set.
Sources are bounded to 4 MiB, snapshots to 64 files and 16 MiB, WVB to 16 MiB,
native images to 64 MiB and records to 64 KiB. Existing entries require exact
inventory, request and product bytes; corruption fails without reconstruction.
Atomic concurrent publication requires equal products, and uncertain producer
termination preserves its work. Changing this owner invalidates the current
construction key; only exact unchanged phase requests may reuse earlier caches.
The first complete integrated cold duration and dual-host reconstruction remain
to be measured. Completed private source proofs are not a qualified checkpoint.

Later ordinary builds use the completed current set. Independent
reconstruction and two-generation byte convergence remain qualification gates;
unit-tested construction ordering alone does not satisfy them.

`Verify-Current-Split-Compiler-Convergence.mjs` builds two generations using
separate initially empty emission caches and compares exact analyzer and emitter
WVB bytes. Native packaging checkpoints remain reusable. The gate packages the
current compiler-aligned verifier with existing Profile 7 for these large
modules, checks both second-generation products, and rejects a malformed WVB.
Profile 2 remains the small-module verifier profile; exhausting it is not a
semantic rejection or evidence that compiler-scale output passed verification.
The gate binds its own implementation and verifier project inputs, retains its
private work after failure, and has a sixty-minute execution bound after
separately selected current-compiler preparation.

## Compiler-scale development sentinel

The immutable native Seed is recovery provenance, not the semantic definition
or ordinary compiler-scale development front door. It may reconstruct a later
compiler stage, but a current source closure is not required to remain within
the historical Seed's capacity. The active Language 1.0 owner reconstructs one
current analyzer/emitter pair and uses that pair for compiler-scale projects.
This is one compiler divided at its explicit phase artifact, not a parallel
compiler implementation.

The Generic-WIR sentinel compiles its canonical Project 2 closure twice through
the split coordinator, requires byte-identical WVB output with a pinned size and
digest, and admits the first output through the independent compiler-aligned
WVB verifier. It does not widen the native staging output envelope or the
general runner's call-depth bound merely to execute this compiler-scale product.
The Language 1.0 gate separately executes its language fixtures. Failure of the
historical Seed on this closure does not waive the sentinel and does not
authorize replacing the recovery artifact.
