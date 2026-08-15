# Windvale package-system implementation plan

## Status

- Date: 2026-08-14
- Status: Active implementation; focused package-format development started from merged commit `ce30152b`, while independent dual-host qualification remains a promotion gate
- Product direction: [packages, releases, updates, and recovery](../Architecture/Packages-Releases-And-Recovery.md)
- Official-source proposal: [hybrid Windvale endpoint and immutable GitHub archive](Windvale-Package-Source-Proposal.md)
- Proposed formats and transactions: [release discovery](../Architecture/Windvale-Release-Discovery.md) and [bundle and installation](../Architecture/Windvale-Package-Bundle-And-Installation.md)

This plan turns the accepted package direction and first WVDB Query source package
into an ordered implementation path for the one-time native `wv` bootstrap. It
does not claim that proposed release, bundle, store, generation, signature,
network, launch, or update contracts are implemented.

## End-state requirements

The goal is complete only when one independently obtainable native `wv` entry on
Windows x64 and Linux x64 can:

1. authenticate the official Windvale trust root and signed release discovery;
2. retrieve or consume offline exact package bundles;
3. verify package, lock, bundle, target, release, and executable identities;
4. publish immutable content objects without partial visibility;
5. construct and atomically activate a complete installation generation;
6. update and roll back without rewriting installed content;
7. inspect and bind exact rights-limited capability approvals before launch;
8. launch the selected compiler, tools, and applications without ambient authority;
9. update its client generation and recover the previous client; and
10. reproduce matching semantic reports and exact portable bytes on Windows and
    Linux, including offline recovery and hostile-input evidence.

`packages.windvale.ca` is the official logical discovery endpoint and immutable
GitHub Releases are the initial content archive. Neither is an artifact identity
or authority grant.

## Current reusable evidence

| Needed boundary | Current repository evidence | Readiness |
| --- | --- | --- |
| Source package and exact lock | Package 1, Lock 1, WVDB Query manifest and lock, canonical text reader, general manifest and lock readers, cross-file consistency and resource-admission cores, and a 58-case native package-format owner | All current lock resource kinds have general byte admission; the publication shell remains specialized to one digest-pinned package |
| Cross-component source build | Workspace 1 and Project 2 native front door | Implemented candidate |
| Compiler data and call support | Native `u64` values, nested records, byte concatenation, execution context 9, capability-provider table, and provider-call lowering | Enough language/backend surface for bounded package parsers and bundle geometry; the merged baseline still requires its dual-host qualification result |
| SHA-256 | `Foundation/Sha256.wv`, compression core, streaming core, native streaming evidence | Reusable, but the current portable streaming state is capped at 64 MiB and tracks a `u32` total |
| WVB admission | Native semantic verifier and read-only front door | Qualified normal path |
| Native target admission | WVO, PE/ELF hosted-container, console-application, and publisher admission paths | Reusable target-specific owners |
| Durable replacement | Portable WVB publication state machine plus native Windows/Linux sibling, flush, reread, replace, and directory-durability adapters | Strong reusable transaction model, not yet a general package-store interface |
| Immutable resource semantics | Foundation resource store and read-only directory snapshot library | Reusable for package resources and tests |
| Random-access storage semantics | `storage.random_access_v1`, execution context 9, ABI-23 provider calls, Windows/Linux storage leaves, and durable database recovery | Focused native provider exists; it owns one fixed database object and is not a package-store or general launcher binding |
| Hosted CLI basics | Process arguments, console/diagnostic output, whole-file read/write, native tool packaging | Enough for the first parser/verifier tools |
| Capability metadata | WVB capability catalog, root transitive approval, Package 1 complete capability closure | Requirements exist; typed installed approval objects and launch binding do not |
| Useful second package | Existing complete native `wvdump` / WVB inspector project | Ready to exercise a general Package 1 parser without creating a duplicate application |

The merged durable-database and compiler batch now provides valuable `u64`,
nested-record, byte-concatenation, provider-table, positioned-storage, flush,
writer-fencing, and recovery evidence. The package store should reuse those
semantic lessons and shared primitives, not depend on the WVDB format or make the
database a bootstrap prerequisite.

## Readiness decision

The repository has enough compiler and portable-library surface to implement the
offline package foundation now. Slice 1 began from the latest merged baseline
without waiting for the unrelated broad verification backlog. Slice 2 may follow
after the general Lock 1 and cross-file contracts are complete. Slice 3 may build
directly on the new `u64` lowering and random-access provider work, but still has
to implement the large streaming SHA-256 contract described below.

