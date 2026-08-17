# Workload 11 rejected and boundary cases

## Rule

Each case names its earliest rejecting owner. A later provider or backend must
not receive input that an earlier deterministic boundary was required to reject.
No rejection is allowed to publish a partial package, task, residency, batch,
submission, output, or capability grant.

## Source and build rejection

| Case | Mutation | Earliest rejection | Required evidence |
| --- | --- | --- | --- |
| 1. Missing `accelerator.kernel` requirement | Delete the requirement from `Inferenceˉaccelerated` or the root application while retaining `Addˉkernel`. | Capability/effect closure analysis. | Stable missing-capability diagnostic; no artifact. |
| 2. Hidden closure capture | Remove `copy Model` from `[copy Model, copy Context]` while the closure reads `Model`. | Closure capture analysis. | Diagnostic points to closure and outer binding. |
| 3. Invalid borrowed task capture | Capture `borrow Model` in an escaping/suspending child without a proven immobile owner. | Borrow/task analysis. | Origin, suspension, and required lifetime are bounded related locations. |
| 4. Resource move during borrow | Move/release `Residency` while `Batch` or `Submission` retains it. | Ownership analysis. | Diagnostic names owner, borrow, attempted move, and generation dependency. |
| 5. Kernel target absent | Build the kernel module only for an ordinary host scope. | Build target graph. | Unsupported target part before kernel lowering. |
| 6. Dynamic kernel substitution | Replace the statically bound kernel identity with runtime-selected text or an unbound artifact. | Package/build graph. | Unknown kernel identity/digest; no package publication. |
| 7. General sub-byte source type | Replace the nominal I4 format code with an undeclared `i4` field/type. | Parser/name/type analysis. | Unknown type; no inferred storage primitive. |
| 8. Native extension without authority | Add a vendor/native command while retaining only the four portable capabilities. | Capability/effect analysis. | Missing native-extension capability and target requirement. |

Representative source mutations:

```text
// Case 1: rejected capability closure.
// requires capability accelerator.kernel version 1;  // removed
accelerator.kernel.Addˉkernel(...);

// Case 2: rejected explicit capture.
let Work = async fn [copy Context]() -> Result<...> effects(...) {
    return await Accelerated.Run(
        Model: borrow Model,
        Context: borrow Context,
    );
};

// Case 7: no such Language 1.0 primitive.
record Invalidˉformat {
    Weight: i4;
}
```

## Package and metadata rejection

| Case | Mutation | Earliest rejection | State after rejection |
| --- | --- | --- | --- |
| 9. Missing resource | Omit any of the four bindings. | Package construction. | No application/package publication. |
| 10. Duplicate declaration binding | Bind one declaration twice. | Package construction. | No selected binding and no charge. |
| 11. Digest mismatch | Flip one weight bit without changing the declared digest. | Content admission. | No content object publication. |
| 12. Oversized content | Bind 25 tokenizer bytes or 5 weight bytes. | Package construction before retained allocation. | No package-data value. |
| 13. Wrong resource type | Bind text to a bytes declaration. | Package construction. | No package-data value. |
| 14. Tokenizer truncation | Supply fewer than 24 admitted tokenizer bytes. | Core decoder length check. | No provider discovery. |
| 15. Tokenizer special f16 | Replace a feature with zero/subnormal, infinity, or NaN encoding. | Finite-f16 decoder. | `Invalidˉtokenizer`; no provider discovery. |
| 16. Unknown token/order | Replace `01 02 03 04`, duplicate a token, or reorder it. | Tokenizer fixture validation. | `Invalidˉtokenizer`; no provider discovery. |
| 17. Model shape mismatch | Change input/output/weight geometry from 4/2/4. | Model decoder. | `Invalidˉmodel`; no shape product or allocation. |
| 18. Unknown format/layout | Change any code at offsets 20 through 23. | Model decoder. | `Invalidˉmodel`; no provider selection. |
| 19. Non-finite scale/bias | Encode NaN or infinity. | Model decoder bit classification. | `Invalidˉmodel`; no provider selection. |
| 20. Invalid scale | Encode zero or a negative row scale. | Model semantic validation. | `Invalidˉmodel`; no provider selection. |
| 21. Excessive declared budget | Request host bytes above 16,384, device bytes above 320, more than eight commands, or more than 16 diagnostics. | Model validation. | `Invalidˉmodel`; no reservation. |
| 22. Shape-product overflow | Use a future metadata shape whose element/byte product overflows its fixed width. | Metadata/operation admission checked arithmetic. | Arithmetic rejection before allocation. |

