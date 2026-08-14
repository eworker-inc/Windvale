# Decision 0548: First durable tree node and upsert

- Date: 2026-08-14
- Status: Implemented candidate with focused Windows and clean changed-file evidence
- Requires: [Decision 0547](0547-First-Native-Single-Writer-Transaction.md)
- Defines: [`WVTN 1`](../../Specifications/Windvale-Database-Tree-Node.md)

## Context

The first native transaction deliberately accepted an opaque root payload. It
proved durability but not database behavior: no durable key ordering, lookup,
replacement, or copy-on-write insertion was defined. The old `WVDB 1`
experiment used fixed `u32` keys, `i32` values, 256-byte pages, and a different
non-durable envelope, so carrying it forward would have made an experiment the
storage engine's semantic contract.

## Decision

- Define a new `WVTN 1` payload inside `WVPG 1`; do not revise or migrate the
  experimental `WVDB 1` bytes.
- Use variable byte keys and values. Require callers to own typed encoding and
  collation while the tree owns unsigned lexicographic ordering.
- Admit the complete `WVPG 1` payload range through 65,408 bytes, with at most
  4,096 entries, 4,096 key bytes, and 61,440 value bytes. Enforce the selected
  page's narrower exact ceiling during mutation.
- Reserve leaf and branch shapes now. A branch separator owns its left `u64`
  child and the header owns the rightmost child. Implement local branch decode
  only; defer global graph validation and branch mutation.
- Treat the empty generation-1 root as a bootstrap exception. The first insert
  produces a canonical nonempty node; do not make arbitrary empty bytes a node
  encoding.
- Implement leaf lookup and upsert as portable pure functions. Replacement
  preserves count; insertion preserves strict order; a full page is a typed
  outcome.
- Compose upsert only after the decoded root agrees exactly with the selected
  generation. Publish through the existing immutable root/log allocation and
  four-action transaction; never update the old root in place.
- Validate the structured node after native restart before issuing the next
  provider read, preserving the borrowed-response lifetime rule.

## Evidence

After the final bootstrap-empty-root hardening, the portable tree-node library,
single-leaf composition, and 107-function fixture compiled in 0.258, 2.521,
and 3.045 seconds. The fixture lowered in 2.959 seconds to a valid
2,251,372-byte WVO and executed in 1.169 seconds. It covers format, ordering,
lookup, replacement, capacity, malformed input, branch shape, first commit,
second-generation insertion, and deterministic bytes.

The composed host compiled in 4.278 seconds. Its cached Windows lifecycle
passed create, structured upsert, interruption after zero through four actions,
recovery, structured lookup, commit-log validation, and stable reopen in
111.420 seconds. The clean changed-file gate then passed the 12-case durable
commit, 11-case database-storage, 8-case Project 2 workspace, and 26-case
library owners in 876.195 seconds. Independent Linux execution and GitHub's
dual-host qualification remain pending.

## Consequences

- The durable path now commits database content rather than an opaque test
  byte.
- Variable byte keys can support future catalog, row, and secondary-index
  codecs without coupling storage to SQL types.
- Root-leaf replacement and insertion are real, but capacity currently returns
  `Full`; it does not yet split or create a branch root.
- Branch bytes have a canonical local contract before traversal and mutation
  depend on them.

## Reconsideration triggers

Version the node format before adding prefix compression, slot directories,
overflow values, sibling links, alternate collation metadata, per-node
checksums, or a different branch separator model. Reconsider the fixed limits
only with measured page-cache, compiler, and provider evidence.
