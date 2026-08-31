# Decision 0896: promote the qualified current split compiler as the two-file bootstrap

## Status

Accepted and implemented on 2026-08-31. The exact analyzer and emitter already
had fixed-point and paired-host front-door evidence. The active bootstrap
inventory now contains only those two products, and the rewritten direct
convergence path passes locally on Windows with an isolated empty cache.
Paired-host execution of the shortened path remains required before claiming
cross-host convergence for the implementing commit.

## Context

[Decision 0846](0846-Compact-Wvir-Operation-Records.md) retained one
digest-pinned bridge emitter so the Decision 0813 compiler could cross the
incompatible 32-byte to 28-byte WVIR operation-record transition. That bridge
was intentionally temporary: it contained only the current reader side and was
to be removed after a promoted compiler could consume current WVIR directly.

The current analyzer and emitter now satisfy that condition. They are the two
products of one compiler separated at the canonical source-analysis phase
artifact. Each product consumes current compact WVIR, has an exact role and
target identity, and reached a byte-for-byte fixed point without the bridge
being part of the resulting compiler pair. Retaining the earlier pair plus the
bridge would add three obsolete large inputs, extra host packaging, hashing,
and process launches to every cold proof without preserving a live semantic
capability.

The promoted bytes were constructed at commit
`c02e6bd47554242b3be0a5fcd16fe9c178ab4d2d`. Commit
`14fd50ea67fed612f7d8fd85b55ef4b9ead48658` then reproduced their exact
identities in all 482 Language 1.0 front-door cases on Windows and Linux. Those
jobs stopped only after the cases because a registry summary still expected a
473-byte fixture instead of the observed 531-byte fixture. Commit
`a32c9e4cd8231014f1698f558226bc95559a2aa1` corrected that unrelated expected
identity, and the following paired-host owner-stream run plus final gate passed.

## Decision

1. Promote exactly these two portable WVB products as the active bootstrap:

   | Product | Target | Bytes | SHA-256 |
   | --- | --- | ---: | --- |
   | analyzer | `source-analysis-v1` | 1,552,090 | `5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77` |
   | emitter | `portable-wvb-optimized-v1` | 1,556,434 | `d16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9` |

2. Advance the target-aware bootstrap manifest to version 4 and bind the exact
   source commit, root source/project identities, product sizes, digests, roles,
   and targets. Do not infer identity from file names or ambient executables.
3. Make the active bootstrap inventory contain only `wvanalyze.wvb` and
   `wvemit.wvb`. The Decision 0813 WVB pair, Decision 0846 bridge WVB, bridge
   patch, transition producer, and their cache keys remain recoverable from
   history but are not active inputs, fallbacks, or a second compiler.
4. Package the promoted analyzer and emitter for the current host, publish their
   version-2 producer identities, rebuild both current compiler halves, build
   the compiler-aligned verifier, independently admit both results, and require
   exact generation equality for the analyzer and emitter.
5. Keep the private empty cache, fixed product and diagnostic bounds, child
   timeouts, visible progress, exact producer rechecks, and guarded cleanup of
   the existing convergence proof. Removing the transition must not weaken any
   admission or fixed-point check.
6. Do not repin the immutable native Seed or managed recovery release. This is
   the evolving native compiler's bootstrap checkpoint, not a managed recovery
   change, release artifact, or new source-language definition.
7. Record the prior fixed-point and paired-host results as qualification of the
   promoted bytes. Run the rewritten two-file convergence path after the
   artifact replacement; do not reuse the former three-file path's result as if
   it had exercised the new bootstrap inventory.

## Evidence

The source commit's complete 18-phase convergence run reproduced Stage 2
analyzer and emitter bytes exactly. The analyzer compiled its exact
2,132,771-byte source set to 3,815,704 WVIR bytes, leaving 378,600 bytes under
the fixed 4 MiB immutable-value ceiling. Its resulting WVB is 1,552,090 bytes
at SHA-256
`5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77`.
The emitter is 1,556,434 bytes at SHA-256
`d16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9`.

GitHub run `33339426829` rebuilt and checked those exact products on Windows
and Linux as part of all 482 front-door cases. GitHub run `33340292739` then
passed the corrected four-case verification-owner stream on both hosts and the
final gate. Together they close the unrelated registry interruption and
qualify the pair on the `a32c9e4c` lineage.

On 2026-08-31, the rewritten 16-phase coordinator passed on Windows x64 with an
isolated empty cache over the version-4 manifest and exact two-file inventory.
The promoted inputs rebuilt a 1,573,433-byte current analyzer at SHA-256
`23d9ec0c223d214a69fcb4179abec5b3b9a6d579d8557f3ccf4248c2904267b6`
and a 1,575,647-byte current emitter at SHA-256
`0972defc2debdad47cd36268516c15d947a364b93aede84f0b55cf17ad061d77`.
The current analyzer compiled its 2,156,125-byte source set to 3,873,384 WVIR
bytes, leaving 320,920 bytes under the 4 MiB immutable-value ceiling. The
current emitter compiled its 2,098,697-byte source set to 4,096,784 WVIR bytes,
leaving 97,520 bytes. The same current pair rebuilt the exact
399,387-byte compiler-aligned verifier at SHA-256
`7da624b070b69c3a720a00df12b753ed28276b7909c48ec5e6c349bd15ed9800`.
That verifier independently admitted both Stage 2 products, and the final gate
proved exact Stage 1/Stage 2 byte equality for both compiler halves. This is
current-host evidence; the shortened path has not yet run on Linux for the
implementing commit.

## Consequences

- Cold convergence starts from two current products instead of two obsolete
  products plus a transition emitter.
- Bootstrap no longer packages, hashes, launches, or maintains a one-time WVIR
  bridge on every proof.
- Historical decisions keep their exact evidence without forcing active tools
  to carry obsolete formats or artifacts.
- Analyzer/emitter separation remains a phase and resource boundary within one
  compiler; this promotion does not introduce parallel compiler semantics.
- The bootstrap remains intentionally stronger and slower than the ordinary
  content-addressed development path.

## Reconsideration triggers

Reconsider this pair when a later accepted compiler checkpoint cannot consume
its emitted WVIR, when either product approaches a stable resource bound, when
the phase artifact changes incompatibly, or when a smaller independently
qualified pair preserves the same source semantics, producer identity,
verification, and fixed-point evidence. A future transition may use another
temporary bridge only through a new named decision with an explicit removal
condition.
