# Decision 0392: Shared immutable-snapshot publisher shells

- Status: Implemented candidate; Linux execution pending
- Date: 2026-08-08
- Advances: [Decision 0391](0391-Reusable-Windows-Durable-Multi-Chunk-Publication.md), [Decision 0390](0390-Reusable-Linux-Durable-Multi-Chunk-Publication.md), [Decision 0389](0389-Shared-Immutable-Snapshot-Sequence.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native x64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md#hosted-immutable-snapshot-staging-boundary)

## Context

Decisions 0390 and 0391 extracted durable mutation from the staged-WVO
adapters, but the remaining 467-line Linux and 689-line Windows adapters still
owned format-neutral argument handling, hosted runtime setup, snapshot
acquisition, resource reopening, byte comparison, native identity checks, and
destination-alias rejection. The hosted-container publisher needs those exact
operations with a different immutable-snapshot selection policy.

Copying the remaining adapters would restore two large parallel platform
implementations. Keeping their WVO names would also obscure that the code is a
shared host boundary rather than object-format behavior.

## Decision

Extract `Linux-X64-Immutable-Snapshot-Publisher.wva` and
`Windows-X64-Immutable-Snapshot-Publisher.wva` as the platform-specific,
format-neutral acquisition shells. Each shell receives:

- the first selected snapshot ordinal;
- stride one or two;
- the fixed payload header skip; and
- the address of the format-specific immutable-sequence validator.

The shell owns process arguments, hosted runtime storage, resource reopening,
complete snapshot byte comparison, source/destination native identity checks,
alias rejection, and the call into the reusable durable transaction. The
validator remains the only owner of the selected table policy, while the
durable transaction remains the only owner of mutation.

Replace each former staged-WVO adapter with a 14-line tail-jump policy wrapper.
It selects `(2, 1, 0)` and the WVO validator. Add an equivalent 14-line hosted
wrapper selecting `(3, 2, 40)` and the hosted-container validator. The tail jump
preserves the original platform entry stack and alignment for the shared shell.

The resulting Linux shell is 461 lines and the Windows shell is 674 lines.
Those files remain cohesive platform boundaries; neither is split into numbered
fragments merely to reduce line count.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Linux staged-WVO policy adapter WVO | 281 | `d0a3cb41b6ffcc0fe6e616e1d2ac3b067252fe1ae20c8c40532505bcd6491be5` |
| Linux hosted-container policy adapter WVO | 294 | `fe6b4d60fcf459d2f3f624b58b461b95fc9bf325421712e19ac9aa72dcebf527` |
| Linux immutable-snapshot publisher shell WVO | 3,485 | `423bd086f68c03b3fd26c296a1789392ebd72a74e5fd10adf0d2e596d2fd2e6d` |
| Windows staged-WVO policy adapter WVO | 285 | `86dd44e921418a82c69aa155b671662fa2961041d0ead661a0328f3371f7f045` |
| Windows hosted-container policy adapter WVO | 298 | `bfb42ca6a679a25c7a45660bf0743ee3f4e64febceeede6b079126e1df0aab75` |
| Windows immutable-snapshot publisher shell WVO | 6,116 | `d5233eb678b1c96eb6c8c4108ff10d7bcc263678defb81915ab1c67a6b398110` |
| Linux staged-WVO publisher application | 6,455,869 | `01f645e2a2a6f46e059eb7adcbba3d918b55a848e11c2c4f7a50271c7d734c22` |
| Windows staged-WVO publisher application | 6,458,880 | `edb6bf2c08117dfe3f62a5c368abe2b0708ed040fa9e17b98494e41edea1226a` |

The reviewed focused test assembles and pins all six changed objects, rebuilds
both platform applications, and executes the shared Windows shell through the
existing success, changed-content, destination-alias, preservation, and scratch
cleanup matrix. It passes 1/1 in 6.830 test seconds after a 9.83-second
zero-warning build.

This Windows run proves Linux package construction but not Linux process
behavior. Linux execution, both hosted-container application packages, hosted
payload publication, and the grouped dual-host gate remain open. No broader
verifier was run.

## Consequences

- WVO and hosted-container publication now share one acquisition and identity
  implementation per permanent host.
- Format policy, platform acquisition, and durable mutation have distinct
  object boundaries.
- Hosted-container packaging can connect its admitted snapshot set without
  copying the platform adapter or concatenating segments in managed code.
- C# changes remain deletion-bound Stage 0 object layout, relocation, and exact
  identity pinning; no new product acquisition or mutation semantics moved into
  managed code.

## Reconsideration triggers

Version or replace the shell interface if platform entry conventions, hosted
runtime layout, immutable snapshot lifetime, resource identity, or destination
alias policy changes. Do not pass an unvalidated selector or mutable snapshot
set into the durable transaction.
