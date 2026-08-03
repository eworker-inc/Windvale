# Decision 0119: First Windows console application target

- Date: 2026-08-02
- Status: Windows qualification passed locally; cross-host deterministic construction pending
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
- Use the existing default instruction and call-depth budgets. Return a successful `i32` as the process result and map every packed trap status to result `1`.
- Independently verify the complete PE container, entry stub, relative targets, context, arenas, relocation block, sizes, permissions, and padding before publication.
- Keep source-to-WVB compilation and native packaging in Stage 0. The produced executable loads no .NET runtime, but its construction is not a native bootstrap.

## Initial evidence

On Windows, the current implementation produces a deterministic 5,120-byte executable from `Examples/Seed/Sum-Data.wv`, SHA-256 `c6c4568f0a47e36ce8fdb145f4c3de3ce9a28bb2fb1935add75d44e48a2ac805`. Windows loads it directly and reports process result `29`. Focused tests also execute the existing nominal-record and dynamic-byte fixtures to result `42`, proving both fixed arena pointers, and map a checked-overflow native status to process result `1`.

The same focused case covers deterministic repetition, independent recovery of exact native bytes and `Main`, required-service and descriptor-entry rejection, changed-fragment rejection, truncated/oversized/trailing files, targeted corruption in every PE/startup/context/relocation class, and bounded random hostile input. The complete local Windows qualification gate passes a zero-warning Release build, all 71 Seed tests including the golden compiler contract, all 25 OS tests, Linux-x64 CLI publication, and the complete native CLI path including exact PE construction and Windows execution. This is Windows evidence, not Windows/Linux cross-host qualification.

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
