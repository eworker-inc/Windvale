# Windvale bound model provider

## Status and purpose

This specification defines the implemented hosted facade over the
provider-neutral [model protocol version 1](Windvale-Model-Protocol.md). It is
the first executable model-provider seam; it is not a public-network adapter.

The facade owns two separately authorized capabilities:

```text
model.catalog_v1(bytes) -> bytes
model.inference_v1(bytes) -> bytes
```

Both accept one complete `WVMQ 1` request. Catalog returns `WVMC 1`; inference
returns `WVMG 1`. The capability names distinguish discovery authority from
inference authority even though the current static dependency closure requires
an importing root to approve both.

## Facade contract

`Boundˉmodelˉcatalog` and `Boundˉmodelˉgenerate` first decode and validate the
complete request. An invalid envelope or wrong operation never reaches the
provider. After a successful call, the facade decodes the complete response and
requires its request identity to match. A successful generation must also match
the request's observed provider generation; a successful catalog with a
nonzero requested generation has the same rule.

Codec rejection and provider status remain separate. A provider may return a
well-formed denied, unavailable, rate-limited, or other typed failure. A
malformed response becomes an invalid protocol result and is never treated as
provider success.

Provider-returned bytes are borrowed for the provider binding's documented
lifetime. A caller that must retain a response across another provider call
must make an owned copy first. No pointer, native handle, API key, endpoint, or
provider-specific object crosses this interface.

## Native binding

An actual model call selects ABI 23 and the exact `WVPT 1` entry by the WVB
capability ordinal. The lowering copies one complete bytes argument cell into
the fixed five-cell call frame, passes argument count one, and publishes one
caller-owned bytes result cell only when the provider returns zero.

The deterministic qualification host binds an exact two-entry provider table
and immutable scripted responses. It validates the selected state, one-cell
ABI, bytes descriptor, `WVMQ 1` prefix, operation, and admitted request identity.
It grants no network, filesystem, process, clock, entropy, or credential
authority.

## Current evidence and limits

`Test-Model-Provider` reconstructs the source build driver and native lowerer,
compiles and lowers the hosted fixture twice, assembles the provider twice,
checks deterministic WVB/WVO output, links one host-neutral image, constructs
Windows and Linux hosted applications, and executes the current host. The
fixture covers catalog success, request rejection before dispatch, malformed
catalog response, generation success, stale generation, and malformed
generation response.

This implementation does not contact OpenAI, Anthropic, Google, or another
service. It has no HTTP, TLS, DNS, proxy, JSON, credential custody, deadline,
cancellation, streaming, retry, uncertain-submission, cost, or live catalog
contract. Nonzero provider-call status still follows the existing runtime
service-failure path; catchable revocation and provider-loss results require a
later language/runtime result boundary.
