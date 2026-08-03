# Windvale experimental WebAssembly target

- Status: Implemented locally through experimental profile 16 plus a compiler-capacity verifier bundle, and cross-host qualified through profile 10, but not an accepted permanent target
- Target identifier: `wasm32-browser-v1-experimental`
- WebAssembly binary version: 1
- Portable input identity: canonical WVB 1.6

## Purpose

This contract defines Windvale's first direct WebAssembly lowering slices. A portable Windvale implementation validates bounded canonical WVB profiles and emits deterministic WebAssembly binary modules. WebAssembly is an execution target for already defined Windvale semantics; it does not replace typed WIR, canonical WVB, the mandatory WVB verifier, or the reference runtime.

The implementation is `Compiler/Windvale/WebAssembly-Core.wv`. `Examples/Compiler/WebAssembly-Tool.wv` is the first hosted shell. The shell reads one WVB resource and publishes one `.wasm` resource only after complete successful selection and encoding.

## Validation boundary

WVB verification remains mandatory before a WebAssembly execution path may trust a module. The current hosted shell nevertheless treats its raw file input as untrusted and revalidates every byte range and every field needed by this profile. It rejects a truncated header, wrong version or section count, malformed or reordered section envelope, oversized payload length, trailing bytes, unsupported profile, unsupported module shape, unsupported function metadata, and unsupported code before producing output.

This selector is not itself a general Windvale-native WVB semantic verifier. Profile 11 can execute a Windvale-written verifier that completely consumes the seven bounded section payloads, profile 12 can decompose a larger Windvale-written verifier across descriptor-bearing private functions, and profile 13 supplies capacity for the complete compiler-aligned consumer. That retained ten-function consumer proves canonical metadata and references plus typed local, operand-stack, call, capability, record, enum, return, reachability, and declared-stack contracts for the source compiler's empty-stack control boundaries. Profile 14 separately lowers the first bounded WVB scalar interpreter, profile 15 adds bounded immutable text and bytes values, invariant formatting, and deterministic quoting, and profile 16 adds import-free SHA-256. The retained profile-16 interpreter now also executes bounded direct record and enum values without changing the outer selector. A three-artifact compiler-capacity bundle derived from the same canonical verifier sources admits the exact 599,868-byte compiler WVB under independent metadata/reference, typed-execution, and control/reachability meters. It does not yet execute that compiler. General WVB graphs with nonempty stack joins and executable capability authorization remain outside this claim. A future default browser execution path must compose the verifier, explicit worker authorization, and the selected interpreter profile before removing the Stage 0 path.

## Accepted WVB profiles

Profiles 1 through 5 require the shared envelope and module shape below:

- WVB 1.6 with all seven mandatory sections in canonical order and no trailing bytes;
- a `portable` module with a nonempty module name;
- zero capabilities, data declarations, and nominal types;
- one function and one export, both named `Main`; and
- `Main() -> i32` with compiler-synthesized locals admitted by the selected profile and no other function.

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

### Profile 4: one metered structured loop

Profile 4 accepts one compiler-produced `while` control-flow region in `Main() -> i32`:

- zero through 256 `i32` or `bool` locals, represented as target `i32` locals while preserving WVB type checks at selection;
- code offset zero, one through 16,384 code bytes, one through 4,096 instructions, and declared maximum operand-stack depth exactly two;
- the profile-3 scalar instructions plus signed `i32` equality and ordering, `jump`, and `branch.false`;
- one forward `branch.false` from the loop condition to the exit, one backward `jump` from the end of the body to the loop header, and only compiler block-transition jumps whose target is the immediately following instruction; and
- empty operand stacks at every jump, a `bool` consumed by `branch.false`, an exact exit boundary immediately after the backward edge, and one final `i32` return.

The selector validates local types, stack types, instruction boundaries, branch directions and targets, the single-entry/single-back-edge shape, exact maximum stack, and final return before emission. It reconstructs the WVB offsets as one WebAssembly `block` containing one `loop`; the false edge uses `br_if 1`, and the back edge uses `br 0`. Every dynamic WVB instruction is metered before execution through ABI 2, including forward block-transition jumps, the false edge, the back edge, a failing arithmetic operation, and the final return.

### Profile 5: sequential structured control

Profile 5 accepts two or more nonnested compiler-produced control-flow regions in `Main() -> i32`, with at least one conditional region:

- each region is a `while`, `if`, or `if/else` over the profile-4 scalar and comparison instructions;
- regions may appear sequentially in one function but may not overlap or nest;
- every region begins and ends at validated instruction boundaries with empty operand stacks;
- a loop has one canonical entry transition, one forward false edge, and one final back edge to that entry;
- an `if` has one forward false edge to its join, while an `if/else` additionally has one then-to-join jump immediately before the false target; and
- compiler block-transition jumps remain exact no-ops whose target is the immediately following instruction.

The selector independently classifies every branch region before emission, rejects crossing edges and malformed joins, and requires one final `i32` return after all regions close. It emits each loop as a WebAssembly `block` and `loop`, each conditional as `if`, and each two-route conditional with `else`. Dynamic ABI-2 metering remains immediately before every WVB instruction on either selected route.

### Profile 6: bounded direct calls

Profile 6 accepts one canonical portable module with two through eight functions:

- every function returns `i32`, has zero through two `i32` parameters, and has no non-`i32` local;
- each function has at most 256 parameters plus locals, 16,384 code bytes, 4,096 instructions, and declared operand-stack depth one or two;
- aggregate code is at most 32,768 bytes and aggregate instructions are at most 8,192;
- function bodies use profile-3 scalar instructions plus direct `call`, with at least one call in the module;
- `Main() -> i32` is the only export and the final canonical function; and
- every call target has a lower canonical function ordinal than its caller and receives exactly its declared argument count.

The decreasing-ordinal rule rejects forward calls, self-calls, mutual cycles, and recursion before emission. With at most eight functions, it also bounds the dynamic Windvale call depth to eight without a separate runtime depth counter. Profile 6 intentionally does not combine calls with branches or profile-5 control regions yet.

The target module contains an exported ABI-2 wrapper plus one private WebAssembly function per WVB function. Calls lower to real WebAssembly direct `call` instructions. All generated functions share the same instruction-count, instruction-limit, and status globals, so callee instructions consume the caller's one budget and `WVR3007` or `WVR3011` propagates through every active caller without becoming an engine trap. The wrapper publishes `Main`'s result only after shared status remains zero.

### Profile 7: bounded calls with structured control

Profile 7 composes the profile-6 call graph with the profile-5 structured-control model:

- at least one real direct call and at least one `while`, `if`, or `if/else` region occur in the module;
- compiler-produced `bool` locals are admitted in addition to `i32` locals and are represented as target `i32` locals after their WVB types are checked;
- each function may contain sequential nonnested control regions, while regions in different functions remain independent;
- calls may occur before, after, or inside a control region and still obey the decreasing-target-ordinal and exact-arity rules; and
- comparison, jump, branch, call, local, stack, and region evidence is reconstructed before any bytes are published.

Profile 7 retains profile 6's function, call-depth, per-function, aggregate-code, aggregate-instruction, and output limits. Nested, overlapping, or crossing control regions remain unsupported. Every dynamic instruction in every selected caller, callee, loop iteration, or conditional route charges the same ABI-2 global budget. Arithmetic overflow and instruction exhaustion propagate through all active callers exactly as in profile 6.

### Profile 8: versioned linear-memory text and byte buffers

Profile 8 accepts either of two exact compiler-produced identity functions:

- `Main(Input: bytes) -> bytes`; or
- `Main(Input: text) -> text`.

