# Library development history through September 26, 2026

> Status: Historical checkpoint notes
> Authority: Informative; linked evidence owns exact claims
> Last reviewed: 2026-09-26

This snapshot preserves detailed checkpoint prose removed from active Progress
and the compiler/tools/libraries completion plan during simplification. Statements
about pending work describe their recorded checkpoints. Use [Progress](Progress.md)
for present standing and the [completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md)
for current consumer blockers. This archival move changes no qualification claim.

## WVB version scopes

Two active tracks intentionally use different bytecode generations:

- Windvale Seed and its frozen bootstrap/recovery path emit and consume their
  qualified WVB 1.11 contract.
- The evolving Language 1.0 compiler uses later versioned WVB contracts, with
  the executable structured-task slice at WVB 1.32 and the contained unsafe
  memory operations at WVB 1.37. The qualified source compiler publishes the
  authenticated call as WVB 1.38, the complete verifier admits its
  exact registered binding, the scalar provider executes it against private
  logical heap state, and the native x64 lowerer executes the same exact binding
  through its typed SysV ABI provider on Windows and Linux. WebAssembly and
  other native targets retain narrower declared boundaries.
- The Libraries 1.0 track has a source-publication candidate at WVB 1.39 for
  immutable Option/Result payload borrowing. Its source writer and bounded
  independent reader pass on Windows. The complete verifier now applies semantic,
  typed-stack, and lifetime checks to 1.39. The source-built host scalar runner
  executes the published fixture on both hosts; installed tools and other targets
  remain narrower. The verifier's small typed-directory component now
  preserves distinct borrowed payload identities and bounds-checks shape and
  instruction decoding. The control-phase component also checks that each
  payload owner is initialized on every path and cannot be overwritten or
  consumed after borrowing, including loops. Its isolated call checker now
  matches exact borrowed parameters and permits by-value reads only for payloads
  proved safe to copy or share. The typed-stack pass preserves borrowed identity
  through the published record/u32 fixture's locals, projections, and helpers.
  Its loan pass tracks origins through branches and loops, including values
  waiting on the operand stack. Owner changes invalidate later borrowed reads;
  changes after the last use and fresh reborrows remain valid. Array, Sequence,
  and callable composition now has focused evidence. Source read-through also
  classifies only exact WVFT-backed callable shapes as owned and rejects absent,
  truncated, or out-of-range callable evidence. The candidate bytecode checker
  now rejects borrowed callable copies while preserving exact invocation,
  including array and Option projections; its existing 217-group selector passes
  on Windows in the [callable reconciliation checkpoint](../Evidence/2026-09-07-Foundation-Borrow-Callable-Reconciliation.json).
  Runtime leases connect dispatch, locals, calls, collector roots, and cleanup.
  The [owned-payload checkpoint](../Evidence/2026-09-08-Foundation-Owned-Payload-Paired-Host.json)
  passes 305 component groups on Windows and Debian with identical bytecode.
  It permits borrowed forwarding of an array containing owned Vectors while
  rejecting copying, consuming calls, and Vector extraction. Wider payload
  execution and real-consumer integration remain pending. The subsequent
  [complete-verifier checkpoint](../Evidence/2026-09-08-Foundation-Complete-Verifier.json)
  removes the blanket version rejection and checks phase results through the
  existing metadata and typed-stack matrices: 305 groups pass on both hosts.
  The [runtime checkpoint](../Evidence/2026-09-08-Foundation-Borrow-Runtime-Execution.json)
  now returns 42 from the published three-projection fixture on Windows and
  Debian, rejects nine unsafe or malformed variants before execution, and
  passes three earlier-version aggregate Sequence lifetime workloads per host.
  The [paired fresh-source checkpoint](../Evidence/2026-09-08-Foundation-Borrow-Fresh-Paired-Host.json)
  passes 371 selected groups on each host: record/u32 and allocated-text borrows
  compile identically and return 42. Seven existing call-site cases also pass
  on both hosts after correcting the owned-builder rejection fixture. A
  maintained real consumer remains pending.

