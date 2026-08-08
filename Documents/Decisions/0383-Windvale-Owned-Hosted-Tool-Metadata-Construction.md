# Decision 0383: Windvale-owned hosted-tool metadata construction

- Status: Accepted current-host normal-path construction transfer; native outer-container composition, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0382](0382-Windvale-Owned-Hosted-Tool-Runtime-Header.md), [Decision 0161](0161-Exact-Compiler-Service-Bundle-And-Manifest.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native hosted-tool metadata construction](../../Specifications/Windvale-Native-Hosted-Tool-Metadata-Construction.md)

## Context

Decision 0382 moved construction of the shared 4 KiB hosted-tool runtime
header into Windvale, but normal packaging still asked C# to construct the
embedded 1 KiB `WVH* 1` metadata record. That writer owned the six hosted
profile variants, capability and service directories, target adapter mapping,
fixed limits, layout, and reserved bytes. Native outer-container construction
should consume one canonical metadata contract rather than reproduce that
policy independently for every PE/ELF tool family.

## Decision

- Define exact `WVHM 1` and `WVHD 1` envelopes for one target, one of six
  hosted profiles, verified bundle/native extents, ten ordered service
  placements, and their raw SHA-256 evidence.
- Keep canonical identities and policy out of the request. Windvale derives
  every capability identity/signature, service identity/capability, table slot,
  target adapter, profile magic/container/flags, fixed bound, and reserved byte.
- Reuse the separate Windvale metadata-admission core as a final constructor
  self-check, then require the normal managed seam to verify the returned bytes
  independently against the actual service bundle.
- Make normal hosted runtime-data construction consume one digest-bound,
  service-free WVNF before invoking Decision 0382's header constructor.
- Rename the former C# byte constructor to `Buildˉstage0`. It remains only for
  recovery and differential evidence and is never called by normal packaging.
- Treat the new managed request projection, WVNF invocation, response verifier,
  and focused C# test as temporary migration code. Replace them with native
  container orchestration and native qualification evidence before deleting
  their managed owners.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-construction core WVB | 23,725 | `d67f0f247b73f0ff483a158713e51239206ab176d3414f80744e0dd4a6797d22` |
| Retained bridge WVB | 23,626 | `e229fecdee3d70fc6937f6f2b0e3a0c28f6bef6ff1b3f737cfdccb29137ef983` |
| Retained bridge WVNF | 211,629 | `1d8f6f2c45a13c3c996ae00a950c84a12a107a94a85fd83c3c8471cb767cff7c` |

## Evidence and consequences

The reviewed focused owner case reproduces all source/WVB/WVNF identities,
confirms no constructor WVB is embedded, compares interpreter and native
execution, rejects fifteen malformed envelopes, covers all six profiles for
both targets, and compares every successful byte against the frozen Stage 0
oracle and the independent managed metadata verifier. It passes 1/1 in 3.720
seconds. The affected Decision 0382 runtime-header case passes 1/1 in 2.308
seconds. The existing real console-packager PE/ELF materialization case passes
1/1 in 8.709 seconds, preserving the pinned 708,608-byte Windows and Linux
identities. The Release test application builds with zero warnings and errors.

The exact compiler, Development, Standard, Qualification, Linux-host
execution, and broader hosted gates were not run under the goal's
deferred-broad-verification rule.

Normal hosted-tool metadata bytes are now Windvale-owned. C# still verifies
bundle/capability projections, calculates and projects actual digests, invokes
and verifies the retained constructor, constructs the outer PE/ELF, and builds
the service bundle. The next slice should consume these exact metadata and
runtime-header contracts while transferring the outer Windows/Linux hosted
container. The new C# bridge is therefore a deletion-bound adapter, not a
permanent product dependency.

## Reconsideration triggers

Version the request if the hosted profile set, service directory, metadata
size, digest algorithm, ABI constants, bundle layout, or placement count
changes. Do not let a host choose canonical identities or adapters, and do not
accept a projected digest without independent verification against the actual
bundle.
