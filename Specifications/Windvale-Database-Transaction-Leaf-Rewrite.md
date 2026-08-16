# Windvale database transaction leaf rewrite

## Status

- Version: transaction leaf rewrite 1
- Profile: portable
- Input: one valid `WVTN 1` leaf and one canonical `WVTM 1` mutation set
- Evidence: focused Windows native execution; independent Linux execution pending

## Contract

`Databaseˉtransactionˉleafˉrewriteˉapply` is the first tree-planning
consumer of a complete transaction mutation set. It validates and owns the
mutation set once, applies its sorted puts and deletes to an in-memory leaf,
and returns one final leaf only after every mutation succeeds.

The result reports whether bytes changed, how many puts were evaluated, how
many present keys were deleted, how many mutations changed logical state, and
the final entry count. A missing delete is successful but does not increase the
applied or deleted count. A put is applied even when it replaces an existing
key.

If framing, leaf validation, a mutation, or the maximum payload fails, the
result contains no leaf bytes and all counts are zero. A caller cannot publish
an intermediate leaf from this API.

## Bounds and performance

The input inherits the `WVTM 1` limits of 32 mutations and 256 KiB. Keys and
values inherit `WVTN 1` limits. The maximum leaf payload is explicit and
validated by every tree operation.

The reference rewrite decodes the mutation set once and uses the owned decoded
record for indexed reads, avoiding one full mutation-set copy per operation.
It currently applies each mutation through the individually verified tree
operation, so worst-case work is 32 bounded leaf rewrites. The future grouped
path planner may replace this with one merge pass if measurements justify the
extra implementation, without changing this result contract.

## Verification

The focused self-test proves deterministic four-operation rewrite, replacement,
present and missing delete, insertion, exact final lookup values and counts, a
byte-identical all-missing delete result, invalid mutation and leaf rejection,
and all-or-nothing output when a later put exceeds the payload ceiling.

## Exclusions and next step

This component does not split a full leaf, read storage, group keys by durable
path, rebuild branch ancestors, publish pages, or claim multi-record durability.
The next planner groups the sorted mutation stream by selected leaf, uses this
rewrite where no split is needed, handles bounded split output, then rewrites
each shared ancestor once before one `Commit-Batch` publication.
