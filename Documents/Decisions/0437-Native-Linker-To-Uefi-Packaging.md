# Decision 0437: Native linker to UEFI packaging

- Status: Implemented current-host integration candidate; retained host-container publication and Probe 40 composition pending
- Date: 2026-08-09
- Advances: [Decision 0436](0436-Windvale-Native-Uefi-Application-Construction.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale UEFI application format](../../Specifications/Windvale-Uefi-Application.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0436 transferred canonical UEFI v3 construction and verification to
portable Windvale, but its byte-envelope constructor was not connected to a
hosted file boundary. The digest-bound native linker already writes one exact
flat image and reports the verified entry offset. Reintroducing either PE
layout or linker interpretation in a new host adapter would duplicate semantic
ownership and preserve the managed package constructor in the normal path.

## Decision

- Keep UEFI construction as a typed portable function over linked-image bytes
  and an entry offset. Retain the `WVUR`/`WVUC` bridge as a separate test and
  native-tool envelope rather than making it the reusable library interface.
- Add one hosted Windvale packager with the exact command boundary
  `wvuefi <native-image.bin> <entry-offset> <output.efi>`.
- Give that packager only argument, diagnostic, console, file-read, and
  file-write capabilities. It parses the decimal entry, reads the already
  linked image, invokes the typed constructor, and writes only a verified
  successful result.
- Reuse the existing hosted console-packager service profile; add no new C#
  production implementation, service, PE writer, or fallback.
- Add a Project 1 front door and pin the exact compiler-aligned WVB identity.
  Do not retain generated WVB, WVO, EFI, native host-container, firmware, or
  vendor-specific metadata in this slice.

## Evidence and consequences

The reviewed focused linker selection passes both UEFI cases on the current
Windows host: 2/2 tests in 6.854 seconds. The integration case invokes the real
digest-bound native `Link-Wvo` launcher on a WVO whose exported `Main` begins at
offset one, then passes the exact six linked bytes and reported entry into the
Windvale packager. The resulting 1,536-byte EFI matches the frozen Stage 0
writer byte for byte, independently verifies back to the same code and entry,
and repeats deterministically. An entry at the end of the image fails without
changing an existing destination. The executed native packager loads no CLR.

Native Project 1 builds reproduce these pinned WVB identities:

| Application | Bytes | SHA-256 |
| --- | ---: | --- |
| UEFI verifier | 14,831 | `dc069d256ec0cba2c402afc7fc32421704a7c20c5d332b3bde013a11f80aa83e` |
| UEFI constructor bridge | 24,811 | `858f718b26e34966f19d53ff725a215935bd6dcaa0a93a2b5329367bbdece956` |
| Hosted UEFI packager | 25,999 | `063f95f53e39390c76bcf31fbf7bdc87eed6194388101fadc4d60ee41b2802e4` |

This closes the source/tool handoff from native flat linking to canonical EFI
bytes. It does not yet promote O2. The focused test constructs the temporary
Windows/Linux hosted containers with the frozen Stage 0 builders as
differential evidence. Retained digest-bound host-container construction,
independent Linux execution, upstream native Probe 40 object composition, all
five scenario images, and the grouped retirement gate remain open.

## Reconsideration triggers

Change the command boundary if native linking publishes a typed manifest that
can replace the decimal entry argument, or if Probe 40 requires segmented input
beyond the current 4 MiB value limit. Do not duplicate linker-map or PE layout
rules in the coordinator, weaken destination preservation, or restore an
implicit managed fallback.
