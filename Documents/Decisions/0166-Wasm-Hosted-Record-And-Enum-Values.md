# Decision 0166: Wasm-hosted record and enum values

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0162](0162-Import-Free-WebAssembly-Sha256-Lowering.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0162 completed the Windvale compiler's pure scalar, text, bytes, formatting, quoting, UTF-8, and SHA-256 operations in the Wasm-hosted WVB interpreter. Compiler-produced WVB still could not execute when a function signature, local, or instruction used a nominal record or enum. This was the final required runtime-value family before attempting compiler execution.

The complete Windvale-native verifier already proves nominal declarations, identities, shapes, operands, calls, locals, and field/member indices. The interpreter therefore needs a bounded execution representation after that verifier, not a second semantic verifier. It must nevertheless preserve deterministic WVB defaults and explicit resource failures rather than inheriting JavaScript objects or host garbage collection.

## Decision

- Retain WebAssembly selector profile 16, execution ABI 3, `WVXI 1`, `WVXO 1`, fixed linear memory, the complete-verifier-first pipeline, the 4,096 guest-instruction ceiling, call depth eight, and the existing 65,536-byte dynamic text/byte heap.
- Parse the complete nominal-type payload and variable-width record/enum value shapes. Admit at most the WVB format limits of 1,024 nominal types, 64 fields per record, and 256 members per enum. Continue to reject guest `i64`, `u64`, capabilities, and unsupported code shapes.
- Keep every guest stack/local value in one eight-byte cell. Enums contain the canonical signed 32-bit backing bytes and nominal type index. Records contain an arena offset and nominal type index.
- Add one execution-scoped, append-only 4,096-byte record arena. Record construction copies declaration-ordered eight-byte field cells into the arena. Field access copies the selected cell. Exhaustion returns guest `WVR3017` after charging the failing instruction and publishes no result.
- Reserve record offset `0xFFFF_FFFF` as a default-record sentinel. A zero nominal local is resolved on load: records receive the sentinel and declared type; enums receive the first member's backing value and declared type. Field access over a sentinel derives the declared field default without eager record allocation.
- Execute WVB `record.create`, `record.field`, `enum.const`, `enum.equal`, `enum.not_equal`, and `enum.name`. Enum names are copied from verified module metadata into the existing charged dynamic heap and retain the established `WVR3015` / `WVR3018` failures.
- Precompute bounded function offsets, parameter counts, and code extents so nominal value shapes do not require an incorrect one-byte declaration scan during calls.

## Consequences

Compiler-produced records and enums now survive locals, calls, returns, comparisons, field access, and name lookup in the import-free Wasm interpreter. Default-valued nominal locals agree with canonical WVB behavior rather than being treated as uninitialized. JavaScript sees only the existing fixed memory ABI and numeric status; it does not allocate, inspect, or define Windvale nominal values.

This is an interpreter expansion, not profile 17. The accepted outer WVB vocabulary and limits did not change, execution ABI 3 did not change, and the previous generated Wasm artifacts remain byte-identical.

The 4 KiB arena is intentionally a measured bounded starting point, not a compiler-capacity claim. It exposes the next work honestly: run the actual compiler WVB and measure its function, frame, instruction, record-lifetime, dynamic-value, and capability requirements before expanding or replacing any bound.

## Local evidence

`Wvb-Scalar-Interpreter-Main.wv` compiles to 65,749 WVB bytes with SHA-256 `ce6b7d93896e88aac682c66b9bcaa695e159e582a2a5b3a4b84b48482e608de1`. Its one function has 4,026 nonparameter locals, 61,550 code bytes, 13,466 instructions, and maximum stack three. The unchanged profile-16 backend lowers it in exactly 279,819,074 Windvale instructions to a deterministic 404,340-byte import-free Wasm module with SHA-256 `8c23fe32341aaf37fb2bd0d517e531a03937f00ce416175976f76f59f5380b55`.

The existing 1,781-byte compiler-produced nominal fixture has SHA-256 `1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a`. The complete verifier accepts it in 3,250,582 instructions. The reference runtime and Wasm interpreter both return `11` after 197 guest instructions; the interpreter consumes 311,902 outer instructions. The program covers two records, two enums, record values through a call, all supported primitive/descriptor fields, equality, inequality, field access, and `enum.name`.

The 820-byte default-value fixture has SHA-256 `589b7d8d2cd2a22ccc02e32ea9a38051b3c23e725d325b8c778ef4893d534fae`. Its ordinary form passes the complete verifier in 523,319 instructions and returns `42` after 67 guest and 96,474 outer instructions. A typed mutation redirects the first record load to a later default-valued local; the complete verifier accepts that exact module in 523,386 instructions, and both runtimes follow the default record's first enum member to result `2` after 37 guest and 71,515 outer instructions.

The 1,077-byte record-arena fixture has SHA-256 `04ab05ec92ce495ce8796524c4a431f1793980dee57d07d03e514a032bc9cc80`. The complete verifier accepts it in 785,978 instructions and the unrestricted reference runtime completes 2,522 guest instructions with result `74`. The Wasm interpreter instead reaches exact guest `WVR3017` on instruction 2,411 after 1,980,130 outer instructions. A repeated execution reaches the identical result, proving arena reset.

The focused Seed WebAssembly case completes a zero-warning Release build and passes in 73.462 seconds. The complete repository WebAssembly gate rebuilds the backend, complete verifier, expanded interpreter, all prior fixtures, and both new fixtures; it checks exact bytes and meters, rejects imports, validates every generated module, exercises verifier-first nominal/default/resource cases, and passes under Node.js 24.18.0 on Windows in 104.8 seconds. This is local development evidence, not cross-host or browser qualification.

## Rejected alternatives

JavaScript objects were rejected because host identity, property behavior, allocation, and garbage collection cannot define Windvale record semantics.

Treating nominal default locals as invalid was rejected because canonical WVB defines deterministic defaults and the complete verifier deliberately accepts their use.

An unbounded or silently growing record arena was rejected because the browser path needs deterministic containment and an observable Windvale status.

Calling this profile 17 was rejected because the outer selector's accepted WVB types, operations, limits, and emitted-module ABI are unchanged.

## Reconsider when

- Measured compiler execution identifies the required record high-water mark and lifetime behavior.
- Frame-owned record storage or a reclaiming allocator can replace monotonic storage without changing portable semantics.
- `i64` and `u64` enter the bounded Wasm-hosted interpreter requirement.
- Cross-host or browser engines expose a representation or budget issue not present in local Node.js evidence.
