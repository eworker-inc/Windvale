# Decision 0535: First durable database commit

- Date: 2026-08-13
- Status: Implemented candidate
- Contract: [Windvale Database durable pages and commit publication](../../Specifications/Windvale-Database-Durable-Commit.md)
- Builds on: [Decision 0534](0534-First-Durable-Database-Superblock.md), [`WVDS 1`](../../Specifications/Windvale-Database-Durable-Superblock.md), and [`storage.random_access_v1`](../../Specifications/Random-Access-Storage-Capability.md)

## Context

The first dual superblock can select one committed generation and identify an
unpublished tail, but it previously had no accepted physical page, log record,
or executable ordering rule. Without those contracts, a future writer could
publish a root before its pages are durable, reinterpret unused page bytes, or
blindly replay a mutation whose completion is uncertain.

The reviewed EWDB implementation provides useful design evidence: immutable
copy-on-write pages, a compact commit history, and durable bytes before root
publication. Its C# object graph and .NET filesystem calls do not define the
Windvale format. The new boundary must stay portable, capability-free,
versioned, bounds checked, and independently executable through the native
Windvale toolchain.

## Decision

Adopt three connected contracts:

- `WVPG 1`, a fixed-size page with a 128-byte checksummed envelope, a checksum
  over only the used payload, and canonical zero padding;
- `WVCR 1`, an exact 256-byte commit record that links previous and new
  generation/sequence state, names one contiguous append extent, requires a
  new copy-on-write root, and places its commit-log page last; and
- a pure publication state machine that validates exact agreement among the
  selected superblock, target superblock, and commit record before planning
  append, content-and-length flush, inactive-slot write, and content flush.

Reject reports no progress. A rejection before the final flush safely aborts;
rejection of that final flush, exact partial writes, and indeterminate
mutations require reopen and recovery. None may be retried by the planner. The
previous selected generation remains authoritative only while recovery cannot
select a completely durable newer slot.

Keep the codecs and planner free of capabilities. The later hosted or Windvale
OS writer will consume their plans through a rights-limited random-access
storage binding and must preserve the same outcome vocabulary and ordering.

No C#, .NET, managed fallback, native pathname, or platform filesystem
behavior is introduced.

## Consequences

- Windvale now has an executable, byte-exact durable-before-publish transition
  rather than only prose ordering.
- Corruption in header, used payload, padding, commit linkage, or allocation
  arithmetic fails closed before a page or commit becomes trusted.
- Each commit appends at least a new root page and a final log page. This is a
  deliberate copy-on-write baseline; reclamation and compaction are separate.
- An aborted append can leave a tail, but dual-superblock recovery does not
  make that tail reachable.
- A capability-bearing writer, crash injection, reopen/truncation policy,
  B+tree payloads, transactions, networking, and human SQL remain future
  milestones.

## Evidence boundary

The portable library projects compile through the native Project 2 front
door. The focused owner runs 12 cases covering page and commit round trips,
all exposed header classes, checksum and padding corruption, bounded semantic
relationships, alternating slot selection, the complete publication path,
cross-record disagreement, rejection, exact partial progress, invalid
progress, and indeterminate recovery.

It also proves deterministic WVB and WVO construction, WVO verification,
pinned flat-image and Windows/Linux hosted identities, local execution, and
other-host image construction. GitHub remains responsible for independent
dual-host execution before promotion.

## Reconsideration triggers

Revisit this decision if fault injection disproves the selected flush mapping,
if the first B+tree node codec cannot fit the envelope without weakening its
validation order, if group commit requires a different sequence-to-generation
relationship, or if a qualified provider cannot preserve the two durability
barriers. Any revision must version changed bytes and retain an explicit
recovery and migration rule.
