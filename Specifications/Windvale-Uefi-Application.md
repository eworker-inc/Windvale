# Windvale UEFI application format

## Status and purpose

Windvale UEFI application format version 3 defines the deterministic adapter from a verified Windvale flat link to a PE32+ x86-64 EFI application. Version 1 accepted one relocation-free code object under [Decision 0045](../Documents/Decisions/0045-First-Uefi-Application-And-Boot-Probe.md). [Decision 0048](../Documents/Decisions/0048-First-Kernel-Handoff-And-Relative-Uefi-Link.md) extended version 2 for separately linked code joined by resolved relative calls. Version 3 admits immutable read-only WVO data so the shared native backend can carry ordinary portable WVB into the boot image.

The format follows the [UEFI 2.11 image requirements](https://uefi.org/specs/UEFI/2.11/02_Overview.html) and the [Microsoft PE/COFF layout and base-relocation definitions](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format). Windvale verifies the exact subset below rather than accepting arbitrary PE files.

## Input boundary

The adapter consumes one successful `Linkˉresult` already validated and independently reconstructed by `flat-x86-64-v1`. Version 3 requires:

- link base address zero;
- one or more input sections, all of which are code or read-only data, with at least one code section;
- every import resolved by the successful link;
- no `absolute-u32` relocation and no applied relocation kind other than `relative-i32`;
- an entry offset strictly inside the code bytes; and
- at most 4,194,304 linked image bytes.

The accepted bytes must be address-independent. A resolved `relative-i32` value remains valid when firmware relocates the complete PE image because its source and target move together. Version 3 still cannot discover opaque absolute addresses embedded manually in machine code. Native code at this system boundary is unsafe input supplied by verified bootstrap producers, not portable Windvale code.

## Canonical file layout

All integers are little-endian. Unlisted and padding bytes are zero. The total application is limited to 4,195,328 bytes.

| Region | File offset | Contract |
| --- | ---: | --- |
| DOS header/stub | `0x000` | `MZ`, `e_lfanew = 0x80`, otherwise zero |
| PE signature | `0x080` | `PE\0\0` |
| COFF header | `0x084` | x86-64 `0x8664`, two sections, timestamp zero, optional header `0xF0`, characteristics `0x0022` |
| PE32+ optional header | `0x098` | magic `0x20B`, writer 3.0, image base `0x400000`, section alignment `0x1000`, file alignment `0x200` |
| Section table | `0x188` | exactly `.text` followed by `.reloc` |
| Header padding | through `0x1FF` | zero |
| `.text` data | `0x200` | exact linked image followed by zero file-alignment padding |
| `.reloc` data | after `.text` raw data | 12-byte block followed by zero padding to `0x200` bytes |

The optional header uses:

- entry RVA `0x1000 + link entry offset`;
- EFI application subsystem 10;
- dynamic-base and NX-compatible flags `0x0140`;
- 1 MiB stack and heap reserves with 4 KiB commits;
- 16 data directories, all zero except base relocation;
- zero checksum, loader flags, OS/image/subsystem versions, and ambient metadata; and
- `SizeOfImage` aligned through the `.reloc` section.

`.text` begins at RVA `0x1000`, contains the exact final flat linked image including deterministic inter-section alignment and immutable read-only data, is readable and executable, and is not writable. Version 3 deliberately keeps one load segment so all already-resolved relative references remain exact; the contained data is executable but never writable. A later general PE adapter may split `.rdata` after it has an explicit section-to-RVA relocation contract. `.reloc` begins at the next 4 KiB RVA, is readable, initialized, and discardable. Its directory size is 12 bytes. The block names the `.text` page, has block size 12, and contains two zero `IMAGE_REL_BASED_ABSOLUTE` entries. Those entries are padding and cause no load-address fixup; already-applied relative calls need no PE base relocation.

## Independent verification

`Uefiˉapplicationˉverifier.Verify` accepts untrusted bytes and returns only after checking the complete canonical structure. It validates the outer size before fixed header reads, derives raw and virtual boundaries with checked arithmetic, requires exact section order and permissions, contains the entry in code, rejects missing or trailing bytes, checks the relocation block, and requires all canonical padding to be zero.

The writer invokes that separate parser before publication and compares the parser's recovered code and entry offset with the verified flat link. A failure publishes no bytes.

## Diagnostics

Writer failures return one diagnostic and no application bytes:

| Code | Meaning |
| --- | --- |
| `WVU1001` | The input is null or is not a successful verified link. |
| `WVU1002` | The link base, section kinds, relocation kinds, code range, or entry is unsupported. |
| `WVU1003` | The code exceeds the target limit. |
| `WVU1004` | Independent verification did not reproduce the written application. |

The untrusted-byte verifier throws a bounded format exception:

| Code | Meaning |
| --- | --- |
| `WVU2001` | File size, derived extent, or trailing-byte failure. |
| `WVU2002` | DOS header or stub failure. |
| `WVU2003` | PE signature or COFF header failure. |
| `WVU2004` | PE32+ optional-header or directory failure. |
| `WVU2005` | Section layout, entry, size, or permission failure. |
| `WVU2006` | Base-relocation block failure. |
| `WVU2007` | Nonzero canonical padding. |

## Deliberate limits

Version 3 has no general PE section mapping, writable data mapping, separately protected `.rdata`, PE import table, PE export table, resources, debug data, authenticode signature, exception/unwind table, TLS, absolute WVO-to-PE relocation translation, Secure Boot signature, or compatibility mode. It does not make PE an implicit definition of Windvale semantics. Generalization requires another concrete native-backend or boot-stage case and an independently verifiable rule.
