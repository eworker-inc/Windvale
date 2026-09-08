# Windvale 2.0 stack and application review

> Status: Proposed; sampled code-backed improvement candidates
> Authority: Informative; not an accepted implementation or release gate
> Last reviewed: 2026-09-08

## Intended outcome and review boundary

Make the rest of the stack easier to maintain, more predictable under load, and
clearer about what an operation actually accomplished. Prefer focused changes
to owned components over a stack-wide rewrite. Some findings merit compatible
corrections before 2.0; a major release is not a reason to postpone them.

This AI review sampled code at revision `b4b51308` after the earlier language,
[compiler/tooling](Windvale-2.0-Compiler-And-Tools.md), and
[qualification/testing](Windvale-2.0-Qualification-And-Testing.md) reviews.
It inspected representative implementation paths and their existing plans;
it did not audit every file, run benchmarks, boot an OS, exercise providers,
inspect credentials, or establish a new security or qualification claim.

| Area previously needing deeper review | Scope of this follow-up |
| --- | --- |
| Runtime and native tools | Execution state, hosted boundaries, assembler/object/linker representation and construction. |
| Foundation and libraries | Byte handling, library ownership, database and service-facing resource/operation contracts. |
| OS, packages, and distribution | FAT32 services, package/release publication, and native bootstrap boundaries; static inspection only. |
| Applications and browser hosts | WVDB Workbench, shared browser state/lifecycle, playground workers, public repository browsing, and hosted model-chat input. |

The [2.0 release plan](Windvale-2.0-Release-Plan.md) owns selection and
compatibility. The [1.0 product plan](Windvale-1.0-Product-Plan.md),
[library plan](Windvale-Libraries-1.0-Plan.md), and existing component plans retain
their work. Referencing an unfinished feature here does not move it to 2.0 or
make it a new idea. Proposals below separate inspected mechanisms, recommended
changes, and the evidence required before adoption.

## Current-contract concerns to investigate first

These source-level findings deserve focused reproduction and correction review
before optional feature work. They are not established credential disclosures,
host-provider escapes, or reproduced concurrent failures. No fix was made as
part of this documentation review.

### Account for concurrent stream reservations against shared authority

Observation: [Connect-Stream-Core](../../Libraries/Network/Connect-Stream-Core.wv)
charges unfinished operations to the queue limit, but new transfer admission
subtracts only completed progress from the aggregate transfer allowance.
Advancement checks the old totals and integer overflow before adding progress.
With a transfer allowance of 100 bytes and queue allowance of at least 160,
an 80-byte read and an 80-byte write appear able to pass admission before either
completes, then produce totals of 160. This is an unexecuted state-transition
finding in a capability-free model, not a demonstrated live network escape.

Proposal: investigate and protect the invariant that consumed transfer bytes
plus outstanding reservations never exceed the grant. The
[current stream contract](../../Specifications/Network-Connect-Stream-Core.md)
already requires aggregate bounds. A later reusable reservation API could bind
leases to provider/operation generations, convert reservations into exact
progress, and refund only known unused capacity.

Tradeoff/evidence: conservative reservations may reduce utilization. Extend the
existing stream owner with both completion orders, partial progress,
cancellation, restart, stale events, and indeterminate writes; prove no double
spend or double refund. The existing-limit correction should not wait for 2.0.
Any new public lease interface needs its own explicit contract/version.

### Enforce one activation transaction across publication and recovery

Observation: [activation publication](../../Tools/Package/Publish-Installation-Activation.mjs)
checks existing candidates and the expected current digest, creates a
next-digest-specific candidate, then replaces the activation. The adapter has
no common exclusive guard around those steps. Two different next digests can
therefore pass the initial checks concurrently. Recovery can also remove a
candidate without establishing that its publisher has stopped.

