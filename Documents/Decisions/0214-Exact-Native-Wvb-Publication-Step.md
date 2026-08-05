# Decision 0214: Exact native WVB publication step

- Date: 2026-08-04
- Status: Accepted; first native publisher profile cross-host qualified; extended fault/concurrency matrix remains
- Advances: [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md) and Phase 10
- Builds on: [Decision 0185](0185-Standalone-Compiler-Wvb-Verifier-Applications.md), [Decision 0186](0186-First-Windvale-Native-Compiler-Build-Driver.md), and [Decision 0187](0187-Project-Aware-Windvale-Native-Build-Driver.md)

## Context

The qualified native build driver compiles source or Project 1 input, admits the
candidate through the shared Windvale verifier, and invokes `file.write_bytes`
only after acceptance. That capability is deliberately durable and whole-value
but non-atomic. Giving it stronger behavior only in the driver would use one
operation name for two different mutation contracts.

The ordinary native front door needs an exact replacement boundary before it can
publish directly to the requested build output. A script that runs the verifier
and then invokes an ambient move command leaves a changeable-file gap between
admission and publication, inherits undocumented host-command semantics, and
cannot distinguish a rejected mutation from indeterminate completion.

Adding a general filesystem capability, process launcher, or new WVB operation
would be broader than this immediate need. The portable compiler-aligned verifier
already exists and must remain the only semantic admission implementation.

## Decision

### Use a separate fixed native publisher

Add one fixed native tool with this conceptual invocation:

```text
wvpublish <candidate.wvb> <destination.wvb>
```

The candidate is caller-owned input. The publisher reads it into one bounded
immutable snapshot, admits that snapshot through the shared portable
`Compilerˉwvbˉverify(bytes) -> u32` implementation, and publishes exactly those
admitted bytes. The destination is never used as compiler scratch space.

The publisher is a native Windows/Linux tool profile, not a new source-visible
filesystem capability. Its portable admission and transcript logic is Windvale
code. Only native resource identity, exclusive sibling creation, durable writes,
atomic replacement, and directory-entry durability belong to the narrow host
adapters. Stage 0 may construct and independently verify the first packages, but
the resulting raw tools must execute without loading .NET.

### Own the complete replacement transaction

For one invocation, the publisher:

1. validates the two-argument shape and exact `.wvb` suffixes;
2. opens and snapshots at most 4 MiB from the candidate without following a
   publisher-created output alias;
3. rejects malformed or compiler-incompatible WVB through the shared verifier;
4. opens the destination directory as the anchored publication domain and rejects
   candidate/destination identity equality when the destination exists;
5. creates one unique sibling exclusively in that directory, never opening an
   attacker-selected pre-existing sibling;
6. writes and durably flushes the complete admitted snapshot;
7. rereads or independently measures the sibling and requires exact length and
   SHA-256 agreement with the admitted snapshot;
8. performs one same-directory atomic replacement of the requested destination;
9. completes the platform's required directory-entry durability step; and
10. reports the admitted byte count and SHA-256 only after complete success.

The caller-owned candidate remains after success or failure. The publisher-owned
sibling is removed after any failure known to precede replacement. The publisher
does not create the destination's parent directory, discover projects, compile
source, lower native code, or accept output larger than the ordinary WVB bound.

### Distinguish mutation outcomes

The public result separates these states:

| Outcome | Destination guarantee | Retry rule |
| --- | --- | --- |
| Invocation or candidate rejected | Unchanged | Correct input before retry. |
| Publication rejected before replacement | Unchanged | Safe to retry after the reported provider condition changes. |
| Complete | Contains the exact admitted snapshot and the required durability step completed | No retry is needed. |
| Indeterminate completion | Replacement may have occurred, but completion or durability could not be confirmed | Do not replay blindly; inspect the destination identity and digest first. |

No error reported after the replacement boundary may be recategorized as a known
unchanged failure. An implementation that cannot provide the required atomic or
durability evidence on a selected provider must reject that provider before
replacement.

### Keep orchestration explicit

The first ordinary source-to-WVB workflow is:

```text
native build driver -> caller-owned candidate WVB
native publisher    -> independently admitted atomic destination replacement
```

The build driver retains its in-process admission so it never emits a knowingly
invalid candidate. The publisher repeats admission over the actual immutable bytes
it will publish. This is intentional boundary defense, not a second verifier
implementation. A later integrated front door may pass an immutable candidate
directly to the same publisher contract without changing source or WVB semantics.

## Required evidence

Qualification must cover both Windows and Linux and include:

- exact native publisher packages, manifests, target identities, SHA-256 digests,
  source inventory, and reconstruction provenance;
- direct raw-tool execution with no CLR/.NET host or runtime mapping;
- missing, empty, oversized, malformed, truncated, extended, and verifier-rejected
  candidates with an absent and an existing destination;
- candidate/destination equality through the ordinary exact path plus native file
  identity where the platform exposes aliases;
- exclusive sibling collision, write, flush, reread/digest, replacement, and
  directory-durability fault injection at every boundary;
