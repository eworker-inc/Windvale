# Windvale source-to-WVB backend

## Status and purpose

`Compilerˉsourceˉwvb` is the first portable Windvale-written executable backend. It consumes a validated WVSS 1 source set through `Compilerˉsourceˉwir`, lowers the accepted `WVIR 1` subset to a complete canonical WVB 1.6 module, and returns the bytes without using hosted capabilities.

The current slice proves the complete source → symbols/bindings → typed WVIR → canonical identity remapping → static data, nominal and capability metadata, and code → WVB → verifier → runtime path. Multi-module translation remains a later bounded extension.

## Public result

```text
Compilerˉcompileˉsourceˉwvb(Input: bytes)
    -> Compilerˉsourceˉwvbˉsummary
```

On success, `Status` and `Wirˉstatus` are `Valid`, `Bytecode` contains one complete WVB 1.6 module, and the summary reports function and code-byte counts. On failure, `Bytecode` is empty and the summary identifies the first function and WVIR operation involved.

The status contract distinguishes upstream WVIR rejection, unsupported module counts, declarations, shapes and operations, invalid data, and WVB limits. Its existing profile status remains reserved for stable diagnostic numbering; every currently validated profile is accepted.

## Accepted subset

The backend accepts:

- exactly one `portable`, `hosted`, or `system` source module;
- zero capabilities for a portable module or declarations from the complete current Seed capability catalog for hosted/system modules;
- private or exported functions, static data, records, and enums in any valid source declaration order;
- `[i32]`, `text`, and `bytes` static data;
- the current immutable nominal record and enum declarations, including primitive or enum record fields;
- `void`, primitive, record, and enum function returns, parameters, locals, and temporaries;
- constants, parameter/local load and store, static-data length and integer-array indexing, and function calls;
- record construction and field reads plus enum constants, exact equality/inequality, and declared names;
- capability calls with their validated catalog parameter and result shapes;
- the implemented Foundation byte, text, formatting, conversion, and SHA-256 intrinsics;
- signed and unsigned arithmetic, comparisons, equality, signed negation, and boolean negation; and
- explicit jump, branch, and return terminators produced by `if`, `else`, and `while`.

It deterministically rejects imports and multi-module input. Unknown, repeated, or portable-profile capabilities remain upstream semantic failures rather than being silently omitted.

## Canonical identity translation

WVSD entries are source-declaration identities. WVIR preserves those identities for function calls and data references. WVB instead numbers its function and data sections in strict ordinal name order.

The backend derives canonical function and data ranks from the independently validated WVSD directory. It emits functions, code, exports, and data in canonical order and translates each WVIR target during emission. Source declaration order is therefore semantically irrelevant, including when functions and data are interleaved.

Function exports are emitted in the same canonical function order and target the translated WVB function index. Explicit and synthetic data share one canonical ordinal namespace.

## Nominal type translation

WVSD assigns canonical nominal indices independently of source order: records sorted by ordinal name first, then enums sorted by ordinal name. That order is already the WVB Types index space, so the backend serializes it directly rather than introducing another remapping directory.

Each Types entry carries its existing WVB kind tag and name. Record fields and enum members retain source declaration order. Record field types are rebound through the validated symbol evidence so enum fields carry the exact canonical Types index. Enum member values preserve their exact nonnegative `i32` bit pattern.

Primitive value shapes occupy one byte. Record shapes encode byte `7` plus `u32(Shape - 65536)`; enum shapes encode byte `8` plus `u32(Shape - 131072)`. These encodings apply uniformly to parameters, results, user locals, and compiler temporaries.

WVIR operations `17` through `22` lower to the established WVB record construction/field and enum constant/equality/inequality/name opcodes. Their target and auxiliary fields are already canonical type and field/member identities validated by WVIR.

## Capability translation

The validated source profile maps directly to the existing WVB profile byte. Capability declarations are emitted by ordinal name, independently of source declaration order. Their parameter/result tags come from the fixed seven-entry Seed catalog and therefore reproduce Stage 0's canonical Capabilities section exactly.

WVIR operation `63` carries a validated WVSD capability directory entry. The backend ranks that entry among capability declarations by ordinal name and emits WVB `call.capability` with the resulting index. No capability is inferred, removed, or authorized by compilation; host support and runtime authorization remain separate required boundaries.

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

The focused conformance test compiles the backend core, runs its profile/acceptance demo, and runs the hosted tool over four differential fixtures. Each returned WVB passes the mandatory Stage 0 verifier, executes in the reference runtime, and compares byte for byte with Stage 0 compiler output.

`Tests/Fixtures/Source-Wvb/Function-Only.wv` retains the original four-function primitive/control-flow baseline. Both backends produce the exact 815-byte WVB module with SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`; it executes with result `6`.

`Tests/Fixtures/Source-Wvb/Data-And-Text.wv` deliberately interleaves unsorted functions and static data. It covers signed integer arrays, bytes, explicit and synthetic text, literal reuse, a synthetic-name collision, escaped Unicode and a surrogate pair, Foundation intrinsics, remapped data references, and remapped calls. Both backends produce the exact 1,651-byte WVB module with SHA-256 `5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704`; it executes with result `13`.

`Tests/Fixtures/Source-Wvb/Nominal-Types.wv` deliberately interleaves records, enums, data, and unsorted functions. It covers canonical record/enum grouping and ordering, every primitive record field plus enum fields, nominal parameters/results/locals/temporaries, and all six nominal WVIR operations. Both backends produce the exact 1,781-byte WVB module with SHA-256 `1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a`; it executes with result `11`.

`Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv` deliberately declares all seven catalog capabilities out of order. Its seven functions cover every capability call, parameter shape, and result shape. Both backends produce the exact 849-byte hosted WVB module with SHA-256 `1df4503a21abf5f2c0b0307ac2dc79402bc8550ec5e4a016df43fdeb8197d528`; the authorized no-argument path executes with result `0` and performs no file read or write.

The current candidate bootstrap artifacts are:

- `Source-Wvb-Core.wvb`: 567,387 bytes, SHA-256 `4f8738e60e152e8cb20b8aa85792536303c5a05c42636205b426d826f65f3aa6`.
- `Source-Wvb-Demo.wvb`: 567,964 bytes, SHA-256 `34c08767a264b75bf552583dd868720306a04b99a03e7324b93c96bc6046eead`.
- `Source-Wvb-Tool.wvb`: 567,620 bytes, SHA-256 `3862a74e7f0b1a3fc42dc043a6dcbe14651bec15b264fe7b4c65574f1a4c16c7`.

The preceding nominal implementation was qualified from exact commit `f39ff73913177de9e0f03896074262001d4eee00`. The capability/profile candidate identities above require exact-commit Windows and Debian qualification before they are called cross-host-qualified.

## Expansion path

The next backend slice should add multi-module input and canonical flattening. Full bootstrap closure remains separate and still requires closing the current source-envelope and repeated-body-traversal performance gaps.

Optimization, native code, object emission, executable containers, and OS-specific lowering are not part of this contract.
