# Decision 0158: Wasm-hosted WVB formatting and quoting

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0157](0157-Wasm-Hosted-Wvb-Text-And-Bytes-Values.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0157 established bounded immutable text and bytes values in the complete-verifier-approved Wasm-hosted WVB interpreter. Running the Windvale compiler also requires invariant scalar formatting and deterministic text quoting. These are pure operations, but they cannot be delegated to JavaScript without making browser string behavior part of Windvale semantics.

The selector already admits every arithmetic, byte, control, and allocation operation needed to implement both algorithms inside the Windvale-authored interpreter. Expanding the guest operation set therefore does not require a new WebAssembly selector profile, execution ABI, protocol version, host import, or memory limit.

## Decision

- Expand the retained profile-15 `Wvb-Scalar-Interpreter-Main.wv` consumer to interpret WVB opcodes `i32.format`, `u8.format`, `u32.format`, and `text.quote`.
- Keep the complete Decision 0149 verifier as the mandatory first stage and retain `WVXI 1`, `WVXO 1`, execution ABI 3, the eight-byte guest cell, fixed frames, 16 KiB individual-value ceiling, and append-only 64 KiB guest heap.
- Format signed and unsigned values as invariant ASCII decimal with no grouping or leading zeroes. Signed minimum is converted through its raw two's-complement magnitude rather than checked negation, so `-2147483648` is admitted exactly.
- Quote text as the existing deterministic JSON-style report representation. Quote, reverse solidus, backspace, form feed, line feed, carriage return, and tab use two-byte short escapes. Printable ASCII U+0020 through U+007E is preserved. Every other UTF-16 code unit is emitted as uppercase `\uXXXX`; a supplementary UTF-8 scalar becomes the exact high- and low-surrogate pair used by the reference runtime.
- Allocate each formatted or quoted result in the existing guest heap. Aggregate exhaustion returns `WVR3018`; a quoted result above the interpreter's 16 KiB value ceiling returns profile status `WVR3015` before publication.
- Continue to reject `bytes.sha256_hex`, records, enums, capabilities, recursion, reclaiming allocation, and general nonempty-stack joins as outside the interpreter profile.

## Consequences

The Wasm-hosted interpreter now executes the compiler-produced data/text fixture that previously stopped at `text.quote`. Formatting and quoting remain Windvale semantics implemented in Windvale source and lowered to import-free Wasm; neither browser strings nor host locale participate.

This remains a bounded interpreter profile. Its append-only heap can reject allocation-heavy valid WVB earlier than the reference runtime, and its 16 KiB quote ceiling is deliberately smaller than the general 1 MiB text contract. Those differences are explicit resource failures rather than semantic substitutions.

SHA-256 is now the only pure text/bytes opcode still missing. Unlike formatting and quoting, the current Windvale source vocabulary has no bitwise operations; the Foundation contract already identifies that limitation. The next slice should therefore measure an import-free target implementation deliberately rather than conceal JavaScript Web Crypto behind the worker boundary.

## Local evidence

The expanded interpreter source compiles to 52,942 WVB bytes with one function, 3,193 nonparameter locals, 49,576 code bytes, 10,834 instructions, maximum stack three, and SHA-256:

```text
24b6b4164a140cc2eabf74c940683b7e82ae3607a0db4952f3a939b29c82940c
```

The unchanged profile-15 backend lowers it in exactly 177,554,863 Windvale instructions to a deterministic 306,560-byte import-free Wasm module with SHA-256:

```text
c43569edb77a841388720ab23b144e3873bca08bd7b5a9ffb5800fcbc5bc9924
```

The 1,639-byte formatting/quoting fixture has SHA-256 `1f2458fe89edd7853b8c3e92008c897894e293024d735ad1d43b534c1d214ac9`. It covers signed minimum, signed maximum, signed zero, unsigned maximum, byte maximum, every short escape, printable ASCII, DEL, two- and three-byte BMP values, and one supplementary scalar. The reference runtime and interpreter both return `42` after exactly 4,070 guest instructions; the interpreter consumes 2,786,832 outer instructions. The complete verifier admits it in exactly 2,479,517 instructions.

The retained 1,651-byte compiler-produced data/text fixture has SHA-256 `5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704`. It now passes the complete verifier in 4,181,579 instructions and executes through the interpreter as result `13` after 233 guest and 276,749 outer instructions.

A 335-byte SHA-256 boundary fixture has SHA-256 `4d62874485fd3cfa1a3a9c985a253a475de7306e53cc7b244514031dd5475061`. The reference runtime returns `42` after 28 instructions, and the complete verifier admits it in 164,074 instructions. The interpreter rejects it during operation selection with empty output after 4,254 outer instructions, proving that the remaining boundary is explicit and still follows complete verification.

The focused Seed WebAssembly case checks source/WVB shape, exact formatting and UTF-16-compatible quoting, reference differential results, all retained failures, deterministic lowering, and an independent complete emitted-Wasm decode. The complete twenty-nine-artifact `Tools/Verify/Verify-WebAssembly.ps1` gate rebuilds every output, proves that the earlier twenty-eight generated Wasm artifacts remain byte-identical, validates and instantiates every module without imports, and passes under Node.js 24.18.0 on Windows.

Change-aware verification completes a zero-warning Release build and passes all 86 selected Seed tests in 372.155 suite seconds; the WebAssembly and golden cases take 63.474 and 211.347 seconds. The complete command finishes in 379.8 seconds. This remains local development feedback, not cross-host or browser qualification evidence.

## Rejected alternatives

Calling JavaScript number/string formatting was rejected because locale, coercion, and host representation would enter the Windvale semantic boundary.

Calling `JSON.stringify` was rejected because Windvale's quoted report form is defined over UTF-16 code units with exact uppercase escapes and deliberately does not inherit a host JSON implementation.

Labeling this expansion profile 16 was rejected because the selector's accepted outer WVB operations, limits, execution ABI, and emitted-module contract are unchanged. This decision broadens the guest interpreted by the existing profile-15 artifact.

Implementing SHA-256 through Web Crypto in the same slice was rejected because it would add a host import to a currently import-free pure runtime path and would not prove Windvale-controlled semantics.

## Reconsider when

- A dedicated import-free SHA-256 lowering demonstrates a smaller or clearer boundary than adding general Windvale bitwise primitives.
- Quote-heavy compiler workloads prove that the 16 KiB per-value or 64 KiB aggregate heap limits are too small for a useful bounded request.
- Records and enums require a value representation incompatible with the current uniform eight-byte cell.
- The verifier and interpreter have matching Windows/Linux construction and Chromium, Firefox, and WebKit execution evidence.
