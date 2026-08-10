# Decision 0511: Native service-source build and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

The broad Seed verification scripts still compiled eight exact Windvale
source products through the feature-frozen Stage 0 CLI: the native-stencil
core, demo, and bridge; the UTF-8 core and bridge; the integer-format core and
bridge; and the shared native service-code builder. Seven of those products
were then inspected through the managed CLI. Their exact source products were
already accepted by the native Project 1 builder and native WVB inspector.

Project placement also needed correction. Two service bridge manifests lived
at repository root even though both their roots and dependencies belong to
`Runtime/Windvale`. The new single-component manifests can all live beside
their source. Only the Stencil demo spans `Examples/Compiler` and
`Compiler/Windvale`, so it retains one explicit root aggregate while Project 1
forbids parent-directory escape.

## Decision

Extend the paired `Verify-Seed-Native-Front-Door` helpers with the following
exact native Project 1 builds and native inspections:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| native-stencil core | 21,296 | `6df3c524d0f9bec79cd2516a758985c487cc237c6f94bc5b80e015975d50cca3` |
| native-stencil demo | 25,683 | `6b27fbd10d5f06855354f433ec0b8c9b1af1761ef04458817931e675c26e0da8` |
| native-stencil bridge | 20,800 | `0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da` |
| UTF-8 service core | 11,577 | `adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7` |
| UTF-8 service bridge | 11,511 | `4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f` |
| integer-format service core | 11,611 | `6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2` |
| integer-format service bridge | 11,598 | `851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9` |
| native service-code builder | 4,135 | `adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06` |

The helpers bind the exact two-line build reports and inspect the canonical
module profile, export count, and named ownership functions. The broad scripts
consume those native-built WVBs. They retain the byte-for-byte comparisons
against the embedded bridge WVBs and exact service leaves, but no longer repeat
eight managed compiles or seven managed inspections.

The Stencil demo execution remains managed. It intentionally requests a
20,000,000-instruction ceiling, beyond the current runner's fixed ordinary
1,000,000-instruction policy. Moving its build does not imply that execution
policy has been transferred.

Project manifests normally live beside the component source they own. Root
manifests are reserved for genuine cross-component aggregates under the
current contained Project 1 format. Future workspace, package-index, or
project-reference semantics should remove that placement pressure without
allowing a Project 1 path to escape its manifest directory.

## Evidence

- All eight native builds reproduce the established Stage 0 WVB identities.
- All seven native inspections admit the exact portable profile, exports, and
  named entry points.
- `Verify-Seed-Native-Front-Door.ps1` passes its 39-case ownership contract
  over twenty artifacts in 16.1 seconds.
- The focused frozen differential selection passes the UTF-8, integer-format,
  and text-concatenation service owners 3/3 in 4.372 test seconds.

This removes fifteen additional managed invocations from each broad host
script, forty-five cumulatively across Decisions 0505, 0506, 0508, 0509, 0510,
and 0511. It does not remove a direct managed entry file: the inventory remains
three normal direct files plus nine recovery files, and T2 remains
`managed-normal`.

## Consequences

Seven new manifests are component-local, two former root bridge manifests move
into `Runtime/Windvale`, and only the cross-component Stencil demo adds a root
aggregate. Existing older root manifests are not moved mechanically; they can
be colocated when their ownership is touched and their complete path consumer
set is updated.

Current evidence is Windows-host native build and inspection evidence. It is
not independent Linux execution, native 20-million-step Stencil execution,
complete capability-bearing execution, a clean or previous-seed bootstrap,
grouped qualification, promotion, or recovery deletion.

## Reconsideration triggers

Transfer the Stencil demo execution after the native runner owns an explicit
bounded execution-policy input or an equally exact product policy. Replace the
remaining root aggregates after a workspace/reference contract can name
cross-component inputs without weakening Project 1 containment or canonical
source identity.
