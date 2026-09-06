# Compiler, tools, and libraries completion plan

> Status: Proposed completion sequence based on the 6 September 2026 repository review
> Authority: Informative; accepted specifications and decisions own contracts
> Last reviewed: 2026-09-06

The next useful result is complete Option/Result support through the compiler,
verifier, runtime, libraries, and a real application. Continue from that result
in substantial usable chunks, with short checks during implementation and one
combined affected verification plan at each chunk boundary.

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
| Option/Result | Canonical variant declarations, immutable projection publication, and focused verifier components exist. The [6 September operand checkpoint](../Evidence/2026-09-06-Foundation-Borrow-Operand-Integration.json) passes 185 groups on Windows and Debian. | Complete WVB 1.39 admission remains closed. Owned payloads, authority operations, source value classification, runtime retention, exclusive borrow, take, mapping, and a real consumer remain. |
| Foundation and Data | Memory/collection contracts, bounded components, byte algorithms, SHA-256, and database JSON implementations provide starting points. | Close public operations and ownership behavior; extract shared data APIs and migrate consumers. General CBOR is still unimplemented according to the library owner plan. |
| Hosted libraries | Filesystem/storage facades, operation state machines, network values, and bounded hosted network/TLS/HTTP implementations exist. | Deliver the selected Language 1.0 APIs, instance binding, provider lifecycle, and shared consumers. Existing isolated evidence does not qualify the complete Backend profile. |
| Developer and delivery tools | Native build, verification, execution, assembly, linking, packaging, recovery, editor grammar, and a bounded browser playground exist. | Reconcile delivered compiler/package identities, finish installed workflows and service operations, and document exact supported targets. Editor highlighting and browser subsets do not establish full compiler support. |
| Verification tools | Focused selectors and reusable construction products already provide short feedback for some workloads. | Remove repeated expensive construction on the active path, retain exact input identity, and make cost, timeout, and incomplete results explicit. Completing every throughput target is not a feature prerequisite. |

This is a planning review of owner documents, recent evidence, and selected
source entry points, not a fresh implementation or conformance audit. Older
overviews must not override newer focused evidence. Existing cases are evidence
for their exact recorded inputs, not an automatic pass for later source states.

## Ordered completion chunks

### 1. Finish Option/Result end to end

Close the remaining immutable-borrow verifier composition and source
classification seams, then implement bounded runtime retention and execution.
Keep unsupported admission closed until the complete path is sound. Complete
exclusive borrowing, take, and mapping with exact success/failure ownership.
Select a maintained parser or database consumer and migrate one useful operation
to the canonical APIs.

Exit: the real consumer compiles and executes on the declared interpreter/native
paths; invalid lifetime, owned-copy, escape, and authority cases reject; resource
release and deterministic output are demonstrated on Windows and Debian. This
is one delivery chunk even when implementation needs several focused commits.

### 2. Finish the usable Foundation nucleus

Follow Option/Result with primitive ordering, collection mutation and slicing,
and bounded bytes/text construction. Close the remaining required numeric
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
Mark each row implemented, locally verified, paired-host qualified, or pending
with a concrete blocker. Resolve draft API decisions before dependent code.
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

The immediate recommendation is chunk 1. The existing library plan already
names it as active; finish that usable result before widening the feature lane.
