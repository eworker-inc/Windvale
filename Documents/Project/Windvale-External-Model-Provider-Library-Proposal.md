# Windvale external model provider library proposal

> Status: Proposed library and provider-boundary direction. This document does
> not define an accepted serialized format, public capability, provider adapter,
> credential mechanism, or implementation claim. The provider survey is a
> documentation snapshot from 2026-08-15; live catalog discovery, not this file,
> must determine what an authorized account can use.

## Purpose

Windvale should have one reusable library for discovering externally hosted
models and requesting bounded inference without making OpenAI, Anthropic,
Google, another vendor, JSON, HTTP, an SDK object, or an API key part of portable
application semantics.

The library is useful independently of the proposed
[agent runtime](../Architecture/Agent-Runtime-And-Digital-Subconscious.md). A
small application should be able to list models visible to one bound provider,
select an exact model, submit a caller-owned conversation, and receive a typed
result. The later agent runtime can consume the same interface while retaining
ownership of run state, context compilation, memory, routing, tools, approvals,
and continuity.

## Summary conclusion

Windvale has enough implemented language and library machinery to begin the
provider-neutral core now:

- immutable records, enums, bounded sequences, bytes, text, checked integer
  operations, strict UTF-8, and deterministic codecs;
- hosted capability declarations and a bounded native provider table;
- an established pattern in which a typed platform library validates a strict
  byte response from a rights-limited provider; and
- deterministic native project, fixture, malformed-input, and cross-host
  verification paths.

Windvale does **not** yet have enough implemented infrastructure for a
production library to contact public model APIs directly. The missing pieces
are shared HTTPS/TLS and HTTP framing, production credential custody, general
deadlines and cancellation, recoverable provider failures, a native
`bytes -> bytes` provider-call shape, and multiple typed provider instances.

The honest first implementation is therefore a portable request/result codec
and scripted provider. A build-restricted host reference adapter could contact
one real API as integration evidence, but it would remain an oracle outside the
portable contract. Production live adapters wait for the shared networking and
credential boundaries rather than introducing a model-specific `post(url)` or
`download(url)` host call.

## Current external API landscape

The three providers for which credentials are already available all expose
direct model APIs and catalog discovery:

