# Decision 0210: First hosted WVDB snapshot consumer

- Date: 2026-08-04
- Status: Implemented candidate with focused Windows evidence; independent Linux qualification pending
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md), [WVDB reader experiment](../../Specifications/Windvale-Database-Reader.md), and [read-only directory capability](../../Specifications/Read-Only-Directory-Capability.md)
- Retains: experimental `WVDB 1` bytes, its `u32` fields and 16,416-byte bound, one explicitly granted immutable directory instance, and no durable or mutable storage claim

## Context

The portable `Windvaleˉdatabaseˉreader` validates and searches one bounded byte
snapshot, while `filesystem.directory_read_v1` exposes a real rights-limited
immutable directory on the reference Windows/Linux launcher. Applications
could compose those pieces manually, but no Windvale-owned library joined the
provider result, bounded chunk assembly, database validation, and typed lookup
boundaries.

Promoting the reader's 256-byte page experiment into a durable format would be
premature. The useful next pressure is instead a complete read-only hosted path
that keeps provider failures distinct from invalid database bytes and exercises
explicit transitive capability approval.

## Decision

- Add `Libraries/Platform/Database/Read-Only-Wvdb.wv` as hosted module
  `Readˉonlyˉwvdb`.
- Import the hosted `Readˉonlyˉdirectory` adapter and portable
  `Windvaleˉdatabaseˉreader`; redeclare `filesystem.directory_read_v1` so every
  importing application must approve the complete transitive requirement.
- Export `Wvdbˉsnapshotˉlookup(Name: text, Key: u32)` with a typed payload result
  that distinguishes `Found`, `Missing`, `Storageˉfailure`, and
  `Databaseˉfailure`.
- Read exact chunks of at most 3,072 bytes, reject a file above 16,416 bytes
  after the first successful response, and assemble at most six chunks. Verify
  stable file length, strict forward progress, and exact final length before
  invoking the database reader.
- Preserve provider lifecycle and request failures as a dedicated storage
  status. Preserve the reader's exact `Wvdbˉfailure` separately; malformed
  database bytes are not reported as provider failure.
- Keep `WVDB 1` experimental. This path does not add writes, handles, caching,
  transactions, flush, recovery, native paths, Windvale OS binding, or a new
  durable format identity.

## Evidence

The focused Windows Seed test composes the adapter in both dependency orders,
verifies one canonical capability requirement, and executes it through the
reference runtime. It proves a two-chunk found lookup, a six-chunk maximum-size
missing lookup, malformed database separation, provider `Not_found`, immediate
oversized rejection after one provider call, and rejection when a later chunk
reports a different immutable length.

## Consequences

Windvale now has one concrete hosted database-consumption path using ordinary
Windvale modules and a rights-limited provider. It remains suitable for small
catalogs, demonstrations, and inspection workloads only. Immutable concatenation
copies a bounded cumulative amount of data across at most six chunks; a page
cache or streaming durable reader is neither required nor implied.

The next database milestone moves into Stage 2 prerequisites: select a real
product consumer and specify the first pre-opened storage-object/resource
contract with `u64` positions, typed lifecycle outcomes, and exact mutation and
durability semantics before implementing writes.

## Reconsider when

- the experimental reader limit changes;
- the directory capability gains multiple typed instances or independent
  interface-version metadata;
- a real consumer requires lookup without materializing the complete snapshot;
  or
- the first durable page-storage contract supersedes this bounded adapter.
