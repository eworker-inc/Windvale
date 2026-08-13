# Decision 0302: Digest-bound native WVO linker candidate

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification and promotion pending
- Advances: [Decision 0221](0221-First-Native-Wv-Linker-Front-Door.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native linker](../../Specifications/Windvale-Native-Wv-Linker.md)

## Context

Decision 0221 moved the standard flat-image linker into one Windvale-owned
core and defined exact Windows and Linux native package profiles. The loose
experimental artifact directory contained the current WVB but older PE/ELF
package identities, so binding a launcher to those files would have preserved
stale bootstrap output.

The source-to-WVB front door is already qualified and native. The hosted
application constructor remains an explicit Stage 0 recovery dependency. This
slice can therefore regenerate one clean candidate inventory, expose it behind
a digest check, and leave package-constructor retirement plus cross-host
promotion for later gates.

## Decision

- Rebuild `Projects/Linker/Windvale-Wv-Linker.wvproj` through the qualified native source
  front door into a clean candidate directory. Construct its exact Windows and
  Linux hosted applications once through the retained Stage 0 package writer.
- Add a manifest that pins the WVB, PE, and ELF size, digest, target, source,
  and pending qualification status.
- Add digest-bound `Link-Wvo.cmd` and `Link-Wvo.sh` candidate launchers. They
  require the link form's minimum four arguments, verify the complete
  current-host application digest, and forward the base address, entry,
  output, and one through 64 WVO inputs to the Windvale linker.
- Keep `windvale link` as the explicit Stage 0/recovery route until this exact
  candidate passes the grouped dual-host gate. Do not claim promotion from the
  current-host check.
- Cover only the new boundary: manifest and cross-host digest pins, one exact
  canonical two-object link, and deterministic usage rejection. The existing
  package test continues to own construction, malformed input, output
  preservation, and absence-of-CLR evidence and is not rerun for unchanged
  source.

## Evidence and consequences

- The canonical WVB is 127,482 bytes at SHA-256
  `592467003974dab240e1f90b5a647d360cfd4cc6d7186bfdedbcc3ba8788f386`.
- The Windows application is 1,655,296 bytes at SHA-256
  `ca88735061d7e36e79813346621a867a9293d04d3c01ffb0336f4ee32cbe316d`.
- The Linux application is 1,654,784 bytes at SHA-256
  `994f27f5a2449990b767c0ed8c8c367e2676d41d652ee9a61eab1de36de82dc2`.
- Clean native-WVB plus paired Stage 0 package regeneration took 12.1 seconds
  and reproduced all three identities already pinned by the reviewed package
  test.
- The focused linker selection passes 1/1 in 0.837 test seconds after a
  9.64-second zero-warning Release build; the complete command takes 14.9
  seconds.
- The current-host launcher emits the exact canonical map and output bytes.
  The WVO linker process itself is native; C#/.NET remains involved only in
  candidate package construction and the current test harness.
- No linker semantics, WebAssembly implementation, or source-language behavior
  changed. Development, Standard, Qualification, promotion, native package
  construction, and ordinary-path cutover remain deferred.

## Reconsideration triggers

Regenerate all three identities if the linker source, native backend, hosted
profile, startup, service bundle, or package writer changes. Do not retain the
older experimental packages as compatibility artifacts; only a named recovery
archive may preserve obsolete bootstrap output.
