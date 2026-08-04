# Decision 0212: First pre-opened random-access storage

- Date: 2026-08-04
- Status: Implemented candidate with focused Windows evidence; independent Linux and crash-recovery qualification pending
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md), [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md), and [Decision 0211](0211-U64-Database-Storage-Geometry.md)
- Defines: [`storage.random_access_v1`](../../Specifications/Random-Access-Storage-Capability.md)
- Retains: experimental `WVDB 1` bytes, no database writer, no accepted durable format, no ambient filesystem authority, and no publication/rename guarantee

## Context

Windvale now has `u64` durable fields and checked page geometry, but the hosted
database path can only consume an eager immutable `u32` directory snapshot.
The first writer cannot safely inherit `.NET`, Win32, POSIX, or a future
Windvale OS filesystem API: reads, partial writes, uncertain mutations,
provider restart, flush strength, and writer ownership need one semantic
contract.

The current WVB metadata can declare a capability requirement but cannot carry
multiple typed resource values. Splitting describe, read, write, resize, and
flush into unrelated capability names would not prove that they operate on the
same storage object or generation.

## Decision

- Add one capability identity, `storage.random_access_v1`, with typed scalar
  request fields and a strictly validated `WVSA 1` response.
- Bind exactly one pre-opened object per current runtime context. Windvale code
  receives no path, handle, file descriptor, or ambient namespace.
- Use `u64` generation, position, and storage-length fields and a 65,536-byte
  transfer ceiling.
- Provide describe, exact read-at, positioned write, resize, and content or
  content-and-length flush operations behind one typed Windvale platform
  library.
- Require a nonzero generation from describe and exact generation fencing on
  every later operation. Treat stale, revoked, peer-exited, and unavailable as
  distinct typed outcomes.
- Distinguish rejected-before-change, exact partial write progress, completed
  mutation, and indeterminate mutation. Never quantify an indeterminate write
  or permit a caller to infer safe replay.
- Keep native-path publication, parent-directory durability, atomic
  replacement, append identities, and transactions outside version 1.
- Add one shared Stage 0 Windows/Linux provider and
  `windvale run --bind-random-access-storage <path>`. Open an existing ordinary
  file, retain one whole-file writer lease for the run, permit readers, use
  generation `1`, and release the binding at teardown. Keep `--allow` separate.
- Map both admitted flush classes to `FileStream.Flush(true)` in the reference
  adapter without claiming parent-directory durability or independently
  qualified power-loss behavior.
- Trap malformed or inconsistent providers as `WVR3031`; reject invalid source
  requests before invoking a provider.

## Library organization

The implementation preserves the intended hierarchy:

- `Libraries/Platform/Storage/Random-Access-Storage.wv` owns the application-
  facing capability adapter;
- `Libraries/Database/` owns capability-free database formats, page geometry,
  indexing, and later recovery algorithms;
- source imports use a concise explicit alias such as
  `import Randomˉaccessˉstorage as Storage;`; and
- repository paths express ownership and platform scope while future package
  metadata will supply globally unique part identities.

The storage library does not import or name WVDB. A future WVDB storage adapter
depends on Storage, preserving one-way ownership.

## Evidence

The focused Windows test compiles the library and a hosted consumer twice and
compares exact WVB bytes. The consumer describes one object, performs exact and
end-bound reads, rejects an oversized request without provider dispatch,
observes a stale generation, writes and resizes, flushes content and length,
and distinguishes outside-storage.

The runtime test separately proves missing authorization, missing binding,
revocation, exact partial progress, indeterminate completion, invalid partial
progress rejection, corrupt-envelope rejection, and invalid provider
containment. The real CLI binds an ordinary temporary file, executes the same
compiled consumer, verifies the resulting bytes, and proves teardown releases
the host writer lease.

## Consequences

Windvale has its first mutable `u64` storage resource and a concrete shared
Windows/Linux adapter. Database code can now address pages beyond the `u32`
range without learning host paths or assuming native file-cursor behavior.

This is necessary but insufficient for a database writer. Generation `1` is
only a fence for one Stage 0 process lifetime. Linux locking remains advisory
against non-cooperating host processes. Independent Linux evidence, crash-
injection, mutation identities, internal commit publication, recovery, page
ownership, caching, WAL policy, and concurrency remain later milestones.

## Reconsider when

- WVB admits typed capability values and multiple bound instances;
- a restartable provider requires durable generation or mutation identities;
- a real database workload needs a different bounded transfer profile;
- Windows and Linux cannot satisfy the same stated flush class under measured
  failure injection; or
- a Windvale OS storage service supplies a stronger object-native contract.
