# Decision 0401: Native streaming SHA-256 evidence

- Status: Implemented candidate; advanced by Decision 0402
- Date: 2026-08-08
- Advances: [Decision 0400](0400-Standalone-Native-Hosted-Service-Bundle.md), [Decision 0399](0399-Standalone-Native-Hosted-Container-Metadata.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native streaming SHA-256 evidence](../../Specifications/Windvale-Native-Streaming-Sha256-Evidence.md)
- Advanced by: [Decision 0402](0402-Native-Hosted-Metadata-Request.md)

## Context

The standalone service-bundle producer emits canonical `WVSI 2` segments, and
the standalone metadata constructor consumes one exact `WVHM 1` request. The
remaining bridge cannot honestly hash the complete native image through
`Bytesˉsha256ˉhex`: one Windvale value is capped at 4 MiB, while the real
compiler-family native image is about 26 MiB.

Copying host SHA calls into the managed bridge, raising the language value
limit, trusting a hardcoded digest, or changing metadata to a tree hash would
either retain .NET, weaken a deliberate bound, or change the container
identity contract.

## Decision

Add a focused portable SHA-256 compression module and a separate streaming
state module. Keep wrapping addition within checked Windvale semantics by
adding two 16-bit halves whose maximum intermediate value is 131,071. Retain
at most 63 tail bytes between updates, construct SHA padding only at finish,
and return the standard raw 32-byte digest.

Add `WVHS 1` to describe one bounded logical sequence over up to 16 immutable
4-MiB resources and up to 16 ordered nonoverlapping identity regions. Add
`WVHE 1` to bind every raw region digest to the exact complete manifest digest.
The hosted shell derives chunk names from one prefix, verifies every selected
resource length and extent, hashes regions across chunk boundaries, and writes
only a complete evidence envelope.

Expose exact Windows/Linux package targets through `windvale compile` and
`windvale aot`. Keep the new C# surface limited to deletion-bound target layout
and exact package identities; it does not compute, select, or validate product
digests.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Streaming evidence WVB | 39,483 | `9ea136c44f76d9a53474d81e989f38cf15ffa8dc267638e2224e20f733205e6b` |
| Windows streaming evidence tool | 646,656 | `e69a9a4c0a33d4bc05d98e5c977ba537715081d08d8c8493da87990989547af3` |
| Linux streaming evidence tool | 647,168 | `eb1ba149a94741801732e53773f566aee752a19510d22f8e2768def7614d396c` |

The reviewed focused test passes 1/1 in 15.231 seconds on the final shared
source state. It hashes three regions over a 4,194,401-byte
logical sequence split across two immutable resources. The first region alone
is 4,194,200 bytes and crosses the resource boundary. Independent platform SHA
checks match the manifest and all three raw region digests. The test also pins
both packages, exercises the public current-host target without loading the
CLR, preserves an existing output after manifest corruption, and reconstructs
the WVB through the native front door. No broader verifier was run.

## Consequences

- A real compiler-size image can obtain standard SHA-256 evidence without one
  oversized Windvale value or a managed hashing process.
- Digest numbers are outputs bound to actual immutable resources and an exact
  manifest; they are not behavioral test oracles.
- Compression, streaming state, evidence admission, portable resource state,
  and hosted I/O remain focused owners rather than one large mixed source.
- Decision 0402 now recomputes actual bundle evidence and constructs `WVHM` in
  one native process; this decision still does not promote or delete Stage 0.

## Reconsideration triggers

Version the formats if the resource count, logical size, region ordering,
digest algorithm, resource naming, or evidence binding changes. Replace the
portable compressor with a target optimization only if the same source-defined
semantics and raw evidence remain independently reproducible on both hosts.
