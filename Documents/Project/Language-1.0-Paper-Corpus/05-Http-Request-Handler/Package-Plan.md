# Workload 5 package and build plan

## Mapping

Package identity: `windvale.paper.http_request_handler.v1`.

| File | Module | Profile | Authority |
| --- | --- | --- | --- |
| `Source/Http-Types.wv` | `Httpˉtypes` | Core | library |
| `Source/Http-Work.wv` | `Httpˉwork` | Core | library |
| `Source/Http-Ordering.wv` | `Httpˉordering` | Core | library |
| `Source/Http-Bytes.wv` | `Httpˉbytes` | Core | library |
| `Source/Http-Parser.wv` | `Httpˉparser` | Core | library |
| `Source/Http-Response.wv` | `Httpˉresponse` | Core | library |
| `Source/Http-Application.wv` | `Httpˉapplication` | Hosted | application |

Platforms are Windows, Linux, and Windvale. The sole required capability is
`network.service.accept` version 1; optional capabilities are empty. Entry is
the exact monomorphic `Httpˉapplication.Run` signature. A WebAssembly target is
admitted only when its launcher binds the same semantic provider contract.

## Reference limits

| Limit | Value |
| --- | ---: |
| start line | 2,048 bytes |
| complete header section | 8,192 bytes |
| header fields | 32 |
| body bytes/runes | 16,384 / 16,384 |
| complete request buffer | 24,576 bytes |
| one read | 4,096 bytes |
| response | 32,768 bytes |
| total read/write operations after accept | 64 |
| application scan/parse work | 100,000 units |
| accepted streams/tasks/queues/recursion | 1 / 0 / 0 / 0 |
| retained recognized headers | 4 |
| retained diagnostics | 1 typed result, no diagnostic collection |

The header scan revisits at most the last three bytes of each completed read.
With six maximum-size input reads its scan work is at most request bytes plus 18
boundary starts. Header parsing, comparisons, validation, and decimal input
conversion charge that same meter. UTF-8 decode and builder work are separately
bounded by admitted input/output bytes; provider operations have their own
counter.

## Root memory plan

The launcher supplies one 131,072-byte root memory budget admitting four
children. Reference source splits at most:

| Child | Maximum bytes | Lifetime |
| --- | ---: | --- |
| initialized request buffer | 24,576 | through route/body decode |
| four-item ordered header map | 4,096 | header parse only |
| strict decoded echo text | 65,536 | echo response only |
| reserved encoded response | 32,768 | through write completion |
| simultaneous maximum | 126,976 | echo path before input/text release |

No allocation grows after its maximum is committed. Header ranges borrow the
input buffer; they do not copy names or values. Unknown headers retain no state.
The decoded echo text and response bytes are separately charged because the
response may outlive the mutable input borrow. Unused root authority is not
allocated.

## Value and ownership inventory

| Value | Class | Owner/lifetime |
| --- | --- | --- |
| limits, ranges, enum keys, counters, scan state | Copy | ordinary stack/value state |
| operation context | shared immutable opaque | launcher owns; handler borrows |
| memory root/children | move-owned accounting | handler and consuming constructors |
| request stream | move-only resource | one `using` scope |
| byte buffer | move-owned initialized storage | receive/prepare phase |
| buffer slices | immutable/exclusive borrows | one expression/parse operation |
| header map | move-owned ordered collection | parse call only |
| decoded text | shared immutable | response plan/backing |
| byte builder | move-owned reserved owner | render call |
| response bytes | shared immutable | write loop |

No borrow is stored in a record, map, task, module datum, or returned result.
Header map values are offsets/lengths, not slices or pointers.

## Capability and effect closure

Core modules have no external capability. Their only nonempty effects are
bounded `memory.allocate` and deterministic local release where constructors are
used. `Httpˉapplication.Run` closes over:

```text
memory.allocate
network.service.accept
resource.acquire
resource.release
```

It has no filesystem, raw network, DNS, listener, TLS, entropy, clock, process,
environment, locale, terminal, logging, task, unsafe, or reflection effect.

## Build and artifact plan

The seven modules lower through ordinary records, variants, maps, slices,
builders, borrows, resource scopes, and capability calls. No HTTP-specific WIR
or WVB instruction is required. The provider interface remains a canonical
capability import and the operation context an ordinary opaque imported type.

The reviewed paper bundle contains 7 source files, 2,206 LF-terminated source
lines, 72 top-level declarations, and 73,744 UTF-8 bytes. `Http-Application.wv`
is 27,932 bytes/731 lines and `Http-Parser.wv` is 24,621 bytes/746 lines; their
separation keeps provider lifecycle out of pure framing code. Current Seed does
not accept edition-1 source, so WIR, WVB, native size, compiler time, execution
time, and working set are explicitly unmeasured rather than estimated. The
implementation owner must record them against the fixed request/resource cases.

Canonical transcript evidence is independently byte-counted: the health request
and response are 63/101 bytes and the UTF-8 echo request/response are 129/105
bytes, with exact SHA-256 identities in [Expected outcomes](Expected-Outcomes.md).

There is no package data, schema, digest, installer payload, generated route
table, certificate, key, trust data, or shipped content object in this workload.
