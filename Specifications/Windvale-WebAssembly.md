# Windvale experimental WebAssembly target

- Status: Implemented and cross-host qualified through experimental profile 5, but not an accepted permanent target
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

## Execution ABI 2 and profiles 4 and 5 output

Profiles 4 and 5 retain ABI 1's three globals and exact export names but change the function type to `(i32) -> i32` and set `Windvale.abi` to `2`:

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

## Limits and failure behavior

- Input WVB is limited to the current 4 MiB immutable-`bytes` value and hosted-file boundary. This is narrower than WVB's general 16 MiB module limit.
- Profiles 3 through 5 are independently bounded to 256 locals, 16,384 code bytes, and 4,096 instructions. Profile 3 admits maximum operand-stack depth through 256; profiles 4 and 5 require exactly two.
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

Profile 4 additionally requires independent reconstruction of the emitted block, loop, dynamic meter, comparisons, and branches; exact agreement with the reference runtime at the measured success budget and one instruction below it; nonterminating-loop containment; reset and deterministic repeat behavior; malformed back-edge and exit-target rejection without publication; and real browser-worker execution before browser integration is claimed.

Profile 5 additionally requires independent reconstruction of every emitted loop, `if`, `else`, join, and meter; exact reference agreement for both selected conditional routes; exact success and one-instruction-short exhaustion; deterministic repeat behavior; and rejection of malformed, crossing, overlapping, or nested region edges without publication.

On Windows, `pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1` rebuilds the Windvale-authored backend, compiles eleven profile-2 through profile-5 fixtures, lowers them by running the hosted `.wv` tool, checks exact sizes and digests, and executes every output under the installed Node.js WebAssembly engine. The ABI-2 cases require exact success, one-instruction-short exhaustion, reset across repeated runs, both conditional routes, and nonterminating-loop containment.

Exact profile-4 implementation commit `1342f63bc7eaae17a526ca440b075c2abf3c3b31` passed GitHub [Verify run 30770158910](https://github.com/eworker-inc/Windvale/actions/runs/30770158910). Its Windows and digest-pinned Debian jobs each passed the complete repository verifier with zero-warning builds, all 68 Seed tests, and all 25 OS tests. This establishes deterministic equality for the retained profile-4 WVB and WebAssembly identities and the exact `0/42/157`, `3011/0/156`, and nonterminating `3011/0/50` execution evidence on both hosts.

Exact commit `a2285f5a0c09598ec701691bdbf0af9080e8cf0c` establishes Windows and digest-pinned Debian 12 equality for the backend WVB, selected input WVB, and generated WebAssembly digests through GitHub [Verify run 30762541741](https://github.com/eworker-inc/Windvale/actions/runs/30762541741). Both host qualification jobs completed successfully; the run-level conclusion changed to `cancelled` only after both jobs completed, when a later `main` push activated workflow concurrency cancellation.

[Decision 0107](../Documents/Decisions/0107-Playground-Disposable-WebAssembly-Worker.md) integrates the exact profile-3 success WVB and Wasm identities into the playground. On 2026-08-02, a Chromium-based in-app browser twice validated and ran the transferred 432-byte import-free module in a fresh worker, reporting ABI `1`, status `0`, result `42`, and 30 instructions equal to the .NET reference path, with no browser warning or error. This is local browser integration evidence, not cross-browser qualification or complete playground isolation.

[Decision 0113](../Documents/Decisions/0113-Metered-WebAssembly-Control-Flow.md) advances the same worker and the .NET-free page to ABI 2. On 2026-08-02, a Chromium-based in-app browser validated and ran the pinned 972-byte loop module in fresh workers: budget 157 reported `0/42/157`, budget 156 reported `3011/0/156`, and the page recorded zero .NET/Blazor requests. The seven observed assets were the stylesheet, logo, analytics script, application, artifact data, shared host, and shared worker; the browser console contained no warning or error. This remains one browser-engine family rather than cross-browser qualification.

The exact implementation commit also passed GitHub [Deploy homepage run 30770158921](https://github.com/eworker-inc/Windvale/actions/runs/30770158921): the deployment lane independently reconstructed and executed the embedded artifact before successfully publishing the static playground content.

[Decision 0116](../Documents/Decisions/0116-Sequential-WebAssembly-Control-Regions.md) advances the selector and .NET-free page to profile 5. On 2026-08-02, a Chromium-based in-app browser validated and executed the pinned 1,923-byte mixed-control module in fresh workers: budget 184 reported `0/42/184`, budget 183 reported `3011/0/183`, and the page recorded zero .NET/Blazor requests. Exact implementation commit `87cb0a3c83441d34c8307243df5dee4ffb220417` passes GitHub [Verify run 30772366223](https://github.com/eworker-inc/Windvale/actions/runs/30772366223): Windows and digest-pinned Debian 12 each pass zero-warning builds, all 70 Seed tests, all 25 OS tests, and the complete native CLI gate. [Deploy homepage run 30772366229](https://github.com/eworker-inc/Windvale/actions/runs/30772366229) independently verifies and publishes the embedded artifact. Profile 5 is therefore cross-host qualified; cross-browser qualification remains open.

## Non-claims

This profile does not establish:

- WebAssembly as a permanent Windvale host or distribution format;
- a direct source-to-WebAssembly compiler;
- a general WVB-to-WebAssembly backend;
- a Windvale-native general WVB verifier or interpreter;
- calls, nested or overlapping control-flow regions, `break`, `continue`, arbitrary or unbounded instruction streams, other scalar families, general resource counters, text, bytes, records, enums, memory management, or capabilities in WebAssembly;
- compilation of the Windvale compiler itself to WebAssembly;
- replacement of the .NET playground path; or
- production browser isolation.

## Next extension boundary

The next backend slice should admit nested structured regions or a deliberately bounded call graph over the existing scalar families. Calls, `break`, `continue`, linear memory, and browser capability imports remain outside the profile until the verifier-evidence boundary and resource contract are explicit for each. Independently, the Stage 0 compiler, verifier, `.wv` lowerer execution, and fallback interpreter should move off the UI thread before the playground is treated as hardened against hostile inputs.
