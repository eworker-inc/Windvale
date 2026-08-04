# Decision 0207: `u64` binary fields for durable storage

- Date: 2026-08-04
- Status: Implemented candidate; dual-host qualification pending
- Adds: WVB 1.12 opcodes `0xBD` and `0xBE`
- Retains: Exact lower-minor WVB output when neither operation is used, `u32`
  module counts and byte-value indices, and the bounded experimental `WVDB 1`
  reader
- Advances: [Windvale database proposal](../Project/Windvale-Database-Proposal.md),
  [Seed bytecode](../../Specifications/Seed-Bytecode.md), and
  [Foundation binary primitives](../../Specifications/Foundation-Bytes.md)

## Context

The experimental database reader uses `u32` byte positions and page identities
inside a complete immutable image of at most 16,416 bytes. Those widths are
correct for that experiment but are not the intended durable page-file
contract. A `u32` byte position stops below 4 GiB, and computing a byte position
from even a `u32` page identity requires a wider checked intermediate once the
file can exceed that boundary.

Stage 0 already compiles, verifies, and interprets checked `u64` values under
WVB 1.7, but Windvale source had no exact operation for reading or constructing
an eight-byte little-endian field. Database code would otherwise need an ad hoc
pair of `u32` fields and would risk freezing that temporary representation into
a durable format.

## Decision

- Add `Bytesˉreadˉu64ˉlittle(bytes, u32) -> u64` and
  `Bytesˉfromˉu64ˉlittle(u64) -> bytes` as pure Foundation intrinsics.
- Assign WVB 1.12 opcodes `0xBD` and `0xBE`. The canonical writer selects 1.12
  only when either opcode occurs; every existing module continues to select its
  lowest sufficient minor and retains its exact bytes.
- Require the read to validate the complete eight-byte range before access and
  trap as `WVR3008` on failure. The constructor returns exactly eight
  little-endian bytes.
- Use `u64` in the future durable database contract for byte offsets, file
  lengths, page identities, generations, commit sequence numbers, WAL
  positions, and mutation/idempotency identities. Retain narrower widths for
  bounded counts and fields whose product contract is intentionally narrower.
- Treat page identity and byte offset as different domains. Every conversion
  from a page identity to `header + page_id * page_size` must use checked `u64`
  arithmetic and reject a value outside the bound storage instance.
- Do not revise the experimental `WVDB 1` format. A durable successor receives
  a new format identity only after its storage, commit, checksum, recovery, and
  malformed-input contracts are selected together.

## Current boundary

The Stage 0 compiler, WVB verifier, inspector, and reference runtime implement
the two operations. The Windvale-written compiler, native x86-64 backend,
WebAssembly profiles, and Windvale OS execution profiles do not yet implement
the WVB 1.12 operations. This change makes the durable field representation
executable through the reference path; it does not claim a writable database,
general filesystem, native database process, or cross-target `u64` parity.

## Evidence

The focused conformance case compiles a portable WVB 1.12 module, checks exact
little-endian construction, reads `4,294,967,296` and
`18,446,744,073,709,551,615`, round-trips canonical module bytes, inspects both
opcodes, rejects a 1.12 module relabeled as 1.11 with `WVB2107`, and traps a
seven-byte read with `WVR3008`.

## Consequences

Future database formats can use one canonical scalar field instead of a
format-specific high/low pair. WVB section counts, code offsets, byte-value
indices, and the `bytes` length remain `u32`; adding a durable storage scalar
does not silently widen unrelated contracts.

The next database-storage capability can accept `u64` positions from its first
version. It still requires typed provider outcomes, bounded chunks, exact
partial-progress evidence, flush classes, fencing, and recovery semantics
before mutation is implemented.

## Reconsider when

- An intended execution target cannot preserve exact checked `u64` behavior.
- A database design deliberately caps page identity below `u32` and has a
  measured migration or recycling contract that makes a wider identity harmful.
- A future binary-value model replaces individual fixed-width intrinsics with
  an equally exact portable codec abstraction.
