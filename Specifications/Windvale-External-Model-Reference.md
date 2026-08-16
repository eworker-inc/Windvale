# Windvale external-model reference oracle

## Status and purpose

`Tools/Models/External-Model-Reference.mjs` is the implemented developer
reference client selected by [Decision 0597](../Documents/Decisions/0597-First-External-Model-Reference-Oracle.md).
It translates canonical [Windvale model protocol](Windvale-Model-Protocol.md)
requests to pinned public HTTPS APIs and translates admitted JSON responses
back to canonical catalog or generation records.

The tool provides real host HTTPS today. It is not a Windvale runtime library,
a production gateway, a secret store, or proof that the Windvale secure-stream
stack is complete. Its deterministic verifier makes no live calls.

## Invocation

```text
node Tools/Models/External-Model-Reference.mjs \
  --provider <openai|anthropic|google> \
  --request <WVMQ-file> \
  --output <WVMC-or-WVMG-file> \
  --generation <nonzero-u64> \
  [--timeout-ms <1-300000>]
```

The request must be one complete `WVMQ 1` record. The output is one complete
`WVMC 1` catalog response or `WVMG 1` generation response. The explicit
generation is the binding generation: a nonzero request generation must match
it, and continuations remain bound to it.

Credentials are read only from the selected environment variable:

| Provider | Credential variable |
| --- | --- |
| OpenAI | `OPENAI_API_KEY` |
| Anthropic | `ANTHROPIC_API_KEY` |
| Google Gemini | `GEMINI_API_KEY` |

The command prints only provider, operation, normalized status, caller request
identity, and canonical output size. It does not print a credential, request
body, prompt, response text, provider JSON, or raw provider error body.

## Pinned mappings

| Provider | Catalog | Generation | Authentication |
| --- | --- | --- | --- |
| OpenAI | `GET https://api.openai.com/v1/models` | `POST https://api.openai.com/v1/responses`, with `store: false` | `Authorization: Bearer ...` |
| Anthropic | `GET https://api.anthropic.com/v1/models` | `POST https://api.anthropic.com/v1/messages` | `x-api-key` and `anthropic-version: 2023-06-01` |
| Google Gemini | `GET https://generativelanguage.googleapis.com/v1beta/models` | `POST https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent` | `x-goog-api-key` |

These mappings follow the current official
[OpenAI Models](https://developers.openai.com/api/reference/resources/models/methods/list),
[OpenAI Responses](https://developers.openai.com/api/reference/resources/responses/methods/create),
[Anthropic Models](https://platform.claude.com/docs/en/api/beta/models/list),
[Anthropic Messages](https://platform.claude.com/docs/en/api/messages/create),
[Google Models](https://ai.google.dev/api/models), and
[Google `generateContent`](https://ai.google.dev/api/generate-content)
contracts as observed on 2026-08-15.

Google's newer Interactions surface is not used in this version. The version-1
Windvale protocol is stateless and caller-owned; `generateContent` preserves
that contract without adopting stored provider interactions or private thought
state.

OpenAI accepts all three Windvale roles directly. Anthropic and Google accept
at most one leading system message followed by alternating user/assistant
messages ending with a user message. OpenAI private reasoning items are ignored
and never enter the portable response. Provider tool calls, images, audio,
files, and other non-text output are rejected as unsupported rather than
silently flattened.

## Bounds and security behavior

- Origins and endpoint families are fixed by provider; a request cannot supply
  a URL, base path, redirect, credential, or arbitrary header.
- Redirects are rejected. HTTPS validation and the trust store are those of the
  installed Node host runtime.
- The input is an ordinary non-symbolic-link file no larger than 65,536 bytes.
- Provider response bodies must identify JSON and are read incrementally with a
  1 MiB limit before parsing.
- Catalogs admit at most 8,192 provider entries and return at most 128 canonical
  entries per page. Continuations contain only provider name, catalog digest,
  and offset; a changed catalog makes the continuation stale.
- Identifiers, messages, catalog pages, diagnostics, and output budgets retain
  the exact `WVM* 1` limits.
- HTTP authentication, not-found, rate-limit, and availability statuses map to
  the closed provider status enum. Provider error bodies are not copied into
  diagnostics.
- The adapter never retries. A failed generation transport after invocation
  starts returns `Submission_indeterminate` because acceptance, charge, or
  retention may have occurred.

The tool cannot prevent an operator from running inside a host process already
modified by debuggers, custom Node dispatchers, or other ambient instrumentation.
Production use requires a supervised service with rights-limited network and
secret bindings rather than this developer process.

## Verification

`Tools/Models/Test-External-Model-Reference.mjs` owns 24 offline cases across
all three providers. Injected HTTPS responses cover pinned origins and headers,
canonical mapping, sorting and pagination, stale generations, content filters,
usage, HTTP status translation, malformed or oversized JSON, unsupported
output, role rejection, credential absence, and the distinction between an
unavailable catalog and indeterminate generation submission.

The `external-model-reference` verification owner runs this corpus on Windows
and Linux. Its accepted summary explicitly records `live-calls=0` and
`secrets=0`. A separately authorized real smoke is useful interoperability
evidence but is never a deterministic build or conformance dependency.
