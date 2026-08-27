# Decision 0862: Restore cross-host Slice 7 development gates

- Date: 2026-08-27
- Status: Implemented candidate with focused Windows evidence; paired-host rerun pending
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Refreshes: [Decision 0496](0496-Native-Segmented-Compiler-Toolset-Reconstruction.md),
  [Decision 0583](0583-Native-Wvb-To-Wvo-Self-Reconstruction.md)

## Context

The first pushed Slice 7 checkpoint reached the affected-owner planner on both
GitHub hosts but failed before the structured-task owner ran. Windows rejected
the ordinary owner-log directory because the runner exposed one ancestor with
its legitimate NTFS short spelling (`RUNNER~1`) while `realpath` returned the
long spelling. Linux rebuilt the segmented compiler toolset and found that the
WVO-staging producer still described the pre-WVB-1.32 compiler closure.

Neither failure changes Language 1.0 semantics. Both are development-gate
defects that must be repaired before cross-host task evidence is meaningful.

The first repair then passed the stream owner and segmented reconstruction on
both GitHub hosts. Both hosts advanced to the independent WVB-to-WVO
reconstruction owner and found that its lowerer family still carried the
pre-WVB-1.32 compiler-closure identity. Its two independent input WVBs and WVO
outputs remained byte-identical; only the source-built lowerer WVB and its two
host launchers changed.

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
- Reconstruct all seven WVB-to-WVO artifacts from current source. Refresh only
  the lowerer WVB and its Windows and Linux launchers; require both independent
  input modules and both produced WVO objects to remain byte-for-byte unchanged.
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

The refreshed self-reproducing WVB-to-WVO lowerer family is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-To-Wvo.wvb` | 567,615 | `77ce798c67281e2fa5d576a1d229f8ec947427a092f8720909a09e32e9711e60` |
| `Wvb-To-Wvo.exe` | 8,160,256 | `f21a0767685e6e29604625852794ae1118fe41060e639fc690baecb7c60dedad` |
| `Wvb-To-Wvo.elf` | 8,159,232 | `1420be3ab40e02a5a7f2e837501c834c80eb8beed6e0c201451b4bda00520185` |

The focused Windows WVB-to-WVO reconstruction owner passes all six cases.
`Return-42.wvb`, `Return-42.wvo`, `Metadata.wvb`, and `Metadata.wvo` retain
their previous byte identities. Constructor failures now report their own
captured output and diagnostic instead of showing only the preceding metadata
verifier report.

Independent GitHub Windows and Linux development results remain the pushed
evidence required to change this decision from a Windows checkpoint to a
cross-host gate repair.

## Consequences

Windows short-name aliases no longer create a false link alarm, while actual
linked log parents remain rejected. The segmented staging candidate again
embeds the same compiler closure that current source and WVB 1.32 tests verify.
The raw native lowerer family now embeds that closure as well without changing
the object bytes it produces for the two independent golden inputs.
This decision restores prerequisites for Slice 7; it does not close Slice 7 or
claim parallel task scheduling.

## Reconsideration triggers

Replace component walking if the coordinator gains an atomic directory-handle
API that can prove non-link traversal more strongly. Replace whole-family
staging refreshes when content-addressed compiler-closure keys can promote a
self-reproducing affected family without manual identity updates.
