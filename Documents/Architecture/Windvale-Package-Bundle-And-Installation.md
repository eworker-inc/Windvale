# Windvale package bundle and installation architecture

## Status and relationship

This document defines the broader deterministic package bundle, immutable host
store, installation generation, activation, rollback, and capability-aware
launch transaction. Decision 0561 implements Bundle 1 and bounded immutable
publication for the exact WVDB Query and WVB Inspector Package 1 / Lock 1 applications.
Decisions 0562 and 0565 implement separate development/stable native-tool
installer artifacts with one payload-derived per-user generation. General activation, approvals,
rollback, self-update, repair, and garbage collection remain proposed.

The design composes with [release discovery 1](Windvale-Release-Discovery.md): a
signed Release names one exact bundle digest and size; bundle admission publishes
verified content objects; a generation selects exact admitted bundles and approval
objects; one small activation record selects a generation. These are separate
identities and transactions.

Bundle 1 uses `.wvbundle`. Generation and activation records are internal canonical
metadata rather than distributable packages. Promote these layouts into
`Specifications/` only with their native parser, verifier, malformed-input owner,
and paired Windows/Linux evidence.

## Bundle 1 goals

Bundle 1 carries already selected immutable bytes. It does not resolve version
ranges, run a build, execute installation scripts, grant capabilities, select a
newer dependency, or reinterpret a host path. Package 1 describes source parts and
Lock 1 pins the selected source graph and output; Bundle 1 transports the exact
manifest, lock, output, resources, licenses, and evidence required by one target.

The first bundle is uncompressed. This avoids decompression bombs, canonical
identity ambiguity, library dependencies in the bootstrap, and platform-varying
compression output. A later format may add an explicitly bounded compression
method without changing the identity of an uncompressed content object.

## Binary header

Every integer is unsigned little-endian. The fixed 128-byte header is:

| Offset | Size | Field | Bundle 1 value |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPB` |
| 4 | 2 | major version | `1` |
| 6 | 2 | minor version | `0` |
| 8 | 4 | header bytes | `128` |
| 12 | 4 | flags | `0` |
| 16 | 8 | total bytes | exact file length |
| 24 | 8 | index offset | `128` |
| 32 | 8 | index bytes | canonical index length |
| 40 | 8 | content offset | `128 + index bytes` |
| 48 | 8 | content bytes | exact remaining length |
| 56 | 4 | blob count | number of `blob` records |
| 60 | 4 | item count | number of `item` records |
| 64 | 32 | index SHA-256 | digest of exact index bytes |
| 96 | 32 | reserved | all zero |

There is no alignment padding between the header, index, and content. Checked
arithmetic proves every sum within the declared file length before any range is
used. The initial bounds are 1,048,576 index bytes, 4,096 blobs, 4,096 items, and
2,147,483,648 total bundle bytes. A bootstrap may impose a smaller policy limit
before retrieval, but it cannot silently accept a larger object than Bundle 1.

## Canonical bundle index

The index is strict UTF-8 without a byte-order mark, uses LF, ends with one LF,
and follows the Package 1 lexical rules. It begins with:

```text
windvale-bundle-index 1
```

The remaining records are:

```text
package <package-id> <version> <target-id>
manifest <sha256> <bytes>
lock <sha256> <bytes>
item <role> <item-id> <target-id> <sha256>
blob <sha256> <bytes> <content-relative-offset>
```

There is exactly one `package`, `manifest`, and `lock` record; one or more `item`
records; and one or more `blob` records. Items are ordered by role, item identifier,
target, and digest. Blobs are ordered by digest. Duplicate item tuples, blob
digests, or package identifiers are invalid.

The initial item roles are:

- `executable` for a WVB or admitted target-native application;
- `library` for an immutable reusable module;
- `resource` for immutable application data addressed by package resource name;
- `license` for package and dependency license material;
- `provenance` for bounded build evidence; and
- `qualification` for target qualification evidence.

Every manifest, lock, and item digest names exactly one blob. Unreferenced blobs
are invalid. Multiple items may reference one blob. Manifest and lock blobs may
also be items only when a later explicit role requires it; their fixed records are
not duplicated merely for enumeration.

Content contains each blob exactly once in digest order. The first offset is zero;
each following offset equals the checked end of the preceding blob; and the final
end equals `content bytes`. Gaps, overlaps, padding, aliases, out-of-order offsets,
and trailing data are invalid. Each blob's bytes and SHA-256 are verified while
streaming before it becomes publishable.

## Complete bundle admission

Admission is fail-closed and ordered:

1. Enforce the caller or Release size ceiling before allocation or download.
2. Read and validate the complete fixed header, versions, zero fields, and ranges.
3. Read the bounded index, verify its digest, UTF-8, grammar, ordering, and counts.
4. Prove complete contiguous blob geometry with checked arithmetic.
5. Stream every blob through SHA-256 and require exact length and end-of-file.
6. Parse the manifest and lock and require package, version, target, part graph,
   capability closure, license, and expected output to agree with the index.
7. Verify each WVB and independently admit each target-native container before it
   can be named executable.
8. Recompute the entire bundle digest and require the signed Release identity when
   the bundle came from an official source.
9. Publish content objects and the bundle record only after every check succeeds.

A valid bundle from an unsigned local source may enter a development store under
explicit local policy. It is not relabeled as an official release and cannot
inherit official trust or capability approval.

## Logical per-user store

The store has this logical structure:

```text
store/
  objects/sha256/ab/<remaining-digest>
  bundles/sha256/ab/<remaining-digest>.wvbundle
  generations/sha256/ab/<remaining-digest>/generation.wvgen
  approvals/sha256/ab/<remaining-digest>.wvapproval
  trust/
  channels/
  activation.wvactive
  transactions/
  locks/
