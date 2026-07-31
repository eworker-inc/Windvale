# Windvale source-to-WVB backend

## Status and purpose

`Compilerˉsourceˉwvb` is the first portable Windvale-written executable backend. It consumes a validated WVSS 1 source set through `Compilerˉsourceˉwir`, lowers the accepted `WVIR 1` subset to a complete canonical WVB 1.6 module, and returns the bytes without using hosted capabilities.

The current slice proves the complete source set → symbols/bindings → typed WVIR → canonical cross-module identity flattening → static data, nominal and capability metadata, and code → WVB → verifier → runtime path. It emits one self-contained module and does not introduce runtime linkage.

## Public result

```text
Compilerˉcompileˉsourceˉwvb(Input: bytes)
    -> Compilerˉsourceˉwvbˉsummary
```

On success, `Status` and `Wirˉstatus` are `Valid`, `Bytecode` contains one complete WVB 1.6 module, and the summary reports function and code-byte counts. On failure, `Bytecode` is empty and the summary identifies the first function and WVIR operation involved.

The status contract distinguishes upstream WVIR rejection, declarations, shapes and operations, invalid data, and WVB limits. Its existing module-count and profile statuses remain reserved for stable diagnostic numbering; every currently validated WVSS module count and root profile is accepted.

## Accepted subset

The backend accepts:

- one complete validated WVSS graph containing a `portable`, `hosted`, or `system` root plus as many as 63 portable dependencies;
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

The root owns the emitted module name, profile, capabilities, static data, and exports. Dependencies follow the WVSS contract: imports, records, enums, and exported functions only. Their functions become internal WVB functions. Invalid graph topology, dependency order/profile/shape, unknown or repeated capabilities, and portable-profile capabilities remain upstream semantic failures rather than being silently omitted.

## Canonical identity translation

WVSD entries are source-declaration identities. WVIR preserves those identities for function calls and data references. WVB instead numbers its function and data sections in strict ordinal name order.

The backend derives canonical function and data ranks from the independently validated global WVSD directory. Each directory entry names its owner module, so comparisons and declaration reads resolve the corresponding WVSS source first. It builds one immutable entry-to-rank and rank-to-entry table for each capability, data, record, enum, and function kind, then reuses those tables throughout emission. This removes repeated whole-directory ranking without changing any public packed format. It emits functions, code, exports, and data in canonical order and translates each WVIR target during emission. Source declaration order and owner module are therefore irrelevant to final indices.

Only root function exports are emitted, in canonical function order, and they target translated global WVB function indices. Dependency exports are internalized. Explicit and synthetic data share one canonical ordinal namespace.

## Nominal type translation

WVSD assigns canonical nominal indices independently of source order or module ownership: records sorted by ordinal name first, then enums sorted by ordinal name. That order is already the WVB Types index space, so the backend serializes it directly rather than introducing another remapping directory.

Each Types entry carries its existing WVB kind tag and name. Record fields and enum members retain source declaration order. Record field types are rebound through the validated symbol evidence so enum fields carry the exact canonical Types index. Enum member values preserve their exact nonnegative `i32` bit pattern.

Primitive value shapes occupy one byte. Record shapes encode byte `7` plus `u32(Shape - 65536)`; enum shapes encode byte `8` plus `u32(Shape - 131072)`. These encodings apply uniformly to parameters, results, user locals, and compiler temporaries.

WVIR operations `17` through `22` lower to the established WVB record construction/field and enum constant/equality/inequality/name opcodes. Their target and auxiliary fields are already canonical type and field/member identities validated by WVIR.

## Capability translation

The validated root profile maps directly to the existing WVB profile byte. Root capability declarations are emitted by ordinal name, independently of source declaration order. Dependencies cannot declare capabilities. Parameter/result tags come from the fixed seven-entry Seed catalog and therefore reproduce Stage 0's canonical Capabilities section exactly.

WVIR operation `63` carries a validated WVSD capability directory entry. The backend ranks that entry among capability declarations by ordinal name and emits WVB `call.capability` with the resulting index. No capability is inferred, removed, or authorized by compilation; host support and runtime authorization remain separate required boundaries.

## Text and static data

Integer-array elements are serialized as exact 32-bit little-endian two's-complement values. Byte data preserves each declared byte. Text data and text literals are serialized as strict UTF-8.

The portable backend decodes the source escape set `\"`, `\\`, `\n`, `\r`, `\t`, and `\uXXXX`. A UTF-16 high-surrogate escape must be followed by its low-surrogate escape and is emitted as one Unicode scalar value.

Root explicit text declarations register decoded values in source order. A string literal from any module first reuses a matching explicit declaration, then a prior synthetic value. New literal values receive the first available six-digit name beginning at `__Text_000000`; root data-name collisions are skipped. Literal discovery traverses global functions in canonical function order and operations in WVIR order. The final merged data section is sorted by ordinal name.

The combined explicit and synthetic data count is bounded to 4,096 entries. Synthetic names are bounded by `__Text_999999`. Any limit or invalid data condition fails before a WVB value is published.

## Code lowering contract

