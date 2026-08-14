# Decision 0552: Bounded and fast native WVO admission

- Date: 2026-08-14
- Status: Implemented candidate with complete Windows changed-file evidence
- Advances: [native WVO inspector](../../Specifications/Windvale-Native-Wvo-Inspector.md) and [verification throughput](../Architecture/Seed-Verification-Throughput.md)
- Retains: WVO 1.0, WVB 1.11, native ABI 22, hosted inspector profile 6, and the existing validation rules

## Context

Native WVO admission and SHA-256 reporting were coupled behind the `verify`
command even when a build or test caller discarded the report. On the measured
Windows host, the retained candidate verified and hashed a 2,484,162-byte WVO
in a 7.048-second median. Its allocation-heavy one-shot Foundation SHA-256
reporter structurally admitted a valid 4,078,324-byte database WVO but could not
finish the digest within the hosted value arena.

The compiler, lowerer, assembler, and linker already have distinct ownership:
the source compiler emits canonical WVB; the native lowerer consumes WVB and
encodes x86-64 WVO directly; the textual WVA assembler independently emits WVO;
and both object paths converge on the same WVO validator and linker. This
failure was in the converged object-reporting tool, not in WVO encoding or the
database program.

## Decision

- Add `check <object.wvo>` to the Windvale-written inspector. It reads one
  bounded snapshot, performs the complete existing WVO validation, returns the
  same structural failures, and emits nothing on success.
- Retain `verify` and `inspect` as report-bearing commands over one admitted
  in-memory snapshot. Their SHA-256 path now composes the existing bounded
  compression and streaming Foundation modules instead of the allocation-heavy
  one-shot reporter.
- Add digest-bound `Check-Wvo.cmd` and `Check-Wvo.sh` launchers. Move only
  database, native-u64, AOT-chain, and OS-kernel-lowering callers that discarded
  verification output to the structural-only launcher. Exact report and
  rejection-contract callers continue to use `Verify-Wvo`.
- Extend the hostile-size owner with one `check` case and advance the fixed
  retirement inventory from 3,288 to 3,289 cases without adding a suite.
- Reconstruct and pin candidate-4 WVB, WVO, linked fragment, and paired hosted
  applications through the existing compiler, direct WVB-to-WVO lowerer,
  linker, textual startup assembler, and container constructors.
- Do not change the textual assembler, direct lowerer encoder, WVO serializer,
  linker, bytecode, language semantics, native ABI, capability list, startup,
  or service order in this slice. No Stage 0 compiler or managed runtime is
  used by the normal construction or measurement path.

## Candidate identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVO inspector WVB | 73,322 | `40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070` |
| WVO inspector WVO | 1,022,822 | `bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae` |
| Linked inspector fragment | 1,017,780 | `1410b92ebc614f17cbf6e8a1147cb2cd448ae687a3b776e8d4ec3eb96a434854` |
| Windows WVO inspector | 1,037,312 | `5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03` |
| Linux WVO inspector | 1,036,288 | `fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840` |

The module retains the exact profile-6 entry address `82,280`. The enlarged
native object and applications are the explicit cost of composing the current
portable streaming implementation before the native backend has a qualified
lowering for the existing `Bytesˉsha256ˉhex` intrinsic.

## Evidence

The pinned native build front door produced the WVB in 2.204 seconds. A cached
current native build driver produced the same bytes in 1.958 seconds; the
standalone lowerer took 2.764 seconds, the linker 4.668 seconds, and exact paired
candidate reconstruction 29.068 seconds.

End-to-end `Check-Wvo.cmd` measurements were:

| Input | Bytes | Old `verify` median | New `check` median | Result |
| --- | ---: | ---: | ---: | --- |
| Tracked tree-node WVO | 2,484,162 | 7.048 s | 1.488 s | 4.74 times faster |
| Database host-tree WVO | 4,078,324 | reporter exhausted its arena | 3.767 s | complete admission |

The new direct `verify` completed the 4,078,324-byte object in 14.457 seconds
and printed the independently matched digest
`09ff9e759d2da0f3b185444b2db64f58e655df5e411fc84c5d0f6078451940fe`.
This is a bounded correctness result, not a claim that the portable streaming
hash is faster than the old reporter on smaller objects.

The focused Windows owners pass exact paired reconstruction 3/3 in 35.460
seconds, all thirteen stable malformed families through `check`, `verify`, and
`inspect` in 8.150 seconds, and hostile-size containment 5/5 in 1.459 seconds.
The changed-plan contract passes all 27 general and 73 native planner cases.
Independent Linux execution and the final dual-host workflow remain required
before this descendant is described as cross-host qualified.

The settled change-aware Windows gate passed all 20 selected native owners and
1,860 cases in 2,701.451 seconds. Its WVO owners included reconstruction in
34.050 seconds, stable read-only rejection in 6.800 seconds, differential
coverage in 51.880 seconds, random containment in 10.640 seconds, and hostile
size in 1.040 seconds. Database storage passed 14/14 in 1,110.670 seconds,
116.220 seconds (9.5%) below the immediately preceding 1,226.890-second run;
host load and other reconstruction variance mean that observed reduction is
not assigned wholly to the structural checker. The 737.220-second native front
door and database product reconstruction remain the two clearest next targets.

## Consequences

Build and test paths no longer pay for a digest they discard, while human and
exact-report paths preserve same-snapshot identity. Valid near-limit WVOs can
be reported without allocation exhaustion. The focused database/lowerer path
gains immediately without weakening malformed-input or report evidence.

The next compiler/backend opportunity is to qualify native lowering of the
already specified `Bytesˉsha256ˉhex` intrinsic, then compare its size and speed
against the source-composed streaming implementation. Separately, the direct
lowerer and textual assembler should converge on one production x86-64
instruction encoder and one typed WVO object writer; their front ends should
remain distinct.

## Reconsideration triggers

Revisit the source-composed hash when native intrinsic lowering can preserve
the exact lowercase digest, allocation bound, and same-snapshot behavior with
smaller or faster code. Revisit the structural/report split if a caller needs a
cryptographic identity as an admission grant rather than diagnostic output.
