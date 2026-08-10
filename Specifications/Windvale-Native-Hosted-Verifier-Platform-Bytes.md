# Windvale native hosted-verifier platform bytes

## Status and scope

This contract transfers the platform-owned outer bytes for fixed hosted-verifier
format 4 into portable Windvale. It consumes the same admitted 4,096-byte
runtime header as the verifier startup process and emits only the regions that
are not startup, service-bundle, or runtime payload bytes.

Windows output contains the 512-byte PE/COFF header, the exact 4,096-byte import
page, and the 12-byte base-relocation block. Linux output contains the exact
4,096-byte ELF and program-header page. Final file placement and durable
publication remain a later boundary.

## Target contracts

The Windows producer derives text, data, runtime, and relocation placement from
the admitted format-4 layout. It writes PE32+ machine `0x8664`, format version
4, three sections, the fixed image base, bounded stack/heap fields, import and
IAT directories, and one relocation block. Its import page contains exactly
eleven Kernel32 entries plus `CommandLineToArgvW`; it deliberately excludes the
compiler-family `FlushFileBuffers` import.

The Linux producer writes one ET_DYN x86-64 image with five program headers:
read-only header, read/execute text, read/write runtime data, the Windvale
format-version note, and non-executable stack policy. All file and virtual
placements come from the admitted format-4 layout.

Both responses use the existing version-1 platform-region envelope. Invalid
runtime metadata or target evidence returns a failure response and no platform
payload. Profile 7 increases only the admitted runtime virtual extent: its
second name and data strides move the input scratch and image end by exactly
5 MiB relative to the one-snapshot profiles. Windows therefore moves the
relocation RVA and image end by 5 MiB, while Linux increases the runtime
segment memory extent by 5 MiB. Neither target changes its platform-response
size or application file extent solely because of the second snapshot.
The exact profile-7 runtime virtual extents are 148,975,616 bytes for Windows
and 147,927,040 bytes for Linux.

## Command and identities

```text
wvhostverifierbytes <runtime.wvhr> <regions.wvhb>
wvhostverifierbytes wvo-inspector <runtime.wvhr> <regions.wvhb>
wvhostverifierbytes console-verifier <runtime.wvhr> <regions.wvhb>
```

Success reports `Valid`, writes the exact response, and returns zero. Rejection
returns 2 without overwriting an existing output. An input/output alias reports
usage, returns 64, and preserves the runtime input.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 38,484 | `85b9ef76f109aacb1ed88b2724180c501bf87f8de7dd9d8309d0e941a89215b1` |
| Windows application | 465,408 | `4223a30bf7000d55c46aff94ef0352958305680215560cdc86e0243c19363919` |
| Linux application | 466,944 | `4f77aeefce7b121777aaac1e91a00f79228c91c01ce0c4ba1acaf28dfe2aa7c3` |

The native front door builds the WVB and the shared hosted-container packager
reconstructs both applications. One focused current-host differential test
matches every emitted region against the frozen Windows/Linux application
oracle and covers invalid metadata plus alias preservation. C# remains
deletion-bound test evidence and does not build a production artifact.

Final source-set assembly, durable publication, independent Linux execution,
promotion, and the grouped retirement gate remain pending.
