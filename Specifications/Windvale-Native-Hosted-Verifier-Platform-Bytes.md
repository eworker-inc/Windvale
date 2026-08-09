# Windvale native hosted-verifier platform bytes

## Status and scope

This contract transfers the platform-owned outer bytes for compiler-verifier
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
payload.

## Command and identities

```text
wvhostverifierbytes <runtime.wvhr> <regions.wvhb>
```

Success reports `Valid`, writes the exact response, and returns zero. Rejection
returns 2 without overwriting an existing output. An input/output alias reports
usage, returns 64, and preserves the runtime input.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 34,376 | `03ad87aa7cef5d440fbd1ac94569aa9f07f979b625f81acbfa5405d9bc8a1fce` |
| Windows application | 431,104 | `5288573a8eaedb5745f5b0aae733e2ba7dd89253bfb26d01beffe6279d3540c0` |
| Linux application | 430,080 | `b988c45fd1eada051e93243d506e33e0f15325c03ea83c1b9cdd2e994c322e07` |

The native front door builds the WVB and the shared hosted-container packager
reconstructs both applications. One focused current-host differential test
matches every emitted region against the frozen Windows/Linux application
oracle and covers invalid metadata plus alias preservation. C# remains
deletion-bound test evidence and does not build a production artifact.

Final source-set assembly, durable publication, independent Linux execution,
promotion, and the grouped retirement gate remain pending.
