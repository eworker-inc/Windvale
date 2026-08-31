# Decision 0893: authenticate production source-analysis ingress

## Status

Proposed implementation checkpoint on 2026-08-30. The target-aware hosted
admitter, complete independent authenticator, Analyzer handoff, host
coordinator, and focused malformed-input owner now have a local Windows
implementation result and fixed candidate identities. The shared Language 1.0
front door now passes on Windows and Linux, but the 21-case production-ingress
owner still lacks the corresponding Linux result. Decision 0893 therefore
remains Proposed.

## Context

[Decision 0892](0892-Coordinate-Authenticated-Source-Admission.md) composes
source-profile admission, exact target admission, canonical foreign-catalog
production, public readback, and WVAE construction in one portable in-memory
function. [Decision 0887](0887-Use-A-Separately-Bounded-Admission-Validator.md)
also supplies a packaged leaf that validates the six value shapes, module
counts, and WVAE digests. Neither checkpoint connects the real hosted
Language 1.0 compiler path.

Before this checkpoint, the production runner invoked a legacy profile-only
`wvadmit`, passed one WVSS path through public Analyzer option
`--admitted-source-set`, rewrote that WVSS in place, and then invoked the
emitter. The validator did not independently prove that WVFC was complete for
the admitted source. A path, cache key, executable identity, exit-code file,
secret command argument, or generated marker would be a transferable and
forgeable claim; it cannot close that gap.

The permanent hosted argument table admits at most 67 values. Passing four
products, admission metadata, up to 64 source paths, and four output paths in
one child invocation would silently reduce the language's 64-module bound.
The host therefore needs one canonical private source-set snapshot rather than
a smaller positional source limit.

## Decision

1. Make one host orchestration command the public Language 1.0 compilation
   boundary. It snapshots the ordered source closure, source-input lock,
   selected profile, and WVTD once, then retains every successor value in one
   exact private candidate directory until emission completes.
2. Construct canonical WVSS 1 from the private ordered source bytes in the
   host coordinator. The hosted `wvadmit` accepts that one WVSS path together
   with the lock and supplied lowercase digest, profile, and mandatory WVTD.
   This preserves all 64 modules within the 67-argument host contract.
3. The hosted `wvadmit` calls
   `Compilerˉsourceˉadmissionˉcoordinate` and publishes private WVSS 2,
   unchanged WVTD, canonical WVFC, and WVAE only after one all-or-nothing
   portable success. It writes WVAE last. The hosted file capability has no
   catchable transactional write or delete result; the surrounding private
   coordinator therefore owns cleanup after a trapped partial write.
4. Complete `wvauth` as a separately bounded six-input product. It first
   retains WVAE, WVSS, WVTD, WVFC, lock, and profile under their existing
   limits. A dedicated authentication composition calls the small
   admission-evidence structural/digest validator first, without enlarging
   that leaf's fixed capacity owner, and then independently scans admitted
   source and consumes WVFC record by record
   to prove exact declaration order, completeness, spans, `System` and
   `unsafe`, ABI, symbol, signature, `effects(ffi.call)`, signature digest, and
   exact target predicates.
5. `wvauth` must not call the canonical WVFC producer, materialize a second
   complete catalog merely to compare it, or publish a certificate, marker,
   rewritten catalog, cache value, or successor artifact. Success is control
   flow while the coordinator retains the same six immutable snapshots.
6. Replace the public Analyzer option with one explicitly non-authoritative
   coordinator-internal source-set mode. It accepts one WVSS input followed by
   distinct WVSS, WVCA, WVLB, and WVIR output paths. The Analyzer treats the
   input as untrusted source and republishes the exact bytes it consumed.
7. Do not import admission-evidence construction or validation, source-target
   admission, or the canonical catalog producer into the complete Analyzer.
   Complete `wvauth` is the independent source/catalog/target authenticator;
   the private host retains and rechecks its six exact snapshots. The host
   compares the Analyzer's republished WVSS byte for byte with the retained
   authenticated WVSS before the emitter independently validates that original
   WVSS against WVCA, WVLB, and WVIR. Neither the internal option nor a direct
   Analyzer/emitter artifact carries admission authority.
8. An authenticated empty catalog proceeds into the existing analysis core and
   must preserve the same WVCA, WVLB, and WVIR bytes. Until foreign binding and
   lowering land, the coordinator reports one named
   `Foreignˉsemanticsˉpending` rejection for a fully consistent nonempty catalog
   and launches neither Analyzer nor emitter.
