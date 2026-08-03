# Decision 0119: First Windows console application target

- Date: 2026-08-02
- Status: Cross-host qualified at `ea1aa89`
- Numbering note: This historical record shares number 0119 with [Expanded WVA x86-64 foundation](0119-Expanded-Wva-X64-Foundation.md); references must use the complete title and filename.
- Target: `windows-x64-console-v1`
- Retains: Canonical WVB 1.6, native ABI 20/context 7, WVO 1.0, `flat-x86-64-v1`, the 4 MiB object/link limits, and the .NET retirement gate

## Context

Windvale could already compile portable source to verified WVB, lower representative and compiler-scale WVB through the shared x86-64 backend, emit WVO, link deterministic flat images, and execute native fragments through Windows/Linux W^X host adapters. It still had no Windows process container. The CLI could produce `.wvb`, `.wvo`, or a raw `.bin`, but no file Windows could load directly.

Waiting for complete native compiler reproduction would leave the process-container boundary untested and couple several independent risks. The first executable should instead prove one exact vertical slice without claiming hosted I/O, a general runtime, or .NET retirement.

## Decision

- Add target `windows-x64-console-v1` to `windvale compile`.
- Accept only portable, capability-free native fragments with scalar `Main() -> i32` and no required runtime services.
- Reuse the existing native fragment verifier, WVO sink, and base-zero flat linker. Require linked bytes and entry evidence to reproduce the verified fragment exactly.
- Package the result as a deterministic, import-free PE32+ console application with a fixed RIP-relative entry stub.
- Carry ABI-20 context version 7 plus fixed 2 MiB record and 16 MiB dynamic-value arenas in a writable loader-zeroed section.
- Use the existing default instruction and call-depth budgets. Preserve successful results from `0` through `255` and map every other successful `i32` or packed trap status to process result `1`, matching Linux without changing source semantics.
- Independently verify the complete PE container, entry stub, relative targets, context, arenas, relocation block, sizes, permissions, and padding before publication.
- Stage the complete verified executable under a unique sibling name and publish it through one atomic replacement so a prepublication failure leaves the requested output missing or unchanged.
- Keep source-to-WVB compilation and native packaging in Stage 0. The produced executable loads no .NET runtime, but its construction is not a native bootstrap.

## Initial evidence

On Windows, the current implementation produces a deterministic 5,120-byte executable from `Examples/Seed/Sum-Data.wv`, SHA-256 `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77`. Windows loads it directly and reports process result `29`. Focused tests also execute the existing nominal-record and dynamic-byte fixtures to result `42`, proving both fixed arena pointers, map a checked-overflow native status to process result `1`, and require exact portable process results at `0`, `1`, and `255` while mapping `-1`, `256`, and `2,147,483,647` to `1`. Decision 0124 moves the exact startup candidate into WVA while retaining the C# writer as an independently checked recovery oracle.

The same focused case covers deterministic repetition, independent recovery of exact native bytes and `Main`, required-service and descriptor-entry rejection, changed-fragment rejection, truncated/oversized/trailing files, targeted corruption in every PE/startup/context/relocation class, and bounded random hostile input. It passes after the portable-result update with a zero-warning build and direct Windows execution.

Exact descendant `ea1aa89ba204ead633f8340c61b2bacc716881fd` passes GitHub [Verify run 30783457203](https://github.com/eworker-inc/Windvale/actions/runs/30783457203). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 77 Seed tests, all 31 OS tests, the golden compiler contract, and the native CLI gate. The current-host branches directly execute the canonical PE on Windows and its paired ELF on Linux, including the normalized process-result corpus; both hosts reproduce and independently verify the exact version-1 artifacts. This qualifies the Windows target, shared result semantics, and atomic publication path as part of the paired baseline.

## Consequences

Windvale now has a real `.wv` to Windows `.exe` path for a documented portable scalar subset. The result is useful as an executable-boundary proof and a target for further differential testing.

The 18 MiB fixed writable mapping is intentionally simple and is not a small-application memory design. Later evidence may derive smaller per-program arenas from verified native requirements.

Returning from the import-free primary entry thread is the only Windows termination boundary in version 1. Adding output, arguments, explicit process termination, or detailed traps will require an owned import/native-service policy and a new target contract.

The exact compiler remains outside this file path because its 4,556,121-byte fragment exceeds the 4 MiB WVO/link limit and ABI 20 still uses monotonic record allocation for full bootstrap. This decision does not advance ABI 21 or any .NET-retirement condition beyond the narrow standalone application evidence.

## Reconsider when

- Cross-host construction differs despite identical source, tool versions, and options.
- Windows loader behavior requires an explicit termination import or a different process-entry contract.
- Fixed arena reservation materially harms the first useful application cases.
- Hosted output, arguments, files, embedded WVB verification, code signing, unwind metadata, or native compiler packaging becomes the next measured requirement.
