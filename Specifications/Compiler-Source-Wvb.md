# Windvale source-to-WVB backend

## Status and purpose

`Compilerˉsourceˉwvb` is the first portable Windvale-written executable backend. It consumes a validated WVSS 1 source set through `Compilerˉsourceˉwir`, lowers the accepted `WVIR 1` subset to one complete canonical WVB 1.11 module, and returns the bytes without using hosted capabilities.

The current slice proves the complete source set → symbols/bindings → typed WVIR → canonical cross-module identity flattening → static data, nominal and capability metadata, and code → WVB → verifier → runtime path. It emits one self-contained module and does not introduce runtime linkage.

## Public result

```text
Compilerˉcompileˉsourceˉwvb(Input: bytes)
    -> Compilerˉsourceˉwvbˉsummary
```

On success, `Status` and `Wirˉstatus` are `Valid`, `Bytecode` contains one complete canonical WVB 1.11 module, and the summary reports function and code-byte counts. On failure, `Bytecode` is empty and the summary identifies the first function and WVIR operation involved.

The status contract distinguishes upstream WVIR rejection, shapes and operations, invalid data, and WVB limits. The former unsupported-declaration value and the existing module-count and profile values remain reserved for stable diagnostic numbering: upstream WVIR admission now owns declaration-kind rejection, and every currently validated WVSS module count and root profile is accepted.

## Portable in-memory adapter

`Compiler/Windvale/Source-Wvb-Memory-Adapter.wv` is the capability-free execution adapter for hosts that already own immutable source bytes, including the browser playground. Its exported contract is:

```text
Main(Input: bytes) -> bytes
```

`Input` is one complete canonical WVSS 1 value. The adapter passes it unchanged to `Compilerˉcompileˉsourceˉwvb`; it does not resolve paths, enumerate files, read process arguments, write output, or print diagnostics. `Windvale-Compiler-Memory.wvproj` builds the adapter with the same compiler-core source inventory as the hosted tool.

