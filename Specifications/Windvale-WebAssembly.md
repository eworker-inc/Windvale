# Windvale experimental WebAssembly target

- Status: Implemented experimental profiles 1 and 2; not an accepted permanent target
- Target identifier: `wasm32-browser-v1-experimental`
- WebAssembly binary version: 1
- Portable input identity: canonical WVB 1.6

## Purpose

This contract defines Windvale's first direct WebAssembly lowering slice. A portable Windvale implementation validates one exact canonical WVB profile and emits a deterministic WebAssembly binary module. WebAssembly is an execution target for already defined Windvale semantics; it does not replace typed WIR, canonical WVB, the mandatory WVB verifier, or the reference runtime.

The implementation is `Compiler/Windvale/WebAssembly-Core.wv`. `Examples/Compiler/WebAssembly-Tool.wv` is the first hosted shell. The shell reads one WVB resource and publishes one `.wasm` resource only after complete successful selection and encoding.

## Validation boundary

WVB verification remains mandatory before a WebAssembly execution path may trust a module. The current hosted shell nevertheless treats its raw file input as untrusted and revalidates every byte range and every field needed by this profile. It rejects a truncated header, wrong version or section count, malformed or reordered section envelope, oversized payload length, trailing bytes, unsupported profile, unsupported module shape, unsupported function metadata, and unsupported code before producing output.

This selector is not a general Windvale-native WVB semantic verifier. A future browser execution path must either consume independently verified evidence or qualify a complete Windvale-native verifier before removing the Stage 0 verifier.

## Accepted WVB profiles

Both implemented selectors require the shared envelope and module shape below:

- WVB 1.6 with all seven mandatory sections in canonical order and no trailing bytes;
- a `portable` module with a nonempty module name;
- zero capabilities, data declarations, and nominal types;
- one function and one export, both named `Main`; and
- `Main() -> i32` with compiler-synthesized `i32` locals and no other function.

### Profile 1: direct constant

Profile 1 additionally accepts exactly:

- `Main() -> i32` with the one synthesized `i32` return local produced by the current source compiler;
- code offset zero, a declared maximum operand stack of one, and the exact verified sequence `i32.const <value>; local.store 0; local.load 0; return`; and
- no other instruction.

The `i32.const` operand may contain any signed 32-bit value. Wider language or WVB coverage requires a later profile revision or a replacement general lowering contract; it must not be inferred from this slice.

### Profile 2: checked constant addition

Profile 2 accepts the exact compiler-produced shape for `return <left> + <right>`:

- three synthesized `i32` locals;
- code offset zero and declared maximum operand stack depth two; and
- the exact verified sequence `i32.const <left>; local.store 0; i32.const <right>; local.store 1; local.load 0; local.load 1; i32.add; local.store 2; local.load 2; return`.

Both operands may contain any signed 32-bit value. The generated WebAssembly executes `i32.add`, detects signed overflow explicitly, and reports Windvale runtime status `WVR3007` through execution ABI 1. It does not depend on WebAssembly's wrapping addition as the Windvale result and does not convert overflow into a WebAssembly engine trap.

## Profile 1 output module

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

## Execution ABI 1 and profile 2 output

Profile 2 emits a WebAssembly binary version-1 module with one `() -> i32` function, one internal `i32` local, three `i32` globals, and no imports, tables, memory, start function, element section, or data section. It exports:

| Export | WebAssembly kind | Contract |
| --- | --- | --- |
| `Windvale.run` | function `() -> i32` | Executes the selected WVB function and returns `0` on success or `3007` for `WVR3007` integer overflow. |
| `Windvale.abi` | immutable `i32` global | Contains execution ABI version `1`. |
| `Windvale.result` | mutable `i32` global | Contains `Main`'s result only when `Windvale.run` returned `0`; reset to zero before every run. |
| `Windvale.instructions` | mutable `i32` global | Contains the exact number of WVB instructions attempted; reset to zero before every run. |

A conforming host checks `Windvale.abi`, invokes `Windvale.run` once, then reads `Windvale.instructions` and, on status zero, `Windvale.result`. Status `3007` maps to the existing `WVR3007` runtime diagnostic and makes the result global invalid. The current bounded profile reports ten instructions on success and seven on overflow, including the failing `i32.add`, matching the reference runtime's pre-execution instruction charge.

Signed addition overflow is detected from the wrapped sum using `((left xor sum) and (right xor sum)) < 0`. The check is target implementation detail; `WVR3007`, result validity, and instruction accounting are Windvale contracts. The output uses shortest LEB128 encodings and is deterministic for identical WVB bytes.

The successful `2147483640 + 7` artifact is exactly 176 bytes with SHA-256:

```text
4057797732dd7250413f44aa71e012222591ae7e219e27a7680f246b2cedeb8a
```

The `2147483647 + 1` overflow artifact is also 176 bytes and has SHA-256:

```text
984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52
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

Profile 2 additionally requires positive-overflow, negative-overflow, both signed extrema, mixed-sign, and non-overflow cases; exact `WVR3007` status mapping; exact success and failure instruction counts; reset-before-run behavior; and proof that overflow does not escape as an engine trap.

On Windows, `pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1` rebuilds the Windvale-authored backend, compiles both checked-add fixtures, lowers them by running the hosted `.wv` tool, checks exact digests, and executes both outputs under the installed Node.js WebAssembly engine.

Cross-host equality is required before this slice is described as qualified. Browser support is not established until the generated module runs inside the playground's worker containment boundary on the explicitly tested browser profile.

## Non-claims

This profile does not establish:

- WebAssembly as a permanent Windvale host or distribution format;
- a direct source-to-WebAssembly compiler;
- a general WVB-to-WebAssembly backend;
- a Windvale-native general WVB verifier or interpreter;
- checked subtraction, multiplication, negation, arbitrary instruction streams, calls, source branches, loops, general resource counters, text, bytes, records, enums, memory management, or capabilities in WebAssembly;
- compilation of the Windvale compiler itself to WebAssembly;
- replacement of the .NET playground path; or
- production browser isolation.

## Next extension boundary

The next backend slice should replace the two exact code templates with a bounded one-function `i32` instruction-stream lowering model, retaining execution ABI 1 while adding checked subtraction or multiplication through the same status seam. It must keep control-flow reconstruction, calls, linear memory, and browser capability imports outside the profile until the verifier-evidence boundary and instruction accounting are explicit for each.
