# Decision 0518: Native source-bindings, WVIR, and WVB build/inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

Decision 0517 transferred ordinary construction and inspection through source
sets, import graphs, and declaration/signature symbols. The next contiguous
construction block in both broad Seed scripts was source-body binding, typed
WVIR lowering, and executable WVB emission. Stage 0 still built the core, demo,
and hosted-tool product for each phase and performed one managed core inspection
before executing the retained semantic and differential behaviors.

The generic native Project 1 front door reproduces the bindings and WVIR
families exactly. The pinned generic native build-driver artifact does not yet
compile the current source-WVB core or tool closure, even though the native
compiler-seed application directly reproduces those products. This is a
bootstrap-artifact boundary rather than a source-language or Project 1 contract
change. A bounded source-compiler-product launcher is therefore required for
the three WVB products until the generic driver is rebuilt and qualified.

The current scalar native runner still stops all three demos with runtime code
`3004`. The bindings and WVIR hosted tools require explicit console, diagnostic,
file, and process capabilities. The WVB hosted tool additionally remains the
compiler/differential owner for five fixture families, verification, inspection,
execution, malformed ordering, and Stage 0 byte oracles. Construction and core
inspection can transfer without claiming those separate behavior boundaries.

## Decision

Add eight explicit repository-root Project 1 aggregates, reuse the existing
`Projects/Examples/Windvale-Compiler.wvproj` aggregate for the WVB tool, and make the paired
`Verify-Seed-Native-Front-Door` helpers the normal builder for these products:

| Product | Functions | Code bytes | Module bytes | SHA-256 |
| --- | ---: | ---: | ---: | --- |
| source-bindings core | 263 | 437,438 | 542,309 | `a772a75fe625f47e165ca190e76d8cd59fa0b591a0270a5817e02e0fac62542c` |
| source-bindings demo | 271 | 443,818 | 548,036 | `563caeb4a76fb34d6c2b2b8340260cc1da518c4cbaad9e5f355201f6bd1fa933` |
| source-bindings tool | 268 | 441,068 | 542,334 | `17e877b3c59d2f9a99d26be4c478f10ce8879e6bce925b65894d158fd4a6e0a9` |
| source-WVIR core | 346 | 665,606 | 817,391 | `c4c3bd9164ccdf75acd1140e74c256295bb1f8ea8bdbf69cdcd3225ceea70fbb` |
| source-WVIR demo | 352 | 672,121 | 822,254 | `7f533fcb38a9311ba4d390b814ea3741ab25d5db9ac2167bd9f4f6b58bddc02f` |
| source-WVIR tool | 351 | 669,118 | 815,722 | `7fbfc8f57620dd81a5d2024310a21a8ce32d56cc986d94b39ca03428c1404db5` |
| source-WVB core | 422 | 757,261 | 923,514 | `c4602b6c026a65e0b9de11c025768b7f652ee73640b6f5ff1806d40ee5d0071b` |
| source-WVB demo | 426 | 760,228 | 923,210 | `ef5a7cad94cce135dd937756980f9268fa2964f49dbb4fccca95ba4d09713fc9` |
| source-WVB tool | 427 | 759,920 | 921,640 | `18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754` |

The helpers natively inspect the three core modules. They bind the exact
portable profile and these current type/export surfaces:

| Core | Export section | Type section | Required ownership surface |
| --- | --- | --- | --- |
| bindings | offset 526,082, bytes 2,996, count 59 | offset 529,086, bytes 13,223, count 55 | binding status/summary, directory validation, complete validation |
| WVIR | offset 794,006, bytes 3,755, count 75 | offset 797,769, bytes 19,622, count 66 | operation catalog, summary, directory validation, complete validation |
| WVB | offset 898,984, bytes 3,322, count 70 | offset 902,314, bytes 21,200, count 82 | source-WVB summary and compilation |

The WVIR and WVB export counts correct stale managed inspection expectations of
72; the exact native inspection binds the serialized module rather than carrying
forward those obsolete textual assertions.

`Build-Source-Compiler-Product.cmd` and `.sh` accept only `core`, `demo`, or
`tool`. They verify the native compiler seed, current-host compiler application,
qualified publisher, and selected manifest identities; pass one exact source
inventory; compile into a process-private candidate; and let the qualified
publisher verify and atomically replace the requested destination. Invalid
usage returns 64, and identity, compilation, or publication failure preserves
an existing destination. The Linux launcher additionally admits the complete
seed and front-door checksum inventories before selecting its applications.

The manifests remain at repository root because each complete closure spans
`Compiler/Windvale` or `Examples/Compiler` plus `Foundation`. Their dependencies
use canonical module-name order as the same compatibility workaround recorded
by Decision 0517 for the pinned Project driver's order-sensitivity defect.

The broad scripts consume the nine native-built WVBs. They retain all three
managed demo runs, the bindings/WVIR hosted-tool runs, and the source-WVB hosted
tool and fixture/differential/oracle sequence. Those retained cases remain
behavior evidence and are not counted as transferred.

## Evidence

- `Verify-Seed-Native-Front-Door.ps1` passes its 156-case contract over 97
  artifacts in 903.3 seconds on the current Windows host.
- The focused bindings owner passes 1/1 in 6.291 test seconds.
- The extended typed-WVIR owner passes 1/1 in 10.069 test seconds.
- The extended source-WVB backend owner passes 1/1 in 32.144 test seconds,
  including the Function-Only, Data-And-Text, Nominal-Types,
  Hosted-Capabilities, and composition differential fixtures.
- Direct native demo probes stop with runtime code `3004`: bindings after 791
  instructions, WVIR after 767, and WVB after 770. Their managed executions
  therefore remain explicit rather than being counted as transferred.
- Invalid source-compiler-product usage returns 64 and preserves an existing
  destination.

This removes nine managed builds and three managed inspections from each broad
host script: twelve calls in this change and 162 cumulatively across Decisions
0505, 0506, 0508 through 0518. It does not remove a direct managed entry file.
The inventory remains three normal direct files plus nine recovery files, and
T2 remains `managed-normal`.

## Consequences

The paired native helper grows from 88 to 97 exact artifacts and from 144 to
156 owned cases. Source-bindings, typed-WVIR, and source-WVB construction and
core inspection no longer use the managed CLI in either permanent-host script.
Stage 0 remains the execution/differential owner for the three demos, the two
capability-bearing phase tools, and the complete source-WVB fixture family.

The bounded seed-backed WVB launcher is now an explicit normal construction
seam. It does not replace or weaken the generic Project 1 front door. Once the
generic native driver is rebuilt against the current compiler closure and
qualified, these three products should converge on the ordinary Project route
and the special launcher should be removed.

Current evidence is Windows-host native construction, inspection, and focused
differential behavior. It is not independent Linux execution, native semantic
demo/tool execution, replacement of the broad managed test harness, clean or
previous-seed bootstrap, grouped qualification, promotion, or recovery deletion.

## Reconsideration triggers

Continue with the WvDump, object-model, WVA assembler, and linker construction
and inspection block that follows source-WVB behavior in the broad scripts.
Separately diagnose native runner code `3004` before moving semantic demo
execution, add the required hosted capability profiles before moving tool
execution, and transfer the source-WVB fixture/differential lane only with an
independently owned native oracle. Remove both the manifest-order workaround
and the bounded WVB product launcher after a current generic native Project
driver restores the complete contract.
