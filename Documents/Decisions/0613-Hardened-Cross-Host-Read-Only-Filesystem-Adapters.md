# Decision 0613: Hardened cross-host read-only filesystem adapters

- Status: Implemented Windows native candidate; independent Linux execution pending
- Date: 2026-08-15
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [read-only directory capability](../../Specifications/Read-Only-Directory-Capability.md)

## Context

The fixed WVDB Query capability already separated application source from host
paths and native handles, but the host leaves did not explicitly prevent a final
link or reparse-point substitution. The broader filesystem plan requires Windows
and Linux adapters to preserve the same no-link, regular-file semantic boundary
before configurable directories or mutation are added.

## Decision

- Keep the public capability and provider table unchanged; harden only the
  private platform leaves.
- On Linux x64, open the fixed entry with `O_RDONLY | O_CLOEXEC | O_NOFOLLOW`,
  then require `fstat` to report `S_IFREG` before retaining its descriptor and
  length.
- On Windows x64, add `FILE_FLAG_OPEN_REPARSE_POINT`, resolve
  `GetFileInformationByHandle` from the admitted Kernel32 image, and reject
  directory or reparse-point attributes before retaining the handle and length.
- Treat every rejected link, reparse point, directory, oversized object, or host
  failure as an explicit provider failure; never expose a native error or retry a
  substituted object.

## Evidence and consequences

The Linux leaf assembles to 681 bytes with SHA-256
`0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86`;
the Windows leaf is 1,951 bytes with SHA-256
`d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34`.
They produce pinned 258,048-byte hosted applications with respective SHA-256
identities `b21095d6ab62209b67053b7dfe1cf5a2f0130b3722a09a8e48284fc1aa988b3f`
and `198d44b49db6765792c835c6419da88f0cbcc0de0422748b0d15cb4ae5e6ba32`.

The Windows owner passes six real-I/O cases: two successful values, missing key,
unauthorized name, unavailable object, and junction traversal denial. The Linux
owner contains the same corpus with a symbolic-link denial and pins the exact
Linux image, but independent Linux execution is required before cross-host
qualification is claimed.

This closes the first fixed read-only native adapter, not filesystem slice 2 as a
whole. Configurable directory instances, complete native-status normalization,
writable operations, replacement, durability, revocation/restart, guest service
composition, FAT32, and persistent layout remain open.

## Reconsideration triggers

Reconsider the fixed-path leaf when the launcher can bind typed directory
instances. Do not weaken no-link or regular-file enforcement to obtain broader
host compatibility; introduce a separately named capability if links become a
required semantic feature.
