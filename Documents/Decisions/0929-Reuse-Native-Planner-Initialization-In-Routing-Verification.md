# Decision 0929: reuse native planner initialization in routing verification

- Date: 2026-09-02
- Status: Accepted and implemented
- Extends: [Decision 0928: run shared development meta-verification once](0928-Run-Shared-Development-Meta-Verification-Once.md)
- Current contract: [native changed-file verification](../../Specifications/Windvale-Native-Changed-Verification.md)

## Context

The native routing contract contains 264 independent changed-path cases. Each
case needs a fresh selection result, but it does not need to reread and rebuild
the same verification-owner registry, duration profiles, development-target
maps, and project metadata. Invoking the planner as a new script command for
every case repeated that immutable initialization and PowerShell command
resolution.

On the same Windows checkout, the complete routing verifier took 68.6 seconds
before this change. A 20-call probe took 5.4 seconds without initialization
reuse and 0.67 seconds with it.

## Decision

- Let a caller supply an in-memory initialization cache to the native
  changed-path planner. Ordinary planner calls remain uncached unless they
  explicitly supply that cache.
- Scope a cache to one PowerShell process and one immutable repository source
  state. Do not serialize it, share it between hosts, or reuse it after changing
  planner inputs.
- Cache only the initialized routing registries and their validated base
  eligibility. Create new suite, gap, target, mode, and verification-selection
  state for every changed-path request.
- The routing contract verifier owns one cache for its 264 cases and resolves
  the planner command once. It still evaluates and asserts every case
  independently.
- A cache optimization must fail the same contract cases as an uncached call.
  It may reduce setup work, but it must not remove cases or weaken assertions.

## Consequences

All 264 native and 31 general routing cases still pass. On the same checkout,
the complete verifier fell from 68.6 seconds to 24.6 seconds, a 64 percent
wall-clock reduction. The public changed-file planning path keeps its previous
fresh-initialization behavior, so long-lived callers cannot accidentally use
stale repository metadata unless they explicitly opt into and mis-scope a
cache.

## Reconsideration triggers

Replace the caller-owned cache with a reusable planner object or Windvale-native
implementation when that boundary becomes simpler than the PowerShell script.
Revisit the captured initialization set if routing starts mutating registry
objects, if cases run concurrently, or if repository inputs can change during a
single routing-contract run.
