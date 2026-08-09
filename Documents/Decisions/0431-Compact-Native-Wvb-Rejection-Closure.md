# Decision 0431: Compact native WVB rejection closure

- Status: Implemented current-host focused evidence; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0430](0430-Fixed-Native-Typed-Wvb-Rejections.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native WVB unsafe rejection tests](../../Specifications/Windvale-Native-Wvb-Unsafe-Rejection-Tests.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

After Decision 0430, four distinct compact verifier boundaries remained useful
to both permanent native read-only tools: declared stack agreement, record-field
receiver typing, enum-name receiver typing, and nominal-kind agreement. Each can
be represented by a 157-to-204-byte WVB and belongs in the existing cohesive
unsafe-WVB lane.

Three other managed assertions should not become duplicate permanent fixtures.
The truncated `i64.const` case exercises the same fixed-width operand decoder as
the retained truncated scalar fixture. Invalid in-memory UTF-16 is rejected
before canonical WVB can exist and remains a frozen object-model recovery
assertion. The first byte above the byte-data limit needs a multi-megabyte value
and belongs with bounded hostile-size evidence.

## Decision

Use the frozen Stage 0 verifier once to serialize separately verified valid
bases, apply one exact byte mutation to each, and retain only these base64
fixtures:

| Case | Bytes | Exact mutation | Phase | SHA-256 |
| --- | ---: | --- | --- | --- |
| `stack-capacity` | 157 | maximum-stack byte 101, `1` to `0` | `typed-execution` | `ba69564377f6e9b2ded8b9c6125205654eaf22cb4015be535015de33af23c728` |
| `record-field-on-primitive` | 190 | code byte 118, `local.load` to `u32.const` | `typed-execution` | `d5deb4c26a19234066db169a40e5a2eaac99a4e03a4f0d08b816485431ca3396` |
| `enum-name-on-primitive` | 204 | code byte 118, `local.load` to `i32.const` | `typed-execution` | `155d619ae7732c705b7881693ba1e6f1cd7db3cbbe2e8a5687fbd27e60097405` |
| `wrong-nominal-kind` | 197 | parameter-shape byte 88, enum `8` to record `7` | `semantic` | `da375377c69ca8c87fe17f34460617330fdcc1763e1a465de4805e1ead98cc93` |

Both digest-bound native readers must reject every fixture with the exact phase
report, empty standard output, exit `1`, and unchanged complete input identity.
The existing lane grows from sixteen to twenty cases. The 2,054-byte retirement
plan remains 24 suites and grows from 3,044 to 3,048 fixed cases at SHA-256
`9c960e03e59a9fdd76fecfbf962e0cae9b33b96e941fa3c8254288380ef52960`.

## Evidence and consequences

The focused managed source assertion passed 1/1 in 59 ms after the valid bases
and exact mutations were reviewed. Temporary generator code was removed and
`Program.cs` returned byte for byte to its committed state. The generated
directory was sent to the Recycle Bin after the immutable base64 values were
added. The retained managed native-wrapper expectation receives only the four
new output lines and summary required by the frozen recovery check.

The reviewed Windows command
`Test-Retirement-Suite.cmd --filter unsafe-wvb` passes all twenty cases in 8.9
seconds. Every case traverses both native readers without starting .NET. The
other 23 lanes, managed wrapper, broad local verifier, Linux execution, and
grouped retirement gate were not run.

This closes the useful compact cases in the managed unsafe-bytecode assertion
without copying it line for line. Future additions require a genuinely distinct
portable boundary or a focused hostile-size owner. No verifier, compiler,
runtime, WebAssembly, or product artifact changed.

## Reconsideration triggers

Regenerate a fixture from its valid base if WVB encoding or verification-phase
ownership changes. Reconsider excluded assertions only if their decoder or
object-only ownership changes, or if a bounded hostile-size contract can carry
the large value without normalizing multi-megabyte inline fixtures.
