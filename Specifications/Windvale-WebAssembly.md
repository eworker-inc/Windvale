# Windvale experimental WebAssembly target

- Status: Implemented experimental profile; not an accepted permanent target
- Target identifier: `wasm32-browser-v1-experimental`
- WebAssembly binary version: 1
- Portable input identity: canonical WVB 1.6

## Purpose

This contract defines Windvale's first direct WebAssembly lowering slice. A portable Windvale implementation validates one exact canonical WVB profile and emits a deterministic WebAssembly binary module. WebAssembly is an execution target for already defined Windvale semantics; it does not replace typed WIR, canonical WVB, the mandatory WVB verifier, or the reference runtime.

The implementation is `Compiler/Windvale/WebAssembly-Core.wv`. `Examples/Compiler/WebAssembly-Tool.wv` is the first hosted shell. The shell reads one WVB resource and publishes one `.wasm` resource only after complete successful selection and encoding.

## Validation boundary

WVB verification remains mandatory before a WebAssembly execution path may trust a module. The current hosted shell nevertheless treats its raw file input as untrusted and revalidates every byte range and every field needed by this profile. It rejects a truncated header, wrong version or section count, malformed or reordered section envelope, oversized payload length, trailing bytes, unsupported profile, unsupported module shape, unsupported function metadata, and unsupported code before producing output.

This selector is not a general Windvale-native WVB semantic verifier. A future browser execution path must either consume independently verified evidence or qualify a complete Windvale-native verifier before removing the Stage 0 verifier.

## Accepted WVB profile 1

The first selector accepts exactly:

- WVB 1.6 with all seven mandatory sections in canonical order and no trailing bytes;
- a `portable` module with a nonempty module name;
- zero capabilities, data declarations, and nominal types;
- one function and one export, both named `Main`;
- `Main() -> i32` with the one synthesized `i32` return local produced by the current source compiler;
- code offset zero, a declared maximum operand stack of one, and the exact verified sequence `i32.const <value>; local.store 0; local.load 0; return`; and
- no other function, instruction, import, table, memory, global, start function, element, data, or custom-section behavior.

The `i32.const` operand may contain any signed 32-bit value. Wider language or WVB coverage requires a later profile revision or a replacement general lowering contract; it must not be inferred from this slice.

## Output module

Successful lowering emits a WebAssembly binary version-1 module with these sections in ascending order:

1. one function type `() -> i32`;
2. one function using type index zero;
3. one function export named ASCII `Main`; and
4. one body with zero locals, `i32.const <value>`, and `end`.

The module has no imports and therefore defines no browser capability ABI. It has no linear memory. Section lengths and indices use the shortest unsigned LEB128 encoding. The signed constant uses the shortest valid signed LEB128 width from one through five bytes. Identical accepted WVB bytes produce identical WebAssembly bytes.

The first `42` artifact is exactly 37 bytes with SHA-256:

```text
1b62162dbc97b579c02834e9623e3ac9eccc7bc444e4b48a9e4d6c39b77ea3f1
```

## Limits and failure behavior

- Input WVB is limited to the current 4 MiB immutable-`bytes` value and hosted-file boundary. This is narrower than WVB's general 16 MiB module limit.
- Output is limited to 65,536 bytes for this experimental profile.
- All offset and length checks precede reads or additions that depend on untrusted values.
- Failure returns a typed status and an empty output value.
- The hosted shell writes no output resource on failure.

The selector statuses are `Valid`, `Invalidˉwvb`, `Unsupportedˉprofile`, `Unsupportedˉmodule`, `Unsupportedˉfunction`, `Unsupportedˉcode`, and `Outputˉlimit`.

## Evidence requirements

The profile requires:

- exact output-byte and digest comparison;
- structural validation by an implementation independent of the `.wv` encoder;
- differential `Main` results against the reference WVB runtime;
- signed-LEB boundary coverage including both `i32` extrema;
- repeated-build byte identity;
- truncated, oversized, inconsistent, unsupported-profile, and unsupported-code rejection with no output; and
- execution by a conforming WebAssembly engine before browser integration is claimed.

Cross-host equality is required before this slice is described as qualified. Browser support is not established until the generated module runs inside the playground's worker containment boundary on the explicitly tested browser profile.

## Non-claims

This profile does not establish:

- WebAssembly as a permanent Windvale host or distribution format;
- a direct source-to-WebAssembly compiler;
- a general WVB-to-WebAssembly backend;
- a Windvale-native general WVB verifier or interpreter;
- checked arithmetic, traps, calls, branches, loops, resource counters, text, bytes, records, enums, memory management, or capabilities in WebAssembly;
- compilation of the Windvale compiler itself to WebAssembly;
- replacement of the .NET playground path; or
- production browser isolation.

## Next extension boundary

The next slice should add one checked `i32` arithmetic operation and its exact Windvale overflow result without using a WebAssembly engine trap as the semantic contract. That experiment must choose a versioned result/trap ABI and preserve differential instruction accounting before branches, calls, memory, or browser capability imports are added.
