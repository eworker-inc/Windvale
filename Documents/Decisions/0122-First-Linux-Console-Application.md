# Decision 0122: First Linux console application target

- Date: 2026-08-02
- Status: Implemented candidate; direct Debian execution and dual-host qualification pending
- Target: `linux-x64-console-v1`
- Retains: Canonical WVB 1.6, native ABI 20/context 7, WVO 1.0, `flat-x86-64-v1`, the 4 MiB object/link limits, and the .NET retirement gate

## Context

Decision 0119 proved the first directly loadable Windows process container for capability-free scalar Windvale programs. Linux remained dependent on the in-process W^X adapter even though Windows and Linux are permanent hosts and already execute the same verified native fragment.

A literal PE translation would be misleading. Linux enters an ELF at `_start` rather than through a loader-owned callable function, reports process results through `exit`, and does not enforce a requested stack extent from `PT_GNU_STACK`. Inheriting the invoking shell's stack limit would weaken ABI 20's retained 1,024-call budget.

## Decision

- Add target `linux-x64-console-v1` to `windvale compile`, defaulting to `.elf`.
- Reuse the exact shared native-fragment verification, WVO production, and base-zero flat-link reproduction required by the Windows target.
- Package the result as a deterministic sectionless ELF64 static PIE with separate read-only header, read/execute code, and read/write data loads.
- Include no interpreter, dynamic table, imported symbol, relocation, libc, or .NET runtime dependency.
- Own a 64 MiB private anonymous stack through Linux x86-64 `mmap` syscall 9 before invoking Windvale code; terminate only through `exit` syscall 60.
- Preserve successful process results from `0` through `255` and map every other successful `i32` or native failure to `1`, matching the Windows target without changing source-level `Main() -> i32` semantics.
- Retain the ABI-20 context, fixed 2 MiB record arena, 16 MiB dynamic-value arena, default instruction budget, and default call-depth budget.
- Carry format version 1 in an exact `Windvale` ELF note and independently verify the complete container, load permissions and extents, startup/syscall sequence, relative targets, context, and padding before publication.
- Require PE and ELF recovery to produce the same native bytes and `Main` offset.
- Set executable mode `0755` when the CLI runs on Linux. Treat Unix mode as installation metadata rather than part of deterministic artifact bytes.
- Stage the complete verified ELF and its executable mode under a unique sibling name before one atomic replacement so a prepublication failure leaves the requested output missing or unchanged.
- Keep the C# implementation as Stage 0 oracle/recovery code. Name a portable `.wv` constructor/verifier and `.wva` startup template as the next ownership transfer rather than treating C# as permanent product code.

## Initial evidence

On Windows, the implementation deterministically constructs an 8,304-byte ELF from `Examples/Seed/Sum-Data.wv`, SHA-256 `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`. An external file classifier identifies it as a statically linked, sectionless, x86-64 ELF shared object. The independently verified PE and ELF containers recover byte-identical native images and the same `Main` offset. Decision 0124 moves the exact startup candidate into WVA while retaining the C# writer as an independently checked recovery oracle.

The focused construction test passes a zero-warning build and covers deterministic repetition; malformed fragment, hosted-service, and descriptor-entry rejection; truncated, oversized, trailing, and targeted corruptions across every ELF/header/note/startup/context/padding diagnostic class; and bounded random hostile inputs. Linux-only branches directly execute sum, nominal-record, dynamic-byte, checked-overflow, and the six shared process-result boundary fixtures. This Windows construction result does not claim those branches have run; direct Debian execution and the repository's dual-host Qualification gate remain pending.

## Consequences

Windvale now has symmetric narrow source-to-host-container contracts for Windows PE32+ and Linux ELF64 without changing portable source semantics or native bytes. The Linux target owns its required stack and termination syscalls explicitly instead of importing libc or ambient host policy.

The target introduces Linux-specific system behavior only in its exact startup adapter. Portable `Main` remains capability-free and cannot invoke syscalls, observe arguments, or depend on Linux state.

The paired adapters expose a measured Windvale-ownership task: reproduce two concrete byte constructors and untrusted-input verifiers in `.wv`, with startup machine templates in `.wva`, while retaining C# as an independent oracle until the native-retirement gate permits recovery-only status.

## Reconsider when

- Direct Debian execution disagrees with the independently verified container contract.
- A supported Linux architecture or kernel rejects the sectionless static-PIE layout or anonymous stack policy.
- Fixed arena or stack reservation materially harms the first useful application cases.
- Hosted output, arguments, files, detailed traps, seccomp, signing, unwind metadata, or native compiler packaging becomes the next measured requirement.
