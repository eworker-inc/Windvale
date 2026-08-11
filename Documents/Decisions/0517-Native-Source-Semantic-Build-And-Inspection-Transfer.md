# Decision 0517: Native source-semantic build and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

Decision 0516 transferred ordinary construction and inspection of the source
lexer, declaration parser, and body parser. The next contiguous block in both
broad Seed scripts was the first three semantic phases above parsing: canonical
source-set production, import-graph validation, and declaration/signature
symbol binding. The feature-frozen Stage 0 CLI still built the core, demo, and
hosted-tool WVB for each phase and performed three managed core inspections
before executing the six behaviors.

The native Project 1 front door can reproduce all nine products exactly. The
current scalar native runner still stops all three demos with runtime code
`3004`, and the three hosted tools require explicit console, diagnostic, file,
and process capabilities. Construction and inspection can therefore transfer
without claiming that the separate execution boundary is closed.

## Decision

Add nine explicit repository-root Project 1 aggregates and make the paired
`Verify-Seed-Native-Front-Door` helpers the normal builder for these products:

| Product | Functions | Code bytes | Module bytes | SHA-256 |
| --- | ---: | ---: | ---: | --- |
| source-set core | 110 | 206,538 | 257,873 | `1121320e20d83f685c559ea2d0cff8b8e57583d047a3c6aaf9f5c1fdc9423acb` |
| source-set demo | 116 | 214,034 | 267,203 | `ac7fb0e04cf042ab9f9f3bfc8f344f0fdbcdc4198189b65f152eaead84b07742` |
| source-set tool | 115 | 209,802 | 261,726 | `6e8b8c8aaa6fe2c5735719a9b317e8897cf70f87828ea1be5d26d670bc2ed30f` |
| source-graph core | 126 | 223,460 | 278,894 | `9c1ae01b93b9a598fd6b726071dad9a8b4c6fe47d9c8e2d060eff9451724c85b` |
| source-graph demo | 131 | 228,355 | 284,848 | `a762e564411e9fe72b906c3c37521c9047bb40b1267d2fb46223f382f1c7966c` |
| source-graph tool | 131 | 226,370 | 282,035 | `0a23a10c6abb9eb82229300ab92324f3298fcbf26d3be0948dbc984274a9ac10` |
| source-symbols core | 204 | 351,993 | 439,545 | `a7df71802871d48561c8045d7e997266365d74f7e5158d531164ae636d57a5e7` |
| source-symbols demo | 213 | 362,117 | 450,431 | `4cf84322af1cd514bc7ac9ac5e752ef689bb1729e83ea9021b9660c823243457` |
| source-symbols tool | 209 | 355,987 | 438,378 | `58732a7cb3352f1f61ba4cecb65ae0280aecc975ca06eca359a2881e14477a66` |

The helpers also natively inspect the three core modules. They bind the exact
portable profile, export/type section extents, nominal-type counts 29, 34, and
45, export counts 10, 12, and 66, and the established source-set, graph, and
symbol ownership surfaces.

The manifests remain at repository root because each complete closure spans
`Compiler/Windvale` or `Examples/Compiler` plus `Foundation`. Project 1
containment deliberately forbids a component- or example-local manifest from
escaping upward to those dependencies.

The pinned native build driver currently passes dependency resources to the
compiler in manifest order. Initial aggregates whose dependencies were not in
canonical module-name order were rejected at source binding. The committed
aggregates therefore use canonical module-name order as the compatibility
workaround already documented in `Specifications/Windvale-Project.md`. This is
an implementation defect, not a Project 1 contract change: source directives
remain order-independent, and a future native driver must remove the workaround.

The broad scripts consume the nine native-built WVBs. They retain the three
managed demo runs and three capability-bearing hosted-tool runs as behavioral
evidence. The tools continue to bind console, diagnostic, file, and process
rights explicitly; execution is not folded into construction ownership.

## Evidence

- A focused native build probe reproduced all nine pre-existing managed byte
  lengths and SHA-256 identities in 234.3 seconds.
- Native inspection reports the exact three portable type/export surfaces.
- `Verify-Seed-Native-Front-Door.ps1` passes its 144-case contract over 88
  artifacts in 580.1 seconds on the current Windows host.
- The three focused managed behavioral owners pass 3/3 in 28.950 test seconds:
  source set in 6.335 seconds, extended import graph in 14.955 seconds, and
  source symbols in 7.660 seconds.
- Direct native demo probes stop with runtime code `3004`: source set after
  13,098 instructions, source graph after 1,511, and source symbols after 1,430.
  Their managed runs therefore remain explicit rather than being counted as
  transferred.

This removes nine managed builds and three managed inspections from each broad
host script: twelve calls in this change and 150 cumulatively across Decisions
0505, 0506, 0508, 0509, 0510, 0511, 0512, 0513, 0514, 0515, 0516, and 0517.
It does not remove a direct managed entry file. The inventory remains three
normal direct files plus nine recovery files, and T2 remains `managed-normal`.

## Consequences

The paired native helper grows from 79 to 88 exact artifacts and from 132 to
144 owned cases. Source-set, source-graph, and source-symbol construction and
core inspection no longer use the managed CLI in either permanent host script.
Stage 0 remains the execution/differential owner for the three demos and three
capability-bearing tool cases.

Current evidence is Windows-host native construction, inspection, and focused
differential behavior. It is not independent Linux execution, native semantic
demo/tool execution, replacement of the broad managed test harness, clean or
previous-seed bootstrap, grouped qualification, promotion, or recovery
deletion.

## Reconsideration triggers

Continue with the source-binding, typed-WVIR, and WVB-backend construction block
that follows source symbols in the broad scripts. Separately diagnose native
runner code `3004` before moving semantic demo execution, and add the required
hosted capability profile before moving tool execution. Remove the manifest
ordering workaround when the native Project driver restores Project 1 order
independence.
