# Decision 0534: First durable database superblock

- Date: 2026-08-13
- Status: Implemented candidate
- Contract: [Windvale Database durable superblock](../../Specifications/Windvale-Database-Durable-Superblock.md)
- Builds on: [checked random-access database pages](../../Specifications/Windvale-Database-Storage-Page.md) and [random-access storage](../../Specifications/Random-Access-Storage-Capability.md)

## Context

Windvale Database already has checked `u64` page geometry, immutable page
admission, a typed random-access storage contract, and a narrow native hosted
page seam. It did not have durable bytes that distinguish a committed root
from an arbitrary file tail, or a recovery rule that survives a torn root
publication without depending on atomic host-file replacement.

The reviewed EWDB design supplies an important invariant: durable pages must
precede publication of the root that makes them reachable. Its C# class shape
and .NET filesystem APIs are not the Windvale contract. The new boundary must
remain portable, capability-free, checked as untrusted input, and usable by
the same future engine on Windows, Linux, and Windvale OS.

The implementation also exposed one native compiler gap. Canonical WVB 1.11
already defined `bytes.from_u64_little`, but the x64 selector admitted only the
matching read operation. A durable format cannot claim native execution while
its encoder depends on an unsupported bytecode operation.

## Decision

Adopt `WVDS 1` as the first durable Windvale Database record:

- reserve two 256-byte slots at offsets 0 and 256;
- begin database pages at offset 512;
- identify one database with an opaque 128-bit value represented by two `u64`
  fields;
- store generation, commit sequence, root, commit-log head, retention floor,
  page count, and committed length as `u64`;
- restrict page size to a closed power-of-two set from 4 KiB through 64 KiB;
- reserve bytes 96 through 223 as zero;
- checksum bytes 0 through 223 with raw SHA-256; and
- require committed length to equal `512 + page_count * page_size` under
  checked `u64` arithmetic.

Recovery validates both slots independently, rejects a record that extends
beyond observed storage, requires equal database identity, selects the greater
generation, and rejects non-identical records that claim the same generation.
Bytes after committed length are reported only as an unpublished tail.

Implement `bytes.from_u64_little` in the Windvale-written native x64 lowerer
and add a direct native round-trip fixture. Refresh the reproducible segmented
compiler and WVB-to-WVO candidates and every exact consumer pin. No C#, .NET,
managed fallback, or alternate semantic path is introduced.

## Consequences

- A future writer can publish inside one pre-opened object without assuming
  cross-platform atomic pathname replacement.
- A torn inactive slot leaves the previous valid generation recoverable.
- Same-generation disagreement and cross-database slot mixing fail closed.
- The record commits only bounded root metadata. Follow-on
  [Decision 0535](0535-First-Durable-Database-Commit.md) now owns page envelopes,
  compact log records, and publication planning; reclamation, transactions,
  and a server remain future work.
- The human SQL layer remains above typed engine and transaction contracts. It
  is not an on-disk format and does not become the machine service protocol.
- Native compiler candidate identities change because the missing `u64` byte
  constructor is now implemented and covered.

## Evidence boundary

The portable library and hosted self-test cover valid encoding and decoding,
the exact golden checksum, truncated and malformed headers, reserved bytes,
checksum corruption, semantically forged but correctly checksummed fields,
all bounded field relationships, checked length overflow, single-slot
recovery, generation ordering in both slots, identical generations, conflicting
generations, conflicting database identities, unpublished tails, and storage
shorter than the declared commit.

The focused native owner runs 13 independently budgeted cases, compares two
WVB builds and two WVO lowerings byte-for-byte, verifies the WVO, pins linked
and hosted artifacts, executes all cases on the local host, and constructs the
other host image. GitHub remains responsible for independent Windows and Linux
qualification before promotion.

## Reconsideration triggers

Revisit this decision if crash injection shows that the storage flush contract
cannot preserve page-before-superblock ordering, if the first page/log format
cannot express recovery from the committed fields, if database identity needs
authenticated binding, or if a qualified provider requires a different atomic
publication unit. Any revision must version the durable bytes and retain an
explicit recovery and migration rule.
