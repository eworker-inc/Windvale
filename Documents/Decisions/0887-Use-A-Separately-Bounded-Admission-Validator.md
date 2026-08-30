# Decision 0887: use a separately bounded admission validator

## Status

Accepted on 2026-08-29 as the capacity-triggered continuation of Decision
0886. This decision publishes the WVAE 1.0 structure and digest foundation in
an independent admission-evidence validator leaf. The complete `wvauth`
validator, WVFC source authentication, and coordinator wiring remain later
implementation checkpoints.

## Context

Decision 0886 made complete analyzer WVIR capacity a precondition for placing
WVAE authentication in the analyzer. The required Windows capacity case used a
canonical 2,145,693-byte WVSS containing the complete analyzer closure plus the
candidate admission-evidence module. After approximately 106 seconds, source
symbols and bindings were both `Valid`, but typed lowering returned
`Evidenceˉlimit`. The 2,145,693-byte `Admitted.wvss` input already existed;
the phase published no successor `Source.wvss`, WVCA, WVLB, or WVIR.

That result cannot be answered by widening the retained 4,194,304-byte analyzer
WVIR limit or importing `Foundation/Sha256.wv`. Decision 0886 requires either a
separately bounded independent validator or a new evidence version. WVAE 1.0's
fixed binding contract remains sufficient, so a new version would add format
churn without solving the capacity boundary.

## Decision

1. Keep WVAE 1.0 unchanged and compile
   `Compilerˉadmissionˉevidence` only into the small independent
   `wvverify-admission-evidence` admission-evidence validator leaf. Reserve
   `wvauth` for the eventual complete validator. Do not import either new
   admission module into the complete analyzer closure.
2. Give this leaf fixed 262,144-byte WVIR and WVB development ceilings. These
   do not establish the eventual `wvauth` ceiling. Its exact inputs retain the
   ordinary 4,194,304-byte value limit, narrower WVAE, WVTD, lock, and profile
   bounds, and a checked 9,503,264-byte aggregate retained-input ceiling. The
   leaf reads WVAE, WVTD, lock, profile, WVSS, then WVFC, validates each value
   and cumulative total before the next read, and thereby bounds even the
   transient peak caused by a malformed maximum-size service response.
3. Require construction and authentication to compute all five digests through
   `Bytesˉsha256ˉhex`, decode exactly 64 lowercase ASCII hexadecimal bytes to
   32 raw bytes, and compare every digest. Foundation SHA-256, host hashing,
   producer-supplied hashes, and cache identities are not validator inputs.
4. Give `wvverify-admission-evidence` no output or certificate format. Its
   successful control-flow result is meaningful only while the coordinator
   retains the exact immutable private snapshots that were validated.
5. Keep the security boundary explicit: `wvadmit` owns private immutable lock,
   profile, WVSS, WVTD, WVFC, and WVAE snapshots; the independent validator
   reads those exact snapshots and ultimately authenticates both WVAE and the
   complete source/catalog evidence; only its success permits the coordinator
   to invoke the internal admitted analyzer mode on those same snapshots.
6. Forbid ambient rereads, direct public analyzer bypass, and a forgeable
   certificate or marker as a substitute for validation. The analyzer still
   independently validates WVSS, WVTD, and WVFC structures and proves semantic
   and source/catalog consistency.
7. Make the leaf validate WVTD and WVFC through their existing lightweight
   structural validators. Add a separate bounded WVSS 2 reader that checks
   magic, exact version 2.0, count, exact 20-byte entries, edition 1, English
   binding 1, origin bounds, nonempty sources, and canonical contiguous
   payloads without importing the heavyweight source-set parser closure.
   Derive counts only after those checks succeed.

## Initial bounded evidence

The seven-source standalone admission-evidence validator project contains the
hosted driver, portable format and post-read validator cores, small WVSS 2
reader, existing source-descriptor identity owner, and existing lightweight
WVTD and WVFC structural validators. On Windows it publishes:

- 106,530 WVSS bytes;
- 13,572 WVLB bytes;
- 191,712 WVIR bytes under the 262,144-byte ceiling, SHA-256
  `067b8bf2761b28189761f226b81cfd743b11ce1edc6a247fdf17a62edc1c3391`;
  and
- 72,600 WVB bytes under the 262,144-byte ceiling, SHA-256
  `706e34eb031be9c6d4695fe15cc7215d507e8768d44b5f5409813b58f6e46a59`.

The maintained portable core project separately builds a 24,292-byte WVB at
SHA-256
`5b76731abff311ff51dd2e302da8da7bfe8439250d5f32647bda5f0ee51f9537`.
The focused owner builds and pins this project; the historical complete
Analyzer capacity failure remains documentary evidence rather than an
unexecuted mapped project.

