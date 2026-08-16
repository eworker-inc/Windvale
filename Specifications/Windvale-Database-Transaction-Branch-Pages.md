# Windvale database transaction branch pages

## Status

- Version: `WVBD 1`
- Profile: portable
- Maximum parent groups: 32
- Maximum durable branch pages: 95
- Maximum manifest bytes: 589,824
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉbranchˉpagesˉallocate` turns the logical parent output
from `WVPP 1` into durable, checksummed `WVPG 1` branch pages. Page identities
start immediately after the transaction's replacement leaf pages. The result
also contains one generic `WVCR 1` plan that can replace the old branches in
the next ancestor level.

`Databaseˉtransactionˉbranchˉpagesˉplan` is the integrated entry point. It
runs parent grouping once and then allocates its durable output. Neither entry
point writes storage or publishes a superblock.

## Allocation rules

The input root depth is 2 through 128. A changed plan allocates the `WVLD 1`
leaf pages first and then assigns consecutive identities to every `WVPP 1`
output branch. The first replacement branch for each old parent records that
parent as its previous-page identity; later split branches use no previous
page. Every page uses the leaf plan's next generation and commit sequence.

At depth two, exactly one parent group must replace the current root. When its
final state fits one page, that page is encoded as a durable root and
`Rootˉready` is true. If it splits, all output pages remain ordinary branches
and their `WVCR 1` result must be consumed by the later root-growth step. At
depth three or deeper, all output pages are ordinary branches and the same
replacement shape is consumed by the next ancestor pass.

An unchanged parent plan produces a valid manifest with no page identities,
page bytes, or replacement bytes.

## `WVBD 1` encoding

The manifest uses a 160-byte little-endian header followed by one complete
`WVCR 1` payload:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVBD` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `160` |
| 12 | `u32` | flags: bit 0 changed, bit 1 root ready |
| 16 | `u32` | durable page size |
| 20 | `u32` | input root depth |
| 24 | `u32` | parent-group count |
| 28 | `u32` | durable branch-page count |
| 32 | `u32` | durable replacement leaf-page count |
| 36 | `u32` | replacement-plan byte length |
| 40 | `u64` | target generation |
| 48 | `u64` | target commit sequence |
| 56 | `u64` | first transaction allocation page |
| 64 | `u64` | first branch page |
| 72 | `u64` | completed root page, or no-page sentinel |
| 80 | `u32` | companion `WVPP 1` byte length |
| 84 | `u32` | companion durable-page byte length |
| 88 | `u32` | total manifest length |
| 92 | `u32` | reserved, zero |
| 96 | 32 bytes | SHA-256 of the complete `WVPP 1` companion |
| 128 | 32 bytes | SHA-256 of the complete page companion |

The manifest deliberately binds, but does not duplicate, its potentially
multi-megabyte parent-group and page companions. Decoding validates the
manifest and embedded replacement plan. `validate` additionally requires the
exact companion lengths and hashes, decodes every parent partition and
durable page, and checks page identity, generation, sequence, kind, previous
page, item count, payload, separator, and aggregate counts.

## Failure and bounds

Invalid leaf or parent plans, inconsistent generations or counts, exhausted
page identities, malformed partitions or replacements, page-encoding
failure, invalid manifests, digest mismatch, and invalid durable pages fail
atomically with no partial result.

At most 32 parent groups produce at most 95 branch pages. Each parent group
produces at most 64 pages, and the complete `WVCR 1` level may contain 95
children. The manifest is capped at 576 KiB, its parent companion at 3 MiB,
and its replacement payload at 512 KiB. Page bytes are exactly page count
times the validated 4 through 64 KiB page size. Checked page-identity
arithmetic rejects wraparound.

The current immutable byte builder can copy while assembling pages. The
persistent-server benchmark milestone must measure this cost and peak memory
before selecting a pre-sized arena or streaming writer.

## Verification and next step

Three focused native executables keep each native object below the existing
4 MiB code limit. They cover deterministic depth-two root completion,
depth-three multi-parent allocation, unchanged output, exact page and route
metadata, truncated and oversized manifests, bad magic and version,
inconsistent path/depth input, companion digest mismatch, and a digest-bound
but invalid durable page. `WVCR 1` also has an explicit 64-child group and
95-child level boundary test.

`WVAG 1` now consumes the emitted replacement plan at any remaining ancestor.
The next milestone assigns durable identities to that logical output and adds
the bounded grouping/allocation loop, including new-root construction, before
compact-log and superblock publication.
