# Decision 0722: Boot-link application-start operation 8

**Status:** Accepted
**Date:** 2026-08-16

## Context

Windvale already owned the fixed `WVSR 1` decoder, copy-before-parse leaf, and
current-process syscall context, but the retained process machine dispatched
only operations 1 through 7. Those pieces therefore had focused evidence but
were not reachable in the ordinary Probe 40 image.

The retained process code is a reviewed architecture fixture. Rebuilding the
whole fixture would enlarge the trusted migration surface; silently patching
unverified offsets would make drift unsafe.

## Decision

The Windvale-owned process-object producer checks the exact retained 68-byte
dispatch window, preserves its seven existing compare-and-branch entries, and
redirects only the former failure jump to a bounded 9-byte extension. The
extension admits operation 8 by falling through to a separately owned 183-byte
handler and relocates every other value to the exact retained failure address.
The combined 192-byte canonical WVO code section therefore changes no live
register contract for operations 1 through 7. The handler:

- admits only process 1, generation 1;
- charges the existing per-process syscall budget;
- derives the request page, process identity, and generation from GS-backed
  kernel state rather than user arguments;
- copies exactly 64 bytes through the assembled current-context and user-copy
  leaves; and
- clears the kernel snapshot on every return after the copy attempt.

The ordinary Windows and Linux process-object toolsets carry identical portable
WVB semantics. Probe 40 assembles and links both leaves explicitly, and all
three current EFI scenarios pin the resulting identities.

The added boot-linked code crosses the former 768 KiB supervisor executable
boundary. Paging version 6 expands that fixed read-only/executable window by
exactly one 4 KiB leaf to 772 KiB while retaining the seven-page hierarchy,
null guard, supervisor-only permissions, NX enforcement, and MMIO mappings.

## Consequences

Operation 8 is now present in the boot-linked machine and cannot be redirected
by a caller-supplied identity or window. The retained init image does not yet
invoke it, and an admitted snapshot does not yet construct or publish a child.
Those are separate successor changes.

The process WVO grows to 951,843 bytes. The normal, invalid-opcode, and
general-protection EFI artifacts grow to 1,692,160 bytes. The checked fixture
input remains byte-for-byte unchanged. The normal image completes the pinned
QEMU/OVMF marker and guest-controlled shutdown with paging version 6 active.

## Reconsideration triggers

Reconsider this patch boundary when the retained process fixture is replaced
by a fully source-owned process machine, when operation dispatch becomes a
generated table, or when application start supports more callers or request
profiles.
