# Decision 0430: Fixed native typed WVB rejections

- Status: Implemented current-host focused evidence; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0347](0347-Fixed-Native-Nominal-Wvb-Rejections.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native WVB unsafe rejection tests](../../Specifications/Windvale-Native-Wvb-Unsafe-Rejection-Tests.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The fixed unsafe-WVB lane owned five core instruction-stream failures and five
nominal metadata/identity failures. The managed verifier still exclusively
owned several compact control-merge, typed receiver, record-construction, and
enum-kind cases even though both digest-bound native read-only tools enforce
those boundaries.

Large text/byte value limits would require multi-megabyte fixtures and belong
with the hostile-size owners. The six selected cases remain between 168 and 225
bytes and add distinct verifier boundaries without making the cohesive unsafe
lane or its source files unusually large.

## Decision

Use the frozen Stage 0 verifier once as a fixture-construction oracle. For each
case, serialize a separately verified valid small WVB, change exactly one byte,
and retain only the resulting immutable base64 fixture. Permanent Windows and
Linux commands decode and digest-check these values; they never generate or
mutate WVB.

Extend `Test-Wvb-Unsafe-Rejections.cmd` and `.sh` with:

| Case | Bytes | Exact mutation | Phase | SHA-256 |
| --- | ---: | --- | --- | --- |
| `mismatched-merge` | 168 | branch target byte 116, `7` to `17` | `typed-execution` | `f3f98931b5a701c805e9889768abe2c8536fb4ff04fd6a614ddf7f0732f6b7a2` |
| `bytes-length-on-i32` | 173 | code byte 128, `bytes.const` to `i32.const` | `typed-execution` | `f06d084a5f78b8d12e8503cfacd841565527c7a075dbcad40626e48f6d9e48c0` |
| `record-create-wrong-field-type` | 185 | code byte 113, `u32.const` to `i32.const` | `typed-execution` | `a074c6a8229870bb45a3de8764a2ffd51b8091f0e4d50f48330c560927ca4c59` |
| `invalid-enum-member` | 202 | enum-member byte 118, `1` to missing `2` | `semantic` | `ddd000954aeb8d0c02775128ae52615d9bf4237bda9741eb39e6f9efb4f2ddbe` |
| `enum-const-on-record` | 225 | nominal-index byte 114, enum `1` to record `0` | `semantic` | `3d09445c44bf2d1e3f5b811f254e0bccc902366ad242ea4cf101fc44f23b99d8` |
| `duplicate-enum-value` | 192 | second enum-value byte 188, `1` to duplicate `0` | `semantic` | `da453ca0cbe661ab695e21ce8f2ee2530a303ad996bbedfe6f0ae5e9bbb0a00c` |

Both native verifier and inspector must reject each input with the exact phase
report, empty standard output, exit `1`, and unchanged complete input identity.
Keep all sixteen cases in the existing `unsafe-wvb` lane. The retirement plan
remains 24 suites and grows to 3,044 fixed cases.

## Evidence and consequences

The exact managed source assertion used to construct the bases passes 1/1. Its
temporary export instrumentation and generated binary directory were removed
before repository staging; `Program.cs` returned byte for byte to its committed
state. The retained managed native-wrapper expectation receives only the bounded
six-line synchronization required by the expanded recovery check.

The reviewed Windows command
`Test-Retirement-Suite.cmd --filter unsafe-wvb` passes all sixteen cases in 7.1
seconds. Every case traverses both native readers without starting .NET. The
other 23 suite lanes, broad managed verifier, Linux execution, and grouped gate
were not run.

This reduces the set of compact typed-verifier assertions that depend on the
managed harness. Oversized value limits, other typed opcode families, and
randomized containment retain their existing focused or recovery owners. No
verifier, compiler, runtime, WebAssembly, or product artifact changed.

## Reconsideration triggers

Regenerate one fixture from its valid base when WVB encoding or verification
phase ownership changes. Do not introduce permanent host mutation logic or
large padded fixtures merely to increase a case count; route large values to a
bounded hostile-size contract instead.
