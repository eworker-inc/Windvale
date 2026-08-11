# Decision 0516: Native source-parser build and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

After Decision 0515 transferred hosted construction and publication lifetime,
the next contiguous block in both broad Seed scripts was the first three
Windvale-written source-compiler phases: the source lexer, declaration parser,
and body parser. The feature-frozen Stage 0 CLI still built eight core, demo,
and hosted-tool WVBs and performed three managed inspections before executing
the parser behaviors.

The native Project 1 front door already accepts every exact source closure.
However, no explicit aggregate manifests owned these cross-component products,
and the native scalar runner does not yet execute their demos successfully.
Construction and inspection can therefore transfer without claiming that the
separate execution boundary is closed.

## Decision

Add eight explicit repository-root Project 1 aggregates and make the paired
`Verify-Seed-Native-Front-Door` helpers the normal builder for these products:

| Product | Functions | Code bytes | Module bytes | SHA-256 |
| --- | ---: | ---: | ---: | --- |
| source-lexer core | 20 | 40,152 | 49,470 | `411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e` |
| source-lexer demo | 21 | 46,427 | 56,674 | `f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db` |
| declaration-parser core | 52 | 120,804 | 151,197 | `8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb` |
| declaration-parser demo | 53 | 124,556 | 154,365 | `9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf` |
| declaration-parser tool | 55 | 122,750 | 151,731 | `ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0` |
| body-parser core | 100 | 197,096 | 248,663 | `68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589` |
| body-parser demo | 101 | 204,515 | 254,805 | `2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f` |
| body-parser tool | 103 | 198,924 | 247,844 | `0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f` |

The helpers also natively inspect the three core modules. They bind the exact
portable profile, export/type section extents, nominal-type counts 7, 15, and
25, export counts 17, 32, and 47, and the established lexer, declaration, and
body-parser ownership surfaces.

The manifests remain at repository root because each complete closure spans
`Compiler/Windvale` or `Examples/Compiler` plus `Foundation`. Project 1
containment deliberately forbids a component- or example-local manifest from
escaping upward to those dependencies. Root placement states the aggregate
truth without weakening containment.

The broad scripts consume the eight native-built WVBs. They retain the three
managed demo runs and five capability-bearing hosted-tool runs as behavioral
evidence. The declaration/body tools still require explicit console,
diagnostic, file, and process capabilities; their execution is not folded into
construction ownership.

## Evidence

- A focused native probe reproduced all eight pre-existing managed byte lengths
  and SHA-256 identities exactly.
- Native inspection reports the exact three portable type/export surfaces.
- `Verify-Seed-Native-Front-Door.ps1` passes its 132-case contract over 79
  artifacts in 264.4 seconds on the current Windows host.
- The focused managed compiler selection passes all 7 matching regular and
  extended owners in 45.466 test seconds, including the lexer, declaration
  parser, and body parser.
- Direct native demo probes do not return the required result: declaration and
  body stop with runtime code `3004`, while the lexer exits without producing
  `Result: 0`. Their managed runs therefore remain explicit rather than being
  counted as transferred.

This removes eight managed builds and three managed inspections from each
broad host script: eleven calls in this change and 138 cumulatively across
Decisions 0505, 0506, 0508, 0509, 0510, 0511, 0512, 0513, 0514, 0515, and
0516. It does not remove a direct managed entry file. The inventory remains
three normal direct files plus nine recovery files, and T2 remains
`managed-normal`.

## Consequences

The paired native helper grows from 71 to 79 exact artifacts and from 121 to
132 owned cases. The lexer, declaration parser, and body parser no longer use
the managed CLI for ordinary construction or inspection on either permanent
host script. Stage 0 remains an execution/differential owner for the three
demos and five capability-bearing tool cases.

Current evidence is Windows-host native construction, inspection, and focused
differential behavior. It is not independent Linux execution, native parser
demo/tool execution, replacement of the broad managed test harness, clean or
previous-seed bootstrap, grouped qualification, promotion, or recovery
deletion.

## Reconsideration triggers

Continue with the source-set, module-graph, and symbol construction block that
follows the body parser in the broad scripts. Separately diagnose native runner
code `3004` and the lexer demo's resultless exit before moving parser execution;
do not widen construction ownership into an unsupported runtime claim.