The module remains canonical WVB 1.6, portable, capability-free, and single-function/single-export. The accepted function has exactly one parameter, one synthesized return local of the same type, declared maximum operand-stack depth one, and the four-instruction sequence `local.load 0; local.store 1; local.load 1; return`. The selector rejects a mixed parameter/return family, any other local or instruction shape, and every malformed envelope without publication.

This deliberately narrow identity profile establishes transport and validation semantics before general dynamic values. It does not yet lower arbitrary text or byte operations, calls, branches, allocation, records, or enums. `bytes` are opaque and may contain any octets. `text` input is validated inside the generated guest by a strict UTF-8 state machine that rejects truncation, invalid continuation ranges, overlong encodings, surrogate code points, and values above U+10FFFF.

### Profile 9: bounded straight-line runtime values

Profile 9 accepts one canonical portable, capability-free `Main(Input: bytes) -> bytes` function with:

- zero through 255 nonparameter locals of type `i32`, `bool`, `u8`, `u32`, or `bytes`;
- one through 16,384 code bytes, one through 4,096 instructions, and declared maximum operand-stack depth one through four;
- `i32`, `bool`, `u8`, and `u32` constants; `local.load`; `local.store`; `pop`; and one final `return`;
- `bytes.length`, `bytes.slice`, `bytes.read_u8`, `bytes.read_u16_little`, `bytes.read_u32_little`, `bytes.read_i32_little`, `u32.from_u8`, `bytes.concat`, `bytes.from_u8`, `bytes.from_u16_little`, `bytes.from_u32_little`, and `bytes.from_i32_little`; and
- exact static operand types, in-range local indices, proof that every nonparameter local is stored before it is loaded, agreement with the declared maximum stack, and exactly one `bytes` value at the final return.

Internally, generated code represents a bytes value as an `i64` descriptor with a low unsigned 32-bit pointer and high unsigned 32-bit length. The input value and its slices borrow storage; constructed values use a checked monotonic arena over the existing output window. Concatenation copies both inputs into one fresh extent. Fixed-width constructors store one, two, or four little-endian bytes in a fresh extent. The returned descriptor is copied to the output base before its length is published, so the internal descriptor and temporary layout do not cross the host ABI.

Each bytes value is independently bounded to 4 MiB and aggregate construction is bounded by the same 4 MiB arena. Range errors return `WVR3008`; value-size overflow returns `WVR3015`; an out-of-range u16 constructor input returns `WVR3016`; aggregate arena exhaustion returns `WVR3018`; and instruction exhaustion retains `WVR3011`. Every failure leaves output length zero. Profile 9 does not yet compose these values with profile-3 arithmetic, profile-7 calls/control, general text operations, records, or enums.

### Profile 10: bounded runtime control and WVB envelope verification

Profile 10 composes profile 9's primitive and byte values with:

- checked `u32.add` and `u32.subtract`;
- all six unsigned `u32` comparisons plus `u8` equality and inequality;
- `jump` and `branch.false` over decoded instruction-boundary targets; and
- one or more `bytes` returns, including an early return before physically later loop blocks.

Every absolute control target must be function entry or immediately follow a decoded `jump`, `branch.false`, or `return`. Operand stacks are empty at control boundaries. The selector lowers the verified basic-block graph through one private `i32` program-counter local and one Wasm dispatch loop. Dispatch comparisons and branches are target implementation details and do not consume the WVB instruction budget. Every dynamically selected WVB instruction is still charged exactly once before its operation. Checked unsigned overflow or underflow returns `WVR3007` without publishing output.

The retained profile-10 consumer is `Wvb-Envelope-Verify-Main.wv`. Given arbitrary input bytes, it returns `[1]` only when the input is a completely consumed WVB 1.6 envelope with magic `WVB1`, version `1.6`, section count seven, section kinds one through seven in order, zero section flags and reserved fields, and every payload extent in range. It returns `[0]` for an invalid envelope. This is outer-envelope verification, not complete validation of section payloads or executable semantics.

### Profile 11: bounded WVB section-payload verification

Profile 11 retains profile 10's operations and execution ABI while scaling the one accepted runtime function to at most 2,047 nonparameter locals, 32,768 code bytes, and 100,000 decoded instructions. The selector records instruction boundaries once in an immutable byte mask and uses the mask for bounded control-target checks. The emitted control wrapper accumulates one basic block at a time before appending it to the complete Wasm body. These are selector construction changes; every dynamic WVB operation remains metered and prior generated artifacts retain their exact bytes.

The retained consumer is `Wvb-Structural-Verify-Main.wv`. It requires the profile-10 envelope and then completely consumes all seven payload schemas under explicit bounds. It checks module profile/name extents; capability name and primitive-signature shapes; data names, kinds, counts, and payload extents; function names, type encodings, contiguous code ranges, and declared limits; complete known-opcode widths and canonical boolean operands; function-export target ranges; and record/enum names, item counts, and item payload extents. Valid input returns `[1]`; any structural rejection returns `[0]`.

Profile 11 is deliberately not the mandatory semantic verifier. It does not yet validate UTF-8 or source-name grammar, uniqueness or canonical declaration order, capability catalog identities, nominal-type target identities, instruction indices and branch targets, typed stack and local-initialization flow, control joins and reachability, maximum-stack agreement, export uniqueness, or capability authorization.

### Profile 12: descriptor-bearing calls with runtime control

Profile 12 composes profile 11's runtime values and control model with an acyclic call graph:

- the module has two through eight functions, each with exactly one `bytes` parameter and a `bytes` result;
- each function has at most 2,047 nonparameter locals of the profile-11 primitive/bytes types, 32,768 code bytes, 100,000 decoded instructions, and declared operand-stack depth one through four;
- aggregate code is at most 65,536 bytes and aggregate decoded instructions are at most 200,000;
- `Main` is the only export and final canonical function, and the module contains at least one call; and
- every direct call targets a lower canonical function ordinal and may occur before, after, or inside terminator-aligned control.

The decreasing-ordinal rule rejects forward calls, self-calls, cycles, and recursion and statically bounds dynamic call depth to eight. The target module contains the public ABI-3 wrapper plus one private `(i64) -> i64` Wasm function for each WVB function. The private `i64` is the existing pointer/length descriptor and never crosses the public host ABI.

All functions share instruction-count, instruction-limit, status, and arena globals. A caller publishes its current arena immediately before a call, reloads the callee's advanced arena afterward, and propagates a nonzero shared status without using the returned descriptor. The public wrapper resets all shared state before every run and publishes output only after `Main` completes with status zero.

### Profile 12 canonical metadata/reference consumer

`Wvb-Semantic-Verify-Main.wv` is the retained eight-function consumer. It first applies the complete profile-11 structural pass and then validates Seed identifiers, strict text UTF-8, canonical module/data/function/export/type ordering, the exact capability catalog and signatures, nominal declaration shapes, enum identities and backing values, local/function/capability/data/type instruction operands, exact jump and branch boundaries, and export-to-function identity. Every phase receives and returns the same bounded input descriptor; `Main` returns `[1]` only after all phases succeed and returns an empty result after any rejection.

The artifact occupies exactly 65,536 aggregate WVB code bytes. This is canonical declaration/reference evidence, not complete executable semantic verification. Typed operand-stack flow, type-correct access to deterministically default-valued locals, call argument/result flow, record-field receiver identity, joins, reachability, maximum-stack agreement, and authorization remain outside this profile-12 consumer's acceptance claim. Exact branch-boundary checks use bounded allocation-free rescans and are quadratic on branch-heavy modules; ordinary retained inputs finish below 1.2 million instructions, while the unusually large verifier module itself exceeds 500 million verification instructions.

