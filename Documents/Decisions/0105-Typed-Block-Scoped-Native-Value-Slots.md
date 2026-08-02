# Decision 0105: Typed block-scoped native value slots

- Date: 2026-08-02
- Status: Qualified
- Advances: Native ABI 18 and kernel memory `WVKMEM10`
- Refines: [Decision 0061](0061-Typed-Native-Blocks-And-Forward-Control-Flow.md), [Decision 0099](0099-Bounded-Native-Frame-Admission.md), and [Decision 0103](0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md)

## Context

The ABI-17 backend assigned one 16-byte physical frame cell to every static machine-IR result for the lifetime of its function. This was simple, but the machine IR already requires an empty operand stack at every basic-block edge. A result produced in one block therefore cannot be consumed in another block, yet its cell could not be reused.

Probe 32 made the cost concrete. The Windvale bytecode interpreter's `Executeˉmain` used 663 locals plus 1,236 semantic values, and the complete deepest native call path was conservatively sized at 58,800 bytes. Its 577,140-byte WVO required 141 client code pages and a 15-page stack. The same pressure stopped exact native-compiler preflight at the first disallowed combined slot, 2,049, in `Compilerˉbodyˉparseˉprimary`. Increasing the 2,048-cell ceiling again would preserve the cause and enlarge every consumer.

## Decision

- Preserve globally canonical semantic value IDs and types in native machine IR. Add a separate canonical physical-slot index for each value and an explicit physical value-slot count.
- Reuse physical value slots only across basic blocks. Within one block, every result of the same exact `Nativeˉvalueˉtype` receives a distinct ordinal cell. Each type owns a disjoint range sized to that type's maximum simultaneous block count. This keeps the independent fragment decoder's scalar/descriptor reasoning stable without pretending to perform intra-block liveness or register allocation.
- Retain the empty-stack control-edge rule and strengthen machine-IR verification so every operand must reference a value produced in its current block. Recompute the complete typed slot map independently and require byte-for-byte canonical equality before selection.
- Bound a function to 100,000 semantic value identifiers, matching the verified WVB instruction ceiling, while retaining ABI 17's hard limit of 2,048 physical 16-byte frame cells. A frame contains parameters/locals, the canonical typed block-slot ranges, and a hidden result cell only for borrowed text/bytes descriptor returns.
- Permit the byte-level fragment verifier to observe a descriptor cell being defined again in a later block. Reject aliasing with the operation's current descriptor inputs, and replace or clear static-descriptor provenance whenever a reused cell receives a new value.
- Advance the experimental target to `x86-64-wvb-baseline-v18` and ABI 18. WVB 1.6, WVO 1.0, execution context 7, service table 5, the 64-parameter convention, result representation, and the 2,048-cell physical ceiling remain unchanged. WVO 1.0 still does not serialize ABI/service metadata.
- Make the OS stack proof consume physical value slots and the backend's actual descriptor hidden-result rule. Probe 32's program, interpreter profile, protected-process format, paging format, firmware format, resource contracts, instruction count, call depth, result, and serial behavior remain unchanged.
- Advance kernel memory to `WVKMEM10`: the compact client uses 102 code pages, six stack pages, one data page, and four root/table pages, so its reclaimable allocation is 113 pages and the complete bounded kernel arena is 134 pages.

## Initial evidence

The same 815-byte `Function-Only.wv` WVB still executes 199 guest instructions to result `6`. `Executeˉmain` retains 1,236 semantic values but now needs 82 physical value cells; its complete frame falls from the previously recorded 1,900-cell conservative figure to 745 actual cells. The independently derived deepest path is `Main -> Executeˉmain -> Executeˉprobe -> Isˉinstructionˉboundary -> Instructionˉbytes` and consumes exactly 23,824 bytes including return addresses and the entry shim's saved `r15`. Six pages (24,576 bytes) are minimal; five pages are insufficient.

