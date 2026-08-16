# Decision 0648: Segmented compiler development application checkpoint

Status: Accepted

Date: 2026-08-16

Extends: [Content-addressed hosted-application development checkpoints](0554-Content-Addressed-Hosted-Application-Development-Checkpoints.md)

## Context

Packaging the source compiler lowers a large WVB into dozens of native fragments,
links and transports those fragments, and then assembles the complete hosted
application. The final hosted-container step repeats deterministic work even when
its complete inputs are unchanged. Decision 0554 already defines a fail-closed,
content-addressed application checkpoint, but the segmented compiler command did
not expose it.

The default build and qualification paths must remain independent of host-local
cache state. Reuse must not hide changes in staging, linking, transport, target
selection, services, startup objects, executable mode, or application bytes.

## Decision

Add an explicit `--development-cache` option to the Windows and Linux segmented
compiler packaging commands.

- Always stage the input WVB, link its native objects, and perform canonical image
  transport freshly.
- With the option present, pass the fresh WVB, ordered fragments, fragment count,
  entry, profile, and target to Decision 0554's existing hosted-application
  checkpoint.
- Retain the complete versioned key over every hosted publication input and the
  existing hit validation, byte rehash, executable-mode preservation, and
  fail-closed corruption behavior.
- Keep the option absent by default. Reconstruction owners, GitHub shards, and
  qualification remain cache-independent.
- Maintain repeatable measurement tools that require cold creation, warm reuse,
  and byte-identical results rather than inferring improvement from a cache-status
  line alone.

## Consequences

- Repeated local packaging can reuse the most expensive final deterministic
  application assembly without weakening the freshly produced native-image
  boundary.
- An isolated Windows x64 profile-7 measurement of the 950,265-byte source
  compiler WVB took 270.088 seconds cold and 101.729 seconds warm, a 2.66 times
  speedup. Both runs produced the same 28,313,600-byte application with SHA-256
  `1ee8066b91834bdd1d943a34c5bee9dd8e78aba0abf6015bdc8d478ad3a10c2e`.
- Any change to the compiler WVB, fragments, entry, profile, target, driver,
  toolset, enum requests, services, or startup object selects another key.
- Invalid cache entries reject the build and are not repaired automatically.
- Cold and warm timings remain machine-specific performance evidence, not a
  semantic guarantee or required threshold.
- Extending reuse into staging, object lowering, linking, or transport requires a
  separately keyed and verified checkpoint at that boundary.

## Reconsideration triggers

Reconsider this decision if a relevant input can change without selecting another
key, a hit changes bytes or executable mode, either host behaves differently, the
default or qualification path begins consulting the cache, or a native incremental
publisher provides independently admitted reuse with a smaller trusted surface.
