# Decision 0406: Native hosted source-geometry production

- Status: Implemented candidate; ordered process orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0405](0405-Native-Hosted-Publication-Request.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted source geometry](../../Specifications/Windvale-Native-Hosted-Source-Geometry.md)

## Context and decision

Decision 0405 removed managed `WVPQ` serialization, but its input `WVSG` was
still produced by a C# fixture. An orchestration script using that fixture
would retain a hidden managed constructor.

Add a focused paired native command that reads one through eight canonical
fragment chunks and ten service chunks, derives their exact logical extents
and publication placements, constructs `WVSG`, and self-admits it before
publication. Keep resource acquisition explicit and keep process ordering and
temporary lifecycle in the following orchestration slice.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Source-geometry WVB | 17,802 | `22549f1e50084b3cf20113bee6c30c3df9c4f91aad58b0a3ebe247d02a9e4a28` |
| Windows producer | 198,656 | `209d77bd3dc10ccaec33bb0ee5351d0f4a569421ba938a26bbfc8e54d9dea996` |
| Linux producer | 200,704 | `0457b23abbe871314eba3a91f992b1a479aa6b252f01c82a8be362670dce0f17` |

After test review, the final focused check passes 1/1 in 4.264 seconds with a
zero-warning build. It checks exact eleven-region arithmetic, public target
routing, native execution without CLR loading, rejection/output preservation,
and native-front-door reproduction. No broader verifier ran.

C# no longer owns `WVSG` construction in this candidate path. Creation of the
raw fragment/service resources, ordered request/response and manifest
lifecycle, Linux execution, promotion, and grouped qualification remain.

## Reconsideration triggers

Version the command if chunk sizing, canonical hosted service order, `WVSG`,
or publication alignment changes. Do not add child-process policy or final
publication authority to this geometry owner.
