# Terminal line-input capability version 1

## Status

This is an accepted candidate contract with migrated source, native-provider,
and deterministic-test implementations. Current split-compiler construction,
native execution on the local host, and independent Linux execution remain
promotion evidence; until they pass, no released Windvale application may
claim this capability as implemented.

## Purpose and authority

`terminal.line_read_v1(bytes) -> bytes` is the first semantic interactive-input
capability available to an ordinary hosted Windvale program. It reads one
visible line from a real user terminal. It is not a general standard-input byte
stream, terminal-control API, credential API, raw host handle, or grant of file,
process, model, or network authority.

The application supplies a canonical `WVLR 1` request and receives one `WVLI 1`
response. The provider validates the request before reading. The portable
decoder validates the entire response before a line becomes `text`.

This capability deliberately does not return protected credentials. The model
chat's API key and passphrase remain in the supervised credential/gateway host;
the Windvale application receives only a model capability already bound to that
credential. A later credential-custody capability may request a host-owned
unlock operation, but it must not disguise secret bytes as an ordinary line.

## WVLR request

All integers are unsigned little-endian. The request is exactly 32 bytes:

| Offset | Bytes | Field | Rule |
|---:|---:|---|---|
| 0 | 4 | magic | ASCII `WVLR` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` |
| 12 | 4 | maximum line bytes | `1..3072` |
| 16 | 4 | mode | `0`, visible input |
| 20 | 4 | reserved | zero |
| 24 | 8 | expected provider generation | `1` |

Unknown modes are rejected. In particular, version 1 has no masked or secret
mode because such a mode would expose the resulting secret to the caller.

## WVLI response

The response has a 32-byte header followed by the admitted line bytes:

| Offset | Bytes | Field | Rule |
|---:|---:|---|---|
| 0 | 4 | magic | ASCII `WVLI` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | header plus content |
| 12 | 4 | status | `0..6` |
| 16 | 4 | content bytes | exact trailing byte count |
| 20 | 4 | reserved | zero |
| 24 | 8 | provider generation | nonzero |

Statuses are `Completed`, `End_of_input`, `Interrupted`, `Rejected`,
`Provider_lost`, `Stale`, and `Revoked`. Only `Completed` carries content. Its
content may be empty, must not exceed the requested maximum, must be strict
UTF-8, and contains no NUL, CR, or LF. The provider removes the terminal line
ending. Every other status has zero content. `Stale` and `Revoked` carry a
generation other than 1; all other valid version-1 statuses carry generation 1.

Malformed, truncated, oversized, wrong-generation, or internally inconsistent
responses become `Invalid_response`; content from them is never decoded as
text. A provider must consume or reject an overlong physical line as one bounded
operation and must not return its suffix as the next user command.

## Platform binding

The Windows and Linux leaves are granted a terminal endpoint by the supervised
native application host. The endpoint is separate from the private
request/response transport used by `model.catalog_v1` and
`model.inference_v1`. Binding a terminal endpoint does not grant an ambient
socket or reveal the protected provider credential.

Version 1 is serial: at most one line read is active. The provider bounds input,
drain work, diagnostics, retained state, and teardown. Terminal disappearance,
peer exit, interruption, revocation, and provider replacement remain typed.

## Required executable evidence

`Terminal-Line-Input-Core-Self-Test.wv` covers request creation and admission,
completed and empty lines, end/interruption, stale/revoked generations, unknown
statuses, oversized content, status/payload contradictions, embedded line
terminators, malformed UTF-8, wrong magic, reserved fields, and invalid caller
limits. It makes no terminal or network call.
