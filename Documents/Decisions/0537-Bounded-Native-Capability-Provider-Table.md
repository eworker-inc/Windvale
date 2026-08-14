# Decision 0537: Bounded native capability-provider table

- Date: 2026-08-13
- Status: Implemented candidate with focused Windows execution and Linux image evidence
- Requires: [native-only forward development](0527-Native-Only-Forward-Development-Boundary.md)
- Advances: [first pre-opened random-access storage](0212-First-Preopened-Random-Access-Storage.md) and the [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [`WVPQ 1`, `WVPR 1`, and `WVPT 1`](../../Specifications/Windvale-Native-Capability-Provider-Table.md)
- Retains: Native ABI 22, execution-context version 7, service-table version 5, WVB 1.11, WVO 1.0, and every accepted hosted-tool container

## Context

Windvale source and WVB can express `storage.random_access_v1`, and the portable
database layers now own durable pages, dual-superblock publication, exact
mutation observations, and reopen/tail repair. The retired Stage 0 adapter is
only historical reference evidence. The native compiler accepts only six fixed
hosted calls, while its service-table version 5 is a closed twelve-slot list.

Adding one permanent slot for storage and another for every network, clock,
process, or device interface would turn capability identities into native ABI
slots and repeatedly version the service table. It would also fail to represent
multiple rights-limited instances. Before selecting a successor call ABI, the
runtime needs one bounded object that binds admitted capability ordinals to
opaque provider code and instance state without exposing native handles.

## Decision

- Add a portable constructor for one execution-private provider table containing
  1 through 32 canonical WVB capability identities.
- Preserve each complete name and primitive signature byte-for-byte. Reject
  malformed names, signatures, types, ordering, coverage, and trailing bytes.
- Use a 32-bit provider mask and one fixed 24-byte entry per capability ordinal.
  A selected entry requires both a nonzero opaque target and a nonzero opaque
  rights-limited state record; an unselected entry requires both zero.
- Keep the table separate from the closed ABI-22 runtime-service table. Existing
  fixed services may coexist during migration, but every call must have exactly
  one authorized binding.
- Make all offsets relative to the table and keep provider state execution-owned.
  No path, handle, descriptor, target, or state address enters portable source,
  WVB, WVO, packages, or diagnostics.
- Do not publish ABI 23 or a successor context in this decision. Decision 0151's
  reserved allocator-state and allocator-leaf fields at offsets 112 and 120 stay
  untouched. A successor ABI must integrate allocator and provider references
  explicitly rather than silently reinterpreting either plan.
- Select `storage.random_access_v1` as the first stateful provider consumer. Its
  future state owns the pre-opened object, generation, writer fence, bounded
  scratch, revocation, and test-only fault plan; this decision does not pretend
  that the Windows/Linux I/O leaves already exist.

## Evidence

The portable constructor and bridge compile through the native Project 2 front
door. The focused self-test builds to canonical WVB, lowers to a verified ABI-22
WVO, links, packages, and executes as a native Windows application with result
zero. The same source and image package as a Linux application. The test includes
the two-entry migration case, maximum 32-entry case, deterministic reconstruction,
and thirteen malformed or inconsistent request categories.

The self-test deliberately found two existing product limitations during
development: ABI-22 does not lower WVB byte equality, and the fixed hosted
packager requires a nonempty nominal-type table. The test uses a bounded explicit
byte loop and one exercised enum; neither workaround changes the production
provider-table contract. These limitations remain compiler/packager improvement
work rather than reasons to restore the retired managed path.

## Consequences

- Stateful library capabilities now have a concrete bounded native binding
  object instead of a proposed collection of special service-table slots.
- Capability authorization remains separate from provider selection and binding.
- [Decision 0538](0538-First-Native-Capability-Provider-Call-Emission.md) now
  supplies exact candidate call bytes and independent structural admission for
  direct ordinal dispatch without runtime string lookup.
- Networking can later reuse the same binding model with distinct stream,
  listener, datagram, resolver, and clock provider states.
- The database is still not a native server: main-lowerer/context integration,
  Windows and Linux random-access leaves, writer-fence lifecycle, real-file
  recovery, fault injection, transactions, networking, typed client APIs, and
  SQL remain open.

## Reconsideration triggers

Revisit the format when nominal capability signatures are required, more than 32
simultaneous identities are justified, a provider needs multiple independently
selected instances before typed capability values exist, or independent ABI
verification shows that target/state separation cannot preserve lifetime and
revocation guarantees.
