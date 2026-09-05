# Windvale development roadmap

> Status: Current dependency plan for the direct Windvale 1.0 product
> Authority: Informative plan; accepted decisions and specifications own contracts
> Last reviewed: 2026-09-05

Windvale's next intended product tag is `v1.0.0`. Decision
[0800, Target Windvale 1.0 directly](../Decisions/0800-Target-Windvale-1.0-Directly.md)
ended the earlier `v0.2.0` product plan. The signed `v0.1.0` preview remains the
completed public foundation.

This roadmap shows dependencies and completion gates. It is not an activity
diary. Current implementation standing lives in [Progress](Progress.md), and
the [historical roadmap](Roadmap-History-2026-08-31.md) retains the detailed
milestone audits and measurements that preceded this concise plan.

## Product outcome

Windvale 1.0 is one useful and supportable Windows/Linux product built from:

1. the implemented and qualified Language 1.0 contract;
2. the required Foundation, Data, and Backend Libraries 1.0 profiles;
3. a production-usable WVDB 1.0;
4. immutable packages and supervised services with safe lifecycle operations;
5. documented compatibility, migration, support, recovery, and security policy;
   and
6. exact integrated qualification and signed distribution evidence.

Windvale OS has its own qualification path. It may contribute shared contracts
and evidence, but it is not an undeclared blocker for the host product.

## Critical path

### 1. Language 1.0 — complete

Outcome: the frozen source design compiles through one authenticated,
target-aware path into verified and executable representations on Windows and
Linux.

The [Slice 8 qualification decision](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md)
and [exact paired-host evidence](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json)
close this gate for the frozen source design:

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

### 2. Complete required Libraries 1.0 profiles

Outcome: ordinary applications can use stable bounded data and hosted services
without inventing private contracts.

Completion requires:

- one accepted catalog for required Foundation, Data, and Backend APIs;
- explicit portability, platform, authority, and capability classification;
- exact limits and failure behavior for each public operation;
- at least one real consumer for every required profile; and
- Windows/Linux conformance for shared profiles and honest target evidence for
  platform-specific profiles.

Library work follows Language 1.0 where new source semantics are required, but
independent library contracts and consumers may advance in parallel.

Immediate sequence:

1. finish the remaining operand, owned-payload, and authority-operation composition and
   source value-classification reconciliation for the immutable Option/Result
   candidate, then audit complete admission; retain the gate while current
   metadata, typing, loan-flow, and collection/callable probes cover components
   rather than the complete compiler-to-runtime contract;
2. implement bounded execution, reproduce it on Linux, and qualify the complete
   compiler-to-runtime path;
3. complete Option/Result take and mapping operations;
4. add primitive ordering, collection mutation and slicing, and bounded byte
   construction; and
5. migrate required real consumers and run one paired-host Libraries 1.0 gate.

### 3. Complete WVDB 1.0

Outcome: WVDB is a Windvale-owned database suitable for bounded production use,
not only a collection of storage experiments.

Completion requires:

- normative entity, table, relationship, index, query, and transaction behavior;
- bounded storage geometry, recovery, durability, and corruption handling;
- complete full backup and restore before a production claim;
- service, authentication/authorization boundary, observability, and safe
  operational procedures;
- compatibility and migration rules; and
- conformance, hostile-input, performance, and memory evidence on both hosts.

Follow the [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md). Existing
database code is implementation evidence until it is reconciled with the 1.0
contract.

### 4. Integrate packages, services, and support

Outcome: users can install, run, update, recover, roll back, and remove Windvale
components without losing separately owned data or receiving undeclared
authority.

Completion requires:

- immutable package and dependency identities;
- signed release admission and offline verification;
- separate application approval and rights-limited provider binding;
- bounded service start, stop, restart, upgrade, rollback, and teardown;
- data ownership and migration rules;
- stable command, diagnostic, support, and compatibility policy; and
- Windows/Linux installers and recovery instructions tested from clean systems.

The completed `v0.1.0` and offline package-lifecycle gates are foundations. Do
not reopen them or rename their artifacts to simulate 1.0 completion.

### 5. Run integrated qualification and release

Outcome: one selected source state produces the release artifacts and evidence
needed for the `v1.0.0` claim.

Completion requires:

- the exact 1.0 language, library, database, package, service, and support gates
  are closed;
- deterministic outputs and resource limits are checked on Windows and Linux;
- security, malformed-input, recovery, upgrade, and compatibility cases pass;
- published manifests and checksums identify the exact release artifacts;
- an independent offline verification path succeeds; and
- the release notes state remaining platform and product limits plainly.

Complete qualification runs once for the deliberately selected state. It is not
a routine per-commit development test.

## Parallel Windvale OS path

OS-1 advances one cleanly launched and supervised service/application
composition while preserving the exact WVB portability proof already qualified
across Windows, Linux, and the guest.

The next sequence is:

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

## Strategic and proposed lanes

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
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

Use focused development owners for ordinary work. Run complete paired-host
qualification only for a selected release, promotion, bootstrap, security, ABI,
or conformance state.

## Completed foundations and detailed history

The [historical roadmap](Roadmap-History-2026-08-31.md) preserves the full
completion gates and audits for predictable development feedback, the
package-backed host application, the signed `v0.1.0` preview, the offline
package lifecycle, OS-1 foundations, and earlier proposed product lanes.