### Profile 13: expanded descriptor-bearing call graph

Profile 13 retains profile 12's execution ABI, operation set, signatures, decreasing-ordinal call rule, and per-function ceilings while increasing the bounded graph to:

- two through sixteen `bytes -> bytes` functions;
- 131,072 aggregate WVB code bytes;
- 400,000 aggregate decoded instructions; and
- 1,048,576 generated Wasm bytes when the input crosses a profile-12 function, code, or instruction boundary.

Profile-12-sized inputs retain the 524,288-byte output ceiling and their exact generated bytes. `Main` remains the only export and final canonical function. With decreasing call ordinals, dynamic call depth remains statically bounded by sixteen without recursion.

### Profile 13 compiler-aligned executable consumer

The complete consumer composes `Wvb-Executable-Verify-Phase.wv` after the seven retained semantic phases. `Hˉexecutable` proves exact local and operand-stack shapes, primitive and bytes operations, calls, capability signatures, record construction and field receivers, enum identities, returns, and declared maximum stack. `Iˉcontrol` proves the source compiler's empty-stack control-boundary, terminator-target, and reachability contract. General WVB locals retain deterministic type-specific defaults; this consumer does not impose store-before-load semantics.

Candidate input is bounded to 256 functions, 131,072 aggregate code bytes, 16,000 aggregate decoded instructions, and declared stack depth sixteen. The control proof is deliberately compiler-aligned: it does not claim to accept every valid general WVB graph with nonempty stack joins. Capability declarations and argument/result shapes are verified, but authorization remains a separate host or worker decision.

The complete consumer has ten functions and 108,331 aggregate code bytes. Its two added phase functions respectively use `2,019 / 32,766 / 3` and `564 / 9,793 / 3` nonparameter locals, code bytes, and maximum stack, preserving every profile-13 per-function ceiling.

### Profile 14: scalar runtime operations and first WVB interpreter

Profile 14 retains profile 11's one `bytes -> bytes` function, execution ABI 3, fixed memory, runtime values, terminator-aligned control, exact metering, 2,047-local, 32,768-code-byte, and 100,000-instruction limits. It additionally admits:

- checked `i32.add`, `i32.subtract`, `i32.multiply`, and `i32.negate`;
- all six signed `i32` comparisons; and
- checked `u32.multiply` in addition to profile 10's unsigned addition, subtraction, and comparisons.

The generated code detects every Windvale overflow explicitly. Signed operations reuse the existing widened or sign-bit proofs. Unsigned multiplication widens both operands to `i64`, multiplies, rejects a value above `u32` maximum as `WVR3007`, and wraps to `i32` only after that proof. No target wrapping operation defines a Windvale result.

The retained consumer is `Wvb-Scalar-Interpreter-Main.wv`. It accepts a versioned `WVXI 1` request containing a guest instruction budget, guest call-depth limit, and exact WVB bytes that have already passed the complete Decision 0149 verifier. It returns a fixed `WVXO 1` response containing guest status, charged guest instructions, and one `i32` result. The interpreter preflight is a bounded profile selector and must not be used as the untrusted-input verifier.

The candidate execution subset is portable, capability-free WVB 1.6 with no nominal types; one through sixteen scalar functions; at most eight parameters and thirty-two parameters plus locals per function; declared stack depth at most sixteen; at most 4,096 aggregate instructions; and `Main() -> i32`. It interprets deterministic default-valued `i32`, `u32`, `u8`, and `bool` locals; checked arithmetic and scalar comparisons; boolean operations; `u32.from_u8`; terminator-aligned jumps and branches; direct calls; `pop`; and returns. Guest budget is one through 4,096 and call depth is one through eight.

The complete verifier and interpreter are deliberately separate import-free artifacts. A worker runs the verifier first, applies capability policy, and only then builds `WVXI`. Empty interpreter output after successful verification means the candidate is outside profile 14. `WVXO` status `3011` reports guest instruction exhaustion and `3004` reports guest call-depth exhaustion; an arithmetic trap propagates through the enclosing execution ABI as status `3007` with no output.

### Profile 15: bounded WVB text and bytes values

Profile 15 retains profile 14's one `bytes -> bytes` runtime function, execution ABI 3, fixed memory, scalar operations, terminator-aligned control, exact metering, and 100,000 decoded-instruction limit. It expands the selector ceiling to 4,095 nonparameter locals and 65,536 code bytes while retaining the 524,288-byte generated-Wasm limit.

The retained `Wvb-Scalar-Interpreter-Main.wv` consumer keeps `WVXI 1` and `WVXO 1`, guest budget 4,096, guest call depth eight, sixteen functions, eight parameters, sixteen operand-stack cells, and `Main() -> i32`. It expands each frame to 128 eight-byte local cells plus sixteen eight-byte stack cells. Scalars occupy the low four bytes of a zero-extended cell; text and bytes occupy an unsigned heap-offset/length descriptor. Eight fixed 1,040-byte frames preserve the existing call-depth bound.

One append-only 65,536-byte guest heap owns static and constructed descriptor values. `text.const` and `bytes.const` copy their static data when executed; slices are borrowed views; each constructed or copied value is limited to 16,384 bytes. The interpreter admits static-data length and `i32` load, text and bytes constants, descriptor movement through locals and calls, byte length/slice/little-endian reads, byte concatenation and fixed-width construction, text concatenation, strict UTF-8 validation and construction, and text-to-UTF-8 descriptor reinterpretation.

Range, invalid-UTF-8, u16-narrowing, per-value, and aggregate-heap failures return guest statuses `3008`, `3014`, `3016`, `3015`, and `3018`. The retained interpreter also formats `i32`, `u8`, and `u32` as invariant ASCII decimal and quotes strict UTF-8 text into the reference runtime's ASCII JSON-style report form. Short escapes cover quote, reverse solidus, and five controls; printable ASCII is preserved; all other UTF-16 code units use uppercase `\uXXXX`, including surrogate pairs for supplementary scalars. Formatted and quoted results use the same bounded heap and value charging.

`bytes.sha256_hex`, records, enums, capabilities, reclaiming allocation, and general nonempty-stack joins remain outside profile 15. Empty output after successful complete verification continues to mean “outside interpreter profile.”

### Profile 16: import-free SHA-256

Profile 16 retains profile 15's execution ABI, selector ceilings, fixed memory, interpreter frames, guest budgets, per-value limit, and append-only guest heap. It admits text descriptor locals plus WVB `text.to_utf8` and `bytes.sha256_hex`. Text-to-UTF-8 preserves the verified descriptor; SHA-256 emits deterministic Wasm integer and memory instructions directly and adds no import.

Bytes 0 through 335 of linear memory are private SHA-256 scratch. The region holds 64 schedule words, eight hash-state words, eight working words, padded length, block cursor, and two temporaries. It is disjoint from the ABI input at 65,536, output at 4,259,840, and allocation arena. A successful hash appends exactly 64 lowercase ASCII hexadecimal bytes to the charged arena; insufficient space returns `WVR3018` before publication. The retained interpreter uses that target operation over a guest-heap slice and charges it as one semantic WVB instruction while the enclosing Wasm execution retains its independent outer budget.

### Retained profile-16 interpreter: direct records and enums

The retained interpreter additionally consumes the WVB nominal-type section and admits value shapes and guest opcodes for records and enums. This is an interpreter expansion, not profile 17: the outer interpreter source still uses only profile-16 types and operations, execution ABI 3 and the `WVXI 1` / `WVXO 1` formats do not change, and all previously generated Wasm artifacts remain byte-identical.

