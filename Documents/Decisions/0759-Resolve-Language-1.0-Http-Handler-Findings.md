# Decision 0759: Resolve the Language 1.0 HTTP-handler findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0756](0756-Resolve-Language-1.0-File-Copy-Findings.md), and
[Decision 0758](0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md).
It accepts all five general findings from the HTTP request-handler paper bundle.

It does not freeze edition 1, change Windvale Seed, select a final service
capability identity, make provider calls asynchronous, or claim a native HTTP
implementation.

## Context

The fifth mandatory paper workload accepts one rights-limited reliable stream,
reads one bounded request into fixed initialized storage, parses strict framing
through borrowed slices, retains recognized headers deterministically, decodes
only a declared text body, renders one response, and advances writes using exact
local-provider progress.

The existing candidate specified slice lifetimes and constructors but not enough
checked observation to write an ordinary parser. Strict UTF-8 decode accepted
only complete immutable `bytes`, forcing an unnecessary intermediate copy from
a caller-owned buffer. Byte builders could append fixed-width integers but not
the invariant decimal required by text protocols. Finally, the file workload
had intentionally deferred a reusable deadline/cancellation input.

## Decision

### Complete checked slice and immutable-byte borrowing

Accept:

```text
export fn Sliceˉlength<T>(Value: Slice<T>) -> u64 effects();

export fn Sliceˉat<T>(
    Value: Slice<T>,
    Index: u64,
) -> borrow T effects();

export fn Borrowˉrange(
    Value: borrow bytes,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects();
```

Slice construction has already proved its complete range. `Sliceˉlength`
returns exact elements. `Sliceˉat` checks `Index < length` and its result inherits
the slice's one underlying owner; it cannot escape that owner. Copy/shared
read-through remains Decision 0758's ordinary rule. `Borrowˉrange` checks
`Start + Length` with checked arithmetic against `Bytes.Length` before forming
the view. Empty ranges use only the admitted one-past-end boundary. No unchecked
counterpart is implied.

### Strict UTF-8 decode directly from a byte slice

Accept:

```text
export fn Decodeˉutf8ˉsliceˉreserved(
    Budget: Memoryˉbudget,
    Value: Slice<u8>,
    Maximumˉbytes: u64,
    Maximumˉrunes: u64,
) -> Result<text, Decodeˉutf8ˉfailure>
    effects(memory.allocate);
```

This has the same strict shortest-form Unicode scalar validation, byte/rune
limits, consuming budget, complete publication, and allocation-versus-source
failure split as `Decodeˉutf8ˉreserved`. The input remains borrowed only during
the call. Success does not retain the mutable-buffer owner or expose an
intermediate immutable byte value.

### Invariant decimal append for byte builders

Accept:

```text
export fn Appendˉu64ˉdecimal(
    Builder: borrow mut Bytesˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects();
```

It appends the shortest unsigned ASCII decimal representation. Zero is `0`.
There is no sign, locale, grouping, padding, radix prefix, or host formatting.
The operation proves complete resulting length before mutation and is
all-or-nothing on limit failure.

### Provider-facing operation context

Accept the source-level role of one shared immutable opaque Hosted
`Operationˉcontext` supplied by the launcher/provider boundary. It binds a
nonzero monotonic clock identity/generation, absolute deadline, nonzero
cancellation-view identity/generation, and already admitted provider deadline
span. Application source may borrow/pass it but cannot construct, inspect civil
time through it, extend its deadline, or request cancellation.

Accept, read, and write calls are explicit observation points. At or after the
deadline, timeout wins. Pre-dispatch cancellation proves no operation progress.
Post-dispatch mutation may be indeterminate. Provider loss/restart and stale
generations remain distinct. The value is not a capability grant, keyword,
task handle, or alternate cancellation system.

Workload 6 must reconcile task-scope cancellation request/propagation and any
asynchronous provider signature with this context before its final canonical
identity is frozen.

### Exact reliable-stream outcome meanings

For the synchronous workload-5 profile, accept these semantic requirements:

- read completion reports one exact initialized target prefix and optional
  orderly peer close;
- write completion reports one exact positive prefix accepted locally;
- a rejected call proves zero current-call progress;
- an indeterminate dispatched write proves no safe replay point;
- later calls receive only the previously uncompleted suffix; and
- local stream release performs bounded teardown without claiming remote receipt
  or graceful protocol completion.

Zero progress without peer close, over-range progress, stale generation events,
and cross-stream events are provider defects. No automatic reconnect or retry is
part of the contract.

The strict HTTP framing/routing policy remains workload/library behavior rather
than language semantics.

## Consequences

The HTTP-handler bundle becomes draft reviewed. Six of eleven workloads are now
draft reviewed and five remain.

Parsers can consume caller-owned bytes without raw pointers or forced copies.
Text protocols can format bounded unsigned fields directly into byte builders.
Hosted I/O can receive deadline/cancellation evidence without ambient clock
access or premature task syntax. The same exact progress rules remain usable if
workload 6 later selects asynchronous provider calls.

This decision adds no HTTP-specific syntax, WIR/WVB opcode, exception, tracing
GC, automatic retry, ambient network authority, socket, TLS, clock, entropy,
reflection, or second compiler.

## Reconsideration triggers

Reconsider slice observations only if a later workload proves one-owner lifetime
inheritance cannot be diagnosed without a different ephemeral-view type. Never
replace checked ranges with raw pointer arithmetic in Core or Hosted source.

Reconsider slice decode only if measured implementations cannot publish text
without retaining the input owner. Any alternative must avoid hidden copying or
state its extra allocation/accounting explicitly.

Reconsider decimal append only for a separately named richer formatting API.
The accepted operation remains invariant, unsigned, shortest, and
all-or-nothing.

Reconsider operation-context spelling during workload 6. Preserve opaque
launcher construction, monotonic generation, absolute deadline, cooperative
cancellation, explicit observation points, and indeterminate post-dispatch
mutation.

Reconsider synchronous provider spelling if structured-task evidence requires
`async`/`await`. Preserve the exact progress, generation, no-replay, and local
release semantics independent of suspension implementation.
