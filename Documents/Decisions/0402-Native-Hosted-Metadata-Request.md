# Decision 0402: Native hosted metadata-request construction

- Status: Implemented candidate; process-pipeline composition pending
- Date: 2026-08-08
- Advances: [Decision 0401](0401-Native-Streaming-Sha256-Evidence.md), [Decision 0399](0399-Standalone-Native-Hosted-Container-Metadata.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted metadata-request producer](../../Specifications/Windvale-Native-Hosted-Metadata-Request.md)

## Context

Decision 0401 can hash logical regions across bounded immutable resources, and
Decision 0399 can turn one exact `WVHM 1` request into canonical metadata. A
managed seam still acquired the bundle, projected its digests, and built the
request between those processes. Trusting a separately supplied evidence file
would only move that seam rather than retire it.

## Decision

Add `WVMI 1` for the target, hosted profile, and native entry. Add one native
hosted command that admits the exact ten-service `WVPQ 1` plan, admits `WVHS 1`,
reads the named bundle chunks, recomputes all eleven raw SHA-256 leaves in the
same process, and constructs the exact 576-byte `WVHM 1` request.

Require the canonical service order, publication placements, fragment region,
ten service regions, logical image size, manifest-bound evidence, and all
reserved zeros. Publish no output until all checks pass. Expose paired public
Windows/Linux AOT targets. Keep C# limited to deterministic package wiring and
the deletion-bound differential test oracle.

The reusable hashing/resource state is portable and focused. The exact
request-format code temporarily remains in the CLI root because the current
native source composer rejects the extracted request module when combined with
the streaming-resource graph. Both Stage 0 and native compilation accept the
single-root form and reproduce identical bytes. Re-extraction is a compiler
follow-up, not a reason to duplicate request logic.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-request WVB | 54,135 | `db433d551ac3530c8b9c36e8bf035177181c3d403912030ef9fd5bba37698034` |
| Windows metadata-request tool | 782,848 | `73fac9bc9d023f9ad4dca1f8c7fbcad899b26a92227f4ca32eaae6eeb36a5596` |
| Linux metadata-request tool | 782,336 | `86fc9a3860b68eabe8500ba0256c5d01dbf6918baed3fbc4e3711c6670258443` |

The reviewed metadata-request test passes 1/1 in 5.170 seconds. It materializes
the publication planner's actual aligned bundle, executes the public current-
host application without loading the CLR, recomputes evidence from the chunk
resource, and matches the frozen C# request oracle byte-for-byte. A corrupted
plan is rejected without overwriting a sentinel output, and the native Project
1 front door reproduces the exact Stage 0 WVB.

The affected streaming-evidence test also passes 1/1 in 15.231 seconds on the
final shared source state, including a region that crosses the 4 MiB boundary.
No broader verifier was run under the agreed end-of-goal verification policy.

## Consequences

- Managed code no longer needs to calculate or project the eleven `WVHM`
  digests for the candidate native process pipeline.
- A loose digest file is not a trust input; evidence is derived from the actual
  immutable chunk bytes immediately before request construction.
- The next slice can orchestrate the service-bundle responses, metadata request,
  metadata constructor, runtime header, planner, platform/startup pieces, and
  bounded segment requests as one native process path.
- Stage 0 remains frozen recovery/differential evidence until the final gate.

## Reconsideration triggers

Version `WVMI` if target/profile/native-entry policy changes. Version the
command if publication order, manifest regions, resource naming, evidence
binding, or `WVHM` changes. Re-extract the request-format core once the native
source compiler accepts that exact module graph through its normal front door.