Every guest value remains one eight-byte cell. An enum stores its canonical signed 32-bit backing bytes plus nominal type index. A record stores a checked offset into a separate append-only 4,096-byte immutable-field arena plus nominal type index; each field consumes one complete eight-byte cell. Record construction preserves declaration order, field access copies one cell, and calls and locals copy descriptors without host identity. A reserved record offset represents the deterministic default record, so default field access can produce zero primitives and descriptors, the first declared enum member, or another default record without eager recursive allocation. Enum constants, equality, inequality, and name lookup use canonical type metadata; `enum.name` copies UTF-8 bytes into the existing charged 65,536-byte guest heap.

The selector accepts at most 1,024 nominal types, 64 fields per record, and 256 members per enum, matching the WVB format limits, while retaining sixteen guest functions, eight parameters, 128 local cells, sixteen stack cells, 4,096 aggregate instructions, guest budget 4,096, and call depth eight. Unsupported `i64`/`u64`, capabilities, and code shapes still produce empty interpreter output after complete verification. Record-arena exhaustion returns guest `WVR3017`; enum-name value and heap failures use the existing `WVR3015` and `WVR3018` statuses. Every failing operation is charged before the resource check.

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

## Execution ABI 2 and profiles 4 through 7 output

Profiles 4 through 7 retain ABI 1's three published globals and exact export names but change the exported function type to `(i32) -> i32` and set `Windvale.abi` to `2`:

| Export | WebAssembly kind | Contract |
| --- | --- | --- |
| `Windvale.run` | function `(i32) -> i32` | Accepts a positive instruction limit and returns `0`, `3007`, or `3011`. A limit of `N` permits exactly `N` WVB instruction charges and returns `3011` before instruction `N + 1`. |
| `Windvale.abi` | immutable `i32` global | Contains execution ABI version `2`. |
| `Windvale.result` | mutable `i32` global | Contains `Main`'s result only after status zero; reset before every run. |
| `Windvale.instructions` | mutable `i32` global | Contains the exact number of WVB instructions charged; reset before every run and never exceeds the supplied limit. |

Status `3011` maps to the existing `WVR3011` instruction-limit diagnostic. The host admits limits from 1 through 2,147,483,647. The generated meter compares the current unsigned count with the supplied limit before each instruction, returns `3011` when no charge remains, and otherwise increments the count once. The browser host also retains its independent two-second disposable-worker timeout.

The retained terminating fixture performs six loop iterations and returns `42`. A limit of `157` succeeds with 157 instructions; `156` returns `3011`, leaves result zero, and reports 156. Its 972-byte artifact has SHA-256:

```text
1c429ca20faa42b5018ea565ad10f148792dfbf6a8ecd438cf990cd60d664afe
```

The retained nonterminating fixture returns `3011` with result zero and exactly 50 instructions under limit 50. Its 663-byte artifact has SHA-256:

```text
325b6f8c9f8d7e2557f93c412aa85b913295dc4bfda5fbb32fb2337915109fde
```

The retained profile-5 mixed-control fixture contains two loops separated by one `if/else`. Its 566-byte WVB has SHA-256 `28eeed9d8f77f87f2c69399be05a1e6f3cb53b813ed949d7d2fde65a83dac50f`. The selected true route succeeds at budget 184 with result `42`; budget 183 returns `3011/0/183`. Its 1,923-byte artifact has SHA-256:

```text
454e8af4f739ede63e0b2d55b8907f6075fec1495a4123df53ef5ebcf3ea2c4b
```

The matching 544-byte false-route WVB has SHA-256 `37dcab42a4bdff5c4f89a2252b79880a1da65bf66d3251c1edfd2398f714ae49`. It executes the `else` route and succeeds at budget 331; budget 330 returns `3011/0/330`. Its 1,770-byte artifact has SHA-256 `242116d69f8c28acf4886b1210ffd2b75e622ce92b44586a8a1668188930a84b`.

The retained 399-byte two-`if` WVB has SHA-256 `061e1db0f14dd36d32235a44502b0b3accdd5c3cad529c3926a381a293884148`. Its first conditional takes the false route and its second takes the true route. It succeeds at budget 41 and returns `3011/0/40` one instruction below it. Its 1,164-byte artifact has SHA-256 `d4fd2bf65a6b4aebf55aaf033e86984a4e882761a4c9a59d85bd7ca8353a21ba`.

The retained profile-6 fixture has three functions: `Add(i32, i32)`, `Double(i32)`, and exported `Main()`. `Main` calls `Double` twice and `Add` once, while `Double` calls `Add`; the maximum dynamic call depth is three. Its 399-byte WVB has SHA-256 `502f5e9394248db4e21b49a3a98173917c2ff6f9a8252bef606a7a6c845d6482`. The 1,185-byte import-free artifact succeeds at budget 66 with result `42`, returns `3011/0/65` one instruction below it, and has SHA-256:

```text
d92667752762a992bdb626e34b83b78ee9c531f167b911737dfbf5f6443f3518
```

The retained callee-overflow fixture produces 301-byte WVB SHA-256 `9e2b2a747287ff49ffce4d34f888b557a48064062e75ff5147bfc0224b54dca2` and a 737-byte Wasm artifact SHA-256 `4e936e5c4b077d1bce8719f5cc5c974961088f1171ed00158f9ac251f7652bd7`. A multiplication overflow inside `Calculate` propagates through `Main` as `3007/0/14` under budget 100 and repeats with the same reset evidence.

The retained profile-7 true-route fixture defines `Add`, looping `Build`, and exported `Main`; both the loop body and selected `if` route make real direct calls. Its 716-byte WVB has SHA-256 `cf519c2d636d6e7b22b54afacf632bf5e514982e030d5c8b799a5585e7f39120`. The 2,729-byte import-free artifact succeeds as `0/42/196`, returns `3011/0/195` one instruction below the exact reference budget, and has SHA-256:

```text
3be50be3c2436638973eb68743f9fdd2e00df9816e50e498b432ff36468c3a77
```

The matching false-route fixture has 722-byte WVB SHA-256 `77e65ba692c8abc87dbac4dfeba174f3afc9191ac784b47a65becae8f0df2752`. It calls `Add` from the `else` route, succeeds as `0/42/153`, returns `3011/0/152` one instruction below it, and emits a 2,729-byte artifact with SHA-256 `35d75c30ef03dbb693a976cfaa31405ce90ecca4d393c5e93de8953fcf4658da`.

## Execution ABI 3 and profiles 8 through 14 output

Profile 8 emits an import-free WebAssembly binary version-1 module with one fixed, non-growable 129-page memory. Page zero is reserved. The host owns a 4 MiB input window beginning at byte 65,536; the guest owns a separate 4 MiB output window beginning at byte 4,259,840. The regions are disjoint, checked before access, and exactly fill the remaining memory.

The module exports these values in this exact order:

| Export | WebAssembly kind | Contract |
| --- | --- | --- |
| `Windvale.run` | function `(i32, i32) -> i32` | Accepts instruction budget and input byte length; returns `0` or a profile-defined Windvale status. Profiles 8 through 14 currently use `3007`, `3008`, `3011`, `3014`, `3015`, `3016`, and `3018`. |
| `Windvale.abi` | immutable `i32` global | Contains execution ABI version `3`. |
| `Windvale.memory` | memory | The fixed 129-page linear memory. |
| `Windvale.input_offset` | immutable `i32` global | Contains `65,536`. |
| `Windvale.input_capacity` | immutable `i32` global | Contains `4,194,304`. |
| `Windvale.output_offset` | immutable `i32` global | Contains `4,259,840`. |
| `Windvale.output_capacity` | immutable `i32` global | Contains `4,194,304`. |
| `Windvale.output_length` | mutable `i32` global | Contains the valid output byte count only after status zero; reset before every run. |
| `Windvale.output_kind` | immutable `i32` global | Contains `1` for opaque bytes or `2` for strict UTF-8 text. |
| `Windvale.instructions` | mutable `i32` global | Contains exact charged WVB instructions; reset before every run and never exceeds the supplied budget. |

