# Workload 5 implementation responsibilities

## Status

This map assigns future implementation and verification ownership. No row
authorizes Language 1.0 implementation before source freeze. Existing network,
TLS, and bounded-operation candidates remain independent evidence rather than a
claim that this paper application runs today.

## Responsibility map

| Boundary | Future owner | Required work | Language/compiler special case? |
| --- | --- | --- | --- |
| Slice length/index and immutable byte-range borrow | Foundation collections/bytes | Implement checked one-owner slice observations and malformed-range tests. | No; ordinary borrow/generic lowering. |
| Strict UTF-8 decode from `Slice<u8>` | Foundation text | Reuse the strict decoder without an intermediate immutable-byte copy; retain allocation/source distinction. | No. |
| Decimal append to byte builder | Foundation bytes | Implement invariant shortest `u64` decimal append with all-or-nothing capacity check. | Optional ordinary intrinsic only. |
| Opaque operation context | Foundation operation/task plus launcher | Bind monotonic generation/deadline and scope-derived cancellation view; reject forgery, escape, and extension. | Lifetime evidence; no new syntax or WVB opcode. |
| Rights-limited stream accept | Network service provider/launcher | Bind one Copy shared-call endpoint to exact service/rights/limits/generation; awaited accept returns one move-only stream. | Canonical capability import only. |
| Reliable stream read/write | Host adapters plus shared stream core | Suspend explicitly while preserving exact counts, deadline/cancellation races, generation checks, and indeterminate mutation. | Ordinary async capability call. |
| HTTP parser/router/renderer | Foundation/application library | Implement source after freeze and keep strict single-request framing. | No HTTP compiler feature. |
| Resource teardown | Provider/runtime | Local release invalidates one handle and bounds shutdown without inventing graceful completion. | Existing `Localˉrelease` recognition only. |
| Capability approval | Launcher/service manager | Approve exact transitive requirement and bind provider separately from application package. | No. |
| Verification | Focused Language 1.0 HTTP/network owners | Convert every valid, rejected, limit, UTF-8, progress, cleanup, race, and cross-host case into bounded fixtures. | No. |

## Compiler work after source freeze

The existing Windvale compiler must support the already selected edition-1
surface used here: modules/profiles/effects, records/variants, explicit generic
map construction, rank lookup, Copy read-through, borrows/slices, `using`,
checked arithmetic, builders, strict results, and capability calls. It should
emit diagnostics for:

- a slice escaping its owner;
- immutable buffer access while a mutable provider target is live;
- a missing transitive capability/effect;
- a provider operation called without `await` or `task.suspend`;
- construction of an opaque operation context;
- use after stream move/release; and
- an unproved checked slice/map access.

This workload adds no second compiler, HTTP AST/opcode, exception, coroutine,
GC, macro, reflection, dynamic dispatch, or automatic retry lowering.

## Provider implementation sequence

1. Freeze the reconciled workload-6 operation-context, task, endpoint, and async
   provider contracts.
2. Freeze the general slice/decode/builder contracts.
3. Implement Foundation calls with simple correctness oracles and malicious
   range/UTF-8/count tests.
4. Adapt the existing bounded-operation and reliable-stream cores to the frozen
   source-visible provider interface.
5. Implement one host provider on Windows and Linux, preserving generation and
   exact-progress evidence.
6. Compile the seven source modules with the one shared compiler/backend path.
7. Execute canonical and adversarial transcripts on both hosts and compare
   response bytes, typed outcomes, cleanup, parser work, and memory maxima.

## Qualification evidence

The final focused owner must report source/WIR/WVB/native sizes, compile and
execution time, peak working set where practical, request bytes, read/write
calls, parser work, response bytes, and deterministic hashes. Cross-host claims
require independent Windows and Linux execution with identical canonical
responses and semantics. A broader gate is not a substitute for these focused
cases.
