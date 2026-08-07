# Windvale native WVO differential tests

## Status and scope

This fixed contract transfers the Stage 0 WVO acceptance differential loops to
the digest-bound native WVO verifier. It freezes 128 single-byte mutations of
the canonical sample object and 128 arbitrary byte values together with the
reference codec's accepted/rejected decision. The permanent test compares the
public native verifier to that immutable oracle manifest without starting .NET.

The existing [read-only rejection matrix](Windvale-Native-Wvo-Read-Only-Rejection-Tests.md)
owns exact reports for all thirteen stable native status families through both
verifier and inspector launchers. This differential contract instead owns the
broader acceptance boundary, including mutations that remain valid. It does not
replace hostile-size, successful inspection-detail, linker, publication, or
final dual-host qualification evidence.

## Reference provenance and corpus

The reference snapshot was produced once from commit `c183e9a` with the exact
logic in `Wvˉlinkerˉcoreˉrecognizesˉobjectˉformat`:

1. encode the canonical two-section, three-symbol, one-relocation sample WVO;
2. initialize framework `Random` with seed `0x57_56_4F_31`;
3. for 128 cases, choose one of the 189 sample bytes and increment it, wrapping
   `255` to zero;
4. continue the same generator for 128 values whose lengths are selected from
   zero through 256 and whose contents come from `NextBytes`; and
5. admit every value through `Objectˉcodec.Readˉandˉverify`.

The canonical sample has SHA-256
`006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a`.
The mutation family contains 128 189-byte values across 91 distinct offsets:
32 remain valid and 96 reject. The arbitrary family contains 128 rejected
values, 104 distinct lengths from 3 through 256, and 15,680 bytes. The complete
corpus contains 39,872 input bytes, 32 accepted cases, and 224 rejected cases.

Files are named `Mutation-000.wvo` through `Mutation-127.wvo` and
`Random-000.wvo` through `Random-127.wvo`. `Manifest.txt` begins with
`windvale-wvo-differential-corpus 1`; every later LF-terminated line fixes:

```text
filename|family|case|detail|length|input-sha256|outcome|oracle-code|oracle-offset|accepted-report-sha256
```

`detail` is the mutated byte offset or `-1` for arbitrary inputs. Rejected rows
retain the exact Stage 0 `WVO` code and optional byte offset as provenance;
accepted rows retain the exact native success-report identity derived from the
input digest. The 32,682-byte manifest has SHA-256
`ef6a187dfc5d0bbffcfb61df40146af54f74d76302dee1358b4a3fbefd7aa556`.

The manifest and 256 files are stored in one 34,894-byte gzip tar archive at
SHA-256
`74d90d981ef3665eee2fb16a5abb57ae2e9d308a8e56b1aff56c49d97997d684`.
Its repository representation is 47,141 LF-only base64 bytes at SHA-256
`5936f76a0a915d096fc0428dd8470fe219421dc983c7fcf98b1454fca4c47ec1`.
The generator and its managed build products are not retained.

## Native comparison contract

`Tools/Native/Test-Wvo-Differential.cmd` and `.sh` verify the archive,
manifest, family/outcome counts, and every complete input identity before
calling only `Verify-Wvo`. Every input is rehashed afterward.

For an oracle-accepted row, the native verifier must return `0`, write no
diagnostic, and emit the exact report identity stored in the manifest. That
report is:

```text
Verified object: X86ˉ64
SHA-256: <input-sha256>
```

including the final LF. For an oracle-rejected row, the native verifier must
return `2`, write no standard output, and emit exactly one nonempty line through
the stable `object status=` boundary. Exact per-status text remains owned by the
thirteen-case rejection matrix rather than being duplicated 224 times here.

Success prints all 256 manifest-ordered `PASS` lines followed by:

```text
Tests: 256, Passed: 256, Failed: 0
```

The permanent command generates no input, invokes no managed runtime, mutates
no expected result, and does not rerun the inspector or linker.
