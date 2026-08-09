# Windvale native hosted-verifier publisher Windows imports

## Status and scope

This contract constructs the exact publisher-only Windows x64 import page.
It owns the additional file and durability functions needed by the native WVB
publisher and does not alter the smaller ordinary hosted-verifier profile.

## Request and response

`WVIR 1` is a 16-byte little-endian request containing magic `WVIR`
(`0x52495657`), version 1, total bytes 16, and import address 253,952.

`WVIM 1` begins with a 32-byte little-endian header containing magic `WVIM`
(`0x4d495657`), version, total bytes, status, consumed input bytes, page offset,
page bytes, and admitted address. Success appends the 4,096-byte page at offset
32. Rejection returns only the header with status 1.

## Canonical construction

The page contains three descriptors for `KERNEL32.dll`, `ntdll.dll`, and
`SHELL32.dll`; separate lookup and IAT tables; and 17 hint/name entries. The
kernel table contains 15 functions in the frozen publisher order, followed by
`NtSetInformationFile` from ntdll and `CommandLineToArgvW` from shell32.
Unused bytes are zero.

The canonical page SHA-256 is
`ff9b9a84ea0d74386337ab605a4d1afc76bd426bff49d6dfd96845b06207bee5`.
The constructor WVB is 9,310 bytes with SHA-256
`8d233b54d0387e9a1348447f9095e683415075da31104f4b80c935b09c960831`.

## Evidence and remaining work

One focused test checks native/interpreter equality, service-free entry shape,
exact equality with the page embedded in the canonical Windows publisher, and
malformed request rejection. [Decision 0479](../Documents/Decisions/0479-Native-WVHV-Publisher-Linux-Materialization.md)
now performs the complete Linux ELF materialization. The Windows PE counterpart
must combine this response, Decision 0477 object bytes, Decision 0475 metadata,
and the admitted base application.
