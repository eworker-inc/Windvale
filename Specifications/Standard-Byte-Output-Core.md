# Standard Byte Output Core

## Status

Implemented candidate under
[Decision 0704](../Documents/Decisions/0704-First-Portable-Standard-Byte-Output-Core.md).
The paired focused owner reconstructs the exact accepted Decision 0587 compiler
generation because general current-compiler packaging is not yet a promoted
front door. This tooling condition does not imply a host stream provider.

## Purpose and scope

`Windvaleˉstandardˉbyteˉoutputˉcore` defines the portable state machine
between one writer and one directional standard-output byte sink. It preserves
arbitrary bytes, exact locally accepted progress, bounded buffering,
backpressure, orderly drain, peer closure, provider loss and restart, teardown,
and conservative post-dispatch mutation outcomes.

The core owns no capability, pipe, descriptor, endpoint, worker, thread, wait,
callback, terminal encoding, text conversion, or native buffer. Windows pipes,
Linux pipes, Windvale OS endpoints, and browser-worker messages are later
providers that must preserve this state machine. Their partial-write behavior
does not define Windvale semantics.

This is standard output, not a general duplex stream. Standard input, terminal
editing, file writes, pipeline topology, and redirection remain separate
contracts.

## Identity and lifetime

`Outputˉopen` binds one nonzero provider identity and generation, one nonzero
stream identity and generation, and one nonzero virtual monotonic-clock
generation. Every peer-consumption, peer-close, provider-loss, restart, and
teardown observation must match the exact bound identities and generations.
A mismatch is not applied and leaves the supplied immutable state unchanged.

Provider restart and teardown require a nonzero replacement provider generation
different from the bound generation. They terminate the old stream and never
retarget it. Provider loss carries no replacement generation.

`Outputˉrelease` is accepted only in a terminal phase and is idempotent. A
released stream retains no byte buffer or active write. Releasing a stream does
not convert bytes that were abandoned after acceptance into consumed bytes.

## Limits and deadlines

An opened stream declares limits no greater than:

- 65,536 bytes in one write;
- 262,144 retained bytes awaiting peer consumption; and
- 4,194,304 accepted bytes over the stream lifetime.

The limits must be nonzero. Each write also carries an absolute deadline in the
bound clock generation. The deadline must be later than the current observation,
must not exceed the stream expiration, and must fit the stream's declared
maximum deadline span.

The core performs checked comparisons before every subtraction and bounds every
byte slice. It does not assign a unit to virtual ticks. A concrete provider must
use monotonic time and publish its unit without importing civil wall-clock
behavior into this contract.

## Writes, acceptance, and backpressure

Only one write may be active. `Outputˉwriteˉbegin` copies one immutable byte
value into the pending write and begins one mutating bounded operation. A
zero-byte write completes immediately, accepts zero bytes, and creates no
buffer entry.

For a nonempty write, dispatch, cumulative progress, completion, rejection, and
cancellation are explicit observations. Progress is cumulative and strictly
increasing under the bounded-operation contract. Completion through the public
write API must name the complete requested length. A peer-close observation is
the sole path that may terminate a dispatched write at its exact shorter known
progress.

Newly reported progress appends exactly the corresponding pending-byte slice to
the retained buffer and advances `Acceptedˉbytes` by exactly that length. If
the slice would exceed the declared buffer limit, the transition returns
`Backpressured`, is not applied, and leaves both stream and operation progress
unchanged. The provider may retry the same cumulative progress after the peer
consumes bytes. Backpressure is never reported as partial success.

Accepted progress means that the local semantic provider assumed ownership of
those exact bytes. It does not prove display, remote receipt, file durability,
or application commit.

## Consumption and accounting

`Outputˉconsume` is a peer/provider observation over one exact positive prefix
of the retained buffer. It removes that prefix and advances
`Consumedˉbytes`. Zero consumption and consumption beyond the retained prefix
are rejected.

Every valid state preserves this exact equation:

```text
Acceptedˉbytes =
    Consumedˉbytes +
    Releasedˉwithoutˉdeliveryˉbytes +
    Bytesˉlength(Buffer)
```

The three destinations are disjoint. `Consumedˉbytes` is known peer
consumption. `Releasedˉwithoutˉdeliveryˉbytes` is accepted ownership that
ended without such evidence. `Buffer` is accepted ownership still retained for
the peer. Bytes remaining only in an unfinished pending write were never
accepted and appear in none of the three counts.

## Close and terminal phases

`Outputˉwriterˉclose` requires no active write. It immediately enters
`Closed` when the buffer is empty, otherwise enters `Closing`. A closing stream
accepts no new writes. Exact peer consumption of its last buffered byte moves it
to `Closed`.

Peer close terminates the stream as `Peerˉclosed`, releases every retained
accepted byte without claiming consumption, and clears the unfinished pending
suffix. A queued write is rejected; a dispatched write terminates at its exact
known progress.

Provider loss, provider restart, and teardown respectively produce
`Providerˉlost`, `Stale`, and `Tornˉdown` unless a mutating write was already
dispatched. In that case the underlying bounded operation reports
`Submissionˉindeterminate` and the stream enters `Writeˉindeterminate`.
Every retained accepted byte moves to released-without-delivery accounting.
No indeterminate write is retried automatically.

All terminal phases retain zero buffered bytes and no active write. They keep
the exact terminal operation outcome and cause for observation and audit.

## Executable evidence

`Standard-Byte-Output-Core-Self-Test.wv` owns ten groups:

1. invalid identity, generation, clock, and limit admission;
2. zero-byte completion and idempotent release;
3. one exact byte and peer consumption;
4. the exact 65,536-byte maximum chunk;
5. preservation of invalid UTF-8 byte sequences;
6. multiple ordered chunks and draining close;
7. slow-reader backpressure, prefix consumption, and exact retry;
8. early peer exit with released-without-delivery accounting;
9. cancellation under backpressure after mutating dispatch; and
10. provider loss, restart, teardown generation safety, and complete cleanup.

The library WVB is 55,898 bytes with SHA-256
`d80e98f785e8dfab0e357a7d74457f07775141bf31d2773e2d7745c061a7aa26`.
The test WVB is 75,874 bytes with SHA-256
`7fba163fd1087c324bf640879b72a5208375e49ab298950ba97d987a7c2a4d17`.
Two independent builds reproduced both WVBs byte for byte. Two lowerings
reproduced the 2,650,952-byte test WVO with SHA-256
`2abd417b75f497c6f1b9c99395101fec722597bb38ce436ea1bea3fa9ba476b2`.

The same linked image executes all ten groups with result `42` on Windows and
Debian. The Windows container is 2,668,544 bytes with SHA-256
`abd3a197a58b50103364096d6b84d7972f06cf65d519e121cf507560f9059d9a`;
the Linux container is 2,670,592 bytes with SHA-256
`3d43d3553122a443e4b90211720a755a196182d06e75c98468bb37a745071d80`.
Compilation used the exact native compiler closure reconstructed from Decision
0587's accepted commit `4aca9935679b67f46bfb97f37c2e566980bbab68`; no
managed compiler or host-specific semantic implementation was used.

## Next boundary

This core supplies the semantic state machine used by the hosted
`standard_output.write_v1` adapter. Decision 0713 binds that capability and the
existing immutable read-only directory to the ordinary Windvale `file-read`
application. Its focused owner proves exact bytes with no appended newline on
Windows and Linux. The fixed `cat` alias already canonicalizes to `file-read` in
the Shell 1 parser; active-generation packaging and browser execution remain
separate integration boundaries.