The returned `WVCO 1` value has this exact little-endian layout:

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVCO` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Kind: `0` WVB success, `1` UTF-8 diagnostic |
| 12 | 4 | Payload length |
| 16 | `Payload length` | Exact payload bytes |

The payload must end exactly at the response length. Success carries the complete canonical WVB returned by the portable compiler. Failure carries strict UTF-8 text with stable `source-wvb status=`, `wir-status=`, `function=`, and `operation=` fields derived from the compiler summary. A consumer must reject an unknown version or kind, overflow, inconsistent length, malformed UTF-8 diagnostic, malformed WVB success payload, or trailing bytes before publication.

## Accepted subset

The backend accepts:

- one complete validated WVSS graph containing a `portable`, `hosted`, or `system` root plus as many as 63 portable dependencies;
- zero capabilities for a portable module or declarations from the complete current Seed capability catalog for hosted/system modules;
- private or exported functions, static data, storage-free constants, records, enums, and variants in any valid source declaration order;
- `[i32]`, `text`, and `bytes` static data;
- immutable nominal record, enum, and variant declarations plus bounded sequence and local affine-builder shapes;
- `void`, `i32`, `i64`, `u8`, `u32`, `u64`, `bool`, `text`, `bytes`, record, enum, variant, and sequence function returns, parameters, explicitly typed or initializer-inferred locals, and temporaries, with builders restricted to verified locals;
- literal operations produced directly or by typed-constant substitution, parameter/local load and store, static-data length and integer-array indexing, and function calls;
- positional and named record construction through the same canonical operation, record field reads, enum constants, exact equality/inequality, and declared names;
- capability calls with their validated catalog parameter and result shapes;
- the implemented Foundation byte, text, formatting, conversion, and SHA-256 intrinsics, including exact little-endian `u64` read and construction plus lossless `u32` to `u64` widening;
- checked `i32`/`i64`/`u32`/`u64` arithmetic including division and remainder; `u8`/`u32`/`u64` bitwise and shift operations; exact text/bytes equality; full fixed-width scalar comparison, signed negation, invariant formatting, Boolean negation, short-circuit Boolean conjunction/disjunction, and mutable-local compound assignment; and
- variant and collection operations plus explicit jump, branch, and return terminators produced by `if`, `else if`, `else`, `match`, `while`, `for`, `break`, and `continue`.

The root owns the emitted module name, profile, capabilities, static data, and exports. Dependencies follow the WVSS contract: imports, records, enums, and exported functions only. Their functions become internal WVB functions. Invalid graph topology, dependency order/profile/shape, unknown or repeated capabilities, and portable-profile capabilities remain upstream semantic failures rather than being silently omitted.

## Canonical identity translation

WVSD entries are source-declaration identities. WVIR preserves those identities for function calls and data references. WVB instead numbers its function and data sections in strict ordinal name order.

The backend derives canonical function and data ranks from the independently validated global WVSD directory. Each directory entry names its owner module, so comparisons and declaration reads resolve the corresponding WVSS source first. It builds one immutable entry-to-rank and rank-to-entry table for each capability, data, record, enum, and function kind, then reuses those tables throughout emission. This removes repeated whole-directory ranking without changing any public packed format. It emits functions, code, exports, and data in canonical order and translates each WVIR target during emission. Source declaration order and owner module are therefore irrelevant to final indices.

Only root function exports are emitted, in canonical function order, and they target translated global WVB function indices. Dependency exports are internalized. Explicit and synthetic data share one canonical ordinal namespace.

WVSD constant entries participate in source validation and name lookup but are excluded from every WVB runtime section. A constant read has already become an ordinary typed literal or enum WVIR operation. Even an `export const` declaration creates no WVB export or storage identity in this root-only slice.

## Nominal type translation

WVSD assigns canonical nominal indices independently of source order or module ownership: records sorted by ordinal name first, then enums sorted by ordinal name. That order is already the WVB Types index space, so the backend serializes it directly rather than introducing another remapping directory.

Each Types entry carries its existing WVB kind tag and name. Record fields and enum members retain source declaration order. Record field types are rebound through the validated symbol evidence so enum fields carry the exact canonical Types index. Enum member values preserve their exact nonnegative `i32` bit pattern.

Primitive value shapes occupy one byte. Internal shapes `7` and `8` encode WVB `i64` and `u64` value tags `9` and `10`. Record shapes encode byte `7` plus `u32(Shape - 65536)`; enum shapes encode byte `8` plus `u32(Shape - 131072)`. These encodings apply uniformly to parameters, results, user locals, and compiler temporaries.

WVIR operations `17` through `22` lower to the established WVB record construction/field and enum constant/equality/inequality/name opcodes. Their target and auxiliary fields are already canonical type and field/member identities validated by WVIR.

WVIR operations `126` and `127` lower to WVB opcodes `BD` and `BE` for `Bytesˉreadˉu64ˉlittle` and `Bytesˉfromˉu64ˉlittle`. They are ordinary members of the canonical WVB 1.11 vocabulary; the backend does not select another minor version when they occur.

WVIR operation `128` lowers to WVB opcode `BF` for `U64ˉfromˉu32`. It preserves the complete `u32` numeric domain exactly and is likewise part of canonical WVB 1.11.

Named-record syntax has disappeared by this boundary: typed WVIR has already evaluated source fields left to right and reordered their temporary operands to canonical declaration order. It therefore lowers through the same record-construction opcode and value layout as the retained positional spelling.

## Capability translation

The validated root profile maps directly to the existing WVB profile byte. Root capability declarations are emitted by ordinal name, independently of source declaration order. Dependencies cannot declare capabilities in the qualified Windvale-written backend. Parameter/result tags for its qualified seven-entry baseline come from the fixed internal catalog and therefore reproduce Stage 0's canonical Capabilities section exactly. The later Stage 0 static-composition candidate admits capability-bearing dependencies and Decision 0153's eighth catalog entry; the Windvale-written compiler has not yet adopted those candidate extensions.

WVIR operation `63` carries a validated WVSD capability directory entry. The backend ranks that entry among capability declarations by ordinal name and emits WVB `call.capability` with the resulting index. No capability is inferred, removed, or authorized by compilation; host support and runtime authorization remain separate required boundaries.

## Text and static data

Integer-array elements are serialized as exact 32-bit little-endian two's-complement values. Byte data preserves each declared byte. Text data and text literals are serialized as strict UTF-8.

The portable backend decodes the source escape set `\"`, `\\`, `\n`, `\r`, `\t`, and `\uXXXX`. A UTF-16 high-surrogate escape must be followed by its low-surrogate escape and is emitted as one Unicode scalar value.

