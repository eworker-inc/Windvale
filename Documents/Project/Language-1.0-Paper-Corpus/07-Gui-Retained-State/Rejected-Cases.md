# Workload 7 rejected and boundary cases

## Source and ownership rejection

| Case | Mutation | Earliest rejecting owner | Required result |
| --- | --- | --- | --- |
| 1. Implicit retained-state global | Read or mutate `Stateˉvalue` without a parameter/capture. | Name/capture analysis. | Unknown binding or missing explicit capture; no implicit UI-thread singleton. |
| 2. Mutable state captured by child | Change the layout closure to `[borrow mut Stateˉvalue]`. | Borrow/task analysis. | Diagnostic names state owner, capture, suspension/task lifetime, and parent mutation. |
| 3. Detached background work | Drop the task handle or return before join. | Task ownership/scope analysis. | Unconsumed handle / child cannot outlive scope. |
| 4. Capability hidden in child | Call display/input/timer from the child without matching module/effect closure. | Capability/effect analysis. | Stable missing capability/effect diagnostic. |
| 5. Hosted dependency in Core | Import `Retainedˉguiˉhostˉtypes` from layout/render/state. | Profile analysis. | Core-to-Hosted dependency rejected. |
| 6. Provider call without `await` | Remove `await` from publish/input/timer. | Async/type/effect analysis. | Async value mismatch plus missing suspension site. |
| 7. Fabricated operation context | Construct or deserialize context fields. | Type/visibility analysis. | Opaque construction rejected. |
| 8. General exception/retry loop | Catch provider failure and retry publication implicitly. | Grammar/name analysis or review contract. | No catch syntax; retry lacks a permitted idempotency contract. |

Representative invalid capture:

```text
let Work = async fn [borrow mut Stateˉvalue]() effects() {
    Stateˉvalue.Counter = Stateˉvalue.Counter + 1u64;
};
Task.Spawn(Scope: borrow mut Scope, Work: Work);
```

The parent continues to process input, so this would permit two mutable paths.
The compiler rejects it; a host scheduler promising single-thread execution does
not weaken the source alias rule.

## Package/theme rejection

| Case | Mutation | Required result |
| --- | --- | --- |
| 9. Missing theme binding | Omit `Theme`. | Package rejection before source start. |
| 10. Wrong resource type | Bind text instead of bytes. | Package type rejection. |
| 11. Oversized payload | Bind 65 bytes. | Package maximum rejection before retention. |
| 12. Wrong exact length | Bind 35 or 37 bytes within the maximum. | `Wrongˉlength` before indexed reads. |
| 13. Wrong magic/version | Change one byte of `WVTHEME1`. | Exact `Wrongˉmagic` byte offset/values. |
| 14. CRLF or missing LF | Change any separator. | Exact `Wrongˉseparator` offset/observed byte. |
| 15. Uppercase/non-hex color | Use `F`, `g`, space, sign, or locale digit. | Exact `Invalidˉhex`; lowercase ASCII only. |
| 16. Digest mismatch | Change bytes without the binding digest. | Content admission rejects before decode. |

## Arena, map, and state boundaries

| Case | Mutation | Required result/state |
| --- | --- | --- |
| 17. Widget maximum below four | Set maximum to 3. | Invalid limit; no arena/map publication. |
| 18. Arena capacity | Insert a fifth widget against maximum 4. | Capacity failure returns the proposed widget; existing nodes unchanged. |
| 19. Duplicate logical identity | Insert identity 3 again. | `Duplicateˉidentity(3)`; map unchanged and returned key/handle released by caller. |
| 20. Missing logical identity | Look up identity 9. | `Missingˉidentity(9)`; no arena access. |
| 21. Wrong-arena handle | Store a handle from another arena. | `Wrongˉarena`; no node read/mutation. |
| 22. Out-of-range handle | Corrupt the slot index. | `Slotˉoutˉofˉrange`; no access. |
| 23. Stale status handle | Read identity 4 after removal. | Exact stale generation; never aliases a reused slot. |
| 24. Retired slot | Force generation exhaustion in a bounded model. | `Retired`; slot is never reused. |
| 25. Replace invalid handle | Call `Arenaˉreplace` with any invalid handle. | Arena unchanged; proposed widget returned in the failure. |
| 26. Remove invalid handle | Call `Arenaˉremove` with any invalid handle. | Arena unchanged; no value returned. |
| 27. Partial layout update attempt | Make the fourth layout handle stale. | Prevalidation returns `Staleˉwidget` before replacing the first node. |
| 28. Status-presence mismatch | Supply status layout for absent state or vice versa. | Typed mismatch; no widget replacement. |
| 29. Surface below 32 | Resize to width/height 31. | Typed invalid-surface failure; state/event sequence unchanged. |
| 30. Surface above admitted maximum | Resize beyond the exact limit. | Same all-or-nothing rejection. |
| 31. Pointer outside action | Press any non-action point. | Accepted `Unchanged`; event sequence advances once, counter does not. |
| 32. Second status removal | Remove after already absent. | Accepted `Unchanged`; event sequence advances once. |

