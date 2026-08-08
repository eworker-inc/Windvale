# Decision 0395: Standalone native hosted-container planner

- Status: Implemented candidate; advanced by [Decision 0396](0396-Standalone-Native-Hosted-Container-Platform-Bytes.md)
- Date: 2026-08-08
- Advances: [Decision 0394](0394-Pruned-Staged-Publisher-Bridge-Closure.md), [Decision 0385](0385-Windvale-Owned-Hosted-Container-Construction.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container planner](../../Specifications/Windvale-Native-Hosted-Container-Planner.md)

## Context

Windvale already owned hosted-container layout and startup-target derivation,
but normal Stage 0 package construction invoked that planner only as an embedded
service-free fragment. The later native segmenter and publisher therefore had
no process-level plan producer with which to form a complete no-.NET pipeline.

Adding another managed planner wrapper would preserve the same dispatch seam.
Duplicating layout rules in a hosted command would recreate the product logic
that Decision 0385 had already transferred.

## Decision

Add `Native-Hosted-Container-Planner-Tool.wv` as a 97-line hosted shell over the
existing portable construction core. It reads one exact runtime header, derives
only the target/profile request fields from its embedded metadata, invokes the
shared planner, admits the successful response envelope, and writes the plan.

Expose exact Windows/Linux targets through `windvale compile` and
`windvale aot`. Reuse the established compiler-authority host envelope rather
than versioning hosted metadata solely for this transition tool. Refactor the
segmenter and planner package writers through one focused deletion-bound C#
builder so the new target does not copy platform construction logic.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Planner WVB | 37,289 | `81cf3932c5e1d4f711b779c515a718ec1acd32c09ae17031aa63b8a66f5ce788` |
| Windows planner | 584,704 | `e401ad5aef792a49be72cf711cfc427a859fe4a534aa780ad47d3b4a2c12a5dc` |
| Linux planner | 585,728 | `8032370c7391bbc6afa94c1e8804db78f682da4e57144a2907394e202806c0d3` |

The reviewed planner test passes 1/1 in 6.180 test seconds after a 10.37-second
zero-warning build. It pins both packages, exercises the public CLI target,
matches a real retained-fragment plan exactly, observes no CLR load, preserves
an existing output on inconsistent metadata, rejects an alias, and rebuilds the
WVB through the native front door. The refactored segmenter regression passes
1/1 in 3.430 test seconds after an 8.03-second zero-warning build. No broader
verifier was run.

## Consequences

- Hosted layout planning now has a real native process boundary on both hosts.
- Planner behavior is not copied into C# or a second Windvale implementation.
- Decisions 0396 and 0397 now consume this plan for platform and startup bytes;
  Decisions 0398 and 0399 supply its raw runtime-header and metadata inputs.
  Metadata-request/service-bundle evidence and segment-request orchestration
  remain.
- Stage 0 still constructs the planner packages and the ordinary hosted builders
  still use the embedded fragment until the complete pipeline is promoted.

## Reconsideration triggers

Version the command if the runtime-header identity, `WVCR` request, profile
mapping, or `WVCD` response changes. Introduce a distinct hosted metadata profile
only when the permanent package authority differs from the existing six-capability
host envelope; do not allocate profiles merely to label transition commands.