| Artifact or bound | ABI 17 / Probe 32 | ABI 18 qualified | Change |
| --- | ---: | ---: | ---: |
| Interpreter WVO | 577,140 bytes | 418,372 bytes | -158,768 (-27.5%) |
| Interpreter client image | 576,541 bytes | 417,773 bytes | -158,768 (-27.5%) |
| Largest interpreter frame | 1,900 recorded cells | 745 actual cells | -1,155 (-60.8%) |
| Deepest native stack | 58,800 bytes | 23,824 bytes | -34,976 (-59.5%) |
| Client code / stack pages | 141 / 15 | 102 / 6 | -39 / -9 |
| Client allocation | 161 pages | 113 pages | -48 (-29.8%) |
| Kernel arena | 182 pages | 134 pages | -48 (-26.4%) |

The compact interpreter WVO is SHA-256 `c712a8cf7aa674b89e01e8fbc1632eeb6414fd1fde0bc4ead763e45c38037bb1`. The four deterministic Probe-32-compatible firmware images are:

| Scenario | Bytes | SHA-256 | Expected exit |
| --- | ---: | --- | ---: |
| Normal | 531,456 | `b8f0e656066b1e4f28edc4124eca6eea18130a0d6c0f4a9018e8ae817a0fa985` | 0 |
| Invalid opcode | 531,456 | `0322ce3d3a9fecfa5c84809d8594f4f3ea643aaff2776f8d25668f1d723b9b54` | 3 |
| General protection | 531,456 | `1a0bd9f37c595d4170bd05fe83cc05dc344d2223674a01812f253ceb77893e40` | 3 |
| Contained user fault | 531,968 | `68319856b2913b3c857012d3fd38f147cf2a2307afacc9ffc8c8a33c005d0cf9` | 0 |

Focused Windows verification passes all ten native compiler cases and all four pinned-QEMU 11.0/Q35/TCG scenarios. The complete local Standard gate passes a zero-warning Release build, all 68 Seed tests including the golden cross-host contract, and all 25 OS tests. Exact compiler preflight now clears the former slot-2,049 failure and reaches `WVN2003` in `Compilerˉcompileˉsourceˉwvb` for the next unsupported opcode, `Bytesˉfromˉu8`.

Exact implementation commit `484c228c666e57bd6d3dc67d60c855f528cff3bf` passes GitHub [Verify run 30762156220](https://github.com/eworker-inc/Windvale/actions/runs/30762156220). Windows and digest-pinned Debian 12 each pass all 68 Seed tests, all 25 OS tests, and the complete native CLI qualification gate. Seed elapsed time is 237.931 seconds on Windows and 196.849 seconds on Debian. This qualifies ABI 18, `WVKMEM10`, Decision 0105, and compacted Probe 32 as the latest cross-host native/OS baseline. QEMU execution remains Windows-only evidence.

## Consequences

Large branch-heavy functions no longer pay permanent stack and initialization-code cost for every value ever produced. The change benefits the shared Windows/Linux JIT and WVO/AOT path, Windvale OS, and native compiler progress without creating a second compiler or interpreter.

Semantic value identity remains explicit evidence rather than being conflated with storage. Exact-type ranges deliberately use slightly more cells than an untyped maximum block width, but preserve independently checkable descriptor safety and deterministic output.

This is not register allocation, intra-block liveness, phi nodes, native compiler execution, support for `Bytesˉfromˉu8`, an independently loadable service-bearing WVO, a stable public ABI, or .NET retirement. The Stage 0 compiler/verifier and OS image builders remain named replacement seams.

## Reconsider when

- Intra-block value pressure, rather than accumulated cross-block values, reaches the 2,048-cell physical limit.
- A register allocator can publish independently verifiable spill, initialization, descriptor, and safe-point evidence.
- Cross-block values or phi nodes become necessary and invalidate the empty-stack ownership boundary.
- A future WVO revision serializes enough ABI and service metadata for independent loading.