Proposal: use one bounded transaction guard for publish and recover, and check
the expected activation while holding it. Require host-observed stale-owner
evidence, not just a PID in a filename. Keep readers on immutable snapshots and
preserve indeterminate publication outcomes. Exclusive writers are already an
[installation architecture requirement](../Architecture/Windvale-Package-Bundle-And-Installation.md#concurrency-and-readers),
not new 2.0 package-manager functionality.

Tradeoff/evidence: locking and stale recovery have host-specific failure modes.
Extend the [existing publisher checks](../../Tools/Package/Verify-Installation-Activation-Publisher.mjs)
with synchronized writers, publish/recover races, writer death, lock timeout,
and concurrent readers on Windows/Linux. Preserve activation/generation bytes;
integrate through the [package implementation plan](Windvale-Package-System-Implementation-Plan.md).

### Close maintained secret-buffer cleanup gaps

Observation: [Protected-Credential](../../Runtime/Hosted/Credentials/Protected-Credential.mjs)
copies the key returned by the derivation callback, while later cleanup erases
the copy rather than that original buffer. Unlock concatenates temporary
decryption buffers without individually retaining them for cleanup; if final
authentication throws, the earlier temporary is not assigned to the variable
that the catch handler erases. Explicit cleanup already exists for many other
owned buffers and must be preserved.

Proposal: give every secret-bearing buffer one explicit owner, avoid the
unnecessary derived-key copy, and name/erase temporary decryption chunks in
every success and failure path. This strengthens the existing
[credential custody contract](../../Specifications/Protected-Provider-Credential.md)
without changing WVSC bytes, key derivation, or algorithms.

Tradeoff/evidence: cleanup of maintained buffers does not prove erasure inside
the JavaScript engine, crypto library, or OS. Extend existing credential checks
using synthetic keys and observable owned buffers under successful unlock,
authentication failure, and injected failures. Preserve caller ownership and
generic failure diagnostics. Do not read real credentials to validate this.

## Native tools and admitted working state

### Index WVO records during admission

Observation: [WVO verification](../../Object-Model/Windvale/Wvo-Object-Verification.wv)
checks each symbol against earlier names; each earlier-symbol lookup restarts
at the table beginning. For valid unique names this nests three record walks,
giving worst-case cubic traversal in symbol count even under the 4,096-symbol
limit. Existing validation is real; the issue is its work amplification.

Proposal: admit records sequentially, retain a bounded offset directory, and
check uniqueness with deterministic indexes or merges of canonical binding
ranges. Bind every admitted directory to its exact immutable input. The
[linker](../../Linker/Windvale/Wv-Linker-Core.wv) already contains a cross-binding
merge approach worth comparing, not copying into an independent oracle blindly.

Tradeoff/evidence: retained offsets cost memory and reordered checks can alter
the first reported error. Compare acceptance, status, failure location, and
inspection output against the current validator; include maximum symbol counts,
cross-binding duplicates, long common prefixes, and malformed records. Measure
time and peak memory. Preserve WVO bytes and diagnostic contracts.

### Retain assembler and linker plans instead of rediscovering structure

Observation: [WVA assembly](../../Assembler/Windvale/Wva-Assembler-Core.wv)
rescans declarations per symbol and searches definition ranges again during
emission. Measurement and section/symbol/relocation emission are separate walks.
The [linker](../../Linker/Windvale/Wv-Linker-Core.wv) repeatedly calculates
placements and resolves relocations during validation and emission. Its
[first-read snapshots and streaming emission](../../Specifications/Wv-Linker-Core.md)
already exist; this is not a proposal to add them again.

Proposal: retain two distinct bounded models: an assembly plan with source
spans, declarations, labels, sizes, and relocation requests; and a production
link plan with admitted object directories, symbol resolutions, placements,
and relocation values. Consume each in canonical output order. Continue the
accepted [structured object-writer migration](../../Specifications/Windvale-Wvo-Object-Construction.md)
without treating WVA, WVO, and executable images as one format.

Tradeoff/evidence: explicit plans add memory and bootstrap-transition work.
Preserve exact WVO/image/map bytes, diagnostics and error precedence, alignment,
overflow rejection, and output preservation. Measure declaration-heavy assembly
and relocation-heavy links independently. Keep reconstruction verification
algorithmically independent. Source edits alone do not update pinned native
applications; construction and promotion need their owned evidence.

## Database, Foundation, and service composition

### Reuse admitted transaction batches and shared tree paths

Observation: the [durable transaction writer](../../Libraries/Platform/Database/Durable-Transaction-Writer.wv)
decodes a mutation batch, then calls the
[public mutation reader](../../Libraries/Database/Transaction-Mutations.wv)
for each item. That reader decodes again, scans from the beginning, and copies
the selected key/value. The writer also gathers a full root-to-leaf path per
mutation; [path admission](../../Libraries/Database/Transaction-Paths.wv)
expects mutation-count times depth times page-size bytes, repeating ancestors.
These costs are bounded, not unbounded execution.

Proposal: admit once into a transaction-scoped cursor/index, retain unique pages
under an exact database/storage snapshot, and represent per-mutation paths as
references into that bounded table. Keep external byte admission and reject
forged or stale evidence; a public `Valid` flag alone is not proof.

Tradeoff/evidence: indexes retain metadata and require explicit lifetimes.
Compare 1/8/32 mutations, clustered/dispersed keys, and depths 2/4/8; measure
decoder calls, page reads, allocations, and memory. Preserve logical results,
determinism, and rejection of inconsistent snapshots/pages. Keep persisted
formats unchanged where possible; version changed serialized path evidence.
Collections and storage caching are already
[1.0 library work](Windvale-Libraries-1.0-Plan.md); the added focus is
transaction-wide reuse with an exact identity boundary.

### Preserve bounded segments through encoding and I/O

Observation: [commit construction](../../Libraries/Database/Commit-Batch.wv)
combines data pages and a log into one byte value, then
[storage publication](../../Libraries/Database/Storage-Publication.wv) slices
it into transfers through a [contiguous-byte API](../../Libraries/Platform/Storage/Random-Access-Storage.wv).
[SHA streaming](../../Foundation/Sha256-Streaming.wv) already processes complete
blocks directly and combines only boundary tails, providing a useful contrast.
The amount of physical copying still needs measurement.

Proposal: evaluate a bounded immutable segment sequence shared by encoders,
digest consumers, and optionally vectored I/O: one operation over multiple
buffers. Specify segment-count and retained-byte limits, ownership until
completion, and exact progress as a prefix of the logical concatenation. Make
flattening explicit; do not promise zero-copy across all hosts.

Tradeoff/evidence: descriptors and retained owners can cost more for small
values. Compare fragment counts/page sizes, allocations, calls, and memory;
test partial completion inside a segment, cancellation, and teardown. Require
identical logical output/digests. Builders and streaming are already planned;
this proposal preserves segments across their boundaries. Retain contiguous
APIs and version any new provider operation separately.

### Evaluate group commit without adding concurrent writers

Observation: [Commit-Batch](../../Libraries/Database/Commit-Batch.wv) groups
pages for one committed sequence, not multiple independently acknowledged
transactions. [Publication](../../Libraries/Database/Commit-Publication.wv)
orders page write/flush and superblock write/flush before reporting commitment.
The [transaction specification](../../Specifications/Windvale-Database-Transaction-Commit.md)
already names group commit as future work; this is a concrete evaluation of
that item, not a newly discovered missing feature.

Proposal: keep one serialized writer but consider sharing durability barriers
across a bounded group of independent transactions. Preserve each request's
identity, conflict result, sequence, and uncertainty. Bound group count, bytes,
and maximum waiting time; acknowledge only after required durable publication.

Tradeoff/evidence: fewer barriers may improve throughput while harming tail
latency and complicating recovery. Compare groups of 1/4/16 at low and sustained
load; inject failure around every write/flush and verify acknowledgment and
recovery ordering. Review stored-format and recovery compatibility explicitly.
Do not trade durability for a speed claim or make this new 1.0 scope.

## OS, distribution, and bootstrap boundaries

### Retain a versioned FAT32 chain-admission witness

Observation: [file-read preparation](../../Operating-System/Services/Fat32-File-Read-Transaction.wv)
calls [chain-position resolution](../../Operating-System/Services/Fat32-Chain-Position.wv)
which re-admits the full trace. [Chain admission](../../Operating-System/Services/Fat32-Cluster-Chain.wv)
searches the growing visited sequence for cycles, producing quadratic
comparisons before repeated read steps are considered. The cluster bound is
already explicit; raising it is not the proposed optimization.

Proposal: retain an immutable admitted witness bound to trace bytes, geometry,
and media generation, then resolve positions from it. Separately measure a
bounded membership index for first admission. This follows the measurement
and versioned-proof requirement in the
[bounded FAT32 file-read decision](../Decisions/0708-Compose-Bounded-Fat32-File-Reads.md),
not an assumption that the pending live driver already works.

Tradeoff/evidence: retained proof state needs an exact invalidation lifetime.
Extend existing FAT32 owners with maximum traces, early/late cycles, malformed
endings, cross-cluster reads, and media changes. Compare against the current
validator and measure comparisons, allocation, elapsed time, and retained bytes.
Preserve external filesystem/block contracts and failure classifications.

### Enforce release-tool limits before buffering, then stream payloads

Observation: [installer-repository construction](../../Tools/Release/Build-Installer-Repository.mjs)
reads payloads before checking declared length/object limits and retains
compressed objects during construction. The
[installer builder](../../Tools/Release/Build-Installers.mjs) also reads payloads
before its aggregate limit check. The
[release-envelope verifier](../../Tools/Release/Verify-Release-Envelope.mjs)
does preflight size checks, but still reads each admitted artifact into one buffer.
Existing digest checks establish identity; they do not avoid these allocations.

Proposal: preflight metadata, file type, count, individual and aggregate sizes;
enforce ceilings during reads as well, including files that grow. Stream hashes
and spool compressed objects while retaining bounded index metadata. Handle
text normalization with explicit raw and canonical byte budgets.

Tradeoff/evidence: temporary-file lifecycle and deterministic compression need
care. Extend existing installer/repository/envelope owners with oversized and
growing files, aggregate excess, normalization boundaries, interruption, and
near-limit memory measurements. Require byte-for-byte Windows/Linux output;
if compression bytes change, publish explicit successor identities. Preserve
signatures, roots of trust, index ordering, and package/release ownership.

### Bound native bootstrap teardown and retain useful failed evidence

Observation: the [split-compiler convergence coordinator](../../Tools/Native/Verify-Current-Split-Compiler-Convergence.mjs)
launches wrappers but uses direct-child `kill()` on timeout/output excess.
Timeout completion depends on process close; output-limit rejection does not
wait for descendant teardown. Its final cleanup removes the private workspace
on failure too. This helper therefore does not itself establish bounded
process-tree termination before deleting intermediate evidence.

Proposal: reuse the existing bounded descendant-termination policy from the
[verification process helper](../../Tools/Native/Stream-Verification-Owner.mjs),
add an explicit overall budget, and retain bounded failed-run artifacts where
useful. This is a concrete extension of the
[testing review](Windvale-2.0-Qualification-And-Testing.md), not another runner.

Tradeoff/evidence: retained evidence needs disk limits and an explicit cleanup
policy. Use synthetic child/grandchild tests for inherited pipes, ignored
termination, excessive output, failed launch, and cancellation. Later approved
convergence runs must still prove exact Stage 1/2 equality and independent
admission. Failed artifacts are not passing qualification receipts. Keep
managed Stage 0 recovery-only and ordinary launch free of hidden cold builds.

## Browser and application boundaries

### Publish coherent state and bound pending renders

Observation: [State-Owner](../../Libraries/Web/Framework/State/State-Owner.ts)
shallow-freezes snapshots, then synchronously calls subscribers using the
current snapshot field. A subscriber can reenter `Update`; later subscribers
in the outer notification can then receive a newer snapshot with the older
change list. A thrown subscriber also stops that notification loop. The
[render scheduler](../../Libraries/Web/Framework/UI/Render-Scheduler.ts)
accumulates changes in an array until its microtask and scans them per boundary.
These are code-level observations, not a reproduced application failure.

Proposal: specify reentrant publication and subscriber-failure behavior, bind
each notification to its own snapshot/revision, and queue or reject nested
updates explicitly. Coalesce render invalidations by affected boundary under a
bounded queue; provide cancellation/disposal for scheduled work. Keep the
existing [lifecycle scope](../../Libraries/Web/Framework/Lifecycle/Lifecycle-Scope.ts),
which already disposes in reverse order and collects cleanup failures.

Tradeoff/evidence: define ownership of nested immutable data rather than blindly
deep-freezing large graphs. Test two subscribers with nested updates, thrown
callbacks, unsubscribe/dispose during publication, and update bursts. Measure
retained memory and input-to-render latency. These can be compatible library
improvements, but notification ordering is a contract to migrate deliberately.

### Make browser requests cancellable and reject stale results

Observation: [compiler](../../Tools/Windvale.Playground/wwwroot/js/windvale-compiler-host.js)
and [WebAssembly](../../Tools/Windvale.Playground/wwwroot/js/windvale-wasm-host.js)
hosts already use disposable workers and terminate them on completion or
timeout. They expose no caller cancellation parameter and initially admit
responses by object shape and request identity; the application adds selected
checks. The [document viewer](../../Website/docs/docs.js) can publish a fetched
document after the user has selected another one, because publication has no
selection-generation check.

Proposal: add explicit caller cancellation, source/selection revision identity,
and complete bounded request/result schemas. Ignore stale success and failure
responses, settle once, and release listeners/timers/workers on every path.
Keep compilation, execution, cancellation, and timeout outcomes distinct.
Do not replace isolation with a permanently mutable shared compiler worker just
to reuse downloads.

Tradeoff/evidence: cached immutable package bytes may be reusable after exact
identity checks; live execution state is different. Test out-of-order fetches,
rapid edits, abort/completion races, invalid result fields, synchronous launch
failure, and page teardown. Preserve the pinned engine, admission checks, and
instruction budgets. Version machine-facing message changes explicitly.

### Bound browser loading and make shell updates coherent

Observation: the [repository browser](../../Website/repository-browser.js)
checks the manifest's top-level shape, rebuilds the directory model, and creates
the whole filtered tree on render. The
[publication generator](../../Website/Scripts/Generate-Repository-Browser.mjs)
already sanitizes rendered Markdown and limits highlighted file size; those
protections are not absent. The
[compiler worker](../../Tools/Windvale.Playground/wwwroot/js/windvale-compiler-worker.js)
checks artifact size/hash after reading the full response. The
[Workbench service worker](../../Applications/Web/Wvdb-Workbench/Public/Service-Worker.js)
uses a manually named shell cache, immediate activation, and runtime caching
for eligible same-origin assets while excluding API requests.

Proposal: validate manifest entries and impose response/count/depth limits
before expensive parsing or retention. Index the tree once per snapshot and
render expanded or visible branches while preserving accessible navigation.
Use a build-identified shell asset manifest and explicit update readiness so
HTML and required assets belong to the same application version. Bound cache
growth and keep live service responses and mutation replay out of shell caching.

Tradeoff/evidence: lazy rendering and offline updates introduce more lifecycle
state. Test truncated/oversized downloads, invalid paths, interrupted installs,
old open clients, missing assets, offline reload, focus retention, and large
repository snapshots. Measure DOM count and memory. This extends existing
browser architecture; it does not claim an observed injection vulnerability.

### Keep demonstration, validation, execution, and saving distinct

Observation: [Workbench state](../../Applications/Web/Wvdb-Workbench/Source/Wvdb-Workbench-State.ts)
marks `query.validate` as valid and emits a fixed success message without
parsing the edited query. The [frontend plan](WVDB-Workbench-Frontend-Plan.md)
explicitly defines this as a deterministic local demonstration, and the UI
labels itself a synthetic preview. It is not a live WVDB query validator.

Proposal: represent demonstration status separately from actual validation.
Before connecting real services, bind validation results to exact draft,
parameter, schema, and validator revisions; any relevant edit invalidates them.
Keep execution admission, accepted progress, completion, durable save, and
indeterminate mutation as separate typed states. Model-generated suggestions
remain drafts until explicitly admitted and authorized.

Tradeoff/evidence: this adds visible states but prevents a reassuring label from
outliving its evidence. Test invalid edited queries, stale validation responses,
schema changes, cancellation, and uncertain mutations. Real service integration
already belongs to the backend-gated plan; it must not be enabled as a UI-only
shortcut. Clearer demonstration wording can ship before that integration.

### Make terminal editing respect Unicode without weakening secret handling

Observation: the supported [hosted model-chat input](../../Applications/Model-Chat/Windvale-Model-Chat.mjs)
reads and echoes one byte at a time and backspace removes one byte. Visible
lines are decoded as strict UTF-8 afterward. Backspacing once through a
multibyte character can therefore leave a byte prefix that fails decoding.
The [chat specification](../../Specifications/Hosted-Model-Chat-Command.md)
already distinguishes the supported hosted command from its Windvale-native
migration candidate; migration itself is not a new 2.0 proposal.

Proposal: share a bounded visible-line editing contract across the selected
hosts, at least deleting whole UTF-8 code points and explicitly specifying any
grapheme-aware behavior. Keep masked secret entry a separate path with buffer
erasure, no secret-bearing arguments/logs, and explicit terminal restoration.

Tradeoff/evidence: display cells, code points, and bytes differ; do not silently
normalize credentials. Test accented text, combining sequences, emoji, invalid
input, interrupted entry, and byte ceilings on both hosts. This is a focused
usability correction, not justification for a new terminal framework or
automatic model retry/fallback behavior.

## Recommended order and compatibility

| Priority | Selected next investigation | Why this order |
| --- | --- | --- |
| Current-contract follow-up | Stream transfer reservations, activation writer exclusion, and owned secret-buffer cleanup. | Investigate possible violations of existing guarantees before adding features. |
| Focused compatible corrections | Coherent browser publication, stale-result rejection, cancellation, honest preview states, Unicode line editing, and bounded bootstrap teardown. | Small owned boundaries with specific failure cases; no source-edition break is needed. |
| Measured internal refactors | WVO indexes, retained assembly/link plans, transaction/path reuse, FAT32 witnesses, and streamed release construction. | Remove repeated work while retaining existing oracles, bytes, and limits. |
| Explicit future API/durability review | Cross-operation reservation leases, segmented I/O, group commit, and coherent browser update contracts. | Larger ownership, provider, persistence, or lifecycle decisions need workload evidence first. |

Three recurring patterns are more valuable than adding unrelated features:
admit an immutable input into a bounded reusable model; reserve shared resources
before starting concurrent work; and distinguish a request from its accepted,
completed, durable, or uncertain outcome. Reuse those patterns through focused
owned contracts, not one universal framework spanning compiler, database, and OS.

No proposal here requires breaking edition-1 source. Internal formats, provider
APIs, notification ordering, admission witnesses, and database recovery rules
can still be compatibility boundaries. Version only the boundary that actually
changes, preserve recovery/migration, and never silently reinterpret existing
stored data or weaken accepted authority. Keep independent validation where it
is part of the evidence contract; reuse is not permission to skip it.

## What this review does not close

Representative gateway supervision, runtime tables, task/operation state, library
facades, and OS services were sampled, but this was not a complete interpreter,
collector, scheduler, crypto, database-recovery, kernel, driver, ABI, or package
security audit. Binary artifacts, generated machine code, live provider behavior,
physical devices, and deployment behavior were not qualified. Examples were not
exhaustively executed. Those remain named future review scopes when selected;
the coverage map does not mean every line in each directory was examined.

New primitive libraries, live WVDB services, accessibility follow-ups, native
chat migration, package-manager slices, and OS drivers already have owners and
plans. Do not count their incompleteness as a novel 2.0 feature list, remove them
from 1.0 by implication, or expand this review into parallel implementations.
For any selected change, extend the narrow existing verifier and record exact
acceptance/failure behavior plus time and memory measurements where relevant.