9. Retain descriptorless Project 2 only as a development path. Before building
   WVSS 1, its Analyzer route rejects a `System` source profile, every platform
   declaration, and every foreign token. It cannot be used as a Language 1.0
   package, release, or conformance front door.
10. Only successful `wvauth` plus unchanged retained snapshots permits the host
    to invoke the internal Analyzer mode. The Analyzer's WVSS result must equal
    the retained authenticated value before emission. Validator failure must
    prevent Analyzer launch; Analyzer failure or source-set mismatch must
    prevent emitter launch; every failure must preserve any competing
    destination and remove only the exact locally created private candidate.
11. Bound each child to 300 seconds and 65,536 aggregate diagnostic bytes.
    Emit stable activity at most every 30 seconds. On timeout or output
    overflow, terminate the complete descendant process tree and require a
    bounded five-second settle before cleanup.
12. Copy and sync the completed private WVB into a uniquely named candidate in
    the destination directory, then create the public path with an atomic
    no-overwrite hard link. A lost publication race preserves the winner. The
    coordinator removes its exact candidate and private directory in every
    terminal path.
13. Retain WVSS 2, WVTD 1.0, WVFC 1.0, WVAE 1.0, WVCA 1.0, and the current
    WVLB, WVIR, and WVB versions. Advance the hosted admitter, authenticator,
    Analyzer, orchestration, producer-target, and cold-cache identities without
    inventing a serialized authentication format.
14. Add a new focused `language-1-production-admission-ingress` owner rather
    than enlarging the portable coordinator, packaged validator, broad
    language front door, or compiler-split owners. It packages the actual
    successor products and proves production sequencing, bypass rejection,
    tamper rejection, destination preservation, cleanup, process-tree
    termination, progress, and deterministic accepted output.

## Resource geometry

The immutable six-input limits remain:

- WVAE: exactly 224 bytes;
- WVSS: 37 through 4,194,304 bytes and 1 through 64 modules;
- WVTD: 64 through 320 bytes;
- WVFC: 48 through 4,194,304 bytes and at most 43,690 records;
- source-input lock: 1 through 1,048,576 bytes;
- selected profile: 1 through 65,536 bytes; and
- all six inputs: at most 9,503,264 bytes.

The host rejects source payload accumulation as soon as it would exceed the
4 MiB WVSS 1 value after its exact 16-byte header and eight bytes per module.
`wvauth` and the complete Analyzer must separately publish observed product,
elapsed-time, and sampled peak-working-set bounds; the immutable-input total is
not a process-memory claim.

Under the pinned bootstrap transition, the complete pre-ingress Analyzer
closure publishes 4,181,228 WVIR bytes, leaving 13,076 bytes under Windvale
1.0's fixed 4 MiB immutable-`bytes` limit. Adding the foreign-catalog and target
contracts costs 79,212 WVIR bytes; the first duplicated admission adapter costs
another 48,860 bytes; even a geometry-only six-input adapter costs 9,748 bytes.
The selected ordinary WVSS handoff plus Project 2 prechecks publishes 4,182,928
bytes and leaves 11,376 bytes in that transition.

One packaged current Analyzer has since compiled its exact 2,132,771-byte
source set to 3,815,704 WVIR bytes, leaving 378,600 bytes. The complete
18-phase current split-compiler convergence check reproduced its stage-2 bytes
exactly. The resulting Analyzer WVB is 1,552,090 bytes at SHA-256
`5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77`;
the converged emitter WVB is 1,556,434 bytes at SHA-256
`d16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9`.
The same focused production owner pins `wvadmit` at 572,926 bytes and SHA-256
`a9c2e966b84420aaa64de89a232246a15b8fb859ba5ef737e853d2482d5f5831`
and the separately composed `wvauth` at 91,774 bytes and SHA-256
`88eec2e572e03cdd87de3bedc01c555da3a246fd2d160a62246da0d39331f580`.
The bootstrap result still rejects duplicate ingress closure in the Analyzer,
but it is a transition constraint rather than the measured margin of the
current-compiler run. Later growth must still remain measured under the
unchanged immutable-value limit.

