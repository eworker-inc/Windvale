# Seed verification throughput

## Purpose

Windvale keeps the complete cross-host qualification gate, but ordinary development should pay only for the narrowest evidence relevant to the current edit. Verification therefore has three explicit levels rather than one expensive all-purpose command.

## Levels

- `Fast` builds once and runs an explicit selection of one or more test areas, a case-insensitive displayed-name substring, or their intersection. It may fail fast and may write a local timing report. It does not produce conformance evidence.
- `Standard` builds once, runs all 47 in-process conformance tests, and writes the normal host report. It stops before native CLI qualification.
- `Qualification` is the default and remains the milestone gate. It adds every native CLI, hosted-boundary, deterministic-artifact, and failure-preservation check.

Qualification builds the CLI once and invokes the resulting `windvale.dll` directly in each separate process. This preserves command parsing, process exit codes, native file behavior, capability boundaries, and output checks while removing repeated `dotnet run` project evaluation.

## Development selection

The conformance runner assigns every registered test to one or more stable areas: `assembler`, `bytecode`, `compiler`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. Repeated area selections form a union. Supplying a displayed-name filter as well intersects that filter with the selected areas. The runner exposes the canonical names through `--list-areas`.

`Tools/Verify/Verify-Changed.ps1` maps changed paths to those areas and invokes the Fast verifier. Compiler, runtime, bytecode, assembler, object-model, linker, and Foundation paths select their owned areas. Specification paths select the areas that implement the named contract. Tests, build configuration, workflows, verification tooling, broad Seed examples, unknown specifications, and unrecognized implementation paths fail closed to all areas. Documentation outside `Specifications/`, the root license, and editor-only changes do not run Seed; editor-relevant paths still run the editor verifier.

Changed-file selection is an inner-loop optimization only. It never writes a conformance report or supplies Standard or Qualification evidence.

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

Runtime optimization does not require the entire suite on every edit. Use the focused `bodies, locals` compiler test for the seconds-scale correctness loop, run the `golden` area alone with a timing report for occasional performance checkpoints, and reserve Standard plus Qualification for the final candidate:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Fast -TestArea compiler -TestFilter 'bodies, locals' -FailFast
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

The ten-module compiler self-lowering experiment remains outside Fast and Standard. Symbol preparation alone is substantial, and the separate local-discovery plus IR body traversals currently exceed the unchanged 4,000,000,000-instruction ceiling. This is recorded as a body-traversal fusion target, not addressed by increasing limits or putting an unstable multi-minute case in every development loop.

## Qualification evidence

The optimized verifier was qualified from the exact pre-normalization commit `de88007b4716c88604321baaad4c4d5c417d317e`, archived as `windvale-verification-speed-de88007.tar.gz`, 362,715 bytes with SHA-256 `dc5fab40a06a3f19706923fa3f569178297cd97bda5b9a8dc9e2b9c128942b92`. The attribution migration ledger maps it to tree-identical normalized commit `00466fd9e9feaac4655cdf9748ac1dc56b586a84`.

Windows x64 completed Qualification in 481.3 seconds with a 253.025-second suite; Debian GNU/Linux 12 x64 completed it in 501.8 seconds with a 270.660-second suite. Both hosts passed all 45 tests, the complete native CLI verifier, and zero-warning Release builds. Their normalized conformance reports matched, and all 42 directly compared artifacts, totaling 3,102,891 bytes, were byte-identical. The Windows report SHA-256 is `2f9f0f62b8a98e411500ef34fe697936808c74b39bcfd915c6de780f6fffd1ff`; the Debian report SHA-256 is `bd514ce1a9ba154cde689e4b1cf4cac23f0b5c50d21b5e58e40074022a04e5dd`. After retrieving the evidence, the exact Debian QA directory and transferred archive were removed and confirmed absent.

The binding candidate commit `9185b28af7a7a7802d8606162ef7a5413d82dc33` was archived as `windvale-source-bindings-9185b28.tar.gz`, 2,583,180 bytes with SHA-256 `4f6c8d12ea80a52ad9d9cf989b80ac1e6cb1d2f9d266ad9e71effa3afb81c2ef`. Both hosts passed all 46 tests, the complete native CLI verifier, and zero-warning Release builds. The Windows report is 15,561 bytes with SHA-256 `cb17433f772f0336208e39623be65726b3b9d57525afa5fc7a53a80f93af9972`; the Debian report is 15,471 bytes with SHA-256 `f25adeb591fa8547ffadbd38c61f1a1f667cbbf57f1e909e9e48a520fb3e54e8`. Their normalized contracts matched, and all 45 directly compared artifacts, totaling 4,076,491 bytes, were byte-identical. The retrieved Debian artifact bundle was 1,188,300 bytes with SHA-256 `a91577a01962854ee7f0f5f806f860adfd77698c43ecea2c32cd8e03c098b7b5`. After retrieval, the resolved Debian QA directory, source archive, and artifact bundle were removed and confirmed absent.

## Evidence rules

- Timing values are diagnostic host evidence, not portable semantics, and never enter the conformance contract.
- A filtered or fail-fast run cannot write a conformance report.
- Standard and Qualification always run all registered tests.
- Cached artifacts may accelerate local experiments but cannot replace a clean exact-commit qualification build.
- A faster implementation may replace demonstrated overhead; it may not silently remove independent verification, hostile-input coverage, or direct native-boundary checks.
