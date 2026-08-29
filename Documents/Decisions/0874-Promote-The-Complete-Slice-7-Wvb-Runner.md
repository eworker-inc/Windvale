# Decision 0874: promote the complete Slice 7 WVB runner

## Status

Accepted on 2026-08-29. Exact local reconstruction is complete; independent
Windows/Linux Development execution and the final Slice 7 Qualification gate
remain pending.

## Context

The tracked profile-5 WVB-runner family still represented the execution-major-6
task-environment checkpoint from Decision 0867. Later Slice 7 work added queued
children, completion-slot and retained-memory reservation, deterministic
completion-order evidence, awaited provider calls, and provider-generation
recovery. The complete current source therefore produced a different runner
while the reconstruction owner continued to require the older three artifacts.

Decision 0873 removed the compiler-scale blocker and made it possible for the
source-built analyzer/emitter pair to reconstruct the complete runner within
the unchanged profile-7 carrier. Promotion must retain one active canonical WVB
and one active application per host, reconstruct both applications from the
same native image, and must not preserve the obsolete runner as an active
compatibility family. The published 0.1.0 installer is a separate immutable
release contract and still needs its exact historical host inputs.

The first pushed Development attempt also exposed a separate Windows path
validation defect. `Build-Cached-Split-Project-Wvb.mjs` compared the spelling
returned by `realpath` with a legitimate NTFS short spelling such as
`RUNNER~1`, falsely treating the ordinary directory as a link. Path admission
must inspect metadata for every traversed component instead of equating a
different valid spelling with a reparse point.

## Decision

1. Promote the complete 228-function Slice 7 runner as the sole tracked
   `Native-Wvb-Runner-Candidate` family.
2. Reconstruct the WVB through the current split analyzer/emitter pair, then
   stage, link, and canonically transport one native image before materializing
   the Windows and Linux profile-5 applications.
3. Update the development installer input, public run helpers, exact runner
   specifications, and focused source tests to the promoted identities.
   Preserve the stable 0.1.0 installer's exact host runners under the versioned
   `Native-Wvb-Runner-0.1.0` owner instead of letting that immutable manifest
   reference the mutable development candidate path. Historical decisions keep
   their historical checkpoint values.
4. Validate every component of a split-cache file or directory parent with
   `lstat`. Reject missing components, symbolic links, reparse points exposed as
   links, non-directories, and non-ordinary leaf files, while accepting an
   alternate spelling of the same ordinary Windows path.
5. Keep independent paired-host Development and the explicit final Slice 7
   Qualification gate as later evidence. Promotion does not by itself claim
   parallel scheduling or cross-host conformance.

## Evidence

The source-built analyzer and emitter retain the Decision 0873 WVB identities.
They reconstruct this runner family:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-Runner.wvb` | 482,767 | `fc4724c7756f22eb52dd6ed4da9737a865e14ea4d52df1de69fc10236970ff4f` |
| `windows-x64-wvrun.exe` | 5,907,456 | `2721b80158cf4825919be5a6b5c58cfa40d417dc802d5bf27b2584b822ad817b` |
| `linux-x64-wvrun.elf` | 5,906,432 | `611cfbf9fd95e9b29df4a38e3ac392dc9eea87b760b81ff572bad8af6f235eae` |

The WVB stages to 5,899,132 object bytes in 13 chunks. Linking produces a
5,889,164-byte image at entry offset 150,541 in nine chunks, and canonical
transport preserves those bytes in two chunks. Both applications materialize
from that exact image. The direct split-cache test passes its module-order,
identity-publication, and forced-failure-cleanup cases after the path repair.
The 15-object selective installer repository remains structurally unchanged;
its 15 deterministic compressed blobs total 11,003,293 bytes and its 3,548-byte
index has SHA-256
`7200b729d0a3d35717420444ce5398bd222327f3d3b6588dc82d2ad624e31775`.
The repinned development installers are 5,516,228 Windows ZIP bytes at
SHA-256
`e71218d21d28c42cf6e3e69531686fd5fe2b79f25e4cb2b0489593d733b029b6`
and 5,511,747 Linux tar-gzip bytes at SHA-256
`eff1bd1aa4354099f3ff6ce625d00136d53db02436ae90b2415f2629fc6d5839`.
The immutable `0.1.0` stable installer bytes remain unchanged. Its two runner
source paths now select versioned copies with the same already-tracked Git blob
identities, so future active-candidate promotion cannot invalidate stable
reconstruction and repository content storage is not duplicated.

The first local reconstruction reached all 13 current split-compiler phases
and emitted the exact promoted WVB before correctly failing against the stale
tracked identity. The refreshed reconstruction owner and paired-host workflow
remain the acceptance evidence for the settled promotion commit.

## Consequences

- Ordinary `wv run`, effect-front-end execution, scripting composition, and
  installer inputs now select the complete Slice 7 runtime rather than the
  earlier task-environment checkpoint.
- Windows short-name aliases no longer create a false link rejection in the
  split-project cache, while linked ancestors remain forbidden.
- Only three active runner products are retained; the two historical host
  binaries needed to reconstruct the immutable 0.1.0 installers are isolated
  under a versioned release-input owner. Intermediate object, image, and
  transport chunks remain reproducible construction evidence.
- This promotion closes artifact drift but does not yet close Slice 7.

## Reconsideration triggers

Replace component walking if a directory-handle API can provide a stronger
race-resistant no-link traversal contract on both hosts. Repromote the runner
only in a coherent batch when runtime semantics, its source closure, or its
native materialization contract changes.
