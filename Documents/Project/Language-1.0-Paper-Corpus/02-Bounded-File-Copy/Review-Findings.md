# Workload 2 review findings

## Status

Draft-reviewed under
[Decision 0756](../../../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md).
The workload passes its paper acceptance criteria. Its general byte-buffer,
explicit-completion, known-partial-progress, authority-split, and synchronous
cancellation findings are resolved. Capability catalog publication remains
provisional pending later workloads.

## Acceptance matrix

| Required pressure | Evidence | Status |
| --- | --- | --- |
| Required filesystem capability and rights-reduced instances | Independent source/destination roots acquire one read-only snapshot and one create/write handle. | Pass; authority split accepted, catalog identities provisional |
| Move-only resources and `using` | Nested source/destination scopes release on success, return, and `try` propagation. | Pass; existing language rule sufficient |
| Borrowed mutable buffer slices | One budgeted zero-initialized `Byteˉbuffer`; exclusive read target and later immutable write slices never overlap. | Pass; normative-candidate Foundation signatures accepted |
| Exact partial read/write progress | Positive short reads advance source; partial writes advance only proved bytes; only short acceptance continues. | Pass; exact policy accepted |
| Explicit input/output/chunk/work maxima | 1 MiB copy, 64 KiB transfer, and 2,097,152-call ceilings plus per-launch smaller bounds. | Pass on paper |
| Typed completion versus release | One explicit combined durability call after body success; `using` performs local release only. | Pass; semantic clarification accepted |
| Cancellation and provider loss | Cancellation, loss, restart, source change, and post-dispatch uncertainty remain distinct. | Pass; synchronous profile accepted, general token deferred |
| Empty file and exact maximum | No-transfer empty finish and 16-read/16-write maximum transcript recorded. | Pass on paper |
| Source growth and destination full | Content generation change and exact partial capacity progress are terminal typed outcomes. | Pass on paper |
| Zero-progress provider defect | Pre-EOF zero read and zero partial write cannot spin. | Pass on paper |
| Finish failure and early propagation | Body failure skips finish; finish failure replaces no body result; all handles release in reverse order. | Pass on paper |
| Provider restart | Replacement generation is explicit and no live handle retargets. | Pass on paper |
| No retry after uncertainty | Write and finish indeterminate cases return immediately and never switch provider or repeat. | Pass on paper |

## Main design findings

### A safe mutable byte buffer belongs in Foundation

The existing candidate described mutable slices and byte builders but lacked an
exact fixed caller-owned buffer constructor. File reads need writable memory
without exposing uninitialized safe bytes or abusing a text/append builder.

`Constructˉbuffer`, `Bufferˉlength`, `Borrowˉslice`, and
`Borrowˉsliceˉmut` form the smallest reusable surface. Zero initialization keeps
all safe values valid; read completion determines which prefix may be observed.
The same group will support sockets, decoders, and device I/O without adding raw
pointers.

### `using` should remain release-only

Automatically finishing a destination when a scope exits would obscure whether
the body succeeded, whether finish ran, and which error should be returned.
The source is clearer when `Engine.Copy` must succeed before one visible
`Finishˉdurable` call. `using` then has one universal job: consume and invalidate
the local handle on every ordinary exit.

This workload does not need a general combined body/finish result because it
does not finish a failed partial copy. A later protocol that must finish after
body failure needs an explicit named composition type rather than a hidden
precedence rule.

### Known partial progress and uncertainty are different control paths

An exact positive `Shortˉacceptance` can safely advance the position and submit
only the remaining suffix. It is not an automatic retry. Capacity, cancellation,
or another terminal reason after a proved prefix preserves that prefix and ends
the body. Indeterminate progress provides no count and ends immediately.

This distinction keeps ordinary host short writes usable without weakening the
no-replay rule for uncertain mutation.

### Source and destination authority should not be one broad root

