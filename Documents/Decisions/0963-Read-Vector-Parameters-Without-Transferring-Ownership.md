# Decision 0963: read Vector parameters without transferring ownership

## Status

Candidate implementation verified in focused Windows and Debian runs on
2026-09-15. Native lowering, installed promotion, and full qualification remain
open. See the [parameter-read evidence](../Evidence/2026-09-15-Vector-Parameter-Length.json).

## Context

The accepted `Vectorˉlength` API observes an immutable borrow. The current
source compiler nevertheless rejects a direct parameter, including immutable
and exclusive Vector parameters already represented by WVB 1.26. Owned
Option/Result consumers need useful helpers over their collections.

The existing `CA vector.length` preserves a uniquely owned operand. Allowing
borrowed operands there would weaken an existing bytecode contract and could
manufacture ownership through its preserved stack value.

## Decision

1. Add candidate WVB 1.40 instruction `E2 vector.parameter_length`, with a
   direct parameter slot and exact Vector type index. It produces only `u64`;
   it neither consumes nor produces collection ownership.
2. Permit the existing source `Vectorˉlength` operation on by-value, immutable,
   and exclusive Vector parameters. Keep source move, alias, lifetime, and
   mutation restrictions. Length access does not make mutation or freezing of
   borrowed parameters legal.
3. Select minor 40 only when the new operation is emitted. Retain `CA` and
   byte-identical lowering for owned locals. Earlier versions reject `E2`.
4. Require complete semantic, type, and live-owner validation before host
   execution. An owned parameter consumed on any incoming path is unreadable;
   borrowing cannot extend the caller's ownership lifetime.
5. Initially connect the capability-free synchronous scalar host profile,
   including existing Main-owned reserved construction and append. Keep
   browser, native lowering, installed tools, and OS promotion separate.

The bytecode specification owns the exact encoding and bounds. The existing
memory-budget split-execution test owner owns focused coverage; this change
does not introduce another coordinator or public library signature.

## Completion boundary

This prerequisite does not deliver a maintained owned-resource consumer,
direct Vector payload extraction, exclusive Option/Result borrowing, Take,
mapping, or the full Libraries 1.0 migration. The focused fixture covers direct
reads of empty and nonempty collections in all three parameter modes, subsequent
ownership transfer, deterministic publication, and malformed/source rejection.

Broader attempted cases exposed still-open call-lowering gaps: forwarding a
borrowed Vector through another helper produces a temporary that fails typed
verification; repeated borrowing in a loop fails the control-flow ownership
check. These are not covered by the passing direct-read claim. They require
coherent borrow-temporary and loop-lifetime handling, not relaxed verification.

Native construction also exposed an oversized verifier function. Reusing the
existing fixed instruction-width decoder removed duplicated dispatch and kept
the native slot bound unchanged. Both rebuilt verifier and runner packaged
successfully; candidate minor-40 native lowering itself remains closed.
