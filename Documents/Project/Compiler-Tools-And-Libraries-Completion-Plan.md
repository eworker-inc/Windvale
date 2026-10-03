# Compiler, tools, and libraries completion plan

> Status: Current delivery milestones; wider library catalog remains proposed
> Authority: Informative; accepted specifications and decisions own contracts
> Last reviewed: 2026-10-02

The maintained package parser now uses canonical `Option<u64>` and immutable
payload borrowing through ordinary project build, safe publication, and
Windows/Debian package execution. Focused candidate WVB 1.40 through 1.42 bridges
let immutable helpers observe raw Vector payloads and borrow scalar or Copy-record
elements without consuming their owners. The immediate priority is completing
the Language 1.0 ownership-to-storage memory path before expanding consumers
that depend on it. A separate owned-resource consumer, wider composition,
exclusive borrowing, take, mapping, and the wider Libraries 1.0 suite follow
that prerequisite. The
maintainer approved this delivery split on
15 September 2026; it changes progress reporting, not language or release scope.

This plan coordinates the [roadmap](Roadmap.md),
[Libraries 1.0 delivery plan](Windvale-Libraries-1.0-Plan.md), and
[verification throughput plan](Verification-Throughput-Plan.md). It does not
accept draft signatures, reopen the frozen language design, or add new release
requirements. “Complete” means the required Windows/Linux 1.0 host surface in
the [product plan](Windvale-1.0-Product-Plan.md), with explicit target limits.
Windvale OS and optional profiles retain their own gates.

The [1.0 completion matrix](Compiler-Tools-And-Libraries-1.0-Matrix.md) tracks
the finite Foundation registry, required host families, draft API decisions,
package/target evidence and installed-toolchain gates. It separates accepted
contracts from proposals and names the next owned-resource consumer's missing
collection operations; no row closes from a filename or isolated passing test.

## What the review establishes

| Area | What already works | Remaining work |
| --- | --- | --- |
| Frozen Language 1.0 compiler | The [Slice 8 decision](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md) closes its exact paired-host compiler and reconstruction gate. | Preserve that baseline. Complete versioned library-driven compiler/runtime additions and integrate the selected generation into the delivered toolchain. Cold construction remains performance work. |
| Option/Result | Canonical variants and bounded immutable borrowing work in the maintained package parser/lock consumer through the normal build/publication path on Windows and Debian. The focused raw-Vector projection-to-helper bridge is implemented; see the [integration evidence](../Evidence/2026-09-15-Source-Edition-Package-Integration.json) and [projected-payload checkpoint](../Evidence/2026-09-22-Borrowed-Vector-Payloads.json). | A separate owned-resource consumer, arbitrary composition, exclusive borrow, take, mapping, and installed promotion remain separate gaps. |
| Foundation and Data | Memory/collection contracts, bounded components, byte algorithms, SHA-256, and database JSON implementations provide starting points. | Close public operations and ownership behavior; extract shared data APIs and migrate consumers. General CBOR is still unimplemented according to the library owner plan. |
| Hosted libraries | Filesystem/storage facades, operation state machines, network values, and bounded hosted network/TLS/HTTP implementations exist. | Deliver the selected Language 1.0 APIs, instance binding, provider lifecycle, and shared consumers. Existing isolated evidence does not qualify the complete Backend profile. |
| Developer and delivery tools | Native build, verification, execution, assembly, linking, packaging, recovery, editor grammar, and a bounded browser playground exist. | Reconcile delivered compiler/package identities, finish installed workflows and service operations, and document exact supported targets. Editor highlighting and browser subsets do not establish full compiler support. |
| Verification tools | Focused selectors and reusable construction products already provide short feedback for some workloads. | Remove repeated expensive construction on the active path, retain exact input identity, and make cost, timeout, and incomplete results explicit. Completing every throughput target is not a feature prerequisite. |

This is a planning review of owner documents, recent evidence, and selected
source entry points, not a fresh implementation or conformance audit. Older
overviews must not override newer focused evidence. Existing cases are evidence
for their exact recorded inputs, not an automatic pass for later source states.