An isolated local Windows observation packaged the 91,774-byte `wvauth` WVB
at SHA-256
`88eec2e572e03cdd87de3bedc01c555da3a246fd2d160a62246da0d39331f580`
as a 968,192-byte application at SHA-256
`52eb2a20a946f63de0b2837b9c138e971b4322d9e4e4fedf4abbcf0286ce007d`
through the standard profile-7 segmented package path.
The exact Analyzer source closure still uses legacy combined module/profile
headers and therefore cannot enter Language 1.0 admission unchanged. A current
23-module reconstruction starts with the exact 2,132,771-byte Analyzer source
set, adds the source descriptor, and separates each legacy header into `module`,
`profile core`, `platform linux`, and `authority application` clauses while
preserving every post-header source byte. The resulting migration-scale WVSS 1
is 2,133,877 bytes; its canonical admitted WVSS 2 is 2,133,900 bytes. Together
with the 224-byte WVAE, 64-byte WVTD, 48-byte empty WVFC, 760-byte lock, and
508-byte profile, `wvauth` consumed 2,135,504 immutable input bytes. Two direct
runs accepted the exact six snapshots in 697.678 ms and 705.534 ms. Sampled
peak working sets were 137,244,672 and 9,924,608 bytes. The first observation
records a real late-run transient after remaining near 10 MB for its first 501
milliseconds; a separately labelled diagnostic run completed in 668.716 ms at
9,977,856 bytes and corroborated the second memory observation. Every run
produced the exact 66-byte accepted report and no diagnostics under a
one-millisecond sampling interval, 300-second timeout, and 65,536-byte output
limit. This is an observed migration-scale result, not a byte-identical
current-source result or a hard process ceiling.

## Required evidence

Local Windows evidence now includes deterministic double builds and pinned
identities for the hosted admitter, complete `wvauth`, successor Analyzer, and
emitter; execution of the actual packaged products; the focused 21-case
production owner; and current split-compiler convergence. Changed-file and
registry planner checks remain part of the checkpoint gate; the current planner
passes 31 general and 247 native routing cases. At commit `14fd50ea`, GitHub
run `33339426829` passed all 482 `language-1-front-door` cases on Windows and
Linux before a stale expected fixture-size summary stopped both jobs. Commit
`a32c9e4c` repaired that owner-registry identity, and run `33340292739` passed
the four-case `verification-owner-stream` on both hosts plus the final gate.
Those runs prove the shared front-door and registry repair, but neither ran the
21-case `language-1-production-admission-ingress` owner on Linux. That exact
corresponding report is still required before a paired production-ingress
claim or acceptance.

The owner must include valid empty-catalog compilation, a valid nonempty
catalog reaching the staged semantic boundary, legacy Analyzer bypass
rejection, raw Project 2 System/platform/foreign rejection, missing or mixed
snapshots, evidence and catalog tampering, rehashed source/catalog mismatch,
target mismatch, zero/65-module cardinality, exact 64-module and canonical-WVSS
maximum acceptance, malformed digest, exact lock/profile/target maxima, empty
and one-past snapshot bounds, duplicate paths, hard-link aliases, per-file and
aggregate-WVSS pre-scan rejection, output alias and pre-existing destination rejection,
validator-before-Analyzer and Analyzer-before-emitter sentinels, destination
preservation, exact private cleanup, descendant timeout termination,
aggregate-output termination, and bounded heartbeat evidence.

## Consequences

Admission becomes a retained process relationship rather than a claim carried
by a file name or token. Direct invocation of an internal compiler product may
still create untrusted intermediate bytes, but it cannot use the production
coordinator to publish a final WVB without executing the complete validation
sequence on the same private snapshots.

The host constructs WVSS 1 once because the argument-table limit is durable.
This is not a second parser or a source-authentication shortcut: the source
lock, profile admission, WVSS validation, independent `wvauth`, ordinary
Analyzer source semantics, exact WVSS comparison, and emitter validation remain
authoritative at their own boundaries.

## Nonclaims

This checkpoint does not bind foreign declarations into the symbol graph,
normalize callable facts, lower foreign pointer/region/scratch semantics,
change WVLB or WVIR, emit WVB foreign imports, perform a native ABI call,
authorize dynamic symbol lookup, contain a hostile provider, complete Slice 8,
or qualify the whole Language 1.0 compiler.

## Reconsideration triggers

Reconsider if the permanent host argument contract grows; if a rights-limited
inherited capability can replace path-based child access without becoming a
transferable token; if hard-link publication is unavailable on a required
filesystem; if independent linear authentication approaches its measured
resource ceiling; if the Analyzer cannot retain adequate compiler-growth
headroom under the immutable-value contract; or if Windows and Linux differ in
snapshot bytes, status, offset, cleanup, or publication behavior.