Read-only acquisition and create/write/durable-finish authority have different
security and failure contracts. Independent roots let deployment bind different
directories/providers and deny replacement, deletion, enumeration, or source
mutation. One general filesystem object would make least-authority review harder
without simplifying source materially.

The exact root operation signatures remain provisional because transaction and
concurrent service workloads may require owned directory instances, deadlines,
or richer recovery identities. The authority separation is accepted now.

### Synchronous cancellation does not yet justify a general token API

The named launcher profile can bind one cancellation generation into both
provider roots, making each I/O call a documented cancellation point. This
proves distinct cancellation behavior without inventing a task or hidden
asynchronous exception.

Workloads 5 and 6 will test source-requested cancellation, deadlines, waits, and
concurrent restart. They should decide the exact general cancellation-token
surface with stronger evidence.

## Owner resolutions

1. Accept the four exact byte-buffer signatures and zero-initialized ownership
   contract in the normative-candidate Foundation.
2. Clarify that `using` performs local release only; workload 2 attempts finish
   once after body success and preserves its exact result.
3. Accept suffix-only continuation after a positive `Shortˉacceptance`; prohibit
   continuation after indeterminate mutation.
4. Accept independent source and destination filesystem authorities while
   keeping their final catalog/signature-set identities provisional.
5. Accept launcher/provider cancellation generation for this synchronous
   profile and defer a general source-visible token to workloads 5 and 6.

## Quantitative review record

| Measure | Recorded value |
| --- | --- |
| Source size | 4 modules, 807 physical lines, 10 functions, 2 records, 3 enums, 1 variant. |
| Maximum source width | 6 function parameters; 5 record fields; 14 cases in `Copyˉfailure`; largest module 323 lines; largest function 186 lines. |
| Explicitness | 2 capability roots, 2 `using` scopes, 1 owned byte buffer, 11 mutable-borrow expressions, 8 mutable locals, and 1 finish call. |
| Resources | 98,304-byte application domain; 65,536-byte maximum source buffer; 1 source and 1 destination handle; 0 tasks/queues/timers. |
| Work | 1,048,576 copied bytes, 65,536 bytes per call, 2,097,152 transfer calls, 1 finish, and 0 uncertainty retries. |
| Failure surface | 14 top-level copy cases, 17 normalized I/O reasons, 4 mutation completion states, and 3 finish states. |
| Compiler planning | Ceilings of 24 generic instances, 192 WIR blocks, 1,536 WIR operations, 16 call-depth units, 32 diagnostics, and 384 KiB retained evidence. |
| Artifacts | WVB/native sizes unknown until implementation; no new syntax, WIR family, backend format, or package data. |
| Usability | Successful orchestration is one validation, one buffer, two nested resources, one transfer call, and one explicit finish. Most detail stays in the reusable engine and typed contract. |

The compiler values are admission ceilings for a future executable fixture, not
measurements. Implementation must record tokens, phase time, generic instances,
WIR blocks/operations, retained evidence, WVB/native bytes, execution time, peak
memory, provider calls, and cleanup order.

## Review revisions

1. Split source and destination authority instead of granting a general
   filesystem root.
2. Selected exclusive destination creation so failure never truncates an
   existing object.
3. Chose an immutable source snapshot contract so source growth is explicit.
4. Reused `Mutationˉoutcome` for writes and added a smaller read/finish shape.
5. Made only `Shortˉacceptance` resumable and kept every other partial reason
   terminal.
6. Put a call ceiling alongside byte/chunk ceilings to bound one-byte progress.
7. Kept finish explicit and release locally infallible.
8. Deferred general cancellation tokens until concurrent workloads exercise
   them.

## Owner resolution

The project owner accepts all five resolutions through Decision 0756 with no
source revision required. Workload 2 is draft reviewed. The general Foundation
and resource rules are normative-candidate contracts; the two filesystem roots
remain accepted paper candidates until cross-workload reconciliation.
