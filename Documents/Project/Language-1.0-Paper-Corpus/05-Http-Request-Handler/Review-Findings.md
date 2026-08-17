# Workload 5 review findings

## Status

Draft reviewed by the project owner on 2026-08-17. All five general findings are
accepted by
[Decision 0759](../../../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md).
Decision 0760 completes the async provider, operation-context, and typed service
endpoint boundary without reopening the five findings below.

## Pressure matrix

| Required pressure | Evidence | Status |
| --- | --- | --- |
| Hosted capability and owned stream | One typed generation-bound endpoint admits an awaited accept returning one resource in `using`. | Pass; completed by Decision 0760 |
| Deterministic ordered maps | Four recognized singleton enum keys map to Copy buffer ranges in canonical order. | Pass |
| Slices into input buffers | Exclusive read target ends before checked immutable parse slices; no slice is retained. | Pass; Foundation completion accepted |
| Explicit text/bytes decode | Only `/echo` body crosses strict UTF-8 slice decode; all framing stays bytes/ASCII. | Pass |
| Partial and indeterminate writes | Known positive prefixes advance suffix; rejection preserves known total; uncertainty stops. | Pass |
| Deadlines and cancellation | One scope-derived opaque context reaches every awaited provider call. | Pass; task propagation completed by Decision 0760 |
| Builders and formatting | Reserved byte builder plus exact invariant decimal Content-Length. | Pass |
| No ambient socket/TLS/clock/entropy | Binding supplies semantic stream; source has none of those effects. | Pass |

## Finding 1: slices need a complete checked observation surface

The candidate defined slice lifetime rules and byte-buffer slice construction,
but ordinary parser source lacked exact length/index operations and a way to
borrow a range from immutable response bytes. Without those operations, source
would either copy every fragment, add raw pointers, or depend on compiler-only
indexing not represented in Foundation.

Accepted resolution:

```text
Sliceˉlength<T>(Value: Slice<T>) -> u64 effects()
Sliceˉat<T>(Value: Slice<T>, Index: u64) -> borrow T effects()

Bytes.Borrowˉrange(
    Value: borrow bytes,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects()
```

Each operation checks complete geometry before access. `Sliceˉat` inherits the
slice's one underlying owner; the Copy read-through rule yields a `u8` value.
No unchecked Hosted/Core alternative follows.

## Finding 2: strict text decode should accept a byte slice

Requiring `Byteˉbuffer -> bytes -> text` would allocate and retain an
intermediate immutable byte copy solely to satisfy a decoder signature. The
source already has an exact immutable body slice and a separate text budget.

Accept `Decodeˉutf8ˉsliceˉreserved` with the same strict shortest-form,
scalar-range, byte/rune maxima, allocation/source failure split, and consuming
budget behavior as `Decodeˉutf8ˉreserved`. The slice remains borrowed only for
the call; success publishes independent shared immutable text.

## Finding 3: byte builders need invariant decimal append

HTTP `Content-Length` is ASCII bytes. Routing through a text builder and second
UTF-8 append would introduce another allocation/budget only to format one
bounded integer. Accept:

```text
Bytes.Appendˉu64ˉdecimal(
    Builder: borrow mut Bytesˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects()
```

It emits shortest unsigned ASCII decimal with no locale, sign, grouping, or
padding. It proves complete capacity before mutation and is all-or-nothing.

## Finding 4: provider calls need one opaque operation context

The file-copy workload deferred the general deadline/cancellation shape. An
HTTP handler needs the same absolute deadline and cancellation observation at
accept, read, and write, but does not need to create tasks or request
cancellation.

Accept one shared immutable opaque `Operationˉcontext` supplied by the launcher
and borrowed by provider calls. It binds monotonic clock identity/generation,
absolute deadline, cancellation-view identity/generation, and admitted span.
Source cannot inspect civil time, extend it, or forge cancellation. This is an
imported Hosted Foundation value, not a keyword, capability grant, task handle,
or second cancellation system. Decision 0760 derives a child view from one
lexical task scope and makes the scope the sole cancellation requester.

## Finding 5: exact stream outcomes are independent of suspension spelling

The first review proved the exact progress meanings without selecting final
suspension spelling. Workload 6 later showed that a hosted service must expose
potential suspension, continuation ownership, and cancellation points in source.
Decision 0760 therefore makes accept/read/write async while preserving the
outcomes below.

Accept the paper profile:

- read completion publishes an exact target prefix and optional peer close;
- write completion publishes one exact positive locally accepted prefix;
- rejection proves zero current-call progress;
- an uncertain dispatched write is explicitly indeterminate;
- every next successful call addresses only the uncompleted suffix; and
- local release is teardown, not graceful response completion.

Every call now requires `await` and `task.suspend`; this changes no completion,
rejection, uncertainty, or no-replay meaning.

## Workload-specific accepted policy

The strict one-request HTTP profile, four recognized singleton headers, unknown
header ignore/repeat policy, no transfer encoding, one response, incremental
linear scan, and route table are application/library rules. They do not become
general language syntax or universal HTTP behavior.

## Acceptance

The bundle is complete first-author source and paper evidence. General findings
are normative-candidate inputs. Decision 0760 synchronizes its provider surface
with the reviewed concurrent service. No current Seed, provider, cross-host, or
source-freeze implementation claim is made.
