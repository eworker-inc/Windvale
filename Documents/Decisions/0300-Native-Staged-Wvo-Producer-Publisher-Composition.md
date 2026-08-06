# Decision 0300: Native staged-WVO producer/publisher composition

- Date: 2026-08-06
- Status: Implemented candidate; full-tool self-lowering and grouped dual-host qualification pending
- Advances: [Decision 0299](0299-Fixed-Native-Staged-Wvo-Publication.md), [Decision 0284](0284-Versioned-Native-Object-Staging-Manifest.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0299 supplies the fixed native consumer for exact staged WVO
snapshots. The existing Windvale staging tool already produces those bounded
chunks and the strict `WVOP 1` manifest, but it had no digest-bound standalone
Windows/Linux application identity. Tests therefore could not exercise the
complete producer-to-publisher process boundary without a managed execution
host.

Running the expensive compiler self-lowering case is deliberately part of the
grouped end-of-goal gate. The next bounded step is to package the existing
producer exactly and compose both native processes on the small canonical
return-42 fixture.

## Decision

- Add exact application targets
  `windows-x64-wvo-staging-producer-v1` and
  `linux-x64-wvo-staging-producer-v1` for the existing
  `Compilerˉnativeˉx64ˉloweringˉstagingˉtool` module.
- Require the canonical module byte identity, hosted profile, six declared
  capabilities, ten native services, and one exported `Main`. Independently
  verify the constructed hosted package and pin its complete byte identity.
- Reuse the established `Wvb_to_wvo` hosted container because the staging
  producer has that exact authority and service shape. No second staging
  algorithm or platform implementation is introduced.
- Expose both targets through `windvale compile` and `windvale aot`, including
  deterministic extension selection and Linux executable publication.
- Compose the current-host producer and Decision 0299 publisher as separate
  native processes. The producer writes three bounded chunks and the manifest;
  the publisher independently admits those files and atomically replaces a
  sentinel destination with the exact canonical WVO.
- Keep package construction and the new pipeline test in focused files. The
  full compiler self-lowering input remains unchanged and is not run as a
  narrow inner-loop check.

## Evidence and consequences

- The exact producer WVB is 394,780 bytes at SHA-256
  `77158b228c204b587dbf559621ad7c717d4eb5b418c32b783204cd350525ac76`.
- The Windows producer is 5,723,136 bytes at SHA-256
  `993b2c5a531261cc5290e45edef0daa329de95b024f5ea749660895df84466de`.
- The Linux producer is 5,722,112 bytes at SHA-256
  `b38352b1e8d04bd3ac3f66e4ea27dde8391a738e9ce50031ad1f4927a53065d8`.
- Current-host Windows composition publishes the exact 479-byte return-42 WVO
  at SHA-256
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`,
  observes exactly three chunk sidecars, leaves no publisher scratch file, and
  loads no CLR component in either process.
- The reviewed focused compiler selection passes 1/1 in 10.783 test seconds
  after a 9.61-second zero-warning Release build; the complete command takes
  24.8 seconds. No broader local verification level was run.
- C# changes are limited to Stage 0/recovery package construction, CLI routing,
  and the test harness. Lowering, staged production, admission, identity checks,
  mutation, and cleanup execute in Windvale/WVA. No WebAssembly implementation
  changed.

This does not run the producer on its own full compiler WVB, prove Linux
process composition, make the producer/publisher chain the ordinary launcher,
replace either Stage 0 package constructor, promote artifacts, or close the
retirement gate. Development, Standard, Qualification, and the final grouped
dual-host gate remain deferred.

## Reconsideration triggers

Revisit the split if native process startup becomes a measured bottleneck, the
staged format changes, or a later Windvale supervisor can own both tools and
their private sidecars with equal or stronger isolation. A fused application
must retain the independent snapshot/content verification boundary; it may not
turn staging buffers into implicitly trusted publication input.