The identity map deliberately retains removed identities as tombstones. This is
not a stale-pointer bug: every lookup revalidates through the arena before
borrowing. A mutation that reads the map handle without `Arenaˉvalidate` violates
the Foundation precondition and traps before memory access.

## Event/task boundaries

| Case | Mutation | Required result |
| --- | --- | --- |
| 33. Zero event maximum | Configure zero. | Configuration rejection before provider call. |
| 34. Oversized batch | Provider returns `Maximum + 1`. | Reject before index zero; no batch event applied. |
| 35. Wrong batch sequence | Return sequence 0 or 2 when 1 expected. | Exact expected/observed failure; no event applied. |
| 36. Empty batches to limit | Never deliver close. | Bounded batch-limit failure; child cancelled/joined on scope exit. |
| 37. Event after close | Append an event after `Close`. | Prior events retained, later event rejected, no final frame. |
| 38. Spawn rejected before acceptance | Close scope or exhaust child/queue state. | Failure returns exact closure; snapshot remains owned/released locally. |
| 39. Stale background layout | Mutate layout generation after spawn. | `Staleˉsnapshot`; no partial widget update. |
| 40. Background typed failure | Invalid snapshot surface. | `Applicationˉfailure.State`; scope joins. |
| 41. Background cancellation/deadline | Cancel or expire context. | Distinct typed task outcome; never provider restart. |
| 42. Task runtime loss/restart | Change task-runtime generation. | Exact runtime generations; distinct from GUI provider generations. |
| 43. Contained trap | Runtime contains child trap identity. | Bounded `Backgroundˉtrapped`; no catchable exception/stack trace. |

## Render and provider boundaries

| Case | Mutation | Required result |
| --- | --- | --- |
| 44. Frame work limit | Pixel count exceeds admitted work. | Reject before byte-builder allocation. |
| 45. Frame byte limit | RGBA length exceeds maximum. | Reject before allocation/publication. |
| 46. Frame arithmetic overflow | Make `Pixels * 4` exceed u64 in a malformed direct call. | Typed overflow before multiplication. |
| 47. Allocation failure | Frame/state/map/task budget unavailable. | Exact allocation/task failure; prior owners release. |
| 48. Wrong frame geometry | Provider observes wrong stride/length/dimensions. | Rejected with full frame returned; zero publication. |
| 49. Short accepted count | Receipt count differs from full frame length. | Provider `Invalidˉresponse`; no valid receipt. |
| 50. Publish rejected | Cancellation/revocation before dispatch. | Zero publication, frame returned then locally released, no retry. |
| 51. Publish indeterminate | Loss/restart after dispatch. | Exact small failure, no frame/replay claim. |
| 52. Input provider restart | Input generation changes. | Exact expected/observed provider failure; task still joined. |
| 53. Timer provider restart | Timer generation changes. | Same for timer domain; no fabricated tick. |
| 54. Surface provider restart | Surface generation changes. | Same for surface domain; old endpoint never rebinds. |

## Verification shape

Cases 1–8 are compile/profile/effect/ownership fixtures; 9–16 package/decoder;
17–32 collection/state; 33–43 task/event; and 44–54 render/provider. Executable
implementation should add valid, boundary, malformed, cleanup, deterministic
schedule, and Windows/Linux differential evidence under focused owners rather
than an unfiltered qualification gate.
