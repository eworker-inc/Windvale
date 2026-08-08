# Decision 0404: Native hosted-container segment-request construction

- Status: Implemented candidate; complete process orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0403](0403-Native-Hosted-Service-Bundle-Request.md), [Decision 0387](0387-Standalone-Native-Hosted-Container-Segmenter.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container segment-request producer](../../Specifications/Windvale-Native-Hosted-Container-Segment-Request.md)

## Context

Decision 0403 introduced `WVSG 1` and transferred one exact service-bundle
request to a native process. The final hosted-container session still used C#
to map header, startup, service bundle, imports, runtime, and relocation bytes
into each `WVHT 1` request. Copying Decision 0403's resource-to-region loop into
a second large root would make the retirement path harder to review.

A hosted imported helper would be a natural extraction, but the current native
source composer rejects that capability-bearing module graph even though Stage
0 accepts it. The shared boundary therefore must remain capability-free.

## Decision

Extend the portable `WVSG` owner with a region-append state machine. It derives
one segment intersection and consumes bounded resources one at a time; command
roots retain only explicit `file.read_bytes` acquisition. Refactor the service-
bundle request producer onto that state and keep all three product sources
focused at 263 through 276 lines.

Add `wvhostsegmentrequest` as a paired Windows/Linux command. It admits the
complete successful `WVCD` plan, requires the exact six canonical source
regions, validates every resource and alias boundary, and emits one exact
`WVHT` request for a selected canonical segment. Keep ordered invocation and
multi-file lifecycle for the next composition slice.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Container-request WVB | 42,788 | `f6bb1b03922296916b9afcfbe29e6ba5ce09c557a3345052272c0e58dcdfef00` |
| Windows container-request producer | 512,000 | `4b9cf3e689f348d2791c1eb1add11d3064bf665040999905c1484dcf79fcfe52` |
| Linux container-request producer | 512,000 | `487da501b797bd7285b29c034d30df4bb933b3382d632a19ac7bf6bdfd17ddfd` |

The refactored service-bundle request identities are 27,843-byte WVB
`2cd2311b9053abbe92f64d533d0681b6a5438c89a0548cad5ddc5a114c1b1917`,
294,912-byte Windows application
`e7fe0939f62ce2403e3e24d1f4523dbb2e63c8fe469ee6930a039b1b66cc8576`,
and 294,912-byte Linux application
`256304761afaa42da2df66a2f0e89303a4a00a282b95a235148a2633959d8e2c`.

After reviewing both affected tests, one filtered invocation passes 2/2 in
7.615 seconds: service-bundle request in 4.676 seconds and hosted-container
request in 2.937 seconds. Both cross one source region over two resources,
match frozen recovery oracles, exercise public target routing and real native
processes without loading the CLR, preserve output on malformed geometry and
invalid segments, reject aliases, and reconstruct through the native front
door. The Release build is zero-warning. No broader verifier ran.

## Consequences

- C# no longer owns `WVHT` request selection or bytes in the candidate path.
- One portable resource-to-region state serves both request producers without
  importing a capability-bearing helper or duplicating the algorithm.
- The shared sources remain reviewable rather than growing one broad
  orchestration file.
- Ordered process execution, response/manifest lifecycle, Linux execution,
  promotion, and grouped qualification remain.

## Reconsideration triggers

Version the command if `WVCD`, `WVHT`, segment sizing, source-region order, or
fill ownership changes. Revisit the hosted helper extraction when the native
composer accepts the same capability-bearing module graph. Do not move file
authority into the portable geometry state.
