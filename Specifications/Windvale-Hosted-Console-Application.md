# Windvale hosted console application contract

## Status and scope

Hosted console application format 2 is the first standalone native container that carries an explicit Windvale capability and runtime-service requirement. The paired targets are `windows-x64-console-v2` and `linux-x64-console-v2`. Both accept exactly one verified ABI-22 scalar application requiring exactly `console.write_line`; they reject every other service set.

Version 2 does not change Windvale source, WVB, WVO, native ABI 22, execution-context version 7, service-table version 5, or the existing version-1 container bytes. Its Stage 0 adapter packages the shared verified native image with a WVA startup, the existing exact native console-output leaf, initialized runtime tables, and serialized metadata. The resulting PE or ELF runs without loading .NET. Portable Windvale now owns ordinary format-2 admission and native-image recovery; Stage 0 remains the constructor and independent recovery verifier.

## Fixed runtime data layout

The writable segment begins with a 1,024-byte file-backed header. Remaining bytes are zero-filled by the platform loader.

| Offset | Bytes | Contract |
| ---: | ---: | --- |
| 0 | 112 | ABI-22 execution context, format 7 |
| 112 | 104 | Native service table, format 5 |
| 216 | 48 | Native output table, format 1 |
| 264 | 8 | Zero alignment |
| 272 | 192 | `WVHC 1` hosted metadata |
| 464 | platform-defined | Windows import records or zero Linux padding |
| 1,024 | 2,097,152 | Record arena |
| 2,098,176 | 134,217,728 | Dynamic text/byte arena |

The complete virtual data extent is 136,315,904 bytes. The context carries default instruction and call-depth budgets, the 2 MiB record length, and the 128 MiB text length. Startup installs context, service-table, arena, and output-table pointers. The service table contains only the `console.write_line` pointer at byte 8. All other service slots remain zero.

The output table has magic `WVIO`, version 1, size 48, the target platform identity, and `CONSOLE_PRESENT`. Linux initializes the console target to file descriptor 1 and uses a zero write-function pointer. Windows startup obtains standard output through `GetStdHandle(-11)` and installs both that handle and the imported `WriteFile` address before entering the application.

## `WVHC 1` metadata

All integers are unsigned 32-bit little-endian unless a digest is named. Unknown versions, counts, identities, flags, nonzero reserved fields, mismatched extents, or mismatched digests are invalid.

| Offset | Field | Required value |
| ---: | --- | --- |
| 0 | Magic | `WVHC` (`0x43485657`) |
| 4 | Metadata version | 1 |
| 8 | Metadata bytes | 192 |
| 12 | Target | 1 Windows, 2 Linux |
| 16 | Native ABI | 22 |
| 20 | Execution context | 7 |
| 24 | Service table | 5 |
| 28 | Application format | 2 |
| 32 | Service count | 1 |
| 36 | Capability count | 1 |
| 40 | Service-record offset | 96 |
| 44 | Service-record bytes | 32 |
| 48 | Native-image text offset | 496 Windows, 448 Linux |
| 52 | Native-image bytes | Exact bounded payload length |
| 56 | Native entry offset | Inside the native image |
| 60 | Output-leaf text offset | 224 |
| 64 | Output-leaf bytes | 258 Windows, 213 Linux |
| 68–84 | Header and arena layout | Exact values above |
| 88 | Flags | 1: hosted console present |
| 92 | Reserved | 0 |

The single 32-byte service record at offset 96 contains service identity 1 (`console.write_line`), capability identity 1 (`console.write_line`), service-table pointer offset 8, adapter identity 1 (`WriteFile`) or 2 (`write`), output-table offset 216, output-target flags 1, and two zero reserved words. Bytes 128–159 contain SHA-256 of the exact platform output leaf. Bytes 160–191 contain SHA-256 of the exact native image.

The numeric capability identity is container metadata, not a replacement for the canonical source/WVB capability name. The native fragment verifier must already have proven the exact ordered `Nativeˉservice.Consoleˉwriteˉline` requirement before construction.

## Executable text

Both targets enter at virtual address `0x1000`.

| Target | WVA startup | Output leaf | Native image |
| --- | ---: | ---: | ---: |
| Windows | offset 0, 224 bytes | offset 224, 258 bytes | offset 496 |
| Linux | offset 0, 217 bytes | offset 224, 213 bytes | offset 448 |

The startup sources are `Linker/Startup/Windows-X64-Hosted-Console.wva` and `Linker/Startup/Linux-X64-Hosted-Console.wva`. Their typed relative relocations must reproduce every final startup byte. The output leaves are supplied by `X64ˉnativeˉoutputˉservices` and retain their canonical sizes and SHA-256 identities.

Successful `i32` values 0 through 255 become the same process result. Any other successful scalar or packed native failure becomes result 1. Console output is strict UTF-8 followed by one line-feed byte.

## Platform containers

Windows format 2 is a deterministic PE32+ console executable with `.text`, `.data`, and `.reloc`. It imports exactly `GetStdHandle` and `WriteFile` from `KERNEL32.dll`; every import descriptor, lookup entry, address-table entry, hint/name record, directory, and zero gap is canonical. The maximum file size remains bounded by a 4 MiB native image and is 4,196,864 bytes.

Linux format 2 is a deterministic sectionless static-PIE ELF. It has header, RX text, RW data, Windvale note, and non-executable stack program headers. It uses direct `mmap`, `write`, and `exit` system calls and has no interpreter, dynamic loader, libc, or other imports. The maximum file size is 4,203,520 bytes.

## Verification and publication

Construction fails before publication unless the native fragment verifier, WVO/flat-link reproduction, platform container verifier, `WVHC 1` verifier, exact startup reconstruction, output-leaf digest, native-image digest, and recovered entry/service comparison all agree. Version 2 remains a Stage 0 container-building and independent recovery-verification path. Ordinary admission is now implemented by focused portable Windvale common, Windows, and Linux verifier modules behind the shared console-application dispatcher; it does not change the version-1 layout, construction, or recipe verifier modules.

Verifiers check outer bounds before fixed reads and reject truncated, oversized, inconsistent, trailing, noncanonical, or digest-mismatched files. The CLI publishes the complete verified executable atomically. Linux publication sets mode `0755` on Linux.

The fixed [native hosted-console mutation contract](Windvale-Native-Hosted-Console-Container-Mutation-Tests.md)
preserves both canonical valid applications and the exact thirteen managed
valid-shaped mutations behind the public native publisher. Segmented admission
for the theoretical maximum remains a separate boundary.

## Deliberate limits

Format 2 supports only `console.write_line`. It does not yet provide diagnostics, arguments, file input/output, environment access, clocks, threads, persistent heap, embedded WVB, load-time WVB verification, signing, unwind/debug metadata, or a general native import/FFI model. The existing 4 MiB WVO/link/container limit still excludes the current exact native compiler image; raising that bound and packaging the compiler are separate measured gates.
