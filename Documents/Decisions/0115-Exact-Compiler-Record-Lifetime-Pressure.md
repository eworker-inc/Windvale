# Decision 0115: Exact-compiler record-lifetime pressure

- Date: 2026-08-02
- Status: Qualified at exact integration commit `05e5ef1069eff5283f4f1c46923f40905e04c5db`
- Retains: Native ABI 20, execution-context version 7, target `x86-64-wvb-baseline-v20`, and the 2 MiB host record arena
- Refines: [Decision 0058](0058-Reproducible-Compiler-Bootstrap-Convergence.md), [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), and [Decision 0112](0112-Bounded-Exact-Compiler-Record-Arena.md)

## Context

Decision 0112 measured and closed the smallest exact-compiler execution boundary. The native compiler consumes 1,480,096 record bytes while compiling the 815-byte function-only fixture, so a measured 2 MiB host arena is sufficient for that useful source-to-WVB path.

The next retirement proof is native Stage 1 to Stage 2 reproduction. Its canonical input is one tool root plus eleven ordered dependencies: 12 modules and 677,073 source bytes. The independently qualified bytecode path executes 6,700,562,174 verified instructions and reproduces a byte-identical 599,868-byte compiler with SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.

Running that same inventory through ABI 20 exposed a different scale of record pressure. The ordinary 2 MiB arena reaches `WVR3017` before stdout, diagnostics, or output publication. Diagnostic-only runs then exhausted 64 MiB after using all 67,108,864 bytes and exhausted 256 MiB after using 268,435,440 of 268,435,456 bytes. The temporary capacity override used to obtain those measurements is not a product feature and is not retained.

An opt-in semantic profile of the successful reference-runtime Stage 1 execution attributes constructed record fields to the function executing each `record.create`. Its ten largest counts are:

| Constructed fields | Function |
| ---: | --- |
| 46,621,918 | `Compilerˉsourceˉtokenˉmake` |
| 6,663,870 | `Compilerˉbodyˉexpressionˉmake` |
| 5,915,700 | `Compilerˉbodyˉexpressionˉfailure` |
| 3,022,464 | `Compilerˉparseˉstepˉvalid` |
| 2,720,986 | `Compilerˉbodyˉparseˉname` |
| 2,624,745 | `Compilerˉbodyˉstepˉvalid` |
| 2,135,159 | `Compilerˉsourceˉsymbolsˉbindˉtype` |
| 1,423,334 | `Compilerˉbodyˉstatementˉmake` |
| 1,103,096 | `Compilerˉsourceˉwirˉvalueˉfrom` |
| 1,038,208 | `Compilerˉsourceˉwirˉemit` |

Those ten functions construct 73,269,480 fields. The leading 40 functions construct 77,821,091 fields, which requires at least 1,245,137,456 bytes under ABI 20's 16-byte cell-per-field monotonic representation. `Compilerˉsourceˉtokenˉmake` alone constructs 3,330,137 fourteen-field tokens, corresponding to 745,950,688 current native bytes. The pressure is therefore systemic across lexical, parsing, binding, and WIR value construction rather than one unexpectedly small capacity or one isolated constructor.

The exact compiler contains 49 record types and 22 enums. Its widest record has 34 fields, and none of its record declarations has a record-valued field. General Windvale programs may contain nested records, so this exact-workload fact may bound an initial implementation but must not redefine the language.

The current native machine IR also erases information needed for safe reusable record storage. `Nativeˉfunction` retains only broad `Nativeˉvalueˉtype` entries for parameters, locals, and semantic values. Nominal record indices survive temporarily in the lowering stack and selected record operations, but not across every local, call, result, and physical-value contract. A frame-storage design cannot derive correct record widths and copies from that incomplete model.

## Decision

