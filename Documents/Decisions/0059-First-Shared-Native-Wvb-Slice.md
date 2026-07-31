# Decision 0059: First shared native WVB slice

- Date: 2026-07-31
- Status: Qualified at `962bb854fd8af195ec859c89355bae0e6f85ff33`
- Refines: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)'s shared native backend and differential-execution direction

## Context

The existing x86-64 kernel compiler is a deliberately bounded system-profile target for the UEFI handoff. It proves that typed Windvale source can reach verified WVO and execute after firmware exit, but its handoff validation, console adapter, and kernel entry shape do not define a general WVB JIT or AOT backend.

The next step must establish the shared seam before adding a broad instruction set. A large first compiler would mix verified-bytecode lowering, native ABI choices, instruction selection, object serialization, executable-memory policy, runtime services, and diagnostics before any one boundary had independent evidence. A constant-return function is enough to prove those boundaries and the complete interpreter/JIT/AOT differential route without pretending that arithmetic, calls, control flow, data, capabilities, traps, or memory management already exist.

## Decision

- Add a compiler-owned `Compiler/Native` project containing the shared native machine IR, versioned fragment metadata, typed symbols and patches, the first x86-64 selector, an independent fragment verifier, and the WVO sink.
- Add a runtime-owned `Runtime/Windvale.Native` project containing the Windows/Linux executable-memory adapter. The adapter validates the complete fragment, allocates writable non-executable memory, applies checked patches, copies code, transitions the mapping to read/execute, flushes the Windows instruction cache, publishes the entry only afterward, and releases the mapping after execution.
- Accept only an already verified `Verifiedˉmodule`. The WVB decoder and semantic verifier remain mandatory before the native backend can be called.
- Give the first fragment contract target name `x86-64-wvb-baseline-v1` and native ABI version `1`. The contract is internal and experimental; it is not a serialized replacement for WVB or WVO.
- Define the first machine IR with an explicit `Nativeˉi32ˉconstant` definition and `Nativeˉreturn` use. The selected x86-64 entry follows the shared zero-argument `i32` return convention used by both Windows x64 and System V x86-64 for this shape.
- Accept exactly one portable exported `Main() -> i32`, no capabilities, data, nominal types, or user locals, and the canonical verified WVB sequence `i32.const`, `local.store 0`, `local.load 0`, `return`. Any wider shape fails with a stable `WVN2xxx` diagnostic rather than falling back.
- Emit the same verified native fragment to both sinks. The in-memory path executes it directly; the AOT path serializes it as WVO, passes it through the existing verified linker, reconstructs an executable fragment from the linked image, and executes that image through the same W^X adapter.
- Keep typed patch records in the fragment contract now even though the first accepted function has no patches. Fragment validation checks symbol ordering, ranges, patch targets, non-overlap, bounds, and zero placeholders, then independently restricts the executable first target to the exact safe `mov eax, imm32; ret` shape with one `Main` symbol and no patches. The executor implements checked internal absolute-u32 and relative-i32 patch application for future accepted target shapes and rejects unresolved imports.

## Qualification evidence

The Windows and Debian x64 focused differential test compiles a portable function returning `42`. The reference interpreter, in-memory native fragment, and WVO-linked AOT image all return `42` on both hosts. Selection produces the exact six code bytes:

```text
B8 2A 00 00 00 C3
```

The WVO is 79 bytes with SHA-256 `d69ab30a34a7281ff9911ab89220b405ad0944ede5130dd6f07c44baac1b9d6a`. Two independent compilations and WVO serializations are byte-identical. The focused test also rejects an out-of-range patch, a non-accepted machine-code shape before publication, and an unsupported arithmetic program. The complete solution builds with zero warnings and errors.

Exact commit `962bb854fd8af195ec859c89355bae0e6f85ff33`, tree `f617e322bd01d533f3e139131f9dfded74037ccb`, was archived as `windvale-native-962bb854fd8a.tar.gz`, 2,776,515 bytes with SHA-256 `8f8641289dcbd00092e598b6f5977e0a0eb0ef70e9957c5a5d792c92bc205c3f`. The same digest was verified before extraction on the isolated E-Worker Debian QA host. Windows x64 and Debian GNU/Linux 12 x64 with .NET SDK `10.0.302` both passed zero-warning Release builds, all 49 tests, and the complete native CLI verifier. Their normalized conformance contracts matched, and all 61 directly retrieved portable artifacts totaling 7,752,612 bytes were byte-identical. Complete evidence is recorded in [Seed verification evidence](../Project/Seed-Verification-Evidence.md#first-shared-native-wvb-slice-qualification).

## Consequences

Windvale now has the first general backend boundary shared by a WVO/AOT sink and a baseline in-memory execution sink. The generated code itself runs natively, but the current compiler, fragment verifier, executor orchestration, tests, and host process are still C#/.NET Stage 0. This does not satisfy any remaining .NET-retirement condition by itself.

The next backend slices should extend the same machine IR and fragment path in evidence-driven order: checked i32 arithmetic, control flow, internal calls, static data, runtime-service calls and traps, then the value and memory contracts needed by broader Windvale programs. A new path must not bypass verified WVB, create a second patch model, or use writable/executable memory overlap.

## Reconsider when

- The canonical WVB temporary sequence changes and a different verified lowering boundary is simpler.
- The first control-flow or call slice shows that the two-operation machine IR cannot evolve without hidden stack semantics.
- A host requires a signed or out-of-process compilation path instead of in-process executable memory.
- A later architecture cannot share the fragment, symbol, patch, and ABI-versioning contracts without architecture-specific leakage.
