# Decision 0295: Exact staged-WVO snapshot admission

- Date: 2026-08-06
- Status: Implemented candidate; platform identity checks, transactional replacement, and grouped dual-host qualification pending
- Advances: [Decision 0293](0293-Bounded-Staged-Wvo-Content-Identity.md), [Decision 0284](0284-Versioned-Native-Object-Staging-Manifest.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0293 proves that caller-supplied staged chunks equal the retained
compiler publication sequence, but it does not own the names used to acquire
those values. A fixed native publisher must be able to call portable Windvale
admission once, then consume the same execution-owned immutable file snapshots
without reopening mutable paths or joining a potentially 32 MiB object.

The existing native `file.read_bytes` service already assigns first-successful
snapshots in exact ordinal-name order and retains at most 64 distinct names for
one execution. The missing contract is therefore the deterministic name and
ordinal plan that reserves two entries for the input WVB and manifest before
admitting bounded chunk snapshots.

## Decision

- Add `Native-X64-Lowering-Staging-Resources.wv` as a focused portable module.
  It validates the four control resource names, canonical chunk prefix, strict
  `WVOP 1` manifest, exact-name separation, and native snapshot capacity.
- Limit control resource names to 4,095 UTF-8 bytes and the chunk prefix to
  4,078 bytes. Appending `.chunk-` plus any canonical `u32` decimal index
  therefore stays within the fixed 4,095-byte platform-adapter name buffer.
- Reserve snapshot ordinal zero for the input WVB and ordinal one for the
  manifest. Chunk index `i` maps only to exact resource
  `<prefix>.chunk-<canonical-decimal-i>` and snapshot ordinal `i + 2`.
- Admit at most 62 chunks so input, manifest, and every chunk fit the existing
  64-snapshot native file-input table. Invalid plans expose zero counts; an
  invalid or out-of-range chunk query exposes `0xffffffff`.
- Reject exact-name equality among input, manifest, and destination, and reject
  any canonical chunk name equal to one of those resources. Resource names
  remain ordinal opaque names; this layer does not normalize paths or claim
  native file identity.
- Add one hosted admission root taking input, chunk prefix, manifest, and
  destination. It preflights names before I/O, reads input then manifest once,
  builds the resource/lowering/publication plans, reads each canonical chunk
  once in index order, passes that same borrowed snapshot to Decision 0293's
  content cursor, and requires complete coverage. It never writes or creates
  the destination.
- A following fixed platform adapter may call this `Main` in one native
  execution, independently validate the resulting snapshot table, and write
  the exact retained chunk descriptors into its private sibling. It must still
  anchor resources, reject native aliases, handle partial or indeterminate
  writes, replace atomically, durably flush, and clean scratch state.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 5.542 test seconds
  after an 8.42-second zero-warning Release build; the complete passing command
  takes 18.2 seconds. No broader local verification level was run.
- The resource-plan matrix covers canonical name/ordinal derivation, empty and
  oversized prefixes, exact input/manifest and chunk/control collisions,
  malformed manifests, 62-chunk success at exactly 64 snapshots, and
  63-chunk rejection before any chunk acquisition.
- The hosted admission test derives a real three-chunk WVO from compiler/object
  structure, observes the exact five-name read sequence, rejects changed code
  content, and proves both reference and current-host native execution leave
  the destination absent.
- Stage 0 and the pinned native source front door independently produce the
  exact 404,804-byte, 419-function admission WVB at SHA-256
  `294126d84b7b597e700f6e917c9c9cf6d6b0ee1479675cad4f213fcc844a5805`.
  The native result contains 347,113 code bytes.
- The resource core, admission shell, and tests are separate focused files;
  the already-large lowering and earlier content-test sources remain focused.

No C# product implementation or WebAssembly implementation changed. This
slice does not retain native file handles, prove host inode/file-ID separation,
write the final sibling, replace or clean a destination, integrate the complete
compiler tool, promote artifacts, cut over the ordinary path, or retire .NET.
Development, Standard, Qualification, Linux execution, and the grouped
end-of-goal gate remain deferred.

## Reconsideration triggers

Revisit the ordinal plan if a native execution needs another file snapshot
before staged chunks, if the file-input table capacity changes, if chunk names
stop using canonical decimal indices, or if the platform adapter cannot consume
the verified snapshot table without reopening a resource. A future staged
format that carries authenticated content identities may replace this
execution-local plan, but must retain explicit bounds and immutable evidence.
