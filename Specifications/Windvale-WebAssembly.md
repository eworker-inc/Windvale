# Windvale experimental WebAssembly target

- Status: Implemented experimental profiles 1 through 3; not an accepted permanent target
- Target identifier: `wasm32-browser-v1-experimental`
- WebAssembly binary version: 1
- Portable input identity: canonical WVB 1.6

## Purpose

This contract defines Windvale's first direct WebAssembly lowering slices. A portable Windvale implementation validates bounded canonical WVB profiles and emits deterministic WebAssembly binary modules. WebAssembly is an execution target for already defined Windvale semantics; it does not replace typed WIR, canonical WVB, the mandatory WVB verifier, or the reference runtime.

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

### Profile 3: bounded straight-line `i32`

Profile 3 accepts one validated, straight-line `Main() -> i32` instruction stream with:

- zero through 256 `i32` locals and no locals of another type;
- code offset zero, one through 16,384 code bytes, one through 4,096 instructions, and declared maximum operand-stack depth one through 256;
- `i32.const`, `local.load`, `local.store`, `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, `pop`, and one final `return`; and
- a statically valid operand stack, in-range local indices, exact agreement with the declared maximum stack, and exactly one `i32` at the final return.

Branches, calls, and instructions for other value families are rejected. The generated function retains the source WVB locals, adds three `i32` scratch locals and one `i64` scratch local, and lowers every accepted WVB instruction in order. Addition, subtraction, multiplication, and negation preserve Windvale's checked `i32` semantics. Each operation is charged before it is attempted, so the exported count includes a failing arithmetic instruction exactly as the reference runtime does.

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

## Execution ABI 1 and profiles 2 and 3 output

Profiles 2 and 3 emit a WebAssembly binary version-1 module with one `() -> i32` function, three `i32` globals, and no imports, tables, memory, start function, element section, or data section. Profile 2 has one internal `i32` local. Profile 3 has the selected WVB locals plus target scratch locals. Both export:

| Export | WebAssembly kind | Contract |
| --- | --- | --- |
| `Windvale.run` | function `() -> i32` | Executes the selected WVB function and returns `0` on success or `3007` for `WVR3007` integer overflow. |
| `Windvale.abi` | immutable `i32` global | Contains execution ABI version `1`. |
| `Windvale.result` | mutable `i32` global | Contains `Main`'s result only when `Windvale.run` returned `0`; reset to zero before every run. |
| `Windvale.instructions` | mutable `i32` global | Contains the exact number of WVB instructions attempted; reset to zero before every run. |

A conforming host checks `Windvale.abi`, invokes `Windvale.run` once, then reads `Windvale.instructions` and, on status zero, `Windvale.result`. Status `3007` maps to the existing `WVR3007` runtime diagnostic and makes the result global invalid. Profile 2 reports ten instructions on success and seven on overflow, including the failing `i32.add`. Profile 3 publishes the exact attempted WVB instruction ordinal before every operation, matching the reference runtime's pre-execution instruction charge.

Signed addition overflow is detected from the wrapped sum using `((left xor sum) and (right xor sum)) < 0`. The check is target implementation detail; `WVR3007`, result validity, and instruction accounting are Windvale contracts. The output uses shortest LEB128 encodings and is deterministic for identical WVB bytes.

Profile 3 detects subtraction overflow with `((left xor result) and (left xor right)) < 0`. Multiplication is evaluated in signed `i64`, wrapped to `i32`, sign-extended again, and compared with the wide result. Negation rejects `i32` minimum before computing `0 - value`. These checks return status `3007`; they do not escape as WebAssembly engine traps.

The successful `2147483640 + 7` artifact is exactly 176 bytes with SHA-256:

```text
4057797732dd7250413f44aa71e012222591ae7e219e27a7680f246b2cedeb8a
```

The `2147483647 + 1` overflow artifact is also 176 bytes and has SHA-256:

```text
984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52
```

The profile-3 straight-line fixture returns `42`, reports 30 instructions, is 432 bytes, and has SHA-256:

```text
15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb
```

The subtraction, multiplication, and negation overflow fixtures return status `3007` after 10, 7, and 13 attempted instructions. Their deterministic WebAssembly SHA-256 values are, respectively:

```text
757d26c2cf404cabcf5b78d2c998bc7ddc78ec4531e4571630ae2c1b5c8d7925
e924c7507a363a7b019935622abfbd4bf4ac8445cd37a0412130ce8e5c83d51a
3f098efd63c68d8c62a4f6b373507e12c21808ff01120d165c9dc85a047e99e2
```

## Limits and failure behavior

- Input WVB is limited to the current 4 MiB immutable-`bytes` value and hosted-file boundary. This is narrower than WVB's general 16 MiB module limit.
- Profile 3 is independently bounded to 256 locals, 16,384 code bytes, 4,096 instructions, and maximum operand-stack depth 256.
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

Profiles 2 and 3 additionally require positive-overflow, negative-overflow, both signed extrema, mixed-sign, and non-overflow cases across their accepted arithmetic; exact `WVR3007` status mapping; exact success and failure instruction counts; reset-before-run behavior; and proof that overflow does not escape as an engine trap. Profile 3 also requires local-index, operand-stack, instruction-count, code-size, and output-size boundary coverage.

On Windows, `pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1` rebuilds the Windvale-authored backend, compiles six profile-2/profile-3 fixtures, lowers them by running the hosted `.wv` tool, checks exact sizes and digests, and executes every output under the installed Node.js WebAssembly engine.

Exact commit `a2285f5a0c09598ec701691bdbf0af9080e8cf0c` establishes Windows and digest-pinned Debian 12 equality for the backend WVB, selected input WVB, and generated WebAssembly digests through GitHub [Verify run 30762541741](https://github.com/eworker-inc/Windvale/actions/runs/30762541741). Both host qualification jobs completed successfully; the run-level conclusion changed to `cancelled` only after both jobs completed, when a later `main` push activated workflow concurrency cancellation.

[Decision 0107](../Documents/Decisions/0107-Playground-Disposable-WebAssembly-Worker.md) integrates the exact profile-3 success WVB and Wasm identities into the playground. On 2026-08-02, a Chromium-based in-app browser twice validated and ran the transferred 432-byte import-free module in a fresh worker, reporting ABI `1`, status `0`, result `42`, and 30 instructions equal to the .NET reference path, with no browser warning or error. This is local browser integration evidence, not cross-browser qualification or complete playground isolation.

## Non-claims

This profile does not establish:

- WebAssembly as a permanent Windvale host or distribution format;
- a direct source-to-WebAssembly compiler;
- a general WVB-to-WebAssembly backend;
- a Windvale-native general WVB verifier or interpreter;
- calls, source branches, loops, arbitrary or unbounded instruction streams, other scalar families, general resource counters, text, bytes, records, enums, memory management, or capabilities in WebAssembly;
- compilation of the Windvale compiler itself to WebAssembly;
- replacement of the .NET playground path; or
- production browser isolation.

## Next extension boundary

The next backend slice should reconstruct a deliberately small structured-control-flow subset for one `i32` function while retaining execution ABI 1 and exact instruction accounting. Calls, linear memory, and browser capability imports remain outside the profile until the verifier-evidence boundary and resource contract are explicit for each. Independently, the Stage 0 compiler, verifier, `.wv` lowerer execution, and fallback interpreter should move off the UI thread before the playground is treated as hardened against hostile inputs.