Before calling `Windvale.run`, a conforming host validates the export set, immutable layout globals, memory extent and non-growth property, copies exactly `input_length` bytes into the input window, and supplies a positive instruction budget. On status zero it validates `output_length <= output_capacity`, copies exactly that many bytes from the output window, and treats all other memory as unavailable. It must not use the output descriptor after failure. The input may alias no guest output because the two regions are fixed and disjoint.

`WVR3008` reports a negative or over-capacity input length before any instruction is charged. `WVR3011` preserves the ABI-2 budget contract. `WVR3014` reports malformed text input before the first WVB instruction. The guest resets output length and instruction count before all validation and execution. A successful identity run charges four instructions and copies with WebAssembly `memory.copy`; budget three returns `3011` with empty output and three charged instructions. Memory outside the published successful output length may retain stale bytes and is never result evidence.

Profile 9 additionally uses `WVR3008` for a slice or byte-read range failure, `WVR3015` when one constructed bytes value exceeds output capacity, `WVR3016` when a u16 constructor receives more than 65,535, and `WVR3018` when aggregate monotonic allocation exhausts the output arena. Every operation is metered before its validation or memory access, so the reported count includes the failing WVB operation. Only the successful final copy and length store publish output.

The retained bytes fixture has 179-byte WVB SHA-256 `3d751ca734faed1832b4d33a9f0cfc605b695f3ae8156e3d431504798869c8d9`. It emits a 435-byte Wasm module with SHA-256:

```text
b5f87bd47be7a0ce0bb6755de4ecea8bc311c9412ee28d6091092e7aa4c184f5
```

The retained text fixture has 178-byte WVB SHA-256 `c19463d24d65c1bc46dca48dcda8541491b53b7289483afe4508685f30e0fbda`. Its strict validator produces a 791-byte Wasm module with SHA-256:

```text
c3635b8df4ed9d471faad7e653e975662099c0a2336639586915ce50b768542d
```

The retained profile-9 runtime fixture has 914-byte WVB SHA-256 `6436f97c0e9abf131cc3a503c4449104706aa66eb0292a282a978fb7a5c5e100`. It composes every admitted primitive family and byte operation, succeeds with a 19-byte result after 155 instructions, and emits a 4,878-byte Wasm module with SHA-256:

```text
7bd5d2b0bc256503cd07dc300e528da38f8a09bcfec4c2b1007c1994db1b88f4
```

Focused concatenation, u16-guard, and aggregate-arena artifacts have respective deterministic Wasm identities `94533e9d01bdfcc606a3225ac28c774ecadd3cc0e0eccb02a7dba4f3fdb4ccb2`, `f312812fedae4c8dd45ffcb022301c1e85d7bdad4c71906a771cfc95333cde41`, and `0e37802a606ee67abd467ddc5da84f0d18807bb86b8bf497c4bdf0a41fa5a089`.

The checked unsigned-arithmetic fixture has 447-byte WVB SHA-256 `d6ba02dfe12efdcb7c2f8ed6664551a776e79e3ff2c30134dc7e3642ee7ce743` and 1,893-byte Wasm SHA-256 `f645c0ff095eb06c825fea056659545cc258d857da55fc9dfd1a928812373f61`. It proves success plus distinct checked addition overflow and subtraction underflow paths.

The retained profile-10 envelope verifier has 2,837-byte WVB SHA-256 `1362b2707a4ff442a1458e3f821e01108bb948858db21e022bfee05869c2fb86`. It returns `[1]` for its own valid envelope after 2,206 instructions and emits a 14,902-byte Wasm module with SHA-256:

```text
f493777450b720ef786b60502528819969ad9e0322aa55a9c0259f6de20850fc
```

The retained profile-11 structural verifier has 19,755-byte WVB SHA-256 `72da44ba1292ed3ef4ac62c239dd937862636229a7d60302305a7dd19ac27376`. It returns `[1]` for its own completely consumed payloads after 1,446,276 instructions and emits a 113,385-byte Wasm module with SHA-256:

```text
46fe579fb7082dd4b0dd981e09f6b953127e52c9c6993d7885ca130725762677
```

The profile-12 descriptor-call fixture has 764-byte WVB SHA-256 `a44c8bdbf9983a7929a769d5ca2e0b60323d72cf96b04e31450d9757bb15729a`. Its nested calls and conditional route map `[9, 8]` to `[9, 9, 8]` after 127 instructions; budget 126 returns `WVR3011` without output. It emits a 4,086-byte Wasm module with SHA-256:

```text
5ee04d5b3b33399dce61709135709f0d0ebb7d6374e14759d83986859806eadd
```

## Limits and failure behavior

- Input WVB is limited to the current 4 MiB immutable-`bytes` value and hosted-file boundary. This is narrower than WVB's general 16 MiB module limit.
- Profiles 3 through 5 are independently bounded to 256 locals, 16,384 code bytes, and 4,096 instructions. Profile 3 admits maximum operand-stack depth through 256; profiles 4 and 5 require exactly two.
- Profiles 6 and 7 admit two through eight functions, zero through two parameters per function, 256 combined parameters and locals per function, stack depth one or two, and the same per-function code and instruction bounds; aggregate code and instruction limits are 32,768 bytes and 8,192 instructions. Profile 7 additionally admits compiler-produced `bool` locals.
- Profile 8 admits one exact text or bytes identity function and independently limits both input and output to 4,194,304 bytes in fixed disjoint memory windows.
- Profile 9 admits one straight-line bytes function, at most 255 nonparameter primitive/bytes locals, stack depth four, 16,384 code bytes, and 4,096 instructions. Each bytes value and the aggregate output arena are independently limited to 4,194,304 bytes.
- Profile 10 retains profile 9's type, code, stack, memory, value, and arena bounds while admitting checked unsigned arithmetic and terminator-aligned control targets.
- Profile 11 retains profile 10's operation, stack, memory, value, and arena model while increasing the one-function selector ceiling to 2,047 nonparameter locals, 32,768 code bytes, and 100,000 instructions.
- Profile 12 admits two through eight `bytes -> bytes` functions under those per-function local, code, instruction, stack, memory, value, and arena bounds; aggregate code is at most 65,536 bytes and aggregate instructions at most 200,000.
- Profile 13 admits two through sixteen `bytes -> bytes` functions under the unchanged per-function bounds; aggregate code is at most 131,072 bytes and aggregate instructions at most 400,000.
- Profile 14 returns to profile 11's single-function bounds and adds checked signed scalar arithmetic/comparisons plus checked `u32.multiply`. Its retained interpreter independently limits candidates to sixteen functions, eight parameters, thirty-two frame values, stack depth sixteen, 4,096 aggregate instructions, guest budget 4,096, and guest call depth eight.
- Profile 15 expands the one-function selector ceiling to 4,095 nonparameter locals and 65,536 code bytes. Its retained interpreter uses eight fixed 1,040-byte frames, 128 eight-byte locals and sixteen stack cells per frame, a 16,384-byte per-value limit, and one append-only 65,536-byte guest heap.
- Profile 16 retains every profile-15 bound and additionally admits text descriptor locals, descriptor-preserving `text.to_utf8`, and import-free `bytes.sha256_hex` using a private 336-byte scratch region.
- The retained profile-16 interpreter additionally accepts WVB record/enum shapes and operations under the format's 1,024-type, 64-field, and 256-member bounds, plus a separate 4,096-byte immutable record-field arena with `WVR3017` exhaustion.
- The compiler-capacity verifier bundle independently raises candidate verification to 4,096 functions, 4 MiB aggregate code, 400,000 decoded instructions, and 4,096 operand cells. It retains the existing WVB-format parameter, local, and nominal-type limits and does not enlarge the interpreter.
- Encoded WebAssembly output is limited to 524,288 bytes for profile-12-sized inputs and 1,048,576 bytes when a profile-13 input crosses a profile-12 function, code, or instruction boundary.
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

