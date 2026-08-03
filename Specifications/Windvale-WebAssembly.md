# Windvale experimental WebAssembly target

- Status: Implemented locally through experimental profile 8 and cross-host qualified through profile 5, but not an accepted permanent target
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

## Execution ABI 3 and profile 8 output

Profile 8 emits an import-free WebAssembly binary version-1 module with one fixed, non-growable 129-page memory. Page zero is reserved. The host owns a 4 MiB input window beginning at byte 65,536; the guest owns a separate 4 MiB output window beginning at byte 4,259,840. The regions are disjoint, checked before access, and exactly fill the remaining memory.

The module exports these values in this exact order:

| Export | WebAssembly kind | Contract |
| --- | --- | --- |
| `Windvale.run` | function `(i32, i32) -> i32` | Accepts instruction budget and input byte length; returns `0`, `3008`, `3011`, or `3014`. |
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

The retained bytes fixture has 179-byte WVB SHA-256 `3d751ca734faed1832b4d33a9f0cfc605b695f3ae8156e3d431504798869c8d9`. It emits a 435-byte Wasm module with SHA-256:

```text
b5f87bd47be7a0ce0bb6755de4ecea8bc311c9412ee28d6091092e7aa4c184f5
```

The retained text fixture has 178-byte WVB SHA-256 `c19463d24d65c1bc46dca48dcda8541491b53b7289483afe4508685f30e0fbda`. Its strict validator produces a 791-byte Wasm module with SHA-256:

```text
c3635b8df4ed9d471faad7e653e975662099c0a2336639586915ce50b768542d
```

## Limits and failure behavior

- Input WVB is limited to the current 4 MiB immutable-`bytes` value and hosted-file boundary. This is narrower than WVB's general 16 MiB module limit.
- Profiles 3 through 5 are independently bounded to 256 locals, 16,384 code bytes, and 4,096 instructions. Profile 3 admits maximum operand-stack depth through 256; profiles 4 and 5 require exactly two.
- Profiles 6 and 7 admit two through eight functions, zero through two parameters per function, 256 combined parameters and locals per function, stack depth one or two, and the same per-function code and instruction bounds; aggregate code and instruction limits are 32,768 bytes and 8,192 instructions. Profile 7 additionally admits compiler-produced `bool` locals.
- Profile 8 admits one exact text or bytes identity function and independently limits both input and output to 4,194,304 bytes in fixed disjoint memory windows.
- Encoded WebAssembly output is limited to 65,536 bytes for every experimental profile.
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

On Windows, `pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1` rebuilds the Windvale-authored backend, compiles seventeen profile-2 through profile-8 fixtures, lowers them by running the hosted `.wv` tool, checks exact sizes and digests, and executes every output under the installed Node.js WebAssembly engine. The ABI-2 cases require exact success, one-instruction-short exhaustion, reset across repeated runs, shared-budget call execution, calls within both conditional routes, callee-overflow propagation, and nonterminating-loop containment. The ABI-3 cases additionally exercise exact-capacity bytes, curated UTF-8 boundaries, and deterministic randomized agreement with Node.js's fatal UTF-8 decoder.