Every WVIR temporary becomes a WVB local after the function's parameter and user-local slots. Each operation loads its temporary operands, executes one WVB instruction, and stores a result temporary when present. The operand stack is therefore empty between WVIR operations and at every basic-block boundary.

The backend makes two deterministic passes over each function. The first computes every block byte offset, exact function code length, and maximum operand-stack depth. The second emits code using those offsets, so branches never require mutable backpatching.

The complete source set is fully validated before WVB emission. Declaration reads during emission therefore begin at their already validated byte offsets with relative diagnostic coordinates instead of rescanning from the module header to reconstruct absolute line and column. All source-facing syntax and semantic diagnostics are established by the checked upstream boundary; backend emission consumes only accepted offsets and declaration shapes. This optimization does not change accepted source, emitted bytes, or public diagnostic identities.

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

The encoder writes the fixed WVB 1.6 header followed by canonical Module, Capabilities, Data, Functions, Code, Exports, and Types section envelopes. Capabilities and Types contain canonical zero counts when absent and canonical entries when their accepted declarations are present. Function metadata includes user locals followed by temporary locals, contiguous code offsets, exact code lengths, and the computed maximum stack depth.

## Verification

The focused conformance test compiles the backend core, runs its profile/acceptance demo, and runs the hosted tool over five differential fixtures. Each returned WVB passes the mandatory Stage 0 verifier, executes in the reference runtime, and compares byte for byte with Stage 0 compiler output.

`Tests/Fixtures/Source-Wvb/Function-Only.wv` retains the original four-function primitive/control-flow baseline. Both backends produce the exact 815-byte WVB module with SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`; it executes with result `6`.

`Tests/Fixtures/Source-Wvb/Data-And-Text.wv` deliberately interleaves unsorted functions and static data. It covers signed integer arrays, bytes, explicit and synthetic text, literal reuse, a synthetic-name collision, escaped Unicode and a surrogate pair, Foundation intrinsics, remapped data references, and remapped calls. Both backends produce the exact 1,651-byte WVB module with SHA-256 `5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704`; it executes with result `13`.

`Tests/Fixtures/Source-Wvb/Nominal-Types.wv` deliberately interleaves records, enums, data, and unsorted functions. It covers canonical record/enum grouping and ordering, every primitive record field plus enum fields, nominal parameters/results/locals/temporaries, and all six nominal WVIR operations. Both backends produce the exact 1,781-byte WVB module with SHA-256 `1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a`; it executes with result `11`.

`Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv` deliberately declares all seven catalog capabilities out of order. Its seven functions cover every capability call, parameter shape, and result shape. Both backends produce the exact 849-byte hosted WVB module with SHA-256 `1df4503a21abf5f2c0b0307ac2dc79402bc8550ec5e4a016df43fdeb8197d528`; the authorized no-argument path executes with result `0` and performs no file read or write.

The three `Tests/Fixtures/Source-Wvb/Composition-*.wv` sources cover canonical flattening across a root and two transitive dependencies. Dependency-owned functions, records, enums, and a text literal combine with root static data and a synthetic-name collision. Only `Main` remains exported. Both backends produce the exact 1,030-byte WVB module with SHA-256 `7279011a12f3d2becc1e9775fb92bd7c74b8760b2c94f13a282d71c0849f8e6f`; it executes with result `42`. Reversed dependency order is rejected before output publication.

The current bootstrap-convergence candidate compiler artifacts are:

- `Source-Wvb-Core.wvb`: 599,061 bytes, SHA-256 `9c3f4f6839274766a3633784716147e03e3bce47ec1103dac0eb0d998a1b4b9a`.
- `Source-Wvb-Demo.wvb`: 600,364 bytes, SHA-256 `acf1f5cbde6e2ba3d831ed8390dac85f812d13525847619b3c85903bb7a44c8f`.
- `Source-Wvb-Tool.wvb`: 599,868 bytes, SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.

The static multi-module behavior was first qualified at `cb1db235`, the fused typed-WVIR artifact set at `b1241157310bc597dbdf0d24146f4d81f0128712`, and Decision 0050's bidirectional nominal-index artifact set at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`. Decision 0055 changes embedded compiler implementation bytes but preserves all five differential fixture outputs byte-identical to Stage 0 and is cross-host qualified at `1a4fca7`.

For the Decision 0058 candidate, Stage 0 compiled the canonical 12-module source inventory into the 599,868-byte Stage 1 tool above. Stage 1 then compiled the same inventory in 6,700,562,174 VM instructions and produced an independently verified 599,868-byte Stage 2 module with the same SHA-256. Stage 1 and Stage 2 compare byte for byte. The dedicated bootstrap verifier reconstructs both stages from the explicit inventory and refuses any verification, size, digest, or byte-identity mismatch.

## Expansion path

Exact bytecode compiler self-reproduction is implemented by the Decision 0058 candidate. Cross-host qualification and clean-environment recovery evidence are the remaining acceptance work for this candidate. The 4 MiB WVSS envelope is sufficient for the real compiler closure, while parity with Stage 0's larger input limit remains a separate future contract decision.

The C# Stage 0 compiler remains the independent recovery/reference implementation, and the C# runtime remains the host for this bytecode proof. Native compiler execution, native code, object emission, executable containers, and OS-specific lowering are not part of this contract.
