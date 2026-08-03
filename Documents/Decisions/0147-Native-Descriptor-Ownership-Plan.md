# Decision 0147: Native descriptor ownership plan

- Date: 2026-08-03
- Status: Implemented with local Windows evidence; cross-host qualification pending
- Retains: Native ABI 21, execution-context version 7, service-table version 5, target `x86-64-wvb-baseline-v21`, the existing 16-byte descriptor, the 16 MiB dynamic-value arena, and every generated machine byte
- Refines: [Decision 0133](0133-Frame-Owned-Direct-Native-Records.md), [Decision 0137](0137-Bounded-Owned-Values-Before-Dynamic-Collections.md), and [Decision 0143](0143-Bounded-First-Fit-Dynamic-Arena-Replay.md)

## Context

Decision 0143 proves that one concrete reclaiming arena policy has enough capacity for the exact compiler, including headers, alignment, placement, and fragmentation. Capacity evidence does not tell the selector where ownership begins, which aliases retain a backing, when a last use permits release, or how calls and descriptor-bearing records transfer responsibility.

Emitting allocator operations before publishing those answers would hide lifetime policy inside x86-64 byte selection. The next boundary must therefore be explicit native machine-IR evidence that is deterministic, bounded, and independently reconstructed before it changes the ABI or executable bytes.

## Decision

- Attach a version-1 descriptor-ownership plan to every lowered native module before instruction selection. Bound the complete plan to 4,194,304 actions and reject unsupported or inconsistent machine IR as `WVN2903`.
- Identify every descriptor carrier by kind, function, binding, and optional direct-record field. The carrier vocabulary distinguishes parameters, locals, semantic values, direct-record parameter/local/value fields, and the function-return boundary.
- Treat descriptor parameters and descriptor-bearing record-parameter fields as borrowed on entry. Static text/bytes and hosted process-argument or file-snapshot results are borrowed; they do not participate in the reclaimable text arena.
- Treat allocation results, retained aliases, owned descriptor locals, descriptor-bearing record locals, and accepted call results as owned roots. Direct record fields are planned separately; nested native records remain rejected under ABI 21.
- Publish deterministic `borrow-static`, `borrow-host`, `acquire`, `retain`, `release`, `borrow-call`, `accept-return`, and `transfer-return` actions at exact function-entry, block, operation, or terminator positions.
- Preserve immutable inputs until an allocating or alias result exists. For local and record-field replacement, retain the new backing before releasing the old target. The retain target denotes the pending publication at that operation, so self-assignment remains valid.
- Borrow internal-call inputs before accepting the caller-owned return. A callee transfers a descriptor or each descriptor-bearing record field into the function-return boundary before releasing owned frame roots.
- Release semantic values at their last use within the verified empty-stack block. Release owned descriptor locals and direct-record local fields during normal return cleanup; unassigned borrowed parameters require no cleanup.
- Treat a terminal native failure as destruction of the complete execution-owned arena. This plan does not invent unwind edges or contained recovery semantics that ABI 21 does not have.
- Reconstruct the complete plan with a separately implemented oracle. The verifier compares every function summary and ordered action, and the selector refuses code generation unless the reconstruction agrees.
- Do not consume the plan in machine instruction selection yet. ABI 21, its descriptor reserved word, context layout, service table, fragment bytes, host containers, and OS consumers remain unchanged.

## Local evidence

The exact 12-module compiler lowers to 328 native functions and a deterministic 186,557-action ownership plan. It accounts for 293 descriptor parameter bindings, zero assigned descriptor parameters, 3,190 descriptor locals, 524 descriptor-bearing record-parameter fields, zero assigned record parameters, 9,287 descriptor-bearing record-local fields, 6,182 descriptor value identities, and 17,898 descriptor-bearing record-value fields.

The action stream contains 435 acquisitions, 172 static or hosted borrows, 34,772 retains, 144,983 releases, 3,546 borrowed call arguments, 1,660 accepted caller-owned returns, and 989 callee transfers. The largest function is index 204, `Compilerˉsourceˉwirˉcompileˉblock`, with 48,242 actions. The canonical complete action map has SHA-256:

```text
8681cfd9d8c96e3d5dc70c2b97f62795c2e29b632fb66065f2dea8ca102b0511
```

The exact compiler evidence includes both static borrowing and hosted file-snapshot borrowing. Its ABI-21 fragment and function-only compiler execution retain their established identity and behavior; full compiler execution still reaches `WVR3018` because instruction selection deliberately does not consume the plan yet.

A focused portable fixture combines a static byte constant, allocations, a slice alias, mutable descriptor-local replacement, a two-descriptor direct record, internal calls, a record return, field selection, and a descriptor return. Repeated lowering produces the same actions, the independent reconstruction agrees, and the unchanged ABI-21 fragment executes to result zero. Mutating one action kind or the aggregate action count is rejected as `WVN2903`.

Focused ownership, exact-compiler boundary, dynamic descriptor call/return, nominal record, and exact native-output checks pass locally after zero-warning builds. After integration with Decisions 0144 and 0145, change-aware Windows verification completes a zero-warning Release build and passes all 83 selected Seed tests in 324.221 suite seconds; the golden compiler contract takes 213.423 seconds. After the subsequent Decision 0146 WebAssembly rebase, the focused ownership and exact-compiler checks pass again with a zero-warning build. This is proportional development evidence rather than cross-host qualification. No WVB/WVO bytes, source semantics, native ABI, generated machine bytes, OS source, or guest artifact changes, so QEMU is not rerun.

## Consequences

The next ABI candidate no longer needs to infer lifetime from x86-64 instructions. It can consume one published and independently reconstructed action sequence while preserving the existing semantic IR, frame plan, call convention, and exact allocation-capacity evidence.

The retained 16-byte descriptor has an unused `u32` reserved word. A successor ABI may use zero for borrowed/static/host/external storage and a nonzero arena-relative owner token, such as a checked header offset plus one, for reclaimable allocations. Slices can preserve the token while changing pointer and length; machine lowering can implement plan actions against the Decision 0143 arena without widening the descriptor. This is the selected next candidate, not ABI-21 behavior.

The action plan is lifetime evidence, not a garbage collector or an emitted program. It does not reclaim native allocations yet, qualify the full compiler under native execution, handle cycles, add collection syntax, retire .NET, or change the Windvale OS guest ABI.

[Decision 0148](0148-First-Wva-Native-Descriptor-Allocator-Leaf.md) subsequently implements the selected first-fit/reference-count mechanics as one exact WVA-owned x86-64 leaf and projects this plan to 180,190 allocator calls. [Decision 0151](0151-Native-Descriptor-Allocator-Emission-Schedule.md) then publishes and independently reconstructs every physical owner location and emission phase. Decision 0150's ABI-22 generation/checkpoint policy deliberately does not call the leaf; owner-token descriptors and emitted reference-count operations remain a later transition.

## Reconsider when

- A contained trap or recoverable exception requires cleanup across more than the terminal arena boundary.
- Closures, globals, cyclic values, concurrency, or shared long-lived resources require ownership edges that the current acyclic action vocabulary cannot express.
- A valid native module exceeds the bounded action count or needs descriptor-bearing nested records before a successor record ABI defines them.
- Consuming the plan cannot preserve independent byte-level verification or the measured Decision 0143 capacity.
