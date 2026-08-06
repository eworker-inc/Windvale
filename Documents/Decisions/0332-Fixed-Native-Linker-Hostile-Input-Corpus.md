# Decision 0332: Fixed native linker hostile-input corpus

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0325](0325-Expanded-Native-Linker-Rejection-Families.md), and [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Contract: [Native linker hostile-input tests](../../Specifications/Windvale-Native-Linker-Hostile-Input-Tests.md)

## Context

The managed `Stage 0 linker contains hostile objects and remains deterministic`
test creates 200 zero-through-511-byte values with a seeded framework PRNG and
requires every value to return `WVL1002` with empty image and map results. The
input count and bounds are useful containment evidence, but the framework PRNG
sequence is not a language or linker semantic and should not become a permanent
native-test dependency.

The curated native rejection matrix already pins every public diagnostic family.
Folding 200 raw values into that ten-case matrix would obscure its boundary and
produce a much slower ordinary diagnostic check.

## Decision

- Replace the live framework-specific generation dependency with 200 immutable
  values derived from a fully specified xorshift32 sequence seeded by
  `0x0057564C`; fix empty and 511-byte boundary cases explicitly.
- Retain the 48,877 input bytes plus their 16,378-byte per-file manifest in one
  63,224-byte digest-bound gzip archive rather than 200 source fragments.
- Add a separate Windows/Linux `Test-Linker-Hostile-Inputs` command. Require
  every case to match exact `WVL1002`, empty standard output, unchanged input,
  and unchanged destination behavior through the public native launcher.
- Add `linker-hostile` to the direct retirement suite as its own 200-case lane.
  Keep `linker-rejections` focused on the ten distinct diagnostic families.
- Generate nothing and consult no managed oracle during the permanent run.

## Evidence and consequences

- The manifest contains exactly 200 inputs, 164 distinct lengths, explicit zero
  and 511-byte boundaries, and 48,877 total bytes. Every extracted length and
  SHA-256 was independently checked before native execution.
- The first focused Windows attempt stopped before linker invocation because
  `certutil` rejected the valid zero-byte file. The reviewed correction accepts
  only length zero paired with the canonical empty SHA-256; it changes no corpus
  byte or expected linker result.
- The final Windows retirement-suite filter passes all 200 cases in 55.668
  seconds. The other 74 previously transferred cases were not rerun.
- The current direct plan is 868 LF bytes at SHA-256
  `088bf17789d2aaef00c1b063ac98e39cd2c3d6aa9de119bb127fd16d5c81565c`
  and now fixes 11 suites containing 274 cases.
- This transfers one complete seeded managed-test responsibility without
  changing linker, WVO, candidate application, or WebAssembly implementation.
  Linux execution, promotion, and the grouped end-of-goal gate remain deferred.

## Reconsideration triggers

Revise the corpus version and identities if the raw-input bound, case count,
WVO magic, linker report, or `WVL1002` contract changes. Add valid-shaped
mutations in their own differential owner rather than weakening this raw-byte
contract or increasing this suite merely to accumulate more arbitrary samples.