The Language 1.0 track does not silently redefine the frozen Seed recovery
contract. A current document must name the track when a WVB version matters.

## Earlier development feedback checkpoints

Ordinary front-end verification now shares exact build products and selects
affected test-project inputs while executing the behaviors afresh. The Windows
checkpoint passed all 329 development claims in 29.94 seconds warm, versus
225.71 seconds while creating its project/package checkpoints. A changed parser
defect was rebuilt and rejected in 14.63 seconds; restoring it passed the focused
254-claim selection in 2.10 seconds. These are development observations, not a
clean-machine or paired-host qualification claim. See the
[focused evidence](../Evidence/2026-09-04-Front-End-Development-Product-Reuse.json).

Foundation component development uses small current-source WV packages through
one existing owner. The
[planner's 16 cases](../Evidence/2026-09-04-Foundation-Borrow-Plan-Development.json)
and [typed directory's 24 cases](../Evidence/2026-09-04-Wvb-Typed-Directory-Development.json)
have separate focused selectors and changed-source rejection evidence.

The [cross-call publication evidence](../Evidence/2026-09-04-Foundation-Borrow-Cross-Call-Publication.json)
records 39 passing Windows cases and the separate cold-construction cost; it is
not clean-machine or cross-host qualification.

The `--foundation-borrow-owners` selector combines
[18 owner-flow groups](../Evidence/2026-09-04-Foundation-Owner-Flow-Development.json),
[18 direct-call groups](../Evidence/2026-09-04-Foundation-Borrow-Call-Development.json),
[37 semantic metadata groups](../Evidence/2026-09-04-Foundation-Borrow-Metadata-Development.json),
[15 typed-stack groups](../Evidence/2026-09-04-Foundation-Borrow-Stack-Development.json),
[28 lifetime groups](../Evidence/2026-09-05-Foundation-Borrow-Lifetime-Development.json),
and [34 composition groups](../Evidence/2026-09-05-Foundation-Borrow-Composition-Development.json).
They consume the actual published signatures and projections, check exact
nominal identities, reject every truncated fixture prefix, and preserve an
earlier-bytecode regression. Type and local directories avoid repeated scans;
cached products keep fresh execution separate from construction. Bounded host
minor-39 execution now has paired-host evidence. Full-verifier source
changes retain broader routing.

## Delivered package consumer checkpoints

The 15 September implementation connects Project 4 to the current-source native
publisher. Nine focused Windows cases pass, including replacement, deterministic
bytes, bad source-lock rejection, malformed WVB, and resource aliases. Five
native publisher cases also pass on Debian using a Windows-constructed Linux
executable. This is not independent Linux construction or fault-injection
qualification; exact inputs and exclusions are in the
[publication evidence](../Evidence/2026-09-15-Project4-Native-Publication.json).

The earlier native-lowering blocker was resolved for the selected current
product. The approved current-lowerer rebuild
completed in 2 minutes 10 seconds; 43 focused Windows cases passed. Both the
canonical parser and package-lock tests now return 42 on Windows and Debian
using explicitly selected native lowering and image-mode packaging. The earlier
package-lock rejection came from the older lowerer. See the
[current-lowerer execution evidence](../Evidence/2026-09-15-Current-Lowerer-Package-Execution.json)
for exact identities, runtime profiles, cache reuse, and host limits.

The package-lock borrow rejection is resolved in the delivered source batch.
Its large scanner exceeded the documented per-function borrow-proof bounds:
281 blocks and 87 slots versus the 64/64 limits. The repeated digest-and-size
validation now belongs to one private lock-content reader, whose borrowed match
uses seven blocks and six slots. The canonical decimal parser still returns
`Option<u64>`; that private extraction changes no compiler limit, public API,
or wire format. Forty-two
new content cases plus the existing lock tests pass on Windows and Debian, and
ordinary Project 4 build/replacement produces identical WVB bytes. See the
[borrowed lock-reader evidence](../Evidence/2026-09-15-Package-Lock-Borrowed-Content.json).
