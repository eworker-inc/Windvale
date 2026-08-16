# Decision 0704: First portable standard byte-output core

- Date: 2026-08-16
- Status: Implemented candidate with paired focused Windows and Debian owner evidence
- Implements: the first prerequisite of Shell Slice 3
- Enables: exact `file-read`/`cat`, later terminal output, and browser/OS stream adapters

## Context

The real Shell 1 parser, `echo` application, package approval, command
resolution, and bounded launch proof now execute on Windows and Linux. The next
catalog application is byte-exact `file-read`, with fixed alias `cat`. Existing
`console.write_line` and text-oriented browser output cannot preserve arbitrary
bytes or avoid an appended newline.

Windows pipes, Linux pipes, Windvale OS endpoints, and browser worker messages
have different buffering and partial-write behavior. Letting any one of those
providers define the shared contract would fork shell semantics before the first
real file command exists.

Decision 0587 already supplies generation-bound operations, monotonic deadlines,
exact cumulative progress, cancellation races, provider invalidation, teardown,
and conservative outcomes for dispatched mutation. Standard output needs to
specialize those semantics with exact owned bytes, bounded buffering, and peer
consumption rather than create a parallel operation model.

## Decision

- Add the portable, capability-free
  `Windvaleˉstandardˉbyteˉoutputˉcore` under
  `Libraries/Platform/Streams/`.
- Bind one directional stream to exact provider, stream, and clock identities
  and generations. Restart and teardown invalidate the old generation rather
  than retargeting it.
- Reuse `Windvaleˉboundedˉoperationˉcore` for every write lifecycle.
  Keep at most one active write and retain its exact terminal outcome and cause.
- Limit one write to 65,536 bytes, retained buffering to 262,144 bytes, and
  lifetime accepted output to 4 MiB. Opened streams may choose smaller nonzero
  limits.
- Treat bytes as bytes. Never decode, normalize, append a newline, or reject an
  invalid UTF-8 sequence in this layer.
- Append only the exact newly accepted pending-write slice. Backpressure rejects
  the observation without mutation so the provider can retry the same cumulative
  progress after peer consumption.
- Maintain the exact invariant `accepted = consumed + released-without-delivery
  + buffered`. Pending but unaccepted bytes do not participate in that equation.
- Distinguish orderly writer close and drain from peer close, provider loss,
  restart, teardown, and indeterminate dispatched mutation.
- Release all retained accepted bytes without claiming delivery on terminal
  failure paths. Make final release idempotent and require zero retained bytes.
- Own ten semantic groups covering the planned stream corpus. Execute the same
  linked image on Windows and Debian and reproduce WVB and WVO artifacts byte for
  byte.
- Register one paired focused owner that reconstructs and verifies the exact
  accepted Decision 0587 compiler generation from full Git history, compiles
  both project closures twice, requires exact WVB/WVO identities, executes the
  current host, constructs the other-host image, and cleans the recovery
  worktree. Do not weaken the core for the obsolete ordinary front door.

## Consequences

Windvale now has an executable standard-output semantic core that is independent
of host pipe sizes and text consoles. `file-read` can be designed against exact
accepted bytes, explicit backpressure, and conservative teardown accounting.
Terminal input, a live output provider, application launch binding, and the
browser and OS adapters remain unimplemented.

The focused owner is permanent, but its compiler is an explicit recovery input.
The repository's ordinary pinned compiler front door predates the
bounded-operation source. Decision 0696 restores current compiler-scale native
staging, but does not promote complete general compiler packaging. The owner
therefore uses the exact accepted compiler generation from commit
`4aca9935679b67f46bfb97f37c2e566980bbab68`. Future front-door promotion may
retire this recovery step without changing stream semantics.

## Reconsideration triggers

Revisit the limits after measured standard-output workloads; when a concrete
provider proves that accepted ownership needs a distinct durable or remote-ack
state; when duplex terminal flow needs coordinated half-close; or when pipelines
require aggregate cancellation and teardown across several streams. Any mapping
from indeterminate dispatched mutation to a retryable result requires a separate
idempotency contract.
