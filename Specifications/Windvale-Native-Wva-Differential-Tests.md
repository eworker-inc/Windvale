# Windvale native WVA differential tests

## Status and scope

This fixed contract transfers the 200-case deterministic mutation loop from the
Stage 0 WVA differential test to the ordinary digest-bound native assembler. It
freezes every mutated source, the reference assembler's accepted/rejected
decision, the reference diagnostic code for rejection, and the exact reference
WVO identity for acceptance. The permanent test consumes those values without
starting .NET or regenerating input.

The separate [native WVA rejection matrix](Windvale-Native-Wva-Assembler-Rejection-Tests.md)
owns one complete report for every stable `WVA1001` through `WVA1011` family.
This differential contract instead owns the exact seeded mutation sequence and
its acceptance, diagnostic-family, and successful-byte agreement. It does not
replace the remaining representative valid-source vectors, arbitrary-source
containment, Linux execution, or final grouped qualification.

## Reference provenance and corpus

The reference snapshot was produced once from commit `d933dec` with .NET SDK
10.0.302. The reviewed generator read the compiled private
`COMPLETE_ASSEMBLY_SOURCE` constant directly from `Windvale.Seed.Tests`, then
reproduced the exact managed loop:

1. initialize framework `Random` with seed `0x57_56_41`;
2. copy the canonical 432-character ASCII WVA source for each case;
3. select one through four assignments with `Next(1, 5)`;
4. select each position from the current source length and each replacement
   from `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._$- #`,
   tab, carriage return, or line feed; and
5. assemble with `Assemblyˉcompiler.Assemble` and structurally verify every
   accepted WVO with the Stage 0 object codec.

The canonical source has SHA-256
`4d6e48a10ecc501552d6e339b93eea912a71d06729c5d546b413d256f92ba05e`.
The corpus contains 200 distinct 432-byte sources totaling 86,400 bytes. Its
assignment-count distribution is 58 one-assignment, 45 two-assignment, 50
three-assignment, and 47 four-assignment cases.

Only `Case-003.wva` is accepted. Its sole assignment writes space to an existing
space at offset 345, so its source remains canonical. It produces the exact
243-byte WVO with SHA-256
`fd9db82653a0de0af8950340e7b43ac215c3cd0d8f3c416268ebcb92c88b9ab3`,
three sections, three symbols, and two relocations. The other 199 sources reject
with this Stage 0 diagnostic-code distribution:

| Code | Cases |
| --- | ---: |
| `WVA1001` | 21 |
| `WVA1002` | 111 |
| `WVA1003` | 40 |
| `WVA1004` | 2 |
| `WVA1005` | 12 |
| `WVA1007` | 5 |
| `WVA1009` | 8 |

`Manifest.txt` begins with `windvale-wva-differential-corpus 1`, followed by a
column header and 200 LF-terminated rows with this grammar:

```text
filename|case|assignment-count|operations|source-bytes|source-sha256|outcome|oracle-code|oracle-line|oracle-column|object-bytes|object-sha256|sections|symbols|relocations
```

Each operation is `<character-offset>:<replacement-code-point>`, joined by
commas in execution order. The 27,485-byte manifest has SHA-256
`50153c0f7a6e9b596f3a7e0c4ce5bc1c6f240b01ce8657d99c5775a61d9391e4`.
The manifest and 200 sources are stored in one 17,301-byte gzip tar archive at
SHA-256
`b9a076cf9416488d733ed4c4887c052e61548acb45574256cd3c65d94da31970`.
Its repository representation is 23,372 LF-only base64 bytes at SHA-256
`b40567d2d0208f2f4ec3a5a93050e703efd1d8e0b7a5f708c3db96577cb10dcb`.
The generator and its managed build products are not retained.

## Native comparison contract

`Tools/Native/Test-Wva-Differential.cmd` and `.sh` verify the archive,
manifest, assignment distribution, family totals, and every complete source
identity. Each case starts with the fixed return-42 WVO as its destination
sentinel and invokes only the ordinary `Assemble-Wva` launcher. Every source is
rehashed afterward.

For an oracle-rejected row, the native assembler must return `2`, write no
standard output, emit exactly one nonempty diagnostic line beginning with the
frozen Stage 0 `assembly status=<oracle-code> ` prefix, and preserve the complete
destination sentinel.

For `Case-003.wva`, the native assembler must return `0`, write no diagnostic,
replace the destination with the exact 243-byte reference WVO, and emit exactly:

```text
wvasm 1
assembly status=valid object-bytes=243 sections=3 symbols=3 relocations=2 offset=432 line=27 column=12
```

including the final LF. That 111-byte report has SHA-256
`4713cc6a74e88cab45421a8bed22b4c72de19fb330f77212a8193aa0e1224c73`.
The output then passes the existing native WVO verifier with its exact
digest-bearing report.

Success prints all 200 manifest-ordered `PASS` lines followed by:

```text
Tests: 200, Passed: 200, Failed: 0
```

The permanent command generates no source or expected result, starts no managed
runtime, and does not rerun the broader managed assembler test.
