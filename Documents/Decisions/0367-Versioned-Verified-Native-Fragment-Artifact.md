# Decision 0367: Versioned verified native-fragment artifact

- Status: Accepted current-host format and malformed-input evidence; production-consumer migration, Linux execution, and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0365](0365-Native-Publication-Planner-Execution.md)
- Contract: [Windvale native fragment artifact](../../Specifications/Native-Fragment-Artifact.md)

## Context

After Decision 0366, the normal runtime still embeds three variable-input WVBs:
the segmented enum-metadata consumer and the two publication planners. Each is
verified and lowered once, then its resulting native fragment is cached and
executed. Removing that decode/lowering step cannot discard the target, ABI,
nominal metadata, service requirements, symbols, or patch evidence required by
the independent native verifier.

Raw final code would hardcode missing facts. WVO preserves sections, symbols,
and relocations but deliberately clears patch fields and omits target, ABI,
nominal types, and required services. Loading WVO would therefore add a second
linker path and require parallel metadata assumptions.

## Decision

- Define `WVNF 1.0`, a direct bounded serialization of the complete verified
  `Nativeˉfragment`: explicit target, ABI, architecture, alignment, final code,
  symbols, patches, nominal types, and required services.
- Preserve already applied base-independent code and the patch records used to
  verify it. Serialize patch targets as bounded symbol indices rather than
  repeated names.
- Bound the complete artifact at 64 MiB and retain the existing 32 MiB code,
  4,096-symbol, 65,536-patch, 1,024-type, 64-field, 256-member, and 12-service
  ceilings. Strings remain strict UTF-8 and at most 255 bytes.
- Keep parsing, nominal metadata, and byte I/O in separate focused source files.
  Structural decoding never authorizes execution; every production load must
  call `Readˉandˉverify` and then apply the exact artifact identity selected by
  its owner.
- Keep WVB as the portable semantic contract and recovery source. WVNF is
  target/ABI-specific derived evidence, not a replacement module format or a
  reason to remove the independent WVB differential lane.

## Evidence and consequences

One focused contract compiles a real hosted fragment containing final machine
code, static-data patches, record and enum metadata, and two ordered services.
It proves deterministic write/read/write bytes and field-by-field equality,
the exact 255-byte name boundary, encoder rejection above it, and independent
native verification after decoding.

Malformed coverage includes an artifact above 64 MiB, empty and truncated
input, invalid magic/version/total length, zero code, excessive counts,
unsupported flags, invalid UTF-8, trailing data, an unknown service, a
noncanonical value shape, and machine-code corruption that passes structural
decoding but fails the existing x86-64 verifier.

The final focused Release build succeeds with zero warnings and errors in 7.70
seconds; the single affected contract passes 1/1 in 0.265 seconds. No
Development, Standard, Qualification, or grouped cross-host gate was run. No
production consumer changes in this decision. The next slice will generate and
digest-bind exact WVNF artifacts from the three retained WVBs, compare them
against fresh Stage 0 lowering, remove those WVBs from the normal runtime
assembly, and retain the WVBs only for qualification and recovery.

## Reconsideration triggers

Revise the format rather than appending assumptions when a future native target
needs different verifier evidence. A native loader may replace the temporary
managed decoder only after it rejects the same malformed corpus and reproduces
the same verified fragments. Do not promote raw code or WVO plus ad hoc C#
metadata as a shortcut.
