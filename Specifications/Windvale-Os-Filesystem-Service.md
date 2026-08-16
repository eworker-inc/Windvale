# Windvale OS filesystem service protocol

## Status and scope

Filesystem service wire profile 1 is the implemented-candidate request and
response validation boundary for the shared filesystem semantic core. Portable
Windvale validates every request before a provider can run and every response
before it can reach a caller. The kernel transports bounded bytes and
capability references but remains path- and format-blind.

Native provider invocation, queueing, peer-loss reply construction, and guest
integration are not yet implemented and are not implied by wire validation.
The first capacity-one provider-state candidate is implemented separately: it
owns generation advance, caller ownership, open-profile authority, stale
references, close, and peer-exit reclamation without exposing its native token.

## Request envelope

`WVFQ 1` is little-endian and contains 64 through 65,600 bytes:

| Offset | Width | Field |
| ---: | ---: | --- |
| 0 | 4 | Magic `WVFQ` (`0x51465657`) |
| 4 | 4 | Version `1` |
| 8 | 4 | Exact total bytes |
| 12 | 4 | Operation |
| 16 | 4 | Nonzero correlation |
| 20 | 4 | Directory reference |
| 24 | 8 | File reference |
| 32 | 8 | Position or requested length |
| 40 | 4 | Operation control |
| 44 | 4 | Exact payload bytes |
| 48 | 8 | Deadline; zero in request profile 1 |
| 56 | 8 | Reserved; zero |
| 64 | variable | Open segment or write payload |

Open requires a nonzero generation-safe directory reference, zero file
reference and position, control profile 1 through 4, and one valid shared
single-segment payload. It is the only path-bearing operation.

Read, write, set-length, close, and flush require a zero directory reference
and a nonzero generation-safe `u64` file reference. They use the exact
operation geometry and 65,536-byte bound from the
[filesystem semantic core](Windvale-Filesystem-Semantics.md). Position-plus-
write-length overflow is rejected before provider invocation. A missing
correlation, inconsistent total/payload length, unknown operation, traversal,
wrong reference shape, nonzero deadline/reserved field, or invalid operation
control is structurally rejected.

## Response envelope

`WVFP 1` is little-endian and contains 64 through 65,600 bytes. Its header
echoes the admitted operation and correlation, then carries semantic status,
the resulting generation-safe file reference, exact length and position,
mutation progress and completion class, payload length, and one zero reserved
field. Only a successful read may carry payload bytes.

Portable validation rejects a mismatched operation or correlation, malformed
geometry, an unknown status or completion class, dirty rejection fields, an
invalid open reference, an impossible read result, an inconsistent mutation
outcome, a close that retains a file reference, or any response that violates
the shared filesystem semantic core. An admitted request therefore does not
make a malformed or provider-forged response trustworthy.

## Provider state and host translation

[`Filesystem-Provider-State.wv`](../Operating-System/Services/Filesystem-Provider-State.wv)
is the bounded first handle inventory. One active handle is charged to one
owner. Its public `u64` reference is generation-stamped and is never the native
handle or file descriptor. Read-only opens authorize only read and close;
write-capable profiles authorize the complete bounded operation set. A normal
close advances the generation before reuse. An indeterminate close or client
exit enters `Stopping` and retains the private native token until explicit
provider release completes reclamation.

[`Filesystem-Host-Adapter-Core.wv`](../Runtime/Windvale/Filesystem-Host-Adapter-Core.wv)
translates the four shared open profiles into exact Windows and Linux plans.
Windows plans select explicit access, creation disposition, sharing, and
open-reparse-point behavior. Linux plans select close-on-exec, no-follow,
read/write and create/exclusive flags plus owner-only creation mode. Both plans
require a post-open regular-file check. This is deterministic adapter policy;
the writable native syscall leaves and complete error normalization remain the next implementation
boundary.

## Evidence and limits

[`Filesystem-Service-Core.wv`](../Operating-System/Services/Filesystem-Service-Core.wv)
returns compact validation results rather than exposing a native record layout
across the trust boundary. The composed focused self-test is a 33,871-byte WVB
at SHA-256
`e2b9279e18676c1a6e3ede3a92d6dee21305c70b14e2f37826ad70b4f2637133`.
The native backend produces exact Windows and Linux executable images; the
current Windows image returns 43. Nineteen cases cover accepted and rejected
open/read/write/close exchanges, malformed request/response families,
Windows/Linux open-policy translation, capacity-one handle ownership, rights,
generation reuse, stale references, and peer-exit reclamation.

This protocol does not expose native paths or handles, follow links, invoke a
native host syscall, prove two-client queueing, or establish mutation completion
after peer loss. Those are required successor parts of the same service, not
optional claims.
