# Seed verification throughput

## Purpose

Windvale keeps the complete cross-host qualification gate, but ordinary development should pay only for the narrowest evidence relevant to the current edit. Verification therefore has four explicit levels rather than one expensive all-purpose command.

## Levels

- `Fast` builds once and runs an explicit selection of one or more test areas, a case-insensitive displayed-name substring, or their intersection. It may fail fast and may write a local timing report. It does not produce conformance evidence.
- `Development` builds once and runs every regular in-process test. It excludes only the qualification-only golden cross-host contract, does not write a conformance report, and is the broad check for a coherent development batch.
- `Standard` builds once, runs all registered in-process conformance tests, and writes the normal host report. It stops before native CLI qualification.
- `Qualification` is the default and remains the milestone gate. It adds every native CLI, hosted-boundary, deterministic-artifact, and failure-preservation check.

Qualification builds the CLI once and invokes the resulting `windvale.dll` directly in each separate process. This preserves command parsing, process exit codes, native file behavior, capability boundaries, and output checks while removing repeated `dotnet run` project evaluation.

## Development selection

The conformance runner assigns every registered test to one or more stable areas: `assembler`, `bytecode`, `compiler`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. Repeated area selections form a union. Supplying a displayed-name filter as well intersects that filter with the selected areas. The runner exposes the canonical names through `--list-areas`.

`Tools/Verify/Verify-Changed.ps1` maps changed paths to those areas and invokes the Fast verifier. Compiler, runtime, bytecode, assembler, object-model, linker, and Foundation paths select their owned areas. Specification paths select the areas that implement the named contract. Tests, build configuration, workflows, verification tooling, broad Seed examples, unknown specifications, and unrecognized implementation paths fail closed to all areas. Documentation outside `Specifications/`, the root license, and editor-only changes do not run Seed; editor-relevant paths still run the editor verifier.

Changed-file selection is an inner-loop optimization only. It never writes a conformance report or supplies Standard or Qualification evidence.

`Development` is deliberately broader than changed-file or Fast selection. It selects every area except `golden`, so a test shared by several regular areas still runs once. It does not cache generated artifacts, runtime outcomes, or passing assertions. Standard and Qualification continue to execute the golden contract cold, preserving the deterministic-byte, real-closure, and cross-host evidence boundary.

The first Windows x64 validation on 2026-07-31 passed all 47 regular tests in 65.270 seconds. The immediately following cold Standard validation passed all 48 tests in 224.121 seconds, including 171.206 seconds in the golden contract. Development therefore removed 158.851 seconds, or 70.9% of suite elapsed time, from the broad development check without removing any test from Standard or Qualification.

## Golden phase telemetry

When `--timing-report` is active and the golden test runs, the diagnostic report includes canonical phases for artifact compilation, baseline runtime execution, parser closures, source-set, graph, symbol, and binding closures, inspection tools, the Windvale assembler, the Windvale linker, and final contract assembly. Each phase records elapsed milliseconds, aggregated executed VM instructions, bytes allocated by the executing managed thread, and generation 0, 1, and 2 collection-count deltas. The phase order is validated before the report is written.

Allocation values are current-thread observations and collection counts are process-wide observations. They are suitable for comparing repeated runs on a controlled host, not for cross-host conformance. Phase telemetry is never serialized into the conformance report and does not change its portable contract.

The first telemetry-enabled Windows x64 Standard run on 2026-07-30 completed all 47 tests in 525.957 seconds. The golden test accounted for 448.328 seconds, while its phase measurements accounted for 448.318 seconds. Those phases executed 6,697,386,493 VM instructions and cumulatively allocated 939.11 GiB on the executing thread; this is allocation traffic, not peak live memory.

| Golden phase | Elapsed | VM instructions | Allocated | Bytes per instruction |
| --- | ---: | ---: | ---: | ---: |
| Parser closures | 26.65 seconds | 400.9 million | 58.21 GiB | 155.9 |
| Source-set closure | 38.33 seconds | 563.2 million | 80.43 GiB | 153.3 |
| Source-graph closure | 42.47 seconds | 643.4 million | 91.65 GiB | 152.9 |
| Source-symbols closure | 85.25 seconds | 1,358.2 million | 185.26 GiB | 146.5 |
| Source-bindings closure | 253.11 seconds | 3,722.8 million | 517.17 GiB | 149.2 |

The consistent allocation rate across the large closures makes allocation reduction in the reference interpreter the next measured optimization target. Artifact compilation took only 1.97 seconds in the same golden run, so compiler-output caching cannot materially address the current bottleneck.

The first allocation reduction removes ordinary function-call argument arrays and rents bounded locals and operand storage from managed pools. A golden-only checkpoint preserved the same 6,697,386,493 executed instructions while reducing cumulative allocation from 939.11 GiB to 565.15 GiB, a 39.8% reduction. Golden elapsed time fell from 448.328 to 411.003 seconds, an 8.3% improvement. The source-bindings phase fell from 253.11 to 226.41 seconds and from 517.17 to 319.98 GiB of allocation traffic.