The repository does not yet have enough complete libraries and host services to
claim a working installer, online updater, or installed application launcher.
Slices 4 through 8 contain required implementation, not integration of facilities
that already exist. In particular, there is no release-signature verifier,
package-store mutation service, civil time, secure network client, or dynamic
capability-aware process launcher.

At the start of this implementation, GitHub Verify run
[`31828399859`](https://github.com/eworker-inc/Windvale/actions/runs/31828399859)
for merged commit `ce30152b` passed both native bootstrap jobs, both WebAssembly
jobs, and every completed Linux retirement shard, but failed the Windows shard-4
`wvo-export-renamer` owner. That failure does not identify a package-language
feature requirement and does not block focused development. It does block using
that commit as release or cross-host qualification evidence until corrected or
explained by a passing descendant.

## Verification policy while the broad gate is being repaired

Development, integration, and promotion are separate gates:

- A development slice runs its narrow native owner, malformed-input cases, and a
  reference-runtime differential when the ordinary native runner cannot execute
  the required profile. It may proceed while an unrelated broad owner is red.
- An integration gate runs the changed-file planner and the directly affected
  cross-area owners once a coherent slice is ready to merge. Any uncovered path
  gains an explicit native owner; it does not trigger an unfiltered managed
  fallback.
- A promotion or release claim requires the independent dual-host Qualification
  result from the exact source state, reproducible byte identities, and all named
  security and recovery evidence. No local focused result substitutes for it.

The first four implementation increments now own strict canonical package text,
general Package 1 manifest and Lock 1 readers, and their cross-file consistency
and locked-resource admission cores in portable Windvale source. Focused tests
compile with the native build driver, reject malformed and mismatched content,
verify the real WVDB Query pair and all nine of its locked inputs, lower through
the native backend, and are packaged as both Windows and Linux hosted executables.
Local execution is development evidence only; independent Linux execution and a
second real package remain in Slice 1.

## Confirmed missing boundaries

### General package and lock admission

The current package shell accepts only the exact checked-in WVDB Query manifest
and lock identities. Reusable strict text, Package 1 manifest, and Lock 1 readers
now exist together with cross-file consistency and locked-resource admission
cores. Their consumers check the actual WVDB Query pair and all nine locked input
resources. There is still no general resolver shell or second package, and the
publication command has not yet migrated from its digest-pinned specialization.

### Large streaming identity

Bundle 1 uses `u64` geometry and permits an object larger than the current 64 MiB
portable streaming SHA-256 ceiling. Before that bound is implemented, Windvale
needs a streaming SHA-256 state with checked `u64` total length and correct
SHA-256 64-bit bit-length padding. Tests must cross the old 64 MiB boundary without
materializing the complete input in a Windvale value.

### Signature verification

There is no Ed25519, EdDSA, SHA-512, public-key parser, threshold verifier, or key
rotation implementation. Release discovery cannot be promoted into a specification
until an independently reviewed verifier agrees with RFC 8032 vectors and rejects
noncanonical points, out-of-range scalars, malformed keys, duplicate signers, and
threshold failures.

Signing private keys are never needed by the installed client. Release signing is
an offline/controlled publishing operation with separately documented custody.

### Package-store host service

The ordinary native tool profile exposes six fixed services: process arguments,
console and diagnostic lines, whole-file read, and whole-file write. The new
execution-context/provider-table path can bind a focused
`storage.random_access_v1` instance, but its current native host owns one fixed
database object. Neither path provides anchored directory creation,
digest-derived exclusive object publication, existing-object verification,
activation replacement, directory durability, inventory, package transaction
locking, or package-store recovery observation.

Do not add a broad ambient filesystem API merely to implement the manager. Define
a rights-limited package-store host service whose requests name admitted digests,
store-relative object classes, bounded chunks, and exact transaction operations.
Windows and Linux adapters retain native handles and paths behind that boundary.

### Freshness and networking

There is no implemented civil-time capability, name resolver, HTTPS client, TLS
profile, certificate validation, redirect policy, or secure network provider in
Windvale source or the native hosted service set. Signed offline release discovery
must precede networking. Network `latest` behavior must not become the first parser
or trust oracle.

### Dynamic capability-aware launch

The general native WVB runner still supports a bounded portable `Main() -> i32`
profile and cannot dynamically bind `filesystem.directory_read_v1` or
`storage.random_access_v1`. The focused ABI-23 storage host proves one explicit
provider-table binding, but it is not a configurable product launcher. There is
no general installed command resolver, typed approval store, immutable launch
plan, dynamic clean spawn, or launcher-owned provider-table construction.

The first `wv run` implementation therefore needs a host launcher boundary in
addition to the package client. It must resolve an exact generation and bind only
approved provider instances; it cannot pass through the package manager's store,
network, signing, or administrative authority.

## Implementation slices

Each slice adds one permanent owner and one focused verifier. Do not implement all
formats in one command or hide semantic parsing in host scripts.

### Slice 1: general Package 1 and Lock 1 parser

Development prerequisite: a pinned merged baseline and passing focused native
package-format evidence. No additional compiler or database feature is required.
The independent dual-host Qualification result remains mandatory before the
slice is promoted as qualified or released.

Add capability-free portable parsing and verification cores that consume manifest
and lock bytes. They enforce the complete current grammar, bounds, ordering,
identifier/path rules, graph consistency, capability equality, exact locked byte
identities, and output selection. The core returns bounded structured evidence;
host shells only acquire files and render stable diagnostics.

Use two real packages:

- `windvale.wvdb-query`, retaining its multi-library graph and five capabilities;
- `windvale.wvb-inspector`, reusing
  `Projects/Examples/Windvale-Wvb-Inspector.wvproj` and the existing complete
  `Examples/Foundation/Wv-Dump-Core.wv` source rather than copying the inspector.

The second package exercises a distinct root, zero package dependency edges, five
hosted capabilities, a different filesystem boundary (`file.read_bytes` rather
than `filesystem.directory_read_v1`), and a separately useful installed tool. A
general resolver must accept both from their content rather than select them by
hard-coded digest.

Required cases include both valid packages, deterministic reports, empty and
maximum collections, malformed UTF-8, every record-order error, duplicate names
and paths, invalid identifiers, path escape, missing and extra graph edges,
unknown parts, capability mismatch, manifest/lock mismatch, resource identity
failure, trailing data, and existing-output preservation.

Exit gate: both packages build from exact locks on Windows and Linux, produce the
same WVB identities, and the changed-file planner selects a native package-format
owner with no uncovered specification path.

### Slice 2: Bundle 1 writer and read-only verifier

Decision 0561 implements the bounded in-memory portion of this slice. Distinct
Windvale-written writer and verifier tools produce and admit the exact WVDB Query
bundle, and the permanent owner pins its identity and malformed-input self-test.
The larger streaming boundary remains Slice 3 rather than weakening the 4 MiB
implementation policy.

Promote only the Bundle 1 header and index sections of the architecture into a
specification. Implement a deterministic writer over the two real package outputs
and a separate read-only verifier. Start with bounded in-memory fixtures while the
large streaming service is developed; this is test geometry, not a reduction of
the final Bundle 1 bound.

The verifier owns checked header arithmetic, index UTF-8 and ordering, complete
blob geometry, manifest/lock agreement, item-to-blob references, exact SHA-256,
and target-specific executable admission. It never extracts to native paths.

Exit gate: Windows and Linux produce byte-identical bundles and agree on valid,
boundary, truncated, oversized, overlapping, gapped, duplicate, corrupt-index,
corrupt-blob, wrong-target, and hostile-executable reports.

### Slice 3: streaming Bundle 1 admission

Add chunked read and SHA-256 with `u64` total geometry. The source core receives
bounded immutable chunks and explicit positions; the host adapter cannot choose
semantic offsets. Exercise chunk sizes one, 63, 64, 65, 3,072, and 65,536; short
nonterminal reads; old 64 MiB crossing; declared two-GiB rejection/acceptance
boundaries without allocating two GiB; cancellation; and exact end-of-file.

Exit gate: the streaming verifier produces the same report and identities as the
small in-memory oracle and never requires complete bundle bytes in one Windvale
`bytes` value.

### Slice 4: offline signature and release metadata

Implement SHA-512 and Ed25519 verification as capability-free portable code with
RFC 8032 test vectors plus an independent host-library differential oracle used
only for tests. Then implement Root 1, Signature 1, Channel 1, and Release 1
canonical parsers and threshold policy.

Use fixed test keys generated solely for public fixtures. Do not generate or store
production private keys in the repository. Cover initial root pinning, valid
old/new threshold rotation, missing signatures, duplicate signers, skipped
generation, expiry inputs through a deterministic test clock, channel rollback,
release mismatch, and signature domain separation.

Exit gate: one completely offline directory authenticates an exact Release and
the two package bundles on Windows and Linux without network or ambient trust.

### Slice 5: immutable local object store

Decision 0561 also implements the Milestone 2 publication subset: an already
admitted bundle publishes digest-derived blobs and the bundle through private
reread-verified files, rejects identity corruption, and proves idempotent repeat
publication. This is not yet the general semantic request/response service,
durable activation store, or crash matrix required to complete this full slice.

Define the semantic package-store request/response contract and paired host
adapters. Reuse the proven native publication transaction states: private sibling,
bounded writes, durability, exact reread, atomic publish, directory durability,
cleanup, and explicit indeterminate observation.

Publish digest-derived objects and bundle records only. Existing matching objects
are idempotent success; existing mismatches are corruption. Cover aliases, links,
device paths, case collisions, partial writes, out-of-space, flush failure,
replacement failure, concurrent readers, writer exclusion, crash-point recovery,
and no mutation before complete admission.

Exit gate: installing both offline bundles constructs the same logical store
inventory on Windows and Linux, with host paths absent from portable evidence.

### Slice 6: generations, approvals, activation, and rollback

Implement Generation 1 and Activation 1 parsing/construction. Introduce typed
approval objects one capability interface at a time, beginning with console,
diagnostic, process arguments, whole-file read for the inspector, and the
rights-limited read-only directory for WVDB Query.

Activation uses the shared durable transaction and verifies the published record.
Rollback increments the activation serial and selects an already admitted
generation; it does not lower the channel high-water mark or rewrite objects.

Exit gate: install, update, interrupted activation recovery, rollback, approval
change, denial, revocation, unavailable provider, corrupt generation, and
deterministic reachability pass on both hosts.

### Slice 7: native launcher and `wv` client bootstrap

Build one minimal platform launcher and one package-client application per host.
The launcher embeds or pins the initial Root and client identity, selects the
active client generation, and retains bounded previous-client recovery. The
client supplies:

```text
wv package inspect <bundle>
wv install --offline <release-directory> <package>
wv list
wv verify
wv generation list
wv generation inspect <digest>
wv rollback
wv run <command> -- <arguments>
```

Install per-user first. The host installer places the launcher, initial client
generation, trust root, official source configuration, PATH shim, and one
offline-verifiable base package set containing the compiler, assembler, linker,
runtime, and core inspectors. These core tools retain package identities even
when the installer embeds them for an offline first installation. WVDB Query,
database servers, and later applications remain separate optional packages or
projects.

Exit gate: a clean Windows and Linux user profile installs the bootstrap once,
installs both packages offline, runs the inspector and WVDB Query with exact
approved providers, updates the client, recovers a failed new client, rolls an
application generation back, and uninstalls without deleting separately owned
application data.

### Slice 8: official network source

Add qualified civil time, resolver, secure connection, HTTP retrieval, TLS trust,
bounded redirects, and streaming download behind explicit package-manager host
services. Deploy signed metadata at `packages.windvale.ca/v1`; publish exact
objects as immutable GitHub Release assets; allow the object endpoint to redirect
to those assets.

The client never calls GitHub `latest` to select identity. It first authenticates
Root, Channel, and Release metadata, then streams only declared object sizes and
digests. Cover DNS/TLS/HTTP failure, redirect loops and downgrade, truncation,
excess bytes, timeout, stale metadata, wrong content type where relevant, mirror
substitution, offline fallback, and source unavailability.

Exit gate: the same bootstrap performs a fresh online install and update from the
official source on Windows and Linux, while an offline archive reproduces the same
objects and generation without GitHub API access.

### Slice 9: SDK release and qualification

Package the compiler, assembler, linker, verifier, inspector, runner, and required
libraries as exact independently inspectable bundles plus an SDK root package.
Publish source, licenses, provenance, Stage 0 recovery evidence, and Windows/Linux
qualification reports in one signed Release.

Exit gate: the complete original objective and Windvale 0.1 package checklist are
audited requirement by requirement from one source state. Cross-host qualification
must prove every package identity, install/update/rollback behavior, capability
decision, bootstrap recovery path, and offline release reconstruction before the
package system is described as implemented.

## Work deliberately deferred beyond the first system

- Public search and third-party package registry.
- Background unattended updates.
- Delta bundles or compression.
- Arbitrary installation scripts.
- Machine-wide multi-user policy.
- Cross-device account synchronization.
- Automatic destructive garbage collection.
- Complex version-range solving.
- Windvale OS A/B system-slot updates.

These are not substitutes for any end-state requirement above. They remain later
features because the first official source can install exact pre-resolved packages
without them.
