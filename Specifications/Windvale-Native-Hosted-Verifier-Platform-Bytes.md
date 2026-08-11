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
| Tool WVB | 40,197 | `6b3dc11da9a864917304bf740c5edcad3ed87ec08dc5a837e60fc752b212a0ee` |
| Windows application | 477,696 | `5a0c759c44f2a4d7103c82e636feb635a2876c7207ac3dcd88d8d72159d5fc2f` |
| Linux application | 479,232 | `3f0a5fbd17caf75c01e3ff188b88327cceb6d85f6fd886aff188d6d2dc056f63` |

The native front door builds the WVB and the shared hosted-container packager
reconstructs both applications. One focused current-host differential test
matches every emitted region against the frozen Windows/Linux application
oracle and covers invalid metadata plus alias preservation. C# remains
deletion-bound test evidence and does not build a production artifact.

Final source-set assembly, durable publication, independent Linux execution,
promotion, and the grouped retirement gate remain pending.
