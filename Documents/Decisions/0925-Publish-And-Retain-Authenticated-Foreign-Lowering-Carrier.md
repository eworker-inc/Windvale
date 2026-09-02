# Decision 0925: publish and retain the authenticated Foreign lowering carrier

## Status

Accepted implementation checkpoint on 2026-09-02. Private WVFB publication,
coordinator-side validation, and retained binding are implemented. Passing the
carrier into typed analysis and emission remains later Slice 8 work.

## Context

Decision 0923 introduced the compiler-owned WVFB lowering carrier, but the
hosted `wvbind` product still returned only a digest line and the production
coordinator could not retain those facts. Reconstructing the facts later would
duplicate semantic work and could separate native lowering from the exact
authenticated source, target descriptor, and foreign catalog that were bound.

The carrier must remain non-authoritative. `wvbind` does not consume the
admission evidence, source lock, or profile and therefore cannot establish that
its inputs belong to an authenticated production invocation.

## Decision

1. The hosted binder accepts exactly four distinct paths: WVSS, WVTD, WVFC, and
   a private WVFB output path.
2. After complete binding success, `wvbind` serializes the bounded canonical
   WVFB carrier and writes it before publishing its single success-evidence
   line. The line includes the carrier byte length and SHA-256 digest.
3. The production coordinator owns the output path. It requires a new bounded
   single-link ordinary file, makes it read-only, and snapshots its bytes.
4. The coordinator independently validates the WVFB structure, target identity,
   catalog ordering and correlation, and every currently supported lowering
   fact. It constructs the expected success evidence from retained bytes rather
   than trusting binder-provided fields.
5. After exact evidence comparison, the coordinator rechecks the six retained
   authenticated input snapshots and the WVFB snapshot. It retains the carrier
   but still stops with exact `Foreignˉloweringˉpending`; the Analyzer and emitter
   are not launched for a foreign-bearing source set.

## Verification

The production-ingress sentinel owner covers the exact successful handoff and
rejects a missing, truncated, wrong-target, remapped-record, or unsupported-fact
carrier without launching the Analyzer, emitter, or final publication. The
authenticated-binding owner compiles the four-path Windvale driver and retains
the WVFB structural and semantic binding cases.

## Consequences

The production coordinator now owns an independently checked typed input for
the next lowering phase instead of requiring semantic reconstruction. The
bounded evidence line grows from 351 to 447 UTF-8 bytes. WVFB remains private,
non-transferable, and insufficient to prove admission.

The next implementation step is a typed Foreign-call operation in the
Analyzer/WVLB/WVIR path, followed by ABI lowering and native emission using this
retained carrier.

## Nonclaims

This checkpoint does not implement Foreign-call WVLB or WVIR, WVB imports,
native thunks, dynamic linking, execution containment, additional ABIs, or
portable foreign execution.
