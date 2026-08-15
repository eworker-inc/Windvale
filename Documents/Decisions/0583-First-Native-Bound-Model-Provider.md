# Decision 0583: First native bound model provider

- Date: 2026-08-15
- Status: Implemented candidate with current-host execution and cross-host image construction
- Defines: [bound model provider](../../Specifications/Windvale-Bound-Model-Provider.md)
- Extends: [Decision 0573](0573-First-Provider-Neutral-Model-Protocol.md)
- Advanced by: [Decision 0585](0585-Catchable-Model-Provider-Lifecycle-Results.md)

## Context

Decision 0573 established strict provider-neutral model records but deliberately
stopped before a capability call. The native provider emitter already had a
bounded fixed call frame and the source compiler had exact capability catalogs,
so the repository could implement an offline hosted seam without inventing
networking, credential, or vendor semantics.

## Decision

- Add `model.catalog_v1(bytes) -> bytes` and
  `model.inference_v1(bytes) -> bytes` to the exact Seed, WVB-verifier, and
  native-lowering catalogs.
- Reuse ABI 23's fixed provider call for one argument cell and one bytes result.
- Add a hosted facade that validates requests before dispatch and independently
  admits response identity, generation, status, and complete protocol shape.
- Add one exact two-entry scripted provider table with execution-owned state and
  immutable offline responses. It receives no ambient authority.
- Add a native retirement owner that reconstructs the changed compiler and
  lowerer, proves deterministic source/object construction, creates both host
  packages, and executes the local host.
- Reconstruct and repin only the segmented WVO staging producer family because
  that producer embeds the changed native lowerer. The independent image-staging
  and canonical-transport families remain byte-identical.
- Reconstruct and repin the standalone WVB-to-WVO candidate family for the same
  reason; its retained return-42 WVB and WVO controls remain byte-identical.
- Keep provider-call failure as the existing runtime failure until Windvale has
  a catchable provider-loss/revocation result boundary.
- Defer all live provider transports, JSON mappings, credentials, cancellation,
  uncertain submission, retries, streaming, and multiple bound instances.

## Consequences

Hosted Windvale code can now call a rights-limited model provider through the
same canonical protocol used by portable code. The deterministic provider proves
the complete compiler, verifier, lowering, provider-table, facade, and native
execution path without a public network or secret.

This is infrastructure for live adapters, not a claim that Windvale can reach a
public model API. A future adapter must preserve the canonical protocol and earn
the shared network, trust, secret, cancellation, and malformed-response evidence
separately. Responses remain borrowed unless copied before another provider call.

The retirement plan gains one eight-case shard-4 owner. It now contains 66
suites and 3,500 declared cases.

## Reconsideration triggers

Revisit this boundary when catchable provider loss is implemented, when typed
capability values permit several simultaneous provider instances, or when a
real adapter proves that a canonical request or result cannot preserve required
transport-completion evidence. Do not widen this capability into ambient HTTP
or credential access.
