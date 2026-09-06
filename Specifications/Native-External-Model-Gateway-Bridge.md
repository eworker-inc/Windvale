# Windvale native external-model gateway bridge version 1

## Status and purpose

The existing catalog/inference native bridge is an implemented candidate. The
visible terminal binding and Windvale-owned model-chat composition described
below are migrated candidates pending current split-compiler construction and
native host execution.

The bridge selected by
[Decision 0647](../Documents/Decisions/0647-First-Native-External-Model-Gateway-Bridge.md)
connects the existing ABI-23 `model.catalog_v1` and `model.inference_v1`
provider calls to the
[supervised external-model gateway](Supervised-External-Model-Gateway.md).
It is the first executable native-to-hosted composition boundary. It does not
grant a native application a socket, resolver, TLS object, credential, URL,
environment, or provider JSON surface.

One launch owns one native model-chat worker and one credential-owning gateway
child. The launcher dedicates the worker's standard output to requests and its
standard input to responses. Visible output and line input use separate
rights-limited terminal endpoints, so terminal text cannot corrupt private
model traffic. A later general service manager may replace the process-private
channels and terminal discovery with named inherited handles without changing
ABI 23 or the model protocol.

## Native provider binding

`X64-External-Model-Gateway-Host.wva` is extended to derive execution context 9
from the admitted context 7 and construct the exact six-entry `WVPT 1` table
required by the native chat. Catalog, inference, standard byte output, and
terminal line input have separate state objects. The two process-argument
identities retain zero provider bindings because they use existing fixed
runtime services.

For every call the target independently requires:

- one complete borrowed-bytes argument cell and zero descriptor generation;
- a `WVMQ 1` record from 48 through 65,536 bytes;
- exact declared length, reserved fields, request operation, and selected
  capability state; and
- a matching `WVMC 1` catalog response no larger than 65,536 bytes or `WVMG 1`
  generation response no larger than 1,048,576 bytes, with the same request
  identity.

The provider writes one complete request, reads the fixed response prefix and
then its exact remaining bytes, and publishes a borrowed descriptor only after
all bridge checks pass. The response occupies execution-owned RW/NX stack
scratch and remains valid only until the next call on the same binding. Before a
later call and after `Main` returns, the host erases the exact prior response
span. Invalid ABI/request input returns nonzero without pipe I/O; loss of a pipe
before a canonical result is available also returns nonzero.

## Platform leaves

The Linux x64 leaf holds private file descriptors 0 and 1 plus one `/dev/tty`
descriptor when an interactive terminal is present. Its direct `read` and
`write` loops require exact completion, retry `EINTR`, reject EOF, zero progress,
oversized progress, and every other syscall failure, and retain no descriptor
after process exit. Line input is bounded to 3,072 bytes and drains at most
65,536 bytes when rejecting an overlong line.

The Windows x64 leaf obtains private standard input through
`GetStdHandle(-10)` and uses
the output handle, `ReadFile`, and `WriteFile` already admitted by the hosted
container. It separately opens `CONIN$` and `CONOUT$`, reads visible text through
`ReadConsoleW`, and converts it with strict UTF-8 validation. It resolves these
functions only from the same bounded KERNEL32 image that owns the admitted
writer, rejects forwarded or malformed exports, and requires exact bounded
partial-I/O completion. It imports no socket, TLS, registry, credential,
environment, or clock function.

## Supervision and timer authority

`Native-External-Model-Gateway-Supervisor.mjs` first waits for authenticated
gateway readiness, then launches the absolute native worker path with an empty
environment and no credential arguments. It accepts exactly one complete WVMQ
frame while a request is in flight, forwards it to the existing gateway
supervisor, and writes exactly one matching canonical response. Malformed or
extra native output, more than 4 KiB of diagnostics, pipe failure, worker exit,
or lifetime expiry tears down both peers.

The launcher supplies finite operation and lifetime milliseconds to the gateway
and owns the native worker lifetime timer. A gateway loss after a generation
request has been accepted becomes a canonical `Submission_indeterminate`
response; catalog loss becomes `Unavailable`. Neither result is retried. The
gateway continues to own monotonic request deadlines, credential lease
generation, TLS trust generation, and all external dispatch.

## Existing and required executable evidence

`Test-Native-External-Model-Gateway` owns 14 deterministic cases. It verifies
five launcher/supervision behaviors; byte-identical assembly and structural
admission of the shared host, independent ABI probe, and both platform leaves;
both final hosted images; and execution on the current host. The ABI probe sends
one generation request with a deliberately stale provider generation. The real
protected gateway returns a canonical stale result before network construction,
so the test uses a fake credential, no public network, and no plaintext file.

The existing model-provider owner separately proves that Windvale source lowers
catalog and inference calls to this exact ABI-23 table/cell convention. The
bridge probe intentionally remains independent WVA so transport qualification
does not duplicate compiler semantics. The migrated model-chat owner must add
current-compiler terminal-call lowering, deterministic core execution, and
native application construction before promotion. A source-level live
application and an explicitly authorized provider smoke remain later product
evidence; neither changes this binding contract.

This version remains a hosted bootstrap around pinned Node supervisors. The
chat UI and bounded conversation have migrated to Windvale source but are not
the supported path until their current-compiler evidence passes. OS
keyring/HSM integration, protected interactive unlock, operational rotation and
recovery, a general inherited-handle service manager, streaming, concurrency,
and multi-provider routing remain outside version 1.
