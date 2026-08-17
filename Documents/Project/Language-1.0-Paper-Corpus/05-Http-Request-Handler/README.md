# Language 1.0 paper workload 5: HTTP request handler

## Status

Complete first-author bundle, draft reviewed by the project owner on
2026-08-17. [Decision 0759](../../../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md)
accepts the checked-slice, strict slice-decode, decimal byte-builder,
operation-context, and reliable-stream findings.
[Decision 0760](../../../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md)
subsequently makes the provider surface explicitly async and supplies one
generation-bound service endpoint without changing exact progress semantics.

This is paper Language 1.0 source. It is not accepted by current Seed tools, is
not a production HTTP server, and does not freeze or implement edition 1.

## Result

Seven modules implement one strict, bounded HTTP/1.1 request handler:

1. await one accept on an already bound, rights-limited service endpoint;
2. read into one fixed initialized byte buffer under one absolute deadline and
   cancellation view;
3. scan the header terminator incrementally without rescanning the complete
   prefix;
4. parse one origin-form request with exact CRLF and framing rules;
5. retain four recognized singleton headers in a deterministic ordered map;
6. route `GET /health` and strict UTF-8 `POST /echo` requests;
7. render one bounded response; and
8. advance response output only by exact locally accepted prefixes.

Malformed requests receive one deterministic `400` or `413` response while the
stream remains usable. Deadline, cancellation, peer reset, provider loss,
provider restart, and indeterminate response mutation remain handler failures.
There is no reconnect or replay.

## Source modules

| Module | Responsibility |
| --- | --- |
| `Httpˉtypes` | Limits, request/header/routing types, reports, and failures. |
| `Httpˉwork` | One exact shared work meter. |
| `Httpˉordering` | Canonical total order for recognized header keys. |
| `Httpˉbytes` | Checked slice comparison, ASCII validation, and bounded decimal parsing. |
| `Httpˉparser` | Incremental header scan, strict request parsing, deterministic header map. |
| `Httpˉresponse` | Status/body planning and reserved exact response construction. |
| `Httpˉapplication` | Capability acquisition, reads, decoding, writes, cleanup, and report. |

The first six modules are Core. `Httpˉapplication` is Hosted and requires only
`network.service.accept` version 1. Its entry receives one Copy endpoint whose
interface explicitly permits shared accepts and one borrowed operation context.

## Evidence index

- [HTTP and stream contract](Http-Contract.md)
- [Package plan](Package-Plan.md)
- [Semantic review](Semantic-Review.md)
- [Rejected cases](Rejected-Cases.md)
- [Expected outcomes](Expected-Outcomes.md)
- [Implementation responsibilities](Implementation-Responsibilities.md)
- [Review findings](Review-Findings.md)

## Acceptance answer

Transport implementation remains hidden, but authority is visible in the
module requirement, stream ownership in `using`, resource limits in `Limits`,
the deadline/cancellation boundary in `Operationˉcontext`, and mutation progress
in the exact write outcome. The successful route is readable without ambient
socket, listener, TLS, clock, entropy, locale, or process state.

## Nonclaims

The bundle does not implement TCP, listening, TLS, HTTP/2, HTTP/3, WebSocket,
chunked transfer coding, keep-alive, pipelining, compression, authentication,
proxying, civil time, cookies, multipart bodies, general routing, or a public
capability catalog. Those are separate bounded contracts.
