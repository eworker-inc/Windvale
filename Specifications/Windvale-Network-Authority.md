# Windvale network address and authority model

## Status and scope

Network authority profile 1 is the first implemented-candidate portable model
for numeric addresses, prefixes, ports, directions, resource limits, and rights
reduction. It is
capability-free and performs no resolution, connection, packet, host-socket, or
guest-device operation.

## Address and prefix contract

An address carries an explicit IPv4 or IPv6 family, four network-order `u32`
words, and an optional numeric interface scope. IPv4 uses only word zero and
requires the other words and scope to be zero. IPv6 link-local addresses require
a nonzero scope; non-link-local addresses reject a scope in this first profile.

A prefix is a flat family, four-word network address, and bit length. IPv4
lengths are 0 through 32 and IPv6 lengths are 0 through 128; a complete grant
validator must require every host bit to be zero. The first executable matching
primitive deliberately accepts one network-order word and 0 through 4 complete
prefix bytes. Full arbitrary-bit multiword matching remains part of this slice's
exit gate. No value inherits host byte order, native structure padding, text
normalization, or a native zone identifier.

## Authority contract

The first composable authority primitives limit:

- one separately validated numeric prefix;
- one inclusive port range;
- an explicit subset of connect, listen, datagram-send, and datagram-receive
  direction bits (`1` connect, `2` listen, `4` datagram send, `8` datagram
  receive);
- at most 1,024 concurrent connections and 16 MiB of queued bytes;
- a nonzero `u64` transfer budget; and
- an optional monotonic deadline, where zero means no authority-level deadline.

Port and direction validators reject zero, inverted, oversized, and unknown-bit
grants. Resource validation bounds connections, queued bytes, and the transfer
budget. Separate `u32`, `u64`, and deadline narrowing checks make restoration of
authority explicit. A later composed grant record must combine all these checks
with complete prefix containment before provider binding. Service-name and
resolver grants remain separate so resolving an approved name never creates
ambient numeric-address authority.

## Evidence and limits

The exact 7,813-byte self-test WVB has SHA-256
`1d3be8e490b5a7927156a57b019ce7fef2956d8793c8085f77d01afa395bf8e4`.
Its 18 address-shape, IPv6-scope, prefix-shape, byte-aligned containment, port,
direction, resource, and narrowing cases return 45 on Windows. Deterministic
Windows and Linux images have SHA-256
`bcbeaf820e970c7369a942ffb2cf407a92c3f399002c2fb478b96588986449a3`
and `95c342a6a027baec2f41aa2959cc78e855463619dd04eb4eaca9aeaa4ac73b9e`.

Arbitrary-bit multiword containment, a composed peer grant, text
parsing/formatting, IPv6 multicast scopes, service names, name-resolution policy,
interface generations, wait batches, provider IPC, host adapters, packets,
streams, secure transport, and guest networking remain successor work.
