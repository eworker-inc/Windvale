# Decision 0862: Restore cross-host Slice 7 development gates

- Date: 2026-08-27
- Status: Implemented candidate with focused Windows evidence
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Refreshes: [Decision 0496](0496-Native-Segmented-Compiler-Toolset-Reconstruction.md)

## Context

The first pushed Slice 7 checkpoint reached the affected-owner planner on both
GitHub hosts but failed before the structured-task owner ran. Windows rejected
the ordinary owner-log directory because the runner exposed one ancestor with
its legitimate NTFS short spelling (`RUNNER~1`) while `realpath` returned the
long spelling. Linux rebuilt the segmented compiler toolset and found that the
WVO-staging producer still described the pre-WVB-1.32 compiler closure.

Neither failure changes Language 1.0 semantics. Both are development-gate
defects that must be repaired before cross-host task evidence is meaningful.

## Decision

- Validate every owner-log directory component with `lstat` metadata. Reject a
  symbolic link, junction, or non-directory component, but do not infer a link
  from two different valid spellings of the same Windows path.
- Give the stream boundary one independently registered four-case owner for a
  fresh path, occupied path, ordinary directory, and linked parent.
- Reconstruct all nine segmented compiler-toolset artifacts from current
  source. Refresh only the WVO-staging WVB and its Windows and Linux launchers;
  require the compiler-image staging and canonical-transport families to remain
  byte-for-byte unchanged.
- Emit bounded build and package phase progress from the multi-minute
  constructor and preserve its captured output when reconstruction fails.
- Do not add a source alias, task opcode, runtime behavior, bootstrap stage, or
  compatibility reader as part of this repair.

## Evidence

The refreshed self-reproducing WVO-staging family is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvo-Staging-Producer.wvb` | 576,810 | `db08361446b4524c63c04e6265e1775d65b2b2ae464fe8cfc351b7d1f77d62be` |
| `windows-x64-wvstage.exe` | 8,416,768 | `fa939fb8f0d45182dcffe1a4a4e9e2eedb264bcba5e71544e81e7c5c0013628e` |
| `linux-x64-wvstage.elf` | 8,417,280 | `3e50fb3170779cfe3d2ee6b12581f2b0b69793c372d5bdf7e9777d2cc7e3e564` |

A second-generation staging, linking, transport, and hosted package reproduces
both launcher byte identities exactly. The other six toolset artifacts retain
their prior bytes and digests. The focused Windows reconstruction owner passes
all four cases, including staging the fixed 992,412-byte bootstrap analyzer as
31,736,596 object bytes across 41 chunks.

The stream self-test passes four cases through the real verification-owner
coordinator. The registry advances to 114 owners and 5,529 cases at 18,379
LF-only bytes and SHA-256
`4f4d747218e8c2e7d168aba1da75e35c2d013e0da66d591205b83923ecc61238`.

Independent GitHub Windows and Linux development results remain the pushed
evidence required to change this decision from a Windows checkpoint to a
cross-host gate repair.

## Consequences

Windows short-name aliases no longer create a false link alarm, while actual
linked log parents remain rejected. The segmented staging candidate again
embeds the same compiler closure that current source and WVB 1.32 tests verify.
This decision restores prerequisites for Slice 7; it does not close Slice 7 or
claim parallel task scheduling.

## Reconsideration triggers

Replace component walking if the coordinator gains an atomic directory-handle
API that can prove non-link traversal more strongly. Replace whole-family
staging refreshes when content-addressed compiler-closure keys can promote a
self-reproducing affected family without manual identity updates.
