# Decision 0616: First checked application-start request

- Status: Implemented current-host native candidate; syscall and cross-host qualification pending
- Date: 2026-08-15
- Advances: [process launch and supervision](../Architecture/Process-Launch-And-Supervision.md)
- Contract: [`WVSR 1`](../../Specifications/Windvale-Os-Application-Start-Request.md)

## Context

Application-launch policy 1 accepted typed scalar arguments, but an untrusted
process cannot safely supply those values without a bounded serialization and
validation boundary. Extending the fixed Probe 40 supervisor object with a full
decoder would also consume its remaining RX margin before a real syscall or
dynamic provider exists.

## Decision

- Define `WVSR 1` as one exact 64-byte little-endian value with no offsets,
  pointers, handles, paths, or variable-length regions.
- Validate outer size before every field read, then magic, version, encoded
  size, reserved fields, role, references, resources, and bindings.
- Admit only the measured application role and exact profile-1 charges. Reject
  service roles until their executable and endpoint profiles are explicit.
- Pass only a structurally valid value to application-launch policy 1. Return a
  rejected zero-reference transition for malformed serialization.
- Keep this decoder independently executable until the checked syscall copies
  the value into kernel-owned memory and can call it without weakening the
  fixed supervisor window.

## Evidence and consequences

The decoder WVB is 6,555 bytes at SHA-256
`1c30a368dbe8a1f233f652fb9211d8f85273fdc09716ec2559fd5b3b1c91f90a`.
Its focused test is 9,339 bytes at
`c71718f52bce5924b1645183930be05d6d38b8d632b03105d155c16ffef52229`.
The launch owner passes 20 cases across the typed transaction, machine policy,
and serialized request behavior.

This freezes the first user/kernel data shape but does not publish a syscall,
copy user memory, allocate objects, launch arbitrary code, transfer
capabilities, launch a provider, or change the live Probe 40 identity.

## Reconsideration triggers

Add a new version rather than widening version 1 when the first filesystem or
network provider executable, endpoint charge, and initial capability set are
published. Reconsider the separate compiled owner only when the live syscall
integration has measured code and memory budgets.
