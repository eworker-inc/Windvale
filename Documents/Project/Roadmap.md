# Windvale development roadmap

> Status: Current language, toolchain and essential-library delivery plan
> Authority: Informative plan; accepted decisions and specifications own contracts
> Last reviewed: 2026-10-03

Windvale's next intended product tag is `v1.0.0`. Decision
[0800, Target Windvale 1.0 directly](../Decisions/0800-Target-Windvale-1.0-Directly.md)
ended the earlier `v0.2.0` product plan. The signed `v0.1.0` preview remains the
completed public foundation.

The [language and essential-library scope decision](../Decisions/0976-Focus-Windvale-1.0-On-The-Language-And-Essential-Libraries.md)
now makes a usable compiler, runtime and libraries the first delivery. WVDB and
secondary applications are outside this critical path; the previous mandatory
database/service bundle is superseded.

This roadmap shows dependencies and completion gates. It is not an activity
diary. Current implementation standing lives in [Progress](Progress.md), and
the [historical roadmap](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Roadmap-History-2026-08-31.md) retains the detailed
milestone audits and measurements that preceded this concise plan.

The [compiler, tools, and libraries completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md)
proposes usable delivery chunks, concrete exit criteria, and a verification
rhythm that reserves long runs for selected chunk and qualification gates.

## Product outcome

Windvale 1.0 is one useful and supportable Windows/Linux product built from:

1. the implemented and qualified Language 1.0 contract;
2. the selected essential libraries used by ordinary programs and the compiler;
3. deterministic self-host reconstruction on Windows and real Debian;
4. one ordinary toolchain and clean installations with safe package lifecycle;
5. documented compatibility, migration, support, recovery, and security policy;
   and
6. exact integrated qualification and signed distribution evidence.

Windvale OS has its own qualification path. It may contribute shared contracts
and evidence, but it is not an undeclared blocker for the host product.

## Critical path

### 1. Select one supported toolchain over the qualified language baseline

Outcome: the frozen source design compiles through one authenticated,
target-aware path into verified and executable representations on Windows and
Linux.

The [Slice 8 qualification decision](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md)
and [exact paired-host evidence](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json)
close their exact compiler gate for the frozen source design:

- every frozen source feature has compiler, diagnostic, malformed-input, and
  execution evidence;
- unsafe and Foreign operations carry explicit effects, authority, ownership,
  target, ABI, and containment evidence;
- interpreter and native paths agree on their shared semantic subset;
- the current compiler reconstructs deterministically from its declared inputs;
- Windows and Linux pass the named Language 1.0 conformance gate; and
- Seed recovery remains frozen and separate rather than becoming a second
  forward compiler.

All six requirements passed at one exact source state. Seed remains the frozen
recovery compiler, while the qualified Language 1.0 compiler is the forward
path. A future language or WVB change must establish a new versioned contract
and evidence rather than silently weakening this gate.

