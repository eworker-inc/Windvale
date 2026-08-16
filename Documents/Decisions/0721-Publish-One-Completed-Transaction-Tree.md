# Decision 0721: Publish one completed transaction tree

## Status

Accepted on 2026-08-16.

## Context

`WVTC 1` could finish a bounded transaction tree at depths two through eight,
but it stopped before the durable commit boundary. The older commit batch could
publish only 63 data pages, while the completed tree admits 792. Its
duplicate-obsolete-page test also decoded and hashed every earlier page for
every replacement, making the declared maximum needlessly expensive.

The persistent server needs one capability-free operation that either reports a
logical no-op or returns every byte and publication coordinate required for one
crash-safe generation.

## Decision

Windvale adds the portable transaction commit coordinator described by
`Specifications/Windvale-Database-Transaction-Commit.md`.

The coordinator runs `WVTC 1`, validates unique obsolete-page ownership,
appends one `WVCR 1` commit-log page, constructs the inactive `WVDS 1`
superblock, and returns the existing four-action publication state. It performs
no I/O. A logical no-op allocates and publishes nothing.

The commit-batch ceiling increases from 63 to 792 data pages, matching the tree
coordinator. The maximum append is therefore 793 pages including the log, or
51,970,048 bytes at the 64 KiB page ceiling.

Duplicate ownership still uses a bounded pairwise check. Each page is fully
decoded once; comparison against already validated earlier pages reads only the
fixed `WVPG 1` previous-page field. At 792 pages the worst case is 313,236
scalar comparisons rather than repeatedly hashing more than 20 GB of page
content.

## Evidence

The focused Windows development owner passes two cases:

- `TransactionCommitCapacity` accepts 64 data pages, rejects 793 before
  page-byte work, rejects duplicate ownership, and proves deterministic append
  and superblock bytes.
- `TransactionCommit` turns a depth-four, two-mutation tree into seven tree
  pages plus one log page. The target is generation 4, sequence 3, root page
  27, log page 28, 29 total pages, and 119,296 committed bytes. It validates
  the compact record, inactive slot, exact publication range, replay, and
  logical no-op.

The final rebased focused owner passed in 337,990 ms: 277,620 ms recreated the
native tool cache after the shared compiler-product refresh, 14,710 ms ran the
ordinary capacity build/package/run, and 45,330 ms ran the segmented
coordinator build/package/run. Reconstructing both applications directly with
the refreshed products reproduced the artifact sizes and hashes below.

The capacity artifacts were:

- WVB: 89,971 bytes,
  SHA-256 `bbc96caa4eccaecae41271bcfed1a11f59e773e32127bc7eaf8eb7331bf5dee9`;
- WVO: 1,810,690 bytes,
  SHA-256 `ddf564c9e7604f89453927b568aec2fc02fd54ef1fc1986132bff67915d0d76d`;
- Windows application: 1,829,888 bytes,
  SHA-256 `75cadb04e014a36713cc8b6110bd96111b8c24f0a22e6f42de180d2591e1f148`.

The composed artifacts were:

- WVB: 396,327 bytes,
  SHA-256 `d9fd5e67cb38069bff66dd4c1415f13b20d19c7d4e107bffd10bd52df5ac1b34`;
- staged WVO: 7,152,741 bytes;
- canonical image: 7,143,012 bytes, entry offset 26,383, two compiler-image
  fragments;
- Windows application: 7,173,632 bytes,
  SHA-256 `31304606aa08d74c3a6b7841130400364abae91ba2b4b4d8d57de1b8872d0154`.

A 20-run whole-process sample reports:

| Fixture | Median | Mean | Range | Sampled peak memory |
| --- | ---: | ---: | ---: | ---: |
| 64-page capacity | 189.319 ms | 196.548 ms | 185.875-270.005 ms | 9,973,760 bytes |
| transaction commit and replay | 1,637.379 ms | 1,646.963 ms | 1,593.023-1,740.012 ms | 15,118,336 bytes |

These measurements include process startup and test-side deterministic replay.
They are not persistent-server throughput.

Changed-file planning passes 24 general and 150 native routing cases. Native
development dependency closure passes all 34 declarations; shell syntax,
Windows line endings, and whitespace checks pass. Independent Linux execution
and broad qualification remain pending.

## Consequences

The portable database core can now produce one complete crash-safe publication
plan for a bounded full-depth transaction. The hosted writer no longer needs to
reconstruct tree, log, or superblock rules.

The maximum immutable append remains large, and current byte concatenation
still copies data. The persistent server must measure allocation and peak
memory, then introduce a pre-sized or streaming page sink before enabling large
transactions by default if those measurements are material.

The next milestone is a persistent hosted writer loop over one already-open
storage object with fresh selection, bounded path gathering, exact publication
execution, uncertain-result reopen/recovery, and request-level timing and memory
counters. EWDB, PostgreSQL, and SQLite comparison follows that long-lived path,
using the quiet comparison host separately from the busy development VM.
