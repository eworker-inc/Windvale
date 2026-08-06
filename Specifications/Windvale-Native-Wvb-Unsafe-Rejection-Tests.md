# Windvale native WVB unsafe rejection tests

## Status and scope

This fixed contract exercises five core unsafe instruction-stream boundaries
through both digest-bound native WVB read-only launchers. It transfers stable
phase, read-only, and process behavior without a live .NET oracle. It is not a
replacement for the broader managed verifier suite or deterministic randomized
containment.

## Fixed inputs

The canonical base is the 174-byte `Wvbˉtoˉwvoˉfixture` WVB at SHA-256
`7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31`.
It contains one `Main() -> i32`, one `i32` local, and the 16-byte code sequence
`i32.const 42; local.store 0; local.load 0; return`. Each fixture changes only
the specified code bytes:

| Case | Mutation | Phase | SHA-256 |
| --- | --- | --- | --- |
| `unknown-opcode` | absolute byte 121, `i32.const` (`0x01`) to `0xFF` | `semantic` | `f84528a577647a8d9c988f2cf082ea642dc7b8f61220bb5d23d57e8d3238c0aa` |
| `truncated-operand` | final byte 136, `return` to operand-bearing `i32.const` | `semantic` | `eac2a31112958af23f89941be6e9591e870438439ea037e2b12a6c23216f74d9` |
| `local-index` | `local.store` operand at bytes 127–130, zero to 999 | `semantic` | `857f94ae40c95dd2f2e3f27ba07892c0ae351f1875fc16c91695e5a3872f56a3` |
| `jump-target` | code becomes `jump 999; i32.const 0; i32.const 0; return` | `semantic` | `b56e962d4e4d24d6366354e1f4798c4352de236dcad421829d4b8714db3eb2a3` |
| `after-return` | code becomes `return` followed by three `i32.const 0` instructions | `typed-execution` | `ece563bb06b953ef1587004c3517c21098702b644511cdda989e49d89d9061e7` |

## Rejection contract

For every fixture, `Verify-Wvb.cmd` / `.sh` and `Inspect-Wvb.cmd` / `.sh` must:

- return process exit `1`;
- write no standard output;
- preserve the complete input identity; and
- emit exactly one LF-terminated phase report.

The semantic report is `wvb status=Invalid phase=semantic` plus LF at SHA-256
`4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5`.
The typed report is `wvb status=Invalid phase=typed-execution` plus LF at
SHA-256
`c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930`.

Success prints the five ordered `PASS` lines followed by:

```text
Tests: 5, Passed: 5, Failed: 0
```

The fixture set is representative, not exhaustive. Nominal metadata and shape
limits, every typed opcode family, hostile lengths, seeded random bytes, and
random source/assembly remain separate evidence.