At the pre-Decision 0892 checkpoint, the separate 44-case fixture published a
94,299-byte WVB at SHA-256
`37772c1a75d03b2d8eb22015fde4efacbcc27718cd891f4486bc597317ebeee9`
and passed every isolated case with result 42. It covers exact construction,
shape and authentication success, rejected empty values, truncation, trailing
bytes, every fixed field, every digest mismatch, strict decoder failures,
determinism, accepted-byte preservation, and numeric boundary arithmetic
without oversized allocation. It additionally exercises canonical WVSS 2 and
malformed payload contiguity, invalid WVTD and WVFC structure, and exact
aggregate retained-input arithmetic. The same fixture covers exact portable
input-status/offset mappings for all six values and a structurally valid
WVFC/WVSS module-count mismatch at cross-field offset 24. Its in-range
malformed WVTD and WVFC cases run through the shared snapshot validator and
pin the delegated structural phase, status, and inner offset. After the direct
closure input changed, two current-compiler builds produce a byte-identical
94,839-byte fixture WVB at SHA-256
`5bc71d4c549da5a22a8210cd15fb01bfe19941d26db3cbb9f4b46be050b0ac7b`,
and the maintained verifier accepts it as compiler-aligned. The current-closure
selector execution is included in the final focused-owner evidence below.

On 2026-08-30, after the profile-7 segmented stager gained native SHA-256
lowering, the focused owner packaged the exact validator WVB through the
standard `Package-Segmented-Compiler-Wvb` product path and executed the
resulting `wvverify-admission-evidence` application. Twelve bounded actual-product
cases passed: one accepted six-snapshot set, exact missing- and surplus-argument
usage failures, truncated and trailing WVAE rejection, delegated WVSS, WVTD,
and WVFC structural rejection, WVFC/WVSS module-count mismatch, digest mismatch,
and empty lock and profile rejection. The existing 44 portable selectors and
15 WVSS structure subcases also passed, for 56 total cases. The owner retained
the exact 24,292-byte core WVB. The current compiler follow-up below supersedes
the WVIR and validator-WVB identities from that packaged run.

Each child has a bounded aggregate output capture and execution timeout;
product cases have a 30-second timeout, construction has a 10-minute timeout,
the complete owner has a 20-minute child-execution budget, and active children
emit 30-second heartbeats. Timeout or output-limit failure terminates the whole
Windows process tree or detached POSIX process group, uses a direct-child
fallback, and has a separate five-second forced-settle ceiling with an explicit
cleanup diagnostic. A child-plus-descendant termination probe runs before
product work. Generated snapshot writes and the packaged application have
explicit size ceilings, and the temporary directory is removed with bounded
retries and checked absent.
This checkpoint uses neither a source-built packaging workaround nor
`Foundation/Sha256.wv` or host hashing inside the validator, and it does not
complete Slice 8.

Decision 0892 later evolved the directly included target-descriptor source.
The retained analyzer independently reproduced the resulting 191,712-byte
WVIR twice with the exact identity above and valid WVCA count/length bindings.
The validator WVB therefore depends on the current reconstructed compiler
rather than the frozen `Build-Wvb` front door, which reports its total-function
sentinel after the evolved 75-function closure. Two current-compiler builds
produce byte-identical, verifier-accepted 72,600-byte WVB values at SHA-256
`706e34eb031be9c6d4695fe15cc7215d507e8768d44b5f5409813b58f6e46a59`.
The final focused Windows owner pins those current WVIR and validator-WVB
identities, the 24,292-byte core WVB at SHA-256
`5b76731abff311ff51dd2e302da8da7bfe8439250d5f32647bda5f0ee51f9537`,
and the 94,839-byte fixture identity above. It packages and executes the current
validator and reports exactly `PASS admission evidence cases=56
portable-selectors=44 dynamic-cases=12 wvss-structure-subcases=15
execution=native-packaged core-wvb-bytes=24292 wvir-bytes=191712
wvb-bytes=72600`. The focused suite completes in 137,850 ms and its owner
wrapper in 138,530 ms. This remains local Windows development evidence; it does
not add Linux or paired-host qualification, production coordinator or Analyzer
consumer integration, or broader Slice 8 completion.

At historical source commit `b93b88dab04ec5b95d9eca197e6ec49a8e841f06`,
the canonical analyzer manifest is 1,289 bytes at SHA-256
`a75810d22a74e602f7b9a2f10c1e08d23fd53d147f6c33f16107b686cb508745`.
Its driver remains 8,210 bytes at SHA-256
`7199e7c50e4d64230325c7125ed1f2cbfd0c3cc6ad8b4bd946c5c3df8c788849`.
Neither its exact project source list nor its driver imports either
admission-evidence module. The focused verification planner owns this exclusion;
the hashes record historical measurement only and are not active pins.

This is local Windows development evidence, not paired-host qualification.

## Consequences

WVAE can be published without consuming the analyzer's retained WVIR margin or
weakening its limit. The additional process boundary costs one cold start and
requires careful private-snapshot lifecycle control. The validator remains
small enough to gain independent hostile-input coverage and capacity evidence.

The current leaf authenticates the exact WVAE structure and five snapshots and
validates WVSS 2, WVTD, and WVFC structure. It does not yet authenticate source
syntax or spans, catalog completeness, target predicates, complete WVFC source
evidence, target admission, the `wvadmit` construction path, coordinator
lifecycle, internal analyzer invocation, cache migration, or public-front-door
removal. No such integration or qualification is claimed.

## Reconsideration triggers

Reconsider the product geometry if complete source/catalog authentication
approaches either 262,144-byte product ceiling, duplicates a complete producer
parser, or cannot retain bounded work and diagnostics. A later design must
choose a separately bounded segmented validator or a new evidence version; it
must not widen the analyzer, introduce a forgeable certificate, or fall back to
Foundation or host hashing.
