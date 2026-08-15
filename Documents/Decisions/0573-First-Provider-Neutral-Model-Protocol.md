# Decision 0573: First provider-neutral model protocol

- Date: 2026-08-15
- Status: Implemented candidate with native source-compilation evidence
- Defines: [model protocol version 1](../../Specifications/Windvale-Model-Protocol.md)
- Informed by: [external model provider library proposal](../Project/Windvale-External-Model-Provider-Library-Proposal.md)

## Context

Windvale needs a reusable way to describe external-model catalogs and text
generation without making one provider's JSON, SDK, API key, endpoint, mutable
`latest` alias, or server-side conversation state part of portable semantics.
The language already supports strict bounded byte codecs and typed records, but
the repository does not yet implement shared HTTPS, production secret custody,
general cancellation, or a native `bytes -> bytes` provider call.

## Decision

- Add capability-free `WVMM 1` messages, `WVMQ 1` catalog/generation requests,
  `WVMC 1` catalog responses, and `WVMG 1` generation responses.
- Keep version 1 text-only, non-streaming, stateless, bounded, and caller-owned.
- Require generation requests to name both a model identifier and the observed
  nonzero provider generation. Mutable aliases remain visible catalog evidence;
  there is no universal silently changing `latest` model.
- Separate codec rejection from typed provider status and require strict total,
  count, range, reserved-zero, UTF-8, and cross-field validation.
- Add one capability-free scripted provider with stable deterministic catalog,
  generation, stale-generation, and malformed-request behavior.
- Add Project 2 library and corpus projects to the focused native library build
  owner. This milestone claims source compilation, not a live API or completed
  native execution seam.
- Defer provider-specific JSON, endpoints, credentials, network access,
  cancellation, uncertain-submission evidence, streaming, tools, images,
  embeddings, and provider-managed conversations.

## Consequences

Portable applications and future adapters now share one exact provider-neutral
language and byte boundary. OpenAI, Anthropic, Google, and later adapters can be
implemented independently without forcing their schemas into Windvale source
semantics. Catalog adoption remains a caller policy decision.

The current library cannot contact a public service. A hosted adapter must not
bypass the missing boundaries with an ambient `post(url, headers, bytes)` leaf
or place credentials in Windvale-visible values. Provider success also remains
untrusted until its complete canonical response passes the portable decoder.

The library owner grows from 26 to 29 cases: two additional reusable projects
and one additional conformance build. The complete retirement manifest contains
64 suites and 3,483 declared cases.

## Reconsideration triggers

Revise the protocol only when a concrete live adapter proves that a bound
version-1 field cannot be preserved, when uncertain submission needs a frozen
transport-completion record, or when an implemented consumer requires a new
modality. Widen limits only from measured provider and runtime evidence. Add
streaming, tools, structured output, images, embeddings, or multiple provider
instances through separate contracts rather than optional unvalidated fields.
