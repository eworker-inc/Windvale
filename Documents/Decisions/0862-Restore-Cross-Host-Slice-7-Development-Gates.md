# Decision 0862: Restore cross-host Slice 7 development gates

- Date: 2026-08-27
- Status: Implemented candidate with focused Windows evidence; paired-host rerun pending
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Refreshes: [Decision 0496](0496-Native-Segmented-Compiler-Toolset-Reconstruction.md),
  [Decision 0583](0583-Native-Wvb-To-Wvo-Self-Reconstruction.md), and
  [Decision 0510](0510-Native-Wvb-Runner-Source-Reconstruction.md)

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

The second repair passed that refreshed lowerer owner on both hosts, including
both unchanged produced WVO objects, then reached the WVB-runner reconstruction.
That owner still required the obsolete 183,537-byte runner to rebuild through
the pre-WVB-1.32 monolithic front door. The implemented sequential task runner
is a 445,196-byte current split-compiler product and exceeds the single-object
lowering profile; its already accepted development package uses segmented
staging. Both hosts therefore rejected the stale runner family before later
owners ran.

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
- Reconstruct the current WVB runner through one reusable cache-aware split
  project builder and the shared segmented staging/link/transport path. Retain
  only the canonical WVB and two host applications; remove the obsolete
  monolithic WVO instead of widening its single-object profile.
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

GitHub Actions run `33081618051` passed `seed` and the refreshed six-case
WVB-to-WVO reconstruction on both Windows and Linux. Both hosts then rejected
the stale WVB-runner reconstruction, confirming that the prior repair was
correct and moving the first failure boundary forward.

The refreshed runner family is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-Runner.wvb` | 445,196 | `4cdfb53bcd6fe49c7931ec8a0fed0f74aac3f4e10a465f0395c458af4d0a5d67` |
| `windows-x64-wvrun.exe` | 5,327,872 | `7a8b97c68c3463af858b47178978f30507af947d7cf0e86e5ec71829702157c0` |
| `linux-x64-wvrun.elf` | 5,328,896 | `3741a659a5bb3375fa2b0560679a19b746a03596ed8ca0c559e0f6c870f10f27` |

The current split builder reproduces the documented runner WVB through 13
visible phases. Segmented staging emits 5,318,451 object bytes in 11 chunks;
linking emits a 5,309,541-byte image at entry offset 105,270; canonical
transport uses two chunks. The Windows application executes `Return-42.wvb`,
reports exactly four guest instructions, rejects an unknown option with status
64, and rejects the WVO-as-WVB malformed input with status 1. The first exact
owner attempt completed all 13 build phases and reproduced the staged bytes,
then exposed a Windows `findstr` limitation on the macron in the success line;
the guard now matches the exact ASCII byte/chunk suffix. A paired-host pushed
rerun remains required.

GitHub Actions run `33087197973` then exposed a clean-checkout dependency that
the development host had accidentally satisfied with an ignored
compiler-scale overlay. Both hosts passed `seed` and `seed-native-front-door`,
entered runner reconstruction, and rejected the missing overlay before the
current split build could continue. The overlay contained newer native builds
of the same canonical `wvhostrequest` and `wvhostsources` WVBs. The repair
replaces the two limited Windows/Linux application pairs in the canonical
hosted-container toolset instead of retaining a second four-binary family. Its
6,927-byte inventory is now SHA-256
`b15800d907e46c866292302a989584b9825a0594494a529ca96578dab686cb35`.

With the ignored overlay removed, the unified toolset completes all 13 current
split phases and reproduces the exact 445,196-byte runner WVB. The earlier
canonical tools could package the eight-fragment bootstrap analyzer but failed
while hashing the 22-chunk current analyzer; the promoted tools complete that
48,591,360-byte application and the 33,210,368-byte current emitter
application. The resulting analyzer, emitter, bridge, and runner identities are
unchanged. Direct materialization from the validated canonical image reproduces
the tracked Windows application at SHA-256
`7a8b97c68c3463af858b47178978f30507af947d7cf0e86e5ec71829702157c0`
and Linux application at SHA-256
`3741a659a5bb3375fa2b0560679a19b746a03596ed8ca0c559e0f6c870f10f27`.
The integrated Windows owner then reproduced the full compiler and linked image
but exposed the same `findstr /x` limitation on the LF-only link report. Link
and transport guards now match their exact numeric suffixes; isolated reruns of
both guards pass. A second paired-host pushed rerun remains required.

Independent GitHub Windows and Linux development results remain the pushed
evidence required to change this decision from a Windows checkpoint to a
cross-host gate repair.

## Consequences

Windows short-name aliases no longer create a false link alarm, while actual
linked log parents remain rejected. The segmented staging candidate again
embeds the same compiler closure that current source and WVB 1.32 tests verify.
The raw native lowerer family now embeds that closure as well without changing
the object bytes it produces for the two independent golden inputs.
The runner reconstruction now consumes the same current split compiler used by
its Language 1.0 development evidence and no longer retains a duplicate
monolithic WVO.
Compiler-scale hosted packaging now has one tracked canonical toolset owner and
no ignored overlay dependency or duplicate four-binary inventory.
This decision restores prerequisites for Slice 7; it does not close Slice 7 or
claim parallel task scheduling.

## Reconsideration triggers

Replace component walking if the coordinator gains an atomic directory-handle
API that can prove non-link traversal more strongly. Replace whole-family
staging refreshes when content-addressed compiler-closure keys can promote a
self-reproducing affected family without manual identity updates.