## Ordered completion chunks

### 0. Complete the ownership-to-storage memory path

The maintainer selected this priority on 27 September 2026; the
[handbook](../../AGENTS.md#memory-management-current-limitations-and-target)
owns the durable direction and completion rules. Existing native arena
checkpoints, copy reductions, and deferred interpreter writes are bounded
mitigations. Guest allocation release does not establish reuse of the native
storage executing the interpreter.

The current source lowerer now reclaims call-local byte/text temporaries when
returning descriptor-bearing records and variants. It compacts the union of
returned ranges, including overlapping slices and nested fields, while keeping
caller-owned ranges unchanged. A workload returning borrowed data holds its
arena requirement at 1 KiB from one through 4,096 calls on Windows and Debian; a 127-byte arena
still refuses an allocation that cannot fit. This closes that return-path
retention defect, not general last-share release or the gates below. Pinned
bootstrap tools require separate reconstruction and qualification before their
identities change. The [native lowering contract](../../Specifications/Windvale-Native-X64-Lowering.md#record-return-arena-reclamation)
owns the exact bounds and conservative fallback.

Eligible immutable byte/text loops now reclaim obsolete storage at loop entry
while retaining live local slices, nested records, and active variants. The
fixed-state replacement regression uses an 8-byte arena through 32,768
iterations; the shared-slice regression uses 12 bytes. Bounds on analyzed
locals, blocks, and aggregate fields preserve conservative retention outside
this supported subset. This adds reuse within a call, but leaves owned mutable
storage, allocation leases, later bytecode shapes, and complete interpreter
working-storage reclamation open. See the
[loop reclamation contract](../../Specifications/Windvale-Native-X64-Lowering.md#loop-entry-arena-reclamation).

The runtime-private [owned-storage provider](../../Specifications/Windvale-Native-Owned-Storage.md)
implements the physical part of gate 2: committed zeroed capacity, exact physical
charging, generation-safe reuse, and bounded teardown. Its repeated-reuse case
holds 17 bytes of capacity within a 64-byte arena across 32,768 allocations,
with 2,112 bytes of fixed metadata. The
[budgeted adapter](../../Specifications/Windvale-Native-Budgeted-Storage.md) now
joins it to canonical budget/lease accounting: refusal preserves both domains,
release credits parents, and teardown clears all owned storage and accounting.
Its metadata totals 5,816 bytes and its repeated-credit workload returns the
parent reservation to zero through 32,768 cycles. Interpreter state still needs
integration; existing consumer process-memory measurements remain unchanged
by this runtime connection alone.

The candidate [native collection path](../../Specifications/Windvale-Native-Owned-Collections.md)
connects scalar Vector reservation, append, indexed reads, explicit replacement
growth, owned helper transfers and cleanup to that storage and accounting.
[Decision 0972](../Decisions/0972-Pass-Native-Vector-Borrows-To-Helpers.md) adds
borrowed Vector helper parameters for scalar reads and nested forwarding, so an
owning caller can release its Vector after the loan ends. Borrowed append and
growth, mutable indexed access, parameter length, normal launchers and arbitrary
owner aggregates remain open. Complete those lifetimes before migrating
interpreter working buffers or closing the memory gate.

Work proceeds through these gates, preserving existing qualified contracts:

1. Carry descriptor and aggregate ownership through the current Windvale
   compiler's calls, assignments, last uses, returns, and failure paths.
   Reassess the historical ownership plan and allocator leaf against the
   current implementation; version any changed native ABI through a decision.
2. Integrate reusable backing storage and allocation leases, including shared
   immutable aliases, uniquely owned mutable storage, and typed allocation
   refusal. Allocating runtime services must follow the same lifetime rules.
3. Bound and reclaim interpreter working storage independently of the guest
   heap. Fixed-live-state loops must stabilize as iteration counts increase.
4. Complete the typed package-lock consumer with allocation failure preserved
   through bundle writing and verification, unchanged-on-refusal behavior,
   cleanup, alias-survival, and repeated allocation/release coverage.
5. Establish Windows and real Debian evidence for those paths, measuring live,
   reusable, cumulative, and budget-charged storage separately from process
   peak memory. Close only the exact supported target and API scope.

The unintegrated typed package-lock work is useful consumer groundwork, but its
capacity probes do not close these gates. A larger hosted runner arena is not
the selected solution. The maintained parser/runtime optimizations can be
integrated independently while this memory prerequisite remains open.

### 1. Deliver the package parser, then complete Option/Result

<a id="active-milestone-package-parser-with-immutable-borrowing"></a>

#### Completed selected milestone: package parser with immutable borrowing

Consumer: `Libraries/Package/Canonical-Package-Text.wv` decimal parsing and its
maintained package-lock consumer. Canonical `Option<u64>` replaces the private
presence/value result, with immutable payload borrowing and unchanged decimal
syntax, overflow rejection, and package wire formats. An operation needing only
optional presence does not acquire an artificial Result layer.

| Gate | Delivered scope | Completion evidence |
| --- | --- | --- |
| Source closure | 394 connected Project 4 manifests and 593 edition-1 sources, retaining one shared SHA-256 implementation. | Native admission on both hosts, plus exact two-generation compiler convergence and compiler-scale verification. |
| Normal build and publication | Current-source compilation, authenticated cache reuse, and native final publication. | Nine launcher/publication cases per host, including replacement, aliases, malformed WVB, and bad-lock preservation. Transaction fault-injection qualification remains separate. |
| Consumer behavior | Maintained parser/lock and related package consumers use current native lowering. | 82 package-format groups and 43 borrow/pointer cases per host; bundle, generation, resolution, dispatch, and offline-stage workflows also pass. |
| Delivery record | Exact input identities, commands, measured durations where available, runtime profiles, and exclusions. | The [integration record](../Evidence/2026-09-15-Source-Edition-Package-Integration.json) distinguishes selected paired-host execution from installed promotion and full qualification. |

The existing `package-format` and production-admission owners own these
boundaries. Extend them with focused cases where needed; do not introduce a new
coordinator or replay unrelated suites. This milestone does not wait for every
Option/Result operation or installed-toolchain qualification. It does not claim
arbitrary payload or target support when only its declared subset passes.

The [dated delivery history](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Library-Development-History-2026-09-26.md#delivered-package-consumer-checkpoints)
preserves publication timings, the resolved older-lowerer mismatch and the
private lock-content extraction that kept borrowing within its proof bounds.
The table above and linked evidence own the delivered consumer's exact scope.

The maintainer approved the expanded source-edition rollout after the dependency
audit connected the package sources to shared compiler and verifier projects.
The [bounded rollout plan](Repository-Source-Edition-Rollout.md) owns its
bootstrap-first order, source classification, and completed selected gates.
Current compiler/admission products and the lowerer are constructed on both
hosts; the normal package paths use the current lowerer and segmented staging
where required. Forty production-admission cases pass per host after the runner's
private failure helper was kept within the supported native subset. Bootstrap
pins and installed identities remain unchanged. Full installed delivery and
repository-wide qualification are not implied by this completed consumer gate.

#### Following milestones: complete Option/Result operations

Track these as separate deliverables, not additions to the active consumer gate:

1. Wider owned-payload composition with a named consumer and exact supported
   shapes; retain copying, escape, lifetime, and authority rejection cases.
2. Exclusive borrowing with mutation, alias exclusion, and release behavior.
3. Take with exact ownership transfer and post-take owner state on every path.
4. Mapping with payload/callback ownership, failure, and bounded resource use.

The owned-payload audit found a prerequisite compiler defect: constructing a
variant from an owned record used a copying load instead of an ownership
transfer. The emitter now selects the existing transfer instruction for variant
construction, as it already does for records and arrays. The focused regression
source is `Foundation-Value-Borrow-Vector-Executable.wv`; the existing
`language-1-memory-budget-split-execution` owner has an explicit
`--foundation-owned-payloads` selection with supplied compiler, verifier, and
runner products, avoiding hidden cold construction. Its declared shapes are
Option and either Result side containing a generic record that owns an
`i32` Vector, plus a Copy-record control. This prerequisite does not close the
named-consumer milestone, direct Vector extraction, borrowed Vector helper
observations, arbitrary payload composition, or installed/native promotion.
The [paired-host publication checkpoint](../Evidence/2026-09-15-Owned-Variant-Payload-Publication.json)
passes complete verification and copied-payload rejection for those shapes.
The later [runtime and reclamation checkpoint](../Evidence/2026-09-15-Owned-Payload-Runtime-Reclamation.json)
executes all nine selected scenarios on Windows and Debian. Three owned-record
shapes each survive 128 allocate/observe/release cycles; repeated allocation
refusal releases its child budget, and unaddressable capacity reports the exact
failure. Copied payloads and invalid budget reuse still reject. The runner now
admits the existing memory-budget shapes and reserved Vector construction in
this minor-39 profile. The compiler also tracks budget temporaries by declared
ID rather than block visitation order, fixing valid loops without widening
ownership-proof limits. The next gate is a maintained owned-resource consumer;
direct Vector extraction, borrowed Vector helpers, arbitrary composition,
native lowering, and installed promotion remain outside this checkpoint.

The owned-consumer work exposed a missing collection prerequisite. Candidate
WVB 1.40 now lets helpers directly observe a Vector parameter's length using a
read-only parameter instruction without weakening the existing unique-owner
length instruction. Compiler, verifier, and runner pass focused Windows and
Debian direct-read tests with identical bytes; see the
[paired-host evidence](../Evidence/2026-09-15-Vector-Parameter-Length.json) and
[Vector parameter-read decision](../Decisions/0963-Read-Vector-Parameters-Without-Transferring-Ownership.md).

The two direct-call gaps are now closed in the focused candidate: a borrowed
Vector can pass through another helper and can be borrowed repeatedly inside a
loop without creating a false owner temporary. Windows and Debian emit identical
bytes and execute the broader fixture, including named argument order and
conflicting-access rejection; see the
[forwarding checkpoint](../Evidence/2026-09-22-Vector-Borrow-Forwarding.json).
That checkpoint did not enable Foundation-projected Vector forwarding or
consuming payload extraction.

The next focused bridge now lets an immutable helper observe a raw Vector
projected from an Option or either Result side, without consuming the payload or
releasing its original owner. Nine positive programs and nine malformed-bytecode
cases pass on Windows and Debian with identical bytecode. Coverage includes `Option.Present` and
`Option.Absent`, both Result sides and their unselected opposite projections,
nonempty and empty Vectors, 128 allocation/borrow/release cycles, and repeated
helper calls in loops. A phantom-type control checks that an unused Vector type
argument does not turn a Copy value into an owned value. The
[projected-payload evidence](../Evidence/2026-09-22-Borrowed-Vector-Payloads.json)
owns the exact host results and limits.

The [scalar indexed-borrow checkpoint](../Evidence/2026-09-22-Scalar-Vector-Indexed-Borrow.json)
now verifies eight positive cases, fourteen malformed modules, ten source
rejections and three bounds traps for `i32`, `u64` and one `u8`-backed enum.
The supplied-product selector is the current evidence; automatic CI execution
of those cases remains open. Paired Linux execution uses Windows-produced
native images, not independent reconstruction or native E3 lowering. The next
consumer gate now has the Copy-record element and owned-budget helper
prerequisites described below, but still needs scanner ownership integration
before the typed package-lock directory can replace its byte representation. Installed promotion
and full generic collection access are not established by this checkpoint.

The record-element gate has a distinct runtime dependency. The intended
Package-Lock entries contain scalar offsets and lengths, but a record value is
held by an aggregate handle. The interpreter's eight-byte collection backing
now retains its nominal Vector or Sequence type through construction, append,
and growth. That type identifies the element shape without changing the WVB
format or public collection API. At that internal-prerequisite checkpoint,
collection admission, the compiler, and the verifier still accepted only scalar
elements.
The current-source interpreter now follows record handles held by live Vector
and Sequence backings at allocation-pressure and function-return collection
points, including collections reached through a marked record. The scan is
bounded by the heap and allocation limits and rejects stale or wrong-type
record handles. The focused
[trace probe](../../Tests/Fixtures/WebAssembly/Wvb-Record-Vector-Trace-Probe.wv)
checks the nested record-to-Vector-to-record path; the
[paired-host checkpoint](../Evidence/2026-09-22-Record-Vector-Trace-Checkpoint.json)
records its limited result. This is not record-element source admission or
paired-host qualification. The subsequent trace-boundary checks validate type
directory lengths, offsets, names, record references and backing-allocation
tables before reading them. The existing trace probe includes truncated and
overflowing inputs, stale allocations, wrong nominal kinds, empty collections,
and grown Sequence backings; its
[bounds checkpoint](../Evidence/2026-09-25-Record-Vector-Trace-Bounds.json)
records the exact development evidence and limits.

Candidate WVB 1.42 now joins source/WIR admission, complete verification and
interpreter execution for recursively Copy record elements. Seven positive
programs and fifteen rejection groups pass on each host, with identical emitted
bytes and instruction counts for all seven programs. The
[Copy-record collection evidence](../Evidence/2026-09-25-Copy-Record-Collections.json)
covers ordinary and materialized generic records, explicit indexed borrowing,
growth retaining nested records, freezing
and Sequence aliases, append refusal, and 900 reclamation iterations. Growth
collects unreachable result wrappers before enforcing exclusive backing
ownership. Existing scalar borrowing, owned-payload tracing, and collection
lifetime selections also pass. Source empty-record declarations remain rejected;
the internal sentinel representation has direct runtime boundary coverage.

The [owned-budget helper implementation](../Decisions/0968-Thread-Owned-Budgets-Through-Collection-Helpers.md)
now supports construction, splitting, append and growth outside Main in the
candidate compiler/verifier/interpreter path. Helper return cleanup retains
caller, operand, returned-aggregate, task and allocation-lease roots. The
[development stabilization evidence](../Evidence/2026-09-26-Development-Stabilization.json)
records focused Windows/Debian coverage. Source freezing still requires a
single block.

The typed Package-Lock consumer is not delivered. The September 26
[ownership scan fix](../Evidence/2026-09-26-Ownership-Scan.json) removes repeated
constructor counts from the recursive child budget. The maintained Copy-record
regression executes on Windows and Debian, and the original parser-state
snapshot now emits deterministic bytes on both hosts. Excessive ownership
graphs still reject with the exact rule and source declaration.
The unused-reader emission rejection is fixed: optimized emission now validates
unreachable functions with their Foundation borrow context. The maintained
[`Emission-Unused-Borrow.wv`](../../Tests/Fixtures/Language-1.0/Emission-Unused-Borrow.wv)
regression compiles deterministically and executes on Windows and Debian.
The saved typed scanner emits with an owned-budget harness, but its execution
is not delivered. The old harness also forwards mutable budget borrows through
helpers, outside the current owned/immutable helper boundary; replace that
harness flow with explicit owned child budgets rather than inferring new support.

The runner keeps its eight-call-depth and 128 MiB byte-arena limits. Deferring
local-frame writes across scalar comparisons lets the saved typed-table probe
return `42` in 722,721 instructions on Windows and Debian; the previous runner
exited `66`. The short probe retains its 412,520-instruction result on both
hosts, with Debian peak RSS falling from 50,300 to 43,168 KiB in one paired
measurement. The [bounded comparison](../Evidence/2026-09-27-Typed-Parser-Deferred-Comparisons.json)
closes that probe's measured arena blocker, not the maintained typed consumer.
The typed-table WVB came from an ignored candidate source and harness. Other
saved variants can still reach the call-depth limit; the maintained scanner's
owned-budget harness is not yet executable. Migrate the candidate through the
maintained package API, preserve exact lock bytes, failure order, allocation
failures and resource bounds, then run the maintained consumer on both hosts.
With the corrected prepared emitter, the candidate reproduces the saved WVB
byte for byte. A realistic five-part lock returns `42` in 722,721 instructions,
an invalid digest reports the expected line and column in 180,120 instructions,
and a helper receiving an owned child budget returns `42` in 722,755
instructions. Each WVB has matching results and instruction counts on Windows
and Debian. These are candidate checks; the existing package consumers still
use the byte-directory API and need budget-aware migration.
The existing component fixtures use the current collection signature, valid
typed backings and minor-42 expectations. Emission alone does not establish
consumer delivery.

These immutable bridges do not enable consuming extraction or arbitrary payload
composition. Native lowering for minors 40 through 42, installed promotion,
exclusive Option/Result borrowing, Take, mapping, and full qualification remain
separate. A maintained owned-resource consumer is the next wider milestone; it
does not reopen the already delivered package-parser consumer gate.

Before starting each, enumerate its finite accepted public operations, existing
implementation, missing implementation, consumer, targets, and focused verifier.
An operation is complete only when those rows have evidence on its claimed hosts.
Report the immutable-borrow consumer as delivered when its gates close, even
while these later milestones remain open.

### 2. Finish the usable Foundation nucleus

Follow the selected Option/Result milestones with primitive ordering, collection
mutation and slicing, and bounded bytes/text construction. Close the remaining required numeric
conversion, parsing, Decimal128, map/set/iterator/arena, formatting, memory-budget,
operation, and task API rows from the accepted Foundation registry. Distinguish
already implemented language operations from missing public library operations.

Exit: compiler/package parsing and a database operation use the canonical
collections and builders; capacity exhaustion, bounds, failure ownership,
iteration/mutation rules, and release behavior pass focused conformance. Every
required row has an implementation, consumer, target scope, and evidence owner.

### 3. Complete the shared Data profile

Extract the existing strict JSON implementation into its shared owner while
keeping database envelopes under WVDB. Add the required CBOR profile, URI,
validation, encodings, sorting/searching, and digest facade. Move maintained
consumers with their contracts and fixtures so two general parsers do not evolve
independently. Keep optional CDDL, sequences, compression, and trusted fast paths
under the scope decisions in the library plan.

Exit: package, database, and HTTP consumers share the intended modules; malformed
and oversized inputs fail within stated work/memory limits; exact deterministic
encodings and schema-supported JSON/CBOR value equivalence are demonstrated.

### 4. Complete hosted file, storage, and operation support

Deliver the selected file/directory/publication APIs, instance-bearing storage,
and required time/operation providers. Preserve exact partial and indeterminate
mutation results, cancellation, revocation, stale generations, and teardown.
Retain the existing portable state machines as reference behavior.

Exit: bounded file copy, package publication, and a WVDB storage consumer use
the same public interfaces on both hosts, including restart and failure cases.
WVDB storage-format decisions remain owned by its separate specification plan.

### 5. Complete the required Network and Backend profile

Consolidate network authority, then deliver the selected resolver, stream,
listener, datagram, entropy, certificate/trust, key-operation, secure-artifact,
and TLS APIs. Build HTTP framing, bodies, client, routing, authorization,
diagnostics, configuration, and service drain on those contracts. Reuse existing
hosted/reference implementations and hostile-input corpora where applicable.

Exit: the library plan's bounded JSON/CBOR item service runs through isolated
Windows/Linux peers with denial, cancellation, provider loss, limits, and clean
shutdown. Its persistent CRUD result depends explicitly on the relevant WVDB
contracts; useful HTTP/provider work can proceed before all WVDB gates close.

### 6. Finish the installed developer and service toolchain

Integrate tools throughout chunks 1–5; reserve this chunk for the complete
installed workflow. Audit supported build, verify, run, inspect, assemble, link,
package, and publish entry points against the selected compiler and library
identities. Resolve ordinary-front-door promotion gaps deliberately. Finish
package/dependency admission, capability approval, service start/stop/restart,
upgrade, rollback, uninstall, data preservation, and recovery promises.

Exit: a clean Windows and Linux installation can build, inspect, package, and
run the reference application without a development checkout, then update and
recover it. Document finite diagnostics/exit behavior, compatibility, bootstrap
dependencies, and target support. Keep editor grammar synchronized; add semantic
editor features or wider browser support only through an explicit product need.

### 7. Qualify the selected product state

Reconcile the required API/package/target matrix and close the separate WVDB,
support, security, and operations gates. Select one exact candidate for complete
Windows/Linux qualification, reproducibility, recovery/upgrade, bounded resource
workloads, signed distribution, and independent offline verification.

Exit: the product plan's release gates pass for the identified artifacts.
Finishing the compiler and libraries alone does not qualify Windvale 1.0.

## Scope and dependency control

At the start of each chunk, enumerate its finite public operations, accepted
contract, source owners, consumer, target rows, and existing verifier selectors.
Separate existing implementation, source/API migration, missing code, and missing
verification; these are different kinds of work. Mark each row implemented,
locally verified, paired-host qualified, or pending with a concrete blocker.
Resolve draft API decisions before dependent code.
Do not equate an existing filename or a passing storage suite with API closure.

Keep one main feature chunk active. Contract review, consumer preparation, and
small enabling tool changes may proceed when independent. Limit throughput work
to a measured bottleneck obstructing that chunk; broader optimization retains
its own plan. Do not raise resource limits just to make a combined test fit.

Use dependency order and exit criteria rather than an unsupported completion
percentage or calendar promise. After chunks 1 and 2, estimate remaining work
from actual delivery time, unresolved API rows, and cold verification costs.

## Verification rhythm

The maintainer approved these qualification checkpoints on 15 September 2026:

1. During implementation, retain focused Windows/Debian checks and check the
   normal GitHub CI gate. Investigate failed CI now; deferring full qualification
   does not waive ordinary verification or turn a bypassed gate into a pass.
2. Before promoting a selected generation into the installed toolchain, integrate
   its compiler/runtime changes and maintained consumers, then run complete
   independent Windows/Linux qualification against that exact candidate.
3. Before release, close the required migration and product gates and qualify
   the final identified artifacts. Do not postpone a selected promotion gate
   until every draft library proposal is implemented.

These checkpoints apply the existing verification policy; they do not remove
earlier qualification required by a security, bootstrap, ABI, or explicit
conformance claim. Reuse unchanged evidence and rerun invalidated owners rather
than treating every commit as a new full-qualification event.

- During implementation, write coverage with the code and run only short checks
  needed to detect the changed contract's failures. Exercise ownership,
  authority, malformed-input, and mutation failures as those boundaries change.
  Defer long test execution, not the design of failure coverage.
- At a coherent chunk boundary, inspect
  `pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -PlanOnly`.
  Select one combined causal plan; count cold compilation, packaging, and
  reconstruction in its duration. A significant chunk is defined by its usable
  exit result, not its commit count or number of changed files.
- Ordinary local verification retains the ten-minute default. Longer local
  commands have [standing maintainer approval](../../AGENTS.md#testing-and-verification);
  state the command, expected cold duration, and finite maximum without asking
  again. If a run cannot fit its selected deadline, preserve the checkpoint,
  diagnose the cause, and select a focused check or a separately bounded
  continuation. Report every remaining gate as unverified; duration approval
  does not turn a development checkpoint into qualification.
- Share immutable construction products and valid development evidence using
  complete declared inputs. Execute required changed behaviors; give mutable
  recovery tests fresh state. After failure, rerun invalidated dependencies and
  affected cases, preserving unrelated completed evidence.
- Run affected Windows/Debian conformance at the chunk gate before making its
  cross-host claim. Reserve complete qualification, bootstrap reconstruction,
  and broad ABI/security gates for the exact boundaries or claims that require
  them. Do not repeat broad gates after documentation, commits, or pushes.
- Report implemented, verified, deferred, elapsed time, and the next gate
  separately. This planning change needs documentation checks only.

The package-parser milestone is delivered. Chunk 1 now targets a maintained
owned-resource consumer after the selected composition/reclamation gate.
Status updates name the exact remaining consumer or execution gate, rather than
repeating "Option/Result pending" or estimating a percentage for the entire
draft Libraries 1.0 catalog.
