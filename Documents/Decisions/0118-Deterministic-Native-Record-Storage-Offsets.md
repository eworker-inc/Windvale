# Decision 0118: Deterministic native record-storage offsets

- Date: 2026-08-02
- Status: Qualified
- Retains: Native ABI 20, execution-context version 7, target `x86-64-wvb-baseline-v20`, the 2,048-cell physical frame ceiling, and the 2 MiB host record arena
- Refines: [Decision 0105](0105-Typed-Block-Scoped-Native-Value-Slots.md), [Decision 0115](0115-Exact-Compiler-Record-Lifetime-Pressure.md), and [Decision 0117](0117-Nominal-Native-Record-Storage-Plan.md)

## Context

Decision 0117 proved that liveness-bounded direct-record storage fits the existing native frame envelope. It measured required cells but deliberately stopped before publishing exact offsets. A selector cannot safely emit frame addresses from totals alone, and an independent decoder cannot prove that simultaneously live values remain disjoint without a canonical placement contract.

The offset contract must preserve immutable value semantics. A record result produced by an operation must coexist with every record operand consumed by that operation. A loaded record must remain unchanged if its source local is subsequently overwritten. Persistent local state must survive control-flow edges, while semantic values may reuse storage between blocks only because native machine IR independently requires an empty operand stack at every edge.

## Decision

- Extend each Stage 0 function storage plan with complete local and semantic-value offset arrays. Their lengths exactly match the function's local and semantic-value identity spaces.
- Express every published offset as an absolute 16-byte cell index in the projected function frame. `-1` means the identity owns no frame backing.
- Retain the existing ABI-20 handle area first. The canonical projected layout is:
  1. existing parameter/local handles, block-reused physical value handles, and any existing descriptor-result cell;
  2. persistent record-local backing;
  3. block-reused record-result scratch backing;
  4. one record-return destination-pointer cell when required.
- Unassigned record parameters borrow caller storage and therefore publish `-1`. Assigned record parameters and non-parameter record locals participate in the persistent interference graph; dead stored values may still require no offset.
- Allocate persistent offsets from whole-function control-flow liveness. Allocate semantic-result offsets from definition-to-last-use interference within each basic block. A result interferes with the live set before operands used by its defining operation are released.
- Use one deterministic weighted allocator for both regions: descending record width, then ascending identity, placed at the first gap not occupied by an interfering identity. Proven non-interfering identities may publish the same cells.
- Define each region's size as the greatest published end offset relative to its base. Define the projected frame end from the scratch region plus the optional record-return pointer cell.
- Continue to flag record-valued fields without admitting them. These direct-field offsets are sufficient for the exact compiler but are not a recursive layout or deep-copy contract.
- Keep the plan as immutable Stage 0 evidence. It is not serialized into WVB, WVO, or native fragments and does not change code selection, ABI 20, or execution behavior.

## Evidence

The canonical 328-function compiler publishes complete maps for all local and semantic-value identities. Independent test code reconstructs local CFG liveness and block-local result lifetimes, then proves:

- every non-record or borrowed-only identity has offset `-1`;
- every owned range lies completely inside its declared persistent or scratch region;
- every simultaneously live pair is disjoint;
- result storage coexists with operands consumed by the same operation;
- region sizes equal their greatest published range end;
- persistent, scratch, optional return-pointer, and projected-frame boundaries are contiguous and exact;
- every record semantic value is defined exactly once and released before its block edge.

Repeated planning over separately lowered native IR produces byte-for-byte equal offset arrays. A test-only canonical digest writes function identity/layout integers and both complete offset arrays in little-endian order. The exact compiler map has SHA-256 `aff287fba46a840e454e4cc7bf4751d3152474caf09331a526f3730ba280816e`.

Static weighted allocation introduces no fragmentation for the exact compiler: scratch storage remains exactly the 7,463 peak-live field cells summed across functions. Persistent storage remains 9,291 cells summed across functions, and `Compilerˉsourceˉwirˉcompileˉblock` remains the largest projected frame at 1,489 cells: 1,178 existing, 196 persistent, 114 scratch, and one record-return pointer. Every function remains within the unchanged 2,048-cell ceiling.

The exact selected compiler remains 4,556,121 bytes with SHA-256 `8e74707df03a535e3ef68cfcfc8da6fa68fda29ccf4344e272fc50c8a5845bab` and passes the existing independent ABI-20 fragment decoder. Focused Windows verification passes the small nominal record plan and exact compiler plan. Windows Development completes a zero-warning Release build, all 69 regular Seed tests, and all 25 bounded OS tests in 81.2 seconds wall time.

Exact implementation commit `060cf481d9783170cde4cf2cdd7ddab3cee1028e` passes GitHub [Verify run 30774669075](https://github.com/eworker-inc/Windvale/actions/runs/30774669075). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 70 Seed tests, all 25 OS tests, and the complete native CLI gate. The nominal record case takes 93 ms on Windows and 33 ms on Linux; the exact compiler-plan case takes 2.637 and 2.174 seconds; the retained full-bootstrap boundary takes 1.037 seconds and 623 ms. Windows Seed takes 252.041 seconds with a 186.165-second golden contract; Linux Seed takes 176.775 seconds with a 128.564-second golden contract. The complete host jobs finish in 9m15s and 6m31s. QEMU is not rerun because ABI 20, every generated machine byte, and all OS source/artifact inputs remain unchanged.

## Consequences

ABI 21 can now consume one canonical offset source rather than reimplementing allocation inside selection. The next selector slice can derive record addresses from verified frame-cell constants, copy direct fields, pass borrowed parameter pointers, and use caller-owned return destinations. The independent decoder can reconstruct the same plan from native machine IR and reject changed offsets, overlaps, widths, or frame bounds.

This decision does not reclaim one runtime record, change the current `WVR3017` boundary, complete native compiler reproduction, or alter the Windvale OS image. ABI 20 remains the executing contract until the selector, decoder, host runtime, and OS consumer are advanced and qualified together.

The exact maps are Stage 0 replacement evidence. Long-term ownership belongs in the Windvale-native compiler/backend; exposing offsets now makes that transfer explicit rather than embedding hidden C# selector state.

## Reconsider when

- ABI-21 code selection requires a different frame region order or address convention.
- Independent decoding cannot reconstruct the maps without trusting selected bytes.
- An assigned record parameter or new compiler control-flow shape changes interference.
- Nested records require recursive backing, deep copies, or a separately owned representation.
- Register allocation or stack-frame compaction introduces a smaller verified representation without weakening immutable value semantics.
