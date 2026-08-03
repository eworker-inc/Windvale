# Decision 0152: First Wasm-hosted WVB scalar interpreter

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0149](0149-Windvale-Native-WebAssembly-Wvb-Executable-Verifier.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0149 removed the Stage 0 verifier from the execution-admission boundary for bounded compiler-produced WVB, but the editable playground still had no Windvale-authored Wasm component that could execute the admitted bytecode. The existing direct backend is valuable qualification evidence and an eventual optimization path, but widening it into a second general compiler before the browser can execute ordinary WVB would duplicate semantics and delay the shortest route away from .NET.

The first interpreter should therefore consume canonical WVB as data, preserve the reference runtime's scalar semantics, and remain independently replaceable from the verifier. Combining the complete verifier and interpreter into one profile-13 source module would exceed the retained aggregate-code ceiling by approximately one KiB. Separate digest-pinned Wasm components also make the trust and recovery boundary clearer: verify first, then execute only the accepted bytes.

## Decision

- Add `Wvb-Scalar-Interpreter-Main.wv` as one portable `Main(bytes) -> bytes` function and lower it as an import-free execution-ABI-3 WebAssembly module.
- Keep the Decision 0149 executable verifier and this interpreter as separate artifacts. A conforming worker must run the complete verifier first and must not present unverified WVB to the interpreter. The interpreter's bounded preflight selects its execution subset; it is not a replacement untrusted-input verifier.
- Add profile 14 to the Windvale-authored WebAssembly selector. It retains the single-function profile-11 runtime-value, control, memory, metering, and output contracts while adding checked `i32` addition, subtraction, multiplication, and negation; all six signed `i32` comparisons; and checked `u32` multiplication.
- Use a versioned little-endian `WVXI 1` request: four-byte magic, `u16` version, zero `u16` reserved field, `u32` guest instruction budget, `u32` maximum guest call depth, and the exact verified WVB bytes. Guest budget is one through 4,096 and call depth is one through eight.
- Return successful interpreter outcomes in a fixed twenty-byte `WVXO 1` value: magic, version, reserved field, `u32` guest status, `u32` charged guest instructions, and the four-byte `i32` result. Guest instruction exhaustion returns status `3011`; guest call-depth exhaustion returns `3004`. Checked arithmetic overflow propagates as execution-ABI status `3007` with no output.
- Admit only portable, capability-free WVB 1.6 with no nominal types, one through sixteen functions, no more than eight parameters per function, no more than thirty-two parameters plus locals per frame, declared stack depth at most sixteen, and at most 4,096 aggregate decoded instructions. `Main() -> i32` is required.
- Interpret deterministic default-valued scalar locals, `i32`, `u32`, `u8`, and `bool` constants and local movement; checked signed and unsigned arithmetic; scalar comparisons and boolean operations; `u32.from_u8`; `pop`; absolute `jump` and `branch.false`; direct calls; and returns. Fixed 128-byte local frames, four-byte value cells, a sixteen-value operand stack, and 144-byte saved frames keep all storage bounded.
- Treat a zero-length interpreter result after successful complete verification as “outside scalar interpreter profile” at the worker boundary. Capability execution, text, bytes, records, enums, and general nonempty-stack control joins remain later profiles.

## Consequences

The browser stack now has both halves of its first .NET-free WVB execution path: an independently pinned complete compiler-aligned verifier and a separately pinned bounded interpreter. Node.js proves the actual composition by verifying each candidate before passing the same bytes to the interpreter. This is execution of WVB as runtime data, not direct recompilation of each guest to Wasm.

The two-level budget is intentional. `Windvale.run` bounds the interpreter implementation itself, while `WVXI` bounds charged guest instructions. A hostile or unexpectedly expensive interpreter path therefore cannot escape the outer Wasm budget, and a valid nonterminating guest cannot escape its inner Windvale budget.

This does not switch the editable playground. The static worker still needs packaged verifier/interpreter assets, explicit orchestration and result mapping, then the Windvale compiler. Text, bytes, records, enums, capability authorization, cross-host construction, Chromium/Firefox/WebKit evidence, deployment recovery, and the default switch remain open gates.

## Local evidence

The interpreter source compiles to 25,568 WVB bytes with one function, 1,572 nonparameter locals, 23,823 code bytes, 5,208 instructions, maximum stack three, and SHA-256:

```text
f0f51936fec70d64f5d8021733b8d8312bce14ba411d90e4896ef19e317fb7a4
```

The profile-14 backend lowers it in exactly 82,657,852 Windvale instructions to a deterministic 145,469-byte import-free Wasm module with SHA-256:

```text
683410069c64d0143f748d34cb63f16b7d36c130662c282c003b981b24d37580
```

The existing `Function-Only.wv` fixture compiles to 815 WVB bytes with SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`. The reference runtime and interpreter both return `6` after exactly 199 guest instructions; the outer interpreter consumes 121,003 instructions. Guest budget 198 returns `3011/198`, call-depth limit one returns `3004/27`, and an outer budget of 121,002 returns `WVR3011` without output.

The 2,366-byte scalar coverage fixture has SHA-256 `6e8c2c29dc5f42d5dc2a7283604acca8d183325d9ae972c1dce3430aa7976414`. It covers all admitted scalar arithmetic and comparisons plus calls and control, and both runtimes return `42` after exactly 351 guest instructions. The outer interpreter consumes 270,950 instructions.

Dedicated signed-addition and unsigned-multiplication overflow candidates have WVB identities `f10665d894c6b7cd3d198dc50469f3320e0fad71442aded4bf4eae06a1a1c85c` and `fd0c02a650d54d6618f760a36613eb811c49b8304bd4a9ab20e94f03bd733d2d`. Both the reference runtime and generated Wasm report `WVR3007`; the interpreter paths charge 10,967 and 16,983 outer instructions and publish no output.

Before execution, the unchanged Decision 0149 verifier accepts the function/control, complete scalar, signed-overflow, and unsigned-overflow candidates in exactly 609,651; 3,056,208; 52,228; and 170,625 instructions. One instruction below the first verifier budget returns `WVR3011`.

The focused Seed WebAssembly test independently checks source/WVB shape, reference differential behavior, protocol bytes, all limits above, exact construction, and emitted Wasm structure. `Tools/Verify/Verify-WebAssembly.ps1` rebuilds and executes the complete twenty-nine-artifact gate under Node.js 24.18.0 on Windows. Every earlier Wasm artifact remains byte-identical.

Change-aware Windows verification completes a zero-warning Release build, passes the editor contract, and passes all 84 selected Seed tests in 363.365 suite seconds. The WebAssembly case takes 55.732 seconds and the qualification-only golden compiler contract takes 211.051 seconds. This is proportional integrated development feedback rather than cross-host or cross-browser qualification.

Extending the backend changes its own WVB identities to `1cc90cbe50b2605096c7002e52c5075423dbf3f567ee4bd70d141dee52e61008` for the core and `851f8122acc9314b1061db5a907841dbb4f9aa68641aedc6ef73f3d94f949cd3` for the composed tool. The unchanged complete verifier now takes 222,376,689 construction instructions under the larger selector while retaining its exact 722,837 Wasm bytes and digest.

## Rejected alternatives

Fusing verification and interpretation was rejected because the combined source narrowly exceeds the established profile-13 aggregate limit and would couple two independently useful trust artifacts.

Expanding direct WVB-to-Wasm lowering first was rejected because the interpreter is the smaller semantic bridge for arbitrary editable inputs and will also execute the Windvale-written compiler.

Treating the interpreter's preflight as an untrusted-input verifier was rejected because it intentionally depends on Decision 0149 for complete range, identity, type-flow, and control-flow proof.

Adding text, byte descriptors, records, enums, or capabilities to the first execution slice was rejected so scalar value representation, call frames, dual budgets, and failure propagation could be measured independently.

## Reconsider when

- Text, bytes, records, or enums require a shared dynamic-value representation across guest frames.
- General WVB control joins require a bounded runtime operand-stack shape beyond the compiler-aligned verifier contract.
- Direct lowering has enough complete-language evidence to replace interpretation for startup or throughput without forking semantics.
- The verifier and interpreter have matching Windows/Linux construction and Chromium, Firefox, and WebKit execution evidence.