```

The two-character fanout is lowercase hexadecimal. A path is derived only from a
validated digest, never from a package-supplied native path. The physical root is
selected by the host installer and passed explicitly to the launcher:

- Windows initially uses a per-user directory below the user's local application
  data location.
- Linux initially uses the user's XDG data location, falling back to the defined
  per-user data directory when XDG configuration is absent.
- Windvale OS later binds the same logical store to an explicitly authorized
  package-storage capability.

The package semantic contract does not expose these host paths. The launcher and
manager use handle-relative or otherwise anchored host operations, refuse path
escape and destination aliases, and do not follow package-controlled links.

## Immutable object publication

For every object or bundle, the manager:

1. creates an exclusive private sibling under the selected store transaction;
2. writes with exact partial-progress handling and a declared maximum;
3. flushes when the host durability contract requires it;
4. rereads the private object and verifies its exact length and digest;
5. atomically publishes it at the digest-derived destination; and
6. makes the containing directory durable when the host can provide that evidence.

If the destination already exists, the manager opens and verifies it. Matching
bytes make publication idempotently complete; a mismatch is store corruption and
never causes replacement under the same digest. Rejection removes only the private
candidate. Indeterminate completion triggers reread and recovery before any retry.

Objects are immutable after publication. Mutable application data, caches, logs,
approval policy, channel high-water marks, and activation state never live inside
content objects.

## Generation 1

A generation is selected by the SHA-256 digest of its exact canonical
`generation.wvgen` bytes. The text uses the common strict canonical rules and
begins:

```text
windvale-generation 1
```

The records are:

```text
source <source-id>
channel <channel-id>
release <sha256> <sequence>
target <target-id>
root <package-id>
bundle <package-id> <version> <sha256>
command <command-id> <package-id> <item-id> <entry-id>
approval <package-id> <capability-id> <approval-sha256>
```

There is one source, channel, release, target, and root. Bundle records are ordered
by package identifier and contain the complete resolved graph. Command records are
ordered by command identifier and map a stable command to one exact executable
item and declared entry. Approval records are ordered by package and capability
and reference immutable approval objects. Duplicate commands, bundles, or approval
tuples are invalid.

The generation contains no timestamp, host path, random identifier, or self-digest.
The same selection and approvals therefore produce the same generation bytes.
Creation time and human annotations may be stored as non-authoritative local audit
records outside the generation.

Before generation publication, the manager proves that every bundle, object,
command item, lock, target, capability requirement, approval, and provider
expectation is reachable and mutually consistent. A package signature or bundle
entry is never substituted for an approval.

## Approval objects

An approval object is immutable canonical policy selected by the application owner
or administrator. It binds:

- package and capability identities;
- exact semantic interface major version;
- allowed provider class;
- rights reduction such as a selected directory, endpoint, device, or operation;
- installation or user scope;
- optional expiry or revocation generation; and
- policy schema version.

Approval syntax remains a separate capability-policy contract because each
interface needs typed rights rather than one loosely shaped permission string.
Generation 1 records only its digest. Changing an approval creates a new object and
new generation; it does not rewrite an installed package or existing generation.

## Activation 1 and rollback

The one mutable selection file begins:

```text
windvale-activation 1
serial <positive-u64>
current <generation-sha256>
rollback <generation-sha256|none>
```

An activation candidate increments the serial exactly once, selects a fully
admitted current generation, and retains the previously current generation as the
rollback target unless policy forbids it. The manager writes, flushes, rereads,
and atomically replaces the activation file through the repository's existing
durable publication pattern. It then rereads the published record and reports the
observed result.

Rejection preserves the old activation. Indeterminate replacement is not reported
as success: recovery rereads the activation, validates its serial and generation,
and determines whether the old or new selection became visible. Retrying an
unknown mutation without that observation is invalid.

Rollback creates another activation record with a higher serial and the admitted
rollback generation as current. It never modifies generations or decrements a
signed channel high-water mark. A signed minimum-version or revoked-generation
policy may reject rollback to a known-vulnerable generation, but that policy needs
its own trustworthy persistent-state and recovery evidence.

## Concurrency and readers

One bounded exclusive manager transaction may mutate store metadata at a time.
Acquisition has an explicit timeout and a stale-owner recovery rule based on
host-observed process identity or a qualified lease; a filename containing an
unverified process number is not sufficient evidence to break a lock.

Launchers do not take the writer lock for ordinary execution. They read one
complete activation snapshot, validate the selected immutable generation, and
retain handles or immutable identities for everything needed by the launch. A
concurrent activation creates a new selection for later launches and cannot swap
bytes under an admitted running process.

## Capability-aware launch

The stable host command is initially `wv`. Additional command shims may contain
only a command identity and delegate resolution to the active client; they never
point directly to a mutable package directory.

For `wv run` or a command shim, the client:

1. reads and admits one activation snapshot;
2. resolves the command through its exact Generation 1 record;
3. opens the exact bundle/object identities and repeats executable admission;
4. compares the package's complete required capability closure with the generation
   approval objects;
5. asks the host adapter for rights-reduced provider instances matching those
   approvals;
6. constructs and validates the immutable launch plan completely;
7. creates a non-running process, binds only those providers and selected streams;
8. publishes the process as runnable only after every binding succeeds; and
9. reports launch failure separately from the application's result.

There is no ambient package `PATH` scan, implicit current-directory execution,
authority inheritance from `wv`, or network grant because bytes came from a
network source. Unsupported, unavailable, revoked, stale, and denied capabilities
remain distinct failures.

## Bootstrap and self-update

The one-time host installer contains:

- a small native `wv` launcher whose only normal job is selecting and starting the
  active verified client;
- one initial platform-specific `wv` client object and generation;
- the initial trusted Root 1 and official source configuration; and
- the empty or initial package-store metadata needed for recovery.

The compiler, runtime, assembler, linker, and applications are ordinary bundles.
The launcher is stable enough to recover a previous client but does not contain
package resolution, network discovery, or application semantics.

Development installer 1 is a deliberate precursor, not this complete bootstrap:
it packages exact retained native tools directly, uses platform scripts and fixed
shims instead of the future native recovery launcher, and owns only clean install,
idempotent reuse, tamper detection, and uninstall of one recorded generation.

Client self-update constructs a new client generation through the normal object
and generation transaction, exits the old client, and asks the launcher or a
narrow replacement helper to activate it. Windows never overwrites the running
executable. A failed new client start leaves or restores the previous client
generation through a bounded attempt record; application generations remain
separate from client recovery.

## Inspection, repair, and garbage collection

The initial manager exposes read-only inspection before destructive maintenance:

```text
wv package inspect <bundle>
wv verify
wv generation list
wv generation inspect <digest>
wv rollback
wv store inventory
wv store gc --dry-run
```

`verify` checks activation, selected and rollback generations, approvals, bundles,
objects, executables, trust metadata, and channel state. Repair never invents an
identity or silently redownloads a mutation; it reports the exact missing or
corrupt object and requires an admitted source or offline object.

Garbage collection is deferred until reachability is qualified. Its roots include
current, rollback, recovery-client, explicitly pinned, audit-retained, and active
transaction generations. A dry-run inventory and deterministic reachability proof
precede deletion. Ordinary uninstall initially removes only a root from a new
generation and leaves unreachable content for later authorized collection.

## Recoverable bounded uninstall

The bounded offline installation owns only `state`, `generations`, and `store`
beneath an existing ordinary installation root. The host uninstall adapter
validates the exact Activation 1, Generation 1, and SHA-256 store layouts before
mutation; links, unknown entries, malformed transaction state, and policy-bound
excess are rejected without removal. Interrupted activation or generation
candidates must be resolved by their owning recovery adapters first.

The adapter durably records `.windvale-uninstall-1/Uninstall-1.txt`, atomically
quarantines `state` first so later command resolution cannot start, then
quarantines `generations` and `store`. It revalidates and removes only those
quarantined owned trees. The installation root, application data, and unrelated
sibling files are never removal targets.

Recovery completes a canonical recorded transaction rather than guessing or
reactivating it. An empty transaction directory is an uncommitted attempt and is
removed without changing owned state. Malformed or ambiguous transactions are
preserved for inspection. A completed uninstall is idempotent.

## Required evidence

Bundle qualification requires valid, boundary, truncated, oversized,
noncanonical, duplicate, overlapping, gapped, aliased, corrupt-index,
corrupt-content, wrong-target, mismatched-manifest/lock, and hostile executable
cases. Exact bundle bytes and reports must agree on Windows and Linux.

Store and generation qualification requires interrupted writes at every mutation
stage, existing-object verification, corrupt-existing-object refusal, concurrent
read/activation, writer exclusion, stale-lock recovery, out-of-space behavior,
activation rejection preservation, old/new indeterminate observation, rollback,
minimum-version refusal, offline recovery, and deterministic reachability.

Launch qualification requires exact command resolution, capability closure,
approval mismatch, denial, unavailable and revoked providers, stale handles,
provider restart, partial launch construction, no-authority inheritance, and
matching application results on both hosts. Bootstrap qualification additionally
requires clean first installation, self-update, failed-start recovery, interrupted
activation, and removal without deleting separately owned mutable application data.
