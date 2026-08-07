# Decision 0347: Fixed native nominal WVB rejections

- Status: Accepted current-host evidence; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0329](0329-Fixed-Native-Wvb-Unsafe-Rejections.md) and [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Contract: [Native WVB unsafe rejection tests](../../Specifications/Windvale-Native-Wvb-Unsafe-Rejection-Tests.md)

## Context

Decision 0329 moved five representative instruction-stream failures from the
managed verifier test into one fixed native lane. Nominal type identities,
record metadata, record-field selection, and enum comparison still had only
managed case ownership even though the digest-bound native verifier and
inspector already enforced those boundaries.

Generating those mutations in each host script would duplicate WVB layout
logic and let the candidate create its own evidence. Adding another suite would
also split one cohesive read-only rejection boundary merely to keep case counts
small.

## Decision

- Extend the existing unsafe-WVB lane with five fixed base64 fixtures derived
  from separately verified small WVB modules and changed at one exact nominal
  field or same-length name.
- Cover a missing record parameter type, missing record field, duplicate record
  field name, mismatched nominal enum comparison, and duplicate nominal type
  name.
- Pin each complete input identity. The permanent Windows and Linux commands
  decode and digest-check the bytes but do not parse, generate, or mutate WVB.
- Require both native read-only launchers to return `1`, write no standard
  output, emit the exact semantic or typed-execution report, and preserve the
  complete input.
- Keep these cases in the existing `unsafe-wvb` suite. Update its declared
  count from five to ten and the complete retirement plan from 3,030 to 3,035
  cases without adding another command lane.
- Retain nominal count/value-size limits, other typed opcode families, and the
  broader managed verifier as separate evidence pending their own bounded
  transfer or the final recovery archive.

## Evidence

| Case | Bytes | Phase | SHA-256 |
| --- | ---: | --- | --- |
| `record-parameter-type` | 223 | `semantic` | `8e89cf9b526e1ea93d81d62425f95986daff4469dc7f113f5e38b580ccf163aa` |
| `record-field-index` | 199 | `typed-execution` | `1d5ed90586e2327af309cb9fe6ba1110da879ee461f7fd56d7c5414d1c637999` |
| `duplicate-record-field` | 191 | `semantic` | `73867dcf74f30f4b9237091aa59ea981200f4139636b67eb730bdb71752571b6` |
| `mismatched-enum-comparison` | 234 | `typed-execution` | `6ae2e65a43f68f0aa4b46b7ca306ad1dd06b72b1328e02e611f98e9f7abc869e` |
| `duplicate-nominal-name` | 211 | `semantic` | `60d12d56015678f3197a1413cfb058bff64188a8e2256d09f504280fad805f9c` |

The reviewed 1,982-byte LF-only retirement plan is SHA-256
`b5c16309688400b138a76c72e147533d124e0b26615a9b382ebf4b55dda07aaa`
and fixes 23 suites with 3,035 cases.

The focused Windows command
`Test-Retirement-Suite.cmd --filter unsafe-wvb` passes its one selected suite
and all ten cases in 5.144 seconds. It starts no .NET process and builds no
artifact. The same cases were not repeated through the managed wrapper or the
complete coordinator. Linux execution, Development, Standard, Qualification,
promotion, and the grouped end-of-goal gate remain deferred.

## Consequences

The fixed native test boundary now owns representative nominal metadata,
identity, and field-selection rejection in addition to instruction-stream
safety. The managed test remains useful as independent recovery evidence, but
these ten stable cases no longer require it in the retirement suite.

No verifier implementation, WVB format, compiler, WebAssembly artifact, or
product package changed.

## Reconsideration triggers

Regenerate the affected fixture and report identities if WVB 1.11, nominal
encoding, verification phases, or either read-only launcher changes. Add future
cases only for distinct unsafe boundaries; do not duplicate randomized
containment or move host-side mutation logic into the permanent command.
