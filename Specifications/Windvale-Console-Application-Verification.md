# Windvale console-application verification

## Status and purpose

The console-application admission boundary is portable Windvale-owned logic for completed `windows-x64-console-v1`, `linux-x64-console-v1`, `windows-x64-console-v2`, and `linux-x64-console-v2` files. It accepts untrusted application bytes, proves every container-owned byte against the applicable canonical contract, recovers the one opaque native image and its entry offset, and emits fixed evidence.

The verifier does not redefine PE, ELF, native ABI, startup, context, or process-result semantics. Those inputs remain defined by the target specifications, the [console-application construction contract](Windvale-Console-Application-Construction.md), and the [hosted console application contract](Windvale-Hosted-Console-Application.md). The retained C# PE and ELF verifiers remain independent structural recovery oracles during Stage 0.

`Console-Application-Admission-Core.wv` dispatches exact format-2 PE/ELF
markers to focused hosted common/platform verifiers and otherwise invokes the
unchanged version-1 recipe verifier. Both routes produce the same `WVCV 1`
result. Current format-2 admission requires one first-chunk value and an empty
second chunk; segmented format-2 maximum-size evidence remains separate.

## Segmented input

One completed application can be larger than Windvale's 4 MiB per-value byte limit. The hosted bridge therefore reads two named byte values and treats them as one logical file:

- `console-application-first.bin`: zero through 4,194,304 bytes.
- `console-application-second.bin`: zero through 8,304 bytes.
- A nonempty second chunk is canonical only when the first chunk is exactly 4,194,304 bytes.
- Logical offsets below the first length address the first chunk; later offsets address the second chunk after subtracting the first length.

This covers the exact maximum Windows application of 4,196,352 bytes and maximum Linux application of 4,202,608 bytes without increasing the byte-value limit. Noncanonical chunking is rejected before container inspection.

## Target and native derivation

The verifier recognizes only `MZ` or `0x7FELF` at logical offset zero. It derives the candidate native length and entry from bounded early fields that are present in the first chunk:

| Target | Native-length evidence | Entry evidence |
| --- | --- | --- |
| Windows | `.text` virtual size at offset 400 minus the fixed 112-byte native-image offset | Startup call displacement at offset 566 minus 54 |
| Linux | Text `PT_LOAD` file size at offset 152 minus the fixed 160-byte native-image offset | Startup call displacement at offset 4,208 minus 44 |

The derived native length must be `1…4,194,304`, and the entry must be within it. These fields are derivation inputs, not trusted validation shortcuts. The verifier builds the accepted 32-byte plan request, invokes the portable constructor, and requires the candidate to match the resulting exact sparse recipe.

## Recipe comparison and recovery

The recipe envelope and every descriptor are revalidated before comparison. Literal spans must agree byte for byte. Every gap before, between, and after segments must be zero. There must be exactly one native-copy segment of the derived length, and the candidate length must equal the recipe's complete application length.

The native span is deliberately excluded from literal comparison and is copied into one bounded result value. If it crosses the chunk boundary, the verifier concatenates only its tail and head portions; the recovered value never exceeds 4 MiB. All PE/ELF headers, program or section records, startup bytes and displacements, execution-context bytes, notes, relocation metadata, and alignment padding are therefore verified, while native contents remain opaque.

## WVCV 1 evidence

The bridge returns exactly 36 bytes. All fields are unsigned 32-bit little-endian values.

- Magic: bytes `WVCV`, integer `0x56435657`.
- Format version: `1`.

| Offset | Field |
| ---: | --- |
| 0 | Magic |
| 4 | Version |
| 8 | Evidence bytes, always 36 |
| 12 | Status |
| 16 | Failure offset, or complete application length on success |
| 20 | Target, when established |
| 24 | Logical application bytes |
| 28 | Recovered native-image bytes, zero on failure |
| 32 | Native-entry offset, zero on failure |

Status values are:

| Value | Meaning |
| ---: | --- |
| 0 | Valid |
| 1 | Invalid chunk shape |
| 2 | Invalid application size |
| 3 | Invalid target identity |
| 4 | Invalid native-image derivation |
| 5 | Invalid native entry |
| 6 | Invalid internal construction recipe |
| 7 | Candidate differs from the canonical container |

On success, the hosted bridge performs exactly one `file.write_bytes` to `console-application-native-image.bin` and returns valid evidence. On failure, it performs no write and returns evidence with zero native length and entry. Stage 0 independently validates the complete evidence envelope, target and length relationships, success write name/count/length, and the absence of output on rejection.

## Stage 0 integration and limits

`Linker/Windvale/Console-Application-Verification-Core.wv` and the focused
format-2 verifier modules are portable and capability-free. Their hosted bridge
declares only `file.read_bytes` and `file.write_bytes`, uses the fixed resource
names above, and is bounded to ten million instructions. The current retained
bridge WVB is exactly 103,393 bytes with SHA-256
`460402786c3f2db63f7e13e56124197f6575f93eb8fa0f1b535275ad2c6512fa`.

Both Stage 0 console writers and the [native console packager](Windvale-Native-Console-Packager.md) require the Windvale verifier to reproduce the original target, native bytes, and entry. The native packager performs this check before its one output call. The independent detailed C# PE or ELF verifier then performs the same recovery through separately maintained structural logic during transition evidence. Any rejection or disagreement prevents successful application output. The [native console-application publisher](Windvale-Native-Console-Application-Publisher.md) reuses this verifier as its mutation-admission boundary before the shared atomic transaction begins.

The existing PE and ELF malformed-input corpora are shared by both verifier
families. Verifier-specific tests cover retained source/WVB identity,
deterministic evidence, malformed evidence, no-write rejection, noncanonical
chunks, and exact maximum-size recovery. The separate
[hosted-console mutation contract](Windvale-Native-Hosted-Console-Container-Mutation-Tests.md)
adds two valid format-2 applications and thirteen exact mutation outcomes
without a permanent live managed oracle.
