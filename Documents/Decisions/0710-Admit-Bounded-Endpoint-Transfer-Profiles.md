# Decision 0710: Admit bounded endpoint transfer profiles

- Status: Implemented architecture-neutral policy; live x86-64 adapter pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [endpoint transfer profiles 1](../../Specifications/Windvale-Os-Endpoint-Transfer-Profile.md)

## Context

The qualified protected-process endpoint accepts at most 4,096 request and
reply bytes. The implemented FAT32 provider reply can require 4,144 bytes, and
the shared filesystem request or response can require 65,600 bytes. Launching
those providers without an explicit replacement policy would either reject
valid protocol messages, truncate them, or turn a broad unchecked buffer into
kernel authority.

## Decision

- Keep profile 1 as the existing 1..4,096-byte control-message geometry.
- Add exact block profile 2 for a 48-byte request and 4,144-byte reply window.
- Add filesystem profile 3 for complete 64..65,600-byte envelopes.
- Bind every admitted transfer to exact endpoint, caller/provider identities,
  process generations, user windows, and response capacity.
- Require checked ranges, RX sources, RW/NX destinations, non-overlap, and at
  most 17 pages per source or destination window.
- Admit a reply only from the exact provider generation and convert exact peer
  exit into a zero-byte `Peer_lost` result.

The portable policy accepts mapping booleans only as kernel-supplied evidence.
The live machine adapter must calculate those facts from page tables.

## Consequences

The filesystem and block wire formats now have a coherent bounded kernel
transfer policy, including their maximum messages. This removes the size
mismatch before machine integration and gives the x86-64 cutover an exact
reviewable contract.

No qualified boot behavior changes yet. The next slice must encode these checks
in the syscall 6/7 handler, prove bounded multi-page copies and cleanup, then
launch the immutable block provider and filesystem process through the typed
start path.

## Reconsideration triggers

Add scatter/gather, shared mappings, larger messages, concurrent calls, or
zero-copy only from a named workload and with explicit pinning, accounting,
revocation, cancellation, teardown, and cache-coherency contracts.
