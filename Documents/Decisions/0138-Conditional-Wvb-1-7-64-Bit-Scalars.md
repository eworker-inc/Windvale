# Decision 0138: Conditional WVB 1.7 64-bit scalars

- Date: 2026-08-03
- Status: Implemented candidate; focused Windows conformance passes
- Adds: Source `i64` and `u64`, value types 9 and 10, opcodes `0x80` through `0x96`, and invariant formatting
- Retains: Exact WVB 1.6 output for modules using only the 1.6 vocabulary, seven-section grammar, `u32` counts and offsets, and every pinned boot/OS/self-hosted 1.6 artifact

## Context

Database identities, persistent offsets, generations, row counts, timestamps, and checksums routinely exceed 32 bits. Encoding them as pairs of `u32` values would leak representation through source, records, bytecode, and future storage APIs before Windvale has even defined the database layer.

The current repository also has extensive exact WVB 1.6 consumers: the Windvale-written compiler and dumper, WebAssembly selector profiles, native and OS bootstrap artifacts, golden hashes, and in-guest interpreters. Globally changing every module header to 1.7 would create broad artifact churn even though almost all modules use no new value. Treating 64-bit opcodes as 1.6 would make older readers accept a version whose vocabulary they do not implement.

## Decision

- Add signed `i64` and unsigned `u64` as scalar source, WIR, WVB, verifier, inspector, and reference-runtime values.
- Require explicit `i64` and `u64` literal suffixes. Positive literals are bounded by their widths; the signed minimum is expressible through checked ordinary operations, for example `-9223372036854775807i64 - 1i64`.
- Support checked addition, subtraction, multiplication, signed negation, equality, ordering, and invariant base-10 formatting. Overflow and unsigned underflow trap with the existing `WVR3007` integer-overflow contract.
- Assign WVB value types `9` and `10`. Assign the contiguous WVB 1.7 extension opcodes `0x80` through `0x96` for constants, arithmetic, comparisons, and formatting. Encode constant operands as exact little-endian 64-bit values.
- Keep section structure, counts, indices, lengths, code offsets, enum backing values, and existing Foundation binary APIs at their declared 32-bit widths. Adding a 64-bit scalar does not silently widen another contract.
- Make the canonical writer choose the lowest sufficient minor version. A module containing no 1.7 value shape or opcode remains byte-for-byte WVB 1.6; a module containing either emits WVB 1.7.
- Read and verify both 1.6 and 1.7. Reject a 1.6 header that contains a 1.7 type or opcode with `WVB2107`, and reject unsupported header versions with the existing `WVB1003` boundary.
- Implement execution in the C# reference runtime first. The baseline x86-64 backend rejects the new scalar profile explicitly as `WVN2003`. Existing Windvale-written compiler, WebAssembly, bootstrap, and OS interpreter profiles remain pinned to WVB 1.6 until their own lowering and ABI decisions are implemented and tested.
- Keep the editor grammar synchronized with the implemented Stage 0 lexical surface. Do not describe the Windvale-written self-hosted compiler as accepting 1.7 source until its lexer, semantic directory, WIR, encoder, and exact reproduction contract are deliberately advanced.

## Evidence

The focused conformance case compiles wide values in function signatures, record fields, locals, arithmetic, comparisons, and formatting. It proves a WVB 1.7 header, exact round-trip bytes, typed disassembly, signed-minimum and unsigned-maximum output, seven successful conditions, and `WVN2003` native rejection. The same test recompiles the existing sum fixture and proves that it retains a WVB 1.6 header.

Boundary coverage rejects out-of-range source literals, truncated 64-bit operands, and a 1.7 module relabeled as 1.6. Runtime coverage traps signed overflow plus unsigned overflow and underflow.

## Consequences

Windvale source can now represent the persistent scalar domain needed by a database format without perturbing existing distribution and boot identities. Backend parity is intentionally incomplete and visible. New portable applications that require native, WebAssembly, or Windvale OS execution must remain inside the 1.6 vocabulary until those targets advance.

WVB 1.6 remains a supported current input because it is an active named compatibility and recovery contract, not an obsolete experiment. WVB 1.7 is additive at the reader boundary but does not promise indefinite public compatibility while Windvale remains pre-release.

## Reconsider when

- The native ABI admits 64-bit scalar parameters, locals, calls, results, checked arithmetic, and independent machine-code verification.
- The Windvale-written compiler must reproduce a source inventory containing `i64` or `u64`.
- WebAssembly or Windvale OS consumers need the 1.7 profile.
- Database binary APIs require explicit 64-bit little-endian reads/writes or a distinct persistent-offset type.
