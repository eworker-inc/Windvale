# Decision 0605: First supervised external-model gateway

- Date: 2026-08-16
- Status: Implemented candidate with independent Windows/Linux evidence
- Advances: external-model gateway production path
- Contract: [supervised external-model gateway](../../Specifications/Supervised-External-Model-Gateway.md)
- Builds on: [Decision 0604](0604-First-Protected-Provider-Credential-Custody.md)

## Context

Windvale has provider-neutral model records, a deterministic scripted provider,
an independent three-provider reference oracle, and dual-host resolver/TCP, TLS,
bounded HTTPS, and credential-custody candidates. The remaining usable path must
compose those facilities without turning the reference tool's ambient `fetch`
and environment-key behavior into production authority.

A model request may spend money or trigger provider retention even when it does
not mutate Windvale state. The gateway therefore needs exact lifecycle,
completion, credential, endpoint, JSON, and retry boundaries before it can be
bound to the native model capability.

## Decision

- Add an independent hosted gateway adapter for OpenAI, Anthropic, and Google
  that consumes and produces only canonical `WVM* 1` records.
- Fix each provider's service, port 443, catalog/generation targets, public
  version fields, request JSON, and response admission. Never accept a URL,
  authorization field, credential, or provider JSON from portable input.
- Compose requests through the revocable protected-credential lease and shared
  bounded HTTPS client. Keep redirects, decompression, cookies, proxies,
  connection reuse, and automatic retry absent.
- Launch one child with an empty environment and no secret arguments. Send the
  encrypted wrapper, passphrase, generations, and finite limits through one
  bounded startup frame; return metadata-only readiness and erase maintained
  sensitive buffers at each ownership transition.
- Permit one request in flight. Treat extra frames, framing/identity mismatch,
  oversized records, deadline, child loss, and malformed input as teardown
  conditions; destroy the credential lease on every exit path.
- Admit provider JSON only after HTTPS byte limits and strict content type/UTF-8
  checks. Reject unsupported structured output and never copy provider error
  bodies or host diagnostics into canonical results.
- Preserve definite HTTP/lifecycle failures. Once generation invocation begins,
  any transport failure is submission-indeterminate and is never retried.
- Differentially compare canonical production output with the independent
  reference oracle while keeping production mapping code independent.

## Consequences

Windvale now has an executable protected path from a canonical model request to
the real shared HTTPS stack for all three selected providers. A caller can list
visible models or request bounded non-streaming text without receiving network
or credential authority. The supervised child gives credential replacement and
provider restart a concrete generation/teardown boundary.

This is not yet a complete production promotion. The native capability/timer
bridge, launcher-owned secret delivery, operational
key rotation/recovery, and an explicitly authorized live interoperability smoke
remain. The first child is serial and non-streaming and does not implement model
routing, fallback, tool calls, images, files, audio, provider conversations, or
provider-side idempotent retry.

## Reconsideration triggers

Revisit this decision when one provider changes a pinned API contract, a
qualified use requires streaming or concurrency, an idempotency primitive can
prove safe retry, provider-native tools or media gain portable semantics, or a
native Windvale gateway replaces the hosted bootstrap without changing the
canonical model boundary.