| Provider | Current direct generation surface | Catalog discovery | Design consequence |
| --- | --- | --- | --- |
| OpenAI | The current [model guide](https://developers.openai.com/api/docs/models) places current models behind the Responses API. | [`GET /v1/models`](https://developers.openai.com/api/reference/resources/models/methods/list) returns models visible to the credential, with basic identity, ownership, creation, and shutdown metadata. | The adapter must not infer modalities, context limits, or tool support from the catalog response when those fields are absent. |
| Anthropic | The [Claude API overview](https://platform.claude.com/docs/en/api/overview) identifies `POST /v1/messages` as the general-availability direct-model surface. | The same overview identifies `GET /v1/models`; requests also require an explicit Anthropic API version. | System instructions, messages, token limits, and content blocks need an Anthropic mapper rather than an OpenAI-shaped portable request. |
| Google Gemini | The current [Gemini API reference](https://ai.google.dev/api) recommends the Interactions API and retains standard `generateContent` plus SSE and live variants. | [`GET /v1beta/models`](https://ai.google.dev/api/all-methods) lists available models; individual model metadata can include supported methods and limits. | The adapter must freeze one named Google endpoint/version profile at implementation time while keeping provider state and server-side interaction identities outside portable state. |

Other providers also publish model APIs. Representative current examples are
[Mistral chat and model discovery](https://docs.mistral.ai/api/),
[Cohere v2 Chat](https://docs.cohere.com/v2/reference/chat) with a separate
[model list](https://docs.cohere.com/reference/list-models),
[xAI Responses and richer language-model discovery](https://docs.x.ai/developers/rest-api-reference/inference/models),
and [Groq's OpenAI-compatible chat and model endpoints](https://console.groq.com/docs/api-reference).
Cloud model brokers and self-hosted engines can be later adapters to the same
Windvale contract.

This survey supports an adapter family, not a lowest-common-denominator copy of
one vendor schema. An API that accepts OpenAI-shaped JSON is not thereby
semantically identical in supported fields, limits, error behavior, storage,
streaming, usage evidence, model identity, or retry safety.

## Meaning of “latest”

`latest` must not be a universal Windvale model name. It can mean at least four
different things:

1. a provider-maintained mutable alias;
2. the newest entry returned to one credential;
3. the newest model satisfying a required capability profile; or
4. a reviewed application policy's preferred model.

The library should expose catalog observations and exact model identities. A
policy layer may propose adopting a newer catalog entry, but refresh must not
silently change an executing application's model. Each invocation records the
requested identifier, the provider-returned model identifier, provider
generation, catalog generation when used, and whether the request used an exact
identifier or a mutable alias.

Catalog presence proves that the credential can see a model at observation
time. It does not by itself prove every modality, endpoint, context limit,
output limit, tool, data-location, price, or retention property. Normalized
feature fields therefore need `Supported`, `Unsupported`, and `Unknown` states
rather than optimistic Boolean defaults. Provider documentation or an admitted
profile may supplement catalog metadata; a spending live probe is not an
automatic discovery operation.

## Architectural ownership

The proposed ownership split is:

| Owner | Responsibility | Must not own |
| --- | --- | --- |
| `Libraries/Models/` | Provider-neutral request, response, catalog, usage, selection-evidence, and strict codec logic. | Credentials, URLs, TLS, JSON, provider SDK objects, or ambient model choice. |
| `Libraries/Platform/Models/` | Typed facade over one rights-limited bound catalog or inference provider. | Provider-specific payload construction or run/agent state. |
| Hosted/native provider adapter | Exact provider endpoint/version mapping, authentication, JSON parsing, HTTP status and header translation, secret custody, and bounded response construction. | Portable conversation ownership, model-routing policy, tool authority, or unchecked raw payload publication. |
| Shared network libraries and providers | Resolver, secure stream, HTTP framing, deadlines, cancellation, trust, and teardown. | Model semantics or credential selection. |
| Calling application or agent runtime | Conversation history, model/profile policy, catalog adoption, budgets, data-placement decision, result use, and visible fallback policy. | Provider credentials or permission to broaden a provider binding. |

No source directory should be added until its first accepted contract and
consumer are implemented.

## Version 1 semantic scope

The first public semantic profile should be deliberately small:

- one bound provider instance per capability binding;
- catalog listing with bounded pagination;
- text input and text output;
- non-streaming inference;
- explicit `System`, `User`, and `Assistant` message roles;
- caller-owned stateless conversation history on every invocation;
- an exact model identifier or an explicitly permitted mutable alias;
- a maximum-output budget;
- provider-reported finish and usage evidence when available; and
- bounded provider request identity and diagnostic evidence.

Version 1 should exclude tool calls, provider-hosted tools, images, audio,
video, files, embeddings, reranking, fine-tuning, batches, prompt caches,
provider-managed conversations, background jobs, web search, computer use,
realtime transports, and arbitrary structured-output schemas. Each excluded
family has different authority, resource, data-disclosure, and completion
semantics and should enter through a concrete consumer and focused decision.

Multi-turn conversation is still possible: the caller owns the bounded message
history and resubmits it. A provider response identity may be recorded as
evidence but is not required to resume, and a provider conversation handle never
becomes durable agent memory.

## Candidate portable records

The exact record definitions and byte encoding require a numbered decision.
The first corpus should nevertheless test these conceptual records:

- `Modelˉproviderˉidentity`: stable provider kind, adapter contract version,
  provider generation, account/workspace binding identity, and placement class;
- `Modelˉcatalogˉrequest`: operation identity, expected provider generation,
  bounded page size, and opaque bounded continuation supplied only by the same
  provider generation;
- `Modelˉcatalogˉentry`: exact model identifier, optional display name,
  alias state, lifecycle state, normalized feature evidence, known context and
  output limits, and provider metadata generation;
- `Modelˉcatalogˉresult`: status, provider generation, catalog generation,
  ordered entries, optional continuation, truncation evidence, and diagnostic;
- `Modelˉmessage`: closed role, ordered text parts, and no provider-native JSON;
- `Modelˉinferenceˉrequest`: operation identity, expected provider generation,
  model reference, messages, output budget, data-placement class, and requested
  optional features from a closed version-1 set;
- `Modelˉinferenceˉresult`: status, completion kind, requested and returned model
  evidence, ordered output parts, usage, provider request identity, truncation,
  and bounded diagnostic; and
- `Modelˉusage`: input, cached-input, output, and reasoning-token counts with an
  explicit `Reported`, `Estimated`, or `Unavailable` evidence kind.

Price is not a property that the first model catalog can safely normalize.
Provider APIs commonly return token usage separately from changing price tables,
tiers, regions, cache rules, and tool charges. A later cost owner can combine
reported usage with a versioned admitted pricing snapshot and currency; the
inference adapter must not invent a cost when the provider did not report one.

Candidate corpus bounds, to be measured before acceptance, are 64 messages,
64 KiB of UTF-8 per text part, 1 MiB aggregate request text, 256 UTF-8 bytes per
model identifier, 256 catalog entries per page, 1 MiB normalized output, and a
4 MiB total provider envelope. These fit current value ceilings but are not yet
contract promises.

## Candidate capability seam

The smallest provider boundary is two conceptually separate capabilities:

```text
model.catalog_v1(Request: bytes) -> bytes
model.inference_v1(Request: bytes) -> bytes
```

These names and signatures are candidates, not accepted interfaces. Keeping
catalog and inference separate permits a launch to authorize discovery without
authorizing spending inference or disclosing prompt data. The portable platform
library constructs a canonical request envelope, invokes the bound capability,
then independently validates the complete response before returning typed
records.

The request contains no endpoint, native path, API key, bearer token, account
secret, arbitrary header, or unrestricted provider name. The host binding fixes
the exact provider, origin, endpoint family, API version, credential, account or
workspace, model allow-list, data-placement policy, request and byte quotas, and
provider generation. Changing any of those replaces the binding generation.

Current native capability-provider machinery demonstrates stateful
rights-limited dispatch, but generated calls are implemented only for selected
three-cell and five-cell shapes. A focused `bytes -> bytes` emission,
independent structural verifier, exact host binding, and malformed-result suite
are still required before these candidate calls execute natively.

Current capability identity also binds one provider implementation to one
capability ordinal. Until typed capability references or another exact
multi-instance contract exists, version 1 should run with one external provider
per application launch. A single ambient registry holding every available API
key would make routing easy but grant unnecessarily broad authority. Provider
comparison can initially use separate launches over identical canonical request
fixtures.

## Provider adapter rules

Each adapter translates the same semantic request independently:

- the OpenAI adapter maps it to the current accepted Responses API profile;
- the Anthropic adapter maps system instructions, messages, and the required API
  version to the Messages API;
- the Google adapter maps it to one explicitly selected Interactions or
  `generateContent` profile; and
- later adapters map only fields they can implement exactly.

An adapter rejects a requested feature it cannot preserve. It does not drop the
field, emulate it through hidden prompt text, reinterpret a limit, or accept a
vendor default as Windvale semantics. Provider-specific optional behavior may
be exposed by a later named profile without changing version 1.

Raw JSON is untrusted host-boundary input. The adapter checks HTTP status,
content type, declared and observed body length, JSON depth and collection
counts, required and exclusive fields, UTF-8, numeric ranges, output totals,
finish state, model identity, usage counters, and trailing data before creating
the canonical Windvale response envelope. Raw JSON, SDK objects, streaming event
objects, and provider-private reasoning never enter portable records.

## Credentials, network, and data placement

API keys remain in an execution-owned host secret binding. They never enter
Windvale source, WVB, package metadata, arguments, model requests, checkpoints,
artifacts, ordinary diagnostics, or committed fixtures. A developer-only
reference adapter may use a named host secret supplied outside the repository,
but that is not production key custody and must never print the value.

The provider binding permits only its pinned HTTPS origin and endpoint family.
It does not accept a request-controlled base URL, proxy, redirect target, or
header map. Cross-origin redirects, HTTPS downgrade, ambient proxy inheritance,
and automatic credential forwarding are rejected. TLS validation, trust
generation, entropy, civil-time policy, resolver behavior, body limits, and
teardown come from the shared network contracts.

Every call carries a data-placement class that the binding must admit before
sending any prompt byte. Provider-side storage is disabled where the selected
API profile supports that control. When the provider cannot supply the required
retention or placement guarantee, the call is denied rather than silently sent
under a weaker policy.

## Completion, failure, and retry

The normalized result should distinguish at least:

- completed, truncated, content-filtered, tool-or-other-output-unsupported, and
  invalid provider output;
- denied, unsupported, unavailable, rate-limited, quota-exhausted, revoked,
  stale provider generation, provider lost, cancelled, and expired;
- request known not sent, provider accepted, and submission indeterminate; and
- usage reported, estimated, or unavailable.

A model request can spend money or cause provider-side retention even though it
does not mutate Windvale state. If a connection fails after request bytes were
accepted, completion and charge may be indeterminate. Version 1 performs no
automatic retry in that state. A later provider-specific idempotency contract
may permit retry only when the provider documents and the adapter proves the
exact identity and lifetime semantics.

Fallback is a caller-owned visible policy. It records the failed route, reason,
new provider/model generation, and changed privacy, quality, latency, and cost
expectations. An adapter never silently sends the prompt to another provider.

## Readiness against the current repository

| Required part | Current evidence | Readiness |
| --- | --- | --- |
| Typed provider-neutral state | Current Seed records, nested records, enums, exhaustive matching, and bounded sequences | Ready for the version-1 corpus and pure library. |
| Strict canonical bytes | [`Foundation-Bytes`](../../Specifications/Foundation-Bytes.md) and many bounded binary readers/writers | Ready for a focused envelope after a decision freezes it. |
| Typed platform-facade pattern | [`Random-Access-Storage.wv`](../../Libraries/Platform/Storage/Random-Access-Storage.wv) decodes and revalidates a provider-owned byte envelope | Ready as a pattern, not reusable model code. |
| Rights-limited provider state | [`Windvale-Native-Capability-Provider-Table`](../../Specifications/Windvale-Native-Capability-Provider-Table.md) | Partially ready; one provider per capability identity is practical. |
| Native provider invocation | [`Windvale-Native-Provider-Call`](../../Specifications/Windvale-Native-Provider-Call.md) | Partial; model calls need a new verified `bytes -> bytes` shape and host integration. |
| JSON request and hostile-response codec | Windvale has deterministic JSON-style text quoting, not a general JSON value/parser contract | Not ready; provider JSON belongs first in the host adapter and needs bounded parsing. |
| HTTPS, HTTP, resolver, trust, deadlines, and cancellation | The [networking foundation plan](Windvale-Networking-Foundation-Implementation-Plan.md) defines ordered slices through secure HTTP retrieval | Designed, not implemented. |
| Production API-key custody | Identity/trust architecture exists; no production key store or secret provider is implemented | Not ready. |
| Multiple simultaneous provider instances | Typed capability values and nominal provider signatures are future work | Not ready; use separate launches first. |
| Streaming and concurrent inference | General structured tasks, cancellation, channels, and concurrent provider calls are absent or proposed | Not ready and outside version 1. |
| Agent integration | The agent architecture and staged plan are proposed; the deterministic kernel is not implemented | Not a blocker for the standalone library; integration comes later. |

## Staged implementation route

### Model slice 0: decision and corpus

Freeze only the version-1 records, operation identities, candidate limits,
canonical envelopes, statuses, completion evidence, and one-provider binding
rule. Create hand-authored provider-independent accepted and rejected cases.

Exit gate: a reviewer can distinguish catalog visibility from capability,
exact IDs from mutable aliases, model output from trusted state, provider
rejection from invalid adapter output, and known-not-sent from indeterminate
submission without contacting a public API.

### Model slice 1: portable protocol library

Implement capability-free encoders, decoders, validators, typed records, catalog
admission, and inference-result admission under `Libraries/Models/`. Add a
scripted deterministic provider corpus covering success, unsupported feature,
truncation, filtered output, bad model identity, malformed lengths, invalid
UTF-8, usage overflow, stale generation, rate limit, provider loss, and
indeterminate submission.

Exit gate: the same canonical requests and results produce byte-identical
reports through the Windows and Linux native paths. No API key, network, clock,
entropy, JSON parser, or model quality is required.

### Model slice 2: hosted byte-envelope seam

Add the two candidate capability identities, the verified `bytes -> bytes`
native call shape, a scripted rights-limited host provider, and the typed
`Libraries/Platform/Models/` facade. Provider state and response scratch remain
execution-owned; the facade copies any admitted payload that must survive a
later provider call rather than retaining a borrowed provider descriptor.

Exit gate: exact launch admission, success, denial, malformed provider response,
revocation, stale generation, and provider loss execute without any public
network access.

### Model slice 3: optional external reference oracle

A build-restricted Node reference tool may translate the canonical envelopes to
one real provider using the host's HTTPS and JSON implementation. This can
validate mapping assumptions and capture redacted, hand-admitted structural
fixtures. It is not a runtime dependency, portable implementation, production
credential store, or semantic oracle.

Exit gate: an explicitly enabled smoke can list and invoke one allow-listed
model without logging secrets or prompt bodies by default; deterministic tests
continue to pass offline when the provider is unavailable or changes.

### Model slice 4: production hosted adapter

After the shared networking plan reaches its secure HTTP gate and production
secret custody exists, implement one provider adapter end to end. OpenAI is a
reasonable first candidate because the current Responses and Models APIs cover
the two version-1 operations, but adapter priority is a product decision rather
than a portable semantic choice.

Exit gate: isolated deterministic HTTP/TLS peers own malformed, fragmented,
timeout, cancellation, rate-limit, response-limit, credential-redaction,
revocation, and indeterminate-submission cases. An opt-in live call is smoke
evidence only.

### Model slice 5: adapter plurality

Implement Anthropic and Google against the same accepted corpus, then add one
non-frontier provider such as Mistral, Cohere, xAI, Groq, a cloud broker, or a
local engine to prove the interface is not accidentally limited to the first
three vendors.

Exit gate: each adapter either preserves a requested version-1 field exactly or
rejects it as unsupported; normalized results never hide provider/model changes.

Streaming, multimodal data, tools, structured output, embeddings, batches, and
provider-managed state are later independent slices.

## Decisions required before implementation claims

A numbered decision is required before accepting:

- the first serialized model request, catalog, result, or usage envelope;
- the first public `model.*` capability and native call shape;
- the first host secret and provider-account binding;
- the first live external endpoint and its data-placement profile;
- any automatic model selection, mutable-alias adoption, fallback, or retry;
- multiple simultaneous provider instances; or
- streaming, tools, multimodal data, or provider-managed conversation state.

## Open questions

- Should the first accepted live adapter be OpenAI, or should the choice be made
  only after the offline corpus reveals which provider has the smallest exact
  version-1 mapping?
- Are separate catalog and inference capabilities worth the extra native call
  ownership, or can one operation-tagged capability preserve least authority
  without becoming an ambient provider registry?
- Which candidate byte and item limits remain useful after measuring real
  OpenAI, Anthropic, and Google response shapes?
- Should the build-restricted reference oracle be implemented before shared
  networking, or would it create more temporary surface than useful evidence?
- What exact retention and data-placement classes can all three initial
  adapters prove rather than merely request?
- Which future typed capability-reference design permits several provider
  instances without putting credentials, endpoint selection, or broad routing
  authority into portable requests?

## Immediate recommendation

Accept the provider-neutral direction for discussion and, if this product lane
is selected, implement only model slices 0 and 1 first. They are fully supported
by current Windvale semantics and produce reusable tested code. Treat a live
reference adapter as optional integration evidence. Do not describe Windvale as
able to contact external models until the provider call, secure HTTP, credential,
and live-adapter gates have actually passed.
