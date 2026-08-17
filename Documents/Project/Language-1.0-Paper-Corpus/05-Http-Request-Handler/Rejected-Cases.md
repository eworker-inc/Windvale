# Workload 5 rejected and boundary cases

These cases are part of the paper acceptance suite. Source snippets use the
candidate signatures selected by the bundle.

## Returning an input slice

```text
fn Leak(Buffer: borrow Bytes.Byteˉbuffer) -> Collections.Slice<u8> {
    return Bytes.Borrowˉslice(Buffer: Buffer, Start: 0u64, Length: 1u64);
}
```

Reject because the returned slice would outlive the one borrowed buffer
parameter/owner relation admitted for this public result. The handler stores a
Copy `Byteˉrange` instead.

## Reading while an exclusive provider target is live

```text
let Target = Bytes.Borrowˉsliceˉmut(
    Buffer: borrow mut Input,
    Start: Received,
    Length: Requested,
);
let First = Bytes.Borrowˉslice(Buffer: borrow Input, Start: 0u64, Length: 1u64);
await network.service.accept.Read(
    Stream: borrow mut Stream,
    Target: Target,
    Context: Context,
);
```

Reject the immutable borrow because the exclusive target remains live. Real
source finishes the provider call before scanning the buffer.

## Retaining a slice in the header map

```text
record Badˉheader { Value: Collections.Slice<u8>; }
```

Reject the field type. Borrowed slices cannot be stored in records, variants,
maps, tasks, module data, or serializable state. Header values retain offsets and
lengths.

## Hidden capability call

```text
module Httpˉparser;
profile core;
async fn Read() { await network.service.accept.Read(...); }
```

Reject because a Core module cannot require a Hosted capability and the effect
is absent from the function/module closure. Only `Httpˉapplication` performs
stream operations.

## Forged deadline

```text
let Context = Operation.Operationˉcontext {
    Deadline: 18446744073709551615u64,
};
```

Reject construction because the operation context is opaque. Application code
cannot forge a clock generation, extend a deadline, or mint a cancellation
view.

## Replaying after an indeterminate write

```text
case Stream.Writeˉoutcome.Indeterminate { Error: _ } {
    await network.service.accept.Write(
        Stream: Stream,
        Value: Suffix,
        Context: Context,
    )
}
```

Reject in workload conformance. The first call cannot prove how much of
`Suffix` became visible. Retrying could duplicate bytes and corrupt HTTP
framing.

## Accepting zero write progress

```text
case Stream.Writeˉoutcome.Completed { Accepted: 0u64 } { continue; }
```

Reject as a provider defect. It would permit an unproductive bounded loop and
does not satisfy the stream completion contract.

## Ambiguous request framing

```text
POST /echo HTTP/1.1\r\n
Host: example.test\r\n
Content-Length: 4\r\n
Transfer-Encoding: chunked\r\n
\r\n
...
```

Return `400`. Version 1 rejects every `Transfer-Encoding` field rather than
choosing between two length rules.

## Duplicate singleton after case folding

```text
Host: example.test\r\n
hOsT: second.test\r\n
```

Return `400` with `Duplicateˉheader(Host)`. Recognition is ASCII
case-insensitive and insertion of the second canonical enum key fails.

## Noncanonical content length

```text
Content-Length: 007\r\n
```

Return `400`. The accepted decimal is `0` or a nonzero first digit followed by
digits, with no sign, whitespace, separator, overflow, or leading zero.

## Invalid text body

`POST /echo` with body bytes `66 6f 80` returns `400` with exact invalid byte
offset. Bytes are not implicitly replaced, locale-decoded, or exposed as text.

## Early close and boundary completion

Peer close after the header declares seven body bytes but delivers only six is
`Earlyˉpeerˉclose`, with no response attempt. Peer close in the same read
that completes the seventh byte is accepted and the response write half remains
available.

## Operation and work exhaustion

If the next provider call would exceed `Maximumˉoperations`, return the exact
completed count before dispatch. If the next byte/check charge exceeds
`Maximumˉwork`, return `Workˉexhausted` before that work. Neither limit is
silently raised.