Exact profile-4 implementation commit `1342f63bc7eaae17a526ca440b075c2abf3c3b31` passed GitHub [Verify run 30770158910](https://github.com/eworker-inc/Windvale/actions/runs/30770158910). Its Windows and digest-pinned Debian jobs each passed the complete repository verifier with zero-warning builds, all 68 Seed tests, and all 25 OS tests. This establishes deterministic equality for the retained profile-4 WVB and WebAssembly identities and the exact `0/42/157`, `3011/0/156`, and nonterminating `3011/0/50` execution evidence on both hosts.

Exact commit `a2285f5a0c09598ec701691bdbf0af9080e8cf0c` establishes Windows and digest-pinned Debian 12 equality for the backend WVB, selected input WVB, and generated WebAssembly digests through GitHub [Verify run 30762541741](https://github.com/eworker-inc/Windvale/actions/runs/30762541741). Both host qualification jobs completed successfully; the run-level conclusion changed to `cancelled` only after both jobs completed, when a later `main` push activated workflow concurrency cancellation.

[Decision 0107](../Documents/Decisions/0107-Playground-Disposable-WebAssembly-Worker.md) integrates the exact profile-3 success WVB and Wasm identities into the playground. On 2026-08-02, a Chromium-based in-app browser twice validated and ran the transferred 432-byte import-free module in a fresh worker, reporting ABI `1`, status `0`, result `42`, and 30 instructions equal to the .NET reference path, with no browser warning or error. This is local browser integration evidence, not cross-browser qualification or complete playground isolation.

[Decision 0113](../Documents/Decisions/0113-Metered-WebAssembly-Control-Flow.md) advances the same worker and the .NET-free page to ABI 2. On 2026-08-02, a Chromium-based in-app browser validated and ran the pinned 972-byte loop module in fresh workers: budget 157 reported `0/42/157`, budget 156 reported `3011/0/156`, and the page recorded zero .NET/Blazor requests. The seven observed assets were the stylesheet, logo, analytics script, application, artifact data, shared host, and shared worker; the browser console contained no warning or error. This remains one browser-engine family rather than cross-browser qualification.

The exact implementation commit also passed GitHub [Deploy homepage run 30770158921](https://github.com/eworker-inc/Windvale/actions/runs/30770158921): the deployment lane independently reconstructed and executed the embedded artifact before successfully publishing the static playground content.

[Decision 0116](../Documents/Decisions/0116-Sequential-WebAssembly-Control-Regions.md) advances the selector and .NET-free page to profile 5. On 2026-08-02, a Chromium-based in-app browser validated and executed the pinned 1,923-byte mixed-control module in fresh workers: budget 184 reported `0/42/184`, budget 183 reported `3011/0/183`, and the page recorded zero .NET/Blazor requests. Exact implementation commit `87cb0a3c83441d34c8307243df5dee4ffb220417` passes GitHub [Verify run 30772366223](https://github.com/eworker-inc/Windvale/actions/runs/30772366223): Windows and digest-pinned Debian 12 each pass zero-warning builds, all 70 Seed tests, all 25 OS tests, and the complete native CLI gate. [Deploy homepage run 30772366229](https://github.com/eworker-inc/Windvale/actions/runs/30772366229) independently verifies and publishes the embedded artifact. Profile 5 is therefore cross-host qualified; cross-browser qualification remains open.

[Decision 0120](../Documents/Decisions/0120-Bounded-WebAssembly-Call-Graph.md) advances the selector and .NET-free page locally to profile 6. On 2026-08-02, a Chromium-based in-app browser validated and executed the pinned 1,185-byte three-function module in fresh workers: budget 66 reported `0/42/66`, budget 65 reported `3011/0/65`, and the page recorded zero .NET/Blazor requests. Cross-host and cross-browser qualification remain pending.

[Decision 0121](../Documents/Decisions/0121-WebAssembly-Calls-With-Structured-Control.md) advances the selector and .NET-free page locally to profile 7. The retained Node.js evidence validates both conditional routes, exact shared-budget exhaustion, calls inside loop and conditional bodies, deterministic output, and malformed or nested-region rejection. On 2026-08-02, a Chromium-based in-app browser validates and executes the pinned 2,729-byte artifact in fresh workers: budget 196 reports `0/42/196`, budget 195 reports `3011/0/195`, the console contains no warning or error, and the observed development assets contain no `.NET`, Blazor, or `_framework` URL. Cross-host and cross-browser qualification remain pending.

[Decision 0123](../Documents/Decisions/0123-Versioned-WebAssembly-Linear-Memory-And-Utf8-Buffers.md) advances the selector, shared worker, and .NET-free page locally to profile 8 and execution ABI 3. Node.js validates arbitrary-byte and strict-UTF-8 round trips through fixed 4 MiB regions, exact-boundary copies, memory non-growth, deterministic budget and range failures, and 20,000 generated byte sequences against an independent fatal UTF-8 decoder. On 2026-08-02, a Chromium-based in-app browser round-trips both default and edited multilingual values, reports exact `0/4` success and `3011/3` exhaustion, zero .NET/Blazor requests, and no console warning or error. Cross-host and cross-browser qualification remain pending.

## Non-claims

This profile does not establish:

- WebAssembly as a permanent Windvale host or distribution format;
- a direct source-to-WebAssembly compiler;
- a general WVB-to-WebAssembly backend;
- a Windvale-native general WVB verifier or interpreter;
- recursion, function values, indirect calls, nested or overlapping control-flow regions, `break`, `continue`, arbitrary or unbounded instruction streams, general text or bytes operations beyond the two identity shapes, other scalar families, records, enums, memory allocation, collection, or capabilities in WebAssembly;
- compilation of the Windvale compiler itself to WebAssembly;
- replacement of the .NET playground path; or
- production browser isolation.

## Next extension boundary

The next backend slice should implement the compiler-required scalar, text, bytes, record, enum, and bounded allocation runtime over ABI 3, beginning with representative operations whose reference semantics and resource failures can be compared exactly. Nested regions are an alternative semantic extension. Recursion, indirect calls, `break`, `continue`, and browser capability imports remain outside the profile until the verifier-evidence boundary and resource contract are explicit for each. Independently, the Stage 0 compiler, verifier, `.wv` lowerer execution, and fallback interpreter should move off the UI thread before the playground is treated as hardened against hostile inputs.
