# Windvale native hosted-verifier publisher Windows imports

## Status and scope

This contract constructs the exact publisher-only Windows x64 import page.
It owns the additional file and durability functions needed by the native WVB
publisher and does not alter the smaller ordinary hosted-verifier profile.

## Request and response

`WVIR 1` is a 16-byte little-endian request containing magic `WVIR`
(`0x52495657`), version 1, total bytes 16, and the role-specific import address:
253,952 for the publisher, 679,936 for the promoter, or 1,310,720 for the WVB
publisher.

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

The publisher page SHA-256 is
`ff9b9a84ea0d74386337ab605a4d1afc76bd426bff49d6dfd96845b06207bee5`;
the relocated promoter page SHA-256 is
`e1cc3ab2c1f3cc8ed4c83e2cce4ef8dcb9f520af47d75079911799ba8b52ae82`.
The role-2 page SHA-256 is
`56554387c9db58d57f7f3e95b3a655e5f6f17b581ba7fb4b6d7d31de1696f595`.
The constructor WVB is 9,643 bytes with SHA-256
`12651ff9d806b39b575fb6f6153c6d8a1d16256506be8d98f3f339ccbcd5fbfe`.

## Evidence and remaining work

One focused test checks native/interpreter equality, service-free entry shape,
exact equality with the page embedded in the canonical Windows publisher, and
malformed request rejection. [Decision 0479](../Documents/Decisions/0479-Native-WVHV-Publisher-Linux-Materialization.md)
performs Linux ELF materialization. The role-aware Windows materializer now
combines this response, instantiated objects, metadata, and the admitted base
application for all three roles.
