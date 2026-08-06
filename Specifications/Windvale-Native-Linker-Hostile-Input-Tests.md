# Windvale native linker hostile-input tests

## Status and scope

This fixed contract transfers the Stage 0 linker's deterministic raw-byte
containment family to the digest-bound native Windvale linker. It requires 200
bounded arbitrary byte values to fail through the public `WVL1002` boundary
without a live C# oracle, output publication, or input mutation.

The corpus supplements the curated diagnostic-family and map-limit tests. It
does not replace valid-shaped one-byte mutation, randomized concurrency,
large-native admission, internal reconstruction, or successful-link evidence.

## Corpus construction and identity

Corpus values are derived from portable xorshift32 state, not a host framework's
`Random` implementation. The initial unsigned state is `0x0057564C`, spelling
`WVL` in ASCII. One `Next` operation is:

```text
state = u32(state xor u32(state << 13))
state = u32(state xor (state logical-right-shift 17))
state = u32(state xor u32(state << 5))
return state
```

For each case from 0 through 199, one `Next` value is consumed for the length.
Case 0 has length 0, case 1 has length 511, and every later length is that value
modulo 512. Each byte consumes another `Next` and retains its low eight bits. If
a value of at least four bytes accidentally begins with ASCII `WVO1`, byte zero
is XORed with `0xFF` so this raw containment corpus cannot become a valid object
by chance.

Files are named `Case-000.wvo` through `Case-199.wvo`. The corpus has 164
distinct lengths and 48,877 total input bytes. `Manifest.txt` starts with
`windvale-linker-hostile-corpus 1` and then fixes each filename, decimal length,
and lowercase SHA-256 as pipe-separated fields. Its exact identity is 16,378 LF
bytes at SHA-256
`b3ab716d55e8c2693dbf0610b8638b23780867082bec7e768635a16e8e1fbfef`.

The manifest and 200 files are stored in one 63,224-byte gzip tar archive at
SHA-256
`3648bc4a00bb822096ad669d0f24828f034df5b69023f1bdb2c3b3ab2a034160`.
The repository representation is 85,410 LF-only base64 bytes at SHA-256
`dc5503bbde043e00920d67388cd9bb208b7e9b484081053dcce026e05f7c259c`.
This compact archive avoids 200 committed source fragments while retaining
complete per-input provenance.

## Rejection contract

`Tools/Native/Test-Linker-Hostile-Inputs.cmd` and `.sh` must verify the archive,
manifest, case count, and every input's length and digest before invoking the
linker. Every case calls the digest-bound native linker with base address
`1048576`, entry `Main`, one hostile input, and a pre-existing destination equal
to the 479-byte bad-magic WVO sentinel at SHA-256
`0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288`.

For every input the linker must:

- return process exit `2`;
- write no standard output;
- emit the exact `WVL1002` report at SHA-256
  `18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353`;
- preserve the complete destination sentinel; and
- preserve the input's complete manifest-owned identity.

Windows `certutil` may reject hashing a valid zero-byte file on some volumes.
The Windows coordinator handles only that exact boundary by requiring length
zero and the canonical empty SHA-256
`e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`;
every nonempty identity still passes through `certutil`. Linux uses
`sha256sum` for every value, including the empty case.

Success prints the 200 ordered `PASS  Case-NNN.wvo` lines followed by:

```text
Tests: 200, Passed: 200, Failed: 0
```

The command generates no test input at runtime, starts no .NET process, and
does not consult or change an expected result after linker execution.
