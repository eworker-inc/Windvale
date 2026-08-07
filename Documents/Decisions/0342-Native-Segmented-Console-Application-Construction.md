# Decision 0342: Native segmented console-application construction

- Status: Accepted local implementation; cross-host qualification and promotion pending
- Date: 2026-08-07
- Extends: [Decision 0130](0130-Windvale-Owned-Console-Application-Construction.md), [Decision 0223](0223-First-Native-Console-Application-Packager.md), and [Decision 0341](0341-Fixed-Native-Console-Segmented-Size-Rejections.md)
- Advanced by: [Decision 0343](0343-Native-Console-Packager-Source-Reconstruction.md)

## Context

The sparse Windvale-owned console construction recipe admits a maximum 4 MiB
native image, but the first native packager materializes one complete byte value
and therefore rejects the valid 4,196,352-byte PE and 4,202,608-byte ELF. Stage
0 recovery code remained the only constructor for those two accepted boundary
applications. The two-snapshot verifier already admits their bounded shape.

Freezing Stage 0-produced applications as fixtures would transfer a test result
without transferring construction responsibility. Widening the ordinary
Windvale byte-value limit would weaken an unrelated runtime boundary.

## Decision

- Add a focused portable recipe streamer that validates the existing `WVCC 1`
  sparse recipe and emits exactly two bounded values without joining them.
- Revalidate both completed chunks through the shared portable application
  verifier before exposing success.
- Add the fixed 60-byte `WVCS 1.0` staging manifest and write it only after both
  chunk writes complete.
- Package the hosted tool through explicit Windows and Linux segmented-packager
  targets while retaining the existing narrow console-packager service profile.
- Freeze one maximum native image and exact Stage 0 application identities in a
  two-case native retirement lane.
- Keep the candidate marked `stage0-recovery`: the current project and ordinary
  packager both reach the known native source-binding ceiling, so runnable
  artifacts are not yet reconstructible native replacements.

Decision 0343 later establishes that this report came from noncanonical project
inventory order rather than binding capacity. The exact WVB is now natively
reconstructible; the runnable host containers remain Stage 0-constructed.

## Evidence

- One compile-only Stage 0 tooling build completed with zero warnings and errors.
- The segmented module built as a 68,451-byte WVB and paired 782,336-byte host
  applications with the identities pinned by the candidate manifest.
- The focused Windows native runner passed both maximum constructions, including
  exact chunk, manifest, Stage 0 application, report, verifier, and input hashes.
- The retained managed construction test now pins the same complete maximum PE
  and ELF hashes instead of checking recovery round trips alone.
- The focused managed layout assertion passes 1/1, and the separately selected
  segmented AOT-target discovery assertion passes 1/1 without a second build.
- The native source front door rejected both the existing ordinary packager and
  this project at the same `Sourceˉbindings` frontier; that attempted build is a
  recorded blocker, not qualification evidence.

## Consequences

Maximum valid construction no longer requires one oversized Windvale value or
a live managed oracle during normal candidate execution. The permanent suite
grows to 22 commands and 3,028 fixed cases. Public durable publication still
needs a segmented admission adapter, and the candidate cannot be promoted until
native reconstruction and Windows/Linux qualification close.

## Reconsideration triggers

Reconsider the fixed two-chunk shape if version-1 application limits change, if
the ordinary byte-value ceiling changes through a separate runtime decision, or
if a shared typed streaming/publication abstraction replaces the current WVO
and console-specific staging contracts.