- Keep the ordinary Windows/Linux host record arena at 2 MiB. Do not represent full compiler bootstrap as another fixed-capacity increase.
- Add opt-in per-function record-field construction profiling to the Stage 0 reference runtime and CLI. `Readˉfunctionˉrecordˉfields` returns only positive counts, ordered by descending field count and then function index. `--report-function-record-fields` writes deterministic `Function record-fields=<count> index=<index> name=<name>` lines to standard error. Collection allocates no counter array and produces no output unless requested, and counters reset on every run.
- Count semantic declared fields, not ABI bytes. This keeps the observation meaningful if a later ABI changes field layout, copies, reuse, or allocation.
- Retain a fast conformance case for the exact 12-module native inventory under the ordinary 2 MiB capacity. It must reach `WVR3017` without stdout, diagnostics, or an output file. This is a pinned next boundary, not a passing native-bootstrap claim.
- Preserve the exact compiler's record-shape facts in that test: 49 records, 22 enums, maximum width 34 fields, and no record-valued fields.
- Make the next implementation slice preserve nominal value shapes throughout native machine IR, then derive per-function persistent and block-scoped record-storage requirements for the exact compiler. This metadata step must not change selected bytes, ABI 20, or runtime behavior.
- Use that derived evidence to decide the first reclaiming/value-storage ABI. The leading candidate is bounded block-scoped storage for immutable record temporaries, explicit persistent local storage, and caller-owned record return storage. Nested records, copies, calls, early returns, and failure paths require explicit contracts before adoption.
- Do not introduce a general garbage collector, reference counting, compiler-specific arena reset, or record mutation without separate ownership, roots, failure, and conformance evidence.

## Evidence

The reference profiler completed the canonical Stage 1 run with result zero and the exact 6,700,562,174 instruction count. It produced the exact success report, no diagnostic, and a byte-identical 599,868-byte Stage 2 compiler with the retained SHA-256 above. Profiling does not alter the produced bytes.

The focused profiler test proves disabled behavior, deterministic per-function counts, tie ordering by function index, and reset rather than accumulation across repeated runs. Windows and Linux CLI verifiers use the existing composed record example to require exactly one report line: `Function record-fields=2 index=2 name=Compositionˉmake`.

The focused full-inventory test executes the exact compiler with the normal native executor, reaches the expected bounded failure, and verifies that neither output stream nor the target module is published.

Exact implementation commit `a759b86c7735e6a2a94b24efcd9f48af52e8e6d2`, followed only by the decision-number repair in exact integration commit `05e5ef1069eff5283f4f1c46923f40905e04c5db`, passes GitHub [Verify run 30771491421](https://github.com/eworker-inc/Windvale/actions/runs/30771491421). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 70 Seed tests, all 25 OS tests, and the complete native CLI gate. The bounded full-bootstrap case takes 897 ms on Windows and 678 ms on Linux. Windows Seed takes 238.394 seconds with a 172.632-second golden contract; Linux Seed takes 199.414 seconds with a 147.241-second golden contract. The complete jobs finish in 8m48s and 7m29s respectively.

No native ABI, selected machine byte, WVB/WVO serialization, OS source, or guest artifact changes in this slice. QEMU was not rerun because every native/OS artifact input and identity remains unchanged.

## Consequences

Native compilation of a useful individual source remains qualified under Decision 0112. Full native compiler reproduction remains intentionally incomplete, but its blocker is now measured, reproducible, and represented by a normal bounded test.

The profiling seam is diagnostic Stage 0 evidence rather than a portable Windvale runtime API. It identifies where semantic values are constructed without making current 16-byte native cells part of source semantics.

The next compiler work begins with information preservation and storage measurement, not a prematurely selected allocator. Because the exact compiler has no nested record fields, a first bounded storage implementation may be materially smaller than a general collector, while the existing arena can remain the explicit path for shapes not yet admitted by a future ABI.

This decision does not prove native Stage 1 to Stage 2 reproduction, a reclaiming allocator, standalone PE/COFF or ELF compiler tools, exact-compiler WVO/AOT publication, or the .NET-retirement gate.

## Reconsider when

- Nominal-shape-preserving native IR shows that bounded frame storage exceeds existing frame or stack contracts.
- Liveness proves a smaller safe region boundary than block-scoped storage.
- Nested-record or long-lived graph workloads require tracing, ownership counting, or another general memory model.
- Record identity or mutation is proposed for Windvale source semantics.
- Full native bootstrap reaches a different bounded resource before record pressure is resolved.
