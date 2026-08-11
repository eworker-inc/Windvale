# Decision 0524: Windows native retirement exit closure

## Status

Implemented and qualified by the complete local Windows native-retirement
suite. Independent Linux execution, paired report comparison, candidate
promotion, and the final Stage 0 recovery archive remain pending.

## Context

The grouped retirement run exposed one final Windows process-boundary defect.
The hosted verifier and inspector startups restored their entry frames and
returned to the PE loader after Windvale `Main` completed. That did not provide
an owned process-exit contract and could turn a verifier rejection into process
success. The native Windows platform image also lacked the corresponding
`KERNEL32.dll!ExitProcess` import.

The same run exposed a separate launcher-boundary mismatch. Before the linker's
Windvale `Main` can execute, the hosted compiler shell may reject an immutable
input snapshot with internal result 73: the `WVR3025` base plus file-service
detail 9. That internal composition result is not part of the public linker CLI
contract, whose ordinary rejected-input result is one.

Both changes alter exact startup, platform, container, verifier, runner,
publisher, admission, promotion, reconstruction, and manifest identities. The
result therefore has to be qualified as one deterministic candidate closure.

## Decision

### Terminate Windows verifier processes explicitly

The Windows x64 hosted-verifier and hosted-inspector startups now pass the
Windvale `Main` result to `ExitProcess`. Each call reserves 40 bytes: the
required 32-byte Windows x64 shadow space plus eight bytes needed to retain
16-byte stack alignment at the call boundary. A trailing trap makes unexpected
return from the non-returning host function explicit.

The Windows native hosted-verifier platform imports `ExitProcess` as the final
KERNEL32 entry. The reference C# import and startup mirrors, exact retained
WVOs, Windvale layout/startup contracts, consumers, and process tests advance
together. Windows verification and inspection now preserve rejection results
at the process boundary instead of depending on loader fall-through behavior.

### Normalize only the shell's immutable-input rejection

`Link-Wvo.cmd` and `Link-Wvo.sh` translate exact child result 73 to public
linker result one. Every other child result is preserved. This is a bounded
adapter for the already specified immutable-snapshot failure; it does not
define a general modulo-result translation or change Windvale linker results.

### Refresh the complete deterministic candidate closure

All candidates embedding the affected Windows startup or shared hosted tooling
are regenerated, and every dependent exact size and digest is refreshed. The
hosted-container toolset inventory is 6,927 bytes at SHA-256
`dc1899b252a8ad0f75eeee33cdec82d9cbbba40c7ba8115bb55aaad0b9dd00c8`.
The publisher-construction inventory is 5,064 bytes at SHA-256
`161787321f0741cc5007bd263afb23f11a7c697a557c70e8c1c09e04877c071a`.

The principal final publisher identities are:

| Product | Host | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| Hosted-verifier application publisher | Windows | 256,000 | `2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12` |
| Hosted-verifier application publisher | Linux | 254,965 | `8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e` |
| Publisher promoter | Windows | 681,472 | `5690fb32c7fec85551e0c5cd58e4f56589a5ad4c09108b5dde86fa9fc7b3fb92` |
| Publisher promoter | Linux | 680,949 | `3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5` |
| Publisher admitter | Windows | 570,368 | `1407ed428387986e170b4d8394e9a0a6295408ef668d5d6e16d719102428dd4f` |
| Publisher admitter | Linux | 569,344 | `27fff54e139228586a6948aa234de60e5d4f5439e6b0616a55c057d4ad8661c2` |

Linux identities change where a shared WVB, WVO, construction request, or
manifest is embedded. This is deterministic construction propagation, not
Linux execution evidence.

## Evidence

The complete local Windows native-retirement coordinator passes all 43 suites
and all 3,204 cases in 1,754.6 seconds. It covers the native source, compiler,
verifier, assembler, linker, WVB/WVO inspection and execution, publisher and
promotion pipelines, random malformed-input containment, baseline JIT, AOT,
OS object construction, and retained UEFI boundaries selected by the canonical
retirement plan.

The affected publisher pipeline passes 15/15, console publisher reconstruction
passes 3/3, WVO publisher reconstruction passes 2/2, and the console packager
source/container selections pass 2/2 and 4/4. Random WVO containment passes
500/500. After one non-reproducing source-containment interruption, three
consecutive focused source-containment runs pass 1,500/1,500 and the complete
coordinator's final source-containment selection passes another 500/500. Its
failure diagnostic now records the actual exit result and stderr without
weakening the assertion.

All ten changed JSON manifests parse. Both retained `SHA256SUMS` inventories
were independently recalculated against every listed file. `git diff --check`
passes. The private Linux QA endpoint did not accept the configured SSH
connection during this closure, so no independent Linux result or private QA
configuration is recorded here.

## Consequences

The normal Windows native path now owns exact verifier and inspector process
termination, including rejection status. The linker front doors expose their
specified public rejection result even when the hosted shell rejects the input
before Windvale `Main` begins. The regenerated closure is internally
consistent and completely qualified on Windows.

This decision does not complete Decision 0057 by itself. The remaining final
gate is independent Linux execution of the same retirement plan, followed by
paired evidence comparison, candidate promotion, the explicit Stage 0 recovery
archive, and the final normal-path audit. No QA connection data belongs in the
public repository.

## Reconsideration triggers

Replace the result-73 adapter when the hosted compiler shell exposes a typed
public rejection result or enters the Windvale linker before immutable-input
admission. Revisit the startup only if the Windows ABI, import layout, or
non-returning process-termination contract changes. Never infer Linux
qualification from shared construction identities or Windows execution.
