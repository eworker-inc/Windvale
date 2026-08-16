# Decision 0597: First external-model reference oracle

- Date: 2026-08-15
- Status: Implemented candidate with deterministic offline evidence; live smoke not run
- Implements: model slice 3 in the
  [external-model provider proposal](../Project/Windvale-External-Model-Provider-Library-Proposal.md)
- Contract: [external-model reference oracle](../../Specifications/Windvale-External-Model-Reference.md)

## Context

Windvale already has canonical provider-neutral model messages, catalog and
generation records, a strict platform facade, and an offline native
`bytes -> bytes` provider seam. It now also has bounded operation, network
authority, and reliable-stream semantics. It still lacks production host
resolver, TCP, TLS, HTTP, JSON, and secret-custody providers.

Waiting for every production transport layer before checking current external
API mappings would leave avoidable integration uncertainty. Adding a
model-specific native `post(url)` capability would instead bypass the shared
network and credential boundaries selected for the product.

The installed build runtime already supplies an HTTPS client and JSON parser.
It can serve as a clearly restricted reference oracle without redefining
Windvale runtime semantics.

## Decision

- Add a build-restricted Node reference client under `Tools/Models/`.
- Accept and return only the canonical `WVMQ/WVMC/WVMG 1` byte records.
- Implement explicit mappings for OpenAI Models and Responses, Anthropic Models
  and Messages, and Google Gemini Models and `generateContent`.
- Select Google `generateContent` for version 1 because Windvale owns the full
  stateless message history. Do not depend on provider interaction identities
  or preserve private thought blocks.
- Pin each HTTPS origin, path family, API version, authentication header, and
  credential environment name in the adapter. Accept no request-controlled
  URL, arbitrary header, redirect, proxy, or credential value.
- Disable OpenAI response storage explicitly. Keep prompts, responses, raw
  provider errors, and credentials out of ordinary terminal output.
- Bound request files, provider response bodies, catalog entries, strings,
  pagination, output budgets, timeout values, and canonical result records.
- Treat every listed external model as a mutable alias unless stronger admitted
  lifecycle evidence exists. Do not invent a universal `latest` model.
- Perform no automatic retry. A generation transport failure after dispatch
  begins becomes `Submission_indeterminate`; a catalog transport failure is
  `Unavailable`.
- Keep all deterministic verification offline with injected HTTPS responses.
  A real call is an explicit operator-run smoke and may spend money or retain
  disclosed data according to the provider account and policy.

## Consequences

Developers can now use real HTTPS through the host Node runtime to list or call
OpenAI, Anthropic, or Google with canonical Windvale model envelopes. The
mapping code is reusable evidence for the later hosted gateway, and all three
provider shapes receive the same bounded result contract.

This is not the production Windvale network stack. It does not prove Windvale
TLS, HTTP, trust-generation, revocation, protected secret storage, service
supervision, or multi-provider capability binding. Environment variables are
acceptable only for this explicit developer tool. Production model and package
traffic still waits for the shared secure networking and credential
infrastructure required by Decision 0595.

## Reconsideration triggers

Revisit this decision when a provider removes or materially changes a pinned
endpoint, when provider-side retention cannot meet the selected profile, when
the production HTTPS/secret gateway can replace this oracle, when catalog
metadata supports admitted stable lifecycle evidence, or when version 1 gains
streaming, tools, multimodal content, structured output, or provider-managed
conversation state.
