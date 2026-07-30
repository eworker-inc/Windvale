# Seed verification throughput

## Purpose

Windvale keeps the complete cross-host qualification gate, but ordinary development should pay only for the narrowest evidence relevant to the current edit. Verification therefore has three explicit levels rather than one expensive all-purpose command.

## Levels

- `Fast` builds once and runs tests matching one required case-insensitive displayed-name substring. It may fail fast and may write a local timing report. It does not produce conformance evidence.
- `Standard` builds once, runs all 45 in-process conformance tests, and writes the normal host report. It stops before native CLI qualification.
- `Qualification` is the default and remains the milestone gate. It adds every native CLI, hosted-boundary, deterministic-artifact, and failure-preservation check.

Qualification builds the CLI once and invokes the resulting `windvale.dll` directly in each separate process. This preserves command parsing, process exit codes, native file behavior, capability boundaries, and output checks while removing repeated `dotnet run` project evaluation.

GitHub runs the Windows and Linux qualification jobs concurrently. Exact milestone qualification still uses the committed source archive on Windows and the real Debian QA host and compares their normalized reports and direct artifacts.

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

## Qualification evidence

The optimized verifier was qualified from the exact pre-normalization commit `de88007b4716c88604321baaad4c4d5c417d317e`, archived as `windvale-verification-speed-de88007.tar.gz`, 362,715 bytes with SHA-256 `dc5fab40a06a3f19706923fa3f569178297cd97bda5b9a8dc9e2b9c128942b92`. The attribution migration ledger maps it to tree-identical normalized commit `00466fd9e9feaac4655cdf9748ac1dc56b586a84`.

Windows x64 completed Qualification in 481.3 seconds with a 253.025-second suite; Debian GNU/Linux 12 x64 completed it in 501.8 seconds with a 270.660-second suite. Both hosts passed all 45 tests, the complete native CLI verifier, and zero-warning Release builds. Their normalized conformance reports matched, and all 42 directly compared artifacts, totaling 3,102,891 bytes, were byte-identical. The Windows report SHA-256 is `2f9f0f62b8a98e411500ef34fe697936808c74b39bcfd915c6de780f6fffd1ff`; the Debian report SHA-256 is `bd514ce1a9ba154cde689e4b1cf4cac23f0b5c50d21b5e58e40074022a04e5dd`. After retrieving the evidence, the exact Debian QA directory and transferred archive were removed and confirmed absent.

## Evidence rules

- Timing values are diagnostic host evidence, not portable semantics, and never enter the conformance contract.
- A filtered or fail-fast run cannot write a conformance report.
- Standard and Qualification always run all registered tests.
- Cached artifacts may accelerate local experiments but cannot replace a clean exact-commit qualification build.
- A faster implementation may replace demonstrated overhead; it may not silently remove independent verification, hostile-input coverage, or direct native-boundary checks.
