# Decision 0463: Native WVHV metadata-request ownership

- Status: Implemented current-host candidate; native evidence process and dual-host promotion pending
- Date: 2026-08-09
- Advances: [Decision 0462](0462-Native-WVHV-Runtime-Header-Ownership.md), [Decision 0461](0461-Native-WVHV-Metadata-Ownership.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier metadata request](../../Specifications/Windvale-Native-Hosted-Verifier-Metadata-Request.md)

## Context

The verifier metadata and runtime-header constructors are Windvale-owned, but
their focused evidence still projected the `WVVR 1` request in C#. The generic
Windvale publication planner already supports the verifier's six services, so
the remaining pure boundary is target/entry, plan, and digest projection rather
than another layout implementation.

## Decision

- Define exact 352-byte `WVVE 1` evidence containing target, profile 2, native
  entry, the canonical six-service `WVPQ 1` request, seven digests, and zero
  reserved bytes.
- Reuse the shared Windvale publication planner and require service IDs 1
  through 6. Do not accept caller-supplied placements or bundle length.
- Construct exact `WVVR 1` in a focused service-free Windvale module and return
  bounded `WVVD 1` success or failure.
- Leave immutable resource acquisition and SHA-256 calculation to the next
  native process slice; this constructor consumes identity evidence only.
- Build production WVB through the native front door and retain C# only as the
  post-construction byte oracle.

## Evidence and consequences

The native project front door builds 19 functions and 12,736 code bytes into a
15,070-byte WVB with SHA-256
`fc87cfad498befe8af90fc5201e07c15e13c4a9363b73c344e1f6e49519dd55a`.

One reviewed focused test passes. Windows and Linux evidence executes
identically under the Windvale interpreter and native backend and produces the
same 384 bytes as the frozen C# oracle. Thirteen malformed size, envelope,
profile, entry, publication, digest, and reserved cases agree between both
Windvale execution modes.

This closes pure verifier metadata-request projection. Native immutable
resource hashing, six-service bundle materialization/orchestration, startup,
container construction, independent Linux execution, and promotion remain.
No broad Seed, OS, Standard, Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Change `WVVE` only when the verifier authority or publication evidence changes.
Do not add paths, host handles, trusted offsets, or digest strings.
