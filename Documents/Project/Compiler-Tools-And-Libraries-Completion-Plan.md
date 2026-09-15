# Compiler, tools, and libraries completion plan

> Status: Current delivery milestones; wider library catalog remains proposed
> Authority: Informative; accepted specifications and decisions own contracts
> Last reviewed: 2026-09-15

The next result is the maintained package parser using canonical Option/Result
through the ordinary project build, safe publication, and Windows/Debian
execution. Close that result separately from exclusive borrowing, take, mapping,
and the wider Libraries 1.0 suite. The maintainer approved this delivery split on
15 September 2026; it changes progress reporting, not language or release scope.

This plan coordinates the [roadmap](Roadmap.md),
[Libraries 1.0 delivery plan](Windvale-Libraries-1.0-Plan.md), and
[verification throughput plan](Verification-Throughput-Plan.md). It does not
accept draft signatures, reopen the frozen language design, or add new release
requirements. “Complete” means the required Windows/Linux 1.0 host surface in
the [product plan](Windvale-1.0-Product-Plan.md), with explicit target limits.
Windvale OS and optional profiles retain their own gates.

## What the review establishes

| Area | What already works | Remaining work |
| --- | --- | --- |
| Frozen Language 1.0 compiler | The [Slice 8 decision](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md) closes its exact paired-host compiler and reconstruction gate. | Preserve that baseline. Complete versioned library-driven compiler/runtime additions and integrate the selected generation into the delivered toolchain. Cold construction remains performance work. |
| Option/Result | Canonical variants, complete verification, and bounded immutable-borrow execution exist. [Fresh source](../Evidence/2026-09-08-Foundation-Borrow-Fresh-Paired-Host.json) and [native execution](../Evidence/2026-09-08-Native-Foundation-Borrow-Execution.json) have selected Windows/Debian evidence. | Deliver the maintained consumer through the normal build/publication path. Wider owned payloads, exclusive borrow, take, mapping, and installed promotion remain separate gaps. |
| Foundation and Data | Memory/collection contracts, bounded components, byte algorithms, SHA-256, and database JSON implementations provide starting points. | Close public operations and ownership behavior; extract shared data APIs and migrate consumers. General CBOR is still unimplemented according to the library owner plan. |
| Hosted libraries | Filesystem/storage facades, operation state machines, network values, and bounded hosted network/TLS/HTTP implementations exist. | Deliver the selected Language 1.0 APIs, instance binding, provider lifecycle, and shared consumers. Existing isolated evidence does not qualify the complete Backend profile. |
| Developer and delivery tools | Native build, verification, execution, assembly, linking, packaging, recovery, editor grammar, and a bounded browser playground exist. | Reconcile delivered compiler/package identities, finish installed workflows and service operations, and document exact supported targets. Editor highlighting and browser subsets do not establish full compiler support. |
| Verification tools | Focused selectors and reusable construction products already provide short feedback for some workloads. | Remove repeated expensive construction on the active path, retain exact input identity, and make cost, timeout, and incomplete results explicit. Completing every throughput target is not a feature prerequisite. |

This is a planning review of owner documents, recent evidence, and selected
source entry points, not a fresh implementation or conformance audit. Older
overviews must not override newer focused evidence. Existing cases are evidence
for their exact recorded inputs, not an automatic pass for later source states.

## Ordered completion chunks

### 1. Deliver the package parser, then complete Option/Result

#### Active milestone: package parser with immutable borrowing

Consumer: `Libraries/Package/Canonical-Package-Text.wv` decimal parsing and its
maintained package-lock consumer. Replace the private presence/value result with
canonical `Option<u64>` and exercise immutable payload borrowing without changing
decimal syntax, overflow rejection, or package wire formats. Do not add Result
to an operation that only needs optional presence to satisfy the milestone name.

| Gate | Remaining work | Completion evidence |
| --- | --- | --- |
| Source closure | Finish the consumer migration and reconcile every affected project dependency. Shared SHA-256 still has legacy callers; do not mix source editions or create a second SHA implementation. | The maintained projects compile with declared source inventories, profiles, and explicit targets, without diagnostic source rewriting. |
| Normal build and publication | Reuse Project 4 routing and connect current-source native publication. Manifest conversion is mechanical once each project's target and closure are known. | Ordinary `Build-Wvb` builds and safely replaces the output. Pre-publication rejection preserves prior bytes; injected transaction failures report the contract's exact completion or indeterminate state, without automatic replay. |
| Consumer behavior | Run maintained parser/lock cases and the affected borrow checks using the selected current products. | Valid, absent, zero, maximum, overflow, malformed, and invalid-span behavior passes; repeated inputs produce identical WVB; declared execution paths pass on Windows and Debian. |
| Delivery record | Record exact inputs, targets, commands, elapsed time, resource limits, and remaining exclusions. | Evidence identifies the usable consumer and distinguishes local tests, paired-host execution, and installed promotion. |

The existing `package-format` and production-admission owners own these
boundaries. Extend them with focused cases where needed; do not introduce a new
coordinator or replay unrelated suites. This milestone does not wait for every
Option/Result operation or installed-toolchain qualification. It does not claim
arbitrary payload or target support when only its declared subset passes.

The 15 September implementation connects Project 4 to the current-source native
publisher. Nine focused Windows cases pass, including replacement, deterministic
bytes, bad source-lock rejection, malformed WVB, and resource aliases. Five
native publisher cases also pass on Debian using a Windows-constructed Linux
executable. This is not independent Linux construction or fault-injection
qualification; exact inputs and exclusions are in the
[publication evidence](../Evidence/2026-09-15-Project4-Native-Publication.json).

The package migration remains uncommitted. The canonical parser builds and its
tests execute on Windows/Debian using the selected September 8 native lowerer.
Package-lock builds with ordinary Option matching, but the selected lowerer
rejects its WVB. Moving its four matches to immutable borrowing also exposes an
emitter `Invalid analysis / Invalid WIR` rejection; those unsuccessful changes
were withdrawn. Next: construct the current lowerer, close these consumer
failures, and reconcile shared SHA-256's legacy callers before committing the
source migration. Do not promote the diagnostic lowerer or claim full package
delivery from the isolated parser result.

#### Following milestones: complete Option/Result operations

Track these as separate deliverables, not additions to the active consumer gate:

1. Wider owned-payload composition with a named consumer and exact supported
   shapes; retain copying, escape, lifetime, and authority rejection cases.
2. Exclusive borrowing with mutation, alias exclusion, and release behavior.
3. Take with exact ownership transfer and post-take owner state on every path.
4. Mapping with payload/callback ownership, failure, and bounded resource use.

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

- During implementation, write coverage with the code and run only short checks
  needed to detect the changed contract's failures. Exercise ownership,
  authority, malformed-input, and mutation failures as those boundaries change.
  Defer long test execution, not the design of failure coverage.
- At a coherent chunk boundary, inspect
  `pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -PlanOnly`.
  Select one combined causal plan; count cold compilation, packaging, and
  reconstruction in its duration. A significant chunk is defined by its usable
  exit result, not its commit count or number of changed files.
- Ordinary local verification retains the ten-minute total budget. A longer
  command requires advance approval of that command and maximum duration.
  Finishing a chunk does not itself waive this rule. If the plan cannot fit,
  preserve the checkpoint and select a focused check or a separately approved
  qualification run; report the remaining gate as unverified.
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

The active result is chunk 1's package-parser milestone. Status updates name its
unfinished gate and next executable result, rather than repeating "Option/Result
pending" or estimating a percentage for the entire draft Libraries 1.0 catalog.
