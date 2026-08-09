# Windvale native WVB unsafe rejection tests

## Status and scope

This fixed contract exercises sixteen unsafe instruction-stream, typed, and nominal-type
boundaries through both digest-bound native WVB read-only launchers. It
transfers stable phase, read-only, and process behavior without a live .NET
oracle. It is not a replacement for the broader managed verifier suite or
deterministic randomized containment.

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

Five additional fixtures start from separately verified small WVB modules and
change one exact nominal field or same-length name. Their complete identities,
rather than a host-side generator, are the permanent inputs:

| Case | Bytes | Mutation | Phase | SHA-256 |
| --- | ---: | --- | --- | --- |
| `record-parameter-type` | 223 | helper parameter record index `0` to missing index `1` | `semantic` | `8e89cf9b526e1ea93d81d62425f95986daff4469dc7f113f5e38b580ccf163aa` |
| `record-field-index` | 199 | `record.field` index `0` to missing field `1` | `typed-execution` | `1d5ed90586e2327af309cb9fe6ba1110da879ee461f7fd56d7c5414d1c637999` |
| `duplicate-record-field` | 191 | second field name `Rght` to duplicate `Left` | `semantic` | `73867dcf74f30f4b9237091aa59ea981200f4139636b67eb730bdb71752571b6` |
| `mismatched-enum-comparison` | 234 | second `enum.const` nominal index `0` to distinct enum `1` | `typed-execution` | `6ae2e65a43f68f0aa4b46b7ca306ad1dd06b72b1328e02e611f98e9f7abc869e` |
| `duplicate-nominal-name` | 211 | enum name `Secon` to duplicate record name `First` | `semantic` | `60d12d56015678f3197a1413cfb058bff64188a8e2256d09f504280fad805f9c` |

Six further compact fixtures start from valid WVB values serialized by the
frozen Stage 0 oracle and change exactly one byte. Only the mutated base64 values
are permanent:

| Case | Bytes | Mutation | Phase | SHA-256 |
| --- | ---: | --- | --- | --- |
| `mismatched-merge` | 168 | branch target byte 116, `7` to `17` | `typed-execution` | `f3f98931b5a701c805e9889768abe2c8536fb4ff04fd6a614ddf7f0732f6b7a2` |
| `bytes-length-on-i32` | 173 | code byte 128, `bytes.const` to `i32.const` | `typed-execution` | `f06d084a5f78b8d12e8503cfacd841565527c7a075dbcad40626e48f6d9e48c0` |
| `record-create-wrong-field-type` | 185 | code byte 113, `u32.const` to `i32.const` | `typed-execution` | `a074c6a8229870bb45a3de8764a2ffd51b8091f0e4d50f48330c560927ca4c59` |
| `invalid-enum-member` | 202 | member byte 118, `1` to missing `2` | `semantic` | `ddd000954aeb8d0c02775128ae52615d9bf4237bda9741eb39e6f9efb4f2ddbe` |
| `enum-const-on-record` | 225 | nominal byte 114, enum `1` to record `0` | `semantic` | `3d09445c44bf2d1e3f5b811f254e0bccc902366ad242ea4cf101fc44f23b99d8` |
| `duplicate-enum-value` | 192 | second value byte 188, `1` to duplicate `0` | `semantic` | `da453ca0cbe661ab695e21ce8f2ee2530a303ad996bbedfe6f0ae5e9bbb0a00c` |

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

Success prints the sixteen ordered `PASS` lines followed by:

```text
Tests: 16, Passed: 16, Failed: 0
```

The fixture set is representative, not exhaustive. Nominal count and value-size
limits, remaining typed opcode families, hostile lengths, seeded random bytes,
and random source/assembly remain separate evidence.
