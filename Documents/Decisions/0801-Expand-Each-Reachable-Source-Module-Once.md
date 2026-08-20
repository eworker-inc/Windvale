# Decision 0801: Expand each reachable source module once

- Status: Accepted
- Date: 2026-08-20

## Context

Source-graph reachability is a bounded fixed-point walk over an immutable WVSS.
The prior state byte distinguished only unseen from reachable. When an import
discovered a module earlier than the current scan position, another pass was
required, and that pass reparsed every already reachable module before reaching
the new work.

The real 14-module compiler analysis graph has exactly this shape. Its first
pass expands 13 modules, then its second pass expands all 14 again even though
only one module is new. That repeated leading-import parsing sits on every
compiler analysis path and is independent of the later adjacency-directory and
cycle checks.

## Decision

1. Retain one bounded byte of reachability state per WVSS module.
2. Give the byte three internal meanings: `0` unseen, `1` pending expansion,
   and `2` completely expanded.
3. Mark a newly discovered module pending. After all of one pending module's
   leading imports pass the existing checks, mark that module complete.
4. Visit only pending modules in later passes. Do not retain tokens, syntax
   trees, host paths, or another graph-sized allocation.
5. Preserve the existing public graph summary, WVSS bytes, failure ordering,
   immediate discovery behavior, pass bound, adjacency directory, and cycle
   diagnostic walk.
6. Preserve the existing two-argument `Compilerˉsourceˉgraphˉmark` helper for
   ordinary one-byte bitsets. Perform the one completion write directly inside
   reachability expansion so source-symbol visibility does not inherit the
   three-state meaning or another function.
7. Add a valid canonical-order demo graph where a later importer discovers an
   earlier dependency. Keep compiler-local verification focused; do not rerun
   storage, OS, or Qualification workloads for this checkpoint.

## Evidence

The exact analysis-driver graph reports 14 modules and 41 imports. By source
order, the original walk performs 27 expansion visits across two passes; the
candidate performs 14, a 48.1% reduction.

The original and candidate focused source-graph tools were built with the same
accepted analyzer and target-aware emitter. Four interleaved warmed runs on
Windows 11 build 26200 and an AMD Ryzen 9 3900X produced:

| Implementation | Mean ms | Median ms | Functions | Code bytes | WVB bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reachable/unseen state | 3,784.766 | 3,773.672 | 149 | 295,816 | 364,759 |
| Pending/complete frontier | 3,281.864 | 3,286.330 | 149 | 295,939 | 364,903 |

The mean falls by 13.3% and the median by 12.9%. Both tools exit zero and print
the exact same `Valid` report with 14 reachable modules and 41 imports.

The final candidate analyzer is 1,071,235 bytes with SHA-256
`52feeed48b2526441d36a2335e50ffe26b6974c82255f367a7f3f0e62e3e9cec`.
It deterministically produces the 838,798-byte target-aware emitter with
SHA-256 `e40da70ba3cf1ef85193bd5b2fe2657faf0068d5951cb36f232d80ec7f7223fe`.
The accepted and candidate analyzer applications publish byte-identical current
WVSS, capability-manifest, binding-directory, and WIR files:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVSS | 1,362,838 | `13e507aefdeb8e92d7a17cf5803b9f2f48588607f16071886303840688bc1abe` |
| WVCA | 104 | `c3598fa33170861cdbee932ea413dd1028aecf6f2666a3cdeb41c7f553e34d82` |
| WVLB | 197,756 | `25a22cfd6a73f135d697e00c3a60e3882245b8c320bcaa9068194f4993ba4285` |
| WVIR | 3,253,600 | `cc8198e4cd986071ebeea8a95f97b8ab2fb987bcac1e42b71ce8dace48c423dd` |

The canonical delayed-frontier demo compiles to a 368,739-byte WVB with
SHA-256 `03b3845762a29207631fb8ee74f77c5ce81d696e981027f65307cf2ea06415a4`
and returns zero through its generated native executable.

The final Windows `language-1-front-door` owner passes all 11 phases and 155
declared cases with those exact analyzer and emitter identities.

The compiler-source sentinel reaches native staging but does not pass on either
the candidate or the clean upstream tip. The clean tip already reports 600
functions and 1,048,036 code bytes before failing the existing 4 MiB object
limit; the candidate reports 600 functions and 1,048,159 code bytes before the
same failure. This decision does not raise that limit or count the pre-existing
failure as successful evidence.

## Consequences

Reachability no longer reparses already completed modules merely because a
newly reached dependency appears earlier in canonical WVSS order. The candidate
adds no function and 144 bytes to the focused graph tool while reducing
representative graph time materially. State remains exactly one byte per
module, and no compiler limit or serialized format changes.

This does not remove repeated module-name lookup inside import resolution or
the intentional later scan that constructs the adjacency directory. Those are
separate optimization boundaries and require their own exact-output evidence.

## Reconsideration triggers

Replace this frontier only if a precomputed module-name directory or reusable
parsed-import directory preserves failure ordering and exact output with lower
measured work. If source modules become mutable during analysis, the completed
state requires explicit generation invalidation rather than implicit reuse.
