# Workload 5 expected semantic outcomes

## Canonical health request

Input, shown with escapes:

```text
GET /health HTTP/1.1\r\n
Host: example.test\r\n
Connection: close\r\n
\r\n
```

The input is 63 bytes with SHA-256
`0950f02bc3210950127426e132099205b7301c10bfeb5ef37e579476d6a11fb4`.
The response is exactly:

```text
HTTP/1.1 200 OK\r\n
Content-Length: 3\r\n
Content-Type: text/plain; charset=utf-8\r\n
Connection: close\r\n
\r\n
ok\n
```

It is 101 bytes with SHA-256
`f1ab9fcb6128d897d462db41d240781c9154d72b5e230047ebd54d0b0b15464e`.
The semantic report is `Status.Ok`, `Served(Health)`, request 63, response
101, with exact observed read/write/work counts.

## Canonical UTF-8 echo request

Input:

```text
POST /echo HTTP/1.1\r\n
Host: example.test\r\n
Content-Length: 7\r\n
Content-Type: text/plain; charset=utf-8\r\n
Connection: close\r\n
\r\n
Wind☃
```

`Wind☃` is five Unicode scalars and seven UTF-8 bytes. The complete request is
129 bytes with SHA-256
`63cda5784a38f8cea5806043ea492a86f89e9367dea9f1a34b94226a5bb34dcb`.
The response changes only framing position and is exactly 105 bytes with
SHA-256
`5761b7f7ee449be7189875b730f05720dc1e5c3c15632b1912e1ecea9a57afd4`.
Strict decode and canonical UTF-8 append preserve the seven body bytes.

## Routing results

| Valid request | Status | Body |
| --- | ---: | --- |
| `GET /health` | 200 | `ok\n` |
| `POST /echo` with valid required framing/text | 200 | exact canonical UTF-8 text |
| supported method with another target | 404 | `not found\n` |
| another valid token method | 405 | `method not allowed\n` |

Every response has one canonical `Content-Length`, exact text content type, and
`Connection: close`.

## Client rejection results

Malformed start line/header, duplicate singleton, missing Host, forbidden
transfer encoding, invalid canonical length, invalid connection value, missing
echo content type/length, trailing bytes, or invalid echo UTF-8 produce one
`400` response with body `bad request\n` when the stream remains usable.

Start-line, header-byte, header-count, and body-size limits produce one `413`
response with body `payload too large\n`. The exact typed rejection remains in
the report; status mapping does not erase its observed/maximum evidence.

## Exact progress transcripts

For the 101-byte health response:

| Provider events | Result |
| --- | --- |
| `Completed(101)` | success, one write |
| `Completed(17)`, `Completed(84)` | success, two writes; second slice starts at byte 17 |
| `Completed(17)`, `Rejected(Timedˉout)` | `Writeˉrejected`, known accepted total 17 |
| `Completed(17)`, `Indeterminate(Providerˉlost)` | `Writeˉindeterminate`, known prior total 17; no replay |
| `Completed(0)` or `Completed(102)` | `Invalidˉprogress` provider defect |

Local acceptance is the only completion claim. None of these events alone proves
remote receipt.

## Deadline, cancellation, closure, and generation

- Cancellation before a read/write dispatch returns typed `Cancelled` with no
  progress from that call.
- At the absolute deadline tick, `Timedˉout` wins over simultaneous completion.
- Peer close at an exact request boundary is accepted; before it is
  `Earlyˉpeerˉclose`.
- Provider loss and provider restart retain distinct kind/generation evidence.
- A stale event with another stream/provider generation is translated to
  `Invalidˉresponse`; it cannot advance state.
- Post-dispatch deadline/cancellation/loss/restart on a write may be
  indeterminate and is never retried.

## Maximum case

An admitted maximum request has an at-most 8,192-byte header section and exactly
16,384 body bytes, totaling at most 24,576. With 4,096-byte reads it requires at
most six reads. A maximum echo response remains below the 32,768-byte builder
limit. The reference 64-operation ceiling therefore leaves 58 write calls; a
provider that admits only smaller positive prefixes can terminate with the
explicit operation limit without hidden unbounded work.
