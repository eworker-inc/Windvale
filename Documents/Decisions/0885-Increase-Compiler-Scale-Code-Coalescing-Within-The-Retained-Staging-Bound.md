# Decision 0885: Increase compiler-scale code coalescing within the retained staging bound

## Status

Implemented candidate with Windows cross-target reconstruction and execution.
Independent Linux reconstruction and execution remain pending.

## Context

[Decision 0425](0425-Compiler-Scale-Native-Wvo-Resource-Staging.md) selected a
1 MiB target for deterministic coalescing of consecutive native-code
publication steps. It also required reconsideration when an accepted compiler
could no longer fit the 62 staging resources retained after the input and
manifest occupy two entries of the immutable 64-snapshot table.

The Slice 8 analyzer reaches that trigger. Its exact 1,538,858-byte WVB has
SHA-256
`7f3fbd4ae1b023ad77cd2ffb3e099baccb5699a0c74cc8a2fe51d2a30a7510f4`.
The existing staging producer emits the exact 50,505,412-byte WVO as 63
resources: one prefix, 57 code resources, and five non-code tail resources.
The manifest is 780 bytes. The previous 49,987,776-byte analyzer occupied all
62 retained resources, so increasing the snapshot table would hide rather than
resolve the measured staging-capacity boundary.

The baseline WVO has SHA-256
`499d31bfe2dad3ca561955dac26b55dc376e967e3085fd198dc6bf7e90064e60`.
Its largest actual resource is 1,715,344 bytes. The manifest's 4,194,304-byte
maximum is the per-value admission ceiling, not the observed resource size.

## Decision

Increase only the staging producer's deterministic consecutive-code
coalescing target from 1,048,576 to 1,310,720 bytes, exactly 1.25 MiB.

Keep the existing greedy order and checked arithmetic. A pending code resource
is flushed before a non-code step or before appending a code step would exceed
the target. A naturally larger single code step remains one resource and is
still bounded by the unchanged 4 MiB publication ceiling. Prefix, padding,
read-only, symbol, and relocation boundaries remain unchanged.

Do not change WVO bytes, semantic order, publication positions, manifest
format, resource naming, the 64-snapshot table, the 62-resource retained
staging bound, the 128 MiB monotonic native dynamic text/byte arena, or any
shared lowerer publication policy.

Reject a 2 MiB target for this implementation. Although its resource geometry
would fit, repeated immutable `Bytesˉconcat` construction exhausts the retained
monotonic arena near completion. That candidate wrote one prefix and 24 code
resources, including a 2,094,772-byte resource, then exited with status 66
(`text-arena exhaustion`) before writing a manifest. No arena, snapshot, or
value limit is widened to admit it.

## Evidence

The 1.25 MiB source candidate rebuilds to a 576,810-byte WVB with SHA-256
`0eca0f227cbc3f2e9ad54dce9439ac0c859c8176cf72964cdda627c88d33e5cc`.
The reconstructed Windows application is 8,416,768 bytes with SHA-256
`ca19b920d59987762d423dd8e79e4569878f6da0fc31d455564ef827c0f19e54`;
the cross-target Linux application is 8,417,280 bytes with SHA-256
`499032e30458c0b60ab7225e082aa00d5ebc9e79d49bfb71c9167b721d6c5280`.

That application stages the exact Slice 8 analyzer successfully:

| Measurement | Existing 1 MiB producer | 1.25 MiB candidate |
| --- | ---: | ---: |
| WVO bytes | 50,505,412 | 50,505,412 |
| Prefix / code / tail resources | 1 / 57 / 5 | 1 / 44 / 5 |
| Total resources | 63 | 50 |
| Manifest bytes | 780 | 624 |
| Actual largest resource | 1,715,344 | 1,715,344 |
| Manifest value ceiling | 4,194,304 | 4,194,304 |
| WVO SHA-256 | `499d31bfe2dad3ca561955dac26b55dc376e967e3085fd198dc6bf7e90064e60` | `499d31bfe2dad3ca561955dac26b55dc376e967e3085fd198dc6bf7e90064e60` |

The candidate therefore leaves 12 of the retained 62 staging-resource slots
unused while preserving every WVO byte and non-code boundary. Manifest entries
remain contiguous and ordered, and every actual resource remains below the
unchanged 4 MiB per-value ceiling.

The uncontended candidate run completed in 172.280 seconds with a sampled peak
working set of 525,324,288 bytes. The baseline run took 169.888 seconds while
overlapping another native staging process, so that duration is a
contention-inflated upper bound and is not a valid performance comparison. Its
peak working set was unavailable. The rejected 2 MiB run took 166.421 seconds
and sampled 525,316,096 bytes before arena exhaustion; neither duration nor
working set substitutes for the exact arena failure result.

The checked-in candidate promotion replaces only the WVO-staging WVB plus its
Windows and Linux applications in
`Artifacts/Native-Segmented-Compiler-Toolset-Candidate`. It refreshes their
manifest, exact consumer pins, and the focused bootstrap-analyzer expectation.
That retained 992,412-byte analyzer still produces the exact 31,736,596-byte
WVO, now as 34 resources and a 432-byte manifest instead of 41 resources and a
516-byte manifest. The image-staging and canonical-transport source WVBs and
all four applications remain byte-identical.

The complete local Windows reconstruction owner passes all four cases twice on
the final source and artifact state. The second run streams every bounded
constructor build/package phase before reproducing all nine artifacts and the
retained analyzer result. Independent Linux reconstruction and execution remain
required; Windows cross-target construction alone does not qualify the Linux
application.

## Consequences

The current analyzer fits the native staging snapshot without changing any
serialized format, WVO byte, semantic contract, or authority boundary. The
fixed target keeps the plan deterministic and reviewable, while the 12-resource
margin avoids operating at the exact retained table edge.

The native arena high-water mark is not directly exposed by the hosted
application. Successful bounded completion proves the 1.25 MiB candidate fits
the retained 128 MiB arena, but does not quantify remaining arena bytes. A
future staging producer should prefer an independently bounded streaming or
reclaiming builder if measured growth makes immutable concatenation pressure
the next limit.

## Reconsideration triggers

- An accepted compiler again needs more than 62 staging resources.
- A retained workload fails the 1.25 MiB policy through text-arena exhaustion.
- Measured arena headroom is too small for stable compiler growth.
- A bounded streaming or reclaiming packer reduces retained construction
  pressure without changing WVO bytes, ordering, non-code boundaries, or the
  4 MiB per-value ceiling.