Package cases 9 through 13 occur even when malformed bytes would otherwise be
accepted by source. Runtime decoders do not replace package identity checks.

## Accelerator admission rejection

| Case | Mutation/provider state | Required outcome |
| --- | --- | --- |
| 23. No matching provider | No provider implements operation set 1 and the four exact format/layout codes. | Catalog rejection; no session. |
| 24. Software fallback forbidden | A later caller sets `Allowˉsoftware = false` and no physical provider qualifies. | Catalog rejection rather than silent software selection. |
| 25. Insufficient device ceiling | Provider cannot atomically reserve five 64-byte charged slots. | Residency rejection with requested 320 and provider limit; zero visible slots. |
| 26. Pinned-host limit | Provider needs more than the admitted 64 bytes. | Session or command rejection before pinning beyond the ceiling. |
| 27. Command capacity | Add a ninth command or set a maximum below six. | All-or-nothing command rejection; batch unchanged. |
| 28. Upload source range | Use an offset/length outside package bytes or with overflowing addition. | Add-upload rejection before batch mutation or device transfer. |
| 29. Slot length mismatch | Upload seven input bytes, five weight bytes, or a non-16-byte parameter range. | Add-upload rejection; slot remains uninitialized. |
| 30. Invalid tensor view | Construct a stride/range/shape whose last reachable element exceeds its slot. | Operation admission rejection before dispatch. |
| 31. Quantization grouping mismatch | Supply scale count/grouping inconsistent with two output rows. | Quantized-linear admission rejection. |
| 32. Aliased kernel output | Bind output to accumulator/bias storage when the interface requires non-aliasing. | Kernel admission rejection. |
| 33. Wrong kernel interface/digest | Bind the identity to a different signature or target artifact. | Package or kernel admission rejection; no dispatch. |
| 34. Illegal kernel body | Add recursion, allocation, task work, capability call, unbounded loop, unsupported barrier/atomic, or a target-only operation outside the admitted scalar interface. | Kernel verifier rejection before target publication. |
| 35. Stale generation | Restart/reset provider after selection/session/batch evidence. | Generation rejection or `Providerˉlost`; never rebind to new generation. |
| 36. Work/diagnostic limit | Command validation or provider execution would exceed 64 work units or 16 diagnostic records. | Bounded rejection/fault with no unbounded diagnostic cascade. |

## Async, completion, and output boundaries

| Case | Race or result | Required outcome |
| --- | --- | --- |
| 37. Spawn rejection | Scope is closing or child/runnable/completion/memory limit is exhausted. | Spawn returns the exact closure/captures; mapper releases them; no child. |
| 38. Cancellation before submit | Scope cancellation is observed before device acceptance. | Typed cancellation/rejection; no submission. |
| 39. Cancellation after submit | Cancellation races device work. | Remain joined until completed, cancelled, lost, or faulted; never claim rollback. |
| 40. Deadline | The one admitted timer expires. | `Deadlineˉreached`; teardown still joins and releases. |
| 41. Provider loss/reset | Provider generation disappears during wait. | `Providerˉlost`/fault; discard private output; invalidate old resources. |
| 41a. Provider evidence changes | Completed identity, generation, or attachment mode differs from the pre-session selected description. | `Providerˉevidenceˉmismatch`; reject the completed output. |
| 42. Mis-sized output | Completed terminal carries other than eight bytes. | `Invalidˉproviderˉoutput`; no accepted inference result. |
| 43. Non-finite output | Either output bits encode NaN or infinity. | `Invalidˉproviderˉoutput` regardless of tolerance. |
| 44. Numeric mismatch | Finite output exceeds absolute-plus-relative limit. | `Outputˉmismatch` with index, values, actual error, and allowed error. |
| 45. Tie | Both admitted scores compare equal. | Deterministically select lower index zero. |
| 46. Task trap | Child reaches a contained trap. | Bounded `Taskˉtrapped`; scope cancels/joins remaining work (none here). |

## Required future executable forms

When edition 1 and the accelerator contract are implemented, cases 1 through 8
become compile/build fixtures, 9 through 22 package/decoder fixtures, 23 through
36 software-provider admission fixtures, and 37 through 46 deterministic async
and differential fixtures. The physical-provider lane reuses the accepted valid
and boundary cases; it does not replace the software oracle.
