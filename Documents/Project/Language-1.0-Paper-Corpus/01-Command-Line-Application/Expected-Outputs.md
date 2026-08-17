# Workload 1 expected semantic outputs

## Rule

These observations are backend-independent. Interpreter, JIT, cached execution,
AOT, WebAssembly, Windows, Linux, and Windvale providers must agree on argument
meaning, UTF-8 validation, counts, bytes, capability calls, mutation outcomes,
and returned process status. Host console presentation is not part of the oracle.

## Successful cases

### Help

Arguments:

```text
--help
```

Input calls: zero. Diagnostic calls: zero. Normal output is exactly 73 bytes:

```text
Usage: windvale-inspect --operation bytes|runes [--maximum-bytes NUMBER]
```

SHA-256 is
`3834b674a9f9df457e7e678f3682d9b5fc8fbc02bce71f87e3866b6d8773cc05`.
Complete acceptance returns status 0.

### Byte count with default maximum

Arguments:

```text
--operation bytes
```

Input is nine bytes:

```text
57 69 6E 64 76 61 6C 65 0A
```

Its SHA-256 is
`8d97ccece01814ecc684fcbf1cbaeadea3c79f144085711378c4c8fe9983c6de`.
Normal output is exactly:

```text
bytes=9
```

The eight output bytes have SHA-256
`a49e3a984b84c5065be800c54ed520c1672fcb7b4a9c5d39aede782dbd8f191f`.
Complete acceptance returns status 0.

### Rune count with explicit maximum

Arguments:

```text
--operation runes --maximum-bytes 6
```

Input is `AˉΩ` followed by LF:

```text
41 CB 89 CE A9 0A
```

The input contains six UTF-8 bytes and four Unicode scalar values. Its SHA-256
is `a66803708b3b24960630084c16a4e7c2a5389810d33302d423cd4f79decce322`.
Normal output is exactly:

```text
runes=4
```

The eight output bytes have SHA-256
`4f84288bf37282da411d02e5fbaa63a873cb917c54e28a27c0388a9725e74e79`.
Complete acceptance returns status 0.

### Empty input

Arguments are `--operation bytes --maximum-bytes 0`. Input is empty and EOF is
immediate. Output is `bytes=0` followed by LF and complete acceptance returns
status 0.

### Exact absolute maximum

Arguments are `--operation bytes --maximum-bytes 65536`. Input is exactly 65,536
ASCII `A` bytes followed by EOF. Output is exactly:

```text
bytes=65536
```

The 12 output bytes have SHA-256
`e53d281994b13edd192dfaa056833a42be9366d6585e273a84bf2fc378192ac8`.
Complete acceptance returns status 0.

## Argument failure examples

With no arguments, input and normal output are not called. Diagnostic output is
exactly:

```text
error: no arguments
Usage: windvale-inspect --operation bytes|runes [--maximum-bytes NUMBER]
```

It is 93 bytes with SHA-256
`028c40a46e1b0ff6e34c39bbde46e265f7f43bb6d5633ad9a1b5697729841e2f`.
Complete diagnostic acceptance returns status 2.

For `--unknown`, the diagnostic begins `error: unknown option` followed by LF
and the exact usage object. It is 95 bytes with SHA-256
`c79283b96a78de793fdff075bd4109ede351f20a6f98be11526ca649a7d42579`.
Complete diagnostic acceptance returns status 2.

## Invalid UTF-8 example

Arguments are `--operation runes --maximum-bytes 2`. Provider input bytes are
`C3 28`. Normal output is not called. Diagnostic output is exactly:

```text
error: input is not valid UTF-8
Usage: windvale-inspect --operation bytes|runes [--maximum-bytes NUMBER]
```

It is 105 bytes with SHA-256
`61a82c32ce78ba468477e04c5f2813bd5de77fb9308a66049c8014c057406f2b`.
Complete diagnostic acceptance returns status 3.

## Mutation outcome oracle

For a requested terminal status `S` and value length `L`:

| Provider outcome | Returned status | Additional write |
| --- | ---: | ---: |
| `Rejected(Error)` | 4 | 0 |
| `Acceptedˉpartial(Completed: 0..L-1, Error)` | 4 | 0 |
| `Completed(Completed: L)` | `S` | 0 |
| `Completed(Completed: value other than L)` | 4 | 0 |
| `Indeterminate(Error)` | 5 | 0 |

The oracle applies independently to normal and diagnostic output. A complete
diagnostic preserves its primary status 2, 3, or 6. A non-complete diagnostic
returns 4 or 5 as shown.

## Determinism record

Option order can change parsing traversal but not the final configuration.
`--operation bytes --maximum-bytes 9` and
`--maximum-bytes 9 --operation bytes` therefore produce identical capability
calls, output bytes, and status for the same input. Provider identity and host
scheduling cannot change UTF-8 byte length, rune count, decimal spelling, LF,
or output retry policy.