This does not establish general native memory management, complete libraries or
installed delivery. Trace ordinary commands to the tools they actually run and
record current, required-bootstrap, superseded, recovery-only and parked paths
in the [supported-toolchain map](Compiler-Tools-And-Libraries-Completion-Plan.md#supported-toolchain-and-retirement-map).
Keep necessary construction inputs until their successors reproduce the tools.

### 2. Make ordinary development predictable

Outcome: one selected compiler generation serves the normal build/verify/run
path. Separate bounded preparation from ordinary execution, reuse products by
complete input identity and preserve valid evidence. Missing preparation must
name the required command rather than trigger hidden reconstruction.

Measure a small program and affected compiler component before and after a
workflow change. The [throughput plan](Verification-Throughput-Plan.md) owns
feedback targets; broader optimization waits unless it obstructs delivery.

### 3. Complete ownership-to-storage memory and essential libraries

Outcome: ordinary programs and the toolchain use stable bounded values,
collections, bytes/text and explicit host I/O, with correct reusable storage.

Completion requires:

- a finite essential API/target matrix reconciled with the accepted Foundation
  registry and explicitly selected host boundaries;
- explicit portability, platform, authority, and capability classification;
- exact limits and failure behavior for each public operation;
- at least one real consumer for every required profile; and
- Windows/Linux conformance for shared profiles and honest target evidence for
  platform-specific profiles.

Library work follows Language 1.0 where new source semantics are required, but
independent library contracts and consumers may advance in parallel.

First connect unique mutable owners, shared immutable backing, borrowed
lifetimes, budgets, allocation leases and deterministic release through the
existing compiler/runtime. Cover aggregates, failure exits and terminal cleanup.
Reclaim interpreter working storage separately from guest accounting.
Fixed-live-state loops must stabilize within measured bounds.

Use compiler parsing, symbol tables, interpreter state and output construction
as maintained consumers. Required package functions remain supported; the typed
Package-Lock migration is deferred. Preserve the
[completed package-parser checkpoint](Compiler-Tools-And-Libraries-Completion-Plan.md#active-milestone-package-parser-with-immutable-borrowing).
Resolve draft APIs before dependent implementation rather than importing the
whole Backend catalog into this gate.

### 4. Demonstrate self-hosting and installed use, then retire old paths

Outcome: users can install, run, update, recover, roll back, and remove Windvale
components without losing separately owned data or receiving undeclared
authority.

Completion requires:

- immutable package and dependency identities;
- signed release admission and offline verification;
- separate application approval and rights-limited provider binding;
- deterministic compiler reconstruction from declared inputs on both hosts;
- one selected current compiler/runtime generation in ordinary commands;
- data ownership and migration rules;
- stable command, diagnostic, support, and compatibility policy; and
- Windows/Linux installers and recovery instructions tested from clean systems.

The completed `v0.1.0` and offline package-lifecycle gates are foundations. Do
not reopen them or rename their artifacts to simulate 1.0 completion.

Retire superseded active implementations and compatibility only after their
supported construction/execution dependencies are replaced. Secondary
applications may break and be migrated later. Preserve immutable recovery and
published evidence. Qualify a selected generation before installed promotion.

### 5. Run integrated qualification and release

Outcome: one selected source state produces the release artifacts and evidence
needed for the `v1.0.0` claim.

Completion requires:

- the exact 1.0 language, essential-library, toolchain, package and support gates
  are closed;
- deterministic outputs and resource limits are checked on Windows and Linux;
- security, malformed-input, recovery, upgrade, and compatibility cases pass;
- published manifests and checksums identify the exact release artifacts;
- an independent offline verification path succeeds; and
- the release notes state remaining platform and product limits plainly.

Complete qualification runs once for the deliberately selected state. It is not
a routine per-commit development test.

## Deferred product lanes

WVDB, broad Data/Backend catalogs, network/TLS/HTTP services, privileged service
installation and secondary applications are deferred under the current scope.
The [WVDB plan](WVDB-1.0-Specification-Plan.md) retains its accepted contracts
and unfinished requirements; no new database slice is required now.

### Windvale OS

OS-1 advances one cleanly launched and supervised service/application
composition while preserving the exact WVB portability proof already qualified
across Windows, Linux, and the guest.

When OS work resumes, its separate sequence is:

1. finish the source-owned fixed process-machine replacement and its live boot
   cutover;
2. bind one surviving filesystem consumer and admitted FAT32 media;
3. enter the provider and complete one bounded read with failure rollback and
   teardown;
4. add the sequential isolated network provider only after the filesystem
   lifecycle is sound; and
5. add broader launch, supervision, scheduling, or hardware support only from a
   named consumer and contract.

Pinned QEMU/Q35 remains the reproducible oracle. Physical, accelerated, or
nested providers report separate evidence.

No new OS slice is required before this host-toolchain delivery.

## Strategic and proposed lanes

- [Windvale 2.0 ideas](Windvale-2.0-Ideas.md) are future proposals and do not
  extend the current source contract or delivery gate.
- The [2027 compute and efficiency program](Windvale-2027-Compute-Leadership-Roadmap.md)
  may contribute measured compiler, runtime, accelerator, networking, storage,
  and OS improvements. It does not create unmeasured performance claims or add
  undeclared 1.0 requirements.
- The [agent runtime plan](Windvale-Agent-Runtime-Implementation-Plan.md) remains
  a proposed future product lane. It may consume qualified language, database,
  package, model-provider, and OS contracts without redefining them.
- The [organizational Observatory plan](Windvale-Organizational-Observatory-Implementation-Plan.md)
  remains proposed. It begins with synthetic read-only evidence and cannot
  silently become a surveillance, authority, or action system.

## Workstream rules

- Implement only from an accepted contract or an explicitly labeled proposal
  whose output remains a proposal.
- Route semantic, storage, authority, package, and OS needs to the document that
  owns that boundary.
- Add a shared feature only for a named consumer, finite limits, and executable
  evidence.
- Keep implementation checkpoints in code, specifications, evidence, Progress,
  or the changelog. Reserve numbered decisions for durable and difficult-to-
  reverse choices.
- Preserve passing evidence while its declared inputs remain unchanged.
- Measure performance and memory before and after a material optimization.
- Replan when evidence invalidates a mechanism; never lower an accepted gate by
  describing a narrower demonstration as completion.

## Verification rhythm

Run the change-aware verifier once after a coherent edit:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -PlanOnly
```

Execute one causal selection after reviewing cost. Use focused owners for
ordinary work. Run complete paired-host
qualification only for a selected release, promotion, bootstrap, security, ABI,
or conformance state.

## Completed foundations and detailed history

The [historical roadmap](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Roadmap-History-2026-08-31.md) preserves the full
completion gates and audits for predictable development feedback, the
package-backed host application, the signed `v0.1.0` preview, the offline
package lifecycle, OS-1 foundations, and earlier proposed product lanes.