Root explicit text declarations register decoded values in source order. A string literal from any module first reuses a matching explicit declaration, then a prior synthetic value. New literal values receive the first available six-digit name beginning at `__Text_000000`; root data-name collisions are skipped. Literal discovery traverses global functions in canonical function order and operations in WVIR order. The final merged data section is sorted by ordinal name.

The combined explicit and synthetic data count is bounded to 4,096 entries. Synthetic names are bounded by `__Text_999999`. Any limit or invalid data condition fails before a WVB value is published.

Canonical WVB admits 8,192 combined parameter/local slots. The current Windvale-native source emitter retains its narrower 4,096 combined parameter, source-local, and WIR-temporary implementation limit; compiler-generated temporaries consume only the remaining slots and do not enlarge the source namespace. Stage 0 may emit the wider canonical envelope, but removing this narrower bootstrap limit requires a separately reproduced compiler artifact.

## Code lowering contract

Every WVIR temporary becomes a WVB local after the function's parameter and user-local slots. A concrete WVLB local shape maps directly to WVB metadata; an inferred WVLB shape marker is resolved from the first verified WVIR store for that slot. Each operation loads its temporary operands, executes one WVB instruction, and stores a result temporary when present. The operand stack is therefore empty between WVIR operations and at every basic-block boundary.

The backend makes two deterministic passes over each function. The first computes every block byte offset, exact function code length, and maximum operand-stack depth. The second emits code using those offsets, so branches never require mutable backpatching.

`Boolˉphi = 64` is typed control evidence rather than a new WVB opcode. It has zero operation bytes. Each unconditional predecessor jump to a phi join emits the selected Boolean temporary load and phi-result local store immediately before the ordinary jump. The right-operand block is reached only by the conditional branch, so skipped short-circuit operands retain no bytecode execution path. WVIR validation forbids a conditional or third predecessor from targeting such a join.

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
| 7 | `i64` | 9 |
| 8 | `u64` | 10 |

The encoder writes canonical Module, Capabilities, Data, Functions, Code, Exports, and Types section envelopes in WVB 1.11. Every module carries the metadata-presence byte, including modules without metadata. Capabilities and Types contain canonical zero counts when absent and canonical entries when their accepted declarations are present. Function metadata includes user locals followed by temporary locals, contiguous code offsets, exact code lengths, and the computed maximum stack depth.

## Verification

The focused conformance test compiles the backend core, runs its profile/acceptance demo, and runs the hosted tool over the complete differential fixture family. Each returned WVB passes the mandatory Stage 0 verifier, executes in the reference runtime when it exposes an executable entry, and compares byte for byte with Stage 0 compiler output.

A separate control-flow oracle compiles nested `&&`/`||` precedence, a skipped out-of-bounds operand, `break`, `continue`, and all three compound assignments through both compilers. Their WVB bytes compare exactly and execute with result `7`.

`Tests/Fixtures/Source-Wvb/Function-Only.wv` retains the original four-function primitive/control-flow baseline while using storage-free typed constants, inferred mutable numeric locals, and multiline trailing commas. Both backends produce the exact 816-byte WVB 1.11 module with SHA-256 `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936`; it executes with result `6`.

`Tests/Fixtures/Source-Wvb/Data-And-Text.wv` deliberately interleaves unsorted functions and static data. It covers inferred integer, text, and bytes locals; signed integer arrays; multiline trailing commas; explicit and synthetic text; literal reuse; a synthetic-name collision; escaped Unicode and a surrogate pair; Foundation intrinsics; remapped data references; and remapped calls. Both backends produce the exact 1,652-byte WVB module with SHA-256 `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc`; it executes with result `13`.

`Tests/Fixtures/Source-Wvb/Nominal-Types.wv` deliberately interleaves records, enums, data, and unsorted functions. It covers inferred record, enum, and text locals; named literals; canonical record/enum grouping and ordering; every primitive record field plus enum fields; nominal parameters/results/locals/temporaries; multiline trailing commas; and all six nominal WVIR operations. Both backends produce the exact 1,782-byte WVB module with SHA-256 `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b`; it executes with result `11`.

`Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv` deliberately declares all seven catalog capabilities out of order. Its seven functions cover every capability call, parameter shape, and result shape. Both backends produce the exact 850-byte hosted WVB module with SHA-256 `bad95ed62ed8406c169ddadaa8da8576825d9213af2faa74b945db44afdfd41f`; the authorized no-argument path executes with result `0` and performs no file read or write.

The three `Tests/Fixtures/Source-Wvb/Composition-*.wv` sources cover canonical flattening across a root and two transitive dependencies. Dependency-owned functions, records, enums, a variant, and text literals combine with root static data and a synthetic-name collision. Only `Main` remains exported. Both backends produce the exact 1,388-byte WVB module with SHA-256 `42d134ee0674dcc2cfa97d018ea03b27f014b2f916d8273ba02a0aee868e0fd5`; it executes with result `42`. Reversed dependency order is rejected before output publication.

`Tests/Fixtures/Source-Wvb/Wide-Scalars.wv` covers checked `i64`/`u64` constants, arithmetic, comparisons, bitwise operations, formatting, and exact little-endian `u64` byte construction and reading. Both backends produce the exact 2,750-byte WVB module with SHA-256 `b898bc07461f7d93b2c8bd5806e06fa5c98cdaa5c11a7f4ce1fef89b77a7bf69`; it executes with result `64`.

The current deterministic compiler artifacts are:

- `Source-Wvb-Core.wvb`: 923,514 bytes, SHA-256 `c4602b6c026a65e0b9de11c025768b7f652ee73640b6f5ff1806d40ee5d0071b`.
- `Source-Wvb-Demo.wvb`: 923,210 bytes, SHA-256 `ef5a7cad94cce135dd937756980f9268fa2964f49dbb4fccca95ba4d09713fc9`.
- `Source-Wvb-Tool.wvb`: 921,640 bytes, SHA-256 `18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754`.
- `Source-Wvb-Memory-Adapter.wvb`: 919,317 bytes, SHA-256 `86f964aeeca8ebfabf11ddd252c32efe49303135af04e313b3801afe9656df29`.

Decision 0518 moves ordinary construction of the core, demo, and tool products
to the bounded native compiler-seed launcher and moves exact core inspection to
the paired native front-door helper. The generic native Project driver cannot
yet compile this current closure; that implementation limitation does not
change Project 1 or source-WVB semantics. The managed demo and complete hosted
fixture/differential/oracle sequence remain behavior evidence because the
scalar native runner stops the demo with code `3004` and no independent native
oracle yet replaces Stage 0 for that lane.

The memory adapter contains 425 functions and retains the admitted maximum of 1,408 locals and stack depth 34. These are local candidate identities and measurements. Complete Stage 1/Stage 2 bootstrap and dual-host qualification must still be rerun before the candidate becomes a new cross-host bootstrap claim.

The static multi-module behavior was first qualified at `cb1db235`, the fused typed-WVIR artifact set at `b1241157310bc597dbdf0d24146f4d81f0128712`, and Decision 0050's bidirectional nominal-index artifact set at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`. Decision 0055 changes embedded compiler implementation bytes but preserves all five differential fixture outputs byte-identical to Stage 0 and is cross-host qualified at `1a4fca7`.

For Decision 0058, Stage 0 compiled the then-canonical 12-module source inventory into a 599,868-byte Stage 1 tool. Stage 1 then compiled the same inventory in 6,700,562,174 VM instructions and produced an independently verified 599,868-byte Stage 2 module with the same SHA-256. Stage 1 and Stage 2 compare byte for byte. The dedicated bootstrap verifier reconstructs both stages from the explicit inventory and refuses any verification, size, digest, or byte-identity mismatch. This retained historical proof and artifact set are cross-host qualified at `5c16547`; they do not qualify the candidate identities above.

## Expansion path

Exact bytecode compiler self-reproduction is cross-host qualified under Decision 0058. The 4 MiB WVSS envelope is sufficient for the real compiler closure, while parity with Stage 0's larger input limit remains a separate future contract decision.

The C# Stage 0 compiler remains the independent recovery/reference implementation, and the C# runtime remains the host for this bytecode proof. Native compiler execution, native code, object emission, executable containers, and OS-specific lowering are not part of this contract.
