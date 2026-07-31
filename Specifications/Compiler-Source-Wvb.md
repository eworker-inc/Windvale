# Windvale source-to-WVB backend

## Status and purpose

`Compilerˉsourceˉwvb` is the first portable Windvale-written executable backend. It consumes a validated WVSS 1 source set through `Compilerˉsourceˉwir`, lowers the accepted `WVIR 1` subset to a complete canonical WVB 1.6 module, and returns the bytes without using hosted capabilities.

The current slice proves the complete source → symbols/bindings → typed WVIR → canonical identity remapping → static data and code → WVB → verifier → runtime path. Nominal metadata, capabilities, and multi-module translation remain later bounded extensions.

## Public result

```text
Compilerˉcompileˉsourceˉwvb(Input: bytes)
    -> Compilerˉsourceˉwvbˉsummary
```

On success, `Status` and `Wirˉstatus` are `Valid`, `Bytecode` contains one complete WVB 1.6 module, and the summary reports function and code-byte counts. On failure, `Bytecode` is empty and the summary identifies the first function and WVIR operation involved.

The status contract distinguishes upstream WVIR rejection, unsupported module counts, profiles, declarations, shapes and operations, invalid data, and WVB limits.

## Accepted subset

The backend accepts:

- exactly one `portable` source module;
- private or exported functions and static data in any valid source declaration order;
- `[i32]`, `text`, and `bytes` static data;
- `void`, `i32`, `u8`, `u32`, `bool`, `text`, and `bytes` function returns, parameters, locals, and temporaries;
- constants, parameter/local load and store, static-data length and integer-array indexing, and function calls;
- the implemented Foundation byte, text, formatting, conversion, and SHA-256 intrinsics;
- signed and unsigned arithmetic, comparisons, equality, signed negation, and boolean negation; and
- explicit jump, branch, and return terminators produced by `if`, `else`, and `while`.

It deterministically rejects imports, capabilities, hosted profiles, records, enums, nominal operations, capability calls, and multi-module input. These are expansion boundaries, not silently degraded programs.

## Canonical identity translation

WVSD entries are source-declaration identities. WVIR preserves those identities for function calls and data references. WVB instead numbers its function and data sections in strict ordinal name order.

The backend derives canonical function and data ranks from the independently validated WVSD directory. It emits functions, code, exports, and data in canonical order and translates each WVIR target during emission. Source declaration order is therefore semantically irrelevant, including when functions and data are interleaved.

Function exports are emitted in the same canonical function order and target the translated WVB function index. Explicit and synthetic data share one canonical ordinal namespace.

## Text and static data

Integer-array elements are serialized as exact 32-bit little-endian two's-complement values. Byte data preserves each declared byte. Text data and text literals are serialized as strict UTF-8.

The portable backend decodes the source escape set `\"`, `\\`, `\n`, `\r`, `\t`, and `\uXXXX`. A UTF-16 high-surrogate escape must be followed by its low-surrogate escape and is emitted as one Unicode scalar value.

Explicit text declarations register decoded values in source order. A string literal first reuses a matching explicit declaration, then a prior synthetic value. New literal values receive the first available six-digit name beginning at `__Text_000000`; explicit data-name collisions are skipped. Literal discovery traverses functions in canonical function order and operations in WVIR order. The final merged data section is sorted by ordinal name.

The combined explicit and synthetic data count is bounded to 4,096 entries. Synthetic names are bounded by `__Text_999999`. Any limit or invalid data condition fails before a WVB value is published.

## Code lowering contract

Every WVIR temporary becomes a WVB local after the function's parameter and user-local slots. Each operation loads its temporary operands, executes one WVB instruction, and stores a result temporary when present. The operand stack is therefore empty between WVIR operations and at every basic-block boundary.

The backend makes two deterministic passes over each function. The first computes every block byte offset, exact function code length, and maximum operand-stack depth. The second emits code using those offsets, so branches never require mutable backpatching.

Primitive WVIR shapes map to WVB shapes as follows:

| WVIR shape | Meaning | WVB shape byte |
| ---: | --- | ---: |
| 0 | `void` | 0 |
| 1 | `i32` | 1 |
| 2 | `u8` | 4 |
| 3 | `u32` | 5 |
| 4 | `bool` | 2 |
| 5 | `text` | 3 |
| 6 | `bytes` | 6 |

The encoder writes the fixed WVB 1.6 header followed by canonical Module, Capabilities, Data, Functions, Code, Exports, and Types section envelopes. Capabilities and Types contain canonical zero counts in this subset. Function metadata includes user locals followed by temporary locals, contiguous code offsets, exact code lengths, and the computed maximum stack depth.

## Verification

The focused conformance test compiles the backend core, runs its portable acceptance/rejection demo, and runs the hosted tool over two differential fixtures. Each returned WVB passes the mandatory Stage 0 verifier, executes in the reference runtime, and compares byte for byte with Stage 0 compiler output.

`Tests/Fixtures/Source-Wvb/Function-Only.wv` retains the original four-function primitive/control-flow baseline. Both backends produce the exact 815-byte WVB module with SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`; it executes with result `6`.

`Tests/Fixtures/Source-Wvb/Data-And-Text.wv` deliberately interleaves unsorted functions and static data. It covers signed integer arrays, bytes, explicit and synthetic text, literal reuse, a synthetic-name collision, escaped Unicode and a surrogate pair, Foundation intrinsics, remapped data references, and remapped calls. Both backends produce the exact 1,651-byte WVB module with SHA-256 `5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704`; it executes with result `13`.

The current candidate bootstrap artifacts are:

- `Source-Wvb-Core.wvb`: 554,525 bytes, SHA-256 `c410f775e6c6e5a8a40678a5caf4e7a07a37c4dcf711b2f272f11cc1796d5d8d`.
- `Source-Wvb-Demo.wvb`: 555,160 bytes, SHA-256 `d376b66312dc9005540482f3adfe6be10b6ec8a2fbd9fcbb86c3a412e70e75fa`.
- `Source-Wvb-Tool.wvb`: 555,049 bytes, SHA-256 `364c47c70f04f0133a35ce07dcdfeb5eedbcaaf8acbedd8e002c8c6d93fa867f`.

These candidate identities require exact-commit Windows and Debian qualification before they are called cross-host-qualified.

## Expansion path

The next backend slices should add canonical serialization in measured order: nominal records/enums, capabilities and hosted profiles, then multi-module input. Full bootstrap closure remains separate and still requires closing the current source-envelope and repeated-body-traversal performance gaps.

Optimization, native code, object emission, executable containers, and OS-specific lowering are not part of this contract.
