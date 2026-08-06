# Decision 0318: General scalar dispatcher WebAssembly

- Date: 2026-08-06
- Status: Implemented for the measured scalar/control slice with local engine evidence
- Advances: [Decision 0316](0316-Exact-Compiler-WebAssembly-Operation-Families.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The exact compiler's reachable operations and basic blocks now have explicit
lowering ownership, but the browser remains on the interpreted WVB path. The
first direct-emission proof must show that the general control representation
can produce valid executable Wasm, including a backward edge, without falling
back to the old structured-source pattern selectors.

Importing the existing 10,000-line backend into every new emitter would retain
the organizational and compilation costs this work is intended to remove. The
binary format primitives and dispatcher lowering therefore need focused owners.

## Decision

- Add portable `WebAssembly-Binary-Encoding.wv` for canonical Wasm magic and
  version bytes, unsigned and signed 32-bit LEB128, sections, names, indexed
  instructions, constants, and length-prefixed function bodies.
- Add portable `WebAssembly-Scalar-Dispatcher.wv` for the initial typed-direct,
  direct-call, and dispatcher-control slice.
- Emit only root-reachable functions, in deterministic breadth-first graph
  order. Give each emitted function its own exact Wasm type and map WVB call
  targets through the reachable-order directory.
- Represent supported primitive and enum values as Wasm `i32`. Retain WVB
  parameter/local ordinals and add one private `i32` program-counter local.
- Lower each verified function to one Wasm `loop`. Each basic block is selected
  by a program-counter comparison; fallthrough and jump set the next local block
  ordinal and branch to the loop; a false branch selects its two verified
  successors; return uses Wasm `return`.
- End both the dispatcher body and post-loop path with explicit `unreachable`.
  This makes the result-producing function valid even though all admitted cases
  already return or continue the loop.
- Initially admit constants, typed local load/store, scalar comparisons,
  Boolean not, `u32.from_u8`, discard, direct call, and explicit control. Reject
  checked arithmetic and every descriptor or nominal operation before emitting
  a module.
- Export only `Windvale.run` in this proof. Do not claim the existing execution
  ABI, deterministic instruction metering, managed-value memory, or browser
  package replacement yet.
- Keep the normal memory adapter capability-free (`bytes -> bytes`). A separate
  hosted local wrapper only reads a WVB and writes the resulting Wasm for engine
  probing; it is not a server compiler or browser dependency.

## Focused evidence

The canonical binary encoder is a 7,429-byte WVB at SHA-256
`dd68b60adaaf6eb0d2edfa8671aca08ddcc7a5702a9d1c4c79d245a21166f12a`.
Eight independent unsigned/signed boundary pairs cover single- and multi-byte
LEB encodings, `i32` minimum and maximum, exact sections/names/instructions,
function bodies, and truncated input. The focused contract passes in 0.625 test
seconds.

The compiler-produced general-dispatcher fixture is WVB
`8d0e22bf131addb5c7c0060726cfc9747ad62a4d4dc155bc5e03819da3045fb5`.
It uses Boolean and `i32` locals, two false branches, six explicit jumps, one
return, nine basic blocks, and a real backward edge while requiring no
arithmetic. Stage 0 returns 42.

The capability-free dispatcher tool is WVB
`ff31ed0aed7fc3e40582fc74ac2bb7bd5935bc06888e1c12d1481a5b8f77abbd`;
the hosted local probe wrapper is
`e2060758e0dec07837ad6a832af554b043c544224c2f742987c24856e51eb528`.
Both produce the exact 254-byte Wasm module
`48a530bf167f6dffeb2058585409340589c605b0a365207366a04dbe13ef2f05`.
The retained Seed contract checks deterministic bytes and instruction count,
the complete section envelope, reference result 42, and fail-closed checked
arithmetic in 0.486 test seconds after an 8.15-second zero-warning build.

The first native engine attempt rejected a 253-byte candidate because its
post-loop result path was not explicitly unreachable. After the one-byte
structural correction, Node 24.18 validates the exact module and executes
`Windvale.run()` to 42. The reusable focused probe reports:

```text
dispatcher-engine status=Valid module-bytes=254 result=42
```

No broad Seed verifier or full WebAssembly-engine suite was run; the two new
focused contracts and the direct native-engine probe are the narrowest affected
checks.

## Consequences

The new path has crossed from representation into direct executable Wasm. It
handles arbitrary admitted basic-block order and backward control without
recognizing a source-level `if` or `while` shape. The emitted application runs
in the engine rather than in the WVB interpreter.

This is not yet the browser compiler. Checked arithmetic and its exact failure
status, deterministic instruction metering, descriptor values, nominal arenas,
wide/collection values, static data, and the `bytes -> bytes` execution ABI are
still absent. The next slices add metered checked scalar operations, then the
descriptor and nominal storage required by the portable compiler tool before
attempting its complete direct emission.

## Reconsider when

- measured dispatch cost justifies replacing comparison cases with `br_table`;
- cross-block operand stacks require explicit spill slots;
- a shared failure/metering ABI changes private local layout;
- descriptor or nominal storage requires a second dispatcher representation; or
- direct exact-compiler Wasm makes this bounded proof redundant.
