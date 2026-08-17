# Workload 5 semantic review

## Mutation and ownership

Visible mutation is limited to the root budget, input buffer, work meter, scan
state, ordered header map, byte builder, operation counters, and request stream.
Every exclusive buffer slice ends before the buffer is scanned. Immutable parse
slices end before another read mutates the buffer. Header records retain Copy
ranges, so no borrow crosses a read or escapes the parser.

`Acceptˉone` transfers one stream into `using`. The scope releases it after a
valid response, request rejection response, typed provider failure, `try`
propagation, or operation/work limit. No return path owns a second handle.

## Allocation and retained state

All allocation authority is split before construction. The fixed request buffer
is zero initialized. Map and response capacity are committed before growth. Echo
decode consumes one fixed child budget and either publishes complete valid text
or no text. Unknown headers, parse failures, and stream errors cannot grow a
diagnostic list.

The parser performs an incremental terminator scan. It carries only the next
candidate start and optional first-line end, revisiting at most three boundary
bytes per provider read. It never starts again at byte zero after each chunk.

## Evaluation and ordering

Reads and writes occur in source order. Header lines are examined in wire order,
but retained recognized fields use the explicit canonical enum order. Duplicate
recognized fields are rejected at the second encounter. Unknown valid fields
are counted and ignored deterministically.

The response builder appends status, length, fixed headers, separator, and body
in that order. Unsigned decimal formatting is invariant and shortest. The write
loop sends suffixes in increasing byte-offset order.

## Failure and trap boundary

Recoverable families are:

- configuration admission;
- memory allocation and collection construction;
- request syntax/framing/limit rejection;
- accept/read typed provider failures;
- early peer close;
- strict UTF-8 source/allocation failure;
- response-capacity rejection;
- exact write rejection or indeterminate mutation;
- provider progress defect or impossible consumed-phase state; and
- operation/work exhaustion.

Terminal traps remain only for violated proved Foundation preconditions:
checked slice access/range, map rank access, or arithmetic that source has first
bounded. Every untrusted byte offset and provider count is validated before one
of those operations. Malformed network input therefore returns a typed result,
not a bounds trap.

## Cleanup and cancellation walkthrough

1. Validation failure releases the supplied root on ordinary return.
2. Input/header allocation failure releases all earlier successful children in
   reverse order.
3. Accept failure returns without a stream; the provider owns any internal
   teardown needed to prove no resource was returned.
4. After accept, `using` owns the only stream handle.
5. Read rejection, timeout, cancellation, reset, loss, or restart releases the
   buffer, unused header budget, stream, and root. No response is attempted.
6. Request rejection releases the temporary header map and builds one bounded
   client-error response while the stream is still owned.
7. Echo decode failure releases its consumed child. Invalid UTF-8 becomes a
   `400`; physical allocation failure remains a handler failure.
8. Known positive short write advances by exactly that prefix. Rejection
   returns the known prior total. Indeterminate write returns immediately and
   never retries the uncertain suffix.
9. On full local-provider acceptance, response/input/text backings release and
   `using` locally closes the stream. No graceful peer receipt is claimed.

Cancellation is observable only at awaited accept/read/write in this workload.
The borrowed operation context cannot be retained beyond `Run`. Decision 0760's
task scope derives that context and owns the sole explicit cancellation request;
the handler needs no second flag or cancellation authority.

## Cross-host meaning

The same source has identical parsing, routing, UTF-8, map order, response bytes,
and progress decisions on Windows, Linux, Windvale, and an admitted WebAssembly
provider. Host socket APIs, TLS libraries, event loops, timers, error numbers,
line endings, locales, and scheduler order cannot enter those semantics.

## Usability answer

The annotations identify real boundaries: one capability, one owned stream,
four budgeted owners, mutable read targets, immutable parse/write slices, and
one shared operation context. The route and response code stays nominal and
readable. The missing piece was not an HTTP keyword; it was a small complete
checked-slice and byte-formatting Foundation surface.
