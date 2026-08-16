# Windvale supervised external-model gateway version 1

## Status and purpose

The hosted gateway selected by
[Decision 0605](../Documents/Decisions/0605-First-Supervised-External-Model-Gateway.md)
accepts one canonical [model request](Windvale-Model-Protocol.md), maps it to one
fixed external provider API through [protected credential custody](Protected-Provider-Credential.md)
and [bounded HTTPS](Bounded-Https.md), and returns one canonical catalog or
generation response. It supports OpenAI, Anthropic, and Google without exposing
provider URLs, headers, credentials, SDK objects, or JSON to portable callers.

The gateway is a hosted bootstrap implementation, not a general reverse proxy,
browser fetch surface, provider router, streaming conversation service, or
permission to send a request to a fallback provider. One child owns one provider
identity, provider generation, credential lease, trust generation, and request
at a time.

## Supervised custody boundary

The supervisor launches the pinned Node gateway module with an empty environment,
no provider SDK, and no secret command-line argument. It sends one bounded
`WVGI 1` initialization frame through the child's standard-input pipe before any
model request. The fixed 64-byte little-endian header is:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVGI` |
| 4 | version `1` |
| 8 | complete byte length |
| 12 | encrypted-wrapper length |
| 16 | passphrase length |
| 20 | reserved zero |
| 24 | nonzero provider generation `u64` |
| 32 | nonzero trust generation `u64` |
| 40 | maximum canonical request bytes |
| 44 | maximum HTTP response-header bytes |
| 48 | maximum decoded provider-body bytes |
| 52 | maximum provider wire bytes |
| 56 | maximum operation milliseconds |
| 60 | maximum TLS-provider lifetime milliseconds |

The encrypted `WVSC 1` wrapper and passphrase bytes follow. The complete startup
frame is at most 4,096 bytes. The passphrase is 16 through 1,024 strict UTF-8
bytes; the wrapper is 177 through 1,437 bytes. Limits are finite and internally
consistent. The supervisor and child erase their startup-frame, wrapper,
passphrase, derived-key, credential, request, authorization, and provider-body
buffers at their maintained ownership transitions.

After authenticated unlock, the child returns exactly one 56-byte `WVGR 1`
record. Success contains only provider code, provider generation, credential
generation, and the 16-byte public credential identity. Failure contains status
only and zero metadata. Wrong passphrase, wrapper authentication failure, or
invalid credential plaintext therefore reveals no provider/key distinction.

The child then accepts one complete `WVMQ 1` frame at a time and returns the
matching `WVMC 1` or `WVMG 1` frame with the same caller request identity. A
second frame, extra bytes, malformed framing, oversized input/output, wrong
response identity, child diagnostics beyond 4 KiB, deadline expiry, or peer exit
tears down the process. EOF, signal, protocol failure, or supervisor teardown
destroys the credential lease. Credentials and passphrases are never read from
model records, files, process arguments, environment variables, or provider SDK
discovery.

## Fixed provider mappings

All service names and port 443 come from the authenticated credential profile.
Targets and public fields are constructed internally:

| Provider | Catalog | Generation | Additional public field |
| --- | --- | --- | --- |
| OpenAI | `GET /v1/models` | `POST /v1/responses` with `store:false` | none |
| Anthropic | `GET /v1/models?limit=1000` | `POST /v1/messages` | `anthropic-version: 2023-06-01` |
| Google | `GET /v1beta/models?pageSize=1000` | `POST /v1beta/models/{encoded-model}:generateContent` | none |

The credential lease alone injects `Authorization: Bearer`, `x-api-key`, or
`x-goog-api-key`. The gateway supplies only `accept`, selected provider-version,
and POST `content-type` fields. Bounded HTTPS owns authority, framing, length,
connection close, redirect refusal, trust, deadlines, and exact local write
acceptance. Provider pagination cursors are admitted as bounded text, percent
encoded into a newly exact-bound target, limited to 16 provider pages and 8,192
entries, and never treated as an absolute URL or authority.

OpenAI receives Windvale system/user/assistant roles directly. Anthropic and
Google accept one optional leading system message followed by alternating
user/assistant messages ending in user. Google model identifiers are encoded as
one path component. The gateway never silently reroutes a request or substitutes
a model.

## Bounded JSON admission

Provider response bytes have already passed the HTTPS header/body/wire limits.
The gateway requires exact JSON content type, strict UTF-8, one JSON object, and
the provider-specific bounded arrays, identifiers, text blocks, usage integers,
completion reasons, and pagination fields. It discards OpenAI reasoning items,
but rejects tool calls and every other unsupported structured output rather than
flattening it into text. Google catalog entries without `generateContent` are
not advertised.

Catalog entries are deduplicated and byte-sorted before a local page is encoded.
The opaque Windvale continuation binds provider name, SHA-256 of the complete
visible identifier set, and next offset. A changed catalog makes it stale.
Provider error bodies and host exceptions never enter canonical diagnostics.

HTTP 400/422, 401/403, 404, 408/429, and 5xx map to fixed invalid-request,
unauthorized, unsupported, rate-limited, and unavailable results. Revoked and
stale leases are definite pre-dispatch results. Malformed successful provider
JSON is provider error. Any generation transport failure after invocation begins
is `Submission_indeterminate`; the gateway never retries it. Catalog transport
failure is unavailable. Redirects are returned by HTTPS and mapped as a definite
provider error without another connection.

## Executable evidence

`Test-External-Model-Gateway` owns 30 deterministic cases:

- 20 core cases cover the three exact provider mappings, catalog pagination and
  continuation, request JSON, successful response admission, HTTP/error mapping,
  redirect refusal, malformed JSON/content type, stale/revoked generations,
  unsupported output, uncertain generation submission, fixed origin, buffer
  erasure, real credential-to-bounded-HTTPS composition with a fake key, and
  byte-identical catalog output against the independent reference oracle; and
- 10 supervision cases cover startup/ready framing, bounds, caller-buffer
  preservation, metadata-only readiness, child unlock, no secrets in launch
  arguments, canonical stale response without networking, generic wrong-key
  failure, malformed request rejection, and idempotent teardown.

All fixtures are synthetic. The owner reads no real credential, writes no
plaintext credential file, follows no redirect, and makes no public-network call.
The current evidence is Windows; the same owner must pass independently on Linux
before the gateway becomes dual-host evidence. A separately authorized live smoke
can show current provider interoperability but is never deterministic acceptance.

## Deferred production boundary

The child still runs on the hosted Node bootstrap and uses the previously
implemented supervised resolver/TCP/TLS providers beneath bounded HTTPS. The
Windvale native capability/timer bridge, launcher/service-manager binding,
OS-keyring or HSM custody, protected interactive unlock channel, operational
rotation/recovery, live provider smoke, streaming output, concurrency, and
multi-provider routing remain separate work. None permits credentials, endpoints,
or raw JSON to enter the portable model protocol.
