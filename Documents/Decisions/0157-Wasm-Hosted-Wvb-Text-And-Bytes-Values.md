# Decision 0157: Wasm-hosted WVB text and bytes values

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0152](0152-First-Wasm-Hosted-Wvb-Scalar-Interpreter.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0152 established the first complete-verifier-approved WVB execution path in import-free WebAssembly, but its interpreter admitted only scalar values. The Windvale compiler and ordinary applications also depend on bounded `text` and `bytes` values, static data, descriptor transfer through locals and calls, strict UTF-8, byte construction, and explicit allocation failures. Those semantics must be present before the compiler itself can execute in the same Wasm-hosted runtime.

The next slice should broaden the retained interpreter without weakening the separate verifier trust boundary, changing the public execution ABI, or introducing host garbage-collection behavior. Its allocation and value limits must be small enough to inspect and test exactly.

## Decision

- Add profile 15 to the Windvale-authored WebAssembly selector. It retains profile 14's execution ABI 3 and operations while expanding the single accepted runtime function to at most 4,095 nonparameter locals and 65,536 code bytes. The 100,000 decoded-instruction and 524,288-byte generated-Wasm limits remain unchanged.
- Retain `Wvb-Scalar-Interpreter-Main.wv` as the interpreter artifact name and the version-1 `WVXI` request and `WVXO` result formats. The guest remains a portable, capability-free WVB 1.6 module with `Main() -> i32`; text and bytes may appear in static data, locals, parameters, internal call results, and computations that contribute to that scalar result.
- Keep the complete Decision 0149 verifier as the mandatory first stage. The interpreter's bounded preflight remains a profile selector, not an untrusted-input verifier. Successful verification followed by empty interpreter output means the candidate is outside profile 15.
- Represent every guest local and operand-stack value in an eight-byte cell. Scalars occupy the low four bytes with a zero high half. Text and bytes use an unsigned heap-offset/length descriptor. Each of the at most eight frames owns 128 local cells and sixteen operand-stack cells, for a fixed 1,040-byte frame.
- Use one zero-based, append-only 65,536-byte guest heap. Static text or bytes are copied into this heap when their constant instruction executes. Constructed values are independently limited to 16,384 bytes; aggregate allocation is limited by the heap. Slices are borrowed descriptor views and do not allocate.
- Interpret `data.length` and `data.load.i32`; `text.const`, `bytes.const`, and descriptor movement through locals, parameters, calls, and returns; byte length, slice, unsigned 8/16/32-bit and signed 32-bit little-endian reads; byte concatenation and 8/16/32-bit constructors; text concatenation; strict `text.valid_utf8` and `text.from_utf8`; and `text.to_utf8` descriptor reinterpretation.
- Validate UTF-8 inside the interpreter with exact rejection of malformed continuations, truncation, overlong encodings, surrogate code points, and values above U+10FFFF. Empty, ASCII, two-byte, three-byte, and four-byte values are admitted.
- Preserve the reference runtime's explicit failures: `WVR3008` for range errors, `WVR3014` for invalid UTF-8, and `WVR3016` for out-of-range u16 construction. Use `WVR3015` for the interpreter's per-value limit and `WVR3018` for aggregate heap exhaustion. Every failure publishes a `WVXO` guest status with the charged guest count and no result.
- Keep invariant scalar formatting opcodes 110 through 112, `text.quote` opcode 117, `bytes.sha256` opcode 125, records, enums, capabilities, recursion, reclaiming allocation, and general nonempty-stack control joins outside profile 15.

## Consequences

The Wasm-hosted runtime can now execute the core immutable text/bytes behaviors used by ordinary compiler-produced WVB, including values crossing function frames. The representation is deliberately uniform and inspectable: calls copy eight-byte cells, slices borrow one immutable heap extent, and every constructed value has an exact charge.

The heap is an experimental interpreter resource, not the permanent Windvale dynamic-value ABI. It does not reclaim values, so allocation-heavy valid programs can exhaust the 64 KiB aggregate limit even when individual values are small. That bounded failure is preferable to hidden host allocation while the compiler execution profile is being measured.

This slice does not switch the editable playground. Formatting, quoting, hashing, records, and enums still block broad compiler/runtime coverage. The compiler artifact, verifier, interpreter, result execution, and capability policy must then be packaged into one disposable static worker before the .NET-backed editable path can be retired.

## Local evidence

The extended interpreter source compiles to 43,908 WVB bytes with one function, 2,645 nonparameter locals, 41,090 code bytes, 8,969 instructions, maximum stack three, and SHA-256:

```text
633745ec1c7e8dbd3daf0439637b46c31b1ae53b17d5dd9a2c7d4a2c41638579
```

The profile-15 backend lowers it in exactly 147,410,612 Windvale instructions to a deterministic 253,707-byte import-free Wasm module with SHA-256:

```text
57cc1c9c8a27cca63aaba23716c543450b0cfee5172dd6a1c01db246a637f78c
```

The positive text/bytes fixture is 2,021 WVB bytes with SHA-256 `4ec4c9b17097ef6757ae8bcef4dd0adbed0fe86f14860e26e6df6a1448bead8a`. It covers static data, byte reads, slices, constructors, concatenation, strict UTF-8, text/byte conversion, descriptor calls, and scalar reduction. The reference runtime and interpreter both return `42` after 298 guest instructions; the interpreter consumes 327,758 outer instructions.

A separate 1,744-byte UTF-8 boundary fixture has SHA-256 `45409d0676db8db96138061ca6f17a77e232ef93055ff2d7eacbe5fc17ba38f4`. It covers valid empty, ASCII, two-byte, three-byte boundary, and four-byte boundary forms plus overlong, surrogate, above-maximum, stray-continuation, and truncated failures. Both runtimes return `42` after 153 guest instructions; the interpreter consumes 208,701 outer instructions.

Dedicated invalid-UTF-8, range, and u16-narrowing candidates agree with the reference runtime on `3014`, `3008`, and `3016`. Dedicated per-value and aggregate-heap candidates return interpreter-profile statuses `3015` and `3018`; the reference runtime succeeds because it does not share these intentionally smaller experimental resource ceilings.

Before interpretation, the complete verifier accepts the positive, UTF-8-boundary, invalid-UTF-8, range, u16, per-value, and heap candidates in exactly 7,204,207; 5,394,389; 64,350; 72,820; 44,601; 405,013; and 598,548 instructions. A verified fixture containing `text.quote` is rejected by the interpreter profile after 36,985 outer instructions, proving that successful complete verification does not bypass operation selection.

All twenty-eight earlier generated Wasm artifacts remain byte-identical. The focused Seed WebAssembly test and the complete twenty-nine-artifact `Tools/Verify/Verify-WebAssembly.ps1` gate pass under Node.js 24.18.0 on Windows. The latter rebuilds, validates, instantiates without imports, and executes every retained output. This is local development evidence rather than cross-host or cross-browser qualification.

Change-aware Windows verification also passes the editor contract, a zero-warning Release build, and all 86 selected Seed tests in 368.839 suite seconds. The WebAssembly case takes 60.686 seconds and the qualification-only golden compiler contract takes 207.514 seconds. This is proportional integrated development feedback rather than conformance or cross-host qualification.

The expanded backend compiles to core WVB SHA-256 `9567208c3547cca63fd7ac3f71f31cf1761b1e255884ba56a1bef47668c8cea3`; the composed tool has WVB SHA-256 `e3cb41c7724130ed18c20af7e823a80f76ca7fafd3a0befef0cecb790c9e305f`.

## Rejected alternatives

Passing JavaScript strings or arrays directly into the interpreter was rejected because that would make host object behavior part of Windvale execution semantics and would not exercise WVB descriptor transfer.

Using the full 4 MiB execution-ABI arena for guest construction was rejected for this slice because a smaller 64 KiB heap gives faster exact exhaustion evidence and keeps frame, value, and aggregate limits independently visible.

Adding records and enums at the same time was rejected so dynamic descriptor lifetime, UTF-8, call transfer, and allocation failures could be qualified before nominal identity and aggregate layout are introduced.

## Reconsider when

- Running the Windvale compiler proves that 16 KiB values or a 64 KiB append-only heap are too small for a useful bounded compilation request.
- Records and enums require a tagged cell or ownership rule that cannot preserve the current eight-byte descriptor contract.
- Reclaiming allocation is needed to execute bounded compiler workloads without avoidable `WVR3018` failures.
- The verifier and interpreter have matching Windows/Linux construction and Chromium, Firefox, and WebKit execution evidence.
