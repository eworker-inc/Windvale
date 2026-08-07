# Windvale native console-container hostile-input tests

## Status and scope

This fixed contract transfers the two Stage 0 console-application verifier
random-byte containment loops to the digest-bound Windvale-native console
publisher. It requires 128 bounded arbitrary PE candidates and 128 bounded
arbitrary ELF candidates to fail through the shared portable verifier without
a live C# oracle, destination publication, input mutation, or leftover scratch.

The corpus supplements the curated construction and diagnostic cases. It does
not replace valid-shaped one-byte mutations, maximum admitted application
construction, successful execution, publication fault/concurrency evidence, or
the final dual-host qualification gate.

## Corpus construction and identity

Corpus values are derived from portable xorshift32 state, not a host
framework's `Random` implementation. One `Next` operation is:

```text
state = u32(state xor u32(state << 13))
state = u32(state xor (state logical-right-shift 17))
state = u32(state xor u32(state << 5))
return state
```

The Windows family starts at unsigned state `0x00575657`, spelling `WVW`, and
the Linux family starts at `0x0057564C`, spelling `WVL`. Each family contains
128 cases. One `Next` value is consumed for every length: case 0 is fixed at
zero bytes, case 1 is fixed at the family maximum, and later lengths are the
value modulo maximum-plus-one. Each input byte consumes another `Next` value
and retains its low eight bits.

Windows files are named `Windows-000.exe` through `Windows-127.exe`, have
126 distinct lengths from zero through 4,096, and total 249,420 bytes. Linux
files are named `Linux-000.elf` through `Linux-127.elf`, have 128 distinct
lengths from zero through 9,000, and total 552,826 bytes. The complete corpus
therefore fixes 256 files and 802,246 input bytes.

`Manifest.txt` starts with
`windvale-console-container-hostile-corpus 1`; each later LF-terminated line
fixes filename, console target, decimal size, and lowercase SHA-256 separated
by `|`. The manifest is 27,372 bytes at SHA-256
`94f2fb533dabaa57a54c331458ac0f0b478476e2923263840eff85dbd19dd8db`.

The manifest and 256 files are stored in one 826,091-byte gzip tar archive at
SHA-256
`2aa0a153aaf1c70fe650f99e302ebd2aaa9908228175e0f0bebdd9894a872112`.
The repository representation is 1,115,949 LF-only base64 bytes at SHA-256
`ab935b8071ae59f389d5da0212811d769daa702e5ea84408cb0759a4ad4c960d`.
This single archive avoids 256 committed fixture fragments while retaining
complete per-input provenance. No test input is generated at runtime.

## Rejection contract

`Tools/Native/Test-Console-Container-Hostile-Inputs.cmd` and `.sh` verify the
archive, manifest, family counts, and every input's suffix, length, and digest
before invoking `Publish-Console`. The `.exe` or `.elf` suffix selects the
portable PE or ELF verification route; the current-host publisher can therefore
exercise both container families without executing either candidate.

Before every case, the command copies the fixed 479-byte WVO sentinel at
SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`
to the same-suffix destination. Every publisher invocation must:

- return process exit `1`;
- write no standard output;
- emit exactly `publication status=Rejected phase=console-application` plus LF,
  whose SHA-256 is
  `39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f`;
- preserve the complete input and destination sentinel; and
- leave no `.wvpublish-*` scratch file.

Windows `certutil` may reject hashing a valid zero-byte file on some volumes.
The Windows coordinator handles only the manifest-owned zero-length cases
paired with the canonical empty SHA-256
`e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`;
all nonempty values still pass through `certutil`. Linux uses `sha256sum` for
every value.

Success prints the 256 manifest-ordered `PASS` lines followed by:

```text
Tests: 256, Passed: 256, Failed: 0
```

The command starts no .NET process, rebuilds no candidate, and does not consult
or change an expected value after native execution.
