# Decision 0689: Copy application-start values before admission

- Status: Implemented architecture-neutral native candidate; x86-64 syscall integration pending
- Date: 2026-08-16
- Advances: [Decision 0616](0616-First-Checked-Application-Start-Request.md)
- Contract: [application-start user-copy policy 1](../../Specifications/Windvale-Os-Application-Start-User-Copy.md)

## Context

`WVSR 1` defined the serialized application-start value, but a decoder alone
cannot safely consume bytes that remain controlled by an untrusted process.
The kernel also cannot trust the caller reference encoded by that process as
proof of the active caller's identity.

## Decision

- Require an exact 64-byte copy into an immutable kernel-owned value before
  request decoding or typed launch admission.
- Validate the admitted window and requested buffer with subtraction-first
  bounds checks before slicing.
- Treat size, window, and range failures as rejected zero-reference launch
  transitions.
- Derive the current caller outside the request and compare it with the encoded
  caller before admission. Reject a mismatch as `Invalid_caller`.
- Keep page-table walking, fault recovery, syscall registers, completion, and
  cancellation in a later x86-64 adapter rather than pretending a portable
  bytes value proves those mechanisms.

## Evidence and consequences

The policy WVB is 8,313 bytes at SHA-256
`bd26f61a20867452a99f3db4d3c58988585656387ec93e424d965100a6134247`.
The 11,695-byte test at SHA-256
`0ce6389c99c30c4875cb06da9bedb44d2f832d7b39898647f7a119ad82830f45`
passes nine copy, bounds, identity, and malformed-input cases. The focused
application-launch owner therefore advances from 32 to 41 cases.

This closes the architecture-neutral copy-before-parse invariant. It does not
publish a syscall, prove live user-page access, allocate a process dynamically,
or make arbitrary application images launchable.

## Reconsideration triggers

Reconsider the exact-size-only interface when a new start-request version is
defined. Any architecture-specific optimization must preserve an immutable
snapshot and must not derive authority from caller-controlled bytes.
