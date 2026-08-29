# Decision 0882: complete Windvale Language 1.0 Slice 7 qualification

## Status

Accepted and qualified on 2026-08-29.

## Context

Slice 7 connects the frozen Language 1.0 structured-task design to the current
compiler, WIR, WVB, verifier, source-built scalar runtime, hosted providers, and
native publication path. Earlier decisions established bounded sequential and
parallel-capable execution, observable completion order, explicit
child-provider loss and recovery generations, paired Windows/Linux
reconstruction, candidate promotion, and the complete native verification
ownership boundary.

The final qualification attempts found harness and inventory defects after the
affected product behavior had passed or reached its exact artifact boundary:

- the filesystem provider correctly differs by host, but its summary still
  claimed one cross-host binary identity;
- a refreshed provider implementation had changed the exact native filesystem
  artifacts without changing their public source contract;
- nested Windows short-path spelling could escape the split-cache failure
  test's lexical temporary-root check;
- the frozen console publisher could spend substantial time reconstructing a
  candidate that public packaging would later reject by size or executable
  signature; and
- the offline-package-uninstall owner had 14 cases and six safety rejections,
  while the registry retained the preceding 13-case, five-rejection summary.

These were qualification-boundary defects. They did not justify weakening
compiler, runtime, package, cache, publication, or cross-host checks.

## Decision

1. Report the filesystem provider as `readiness=host-specific` and retain exact
   platform-scoped native artifact identities. Refresh the changed filesystem
   WVO, linked image, process object, and boot-probe evidence together.
2. Canonicalize both the allocated temporary root and locally created child in
   the split-cache failure path. Bound child execution, output, diagnostics,
   and cleanup, and remove only the validated local temporary directory.
3. Reject impossible console publication candidates before invoking the frozen
   publisher. Read at most four leading bytes and enforce the existing public
   platform bounds and executable signatures: Windows `MZ` at
   2,048..4,196,352 bytes and Linux ELF at 5,120..4,202,608 bytes. Keep the
   frozen publisher and its deeper validation unchanged.
4. Correct the offline-package-uninstall inventory to 14 cases with six safety
   rejections. The final registry contains 114 owners and 5,618 cases in 19,004
   LF-only bytes at SHA-256
   `a3d9217b63d187a697a5df116c6ebe14e4ada96ca64df9a93119b202b0ed5668`.
5. Accept commit
   `23c66552d26f0a5f2ca969a62e5f59425100c206` as the exact Slice 7 qualified
   source state. GitHub Actions run
   [33265179717](https://github.com/eworker-inc/Windvale/actions/runs/33265179717)
   completes all 18 jobs: 14 pass, four correctly skip outside the selected
   Qualification scope, and none fail. Both native bootstraps, both
   WebAssembly jobs, all eight Windows/Linux native shards, classification, and
   aggregation pass.

## Consequences

- Slice 7 is complete on both permanent hosts at one exact source state.
- The accepted evidence covers the complete 114-owner, 5,618-case native
  registry rather than a development-only subset.
- Cache and publication failures remain bounded and fail closed before
  avoidable reconstruction work.
- Host-specific native bytes remain explicit; cross-host conformance does not
  falsely require identical platform artifacts.
- Slice 8 may begin from this qualified boundary. Slice 7 completion does not
  claim that the complete Language 1.0 migration or System/FFI surface is
  implemented.

## Reconsideration triggers

Requalify the complete paired-host boundary after a change to portable source
semantics, WIR, WVB, verification rules, runtime behavior, native serialization,
or any exact qualified artifact identity. Change the console preflight only
when the public publisher contract changes. Replace the host-specific provider
classification only if one deliberately portable native provider identity is
defined and qualified on both hosts.