Profile 4 additionally requires independent reconstruction of the emitted block, loop, dynamic meter, comparisons, and branches; exact agreement with the reference runtime at the measured success budget and one instruction below it; nonterminating-loop containment; reset and deterministic repeat behavior; malformed back-edge and exit-target rejection without publication; and real browser-worker execution before browser integration is claimed.

Profile 5 additionally requires independent reconstruction of every emitted loop, `if`, `else`, join, and meter; exact reference agreement for both selected conditional routes; exact success and one-instruction-short exhaustion; deterministic repeat behavior; and rejection of malformed, crossing, overlapping, or nested region edges without publication.

Profile 6 additionally requires independent reconstruction of every generated type, wrapper, private function body, meter, real direct call, and post-call status propagation; exact reference agreement at the measured success budget and one instruction below it; deterministic repeat behavior; and rejection of forward, self, cyclic, unknown, or arity-invalid calls without publication.

Profile 7 additionally requires the profile-5 edge and join reconstruction and profile-6 call reconstruction to succeed together for every function; exact reference agreement for selected true and false routes; calls inside loop and conditional bodies; shared-budget success and one-instruction-short exhaustion; deterministic repeat behavior; and rejection of malformed, crossing, overlapping, or nested regions without publication.

Profile 8 additionally requires independent reconstruction of the exact function shape, type, memory declaration, export order, layout globals, wrapper, meter, UTF-8 validator, and memory copy; empty, ordinary, Unicode, arbitrary-byte, and exact-4-MiB boundary round trips; malformed UTF-8 agreement with an independent strict decoder; input-length and budget failures with invalid output descriptors; fixed-memory growth rejection; and deterministic repeat behavior.

Profile 9 additionally requires independent reconstruction of source local types, definite initialization, operand types, the complete generated local and opcode stream, exact meter count, fixed memory, descriptor-only internal locals, ordered exports, final output normalization, and bulk-memory operations. Execution evidence must cover every admitted value/byte operation, reference-runtime agreement, deterministic repeat output, one-instruction-short exhaustion, read and slice range failures, exact 4 MiB value success, `WVR3015` one byte above it, u16 success and `WVR3016`, distinct aggregate `WVR3018`, invalid input lengths before metering, and fixed-memory growth rejection.

Profile 10 additionally requires independent reconstruction of every WVB control target and terminator-aligned basic block, complete decoding of the generated dispatch loop and program-counter accesses, exact dynamic success and exhaustion budgets, valid and malformed WVB envelope results, deterministic repeat bytes, and rejection of a target inside an instruction operand before publication.

Profile 11 additionally requires exact payload consumption for all seven WVB sections; positive modules with nonempty data/text, nominal types, and hosted capabilities; one targeted malformed case per section; independent decoding of more than 1,000 source locals and 4,000 source instructions; exact self-verification and one-instruction-short budgets; and deterministic 512-KiB-bounded output identity.

Profile 12 additionally requires independent reconstruction of both Wasm types, every function type index, private local layouts, decreasing WVB and Wasm call ordinals, shared status/arena/limit globals, post-call status propagation, and wrapper reset. Execution evidence must cover nested calls, a call inside control, empty and nonempty byte values, exact reference-runtime agreement, one-instruction-short exhaustion inside the shared budget, repeated-run arena reset, and malformed forward or self-call rejection without publication.

Profile 13 additionally requires preservation of every profile-12 artifact; exact enforcement of the sixteen-function, 131,072-code-byte, 400,000-instruction, and conditional 1-MiB output limits; and rejection of a seventeen-function graph. The compiler-aligned executable consumer additionally requires Stage 0 differential agreement for representative data/text, record/enum, and capability-bearing modules; exact success and one-instruction-short budgets; and hostile cases covering operator stack kinds, local stores, call arguments, record receivers, enum identity, branch conditions, unreachable regions, declared maximum stack, and capability arguments.

Profile 14 additionally requires preservation of every earlier generated Wasm identity; exact signed and unsigned overflow behavior; source-profile and emitted-Wasm validation by the independent C# decoder; complete-verifier admission before interpretation; reference-runtime agreement for scalar arithmetic, comparisons, calls, and control; deterministic repeat; exact guest instruction and call-depth exhaustion; exact outer instruction exhaustion; and execution under an independent WebAssembly engine.

Profile 15 additionally requires preservation of every earlier generated Wasm identity; complete-verifier admission for every interpreted candidate; reference-runtime agreement for static data, descriptor calls, byte reads, slices, construction, concatenation, text concatenation, valid/invalid UTF-8, invariant formatting, and deterministic quoting; valid two-, three-, and four-byte UTF-8 boundary coverage; signed minimum, signed maximum, unsigned maximum, zero, short escapes, BMP, DEL, and supplementary quote coverage; exact `WVR3008`, `WVR3014`, and `WVR3016` differential failures; exact interpreter-profile `WVR3015` and `WVR3018` limits; rejection of a verifier-valid unsupported byte-hash operation; deterministic repeat; and independent emitted-Wasm decoding and engine execution.

Profile 16 additionally requires all earlier generated Wasm identities to remain byte-exact; complete-verifier-first execution; reference-runtime agreement for empty, ordinary, padding-boundary, and multi-block SHA-256 inputs; exactly 64 lowercase hexadecimal output bytes; arena-exhaustion behavior; no imports; and independent decoding of the emitted scratch-memory, rotate, shift, bitwise, block, loop, and select structure.

The compiler-capacity bundle additionally requires all earlier generated Wasm identities to remain byte-exact; three fresh, import-free ABI-3 instances over identical candidate bytes; exact independent phase meters; rejection unless metadata/references, typed execution, and control/reachability all return the one-byte success value; high-bit instruction-count preservation by the host; exact reconstruction and identity of the compiler WVB; and no claim of compiler execution from verification alone.

On Windows, `pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1` rebuilds the Windvale-authored backend, compiles and lowers thirty-four generated Wasm artifacts through profile 16 plus the compiler-capacity bundle, reconstructs the exact compiler WVB, checks sizes and digests, and executes every output under the installed Node.js WebAssembly engine. The ABI-2 cases require exact success, one-instruction-short exhaustion, reset across repeated runs, shared-budget call execution, calls within both conditional routes, callee-overflow propagation, and nonterminating-loop containment. The ABI-3 cases additionally exercise exact-capacity bytes, curated UTF-8 boundaries, deterministic randomized agreement with Node.js's fatal UTF-8 decoder, general byte construction, checked unsigned overflow and underflow, distinct value, narrowing, range, and aggregate-allocation failures, descriptor-bearing calls through control, valid plus hostile WVB envelopes, section payloads, canonical metadata, references, and executable flow through the Windvale-native verifiers, followed by complete-verifier-approved scalar, text/bytes, formatting/quoting, SHA-256, record, enum, typed-default, and record-arena interpretation. The final three instances admit the exact compiler independently through canonical metadata/references, typed execution, and control/reachability.

