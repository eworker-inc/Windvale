# Windvale model protocol version 1

## Status and purpose

This specification defines the implemented capability-free `WVMM 1`, `WVMQ 1`,
`WVMC 1`, and `WVMG 1` byte records owned by
`Libraries/Models/Model-Protocol.wv`. They let portable code construct bounded
text conversations, describe catalog and non-streaming generation operations,
and independently admit provider-neutral results.

This is not a public-network API, JSON mapping, credential contract, or claim
that Windvale can contact an external provider. The deterministic
`Scriptedˉmodelˉprovider` is a capability-free corpus provider. The implemented
[bound hosted facade](Windvale-Bound-Model-Provider.md) preserves this protocol
through an offline native capability seam. A future live adapter must separately
earn network, credential, cancellation, transport-completion, and malformed-JSON
evidence.

All integers are unsigned little-endian. All reserved fields are zero. Every
decoder requires the declared total to equal the complete supplied value and
rejects truncation, extension, invalid strict UTF-8, embedded NUL, unknown enum
values, inconsistent counts and lengths, or a violated operation invariant.

## Limits

| Value | Bound |
| --- | ---: |
| Messages per request | 32 |
| One message's UTF-8 content | 3,072 bytes |
| Complete `WVMM 1` message set | 16,384 bytes |
| Model identifier or display name | 256 bytes |
| Catalog entries per page | 128 |
| Complete `WVMC 1` catalog response | 65,536 bytes |
| Opaque continuation | 1,024 bytes |
| Diagnostic | 1,024 UTF-8 bytes |
| Maximum requested output | 4,096 tokens |

These are protocol limits, not statements about a provider's context window,
price, availability, or account policy.

## Messages: `WVMM 1`

The 16-byte header is:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVMM` |
| 4 | version `1` |
| 8 | complete byte length |
| 12 | message count |

Each ordered message is `role:u32`, `content-length:u32`, then exact UTF-8
content. Roles are `System=1`, `User=2`, and `Assistant=3`. Content is nonempty.
The caller owns the complete history and resubmits it; the format grants no
provider-side conversation state.

## Requests: `WVMQ 1`

The fixed header is 48 bytes:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVMQ` |
| 4 | version `1` |
| 8 | complete byte length |
| 12 | operation: catalog `1`, generate `2` |
| 16 | caller request identity `u64` |
| 24 | expected provider generation `u64` |
| 32 | page size or maximum output tokens |
| 36 | model-identifier byte length |
| 40 | payload byte length |
| 44 | reserved zero |

The model bytes and payload follow the header. A catalog request has no model,
uses a page size from 1 through 128, and carries at most 1,024 opaque
continuation bytes. A nonempty continuation requires the generation that
issued it. An initial catalog request may use generation zero.

A generation request requires a nonzero observed provider generation, a
nonempty model identifier, a valid nonempty `WVMM 1` payload, and an output
limit from 1 through 4,096. A mutable alias is ordinary catalog evidence, not a
universal `latest` name. Pinning the observed provider generation prevents an
alias from changing silently within the request contract.

## Catalog responses: `WVMC 1`

The fixed header is 48 bytes:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVMC` |
| 4 | version `1` |
| 8 | complete byte length |
| 12 | provider status |
| 16 | echoed request identity `u64` |
| 24 | provider generation `u64` |
| 32 | entry count |
| 36 | continuation length |
| 40 | diagnostic length |
| 44 | reserved zero |

Entries precede the continuation and diagnostic. Each entry starts with
`entry-length`, `identifier-length`, `display-length`, `features`, and
`lifecycle` as five `u32` values, followed by identifier and display UTF-8.
Feature bit `1` records text input and bit `2` records text output; all other
bits are preserved evidence with no version-1 semantic promise. Lifecycle is
`Stable=1`, `Mutable_alias=2`, or `Deprecated=3`.

A valid response has a nonzero provider generation and no diagnostic. A
failure has no entries or continuation. Catalog presence proves visibility to
that binding at that generation; it does not prove an unreported limit,
modality, price, retention policy, or availability at a later time.

## Generation responses: `WVMG 1`

The fixed header is 64 bytes:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVMG` |
| 4 | version `1` |
| 8 | complete byte length |
| 12 | provider status |
| 16 | echoed request identity `u64` |
| 24 | provider generation `u64` |
| 32 | completion |
| 36 | returned-model length |
| 40 | output-text length |
| 44 | diagnostic length |
| 48 | provider-reported input tokens `u64` |
| 56 | provider-reported output tokens `u64` |

Returned-model, output-text, and diagnostic UTF-8 follow. Successful completion
is `Complete=1`, `Length_limit=2`, or `Content_filter=3`; it requires a nonzero
generation, a returned model, and no diagnostic. Failure has completion zero,
no model or output, and zero usage.

Provider statuses are valid, invalid request, unavailable, unauthorized,
rate-limited, unsupported, provider error, and cancelled. These statuses do not
encode whether a remote submission was known not sent, accepted, or
indeterminate. Live transport work must add that completion evidence before it
may retry a potentially chargeable request.

## Implemented corpus

`Scriptedˉmodelˉprovider` owns generation `1`. Its catalog contains stable
`scripted/text-1` and mutable alias `scripted/current`, both with text input and
output evidence. Its fixed two-entry corpus requires a requested page size of
at least two and otherwise returns `Invalid_request`. A generation request at
generation `1` returns the stable model identity, fixed text, and fixed usage.
A stale generation returns
`Unavailable`; a malformed request returns `Invalid_request`.

The Project 2 portable library manifests and `Model-Protocol-Self-Test.wv` compile under
the native source front door. `Test-Libraries` owns deterministic compilation
of both libraries and the accepted/rejected corpus. It does not claim a live
provider call or public-network execution of a model adapter. The separate
`model-provider` owner executes the deterministic hosted seam.

## Deferred boundary

The two separately authorized catalog and inference operations over
`bytes -> bytes`, native provider-call lowering, rights-limited binding, strict
facade admission, and borrowed-response rule are implemented by Decision 0583.
The live seam still requires HTTPS/HTTP, secret custody, deadlines/cancellation,
transport-completion evidence, and provider-specific bounded JSON mapping. No
API key, endpoint, unrestricted header map, provider SDK object, or raw JSON
belongs in these portable records.