- proof that every known pre-replacement failure preserves an existing destination
  and removes only publisher-owned scratch;
- proof that post-replacement uncertainty reports indeterminate completion rather
  than claiming rollback;
- concurrent readers observing only the complete old or complete new value; and
- identical semantic transcripts and published bytes across both hosts.

Tests should consume shared versioned fixtures and fault plans. Platform-specific
tests own only their adapter evidence; they must not duplicate verifier cases or
portable transaction-state logic.

## Implemented first slice

`Tools/Windvale.Publish/Wvb-Publication-Transaction.wv` now owns the portable
transaction state machine. It distinguishes active progress, cleanup required,
known unchanged rejection, completion, indeterminate completion, and invalid
transitions. A failure before sibling creation is known unchanged; failures while
the sibling is owned require confirmed cleanup before rejection; a failure after
replacement is indeterminate and cannot return to an unchanged state.

The 4,560-byte portable WVB has SHA-256
`6c579d06e481ff5a2cde04463ccc84e78c458eea2c7865bf8797f22136c11a52`.
The separate fixture now composes that core and the native-call bridge into a
13,617-byte executable module with SHA-256
`a9c356ba0bcbd61fd6bac7afd40c10e752f3eedad729077d5abdc5518ae188a4` and
exercises the complete success path, each pre-replacement failure and cleanup,
post-replacement uncertainty, invalid transition preservation, and bridge result
encoding. This evidence implements shared policy only; it does not by itself claim
native file identity, replacement, durability, raw publisher packages, or
cross-host qualification.

## Implemented native-adapter slice

The publisher front door now composes the compiler-aligned verifier and portable
transaction core into one 136,698-byte hosted WVB with SHA-256
`d8fcbebe7915542b0206900bcce5459957cee768470bf64a2999e6ee688af05d`.
Small fixed entry objects transfer control to separately owned Windows and Linux
publication adapters. Both adapters use one capability-free shared x64 SHA-256
object, preserve the admitted candidate through an already-open native identity,
and submit transaction milestones only after the corresponding host operation is
confirmed.

The deterministic candidate object and package identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Linux entry WVO | 164 | `eee997412ced0d7edacaf39dae9c4a3c51e859dce4537045f3972be990b115a4` |
| Linux publication-adapter WVO | 5,507 | `9272c17b0d7234218a6cd7c31131e9d25e62b6c1ccd976d94975e9b436b2ca5a` |
| Windows entry WVO | 168 | `bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367` |
| Windows publication-adapter WVO | 9,544 | `ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93` |
| Shared x64 SHA-256/report WVO | 2,176 | `380af02cf29f85be1f63a4ea1f02ca3cc027e63091659e214a023b03730f6608` |
| Linux raw publisher package | 1,119,173 | `71dccc29333b05cff71e4b36e5e41617e0df4f8d747747479e8a27f4a90ed3b0` |
| Windows raw publisher package | 1,121,792 | `f2502ecf9143cfa1343c5f5cb1de066bdf1f82f0e4782afae178f11c41afd735` |

The publisher slice is cross-host qualified at exact commit
`9d36387867ebff80ee94c6f9f7996da4ef32a4a3` in GitHub
[Verify run 30971408639](https://github.com/eworker-inc/Windvale/actions/runs/30971408639).
Both permanent hosts directly execute their raw publisher without loading the CLR,
replace an existing destination, report the exact admitted byte count and SHA-256
only after durability, preserve the destination after semantic rejection, reject a
hard-link alias by native identity, and leave no publisher scratch. The shared
portable transaction fixture covers every state transition, including cleanup and
post-replacement uncertainty. The paired jobs agree on the deterministic WVB, WVO,
PE, and ELF identities above.

This qualifies the first real Windows/Linux publisher profile and its exact artifact
identities. It does not claim native fault injection at every host boundary or a
concurrent-reader stress schedule; those remain extended hardening evidence.
Distribution of the pinned packages and promotion of the composed ordinary workflow
are owned by the following Decision 0213 cutover slice and require their own
committed dual-host evidence.

## Consequences

- `file.write_bytes` keeps its existing durable, non-atomic contract.
- The normal native front door gains one narrow publication dependency rather than
  a general filesystem or process-control API.
- The extra verification pass closes the mutable candidate-to-destination gap and
  verifies the actual bytes selected for publication.
- Host-specific code is limited to unavoidable resource and replacement mechanics;
  compiler, verifier, transcript, and transaction-state behavior remain shared.
- This step advances ordinary source-to-verified-WVB cutover but does not transfer
  the complete native backend, test runner, packaging suite, or final recovery
  archive and does not complete Decision 0057.

## Reconsideration triggers

Reconsider this fixed tool when:

- a versioned rights-limited filesystem capability provides the same exact
  snapshot, identity, replacement, durability, and indeterminate-result contract;
- an integrated native build owner can pass an immutable admitted byte value into
  the same transaction without a caller-owned candidate file;
- either permanent host cannot supply the required atomic replacement or
  directory-durability evidence; or
- publisher packaging would require a new general runtime or parallel verifier.
