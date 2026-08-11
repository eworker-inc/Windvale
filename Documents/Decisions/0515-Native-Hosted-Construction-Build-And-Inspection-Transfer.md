# Decision 0515: Native hosted-construction build and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

The broad Seed verification scripts still used the feature-frozen Stage 0 CLI
for twelve closely related products at the boundary between runtime metadata,
hosted executable construction, and executable-image publication. The paired
native Project 1 front door already accepts every exact source closure, but the
normal qualification route repeated those builds and nine managed inspections.

Decision 0514 identified hosted-tool metadata, startup, and runtime-header
construction as the next cohesive transfer. Hosted-container construction
consumes those contracts directly, and publication lifetime is the adjacent
capability-free compiler-owned core/bridge pair before the source compiler
block begins. Moving the complete contiguous block avoids leaving isolated
managed inspection calls between native-owned products.

## Decision

Extend the paired `Verify-Seed-Native-Front-Door` helpers with these exact
Project 1 products:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| hosted-tool metadata admission | 10,872 | `d7b0084ed2c69ee03ad65ee4bfffa72550fd8d9ef2889efa0be116350b80b8b5` |
| hosted-tool metadata-construction core | 24,360 | `5808f778eb21c1214b581f0ce03958a74173a801b886aec7ed32124d7446abcd` |
| hosted-tool metadata-construction bridge | 24,252 | `b5e9397326d3106b22ce735369ef8202ff6bb4c8e14f6069a0c467b4266c8208` |
| hosted-startup instantiation | 21,143 | `933864be78b28394b9fc8e495b5ac872311ebca2a624db6e6731cdb8b399d309` |
| hosted-container planner | 35,929 | `ff1b48cfc05baab5f707dcfce7e73b0714e2379ee594e12f6e9c6ea1589fef7e` |
| hosted-container Windows constructor | 17,679 | `a77e4ea3ac2cff35e965ae44cd486f30dd5b0c10aa2cde23c109d0eca37bffcb` |
| hosted-container Linux constructor | 12,328 | `dac93155c68ba18f6cbe3af2d301a4c4171b9a9c05841057ea57398536fa8b42` |
| hosted-container segment constructor | 22,584 | `d6d74f7d27df9f04f02b8eac2e75fde4fc230ba70d198f90b31ad668a06052e6` |
| hosted-tool runtime-header core | 19,516 | `f1c156def9fa6f00bb0401097435bb1d1429d9d4be247b8d11f0de0b5ea51be2` |
| hosted-tool runtime-header bridge | 19,459 | `3cc8d0850b888911ee3338600bc7699578b163e7400c2b3631ef14649b9a3f18` |
| publication-lifetime core | 4,955 | `a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3` |
| publication-lifetime bridge | 4,442 | `f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554` |

Native inspection binds the portable profile, the exact capability-free
`Main(bytes) -> bytes` surface of each public bridge or product, and the
seven-export publication-lifetime type/function surface. The broad scripts
consume those native-built WVBs and retain byte-for-byte comparisons against
the metadata, startup, runtime-header, and publication-lifetime bridge WVBs;
the four hosted-container WVBs; both hosted-startup WVOs; and every associated
linked fragment.

Single-component manifests live beside their owner: metadata admission under
`Runtime/Windvale`, startup instantiation under `Linker/Windvale`, and both
publication-lifetime products under `Compiler/Windvale`. The metadata,
runtime-header, and hosted-container manifests that genuinely compose sources
from multiple components remain repository-root aggregates. New root core
manifests make the two formerly ad hoc compile products explicit without
pretending they have single-component closure.

## Evidence

- All twelve native builds reproduce the established WVB identities.
- Nine native inspections admit the intended portable surfaces.
- `Verify-Seed-Native-Front-Door.ps1` passes its 121-case contract over 71
  artifacts in 79.1 seconds.
- The five hosted-tool behavioral owners pass 5/5 in 14.005 test seconds, and
  the publication-lifetime owner passes 1/1 in 0.733 test seconds.
- The changed-file planner assigns each transferred source, manifest, and the
  removed root startup manifest to the paired native-front-door boundary.

This removes twelve managed builds and nine managed inspections from each
broad host script: twenty-one calls in this change and 127 cumulatively across
Decisions 0505, 0506, 0508, 0509, 0510, 0511, 0512, 0513, 0514, and 0515. It
does not remove a direct managed entry file. The inventory remains three normal
direct files plus nine recovery files, and T2 remains `managed-normal`.

## Consequences

The paired native helper grows from 59 to 71 exact artifacts and from 100 to
121 owned cases. Hosted metadata, startup, outer-container construction,
runtime-header construction, and executable-publication lifetime no longer use
the managed CLI for ordinary build or inspection in either permanent-host
script.

Current evidence is Windows-host native construction, inspection, and focused
differential evidence. It is not independent Linux execution, replacement of
the broad managed test harness, capability-bearing execution transfer, clean or
previous-seed bootstrap, grouped qualification, promotion, or recovery deletion.

## Reconsideration triggers

Continue at the source-compiler construction block that follows publication
lifetime in the broad scripts. Keep source-language compiler phases distinct
from hosted-container construction, and preserve Stage 0 compilation only
where it remains an explicit differential or recovery oracle.