Exact profile-4 implementation commit `1342f63bc7eaae17a526ca440b075c2abf3c3b31` passed GitHub [Verify run 30770158910](https://github.com/eworker-inc/Windvale/actions/runs/30770158910). Its Windows and digest-pinned Debian jobs each passed the complete repository verifier with zero-warning builds, all 68 Seed tests, and all 25 OS tests. This establishes deterministic equality for the retained profile-4 WVB and WebAssembly identities and the exact `0/42/157`, `3011/0/156`, and nonterminating `3011/0/50` execution evidence on both hosts.

Exact commit `a2285f5a0c09598ec701691bdbf0af9080e8cf0c` establishes Windows and digest-pinned Debian 12 equality for the backend WVB, selected input WVB, and generated WebAssembly digests through GitHub [Verify run 30762541741](https://github.com/eworker-inc/Windvale/actions/runs/30762541741). Both host qualification jobs completed successfully; the run-level conclusion changed to `cancelled` only after both jobs completed, when a later `main` push activated workflow concurrency cancellation.

[Decision 0107](../Documents/Decisions/0107-Playground-Disposable-WebAssembly-Worker.md) integrates the exact profile-3 success WVB and Wasm identities into the playground. On 2026-08-02, a Chromium-based in-app browser twice validated and ran the transferred 432-byte import-free module in a fresh worker, reporting ABI `1`, status `0`, result `42`, and 30 instructions equal to the .NET reference path, with no browser warning or error. This is local browser integration evidence, not cross-browser qualification or complete playground isolation.

[Decision 0113](../Documents/Decisions/0113-Metered-WebAssembly-Control-Flow.md) advances the same worker and the .NET-free page to ABI 2. On 2026-08-02, a Chromium-based in-app browser validated and ran the pinned 972-byte loop module in fresh workers: budget 157 reported `0/42/157`, budget 156 reported `3011/0/156`, and the page recorded zero .NET/Blazor requests. The seven observed assets were the stylesheet, logo, analytics script, application, artifact data, shared host, and shared worker; the browser console contained no warning or error. This remains one browser-engine family rather than cross-browser qualification.

The exact implementation commit also passed GitHub [Deploy homepage run 30770158921](https://github.com/eworker-inc/Windvale/actions/runs/30770158921): the deployment lane independently reconstructed and executed the embedded artifact before successfully publishing the static playground content.

[Decision 0116](../Documents/Decisions/0116-Sequential-WebAssembly-Control-Regions.md) advances the selector and .NET-free page to profile 5. On 2026-08-02, a Chromium-based in-app browser validated and executed the pinned 1,923-byte mixed-control module in fresh workers: budget 184 reported `0/42/184`, budget 183 reported `3011/0/183`, and the page recorded zero .NET/Blazor requests. Exact implementation commit `87cb0a3c83441d34c8307243df5dee4ffb220417` passes GitHub [Verify run 30772366223](https://github.com/eworker-inc/Windvale/actions/runs/30772366223): Windows and digest-pinned Debian 12 each pass zero-warning builds, all 70 Seed tests, all 25 OS tests, and the complete native CLI gate. [Deploy homepage run 30772366229](https://github.com/eworker-inc/Windvale/actions/runs/30772366229) independently verifies and publishes the embedded artifact. Profile 5 is therefore cross-host qualified; cross-browser qualification remains open.

[Decision 0120](../Documents/Decisions/0120-Bounded-WebAssembly-Call-Graph.md) advances the selector and .NET-free page locally to profile 6. On 2026-08-02, a Chromium-based in-app browser validated and executed the pinned 1,185-byte three-function module in fresh workers: budget 66 reported `0/42/66`, budget 65 reported `3011/0/65`, and the page recorded zero .NET/Blazor requests. Cross-host and cross-browser qualification remain pending.

[Decision 0121](../Documents/Decisions/0121-WebAssembly-Calls-With-Structured-Control.md) advances the selector and .NET-free page locally to profile 7. The retained Node.js evidence validates both conditional routes, exact shared-budget exhaustion, calls inside loop and conditional bodies, deterministic output, and malformed or nested-region rejection. On 2026-08-02, a Chromium-based in-app browser validates and executes the pinned 2,729-byte artifact in fresh workers: budget 196 reports `0/42/196`, budget 195 reports `3011/0/195`, the console contains no warning or error, and the observed development assets contain no `.NET`, Blazor, or `_framework` URL. Cross-host and cross-browser qualification remain pending.

[Decision 0123](../Documents/Decisions/0123-Versioned-WebAssembly-Linear-Memory-And-Utf8-Buffers.md) advances the selector, shared worker, and .NET-free page locally to profile 8 and execution ABI 3. Node.js validates arbitrary-byte and strict-UTF-8 round trips through fixed 4 MiB regions, exact-boundary copies, memory non-growth, deterministic budget and range failures, and 20,000 generated byte sequences against an independent fatal UTF-8 decoder. On 2026-08-02, a Chromium-based in-app browser round-trips both default and edited multilingual values, reports exact `0/4` success and `3011/3` exhaustion, zero .NET/Blazor requests, and no console warning or error. Cross-host and cross-browser qualification remain pending.

[Decision 0128](../Documents/Decisions/0128-Bounded-WebAssembly-Runtime-Values.md) advances the selector locally to profile 9 while retaining ABI 3. The independent C# decoder and Node.js engine cover primitive and bytes locals, byte reads and slices, widening, little-endian construction, concatenation, output normalization, exact metering, deterministic identities, an exact 4 MiB result, and distinct `WVR3008`, `WVR3011`, `WVR3015`, `WVR3016`, and `WVR3018` failures. The static page remains on the profile-8 text artifact; cross-host and cross-browser qualification remain pending.

[Decision 0131](../Documents/Decisions/0131-Windvale-Native-WebAssembly-Wvb-Envelope-Verifier.md) advances the selector to profile 10 while retaining ABI 3. Compiler-produced nested conditionals and a loop lower through a validated basic-block dispatcher, allowing the first Windvale-written WVB 1.6 envelope verifier to execute as import-free Wasm. The reference runtime, independent C# decoder, and Node.js agree on valid, truncated, hostile-length, bad-magic, trailing-data, deterministic-repeat, and one-instruction-short cases. Exact descendant `ea1aa89` qualifies the portable envelope-verifier path on Windows and Debian; the static page remains on profile 8, and cross-browser qualification remains pending.

[Decision 0134](../Documents/Decisions/0134-Windvale-Native-WebAssembly-Wvb-Structural-Verifier.md) advances the selector locally to profile 11. A one-pass boundary mask and per-basic-block emission keep a 4,062-instruction Windvale verifier inside the retained hosted selector gate. Its import-free Wasm completely consumes bounded WVB 1.6 module, capabilities, data, functions, code, exports, and types payloads and rejects a targeted corruption in every section. It is structural rather than semantic verification; the static page remains on profile 8, and cross-host and cross-browser qualification remain pending.

[Decision 0139](../Documents/Decisions/0139-Descriptor-Bearing-WebAssembly-Call-Graph.md) advances the selector locally to profile 12. Real private `(i64) -> i64` Wasm functions carry bounded bytes descriptors through acyclic calls while shared globals preserve one status, instruction budget, and arena across terminator-aligned control. The three-function differential fixture and malformed self-call coverage pass under the independent C# decoder and Node.js engine; cross-host and cross-browser qualification remain pending.

[Decision 0144](../Documents/Decisions/0144-Modular-WebAssembly-Wvb-Canonical-Metadata-And-References.md) uses profile 12 for the first modular semantic phase. The eight-function Windvale verifier completely consumes WVB 1.6 and validates canonical names, catalog and declaration identities, strict text UTF-8, nominal references, instruction operands, exact control targets, exports, fields, and enum values. Its 70,016-byte WVB occupies the exact 65,536-byte aggregate-code ceiling and lowers deterministically to 440,093-byte import-free Wasm. Three representative modules, one-short exhaustion, and thirteen Stage-0-oracle-rejected semantic mutations pass locally under the reference runtime and Node.js. Typed executable flow, cross-host construction, cross-browser execution, and the default-playground switch remain pending.

[Decision 0146](../Documents/Decisions/0146-Expanded-Descriptor-Bearing-WebAssembly-Call-Graph.md) advances the selector locally to profile 13 without changing execution ABI 3. It doubles the bounded descriptor graph and preserves the profile-12 semantic artifact byte for byte. A derived nine-function verifier crosses both former aggregate limits and rejects a seventeen-function graph.

[Decision 0149](../Documents/Decisions/0149-Windvale-Native-WebAssembly-Wvb-Executable-Verifier.md) uses profile 13 for the compiler-aligned executable phase. The complete ten-function verifier is 115,483 WVB bytes with SHA-256 `6a26b09c0f96e3fa9edf8c180ee8f4b2551f1b1007f0faabcec39be1106285b4` and emits 722,837 import-free Wasm bytes with SHA-256 `6060b8198405b5f8763890ef5b53482398e1e0c7716f91ab279d9307db8d077b`. Under the current profile-16 backend, construction takes 223,863,361 instructions while the artifact remains byte-identical. The reference runtime and Node.js accept data/text, record/enum, and capability-declaration modules and reject nine Stage-0-oracle-matched executable mutations.

[Decision 0152](../Documents/Decisions/0152-First-Wasm-Hosted-Wvb-Scalar-Interpreter.md) advances the selector locally to profile 14 and adds the first bounded Wasm-hosted WVB interpreter. Its 25,568-byte WVB lowers in 82,657,852 instructions to 145,469 import-free Wasm bytes with SHA-256 `683410069c64d0143f748d34cb63f16b7d36c130662c282c003b981b24d37580`. The actual Node.js pipeline first runs the unchanged complete verifier and then interprets scalar calls/control with exact reference agreement, dual-budget exhaustion, call-depth exhaustion, checked overflow, and deterministic repeat. Text, bytes, nominal aggregates, worker packaging, source compilation, cross-host construction, cross-browser execution, and the default-playground switch remain pending.

[Decision 0157](../Documents/Decisions/0157-Wasm-Hosted-Wvb-Text-And-Bytes-Values.md) advances the selector locally to profile 15 and extends the retained interpreter with bounded immutable text and bytes values. Its 43,908-byte WVB lowers in 147,410,612 instructions to 253,707 import-free Wasm bytes with SHA-256 `57cc1c9c8a27cca63aaba23716c543450b0cfee5172dd6a1c01db246a637f78c`. Complete-verifier-approved fixtures prove static data, descriptor calls, byte reads/slices/builders/concatenation, text concatenation and conversion, strict UTF-8 boundaries, and exact range, UTF-8, narrowing, value, and heap failures under Node.js. Formatting, quoting, hashing, nominal aggregates, worker packaging, source compilation, cross-host construction, cross-browser execution, and the default-playground switch remain pending.

[Decision 0158](../Documents/Decisions/0158-Wasm-Hosted-Wvb-Formatting-And-Quoting.md) expands the retained profile-15 interpreter without changing the selector or execution ABI. Its 52,942-byte WVB lowers in 177,554,863 instructions to 306,560 import-free Wasm bytes with SHA-256 `c43569edb77a841388720ab23b144e3873bca08bd7b5a9ffb5800fcbc5bc9924`. Complete-verifier-approved fixtures prove invariant `i32`, `u8`, and `u32` formatting plus exact UTF-16-compatible ASCII quoting under Node.js; the formerly excluded compiler-produced data/text fixture now executes as result `13`. A verifier-approved SHA-256 fixture remains explicitly outside the interpreter profile. Hashing, nominal aggregates, worker packaging, source compilation, cross-host construction, cross-browser execution, and the default-playground switch remain pending.

[Decision 0162](../Documents/Decisions/0162-Import-Free-WebAssembly-Sha256-Lowering.md) advances the selector locally to profile 16 without changing execution ABI 3. The 53,761-byte retained interpreter lowers in 246,994,217 instructions to 334,209 import-free Wasm bytes with SHA-256 `2b932f153be8d428f35ef22a3504a0895cad9e8d1b83d0e2d8e4e3d480489cbe`. Direct target lowering implements SHA-256 with private scratch memory, integer rotates/shifts/bitwise operations, and 64 lowercase output bytes. Complete-verifier-first differential fixtures cover empty, padding-boundary, and multi-block values. Nominal aggregates, worker packaging, source compilation, cross-host construction, cross-browser execution, and the default-playground switch remain pending.

[Decision 0166](../Documents/Decisions/0166-Wasm-Hosted-Record-And-Enum-Values.md) expands the retained profile-16 interpreter without changing the outer selector or execution ABI. Its 65,749-byte WVB lowers in 279,819,074 instructions to 404,340 import-free Wasm bytes with SHA-256 `8c23fe32341aaf37fb2bd0d517e531a03937f00ce416175976f76f59f5380b55`. Complete-verifier-first Node.js cases prove compiler-produced record construction, field access, enum construction/comparison/name lookup, nominal values through calls and locals, deterministic default record/first-enum values, a checked 4 KiB record arena, exact `WVR3017`, and repeat reset. Windvale compiler execution, worker packaging, cross-host construction, cross-browser execution, and the default-playground switch remain pending.

[Decision 0170](../Documents/Decisions/0170-Compiler-Capacity-Wasm-Wvb-Verifier-Bundle.md) adds a compiler-capacity verifier bundle without changing profile 16, execution ABI 3, or the retained verifier and interpreter identities. The exact 599,868-byte compiler has 328 functions, 481,356 code bytes, 100,194 instructions, maximum 1,049 locals, maximum stack 34, recursion, and six hosted capability declarations. Three import-free Wasm instances derived from the canonical verifier sources admit it independently in 1,381,753,055 metadata/reference, 2,434,833,692 typed-execution, and 1,952,101,000 control/reachability instructions. The full 34-artifact Node.js gate passes locally in 366.3 seconds. This closes compiler admission, not compiler execution; the retained interpreter rejects the compiler during preflight.

## Non-claims

This profile does not establish:

- WebAssembly as a permanent Windvale host or distribution format;
- a direct source-to-WebAssembly compiler;
- a general WVB-to-WebAssembly backend;
- a general-WVB verifier with nonempty stack joins or a general Windvale-native WVB interpreter;
- recursion, function values, indirect calls, `break`, `continue`, arbitrary or unbounded instruction streams, unbounded or reclaiming record allocation, collection, or browser capability imports;
- compilation of the Windvale compiler itself to WebAssembly;
- replacement of the .NET playground path; or
- production browser isolation.

## Next extension boundary

The next measured slice should define a portable in-memory compiler entry contract, then execute the already-admitted compiler WVB in the same guest from bounded source input to canonical WVB output. The interpreter must explicitly cover the measured 328-function graph, 1,049-local frame, stack depth 34, recursion, compiler budgets, record lifetime, and dynamic-value demand rather than inheriting the hosted CLI's process, file, console, or diagnostic capabilities. Capability execution remains separately authorized by the worker. Indirect calls, `break`, `continue`, reclaiming allocation, and browser capability imports remain outside the profile until each has an explicit resource and evidence contract. The verifier, interpreter, source compiler, and result execution must all move into one disposable worker before the editable playground becomes the default .NET-free path.