The final telemetry-enabled Standard run passed all 47 tests in 481.156 seconds, down from 525.956 seconds, an 8.5% improvement. Its golden phase completed in 413.716 seconds, down 7.7%, while retaining the same instruction count and 565.14 GiB allocation total.

The next source-level optimization groups intrinsic-call candidates by UTF-8 byte length, orders common compiler intrinsics first, and returns on the first name match. The focused bindings test average fell from 3.661 to 3.366 seconds, an 8.1% improvement. The final Standard run passed all 47 tests in 458.922 seconds, 4.6% below the pooled-frame baseline, while its golden test fell from 413.716 to 390.397 seconds, a 5.6% improvement. The source-bindings phase fell from 240.420 to 220.950 seconds, an 8.1% improvement.

This is a CPU-throughput trade rather than another allocation reduction. The changed bootstrap source increased source-bindings instructions from 3,722,805,908 to 3,746,501,967, or 0.6%, and allocation traffic from 319.98 to 322.46 GiB, or 0.8%, while replacing expensive candidate text conversions with cheaper length and branch instructions. Runtime byte-value allocation therefore remains the next measured allocation target.

Runtime optimization does not require the entire suite on every edit. Use the focused `bodies, locals` compiler test for the seconds-scale correctness loop, use Development after a coherent batch, run the `golden` area alone with a timing report for occasional performance checkpoints, and reserve Standard plus Qualification for the final candidate:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Fast -TestArea compiler -TestFilter 'bodies, locals' -FailFast
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Development
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Fast -TestArea golden -FailFast -TimingReportPath artifacts/seed-timing-golden.json
```

GitHub runs the Windows and Linux qualification jobs concurrently. Exact milestone qualification still uses the committed source archive on Windows and the real Debian QA host and compares their normalized reports and direct artifacts.

GitHub keeps the required `Windows verifier` and `Linux verifier` jobs present on every workflow run, but classifies the changed paths before installing .NET or executing Seed. Markdown outside `Specifications/`, the root license, and editor-only changes use a lightweight `git diff --check` path. Editor verification runs for editor files and source-language compiler or specification changes. Specifications, implementation, tests, build configuration, workflows, verifier code, and any unrecognized path fail closed to complete dual-host Qualification. Manual workflow dispatch always selects Qualification and editor verification. The workflow does not use top-level path filters because a skipped required workflow can leave its checks pending.

## Initial measurements

Measurements on the Windows x64 development host on 2026-07-30:

| Path | Elapsed |
| --- | ---: |
| Focused declaration/signature test after a warm build | 3.55 seconds |
| Fast wrapper including a warm Release build | 8.3 seconds |
| Standard 45-test suite | 257.9 seconds |
| Previous complete qualification | 619.9 seconds |
| Direct-CLI complete qualification, Windows | 481.3 seconds |
| Direct-CLI complete qualification, Debian QA | 501.8 seconds |

The complete qualification improvement is 138.6 seconds, or 22.4%, without removing a test or changing a portable artifact. A five-launch microbenchmark measured 912.9 ms for direct DLL invocation versus 5,256.6 ms through `dotnet run`.

The first timing profile shows where remaining work belongs:

| Test | Elapsed | Approximate suite share |
| --- | ---: | ---: |
| Golden cross-host contract and real closures | 188.4 seconds | 74% |
| Windvale body parser | 25.9 seconds | 10% |
| Windvale linker semantics | 12.9 seconds | 5% |
| Portable import graph | 11.1 seconds | 4% |

Build time is only a few seconds and is not the current bottleneck. Further broad build tuning is unlikely to matter. Any parallel-test or artifact-reuse experiment should start with the golden-contract path, use isolated state/output, measure memory and elapsed time on both hosts, and retain a sequential oracle until equivalence is established.

## Body-binding measurements

Decision 0034 adds one real nine-module body-binding closure to the portable golden contract and a second independent execution through the native CLI qualification boundary. On exact commit `9185b28`, the focused binding test took 2.877 seconds on Windows and 2.691 seconds on Debian. Windows Qualification completed in approximately 1,005.8 seconds with a 526.833-second suite; Debian Qualification completed in approximately 1,066.4 seconds with a 538.947-second suite.

The added semantic work intentionally increases milestone qualification time without increasing the normal inner loop: the Fast binding command remains a few seconds after the warm build, and Standard executes the expensive closure once. Qualification alone pays for the second process-level CLI closure. A future optimization must target the golden real-closure work or execute independent closures concurrently with isolated runtime/output state; repeated project evaluation is no longer the significant cost.

## Typed-WVIR verification

The typed-WVIR slice adds a focused test that compiles the portable core, runs semantic and corruption coverage, and sends a control-heavy fixture through the hosted source-set tool. On the Windows x64 development host, the complete Fast wrapper took 17.1 seconds after the new resources were added; the WVIR test itself took 6.45 seconds. A warm direct hosted-tool run of the fixture took approximately 1.6 seconds. The candidate 47-test Standard suite passed in 528.217 seconds; its golden-contract case accounted for 449.977 seconds.

Exact commit `bf77f70` completed Qualification in approximately 1,090.7 seconds on Windows and 1,118.8 seconds on Debian QA. Both hosts passed zero-warning Release builds, all 47 tests, and the complete native verifier. Their normalized reports matched, and all 48 portable artifacts totaling 5,624,431 bytes were byte-identical. This preserves the tier split: the milestone gate pays for process-level repetition and direct cross-host evidence, while the focused WVIR loop remains measured in seconds.

The ten-module compiler self-lowering experiment remains outside Fast and Standard. Decision 0041 fuses local discovery with typed-WVIR construction on the successful path, but the exact closure still reaches the unchanged 4,000,000,000-instruction ceiling. On the Windows development host, the retained candidate reached the bounded limit after 209.130 seconds versus 222.393 seconds for the separate-pass baseline. Because both runs execute the full ceiling, elapsed time is diagnostic host evidence rather than portable semantics. The remaining target is algorithmic lookup/typed-lowering work, not a higher limit or an unstable multi-minute case in every development loop.

Exact Decision 0041 commit `b124115` completed Qualification in 896.1 seconds on Windows and 908.2 seconds on Debian, with complete-suite times of 450.790 and 458.664 seconds. Both hosts passed zero-warning builds, all 48 tests, and the complete native verifier. The normalized contracts matched, and all 61 portable artifacts totaling 7,418,376 bytes were byte-identical. This qualifies the fused traversal while leaving the multi-minute ten-module closure experiment outside every routine verifier tier.

## Bidirectional symbol-index measurements

Decision 0050 keeps the public WVSD directory in module/source order and adds private WVSI maps in both directions between WVSD directory indices and canonical record-then-enum ordinals. Consumers also read packed directory fields directly and reject unequal name lengths before ordinal comparison. This removes repeated nominal reranking and most temporary symbol-match construction without changing public semantic or serialization contracts.

On the Windows development host, the focused typed-WVIR fixture falls from Decision 0042's 5,735,695 instructions to 5,715,847, a 0.35% reduction. The real nine-module binding closure is the more representative result: it falls from 2,972,056,275 to 2,600,859,185 instructions, a 12.5% reduction, even though the current closure contains 41 more locals, 116 more reads, 31 more assignments, and 29 more calls. The focused 24-test compiler area passes in 47.920 seconds. The complete 48-test Standard suite passes in 369.044 seconds; its golden test takes 297.265 seconds, including 56.969 seconds and 982,519,944 instructions for the symbol closure and 165.039 seconds and 2,600,859,185 instructions for the binding closure.

The exact ten-module typed-WVIR workload still reaches `WVR3011` at the unchanged four-billion acceptance ceiling after approximately 254 seconds on this host. A diagnostic-only six-billion run also reached its temporary ceiling after approximately 400.6 seconds. Both elapsed values are host observations, and the latter is not a gate change. Fixed-ceiling function profiling shows directory-entry construction falling from 488,312,500 to 1,802,017 instructions and the previous 320,228,495-instruction nominal-index helper falling below the retained top-function report. The remaining exact-closure cost is dominated by repeated lexical and parser traversal.

Inlining ASCII whitespace and identifier-continuation logic into the lexer was measured and rejected: it increased the focused typed-WVIR fixture to 5,986,551 instructions. The helper boundaries remain cheaper in the current runtime. The next source-level experiment should share or retain validated lexical/parse evidence rather than repeat local lexer micro-inlining.

Exact Decision 0050 commit `e37204f` completed Qualification in 724.8 seconds on Windows and 738.0 seconds on Debian, with complete-suite times of 366.914 and 379.640 seconds. Both hosts passed zero-warning builds, all 48 tests, and the complete native verifier. Their normalized contracts matched, and all 61 portable artifacts totaling 7,546,823 bytes were byte-identical. This qualifies the symbol-index improvement while leaving the exact ten-module experiment outside routine verification.

## Validated-scan reuse measurements

Decision 0055 separates checked public entry points from compiler-internal entry points that consume already established full-source scan and cursor evidence. It also reuses the declaration summary for body validation, skips valid function bodies with one bounded byte scan, and searches nominal records/enums only within their canonical first-byte ranges. Checked deep-body input still returns the established nesting-limit failure without host-stack exhaustion.

On the Windows development host, the focused typed-WVIR fixture falls from Decision 0050's 5,715,847 instructions to 3,626,693, a 36.55% reduction. The focused 24-test compiler area passes in 37.927 seconds. The complete 48-test candidate Standard suite passes in 246.417 seconds.

The exact ten-module typed-WVIR workload completes in 3,912,239,584 instructions, leaving 87,760,416 instructions under the unchanged four-billion acceptance ceiling. It reports 10 modules, 315 functions, 5,658 blocks, 27,377 operations, 25,094 temporaries, 21,869 operands, and 1,720,804 directory bytes. The exact case remains outside Fast and Standard; it is milestone evidence for the next Stage 0 → Stage 1 → Stage 2 convergence work.

Measured binary nominal search and record-return identifier-tail helpers were removed because they regressed the focused workload. Long shared compiler-name prefixes made midpoint ordinal comparison more expensive than the retained bounded range scan.

Exact commit `1a4fca7` completed Qualification in approximately 449.0 seconds on Windows and 469.5 seconds on Debian, with complete-suite times of 220.663 and 227.434 seconds. Both hosts passed zero-warning builds, all 48 tests, and the complete native verifier. Their normalized contracts matched, and all 61 portable artifacts totaling 7,733,603 bytes were byte-identical. The lower elapsed time is diagnostic host evidence; the portable acceptance evidence remains the unchanged instruction ceiling and exact cross-host artifacts.

## Canonical backend measurements

Decision 0037 adds the interleaved static-data/text fixture to the existing source-to-WVB focused test without adding another complete-suite case. On exact commit `636627c`, that focused test took 4.585 seconds on Windows and 4.701 seconds on Debian. Windows Qualification completed in 902.4 seconds with a 457.742-second suite; Debian Qualification completed in 916.4 seconds with a 471.285-second suite. Both hosts passed all 48 tests, the complete native verifier, and zero-warning Release builds.

This preserves the intended cost split: canonical remapping, text interning, intrinsic lowering, exact Stage 0 differential comparison, mandatory WVB verification, and runtime execution remain available in a seconds-scale focused loop, while only milestone qualification pays for the full native and cross-host gates.

## Nominal backend measurements

Decision 0038 adds the interleaved record/enum fixture to the same source-to-WVB test and adds its Windvale and Stage 0 outputs to native evidence without adding another complete-suite case. On exact commit `f39ff73`, the focused test took 7.382 seconds on Windows. The complete 48-test Standard suite took 434.111 seconds.

Windows Qualification completed in 906.282 seconds with a 458.742-second suite; Debian Qualification completed in 913.811 seconds with a 465.247-second suite. Both hosts passed the complete native verifier and zero-warning Release builds, and all 57 portable artifacts matched byte for byte. Nominal metadata and all six nominal operations therefore remain in the seconds-scale focused loop, while exact-host process evidence remains confined to the milestone gate.

## Capability/profile backend measurements

Decision 0039 adds the hosted all-capabilities fixture to the same source-to-WVB test and adds its Windvale and Stage 0 outputs to native evidence without adding another complete-suite case. On exact commit `98117c1`, the focused test took 8.695 seconds on Windows. Standard verification completed in 445.034 seconds, with 441.254 seconds in the complete 48-test suite.

Windows Qualification completed in 908.170 seconds with a 458.146-second suite; Debian Qualification completed in 926.189 seconds with a 474.315-second suite. Both hosts passed the complete native verifier and zero-warning Release builds, and all 59 portable artifacts matched byte for byte. Profile preservation, canonical capability metadata, all seven call signatures, verifier inspection, authorization, and runtime execution therefore remain in the seconds-scale focused loop, while exact-host process evidence remains confined to the milestone gate.

## Static multi-module backend measurements

Decision 0040 adds one three-module differential fixture to the same source-to-WVB test and adds its Windvale and Stage 0 outputs to native evidence without adding another complete-suite case. On exact commit `cb1db23`, the focused test took 10.354 seconds on Windows. Standard verification completed in 441.8 seconds, with 435.850 seconds in the complete 48-test suite and 8.956 seconds in the backend test.

Windows Qualification completed in 916.6 seconds with a 465.074-second suite and 9.203-second backend test; Debian Qualification completed in 927.5 seconds with a 477.445-second suite and 9.682-second backend test. Both hosts passed the complete native verifier and zero-warning Release builds, and all 61 portable artifacts matched byte for byte. Owner-aware static flattening, cross-module calls and nominal values, global text/data/type/function ordering, root-only exports, rejection without publication, verification, and runtime result `42` therefore remain in the seconds-scale focused loop, while exact-host process evidence remains confined to the milestone gate.

## Qualification evidence

The optimized verifier was qualified from the exact pre-normalization commit `de88007b4716c88604321baaad4c4d5c417d317e`, archived as `windvale-verification-speed-de88007.tar.gz`, 362,715 bytes with SHA-256 `dc5fab40a06a3f19706923fa3f569178297cd97bda5b9a8dc9e2b9c128942b92`. The attribution migration ledger maps it to tree-identical normalized commit `00466fd9e9feaac4655cdf9748ac1dc56b586a84`.

Windows x64 completed Qualification in 481.3 seconds with a 253.025-second suite; Debian GNU/Linux 12 x64 completed it in 501.8 seconds with a 270.660-second suite. Both hosts passed all 45 tests, the complete native CLI verifier, and zero-warning Release builds. Their normalized conformance reports matched, and all 42 directly compared artifacts, totaling 3,102,891 bytes, were byte-identical. The Windows report SHA-256 is `2f9f0f62b8a98e411500ef34fe697936808c74b39bcfd915c6de780f6fffd1ff`; the Debian report SHA-256 is `bd514ce1a9ba154cde689e4b1cf4cac23f0b5c50d21b5e58e40074022a04e5dd`. After retrieving the evidence, the exact Debian QA directory and transferred archive were removed and confirmed absent.

The binding candidate commit `9185b28af7a7a7802d8606162ef7a5413d82dc33` was archived as `windvale-source-bindings-9185b28.tar.gz`, 2,583,180 bytes with SHA-256 `4f6c8d12ea80a52ad9d9cf989b80ac1e6cb1d2f9d266ad9e71effa3afb81c2ef`. Both hosts passed all 46 tests, the complete native CLI verifier, and zero-warning Release builds. The Windows report is 15,561 bytes with SHA-256 `cb17433f772f0336208e39623be65726b3b9d57525afa5fc7a53a80f93af9972`; the Debian report is 15,471 bytes with SHA-256 `f25adeb591fa8547ffadbd38c61f1a1f667cbbf57f1e909e9e48a520fb3e54e8`. Their normalized contracts matched, and all 45 directly compared artifacts, totaling 4,076,491 bytes, were byte-identical. The retrieved Debian artifact bundle was 1,188,300 bytes with SHA-256 `a91577a01962854ee7f0f5f806f860adfd77698c43ecea2c32cd8e03c098b7b5`. After retrieval, the resolved Debian QA directory, source archive, and artifact bundle were removed and confirmed absent.

## First shared native slice measurements

Decision 0059 adds one differential case whose focused execution takes approximately 0.1 seconds on both Windows and Debian x64. The Windows Development tier passes all 48 regular tests in 49.190 seconds; Standard passes all 49 tests in 211.924 seconds. Exact commit `962bb85` completes Qualification in 432.4 seconds on Windows and approximately 464.3 seconds on Debian, with suite times of 219.563 and 235.420 seconds. Both hosts pass zero-warning builds, all 49 tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match.

The native case itself is not a new throughput bottleneck. Keeping the golden compiler closure out of Development continues to provide the useful broad loop. Windows and Debian qualification may run concurrently, but each host must retain its normal UTF-8 console contract: a hidden redirected Windows process decoded a macron name with a replacement character even though the CLI exited successfully, and was discarded in favor of the clean foreground qualification rather than weakening the output check.

Decision 0060 widens that same differential case instead of registering another full-suite test. The final focused validations took 0.140 seconds on Windows and 0.128 seconds on exact-archive Debian. Windows Development passed all 48 regular tests in 54.295 seconds. Standard passed all 49 tests in 230.495 suite seconds and 241.6 wall-clock seconds. Exact commit `84dd908` completed concurrent Qualification in 474.1 seconds on Windows and 489.4 seconds on Debian, with suite times of 223.137 and 235.710 seconds. Both hosts passed zero-warning builds, all 49 tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts matched.

Checked arithmetic therefore remains effectively free in the inner loop relative to the compiler closures. The focused case is the normal edit-time check, Development is the broad coherent-slice check, and exact Standard plus dual-host Qualification remain milestone evidence. The next comparisons/control-flow slice should continue growing this differential case unless it needs a genuinely separate ownership or failure boundary.

Decision 0061 again widens the same differential case, now with typed locals and blocks, all signed i32 comparisons, Boolean operations, nested forward control, an early return, a false-path program, and four new hostile-fragment families. The final focused Windows case passed in 0.223 seconds and the exact-archive Debian case in 0.166 seconds. Windows Development passed all 48 regular tests in 55.008 seconds. Standard passed all 49 tests in 230.225 suite seconds and 234.6 wall-clock seconds. Exact commit `f0a53a9` completed concurrent Qualification in 469.086 seconds on Windows and approximately 489.6 seconds on Debian, with suite times of 224.296 and 243.020 seconds. Both hosts passed zero-warning builds, all 49 tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts matched.

Typed forward control therefore remains a sub-second edit-time test despite substantially wider native semantics and adversarial coverage. Native instruction budgeting and backward branches should stay in this case if their safety boundary can be exercised without adding a separate long-running suite; deliberately terminating loop fixtures must remain bounded so the focused path does not inherit qualification-scale cost.

Decision 0062 adds dynamic success/exhaustion boundaries for a four-instruction constant and a 157-instruction finite loop, WVO-linked loop execution, one deliberately nonterminating loop bounded at 50 instructions, and budget-source/charge/target/status corruption families to the same differential case. The final focused Windows case remains approximately 0.17 seconds; exact-archive Debian takes 0.162 seconds. Development passes all 48 regular tests in 47.721 seconds; Standard passes all 49 tests in 201.986 suite seconds and 205.709 wall-clock seconds. Exact commit `2b67c8a` completes concurrent Qualification in 439.535 seconds on Windows and approximately 457.5 seconds on Debian, with suite times of 216.829 and 233.088 seconds. Both hosts pass zero-warning builds, all 49 tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match.

Budgeted loops therefore preserve the seconds-scale inner loop and add no new complete-suite case. The exact ten-byte charge per WVB instruction increases generated native code size but does not materially affect current verifier throughput because native fixtures remain small; later compaction should be driven by code-size and execution measurements rather than test-suite timing.

Decision 0069 adds two separate full-suite tests because dynamic text/descriptor/void safety and the complete real `wvdump` application form distinct ownership boundaries. Their final focused Windows runs take 0.928 and 0.799 seconds before the warm Release optimization pass, while the shared native case takes 0.253 seconds; the combined edit-time signal remains under two seconds. Exact commit `7979933` completes concurrent Qualification with suite times of 212.675 seconds on Windows and 233.729 seconds on Debian. Both hosts pass zero-warning builds, all 56 tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match. Pinned QEMU probe 12 passes on Windows; the Debian QA host does not provide QEMU.

The added real-tool coverage therefore does not move the inner loop toward bootstrap-scale time. Focused ABI tests remain the normal development path, Standard provides final in-process integration, and concurrent exact-archive Qualification remains the milestone gate. Native service replacement should extend the focused dynamic-text case unless it introduces a distinct process, container, or recovery boundary.

Decision 0070 follows that rule by widening the existing borrowed-bytes/native case with 23 strict UTF-8 boundary ranges, exact 800-byte service identity, deterministic reconstruction, and corruption rejection. Its focused Release pass remains under one second on Windows. Exact commit `53cee69` completes cross-host Qualification with suite times of 214.004 seconds on Windows and 229.082 seconds on Debian. Both hosts pass zero-warning builds, all 56 tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match. Pinned QEMU probe 12 remains unchanged and passes on Windows.

The first runtime-native leaf therefore adds no new full-suite test and keeps its edit-time signal inside the existing focused boundary. Native text-arena ownership and later service replacement should continue extending focused cases; a new long-running test is justified only for a genuinely independent process, ownership, recovery, or security boundary.

Decision 0071 follows the same rule by extending the existing native dynamic-text case with three exact native-service identities, signed-minimum/unsigned-maximum/zero formatting, shared native/managed arena use, deterministic reconstruction, corruption rejection, and separate value-limit/arena-exhaustion failures. The pre-commit Windows Release case takes 1.011 seconds; complete `wvdump` takes 0.843 seconds and the borrowed-byte/UTF-8 native case 0.328 seconds. Standard passes all 56 tests in 235.402 seconds. Exact commit `8888951` completes cross-host Qualification with suite times of 233.375 seconds on Windows and 248.582 seconds on Debian. Both hosts pass zero-warning builds, all 56 tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match. Pinned QEMU probe 13 passes on Windows.

ABI 11 therefore keeps edit-time native evidence around two seconds while broadening the actual machine-code boundary. Exact-commit Standard and dual-host Qualification remain the milestone gate; enum metadata, quoting, and hosted native adapters should continue reusing focused boundaries unless they introduce a genuinely new process, ownership, recovery, or security contract.

Decision 0072 again extends the existing native dynamic-text case rather than adding a full-suite test. It covers exact enum-name and quote leaves, the complete runtime-private `WVEN` layout and independent reconstruction, multiple nominal types, all quote escape families including supplementary Unicode, corrupt metadata, and separate quote/enum value and arena bounds. The final focused Windows Release case takes 0.429 seconds. Pre-commit Standard passes all 56 tests in 209.841 suite seconds; its warm dynamic-text and complete-`wvdump` cases take 0.176 and 0.466 seconds. Exact commit `f97d221` completes cross-host Qualification with suite times of 220.741 seconds on Windows and 233.171 seconds on Debian; the dynamic-text cases take 0.184 and 0.208 seconds. Both hosts pass zero-warning builds, all 56 tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match. Pinned QEMU probe 13 remains exact and passes on Windows.

The final pure-service migration therefore preserves a sub-second focused edit loop. Hosted capability adapters may need separate process/container evidence when their implementation changes, but ordinary semantic and corruption cases should still be attached to the smallest existing owner boundary.

Decision 0073 extends the existing native hosted-input case rather than adding a full-suite test. It reconstructs and corrupts both exact argument leaves, consumes all 67 allowed descriptors including empty, ASCII, euro, and supplementary-Unicode text, proves the zero-count path, and retains exact out-of-range and service-table corruption failures. The warm Windows Standard case takes 20 milliseconds. All 33 runtime-tagged tests pass in 65.794 seconds, pre-commit Standard passes all 56 tests in 220.013 seconds, and all 15 OS tests pass. Exact commit `328e455` completes cross-host Qualification with suite times of 238.075 seconds on Windows and 252.295 seconds on Debian; the hosted-input cases take 0.020 and 0.022 seconds. Both hosts pass zero-warning builds, all 56 tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match. Pinned-QEMU probe 14 passes on Windows.

Decision 0074 again extends focused native cases instead of creating another broad suite. The existing runtime-service case now reconstructs and corrupts all four exact platform/service leaf identities, executes console and diagnostic output against separate OS-backed files, covers empty, euro, and supplementary-Unicode text, checks direct JIT and linked WVO/AOT output, and forces rejected reference and native writes to prove `WVR3029`. Dynamic-text, complete `wvdump`, and hosted-input cases reuse the same capture helper. The warm Release native-output case takes 43 milliseconds and Windows Standard passes all 56 tests in 213.076 seconds. Exact commit `66b273f` completes concurrent Qualification in 481.569 seconds on Windows and 503.169 seconds on Debian, with suite times of 232.193 and 247.986 seconds; the native-output cases take 0.042 and 0.037 seconds. Both hosts pass zero-warning builds, all 56 tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 61 portable artifacts match. Pinned-QEMU probe 15 passes on Windows, and GitHub's independent Windows and Linux jobs pass.

Decision 0076 again extends the existing hosted-input and complete-`wvdump` cases. The focused case reconstructs and corrupts both platform file leaves, executes real OS files through direct JIT and linked WVO/AOT, proves the supplied Stage 0 reader is never called, verifies first-success cache reuse through independent record validation, and covers invalid, missing, oversized, and 65th-name failures. Its warm Release time is 0.134 seconds. Windows Development passes all 56 regular tests in 51.813 seconds. The zero-warning Standard build passes all 57 tests in 275.825 suite seconds and 280.2 wall-clock seconds; its cold hosted-input case takes 1.263 seconds. Exact commit `ef08619` completes concurrent Qualification in 499.037 seconds on Windows and approximately 529 seconds on Debian, with suite times of 238.115 and 257.556 seconds. Their native hosted-input cases take 0.130 and 0.101 seconds. Both hosts pass all 57 Seed tests and all 15 OS tests; normalized reports and all 61 portable artifacts match. Pinned-QEMU probe 16 passes on Windows. The 0.1-second native file case remains the normal iteration loop.

The immutable argument-table slice therefore keeps the inner loop faster than its one-second target while covering the complete hosted snapshot bound. The three remaining output/file callbacks may reuse focused semantic cases, but their first native system-boundary implementation must also retain exact process-level error and capability evidence.

Decisions 0077 and 0078 keep both stencil shapes in one focused ownership-boundary case. It compiles the Windvale-written assembler once, runs it twice over each WVA source, compares exact Stage 0 and embedded WVO bytes, validates one and eight typed patch locations, and contains malformed objects and instantiation inputs. Cold focused Windows execution takes 2.906 seconds; the warm pre-commit change-aware run takes 1.155 seconds. The live hosted-input case remains 0.132 seconds in that warm run.

Exact commit `50294d9` completes concurrent Qualification in 459 seconds on Windows and 477 seconds on Debian, with suite times of 224.398 and 242.491 seconds. The exact-host stencil cases take 1.475 and 1.550 seconds, while live hosted input takes 0.123 and 0.101 seconds. Both hosts pass zero-warning builds, all 60 Seed tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 62 current portable artifacts match. Pinned-QEMU probe 16 and GitHub's independent Windows/Linux gate pass. The multi-patch ownership gain therefore adds roughly one second to the focused loop while leaving complete-suite time dominated by the unchanged golden contract.

Decision 0079 adds one separate full-suite test because execution of the first Windvale-owned native-stencil consumer is a new language/runtime ownership boundary rather than another Stage 0 stencil-parser case. It covers both exact production objects, deterministic immutable construction, representative malformed families, one changed value at every byte position, and agreement across the reference interpreter, native JIT, and linked WVO/AOT. The final pre-commit focused Windows Release run takes 0.879 seconds. Exact commit `f3a4ba4` completes Qualification in approximately 492.4 seconds on Windows and 490.8 seconds on Debian, with suite times of 246.227 and 248.495 seconds. The exact-host consumer cases take 0.390 and 0.358 seconds; the golden contracts take 184.619 and 187.213 seconds. Both hosts pass zero-warning builds, all 61 Seed tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 64 portable artifacts match. Pinned-QEMU probe 16 and GitHub's independent Windows/Linux gate pass. The focused case remains the normal edit loop, while exact portable-artifact and report comparison remains confined to the milestone gate.

Decision 0080 adds one focused boundary case for the capability-free descriptor entry and live stencil consumption. The case recompiles and matches the retained WVB, compares reference/JIT/linked-WVO bytes, corrupts the entry-result prologue, rejects wrong executor kinds and hosted byte entries, covers static and arena result provenance, and observes the live 5/70 service split. The retained consumer executes once per process through a thread-safe lazy owner; later native executions reuse only its exact immutable 75-byte result, avoiding a new per-call compiler/JIT cost. The first complete Windows Release Development gate passes all 61 regular tests in 57.456 seconds; the warm bridge case takes 0.223 seconds. Exact commit `f547af8` completes concurrent Qualification in approximately 491.5 seconds on Windows and 499.6 seconds on Debian, with suite times of 234.260 and 250.804 seconds. The exact-host bridge cases take 0.214 and 0.233 seconds. Both hosts pass zero-warning builds, all 62 Seed tests, all 15 OS tests, and the complete CLI verifier; normalized reports and all 65 portable artifacts match. Pinned-QEMU probe 16 and GitHub's independent Windows/Linux gate pass. The focused bridge case remains the normal edit loop.

Decision 0082 adds one focused publication-layout boundary case. It recompiles and matches the portable core and retained hosted bridge, checks the bridge's sole in-memory file capability, exercises service-free and all-11-service plans, every request failure family, deterministic output, malformed response reconstruction, the 34 MiB ceiling, and live unchanged relative-patch execution. The initial cold Windows Release case takes 0.565 seconds; its warm execution takes approximately 0.022 to 0.029 seconds. All 39 runtime-tagged tests pass in 28.527 seconds, and the zero-warning Development gate passes all 62 regular tests in 59.393 suite seconds. Exact commit `ba2cf69` completes Qualification in 485.476 seconds on Windows and 495.249 seconds on Debian, with suite times of 241.821 and 247.813 seconds; the publication-layout case takes 17 and 22 milliseconds. Both hosts pass all 63 Seed tests, all 17 OS tests, and the complete CLI gate; normalized reports and all 67 portable artifacts match. Both pinned-QEMU probe-17 scenarios and GitHub's independent Windows/Linux gate pass. The publication planner therefore preserves a milliseconds-scale edit signal while the unchanged golden contract continues to dominate milestone time.

Decision 0083 adds one separate focused test because executable-image state and cleanup are a distinct authority boundary. It recompiles and matches the portable core and retained bridge, covers every request status and response/transition field, rejects forged plans and invalid action order, exercises normal lifetime plus release from writable, copied, executable, and invoked states, and retains live execution through the owner. The first complete focused Windows run takes 0.439 seconds, the final warm pre-commit run takes 0.143 seconds, and in one broad process the case takes 15 milliseconds. All 40 runtime-tagged tests pass in 26.790 seconds, and the zero-warning Development gate passes all 63 regular tests in 54.983 suite seconds.

Exact commit `a898fe8` completes concurrent Qualification in 484.1 seconds on Windows and 513.7 seconds on Debian, with suite times of 240.113 and 256.330 seconds. The exact-host lifetime case takes 12 and 14 milliseconds. Both hosts pass zero-warning builds, all 64 Seed tests, all 17 OS tests, and the complete CLI gate; normalized reports and all 69 portable artifacts match. Both pinned-QEMU probe-17 scenarios retain their qualified identities. Publication-lifetime ownership therefore adds a milliseconds-scale edit signal and does not move the qualification bottleneck away from the unchanged golden compiler closure.

Decision 0089 follows the same throughput rule with one focused wide-call case. Exact integrated commit `860c69c` completes concurrent Qualification in 501.3 seconds on Windows and 517.2 seconds on Debian, with suite times of 243.529 and 257.101 seconds. The wide-call case takes 38 and 39 milliseconds while the golden closure takes 180.409 and 191.031 seconds. Both hosts pass zero-warning builds, all 67 Seed tests, all 21 OS tests, and the complete CLI/reproduction gate; normalized contracts and all 69 portable artifacts match. All three exact probe-21 QEMU scenarios and GitHub's Windows/Linux gate pass. ABI 16 therefore removes 103 real compiler signature blockers without moving the edit-time loop or milestone bottleneck materially.

## Evidence rules

- Timing values are diagnostic host evidence, not portable semantics, and never enter the conformance contract.
- A filtered or fail-fast run cannot write a conformance report.
- Standard and Qualification always run all registered tests.
- Cached artifacts may accelerate local experiments but cannot replace a clean exact-commit qualification build.
- The Windows verifier fixes its native-process input/output encoding to UTF-8 internally. Exact macron-bearing CLI checks must not depend on whether the verifier is attached to a console or launched with redirected output.
- A faster implementation may replace demonstrated overhead; it may not silently remove independent verification, hostile-input coverage, or direct native-boundary checks.
