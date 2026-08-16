# Decision 0715: Grow one bounded durable transaction root

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0714](0714-Allocate-Durable-Transaction-Ancestor-Pages.md)
- Defines: [`WVRG 1`](../../Specifications/Windvale-Database-Transaction-Root-Growth.md)

## Context

Ancestor allocation could finish an old root when its logical result fit one
page. When that old root split into multiple durable branches, however, no
general operation could build the new root. The earlier two-child growth paths
were not sufficient for up to 64 children or large separators that require
more than one new level.

## Decision

- Refactor `WVBP 1` partitioning so existing-parent rewrites and new-root child
  packing use one implementation and retain byte-identical existing semantics.
- Consume exactly one complete `WVCR 1` group with 2 through 64 consecutive
  children and require those children to be the transaction's immediately
  preceding allocations.
- Pack and allocate ordinary branch pages until one final partition remains,
  then allocate that partition as the durable root.
- Give synthetic new-level pages no previous-page identity; they do not replace
  one corresponding committed page.
- Bound growth to six rounds, 63 pages, and output depth eight. Reject the
  transaction before returning partial output when any bound is exceeded.
- Bind input replacements and all new pages to one fixed `WVRG 1` manifest and
  replay every packing round during validation.

## Evidence

The direct and maximum-bound scale-qualified projects build deterministically
to 157,898 and 156,716-byte WVBs with SHA-256
`e1047e9c92672fe19f92f003d7faea642a300989646b7ca92ac459a2014dc07b`
and `645a7efc50ff44a9b256a561c100c33c0c1e3f29d37da53efcd1fb250d6da9cc`.
Both verify through the native front door and return zero.

They lower deterministically to 2,452,485 and 2,401,841-byte WVOs with SHA-256
`91aaa533b6de83de766726f5fced7b8bd13efcdf6c3ca8789246de656d95f52d`
and `35b62d83646edf1834bda89a7a74c080168501380a54eaee5711596c15521657`.

The packaged Windows applications contain 2,470,400 and 2,419,712 bytes with
SHA-256
`513f1ead897f3335d0a983fd23f32ffc41490097789c859581042bd7f29f18b7`
and `2a85f27875586e626e6fe5ea24d08a144e3c4f80ca3ed081904fb7e40cf761c7`.
Independent repeat builds reproduced every WVB, WVO, and executable byte.

Twenty fresh sampled whole-process runs measured 124.992 and 4,125.086 ms
medians and 128.963 and 4,143.146 ms means. Sampled peak working sets were
8,343,552 and 61,038,592 bytes. The second case constructs 64 ordered
2,000-byte separators, allocates the maximum 63 pages across six rounds, and
increases depth from two to eight. It deliberately measures the contract's
worst accepted shape and exposes the immutable builder's copy cost. These are
correctness-test costs including process startup and repeated validation, not
persistent-server throughput.

After the maximum-bound fixture changed, the focused Windows root-growth owner
passed its cached direct case and fresh boundary case in 52.170 seconds. Its
fully warm run passes in 13.560 seconds, including 1.500 seconds of cached tool
setup. The fully warm broader branch-partition owner passes all 11 cases in
85.590 seconds, including 1.540 seconds of cached tool setup. It covers the
original partitioner and every parent, branch-page, ancestor-group,
ancestor-page, and root-growth consumer.

Changed-file planning passes 24 general and 147 native routing cases. Native
development dependency closure passes all 34 declarations; shell syntax and
whitespace checks pass. Independent Linux execution and broad qualification
remain pending.

## Consequences

A split old root can now grow by one or more bounded levels until exactly one
durable root exists. Root growth is no longer restricted to two children, and
large separators cannot silently produce an oversized root.

The remaining transaction-planning gap is composition: repeat `WVAG 1` and
`WVAP 1` from the first changed ancestor through level one, then select either
its completed root or `WVRG 1`. Whole-transaction page aggregation, compact-log
assembly, superblock publication, and the persistent server remain later
boundaries.

## Reconsideration triggers

Increase the child, round, page, or depth bounds only with a revised complete-
path contract and measured memory evidence. Replace immutable page accumulation
only when persistent-server measurements demonstrate material copy cost while
preserving deterministic bytes, exact validation, and atomic failure.
