# Decision 0585: Catchable model-provider lifecycle results

- Date: 2026-08-15
- Status: Implemented candidate with portable and current-host native evidence
- Advances: [Decision 0583](0583-First-Native-Bound-Model-Provider.md)
- Extends: [model protocol version 1](../../Specifications/Windvale-Model-Protocol.md)

## Context

The first native bound model provider admitted typed model responses, but a
stale generation used generic `Unavailable` and the qualification host had no
catchable revocation, peer-exit, or uncertain-submission outcomes. Treating all
provider loss as the ABI-23 failure branch would trap the application before
the facade could preserve request identity, provider generation, and retry
safety.

A model request can be billable or retained by a remote provider even though it
does not mutate Windvale storage. Losing a peer before dispatch and losing
certainty after dispatch therefore require different results.

## Decision

- Extend the version-1 provider-status vocabulary additively with `Revoked=8`,
  `Stale=9`, `Peer_exited=10`, and `Submission_indeterminate=11`.
- Define `Revoked`, `Stale`, and `Peer_exited` as pre-dispatch lifecycle
  rejections. `Peer_exited` means the live rights-limited bridge observed that
  its backing peer had already exited before sending this request.
- Require `Submission_indeterminate` once the bridge cannot prove whether a
  generation request was accepted after dispatch. It carries no output or
  usage claim and must never be retried automatically.
- Require an alive bridge to publish a complete `WVMC 1` or `WVMG 1` response
  and return zero from ABI 23 for these expected outcomes. ABI nonzero remains
  the runtime failure path for an invalid call frame, corrupt bridge, or other
  condition that cannot publish a trustworthy protocol response.
- Preserve exact request identity and the bridge's observed provider
  generation in every lifecycle response. The bound facade independently
  admits the envelope and identity before exposing the status.
- Replace the scripted provider's stale-generation use of `Unavailable` with
  the exact `Stale` status.
- Add deterministic portable and native cases for revoked binding, stale
  generation, pre-dispatch peer exit, post-dispatch uncertainty, and rejection
  of unknown status 12.
- Do not add automatic retry, fallback, network access, credentials, streaming,
  or provider-specific policy.

## Consequences

Hosted callers can now catch the first model-provider lifecycle failures without
turning expected revocation or peer loss into a language trap. `Unavailable`
returns to its narrower meaning of temporary provider inability rather than
also standing in for stale identity.

This result boundary does not make a removed capability-table entry catchable.
The bound bridge must remain alive long enough to report loss of its backing
provider. A malformed bridge response is still an invalid protocol result, and
an ABI failure before trustworthy response publication still traps.

The deterministic host remains offline and rights-limited. Its injected
lifecycle responses prove the native compiler, provider table, facade, and
decoder path without claiming a public provider, network transport, or
production supervisor.

The model-provider retirement owner grows from eight to eleven cases. The
complete plan remains 69 suites and advances to 3,557 declared cases.

## Reconsideration triggers

Introduce a separate transport-evidence field or a new protocol major version
if a live adapter needs more states than definite pre-dispatch rejection versus
indeterminate post-dispatch submission. Revisit the ABI boundary when typed
capability values or source-level invocation results can represent loss of the
bridge itself. Never infer safe retry from provider name or HTTP status alone.
