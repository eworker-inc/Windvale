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
| Tool WVB | 40,063 | `b9900f7f3e49f7b99e135a77c3eec09cd3ef8d07a52633e9a70fca578925bb8e` |
| Windows application | 476,672 | `d2df4da421389d77d34e6a0eea981c7ec7f045a201e4ac567dc4be2a357337a5` |
| Linux application | 475,136 | `eac8d2def78033ccc84b87ebacbdc5b82b159031f6a308df8e4e9b37036919f0` |

The native front door builds the WVB and the shared hosted-container packager
reconstructs both applications. One focused current-host differential test
matches every emitted region against the frozen Windows/Linux application
oracle and covers invalid metadata plus alias preservation. C# remains
deletion-bound test evidence and does not build a production artifact.

Final source-set assembly, durable publication, independent Linux execution,
promotion, and the grouped retirement gate remain pending.
